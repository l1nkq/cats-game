Shader "Aidolov/TextureColorMixShader"
{
    Properties
    {
        _ColorA ("ColorA", Color) = (1,1,1,1)
        _ColorB ("ColorB", Color) = (1,1,1,1)
        _ColorC ("ColorC", Color) = (1,1,1,1)
        _ColorD ("ColorD", Color) = (1,1,1,1)
        _ColorE ("ColorE", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _TikTak ("TikTak", Int) = -1
        _DisableRecoloring ("DisableRecoloring", Int) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _ColorA;
        fixed4 _ColorB;
        fixed4 _ColorC;
        fixed4 _ColorD;
        fixed4 _ColorE;
        fixed4 _ColorF;
        int _TikTak;
        int _DisableRecoloring;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        bool color_is_equal(fixed4 current_color, fixed4 check_color)
        {
            fixed step = 0.5; //0.5 включает смешение цветов на границе, возможно есть какой-то более оптимальный вариант, но для текущего случая подходит и так
            if(current_color.r < check_color.r + step && current_color.g < check_color.g + step && current_color.b < check_color.b + step
                && current_color.r > check_color.r - step && current_color.g > check_color.g - step && current_color.b > check_color.b - step)
            {
                return true;
            }else
            {
                return false;
            }
            
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //цвета шаблона текстуры
            fixed4 temp_color_a = fixed4(1,1,0,1);
            fixed4 temp_color_b = fixed4(0,0,1,1);
            fixed4 temp_color_c = fixed4(1,0,0,1);
            fixed4 temp_color_d = fixed4(1,1,1,1);
            fixed4 temp_color_e = fixed4(0,0,0,1);
            fixed4 temp_color_f = fixed4(0,1,0,1);

            int multiplier;
            if(sin(_Time.z) > 0 && _TikTak > -1)
            {
                multiplier = -1;
            }
            else
            {
                multiplier = 1;
            }
            
            fixed4 texture_color = tex2D (_MainTex, float2(multiplier*IN.uv_MainTex.x, IN.uv_MainTex.y));
            fixed4 c = texture_color;

            if(_DisableRecoloring == 0)
            {
                //если не отключено перекрашивание
                if(color_is_equal(texture_color, temp_color_a))
                {
                    c = _ColorA;
                }else if(color_is_equal(texture_color, temp_color_b))
                {
                    c = _ColorB;
                }else if(color_is_equal(texture_color, temp_color_c))
                {
                    c = _ColorC;
                }else if(color_is_equal(texture_color, temp_color_d))
                {
                    c = _ColorD;
                }else if(color_is_equal(texture_color, temp_color_e))
                {
                    c = _ColorE;
                }else if(color_is_equal(texture_color, temp_color_f))
                {
                    c = _ColorF;
                }
            }
            
            
            
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
