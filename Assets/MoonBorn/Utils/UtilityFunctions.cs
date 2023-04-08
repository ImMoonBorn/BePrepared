using UnityEngine;

namespace MoonBorn.Utils
{
    public static class UtilityFunctions
    {
        public static Vector3 GetWorldMousePosition(bool zeroY = true, Vector3 lastPosition = new Vector3())
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                lastPosition = hit.point;
                if (zeroY)
                    lastPosition.y = 0.0f;

                return lastPosition;
            }
            else
                return lastPosition;
        }

        public static Vector3 GetWorldMousePosition(LayerMask layerMask, bool zeroY = true, Vector3 lastPosition = new Vector3())
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                lastPosition = hit.point;
                if (zeroY)
                    lastPosition.y = 0.0f;

                return lastPosition;
            }
            else
                return lastPosition;
        }
    }
}
