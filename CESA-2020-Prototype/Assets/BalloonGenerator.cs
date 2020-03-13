﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class BalloonGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject balloonPrefab;

    bool isCreate;

    Vector3 createPosition;

    // 所持バルーン
    //private GameObject[] m_balloons = new GameObject[Balloon.MAX];
    private List<GameObject> m_balloonList = new List<GameObject>();

    void Awake()
    {
        isCreate = false;

        //for (int i = 0; i < m_balloons.Length; i++)
        //    m_balloons[i] = null;
    }

    void Update()
    {
        //if (isCreate)
        //{
        //    if (m_balloons[Data.num_balloon] == null)
        //    {
        //        // プレファブと同じオブジェクトを作る
        //        m_balloons[Data.num_balloon] = Instantiate(balloonPrefab) as GameObject;
        //        // 座標を設定する
        //        m_balloons[Data.num_balloon].transform.position = createPosition;
        //        // 所持バルーンをカウント
        //        Data.num_balloon++;

        //        // デバッグ
        //        Debug.Log("所持しているバルーン " + Data.num_balloon + " / " + Balloon.MAX + "個");
        //    }
        //    // 作っていない状態にする
        //    isCreate = false;
        //}

        if (isCreate)
        {
            // プレファブと同じオブジェクトを作る
            GameObject go = Instantiate(balloonPrefab) as GameObject;
            // 座標を設定する
            go.transform.position = createPosition;
            // 所持バルーンをカウント
            Data.num_balloon++;
            m_balloonList.Add(go);
            // 作っていない状態にする
            isCreate = false;
        }

        // デバッグ(泡を使用)
        if (Input.GetKeyDown(KeyCode.C) && m_balloonList.Count >= 1)
        {
            UsedBubble();
        }

        // デバッグ
        Debug.Log("所持しているバルーン " + m_balloonList.Count + " / " + Balloon.MAX + "個");
    }

    public void CreateBalloon(Vector3 create_pos)
    {
        createPosition = create_pos;
        isCreate = true;
    }

    public void UsedBubble()
    {
        Destroy(m_balloonList[0]);
        m_balloonList.RemoveAt(0);
        Data.num_balloon--;
    }
}
