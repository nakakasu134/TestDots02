using Unity.Entities;

/// <summary>
/// サブシーンに配置するパネルを置くエンティティの定義
/// </summary>
public struct PlacerBaseData : IComponentData
{
    public Entity PanelPrefab; // エンティティ化したプレハブ
    public bool IsPlaced; // 配置済みかどうか
}