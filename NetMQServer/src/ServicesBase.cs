using System;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ServicesBase
{
    
    public void CheckClientConnection()
    {
        Console.Write("Connection with client is established.");
    }

    public MethodInfo GetServiceInfo(string name)
    {
        MethodInfo methodInfo = GetType().GetMethod(name);
        if (methodInfo == null)
        {
            throw new ArgumentException($"Service '{name}' does not exist");
        }
        
        return methodInfo;
    }

    public string InvokeService(string name, List<object> args)
    {
        MethodInfo serviceInfo = GetServiceInfo(name);
        if (serviceInfo.ReturnType == typeof(void))
        {
            serviceInfo.Invoke(this, args.ToArray());
            return string.Empty;
        }
        
        // return serviceInfo.Invoke(this, args.ToArray()).ToString();   
        return JsonConvert.SerializeObject(serviceInfo.Invoke(this, args.ToArray()));   
        
        
    }

    
}