using System;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttackingAgent : Agent {
    
    [Header("Unity Parameters")]
    [SerializeField] private BoxCollider attackerBase;
    [SerializeField] private Flag flag;
    private LayerMask attackingBaseMask;
    private Rigidbody rb;
    private EyeRayCast baseEyeRayCast;
    private EyeCone[] eyes;
    
    // Game parameters
    private bool iHaveFlag;
    private bool atAttackerBase;

    private void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        
        attackingBaseMask = LayerMask.GetMask("Attacking_Base");
        
        baseEyeRayCast = new EyeRayCast(new Vector3(1, 0, 0), transform);
        eyes = new EyeCone[3];
        eyes[0] = new EyeCone(60, 60, transform, transform);
        eyes[1] = new EyeCone(60, 0, transform, transform);
        eyes[2] = new EyeCone(60, -60, transform, transform);

    }

    public bool HasFlag() {
        return iHaveFlag;
    }
    
    public bool isAlly(ObjectType t) {
        return t == ObjectType.Attacker;
    }

    public void PickedUpFlag() {
        iHaveFlag = true;
        AddReward(1);
    }
    
    
    /** Private methods **/
    
    private string CheckState() {
        string typeSpecificBits = CheckTypeSpecificBits();
        string seenBits = CheckEyes();
        //Debug.Log(typeSpecificBits + seenBits);
        return typeSpecificBits + seenBits;
    }
    
    private string CheckTypeSpecificBits() {
        char[] stateBits = new char[4];
        for(int i = 0; i < stateBits.Length; i++) {
            stateBits[i] = '0';
        }

        // Check for allied base in front
        stateBits[0] = atAttackerBase || baseEyeRayCast.LookForLayerHit(transform, attackingBaseMask) ? '1' : '0'; // 1 if base in front
        
        // Check if my team has the flag
        stateBits[1] = flag.IsFlagInBase() ? '0' : '1'; // 1 if my team has the flag
        
        // Check if I have the flag
        stateBits[2] = iHaveFlag ? '1' : '0'; // 1 if I have the flag

        // Check if I am at my base
        stateBits[3] = atAttackerBase ? '1' : '0'; // 1 if I am at my base
        
        return new string(stateBits);
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

    public static Vector3 RandomPointInBounds(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    
    
    /** Triggers **/

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Attacking_Base")) {
            atAttackerBase = true;
            if (iHaveFlag) {
                AddReward(100);
                Done();
                GameObject.FindWithTag("Academy").GetComponent<CTFAcademy>().AcademyReset();
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Attacking_Base")) {
            atAttackerBase = false;
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
        flag.Reset();
        // Sets his variables back to default
        iHaveFlag = false;
        
        // Resets him to his base
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Vector3 randomPosition = RandomPointInBounds(attackerBase.bounds);
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        transform.position = randomPosition;
        transform.rotation = randomRotation;
        
    }
    
    public override void CollectObservations() {
        string state = CheckState();
        foreach (char c in state) {
            AddVectorObs(c == '1');
        }
    }

    public override void AgentAction(float[] vectorAction) {
        //Debug.Log("H: " + vectorAction[0] + " - V: " + vectorAction[1]);
        float attenuation = 1f;
        if (vectorAction[1] < 0)
        {
            attenuation = 0.8f;
        }

        Vector3 translateVector = new Vector3(vectorAction[1] * Time.fixedDeltaTime * 10 * attenuation, 0, 0);
        //Debug.Log(translateVector);
        transform.Translate(translateVector);
        transform.Rotate(new Vector3(0,vectorAction[0], 0),4);
        AddReward(-100f / agentParameters.maxStep);
    }
    
    
}
