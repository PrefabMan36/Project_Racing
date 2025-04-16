using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_FX_Manager : MonoBehaviour
{
    [SerializeField] private TrailRenderer skidMark;
    [SerializeField] private Transform skidMarkHolder;
    [SerializeField] private ParticleSystem wheelSmoke;
    [SerializeField] private ParticleSystem groundSmoke;
}
