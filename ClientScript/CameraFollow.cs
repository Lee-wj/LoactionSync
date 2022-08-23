using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public GameObject target;      //目标物体

    public float distance = 50;     //相机与目标的距离
    public float rot = 180;           //横向角度
    public float roll = 30f * Mathf.PI * 2 / 360;   //纵向角度roll为弧度，弧度=角度*2π/360

    public float rotSpeed = 0.2f;   //横向旋转速度

    public float rollSpeed = 0.2f;  //纵向旋转速度
    private float maxRoll = 70f * Mathf.PI * 2 / 360;   //纵向旋转角度最大值
    private float minRoll = -20f * Mathf.PI * 2 / 360;  //纵向旋转角度最小值

    public float zoomSpeed = 0.2f;  //相机与目标距离变化速度
    private float maxDistance = 90;    //距离最大值
    private float minDistance = 10;    //距离最小值



    // Use this for initialization
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player"); //找到跟随物体

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player");
        if (target == null)
            return;
        if (Camera.main == null)
            return;

        //鼠标控制相机横向旋转
        Rotate();
        //鼠标控制相机纵向旋转
        Roll();

        //鼠标滚轮控制相机与目标距离
        //不知道为啥一调用就出Bug
        //Zoom();

        //用三角函数计算相机的位置
        Vector3 targetPos = target.transform.position;
        Vector3 cameraPos;
        float d = distance * Mathf.Cos(roll);
        float height = distance * Mathf.Sin(roll);
        cameraPos.x = targetPos.x + d * Mathf.Cos(rot);
        cameraPos.y = targetPos.y + height;
        cameraPos.z = targetPos.z + d * Mathf.Sin(rot);
        Camera.main.transform.position = cameraPos;

        //对准目标
        Camera.main.transform.LookAt(target.transform);
    }


    void Rotate()//横向旋转相机
    {
        float w = Input.GetAxis("Mouse X") * rotSpeed;
        rot -= w;
    }
    void Roll()
    {
        float w = Input.GetAxis("Mouse Y") * rollSpeed * 0.5f;
        roll -= w;

        if (roll > maxRoll)
            roll = maxRoll;
        if (roll < minRoll)
            roll = minRoll;
    }

    void Zoom()
    {
        if (Input.GetAxis("Mouse Scrollwheel") > 0)
        {
            if (distance > minDistance)
                distance -= zoomSpeed;
        }
        else if (Input.GetAxis("Mouse Scrollwheel") < 0)
        {
            if (distance < maxDistance)
                distance += zoomSpeed;
        }
    }

   
}
