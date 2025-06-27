using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class CountDown : NetworkBehaviour
{
    [Header("표시될 UI 연결")]
    [SerializeField] private TextMeshProUGUI countNumberText;
    [SerializeField] private Image finalDisplayImage;
    [SerializeField] private Image countdownTimeImage;
    [SerializeField] private Image countdownTimeBackgroundImage;

    [Header("표시될 내용")]
    [SerializeField] private Sprite goSprite;
    [SerializeField] private Sprite raceFinishSprite;

    //[Header("Network")]
    [Networked, SerializeField] private TickTimer countdownTimer {  get; set; }
    [Networked, SerializeField] private int currentCountdownIndex { get; set; } = -1;
    [Networked, SerializeField] private bool countdownStarted { get; set; } = false;
    [Networked, SerializeField] private TickTimer finalSpriteDisplayTimer { get; set; }
    [Networked, SerializeField] private bool showingFinalSprite { get; set; } = false;
    [Networked, SerializeField] private bool isGameStartCountdown { get; set; } = true; // true: 게임 시작, false: 레이스 종료

    [SerializeField] private float countdownPhaseDuration = 1.0f;
    [SerializeField] private float finalSpriteDuration = 1.0f;

    [SerializeField] private MainGame_Manager mainGameManager;
    [SerializeField] private float remainingTime;
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
        if (countdownTimeImage != null && countdownTimeImage.type != Image.Type.Filled)
            Debug.LogWarning("Countdown Circle Image의 Image Type이 Filled가 아닙니다. Unity 에디터에서 Filled로 변경해주세요.");
    }

    public void SetMainGameManager(MainGame_Manager manager)
    {
        if (manager == null)
        {
            Debug.LogError("SetMainGameManager에 null MainGame_Manager가 전달되었습니다.");
            return;
        }
        mainGameManager = manager;
        Debug.Log("MainGame_Manager가 CountDown에 성공적으로 설정되었습니다.");
    }

    public void StartCountdown(int startNumber, bool isGameStart)
    {
        if (Object.HasStateAuthority)
        {
            countdownStarted = true;
            isGameStartCountdown = isGameStart;
            currentCountdownIndex = startNumber;
            countdownTimer = TickTimer.CreateFromSeconds(Runner, countdownPhaseDuration);
            showingFinalSprite = false;
            UpdateCountdownUI();
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        if(countdownStarted)
        {
            if (currentCountdownIndex > 0)
            {
                if (countdownTimer.Expired(Runner))
                {
                    countdownTimer = TickTimer.CreateFromSeconds(Runner, countdownPhaseDuration);
                    currentCountdownIndex--;
                }
                UpdateCountdownUI();
            }
            else if (currentCountdownIndex == 0 && !showingFinalSprite)
            {
                showingFinalSprite = true;
                finalSpriteDisplayTimer = TickTimer.CreateFromSeconds(Runner, finalSpriteDuration);

                // 이미지가 출력되는 것과 동시에 RaceStart 호출
                if (Object.HasStateAuthority)
                {
                    if (isGameStartCountdown)
                    {
                        if (mainGameManager != null)
                        {
                            mainGameManager.RaceStart(); // <--- RaceStart() 호출
                            Debug.Log("CountDown: 게임 시작 신호를 MainGame_Manager.RaceStart()에 보냈습니다. (GO 이미지와 동시)");
                        }
                        else
                        {
                            Debug.LogError("CountDown: MainGame_Manager 참조가 NULL입니다!");
                        }
                    }
                    // 레이스 종료 카운트다운일 경우 (RaceFinishedCountdown 없음)
                    // 필요하다면 여기에 RaceFinished 관련 로직 추가
                }
                UpdateCountdownUI();
            }
            else if (showingFinalSprite)
            {
                if (finalSpriteDisplayTimer.Expired(Runner))
                {
                    countdownStarted = false;
                    showingFinalSprite = false;
                    currentCountdownIndex = -1;
                }
                UpdateCountdownUI();
            }
            else if (!countdownStarted && currentCountdownIndex == -1 && !showingFinalSprite)
                UpdateCountdownUI();
        }
        else
            UpdateCountdownUI();
    }

    private void UpdateCountdownUI()
    {
        if (countNumberText != null) countNumberText.gameObject.SetActive(false);
        if (finalDisplayImage != null) finalDisplayImage.gameObject.SetActive(false);
        if (countdownTimeImage != null) countdownTimeImage.gameObject.SetActive(false);
        if (countdownTimeBackgroundImage != null) countdownTimeBackgroundImage.gameObject.SetActive(false);

        if (countdownStarted)
        {
            if (currentCountdownIndex > 0)
            {
                if (countNumberText != null)
                {
                    countNumberText.gameObject.SetActive(true);
                    countNumberText.text = currentCountdownIndex.ToString();
                }

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
            else if (showingFinalSprite) // GO 또는 Race Finish 스프라이트 표시 중
            {
                if (countNumberText != null) countNumberText.gameObject.SetActive(false);

                if (finalDisplayImage != null)
                {
                    finalDisplayImage.gameObject.SetActive(true);
                    finalDisplayImage.sprite = isGameStartCountdown ? goSprite : raceFinishSprite; // startOrEnd 변수에 따라 GO/RaceFinish 결정
                }

                // countdownTimeImage와 countdownTimeBackgroundImage 비활성화 (요청 사항)
                if (countdownTimeImage != null) countdownTimeImage.gameObject.SetActive(false);
                if (countdownTimeBackgroundImage != null) countdownTimeBackgroundImage.gameObject.SetActive(false);
            }
        }
        else // 카운트다운이 시작되지 않았거나 완전히 종료된 상태
        {
            // 모든 UI 요소가 이미 위에서 비활성화되었으므로, 이 블록은 필요에 따라 추가 작업만 수행.
            // 예를 들어, 게임 종료 시 다시 UI를 숨기는 용도.
        }
    }
}
