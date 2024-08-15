using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

public static class RequestArgHandler
{
    static string GetMethodSignature(MethodInfo methodInfo)
    {
        ParameterInfo[] paramInfo = methodInfo.GetParameters();
        string paramString = "";
        foreach (var param in methodInfo.GetParameters())
        {
            paramString += $"{param.ParameterType.Name} {param.Name},";
        }
        
        paramString = paramString.Remove(paramString.Length - 1); //Removing the last comma
        string signature_str = $"{methodInfo.ReturnType} {methodInfo.Name}({paramString})";
        return signature_str;
    }

    
    public static void ValidateArgNumber(MethodInfo methodInfo, Dictionary<string, object> args)
    {
        if (methodInfo.GetParameters().Length != args.Count)
        {
            throw new ArgumentException("Incorrect number of arguments provided.");
        }
    }

    
    public static List<object> ValidateAndConvertReqArgs(MethodInfo methodInfo, Dictionary<string,object> args)
    {
        ValidateArgNumber(methodInfo, args);

        List<object> validArgs = new();

        foreach (ParameterInfo param in methodInfo.GetParameters())
        {
            try 
            {
                //TODO: Extend the serialization in order to accept string -> Dictionary, etc
                //TODO: allow something like JsonConvert.DeserializeObject<MyType>(arg)
                //*Implement maybe a set of deserialization functions that are called based on the param.ParameterType
                validArgs.Add(Convert.ChangeType(args[param.Name], param.ParameterType)); 
                // validArgs.Add(JsonConvert.DeserializeObject<param.ParameterType>(args[param.Name]));
                
            }
            catch (Exception e)
            {
                
                throw new ArgumentException($"{e.Message} Argument '{param.Name}' is passed as type '{args[param.Name].GetType()}' and does not match method signature: {GetMethodSignature(methodInfo)}");
            }
            
        }

        return validArgs;
    }

}