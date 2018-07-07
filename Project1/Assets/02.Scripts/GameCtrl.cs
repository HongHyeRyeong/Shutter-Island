﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCtrl : MonoBehaviour
{
    public static GameCtrl instance;

    public GameObject Survivor;
    public GameObject Murderer;

    public GameObject Camera;
    public GameObject GameController;
    public GameObject SurGameController;
    public GameObject MurGameController;
    public GameObject UI;

    [SerializeField]
    private Material SkyboxDay;
    [SerializeField]
    private Material SkyboxNight;

    // inGame
    private int Character;
    private int Skill = 1;

    private int MachineCompleteNum = 0;
    private int PrisonSurNum = 0;

    // Survivor
    [SerializeField]
    private GameObject SurvivorFootPrints;
    [SerializeField]
    private GameObject FootPrint;
    private GameObject[] FootPrints = new GameObject[3000];
    private int FootPrintsNum = -1;
    private float delay = 0.05f;
    private float savedelay;

    // Murderer
    public GameObject Murdererskill1;

    // score
    int SurvivorScore = 0;
    int MurdererScore = 0;

    // fps
    float deltaTime = 0.0f;
    float fps;

    private void Start()
    {
        PhotonNetwork.isMessageQueueRunning = true;
        Application.targetFrameRate = 60;

        instance = this;

        if (CharacterSelect.SelSur == true)
        {
            Character = 1;
            UI = UI.transform.Find("SurvivorUI").gameObject;

            // 맵에 따라 다른 생성 위치
            if (PhotonInit.Map == 1)
                Survivor = PhotonNetwork.Instantiate("Survivor", new Vector3(-37, 0.0f, 36), Quaternion.identity, 0);
            else
                Survivor = PhotonNetwork.Instantiate("Survivor", new Vector3(80, 0.0f, 36), Quaternion.identity, 0);

            // 각 오브젝트 적용
            Camera.GetComponent<CameraCtrl>().SetCharacter(Character);

            SurGameController.SetActive(true);
            SurGameController.GetComponent<SurvivorUICtrl>().Init();
            UI.SetActive(true);
        }
        else
        {
            Character = 2;
            UI = UI.transform.Find("MurdererUI").gameObject;

            if (Skill == 1)
                Murdererskill1.SetActive(true);

            // 맵에 따라 다른 생성 위치
            if (PhotonInit.Map == 1)
                Murderer = PhotonNetwork.Instantiate("Murderer", new Vector3(10, 0.0f, 67), Quaternion.identity, 0);
            else
                Murderer = PhotonNetwork.Instantiate("Murderer", new Vector3(80, 0.0f, 36), Quaternion.identity, 0);

            // 각 오브젝트 적용
            Camera.GetComponent<CameraCtrl>().SetCharacter(Character);

            MurGameController.SetActive(true);
            MurGameController.GetComponent<MurdererUICtrl>().Init();
            UI.SetActive(true);
        }

        // SkyBox
        int sky = Random.Range(1, 3);

        if (sky == 1)
            Camera.AddComponent<Skybox>().material = SkyboxDay;
        else
            Camera.AddComponent<Skybox>().material = SkyboxDay;

        savedelay = delay;

        //StartCoroutine(DisFPS());
    }


    public void MachineComplete()
    {
        MachineCompleteNum++;

        if (Character == 1)
            SurGameController.GetComponent<SurvivorUICtrl>().DisMachine(MachineCompleteNum);
        else if (Character == 2)
        {
            Murderer.GetComponent<MurdererCtrl>().DamageByMachine(40);
            MurGameController.GetComponent<MurdererUICtrl>().DisMachine(MachineCompleteNum);
        }
    }

    public void DisPrison(Vector3 pos, int num)
    {
        if (Character == 1)
            SurGameController.GetComponent<SurvivorUICtrl>().DisPrison(pos, num);
        else if (Character == 2)
            MurGameController.GetComponent<MurdererUICtrl>().DisPrison(pos, num);
    }

    public void DisSurPrison(int num)
    {
        PrisonSurNum += num;

        if (Character == 2)
            MurGameController.GetComponent<MurdererUICtrl>().DisSurPrison(PrisonSurNum);
    }

    public void SetPrisons(int num, bool b, int Surnum)
    {
        if (Character == 1)
            SurGameController.GetComponent<SurvivorUICtrl>().SetPrisons(num, b);
        else if (Character == 2)
        {
            PrisonSurNum -= Surnum;
            MurGameController.GetComponent<MurdererUICtrl>().DisSurPrison(PrisonSurNum);
            MurGameController.GetComponent<MurdererUICtrl>().SetPrisons(num, b);
        }
    }

    public void DisMurHP(float hp)
    {
        if (Character == 1)
            SurGameController.GetComponent<SurvivorUICtrl>().DisMurHP(hp);
    }

    public void UseFootPrint(Vector3 SurPos)
    {
        if (delay != savedelay)
            return;
        StartCoroutine(DelayTime());

        bool isNew = true;

        float randomX = Random.Range(-1.2f, 1.2f);
        Vector3 Pos = new Vector3(
                randomX + SurPos.x, SurPos.y + 1.5f, SurPos.z);

        for (int i = 0; i <= FootPrintsNum; ++i)
        {
            if (!FootPrints[i].activeSelf)
            {
                isNew = false;

                FootPrints[i].SetActive(true);
                FootPrints[i].transform.position = Pos;
                StartCoroutine(FootPrints[i].GetComponent<FootPrintCtrl>().Use());
                break;
            }
        }

        if (isNew)
        {
            FootPrints[++FootPrintsNum] = Instantiate(FootPrint, Pos,
                Quaternion.Euler(90, 0, Random.Range(0, 360)));
            FootPrints[FootPrintsNum].transform.parent = SurvivorFootPrints.transform;
            StartCoroutine(FootPrints[FootPrintsNum].GetComponent<FootPrintCtrl>().Use());
        }
    }

    IEnumerator DelayTime()
    {
        while (true)
        {
            delay -= Time.deltaTime;

            if (delay <= 0)
                break;

            yield return null;
        }
        delay = savedelay;
    }

    public void SetSurvivorScore(int score)
    {
        SurvivorScore += score;
        print(SurvivorScore);
    }

    public void SetMurdererScore(int score)
    {
        MurdererScore += score;
        print(MurdererScore);
    }

    IEnumerator DisFPS()
    {
        while (true)
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;

            if (Character == 1)
                SurGameController.GetComponent<SurvivorUICtrl>().DisFPS(fps);
            else if (Character == 2)
                MurGameController.GetComponent<MurdererUICtrl>().DisFPS(fps);

            yield return null;
        }
    }
}
