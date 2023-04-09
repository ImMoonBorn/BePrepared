using System.Collections;
using UnityEngine;

public class SnapshotTaker : MonoBehaviour
{
    [SerializeField] private Canvas m_SnapshotCanvas;
    [SerializeField] private Camera m_SnapshotCamera;
    [SerializeField] private string m_FileName = "Snapshot";
    private bool m_IsSnapshoting = false;
    private bool m_DoSnapShot = false;

    private void Update()
    {
        if (m_DoSnapShot)
            DoSnapshot();

        if (Input.GetKeyDown(KeyCode.T) && !m_IsSnapshoting)
            PrepareSnapshot();
    }

    private void PrepareSnapshot()
    {
        m_DoSnapShot = true;

        if (m_SnapshotCanvas != null)
            m_SnapshotCanvas.enabled = false;
    }

    private void DoSnapshot()
    {
        m_DoSnapShot = false;

        int height = Screen.height;
        m_SnapshotCamera.targetTexture = RenderTexture.GetTemporary(height, height, 16);

        StartCoroutine(OnSnapshot());
    }

    private IEnumerator OnSnapshot()
    {
        if (m_IsSnapshoting)
            yield break;

        m_IsSnapshoting = true;

        yield return new WaitForEndOfFrame();

        string filePath = $"{Application.dataPath}/Snapshots/{m_FileName}.png";

        RenderTexture renderTexture = m_SnapshotCamera.targetTexture;

        Texture2D renderResult = new Texture2D(renderTexture.height, renderTexture.height, TextureFormat.ARGB32, false);
        Rect rect = new Rect(512.0f, 0.0f, renderTexture.height, renderTexture.height);
        renderResult.ReadPixels(rect, 0, 0);

        byte[] bytes = renderResult.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);

        RenderTexture.ReleaseTemporary(renderTexture);
        m_SnapshotCamera.targetTexture = null;
        print("Screenshot Captured: " + filePath);

        m_IsSnapshoting = false;

        if (m_SnapshotCanvas != null)
            m_SnapshotCanvas.enabled = true;
    }
}
