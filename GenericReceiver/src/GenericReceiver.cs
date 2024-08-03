using System.Threading;
using System.Collections.Concurrent;

namespace GenericsComm
{    

    public class Receiver
    {    
        
        public delegate U ReceiveData<U>();
        public static void PutDataOnQueue<U>(ConcurrentQueue<U> queue, ReceiveData<U> rcvData, int threadTimeSleep = 1)
        {
            while (true)
            {
                queue.Enqueue(rcvData());
            } 
        }

        public static T GetDataFromQueue<T>(ConcurrentQueue<T> queue, T outData)
        {
            while (!queue.IsEmpty) queue.TryDequeue(out outData);
            return outData;
        }

        public static Thread MakeDataReceiverThread<T>(ConcurrentQueue<T> queue, ReceiveData<T> rcvData, int threadTimeSleep = 1)
        {
            Thread thread = new Thread(() => PutDataOnQueue(queue, rcvData, threadTimeSleep));
            return thread;
        }

        public static void AbortThread(Thread thread)
        {
            if (thread.IsAlive)
            {
                try
                {
                    thread.Abort();
                    thread.Join();
                }
                catch (ThreadAbortException ex)
                {

                }
                
            }
        } 

        
    }
}