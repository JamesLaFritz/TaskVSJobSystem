// PerformanceIntensiveJob.cs
// 06-08-2022
// James LaFritz

using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PerformanceIntensiveJob : IJob
{
    #region Implementation of IJob

    /// <inheritdoc />
    public void Execute()
    {
        // Represents a Performance Intensive Method like some pathfinding or really complex calculation.
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    #endregion
}