import zmq
import time
from random import randint
from service_request import (
    RequestStatus,
    ServiceResponse,
    service_request)

class ClientTest:

    def __init__(self, host: str = 'tcp://localhost', port:int = 5555):
            
        self._context = zmq.Context()
        self.socket = self._context.socket(zmq.REQ)
        self.socket.connect(f'{host}:{port}')
        
    @service_request
    def SayHello(self) -> None: ...
    
    @service_request
    def CheckMsg(self, msg: str) -> str: ...
    
    @service_request
    def Sum(self, a: int, b: int) -> int: ...
    
    @service_request
    def Divide(self, a: float, b: float) -> float: ...
    
    @service_request
    def Add(self, a: int, b: float) -> float: ...




if __name__ == '__main__':

    client = ClientTest()
    
    t0 = time.time()

    for i in range(10000):
        
        client.SayHello()
        
        msg = client.CheckMsg(f'Hello from the ClientTest!')
        print(msg)

        n1 = client.Divide(10, 1) + client.Sum(i, 1.0)
        n2 = client.Sum(randint(-10, 10), randint(-10, 10))
        
        print(n1)
        print(n2)

    delta_time = time.time() - t0
    print(f'Delta time: {round(delta_time, 3)} secs')
        