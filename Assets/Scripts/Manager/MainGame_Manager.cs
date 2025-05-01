using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class MainGame_Manager : NetworkBehaviour
{
    [SerializeField] private Player_Car playerCar;
    private bool localPlayer = false;
    // 변경: Car_data 대신 CarData 클래스 사용
    [SerializeField] private CarData carData;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Slider NitroBar;
    [SerializeField] private RPMGauge rpmGauge;
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ExitGame();
        }
    }
    public void Init(NetworkObject _spawnedObject)
    {

        playerCar = _spawnedObject.GetComponent<Player_Car>();
        carData = CarData_Manager.instance.GetCarDataByName("Super2000");

        if(true)
        {
            playerCar.SetCamAndUI(MainCamera, NitroBar, rpmGauge);
            playerCar.CamInit();
            localPlayer = true;
        }
        playerCar.SetCarMass(carData.Mass);
        playerCar.SetDragCoefficient(carData.dragCoefficient);
        playerCar.SetBaseEngineAcceleration(carData.baseEngineAcceleration);
        playerCar.SetEngineRPMLimit(carData.maxEngineRPM, carData.minEngineRPM);
        switch(carData.lastGear)
        {
            case 1:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_FIRST);
                break;
            case 2:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_SECOND);
                break;
            case 3:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_THIRD);
                break;
            case 4:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_FOURTH);
                break;
            case 5:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_FIFTH);
                break;
            case 6:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_SIXTH);
                break;
            default:
                Debug.Log("잘못된 lastGear설정입니다.");
                break;
        }
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_NEUTURAL, 0f);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_REVERSE, carData.gearRatio_eGEAR_REVERSE);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_FIRST, carData.gearRatio_eGEAR_FIRST);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_SECOND, carData.gearRatio_eGEAR_SECOND);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_THIRD, carData.gearRatio_eGEAR_THIRD);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_FOURTH, carData.gearRatio_eGEAR_FOURTH);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_FIFTH, carData.gearRatio_eGEAR_FIFTH);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_SIXTH, carData.gearRatio_eGEAR_SIXTH);
        playerCar.SetFinalDriveRatio(carData.finalDriveRatio);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_NEUTURAL, 0f);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_REVERSE, carData.gearSpeedLimit_eGEAR_REVERSE);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_FIRST, carData.gearSpeedLimite_GEAR_FIRST);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_SECOND, carData.gearSpeedLimit_eGEAR_SECOND);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_THIRD, carData.gearSpeedLimit_eGEAR_THIRD);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_FOURTH, carData.gearSpeedLimit_eGEAR_FOURTH);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_FIFTH, carData.gearSpeedLimit_eGEAR_FIFTH);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_SIXTH, carData.gearSpeedLimit_eGEAR_SIXTH);
        playerCar.Init();
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}