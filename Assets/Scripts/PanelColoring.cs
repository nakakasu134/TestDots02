using Unity.Entities;
using UnityEngine;

/// <summary>
/// マウスの位置をエンティティに記録し、パネルの色の計算法を決めるスクリプト
/// </summary>
public class PanelColoring : MonoBehaviour
{
    [SerializeField,Tooltip("カメラとパネルの距離を取得するために参照")] private PanelPlacer panelPlacer;
    [SerializeField,Tooltip("マウスとの距離の最大値"),Min(1e-5f)] private float mouseDistanceMax = 1f;
    [SerializeField,Tooltip("近くの色")] private Color nearColor = Color.red;
    [SerializeField,Tooltip("遠い色")] private Color farColor = Color.blue;


    private Camera cam;
    private float cameraDistance; // カメラとパネルの距離

    void Awake()
    {
        cam = panelPlacer.GetCamera;
        cameraDistance = panelPlacer.CameraDistance;
    }

    void Update()
    {
        // マウスのワールド座標を取得
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cameraDistance; // カメラからの距離
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        // ECSの世界の「MouseInputData」を更新する
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var mouseQuery = entityManager.CreateEntityQuery(typeof(MouseInputData));
        var coloringQuery = entityManager.CreateEntityQuery(typeof(ColoringData));

        if (mouseQuery.HasSingleton<MouseInputData>())
        {
            var data = mouseQuery.GetSingleton<MouseInputData>();
            data.Position = worldPos;
            mouseQuery.SetSingleton(data);
        }
        else
        {
            // 最初だけEntityを作成
            entityManager.CreateSingleton(new MouseInputData
            {
                Position = worldPos,
            });
        }

        if (!coloringQuery.HasSingleton<ColoringData>())
        {
            entityManager.CreateSingleton(new ColoringData
            {
                distanceMax = mouseDistanceMax,
                nearColor = new(
                    nearColor.r,
                    nearColor.g,
                    nearColor.b,
                    nearColor.a
                ),
                farColor = new(
                    farColor.r,
                    farColor.g,
                    farColor.b,
                    farColor.a
                )
            });
        }
    }
}
