using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ProgressBarColorChanger : MonoBehaviour
{
    [Tooltip("색상을 변경할 Slider의 Fill 영역에 있는 Image 컴포넌트")]
    public Image fillImage; // Inspector에서 연결할 Fill Image

    [Tooltip("값(0~1)에 따라 변할 색상 Gradient")]
    public Gradient colorGradient; // Inspector에서 설정할 Gradient

    private Slider slider; // Slider 컴포넌트 참조

    void Awake()
    {
        // Slider 컴포넌트 가져오기
        slider = GetComponent<Slider>();

        // Slider 값 변경 이벤트에 리스너(UpdateColor 함수) 추가
        slider.onValueChanged.AddListener(UpdateColor);
    }

    void Start()
    {
        // 시작 시 초기 색상 설정
        UpdateColor(slider.value);
    }

    // Slider 값이 변경될 때 호출될 함수
    void UpdateColor(float value)
    {
        if (fillImage != null && colorGradient != null)
        {
            // Gradient에서 현재 값(value)에 해당하는 색상 가져오기
            fillImage.color = colorGradient.Evaluate(value);
        }
        else
        {
            if (fillImage == null)
            {
                Debug.LogWarning("Fill Image가 설정되지 않았습니다.", this);
            }
            if (colorGradient == null)
            {
                Debug.LogWarning("Color Gradient가 설정되지 않았습니다.", this);
            }
        }
    }

    // 스크립트가 비활성화되거나 오브젝트가 파괴될 때 리스너 제거 (메모리 누수 방지)
    void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(UpdateColor);
        }
    }

    // --- 테스트용 코드 (선택 사항) ---
    // 필요에 따라 이 부분을 사용하여 외부에서 ProgressBar 값을 설정할 수 있습니다.
    /*
    [Range(0f, 1f)]
    public float testValue = 0.5f;

    void Update()
    {
        // 테스트 값을 실시간으로 슬라이더에 반영 (실제 게임에서는 다른 로직으로 값을 변경해야 함)
        if (Application.isPlaying && slider != null && slider.value != testValue)
        {
             slider.value = testValue;
        }
    }
    */

    // 외부 스크립트에서 ProgressBar 값을 설정하는 예시 함수
    public void SetProgress(float progress)
    {
        if (slider != null)
        {
            // slider.value 를 직접 설정하면 onValueChanged 이벤트가 자동으로 발생하여 UpdateColor가 호출됩니다.
            slider.value = Mathf.Clamp01(progress); // 값을 0과 1 사이로 제한
        }
    }
}