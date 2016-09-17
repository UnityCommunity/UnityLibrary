using UnityEngine;
using System.Collections;
using System.Threading;

// Reference: http://www.dotnetperls.com/threadpool
// Usage: Attach this script to gameobject in scene, press play, hit 1 key to queue new threads, see console for progress

// we pass thread parameters as object
class ThreadInfo
{
    public int threadIndex;
    public Vector3 myVector;
}


public class ThreadPoolTest : MonoBehaviour
{
    int maxThreads = 2; // set your max threads here

    static readonly object _countLock = new object();
    static int _threadCount = 0;
    static bool closingApp = false;

    int clickCounter = 0;

    void Update()
    {
        // press 1 to spawn thread(s)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Pass these values to the thread.
            ThreadInfo threadData = new ThreadInfo();
            threadData.myVector = Random.insideUnitSphere * 10; // get some random vector3 value
            threadData.threadIndex = ++clickCounter;
            print("Queue new thread #"+ threadData.threadIndex);
            ThreadPool.QueueUserWorkItem(new WaitCallback(MyWorkerThread), threadData);
        }
    }

    private void MyWorkerThread(System.Object a)
    {
        // Constrain the number of worker threads, loop here until less than maxthreads are running
        while (!closingApp)
        {
            // Prevent other threads from changing this under us
            lock (_countLock)
            {
                if (_threadCount < maxThreads && !closingApp)
                {
                    // Start processing
                    _threadCount++;
                    break;
                }
            }
            Thread.Sleep(50);
        }

        if (closingApp) return;

        // we are ready to work now, prepare object that contains necessary info for the thread
        ThreadInfo threadInfo = a as ThreadInfo;
        Vector3 myVector = threadInfo.myVector;
        int myIndex = threadInfo.threadIndex;
        print("---From thread #"+ myIndex + " processing myVector " + myVector);

        // for testing we just sleep here (you could do your heavy calculations here)
        Thread.Sleep(5000);

        // add this to your heavy work loop, so the thread quits if scene is closed
        //if (closingApp) return;

        print("---Finished thread #"+ myIndex);

        // decrease thread counter, so other threads can start
        _threadCount--;
    }

    // set bool to close threads on exit
    void OnDestroy()
    {
        closingApp = true;
    }

}
