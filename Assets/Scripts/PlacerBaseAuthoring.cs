using UnityEngine;
using Unity.Entities;

/// <summary>
/// サブシーンにこれを一つ置く
/// </summary>
public class PlacerBaseAuthoring : MonoBehaviour
{
    /// <summary>
    /// パネルのプレハブを配置
    /// </summary>
    public MeshRenderer PanelPrefab;
}

public class PlacerBaseBaker : Baker<PlacerBaseAuthoring>
{
    public override void Bake(PlacerBaseAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new PlacerBaseData
        {
            PanelPrefab = GetEntity(authoring.PanelPrefab.gameObject, TransformUsageFlags.Renderable),
            IsPlaced = false
        });
    }
}