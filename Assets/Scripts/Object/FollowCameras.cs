using UnityEngine;

public class FollowCameras : MonoBehaviour
{
    [SerializeField] public Camera followCamera;
    [SerializeField] public Camera followCameraTopDown;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
