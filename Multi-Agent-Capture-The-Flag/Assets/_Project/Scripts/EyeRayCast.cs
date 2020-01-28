using UnityEngine;

public class EyeRayCast {
    private Vector3 lookDir;
    private Transform source;

    public EyeRayCast(Vector3 lookDir, Transform source) {
        this.lookDir = lookDir;
        this.source = source;
    }
    
    public bool LookForLayerHit(Transform transform, int layerMask) {
        Vector3 normalizedDir = transform.TransformVector(lookDir).normalized;
        bool hit = Physics.Raycast(source.position, normalizedDir, 40, layerMask);
        Debug.DrawRay(source.position, normalizedDir*42, hit ? Color.yellow : Color.red, 0, false);
        return hit;
    }

}