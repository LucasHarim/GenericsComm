import json
from dataclasses import dataclass
from functools import wraps
import zmq

class RequestStatus:

    INVALID_REQUEST = 'INVALID REQUEST'
    SUCCESS = 'SUCCESS'


@dataclass
class ServiceRequest:
    serviceName: str
    serviceArgs: dict

    def dumps(self) -> str:

        return json.dumps({'serviceName': self.serviceName, 'serviceArgs': self.serviceArgs})

@dataclass
class ServiceResponse:
    requestStatus: str
    serviceOutput: str


#TODO: implement a service_request that takes custom deserialization_functions as argument

deserialization_functions = {
    int: lambda val_str: int(val_str),
    float: lambda val_str: float(val_str),
    str: lambda val_str: val_str,
    list: lambda val_str: json.loads(val_str),
    dict: lambda val_str: json.loads(val_str),
    None: lambda val_str: None
    }

def deserialize_service_output(val: str, to_type: type) -> type:

    deserializer: callable = deserialization_functions.get(to_type)
    return deserializer(val)


def service_request(function: callable) -> callable:
    
    @wraps(function)
    def wrapper(*args, **kwargs) -> dict:
        
        function(*args, **kwargs)
        
        service_args = {
            **{arg: val for arg, val in zip(function.__code__.co_varnames[1:], args[1:])},
            **kwargs}
        
        
        req_socket = args[0].socket
        req_socket.send_string(ServiceRequest(function.__name__, service_args).dumps())

        response = ServiceResponse(**json.loads(req_socket.recv_string()))

        if response.requestStatus != RequestStatus.SUCCESS: 
            raise Exception(f'Invalid request to service {function.__name__}. {response.serviceOutput}')
        
        return deserialize_service_output(response.serviceOutput, function.__annotations__['return'])
        

    return wrapper

