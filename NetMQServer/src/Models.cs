using System.Collections.Generic;

public class ServiceRequest
{
    public string serviceName {get; set;}
    public Dictionary<string, object> serviceArgs {get; set;}
}

public class ServiceResponse
{
    public string requestStatus {get; set;}
    public string serviceOutput {get; set;}
}