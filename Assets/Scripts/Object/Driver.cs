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
    [Networked] private int currentStateNum { get; set; } = 0;

    [SerializeField] Animator animator;

    [SerializeField] private Player_Car car;
    [SerializeField] private int carState = 0;
    [SerializeField] private bool stunned = false;

    private void Start()
    {
        car = transform.GetComponentInParent<Player_Car>();
        if(car == null)
        {
            Debug.LogError($"Driver script must be attached to a child of Player_Car: {gameObject.name}");
            return;
        }

        animator = GetComponent<Animator>();

        StartCoroutine(StateChange());
    }

    private void OnDestroy()
    {
        StopCoroutine(StateChange());
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
        if (!stunned)
        {
            switch (currentState)
            {
                case ESTATE.IDLE:
                    animator.SetBool("TurnLeft", false);
                    animator.SetBool("TurnRight", false);
                    break;
                case ESTATE.LEFT:
                    animator.SetBool("TurnLeft", true);
                    animator.SetBool("TurnRight", false);
                    break;
                case ESTATE.RIGHT:
                    animator.SetBool("TurnLeft", false);
                    animator.SetBool("TurnRight", true);
                    break;
            }
        }
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

    public void Hitted()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
            stunned = true;
        }
    }
    public void HitEnd()
    {
        stunned = false;
    }

    public void ShiftUp()
    { animator.SetTrigger("ShiftUp"); }
    public void ShiftDown()
    { animator.SetTrigger("ShiftDown"); }
}
