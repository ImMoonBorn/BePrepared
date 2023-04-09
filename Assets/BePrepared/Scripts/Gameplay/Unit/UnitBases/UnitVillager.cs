using MoonBorn.BePrepared.Gameplay.BuildSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public enum VillagerType
    {
        Idle = 0,
        Lumberjack = 1,
        Farmer = 2,
        Miner = 3,
        Builder = 4
    }

    public class UnitVillager : MonoBehaviour
    {
        private readonly static int s_SpeedHash = Animator.StringToHash("Speed");
        private readonly static int s_WorkHash = Animator.StringToHash("Work");
        private readonly static int s_WorkIndexHash = Animator.StringToHash("WorkIndex");

        public VillagerType VillagerType => m_VillagerType;

        private UnitResource m_AssignedResource;
        private UnitConstruction m_AssignedConstruction;
        private NavMeshAgent m_Agent;
        private NavMeshObstacle m_Obstacle;
        private VillagerType m_VillagerType;

        [Header("Animation")]
        [SerializeField] private Animator m_Animator;

        [Header("Resource")]
        [SerializeField] private float m_GatherTime = 0.34f;
        [SerializeField] private int m_GatherAmount = 1;
        [SerializeField] private float m_SearchRadius = 15.0f;
        private float m_GatherTimer = 0.0f;
        private Vector3 m_TargetPosition;
        private bool m_TargetReached = false;

        [Header("Tools")]
        [SerializeField] private GameObject m_Axe;
        [SerializeField] private GameObject m_Hoe;
        [SerializeField] private GameObject m_Pickaxe;
        [SerializeField] private GameObject m_Hammer;
        private GameObject[] m_Tools;

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_Obstacle = GetComponent<NavMeshObstacle>();
            m_Obstacle.enabled = false;

            m_TargetPosition = transform.position;

            m_Tools = new GameObject[4];
            m_Tools[0] = m_Axe;
            m_Tools[1] = m_Hoe;
            m_Tools[2] = m_Pickaxe;
            m_Tools[3] = m_Hammer;

            foreach (GameObject tool in m_Tools)
                tool.SetActive(false);

            UnitManager.VillagerCreated();
            UnitManager.AddIdleVillager(this);
            ChangeType(m_VillagerType);
        }

        public void Move(Vector3 direction)
        {
            m_TargetPosition = direction;

            m_Obstacle.enabled = false;
            StartCoroutine(MoveCoroutine(direction));
        }

        private IEnumerator MoveCoroutine(Vector3 direction)
        {
            yield return null;
            m_Agent.enabled = true;
            m_Agent.SetDestination(direction);
        }

        public void Assign(UnitResource resource)
        {
            m_AssignedResource = resource;

            ChangeType(GetVillagerTypeFromResource(resource.ResourceType));

            m_AssignedResource.AddVillager(this);
        }

        public void AssignBuilder(UnitConstruction construction)
        {
            m_AssignedConstruction = construction;
            m_AssignedConstruction.AddVillager(this);
            ChangeType(VillagerType.Builder);
        }

        public void Unassign()
        {
            if (m_AssignedResource != null)
                m_AssignedResource.RemoveVillager(this);

            if (m_AssignedConstruction != null)
                m_AssignedConstruction.RemoveVillager(this);

            m_AssignedResource = null;
            m_AssignedConstruction = null;
            ChangeType(VillagerType.Idle);
        }

        public void ResourceDepleted(bool doSearch)
        {
            if (doSearch)
                if (SearchForOtherResources())
                    return;

            m_AssignedResource = null;
            ChangeType(VillagerType.Idle);
        }

        public void BuildFinished()
        {
            m_AssignedConstruction = null;
            ChangeType(VillagerType.Idle);
        }

        private void SelectTool(VillagerType type)
        {
            foreach (GameObject tool in m_Tools)
                tool.SetActive(false);

            if (type != VillagerType.Idle)
                m_Tools[(int)m_VillagerType - 1].SetActive(true);
        }

        private bool SearchForOtherResources()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_SearchRadius);

            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    if (collider.TryGetComponent<UnitResource>(out var resource))
                    {
                        if (resource.ResourceType == GetResourceTypeFromVillager(m_VillagerType) && m_AssignedResource != resource)
                        {
                            Move(collider.ClosestPoint(transform.position));
                            Assign(resource);
                            return true;
                        }
                    }

                }
                return true;
            }
            return false;
        }

        private void Update()
        {
            Vector3 direction = m_TargetPosition - transform.position;
            direction.y = 0;
            float directionMagnitude = direction.magnitude;
            float reachDistance = 1.0f;

            if (directionMagnitude < reachDistance)
            {
                m_Agent.enabled = false;
                m_Obstacle.enabled = true;
                m_TargetReached = true;
            }
            else
                m_TargetReached = false;

            if (!m_TargetReached && !m_Agent.enabled)
                Move(m_TargetPosition);

            HandleGathering();
            HandleBuilding();
            HandleAnimation();
        }

        private void ChangeType(VillagerType type)
        {
            m_VillagerType = type;

            if (UnitUI.Instance != null)
                UnitUI.Instance.UpdateVillager(this);

            if (type == VillagerType.Idle)
                UnitManager.AddIdleVillager(this);
            else
                UnitManager.RemoveIdleVillager(this);

            SelectTool(m_VillagerType);
        }

        private void LookToTarget(Vector3 target)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0;

            if (direction.magnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 8.0f);
            }
        }

        private void HandleGathering()
        {
            if (m_AssignedResource != null && m_TargetReached)
            {
                LookToTarget(m_AssignedResource.transform.position);

                m_GatherTimer += Time.deltaTime;
                if (m_GatherTimer >= m_GatherTime)
                {
                    m_AssignedResource.Gather(m_GatherAmount);
                    m_GatherTimer = 0.0f;
                }
            }
        }

        private void HandleBuilding()
        {
            if (m_AssignedConstruction != null && m_TargetReached)
            {
                LookToTarget(m_AssignedConstruction.transform.position);
                m_AssignedConstruction.Build();
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

        private VillagerType GetVillagerTypeFromResource(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Wood => VillagerType.Lumberjack,
                ResourceType.Food => VillagerType.Farmer,
                ResourceType.Stone => VillagerType.Miner,
                _ => VillagerType.Idle
            };
        }

        private ResourceType GetResourceTypeFromVillager(VillagerType villagerType)
        {
            return villagerType switch
            {
                VillagerType.Lumberjack => ResourceType.Wood,
                VillagerType.Farmer => ResourceType.Food,
                VillagerType.Miner => ResourceType.Stone,
                _ => ResourceType.None
            };
        }


        private void OnDestroy()
        {
            if (m_AssignedResource != null)
                Unassign();
            UnitManager.VillagerDestroyed();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, m_SearchRadius);
        }
    }
}
