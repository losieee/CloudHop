using UnityEngine;

namespace CloudHop
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float lookAhead = 3f;
        [SerializeField] private float fixedY = 1f;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;
        private float velocity, verticalVelocity, restingY;
        private PlayerController player;
        private StageDirector stages;
        private void Awake() { player = target.GetComponent<PlayerController>(); stages = FindFirstObjectByType<StageDirector>(); restingY = fixedY; }
        private void LateUpdate()
        {
            float x = Mathf.SmoothDamp(transform.position.x, target.position.x + lookAhead, ref velocity, smoothTime);
            if (stages != null && stages.CurrentCourse.Definition.followElevation && player != null)
            {
                if (player.IsGrounded && player.GroundPlatform != null)
                    restingY = Mathf.Max(fixedY, player.GroundPlatform.GetComponent<Collider2D>().bounds.max.y + fixedY - 2.6f);
            }
            else restingY = fixedY;
            float y = Mathf.SmoothDamp(transform.position.y, restingY, ref verticalVelocity, .35f);
            transform.position = new Vector3(x, y, -10f);
        }
        public void ResetPosition()
        {
            velocity = verticalVelocity = 0f; restingY = fixedY;
            transform.position = new Vector3(target.position.x + lookAhead, fixedY, -10f);
        }
    }
}
