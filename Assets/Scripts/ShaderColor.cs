using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

/// <summary>
/// 色を変更できるシェーダーをコンポーネントしたもの
/// Shader Graphなどで色のプロパティを作成し、Base Colorと紐づけることで使用可能に
/// </summary>
[MaterialProperty("_Color")] // プロパティ名を代入する
public struct ShaderColor : IComponentData
{
    public float4 Color;
}