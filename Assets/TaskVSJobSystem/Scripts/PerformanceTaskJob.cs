using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PerformanceTaskJob : MonoBehaviour
{
    private enum MethodType
    {
        NormalMoveEnemy,
        TaskMoveEnemy,
        MoveEnemyJob,
        MoveEnemyParallelJob
    }

    private enum MoveEnemyJobType
    {
        ImmediateMainThread,
        ScheduleSingleWorkerThread,
        ScheduleParallelWorkerThreads
    }

    [SerializeField] private int numberEnemiesToCreate = 1000;
    [SerializeField] private Transform pfEnemy;

    [SerializeField] private MethodType method = MethodType.NormalMoveEnemy;
    [SerializeField] private MoveEnemyJobType moveEnemyJobType = MoveEnemyJobType.ImmediateMainThread;

    [SerializeField] private float methodTime;

    private readonly List<Enemy> m_enemies = new List<Enemy>();

    private void Start()
    {
        for (int i = 0; i < numberEnemiesToCreate; i++)
        {
            Transform enemy = Instantiate(pfEnemy,
                                          new Vector3(Random.Range(-8f, 8f), Random.Range(-8f, 8f)),
                                          Quaternion.identity);
            m_enemies.Add(new Enemy { transform = enemy, moveY = Random.Range(1f, 2f) });
        }
    }

    private async void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        switch (method)
        {
            case MethodType.NormalMoveEnemy:
                MoveEnemy();
                break;
            case MethodType.TaskMoveEnemy:
                Task<Task[]> moveEnemyTasks = MoveEnemyTask();
                await Task.WhenAll(moveEnemyTasks);
                moveEnemyTasks.Dispose();
                break;
            case MethodType.MoveEnemyJob:
            case MethodType.MoveEnemyParallelJob:
                MoveEnemyJob(Time.deltaTime);
                break;
            default:
                MoveEnemy();
                break;
        }

        methodTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        Debug.Log($"{methodTime} ms");
    }

    private void MoveEnemy()
    {
        foreach (Enemy enemy in m_enemies)
        {
            enemy.transform.position += new Vector3(0, enemy.moveY * Time.deltaTime);
            if (enemy.transform.position.y > 5f)
                enemy.moveY = -math.abs(enemy.moveY);
            if (enemy.transform.position.y < -5f)
                enemy.moveY = +math.abs(enemy.moveY);
            Performance.PerformanceIntensiveMethod(1000);
        }
    }

    private async Task<Task[]> MoveEnemyTask()
    {
        Task[] tasks = new Task[m_enemies.Count];
        for (int i = 0; i < m_enemies.Count; i++)
        {
            Enemy enemy = m_enemies[i];
            enemy.transform.position += new Vector3(0, enemy.moveY * Time.deltaTime);
            if (enemy.transform.position.y > 5f)
                enemy.moveY = -math.abs(enemy.moveY);
            if (enemy.transform.position.y < -5f)
                enemy.moveY = +math.abs(enemy.moveY);
            tasks[i] = Performance.PerformanceIntensiveTask(1000);
        }

        await Task.WhenAll(tasks);

        return tasks;
    }

    private void MoveEnemyJob(float deltaTime)
    {
        NativeArray<float3> positions = new NativeArray<float3>(m_enemies.Count, Allocator.TempJob);
        NativeArray<float> moveYs = new NativeArray<float>(m_enemies.Count, Allocator.TempJob);

        for (int i = 0; i < m_enemies.Count; i++)
        {
            positions[i] = m_enemies[i].transform.position;
            moveYs[i] = m_enemies[i].moveY;
        }

        if (method == MethodType.MoveEnemyJob)
        {
            MoveEnemyJob job = new MoveEnemyJob
            {
                deltaTime = deltaTime,
                positions = positions,
                moveYs = moveYs
            };

            switch (moveEnemyJobType)
            {
                case MoveEnemyJobType.ImmediateMainThread:
                    // Schedule job to run immediately on main thread. First parameter is how many for-each iterations to perform.
                    job.Run(m_enemies.Count);
                    break;
                case MoveEnemyJobType.ScheduleSingleWorkerThread:
                case MoveEnemyJobType.ScheduleParallelWorkerThreads:
                {
                    // Schedule job to run at a later point on a single worker thread.
                    // First parameter is how many for-each iterations to perform.
                    // The second parameter is a JobHandle to use for this job's dependencies.
                    //   Dependencies are used to ensure that a job executes on worker threads after the dependency has completed execution.
                    //   In this case we don't need our job to depend on anything so we can use a default one.
                    JobHandle scheduleJobDependency = new JobHandle();
                    JobHandle scheduleJobHandle = job.Schedule(m_enemies.Count, scheduleJobDependency);

                    switch (moveEnemyJobType)
                    {
                        case MoveEnemyJobType.ScheduleSingleWorkerThread:
                            scheduleJobHandle.Complete();
                            break;
                        case MoveEnemyJobType.ScheduleParallelWorkerThreads:
                        {
                            // Schedule job to run on parallel worker threads.
                            // First parameter is how many for-each iterations to perform.
                            // The second parameter is the batch size,
                            //   essentially the no-overhead inner-loop that just invokes Execute(i) in a loop.
                            //   When there is a lot of work in each iteration then a value of 1 can be sensible.
                            //   When there is very little work values of 32 or 64 can make sense.
                            // The third parameter is a JobHandle to use for this job's dependencies.
                            //   Dependencies are used to ensure that a job executes on worker threads after the dependency has completed execution.
                            JobHandle scheduleParallelJobHandle =
                                job.ScheduleParallel(m_enemies.Count, m_enemies.Count / 10, scheduleJobHandle);

                            // Ensure the job has completed.
                            // It is not recommended to Complete a job immediately,
                            // since that reduces the chance of having other jobs run in parallel with this one.
                            // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
                            scheduleParallelJobHandle.Complete();
                            break;
                        }
                    }

                    break;
                }
            }
        }
        else if (method == MethodType.MoveEnemyParallelJob)
        {
            MoveEnemyParallelJob job = new MoveEnemyParallelJob
            {
                deltaTime = deltaTime,
                positions = positions,
                moveYs = moveYs
            };

            // Schedule a parallel-for job. First parameter is how many for-each iterations to perform.
            // The second parameter is the batch size,
            // essentially the no-overhead inner-loop that just invokes Execute(i) in a loop.
            // When there is a lot of work in each iteration then a value of 1 can be sensible.
            // When there is very little work values of 32 or 64 can make sense.
            JobHandle jobHandle = job.Schedule(m_enemies.Count, m_enemies.Count / 10);

            // Ensure the job has completed.
            // It is not recommended to Complete a job immediately,
            // since that reduces the chance of having other jobs run in parallel with this one.
            // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
            jobHandle.Complete();
        }

        for (int i = 0; i < m_enemies.Count; i++)
        {
            m_enemies[i].transform.position = positions[i];
            m_enemies[i].moveY = moveYs[i];
        }

        // Native arrays must be disposed manually.
        positions.Dispose();
        moveYs.Dispose();
    }
}
