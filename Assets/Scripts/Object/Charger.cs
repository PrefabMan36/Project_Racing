using UnityEngine;

[RequireComponent(typeof(Car))] // 이 컴포넌트는 항상 Car 컴포넌트와 함께 있어야 합니다.
public class Charger : MonoBehaviour
{
    [Header("Charger Type")]
    [SerializeField] private ECHARGERTYPE boosterType = ECHARGERTYPE.eCHARGERTYPE_NONE;

    [Header("Supercharger Settings")]
    [Tooltip("엔진 RPM에 따라 추가되는 토크의 최대 배율")]
    [SerializeField] private float superchargerMaxBoost = 1.4f;

    [Header("Turbocharger Settings")]
    [Tooltip("터보가 제공하는 최대 토크 배율")]
    [SerializeField] private float turboMaxBoost = 1.8f;
    [Tooltip("터보 부스트가 최대로 차오르는 속도")]
    [SerializeField] private float turboSpoolRate = 0.5f;
    [Tooltip("터보가 작동하기 시작하는 최소 RPM 비율")]
    [Range(0.3f, 0.8f)]
    [SerializeField] private float turboMinRpmRatio = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private GameObject superchargerWhineSound_Prefab, turboSpoolSound_Prefab, turboBlowOffSound_Prefab;
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private AudioSource superchargerWhineSound;
    [SerializeField] private AudioSource turboSpoolSound;
    [SerializeField] private AudioSource turboBlowOffSound;
    [SerializeField] private float minPitch = 0.1f;
    [SerializeField] private float maxPitch = 1.0f;

    // Car 클래스가 읽어갈 최종 부스트 배율
    public float currentChargeMultiplier { get; private set; } = 1f;

    // 참조할 Car 클래스
    private Car car;
    private float currentTurboBoost = 0f;
    private bool wasThrottlePressed = false;

    // 인스펙터 값 변경 감지를 위한 변수
    private ECHARGERTYPE previousBoosterType;
    private float previousSuperchargerMaxBoost;
    private float previousTurboMaxBoost;

    private void Start()
    {
        // 같은 게임 오브젝트에 있는 Car 컴포넌트를 자동으로 찾아와 참조합니다.
        car = GetComponent<Player_Car>();
        engineSound = GameObject.Find("EngineSound").GetComponent<AudioSource>();

        // 초기 변속 지점 계산을 요청합니다.
        if (car != null && boosterType != ECHARGERTYPE.eCHARGERTYPE_NONE)
        {
            Debug.Log("Charger detected. Recalculating optimal shift points on start...");
            car.CalculateOptimalShiftPoints();
        }
        InitializeAudio();
    }

    private void Update()
    {
        if (car == null) return;

        // 현재 Car의 상태 값을 가져옵니다.
        float currentRPM = car.GetCurrentRPM();
        float maxRPM = car.GetMaxRPM();
        float throttle = car.GetThrottleInput();

        UpdateBoosterLogic(currentRPM, maxRPM, throttle);
        UpdateAudio(currentRPM, maxRPM, throttle);
    }

    /// <summary>
    /// 과급기 종류에 따라 부스트 로직을 업데이트합니다.
    /// </summary>
    private void UpdateBoosterLogic(float currentRPM, float maxRPM, float throttle)
    {
        switch (boosterType)
        {
            case ECHARGERTYPE.eCHARGERTYPE_SUPERCHARGER:
                currentChargeMultiplier = Mathf.Lerp(1f, superchargerMaxBoost, currentRPM / maxRPM);
                break;

            case ECHARGERTYPE.eCHARGERTYPE_TURBOCHARGER:
                if (currentRPM > maxRPM * turboMinRpmRatio && throttle > 0.5f)
                {
                    currentTurboBoost += Time.deltaTime * turboSpoolRate;
                }
                else
                {
                    currentTurboBoost -= Time.deltaTime * turboSpoolRate * 1.5f;
                }
                currentTurboBoost = Mathf.Clamp(currentTurboBoost, 0, turboMaxBoost - 1f);
                currentChargeMultiplier = 1f + currentTurboBoost;
                break;

            case ECHARGERTYPE.eCHARGERTYPE_TWINCHARGER:
                float superchargerEffect = Mathf.Lerp(1f, superchargerMaxBoost, Mathf.Clamp01(currentRPM / (maxRPM * 0.6f)));
                if (currentRPM > maxRPM * turboMinRpmRatio && throttle > 0.5f)
                {
                    currentTurboBoost += Time.deltaTime * turboSpoolRate;
                }
                else
                {
                    currentTurboBoost -= Time.deltaTime * turboSpoolRate * 1.5f;
                }
                currentTurboBoost = Mathf.Clamp(currentTurboBoost, 0, turboMaxBoost - 1f);
                currentChargeMultiplier = superchargerEffect + currentTurboBoost;
                break;

            default: // EBoosterType.None
                currentChargeMultiplier = 1f;
                break;
        }
    }

    /// <summary>
    /// 과급기 종류에 따른 사운드를 재생합니다.
    /// </summary>
    private void UpdateAudio(float currentRPM, float maxRPM, float throttle)
    {
        // 슈퍼차저: RPM에 따라 윙~ 하는 소리의 높낮이를 조절
        if (superchargerWhineSound != null)
        {
            if (boosterType == ECHARGERTYPE.eCHARGERTYPE_SUPERCHARGER || boosterType == ECHARGERTYPE.eCHARGERTYPE_TWINCHARGER)
            {
                if (!superchargerWhineSound.isPlaying) superchargerWhineSound.Play();
                superchargerWhineSound.pitch = Mathf.Lerp(minPitch, maxPitch, currentRPM / maxRPM);
                superchargerWhineSound.volume = Mathf.Lerp(0.1f, 1.0f, currentRPM / maxRPM);
            }
            else
            {
                superchargerWhineSound.Stop();
            }
        }

        // 터보차저: 스풀업 사운드 및 블로우 오프(피슉~) 사운드 처리
        if (turboSpoolSound != null && turboBlowOffSound != null)
        {
            if (boosterType == ECHARGERTYPE.eCHARGERTYPE_TURBOCHARGER || (boosterType == ECHARGERTYPE.eCHARGERTYPE_TWINCHARGER && currentRPM > maxRPM * turboMinRpmRatio))
            {
                // 스로틀을 밟으면 스풀업 사운드 재생
                if (throttle > 0.5f)
                {
                    if (!turboSpoolSound.isPlaying) turboSpoolSound.Play();
                    turboSpoolSound.pitch = Mathf.Lerp(minPitch, maxPitch, currentTurboBoost / (turboMaxBoost - 1f));
                    turboSpoolSound.volume = Mathf.Lerp(0.1f, 0.3f, currentTurboBoost / (turboMaxBoost - 1f));
                    wasThrottlePressed = true;
                }
                // 스로틀을 떼면 블로우 오프 사운드 재생
                else if (wasThrottlePressed && currentTurboBoost > 0.1f)
                {
                    turboBlowOffSound.Play();
                    wasThrottlePressed = false;
                    turboSpoolSound.Stop();
                }
                else
                {
                    wasThrottlePressed = false;
                    turboSpoolSound.Stop();
                }
            }
            else
            {
                turboSpoolSound.Stop();
            }
        }
    }

    /// <summary>
    /// Car 클래스가 변속 지점을 계산할 때 사용할, 부스트가 적용된 가상 토크 값을 반환합니다.
    /// </summary>
    public float GetBoostedTorque(float rpm, AnimationCurve torqueCurve, float maxRPM)
    {
        if (torqueCurve == null) return 0f;

        float baseTorque = torqueCurve.Evaluate(rpm);
        float simulatedBoost = 1f;

        switch (boosterType)
        {
            case ECHARGERTYPE.eCHARGERTYPE_SUPERCHARGER:
                simulatedBoost = Mathf.Lerp(1f, superchargerMaxBoost, rpm / maxRPM);
                break;
            case ECHARGERTYPE.eCHARGERTYPE_TURBOCHARGER:
                if (rpm > maxRPM * turboMinRpmRatio) simulatedBoost = turboMaxBoost;
                break;
            case ECHARGERTYPE.eCHARGERTYPE_TWINCHARGER:
                float superchargerEff = Mathf.Lerp(1f, superchargerMaxBoost, Mathf.Clamp01(rpm / (maxRPM * 0.6f)));
                float turboEff = (rpm > maxRPM * turboMinRpmRatio) ? turboMaxBoost - 1f : 0f;
                simulatedBoost = superchargerEff + turboEff;
                break;
        }
        return baseTorque * simulatedBoost;
    }

    /// <summary>
    /// 에디터에서 값이 변경될 때마다 Car 클래스에 변속 지점 재계산을 요청합니다.
    /// </summary>
    private void OnValidate()
    {
        if (boosterType != previousBoosterType ||
            superchargerMaxBoost != previousSuperchargerMaxBoost ||
            turboMaxBoost != previousTurboMaxBoost)
        {
            if (Application.isPlaying)
            {
                car = GetComponent<Car>();
                if (car != null)
                {
                    Debug.Log("Charger settings changed. Requesting shift point recalculation...");
                    if (car.SetUpFinished)
                        car.CalculateOptimalShiftPoints();
                }
            }
            previousBoosterType = boosterType;
            previousSuperchargerMaxBoost = superchargerMaxBoost;
            previousTurboMaxBoost = turboMaxBoost;
        }
    }

    private void InitializeAudio()
    {
        switch(boosterType)
        {
            case ECHARGERTYPE.eCHARGERTYPE_SUPERCHARGER:
                superchargerWhineSound = Instantiate(superchargerWhineSound_Prefab, transform).GetComponent<AudioSource>();
                superchargerWhineSound.transform.localPosition = engineSound.transform.localPosition;
                superchargerWhineSound.volume = engineSound.volume;
                break;
            case ECHARGERTYPE.eCHARGERTYPE_TURBOCHARGER:
                turboSpoolSound = Instantiate(turboSpoolSound_Prefab, transform).GetComponent<AudioSource>();
                turboSpoolSound.transform.localPosition = engineSound.transform.localPosition;
                turboSpoolSound.volume = engineSound.volume;
                turboBlowOffSound = Instantiate(turboBlowOffSound_Prefab, transform).GetComponent<AudioSource>();
                turboBlowOffSound.transform.localPosition = turboBlowOffSound.transform.localPosition;
                break;
            case ECHARGERTYPE.eCHARGERTYPE_TWINCHARGER:
                superchargerWhineSound = Instantiate(superchargerWhineSound_Prefab, transform).GetComponent<AudioSource>();
                superchargerWhineSound.transform.localPosition = engineSound.transform.localPosition;
                superchargerWhineSound.volume = engineSound.volume;
                turboSpoolSound = Instantiate(turboSpoolSound_Prefab, transform).GetComponent<AudioSource>();
                turboSpoolSound.transform.localPosition = superchargerWhineSound.transform.localPosition;
                turboSpoolSound.volume = engineSound.volume;
                turboBlowOffSound = Instantiate(turboBlowOffSound_Prefab, transform).GetComponent<AudioSource>();
                turboBlowOffSound.transform.localPosition = turboSpoolSound.transform.localPosition;
                break;
            default:
                superchargerWhineSound = null;
                turboSpoolSound = null;
                turboBlowOffSound = null;
                break;
        }
        if (superchargerWhineSound != null) superchargerWhineSound.loop = true;
        if (turboSpoolSound != null) turboSpoolSound.loop = true;
        if (turboBlowOffSound != null) turboBlowOffSound.loop = false;
    }
}