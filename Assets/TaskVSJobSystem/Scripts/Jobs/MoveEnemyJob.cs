// MoveEnemyJob.cs
// 06-08-2022
// James LaFritz

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct MoveEnemyJob : IJobFor
{
    public NativeArray<float3> positions;
    public NativeArray<float> moveYs;
    public float deltaTime;

    #region Implementation of IJobFor

    /// <inheritdoc />
    public void Execute(int index)
    {
        positions[index] += new float3(0, moveYs[index] * deltaTime, 0);
        if (positions[index].y > 5f)
            moveYs[index] = -math.abs(moveYs[index]);
        if (positions[index].y < -5f)
            moveYs[index] = +math.abs(moveYs[index]);

        // Represents a Performance Intensive Method like some pathfinding or really complex calculation.
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    #endregion
}