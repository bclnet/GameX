import os, platform, json, psutil
if platform.system() == 'Windows': import winreg

@staticmethod
def getPathByKey(key: str, family: str, elem: dict[str, object]):
    # store_abandon.init()
    # store_archive.init()
    # store_blizzard.init()
    # store_direct.init()
    # store_epic.init()
    # store_gog.init()
    # store_local.init() 
    # store_steam.init()
    # store_ubisoft.init()
    # store_winreg.init()
    k,v = key.split(':', 1)
    match k:
        case 'Steam': store_steam.init(); return store_steam.paths[v] if v in store_steam.paths else None
        case 'Gog': store_gog.init(); return store_gog.paths[v] if v in store_gog.paths else None
        case 'Blizzard': store_blizzard.init(); return store_blizzard.paths[v] if v in store_blizzard.paths else None
        case 'Epic': store_epic.init(); return store_epic.paths[v] if v in store_epic.paths else None
        case 'Ubisoft': store_ubisoft.init(); return store_ubisoft.paths[v] if v in store_ubisoft.paths else None
        case 'Abandon': store_abandon.init(); return store_abandon.paths[v] if (v := f'{family}/{v}') in store_abandon.paths else None
        case 'Archive': store_archive.init(); return store_archive.paths[v] if (v := f'{family}/{v}') in store_archive.paths else None
        case 'WinReg': store_winreg.init(); return store_winreg.getPathByKey(v, elem)
        case 'Local': store_local.init(); return store_local.paths[v] if v in store_local.paths else None
        case 'Direct': store_direct.init(); return store_direct.getPathByKey(v)
        case 'Unknown': return None
        case _: raise Exception(f'Unknown key: {key}')

#region store_abandon

class store_abandon:
    @staticmethod
    def getPath(): return 'E:\\AbandonLibrary'

    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        paths = cls.paths = {}
        root = cls.getPath()
        if not root or not os.path.exists(root): return
        # query games
        for s in [s for s in os.listdir(root)]:
            for t in [os.path.join(s, t) for t in os.listdir(os.path.join(root, s))]:
                paths[t] = os.path.join(root, t)
        # print(f'Abandon:{paths}')

#endregion

#region store_archive

class store_archive:
    @staticmethod
    def getPath(): return 'E:\\ArchiveLibrary'

    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        paths = cls.paths = {}
        root = cls.getPath()
        if not root or not os.path.exists(root): return
        # query games
        for s in [s for s in os.listdir(root)]:
            for t in [os.path.join(s, t) for t in os.listdir(os.path.join(root, s))]:
                paths[t] = os.path.join(root, t)
        # print(f'Archive:{paths}')

#endregion

#region store_blizzard

class store_blizzard:
    @staticmethod
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            home = os.getenv('ALLUSERSPROFILE')
            paths = [os.path.join(home, 'Battle.net', 'Agent')]
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['.steam', '.steam/steam', '.steam/root', '.local/share/Steam']
            paths = [os.path.join(home, path, 'appcache') for path in search]
        elif system == 'Darwin':
            # mac paths
            home = [os.path.expanduser('~'), '/Users/Shared']
            search = ['Battle.net/Agent']
            paths = [os.path.join(h, s, 'data') for s in search for h in home]
        else: raise Exception(f'Unknown platform: {system}')
        return next(iter(x for x in paths if os.path.isdir(x)), None)

    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        paths = cls.paths = {}
        root = cls.getPath()
        if not root or not os.path.exists(dbPath := os.path.join(root, 'product.db')): return
        # query games
        from .Blizzard_pb2 import Database
        productDb = Database()
        with open(dbPath, 'rb') as f:
            bytes = f.read()
            productDb.ParseFromString(bytes)
            #try: database.ParseFromString(bytes)
            #except InvalidProtocolBufferException: return None
            for app in productDb.ProductInstall:
                # add appPath if exists
                appPath = app.Settings.InstallPath
                if os.path.isdir(appPath): paths[app.Uid] = appPath
        # print(f'Blizzard:{paths}')

#endregion

#region store_direct

class store_direct:
    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        # print(f'Direct:{cls.getPathByKey("%AppPath%")}')

    @staticmethod
    def getPathByKey(key: str) -> str: return key
    
#endregion

#region store_epic

class store_epic:
    @staticmethod
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            home = os.getenv('ALLUSERSPROFILE')
            search = ['Epic/EpicGamesLauncher']
            paths = [os.path.join(home, path, 'Data') for path in search]
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['?GOG?']
            paths = [os.path.join(home, path, 'Data') for path in search]
        elif system == 'Darwin':
            # mac paths
            hhome = [os.path.expanduser('~'), '/Users/Shared']
            search = ['Epic/EpicGamesLauncher']
            paths = [os.path.join(h, s, 'Data') for s in search for h in home]
        else: raise Exception(f'Unknown platform: {system}')
        return next(iter(x for x in paths if os.path.isdir(x)), None)
        
    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        paths = cls.epicPaths = {}
        root = cls.getPath()
        if not root or not os.path.exists(dbPath := os.path.join(root, 'Manifests')): return
        # query games
        for s in [s for s in os.listdir(dbPath) if s.endswith('.item')]:
            with open(os.path.join(dbPath, s), 'r') as f:
                # add appPath if exists
                appPath = json.loads(f.read())['InstallLocation']
                if os.path.isdir(appPath): paths[s[:-5]] = appPath
        # print(f'Epic:{paths}')

#endregion

#region store_gog

class store_gog:
    @staticmethod
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            home = os.getenv('ALLUSERSPROFILE')
            search = ['GOG.com/Galaxy']
            paths = [os.path.join(home, path, 'storage') for path in search]
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['??']
            paths = [os.path.join(home, path, 'Storage') for path in search]
        elif system == 'Darwin':
            # mac paths
            home = [os.path.expanduser('~'), '/Users/Shared']
            search = ['GOG.com/Galaxy']
            paths = [os.path.join(h, s, 'Storage') for s in search for h in home]
        else: raise Exception(f'Unknown platform: {system}')
        return next(iter(s for s in paths if os.path.isdir(s)), None)
        
    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        paths = cls.paths = {}
        root = cls.getPath()
        if not root or not os.path.exists(dbPath := os.path.join(root, 'galaxy-2.0.db')): return
        # query games
        import sqlite3
        from contextlib import closing
        with closing(sqlite3.connect(dbPath)) as connection:
            with closing(connection.cursor()) as cursor:
                for s in cursor.execute('SELECT productId, installationPath FROM InstalledBaseProducts').fetchall():
                    # add appPath if exists
                    appPath = s[1]
                    if os.path.isdir(appPath): paths[s[0]] = appPath
        # print(f'Gog:{paths}')

#endregion

#region store_local

class store_local:
    GAMESPATH = 'Games'

    @classmethod
    def init(cls):
        system = platform.system()
        cls.init = lambda *args: None
        gameRoots = [os.path.join(x.mountpoint, cls.GAMESPATH) for x in psutil.disk_partitions()]
        gameRoots.append(os.path.join(os.path.expanduser('~'), cls.GAMESPATH))
        gameRoots.append(os.path.join('/Users/Shared', cls.GAMESPATH))
        if system == 'Android': gameRoots.append(os.path.join('/sdcard', cls.GAMESPATH))
        paths = cls.paths = {x:os.path.join(r,x) for r in gameRoots if os.path.isdir(r) for x in os.listdir(r)}
        # print(f'Local:{paths}')

#endregion

#region store_steam

class store_steam:
    class AcfStruct:
        def read(path):
            if not os.path.exists(path): return None
            with open(path, 'r') as f: return store_steam.AcfStruct(f.read())

        def __init__(self, region):
            def nextEndOf(str, open, close, startIndex):
                if open == close: raise Exception('"Open" and "Close" char are equivalent!')
                openItem = 0; closeItem = 0
                for i in range(startIndex, len(str)):
                    if str[i] == open: openItem += 1
                    if str[i] == close:
                        closeItem += 1
                        if closeItem > openItem: return i
                raise Exception('Not enough closing characters!')
            self.get = {}
            self.value = {}
            lengthOfRegion = len(region); index = 0
            while (lengthOfRegion > index):
                firstStart = region.find('"', index)
                if firstStart == -1: break
                firstEnd = region.find('"', firstStart + 1)
                index = firstEnd + 1
                first = region[firstStart + 1:firstEnd]
                secondStart = region.find('"', index)
                secondOpen = region.find('{', index)
                if secondStart == -1:
                    self.get[first] = None
                elif secondOpen == -1 or secondStart < secondOpen:
                    secondEnd = region.find('"', secondStart + 1)
                    index = secondEnd + 1
                    second = region[secondStart + 1:secondEnd]
                    self.value[first] = second.replace('\\\\', '\\')
                else:
                    secondClose = nextEndOf(region, '{', '}', secondOpen + 1)
                    acfs = store_steam.AcfStruct(region[secondOpen + 1:secondClose])
                    index = secondClose + 1
                    self.get[first] = acfs

        def repr(self, depth):
            b = []
            for (k,v) in self.value.items():
                b.append(f'{"  "*depth}"{k}": "{v}"\n')
            for (k,v) in self.get.items():
                b.append(f'{"  "*depth}"{k}" {{\n{"  "*depth}')
                if not v is None: b.append(v.repr(depth + 1))
                b.append(f'}}\n')
            return ''.join(b)
        def __repr__(self): return self.repr(0)

    @staticmethod
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            try: key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, 'SOFTWARE\\Valve\\Steam', 0, winreg.KEY_READ)
            except FileNotFoundError:
                try: key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, 'SOFTWARE\\Valve\\Steam', 0, winreg.KEY_READ | winreg.KEY_WOW64_32KEY)
                except FileNotFoundError: return None
            return winreg.QueryValueEx(key, 'SteamPath')[0]
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['.steam', '.steam/steam', '.steam/root', '.local/share/Steam']
            paths = [os.path.join(home, path, 'appcache') for path in search]
        elif system == 'Darwin':
            # mac paths
            home = [os.path.expanduser('~'), '/Users/Shared']
            search = ['Library/Application Support/Steam']
            paths = [os.path.join(h, s) for s in search for h in home]
        else: raise Exception(f'Unknown platform: {system}')
        return next(iter(x for x in paths if os.path.isdir(x)), None)
        
    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        paths = cls.paths = {}
        root = cls.getPath()
        if not root: return
        # query games
        libraryFolders = cls.AcfStruct.read(os.path.join(root, 'steamapps', 'libraryfolders.vdf'))
        for folder in libraryFolders.get['libraryfolders'].get.values():
            path = folder.value['path']
            if not os.path.isdir(path): continue
            for appId in folder.get['apps'].value.keys():
                appManifest = cls.AcfStruct.read(os.path.join(path, 'steamapps', f'appmanifest_{appId}.acf'))
                if appManifest is None: continue
                # add appPath if exists
                appPath = os.path.join(path, 'steamapps', 'common', appManifest.get['AppState'].value['installdir'])
                if os.path.isdir(appPath): paths[appId] = appPath
        # print(f'Steam:{paths}')

#endregion

#region store_ubisoft

class store_ubisoft:
    @staticmethod
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            home = os.getenv('LOCALAPPDATA')
            search = ['Ubisoft Game Launcher']
            paths = [os.path.join(home, path) for path in search]
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['??']
            paths = [os.path.join(home, path) for path in search]
        elif system == 'Darwin':
            # mac paths
            home = [os.path.expanduser('~'), '/Users/Shared']
            search = ['??']
            paths = [os.path.join(h, s) for s in search for h in home]
        else: raise Exception(f'Unknown platform: {system}')
        return next(iter(x for x in paths if os.path.isdir(x)), None)
        
    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        paths = cls.paths = {}
        root = cls.getPath()
        if not root or not os.path.exists(dbPath := os.path.join(root, 'settings.yaml')): return
        # query games
        with open(dbPath, 'r') as f: body = f.read()
        gamePath = body[body.index('game_installation_path:') + 23:body.index('installer_cache_path')].strip()
        for s in [s for s in os.listdir(gamePath)]:
            paths[s] = os.path.join(gamePath, s)
        # print(f'Ubisoft:{paths}')

#endregion

#region store_winreg

class store_winreg:
    @classmethod
    def init(cls):
        cls.init = lambda *args: None
        # print(f'WinReg:{cls.getPathByKey("GOG.com/Games/1207658680", None)}')

    @staticmethod
    def getPathByKey(key: str, elem: dict[str, object]) -> str:
        return store_winreg.getPathByRegistryKey(key, elem[key] if elem and key in elem else None) if platform.system() == 'Windows' else None

    @staticmethod
    def findRegistryPath(paths: list[str]) -> str:
        for p in paths:
            keyPath = p[1:].replace('/', '\\') if p.startswith('@') else f'SOFTWARE\\{p.replace('/', '\\')}'
            try: key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, keyPath, 0, winreg.KEY_READ)
            except FileNotFoundError:
                try: key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, keyPath, 0, winreg.KEY_READ)
                except FileNotFoundError:
                    try: key = winreg.OpenKey(winreg.HKEY_CLASSES_ROOT, f'VirtualStore\\MACHINE\\{keyPath}', 0, winreg.KEY_READ)
                    except FileNotFoundError: key = None
            if key is None: continue
            # search directories
            path = None
            for search in ['Path', 'Install Dir', 'InstallDir', 'InstallLocation', '']:
                try:
                    val = winreg.QueryValueEx(key, search)[0]
                    if os.path.isdir(val): path = val; break
                except FileNotFoundError: continue
            # search files
            if path is None:
                for search in ['Installed Path', 'ExePath', 'Exe']:
                    try:
                        val = winreg.QueryValueEx(key, search)[0]
                        if os.path.exists(val): path = val; break
                    except FileNotFoundError: continue
                if path is not None: path = os.path.dirname(path)
            if path is not None and os.path.isdir(path): return path
        return None

    @staticmethod
    def getPathByRegistryKey(key: str, elem: dict[str, object]) -> str:
        path = store_winreg.findRegistryPath([f'Wow6432Node\\{key}', key])
        if not elem: return path
        elif 'path' in elem: return os.path.abspath(decodePath(elem['path'], path))
        elif 'xml' in elem and 'xmlPath' in elem:
            return store_winreg.getSingleFileValue(decodePath(elem['xml'], path), 'xml', elem['xmlPath'])
        return None

    @staticmethod
    def getSingleFileValue(path: str, ext: str, select: str) -> str:
        if not os.fileExists(path): return None
        with open(path, 'r') as f: content = f.read()
        match ext:
            case 'xml': raise NotImplementedError() #return XDocument.Parse(content).XPathSelectElement(select)?.Value,
            case _: raise Exception(f'Unknown {ext}')
        return os.path.basename(value) if value else None

#endregion
