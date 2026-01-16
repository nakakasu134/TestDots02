using Unity.Entities;
using Unity.Mathematics;


public struct ColoringData : IComponentData
{
    public float distanceMax; // mouseDistanceMaxをここに格納
    public float4 nearColor; // nearColorをここに格納
    public float4 farColor; // farColorをここに格納
}