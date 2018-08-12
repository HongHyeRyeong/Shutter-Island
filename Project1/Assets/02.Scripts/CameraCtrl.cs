﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public static CameraCtrl instance;

    public Camera MainCam;

    // Survivor
    [HideInInspector]
    public Transform targetSurvivorComPivot;

    private float height = 3f;

    //Murderer
    [HideInInspector]
    public Transform targetMurderer;
    [HideInInspector]
    public Transform targetMurdererCamPivot;
    [HideInInspector]
    public MurdererCtrl MurCtrl;
    [SerializeField]
    private CameraFilter_Attack FilterAttack;

    private int saveState = -1;
    private float MouseY;
    private float angleX = 0;
    private float time = 0;
    private int AttackNum = 0;

    private void Awake()
    {
        instance = this;
    }

    void LateUpdate()
    {
        if (GameCtrl.instance.Character == 1)
        {
            if (targetSurvivorComPivot)
            {
                Vector3 pos = targetSurvivorComPivot.position;

                float dist = 0;
                float width = 0;

                RaycastHit hitinfo;
                if (Physics.Raycast(pos, transform.forward * -1, out hitinfo, 4f, (1 << LayerMask.NameToLayer("Map"))))   // 맵과 카메라 충돌
                {
                    dist = hitinfo.distance;
                    height = hitinfo.point.y - pos.y;
                    width = Mathf.Sqrt(dist * dist - height * height);

                    if (Input.GetAxis("Mouse Y") != 0)  // 마우스 움직임 가능
                    {
                        height -= Input.GetAxis("Mouse Y") * Time.deltaTime * 5;
                        height = Mathf.Clamp(height, -0.5f, 3f);
                    }

                    transform.position = pos - (targetSurvivorComPivot.forward * width) + (Vector3.up * height);
                    transform.LookAt(pos);                    

                    if (!Physics.Raycast(pos, transform.forward * -1, out hitinfo, 4, (1 << LayerMask.NameToLayer("Map")))) // 예외 처리
                    {
                        height += 1;
                        height = Mathf.Clamp(height, -0.5f, 3f);

                        transform.position = pos - (targetSurvivorComPivot.forward * width) + (Vector3.up * height);
                        transform.LookAt(pos);
                    }
                }
                else
                {
                    dist = 4f;
                    height -= Input.GetAxis("Mouse Y") * Time.deltaTime * 10;
                    height = Mathf.Clamp(height, -0.5f, 3f);
                    width = Mathf.Sqrt(dist * dist - height * height);

                    transform.position = Vector3.Lerp(transform.position,
                            pos - (targetSurvivorComPivot.forward * width) + (Vector3.up * height),
                            Time.deltaTime * 20);
                    transform.LookAt(pos);
                }
            }
        }
        else if (GameCtrl.instance.Character == 2)
        {
            if (targetMurdererCamPivot)
            {
                int state = MurCtrl.GetState();

                if (state == 0)
                {
                    if (saveState != state)
                    {
                        saveState = state;
                        MainCam.cullingMask = ~(1 << 14);
                    }

                    transform.position = Vector3.Lerp(transform.position, new Vector3(
                        targetMurdererCamPivot.transform.position.x,
                        targetMurderer.transform.position.y + 2.35f,
                        targetMurdererCamPivot.transform.position.z), Time.deltaTime * 20);

                    MouseY -= Input.GetAxis("Mouse Y") * Time.deltaTime * 80;
                    MouseY = ClampAngle(MouseY, -30, 50);
                    transform.rotation = Quaternion.Euler(MouseY, targetMurderer.eulerAngles.y, 0);
                }
                else if (state == 1)
                {
                    if (saveState != state)
                    {
                        saveState = state;
                        MainCam.cullingMask = ~(1 << 14);
                    }

                    transform.position = Vector3.Lerp(transform.position, new Vector3(
                        targetMurdererCamPivot.transform.position.x,
                        targetMurderer.transform.position.y + 2.25f,
                        targetMurdererCamPivot.transform.position.z), Time.deltaTime * 20);

                    MouseY -= Input.GetAxis("Mouse Y") * Time.deltaTime * 80;
                    MouseY = ClampAngle(MouseY, -30, 50);
                    transform.rotation = Quaternion.Euler(MouseY, targetMurderer.eulerAngles.y, 0);
                }
                else
                {
                    if (saveState != state)
                    {
                        saveState = state;
                        MainCam.cullingMask = -1;

                        if (state == 3)
                            angleX = 90;
                        else if (state == 10 || state == 11)
                            angleX = 20;
                        else
                            angleX = 0;

                        time = 0;
                    }

                    transform.position = targetMurdererCamPivot.transform.position;

                    if (state == 3 && angleX > 0)
                    {
                        if (time > 1.5f)
                            angleX -= Time.deltaTime * 100;
                        else
                            time += Time.deltaTime;
                    }

                    transform.rotation = Quaternion.Euler(
                        targetMurdererCamPivot.transform.eulerAngles.x + angleX,
                        targetMurderer.transform.eulerAngles.y, 0);

                    MouseY = transform.localRotation.x + angleX;
                }
            }
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    public void AttackEffect()
    {
        AttackNum++;

        if (AttackNum == 1)
            StartCoroutine(Attack(4));
    }

    IEnumerator Attack(float delay)
    {
        FilterAttack.Fade = 0;

        while (FilterAttack.Fade <= 1)
        {
            FilterAttack.Fade += Time.deltaTime * 0.5f;
            yield return null;
        }

        while (AttackNum != 0)
        {
            yield return new WaitForSeconds(delay);
            AttackNum--;
        }
        yield return new WaitForSeconds(delay);

        FilterAttack.Fade = 1;

        while (FilterAttack.Fade >= 0)
        {
            FilterAttack.Fade -= Time.deltaTime * 0.3f;
            yield return null;
        }
    }
}