Shader "Custom/GlassForBlack"
{
    Properties
    {
        _MainTex ("Texture (RGBA)", 2D) = "white" {} // PNG 텍스처
        _ReflectionCube ("Reflection Cubemap", Cube) = "_Skybox" {} // 반사용 큐브맵
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 1.0 // 반사 강도
        _BlackThreshold ("Black Threshold", Range(0, 0.5)) = 0.05 // '검정색'으로 간주할 밝기 임계값
        _Color ("Tint Color", Color) = (1,1,1,1) // 텍스처에 곱해질 색상 (선택 사항)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200

        ZWrite Off // 투명 객체는 보통 ZWrite를 끕니다.
        Blend SrcAlpha OneMinusSrcAlpha // 일반적인 알파 블렌딩

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // 유니티 헬퍼 함수 포함

            // Properties에서 선언한 변수 연결
            sampler2D _MainTex;
            samplerCUBE _ReflectionCube;
            float _ReflectionStrength;
            float _BlackThreshold;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION; // 정점 위치
                float2 uv : TEXCOORD0;    // UV 좌표
                float3 normal : NORMAL;   // 법선 벡터
            };

            struct v2f // Vertex to Fragment (정점에서 프래그먼트로 전달될 데이터)
            {
                float4 vertex : SV_POSITION; // 클립 공간 위치
                float2 uv : TEXCOORD0;       // UV 좌표
                float3 worldRefl : TEXCOORD1; // 월드 공간 반사 벡터
                float3 worldNormal : NORMAL; // 월드 공간 법선 (디버깅/확장용)
            };

            // Vertex Shader: 정점 처리
            v2f vert (appdata v)
            {
                v2f o;
                // 정점 위치를 클립 공간으로 변환
                o.vertex = UnityObjectToClipPos(v.vertex);
                // UV 좌표 전달
                o.uv = v.uv;

                // 월드 공간에서의 위치 계산
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // 월드 공간 법선 계산 및 정규화
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                // 월드 공간 뷰 방향 계산 (카메라에서 정점으로)
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                // 월드 공간 반사 벡터 계산
                o.worldRefl = reflect(-worldViewDir, o.worldNormal);

                return o;
            }

            // Fragment Shader: 픽셀 처리
            fixed4 frag (v2f i) : SV_Target
            {
                // 메인 텍스처 샘플링
                fixed4 texColor = tex2D(_MainTex, i.uv);
                // 틴트 적용
                texColor *= _Color;

                // 텍스처 색상의 밝기 계산 (간단하게 RGB 중 최대값 사용)
                float brightness = max(texColor.r, max(texColor.g, texColor.b));

                fixed4 finalColor;

                // 밝기가 임계값(_BlackThreshold) 이하이면 검은색으로 간주
                if (brightness <= _BlackThreshold)
                {
                    // 큐브맵에서 반사 색상 샘플링
                    fixed4 reflection = texCUBE(_ReflectionCube, i.worldRefl);
                    // 반사 강도 적용
                    finalColor.rgb = reflection.rgb * _ReflectionStrength;
                    // 검은색 부분은 불투명하게 반사 (텍스처 알파 무시)
                    finalColor.a = 1.0 * _Color.a; // 틴트의 알파는 반영
                }
                else // 검은색이 아니면
                {
                    // 원래 텍스처 색상 사용
                    finalColor.rgb = texColor.rgb;
                    // 원래 텍스처의 알파값 사용
                    finalColor.a = texColor.a;
                }

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit" // 지원하지 않는 하드웨어에서의 대체 셰이더
}
