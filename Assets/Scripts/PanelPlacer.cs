using UnityEngine;
using Unity.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// パネルの数と配置を定義するコンポーネント
/// パネルの仮インスタンスをエディター上で配置し、プレイモード実行時にエンティティとして再生成できるようにする
/// </summary>
public class PanelPlacer : MonoBehaviour
{
    [SerializeField, Tooltip("カメラ")] private Camera targetCamera;
    [SerializeField, Tooltip("エディター上での仮インスタンスの親")] private Transform panelParent;
    [SerializeField, Tooltip("カメラからの距離"), Min(1e-5f)] private float cameraDistance = 10f;
    [Header("パネルの基本設定")]
    [SerializeField, Tooltip("プレハブ")] private MeshRenderer panelPrefab;
    [SerializeField, Tooltip("回転")] private Vector3 panelRotation = Vector3.zero;
    [SerializeField, Tooltip("大きさ"), Min(1e-5f)] private float panelScale = 1f;
    [Header("パネルの数")]
    [SerializeField, Tooltip("横"), Min(1)] private int numberOfWidth = 5;
    [SerializeField, Tooltip("縦"), Min(1)] private int numberOfHeight = 5;
    [SerializeField, Tooltip("仮インスタンスを生成しない")] private bool dontMakeInstances = false;
    [SerializeField, Tooltip("エディター上でパネルを表示するか")] private bool activeInEditor = true;

    [SerializeField, HideInInspector] private Transform[] panelInstances = { }; // エディターで生成したパネルの仮インスタンス
    [SerializeField, HideInInspector] private MeshRenderer prefabCache; // 過去のプレハブの状態

    public Camera GetCamera => targetCamera;
    public float CameraDistance => cameraDistance;

    private Vector3 PanelPosition(int x, int y)
    {

        float differenceX = targetCamera.orthographicSize * 2f * targetCamera.aspect / numberOfWidth;
        float differenceY = targetCamera.orthographicSize * 2f / numberOfHeight;
        return new(
            (x - (numberOfWidth - 1) * 0.5f) * differenceX,
            (y - (numberOfHeight - 1) * 0.5f) * differenceY,
            0);
    }

    private Vector3 PanelScaleVector()
    {
        float differenceX = targetCamera.orthographicSize * 2f * targetCamera.aspect / numberOfWidth;
        float differenceY = targetCamera.orthographicSize * 2f / numberOfHeight;
        return new(
            panelScale * differenceX,
            panelScale,
            panelScale * differenceY
        );
    }

    void Awake()
    {
        // 仮インスタンスを非表示にする
        foreach (Transform instance in panelInstances)
        {
            instance.gameObject.SetActive(false);
        }

        // ECSの世界の「PlacerData」に値を記録する
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var placerQuery = entityManager.CreateEntityQuery(typeof(PlacerData));

        if (!placerQuery.HasSingleton<PlacerData>())
        {
            bool isEmpty = numberOfWidth * numberOfHeight== 0;
            entityManager.CreateSingleton(new PlacerData
            {
                widthCount = numberOfWidth,
                heightCount = numberOfHeight,
                panelScale = isEmpty ? Vector3.one : PanelScaleVector(),
                firstPanelPosition = isEmpty ? Vector3.zero : panelParent.rotation * PanelPosition(0, 0)+panelParent.position,
                horizontalOffset = numberOfWidth > 1 ? panelParent.rotation * (PanelPosition(1, 0) - PanelPosition(0, 0)) : Vector3.right,
                verticalOffset = numberOfHeight > 1 ? panelParent.rotation * (PanelPosition(0, 1) - PanelPosition(0, 0)) : Vector3.up,
                panelRotation = Quaternion.Euler(panelRotation) * panelParent.rotation
            });
        }
    }

    /* ここからはエディター上での操作用 */
#if UNITY_EDITOR

    /// <summary>
    /// パネル群を更新する必要があるか
    /// </summary>
    private bool NeedsCreatePanels
    {
        get
        {
            if (dontMakeInstances) return true;
            if (prefabCache != panelPrefab)
            {
                prefabCache = panelPrefab;
                return true;
            }
            int expectedCount = numberOfWidth * numberOfHeight;
            if (panelInstances.Length != expectedCount
            || panelParent.childCount != expectedCount
            || panelInstances.Length != expectedCount)
            {
                return true;
            }
            for (int i = 0; i < panelInstances.Length; i++)
            {
                if (panelInstances[i] == null)
                {
                    return true;
                }
                if (panelInstances[i].parent != panelParent)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 設定のチェック
    /// </summary>
    void CheckSettings()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        if (targetCamera == null)
        {
            Debug.LogError("targetCamera is null");
            return;
        }
        if (targetCamera.orthographic == false)
        {
            Debug.LogError("targetCamera is not orthographic");
            return;
        }
        if (panelParent == null)
        {
            Debug.LogError("panelParent is null");
            return;
        }
        if (panelPrefab == null)
        {
            Debug.LogError("panelPrefab is null");
            return;
        }
        if (PrefabUtility.GetPrefabAssetType(panelPrefab.gameObject) == PrefabAssetType.NotAPrefab)
        {
            Debug.LogError("panelPrefab is not a prefab");
            return;
        }
        if (panelPrefab.gameObject.activeInHierarchy)
        {
            Debug.LogError("panelPrefab is active in hierarchy, it should be a disabled prefab");
            return;
        }
        panelParent.localPosition = targetCamera.transform.position + targetCamera.transform.forward * cameraDistance;
        panelParent.localRotation = targetCamera.transform.rotation;

        if (NeedsCreatePanels)
        {
            EditorApplication.delayCall += CreatePanels;
        }
        else
        {
            ReplacePanels();
        }
    }

    /// <summary>
    /// 仮インスタンスの生成・削除
    /// </summary>
    void CreatePanels()
    {
        ClearPanels();
        if (dontMakeInstances) return;

        panelInstances = new Transform[numberOfWidth * numberOfHeight];

        for (int x = 0; x < numberOfWidth; x++)
        {
            for (int y = 0; y < numberOfHeight; y++)
            {
                int index = x + y * numberOfWidth;
                GameObject instance = PrefabUtility.InstantiatePrefab(panelPrefab.gameObject, panelParent) as GameObject;
                panelInstances[index] = instance.transform;
                panelInstances[index].parent = panelParent;
            }
        }

        EditorApplication.delayCall -= CreatePanels;
        ReplacePanels();
    }

    void ClearPanels()
    {
        while (panelParent.childCount > 0)
        {
            DestroyImmediate(panelParent.GetChild(0).gameObject);
        }
        panelInstances = new Transform[0];
    }

    /// <summary>
    /// パネルの回転や大きさの変更
    /// </summary>
    void ReplacePanels()
    {
        Vector3 panelScaleVector = PanelScaleVector();

        for (int x = 0; x < numberOfWidth; x++)
        {
            for (int y = 0; y < numberOfHeight; y++)
            {
                int index = x + y * numberOfWidth;
                Transform panel = panelInstances[index];
                panel.gameObject.SetActive(activeInEditor);
                panel.localRotation = Quaternion.Euler(panelRotation);
                panel.localScale = panelScaleVector;
                panel.localPosition = PanelPosition(x, y);
            }
        }
    }

    void OnValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        CheckSettings();
    }
#endif
}
