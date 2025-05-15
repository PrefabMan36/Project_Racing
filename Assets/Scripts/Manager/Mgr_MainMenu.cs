using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mgr_MainMenu : MonoBehaviour
{
    //private eCAR_COLOR CarColor = eCAR_COLOR.Red_tuned;
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
        Shared.scene_Manager.ChangeScene(eSCENE.eSCENE_MAINGAME);
    }
    public void OnClick_Change_Car()
    {

    }
}
