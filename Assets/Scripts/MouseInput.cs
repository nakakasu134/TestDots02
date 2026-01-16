using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// MonoBehaviourで取得したマウス位置をエンティティに記録するためのデータ構造
/// </summary>
public struct MouseInputData : IComponentData
{
    public float3 Position;
}