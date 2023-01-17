using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器摇摆
/// </summary>
public class WeaponSway : MonoBehaviour
{
    /*摇摆的参数*/
    public float amout; //摇摆幅度
    public float smoothAmout;//一个平滑值
    public float maxAmout;//最大摇摆幅度

    private Vector3 originPostion; //初始位置

    // Start is called before the first frame update
    void Start()
    {
        //自身位置（相对于父级物体变换得位置）
        originPostion = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //设置武器手臂模型位置得值，
        float movementX = -Input.GetAxis("Mouse X") * amout;
        float movementY = -Input.GetAxis("Mouse Y") * amout;
        //限制
        movementX = Mathf.Clamp(movementX, -maxAmout, maxAmout);
        movementY = Mathf.Clamp(movementY, -maxAmout, maxAmout);

        Vector3 finallyPostion = new Vector3(movementX, movementY, 0);
        //手柄位置变换
        transform.localPosition = Vector3.Lerp(transform.localPosition, finallyPostion + originPostion, Time.deltaTime * smoothAmout);
    }
}
