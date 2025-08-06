using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Setting_Data
{
    public static float CameraFollowDamping
    {
        get => PlayerPrefs.GetFloat("Setting_CameraFollowDamping", 0.1f);
        set => PlayerPrefs.SetFloat("Setting_CameraFollowDamping", value);
    }
}
