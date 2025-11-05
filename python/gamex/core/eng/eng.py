from __future__ import annotations
import os
from datetime import timedelta
from openstk.core.util import Stopwatch

# GraphicsDeviceManager
class GraphicsDeviceManager:
    def beginDraw() -> bool: return True
    def createDevice() -> None: pass
    def endDraw() -> None: pass

# IGameObject
class IGameObject:
    def initialize() -> None: pass

# Game
class Game:
    maxElapsedTime: timedelta = timedelta(milliseconds=500)

    def __init__(self): 
        self.components: list[IGameObject] = []
        self.deviceManager: GraphicsDeviceManager = None
        self.isDisposed: bool = False
        self.initialized: bool = False
        self.running: bool = False
        self.suppressDraw: bool = False
        # tick
        self.totalGameTime: timedelta = timedelta()
        self.elapsedGameTime: timedelta = timedelta()
        self.isRunningSlowly: bool = False
        self.gameTimer: Stopwatch
        self.accumulatedElapsedTime: timedelta = timedelta()
        self.previousTicks: int = 0
        self.forceElapsedTimeToZero: bool = False
        # events
        self.activated = None
        self.deactivated = None
        self.disposed = None
        self.exiting = None

    def __enter__(self): return self
    def __exit__(self, type, value, traceback): pass


    def assertNotDisposed(self): pass

    # IsActive

    def run(self):
        self.assertNotDisposed()
        if not self.initialized: self.initialize(); self.initialized = True
        self.beginRun()
        self.isActive = True
        self.gameTimer = Stopwatch()
        self.running = True
        while self.running:
            self.tick()
            # Draw unless suppressed
            if self.suppressDraw: self.suppressDraw = False
            elif self.beginDraw(): self.draw(); self.endDraw()
        if self.exiting: self.exiting()
        self.endRun()

    def tick(self):
        self.advanceElapsedTime()
        if self.accumulatedElapsedTime > Game.maxElapsedTime: self.accumulatedElapsedTime = Game.maxElapsedTime
        # advance
        if self.forceElapsedTimeToZero: self.elapsedGameTime = timedelta(); self.forceElapsedTimeToZero = False
        else: self.elapsedGameTime = self.accumulatedElapsedTime; self.totalGameTime += self.elapsedGameTime
        self.accumulatedElapsedTime = timedelta()
        self.assertNotDisposed()
        self.update()

    def advanceElapsedTime(self) -> timedelta:
        currentTicks = self.gameTimer.get_elapsed_time()
        timeAdvanced = timedelta(microseconds=currentTicks - self.previousTicks)
        self.accumulatedElapsedTime += timeAdvanced
        self.previousTicks = currentTicks
        return timeAdvanced

    def exit(self) -> None:
        self.running = False
        self.suppressDraw = True

    def resetElapsedTime(self) -> None:
        self.forceElapsedTimeToZero = True

    def beginDraw(self) -> bool: return self.deviceManager.beginDraw() if self.deviceManager else True

    def endDraw(self) -> None:
        if self.deviceManager: self.deviceManager.endDraw()

    def beginRun(self) -> None: pass

    def endRun(self) -> None: pass

    def loadContent(self) -> None: pass

    def unloadContent(self) -> None: pass

    def initialize(self) -> None:
        for s in self.components: s.initialize()
        self.loadContent()

    def draw(self) -> None:
        pass
        #lock (drawableComponents) {
        #    for (int i = 0; i < drawableComponents.Count; i += 1) {
        #        currentlyDrawingComponents.Add(drawableComponents[i]);
        #    }
        #}
        #foreach (IDrawable drawable in currentlyDrawingComponents) {
        #    if (drawable.Visible) {
        #        drawable.Draw(gameTime);
        #    }
        #}
        #currentlyDrawingComponents.Clear();

    def update(self) -> None:
        pass
        #lock (updateableComponents) {
        #    for (int i = 0; i < updateableComponents.Count; i += 1) {
        #        currentlyUpdatingComponents.Add(updateableComponents[i]);
        #    }
        #}
        #foreach (IUpdateable updateable in currentlyUpdatingComponents) {
        #    if (updateable.Enabled) {
        #        updateable.Update(gameTime);
        #    }
        #}
        #currentlyUpdatingComponents.Clear();
        #FrameworkDispatcher.Update();