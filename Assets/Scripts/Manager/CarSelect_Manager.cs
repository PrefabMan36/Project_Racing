using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CarSelect_Manager : MonoBehaviour
{
    [Header("Rotate Value")]
    [SerializeField] private Transform coreTransform;
    [SerializeField] private Transform ShowTransform;
    [SerializeField] private float baseRadius = 5f;
    [SerializeField] private float radiusPerCar = 0.5f;
    [SerializeField] private float calculatedRadius;

    [SerializeField] private float rotateSpeed = 4f;
    [SerializeField] private float rotateTime = 0f;

    [Header("Car select")]
    [SerializeField] private List<SelectableCar> cars = new List<SelectableCar>();
    [SerializeField] private int currentCarIndex;
    [SerializeField] private float targetAngle;
    [SerializeField] Quaternion startRotation;
    [SerializeField] Quaternion endRotation;

    private float cameraDistance;

    void Start()
    {
        InitializeCarsAsync();
    }

    void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 부모 오브젝트의 자식으로 있는 차량들을 리스트에 추가합니다.
    /// </summary>
    private async void InitializeCarsAsync()
    {
        if (coreTransform == null)
            coreTransform = transform;

        currentCarIndex = Client_Data.CarID;

        while (CarData_Manager.instance == null || CarData_Manager.instance.carDatas == null || CarData_Manager.instance.carDatas.Count == 0)
        {
            Debug.Log("[CarSelector] CarData_Manager 데이터 로딩 대기 중...");
            await Task.Yield();
        }

        Debug.Log("[CarSelector] CarData_Manager 데이터 로딩 완료. 차량 프리팹 로드 시작.");

        foreach (var carData in CarData_Manager.instance.carDatas)
        {
            string prefabPath = "Prefabs/Cars/OnlyModel/" + carData.Name;
            GameObject carPrefab = Resources.Load<GameObject>(prefabPath);

            if (carPrefab != null)
            {
                SelectableCar carInstance = Instantiate(carPrefab, coreTransform).GetComponent<SelectableCar>();
                if(carInstance == null)
                {
                    Debug.LogError($"{carPrefab}에서 SelectableCar 컴포넌트를 찾을수 없습니다.");
                    return;
                }
                cars.Add(carInstance);
                carInstance.gameObject.SetActive(true);
                Debug.Log($"차량 프리팹 로드 및 스폰 완료: {carData.Name}");
            }
            else
                Debug.LogWarning($"경고: 경로 '{prefabPath}'에서 차량 프리팹을 찾을 수 없습니다. Car_spec의 'Name'과 프리팹 이름이 일치하는지 확인하세요.");
        }

        if (cars.Count == 0)
        {
            Debug.LogWarning("CarParent 아래에 차량 오브젝트가 없습니다.");
            return;
        }

        CalculateRadius();
        ArrangeCarsInCircle();
        SetInitialRotation();

        currentCarIndex = Client_Data.CarID;
        DeselectAll();

        StartCoroutine(UpdateRotation());
    }

    /// <summary>
    /// 차량 갯수에 따라 반지름을 계산합니다.
    /// </summary>
    private void CalculateRadius()
    {
        calculatedRadius = baseRadius + (cars.Count - 1) * radiusPerCar;
        if (calculatedRadius < baseRadius) calculatedRadius = baseRadius; // 최소 반지름 보장
    }


    /// <summary>
    /// 차량들을 부모 오브젝트를 기준으로 원형으로 배치합니다.
    /// </summary>
    private void ArrangeCarsInCircle()
    {
        if (cars.Count == 0) return;

        float angleStep = 360f / cars.Count;

        for (int i = 0; i < cars.Count; i++)
        {
            float angle = i * angleStep;
            float x = calculatedRadius * Mathf.Sin(Mathf.Deg2Rad * angle);
            float z = calculatedRadius * Mathf.Cos(Mathf.Deg2Rad * angle);

            cars[i].transform.localPosition = new Vector3(x, 0, z); // 부모를 기준으로 로컬 위치 설정
            cars[i].transform.localRotation = Quaternion.Euler(0, -angle, 0); // 차량이 바깥쪽을 바라보도록 회전
            cars[i].SetInitialRotation();
        }
    }

    /// <summary>
    /// 초기 목표 회전을 현재 선택된 차량에 맞게 설정합니다.
    /// </summary>
    private void SetInitialRotation()
    {
        if (ShowTransform == null || cars.Count == 0) return;

        Vector3 corePosition = ShowTransform.position;
        corePosition.z += calculatedRadius;
        coreTransform.position = corePosition;

        if (cars.Count > 0)
        {
            float targetAngle = currentCarIndex * (360f / cars.Count);
            endRotation = Quaternion.Euler(0, -targetAngle, 0);
            coreTransform.localRotation = endRotation; // 초기 위치로 바로 설정
        }
    }

    /// <summary>
    /// 사용자 입력을 처리하여 차량을 선택합니다.
    /// </summary>
    private void HandleInput()
    {
        if (cars.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectPreviousCar();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectNextCar();
        }
    }

    private void SelectNextCarInternal(int direction)
    {
        if (currentCarIndex >= 1 && currentCarIndex <= cars.Count)
            cars[currentCarIndex - 1].OnDeselected();

        currentCarIndex = (currentCarIndex + direction);
        if (currentCarIndex > cars.Count)
            currentCarIndex = 1;
        else if (currentCarIndex < 1)
            currentCarIndex = cars.Count;
        Client_Data.CarID = currentCarIndex;
        UpdateTargetRotation();
        SelectCar(currentCarIndex);
    }

    /// <summary>
    /// 특정 차량을 선택하고 해당 차량에 이벤트를 전달합니다.
    /// </summary>
    /// <param name="index">선택할 차량의 인덱스</param>
    /// <param name="enableRotation">선택된 차량의 독립 회전 활성화 여부</param>
    private void SelectCar(int index)
    {
        index -= 1;
        if (index >= 0 && index < cars.Count)
        {
            cars[index].OnSelected();
        }
    }
    
    private void DeselectAll()
    {
        foreach(var car in cars)
        {
            car.OnDeselected();
        }
    }
    public void StartSelect()
    {
        SelectNextCarInternal(0);
    }

    /// <summary>
    /// 다음 차량을 선택합니다.
    /// </summary>
    private void SelectNextCar()
    {
        SelectNextCarInternal(1);
    }

    /// <summary>
    /// 이전 차량을 선택합니다.
    /// </summary>
    private void SelectPreviousCar()
    {
        SelectNextCarInternal(-1);
    }

    /// <summary>
    /// 선택된 차량에 맞춰 부모 오브젝트의 목표 회전을 업데이트합니다.
    /// </summary>
    private void UpdateTargetRotation()
    {
        rotateTime = 0;
        targetAngle = currentCarIndex * (360f / cars.Count);
        startRotation = coreTransform.localRotation;
        endRotation = Quaternion.Euler(0, targetAngle, 0);
    }

    IEnumerator UpdateRotation()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame30);
        while (true)
        {
            yield return waitForSeconds;
            if(rotateTime < 1)
            rotateTime += Time.deltaTime * 4;
            coreTransform.localRotation = Quaternion.Slerp(startRotation, endRotation, rotateTime);
        }
    }

    // 디버그 용으로 현재 선택된 차량의 인덱스를 표시합니다.
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 200, 50), "선택된 차량: " + (currentCarIndex + 1) + " / " + cars.Count, style);
    }
}
