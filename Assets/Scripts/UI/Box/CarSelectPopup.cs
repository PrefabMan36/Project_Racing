using UnityEngine;
using UnityEngine.UI;

public class CarSelectPopup : MonoBehaviour
{
    [SerializeField] CarSelect_Manager carSelectManager;
    [SerializeField] Button selectButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;
    [SerializeField] Button noButton;

    private void Awake()
    {
        carSelectManager = Shared.CarSelect_Manager;
        carSelectManager.StartSelect();
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(carSelectManager.SelectNextCar);
        prevButton.onClick.RemoveAllListeners();
        prevButton.onClick.AddListener(carSelectManager.SelectPreviousCar);
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(carSelectManager.ConfirmCurrentCar);
        selectButton.onClick.AddListener(Shared.ui_Manager.OnClickNo);
        noButton.onClick.AddListener(carSelectManager.CancelSelectCar);
    }
}
