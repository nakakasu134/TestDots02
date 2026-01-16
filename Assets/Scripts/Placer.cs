using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 「PanelPlacer」で定義したパネルの数と配置をエンティティに記録するためのデータ構造
/// </summary>
public struct PlacerData : IComponentData
{
    public int widthCount; // 横の数
    public int heightCount; // 縦の数
    public float3 firstPanelPosition; // 最初のパネルの位置
    public float3 horizontalOffset; // 横方向のオフセット
    public float3 verticalOffset; // 縦方向のオフセット
    public quaternion panelRotation; // 回転
    public float3 panelScale; // スケール
}
