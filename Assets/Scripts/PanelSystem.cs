using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

/* パネルの挙動を定義 */
/* マウスとの距離に応じて色を変える */

[BurstCompile]
public partial struct PanelSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // マウス位置のシングルトンを取得
        if (!SystemAPI.TryGetSingleton<MouseInputData>(out var mouseInput)) return;
        if (!SystemAPI.TryGetSingleton<ColoringData>(out var coloringData)) return;

        float3 mousePos = mouseInput.Position;

        // Jobの実行
        new PanelJob
        {
            MousePos = mousePos, //マウス位置
            distanceMax = coloringData.distanceMax, //マウス距離の最大値
            nearColor = coloringData.nearColor,
            farColor = coloringData.farColor
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct PanelJob : IJobEntity
{
    public float3 MousePos;
    public float distanceMax;
    public float4 nearColor;
    public float4 farColor;

    void Execute(ref ShaderColor shaderColor, in LocalTransform localTransform)
    {
        float3 panelPos = localTransform.Position;
        float distanceRaito = math.distance(panelPos, MousePos) / distanceMax;
        float colorValue = math.clamp(1f - distanceRaito, 0f, 1f);
        shaderColor.Color = math.lerp(farColor, nearColor, colorValue);
    }
}