using UnityEngine;
namespace CloudHop
{
    // World-sized platform: flat entry, downhill ramp, separate flat exit platform.
    public sealed class SlidingSlope : MonoBehaviour
    {
        public float DownhillAngle(float x) => x < transform.position.x + 1.7f ? 0 : -52.22f;
        public Vector2 VelocityAt(float x) => new Vector2(3.5f, x < transform.position.x + 1.7f ? -.25f : -4.8f);
    }
}
