using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorProxy : Singleton<ColorProxy> {
    // 在类中添加公共材质数组
    public Material[] materials; // 在Inspector中拖入不同颜色的材质
    public Material matGray;
    public Material matBlack;

    public void Init() {
        string[] colors = { "Red", "Yellow", "Green" };
        materials = new Material[colors.Length];
        for (int i = 0; i < colors.Length; i++) {
            materials[i] = Resources.Load<Material>("Shader/" + colors[i]);
        }
        matGray = Resources.Load<Material>("Shader/Gray");
        matBlack = Resources.Load<Material>("Shader/Black");
    }

}