using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mgr_MainMenu : MonoBehaviour
{
    //private eCAR_COLOR CarColor = eCAR_COLOR.Red_tuned;
    [SerializeField] private List<GameObject> Cars;
    [SerializeField] private Transform SelectPosition;
    [SerializeField] private GameObject selectCar;
    [SerializeField] private Text carName;
    private int carNum;

    private void Start()
    {
        string[] car_path = System.IO.Directory.GetFiles("Assets/Prefabs/Colliders", "*.prefab");
        carNum = car_path.Length - 1;
        for (int i = 0; i < car_path.Length; i++)
            Cars.Add((GameObject)AssetDatabase.LoadAssetAtPath(car_path[i], typeof(GameObject)));

        Shared.CarID = 13;
        selectCar = Instantiate(Cars[Shared.CarID]);
        selectCar.transform.position = SelectPosition.position;
        selectCar.transform.rotation = SelectPosition.rotation;
        carName.text = Cars[Shared.CarID].name;
    }

    public void OnClick_NextScene()
    {
        Shared.CarName = carName.text;
        Shared.mgr_Scene.ChangeScene(eSCENE.eSCENE_MAINGAME);
    }

    public void OnClick_ChangeColor_Right()
    {
        Shared.CarID++;
        if(Shared.CarID > carNum)
            Shared.CarID = 0;
        ChangeCarColor();
    }
    public void OnClick_ChangeColor_Left()
    {
        Shared.CarID--;
        if (Shared.CarID < 0)
            Shared.CarID = carNum;
        ChangeCarColor();
    }

    private void ChangeCarColor()
    {
        Destroy(selectCar);
        selectCar = Instantiate(Cars[Shared.CarID]);
        selectCar.transform.position = SelectPosition.position;
        selectCar.transform.rotation = SelectPosition.rotation;

        carName.text = Cars[Shared.CarID].name;
    }
}
