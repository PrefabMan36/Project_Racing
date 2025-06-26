using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class CountDown : NetworkBehaviour
{
    [SerializeField] private bool startOrEnd;
    [Header("표시될 UI 연결")]
    [SerializeField] private TextMeshProUGUI countNumberText;
    [SerializeField] private Image finalDisplayImage;
    [SerializeField] private Image countdownTimeImage;
    [SerializeField] private Image countdownTimeBackgroundImage;

    [Header("표시될 내용")]
    [SerializeField] private int countdownNumber; // 카운트다운 숫자
    [SerializeField] private Sprite goSprite;
    [SerializeField] private Sprite raceFinishSprite;

    //[Header("Network")]
    [Networked, SerializeField] private TickTimer countdownTimer {  get; set; }
    [Networked, SerializeField] private int currentCountdownIndex { get; set; } = -1;
    [Networked, SerializeField] private bool countdownStarted { get; set; } = false;
    [Networked, SerializeField] private TickTimer finalSpriteDisplayTimer { get; set; }
    [Networked, SerializeField] private bool showingFinalSprite { get; set; } = false;

    [SerializeField] private float countdownPhaseDuration = 1.0f;
    [SerializeField] private float finalSpriteDuration = 1.0f;

    [SerializeField] private MainGame_Manager mainGameManager;
    [SerializeField] private float remainingTime;

    public void SetMainGameManager(MainGame_Manager manager)
    {
        if (manager == null)
        {
            Debug.LogError("SetMainGameManager에 null MainGame_Manager가 전달되었습니다.");
            return;
        }
        mainGameManager = manager;
        Debug.Log("MainGame_Manager가 성공적으로 설정되었습니다.");
    }

    public override void Spawned()
    {
        base.Spawned();
        if (countNumberText != null) countNumberText.gameObject.SetActive(false);
        else Debug.LogError("countNumberText가 할당되지 않았습니다. Unity 에디터에서 할당해주세요.");
        if (countdownTimeImage != null) countdownTimeImage.gameObject.SetActive(false);
        else Debug.LogError("countdownTimeImage가 할당되지 않았습니다. Unity 에디터에서 할당해주세요.");
        if (countdownTimeBackgroundImage != null) countdownTimeBackgroundImage.gameObject.SetActive(false);
        else Debug.LogError("countdownTimeBackgroundImage가 할당되지 않았습니다. Unity 에디터에서 할당해주세요.");
        if (finalDisplayImage != null) finalDisplayImage.gameObject.SetActive(false);
        else Debug.LogError("finalDisplayImage가 할당되지 않았습니다. Unity 에디터에서 할당해주세요.");
    }

    public void StartCountdown(bool isStartOrEnd)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("StartCountdown은 호스트/서버에서만 호출되어야 합니다.");
            return;
        }
        if (countdownStarted)
        {
            Debug.LogWarning("카운트다운이 이미 시작되었습니다.");
            return;
        }

        if (countNumberText == null || countdownTimeImage == null || countdownTimeBackgroundImage == null || finalDisplayImage == null)
        {
            Debug.LogError("카운트다운 UI (countNumberText, countdownTimeImage, countdownTimeBackgroundImage, finalDisplayImage) 중 하나 이상이 할당되지 않았습니다. Unity 에디터에서 할당해주세요.");
            return;
        }
        if (isStartOrEnd && goSprite == null)
        {
            Debug.LogError("레이스 시작 카운트다운에는 goSprite가 필요합니다. Unity 에디터에서 할당해주세요.");
            return;
        }
        if (!isStartOrEnd && raceFinishSprite == null)
        {
            Debug.LogError("레이스 종료 카운트다운에는 raceFinishSprite가 필요합니다. Unity 에디터에서 할당해주세요.");
            return;
        }

        this.startOrEnd = isStartOrEnd;
        showingFinalSprite = false;

        if (this.startOrEnd)
            currentCountdownIndex = 3;
        else
            currentCountdownIndex = 10;

        countdownTimer = TickTimer.CreateFromSeconds(Runner, countdownPhaseDuration);
        countdownStarted = true;
        Debug.Log("호스트: 카운트다운 시작! (StartOrEnd: " + this.startOrEnd + ", 시작 숫자: " + currentCountdownIndex + ")");
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if(Object.HasStateAuthority)
        {
            if (countdownStarted)
            {
                if (!showingFinalSprite)
                {
                    if (countdownTimer.Expired(Runner))
                    {
                        currentCountdownIndex--;

                        if (currentCountdownIndex >= 0)
                            countdownTimer = TickTimer.CreateFromSeconds(Runner, countdownPhaseDuration);
                    }
                    else
                    {
                        showingFinalSprite = true;
                        finalSpriteDisplayTimer = TickTimer.CreateFromSeconds(Runner, finalSpriteDuration);
                        countdownTimer = TickTimer.None; // 숫자 카운트다운 타이머 중지
                        Debug.Log("호스트: 숫자 카운트다운 종료. 최종 스프라이트 표시 시작.");
                    }
                }
                else
                {
                    if (finalSpriteDisplayTimer.Expired(Runner))
                    {
                        // 최종 스프라이트 표시 시간 종료
                        currentCountdownIndex = -1; // 카운트다운 완료 상태 
                        countdownStarted = false;
                        showingFinalSprite = false;

                        Debug.Log("호스트: 최종 스프라이트 표시 종료. GameStart/GameEnd 호출 예정.");

                        // MainGame_Manager의 GameStart 또는 GameEnd 함수 호출
                        if (mainGameManager != null)
                        {
                            if (startOrEnd) // 레이스 시작 카운트다운이었으면 GameStart 호출
                            {
                                mainGameManager.RaceStart();
                                Debug.Log("호스트: MainGame_Manager의 GameStart 함수가 호출되었습니다.");
                            }
                            else // 레이스 종료 카운트다운이었으면 GameEnd 호출
                            {
                                mainGameManager.RPC_ForceRaceEnd();
                                Debug.Log("호스트: MainGame_Manager의 GameEnd 함수가 호출되었습니다.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("호스트: MainGame_Manager가 설정되지 않아 GameStart/GameEnd 함수를 호출할 수 없습니다.");
                        }
                    }
                }
            }
        }
        // 모든 클라이언트에서 UI 업데이트
        UpdateCountdownUI();
    }

    private void UpdateCountdownUI()
    {
        if (!countdownStarted && !showingFinalSprite)
        {
            // 카운트다운이 시작되지 않았거나 완전히 종료된 경우 모든 UI를 비활성화합니다.
            if (countNumberText != null) countNumberText.gameObject.SetActive(false);
            if (finalDisplayImage != null) finalDisplayImage.gameObject.SetActive(false);
            if (countdownTimeImage != null) countdownTimeImage.gameObject.SetActive(false);
            if (countdownTimeBackgroundImage != null) countdownTimeBackgroundImage.gameObject.SetActive(false);
            return;
        }

        // 카운트다운이 진행 중이거나 최종 스프라이트가 표시 중일 때 UI를 활성화합니다.
        // 숫자 카운트다운 단계
        if (countdownStarted && !showingFinalSprite)
        {
            if(countNumberText != null)
            {
                countNumberText.gameObject.SetActive(true);
                countNumberText.text = currentCountdownIndex.ToString();
            }
            if (finalDisplayImage != null) finalDisplayImage.gameObject.SetActive(false);

            if (countdownTimeImage != null && countdownTimeBackgroundImage != null && countdownTimeImage.type == Image.Type.Filled)
            {
                countdownTimeImage.gameObject.SetActive(true);
                countdownTimeBackgroundImage.gameObject.SetActive(true);

                remainingTime = countdownTimer.RemainingTime(Runner) ?? 0f;
                countdownTimeImage.fillAmount = remainingTime / countdownPhaseDuration;
            }
            else if (countdownTimeImage != null && countdownTimeImage.type != Image.Type.Filled)
            {
                Debug.LogWarning("countdownTimeImage의 Image Type이 Filled가 아닙니다. Unity 에디터에서 Filled로 변경해주세요.");
            }
        }
        else if (countdownStarted && showingFinalSprite)
        {
            if (countNumberText != null) countNumberText.gameObject.SetActive(false);

            if (finalDisplayImage != null)
            {
                finalDisplayImage.gameObject.SetActive(true);
                finalDisplayImage.sprite = startOrEnd ? goSprite : raceFinishSprite;
            }

            if (countdownTimeImage != null && countdownTimeBackgroundImage != null && countdownTimeImage.type == Image.Type.Filled)
            {
                countdownTimeImage.gameObject.SetActive(true);
                countdownTimeBackgroundImage.gameObject.SetActive(true);

                remainingTime = finalSpriteDisplayTimer.RemainingTime(Runner) ?? 0f;
                countdownTimeImage.fillAmount = remainingTime / finalSpriteDuration;
            }
            else if (countdownTimeImage != null && countdownTimeImage.type != Image.Type.Filled)
            {
                Debug.LogWarning("countdownTimeImage의 Image Type이 Filled가 아닙니다. Unity 에디터에서 Filled로 변경해주세요.");
            }
        }
    }
}
