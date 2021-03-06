﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankManager : MonoBehaviour
{
    public static RankManager instance;

    //
    [SerializeField]
    public Text UserID;
    [SerializeField]
    public Text SurRankTxt;
    [SerializeField]
    public Text MurRankTxt;
    [SerializeField]
    public GameObject RankUp;

    //
    [HideInInspector]
    public string UserName;

    private int Check;
    private int SurScore;
    private int MurScore;
    private int SurRank;
    private int MurRank;

    //
    public string SurRankURL;
    public string MurRankURL;

    void Start()
    {
        instance = this;

        UserID.text = Login.GetID();
        UserName = UserID.text;
        SurRankTxt.text = Login.GetSurRank();
        MurRankTxt.text = Login.GetMurRank();

        if (ResultCtrl.instance != null)
        {
            int.TryParse(SurRankTxt.text, out SurRank);
            int.TryParse(MurRankTxt.text, out MurRank);

            SurRankURL = "wjddus0424.dothome.co.kr/SurvivorRankUp.php";
            MurRankURL = "wjddus0424.dothome.co.kr/MurdererRankUp.php";

            Check = ResultCtrl.instance.GetCheck();

            if (Check == 1 || Check == 2)
                SurScore = ResultCtrl.instance.GetSurvivorTotalScore();
            else if (Check == 3 || Check == 4)
                MurScore = ResultCtrl.instance.GetMurdererTotalScore();

            if (SurScore >= 3000)
            {
                if (SurRank > 1)
                    SurRank--;
                else if (SurRank == 1)
                    SurRank = 1;

                print(SurRank);

                Login.SetSurRank(SurRank.ToString());
                RankUp.SetActive(true);

                StartCoroutine(SurRankUp());

                SurRankTxt.text = SurRank.ToString();
            }
            else if (MurScore >= 3000)
            {
                if (MurRank > 1)
                    MurRank--;
                else if (MurRank == 1)
                    MurRank = 1;

                print(MurRank);

                Login.SetMurRank(MurRank.ToString());
                RankUp.SetActive(true);

                StartCoroutine(MurRankUp());

                MurRankTxt.text = MurRank.ToString();
            }
        }
    }

    IEnumerator SurRankUp()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", UserName);
        form.AddField("SurvivorRank", SurRank);

        WWW webRequest = new WWW(SurRankURL, form);
        yield return webRequest;

        print(webRequest.text);

        yield return null;
    }

    IEnumerator MurRankUp()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", UserName);
        form.AddField("MurdererRank", MurRank);

        WWW webRequest = new WWW(MurRankURL, form);
        yield return webRequest;

        print(webRequest.text);

        yield return null;
    }
}
