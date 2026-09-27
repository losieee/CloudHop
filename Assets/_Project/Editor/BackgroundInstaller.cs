using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class BackgroundInstaller
    {
        private const string Root="Assets/_Project/";
        public static void Install()
        {
            var sprites=new Sprite[6];
            for(int i=0;i<6;i++)
            {
                string path=Root+"Art/Backgrounds/Background"+(i+1)+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=100;
                importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;
                importer.wrapMode=TextureWrapMode.Clamp;
                importer.filterMode=FilterMode.Bilinear;
                importer.maxTextureSize=4096;
                importer.SaveAndReimport();
                sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            foreach(string path in Directory.GetFiles(Root+"Scenes","*.unity"))
            {
                var scene=EditorSceneManager.OpenScene(path);
                var camera=Camera.main;
                // This installer owns only this named background hierarchy.
                var old=GameObject.Find("Layered Background");
                if(old!=null) Object.DestroyImmediate(old);
                var root=new GameObject("Layered Background");
                root.AddComponent<BackgroundSky>().Configure(camera);
                var material=Object.FindFirstObjectByType<Platform>().GetComponent<SpriteRenderer>().sharedMaterial;
                Layer(root,camera,material,sprites[5],"01 Distant Mountains",34,30,.08f,new Vector2(0,-.5f),-100,.8f);
                Layer(root,camera,material,sprites[4],"02 Distant Castle",10,45,.12f,new Vector2(8,2),-90,.85f);
                Layer(root,camera,material,sprites[3],"03 Floating Islands",5.5f,17,.22f,new Vector2(2,.8f),-80,.65f);
                Layer(root,camera,material,sprites[1],"04 High Clouds",7,23,.30f,new Vector2(-5,4),-70,.7f);
                Layer(root,camera,material,sprites[2],"05 Mid Clouds",6,19,.40f,new Vector2(5,-1.5f),-60,.55f);
                Layer(root,camera,material,sprites[0],"06 Lower Cloud Sea",32,24,.50f,new Vector2(0,-3.8f),-50,.95f);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        private static void Layer(GameObject root,Camera camera,Material material,Sprite sprite,string name,
            float width,float distance,float speed,Vector2 offset,int order,float alpha)
        {
            var layer=new GameObject(name);
            layer.transform.SetParent(root.transform);
            var tiles=new Transform[7];
            for(int i=0;i<tiles.Length;i++)
            {
                var go=new GameObject("Tile "+i);
                go.transform.SetParent(layer.transform);
                var renderer=go.AddComponent<SpriteRenderer>();
                renderer.sprite=sprite;
                renderer.sharedMaterial=material;
                renderer.sortingOrder=order;
                renderer.color=new Color(1,1,1,alpha);
                go.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);
                tiles[i]=go.transform;
            }
            layer.AddComponent<BackgroundLayer>().Configure(camera,speed,distance,offset,tiles);
        }
    }
}
