using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class LutBuilder : EditorWindow
{   
    string sourcePath;
    public enum LutShader {
        PrecomputeAtmosphere,
        PreIntergrateSkin,
        test
    }

    LutShader lutShader;
    Shader shader;
    string test;

    int kernelId;

    [MenuItem("Image/LutBuilder")]
    static void SetReadWriteTrue()
    {
        LutBuilder lutBuilder = EditorWindow.GetWindow(typeof(LutBuilder)) as LutBuilder;
    }

    void OnGUI() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Source lut:");
        sourcePath = (string)EditorGUILayout.DelayedTextField(sourcePath);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Mode：");
        lutShader = (LutShader)EditorGUILayout.EnumPopup(lutShader);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Start")) {
            Execute();
        }
    }

    void Execute() {
        Material material = null;
        Texture2D inputTex2d = Resources.Load("Luts/Lut_init32") as Texture2D;
        ComputeShader computeShader = Resources.Load("ComputeShaders/PreIntegratedSkin") as ComputeShader;
        switch (lutShader)
        {
            case LutShader.PrecomputeAtmosphere:

                break;

            case LutShader.PreIntergrateSkin:
                
                material = new Material(Shader.Find("Unlit/Copy"));

                if (material && computeShader) {
                    kernelId = computeShader.FindKernel("PreIntegratedSkin");
                    
                    RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(256, 256, RenderTextureFormat.ARGB32, 0);
                    renderTextureDescriptor.sRGB = false;

                    RenderTexture rt = new RenderTexture(renderTextureDescriptor);
                    rt.Create();

                    RenderTexture tmpRt = RenderTexture.GetTemporary(renderTextureDescriptor);
                    tmpRt.enableRandomWrite = true;

                    computeShader.SetTexture(kernelId, "Result", tmpRt);
                    material.mainTexture = tmpRt;
                    computeShader.Dispatch(kernelId, 256/8, 256/8, 1);

                    Graphics.Blit(tmpRt, rt, material);

                    Texture2D outputTex2d = new Texture2D(rt.width, rt.height);
                    
                    outputTex2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
                    byte[] dataBytes = outputTex2d.EncodeToPNG();
                    string savePath = Application.dataPath + "/Resources/Luts/SkinLut.png";
                    FileStream fileStream = File.Open(savePath,FileMode.OpenOrCreate);
                    fileStream.Write(dataBytes,0,dataBytes.Length);
                    fileStream.Close();
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    tmpRt.Release();
                    Debug.Log("Finish");
                }

                break;
            
            case LutShader.test:
                shader = Shader.Find("LutBuilder/Test");
                material = new Material(shader);

                if (material && inputTex2d) {
                    Debug.Log("Find");

                    RenderTexture rt = new RenderTexture(inputTex2d.width, inputTex2d.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                    Debug.Log(rt.sRGB);
                    
                    material.SetTexture("_MainTex", inputTex2d);
                    Graphics.Blit(inputTex2d, rt, material);
                    Texture2D outputTex2d = new Texture2D(rt.width, rt.height);
                    
                    outputTex2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
                    byte[] dataBytes = outputTex2d.EncodeToPNG();
                    string savePath = Application.dataPath + "/SampleCircle.png";
                    FileStream fileStream = File.Open(savePath,FileMode.OpenOrCreate);
                    fileStream.Write(dataBytes,0,dataBytes.Length);
                    fileStream.Close();
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    rt.Release();
                    Debug.Log("Finish");
                }
                break;
        }

    }
    
    private void CreateSampleSprite()
    {
        
        // int minRadius = 64;
        // int maxRadius = 128;
 
        // //图片尺寸
        // int spriteSize = maxRadius * 2;
        // //创建Texture2D
        // Texture2D texture2D = new Texture2D(spriteSize,spriteSize);
        // //图片中心像素点坐标
        // Vector2 centerPixel = new Vector2(maxRadius,maxRadius);
        // //遍历像素点
        // Vector2 tempPixel;
        // float tempDis;
        // for(int x = 0; x < spriteSize; x++)
        // {
        //     for(int y = 0; y < spriteSize; y++)
        //     {
        //         //以中心作为起点，获取像素点向量
        //         tempPixel.x = x - centerPixel.x;
        //         tempPixel.y = y - centerPixel.y;
        //         //是否在半径范围内
        //         tempDis = tempPixel.magnitude;
        //         if(tempDis >= minRadius && tempDis <= maxRadius)
        //             texture2D.SetPixel(x,y,Color.red);
        //         else
        //             texture2D.SetPixel(x,y,Color.white);
        //     }
        // }
        // texture2D.Apply();
        // //保存图片
        // byte[] dataBytes = texture2D.EncodeToPNG();
        // string savePath = Application.dataPath + "/SampleCircle.png";
        // FileStream fileStream = File.Open(savePath,FileMode.OpenOrCreate);
        // fileStream.Write(dataBytes,0,dataBytes.Length);
        // fileStream.Close();
        // UnityEditor.AssetDatabase.SaveAssets();
        // UnityEditor.AssetDatabase.Refresh();
        
    }
}
