using System.Threading;
using System.Collections.Concurrent;

namespace GenericsComm
{    

    public class Receiver
    {    
        
        public delegate U ReceiveData<U>();
        static void PutDataOnQueue<U>(ConcurrentQueue<U> queue, ReceiveData<U> rcvData)
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

        public static Thread MakeDataReceiverThread<T>(ConcurrentQueue<T> queue, ReceiveData<T> rcvData)
        {
            Thread thread = new Thread(() => PutDataOnQueue(queue, rcvData));
            return thread;
        }

        
    }
}