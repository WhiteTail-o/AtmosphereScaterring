using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableShadowCaster : MonoBehaviour
{
    public Material[] mats;
    void Start()
    {
        foreach (Material m in mats) {
            if (m) {
                m.SetShaderPassEnabled("", true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
