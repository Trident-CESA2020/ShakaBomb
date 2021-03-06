﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class CameraController : MonoBehaviour
{

    // 背景情報
    [System.Serializable]
    public struct BGInfo
    {
        public GameObject obj;
        public Vector2 size;
    }

    // リスポーン許可
    public bool RespawnApproval { get; set; }

    // カメラ
    [SerializeField]
    Camera mainCamera;
    // プレイヤー
    [SerializeField]
    GameObject player;
    PlayerController pController;
    Rigidbody2D playerRigi;
    // カメラの視野角
    [SerializeField]
    float cameraViewRange;
    // カメラの挙動範囲
    [SerializeField]
    Rect cameraRange;
    [SerializeField]
    Vector3 initializePos;

    [SerializeField]
    float cellX;
    [SerializeField]
    float cellY;

    // デバックカメラ揺れ
    CameraShake cameraShake;
    // 揺れる前の座標
    Vector3 originPos = Vector3.zero;
    // 次のブロックへのポジション
    Vector3 nextPos;
    // 現在のポジション
    Vector3 currentPos;
    // 追従フラグ
    bool followOn = true;
    // リスポーン位置記憶フラグ
    bool rememberPos = false;
    // リスポーンを覚えるまでのカウント
    float respawnCount;
    // プレイヤーロックまでのカウント
    float playerLockCount;
    // 移動距離
    float distance;
    // ラープの開始時間
    float startTime;
    // 移動前のプレイヤーの速度
    PauseManager.RigidbodyVelocity playerRigidInfo;

    // 視差背景
    [Header("size=pixel/PixelPerUnit*scale")]
    [SerializeField]
    List<BGInfo> backGrounds = new List<BGInfo>();
    // 背景の開始Y座標
    [SerializeField]
    float bgBottom = 0.0f;


    private void Awake()
    {
        cameraShake = mainCamera.transform.GetComponent<CameraShake>();
        mainCamera.orthographicSize = cameraViewRange;

        if (initializePos != Vector3.zero)
        {
            initializePos.z = ConstCamera.POSITION_Z;
            mainCamera.transform.position = initializePos;
            nextPos = initializePos;
        }
        else
        {
            nextPos = mainCamera.transform.position;
        }

        pController = player.GetComponent<PlayerController>();
        playerRigi = player.GetComponent<Rigidbody2D>();

        RespawnApproval = false;

        originPos = SetCameraRangePosition(mainCamera.transform.position);
    }

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーのポジションを保存しておく
        Data.initialPlayerPos = player.transform.position;   
    }

    private void FixedUpdate()
    {
        mainCamera.transform.position = originPos;
        var fourCorners = new Rect(GetScreenTopLeft().x, GetScreenBottomRight().y, GetScreenBottomRight().x, GetScreenTopLeft().y);
        RespawnApproval = false;

        if (!followOn)
        {
            // カメラ移動中
            followOn = FollowCamera(currentPos, nextPos);
            playerLockCount += Time.deltaTime;
            if (playerLockCount >= ConstCamera.CANNOT_FRAME)
            {
                pController.EnableControl(false);
                playerLockCount = 0.0f;
            }

            if (followOn)
            {
                pController.EnableControl(true);
                RespawnApproval = true;
                playerRigi.WakeUp();
                playerRigi.velocity = playerRigidInfo.velocity;
                playerRigi.angularVelocity = playerRigidInfo.angularVeloccity;
                playerRigi.constraints = playerRigidInfo.constraints;
                player.GetComponentInChildren<PlayerAnimator>().ResumeAnimation();
            }
        }
        else
        {
            // プレイヤーの向きによって画面を切り替えるかどうかを判断
            if (playerRigi.velocity.x < 0.0f)
            {
                if (fourCorners.x > player.transform.position.x)
                {
                    UpdateNextPos(Vector3.right * -cellX);
                }
            }
            else if(playerRigi.velocity.x > 0.0f)
            {
                if (fourCorners.width < player.transform.position.x)
                {
                    UpdateNextPos(Vector3.right * cellX);
                }
            }
            else
            {
                if(Data.playerDir < 0)
                {
                    if (fourCorners.x > player.transform.position.x)
                    {
                        UpdateNextPos(Vector3.right * -cellX);
                    }
                }
                else
                {
                    if (fourCorners.width < player.transform.position.x)
                    {
                        UpdateNextPos(Vector3.right * cellX);
                    }
                }
            }

            if (fourCorners.height <= player.transform.position.y)
            {
                if (nextPos.y >= ConstCamera.FIRST_CELL_Y)
                    cellY = ConstCamera.SECOND_CELL_Y;
                UpdateNextPos(Vector3.up * cellY);
            }
            if (fourCorners.y >= player.transform.position.y)
            {
                if (nextPos.y <= ConstCamera.FIRST_CELL_Y)
                    cellY = ConstCamera.FIRST_CELL_Y;
                UpdateNextPos(Vector3.up * -cellY);
            }
            // カメラが移動していないときの設定
            startTime = Time.time;
            currentPos = mainCamera.transform.position;
            distance = Vector3.Distance(nextPos, currentPos);
        }

        //// リスポーンポジションの記憶とカウント
        //if (rememberPos)
        //{
        //    respawnCount += Time.deltaTime;
        //    //　数フレームは記憶する
        //    if (respawnCount >= ConstCamera.REMEMBER_FRAME)
        //    {
        //        Data.initialPlayerPos = player.transform.position;
        //        rememberPos = false;
        //    }
        //}
        //else
        //{
        //    respawnCount = 0.0f;
        //}

        // カメラの範囲指定を適用
        originPos = SetCameraRangePosition(mainCamera.transform.position);
        mainCamera.transform.position = SetCameraRangePosition(mainCamera.transform.position + cameraShake.shakeMove);

        // 背景の移動
        MoveBackGrounds();
    }

    public void ResetCameraPos(Vector3 target)
    {
        bool loop = true;
        while (loop)
        {
            loop = false;
            var topLeft = GetScreenTopLeft();
            var bottomRight = GetScreenBottomRight();

            if (bottomRight.x < target.x)
            {
                mainCamera.transform.Translate(cellX, 0, 0);
                loop = true;
            }
            else if (topLeft.x > target.x)
            {
                mainCamera.transform.Translate(-cellX, 0, 0);
                loop = true;
            }

            if (topLeft.y < target.y)
            {
                if (mainCamera.transform.position.y >= ConstCamera.FIRST_CELL_Y)
                    cellY = ConstCamera.SECOND_CELL_Y;
                mainCamera.transform.Translate(0, cellY, 0);
                loop = true;
            }
            else if (bottomRight.y > target.y)
            {
                if (mainCamera.transform.position.y <= ConstCamera.FIRST_CELL_Y)
                    cellY = ConstCamera.FIRST_CELL_Y;
                mainCamera.transform.Translate(0, -cellY, 0);
                loop = true;
            }
        }

        nextPos = mainCamera.transform.position;
        originPos = SetCameraRangePosition(mainCamera.transform.position);
    }

    /// <summary>
    /// カメラ追従
    /// Updateで呼ぶ
    /// </summary>
    /// <param name="playerPos">プレイヤーポジション</param>
    /// <returns>完了していればtrue</returns>
    private bool FollowCamera(Vector3 start, Vector3 end)
    {
        var percentage = ((Time.time - startTime) * ConstCamera.SPEED) / distance;

        mainCamera.transform.position = Vector3.Lerp(start, end, percentage);

        // 移動したらTrueを返す
        if (CheckMove(mainCamera.transform.position, end))
            return true;

        return false;
    }


    /// <summary>
    /// 背景の移動
    /// </summary>
    private void MoveBackGrounds()
    {
        foreach (var bg in backGrounds)
        {
            // デフォルトのオフセット位置を設定
            Vector3 offset = new Vector3(0, 0, bg.obj.transform.localPosition.z);
            if (cameraRange.width - cameraRange.x > Mathf.Epsilon)
            {
                float t = (mainCamera.transform.position.x - cameraRange.x) / (cameraRange.width - cameraRange.x);
                float width =
                    Mathf.Max(bg.size.x * bg.obj.transform.lossyScale.x - mainCamera.orthographicSize * mainCamera.aspect * 2.0f, 0.0f);
                offset.x -= Mathf.Lerp(-width * 0.5f, width * 0.5f, t);
            }
            if (cameraRange.height - cameraRange.y > Mathf.Epsilon)
            {
                if (mainCamera.transform.position.y - bgBottom < mainCamera.orthographicSize)
                {
                    float lim = (bgBottom + bg.size.y * bg.obj.transform.lossyScale.y * 0.5f) - mainCamera.transform.position.y;
                    offset.y = lim;
                }
                else
                {
                    float rangeY = bgBottom + mainCamera.orthographicSize;
                    float t = (mainCamera.transform.position.y - rangeY) / (cameraRange.height - rangeY);
                    float height = Mathf.Max(bg.size.y * bg.obj.transform.lossyScale.y - mainCamera.orthographicSize * 2.0f, 0.0f);
                    offset.y -= Mathf.Lerp(-height * 0.5f, height * 0.5f, t);

                }
            }
            bg.obj.transform.localPosition = offset;
        }
    }

    /// <summary>
    /// カメラのスクロール先を更新する
    /// </summary>
    /// <param name="move">移動量</param>
    private void UpdateNextPos(Vector3 move)
    {
        followOn = false;
        //rememberPos = true;
        nextPos += move;

        // リスポーン地点を更新
        Data.initialPlayerPos = player.transform.position + move.normalized * 1.5f;

        var rigid = player.GetComponent<Rigidbody2D>();
        playerRigidInfo = new PauseManager.RigidbodyVelocity(rigid);
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
        rigid.Sleep();
        player.GetComponentInChildren<PlayerAnimator>().StopAnimation();
    }

    /// <summary>
    /// ラープ完了したかどうか
    /// </summary>
    /// <param name="start">スタートポジション</param>
    /// <param name="end">エンドポジション</param>
    /// <returns>完了していればtrue</returns>
    private bool CheckMove(Vector3 start, Vector3 end)
    {
        //if (Mathf.Approximately(start.y, end.y) && Mathf.Approximately(start.x, end.x))
        //    return true;
        if (CheckDifferences(start, end, 0.1f))
            return true;

        return false;
    }

    /// <summary>
    /// Floatの値を比べる
    /// 既存のものではTrueになるのが遅いため
    /// </summary>
    /// <param name="x">比べる値</param>
    /// <param name="y">比べる値</param>
    /// <param name="differences">許容値</param>
    /// <returns>近い値になったかどうか</returns>
    private bool CheckDifferences(Vector3 start, Vector3 end, float differences)
    {
        float temp = Vector3.Distance(start, end);

        if (temp <= differences)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 画面の範囲指定をする
    /// </summary>
    /// <param name="pos">カメラのポジション</param>
    /// <returns></returns>
    private Vector3 SetCameraRangePosition(Vector3 pos)
    {
        Vector3 temp;
        temp = new Vector3(Mathf.Clamp(pos.x, cameraRange.x, cameraRange.width)
                         , Mathf.Clamp(pos.y, cameraRange.y, cameraRange.height)
                         , pos.z);
        return temp;
    }

    /// <summary>
    /// 画面の左上座標を取得
    /// </summary>
    /// <returns>画面の左上座標</returns>
    public Vector3 GetScreenTopLeft()
    {
        // 画面の左上を取得
        Vector3 topLeft = mainCamera.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 0.0f));
        //topLeft.Scale(new Vector3(1f, -1f, 1f));
        return topLeft;
    }

    /// <summary>
    /// 画面の右下座標を取得
    /// </summary>
    /// <returns>画面の右下座標</returns>
    public Vector3 GetScreenBottomRight()
    {
        // 画面の右下を取得
        Vector3 bottomRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f));
        // 上下反転させる
        //bottomRight.Scale(new Vector3(1f, -1f, 1f));
        return bottomRight;
    }
}