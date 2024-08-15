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
using UnityEngine;
using static RequestArgHandler;

public class Server
{
    public ServicesBase services;
    public ConcurrentQueue<Action> taskQueue;
    public string host;
    public int port;
    Thread reqRcvrThread; 
    private NetMQPoller poller;
    private ResponseSocket repSocket;
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
                response = new ServiceResponse(ERROR, e.InnerException.Message);
            }

            responseQueue.Enqueue(response);
            
        });
                
    }


    void StartRcvServiceRequests(string host, int port)
    {   
        AsyncIO.ForceDotNet.Force();
        ConcurrentQueue<ServiceResponse> responseQueue = new ConcurrentQueue<ServiceResponse>();
        
        using (repSocket = new ResponseSocket($"{host}:{port}"))
        using (poller = new NetMQPoller {repSocket})
        {

            this.poller.Add(repSocket);

            repSocket.ReceiveReady += (s, e) =>
            {
                {
                    string msg = e.Socket.ReceiveFrameString();
                    
                    ServiceRequest validRequest;
                    bool isValidRequest = ValidateAndDeserializeRequest(msg, out validRequest, responseQueue);
                    
                    if (isValidRequest)
                    {
                        EnqueueTask(validRequest, taskQueue, responseQueue);
                    }
                    
                    //Wait until the task is done
                    while (responseQueue.IsEmpty)
                    {
                        Debug.Log($"Wating for new tasks on server {this.host}:{this.port}");
                    }
                    
                    responseQueue.TryDequeue(out ServiceResponse response);
                    e.Socket.SendFrame(JsonConvert.SerializeObject(response));
                }
            };
            
            this.poller.Run();
        }
    }


    public void Start()
    {
        reqRcvrThread = new Thread(() => StartRcvServiceRequests(this.host, this.port));
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

