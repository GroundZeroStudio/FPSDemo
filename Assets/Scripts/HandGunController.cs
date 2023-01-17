using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 手枪(G18)武器射击
/// </summary>
public class HandGunController : MonoBehaviour
{
    public PlayerMovement PM;
    public Transform shooterPoint;//射击的位置（枪口）
    
    [Tooltip("武器射程")] public int range = 100;//武器射程
    [Tooltip("一个弹匣数量")] public int bulletsMag = 31;//一个弹匣数量
    [Tooltip("当前子弹数")] public int currentBullets;//当前子弹数
    [Tooltip("备弹")] public int bulletLeft = 300;//备弹
    [Header("枪口特效")]
    public ParticleSystem muzzleFlash;//枪口火焰特效
    public GameObject hitParticles;//子弹击中粒子特效
    public GameObject bulletHole;//弹孔
    public Light muzzleFlashLight;//枪口火焰灯光
    [Header("武器射速")]
    public float fireRate = 0.1f;//射速
    private float fireTimer;//计时器
    private float SpreadFactor; //射击的一点偏移量

    [Header("键位设置")]
    [SerializeField] [Tooltip("填装子弹按键")] private KeyCode reloadInputName;
    [SerializeField] [Tooltip("查看武器按键")] private KeyCode inspectInputName;
    [SerializeField] [Tooltip("主武器")] private KeyCode AutoRifleKey;
    [SerializeField] [Tooltip("副武器")] private KeyCode HandGunKey;
    [SerializeField] [Tooltip("自动半自动切换")] private KeyCode GunShootModelInputName;


    private Animator anim;
    private AudioSource audioSource;
    [Header("音效片段")]
    public AudioClip HandGunSound;/*枪声音效片段*/
    public AudioClip reloadAmmoLeftClip;//换子弹1音效片段
    public AudioClip reloadOutOFAmmoClip;//换子弹2音效片段（拉枪栓）

    private bool isReloading;//判断是否在装弹
    private bool isAiming;//判断是否在瞄准

    public Transform casingSpawnPoint;//子弹壳抛出的位置
    public Transform casingPrefab; //子弹壳预制体

    private Camera mainCamera;
    public ChooseGunController CGC; //声明切换武器类的实例

    /*使用枚举区分全自动和半自动模式*/
    public enum ShootMode { AutoRifle,SemiGun};
    public ShootMode shootingMode;
    private bool GunShootInput; //根据全自动和半自动 射击的键位输入发生改变
    private int modeNum = 0; //模式切换的一个中间参数（1：全自动模式，2：半自动模式）
    private string shootModelName;

    [Header("UI的设置")]
    public Image crossHairUI;
    public Text ammoTextUI;
    public Text ShootModelTextUI;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        currentBullets = bulletsMag;
        mainCamera = Camera.main;
        shootingMode = ShootMode.SemiGun; //G18手枪默认是半自动模式
        shootModelName = "半自动";
        UpdateAmmoUI();
    }

    private void Update()
    {
        //切换模式(全自动和半自动)
        if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 1)
        {
            modeNum = 1;
            shootModelName = "全自动";
            shootingMode = ShootMode.AutoRifle;
            ShootModelTextUI.text = shootModelName;

        }
        else if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 0)
        {
            modeNum = 0;
            shootModelName = "半自动";
            shootingMode = ShootMode.SemiGun;
            ShootModelTextUI.text = shootModelName;

        }

        /*控制射击模式的转换  后面就要用代码去动态控制了*/
        switch (shootingMode)
        {
            case ShootMode.AutoRifle:
                GunShootInput = Input.GetMouseButton(0);
                fireRate = 0.08f;
                break;
            case ShootMode.SemiGun:
                GunShootInput = Input.GetMouseButtonDown(0);
                fireRate = 0.2f;
                break;
        }



        if (GunShootInput && currentBullets>0)
        {
            GunFire();
        }
        else
        {
            muzzleFlashLight.enabled = false;
        }

        //计时器加时间
        if (fireTimer<fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        anim.SetBool("Run",PM.isRun);//播放跑步动画
        anim.SetBool("Walk",PM.isWalk);
        //获取动画状态机第一层动画的状态
        AnimatorStateInfo info=anim.GetCurrentAnimatorStateInfo(0);
        //两种换子弹的动画
        if (info.IsName("reload_ammo_left")||info.IsName("reload_out_of_ammo"))
        {
            isReloading = true;
        }
        else
        {
            isReloading = false;
        }

        if (Input.GetKeyDown(reloadInputName) && currentBullets < bulletsMag && bulletLeft > 0)
        {
            Reload();
        }
        SpreadFactor = (isAiming) ? 0f : 0.01f; 
        DoingAim();

         
        if (Input.GetKeyDown(inspectInputName))
        {
            anim.SetTrigger("Inspect");
        }

        //切换主武器（自动步枪）
        if (Input.GetKeyDown(AutoRifleKey))
        {
            CGC.ChangeWeapon(0);
        }
        //切换副武器（手枪）
        if (Input.GetKeyDown(HandGunKey))
        {
            CGC.ChangeWeapon(1);
        }

    }

    //更新UI
    public void UpdateAmmoUI() {
        ammoTextUI.text = currentBullets + " / " + bulletLeft;
        ShootModelTextUI.text = shootModelName;
    }

    /// <summary>
    /// 瞄准的逻辑
    /// </summary>
    public void DoingAim()
    {
        if (Input.GetMouseButton(1) && !isReloading && !PM.isRun )
        {
            isAiming = true;
            anim.SetBool("Aim", isAiming);
            crossHairUI.gameObject.SetActive(false);
            mainCamera.fieldOfView = 40;//瞄准的时候摄像机视野变小
        }
        else
        {
            isAiming = false;
            anim.SetBool("Aim", isAiming);
            crossHairUI.gameObject.SetActive(true);
            mainCamera.fieldOfView = 60;//瞄准的时候摄像机视野恢复

        }


    }

    /// <summary>
    /// 射击逻辑
    /// </summary>
    public void GunFire() {
        //控制射速， 当前弹夹打光了,正在装子弹,正在奔跑  就不可以发射了
        if (fireTimer < fireRate||currentBullets<=0||isReloading||PM.isRun) return;


        RaycastHit hit;
        Vector3 shootDirection = shooterPoint.forward;
        //改成这个，shootDirection shooterPoint这个游戏物体进行小的偏移(TransformDirection 将local坐标转换为世界坐标)
        shootDirection = shootDirection + shooterPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));

        if (Physics.Raycast(shooterPoint.position, shootDirection, out hit, range))
        {
            Debug.Log(hit.transform.name + "打到了");

            GameObject hitParticleEffect = Instantiate(hitParticles, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//实例出子弹击中的火光特效
            GameObject bulletHoleEffect = Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//实例出弹孔特效
            //回收特效
            Destroy(hitParticleEffect,1f);
            Destroy(bulletHoleEffect, 3f);
        }

        if (!isAiming)
        {
            anim.CrossFadeInFixedTime("fire", 0.1f); //播放普通开火动画（使用动画的淡出淡入效果）
        }
        else
        {//瞄准状态下，播放瞄准的开火动画
            anim.Play("aim_fire", 0, 0f);
        }
        muzzleFlash.Play(); //播放火光特效
        muzzleFlashLight.enabled = true;
        PlayerShootSound();//播放射击音效
        //实例抛弹壳
        Instantiate(casingPrefab, casingSpawnPoint.transform.position,casingSpawnPoint.transform.rotation);
        currentBullets--;
        UpdateAmmoUI();
        fireTimer = 0f;//重置计时器
    }


    public void PlayerShootSound() {
        audioSource.clip = HandGunSound;       
        audioSource.Play();
    }
    /// <summary>
    /// 填装弹药逻辑
    /// </summary>
    public void Reload()
    {
        if (bulletLeft <= 0) return;
        DoReloadAnimation();

        //计算需要填充的子弹
        int bulletToLoad = bulletsMag - currentBullets;
        //计算备弹扣除的子弹
        int bulletToReduce = (bulletLeft >= bulletToLoad) ? bulletToLoad : bulletLeft;
        bulletLeft -= bulletToReduce; //备弹减少
        currentBullets += bulletToReduce;//当前子弹增加
        UpdateAmmoUI();
    }

    //播放装弹动画
    public void DoReloadAnimation()
    {
        if (currentBullets > 0)
        {

            anim.Play("reload_ammo_left", 0, 0);
            audioSource.clip = reloadAmmoLeftClip;
            audioSource.Play();
        }

        if (currentBullets == 0)
        {
            anim.Play("reload_out_of_ammo", 0, 0);
            audioSource.clip = reloadOutOFAmmoClip;
            audioSource.Play();
        }
    }
  
}
