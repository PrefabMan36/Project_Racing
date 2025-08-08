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

    [SerializeField] private readonly int turnLeftHash = Animator.StringToHash("TurnLeft");
    [SerializeField] private readonly int turnRightHash = Animator.StringToHash("TurnRight");
    [SerializeField] private readonly int hitHash = Animator.StringToHash("Hit");
    [SerializeField] private readonly int shiftUpHash = Animator.StringToHash("ShiftUp");
    [SerializeField] private readonly int shiftDownHash = Animator.StringToHash("ShiftDown");

    [SerializeField] private ESTATE currentState = ESTATE.IDLE;
    [Networked, OnChangedRender(nameof(OnStateChanged))]
    private int currentStateNum { get; set; } = 0;

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
    }

    public void SetState(int tempState)
    {
        if (currentStateNum == tempState) return;

        currentStateNum = tempState;
    }

    public void OnStateChanged()
    {
        if (Enum.IsDefined(typeof(ESTATE), currentStateNum))
            currentState = (ESTATE)currentStateNum;
        else
            Debug.LogError($"Invalid state: {currentStateNum} for {gameObject.name}");
        if (!stunned)
        {
            switch (currentState)
            {
                case ESTATE.IDLE:
                    animator.SetBool(turnLeftHash, false);
                    animator.SetBool(turnRightHash, false);
                    break;
                case ESTATE.LEFT:
                    animator.SetBool(turnLeftHash, true);
                    animator.SetBool(turnRightHash, false);
                    break;
                case ESTATE.RIGHT:
                    animator.SetBool(turnLeftHash, false);
                    animator.SetBool(turnRightHash, true);
                    break;
            }
        }
        else
        {
            animator.SetBool(turnLeftHash, false);
            animator.SetBool(turnRightHash, false);
        }
    }

    public void Hitted()
    {
        if (animator != null)
        {
            animator.SetTrigger(hitHash);
            stunned = true;
        }
    }
    public void HitEnd()
    {
        stunned = false;
    }

    public void ShiftUp()
    { animator.SetTrigger(shiftUpHash); }
    public void ShiftDown()
    { animator.SetTrigger(shiftDownHash); }
}
