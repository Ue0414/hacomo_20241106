using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using UnityEditor;


public class firstScene : MonoBehaviour
{

    [HideInInspector]
    public ARTrackedImageManager arTrackedImageManager;

    // マーカー用オブジェクトのプレハブ
   [HideInInspector, SerializeField]
   public List<NameToPrefab> markerNameToPrefab = new List<NameToPrefab>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

   // TrackedImagesChanged時の処理
   void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
   {
       // 追加された画像
       foreach (var trackedImage in eventArgs.added)
       {
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
           }
           // シーン切り替え
           SceneManager.LoadScene("MainScene");

       }
       // 更新された画像
       foreach (var trackedImage in eventArgs.updated)
       {
          
           if (trackedImage.trackingState == TrackingState.Tracking && IsImageVisible(trackedImage))
           {
              
               trackedImage.gameObject.SetActive(true);


           }
           else
           {
               trackedImage.gameObject.SetActive(false);
           }


       }


       // 削除された画像
       foreach (var trackedImage in eventArgs.removed)
       {
           trackedImage.gameObject.SetActive(false);
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
}


// カスタムエディタ
#if UNITY_EDITOR
[CustomEditor(typeof(firstScene))]
public class firstSceneEditor : Editor
{
   bool showNameToPrefabMappings = true;


   public override void OnInspectorGUI()
   {
       DrawDefaultInspector();


       var manager = (firstScene)target;
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

