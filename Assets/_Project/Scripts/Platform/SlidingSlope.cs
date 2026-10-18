using UnityEngine;
namespace CloudHop
{
    // World-sized platform: flat entry, downhill ramp, separate flat exit platform.
    public sealed class SlidingSlope : MonoBehaviour
    {
        public float DownhillAngle(float x) => x < transform.position.x + .8f ? 0 : -39.23f;
        public Vector2 VelocityAt(float x) => new Vector2(3.5f, x < transform.position.x + .8f ? -.25f : -3.1f);
    }
}
