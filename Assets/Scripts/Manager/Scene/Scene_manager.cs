using UnityEngine;

public partial class Scene_manager : MonoBehaviour
{
    private void Awake()
    {
        Shared.mgr_Scene = this;

        DontDestroyOnLoad(this);
    }
}
