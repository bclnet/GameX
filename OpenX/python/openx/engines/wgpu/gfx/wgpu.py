# WgpuX
class WgpuX:
    buildersByType = {}

# WgpuInstance. createInstance imports wgpu only when called.
class WgpuInstance:
    def __init__(self, adapter):
        self._adapter = adapter
        self._disposed = False

    @property
    def adapter(self): return self._adapter

    @staticmethod
    def createInstance():
        try:
            import wgpu
        except ImportError as e:
            raise ImportError('wgpu is not installed.') from e
        try:
            adapter = wgpu.gpu.request_adapter_sync()
        except Exception as e:
            raise RuntimeError(f'wgpu adapter request failed: {e}') from e
        if adapter is None:
            raise RuntimeError('wgpu found no adapter.')
        return WgpuInstance(adapter)

    def dispose(self) -> None:
        if self._disposed: return
        self._disposed = True
        self._adapter = None

    def __del__(self):
        try: self.dispose()
        except Exception: pass
