using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// パネルのプレハブにこのコンポーネントをつける
/// </summary>
public class PanelAuthoring : MonoBehaviour
{
    
}

public class PanelBaker:Baker<PanelAuthoring>
{
    public override void Bake(PanelAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Renderable);
        AddComponent(entity,new ShaderColor()
        {
            Color = new float4(1f,0f,0f,1f)
        });
    }
}