using System;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class DefendingAgent : Agent {
    
    [Header("Unity Parameters")]
    [SerializeField] private BoxCollider defenderBase;
    [SerializeField] private Flag flag;
    [SerializeField] private float speed = 10;
    private LayerMask defendingBaseMask;
    private Rigidbody rb;
    private EyeRayCast baseEyeRayCast;
    private EyeCone[] eyes;
    
    // Game parameters
    private bool atDefendingBase;
    
    private void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        
        defendingBaseMask = LayerMask.GetMask("Defending_Base");
        
        baseEyeRayCast = new EyeRayCast(new Vector3(1, 0, 0), transform);
        eyes = new EyeCone[3];
        eyes[0] = new EyeCone(60, 60, transform, transform);
        eyes[1] = new EyeCone(60, 0, transform, transform);
        eyes[2] = new EyeCone(60, -60, transform, transform);

    }
    
    public bool isAlly(ObjectType t) {
        return t == ObjectType.Defender;
    }
    
    
    /** Private methods **/
    
    private string CheckState() {
        string seenBits = CheckEyes();
        //Debug.Log(typeSpecificBits + seenBits);
        return seenBits;
    }
    
    private string CheckEyes() {
        SeenBits bits = new SeenBits(eyes.Length * 7);
	    
        for (int eyeIt = 0; eyeIt < eyes.Length; eyeIt++) {
            List<Tuple<ObjectType, float>> seenObjects = eyes[eyeIt].Look();

            foreach (var seenObject in seenObjects) {
                bits.Update(seenObject, eyeIt, this);
            }
        }

        return bits.ToString();
    }
    
    
    /** Triggers **/

    private void OnCollisionEnter(Collision other) {
        if (other.collider.CompareTag("Attacker")) {
            if(other.transform.GetComponent<AttackingAgent>().LostFlag()){
                AddReward(100);
                Done();
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Defending_Base")) {
            atDefendingBase = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Defending_Base")) {
            atDefendingBase = false;
        }
    }
    
    
    /** ML Agents **/
    
    public override float[] Heuristic() {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
    
    public override void AgentReset() {
        // Resets him to his base
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Vector3 randomPosition = AttackingAgent.RandomPointInBounds(defenderBase.bounds);
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        transform.position = randomPosition;
        transform.rotation = randomRotation;
    }
    
    public override void CollectObservations() {
        AddVectorObs(atDefendingBase || baseEyeRayCast.LookForLayerHit(transform, defendingBaseMask));
        AddVectorObs(flag.IsFlagInBase());
        AddVectorObs(atDefendingBase);
        string state = CheckState();
        foreach (char c in state) {
            AddVectorObs(c == '1');
        }
    }

    public override void AgentAction(float[] vectorAction) {
        float attenuation = 1f;
        if (vectorAction[1] < 0)
        {
            attenuation = 0.5f;
        }

        Vector3 translateVector = new Vector3(vectorAction[1] * Time.fixedDeltaTime * speed * attenuation, 0, 0);
        transform.Translate(translateVector);
        transform.Rotate(new Vector3(0,vectorAction[0], 0),4);
        AddReward(-100f / agentParameters.maxStep);
    }
    
}
