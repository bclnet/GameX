# RaylibX
class RaylibX:
    buildersByType = {}

    # Calls is_window_ready. Does not open a window. Imports pyray only when called.
    @staticmethod
    def version() -> str:
        import pyray
        pyray.is_window_ready()
        return pyray.RAYLIB_VERSION
