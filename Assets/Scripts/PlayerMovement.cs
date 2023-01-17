using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 控制玩家移动
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    public float walkSpeed = 10f;// 移动速度
    public float runSpeed=15f;//奔跑速度
    public float jumpForce = 3f;//跳跃力度
    private Vector3 velocity;//设置玩家Y轴的一个冲量变化
    private Vector3 moveDirction; //设置移动方向
    public float gravity = -9f; //设置重力

    public Transform groundCheck;//地面检测物体
    private float groundDistance = 0.4f;//与地面的距离
    public LayerMask groundMask;
    private bool isJump;//判断是否在跳跃
    private bool isGround;//判断是否在地面上
    public bool isWalk;//判断是否在行走
    public bool isRun;//判断是否在奔跑
    private bool isCrouch;//判断是否蹲下

    [SerializeField] private float slopeForce=6.0f; //走斜坡施加的力(是一个乘量)
    [SerializeField] private float slopeForceRayLength=2.0f; //斜坡射线长度（自定义量）

    [Header("键位设置")]
    [SerializeField] [Tooltip("跳跃按键")] private string jumpInputName = "Jump";
    [SerializeField] [Tooltip("奔跑按键")] private KeyCode runInputName;
    [SerializeField] [Tooltip("下蹲按键")] private KeyCode crouchInputName;

    private AudioSource audioSource;
    public AudioClip walkingSound;
    public AudioClip runingSound;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = walkingSound;
        audioSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {

        CheckGround();
        Moveing();
        Crouch();
    }

    /// <summary>
    /// 判断是否在斜坡上
    /// </summary>
    /// <returns></returns>
    public bool OnSlope()
    {
        if (isJump)
            return false;

        RaycastHit hit;
        //向下打出射线(检查是否在斜坡上)
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
        {
            //如果触碰到的点的法线，不是在（0，1，0）这个方向上的，那么就人物处在斜坡上
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// 地面检测
    /// </summary>
    public void CheckGround() {
        //在 groundCheck 位置上做一个球体检测判断是否处在地面上
        isGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        //如果处在地面上，重力设置成一个固定值
        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    public void Jump() {
        isJump = Input.GetButtonDown(jumpInputName);
        //施加跳跃的力 
        if (isJump && isGround)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            //velocity.y = 20f;
        }
    }

    /// <summary>
    /// 下蹲
    /// </summary>
    public void Crouch() {
        isCrouch = Input.GetKey(crouchInputName);
        if (isCrouch)
        {
            characterController.height = 1f;
        }
        else
        {
            characterController.height =1.8f;
        }   
    }

    /// <summary>
    /// 移动
    /// </summary>
    public void Moveing()
    {
        float speed;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
     
        isRun = Input.GetKey(runInputName);

        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;
        speed = isRun ? runSpeed : walkSpeed; //设置行走或奔跑的速度

        moveDirction = (transform.right * h + transform.forward * v).normalized;//设置玩家移动方向(将移动速度进行规范化，防止斜向走速度变大)

        characterController.Move(moveDirction * speed * Time.deltaTime);//移动

        velocity.y += gravity * Time.deltaTime;//不在地面上（空中，累加重力值）
        characterController.Move(velocity * Time.deltaTime); //施加重力
        Jump();
        //如果处在斜面上移动
        if (OnSlope())
        {
            //向下增加力量
            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.deltaTime);
        }
        PlayFootStepSound();



    }

    ///播放移动的音效
    public void PlayFootStepSound() {
        if (isGround && moveDirction.sqrMagnitude > 0.9f)
        {
            audioSource.clip = isRun ? runingSound : walkingSound;//设置行走或者奔跑的音效
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

}
