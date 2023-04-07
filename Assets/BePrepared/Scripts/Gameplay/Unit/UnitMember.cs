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

        [SerializeField] private UnitSO m_UnitSO;
        [SerializeField] private GameObject m_SelectionHUD;

        private void Start()
        {
            m_SelectionHUD.SetActive(false);
        }

        public void Select()
        {
            m_SelectionHUD.SetActive(true);
            UnitUI.Select(this);
        }

        public void Deselect()
        {
            m_SelectionHUD.SetActive(false);
            UnitUI.Deselect();
        }

        public void DestroyUnit()
        {
            UnitUI.DeselectIfSelected(this);
            Destroy(gameObject);
        }
    }
}
