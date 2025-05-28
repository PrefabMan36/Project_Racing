using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomButton : MonoBehaviour
{
    [SerializeField] private CreateRoom roomMaker;
    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(roomMaker.ValidateLobby);
        button.onClick.AddListener(roomMaker.OnClickCreate);
        Debug.Log("��ư �����ʰ� �߰��Ǿ����ϴ�.");
    }
}
