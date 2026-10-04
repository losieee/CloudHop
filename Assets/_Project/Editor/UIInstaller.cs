using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class UIInstaller
    {
        public static void Install()
        {
            const string root="Assets/_Project/";
            var sprites=Directory.GetFiles(root+"Art/UI","*.png",SearchOption.AllDirectories)
                .Select(path=>{
                    path=path.Replace(Path.DirectorySeparatorChar,'/');
                    var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType=TextureImporterType.Sprite;
                    importer.spriteImportMode=SpriteImportMode.Single;
                    importer.alphaIsTransparency=true;
                    importer.mipmapEnabled=false;
                    importer.maxTextureSize=2048;
                    importer.SaveAndReimport();
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }).ToArray();
            var assets=AssetDatabase.LoadAssetAtPath<UIAssets>(root+"ScriptableObjects/GameUIAssets.asset");
            if(assets==null){assets=ScriptableObject.CreateInstance<UIAssets>();AssetDatabase.CreateAsset(assets,root+"ScriptableObjects/GameUIAssets.asset");}
            assets.sprites=sprites;
            assets.character=AssetDatabase.LoadAssetAtPath<Sprite>(root+"Art/Characters/CloudAdventurer/Idle.png");
            assets.stageArt=new[]{1,8,7}.Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>(root+"Art/Platforms/Platform"+i+".png")).ToArray();
            EditorUtility.SetDirty(assets);
            AssetDatabase.SaveAssets();
            string main=root+"Scenes/CloudHop.unity";
            if(!File.Exists(main))AssetDatabase.CopyAsset(root+"Scenes/CloudHopGimmicks.unity",main);
            var scene=EditorSceneManager.OpenScene(main);
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                canvas.gameObject.SetActive(false);
            foreach(var label in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                label.GetComponent<MeshRenderer>().enabled=false;
            var ui=Object.FindFirstObjectByType<GameUI>();
            if(ui==null)ui=new GameObject("Game UI Flow").AddComponent<GameUI>();
            var serialized=new SerializedObject(ui);
            Set(serialized,"assets",AssetDatabase.LoadAssetAtPath<UIAssets>(root+"ScriptableObjects/GameUIAssets.asset"));
            Set(serialized,"game",Object.FindFirstObjectByType<GameManager>());
            Set(serialized,"scores",Object.FindFirstObjectByType<ScoreManager>());
            Set(serialized,"player",Object.FindFirstObjectByType<PlayerController>());
            Set(serialized,"stages",Object.FindFirstObjectByType<StageDirector>());
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        private static void Set(SerializedObject target,string name,Object value)
            =>target.FindProperty(name).objectReferenceValue=value;
    }
}
