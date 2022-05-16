Shader "Unlit/Healtbar"
{
    Properties
    {
       [NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
       _HealtBarValue("Healt Bar",Range(0,1)) = 1
       _BorderSize("Border Size",Range(0,0.5))=0.1
    }
        SubShader
       {
           Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

           Pass
           {
               ZWrite Off
               // src * srcAlpha + dst * (1-srcAlpha)   -> dst = background , src = output color
               Blend SrcAlpha OneMinusSrcAlpha // this is alpha blending or fade 
               CGPROGRAM
               #pragma vertex vert
               #pragma fragment frag



               #include "UnityCG.cginc"

               struct meshData
               {
                   float4 vertex : POSITION;
                   float2 uv : TEXCOORD0;
               };

               struct Interpolator
               {
                   float2 uv : TEXCOORD0;

                   float4 vertex : SV_POSITION;
               };

               sampler2D _MainTex;
               float _HealtBarValue;
               float _BorderSize;

               Interpolator vert(meshData v)
               {
                   Interpolator o;
                   o.vertex = UnityObjectToClipPos(v.vertex);
                   o.uv = v.uv;

                   return o;
               }
               float InverseLerp(float a, float b, float c) {
                   return (c - a) / (b - a);
               }

               float4 Flash(float2 uv) {
                   //clip(healtBarMask-0.5f); // arkaplandaki siyah background'u render etmiyor. Pixelleri öldürüyor
                                  float healtBarMask = _HealtBarValue > uv.x;
                                  float3 healtBarColor = tex2D(_MainTex, float2(_HealtBarValue,uv.y));
                                  if (_HealtBarValue < 0.2) {
                                  float flash = cos(_Time.y * 4) * 0.2 + 1;
                                  healtBarColor *= flash;
                                  }

                                  return float4(healtBarColor * healtBarMask,1);
                              }

                              float4 HealtBarColorReturn(float2 uv) {
                                  float healtBarMask = _HealtBarValue > floor(uv.x);
                                  float3 bgColor = float3(0,0,0);
                                  float3 healtTexture = tex2D(_MainTex, float2(_HealtBarValue,uv.y));
                                  float3 outColor = lerp(bgColor,healtTexture,healtBarMask);

                                  //You can add some opacity under this line
                                  //return float4(healtBarColor,healtBarMask*0.5);

                                  return float4(outColor,healtBarMask);

                              }

                              float4 RoundedCornerClipping(float2 uv) {
                                  float2 coords = uv;
                                  coords.x *= 8;

                                  float2 pointLineSegment = float2(clamp(coords.x,0.5,7.5),0.5);

                                  float sdf = distance(coords,pointLineSegment) * 2 - 1;
                                  clip(-sdf);

                                  //Border Sdf
                                      //Threashold oluþturuyoruz sdf + border size
                                      float borderSdf = sdf + _BorderSize;
                                      float pd = fwidth(borderSdf); // screen space partial derivative

                                      float borderMask = 1 - saturate(borderSdf / pd);
                                      //return float4(borderMask.xxx,1);
                                  //EndBorderSdf


                                  float healtBarMask = _HealtBarValue > uv.x;
                                  float3 healtBarColor = tex2D(_MainTex, float2(_HealtBarValue,uv.y));

                                  if (_HealtBarValue < 0.5) {
                                  float flash = cos(_Time.y * 4) * 0.2 + 1;
                                  healtBarColor *= flash;
                                  }

                                  return float4(healtBarColor * healtBarMask * borderMask,1);
                              }

                              float4 frag(Interpolator i) : SV_Target
                              {
                                  return RoundedCornerClipping(i.uv);

                              /*
                               Eþik Deðer Belirliyoruz.
                               _HealtBarValue<=0.2 0'a eþit olsun
                               _HealtBarValue>=0.8 1'e eþit olsun.
                               Saturate methodu ise bu methodu 0 ve 1 arasýna sýkýþtýrýyor.
                              */
                              //float threashold =saturate(InverseLerp(0.2,0.8,_HealtBarValue));

                            //HealtBar'ý 5 parçaya bölüyoruz.
                                //float healtBarMask = _HealtBarValue>floor(i.uv.x*5)/5;


                            //return HealtBarColorReturn(i.uv);


                            //return Flash(i.uv);

                        }
                        ENDCG
                    }
       }
}
