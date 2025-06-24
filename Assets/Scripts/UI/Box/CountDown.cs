using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : NetworkBehaviour
{
    [SerializeField] private bool startOrEnd;

    [SerializeField] private Sprite[] countNumbers = new Sprite[10];
    [SerializeField] private Image countdownImage;

    [SerializeField] private bool countdownStarted = false;
    [SerializeField] private float countdownInterval = 1.0f;
    [SerializeField] private float animationDuration = 0.5f;

    [SerializeField] private float timer = 0f;

    [SerializeField] private Vector3 maxScale = new Vector3(1.5f, 1.5f, 1.5f);
    [SerializeField] private Vector3 minScale = new Vector3(0.8f, 0.8f, 0.8f);

    [SerializeField] private MainGame_Manager mainGameManager;

    /// <summary>
    /// MainGame_Manager 인스턴스를 받아오는 함수
    /// </summary>
    /// <param name="manager">MainGame_Manager 인스턴스</param>
    public void SetMainGameManager(MainGame_Manager manager,bool isStartOrEnd)
    {
        if (manager == null)
        {
            Debug.LogError("SetMainGameManager에 null MainGame_Manager가 전달되었습니다.");
            return;
        }
        mainGameManager = manager;
        Debug.Log("MainGame_Manager가 성공적으로 설정되었습니다.");

        startOrEnd = isStartOrEnd;//시작 카운트다운인지 종료 카운트다운인지 설정

        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Countdown Image가 할당되지 않았습니다. Unity 에디터에서 할당해주세요.");
        }
    }
    /// <summary>
    /// 카운트다운 시작 함수. 외부에서 호출하여 카운트다운을 시작합니다.
    /// </summary>
    public void StartCountdown()
    {
        if (countNumbers == null || countNumbers.Length == 0)
        {
            Debug.LogError("카운트다운 스프라이트가 할당되지 않았습니다. Unity 에디터에서 스프라이트들을 할당해주세요.");
            return;
        }
        if (countdownImage == null)
        {
            Debug.LogError("Countdown Image가 할당되지 않았습니다. Unity 에디터에서 할당해주세요.");
            return;
        }

        countdownImage.gameObject.SetActive(true); // 카운트다운 이미지 활성화
        countdownStarted = true;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
    }

    private void RunCountdown()
    {
        countdownImage.rectTransform.localScale = Vector3.zero;
        int maxCount = startOrEnd ? countNumbers.Length : 3; // 시작 카운트다운이면 10, 종료 카운트다운이면 3부터 시작
        for (int i = 0; i < maxCount; i++)
        {
            countdownImage.sprite = countNumbers[i];
            countdownImage.gameObject.SetActive(true);
            timer = 0f;
            while (timer < animationDuration / 2)
            {
                countdownImage.rectTransform.localScale = Vector3.Lerp(maxScale, minScale, timer / (animationDuration / 2));
                timer += Runner.DeltaTime;
            }
            countdownImage.rectTransform.localScale = maxScale;

        }
    }

}
