using UnityEngine;

public partial class Scene_manager : MonoBehaviour
{
    private void Awake()
    {
        Shared.scene_Manager = this;

        DontDestroyOnLoad(this);
    }
}
