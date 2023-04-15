using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Transform m_Rotatable;
    [SerializeField] private float m_RotationTime = 4.0f;
    [SerializeField] private Vector3 m_TargetRotation = new Vector3(-360.0f, 0.0f, 0.0f);
    private float m_Timer = 0.0f;

    private void Update()
    {
        Vector3 targetRotation = Vector3.Slerp(Vector3.zero, m_TargetRotation, m_Timer / m_RotationTime);
        m_Rotatable.localRotation = Quaternion.Euler(targetRotation);

        m_Timer += Time.deltaTime;
        if (m_Timer >= m_RotationTime)
            m_Timer = 0.0f;
    }
}
