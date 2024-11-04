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

public class multiMarker_movie : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    // マーカーごとに表示するプレハブを設定するための辞書
    [System.Serializable]
    public class MarkerPrefabMapping
    {
        public string markerName;
        public GameObject prefab;
    }
    [SerializeField] private List<MarkerPrefabMapping> markerPrefabMappings;

    private Dictionary<string, float> markerDetectionStartTime = new Dictionary<string, float>();
    private Dictionary<string, GameObject> markerInstances = new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // 更新された画像の処理
        foreach (var trackedImage in eventArgs.updated)
        {
            string markerName = trackedImage.referenceImage.name;

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // マーカーが初めて検知されたとき、または再度検知されたときに記録
                if (!markerDetectionStartTime.ContainsKey(markerName))
                {
                    markerDetectionStartTime[markerName] = Time.time;
                }
                else
                {
                    // 2秒以上検知されているか確認
                    if (Time.time - markerDetectionStartTime[markerName] >= 2.0f)
                    {
                        // 2秒以上経過していて、まだプレハブが生成されていない場合に生成
                        if (!markerInstances.ContainsKey(markerName))
                        {
                            var prefab = GetPrefabForMarker(markerName);
                            if (prefab != null)
                            {
                                var instance = Instantiate(prefab, trackedImage.transform);
                                markerInstances[markerName] = instance;
                            }
                        }
                    }
                }
            }
            else
            {
                // 検知が失われた場合、タイムスタンプをリセット
                if (markerDetectionStartTime.ContainsKey(markerName))
                {
                    markerDetectionStartTime.Remove(markerName);
                }

                // オブジェクトを削除して非表示にする
                if (markerInstances.ContainsKey(markerName))
                {
                    Destroy(markerInstances[markerName]);
                    markerInstances.Remove(markerName);
                }
            }
        }
    }

    private GameObject GetPrefabForMarker(string markerName)
    {
        foreach (var mapping in markerPrefabMappings)
        {
            if (mapping.markerName == markerName)
            {
                return mapping.prefab;
            }
        }
        return null;
    }
}