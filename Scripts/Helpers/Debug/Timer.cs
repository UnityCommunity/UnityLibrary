// Timer: get time elapsed in milliseconds
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UnityLibrary
{
    public class Timer : MonoBehaviour
    {
        void Start()
        {
            // init and start timer
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // put your function here..
            for (int i = 0; i < 1000000; i++)
            {
                var tmp = "asdf" + 1.ToString();
            }

            // get results in ms
            stopwatch.Stop();
            Debug.LogFormat("Timer: {0} ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }
    }
}
