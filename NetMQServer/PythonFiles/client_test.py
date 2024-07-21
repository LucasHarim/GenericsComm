import zmq
import time
import json
from random import randint

class RequestStatus:

    INVALID_REQUEST = 'INVALID REQUEST'
    SUCCESS = 'SUCCESS'

class Client:

    def __init__(self, host: str = 'tcp://localhost', port:int = 5555):
            
        self._context = zmq.Context()
        self.socket = self._context.socket(zmq.REQ)
        self.socket.connect(f'{host}:{port}')

    def rcv_str(self) -> str:
        return self.socket.recv_string()

    
    def send_str(self, msg: str) -> None:
        self.socket.send_string(msg)



if __name__ == '__main__':

    context = zmq.Context()
    client = context.socket(zmq.REQ)
    client.connect('tcp://127.0.0.1:5555')
    
    requests = [
        {
            'serviceName': 'SayHello',
            'serviceArgs': {}
        },
        
        {
            'serviceName': 'CheckMsg',
            'serviceArgs': {'msg': 'Hi from client!'}
        },

        {
            'serviceName': 'Sum',
            'serviceArgs': {'a': randint(-10, 10), 'b': randint(-10,10)}
        },

        {
            'serviceName': 'Divide',
            'serviceArgs': {'a': time.time(), 'b': randint(-10,10)}
        },

        {
            'serviceName': 'Divide',
            'serviceArgs': {'a': 10, 'b': 1}
        },

        {
            'serviceName': 'Divide',
            'serviceArgs': {'b': 1, 'a': 10}
        }
    ]
    
    
    while True:
        
        
        for req in requests:
        
            client.send_string(json.dumps(req))
            
            res = json.loads(client.recv_string())
            
            if res.get('requestStatus') != RequestStatus.SUCCESS:
                
                _msg = res.get('serviceOutput')
                _service_name = req.get('serviceName')
                raise Exception(f'Invalid request to service \'{_service_name}\'. {_msg}')
            else:
                
                print(f'[{time.time_ns()}] Got answer:\n{res}')
            
            time.sleep(0.1)