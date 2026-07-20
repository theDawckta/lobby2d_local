using UnityEngine;

namespace Game.Environment
{
    // Keeps a quad exactly filling the target camera's view frustum at a fixed distance,
    // regardless of camera movement, FOV, or aspect ratio. Parenting to the camera and placing
    // it far along its forward axis means anything closer to the camera (floor, gameplay)
    // renders in front via normal depth testing -- no manual draw-order bookkeeping needed.
    public class FullScreenBackground : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float distance = 100f;
        [SerializeField] private float marginScale = 1.15f;

        private bool _parented;

        public Camera TargetCamera => targetCamera;
        public float Distance => distance;
        public float MarginScale => marginScale;

        private void OnEnable()
        {
            Reposition();
        }

        private void LateUpdate()
        {
            Reposition();
        }

        public void Reposition()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            if (targetCamera == null)
            {
                return;
            }

            if (!_parented || transform.parent != targetCamera.transform)
            {
                transform.SetParent(targetCamera.transform, false);
                transform.localRotation = Quaternion.identity;
                _parented = true;
            }

            transform.localPosition = new Vector3(0f, 0f, distance);

            float height = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * marginScale;
            float width = height * targetCamera.aspect;
            transform.localScale = new Vector3(width, height, 1f);
        }
    }
}
