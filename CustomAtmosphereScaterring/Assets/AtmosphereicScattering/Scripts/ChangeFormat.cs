using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
public class ChangeFormat : EditorWindow
{
    TextureImporterFormat format = TextureImporterFormat.AutomaticCompressed;
    int size = 512;
    BuildTarget buildTarget = BuildTarget.WebGL;
    bool autoSize = false;
    [MenuItem("Image/ChangeFormat")]
    static void SetReadWriteTrue()
    {
        ChangeFormat changeFormat = EditorWindow.GetWindow(typeof(ChangeFormat)) as ChangeFormat;
    }

    void OnGUI()
    {

        GUILayout.BeginHorizontal();
        GUILayout.Label("平台：");
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(buildTarget);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("格式：");
        format = (TextureImporterFormat)EditorGUILayout.EnumPopup(format);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("贴图大小：");
        size = EditorGUILayout.IntField(size);
        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("使用贴图原始大小：");
        //autoSize = EditorGUILayout.Toggle(autoSize);
        //GUILayout.EndHorizontal();

       //GUILayout.BeginHorizontal();
       //GUILayout.Label("平台：");
       //platform = EditorGUILayout.TextField(platform);
       //GUILayout.EndHorizontal();

        if (GUILayout.Button("开始"))
        {
            Execute();
        }

    }


    void Execute()
    {
        Debug.LogWarning("开始");
        UnityEngine.Object[] selectedAsset = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);
        int currentSize = size;


       // for (int i = 0; i < selectedAsset.Length; i++)
       // {
       //     Debug.LogError(selectedAsset[i].name);
       //     Texture2D tex = selectedAsset[i] as Texture2D;
       //     TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
       //     TextureImporterFormat format = ti.GetAutomaticFormat(buildTarget.ToString());
       //
       //     TextureImporterPlatformSettings texx = new TextureImporterPlatformSettings();
       //     texx.maxTextureSize = currentSize;
       //     texx.format = format;
       //     ti.SetPlatformTextureSettings(texx);
       //     texx.overridden = false;
       //     //texx.crunchedCompression = false;
       //     //texx.allowsAlphaSplitting = false;
       //     AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex));
       //     AssetDatabase.Refresh();
       // }


        for (int i = 0; i < selectedAsset.Length; i++)
        {
           
             Debug.LogError(selectedAsset[i].name);
            Texture2D tex = selectedAsset[i] as Texture2D;
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));

            //  //if (autoSize) {
            //  //    currentSize = ti.maxTextureSize;
            //  //}
            //  ti.SetPlatformTextureSettings(platform, currentSize, format);

            //  TextureFormat desiredFormat; 
            //   ColorSpace colorSpace;
            //  int compressoinQuality;
            //  ti.ReadTextureImportInstructions(BuildTarget.Android, out desiredFormat, out colorSpace, out compressoinQuality);

            if (ti == null) continue;

            //TextureImporterFormat formatt = (TextureImporterFormat)Enum.Parse(typeof(TextureImporterFormat), "DXT1") ;
            TextureImporterFormat formatt =  ti.GetAutomaticFormat(BuildTarget.WebGL.ToString());
            string format_str = formatt.ToString();
            Debug.LogError("format_str:" + format_str);
            if (format_str.Contains("Crunched"))//已经是压缩过的；
            {

            }
            else//没有压缩过的 todo
            {
               if(format_str.Contains("DXT1"))
                {
                    formatt = (TextureImporterFormat)Enum.Parse(typeof(TextureImporterFormat), "DXT1Crunched");
                }
                else
                {
                    formatt = (TextureImporterFormat)Enum.Parse(typeof(TextureImporterFormat), "DXT5Crunched");
                }
            }
           

            TextureImporterPlatformSettings texx = new TextureImporterPlatformSettings();
            //if (ti.maxTextureSize < 1024)
            //{
            //    texx.maxTextureSize = ti.maxTextureSize;
            //}
            //else
            //{
            //    texx.maxTextureSize = currentSize;
            //}
            texx.maxTextureSize = ti.maxTextureSize;
            texx.format = formatt;          
            texx.overridden = false;
            ti.SetPlatformTextureSettings(texx);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex));
            AssetDatabase.Refresh();
        }
        Debug.LogWarning("结束");
    }
}