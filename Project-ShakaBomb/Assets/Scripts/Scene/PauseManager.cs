﻿//==============================================================================================
/// File Name	: PauseManager.cs
/// Summary		: 
//==============================================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Common;
//==============================================================================================
// ポーズ管理クラス
public class PauseManager : MonoBehaviour
{
    // オブジェクト名
    public const string NAME = "PauseManager";


    // Rigidbodyの速度を保存するクラス
    public class RigidbodyVelocity
    {
        public Vector2 velocity;          // 速度
        public float angularVeloccity;    // 角速度
        public RigidbodyConstraints2D constraints;   // 固定状態

        public RigidbodyVelocity(Rigidbody2D rigidbody)
        {
            velocity = rigidbody.velocity;
            angularVeloccity = rigidbody.angularVelocity;
            constraints = rigidbody.constraints;
        }
    }


    [SerializeField]
    [Header("(全てを対象にする場合は空でいい)")]
    [Header("ポーズを適用するオブジェクトのrootオブジェクト")]
    private GameObject objectsWrapper;      // ポーズを適用するオブジェクトの範囲
    
    [SerializeField]
    private GameObject pauseMenu;            // ポーズメニューオブジェクト
    [SerializeField]
    private GameObject[] ignoreGameObjects;  // ポーズの影響を受けないオブジェクト

    private bool canPauseButton = true;                 //ボタンを押したときにポーズ可能かどうか
    private bool isPausing = false;                     //ポーズ中かどうか
    private RigidbodyVelocity[] rigidbodyVelocities;    //ポーズ前の速度の配列
    private Rigidbody2D[] pausingRigidbodies;           //ポーズ中のRigidbodyの配列
    private MonoBehaviour[] pausingMonoBehaviours;      //ポーズ中のMonoBehaviourの配列
    private Animator[] pausingAnimators;                //ポーズ中のAnimatorの配列
    private float[] animatorSpeeds;                     //ポーズ前のアニメーション速度の配列
    private ParticleSystem[] pausingParticleSystems;    //ポーズ中のParticleSystemの配列
    private bool[] particleEmittings;                   //ポーズ中の放出状態の配列

    private static Canvas pauseCanvas;   //ポーズ用Canvas
    private static Image pauseImage;     //ポーズ用Image
    private Color filterColor = new Color(0.0f, 0.0f, 0.0f, 0.6f);   //ポーズ用Imageのカラー
    private Color defaultFilterColor = Color.black;                  //デフォルトのポーズ用Imageのカラー
    private float fadeTime = 0.0f;
    private float time = 0.0f;

    [SerializeField]
    // 効果音
    private AudioClip pauseSE = null;



    //------------------------------------------------------------------------------------------
    // summary : ポーズ用のCanvasとImage生成
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    private void CreatePauseFilter()
    {
        //ポーズ用のCanvas生成
        GameObject FadeCanvasObject = new GameObject("CanvasPause");
        pauseCanvas = FadeCanvasObject.AddComponent<Canvas>();
        FadeCanvasObject.AddComponent<GraphicRaycaster>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //FadeCanvasObject.AddComponent<FadeManager>();

        //前面になるよう適当なソートオーダー設定
        pauseCanvas.sortingOrder = 50;

        //ポーズ用のImage生成
        GameObject imagePauseObject = new GameObject("ImagePause");
        pauseImage = imagePauseObject.AddComponent<Image>();
        pauseImage.transform.SetParent(pauseCanvas.transform, false);
        pauseImage.rectTransform.anchoredPosition = Vector3.zero;

        pauseImage.rectTransform.sizeDelta = new Vector2(9999, 9999);

        //色の設定
        pauseImage.color = new Color(0f, 0f, 0f, 0f);

        // ポーズの影響を受けないオブジェクトを追加する
        ignoreGameObjects = ignoreGameObjects.Concat(new GameObject[] {
            FadeCanvasObject,
            FadeManager.GetCanvas().gameObject,
            SoundPlayer.GetAudioSource().gameObject
        }).ToArray();
        var obj = SceneEffecterController.instance.gameObject;
        if (obj)
        {
            AddIgnoreObject(obj);
        }
    }



    //------------------------------------------------------------------------------------------
    // summary : Start
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    private void Start()
    {
        defaultFilterColor = filterColor;
        CreatePauseFilter();
    }



    //------------------------------------------------------------------------------------------
    // summary : Update
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    private void Update()
    {
        //ボタンが押されたら状態を変更する
        bool pressPause = (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7));

        if (pressPause && !FadeManager.isFadeOut && canPauseButton)
        {
            SoundPlayer.Play(pauseSE);
            filterColor = defaultFilterColor;
            ChangePauseState();
        }

        if (isPausing && time < fadeTime)
        {
            time += Time.deltaTime;
            float t = Mathf.Min((fadeTime > 0.0f ? time / fadeTime : 0.0f), 1.0f);
            pauseImage.color = Color.Lerp(new Color(0, 0, 0, 0), filterColor, t);
        }
    }



    //------------------------------------------------------------------------------------------
    // summary : ボタンを押したときにポーズ可能か設定する
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    public void EnablePauseButton(bool enable)
    {
        canPauseButton = enable;
    }



    //------------------------------------------------------------------------------------------
    // summary : ポーズ状態を変更する
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    public void ChangePauseState()
    {
        var go = GameObject.Find(ConstPlayer.NAME);
        PlayerAnimator playerAnimator = null;
        if (go)
        {
            playerAnimator = go.GetComponentInChildren<PlayerAnimator>();
        }
        if (!isPausing)
        {
            //ポーズメニューを起動する
            pauseMenu.SetActive(true);
            Pause(0.001f);
            if (playerAnimator)
            {
                playerAnimator.StopAnimation();
            }
        }
        else
        {
            Resume();
            if (playerAnimator)
            {
                playerAnimator.ResumeAnimation();
            }
        }
    }



    //------------------------------------------------------------------------------------------
    // summary : 中断処理
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    public void Pause(float time)
    {
        if (isPausing)
        {
            return;
        }
        isPausing = true;
        fadeTime = time;
        this.time = 0.0f;

        bool hasNull = false;
        foreach(var obj in ignoreGameObjects)
        {
            if (obj == null)
            {
                hasNull = true;
                break;
            }
        }
        // nullを破棄する
        //if (ignoreGameObjects.Contains(null))
        if (hasNull)
        {
            var ignoreList = new List<GameObject>(ignoreGameObjects);
            ignoreList.RemoveAll(item => item == null);
            ignoreGameObjects = ignoreList.ToArray();
        }

        //Rigidbodyの停止
        //子要素から、スリープ中でなく、IgnoreGameObjectsに含まれていないRigidbodyを抽出
        Predicate<Rigidbody2D> rigidbodyPredicate =
            obj => !obj.IsSleeping() &&
                   Array.FindIndex(ignoreGameObjects, gameObject => gameObject == obj.gameObject) < 0;
        pausingRigidbodies = Array.FindAll(
                (objectsWrapper
                ? objectsWrapper.GetComponentsInChildren<Rigidbody2D>()
                : Utility.GetComponentsInActiveScene<Rigidbody2D>()),
                rigidbodyPredicate);
        rigidbodyVelocities = new RigidbodyVelocity[pausingRigidbodies.Length];
        for (int i = 0; i < pausingRigidbodies.Length; ++i)
        {
            //速度と角速度の保存
            rigidbodyVelocities[i] = new RigidbodyVelocity(pausingRigidbodies[i]);
            //Rigidbodyの停止
            pausingRigidbodies[i].constraints = RigidbodyConstraints2D.FreezeAll;
            pausingRigidbodies[i].Sleep();
        }

        //MonoBehaviourの停止
        //子要素から、有効かつこのインスタンスでないもの、IgnoreGameObjectsに含まれていないMonoBehaviourを抽出
        Predicate<MonoBehaviour> monoBehaviourPredicate =
            obj => obj.enabled &&
                   obj != this &&
                   Array.FindIndex(ignoreGameObjects, gameObject =>
                    Array.FindIndex(gameObject.GetComponentsInChildren<Transform>(), child => child == obj.transform) >= 0) < 0;
        var debugMonoBehaviours = Utility.GetComponentsInActiveScene<MonoBehaviour>();
        pausingMonoBehaviours = Array.FindAll(
            (objectsWrapper
            ? objectsWrapper.GetComponentsInChildren<MonoBehaviour>()
            : Utility.GetComponentsInActiveScene<MonoBehaviour>()),
            monoBehaviourPredicate);
        foreach (var monoBehaviour in pausingMonoBehaviours)
        {
            //MonoBehaviourの停止
            monoBehaviour.enabled = false;
        }

        //Animatorの停止
        //子要素から、有効である、IgnoreGameObjectsに含まれていないAnimatorを抽出
        Predicate<Animator> animatorPredicate =
            obj => obj.enabled &&
                   Array.FindIndex(ignoreGameObjects, gameObject => gameObject == obj.gameObject) < 0;
        pausingAnimators = Array.FindAll(
            (objectsWrapper
            ? objectsWrapper.GetComponentsInChildren<Animator>()
            : Utility.GetComponentsInActiveScene<Animator>()),
            animatorPredicate);
        animatorSpeeds = new float[pausingAnimators.Length];
        for (int i = 0; i < pausingAnimators.Length; ++i)
        {
            //速度の保存
            animatorSpeeds[i] = pausingAnimators[i].speed;
            //Animatorの停止
            pausingAnimators[i].speed = 0f;
        }

        //パーティクルの停止
        //子要素から、再生中である、IgnoreGameObjectsに含まれていないParticleSystemを抽出
        Predicate<ParticleSystem> particleSystemPredicate =
            obj => obj.isPlaying &&
                   Array.FindIndex(ignoreGameObjects, gameObject => gameObject == obj.gameObject) < 0;
        pausingParticleSystems = Array.FindAll(
            (objectsWrapper
            ? objectsWrapper.GetComponentsInChildren<ParticleSystem>()
            : Utility.GetComponentsInActiveScene<ParticleSystem>()),
            particleSystemPredicate);
        particleEmittings = new bool[pausingParticleSystems.Length];
        for (int i = 0; i < pausingParticleSystems.Length; ++i)
        {
            //放出状態の保存
            particleEmittings[i] = pausingParticleSystems[i].isEmitting;
            //ParticleSystemの停止
            pausingParticleSystems[i].Pause();
        }

    }



    //------------------------------------------------------------------------------------------
    // summary : 再開処理
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    public void Resume()
    {
        if (!isPausing)
        {
            return;
        }
        isPausing = false;

        //画面を元に戻す
        pauseImage.color = new Color(0f, 0f, 0f, 0f);

        //ポーズメニューを終了する
        pauseMenu.SetActive(false);

        //Rigidbodyの再開
        for (int i = 0; i < pausingRigidbodies.Length; i++)
        {
            pausingRigidbodies[i].WakeUp();
            pausingRigidbodies[i].velocity = rigidbodyVelocities[i].velocity;
            pausingRigidbodies[i].angularVelocity = rigidbodyVelocities[i].angularVeloccity;
            pausingRigidbodies[i].constraints = rigidbodyVelocities[i].constraints;
        }

        //MonoBehaviourの再開
        foreach (var monoBehaviour in pausingMonoBehaviours)
        {
            monoBehaviour.enabled = true;
        }

        //Animatorの再開
        for (int i = 0; i < pausingAnimators.Length; ++i)
        {
            pausingAnimators[i].speed = animatorSpeeds[i];
        }

        //ParticleSystemの再開
        for (int i = 0; i < pausingParticleSystems.Length; ++i)
        {
            pausingParticleSystems[i].Play();
            if (!particleEmittings[i])
            {
                pausingParticleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }



    //------------------------------------------------------------------------------------------
    // summary : カラーをセットする
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    public void SetFilterColor(in Color color)
    {
        filterColor = color;
    }



    //------------------------------------------------------------------------------------------
    // summary : カラーをリセット
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    public void ResetFilterColor()
    {
        filterColor = defaultFilterColor;
    }



    //------------------------------------------------------------------------------------------
    // summary : 無視するオブジェクトを追加
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    public void AddIgnoreObject(GameObject obj)
    {
        ignoreGameObjects = ignoreGameObjects.Concat(new GameObject[] { obj }).ToArray();
    }
}
