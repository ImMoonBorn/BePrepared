using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using MoonBorn.Audio;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.BuildSystem;
using MoonBorn.BePrepared.Utils.SaveSystem;
using MoonBorn.UI;
using System;
using System.Collections.Generic;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public enum VillagerType
    {
        Idle = 0,
        Lumberjack = 1,
        Farmer = 2,
        Miner = 3,
        Builder = 4,
        Gatherer = 5,
    }

    public enum VillagerGender
    {
        None,
        Male,
        Female
    }

    public class UnitVillager : MonoBehaviour, ISaveable
    {
        public VillagerType VillagerType => m_VillagerType;

        [SerializeField] private UnitVillagerSO m_VillagerSO;
        [SerializeField] private LayerMask m_SearchLayer;
        [SerializeField] private VillagerGender m_Gender;
        private VillagerType m_VillagerType;
        private UnitResource m_AssignedResource;
        private UnitConstruction m_AssignedConstruction;
        private NavMeshAgent m_Agent;
        private NavMeshObstacle m_Obstacle;

        [Header("Animation")]
        [SerializeField] private Animator m_Animator;
        private readonly static int s_SpeedHash = Animator.StringToHash("Speed");
        private readonly static int s_WorkHash = Animator.StringToHash("Work");
        private readonly static int s_WorkIndexHash = Animator.StringToHash("WorkIndex");

        [Header("Resource")]
        [SerializeField] private float m_SearchRadius = 15.0f;
        private float m_GatherRatePerSecond = 0.5f;
        private float m_GatherTimer = 0.0f;
        private Vector3 m_TargetedPosition;
        private Vector3 m_TargetPosition;
        private Vector3 m_LastPosition;
        private bool m_TargetReached = false;

        [Header("Tools")]
        [SerializeField] private GameObject m_Axe;
        [SerializeField] private GameObject m_Hoe;
        [SerializeField] private GameObject m_Pickaxe;
        [SerializeField] private GameObject m_Hammer;
        private GameObject[] m_Tools;

        [Header("Audio")]
        [SerializeField] private RandomClipPlayer m_ClipPlayer;
        [SerializeField] private Playlist[] m_Playlists;
        [SerializeField] private AudioSource m_VoiceSource;
        private bool m_IsSpeaking = false;

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_Obstacle = GetComponent<NavMeshObstacle>();
            m_Obstacle.enabled = false;

            m_TargetedPosition = transform.position;
            m_TargetPosition = transform.position;
            m_LastPosition = transform.position;

            m_Tools = new GameObject[4];
            m_Tools[0] = m_Axe;
            m_Tools[1] = m_Hoe;
            m_Tools[2] = m_Pickaxe;
            m_Tools[3] = m_Hammer;

            foreach (GameObject tool in m_Tools)
                tool.SetActive(false);

            ChangeType(m_VillagerType);
        }

        private void Start()
        {
            UnitManager.VillagerCreated();
            UnitManager.AddIdleVillager(this);
            UnitImprovements.OnVillagerImprovement += CalculateGatherRate;
        }

        public void Move(Vector3 direction)
        {
            m_Agent.velocity = Vector3.zero;
            m_Obstacle.enabled = false;

            StartCoroutine(MoveCoroutine(direction));

            if (m_Gender != VillagerGender.None && m_VillagerType == VillagerType.Idle)
            {
                m_VoiceSource.clip = m_VillagerSO.GetClipByGender(m_Gender);
                if (m_VoiceSource.clip != null)
                    StartCoroutine(Speak(m_VoiceSource.clip));
            }
        }

        private IEnumerator MoveCoroutine(Vector3 direction)
        {
            yield return null;
            m_TargetReached = false;
            m_Agent.enabled = true;

            direction.y = transform.position.y;

            m_TargetedPosition = direction;
            m_Agent.SetDestination(direction);

            m_TargetPosition = m_Agent.destination;
        }

        private IEnumerator Speak(AudioClip clip)
        {
            if (m_IsSpeaking)
                yield break;

            m_IsSpeaking = true;
            m_VoiceSource.PlayOneShot(m_VoiceSource.clip);
            float timer = 0.0f;
            while (timer <= clip.length)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            m_IsSpeaking = false;
        }

        public void Assign(UnitResource resource, Vector3 movePos)
        {
            if (resource.ReachedGathererLimit)
            {
                NotificationManager.Notificate("This resource reached its gatherer limits.", NotificationType.Warning);
                return;
            }

            Unassign();

            Move(movePos);

            m_AssignedConstruction = null;
            m_AssignedResource = resource;

            ChangeType(m_AssignedResource.VillagerType);
            CalculateGatherRate();

            m_AssignedResource.AddVillager(this);
        }

        public void AssignBuilder(UnitConstruction construction, Vector3 movePos)
        {
            Unassign();

            Move(movePos);

            m_AssignedConstruction = construction;
            m_AssignedConstruction.AddVillager(this);

            ChangeType(VillagerType.Builder);
        }

        public void Unassign()
        {
            m_GatherTimer = 0.0f;

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
            if (SearchForOtherConsturctions())
                return;

            m_AssignedConstruction = null;
            ChangeType(VillagerType.Idle);
        }

        private void SelectTool(VillagerType type)
        {
            foreach (GameObject tool in m_Tools)
                tool.SetActive(false);

            if (type != VillagerType.Idle && type != VillagerType.Gatherer)
                m_Tools[(int)m_VillagerType - 1].SetActive(true);
        }

        private bool SearchForOtherResources()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_SearchRadius, m_SearchLayer);
            int n = colliders.Length;

            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;
                Collider minCol = colliders[i];
                float minDist = Vector3.Distance(minCol.transform.position, transform.position);

                for (int j = i + 1; j < n; j++)
                {
                    Collider collider = colliders[j];
                    float colDist = Vector3.Distance(collider.transform.position, transform.position);

                    if (colDist < minDist)
                    {
                        minDist = colDist;
                        minIndex = j;
                    }
                }

                if (minIndex != i)
                    (colliders[i], colliders[minIndex]) = (colliders[minIndex], colliders[i]);
            }

            for (int i = 0; i < n; i++)
            {
                Collider collider = colliders[i];

                if (collider.TryGetComponent<UnitResource>(out var resource))
                {
                    if (resource.ReachedGathererLimit)
                        continue;

                    if (resource.ResourceType == GetResourceTypeFromVillager(m_VillagerType) && m_AssignedResource != resource)
                    {
                        Assign(resource, collider.ClosestPoint(transform.position));
                        return true;
                    }
                }

            }
            return false;
        }

        private bool SearchForOtherConsturctions()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_SearchRadius);
            int n = colliders.Length;

            for (int i = 0; i < n; i++)
            {
                Collider collider = colliders[i];
                if (collider.TryGetComponent<UnitConstruction>(out var construction))
                {
                    if (m_AssignedConstruction != construction)
                    {
                        AssignBuilder(m_AssignedConstruction, collider.ClosestPoint(transform.position));
                        return true;
                    }
                }
            }
            return false;
        }

        private void Update()
        {
            HandleGathering();
            HandleBuilding();
            HandleAnimation();

            Vector3 direction = m_TargetPosition - transform.position;
            direction.y = transform.position.y;
            float directionMagnitude = direction.magnitude;
            float reachDistance = 0.33f;

            if (directionMagnitude <= reachDistance)
            {
                if (!m_TargetReached)
                {
                    m_Agent.velocity = Vector3.zero;
                    m_Agent.enabled = false;

                    m_Obstacle.enabled = true;

                    m_LastPosition = transform.position;

                    m_TargetReached = true;
                }
            }
            else
            {
                if (m_Agent.enabled)
                {
                    m_TargetPosition = m_Agent.destination;
                    m_TargetPosition.y = transform.position.y;
                }

            }

            if (Vector3.Distance(m_LastPosition, transform.position) > m_Obstacle.carvingMoveThreshold && m_TargetReached)
                Move(m_TargetedPosition);
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
            if (m_VillagerType != VillagerType.Idle)
                m_ClipPlayer.SetClips(m_Playlists[(int)m_VillagerType - 1].Musics);
        }

        private void HandleGathering()
        {
            if (m_AssignedResource != null && m_TargetReached)
            {
                LookToTarget(m_AssignedResource.transform.position);

                m_GatherTimer += Time.deltaTime;
                if (m_GatherTimer >= 1.0f)
                {
                    m_AssignedResource.Gather(m_GatherRatePerSecond);
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
                m_Animator.SetBool(s_WorkHash, m_TargetReached && m_Obstacle.enabled);
                m_Animator.SetFloat(s_WorkIndexHash, (int)m_VillagerType - 1);
            }
            else
                m_Animator.SetBool(s_WorkHash, false);
        }

        public static ResourceType GetResourceTypeFromVillager(VillagerType villagerType)
        {
            return villagerType switch
            {
                VillagerType.Lumberjack => ResourceType.Wood,
                VillagerType.Farmer => ResourceType.Food,
                VillagerType.Miner => ResourceType.Stone,
                VillagerType.Gatherer => ResourceType.Food,
                _ => ResourceType.None
            };
        }

        private void CalculateGatherRate()
        {
            if (!m_AssignedResource)
                return;

            m_GatherRatePerSecond = m_AssignedResource.GatherRatePerSecond;
            m_GatherRatePerSecond += m_GatherRatePerSecond * GetGatherImprovement(m_VillagerType);
        }

        private float GetGatherImprovement(VillagerType type)
        {
            return type switch
            {
                VillagerType.Lumberjack => UnitImprovements.LumberjackSpeed,
                VillagerType.Farmer => UnitImprovements.FarmerSpeed,
                VillagerType.Miner => UnitImprovements.MinerSpeed,
                VillagerType.Gatherer => 0.0f,
                _ => 0.0f
            };
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

        private void OnDestroy()
        {
            if (m_AssignedResource != null)
                Unassign();

            if (m_AssignedConstruction != null)
                Unassign();

            if (m_VillagerType == VillagerType.Idle)
                UnitManager.RemoveIdleVillager(this);

            UnitManager.VillagerDestroyed();
            UnitImprovements.OnVillagerImprovement -= CalculateGatherRate;
        }

        public void SaveState(string guid)
        {
            string assignedResourceGUID = string.Empty;
            string assignedConstructionGUID = string.Empty;

            if (m_AssignedResource != null)
                if (m_AssignedResource.TryGetComponent(out GUIDComponent resourceGUID))
                    assignedResourceGUID = resourceGUID.GUID;

            if (m_AssignedConstruction != null)
                if (m_AssignedConstruction.TryGetComponent(out GUIDComponent constructionGUID))
                    assignedConstructionGUID = constructionGUID.GUID;

            VillagerData data = new VillagerData
            {
                GUID = guid,
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles,
                AssignedResourceGUID = assignedResourceGUID,
                AssignedConstructionGUID = assignedConstructionGUID,
                GatherTimer = m_GatherTimer,
                Gender = (int)m_Gender
            };

            SaveManager.SaveToVillagerData(data);
        }

        public void LoadState(object saveData)
        {
            VillagerData villagerData = (VillagerData)saveData;

            if (TryGetComponent(out GUIDComponent guid))
                guid.SetGuid(villagerData.GUID);

            m_TargetPosition = villagerData.Position;

            if (!string.IsNullOrEmpty(villagerData.AssignedResourceGUID))
            {
                if (SaveManager.TryFindFromGUID(villagerData.AssignedResourceGUID, out SaveableEntity entity))
                {
                    if (entity.TryGetComponent(out UnitResource resource))
                    {
                        if (resource.TryGetComponent(out Collider collider))
                        {
                            Assign(resource, collider.ClosestPoint(transform.position));
                            m_GatherTimer = villagerData.GatherTimer;
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(villagerData.AssignedConstructionGUID))
            {
                if (SaveManager.TryFindFromGUID(villagerData.AssignedConstructionGUID, out SaveableEntity entity))
                {
                    if (entity.TryGetComponent(out UnitConstruction construction))
                    {
                        if (construction.TryGetComponent(out Collider collider))
                            AssignBuilder(construction, collider.ClosestPoint(transform.position));
                    }
                }
            }
        }
    }
}
