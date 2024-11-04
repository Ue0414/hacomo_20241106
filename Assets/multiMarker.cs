using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

using UnityEditor;


[Serializable]
public class NameToPrefab
{
    public string name;
    public GameObject prefab;
}

public class multiMarker : MonoBehaviour
{

    [HideInInspector]
    public ARTrackedImageManager arTrackedImageManager;

    // マーカー用オブジェクトのプレハブ
    [HideInInspector, SerializeField]
    public List<NameToPrefab> markerNameToPrefab = new List<NameToPrefab>();

    // マーカーの検知開始時間を保持する辞書
    private Dictionary<string, float> trackingStartTime = new Dictionary<string, float>();

    // チュートリアルメッセージ
    [SerializeField]
    Text firstMessage;

    // スタートメッセージ
    [SerializeField]
    Text startMessage;

    // チュートリアル画像
    [SerializeField]
    Image tutorial;

    // 正解したオブジェクトカウント用リスト
    [HideInInspector]
    public List<string> countList = new List<string>();

    // 判定用オブジェクトカウント用リスト
    [HideInInspector]
    public List<string> checkList = new List<string>();

    //ゲームの状態
    public enum PlayState
    {
        Ready,
        Play,
        Finish
    }

    // 現在のステータス
    public PlayState CurrentState = PlayState.Ready;

    // 結果メッセージ
    public Text resultMessage;

    RectTransform RectTransform_get;
    /*
        // 現在のアクティブマーカー
        private string activeMarkerName = null;
    */
    void OnEnable()
    {
        // トラッキングされた画像が変更された際のイベントを登録
        arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }


    void OnDisable()
    {
        // トラッキングされた画像が変更された際のイベントを解除
        arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // Update
    void Update()
    {
        // ゲームを終了した時の処理
        if (CurrentState == PlayState.Finish)
        {
            // ゲーム終了時のメソッドを呼び出し
            StartCoroutine(FinishGame());
        }
    }

    // TrackedImagesChanged時の処理
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {

        // 追加された画像
        foreach (var trackedImage in eventArgs.added)
        {
            /*
                        if (activeMarkerName != null)
                            break; // 他のマーカーがアクティブな場合は処理しない
            */
            // マーカー名を取得
            var name = trackedImage.referenceImage.name;

            // マーカー名とプレハブのマッピングからプレハブを取得
            var prefab = markerNameToPrefab.Find(x => x.name == name)?.prefab;
            if (prefab != null)
            {
                // ARTrackedImageのTransformの位置を少し上に調整
                var pos = trackedImage.transform.position;
                pos.y += 0.02f;
                // 180度回転し調整
                var rote = trackedImage.transform.eulerAngles;
                rote.y += 180;


                var instance = Instantiate(
                    prefab,
                    pos,
                    Quaternion.Euler(rote),
                    trackedImage.transform
                );

                trackedImage.gameObject.SetActive(false);

                // VideoPlayerを再開
                var videoPlayer = instance.GetComponent<VideoPlayer>();

                trackingStartTime[name] = Time.time; // 検知開始時間を記録

                /*
                videoPlayer.Pause();
*/
                if (CurrentState == PlayState.Ready)
                {
                    if (name == "1")
                    {
                        /*
                        videoPlayer.prepareCompleted += OnVideoPrepared;
                        videoPlayer.Prepare();  // 動画の準備を開始
                        */
                        trackedImage.gameObject.SetActive(true);
                        /*
                        activeMarkerName = name; // アクティブなマーカーを設定
*/
                        if (videoPlayer != null && !videoPlayer.isPlaying)
                        {
                            videoPlayer.Play();
                        }

                        // リストに重複がなければ正解したオブジェクトカウント用リストに追加
                        if (!countList.Contains(name))
                        {
                            countList.Add("1");
                        }

                        // チュートリアルメッセージを非表示
                        firstMessage.enabled = false;

                        tutorial.enabled = false;

                        // スタートメッセージ表示
                        StartCoroutine(StartMessage());

                        CurrentState = PlayState.Play;

                    }
                }
                else if (CurrentState == PlayState.Play)
                {
                    trackedImage.gameObject.SetActive(true);
                    /*
                    activeMarkerName = name; // アクティブなマーカーを設定
*/
                    // VideoPlayerを再開
                    if (videoPlayer != null && !videoPlayer.isPlaying)
                    {
                        videoPlayer.Play();
                    }

                    // リストに重複がなければ正解したオブジェクトカウント用リストに追加
                    if (!countList.Contains(name))
                    {
                        countList.Add(name);
                    }
                }
            }
        }


        // 更新された画像
        foreach (var trackedImage in eventArgs.updated)
        {
            /*
                        if (trackedImage.referenceImage.name != activeMarkerName)
                            continue; // アクティブなマーカー以外は無視
            */
            var videoPlayer = trackedImage.gameObject.GetComponent<VideoPlayer>();

            if (trackedImage.trackingState == TrackingState.Tracking && IsImageVisible(trackedImage))
            {

                if (!trackingStartTime.ContainsKey(name))
                {
                    trackingStartTime[name] = Time.time; // 検知開始時間を再記録
                }

                // 2秒以上トラッキングしているか確認
                if (Time.time - trackingStartTime[name] >= 2f)
                {
                    // マーカー名を取得
                    var name = trackedImage.referenceImage.name;

                    if (CurrentState == PlayState.Ready)
                    {

                        if (name == "1")
                        {
                            trackedImage.gameObject.SetActive(true);
                            /*
                            activeMarkerName = name; // アクティブなマーカーを設定
                            */
                            if (videoPlayer != null && !videoPlayer.isPlaying)
                            {
                                videoPlayer.Play();
                            }
                        }
                        else
                        {
                            trackedImage.gameObject.SetActive(false);
                            videoPlayer.Pause();
                        }
                    }
                    else if (CurrentState == PlayState.Play)
                    {
                        trackedImage.gameObject.SetActive(true);
                        if (videoPlayer != null && !videoPlayer.isPlaying)
                        {
                            videoPlayer.Play();
                        }

                        // リストに重複がなければ正解したオブジェクトカウント用リストに追加
                        if (!countList.Contains(name))
                        {
                            countList.Add(name);
                        }

                    }
                }

            }
            else
            {
                trackedImage.gameObject.SetActive(false);
                /*
                activeMarkerName = null; // アクティブなマーカーを設定
                */
                videoPlayer.Pause();
                trackingStartTime.Remove(name); // 検知開始時間をリセット
            }


        }


        // 削除された画像
        foreach (var trackedImage in eventArgs.removed)
        {
            // マーカー名を取得
            var videoPlayer = trackedImage.gameObject.GetComponent<VideoPlayer>();
            trackedImage.gameObject.SetActive(false);
            videoPlayer.Pause();

        }
    }



    bool IsImageVisible(ARTrackedImage trackedImage)
    {
        // カメラの視錘台にオブジェクトがあるかどうかを判断する
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("Main camera not found!");
            return false;
        }


        // オブジェクトの位置をスクリーン座標に変換
        Vector3 screenPoint = camera.WorldToViewportPoint(trackedImage.transform.position);


        // オブジェクトが画面内にあるかどうかを判断
        return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
    }


    // リファレンスライブラリに名前が存在するかチェック
    bool HasNameInReferenceLibrary(IReferenceImageLibrary library, string name)
    {
        for (int i = 0; i < library.count; i++)
        {
            if (library[i].name == name)
            {
                return true;
            }
        }
        return false;
    }


    // マーカー名とプレハブのマッピングを更新
    public void UpdateNameToPrefabMappings()
    {
        if (arTrackedImageManager == null || arTrackedImageManager.referenceLibrary == null)
        {
            return;
        }


        // マーカー名とプレハブのマッピングから存在しないマーカー名を削除
        foreach (var pair in markerNameToPrefab)
        {
            if (!HasNameInReferenceLibrary(arTrackedImageManager.referenceLibrary, pair.name))
            {
                markerNameToPrefab.Remove(pair);
            }
        }
        // ReferenceImageLibraryに登録されているすべてのマーカー名に対して、
        // マーカー名とプレハブのマッピングにマーカー名が登録されていない場合は名前を登録する
        for (int i = 0; i < arTrackedImageManager.referenceLibrary.count; i++)
        {
            var name = arTrackedImageManager.referenceLibrary[i].name;
            if (!markerNameToPrefab.Exists(x => x.name == name))
            {
                markerNameToPrefab.Add(new NameToPrefab { name = name });
            }
        }
    }


    // カメラから外れた時の処理
    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }

    // カウントダウンを呼び出すメソッド
    public void StartCountdown()
    {
        TimeCounter timeCounter = GetComponent<TimeCounter>();
        if (!timeCounter.isCountingDown) // カウントダウン中でない場合のみ呼び出し
        {
            StartCoroutine(timeCounter.timeCount(100)); // カウントダウンを10から開始
        }
    }
    /*
        // 動画の準備が完了したときに呼び出されるメソッド
        void OnVideoPrepared(VideoPlayer vp)
        {
            videoObject.SetActive(true);   // 動画が準備できたら表示
            vp.Play();                     // 動画の再生を開始
        }
    */
    // スタートの文字を出して、消す
    public IEnumerator StartMessage()
    {
        // スタートメッセージ表示
        startMessage.text = "スタート！";
        // 3秒間待つ
        yield return new WaitForSeconds(3);
        // メッセージ非表示
        startMessage.enabled = false;

        // カウントダウンスタート
        StartCountdown();
    }

    // ゲーム終了時の処理
    public IEnumerator FinishGame()
    {
        TimeCounter timeCounter = GetComponent<TimeCounter>();
        // 3秒間待つ
        yield return new WaitForSeconds(3);
        // メッセージを消す
        timeCounter.timeUpText.enabled = false;

        RectTransform_get = resultMessage.GetComponent<RectTransform>();

        //正解数に応じてメッセージ表示
        if (countList.Count == 8)
        {
            resultMessage.text = "全問正解！";
        }
        else if (countList.Count >= 5 && countList.Count < 8)
        {
            resultMessage.text = "あともう少しだね！";
        }
        else
        {
            resultMessage.text = "またチャレンジしてね！";
        }

        // 3秒間待つ
        yield return new WaitForSeconds(3);
        // メッセージを消す
        resultMessage.enabled = false;

        // 1秒間待つ
        yield return new WaitForSeconds(1);

        // シーンを再ロード
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}


// カスタムエディタ
#if UNITY_EDITOR
[CustomEditor(typeof(multiMarker))]
public class multiMarkerEditor : Editor
{
   bool showNameToPrefabMappings = true;


   public override void OnInspectorGUI()
   {
       DrawDefaultInspector();


       var manager = (multiMarker)target;
       ARTrackedImageManager newManager = (ARTrackedImageManager)
               EditorGUILayout.ObjectField(
                   "AR Tracked Image Manager",
                   manager.arTrackedImageManager,
                   typeof(ARTrackedImageManager),
                   true
               );
       if (newManager != manager.arTrackedImageManager)
       {
           manager.arTrackedImageManager = newManager;
       }
       if (manager.arTrackedImageManager == null)
       {
           EditorGUILayout.HelpBox(
               "Tracked Image Manager is required.",
               MessageType.Error
           );
       }
       else
       {
           manager.UpdateNameToPrefabMappings();
           if (manager.markerNameToPrefab.Count == 0)
           {
               EditorGUILayout.HelpBox(
                   "There are no reference Image in the Reference Image Library.",
                   MessageType.Warning
               );
           }
           else
           {
               showNameToPrefabMappings = EditorGUILayout.Foldout(
                   showNameToPrefabMappings,
                   new GUIContent("Marker To Prefab", "The mapping from marker name to prefab."),
                   true
               );
               if (showNameToPrefabMappings)
               {
                   foreach (var pair in manager.markerNameToPrefab)
                   {
                       EditorGUILayout.BeginHorizontal();
                       EditorGUILayout.Space();
                       EditorGUILayout.LabelField(pair.name);
                       var newPrefab = (GameObject)
                           EditorGUILayout.ObjectField(pair.prefab, typeof(GameObject), true);
                       if (newPrefab != pair.prefab)
                       {
                           pair.prefab = newPrefab;
                           EditorUtility.SetDirty(manager);
                       }
                       EditorGUILayout.EndHorizontal();
                   }
               }
           }
       }
   }
}
#endif




