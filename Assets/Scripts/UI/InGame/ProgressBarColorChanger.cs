using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ProgressBarColorChanger : MonoBehaviour
{
    [Tooltip("������ ������ Slider�� Fill ������ �ִ� Image ������Ʈ")]
    public Image fillImage; // Inspector���� ������ Fill Image

    [Tooltip("��(0~1)�� ���� ���� ���� Gradient")]
    public Gradient colorGradient; // Inspector���� ������ Gradient

    private Slider slider; // Slider ������Ʈ ����

    void Awake()
    {
        // Slider ������Ʈ ��������
        slider = GetComponent<Slider>();

        // Slider �� ���� �̺�Ʈ�� ������(UpdateColor �Լ�) �߰�
        slider.onValueChanged.AddListener(UpdateColor);
    }

    void Start()
    {
        // ���� �� �ʱ� ���� ����
        UpdateColor(slider.value);
    }

    // Slider ���� ����� �� ȣ��� �Լ�
    void UpdateColor(float value)
    {
        if (fillImage != null && colorGradient != null)
        {
            // Gradient���� ���� ��(value)�� �ش��ϴ� ���� ��������
            fillImage.color = colorGradient.Evaluate(value);
        }
        else
        {
            if (fillImage == null)
            {
                Debug.LogWarning("Fill Image�� �������� �ʾҽ��ϴ�.", this);
            }
            if (colorGradient == null)
            {
                Debug.LogWarning("Color Gradient�� �������� �ʾҽ��ϴ�.", this);
            }
        }
    }

    // ��ũ��Ʈ�� ��Ȱ��ȭ�ǰų� ������Ʈ�� �ı��� �� ������ ���� (�޸� ���� ����)
    void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(UpdateColor);
        }
    }

    // --- �׽�Ʈ�� �ڵ� (���� ����) ---
    // �ʿ信 ���� �� �κ��� ����Ͽ� �ܺο��� ProgressBar ���� ������ �� �ֽ��ϴ�.
    /*
    [Range(0f, 1f)]
    public float testValue = 0.5f;

    void Update()
    {
        // �׽�Ʈ ���� �ǽð����� �����̴��� �ݿ� (���� ���ӿ����� �ٸ� �������� ���� �����ؾ� ��)
        if (Application.isPlaying && slider != null && slider.value != testValue)
        {
             slider.value = testValue;
        }
    }
    */

    // �ܺ� ��ũ��Ʈ���� ProgressBar ���� �����ϴ� ���� �Լ�
    public void SetProgress(float progress)
    {
        if (slider != null)
        {
            // slider.value �� ���� �����ϸ� onValueChanged �̺�Ʈ�� �ڵ����� �߻��Ͽ� UpdateColor�� ȣ��˴ϴ�.
            slider.value = Mathf.Clamp01(progress); // ���� 0�� 1 ���̷� ����
        }
    }
}