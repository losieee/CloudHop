using UnityEditor;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class CharacterSelectionInstaller
    {
        public static void Install()
        {
            const string root="Assets/_Project/";
            var assets=AssetDatabase.LoadAssetAtPath<UIAssets>(root+"ScriptableObjects/GameUIAssets.asset");
            var main=AssetDatabase.LoadAssetAtPath<CharacterData>(root+"ScriptableObjects/CloudAdventurer.asset");
            assets.characters=new[]{main,Create("Girl","여자 모험가"),Create("Fox","구름 여우")};
            EditorUtility.SetDirty(assets);
            AssetDatabase.SaveAssets();
            Debug.Log("Character selection installed.");
        }
        private static CharacterData Create(string name,string displayName)
        {
            string path="Assets/_Project/Art/Characters/Additional/"+name+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;
            importer.maxTextureSize=2048;
            importer.isReadable=true;
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels=texture.GetPixels32();
            int minY=texture.height,maxY=0,left=texture.width,right=0;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                if(pixels[y*texture.width+x].a>128){minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
            for(int y=minY;y<=minY+(maxY-minY)*.08f;y++)for(int x=0;x<texture.width;x++)
                if(pixels[y*texture.width+x].a>128){left=Mathf.Min(left,x);right=Mathf.Max(right,x);}
            var settings=new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment=(int)SpriteAlignment.Custom;
            settings.spritePivot=new Vector2((left+right)*.5f/texture.width,(float)minY/texture.height);
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit=(maxY-minY)/1.2f;
            importer.isReadable=false;
            importer.SaveAndReimport();
            string assetPath="Assets/_Project/ScriptableObjects/"+name+"Character.asset";
            var data=AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
            if(data==null){data=ScriptableObject.CreateInstance<CharacterData>();AssetDatabase.CreateAsset(data,assetPath);}
            data.displayName=displayName;
            if (data.chargeSprite == null) data.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            data.overrideVisualTransform=true;
            data.visualScale=Vector3.one;
            data.visualOffset=new Vector3(0,-.45f,0);
            EditorUtility.SetDirty(data);
            return data;
        }
    }
}
