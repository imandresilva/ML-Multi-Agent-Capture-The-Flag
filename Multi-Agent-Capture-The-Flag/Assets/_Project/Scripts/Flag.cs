using UnityEngine;

public class Flag : MonoBehaviour
{
    
    [SerializeField] private BoxCollider baseCollider;
    
    void Start() {
        Reset();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Attacker")) {
            Debug.Log("Attacker picked up flag");
            other.GetComponent<AttackingAgent>().PickedUpFlag();
            gameObject.SetActive(false);
        }
    }

    public void Reset() {
        Vector3 randomPosition = AttackingAgent.RandomPointInBounds(baseCollider.bounds);
        transform.position = randomPosition;
        gameObject.SetActive(true);
    }

    public bool IsFlagInBase() {
        return gameObject.activeSelf;
    }
}
