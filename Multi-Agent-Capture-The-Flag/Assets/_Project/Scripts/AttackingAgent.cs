using MLAgents;
using UnityEngine;

public class AttackingAgent : Agent {
    
    private Rigidbody rb;
    [SerializeField] private BoxCollider attackerBase;
    
    // Game parameters
    private bool iHaveFlag;

    private void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public bool HasFlag() {
        return iHaveFlag;
    }
    
    
    /** Private methods **/
    
    private static Vector3 RandomPointInBounds(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    

    /** ML Agents **/
    
    public override float[] Heuristic() {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
    
    public override void AgentReset() {
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
        //AddVectorObs();
    }

    public override void AgentAction(float[] vectorAction) {
        float attenuation = 1f;
        if (vectorAction[1] < 0)
        {
            attenuation = 0.8f;
        }
        transform.Translate(new Vector3(vectorAction[1] * Time.fixedDeltaTime * 10 * attenuation,0,0));
        transform.Rotate(new Vector3(0,vectorAction[0], 0),4);
    }

}
