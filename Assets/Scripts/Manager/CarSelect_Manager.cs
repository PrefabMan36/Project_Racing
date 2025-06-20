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
    /// �θ� ������Ʈ�� �ڽ����� �ִ� �������� ����Ʈ�� �߰��մϴ�.
    /// </summary>
    private async void InitializeCarsAsync()
    {
        if (coreTransform == null)
            coreTransform = transform;

        currentCarIndex = Client_Data.CarID;

        while (CarData_Manager.instance == null || CarData_Manager.instance.carDatas == null || CarData_Manager.instance.carDatas.Count == 0)
        {
            Debug.Log("[CarSelector] CarData_Manager ������ �ε� ��� ��...");
            await Task.Yield();
        }

        Debug.Log("[CarSelector] CarData_Manager ������ �ε� �Ϸ�. ���� ������ �ε� ����.");

        foreach (var carData in CarData_Manager.instance.carDatas)
        {
            string prefabPath = "Prefabs/Cars/OnlyModel/" + carData.Name;
            GameObject carPrefab = Resources.Load<GameObject>(prefabPath);

            if (carPrefab != null)
            {
                SelectableCar carInstance = Instantiate(carPrefab, coreTransform).GetComponent<SelectableCar>();
                if(carInstance == null)
                {
                    Debug.LogError($"{carPrefab}���� SelectableCar ������Ʈ�� ã���� �����ϴ�.");
                    return;
                }
                cars.Add(carInstance);
                carInstance.gameObject.SetActive(true);
                Debug.Log($"���� ������ �ε� �� ���� �Ϸ�: {carData.Name}");
            }
            else
                Debug.LogWarning($"���: ��� '{prefabPath}'���� ���� �������� ã�� �� �����ϴ�. Car_spec�� 'Name'�� ������ �̸��� ��ġ�ϴ��� Ȯ���ϼ���.");
        }

        if (cars.Count == 0)
        {
            Debug.LogWarning("CarParent �Ʒ��� ���� ������Ʈ�� �����ϴ�.");
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
    /// ���� ������ ���� �������� ����մϴ�.
    /// </summary>
    private void CalculateRadius()
    {
        calculatedRadius = baseRadius + (cars.Count - 1) * radiusPerCar;
        if (calculatedRadius < baseRadius) calculatedRadius = baseRadius; // �ּ� ������ ����
    }


    /// <summary>
    /// �������� �θ� ������Ʈ�� �������� �������� ��ġ�մϴ�.
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

            cars[i].transform.localPosition = new Vector3(x, 0, z); // �θ� �������� ���� ��ġ ����
            cars[i].transform.localRotation = Quaternion.Euler(0, -angle, 0); // ������ �ٱ����� �ٶ󺸵��� ȸ��
            cars[i].SetInitialRotation();
        }
    }

    /// <summary>
    /// �ʱ� ��ǥ ȸ���� ���� ���õ� ������ �°� �����մϴ�.
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
            coreTransform.localRotation = endRotation; // �ʱ� ��ġ�� �ٷ� ����
        }
    }

    /// <summary>
    /// ����� �Է��� ó���Ͽ� ������ �����մϴ�.
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
    /// Ư�� ������ �����ϰ� �ش� ������ �̺�Ʈ�� �����մϴ�.
    /// </summary>
    /// <param name="index">������ ������ �ε���</param>
    /// <param name="enableRotation">���õ� ������ ���� ȸ�� Ȱ��ȭ ����</param>
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
    /// ���� ������ �����մϴ�.
    /// </summary>
    private void SelectNextCar()
    {
        SelectNextCarInternal(1);
    }

    /// <summary>
    /// ���� ������ �����մϴ�.
    /// </summary>
    private void SelectPreviousCar()
    {
        SelectNextCarInternal(-1);
    }

    /// <summary>
    /// ���õ� ������ ���� �θ� ������Ʈ�� ��ǥ ȸ���� ������Ʈ�մϴ�.
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

    // ����� ������ ���� ���õ� ������ �ε����� ǥ���մϴ�.
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 200, 50), "���õ� ����: " + (currentCarIndex + 1) + " / " + cars.Count, style);
    }
}
