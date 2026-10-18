using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CloudHop.Editor
{
    public static class SlideMapInstaller
    {
        [MenuItem("Cloud Hop/맵/2스테이지 미끄럼 언덕 적용")]
        public static void Install()
        {
            if(EditorApplication.isPlaying) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene(PlatformBuilds.SharedScene);
            var course=Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(c=>c.Definition.difficulty=="NORMAL");
            var data=course.Definition;
            var platform=course.Platforms[29];
            platform.transform.localScale=Vector3.one;
            var box=platform.GetComponent<BoxCollider2D>(); box.enabled=false;
            var art=platform.GetComponent<PlatformArtwork>(); if(art!=null)art.enabled=false;
            foreach(var sr in platform.GetComponentsInChildren<SpriteRenderer>(true))sr.enabled=false;
            var zone=platform.GetComponent<PerfectLandingZone>(); if(zone!=null)zone.enabled=true;
            if(platform.GetComponent<SlidingSlope>()==null)platform.gameObject.AddComponent<SlidingSlope>();
            var polygon=platform.GetComponent<PolygonCollider2D>(); if(polygon==null)polygon=platform.gameObject.AddComponent<PolygonCollider2D>();
            polygon.points=new[]{new Vector2(-1,.3f),new Vector2(.8f,.3f),new Vector2(3.25f,-1.7f),new Vector2(3.25f,-2.6f),new Vector2(-1,-.8f)};
            string folder="Assets/_Project/Art/Slide";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/_Project/Art","Slide");
            var material=AssetDatabase.LoadAssetAtPath<Material>(folder+"/Slide.mat");
            if(material==null){material=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(material,folder+"/Slide.mat");}
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(folder+"/Slope.asset");
            if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,folder+"/Slope.asset");}else mesh.Clear();
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            var rock=new Color(.45f,.48f,.7f);var ice=new Color(.35f,.86f,.96f);
            var outline=new[]{new Vector2(-1,.3f),new Vector2(.8f,.3f),new Vector2(3.25f,-1.7f),new Vector2(3.05f,-2.4f),new Vector2(2.3f,-2.55f),new Vector2(1.4f,-1.9f),new Vector2(.5f,-2f),new Vector2(-.3f,-1.3f),new Vector2(-1,-.8f)};
            for(int i=0;i<outline.Length;i++)
            {
                int n=vertices.Count;
                vertices.Add(new Vector3(.8f,-1,0));vertices.Add(outline[i]);vertices.Add(outline[(i+1)%outline.Length]);
                Color facet=Color.Lerp(rock,new Color(.72f,.8f,.93f),i%3*.22f);
                colors.Add(facet);colors.Add(Color.Lerp(facet,Color.white,.12f));colors.Add(facet);
                triangles.AddRange(new[]{n,n+1,n+2});
            }
            for(int i=0;i<2;i++)
            {
                int n=vertices.Count;Vector2 a=polygon.points[i],b=polygon.points[i+1];
                vertices.Add(a);vertices.Add(b);vertices.Add(b-Vector2.up*.17f);vertices.Add(a-Vector2.up*.17f);
                colors.Add(Color.white);colors.Add(Color.white);colors.Add(ice);colors.Add(ice);
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var visual=platform.transform.Find("Ice Slope");if(visual==null){visual=new GameObject("Ice Slope").transform;visual.SetParent(platform.transform,false);}
            var filter=visual.GetComponent<MeshFilter>();if(filter==null)filter=visual.gameObject.AddComponent<MeshFilter>();filter.sharedMesh=mesh;
            var renderer=visual.GetComponent<MeshRenderer>();if(renderer==null)renderer=visual.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=4;
            // A connected flat floor catches the slide before the next climbing jump.
            SetHeight(course,30,-.6f);SetHeight(course,31,.2f);
            var coins=data.coins.Where(c=>c.position.x<115.1f || c.position.x>124.1f).ToList();
            for(int i=0;i<4;i++){float x=117.2f+i*.55f;coins.Add(new StageCoinLayout{position=new Vector2(x,1.4f-(x-116.9f)*2f/2.45f+.25f),value=1});}
            MeadowMapInstaller.Trail(coins,data.platforms[30].position+Vector2.up*.78f,data.platforms[31].position+Vector2.up*.78f,false);
            data.coins=coins.ToArray();EditorUtility.SetDirty(data);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("SLIDE_INSTALLED");
        }
        static void SetHeight(StageCourse course,int i,float top)
        {
            var p=course.Definition.platforms[i];p.position.y=top-.3f;p.section="SLIDE EXIT";course.Definition.platforms[i]=p;course.Platforms[i].transform.position=p.position;
        }
    }
}
