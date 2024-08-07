using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using static RequestArgHandler;

public class Server
{
    Thread reqRcvrThread; 
    NetMQPoller poller;
    public ServicesBase services;
    public ConcurrentQueue<Action> taskQueue;
    public string host;
    public int port;
    string SUCCESS = "SUCCESS";
    string ERROR = "ERROR";

    public Server(ServicesBase services, string host = "tcp://127.0.0.1",  int port = 5555)
    {

        this.host = host;
        this.port = port;
        this.taskQueue = new ConcurrentQueue<Action>();
        this.services = services;

        Console.Write($"Server {host}:{port} is initialized.\n");
    }


    
    bool ValidateAndDeserializeRequest(string req_string, out ServiceRequest validRequest, ConcurrentQueue<ServiceResponse> repQueue)
    {
        validRequest = new();

        try
        {
            validRequest = JsonConvert.DeserializeObject<ServiceRequest>(req_string);
            return true;
        }
        catch (Exception e)
        {
            ServiceResponse response = new ServiceResponse(ERROR, e.Message);
            repQueue.Enqueue(response);

            return false;
        }
    }


    void EnqueueTask(ServiceRequest request, ConcurrentQueue<Action> taskQueue, ConcurrentQueue<ServiceResponse> responseQueue)
    {
        taskQueue.Enqueue(() =>
        {
            ServiceResponse response;      

            try
            {
                MethodInfo serviceInfo = services.GetServiceInfo(request.serviceName);

                // List<object> validArgs = ValidateAndConvertReqArgs(serviceInfo, request.serviceArgs.Values.ToList());
                List<object> validArgs = ValidateAndConvertReqArgs(serviceInfo, request.serviceArgs);
                string serviceOutput = services.InvokeService(request.serviceName, validArgs);
                response = new ServiceResponse(SUCCESS, serviceOutput);
            }
            catch (Exception e)
            {
                response = new ServiceResponse(ERROR, e.Message);
            }

            responseQueue.Enqueue(response);
            
        });
                
    }


    void StartRcvServiceRequests(string host, int port, NetMQPoller poller)
    {   
        ConcurrentQueue<ServiceResponse> responseQueue = new ConcurrentQueue<ServiceResponse>();

        using (var repSocket = new ResponseSocket($"{host}:{port}"))
        {

            repSocket.ReceiveReady += (s, e) =>
            {
                
                string msg = e.Socket.ReceiveFrameString();
                
                ServiceRequest validRequest;
                bool isValidRequest = ValidateAndDeserializeRequest(msg, out validRequest, responseQueue);
                
                if (isValidRequest)
                {
                    EnqueueTask(validRequest, taskQueue, responseQueue);
                }
                
                //Wait until the task is done
                while (responseQueue.IsEmpty){}
                
                responseQueue.TryDequeue(out ServiceResponse response);
                e.Socket.SendFrame(JsonConvert.SerializeObject(response));
            };

            poller.Add(repSocket);
            poller.Run();
        }
    }


    public void Start()
    {
        poller = new();
        reqRcvrThread = new Thread(() => StartRcvServiceRequests(this.host, this.port, poller));
        reqRcvrThread.Start();
        
        Console.Write($"Starting server {host}:{port}.\n");
    }


    public void RunTasks()
    {
        if (!this.taskQueue.IsEmpty)
        {
            this.taskQueue.TryDequeue(out Action task);
            task();
        }
    }


    public void Stop()
    {
        poller.Stop();
        reqRcvrThread.Join();
    }
}

