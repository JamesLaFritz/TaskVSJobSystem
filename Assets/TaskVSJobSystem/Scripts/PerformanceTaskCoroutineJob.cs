using System.Collections;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PerformanceTaskCoroutineJob : MonoBehaviour
{
    private enum MethodType
    {
        Normal,
        Task,
        Coroutine,
        Job
    }

    [SerializeField] private int numberGameObjectsToImitate = 10;

    [SerializeField] private MethodType method = MethodType.Normal;

    [SerializeField] private float methodTime;

    private Coroutine m_performanceCoroutine;

    private async void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        switch (method)
        {
            case MethodType.Normal:
                for (int i = 0; i < numberGameObjectsToImitate; i++)
                    Performance.PerformanceIntensiveMethod(50000);
                break;
            case MethodType.Task:
                Task[] tasks = new Task[numberGameObjectsToImitate];
                for (int i = 0; i < numberGameObjectsToImitate; i++)
                    tasks[i] = Performance.PerformanceIntensiveTask(5000);
                await Task.WhenAll(tasks);
                foreach (Task task in tasks)
                {
                    task.Dispose();
                }

                break;
            case MethodType.Coroutine:
                m_performanceCoroutine ??= StartCoroutine(PerformanceCoroutine(5000, startTime));
                break;
            case MethodType.Job:
                NativeArray<JobHandle> jobHandles =
                    new NativeArray<JobHandle>(numberGameObjectsToImitate, Allocator.Temp);
                for (int i = 0; i < numberGameObjectsToImitate; i++)
                    jobHandles[i] = PerformanceIntensiveMethodJob();
                JobHandle.CompleteAll(jobHandles);
                jobHandles.Dispose();
                break;
            default:
                Performance.PerformanceIntensiveMethod(50000);
                break;
        }

        if (method != MethodType.Coroutine)
        {
            methodTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            Debug.Log($"{methodTime} ms");
        }

        if (method != MethodType.Coroutine || m_performanceCoroutine == null) return;
        StopCoroutine(m_performanceCoroutine);
        m_performanceCoroutine = null;
    }

    private IEnumerator PerformanceCoroutine(int timesToRepeat, float startTime)
    {
        for (int count = 0; count < numberGameObjectsToImitate; count++)
        {
            // Represents a Performance Intensive Method like some pathfinding or really complex calculation.
            float value = 0f;
            for (int i = 0; i < timesToRepeat; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }

        methodTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        Debug.Log($"{methodTime} ms");
        m_performanceCoroutine = null;
        yield return null;
    }

    private JobHandle PerformanceIntensiveMethodJob()
    {
        PerformanceIntensiveJob job = new PerformanceIntensiveJob();
        return job.Schedule();
    }
}
