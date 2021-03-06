﻿//==============================================================================================
/// File Name	: BalloonController.cs
/// Summary		: バルーン制御
//==============================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
//==============================================================================================
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
//==============================================================================================
public class BalloonController : MonoBehaviour
{
    //------------------------------------------------------------------------------------------
    // member variable
    //------------------------------------------------------------------------------------------
    // リジッドボディ
    private Rigidbody2D      m_rigid2D     = null;
    // ラインレンダラー 
    private LineRenderer     m_line        = null;
    // プレイヤーの情報
    private GameObject       m_player      = null;
    // バルーンジェネレータ
    private BalloonGenerator m_balloonG    = null;

    // 時間経過で消えるまでの時間
    [SerializeField]
    private float            m_lifeTime    = 10.0f;

    // 消えるかどうか
    private bool             m_isDestroy   = false;

    //------------------------------------------------------------------------------------------
    // Start
    //------------------------------------------------------------------------------------------
    private void Start()
    {
        Init();
    }

    //------------------------------------------------------------------------------------------
    // Update
    //------------------------------------------------------------------------------------------
    private void Update()
    {
        m_lifeTime -= Time.deltaTime;
        if (m_lifeTime <= 0.0f)
        {
            Destroy();
            return;
        }

        Vector3 playerPos = m_player.transform.position;
        if (Data.playerDir > 0)
        {
            playerPos.x -= ConstBalloon.DISTANCE_X;
        }
        else
        {
            playerPos.x += ConstBalloon.DISTANCE_X;
        }

        playerPos.y += ConstBalloon.DISTANCE_Y;

        Vector3 move_force = playerPos - this.transform.position;

        if(Vector3.Distance(playerPos,this.transform.position) >= 4)
        {
            m_rigid2D.velocity = move_force * 4.0f;
        }
        m_rigid2D.velocity = move_force * 2.0f;

        Vector3 thisPos = this.transform.position;

        m_line.SetPosition(0, thisPos);
        m_line.SetPosition(1, m_player.transform.position);
    }

    //------------------------------------------------------------------------------------------
    // OnTriggerEnter2D
    //------------------------------------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == ConstStage.DAMAGE_TILE)
        {
            Destroy();
        }
    }

    //------------------------------------------------------------------------------------------
    // 初期化
    //------------------------------------------------------------------------------------------
    private void Init()
    {
        m_rigid2D = GetComponent<Rigidbody2D>();
        m_player = GameObject.Find(ConstPlayer.NAME);
        m_balloonG = GameObject.Find(ConstBalloon.GENERATOR).GetComponent<BalloonGenerator>();
        m_line = GetComponent<LineRenderer>();

        m_line.startWidth = ConstBalloon.LINE_WIDTH;
        m_line.endWidth = ConstBalloon.LINE_WIDTH;
        m_line.positionCount = 2;
    }

    //------------------------------------------------------------------------------------------
    // Destroy
    //------------------------------------------------------------------------------------------
    public void Destroy()
    {
        m_balloonG.BrokenBalloon(gameObject);
        GenerateBurstEffect();
        Destroy(this.gameObject);
    }

    //------------------------------------------------------------------------------------------
    // 破裂エフェクトの生成
    //------------------------------------------------------------------------------------------
    private void GenerateBurstEffect()
    {
        EffectGenerator.BubbleBurstFX(
            new BubbleBurstFX.Param(GetComponent<SpriteRenderer>().color, transform.lossyScale),
            transform.position,
            null);
    }
}
