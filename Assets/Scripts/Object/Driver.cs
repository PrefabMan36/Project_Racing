using System;
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
    [SerializeField] private float targetSteeringAngle;
    [SerializeField] private float currentSteeringAngle;
    [SerializeField] private float currentSteeringTime;
    [SerializeField] private bool LHS;
    [SerializeField] private bool stunned = false;

    private void Start()
    {
        car = transform.GetComponentInParent<Player_Car>();
        if(car == null)
        {
            Debug.LogError($"Driver script must be attached to a child of Player_Car: {gameObject.name}");
            return;
        }

        if(transform.localScale.x < 0)
            LHS = true;
        else
            LHS = false;

        animator = GetComponent<Animator>();
    }

    public void SetState(int tempState, float steering)
    {
        if (currentStateNum == tempState) return;

        if (steering != 0)
        {
            if (LHS == true)
                targetSteeringAngle = steering < 0 ? 1 : -1;
            else
                targetSteeringAngle = steering > 0 ? 1 : -1;
        }
        else
            targetSteeringAngle = 0f;
        currentStateNum = tempState;
    }

    private void Update()
    {
        if (!stunned)
        {
            if(currentState == ESTATE.LEFT || currentState == ESTATE.RIGHT)
                currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle, Time.deltaTime * 5f);
            else
                currentSteeringTime = 0f;
                animator.SetFloat("Steering", currentSteeringAngle);
        }
        else
        {
            animator.SetFloat("Steering", 0f);
            currentSteeringTime = 0f;
        }
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
