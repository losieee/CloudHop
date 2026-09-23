using UnityEngine;

namespace CloudHop
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float lookAhead = 3f;
        [SerializeField] private float fixedY = 1f;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;
        private float velocity;
        private void LateUpdate()
        {
            float x = Mathf.SmoothDamp(transform.position.x, target.position.x + lookAhead, ref velocity, smoothTime);
            transform.position = new Vector3(x, fixedY, -10f);
        }
        public void ResetPosition()
        {
            velocity = 0f;
            transform.position = new Vector3(target.position.x + lookAhead, fixedY, -10f);
        }
    }
}
