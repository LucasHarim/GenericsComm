from functools import wraps


@dataclass
class Command(Event):
    cmd_id: str
    args: dict

def add_event(function) -> callable:
    
    @wraps(function)
    def wrapper(*args, **kwargs):
        
        result = function(*args, **kwargs)
        
        args_vals = {
            **{arg: val for arg, val in zip(function.__code__.co_varnames[1:], args[1:])},
            **kwargs}
        
        _self = args[0]
        _self.events.append(Command(function.__name__, args_vals))

        return result

    return wrapper