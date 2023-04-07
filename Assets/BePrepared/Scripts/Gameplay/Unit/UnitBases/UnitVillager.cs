using UnityEngine;
using UnityEngine.AI;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public enum VillagerType
    {
        Idle,
        Lumberjack,
        Farmer
    }

    public class UnitVillager : MonoBehaviour
    {
        private static int s_SpeedHash = Animator.StringToHash("Speed");
        private static int s_WorkHash = Animator.StringToHash("Work");
        private static int s_WorkIndexHash = Animator.StringToHash("WorkIndex");

        public VillagerType VillagerType => m_VillagerType;

        private UnitResource m_AssignedResource;
        private NavMeshAgent m_Agent;
        private VillagerType m_VillagerType;

        [Header("Animation")]
        [SerializeField] private Animator m_Animator;

        [Header("Resource")]
        [SerializeField] private float m_GatherTime = 0.34f;
        [SerializeField] private int m_GatherAmount = 1;
        private float m_GatherTimer = 0.0f;

        private Vector3 m_TargetPosition;
        private bool m_TargetReached = false;

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_Agent.updateRotation = false;

            m_TargetPosition = transform.position;

            UnitManager.VillagerCreated();
            UnitManager.AddIdleVillager(this);
        }

        public void Move(Vector3 direction)
        {
            m_TargetPosition = direction;

            m_Agent.isStopped = false;
            m_Agent.SetDestination(direction);
        }

        public void Assign(UnitResource resource)
        {
            m_AssignedResource = resource;
            switch (m_AssignedResource.ResourceType)
            {
                case ResourceType.Wood:
                    ChangeType(VillagerType.Lumberjack);
                    break;
                case ResourceType.Food:
                    ChangeType(VillagerType.Farmer);
                    break;
            }
            m_AssignedResource.AddVillager(this);
        }

        public void Unassign()
        {
            if (m_AssignedResource != null)
                m_AssignedResource.RemoveVillager(this);

            m_AssignedResource = null;
            ChangeType(VillagerType.Idle);
        }

        public void ResourceDepleted()
        {
            m_AssignedResource = null;
            ChangeType(VillagerType.Idle);
        }

        private void Update()
        {
            Vector3 direction = m_TargetPosition - transform.position;
            direction.y = 0;

            float directionMagnitude = direction.magnitude;

            if (directionMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 8.0f);
            }

            float reachDistance = 1.0f;
            if (directionMagnitude < reachDistance)
            {
                m_TargetReached = true;
                m_Agent.isStopped = true;
            }
            else
            {
                m_TargetReached = false;
                m_Agent.isStopped = false;
            }

            HandleGathering();

            HandleAnimation();
        }

        private void ChangeType(VillagerType type)
        {
            m_VillagerType = type;
            UnitUI.Instance.UpdateVillager(this);

            if (type == VillagerType.Idle)
                UnitManager.AddIdleVillager(this);
            else
                UnitManager.RemoveIdleVillager(this);
        }

        private void HandleGathering()
        {
            if (m_AssignedResource != null && m_TargetReached)
            {
                m_GatherTimer += Time.deltaTime;
                if (m_GatherTimer >= m_GatherTime)
                {
                    ResourceManager.AddResource(m_AssignedResource.ResourceType, m_GatherAmount);
                    m_AssignedResource.Gather(m_GatherAmount);
                    m_GatherTimer = 0.0f;
                }
            }
        }

        private void HandleAnimation()
        {
            float velocity = m_Agent.velocity.magnitude;

            m_Animator.SetFloat(s_SpeedHash, velocity);

            if (m_VillagerType != VillagerType.Idle && velocity <= 0.1f)
            {
                m_Animator.SetBool(s_WorkHash, m_TargetReached);
                m_Animator.SetFloat(s_WorkIndexHash, (int)m_VillagerType - 1);
            }
            else
                m_Animator.SetBool(s_WorkHash, false);
        }

        private void OnDestroy()
        {
            if (m_AssignedResource != null)
                Unassign();
            UnitManager.VillagerDestroyed();
        }
    }
}
