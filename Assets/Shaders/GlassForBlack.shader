Shader "Custom/GlassForBlack"
{
    Properties
    {
        _MainTex ("Texture (RGBA)", 2D) = "white" {} // PNG �ؽ�ó
        _ReflectionCube ("Reflection Cubemap", Cube) = "_Skybox" {} // �ݻ�� ť���
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 1.0 // �ݻ� ����
        _BlackThreshold ("Black Threshold", Range(0, 0.5)) = 0.05 // '������'���� ������ ��� �Ӱ谪
        _Color ("Tint Color", Color) = (1,1,1,1) // �ؽ�ó�� ������ ���� (���� ����)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200

        ZWrite Off // ���� ��ü�� ���� ZWrite�� ���ϴ�.
        Blend SrcAlpha OneMinusSrcAlpha // �Ϲ����� ���� ����

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // ����Ƽ ���� �Լ� ����

            // Properties���� ������ ���� ����
            sampler2D _MainTex;
            samplerCUBE _ReflectionCube;
            float _ReflectionStrength;
            float _BlackThreshold;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION; // ���� ��ġ
                float2 uv : TEXCOORD0;    // UV ��ǥ
                float3 normal : NORMAL;   // ���� ����
            };

            struct v2f // Vertex to Fragment (�������� �����׸�Ʈ�� ���޵� ������)
            {
                float4 vertex : SV_POSITION; // Ŭ�� ���� ��ġ
                float2 uv : TEXCOORD0;       // UV ��ǥ
                float3 worldRefl : TEXCOORD1; // ���� ���� �ݻ� ����
                float3 worldNormal : NORMAL; // ���� ���� ���� (�����/Ȯ���)
            };

            // Vertex Shader: ���� ó��
            v2f vert (appdata v)
            {
                v2f o;
                // ���� ��ġ�� Ŭ�� �������� ��ȯ
                o.vertex = UnityObjectToClipPos(v.vertex);
                // UV ��ǥ ����
                o.uv = v.uv;

                // ���� ���������� ��ġ ���
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // ���� ���� ���� ��� �� ����ȭ
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                // ���� ���� �� ���� ��� (ī�޶󿡼� ��������)
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                // ���� ���� �ݻ� ���� ���
                o.worldRefl = reflect(-worldViewDir, o.worldNormal);

                return o;
            }

            // Fragment Shader: �ȼ� ó��
            fixed4 frag (v2f i) : SV_Target
            {
                // ���� �ؽ�ó ���ø�
                fixed4 texColor = tex2D(_MainTex, i.uv);
                // ƾƮ ����
                texColor *= _Color;

                // �ؽ�ó ������ ��� ��� (�����ϰ� RGB �� �ִ밪 ���)
                float brightness = max(texColor.r, max(texColor.g, texColor.b));

                fixed4 finalColor;

                // ��Ⱑ �Ӱ谪(_BlackThreshold) �����̸� ���������� ����
                if (brightness <= _BlackThreshold)
                {
                    // ť��ʿ��� �ݻ� ���� ���ø�
                    fixed4 reflection = texCUBE(_ReflectionCube, i.worldRefl);
                    // �ݻ� ���� ����
                    finalColor.rgb = reflection.rgb * _ReflectionStrength;
                    // ������ �κ��� �������ϰ� �ݻ� (�ؽ�ó ���� ����)
                    finalColor.a = 1.0 * _Color.a; // ƾƮ�� ���Ĵ� �ݿ�
                }
                else // �������� �ƴϸ�
                {
                    // ���� �ؽ�ó ���� ���
                    finalColor.rgb = texColor.rgb;
                    // ���� �ؽ�ó�� ���İ� ���
                    finalColor.a = texColor.a;
                }

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit" // �������� �ʴ� �ϵ������� ��ü ���̴�
}
