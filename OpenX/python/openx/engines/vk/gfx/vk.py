from __future__ import annotations

# VkX
class VkX:
    buildersByType: dict[type, object] = {}

# VkInstance. createInstance imports a Vulkan binding only when called.
class VkInstance:
    def __init__(self, vk, instance):
        self._vk = vk
        self._instance = instance
        self._disposed = False

    @property
    def handle(self): return self._instance

    @staticmethod
    def _binding():
        try:
            import vulkan as vk
            return vk
        except ImportError:
            pass
        try:
            import pyvulkan as vk
            return vk
        except ImportError as e:
            raise ImportError('Vulkan binding is not installed (vulkan or pyvulkan).') from e

    @staticmethod
    def createInstance(applicationName: str = 'GameX'):
        vk = VkInstance._binding()
        name = applicationName or 'GameX'
        try:
            makeVersion = vk.VK_MAKE_VERSION
            apiVersion = vk.VK_API_VERSION_1_0
            appInfo = vk.VkApplicationInfo(
                sType=vk.VK_STRUCTURE_TYPE_APPLICATION_INFO,
                pApplicationName=name,
                applicationVersion=makeVersion(1, 0, 0),
                pEngineName='Vulkan',
                engineVersion=makeVersion(1, 0, 0),
                apiVersion=apiVersion)
            createInfo = vk.VkInstanceCreateInfo(
                sType=vk.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
                pApplicationInfo=appInfo)
            instance = vk.vkCreateInstance(createInfo, None)
        except ImportError:
            raise
        except Exception as e:
            raise RuntimeError(f'vkCreateInstance failed: {e}') from e
        return VkInstance(vk, instance)

    def dispose(self) -> None:
        if self._disposed or self._instance is None: return
        self._disposed = True
        self._vk.vkDestroyInstance(self._instance, None)
        self._instance = None

    def __del__(self):
        try: self.dispose()
        except Exception: pass
