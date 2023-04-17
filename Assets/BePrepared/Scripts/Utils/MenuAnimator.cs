using MoonBorn.BePrepared.Gameplay.Unit;
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    private Animator m_Animator;
    [SerializeField] private VillagerType m_VillagerType = 0;
    [SerializeField] private GameObject[] Tools;

    [Header("Tools")]
    [SerializeField] private GameObject m_Axe;
    [SerializeField] private GameObject m_Hoe;
    [SerializeField] private GameObject m_Pickaxe;
    [SerializeField] private GameObject m_Hammer;
    private GameObject[] m_Tools;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Animator.SetBool("Work", true);
        m_Animator.SetFloat("WorkIndex", (int)m_VillagerType - 1);

        m_Tools = new GameObject[4];
        m_Tools[0] = m_Axe;
        m_Tools[1] = m_Hoe;
        m_Tools[2] = m_Pickaxe;
        m_Tools[3] = m_Hammer;

        foreach (GameObject tool in m_Tools)
            tool.SetActive(false);

        if (m_VillagerType != VillagerType.Idle && m_VillagerType != VillagerType.Gatherer)
            m_Tools[(int)m_VillagerType - 1].SetActive(true);

        print("mWP");
    }
}
