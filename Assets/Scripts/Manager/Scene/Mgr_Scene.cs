using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Mgr_Scene : MonoBehaviour
{
    private void Awake()
    {
        Shared.mgr_Scene = this;

        DontDestroyOnLoad(this);
    }
}
