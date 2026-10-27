// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameX.Origin.Netcode.UO;

/// <summary>
/// Login-socket result for LoginScene. Network thread posts; UI thread takes.
/// </summary>
public enum UOLoginPhase {
    ServersReady,
    CharactersReady,
    Failed,
    RelayInProgress,
    WorldEntered
}

/// <summary>One graphic effect from 0x23, 0x70, 0xC0, or 0xC7. The view drains it.</summary>
public readonly struct UOGraphicEffect {
    public UOGraphicEffect(byte type, uint source, uint target, ushort graphic, ushort hue, ushort srcX, ushort srcY, sbyte srcZ, ushort targetX, ushort targetY, sbyte targetZ, byte speed, byte duration, bool fixedDirection, bool explode) {
        Type = type;
        Source = source;
        Target = target;
        Graphic = graphic;
        Hue = hue;
        SrcX = srcX;
        SrcY = srcY;
        SrcZ = srcZ;
        TargetX = targetX;
        TargetY = targetY;
        TargetZ = targetZ;
        Speed = speed;
        Duration = duration;
        FixedDirection = fixedDirection;
        Explode = explode;
    }

    public byte Type { get; }
    public uint Source { get; }
    public uint Target { get; }
    public ushort Graphic { get; }
    public ushort Hue { get; }
    public ushort SrcX { get; }
    public ushort SrcY { get; }
    public sbyte SrcZ { get; }
    public ushort TargetX { get; }
    public ushort TargetY { get; }
    public sbyte TargetZ { get; }
    public byte Speed { get; }
    public byte Duration { get; }
    public bool FixedDirection { get; }
    public bool Explode { get; }
}

/// <summary>
/// ClassicUO's first login socket. The first attempt is clear: 0xEF or a 4-byte seed, then 0x80, with no cipher.
/// A zero-byte close or a socket error tries once more with the login cipher. Login packets are not Huffman.
/// 0x5D switches this socket onto the Huffman game stream. Plain skips the game cipher too.
/// </summary>
public static class UOLoginNet {
    const int AsciiField = 30;
    const uint SeedWhenNotIpv4 = 0x7F000001;
    const int ReadWaitMs = 50;

    static readonly object Gate = new();
    static TcpClient _client;
    static int _runId;
    static string _password;
    static int? _select;
    static bool _have;
    static string[] _takeServers, _takeCharacters;
    static string _takeMessage;
    static UOLoginPhase _takePhase;
    static int? _playIndex;
    static string _playName;
    static readonly Queue<byte> _walks = new();
    static readonly Queue<uint> _fastWalk = new();
    static readonly Dictionary<uint, UOMobileState> _mobiles = new();
    static readonly Dictionary<uint, UOItemState> _items = new();
    static readonly Dictionary<uint, UOEquipState> _equip = new();
    static readonly Dictionary<uint, UOContainedState> _contents = new();
    static readonly Dictionary<uint, UOStatusState> _status = new();
    static readonly Dictionary<uint, string> _names = new();
    static readonly Dictionary<uint, Anim> _anims = new();
    static readonly Dictionary<uint, (int until, bool run)> _moves = new();
    static int _stepUntil;
    static bool _stepRun;
    static readonly List<UOSpeechState> _speech = new();
    static readonly Queue<string> _says = new();
    static readonly Queue<uint> _uses = new();
    static readonly Queue<(uint serial, string title, byte flags)> _paperdolls = new();
    static readonly Queue<(uint serial, ushort gump)> _containers = new();
    static readonly Queue<UOGumpEvent> _gumps = new();
    static readonly Queue<UOMenu> _menus = new();
    static readonly Queue<UOTextEntry> _entries = new();
    static readonly Queue<UOShop> _shops = new();
    static readonly Queue<UOTrade> _trades = new();
    static readonly Queue<UODye> _dyes = new();
    static readonly Queue<UOGraphicEffect> _effects = new();
    static readonly Queue<(uint serial, ushort body, ushort type, ushort action, byte mode)> _newAnims = new();
    static readonly Queue<uint> _spellbooks = new();
    static readonly Queue<uint> _vendors = new();
    static readonly Queue<byte[]> _out = new();
    static readonly Dictionary<ushort, UOSkillState> _skills = new();
    static readonly Dictionary<ushort, string> _skillNames = new();
    static readonly Dictionary<uint, (byte flags, bool sa)> _health = new();
    static readonly Dictionary<uint, (string name, string data)> _props = new();
    static readonly Dictionary<ushort, UOBuffState> _buffs = new();
    static readonly Dictionary<uint, (ushort x, ushort y)> _arrows = new();
    static readonly Dictionary<uint, UOCorpseEquip> _corpse = new();
    static readonly Dictionary<uint, int> _order = new();
    static int _orderNext;
    static int _revision;
    static uint _player;
    static ushort _pbody;
    static int _px, _py, _pz;
    static byte _pdir;
    static uint _auth;
    static byte _seq;
    static bool _inWorld;
    static bool _gameStream;
    static bool _sawCharacters;
    static bool _postedWorld;
    static volatile bool _gameReady;
    static UOHuffman _huff = new();
    static int _mapIndex;
    static int _nextWalk;
    static bool _create;
    static bool _createFemale;
    static int _createSlot;
    static string _createName;
    static byte[] _ack;
    static bool _entered;
    static string _versionText = "";
    static ClientVersion _clientVersion;
    static byte _overall, _personal, _season, _music, _viewRange;
    static uint _features;
    static bool _target, _prompt, _promptUnicode, _war;
    static byte _targetKind, _targetType, _noto = 1, _weather, _wcount, _wtemp, _song, _pingIdx;
    static uint _targetId, _hold, _lastAttack;
    static ulong _promptId;
    static ushort _sound;
    static int _nextPing, _targetedAt;
    static readonly string[] DenyText = {
        "You can not pick that up.",
        "That is too far away.",
        "That is out of sight.",
        "That item does not belong to you.  You'll have to steal it.",
        "You are already holding an item.",
    };

    /// <summary>Stand height for a predicted step. Set from the UO asset slot. Null keeps the current Z.</summary>
    public static Func<int, int, int, int> StepHeight;

    /// <summary>Opens the map pack for 0xBF subcommand 0x0008. Set from the UO asset slot.</summary>
    public static Action<int> FacetSwitch;

    /// <summary>Connect or protocol failure.</summary>
    public static string LastError { get; private set; }

    /// <summary>True after the seed and the encrypted 0x80 have been written.</summary>
    public static bool Started => _started;
    static volatile bool _started;

    /// <summary>
    /// Opens the login server on a background thread and writes the seed and encrypted account packet.
    /// A null or empty host returns without throwing.
    /// </summary>
    public static void Begin(string host, int port, string account, string password, ClientVersion version, string versionText = null) {
        if (string.IsNullOrEmpty(host)) return;
        if (version == 0) {
            _started = false;
            LastError = "client version is missing";
            Post(UOLoginPhase.Failed, message: LastError);
            return;
        }
        UOGamePackets.UseVersion(version);
        _started = false;
        LastError = null;
        var id = Interlocked.Increment(ref _runId);
        TcpClient previous;
        lock (Gate) {
            _select = null;
            _playIndex = null;
            _playName = null;
            _walks.Clear();
            _fastWalk.Clear();
            _mobiles.Clear();
            _items.Clear();
            _equip.Clear();
            _contents.Clear();
            _status.Clear();
            _names.Clear();
            _speech.Clear();
            _says.Clear();
            _uses.Clear();
            _paperdolls.Clear();
            _containers.Clear();
            _gumps.Clear();
            _menus.Clear();
            _entries.Clear();
            _shops.Clear();
            _trades.Clear();
            _dyes.Clear();
            _effects.Clear();
            _newAnims.Clear();
            _spellbooks.Clear();
            _vendors.Clear();
            _out.Clear();
            _target = _prompt = _promptUnicode = _war = false;
            _targetKind = _targetType = _weather = _wcount = _wtemp = _song = _pingIdx = 0;
            _noto = 1;
            _targetId = _hold = _lastAttack = 0;
            _promptId = 0;
            _sound = 0;
            _nextPing = _targetedAt = 0;
            _skills.Clear();
            _skillNames.Clear();
            _health.Clear();
            _props.Clear();
            _buffs.Clear();
            _arrows.Clear();
            _corpse.Clear();
            _order.Clear();
            _create = false;
            _ack = null;
            _entered = false;
            _versionText = string.IsNullOrWhiteSpace(versionText) ? "" : versionText.Trim();
            _clientVersion = version;
            _overall = _personal = _season = _music = _viewRange = 0;
            _features = 0;
            _player = 0;
            _pbody = 0;
            _px = _py = _pz = 0;
            _pdir = 0;
            _mapIndex = 0;
            _nextWalk = 0;
            _revision++;
            previous = _client;
            _client = null;
        }
        previous?.Close();
        FacetSwitch?.Invoke(0);
        Task.Run(() => Connect(host, port, account, password, version, id));
    }

    /// <summary>
    /// True once per new network result. Call from the UI thread. The network thread does not touch the scene.
    /// </summary>
    public static bool Take(out string[] servers, out string[] characters, out string message, out UOLoginPhase phase) {
        lock (Gate) {
            if (!_have) {
                servers = null;
                characters = null;
                message = null;
                phase = default;
                return false;
            }
            _have = false;
            servers = _takeServers;
            characters = _takeCharacters;
            message = _takeMessage;
            phase = _takePhase;
            _takeServers = null;
            _takeCharacters = null;
            _takeMessage = null;
            return true;
        }
    }

    /// <summary>
    /// Queues a 3-byte 0xA0 for the network thread.
    /// </summary>
    public static void SelectServer(int index) {
        lock (Gate) _select = index;
    }

    /// <summary>Queues 0x5D. The network thread sends it. Does not open a socket.</summary>
    public static void SelectCharacter(int index, string name) {
        lock (Gate) {
            _playIndex = index;
            _playName = name ?? "";
        }
    }

    /// <summary>Queues 0x00 or 0xF8. The network thread sends it on the game socket. No password is written.</summary>
    public static void CreateCharacter(string name, bool female, int slot) {
        if (string.IsNullOrWhiteSpace(name)) return;
        lock (Gate) {
            _createName = name.Trim();
            _createFemale = female;
            _createSlot = slot;
            _create = true;
        }
    }

    /// <summary>Queues 0xAD once the world is up.</summary>
    public static void Say(string text) {
        if (!_gameReady || string.IsNullOrWhiteSpace(text)) return;
        lock (Gate) {
            if (_says.Count >= 4) return;
            _says.Enqueue(text.Trim());
        }
    }

    /// <summary>Queues 0x06. Double-click a mobile for the paperdoll, or a container for its contents.</summary>
    public static void Use(uint serial) {
        if (!_gameReady || serial == 0) return;
        lock (Gate) {
            if (_uses.Count >= 8) return;
            _uses.Enqueue(serial);
        }
    }

    /// <summary>Queues 0x02 once the game stream is up. Ignored before 0x5D.</summary>
    public static void Walk(byte direction) {
        if (!_gameReady) return;
        lock (Gate) {
            if (_walks.Count >= 4) return;
            _walks.Enqueue(direction);
        }
    }

    public static uint PlayerSerial { get { lock (Gate) return _player; } }
    public static ushort PlayerBody { get { lock (Gate) return _pbody; } }
    public static int PlayerX { get { lock (Gate) return _px; } }
    public static int PlayerY { get { lock (Gate) return _py; } }
    public static int PlayerZ { get { lock (Gate) return _pz; } }
    public static int MapIndex { get { lock (Gate) return _mapIndex; } }
    public static byte OverallLight { get { lock (Gate) return _overall; } }
    public static byte PersonalLight { get { lock (Gate) return _personal; } }
    public static byte Season { get { lock (Gate) return _season; } }
    public static byte SeasonMusic { get { lock (Gate) return _music; } }
    public static byte ViewRange { get { lock (Gate) return _viewRange; } }
    public static uint Features { get { lock (Gate) return _features; } }
    public static byte PlayerDirection { get { lock (Gate) return _pdir; } }

    /// <summary>True while a step is in flight. run is ClassicUO's direction bit 0x80.</summary>
    public static bool MobileMoving(uint serial, out bool run) {
        lock (Gate) {
            if (serial == _player) {
                run = _stepRun;
                return unchecked(Environment.TickCount - _stepUntil) < 0;
            }
            if (_moves.TryGetValue(serial, out var step)) {
                run = step.run;
                return unchecked(Environment.TickCount - step.until) < 0;
            }
            run = false;
            return false;
        }
    }

    /// <summary>0x6E state. fromServer is false until that packet, and again after its repeats finish.</summary>
    public static void TryServerAnim(uint serial, out bool fromServer, out ushort raw, out byte frame) {
        lock (Gate) {
            if (_anims.TryGetValue(serial, out var anim)) {
                fromServer = anim.FromServer;
                raw = anim.Raw;
                frame = anim.Frame;
                return;
            }
        }
        fromServer = false;
        raw = 0;
        frame = 0;
    }

    /// <summary>ClassicUO Mobile.ProcessAnimation. The first call holds frame 0 for one interval.</summary>
    public static void StepAnim(uint serial, int fileFrames) {
        if (fileFrames <= 0) return;
        var now = Environment.TickCount;
        lock (Gate) {
            if (!_anims.TryGetValue(serial, out var anim)) {
                _anims[serial] = new Anim { Next = unchecked(now + 80) };
                return;
            }
            if (unchecked(now - anim.Next) < 0) return;
            var delay = anim.FromServer ? 80 + 80 * (anim.Delay + 1) : 80;
            if (anim.FromServer) {
                var idx = anim.Frame + (anim.Forward ? 1 : -1);
                var limit = anim.PacketFrames == 0 ? fileFrames : anim.PacketFrames;
                var wrapped = false;
                if (anim.Forward && idx >= limit) { idx = 0; wrapped = true; }
                else if (!anim.Forward && idx < 0) { idx = fileFrames - 1; wrapped = true; }
                if (wrapped && anim.Repeats != 0) {
                    anim.Repeats--;
                    if (anim.Repeats == 0) {
                        if (anim.Repeat) { anim.Repeats = anim.RepeatReset; anim.Repeat = false; }
                        else anim.FromServer = false;
                    }
                }
                anim.Frame = (byte)((idx % fileFrames + fileFrames) % fileFrames);
            }
            else {
                var idx = anim.Frame + 1;
                if (idx >= fileFrames) idx = 0;
                anim.Frame = (byte)idx;
            }
            anim.Next = unchecked(now + delay);
        }
    }
    public static int WorldRevision { get { lock (Gate) return _revision; } }

    public static void CopyActors(List<UOMobileState> mobiles, List<UOItemState> items) {
        lock (Gate) {
            mobiles.Clear();
            items.Clear();
            foreach (var m in _mobiles.Values) mobiles.Add(m);
            foreach (var it in _items.Values) items.Add(it);
        }
    }

    public static void CopySpeech(List<UOSpeechState> dest) {
        lock (Gate) {
            dest.Clear();
            foreach (var line in _speech) dest.Add(line);
        }
    }

    public static void CopySkills(List<UOSkillState> dest) {
        lock (Gate) {
            dest.Clear();
            foreach (var row in _skills.Values) dest.Add(row);
        }
    }

    /// <summary>Drains graphic effects queued by 0x23, 0x70, 0xC0, and 0xC7.</summary>
    public static void TakeGraphics(List<UOGraphicEffect> dest) {
        lock (Gate) {
            dest.Clear();
            while (_effects.Count > 0) dest.Add(_effects.Dequeue());
        }
    }

    public static void CopyEquip(uint parent, List<UOEquipState> dest) {
        lock (Gate) {
            dest.Clear();
            foreach (var e in _equip.Values)
                if (e.Parent == parent) dest.Add(e);
        }
    }

    public static void CopyContents(uint container, List<UOContainedState> dest) {
        lock (Gate) {
            dest.Clear();
            foreach (var it in _contents.Values)
                if (it.Container == container) dest.Add(it);
        }
    }

    public static bool TryStatus(uint serial, out UOStatusState status) {
        lock (Gate) return _status.TryGetValue(serial, out status);
    }

    /// <summary>0x16 and 0x17. Poison is the SA bool from 7.0, otherwise the poisoned flag. Yellow is the yellow-bar flag.</summary>
    public static bool TryHealthBar(uint serial, out bool poisoned, out bool yellow) {
        lock (Gate) {
            if (_health.TryGetValue(serial, out var h)) {
                poisoned = _clientVersion >= ClientVersion.CV_7000 ? h.sa : (h.flags & 0x04) != 0;
                yellow = (h.flags & 0x08) != 0;
                return true;
            }
        }
        poisoned = yellow = false;
        return false;
    }

    /// <summary>0xD6. Name is the first cliloc line. Data is the rest, one line each.</summary>
    public static bool TryProperties(uint serial, out string name, out string data) {
        lock (Gate) {
            if (_props.TryGetValue(serial, out var row)) {
                name = row.name;
                data = row.data;
                return true;
            }
        }
        name = data = "";
        return false;
    }

    /// <summary>0xBA. False when that serial's arrow is off.</summary>
    public static bool TryQuestArrow(uint serial, out ushort x, out ushort y) {
        lock (Gate) {
            if (_arrows.TryGetValue(serial, out var a)) {
                x = a.x;
                y = a.y;
                return true;
            }
        }
        x = y = 0;
        return false;
    }

    public static void CopyBuffs(List<UOBuffState> dest) {
        lock (Gate) {
            dest.Clear();
            foreach (var row in _buffs.Values) dest.Add(row);
        }
    }

    public static void CopyCorpseEquip(uint corpse, List<UOCorpseEquip> dest) {
        lock (Gate) {
            dest.Clear();
            foreach (var row in _corpse.Values)
                if (row.Corpse == corpse) dest.Add(row);
        }
    }

    public static string NameOf(uint serial) {
        lock (Gate) return _names.TryGetValue(serial, out var name) ? name : "";
    }

    /// <summary>Layer 0x15 on the player, or 0 when that item has not arrived.</summary>
    public static uint PlayerBackpack {
        get {
            lock (Gate) {
                foreach (var e in _equip.Values)
                    if (e.Parent == _player && e.Layer == 0x15) return e.Serial;
                return 0;
            }
        }
    }

    public static bool TakePaperdoll(out uint serial, out string title, out byte flags) {
        lock (Gate) {
            if (_paperdolls.Count == 0) {
                serial = 0;
                title = "";
                flags = 0;
                return false;
            }
            (serial, title, flags) = _paperdolls.Dequeue();
            return true;
        }
    }

    public static bool TakeSpellbook(out uint serial) {
        lock (Gate) {
            if (_spellbooks.Count == 0) { serial = 0; return false; }
            serial = _spellbooks.Dequeue();
            return true;
        }
    }

    public static bool TakeVendor(out uint serial) {
        lock (Gate) {
            if (_vendors.Count == 0) { serial = 0; return false; }
            serial = _vendors.Dequeue();
            return true;
        }
    }

    public static bool TakeNewAnim(out uint serial, out ushort body, out ushort type, out ushort action, out byte mode) {
        lock (Gate) {
            if (_newAnims.Count == 0) {
                serial = 0;
                body = type = action = 0;
                mode = 0;
                return false;
            }
            (serial, body, type, action, mode) = _newAnims.Dequeue();
            return true;
        }
    }

    /// <summary>0xE2 after the view has turned the packet into a group.</summary>
    public static void SetServerAnim(uint serial, ushort group, bool repeat) {
        NoteAnimation(serial, group, 0, 1, true, repeat, 0);
    }

    public static bool TakeContainer(out uint serial, out ushort gump) {
        lock (Gate) {
            if (_containers.Count == 0) {
                serial = 0;
                gump = 0;
                return false;
            }
            (serial, gump) = _containers.Dequeue();
            return true;
        }
    }

    public static bool TakeGump(out UOGumpEvent ev) {
        lock (Gate) {
            if (_gumps.Count == 0) {
                ev = default;
                return false;
            }
            ev = _gumps.Dequeue();
            return true;
        }
    }

    /// <summary>0xB1. Checked boxes and text entries are collected by the gump.</summary>
    public static void ReplyGump(uint local, uint server, int button, uint[] switches, (ushort id, string text)[] entries) {
        var pkt = UOGamePackets.GumpResponse(local, server, button, switches, entries);
        lock (Gate) _out.Enqueue(pkt);
    }

    public static bool InWarMode { get { lock (Gate) return _war; } }
    public static bool HasTarget { get { lock (Gate) return _target; } }
    public static bool HasPrompt { get { lock (Gate) return _prompt; } }
    public static bool Holding { get { lock (Gate) return _hold != 0; } }
    public static uint HeldSerial { get { lock (Gate) return _hold; } }
    public static byte Notoriety { get { lock (Gate) return _noto; } }
    public static ushort SoundIndex { get { lock (Gate) return _sound; } }
    public static byte MusicIndex { get { lock (Gate) return _song; } }
    public static byte WeatherType { get { lock (Gate) return _weather; } }
    public static byte WeatherCount { get { lock (Gate) return _wcount; } }
    public static byte WeatherTemp { get { lock (Gate) return _wtemp; } }
    public static uint LastAttack { get { lock (Gate) return _lastAttack; } }

    /// <summary>True for a few hundred milliseconds after a target reply, so the double-click does not also send 0x06.</summary>
    public static bool TargetedJustNow => unchecked(Environment.TickCount - _targetedAt) < 500;

    /// <summary>Mobiles sit below 0x40000000. Items, including containers, sit at or above it.</summary>
    public static bool IsMobile(uint serial) => serial != 0 && serial < 0x40000000;

    /// <summary>Queues 0x72 with the opposite of the stored flag. The server echo is what sets the flag.</summary>
    public static void ToggleWar() {
        if (!_gameReady) return;
        lock (Gate) _out.Enqueue(UOGamePackets.WarMode(!_war));
    }

    /// <summary>0x6C cancel. The kind byte is the one the server sent.</summary>
    public static void CancelTarget() {
        lock (Gate) {
            if (!_target) return;
            _out.Enqueue(UOGamePackets.TargetCancel(_targetKind, _targetId, _targetType));
            _target = false;
            _targetedAt = Environment.TickCount;
            _revision++;
        }
    }

    /// <summary>0x6C object. Graphic and position come from the world, a container, or worn equipment.</summary>
    public static void TargetSerial(uint serial) {
        if (serial == 0) return;
        Place(serial, out var graphic, out var x, out var y, out var z);
        lock (Gate) {
            if (!_target) return;
            _out.Enqueue(UOGamePackets.TargetObject(_targetId, _targetType, serial, x, y, z, graphic));
            _target = false;
            _targetedAt = Environment.TickCount;
            _revision++;
        }
    }

    /// <summary>0x6C ground. The graphic is 0.</summary>
    public static void TargetGround(int x, int y, int z) {
        lock (Gate) {
            if (!_target) return;
            _out.Enqueue(UOGamePackets.TargetGround(_targetId, _targetType, x, y, z));
            _target = false;
            _targetedAt = Environment.TickCount;
            _revision++;
        }
    }

    /// <summary>0x9A or 0xC2. Cancel sends an empty body. This does not go through Say.</summary>
    public static void ReplyPrompt(string text, bool cancel) {
        lock (Gate) {
            if (!_prompt) return;
            var pkt = _promptUnicode
                ? UOGamePackets.PromptUnicode(_promptId, text, cancel)
                : UOGamePackets.PromptAscii(_promptId, text, cancel);
            _out.Enqueue(pkt);
            _prompt = false;
            _revision++;
        }
    }

    /// <summary>0x07. The count is the container amount, or 1. A second lift is ignored.</summary>
    public static void Lift(uint serial) {
        if (!_gameReady || serial == 0) return;
        lock (Gate) {
            if (_hold != 0 || _target) return;
            ushort count = 1;
            if (_contents.TryGetValue(serial, out var row) && row.Amount > 0) count = row.Amount;
            _hold = serial;
            _out.Enqueue(UOGamePackets.Pickup(serial, count));
            _revision++;
        }
    }

    /// <summary>0x08. Container 0xFFFFFFFF is the ground. The hold clears when the packet is queued.</summary>
    public static void DropHeld(int x, int y, int z, uint container) {
        lock (Gate) {
            if (_hold == 0) return;
            var serial = _hold;
            _hold = 0;
            _out.Enqueue(UOGamePackets.Drop(serial, (ushort)x, (ushort)y, (sbyte)z, container));
            _revision++;
        }
    }

    /// <summary>0x05, and a 0x34 status request. The player is not a valid target.</summary>
    public static void Attack(uint serial) {
        if (!_gameReady || serial == 0 || serial == _player) return;
        lock (Gate) {
            _lastAttack = serial;
            _out.Enqueue(UOGamePackets.Attack(serial));
            _out.Enqueue(UOGamePackets.StatusRequest(serial));
            _revision++;
        }
    }

    /// <summary>0x7D. Code 0 is the cancel.</summary>
    public static void ReplyMenu(uint serial, ushort menu, ushort code, ushort graphic, ushort hue) {
        lock (Gate) _out.Enqueue(UOGamePackets.MenuResponse(serial, menu, code, graphic, hue));
    }

    /// <summary>0xAC.</summary>
    public static void ReplyText(uint serial, byte parent, byte button, string text, bool ok) {
        lock (Gate) _out.Enqueue(UOGamePackets.TextEntryReply(serial, parent, button, text, ok));
    }

    /// <summary>Empty 0x3B or 0x9F.</summary>
    public static void ReplyShop(uint serial, bool buy) {
        lock (Gate) _out.Enqueue(buy ? UOGamePackets.Buy(serial) : UOGamePackets.Sell(serial));
    }

    /// <summary>0x6F cancel. Eight bytes.</summary>
    public static void ReplyTrade(uint serial) {
        lock (Gate) _out.Enqueue(UOGamePackets.TradeCancel(serial));
    }

    /// <summary>0x95. The hue is the one the dye box confirmed.</summary>
    public static void ReplyDye(uint serial, ushort hue) {
        lock (Gate) _out.Enqueue(UOGamePackets.Dye(serial, hue));
    }

    public static bool TakeMenu(out UOMenu menu) {
        lock (Gate) {
            if (_menus.Count == 0) {
                menu = null;
                return false;
            }
            menu = _menus.Dequeue();
            return true;
        }
    }

    public static bool TakeText(out UOTextEntry entry) {
        lock (Gate) {
            if (_entries.Count == 0) {
                entry = null;
                return false;
            }
            entry = _entries.Dequeue();
            return true;
        }
    }

    public static bool TakeShop(out UOShop shop) {
        lock (Gate) {
            if (_shops.Count == 0) {
                shop = default;
                return false;
            }
            shop = _shops.Dequeue();
            return true;
        }
    }

    public static bool TakeTrade(out UOTrade trade) {
        lock (Gate) {
            if (_trades.Count == 0) {
                trade = default;
                return false;
            }
            trade = _trades.Dequeue();
            return true;
        }
    }

    public static bool TakeDye(out UODye dye) {
        lock (Gate) {
            if (_dyes.Count == 0) {
                dye = default;
                return false;
            }
            dye = _dyes.Dequeue();
            return true;
        }
    }

    static void Place(uint serial, out ushort graphic, out int x, out int y, out int z) {
        lock (Gate) {
            if (_mobiles.TryGetValue(serial, out var m)) {
                graphic = m.Body;
                x = m.X;
                y = m.Y;
                z = m.Z;
                return;
            }
            if (_items.TryGetValue(serial, out var it)) {
                graphic = it.Graphic;
                x = it.X;
                y = it.Y;
                z = it.Z;
                return;
            }
            if (_contents.TryGetValue(serial, out var c)) {
                graphic = c.Graphic;
                x = c.X;
                y = c.Y;
                z = 0;
                return;
            }
            if (_equip.TryGetValue(serial, out var e)) {
                graphic = e.Graphic;
                if (_mobiles.TryGetValue(e.Parent, out var parent)) {
                    x = parent.X;
                    y = parent.Y;
                    z = parent.Z;
                }
                else x = y = z = 0;
                return;
            }
            graphic = 0;
            x = y = z = 0;
        }
    }

    static void Connect(string host, int port, string account, string password, ClientVersion version, int id) {
        TcpClient client = null;
        _password = password;
        _inWorld = false;
        _gameReady = false;
        _gameStream = false;
        _sawCharacters = false;
        _postedWorld = false;
        _seq = 0;
        _huff = new UOHuffman();
        lock (Gate) _fastWalk.Clear();
        var plain = true;
        try {
            for (var attempt = 0; attempt < 2; attempt++) {
                client = null;
                try {
                    client = new TcpClient { NoDelay = true };
                    client.Connect(host, port);
                    if (id != Volatile.Read(ref _runId)) return;
                    SetClient(client, id);
                    var stream = client.GetStream();
                    stream.ReadTimeout = ReadWaitMs;
                    var seed = Seed(client);
                    var seedUint = ReadU32(seed, 0);
                    var crypt = new UOLoginCrypt(version) { Plain = plain };
                    crypt.Initialize(seedUint);
                    if (((uint)version >> 24) >= 6) {
                        var ef = LoginSeed(seedUint, version);
                        stream.Write(ef, 0, ef.Length);
                    }
                    else stream.Write(seed, 0, seed.Length);
                    var login = AccountLogin(account, password);
                    crypt.Encrypt(login);
                    stream.Write(login, 0, login.Length);
                    stream.Flush();
                    _started = true;
                    var retry = false;
                    Pump(ref client, ref stream, crypt, account, id, ref retry);
                    if (retry && attempt == 0) {
                        plain = !plain;
                        continue;
                    }
                    return;
                }
                catch (Exception) when (attempt == 0 && id == Volatile.Read(ref _runId)) {
                    plain = !plain;
                    continue;
                }
                finally {
                    try { client?.Close(); } catch { }
                    lock (Gate) {
                        if (_client == client) _client = null;
                    }
                }
            }
        }
        catch (Exception ex) {
            if (id != Volatile.Read(ref _runId)) return;
            Fail(ex.Message, id);
        }
        finally {
            if (id == Volatile.Read(ref _runId)) _password = null;
        }
    }

    static void Pump(ref TcpClient client, ref NetworkStream stream, UOLoginCrypt crypt, string account, int id, ref bool retryPlain) {
        var acc = new byte[8192];
        var tmp = new byte[4096];
        var accLen = 0;
        while (id == Volatile.Read(ref _runId)) {
            WriteSelect(stream, crypt);
            WritePlay(stream, crypt, client);
            WriteCreate(stream, crypt, client);
            WriteWalk(stream, crypt);
            WriteUse(stream, crypt);
            WriteSay(stream, crypt);
            WriteAck(stream, crypt);
            WriteOut(stream, crypt);
            WritePing(stream, crypt);
            int n;
            try { n = stream.Read(tmp, 0, tmp.Length); }
            catch (Exception ex) when (TimedOut(ex)) { continue; }
            if (n <= 0) {
                if (!_sawCharacters && !_inWorld) {
                    if (accLen == 0 && !_gameStream && !crypt.Plain) {
                        retryPlain = true;
                        return;
                    }
                    Fail("disconnected", id);
                }
                return;
            }
            if (_gameStream) {
                if (!IngestGame(tmp, n, crypt, ref acc, ref accLen, id)) return;
                continue;
            }
            if (accLen + n > acc.Length) {
                var bigger = new byte[Math.Max(acc.Length * 2, accLen + n)];
                Buffer.BlockCopy(acc, 0, bigger, 0, accLen);
                acc = bigger;
            }
            Buffer.BlockCopy(tmp, 0, acc, accLen, n);
            accLen += n;
            var used = 0;
            while (used < accLen) {
                var need = PacketLength(acc.AsSpan(used, accLen - used));
                if (need == 0) break;
                if (need < 0) {
                    Fail($"unknown packet 0x{acc[used]:X2}", id);
                    return;
                }
                var kind = HandlePacket(acc, used, need, account, id, ref client, ref stream, ref crypt);
                if (kind == HandleResult.Stop) return;
                if (kind == HandleResult.Relay) {
                    accLen = 0;
                    used = 0;
                    break;
                }
                if (acc[used] == 0xA9) _sawCharacters = true;
                used += need;
            }
            if (used > 0 && accLen > used) {
                Buffer.BlockCopy(acc, used, acc, 0, accLen - used);
                accLen -= used;
            }
            else if (used > 0) accLen = 0;
        }
    }

    enum HandleResult { Continue, Stop, Relay }

    static HandleResult HandlePacket(byte[] p, int o, int len, string account, int id, ref TcpClient client, ref NetworkStream stream, ref UOLoginCrypt crypt) {
        switch (p[o]) {
            case 0xA8:
                Post(UOLoginPhase.ServersReady, servers: ReadServerNames(p, o, len));
                return HandleResult.Continue;
            case 0x82:
                Fail(RejectReason(len > 1 ? p[o + 1] : (byte)0xFF), id);
                return HandleResult.Stop;
            case 0x8C:
                return Relay(p, o, account, id, ref client, ref stream, ref crypt) ? HandleResult.Relay : HandleResult.Stop;
            case 0xA9: {
                var names = ReadCharacterNames(p, o, len);
                if (names == null) {
                    Fail("bad character list", id);
                    return HandleResult.Stop;
                }
                Post(UOLoginPhase.CharactersReady, characters: names);
                return HandleResult.Continue;
            }
            default:
                return HandleResult.Continue;
        }
    }

    static bool Relay(byte[] p, int o, string account, int id, ref TcpClient client, ref NetworkStream stream, ref UOLoginCrypt crypt) {
        var ip = new IPAddress([p[o + 1], p[o + 2], p[o + 3], p[o + 4]]);
        var port = (p[o + 5] << 8) | p[o + 6];
        var auth = ReadU32(p, o + 7);
        _auth = auth;
        Post(UOLoginPhase.RelayInProgress);
        client.Close();
        var next = new TcpClient { NoDelay = true };
        next.Connect(ip, port);
        if (id != Volatile.Read(ref _runId)) {
            next.Close();
            return false;
        }
        SetClient(next, id);
        client = next;
        stream = next.GetStream();
        stream.ReadTimeout = ReadWaitMs;
        var seed = new[] { (byte)(auth >> 24), (byte)(auth >> 16), (byte)(auth >> 8), (byte)auth };
        stream.Write(seed, 0, seed.Length);
        crypt.BeginGame(auth);
        _huff.Reset();
        _gameStream = true;
        var login = GameLogin(auth, account, _password);
        crypt.EncryptGame(login);
        stream.Write(login, 0, login.Length);
        stream.Flush();
        return true;
    }

    static int PacketLength(Span<byte> s) => UOGamePackets.FrameLength(s);

    static string[] ReadServerNames(byte[] p, int o, int len) {
        if (len < 6) return [];
        var count = (p[o + 4] << 8) | p[o + 5];
        var names = new List<string>();
        var pos = o + 6;
        var end = o + len;
        for (var i = 0; i < count; i++) {
            if (pos + 40 > end) break;
            names.Add(ReadAscii(p, pos + 2, 32));
            pos += 40;
        }
        return names.ToArray();
    }

    static string[] ReadCharacterNames(byte[] p, int o, int len) {
        if (len < 4) return null;
        var count = p[o + 3];
        if (count > 16 || 4 + count * 60 > len) return null;
        var names = new string[count];
        for (var i = 0; i < count; i++)
            names[i] = ReadAscii(p, o + 4 + i * 60, AsciiField);
        return names;
    }

    static string RejectReason(byte reason) => reason switch {
        0 => "invalid",
        1 => "already in use",
        2 => "blocked",
        3 => "bad password",
        _ => "login rejected"
    };

    static void WriteSelect(NetworkStream stream, UOLoginCrypt crypt) {
        int index;
        lock (Gate) {
            if (_select is not int i) return;
            index = i;
            _select = null;
        }
        var pkt = new byte[] { 0xA0, (byte)(index >> 8), (byte)index };
        crypt.Encrypt(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
    }

    static void Post(UOLoginPhase phase, string[] servers = null, string[] characters = null, string message = null) {
        lock (Gate) {
            _have = true;
            _takePhase = phase;
            _takeServers = servers;
            _takeCharacters = characters;
            _takeMessage = message;
        }
    }

    static void Fail(string message, int id) {
        if (id != Volatile.Read(ref _runId)) return;
        LastError = message;
        Post(UOLoginPhase.Failed, message: message);
    }

    static void SetClient(TcpClient client, int id) {
        TcpClient previous;
        lock (Gate) {
            if (id != _runId) {
                client?.Close();
                return;
            }
            previous = _client;
            _client = client;
        }
        if (previous != null && previous != client) previous.Close();
    }

    static bool TimedOut(Exception ex) {
        for (; ex != null; ex = ex.InnerException)
            if (ex is SocketException se && se.SocketErrorCode == SocketError.TimedOut)
                return true;
        return false;
    }

    /// <summary>Four address bytes of the local endpoint, or 0x7F000001 when that address is not IPv4.</summary>
    static byte[] Seed(TcpClient client) {
        uint seed = SeedWhenNotIpv4;
        if (client.Client?.LocalEndPoint is IPEndPoint ep && ep.Address.AddressFamily == AddressFamily.InterNetwork) {
            var b = ep.Address.GetAddressBytes();
            if (b.Length == 4)
                seed = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }
        return [(byte)(seed >> 24), (byte)(seed >> 16), (byte)(seed >> 8), (byte)seed];
    }

    static bool WritePlay(NetworkStream stream, UOLoginCrypt crypt, TcpClient client) {
        int index;
        string name;
        lock (Gate) {
            if (_playIndex is not int i) return false;
            index = i;
            name = _playName ?? "";
            _playIndex = null;
            _playName = null;
        }
        var pkt = UOGamePackets.PlayCharacter(name, index, ClientIp(client));
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
        _inWorld = true;
        _gameReady = true;
        _nextPing = Environment.TickCount + 1000;
        return true;
    }

    static uint ClientIp(TcpClient client) {
        if (client?.Client?.LocalEndPoint is IPEndPoint ep && ep.Address.AddressFamily == AddressFamily.InterNetwork) {
            var b = ep.Address.GetAddressBytes();
            if (b.Length == 4)
                return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }
        return SeedWhenNotIpv4;
    }

    static void WriteWalk(NetworkStream stream, UOLoginCrypt crypt) {
        if (!_inWorld) return;
        var now = Environment.TickCount;
        if (unchecked(now - _nextWalk) < 0) return;
        byte dir;
        uint key;
        lock (Gate) {
            if (_walks.Count == 0 || _fastWalk.Count == 0) return;
            dir = _walks.Dequeue();
            key = _fastWalk.Dequeue();
        }
        var pkt = new byte[7];
        pkt[0] = 0x02;
        pkt[1] = dir;
        pkt[2] = _seq++;
        WriteU32(pkt, 3, key);
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
        _nextWalk = unchecked(now + ((dir & 0x80) != 0 ? 200 : 400));
        _stepRun = (dir & 0x80) != 0;
        _stepUntil = _nextWalk;
        Predict(dir);
    }

    static void WriteCreate(NetworkStream stream, UOLoginCrypt crypt, TcpClient client) {
        if (!_gameStream || _inWorld) return;
        string name;
        bool female;
        int slot;
        lock (Gate) {
            if (!_create) return;
            name = _createName ?? "";
            female = _createFemale;
            slot = _createSlot;
            _create = false;
        }
        var pkt = UOGamePackets.CreateCharacter(name, female, slot, ClientIp(client));
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
    }

    static void WriteUse(NetworkStream stream, UOLoginCrypt crypt) {
        if (!_inWorld) return;
        uint serial;
        lock (Gate) {
            if (_uses.Count == 0) return;
            serial = _uses.Dequeue();
        }
        var pkt = UOGamePackets.Use(serial);
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
    }

    static void WriteSay(NetworkStream stream, UOLoginCrypt crypt) {
        if (!_inWorld) return;
        string text;
        lock (Gate) {
            if (_says.Count == 0) return;
            text = _says.Dequeue();
        }
        var pkt = UOGamePackets.UnicodeSay(text);
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
    }

    static void QueueEnter(uint serial) {
        if (serial == 0) return;
        lock (Gate) {
            if (_entered) return;
            _entered = true;
            var version = _clientVersion;
            if (version >= ClientVersion.CV_200) {
                _out.Enqueue(UOGamePackets.WindowSize(800, 600));
                _out.Enqueue(UOGamePackets.Language("ENU"));
            }
            if (_versionText.Length > 0) _out.Enqueue(UOGamePackets.VersionPacket(_versionText));
            _out.Enqueue(UOGamePackets.Click(serial));
            _out.Enqueue(UOGamePackets.StatusRequest(serial));
            _out.Enqueue(UOGamePackets.SkillsRequest(serial));
            if (version >= ClientVersion.CV_306E) _out.Enqueue(UOGamePackets.ClientType(version));
            if (version >= ClientVersion.CV_305D) _out.Enqueue(UOGamePackets.ViewRange(24));
            _out.Enqueue(UOGamePackets.Use(serial));
        }
    }

    static void WriteOut(NetworkStream stream, UOLoginCrypt crypt) {
        if (!_inWorld) return;
        List<byte[]> batch;
        lock (Gate) {
            if (_out.Count == 0) return;
            batch = new List<byte[]>(_out.Count);
            while (_out.Count > 0) batch.Add(_out.Dequeue());
        }
        foreach (var pkt in batch) {
            crypt.EncryptGame(pkt);
            stream.Write(pkt, 0, pkt.Length);
        }
        stream.Flush();
    }

    static void WritePing(NetworkStream stream, UOLoginCrypt crypt) {
        if (!_inWorld || _nextPing == 0) return;
        var now = Environment.TickCount;
        if (unchecked(now - _nextPing) < 0) return;
        _nextPing = unchecked(now + 1000);
        var pkt = UOGamePackets.Ping(_pingIdx++);
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
    }

    static void SetSkills(List<UOSkillState> rows, bool single) {
        string note = null;
        lock (Gate) {
            foreach (var row in rows) {
                if (row.Name != null) {
                    _skillNames[row.Id] = row.Name;
                    continue;
                }
                var changed = single && _skills.TryGetValue(row.Id, out var prev) && prev.Value != row.Value;
                _skills[row.Id] = row;
                if (!changed) continue;
                var name = _skillNames.TryGetValue(row.Id, out var known) && known.Length > 0 ? known : "skill " + row.Id;
                note = $"{name} {row.Value / 10}.{row.Value % 10}";
            }
            if (note != null) {
                _speech.Add(new UOSpeechState(0, "", note, 0x03B2));
                if (_speech.Count > 12) _speech.RemoveAt(0);
            }
            _revision++;
        }
    }

    static void SetLight(bool personal, byte level) {
        if (level > 0x1E) level = 0x1E;
        lock (Gate) {
            if (personal) _personal = level;
            else _overall = level;
            _revision++;
        }
    }

    static void SetSeason(byte season, byte music) {
        if (season > 4) season = 0;
        lock (Gate) {
            _season = season;
            _music = music;
            _revision++;
        }
    }

    static void WriteAck(NetworkStream stream, UOLoginCrypt crypt) {
        if (!_gameStream) return;
        byte[] pkt;
        lock (Gate) {
            pkt = _ack;
            _ack = null;
        }
        if (pkt == null) return;
        crypt.EncryptGame(pkt);
        stream.Write(pkt, 0, pkt.Length);
        stream.Flush();
    }

    static void Predict(byte dir) {
        int dx = 0, dy = 0;
        switch (dir & 7) {
            case 0: dy = -1; break;
            case 1: dx = 1; dy = -1; break;
            case 2: dx = 1; break;
            case 3: dx = 1; dy = 1; break;
            case 4: dy = 1; break;
            case 5: dx = -1; dy = 1; break;
            case 6: dx = -1; break;
            default: dx = -1; dy = -1; break;
        }
        lock (Gate) {
            _px += dx;
            _py += dy;
            var step = StepHeight;
            if (step != null) _pz = step(_px, _py, _pz);
            _pdir = (byte)(dir & 7);
            if (_player != 0 && _mobiles.TryGetValue(_player, out var m))
                _mobiles[_player] = new UOMobileState(_player, m.Body, _px, _py, _pz, _pdir);
            _revision++;
        }
    }

    static bool IngestGame(byte[] src, int n, UOLoginCrypt crypt, ref byte[] acc, ref int accLen, int id) {
        var raw = src.AsSpan(0, n);
        crypt.DecryptGame(raw);
        var decoded = new byte[n * 8 + 64];
        int produced;
        try { produced = _huff.Decompress(raw, decoded); }
        catch (Exception ex) {
            Fail(ex.Message, id);
            return false;
        }
        if (produced <= 0) return true;
        if (accLen + produced > acc.Length) {
            var bigger = new byte[Math.Max(acc.Length * 2, accLen + produced)];
            Buffer.BlockCopy(acc, 0, bigger, 0, accLen);
            acc = bigger;
        }
        Buffer.BlockCopy(decoded, 0, acc, accLen, produced);
        accLen += produced;
        var used = 0;
        while (used < accLen) {
            var need = PacketLength(acc.AsSpan(used, accLen - used));
            if (need == 0) break;
            if (need < 0) {
                Fail($"unknown packet 0x{acc[used]:X2}", id);
                return false;
            }
            if (HandleGame(acc, used, need, id) == HandleResult.Stop) return false;
            used += need;
        }
        if (used > 0 && accLen > used) {
            Buffer.BlockCopy(acc, used, acc, 0, accLen - used);
            accLen -= used;
        }
        else if (used > 0) accLen = 0;
        return true;
    }

    static HandleResult HandleGame(byte[] p, int o, int len, int id) {
        switch (p[o]) {
            case 0x1B:
                if (UOGamePackets.LoginConfirm(p, o, out var serial, out var body, out var x, out var y, out var z, out var dir)) {
                    SetPlayer(serial, body, x, y, z, dir, post: true);
                    QueueEnter(serial);
                }
                return HandleResult.Continue;
            case 0x55:
                PostWorld();
                QueueEnter(_player);
                return HandleResult.Continue;
            case 0x20:
                if (UOGamePackets.UpdatePlayer(p, o, len, out serial, out body, out x, out y, out z, out dir))
                    SetPlayer(serial, body, x, y, z, dir, post: true);
                return HandleResult.Continue;
            case 0x21:
                if (UOGamePackets.MoveReject(p, o, out x, out y, out dir, out z)) {
                    _seq = p[o + 1];
                    _nextWalk = 0;
                    lock (Gate) _walks.Clear();
                    SetPlayer(_player, _pbody, x, y, z, dir, post: false);
                }
                return HandleResult.Continue;
            case 0x22:
                if (len >= 3) {
                    var noto = (byte)(p[o + 2] & ~0x40);
                    if (noto == 0 || noto >= 8) noto = 1;
                    lock (Gate) { _noto = noto; _revision++; }
                }
                return HandleResult.Continue;
            case 0xD2:
            case 0x77:
                if (UOGamePackets.Moving(p, o, len, out serial, out body, out x, out y, out z, out dir))
                    SetMobile(serial, body, x, y, z, dir);
                return HandleResult.Continue;
            case 0x78: {
                var equip = new List<UOEquipState>();
                if (UOGamePackets.Incoming(p, o, len, out serial, out body, out x, out y, out z, out dir, equip))
                    SetMobile(serial, body, x, y, z, dir, equip);
                return HandleResult.Continue;
            }
            case 0x2E:
                if (UOGamePackets.WornItem(p, o, len, out var worn))
                    SetEquip(worn);
                return HandleResult.Continue;
            case 0x11:
                if (UOGamePackets.Status(p, o, len, out var status))
                    SetStatus(status);
                return HandleResult.Continue;
            case 0xA1:
                if (UOGamePackets.Vital(p, o, len, out serial, out var cur, out var max))
                    SetVital(serial, 0, cur, max);
                return HandleResult.Continue;
            case 0xA2:
                if (UOGamePackets.Vital(p, o, len, out serial, out cur, out max))
                    SetVital(serial, 1, cur, max);
                return HandleResult.Continue;
            case 0xA3:
                if (UOGamePackets.Vital(p, o, len, out serial, out cur, out max))
                    SetVital(serial, 2, cur, max);
                return HandleResult.Continue;
            case 0x1C:
                if (UOGamePackets.AsciiSpeech(p, o, len, out var line, out var probe))
                    NoteSpeech(line, probe);
                return HandleResult.Continue;
            case 0xAE:
                if (UOGamePackets.UnicodeSpeech(p, o, len, out line, out probe))
                    NoteSpeech(line, probe);
                return HandleResult.Continue;
            case 0x88:
                if (UOGamePackets.OpenPaperdoll(p, o, len, out serial, out var title, out var flags))
                    lock (Gate) _paperdolls.Enqueue((serial, title, flags));
                return HandleResult.Continue;
            case 0x24:
                if (UOGamePackets.OpenContainer(p, o, len, out serial, out var gump)) {
                    lock (Gate) {
                        if (gump == 0xFFFF) _spellbooks.Enqueue(serial);
                        else if (gump == 0x0030) _vendors.Enqueue(serial);
                        else _containers.Enqueue((serial, gump));
                    }
                }
                return HandleResult.Continue;
            case 0x25:
                if (UOGamePackets.ContainedItem(p, o, len, out var one))
                    SetContained(one, clear: false);
                return HandleResult.Continue;
            case 0x3C: {
                var rows = new List<UOContainedState>();
                if (UOGamePackets.ContainedItems(p, o, len, rows) && rows.Count > 0)
                    SetContained(rows);
                return HandleResult.Continue;
            }
            case 0x1A:
                if (UOGamePackets.WorldItem(p, o, len, out var iserial, out var graphic, out x, out y, out z))
                    SetItem(iserial, graphic, x, y, z);
                return HandleResult.Continue;
            case 0x1D:
                if (UOGamePackets.DeleteObject(p, o, len, out var gone) && gone != _player)
                    RemoveActor(gone);
                return HandleResult.Continue;
            case 0xF3:
                if (UOGamePackets.WorldObject(p, o, len, out var kind, out iserial, out graphic, out x, out y, out z, out dir)) {
                    if (kind == 1) SetMobile(iserial, graphic, x, y, z, dir);
                    else if (iserial != _player) SetItem(iserial, graphic, x, y, z);
                }
                return HandleResult.Continue;
            case 0x3A: {
                var rows = new List<UOSkillState>();
                if (UOGamePackets.Skills(p, o, len, rows, out var single))
                    SetSkills(rows, single);
                return HandleResult.Continue;
            }
            case 0x4E:
                if (UOGamePackets.PersonalLight(p, o, len, out var who, out var personal) && who == _player)
                    SetLight(true, personal);
                return HandleResult.Continue;
            case 0x4F:
                if (UOGamePackets.OverallLight(p, o, len, out var overall))
                    SetLight(false, overall);
                return HandleResult.Continue;
            case 0xB9:
                if (UOGamePackets.Features(p, o, len, out var feat))
                    lock (Gate) { _features = feat; _revision++; }
                return HandleResult.Continue;
            case 0xBC:
                if (UOGamePackets.Season(p, o, len, out var season, out var music))
                    SetSeason(season, music);
                return HandleResult.Continue;
            case 0xBD:
                lock (Gate) {
                    if (_versionText.Length > 0) _out.Enqueue(UOGamePackets.VersionPacket(_versionText));
                }
                return HandleResult.Continue;
            case 0xC1:
            case 0xCC:
                if (UOGamePackets.ClilocSpeech(p, o, len, out line))
                    NoteSpeech(line, false);
                return HandleResult.Continue;
            case 0xC8:
                if (len > 1) lock (Gate) { _viewRange = p[o + 1]; _revision++; }
                return HandleResult.Continue;
            case 0xB0: {
                if (UOGamePackets.PlainGump(p, o, len, out var open))
                    lock (Gate) _gumps.Enqueue(new UOGumpEvent(open));
                return HandleResult.Continue;
            }
            case 0xDD: {
                if (UOGamePackets.CompressedGump(p, o, len, out var open))
                    lock (Gate) _gumps.Enqueue(new UOGumpEvent(open));
                return HandleResult.Continue;
            }
            case 0xBF:
                // Subcommand 0x0001 is the fast-walk stack. 0x0002 adds a key. 0x0008 is the facet. 0x0004 closes a gump.
                UOGamePackets.FastWalk(p, o, len, _fastWalk);
                if (UOGamePackets.TryFacet(p, o, len, out var facet)) {
                    lock (Gate) _mapIndex = facet;
                    FacetSwitch?.Invoke(facet);
                }
                if (UOGamePackets.TryCloseGump(p, o, len, out var gser, out var gbutton))
                    lock (Gate) _gumps.Enqueue(new UOGumpEvent(gser, gbutton));
                return HandleResult.Continue;
            case 0x73:
                return HandleResult.Continue;
            case 0x6C:
                if (UOGamePackets.TargetCursor(p, o, len, out var tkind, out var tid, out var ttype))
                    lock (Gate) {
                        _target = true;
                        _targetKind = tkind;
                        _targetId = tid;
                        _targetType = ttype;
                        _revision++;
                    }
                return HandleResult.Continue;
            case 0x7C:
                if (UOGamePackets.Menu(p, o, len, out var menu))
                    lock (Gate) _menus.Enqueue(menu);
                return HandleResult.Continue;
            case 0x72:
                if (len >= 2) lock (Gate) { _war = p[o + 1] != 0; _revision++; }
                return HandleResult.Continue;
            case 0x9A:
                if (UOGamePackets.Prompt(p, o, len, out var pid))
                    lock (Gate) { _prompt = true; _promptUnicode = false; _promptId = pid; _revision++; }
                return HandleResult.Continue;
            case 0xC2:
                if (UOGamePackets.Prompt(p, o, len, out pid))
                    lock (Gate) { _prompt = true; _promptUnicode = true; _promptId = pid; _revision++; }
                return HandleResult.Continue;
            case 0xAB:
                if (UOGamePackets.TextEntry(p, o, len, out var entry))
                    lock (Gate) _entries.Enqueue(entry);
                return HandleResult.Continue;
            case 0x07:
                return HandleResult.Continue;
            case 0x27:
                ClearHold();
                if (len >= 2 && p[o + 1] < DenyText.Length) Note(DenyText[p[o + 1]]);
                return HandleResult.Continue;
            case 0x28:
            case 0x29:
                ClearHold();
                return HandleResult.Continue;
            case 0x05:
                return HandleResult.Continue;
            case 0xAA:
                if (UOGamePackets.Attacked(p, o, len, out var attacked))
                    lock (Gate) {
                        _lastAttack = attacked;
                        _out.Enqueue(UOGamePackets.StatusRequest(attacked));
                        _revision++;
                    }
                return HandleResult.Continue;
            case 0x97:
                if (len >= 2 && _gameReady)
                    lock (Gate) { if (_walks.Count < 4) _walks.Enqueue(p[o + 1]); }
                return HandleResult.Continue;
            case 0x98:
                if (UOGamePackets.UpdateName(p, o, len, out var named, out var nm) && nm.Length > 0)
                    lock (Gate) { _names[named] = nm; _revision++; }
                return HandleResult.Continue;
            case 0x54:
                if (UOGamePackets.Sound(p, o, len, out var snd))
                    lock (Gate) _sound = snd;
                return HandleResult.Continue;
            case 0x6D:
                if (UOGamePackets.Music(p, o, len, out var song, out var stop))
                    lock (Gate) _song = stop ? (byte)0 : song;
                return HandleResult.Continue;
            case 0x65:
                if (UOGamePackets.Weather(p, o, len, out var wtype, out var wcount, out var wtemp))
                    lock (Gate) { _weather = wtype; _wcount = wcount; _wtemp = wtemp; }
                return HandleResult.Continue;
            case 0x5B:
                return HandleResult.Continue;
            case 0x23:
                NoteGraphic(p, o, len);
                return HandleResult.Continue;
            case 0x70:
            case 0xC0:
            case 0xC7:
                NoteGraphic(p, o, len);
                return HandleResult.Continue;
            case 0x2F:
                _ = UOGamePackets.Swing(p, o, len, out _, out _);
                return HandleResult.Continue;
            case 0x6E:
                if (UOGamePackets.CharacterAnimation(p, o, len, out var aser, out var aact, out var aframes, out var arep, out var afwd, out var aagain, out var adelay))
                    NoteAnimation(aser, aact, aframes, arep, afwd, aagain, adelay);
                return HandleResult.Continue;
            case 0xE2:
                if (UOGamePackets.NewCharacterAnimation(p, o, len, out serial, out var ntype, out var naction, out var nmode))
                    NoteNewAnim(serial, ntype, naction, nmode);
                return HandleResult.Continue;
            case 0xD1:
            case 0xBE:
                return HandleResult.Continue;
            case 0x2C:
                if (len >= 2 && p[o + 1] != 1) {
                    lock (Gate) {
                        _weather = _wcount = _wtemp = 0;
                        _out.Enqueue(UOGamePackets.WarMode(false));
                    }
                    Note("You are dead.");
                }
                return HandleResult.Continue;
            case 0x74: {
                var prices = new List<uint>();
                if (UOGamePackets.BuyList(p, o, len, out var bag, prices)) {
                    lock (Gate) _shops.Enqueue(new UOShop(VendorOf(bag), true));
                    SetPrices(bag, prices);
                }
                return HandleResult.Continue;
            }
            case 0x9E:
                if (UOGamePackets.SellList(p, o, len, out var vendor))
                    lock (Gate) _shops.Enqueue(new UOShop(vendor, false));
                return HandleResult.Continue;
            case 0x6F:
                if (UOGamePackets.Trade(p, o, len, out var tradeType, out var tradeSerial, out var tradeName)) {
                    if (tradeType == 0) lock (Gate) _trades.Enqueue(new UOTrade(tradeSerial, tradeName, true));
                    else if (tradeType == 1) lock (Gate) _trades.Enqueue(new UOTrade(tradeSerial, "", false));
                }
                return HandleResult.Continue;
            case 0x95:
                if (UOGamePackets.DyeWindow(p, o, len, out var dyeSerial, out var dyeGraphic))
                    lock (Gate) _dyes.Enqueue(new UODye(dyeSerial, dyeGraphic));
                return HandleResult.Continue;
            case 0x3B:
                if (UOGamePackets.VendorClose(p, o, len, out var shopClose))
                    lock (Gate) _shops.Enqueue(new UOShop(shopClose, false, false));
                return HandleResult.Continue;
            case 0x0B:
                if (UOGamePackets.Damage(p, o, len, out var dmgSerial, out var dmgAmount)) {
                    var dmgName = NameOf(dmgSerial);
                    if (dmgName.Length == 0) dmgName = dmgSerial == _player ? "you" : "someone";
                    Note(dmgName + ": " + dmgAmount);
                }
                return HandleResult.Continue;
            case 0x16:
            case 0x17: {
                if (_player != 0) {
                    var marks = new List<(ushort type, bool enabled)>();
                    if (UOGamePackets.HealthBar(p, o, len, out serial, marks))
                        SetHealth(serial, marks);
                }
                return HandleResult.Continue;
            }
            case 0x89: {
                var corpse = new List<UOCorpseEquip>();
                if (UOGamePackets.CorpseEquipment(p, o, len, out serial, corpse))
                    SetCorpse(serial, corpse);
                return HandleResult.Continue;
            }
            case 0xBA:
                if (UOGamePackets.QuestArrow(p, o, len, out var show, out var ax, out var ay, out serial))
                    SetArrow(show, ax, ay, serial);
                return HandleResult.Continue;
            case 0xD6: {
                var strength = 0;
                lock (Gate) {
                    if (_status.TryGetValue(_player, out var st) && st.HasStats) strength = st.Str;
                }
                if (UOGamePackets.MegaCliloc(p, o, len, strength, out serial, out var pname, out var pdata))
                    lock (Gate) { _props[serial] = (pname ?? "", pdata ?? ""); _revision++; }
                return HandleResult.Continue;
            }
            case 0xDF: {
                if (_player == 0) return HandleResult.Continue;
                var buffs = new List<UOBuffState>();
                if (UOGamePackets.BuffDebuff(p, o, len, out var btype, out var bremove, buffs))
                    lock (Gate) {
                        if (bremove) _buffs.Remove(btype);
                        else if (buffs.Count > 0) _buffs[buffs[buffs.Count - 1].Type] = buffs[buffs.Count - 1];
                        _revision++;
                    }
                return HandleResult.Continue;
            }
            case 0xA9: {
                var names = ReadCharacterNames(p, o, len);
                if (names == null) {
                    Fail("bad character list", id);
                    return HandleResult.Stop;
                }
                _sawCharacters = true;
                Post(UOLoginPhase.CharactersReady, characters: names);
                return HandleResult.Continue;
            }
            case 0x82:
                Fail(RejectReason(len > 1 ? p[o + 1] : (byte)0xFF), id);
                return HandleResult.Stop;
            default:
                return HandleResult.Continue;
        }
    }

    static void NoteAnimation(uint serial, ushort action, ushort frames, ushort repeat, bool forward, bool again, byte delay) {
        lock (Gate) {
            if (!_mobiles.ContainsKey(serial)) return;
            if (!_anims.TryGetValue(serial, out var anim)) _anims[serial] = anim = new Anim();
            anim.FromServer = true;
            anim.Raw = action;
            anim.Delay = delay;
            anim.PacketFrames = (byte)frames;
            anim.Repeats = repeat;
            anim.RepeatReset = repeat;
            anim.Repeat = again;
            anim.Forward = forward;
            anim.Frame = forward ? (byte)0 : (byte)frames;
            anim.Next = unchecked(Environment.TickCount + (80 + 80 * (delay + 1)));
        }
    }

    /// <summary>0xE2. Unknown mobiles return. repeat count 1, forward, from the server. Sea serpents of type 1 or 2 repeat.</summary>
    static void NoteNewAnim(uint serial, ushort type, ushort action, byte mode) {
        if (_player == 0) return;
        lock (Gate) {
            if (!_mobiles.TryGetValue(serial, out var mob)) return;
            _newAnims.Enqueue((serial, mob.Body, type, action, mode));
        }
    }

    /// <summary>0x70, and the same layout on 0xC0 and 0xC7. 0x23 is the same call. Types above FixedFrom return.</summary>
    static void NoteGraphic(byte[] p, int o, int len) {
        if (_player == 0) return;
        if (!UOGamePackets.GraphicEffect(p, o, len, out var type, out var source, out var target, out var graphic, out var hue, out var srcX, out var srcY, out var srcZ, out var targetX, out var targetY, out var targetZ, out var speed, out var duration, out var fixedDirection, out var explode))
            return;
        if (type > 3) return;
        lock (Gate) _effects.Enqueue(new UOGraphicEffect(type, source, target, graphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, speed, duration, fixedDirection, explode));
    }

    sealed class Anim {
        public bool FromServer, Forward, Repeat;
        public ushort Raw, Repeats, RepeatReset;
        public byte Delay, PacketFrames, Frame;
        public int Next;
    }

    static void SetPlayer(uint serial, ushort body, int x, int y, int z, byte dir, bool post) {
        lock (Gate) {
            _player = serial;
            _pbody = body;
            _px = x;
            _py = y;
            _pz = z;
            _pdir = (byte)(dir & 7);
            _mobiles[serial] = new UOMobileState(serial, body, x, y, z, _pdir);
            _revision++;
        }
        if (post) PostWorld();
    }

    static void SetMobile(uint serial, ushort body, int x, int y, int z, byte dir, List<UOEquipState> equip = null) {
        if (serial == _player) SetPlayer(serial, body, x, y, z, dir, post: false);
        else lock (Gate) {
            var moved = _mobiles.TryGetValue(serial, out var old) && (old.X != x || old.Y != y);
            _mobiles[serial] = new UOMobileState(serial, body, x, y, z, (byte)(dir & 7));
            if (moved) _moves[serial] = (unchecked(Environment.TickCount + ((dir & 0x80) != 0 ? 200 : 400)), (dir & 0x80) != 0);
            _revision++;
        }
        if (equip != null) ReplaceEquip(serial, equip);
    }

    static void RemoveActor(uint serial) {
        lock (Gate) {
            var changed = _mobiles.Remove(serial);
            if (_items.Remove(serial)) changed = true;
            if (_equip.Remove(serial)) changed = true;
            if (_contents.Remove(serial)) changed = true;
            DropOrder(serial);
            if (_names.Remove(serial)) changed = true;
            if (_anims.Remove(serial)) changed = true;
            if (_moves.Remove(serial)) changed = true;
            if (_status.Remove(serial)) changed = true;
            if (_health.Remove(serial)) changed = true;
            if (_props.Remove(serial)) changed = true;
            if (_corpse.Remove(serial)) changed = true;
            List<uint> drop = null;
            foreach (var kv in _equip)
                if (kv.Value.Parent == serial) (drop ??= new List<uint>()).Add(kv.Key);
            if (drop != null) foreach (var s in drop) { _equip.Remove(s); changed = true; }
            drop = null;
            foreach (var kv in _contents)
                if (kv.Value.Container == serial) (drop ??= new List<uint>()).Add(kv.Key);
            if (drop != null) foreach (var s in drop) { _contents.Remove(s); DropOrder(s); changed = true; }
            drop = null;
            foreach (var kv in _corpse)
                if (kv.Value.Corpse == serial) (drop ??= new List<uint>()).Add(kv.Key);
            if (drop != null) foreach (var s in drop) { _corpse.Remove(s); changed = true; }
            if (changed) _revision++;
        }
    }

    static void ReplaceEquip(uint parent, List<UOEquipState> equip) {
        lock (Gate) {
            List<uint> drop = null;
            foreach (var kv in _equip)
                if (kv.Value.Parent == parent) (drop ??= new List<uint>()).Add(kv.Key);
            if (drop != null) foreach (var s in drop) _equip.Remove(s);
            foreach (var e in equip) {
                _equip[e.Serial] = e;
                _items.Remove(e.Serial);
                _contents.Remove(e.Serial);
                DropOrder(e.Serial);
            }
            _revision++;
        }
    }

    static void SetEquip(UOEquipState item) {
        lock (Gate) {
            _equip[item.Serial] = item;
            _items.Remove(item.Serial);
            _contents.Remove(item.Serial);
            DropOrder(item.Serial);
            _revision++;
        }
    }

    static void SetStatus(UOStatusState status) {
        lock (Gate) {
            _status[status.Serial] = status;
            if (!string.IsNullOrEmpty(status.Name)) _names[status.Serial] = status.Name;
            _revision++;
        }
    }

    static void SetVital(uint serial, int kind, ushort cur, ushort max) {
        lock (Gate) {
            if (!_status.TryGetValue(serial, out var s))
                s = new UOStatusState(serial, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
            s = kind switch {
                1 => s.WithMana(cur, max),
                2 => s.WithStam(cur, max),
                _ => s.WithHits(cur, max),
            };
            _status[serial] = s;
            _revision++;
        }
    }

    static uint VendorOf(uint serial) {
        lock (Gate) {
            if (_equip.TryGetValue(serial, out var worn) && worn.Parent != 0) return worn.Parent;
            if (_contents.TryGetValue(serial, out var row) && _equip.TryGetValue(row.Container, out worn) && worn.Parent != 0)
                return worn.Parent;
            return serial;
        }
    }

    static void ClearHold() {
        lock (Gate) {
            if (_hold == 0) return;
            _hold = 0;
            _revision++;
        }
    }

    static void Note(string text) {
        if (string.IsNullOrEmpty(text)) return;
        lock (Gate) {
            _speech.Add(new UOSpeechState(0, "", text, 0x03B2));
            if (_speech.Count > 12) _speech.RemoveAt(0);
            _revision++;
        }
    }

    static void NoteSpeech(UOSpeechState line, bool probe) {
        if (probe) {
            lock (Gate) _ack = UOGamePackets.SpeechAck();
            return;
        }
        if (string.IsNullOrEmpty(line.Text)) {
            if (line.Serial != 0 && !string.IsNullOrEmpty(line.Name))
                lock (Gate) _names[line.Serial] = line.Name;
            return;
        }
        lock (Gate) {
            _speech.Add(line);
            if (_speech.Count > 12) _speech.RemoveAt(0);
            if (line.Serial != 0 && !string.IsNullOrEmpty(line.Name)) _names[line.Serial] = line.Name;
            _revision++;
        }
    }

    static void SetContained(UOContainedState item, bool clear) {
        lock (Gate) {
            if (!clear && item.Price == 0 && _contents.TryGetValue(item.Serial, out var prev))
                item = item.WithPrice(prev.Price);
            if (clear) {
                List<uint> drop = null;
                foreach (var kv in _contents)
                    if (kv.Value.Container == item.Container) (drop ??= new List<uint>()).Add(kv.Key);
                if (drop != null) foreach (var s in drop) { _contents.Remove(s); DropOrder(s); }
            }
            _contents[item.Serial] = item;
            NoteContent(item.Serial);
            _items.Remove(item.Serial);
            _equip.Remove(item.Serial);
            _revision++;
        }
    }

    static void SetContained(List<UOContainedState> rows) {
        if (rows.Count == 0) return;
        lock (Gate) {
            var container = rows[0].Container;
            List<uint> drop = null;
            foreach (var kv in _contents)
                if (kv.Value.Container == container) (drop ??= new List<uint>()).Add(kv.Key);
            if (drop != null) foreach (var s in drop) { _contents.Remove(s); DropOrder(s); }
            foreach (var item in rows) {
                _contents[item.Serial] = item;
                NoteContent(item.Serial);
                _items.Remove(item.Serial);
                _equip.Remove(item.Serial);
            }
            _revision++;
        }
    }

    static void SetItem(uint serial, ushort graphic, int x, int y, int z) {
        lock (Gate) {
            _items[serial] = new UOItemState(serial, graphic, x, y, z);
            _revision++;
        }
    }

    static void PostWorld() {
        if (_postedWorld) return;
        _postedWorld = true;
        Post(UOLoginPhase.WorldEntered);
    }

    static byte[] LoginSeed(uint seed, ClientVersion version) {
        var packet = new byte[21];
        var v = (uint)version;
        packet[0] = 0xEF;
        WriteU32(packet, 1, seed);
        WriteU32(packet, 5, v >> 24);
        WriteU32(packet, 9, (v >> 16) & 0xFF);
        WriteU32(packet, 13, (v >> 8) & 0xFF);
        WriteU32(packet, 17, v & 0xFF);
        return packet;
    }

    /// <summary>Fixed 0x80 packet: id, 30-byte account, 30-byte password, 0xFF. Packet length 0x3E, no length word.</summary>
    static byte[] AccountLogin(string account, string password) {
        var packet = new byte[1 + AsciiField + AsciiField + 1];
        packet[0] = 0x80;
        WriteAscii(packet, 1, account);
        WriteAscii(packet, 1 + AsciiField, password);
        packet[packet.Length - 1] = 0xFF;
        return packet;
    }

    static byte[] GameLogin(uint auth, string account, string password) {
        var packet = new byte[1 + 4 + AsciiField + AsciiField];
        packet[0] = 0x91;
        WriteU32(packet, 1, auth);
        WriteAscii(packet, 5, account);
        WriteAscii(packet, 5 + AsciiField, password);
        return packet;
    }

    static void WriteAscii(byte[] dest, int offset, string value) {
        if (string.IsNullOrEmpty(value)) return;
        var bytes = Encoding.ASCII.GetBytes(value);
        var n = bytes.Length < AsciiField ? bytes.Length : AsciiField;
        Buffer.BlockCopy(bytes, 0, dest, offset, n);
    }

    static string ReadAscii(byte[] src, int offset, int max) {
        var n = 0;
        while (n < max && src[offset + n] != 0) n++;
        return n == 0 ? "" : Encoding.ASCII.GetString(src, offset, n);
    }

    static uint ReadU32(byte[] src, int offset)
        => ((uint)src[offset] << 24) | ((uint)src[offset + 1] << 16) | ((uint)src[offset + 2] << 8) | src[offset + 3];

    static void WriteU32(byte[] dest, int offset, uint value) {
        dest[offset] = (byte)(value >> 24);
        dest[offset + 1] = (byte)(value >> 16);
        dest[offset + 2] = (byte)(value >> 8);
        dest[offset + 3] = (byte)value;
    }

    static void SetHealth(uint serial, List<(ushort type, bool enabled)> rows) {
        lock (Gate) {
            if (!_mobiles.ContainsKey(serial)) return;
            _health.TryGetValue(serial, out var cur);
            var flags = cur.flags;
            var sa = cur.sa;
            var saClient = _clientVersion >= ClientVersion.CV_7000;
            foreach (var row in rows) {
                if (row.type == 1) {
                    if (saClient) sa = row.enabled;
                    else if (row.enabled) flags |= 0x04;
                    else flags = (byte)(flags & ~0x04);
                }
                else if (row.type == 2) {
                    if (row.enabled) flags |= 0x08;
                    else flags = (byte)(flags & ~0x08);
                }
            }
            _health[serial] = (flags, sa);
            _revision++;
        }
    }

    static void SetArrow(bool display, ushort x, ushort y, uint serial) {
        lock (Gate) {
            if (!display) _arrows.Remove(serial);
            else _arrows[serial] = (x, y);
            _revision++;
        }
    }

    static void SetCorpse(uint serial, List<UOCorpseEquip> rows) {
        if (rows == null || rows.Count == 0) return;
        lock (Gate) {
            if (!_items.TryGetValue(serial, out var it) || it.Graphic != 0x2006) return;
            foreach (var row in rows) _corpse[row.Serial] = row;
            _revision++;
        }
    }

    static void SetPrices(uint bag, List<uint> prices) {
        if (prices == null || prices.Count == 0) return;
        lock (Gate) {
            if (!_equip.TryGetValue(bag, out var box)) return;
            if (box.Layer != 0x1A && box.Layer != 0x1B) return;
            if (!_mobiles.ContainsKey(box.Parent)) return;
            var rows = new List<UOContainedState>();
            foreach (var it in _contents.Values)
                if (it.Container == bag) rows.Add(it);
            if (rows.Count == 0) return;
            rows.Sort((a, b) => OrderOf(a.Serial).CompareTo(OrderOf(b.Serial)));
            if (box.Graphic == 0x2AF8) SortByX(rows);
            else rows.Reverse();
            var n = prices.Count < rows.Count ? prices.Count : rows.Count;
            for (var i = 0; i < n; i++)
                _contents[rows[i].Serial] = rows[i].WithPrice(prices[i]);
            _revision++;
        }
    }

    static int OrderOf(uint serial) => _order.TryGetValue(serial, out var n) ? n : 0;

    static void NoteContent(uint serial) => _order[serial] = _orderNext++;

    static void DropOrder(uint serial) => _order.Remove(serial);

    static void SortByX(List<UOContainedState> rows) {
        for (var i = 1; i < rows.Count; i++) {
            var key = rows[i];
            var j = i - 1;
            while (j >= 0 && rows[j].X > key.X) {
                rows[j + 1] = rows[j];
                j--;
            }
            rows[j + 1] = key;
        }
    }
}
