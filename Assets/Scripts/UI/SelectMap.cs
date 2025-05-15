using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//UI�ڽ��� ��ӹ޾� ����մϴ�.
public class SelectMap : UIBox
{
    [SerializeField] Texture[] maps;
    [SerializeField] RawImage mapSelected;
    [SerializeField] TextMeshProUGUI mapNameText;
    [SerializeField] int selectedMapIndex = 0;
    [SerializeField] eSCENE targetMap;

    [SerializeField] GameObject[] cars;
    [SerializeField] Transform spawnPosition;
    [SerializeField] GameObject selectedCar;
    [SerializeField] TextMeshProUGUI carNameText;
    [SerializeField] int selectedCarIndex = 0;
    //���۽� �ʱ�ȭ �մϴ�.
    private void Awake()
    {
        SetMapImage();
        SetCar();
    }
    private void FixedUpdate()
    {
        spawnPosition.Rotate(Vector3.up * 10f * Time.deltaTime);
    }
    //�ε����� �´� �� �̹����� �����ϰ� ������ ���� ������ �Ӵϴ�.
    private void SetMapImage()
    {
        mapSelected.texture = maps[selectedMapIndex];
        targetMap = (eSCENE)selectedMapIndex;
        mapNameText.text = maps[selectedMapIndex].name;
    }
    //�ε����� �´� ���� �̹����� �����ϰ� ������ ���� �մϴ�.
    private void SetCar()
    {
        if (selectedCar != null)
            Destroy(selectedCar);
        selectedCar = Instantiate(cars[selectedCarIndex], spawnPosition);
        carNameText.text = cars[selectedCarIndex].name;
    }
    //��ư Ŭ���� ���̹����� �����մϴ�(����).
    public void OnClickMapLeft()
    {
        if(selectedMapIndex > 0)
            selectedMapIndex--;
        else
            selectedMapIndex = maps.Length - 1;
        SetMapImage();
    }
    //��ư Ŭ���� ���̹����� �����մϴ�(������).
    public void OnClickMapRight()
    {
        if (selectedMapIndex < maps.Length-1)
            selectedMapIndex++;
        else
            selectedMapIndex = 0;
        SetMapImage();
    }
    //��ư Ŭ���� ������ �̹����� �����մϴ�(����).
    public void OnClickCarLeft()
    {
        if (selectedCarIndex > 0)
            selectedCarIndex--;
        else
            selectedCarIndex = cars.Length - 1;
        SetCar();
    }
    //��ư Ŭ���� ������ �̹����� �����մϴ�(������).
    public void OnClickCarRight()
    {
        if (selectedCarIndex < cars.Length-1)
            selectedCarIndex++;
        else
            selectedCarIndex = 0;
        SetCar();
    }
    //��ư Ŭ���� �����ص� ���� ���̽� ������ ���� �ε� ������ �̵��մϴ�.
    public void StartRace()
    {
        Shared.scene_Manager.ChangeScene(eSCENE.eSCENE_LOADING);
        Debug.Log("Load Scene: " + targetMap);
    }
}
