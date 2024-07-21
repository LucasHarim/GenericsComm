using System;

class Program
{   

    static bool isServerRunning;
    static void Main(string[] args)
    {
        
        Server server = new Server(services: new ServicesTest());
        server.Start();
        isServerRunning = true;

        while (isServerRunning)
        {
            server.RunTasks();
        }   

        server.Stop();
        
    }

}

