using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//UI박스를 상속받아 사용합니다.
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
    //시작시 초기화 합니다.
    private void Awake()
    {
        SetMapImage();
        SetCar();
    }
    private void FixedUpdate()
    {
        spawnPosition.Rotate(Vector3.up * 10f * Time.deltaTime);
    }
    //인덱스에 맞는 맵 이미지를 설정하고 시작할 맵을 저장해 둡니다.
    private void SetMapImage()
    {
        mapSelected.texture = maps[selectedMapIndex];
        targetMap = (eSCENE)selectedMapIndex;
        mapNameText.text = maps[selectedMapIndex].name;
    }
    //인덱스에 맞는 차랑 이미지를 설정하고 차량을 선택 합니다.
    private void SetCar()
    {
        if (selectedCar != null)
            Destroy(selectedCar);
        selectedCar = Instantiate(cars[selectedCarIndex], spawnPosition);
        carNameText.text = cars[selectedCarIndex].name;
    }
    //버튼 클릭시 맵이미지를 변경합니다(왼쪽).
    public void OnClickMapLeft()
    {
        if(selectedMapIndex > 0)
            selectedMapIndex--;
        else
            selectedMapIndex = maps.Length - 1;
        SetMapImage();
    }
    //버튼 클릭시 맵이미지를 변경합니다(오른쪽).
    public void OnClickMapRight()
    {
        if (selectedMapIndex < maps.Length-1)
            selectedMapIndex++;
        else
            selectedMapIndex = 0;
        SetMapImage();
    }
    //버튼 클릭시 차량을 이미지를 변경합니다(왼쪽).
    public void OnClickCarLeft()
    {
        if (selectedCarIndex > 0)
            selectedCarIndex--;
        else
            selectedCarIndex = cars.Length - 1;
        SetCar();
    }
    //버튼 클릭시 차량을 이미지를 변경합니다(오른쪽).
    public void OnClickCarRight()
    {
        if (selectedCarIndex < cars.Length-1)
            selectedCarIndex++;
        else
            selectedCarIndex = 0;
        SetCar();
    }
    //버튼 클릭시 저장해둔 맵의 레이스 시작을 위해 로딩 씬으로 이동합니다.
    public void StartRace()
    {
        Shared.scene_Manager.ChangeScene(eSCENE.eSCENE_LOADING);
        Debug.Log("Load Scene: " + targetMap);
    }
}
