using UnityEngine;
using UnityEngine.AI;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Door door;
    private int _agentInRange;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<NavMeshAgent>(out _)) return;
        
        _agentInRange++;
        if (!door.IsOpen)
            door.Open(other.transform.position);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<NavMeshAgent>(out _)) return;
        
        _agentInRange--;
        if (door.IsOpen && _agentInRange == 0)
            door.Close();
    }
}