using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CSVToolKit;

public class Mgr_MainMenu : MonoBehaviour
{
    //private eCAR_COLOR CarColor = eCAR_COLOR.Red_tuned;
    [SerializeField] private CSVParser csvReader;
    [SerializeField] private List<GameObject> cars;
    [SerializeField] private List<string> car_Path;
    [SerializeField] private Transform SelectPosition;
    [SerializeField] private GameObject selectCar;
    [SerializeField] private Text carName;
    private int carNum;

    private void Start()
    {
    }

    public void OnClick_NextScene()
    {
        Shared.CarName = carName.text;
        Shared.mgr_Scene.ChangeScene(eSCENE.eSCENE_MAINGAME);
    }
    public void OnClick_Change_Car()
    {

    }
}
