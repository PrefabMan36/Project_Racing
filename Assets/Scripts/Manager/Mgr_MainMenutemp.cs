using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mgr_MainMenutemp : MonoBehaviour
{
    //private eCAR_COLOR CarColor = eCAR_COLOR.Red_tuned;
    [SerializeField] private List<Material> CarColors;
    [SerializeField] private Material CarColorLight;
    [SerializeField] private Transform SelectPosition;
    [SerializeField] private GameObject selectCarBody;
    //[SerializeField] private GameObject selectCar;
    [SerializeField] private Text carName;
    private int carColorNum;

    private void Start()
    {
        string[] color_path = System.IO.Directory.GetFiles("Assets/Materials/Classic A", "*.mat");
        carColorNum = color_path.Length - 1;
        for (int i = 0; i < color_path.Length; i++)
            CarColors.Add((Material)AssetDatabase.LoadAssetAtPath(color_path[i], typeof(Material)));
        CarColorLight = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/Lights.mat", typeof(Material));

        Shared.CarID = 0;
        //selectCar = FindAnyObjectByType<Car_Player>().gameObject;
        ChangeCarColor();
    }

    public void OnClick_NextScene()
    {
        Shared.CarName = carName.text;
        Shared.mgr_Scene.ChangeScene(eSCENE.eSCENE_MAINGAME);
    }

    public void OnClick_ChangeColor_Right()
    {
        Shared.CarID++;
        if(Shared.CarID > carColorNum)
            Shared.CarID = 0;
        ChangeCarColor();
    }
    public void OnClick_ChangeColor_Left()
    {
        Shared.CarID--;
        if (Shared.CarID < 0)
            Shared.CarID = carColorNum;
        ChangeCarColor();
    }

    private void ChangeCarColor()
    {
        MeshRenderer carMesh = selectCarBody.GetComponent<MeshRenderer>();
        Material[] curCarColor = new Material[2];
        curCarColor[0] = CarColors[Shared.CarID];
        curCarColor[1] = CarColorLight;
        carMesh.materials = curCarColor;
    }
}
