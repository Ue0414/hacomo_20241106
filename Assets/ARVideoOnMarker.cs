using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Video;
using UnityEngine.XR.ARSubsystems;

public class NewBehaviourScript : MonoBehaviour
{
    // ARTrackedImageManager コンポーネントをアサインする
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    // ビデオプレーヤー用のPrefab
    public GameObject videoPrefab;

    // マーカーごとにビデオを管理するためのディクショナリ
    private Dictionary<string, GameObject> spawnedVideos = new Dictionary<string, GameObject>();


    private void OnEnable()
    {
        // 画像がトラッキングされたときにイベントを受け取る
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        // イベントを解除する
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // 画像がトラッキングされた/更新された/失われたときの処理
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // 新しくトラッキングされた画像に対してビデオオブジェクトを生成
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            // マーカー名に基づいてビデオオブジェクトを生成
            SpawnOrUpdateVideo(trackedImage);
        }

        // トラッキング中の画像に対してオブジェクトの位置と向きを更新
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            // ビデオオブジェクトの位置を更新
            SpawnOrUpdateVideo(trackedImage);
        }

        // トラッキングが失われた画像に対してオブジェクトを非表示にする
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            if (spawnedVideos.ContainsKey(trackedImage.referenceImage.name))
            {
                // ビデオオブジェクトを非表示にするか破棄
                spawnedVideos[trackedImage.referenceImage.name].SetActive(false);
            }
        }
    }

    // ビデオオブジェクトを生成または更新する
    private void SpawnOrUpdateVideo(ARTrackedImage trackedImage)
    {
        // マーカーの名前（Reference Imageの名前）を取得
        string imageName = trackedImage.referenceImage.name;

        // ビデオがまだ生成されていない場合、生成する
        if (!spawnedVideos.ContainsKey(imageName))
        {
            GameObject videoObject = Instantiate(videoPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
            spawnedVideos[imageName] = videoObject;

            // ビデオプレイヤーを取得し再生開始
            VideoPlayer videoPlayer = videoObject.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.Play();
            }
        }
        else
        {
            // 既に生成されている場合、位置と回転を更新する
            GameObject videoObject = spawnedVideos[imageName];
            videoObject.transform.position = trackedImage.transform.position;
            videoObject.transform.rotation = trackedImage.transform.rotation;
            videoObject.SetActive(true);
        }
    }
}
