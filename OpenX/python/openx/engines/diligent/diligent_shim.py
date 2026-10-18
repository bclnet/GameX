# DiligentShim. Reports that this build did not link Diligent Engine.
# Importing this module does not load a native library.
class DiligentShim:
    @staticmethod
    def linked() -> bool: return False
    @staticmethod
    def version() -> str: return 'not linked'
