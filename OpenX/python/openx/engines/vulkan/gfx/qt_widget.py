import time
from PyQt6.QtCore import QTimer
from PyQt6.QtWidgets import QWidget
from openx.core import ISource
from openx.gfx import ITextureSelect

# typedefs
class Renderer: pass

#region VulkanWidget

# VulkanWidget. Same bindings as OpenGLWidget. Tick only. The C# present path is not ported (ADR 0010).
class VulkanWidget(QWidget):
    renderer: Renderer = None
    id: int = 0

    # Binding

    def __init__(self, parent: object, tab: object, interval: float = 1.0):
        super().__init__()
        self.source: ISource = parent.source
        self.source2: object = tab
        self.path: object = parent.path
        self.value: object = tab.value
        self.type: str = tab.type
        self.deltaTime: int = 0
        self.elapsedTime: float = time.time()
        if interval:
            self.timer = QTimer(self)
            self.timer.timeout.connect(self.tick)
            self.timer.start()

    def createRenderer(self) -> Renderer: pass

    def onSourceChanged(self) -> None:
        if not self.source or not self.path or not self.value or not self.type: return
        self.renderer = self.createRenderer()
        if self.renderer: self.renderer.start()
        if isinstance(self.value, ITextureSelect): self.value.select(self.id)

    def showEvent(self, event: object) -> None:
        super().showEvent(event)
        self.onSourceChanged()

    # Render

    def tick(self) -> None:
        self.deltaTime = round((time.time() - self.elapsedTime) * 1000)
        self.elapsedTime = time.time()
        if self.renderer: self.renderer.update(self.deltaTime)

#endregion
