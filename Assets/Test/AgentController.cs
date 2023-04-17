using MoonBorn.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private NavMeshAgent m_Agent;
    private NavMeshObstacle m_Obstacle;
    private Vector3 m_TargetedPos;
    private Vector3 m_TargetPos;
    private Vector3 m_LastPos;

    [SerializeField] private float m_StopDistance;
    public bool m_TargetReached;

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Obstacle = GetComponent<NavMeshObstacle>();
        m_Obstacle.enabled = false;

        m_TargetedPos = transform.position;
        m_TargetPos = transform.position;
        m_LastPos = transform.position;
    }

    public void Move(Vector3 pos)
    {
        m_Obstacle.enabled = false;

        StartCoroutine(IMove(pos));
    }

    private IEnumerator IMove(Vector3 pos)
    {
        yield return null;
        m_TargetReached = false;

        m_Agent.enabled = true;
        pos.y = transform.position.y;
        m_TargetedPos = pos;
        m_Agent.SetDestination(pos);
        m_TargetPos = m_Agent.destination;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Move(Vector3.zero);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                Move(hit.collider.ClosestPoint(transform.position));
            }
        }

        Vector3 direction = m_TargetPos - transform.position;
        direction.y = 0;
        float directionMagnitude = direction.magnitude;
        float reachDistance = m_StopDistance;

        if (directionMagnitude <= reachDistance)
        {
            if (!m_TargetReached)
            {
                m_Agent.velocity = Vector3.zero;
                m_Agent.enabled = false;

                m_Obstacle.enabled = true;

                m_LastPos = transform.position;

                m_TargetReached = true;
                print("Reached");
            }
        }
        else
        {
            if (m_Agent.enabled)
            {
                m_TargetPos = m_Agent.destination;
                m_TargetPos.y = transform.position.y;
            }
        }

        if (Vector3.Distance(m_LastPos, transform.position) > m_Obstacle.carvingMoveThreshold && m_TargetReached)
        {
            Move(m_TargetedPos);
        }
    }
}
