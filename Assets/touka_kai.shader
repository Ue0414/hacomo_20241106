Shader "Unlit/touka_kai"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Alpha ("Alpha", Range(0,1)) = 0.0  // 初期状態では透明
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha  // アルファブレンドを使用
            SetTexture [_MainTex] { combine texture * primary DOUBLE, texture * primary }
        }
    }
}
