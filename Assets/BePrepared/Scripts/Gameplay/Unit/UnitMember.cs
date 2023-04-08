using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public enum UnitType
    {
        Villager,
        Building,
        Resource
    }

    public class UnitMember : MonoBehaviour
    {
        public UnitSO UnitSO => m_UnitSO;
        public bool IsSelected => m_Selected;

        [SerializeField] private UnitSO m_UnitSO;
        [SerializeField] private GameObject m_SelectionHUD;
        private bool m_Selected = false;

        private void Start()
        {
            m_SelectionHUD.SetActive(false);
        }

        public void Select()
        {
            m_SelectionHUD.SetActive(true);
            m_Selected = true;
            UnitUI.Select(this);
        }

        public void Deselect()
        {
            m_SelectionHUD.SetActive(false);
            m_Selected = false;
            UnitUI.Deselect();
        }

        public void DestroyUnit()
        {
            UnitUI.DeselectIfSelected(this);
            m_Selected = false;
            Destroy(gameObject);
        }
    }
}
