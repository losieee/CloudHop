using UnityEngine;

namespace CloudHop
{
    [DefaultExecutionOrder(190)]
    public sealed class BackgroundSky : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        private void LateUpdate()
        {
            if(targetCamera != null) targetCamera.backgroundColor = new Color(0.22f,0.65f,0.94f);
        }
        public void Configure(Camera camera)
        {
            targetCamera = camera;
            targetCamera.backgroundColor = new Color(0.22f,0.65f,0.94f);
        }
    }
}
