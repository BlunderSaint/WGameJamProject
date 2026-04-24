using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : MonoBehaviour
{
    public float range = 10f;
    public int damage = 10;
    public LayerMask mask; // Walls that block sight

    private NavMeshAgent agent;
    private GameObject[] players;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    void Update()
    {
        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);

            // If player is in range AND not behind a wall
            if (dist < range && !Physics.Linecast(transform.position, p.transform.position, mask))
            {
                agent.SetDestination(p.transform.position);

                // If touching player, hurt them
                if (dist < 1.5f)
                {
                    //p.GetComponent<Health>().TakeDamage(damage);
                }
            }
        }
    }
}