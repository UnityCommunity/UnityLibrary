// made with chatGPT

using UnityEngine;
using UnityEngine.AI;

namespace UnityLibrary
{
    public class NavMeshAgentExample : MonoBehaviour
    {
        public Transform target;

        private NavMeshAgent agent;

        void Start()
        {
            // Get the NavMeshAgent component on this game object
            agent = GetComponent<NavMeshAgent>();

            // Set the destination of the NavMeshAgent to the target Transform
            agent.destination = target.position;
        }
    }
}
