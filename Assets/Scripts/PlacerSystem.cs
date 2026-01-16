using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

/* パネルを実際にサブシーン内に配置するシステム */

[BurstCompile]
public partial struct PlacerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // パネルの数と配置を取得
        if (!SystemAPI.TryGetSingleton<PlacerData>(out var placer)) return;
        // Initialization code can go here if needed
        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

        // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
        new PlacerJob
        {
            Ecb = ecb,
            placerData = placer
        }.ScheduleParallel();
    }

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}

[BurstCompile]
public partial struct PlacerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public PlacerData placerData;

    // IJobEntity generates a component data query based on the parameters of its `Execute` method.
    // This example queries for all Spawner components and uses `ref` to specify that the operation
    // requires read and write access. Unity processes `Execute` for each entity that matches the
    // component data query.
    private void Execute([ChunkIndexInQuery] int chunkIndex, ref PlacerBaseData placerBaseData)
    {
        if (placerBaseData.IsPlaced)
        {
            return; // 配置済みならスキップ
        }

        for (int x = 0; x < placerData.widthCount; x++)
        {
            for (int y = 0; y < placerData.heightCount; y++)
            {
                // インスタンスの生成
                Entity newEntity = Ecb.Instantiate(chunkIndex, placerBaseData.PanelPrefab);

                // 最初に位置と回転を決める
                Ecb.SetComponent<LocalTransform>(chunkIndex, newEntity, new()
                {
                    Position=placerData.firstPanelPosition
                    +placerData.horizontalOffset*x
                    +placerData.verticalOffset*y,
                    Scale=1f, // ここで値のばらついたスケールの定義はできない
                    Rotation=placerData.panelRotation
                });

                // 非一様スケールはPostTransformMatrixで後から定義
                Ecb.AddComponent<PostTransformMatrix>(chunkIndex, newEntity, new()
                {
                    Value=new(
                        placerData.panelScale.x,0,0,0,
                        0,placerData.panelScale.y,0,0,
                        0,0,placerData.panelScale.z,0,
                        0,0,0,1
                    )
                });
            }
        }

        placerBaseData.IsPlaced = true; // 配置済みフラグをたてる
    }
}