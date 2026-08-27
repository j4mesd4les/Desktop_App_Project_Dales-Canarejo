from dataclasses import dataclass


@dataclass
class Equipment:
    id: int
    name: str
    is_available: bool = True
