using System;
using System.Collections.Generic;
using UnityEngine;

public class EyeCone {
    
    private Vector3[] dirs;
    private Transform agentTransform;
    private Transform rayCastSource;
    private int mask;

    public EyeCone(int fov, int midAngle, Transform agentTransform, Transform rayCastSource) {
        GetDirections(fov, midAngle, 6);
        this.rayCastSource = rayCastSource;
        this.agentTransform = agentTransform;
        mask =~ LayerMask.GetMask("Attacking_Base", "Defending_Base");
    }


    /// <summary>
    /// Casts ray casts forward to check what an eye can see in a cone 
    /// </summary>
    /// <returns></returns>
    public List<Tuple<ObjectType, float>> Look() {
        
        List<Tuple<ObjectType, float>> seenObjects = new List<Tuple<ObjectType, float>>();

        foreach (Vector3 dir in dirs) {
            Vector3 normalizedDir = agentTransform.TransformVector(dir).normalized;
            if (Physics.Raycast(rayCastSource.position, normalizedDir, out RaycastHit hit, 20, mask)) {
                GameObject gameObject = hit.transform.gameObject;
                float distance = Vector3.Distance(agentTransform.position, hit.point);
                Debug.DrawRay(rayCastSource.position, normalizedDir * 20,
                    distance > 1 ? Color.green : Color.yellow, 0, false);
                if (gameObject.CompareTag("Attacker")) {
                    if (gameObject.GetComponent<AttackingAgent>().HasFlag()) {
                        seenObjects.Add(new Tuple<ObjectType, float>(ObjectType.AttackerWithFlag, distance));
                        continue;
                    }
                }
                
                Enum.TryParse(hit.transform.tag, out ObjectType result);
                seenObjects.Add(new Tuple<ObjectType, float>(result, distance));
                
            }

        }
        
        return seenObjects;
    }


    private void GetDirections(int fov, int midAngle, int numRays) {
        dirs = new Vector3[numRays];
        int rayAngle = fov / numRays;

        for (int i = 0; i < numRays; i++) {
            dirs[i] = Angle2Vec(midAngle + (i - (numRays / 2 - 1)) * rayAngle);
        }
    }

    private Vector3 Angle2Vec(int angle) {
        return new Vector3(Mathf.Cos(DegreeToRadian(angle)),0, Mathf.Sin(DegreeToRadian(angle)));
    }

    private float DegreeToRadian(double angle)
    {
        return (float) (Math.PI * angle / 180.0);
    }

}