using System.Collections.Generic;
using UnityEngine;
namespace CloudHop
{
    // Optional routes connect real scene platforms; every middle landing remains available.
    public sealed class ComboShortcutGuide : MonoBehaviour
    {
        private readonly List<LineRenderer> arcs = new List<LineRenderer>();
        private StageCourse course;
        private Material material;
        public void Configure(StageCourse source)
        {
            if (course != null) return;
            course = source;
            material = new Material(Shader.Find("Sprites/Default"));
            foreach (var route in course.Definition.choiceRoutes)
            {
                var arc = new GameObject("Optional boost route " + route.launchIndex).AddComponent<LineRenderer>();
                arc.transform.SetParent(transform, false);
                arc.sharedMaterial = material;
                arc.useWorldSpace = true; arc.positionCount = 24;
                arc.startWidth = arc.endWidth = .035f;
                arc.startColor = arc.endColor = new Color(1, .87f, .25f, .5f);
                arc.sortingOrder = 2;
                arcs.Add(arc);
            }
        }
        private void LateUpdate()
        {
            if (course == null) return;
            for (int n = 0; n < arcs.Count; n++)
            {
                var route = course.Definition.choiceRoutes[n];
                var from = course.Platforms[route.launchIndex].transform.position;
                var to = course.Platforms[route.rejoinIndex].transform.position;
                for (int i = 0; i < arcs[n].positionCount; i++)
                {
                    float t = (float)i / (arcs[n].positionCount - 1);
                    var point = Vector3.Lerp(from, to, t);
                    point.y += .7f + Mathf.Sin(Mathf.PI * t) * route.arcHeight;
                    arcs[n].SetPosition(i, point);
                }
            }
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
