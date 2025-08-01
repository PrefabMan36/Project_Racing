using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Driver : Object
{
    public enum ESTATE
    {
        IDLE = 0,
        LEFT = 1,
        RIGHT = 2,
        REVERSE = 3,
        BOOST = 4,
        STUNNED = 5,
        Flipped = 6
    }
    [SerializeField] private ESTATE currentState = ESTATE.IDLE;
    [Networked] private short currentStateNum { get; set; } = 0;

    [SerializeField] private Player_Car car;
    [SerializeField] private short carState = 0;

    private void Start()
    {
        car = transform.GetComponentInParent<Player_Car>();
        if(car == null)
        {
            Debug.LogError($"Driver script must be attached to a child of Player_Car: {gameObject.name}");
            return;
        }

        StartCoroutine(StateChange());
    }

    public void SetState()
    {
        carState = car.GetCarState();

        if (currentStateNum == carState) return;

        currentStateNum = carState;

        if(Enum.IsDefined(typeof(ESTATE), carState))
            currentState = (ESTATE)carState;
        else
            Debug.LogError($"Invalid state: {carState} for {gameObject.name}");
    }

    IEnumerator StateChange()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame60);
        while (true)
        {
            yield return waitForSeconds;
            SetState();
        }
    }
}
