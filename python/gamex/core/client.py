from __future__ import annotations
import os
from openstk.core.client import ClientBase
from gamex import ClientState

# GameClient
class GameClient(ClientBase):
    def __init__(self, state: ClientState):
        super().__init__()
        self.archive = state.archive
        self.tag = state.tag