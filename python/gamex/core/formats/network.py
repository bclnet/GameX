from __future__ import annotations
import os
from openstk.core.debug import LogFile

# PacketLogger
class PacketLogger:
    default: object

    def __init__(self):
        self.enabled = False

    def createFile(self) -> LogFile:
        if self._logFile: self._logFile.close()
        self._logFile = LogFile(os.path.join('logs', 'network'), 'packets.log')
        return self._logFile

    def log(self, message: bytes, toServer: bool) -> None:
        pass
        
PacketLogger.default = PacketLogger()