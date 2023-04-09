using UnityEngine;
using MoonBorn.Utils;

namespace MoonBorn.BePrepared.Gameplay.Player
{
    public class CameraController : Singleton<CameraController>
    {
        public static bool IsFocused => Instance.m_FocusTransform != null;

        [Header("References")]
        [SerializeField] private Transform m_CameraTransform;
        [SerializeField] private Transform m_FocusTransform = null;

        [Header("Movement")]
        [SerializeField] private float m_NormalSpeed = 25.0f;
        [SerializeField] private float m_FastSpeed = 75.0f;
        [SerializeField] private float m_MoveSmoothness = 5.0f;
        private float m_MoveSpeed = 0.0f;
        private Vector3 m_TargetPosition = Vector3.zero;

        [Header("Rotation")]
        [SerializeField] private float m_RotationSpeed = 150.0f;
        [SerializeField] private float m_RotationSmoothness = 5.0f;
        private Quaternion m_TargetRotation = Quaternion.identity;

        [Header("Zoom")]
        [SerializeField] private Vector3 m_ZoomSpeed = new Vector3(0.0f, 50.0f, -50.0f);
        [SerializeField] private float m_ZoomSmoothness = 5.0f;
        private Vector3 m_TargetZoom;

        [Header("Mouse Drag")]
        private Vector3 m_DragStartPos = Vector3.zero;
        private Vector3 m_DragCurrentPos = Vector3.zero;
        private Vector3 m_DragStartRot = Vector3.zero;
        private Vector3 m_DragCurrentRot = Vector3.zero;

        [Header("Restrictions")]
        [SerializeField] private bool m_RestrictPosition = false;
        [SerializeField] private Vector2 m_PositionClampX = new Vector2(-50.0f, 50.0f);
        [SerializeField] private Vector2 m_PositionClampZ = new Vector2(-50.0f, 50.0f);
        [SerializeField] private Vector2 m_ZoomClamp = new Vector2(5.0f, 25.0f);

        private void Awake()
        {
            m_TargetPosition = transform.position;
            m_TargetRotation = transform.rotation;
            m_TargetZoom = m_CameraTransform.localPosition;
        }

        private void Update()
        {
            if (m_FocusTransform != null)
                m_TargetPosition = m_FocusTransform.position;
            else
                HandleMovement();

            HandleZoom();
            HandleRotation();
        }

        private void LateUpdate()
        {
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, m_TargetPosition, m_MoveSmoothness * Time.deltaTime),
                Quaternion.Lerp(transform.rotation, m_TargetRotation, m_RotationSmoothness * Time.deltaTime));

            m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, m_TargetZoom, m_ZoomSmoothness * Time.deltaTime);
        }

        private void HandleMovement()
        {
            m_MoveSpeed = Input.GetKey(KeyCode.LeftShift) ? m_FastSpeed : m_NormalSpeed;
            m_MoveSpeed *= Time.deltaTime;

            if (Input.GetKey(KeyCode.W))
                m_TargetPosition += transform.forward * m_MoveSpeed;
            else if (Input.GetKey(KeyCode.S))
                m_TargetPosition -= transform.forward * m_MoveSpeed;

            if (Input.GetKey(KeyCode.A))
                m_TargetPosition -= transform.right * m_MoveSpeed;
            else if (Input.GetKey(KeyCode.D))
                m_TargetPosition += transform.right * m_MoveSpeed;

            if (Input.GetMouseButtonDown(2))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (plane.Raycast(ray, out float entry))
                    m_DragStartPos = ray.GetPoint(entry);
            }

            if (Input.GetMouseButton(2))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (plane.Raycast(ray, out float entry))
                {
                    m_DragCurrentPos = ray.GetPoint(entry);
                    m_TargetPosition = transform.position + m_DragStartPos - m_DragCurrentPos;
                }
            }

            if (m_RestrictPosition)
            {
                m_TargetPosition.x = Mathf.Clamp(m_TargetPosition.x, m_PositionClampX.x, m_PositionClampX.y);
                m_TargetPosition.z = Mathf.Clamp(m_TargetPosition.z, m_PositionClampZ.x, m_PositionClampZ.y);
            }
        }

        private void HandleRotation()
        {
            if (Input.GetKey(KeyCode.Q))
                m_TargetRotation *= Quaternion.Euler(Vector3.up * m_RotationSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.E))
                m_TargetRotation *= Quaternion.Euler(Vector3.up * -m_RotationSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetMouseButtonDown(1))
                    m_DragStartRot = Input.mousePosition;

                if (Input.GetMouseButton(1))
                {
                    m_DragCurrentRot = Input.mousePosition;
                    Vector3 diff = m_DragStartRot - m_DragCurrentRot;

                    m_DragStartRot = m_DragCurrentRot;
                    m_TargetRotation *= Quaternion.Euler(Vector3.up * (-diff.x * 10.0f * Time.deltaTime));
                }
            }
        }

        private void HandleZoom()
        {
            Quaternion newQuat = Quaternion.identity;
            newQuat.x = m_CameraTransform.rotation.x;
            newQuat.y = transform.rotation.y;

            float zoomMagnitude = m_TargetZoom.magnitude;

            if (Input.mouseScrollDelta.y > 0.1f && zoomMagnitude > m_ZoomClamp.x)
                m_TargetZoom -= m_ZoomSpeed * Time.deltaTime;
            else if (Input.mouseScrollDelta.y < -0.1f && zoomMagnitude < m_ZoomClamp.y)
                m_TargetZoom += m_ZoomSpeed * Time.deltaTime;
        }

        public static void FocusTarget(Transform target)
        {
            Instance.m_FocusTransform = target;
        }

        public static void Unfocus()
        {
            Instance.m_FocusTransform = null;
        }

        public static void MoveToTarget(Vector3 target)
        {
            Instance.m_TargetPosition = target;
        }
    }
}
