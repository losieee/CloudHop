using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class GimmickArtInstaller
    {
        private const string Root="Assets/_Project/";
        public static void Install()
        {
            var sprites=new Sprite[10];
            var sizes=new Vector2[10];
            for(int i=0;i<10;i++)
            {
                string path=Root+"Art/Gimmicks/Gimmick"+(i+1)+".png";
                var texture=new Texture2D(2,2);
                texture.LoadImage(File.ReadAllBytes(path));
                int minX=texture.width,maxX=0,minY=texture.height,maxY=0;
                var pixels=texture.GetPixels32();
                for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                    if(pixels[y*texture.width+x].a>32)
                    {minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
                sizes[i]=new Vector2(maxX-minX+1,maxY-minY+1)/100;
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=100;
                var settings=new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;
                float pivotY=(minY+maxY)*.5f/texture.height;
                if(i==3)pivotY=.56f;
                if(i==4)pivotY=.50f;
                if(i==5)pivotY=.46f;
                if(i==6)pivotY=.56f;
                settings.spritePivot=new Vector2((minX+maxX)*.5f/texture.width,pivotY);
                settings.spriteMeshType=SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;
                importer.maxTextureSize=2048;
                importer.SaveAndReimport();
                Object.DestroyImmediate(texture);
                sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            foreach(string path in Directory.GetFiles(Root+"Scenes","*.unity"))
            {
                var scene=EditorSceneManager.OpenScene(path);
                bool changed=false;
                foreach(var root in scene.GetRootGameObjects())
                    foreach(var mechanic in root.GetComponentsInChildren<Gimmick>(true))
                    {
                        var serialized=new SerializedObject(mechanic);
                        var marker=(SpriteRenderer)serialized.FindProperty("visual").objectReferenceValue;
                        var oldPlatform=mechanic.GetComponent<PlatformArtwork>();
                        if(oldPlatform!=null)oldPlatform.enabled=false;
                        var oldChild=mechanic.transform.Find("PlatformArt");
                        if(oldChild!=null)oldChild.gameObject.SetActive(false);
                        var child=mechanic.transform.Find("GimmickArt");
                        if(child==null){child=new GameObject("GimmickArt").transform;child.SetParent(mechanic.transform,false);}
                        var renderer=child.GetComponent<SpriteRenderer>();
                        if(renderer==null)renderer=child.gameObject.AddComponent<SpriteRenderer>();
                        renderer.sharedMaterial=marker.sharedMaterial;
                        renderer.sortingOrder=2;
                        var art=mechanic.GetComponent<GimmickArtwork>();
                        if(art==null)art=mechanic.gameObject.AddComponent<GimmickArtwork>();
                        art.Configure(mechanic,marker,renderer,sprites,sizes);
                        EditorUtility.SetDirty(art);
                        if(PrefabUtility.IsPartOfPrefabInstance(mechanic))
                        {
                            if(oldPlatform!=null)PrefabUtility.RecordPrefabInstancePropertyModifications(oldPlatform);
                            if(oldChild!=null)PrefabUtility.RecordPrefabInstancePropertyModifications(oldChild.gameObject);
                        }
                        changed=true;
                    }
                if(changed)EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
