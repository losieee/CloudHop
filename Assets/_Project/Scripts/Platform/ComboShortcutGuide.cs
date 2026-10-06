using UnityEngine;

namespace CloudHop
{
    // The ordinary landing remains available. This arc advertises skipping it, not a mandatory new gap.
    public sealed class ComboShortcutGuide : MonoBehaviour
    {
        private Transform launch, target;
        private LineRenderer arc;
        private Material material;
        public void Configure(StageCourse course)
        {
            if (arc != null) return;
            launch = course.Platforms[3].transform; target = course.Platforms[5].transform;
            arc = new GameObject("Optional boost route").AddComponent<LineRenderer>();
            arc.transform.SetParent(transform, false);
            material = new Material(Shader.Find("Sprites/Default")); arc.sharedMaterial = material;
            arc.useWorldSpace = true; arc.positionCount = 20;
            arc.startWidth = arc.endWidth = .035f;
            arc.startColor = arc.endColor = new Color(1, .87f, .25f, .45f);
            arc.sortingOrder = 2;
        }
        private void LateUpdate()
        {
            if (arc == null) return;
            for (int i = 0; i < arc.positionCount; i++)
            {
                float t = (float)i / (arc.positionCount - 1);
                var position = Vector3.Lerp(launch.position, target.position, t);
                position.y += .7f + Mathf.Sin(Mathf.PI * t) * 2;
                arc.SetPosition(i, position);
            }
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
