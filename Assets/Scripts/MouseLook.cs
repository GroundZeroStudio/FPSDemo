using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 摄像机的旋转
/// 玩家左右旋转控制视线左右移动
/// 摄像机上下旋转控制视线上下移动
/// </summary>
public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity=100f;//鼠标灵敏度

    public Transform playerBody;//玩家的位置
    private float xRotation=0f;

    // Start is called before the first frame update
    void Start()
    {
        //将光标锁定在该游戏窗口的中心,并且隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;//将上下旋转的轴值进行累计
        xRotation = Mathf.Clamp(xRotation,-90f,90f);//限制轴值的累计（这里就能发现上90度和下90度角正好相对于了90的轴值）
        transform.localRotation = Quaternion.Euler(xRotation, 0f,0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
