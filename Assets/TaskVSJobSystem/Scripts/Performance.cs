using System.Threading.Tasks;
using Unity.Mathematics;

public static class Performance
{
    public static void PerformanceIntensiveMethod(int timesToRepeat)
    {
        // Represents a Performance Intensive Method like some pathfinding or really complex calculation.
        float value = 0f;
        for (int i = 0; i < timesToRepeat; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    public static Task PerformanceIntensiveTask(int timesToRepeat)
    {
        return Task.Run(() =>
        {
            // Represents a Performance Intensive Method like some pathfinding or really complex calculation.
            float value = 0f;
            for (int i = 0; i < timesToRepeat; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        });
    }
}