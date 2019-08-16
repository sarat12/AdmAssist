using System;
using System.Threading;

namespace AdmAssist.Models
{
    public delegate void MultiThreadedTaskFinishedEventHandler();
    public class MultiThreadedTask
    {
        public event MultiThreadedTaskFinishedEventHandler Finished;

        private readonly byte _threadsCount;
        private int _finishedThreadsCount;

        public MultiThreadedTask(byte threadsCount)
        {
            _threadsCount = threadsCount;
        }

        public void Begin(Action action)
        {
            for (int i = 0; i < _threadsCount; i++)
            {
                new Thread(() => { action(); OneThreadFinnished(); }) { IsBackground = true }.Start();
            }
        }

        private void OneThreadFinnished()
        {
            Interlocked.Increment(ref _finishedThreadsCount);

            if (_finishedThreadsCount == _threadsCount)
            {
                Finished?.Invoke();
            }
        }
    }
}
