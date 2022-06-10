using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleTask : MonoBehaviour
{
    private void Start()
    {
        Task t1 = new Task(() => Thread.Sleep(1000));
        Task t2 = Task.Run(() => Thread.Sleep(2000000));
        Task t3 = Task.Factory.StartNew(() => Thread.Sleep(1000));
        t1.Start();
        Task[] tasks = { t1, t2, t3 };
        int index = Task.WaitAny(tasks);
        Debug.Log($"Task {tasks[index].Id} at index {index} completed.");

        Task t4 = new Task(() => Thread.Sleep(100));
        Task t5 = Task.Run(() => Thread.Sleep(200));
        Task t6 = Task.Factory.StartNew(() => Thread.Sleep(300));
        t4.Start();
        Task.WaitAll(t4, t5, t6);
        Debug.Log($"All Task Completed!");
        Debug.Log($"Task When any t1={t1.IsCompleted} t2={t2.IsCompleted} t3={t3.IsCompleted}");
        Debug.Log($"All Task Completed! t4={t4.IsCompleted} t5={t5.IsCompleted} t6={t6.IsCompleted}");
    }

    public async void Update()
    {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"Update Started: {startTime}");
        Task t1 = new Task(() => Thread.Sleep(10000));
        Task t2 = Task.Run(() => Thread.Sleep(20000));
        Task t3 = Task.Factory.StartNew(() => Thread.Sleep(30000));
        t1.Start();

        await Task.WhenAll(t1, t2, t3);
        Debug.Log($"Update Finished: {(Time.realtimeSinceStartup - startTime) * 1000f} ms");
    }
}
