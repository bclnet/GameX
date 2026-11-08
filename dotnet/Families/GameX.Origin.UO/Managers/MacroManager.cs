// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MacroManager. Key is the SDL keycode number (letters are ASCII). SDL is not referenced.
/// Profile path is not ported. The macro list stays in memory.
/// </summary>
public sealed class MacroManager : LinkedObject {
    const int Spell1 = 64;
    const int Spell2 = 17;
    const int Spell3 = 10;
    const int Spell4 = 6;
    const int Spell5 = 8;
    const int Spell6 = 16;
    const int Spell7 = 30;
    const int WaitForTargetDelay = 5000;
    const int MinViewRange = 5;
    const int MaxViewRange = 24;
    const ushort SpeechHue = 0x02B2;
    const ushort WhisperHue = 0x0033;
    const ushort EmoteHue = 0x0021;
    const ushort PrintHue = 946;
    const byte Regular = 0;
    const byte Emote = 2;
    const byte Whisper = 8;
    const byte Yell = 9;
    const ushort BandageGraphic = 0x0E21;
    const ushort CorpseGraphic = 0x2006;
    const ushort CorpseGump = 0x0009;

    public static readonly string[] MacroNames = Enum.GetNames(typeof(MacroType));

    readonly uint[] _itemsInHand = new uint[2];
    readonly World _world;
    readonly List<UOMobileState> _mobiles = new List<UOMobileState>();
    readonly List<UOItemState> _items = new List<UOItemState>();
    readonly List<UOEquipState> _equip = new List<UOEquipState>();
    readonly List<UOContainedState> _rows = new List<UOContainedState>();
    readonly List<(uint serial, string name)> _party = new List<(uint serial, string name)>();
    readonly LastTarget _last = new LastTarget();

    readonly byte[] _skillTable = {
        1, 2, 35, 4, 6, 12,
        14, 15, 16, 19, 21, 56,
        23, 3, 46, 9, 30, 22,
        48, 32, 33, 47, 36, 38
    };

    readonly int[] _spellsCountTable = {
        Spell1, Spell2, Spell3, Spell4, Spell5, Spell6, Spell7
    };

    MacroObject _lastMacro;
    long _nextTimer;
    uint _lastObject;
    uint _selected;
    int _lastSkill = 1;
    int _lastSpell = 1;
    byte _primary = 11;
    byte _secondary = 5;
    byte _range;
    bool _rangeSet;

    // Profile path is not ported. These are the flags the macros toggle.
    public bool CircleOfTransparency { get; private set; }
    public bool AlwaysRun { get; private set; }
    public bool NoColorObjectsOutOfRange { get; private set; }
    public bool UseNewTargetSystem { get; private set; }
    public bool DrawRoofs { get; private set; } = true;
    public bool TreeToStumps { get; private set; }
    public bool HideVegetation { get; private set; }
    public bool EnableCaveBorder { get; private set; }
    public string SystemLine { get; private set; }
    public ushort SystemHue { get; private set; }

    public MacroManager(World world) { _world = world; }

    public long WaitForTargetTimer { get; set; }

    public bool WaitingBandageTarget { get; set; }

    public void Load() {
        // Profile path is not ported.
        Clear();
        CreateDefaultMacros();
    }

    public void Save() {
        // Profile path is not ported.
        List<Macro> list = GetAllMacros();
        using (XmlTextWriter xml = new XmlTextWriter(Stream.Null, Encoding.UTF8) {
            Formatting = Formatting.Indented,
            IndentChar = '\t',
            Indentation = 1
        }) {
            xml.WriteStartDocument(true);
            xml.WriteStartElement("macros");
            foreach (Macro macro in list)
                macro.Save(xml);
            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
    }

    void CreateDefaultMacros() {
        PushToBack(new Macro("Paperdoll", 'p', true, false, false) {
            Items = new MacroObject(MacroType.Open, MacroSubType.Paperdoll) { SubMenuType = 1 }
        });
        PushToBack(new Macro("Options", 'o', true, false, false) {
            Items = new MacroObject(MacroType.Open, MacroSubType.Configuration) { SubMenuType = 1 }
        });
        PushToBack(new Macro("Journal", 'j', true, false, false) {
            Items = new MacroObject(MacroType.Open, MacroSubType.Journal) { SubMenuType = 1 }
        });
        PushToBack(new Macro("Backpack", 'i', true, false, false) {
            Items = new MacroObject(MacroType.Open, MacroSubType.Backpack) { SubMenuType = 1 }
        });
        PushToBack(new Macro("Radar", 'r', true, false, false) {
            Items = new MacroObject(MacroType.Open, MacroSubType.Overview) { SubMenuType = 1 }
        });
        PushToBack(new Macro("Bow", 'b', false, true, false) {
            Items = new MacroObject(MacroType.Bow, MacroSubType.MSC_NONE) { SubMenuType = 0 }
        });
        PushToBack(new Macro("Salute", 's', false, true, false) {
            Items = new MacroObject(MacroType.Salute, MacroSubType.MSC_NONE) { SubMenuType = 0 }
        });
    }

    public List<Macro> GetAllMacros() {
        Macro m = (Macro)Items;
        while (m?.Previous != null)
            m = (Macro)m.Previous;
        List<Macro> macros = new List<Macro>();
        while (true) {
            if (m != null) macros.Add(m);
            else break;
            m = (Macro)m.Next;
        }
        return macros;
    }

    public Macro FindMacro(int key, bool alt, bool ctrl, bool shift) {
        Macro obj = (Macro)Items;
        while (obj != null) {
            if (obj.Key == key && obj.Alt == alt && obj.Ctrl == ctrl && obj.Shift == shift)
                break;
            obj = (Macro)obj.Next;
        }
        return obj;
    }

    public Macro FindMacro(MouseButtonType button, bool alt, bool ctrl, bool shift) {
        Macro obj = (Macro)Items;
        while (obj != null) {
            if (obj.MouseButton == button && obj.Alt == alt && obj.Ctrl == ctrl && obj.Shift == shift)
                break;
            obj = (Macro)obj.Next;
        }
        return obj;
    }

    public Macro FindMacro(bool wheelUp, bool alt, bool ctrl, bool shift) {
        Macro obj = (Macro)Items;
        while (obj != null) {
            if (obj.WheelScroll == true && obj.WheelUp == wheelUp && obj.Alt == alt && obj.Ctrl == ctrl && obj.Shift == shift)
                break;
            obj = (Macro)obj.Next;
        }
        return obj;
    }

    public Macro FindMacro(string name) {
        Macro obj = (Macro)Items;
        while (obj != null) {
            if (obj.Name == name) break;
            obj = (Macro)obj.Next;
        }
        return obj;
    }

    public void SetMacroToExecute(MacroObject macro) { _lastMacro = macro; }

    public void Update() {
        while (_lastMacro != null) {
            switch (Process()) {
                case 2:
                    _lastMacro = null;
                    break;
                case 1: return;
                case 0:
                    _lastMacro = (MacroObject)_lastMacro?.Next;
                    break;
            }
        }
    }

    int Process() {
        int result;
        if (_lastMacro == null) result = 2;
        else if (_nextTimer <= Time.Ticks) result = Process(_lastMacro);
        else result = 1;
        return result;
    }

    int Process(MacroObject macro) {
        if (macro == null) return 0;
        int result = 0;
        switch (macro.Code) {
            case MacroType.Say:
            case MacroType.Emote:
            case MacroType.Whisper:
            case MacroType.Yell:
                string text = ((MacroObjectString)macro).Text;
                if (!string.IsNullOrEmpty(text)) {
                    byte type = Regular;
                    ushort hue = SpeechHue;
                    switch (macro.Code) {
                        case MacroType.Emote:
                            text = "*" + text + "*";
                            type = Emote;
                            hue = EmoteHue;
                            break;
                        case MacroType.Whisper:
                            type = Whisper;
                            hue = WhisperHue;
                            break;
                        case MacroType.Yell:
                            type = Yell;
                            break;
                    }
                    Say(text, hue, type);
                }
                break;

            case MacroType.RazorMacro:
                // plugin is not ported
                break;

            case MacroType.Walk:
                byte dt = (byte)Direction.Up;
                if (macro.SubCode != MacroSubType.NW) {
                    dt = (byte)((int)macro.SubCode - 2);
                    if (dt > 7) dt = 0;
                }
                // Pathfinder.Active is on LoginScene. The walk sequence lives on UOLoginNet.
                UOLoginNet.Walk(dt);
                break;

            case MacroType.WarPeace:
                UOLoginNet.Send(OutgoingPackets.Send_ChangeWarMode(!UOLoginNet.InWarMode));
                break;

            case MacroType.Paste:
                // clipboard is not ported
                break;

            case MacroType.Open:
            case MacroType.Close:
            case MacroType.Minimize:
            case MacroType.Maximize:
                OpenClose(macro);
                break;

            case MacroType.OpenDoor:
                UOLoginNet.Send(OutgoingPackets.Send_OpenDoor());
                break;

            case MacroType.UseSkill: {
                int skill = (int)macro.SubCode - (int)MacroSubType.Anatomy;
                if (skill >= 0 && skill < 24) {
                    skill = _skillTable[skill];
                    if (skill != 0xFF) {
                        _lastSkill = skill;
                        UOLoginNet.Send(OutgoingPackets.Send_UseSkill(skill));
                    }
                }
                break;
            }

            case MacroType.LastSkill:
                UOLoginNet.Send(OutgoingPackets.Send_UseSkill(_lastSkill));
                break;

            case MacroType.CastSpell: {
                int spell = (int)macro.SubCode - (int)MacroSubType.Clumsy + 1;
                if (spell > 0 && spell <= 151) {
                    int totalCount = 0;
                    int spellType;
                    for (spellType = 0; spellType < 7; spellType++) {
                        totalCount += _spellsCountTable[spellType];
                        if (spell <= totalCount) break;
                    }
                    if (spellType < 7) {
                        spell -= totalCount - _spellsCountTable[spellType];
                        spell += spellType * 100;
                        if (spellType > 2) {
                            spell += 100;
                            if (spellType == 6) spell -= 23;
                        }
                        _lastSpell = spell;
                        UOLoginNet.Send(OutgoingPackets.Send_CastSpell(Version, spell));
                    }
                }
                break;
            }

            case MacroType.LastSpell:
                UOLoginNet.Send(OutgoingPackets.Send_CastSpell(Version, _lastSpell));
                break;

            case MacroType.Bow:
            case MacroType.Salute: {
                int index = (int)macro.Code - (int)MacroType.Bow;
                UOLoginNet.Send(OutgoingPackets.Send_EmoteAction(index == 0 ? "bow" : "salute"));
                break;
            }

            case MacroType.QuitGame:
                UOLoginNet.RequestQuit();
                break;

            case MacroType.AllNames:
                AllNames();
                break;

            case MacroType.LastObject:
                if (StillThere(_lastObject)) DoubleClick(_lastObject);
                break;

            case MacroType.UseItemInHand: {
                uint hand = EquipSerial(1);
                if (hand == 0) hand = EquipSerial(2);
                if (hand != 0) DoubleClick(hand);
                break;
            }

            case MacroType.LastTarget:
                if (UOLoginNet.HasTarget) {
                    if (_last.IsEntity) UOLoginNet.TargetSerial(_last.Serial);
                    else if (_last.X != 0xFFFF) UOLoginNet.TargetGround(_last.X, _last.Y, _last.Z);
                    WaitForTargetTimer = 0;
                }
                else if (WaitForTargetTimer < Time.Ticks) WaitForTargetTimer = 0;
                else result = 1;
                break;

            case MacroType.TargetSelf:
                if (UOLoginNet.HasTarget) {
                    UOLoginNet.TargetSerial(UOLoginNet.PlayerSerial);
                    WaitForTargetTimer = 0;
                }
                else if (WaitForTargetTimer < Time.Ticks) WaitForTargetTimer = 0;
                else result = 1;
                break;

            case MacroType.ArmDisarm:
                ArmDisarm(macro);
                break;

            case MacroType.WaitForTarget:
                if (WaitForTargetTimer == 0)
                    WaitForTargetTimer = (long)Time.Ticks + WaitForTargetDelay;
                if (UOLoginNet.HasTarget || WaitForTargetTimer < Time.Ticks) WaitForTargetTimer = 0;
                else result = 1;
                break;

            case MacroType.TargetNext: {
                uint sel = FindNext(ScanTypeObject.Mobiles, _last.Serial, false);
                if (SerialHelper.IsValid(sel)) _last.SetEntity(sel);
                break;
            }

            case MacroType.AttackLast:
                if (_last.IsEntity) Attack(_last.Serial);
                break;

            case MacroType.Delay: {
                string str = ((MacroObjectString)macro).Text;
                if (!string.IsNullOrEmpty(str) && int.TryParse(str, out int rr))
                    _nextTimer = (long)Time.Ticks + rr;
                break;
            }

            case MacroType.CircleTrans:
                CircleOfTransparency = !CircleOfTransparency;
                break;

            case MacroType.CloseGump:
                CloseGumps();
                break;

            case MacroType.AlwaysRun:
                AlwaysRun = !AlwaysRun;
                Note(AlwaysRun ? "Always run is now on." : "Always run is now off.");
                break;

            case MacroType.SaveDesktop:
                // Profile path is not ported.
                break;

            case MacroType.EnableRangeColor:
                NoColorObjectsOutOfRange = true;
                break;

            case MacroType.DisableRangeColor:
                NoColorObjectsOutOfRange = false;
                break;

            case MacroType.ToggleRangeColor:
                NoColorObjectsOutOfRange = !NoColorObjectsOutOfRange;
                break;

            case MacroType.AttackSelectedTarget:
                if (SerialHelper.IsMobile(_selected)) Attack(_selected);
                break;

            case MacroType.UseSelectedTarget:
                if (SerialHelper.IsValid(_selected)) DoubleClick(_selected);
                break;

            case MacroType.CurrentTarget:
                if (_selected != 0) {
                    if (WaitForTargetTimer == 0)
                        WaitForTargetTimer = (long)Time.Ticks + WaitForTargetDelay;
                    if (UOLoginNet.HasTarget) {
                        UOLoginNet.TargetSerial(_selected);
                        WaitForTargetTimer = 0;
                    }
                    else if (WaitForTargetTimer < Time.Ticks) WaitForTargetTimer = 0;
                    else result = 1;
                }
                break;

            case MacroType.TargetSystemOnOff:
                UseNewTargetSystem = !UseNewTargetSystem;
                Note(UseNewTargetSystem ? "Target System: On" : "Target System: Off");
                break;

            case MacroType.BandageSelf:
            case MacroType.BandageTarget:
                result = Bandage(macro);
                break;

            case MacroType.SetUpdateRange:
            case MacroType.ModifyUpdateRange:
                if (macro is MacroObjectString moss && !string.IsNullOrEmpty(moss.Text) && byte.TryParse(moss.Text, out byte res)) {
                    if (res < MinViewRange) res = MinViewRange;
                    else if (res > MaxViewRange) res = MaxViewRange;
                    SetView(res);
                }
                break;

            case MacroType.IncreaseUpdateRange: {
                int n = View() + 1;
                if (n > MaxViewRange) n = MaxViewRange;
                SetView((byte)n);
                break;
            }

            case MacroType.DecreaseUpdateRange: {
                int n = View() - 1;
                if (n < MinViewRange) n = MinViewRange;
                SetView((byte)n);
                break;
            }

            case MacroType.MaxUpdateRange:
                SetView(MaxViewRange);
                break;

            case MacroType.MinUpdateRange:
                SetView(MinViewRange);
                break;

            case MacroType.DefaultUpdateRange:
                SetView(MaxViewRange);
                break;

            case MacroType.SelectNext:
            case MacroType.SelectPrevious:
            case MacroType.SelectNearest: {
                ScanModeObject scanRange = (ScanModeObject)((int)macro.Code - (int)MacroType.SelectNext);
                ScanTypeObject scantype = (ScanTypeObject)((int)macro.SubCode - (int)MacroSubType.Hostile);
                if (scanRange == ScanModeObject.Nearest) SetLastTarget(FindNearest(scantype));
                else SetLastTarget(FindNext(scantype, _selected, scanRange == ScanModeObject.Previous));
                break;
            }

            case MacroType.ToggleBuffIconGump: {
                BuffGump buff = UIManager.GetGump<BuffGump>();
                if (buff != null) buff.Dispose();
                else UIManager.Add(new BuffGump(_world));
                break;
            }

            case MacroType.InvokeVirtue: {
                byte id = (byte)((int)macro.SubCode - (int)MacroSubType.Honor + 1);
                UOLoginNet.Send(OutgoingPackets.Send_InvokeVirtueRequest(id));
                break;
            }

            case MacroType.PrimaryAbility:
                UseAbility(true);
                break;

            case MacroType.SecondaryAbility:
                UseAbility(false);
                break;

            case MacroType.ToggleGargoyleFly:
                // Race is not stored. These bodies are the gargoyle split.
                if (Gargoyle(UOLoginNet.PlayerBody))
                    UOLoginNet.Send(OutgoingPackets.Send_ToggleGargoyleFlying());
                break;

            case MacroType.EquipLastWeapon:
                UOLoginNet.Send(OutgoingPackets.Send_EquipLastWeapon(UOLoginNet.PlayerSerial));
                break;

            case MacroType.KillGumpOpen:
                // TODO:
                break;

            case MacroType.Zoom:
                // camera zoom is not in this tree
                break;

            case MacroType.ToggleChatVisibility:
                // chat visibility is not in this tree
                break;

            case MacroType.Aura:
                // hold to draw
                break;

            case MacroType.AuraOnOff:
                // AuraManager is not in this tree
                break;

            case MacroType.Grab:
                Note("Target an Item to grab it.");
                // target cursor is not in this tree
                break;

            case MacroType.SetGrabBag:
                Note("Target the container to Grab items into.");
                // target cursor is not in this tree
                break;

            case MacroType.NamesOnOff:
                // overhead names are not in this tree
                break;

            case MacroType.UsePotion: {
                int kind = (int)macro.SubCode - (int)MacroSubType.ConfusionBlastPotion;
                uint potion = FindGraphic(UOLoginNet.PlayerBackpack, (ushort)(0x0F06 + kind));
                if (potion != 0) DoubleClick(potion);
                break;
            }

            case MacroType.UseObject:
                UseObject(macro);
                break;

            case MacroType.CloseAllHealthBars:
                // health bars are not gumps
                break;

            case MacroType.CloseInactiveHealthBars:
                // health bars are not gumps
                break;

            case MacroType.CloseCorpses:
                CloseCorpses();
                break;

            case MacroType.ToggleDrawRoofs:
                DrawRoofs = !DrawRoofs;
                break;

            case MacroType.ToggleTreeStumps:
                // StaticFilters is not in this tree
                TreeToStumps = !TreeToStumps;
                break;

            case MacroType.ToggleVegetation:
                HideVegetation = !HideVegetation;
                break;

            case MacroType.ToggleCaveTiles:
                // StaticFilters is not in this tree
                EnableCaveBorder = !EnableCaveBorder;
                break;

            case MacroType.LookAtMouse:
                // handle in gamesceneinput
                break;

            case MacroType.UseCounterBarSlot: {
                string slotString = ((MacroObjectString)macro).Text;
                if (string.IsNullOrEmpty(slotString) || !ushort.TryParse(slotString, out _))
                    Note(string.Format("Counter Bar: Slot '{0}' is not a valid number", slotString));
                // UseSlot is not public on CounterBarGump
                break;
            }
        }
        return result;
    }

    void OpenClose(MacroObject macro) {
        switch (macro.Code) {
            case MacroType.Open:
                switch (macro.SubCode) {
                    case MacroSubType.Configuration:
                        // options gump is not in this tree
                        break;
                    case MacroSubType.Paperdoll:
                        OpenPaperdoll(UOLoginNet.PlayerSerial);
                        break;
                    case MacroSubType.Status:
                        Show(UIManager.GetGump<StatusGump>(), () => new StatusGump(_world));
                        break;
                    case MacroSubType.Journal:
                        Show(UIManager.GetGump<JournalGump>(), () => new JournalGump(_world));
                        break;
                    case MacroSubType.Skills: {
                        SkillsGump skills = UIManager.GetGump<SkillsGump>();
                        if (skills != null) Bring(skills);
                        else {
                            UOLoginNet.Send(OutgoingPackets.Send_SkillsRequest(UOLoginNet.PlayerSerial));
                            UIManager.Add(new SkillsGump(_world));
                        }
                        break;
                    }
                    case MacroSubType.MageSpellbook:
                    case MacroSubType.NecroSpellbook:
                    case MacroSubType.PaladinSpellbook:
                    case MacroSubType.BushidoSpellbook:
                    case MacroSubType.NinjitsuSpellbook:
                    case MacroSubType.SpellWeavingSpellbook:
                    case MacroSubType.MysticismSpellbook:
                        UOLoginNet.Send(OutgoingPackets.Send_OpenSpellBook(BookType(macro.SubCode)));
                        break;
                    case MacroSubType.Chat:
                        Show(UIManager.GetGump<ChatGump>(), () => new ChatGump(_world));
                        break;
                    case MacroSubType.Backpack: {
                        uint pack = UOLoginNet.PlayerBackpack;
                        if (pack == 0) break;
                        ContainerGump open = UIManager.GetGump<ContainerGump>(pack);
                        if (open == null) DoubleClick(pack);
                        else Bring(open);
                        break;
                    }
                    case MacroSubType.Overview:
                        Show(UIManager.GetGump<MiniMapGump>(), () => new MiniMapGump(_world));
                        break;
                    case MacroSubType.WorldMap:
                        Show(UIManager.GetGump<WorldMapGump>(), () => new WorldMapGump(_world));
                        break;
                    case MacroSubType.Mail:
                        // Macro 'Mail' not implemented
                        break;
                    case MacroSubType.PartyManifest: {
                        PartyGump party = UIManager.GetGump<PartyGump>();
                        if (party == null) {
                            var s = Mouse.Current;
                            int bw = s.BufW > 0 ? s.BufW : 640;
                            int bh = s.BufH > 0 ? s.BufH : 480;
                            UIManager.Add(new PartyGump(_world) { X = bw / 2 - 272, Y = bh / 2 - 240 });
                        }
                        else Bring(party);
                        break;
                    }
                    case MacroSubType.Guild:
                        UOLoginNet.Send(OutgoingPackets.Send_GuildMenuRequest(UOLoginNet.PlayerSerial));
                        break;
                    case MacroSubType.QuestLog:
                        UOLoginNet.Send(OutgoingPackets.Send_QuestMenuRequest(UOLoginNet.PlayerSerial));
                        break;
                    case MacroSubType.PartyChat:
                    case MacroSubType.CombatBook:
                    case MacroSubType.RacialAbilitiesBook:
                    case MacroSubType.BardSpellbook:
                        // Macro not implemented
                        break;
                }
                break;

            case MacroType.Close:
            case MacroType.Minimize:
            case MacroType.Maximize:
                switch (macro.SubCode) {
                    case MacroSubType.WorldMap:
                        if (macro.Code == MacroType.Close)
                            UIManager.GetGump<MiniMapGump>()?.Dispose();
                        break;
                    case MacroSubType.Configuration:
                        if (macro.Code == MacroType.Close) {
                            // options gump is not in this tree
                        }
                        break;
                    case MacroSubType.Paperdoll: {
                        PaperdollGump paperdoll = UIManager.GetGump<PaperdollGump>(UOLoginNet.PlayerSerial);
                        if (paperdoll == null) break;
                        if (macro.Code == MacroType.Close) paperdoll.Dispose();
                        else if (macro.Code == MacroType.Minimize) paperdoll.IsMinimized = true;
                        else if (macro.Code == MacroType.Maximize) paperdoll.IsMinimized = false;
                        break;
                    }
                    case MacroSubType.Status: {
                        StatusGump status = UIManager.GetGump<StatusGump>();
                        if (macro.Code == MacroType.Close) status?.Dispose();
                        else if (macro.Code == MacroType.Maximize && status != null) Bring(status);
                        // health bars are not gumps
                        break;
                    }
                    case MacroSubType.Journal: {
                        JournalGump journal = UIManager.GetGump<JournalGump>();
                        if (macro.Code == MacroType.Close) journal?.Dispose();
                        // minimized flag is not public
                        break;
                    }
                    case MacroSubType.Skills: {
                        SkillsGump skillgump = UIManager.GetGump<SkillsGump>();
                        if (macro.Code == MacroType.Close) skillgump?.Dispose();
                        // minimized flag is not public
                        break;
                    }
                    case MacroSubType.MageSpellbook:
                    case MacroSubType.NecroSpellbook:
                    case MacroSubType.PaladinSpellbook:
                    case MacroSubType.BushidoSpellbook:
                    case MacroSubType.NinjitsuSpellbook:
                    case MacroSubType.SpellWeavingSpellbook:
                    case MacroSubType.MysticismSpellbook: {
                        SpellbookGump spellbook = UIManager.GetGump<SpellbookGump>();
                        if (macro.Code == MacroType.Close) spellbook?.Dispose();
                        // minimized flag is not public
                        break;
                    }
                    case MacroSubType.Overview:
                        if (macro.Code == MacroType.Close)
                            UIManager.GetGump<MiniMapGump>()?.Dispose();
                        // minimap size toggle is not public
                        break;
                    case MacroSubType.Backpack: {
                        uint pack = UOLoginNet.PlayerBackpack;
                        if (pack == 0) break;
                        ContainerGump backpack = UIManager.GetGump<ContainerGump>(pack);
                        if (macro.Code == MacroType.Close) backpack?.Dispose();
                        // minimized flag is not public
                        break;
                    }
                    case MacroSubType.Mail:
                        // Macro 'Mail' not implemented
                        break;
                    case MacroSubType.PartyManifest:
                        if (macro.Code == MacroType.Close)
                            UIManager.GetGump<PartyGump>()?.Dispose();
                        break;
                    case MacroSubType.PartyChat:
                    case MacroSubType.CombatBook:
                    case MacroSubType.RacialAbilitiesBook:
                    case MacroSubType.BardSpellbook:
                        // Macro not implemented
                        break;
                }
                break;
        }
    }

    void ArmDisarm(MacroObject macro) {
        int handIndex = 1 - ((int)macro.SubCode - (int)MacroSubType.LeftHand);
        if (handIndex < 0 || handIndex > 1 || UOLoginNet.Holding) return;
        byte layer = (byte)(1 + handIndex);
        if (_itemsInHand[handIndex] != 0) {
            uint serial = _itemsInHand[handIndex];
            UOLoginNet.Send(OutgoingPackets.Send_PickUpRequest(serial, 1));
            UOLoginNet.Send(OutgoingPackets.Send_EquipRequest(serial, layer, UOLoginNet.PlayerSerial));
            _itemsInHand[handIndex] = 0;
            _nextTimer = (long)Time.Ticks + 1000;
            return;
        }
        uint backpack = UOLoginNet.PlayerBackpack;
        if (backpack == 0) return;
        uint item = EquipSerial(layer);
        if (item == 0) return;
        _itemsInHand[handIndex] = item;
        UOLoginNet.Send(OutgoingPackets.Send_PickUpRequest(item, 1));
        if (Version >= ClientVersion.CV_6017)
            UOLoginNet.Send(OutgoingPackets.Send_DropRequest(item, 0xFFFF, 0xFFFF, 0, 0, backpack));
        else
            UOLoginNet.Send(OutgoingPackets.Send_DropRequest_Old(item, 0xFFFF, 0xFFFF, 0, backpack));
        _nextTimer = (long)Time.Ticks + 1000;
    }

    int Bandage(MacroObject macro) {
        // BandageSelfOld is a profile flag and is not ported.
        if (Version < ClientVersion.CV_5020) {
            if (WaitingBandageTarget) {
                if (WaitForTargetTimer == 0)
                    WaitForTargetTimer = (long)Time.Ticks + WaitForTargetDelay;
                if (UOLoginNet.HasTarget) {
                    if (macro.Code == MacroType.BandageSelf)
                        UOLoginNet.TargetSerial(UOLoginNet.PlayerSerial);
                    else if (_last.IsEntity)
                        UOLoginNet.TargetSerial(_last.Serial);
                    WaitingBandageTarget = false;
                    WaitForTargetTimer = 0;
                }
                else if (WaitForTargetTimer < Time.Ticks) {
                    WaitingBandageTarget = false;
                    WaitForTargetTimer = 0;
                }
                else return 1;
            }
            else {
                uint bandage = FindBandage();
                if (bandage != 0) {
                    WaitingBandageTarget = true;
                    DoubleClick(bandage);
                    return 1;
                }
            }
            return 0;
        }
        uint item = FindBandage();
        if (item == 0) return 0;
        if (macro.Code == MacroType.BandageSelf)
            UOLoginNet.Send(OutgoingPackets.Send_TargetSelectedObject(item, UOLoginNet.PlayerSerial));
        else if (SerialHelper.IsMobile(_selected))
            UOLoginNet.Send(OutgoingPackets.Send_TargetSelectedObject(item, _selected));
        return 0;
    }

    void UseObject(MacroObject macro) {
        uint obj = 0;
        switch (macro.SubCode) {
            case MacroSubType.BestHealPotion:
                obj = FindPreferredCliloc(1041330, 1041329, 1041328);
                break;
            case MacroSubType.BestCurePotion:
                obj = FindPreferredCliloc(1041317, 1041316, 1041315);
                break;
            case MacroSubType.BestRefreshPotion:
                obj = FindPreferredCliloc(1041327, 1041326);
                break;
            case MacroSubType.BestStrengthPotion:
                obj = FindPreferredCliloc(1041321, 1041320);
                break;
            case MacroSubType.BestAgiPotion:
                obj = FindPreferredCliloc(1041319, 1041318);
                break;
            case MacroSubType.BestExplosionPotion:
                obj = FindPreferredCliloc(1041333, 1041332, 1041331);
                break;
            case MacroSubType.BestConflagPotion:
                obj = FindPreferredCliloc(1072098, 1072095);
                break;
            case MacroSubType.HealStone:
                obj = FindCliloc(UOLoginNet.PlayerBackpack, 1095376);
                break;
            case MacroSubType.SpellStone:
                obj = FindCliloc(UOLoginNet.PlayerBackpack, 1095377);
                break;
            case MacroSubType.EnchantedApple:
                obj = FindCliloc(UOLoginNet.PlayerBackpack, 1032248);
                break;
            case MacroSubType.PetalsOfTrinsic:
                obj = FindCliloc(UOLoginNet.PlayerBackpack, 1062926);
                break;
            case MacroSubType.OrangePetals:
                obj = FindCliloc(UOLoginNet.PlayerBackpack, 1053122);
                break;
            case MacroSubType.SmokeBomb:
                obj = FindGraphic(UOLoginNet.PlayerBackpack, 0x2808);
                break;
            case MacroSubType.TrappedBox:
                obj = FindPreferredCliloc(1015093, 1022473, 1044309, 1022474, 1023709, 1027808, 1027809);
                break;
        }
        if (obj != 0) DoubleClick(obj);
    }

    void SetLastTarget(uint serial) {
        if (SerialHelper.IsValid(serial) && StillThere(serial)) {
            string name = UOLoginNet.NameOf(serial);
            if (SerialHelper.IsMobile(serial)) {
                Note(string.Format("Target: {0}", name), Notoriety.GetHue((NotorietyFlag)UOLoginNet.MobileNotoriety(serial)));
                _selected = serial;
                _last.SetEntity(serial);
                return;
            }
            Note(string.Format("Target: {0}", name), 992);
            _selected = serial;
            _last.SetEntity(serial);
            return;
        }
        Note("Entity not found.");
    }

    void Say(string text, ushort hue, byte type) {
        if (Version >= ClientVersion.CV_200)
            UOLoginNet.Send(OutgoingPackets.Send_UnicodeSpeechRequest(text, type, 3, hue, "ENU"));
        else
            UOLoginNet.Send(OutgoingPackets.Send_ASCIISpeechRequest(text, type, 3, hue));
    }

    void DoubleClick(uint serial) {
        uint player = UOLoginNet.PlayerSerial;
        if (serial != player && SerialHelper.IsMobile(serial) && UOLoginNet.InWarMode) {
            // criminal query is a profile flag and is not ported
            UOLoginNet.Send(OutgoingPackets.Send_StatusRequest(serial));
            UOLoginNet.Send(OutgoingPackets.Send_AttackRequest(serial));
        }
        else UOLoginNet.Send(OutgoingPackets.Send_DoubleClick(serial));
        // The human-body check is not stored, so any mobile is remembered.
        if (SerialHelper.IsItem(serial) || SerialHelper.IsMobile(serial)) _lastObject = serial;
        else _lastObject = 0;
    }

    void Attack(uint serial) {
        // criminal query is a profile flag and is not ported
        UOLoginNet.Send(OutgoingPackets.Send_AttackRequest(serial));
    }

    void AllNames() {
        UOLoginNet.CopyActors(_mobiles, _items);
        uint player = UOLoginNet.PlayerSerial;
        for (int i = 0; i < _mobiles.Count; i++)
            if (_mobiles[i].Serial != player)
                UOLoginNet.Send(OutgoingPackets.Send_ClickRequest(_mobiles[i].Serial));
        for (int i = 0; i < _items.Count; i++)
            if (_items[i].Graphic == CorpseGraphic)
                UOLoginNet.Send(OutgoingPackets.Send_ClickRequest(_items[i].Serial));
    }

    void OpenPaperdoll(uint serial) {
        PaperdollGump paper = UIManager.GetGump<PaperdollGump>(serial);
        if (paper == null) UOLoginNet.Send(OutgoingPackets.Send_DoubleClick(serial | 0x80000000));
        else {
            if (paper.IsMinimized) paper.IsMinimized = false;
            Bring(paper);
        }
    }

    void Show<T>(T open, Func<T> create) where T : Gump {
        if (open == null) UIManager.Add(create());
        else Bring(open);
    }

    static void Bring(Gump gump) {
        if (gump == null || gump.IsDisposed) return;
        LinkedListNode<Gump> node = UIManager.Gumps.Find(gump);
        if (node == null) return;
        UIManager.Gumps.Remove(node);
        UIManager.Gumps.AddFirst(gump);
    }

    void CloseGumps() {
        // WorldGump is the viewport ClassicUO calls WorldViewportGump.
        List<Gump> doomed = new List<Gump>();
        foreach (Gump g in UIManager.Gumps) {
            if (g is TopBarGump || g is BuffGump || g is WorldGump) continue;
            doomed.Add(g);
        }
        for (int i = 0; i < doomed.Count; i++) doomed[i].Dispose();
    }

    void CloseCorpses() {
        List<ContainerGump> corpses = new List<ContainerGump>();
        foreach (Gump g in UIManager.Gumps) {
            if (g is ContainerGump box && Corpse(box)) corpses.Add(box);
        }
        for (int i = 0; i < corpses.Count; i++) corpses[i].Dispose();
        // GridLootGump is not in this tree
    }

    static bool Corpse(ContainerGump gump) {
        for (int i = 0; i < gump.Children.Count; i++)
            if (gump.Children[i] is GumpPic pic && pic.Graphic == CorpseGump) return true;
        return false;
    }

    void UseAbility(bool primary) {
        CombatBookGump book = UIManager.GetGump<CombatBookGump>();
        if (book != null) {
            book.Use(primary);
            return;
        }
        // Weapon pair stays on CombatBookGump. A closed book uses the default pair.
        byte ability = primary ? _primary : _secondary;
        byte idx;
        if ((ability & 0x80) == 0) {
            _primary &= 0x7F;
            _secondary &= 0x7F;
            idx = (byte)(ability & 0x7F);
            if (primary) _primary |= 0x80;
            else _secondary |= 0x80;
        }
        else {
            idx = 0;
            if (primary) _primary &= 0x7F;
            else _secondary &= 0x7F;
        }
        UOLoginNet.Send(OutgoingPackets.Send_UseCombatAbility(UOLoginNet.PlayerSerial, idx));
    }

    byte View() {
        if (!_rangeSet) return UOLoginNet.ViewRange;
        return _range;
    }

    void SetView(byte value) {
        _range = value;
        _rangeSet = true;
        UOLoginNet.Send(OutgoingPackets.Send_ClientViewRange(value));
        Note(string.Format("ClientViewRange is now {0}.", value));
    }

    void Note(string message, ushort hue = PrintHue) {
        SystemLine = message;
        SystemHue = hue;
    }

    uint EquipSerial(byte layer) {
        UOLoginNet.CopyEquip(UOLoginNet.PlayerSerial, _equip);
        for (int i = 0; i < _equip.Count; i++)
            if (_equip[i].Layer == layer) return _equip[i].Serial;
        return 0;
    }

    uint FindBandage() {
        uint pack = UOLoginNet.PlayerBackpack;
        if (pack == 0) return 0;
        UOLoginNet.CopyContents(pack, _rows);
        for (int i = 0; i < _rows.Count; i++)
            if (_rows[i].Graphic == BandageGraphic) return _rows[i].Serial;
        return 0;
    }

    uint FindGraphic(uint container, ushort graphic) {
        if (container == 0) return 0;
        List<UOContainedState> rows = new List<UOContainedState>();
        UOLoginNet.CopyContents(container, rows);
        for (int i = 0; i < rows.Count; i++) {
            if (rows[i].Graphic == graphic) return rows[i].Serial;
            uint nested = FindGraphic(rows[i].Serial, graphic);
            if (nested != 0) return nested;
        }
        return 0;
    }

    uint FindPreferredCliloc(params int[] clilocs) {
        for (int i = 0; i < clilocs.Length; i++) {
            uint item = FindCliloc(UOLoginNet.PlayerBackpack, clilocs[i]);
            if (item != 0) return item;
        }
        return 0;
    }

    uint FindCliloc(uint container, int cliloc) {
        if (container == 0) return 0;
        string want = UOGamePackets.ClilocText(cliloc);
        List<UOContainedState> rows = new List<UOContainedState>();
        UOLoginNet.CopyContents(container, rows);
        for (int i = 0; i < rows.Count; i++) {
            if (UOLoginNet.TryProperties(rows[i].Serial, out string name, out _) && name == want)
                return rows[i].Serial;
            uint nested = FindCliloc(rows[i].Serial, cliloc);
            if (nested != 0) return nested;
        }
        return 0;
    }

    bool StillThere(uint serial) {
        if (!SerialHelper.IsValid(serial)) return false;
        if (serial == UOLoginNet.PlayerSerial) return true;
        UOLoginNet.CopyActors(_mobiles, _items);
        for (int i = 0; i < _mobiles.Count; i++)
            if (_mobiles[i].Serial == serial) return true;
        for (int i = 0; i < _items.Count; i++)
            if (_items[i].Serial == serial) return true;
        UOLoginNet.CopyEquip(UOLoginNet.PlayerSerial, _equip);
        for (int i = 0; i < _equip.Count; i++)
            if (_equip[i].Serial == serial) return true;
        uint pack = UOLoginNet.PlayerBackpack;
        return pack != 0 && Contains(pack, serial);
    }

    bool Contains(uint container, uint serial) {
        List<UOContainedState> rows = new List<UOContainedState>();
        UOLoginNet.CopyContents(container, rows);
        for (int i = 0; i < rows.Count; i++) {
            if (rows[i].Serial == serial) return true;
            if (Contains(rows[i].Serial, serial)) return true;
        }
        return false;
    }

    uint FindNearest(ScanTypeObject scan) {
        List<uint> list = new List<uint>();
        List<int> dist = new List<int>();
        Collect(scan, list, dist);
        int best = int.MaxValue;
        uint serial = 0;
        for (int i = 0; i < list.Count; i++) {
            if (dist[i] < best) {
                best = dist[i];
                serial = list[i];
            }
        }
        return serial;
    }

    uint FindNext(ScanTypeObject scan, uint lastSerial, bool reverse) {
        List<uint> list = new List<uint>();
        Collect(scan, list, null);
        if (reverse) list.Reverse();
        if (lastSerial == 0) return list.Count > 0 ? list[0] : 0;
        bool found = false;
        for (int i = 0; i < list.Count; i++) {
            if (list[i] == lastSerial) {
                found = true;
                continue;
            }
            if (found) return list[i];
        }
        if (found || lastSerial != 0) return FindNext(scan, 0, reverse);
        return 0;
    }

    void Collect(ScanTypeObject scan, List<uint> serials, List<int> dist) {
        UOLoginNet.CopyActors(_mobiles, _items);
        uint player = UOLoginNet.PlayerSerial;
        int px = UOLoginNet.PlayerX;
        int py = UOLoginNet.PlayerY;
        if (scan == ScanTypeObject.Objects) {
            for (int i = 0; i < _items.Count; i++) {
                UOItemState item = _items[i];
                serials.Add(item.Serial);
                dist?.Add(Distance(px, py, item.X, item.Y));
            }
            return;
        }
        if (scan == ScanTypeObject.Party) UOParty.CopyMembers(_party);
        for (int i = 0; i < _mobiles.Count; i++) {
            UOMobileState mobile = _mobiles[i];
            if (mobile.Serial == player) continue;
            switch (scan) {
                case ScanTypeObject.Party:
                    if (!InParty(mobile.Serial)) continue;
                    break;
                case ScanTypeObject.Followers:
                    // rename bit is not stored
                    continue;
                case ScanTypeObject.Hostile:
                    NotorietyFlag noto = (NotorietyFlag)mobile.Notoriety;
                    if (noto == NotorietyFlag.Ally || noto == NotorietyFlag.Innocent || noto == NotorietyFlag.Invulnerable)
                        continue;
                    break;
                case ScanTypeObject.Objects:
                    continue;
            }
            serials.Add(mobile.Serial);
            dist?.Add(Distance(px, py, mobile.X, mobile.Y));
        }
    }

    bool InParty(uint serial) {
        for (int i = 0; i < _party.Count; i++)
            if (_party[i].serial == serial) return true;
        return false;
    }

    static int Distance(int px, int py, int x, int y) {
        int dx = Math.Abs(x - px);
        int dy = Math.Abs(y - py);
        return dx > dy ? dx : dy;
    }

    static byte BookType(MacroSubType sub) {
        switch (sub) {
            case MacroSubType.NecroSpellbook: return 1;
            case MacroSubType.PaladinSpellbook: return 2;
            case MacroSubType.BushidoSpellbook: return 4;
            case MacroSubType.NinjitsuSpellbook: return 5;
            case MacroSubType.SpellWeavingSpellbook: return 6;
            case MacroSubType.MysticismSpellbook: return 7;
            case MacroSubType.BardSpellbook: return 8;
            default: return 0;
        }
    }

    static bool Gargoyle(ushort body) {
        return body == 0x029A || body == 0x029B || body == 0x02B6 || body == 0x02B7;
    }

    static ClientVersion Version => UltimaOnline.Current != null ? UltimaOnline.Current.Version : default;

    sealed class LastTarget {
        public uint Serial;
        public ushort X = 0xFFFF;
        public ushort Y = 0xFFFF;
        public sbyte Z;
        public bool IsEntity => SerialHelper.IsValid(Serial);

        public void SetEntity(uint serial) {
            Serial = serial;
            X = Y = 0xFFFF;
            Z = sbyte.MinValue;
        }
    }
}

public class Macro : LinkedObject, IEquatable<Macro> {
    public Macro(string name, int key, bool alt, bool ctrl, bool shift) : this(name) {
        Key = key;
        Alt = alt;
        Ctrl = ctrl;
        Shift = shift;
    }

    public Macro(string name, MouseButtonType button, bool alt, bool ctrl, bool shift) : this(name) {
        MouseButton = button;
        Alt = alt;
        Ctrl = ctrl;
        Shift = shift;
    }

    public Macro(string name, bool wheelUp, bool alt, bool ctrl, bool shift) : this(name) {
        WheelScroll = true;
        WheelUp = wheelUp;
        Alt = alt;
        Ctrl = ctrl;
        Shift = shift;
    }

    public Macro(string name) { Name = name; }

    public string Name { get; }

    /// <summary>SDL keycode number. Letters are ASCII. SDL is not referenced.</summary>
    public int Key { get; set; }
    public MouseButtonType MouseButton { get; set; }
    public bool WheelScroll { get; set; }
    public bool WheelUp { get; set; }
    public bool Alt { get; set; }
    public bool Ctrl { get; set; }
    public bool Shift { get; set; }

    public bool Equals(Macro other) {
        if (other == null) return false;
        return Key == other.Key && Alt == other.Alt && Ctrl == other.Ctrl && Shift == other.Shift && Name == other.Name;
    }

    public void Save(XmlTextWriter writer) {
        writer.WriteStartElement("macro");
        writer.WriteAttributeString("name", Name);
        writer.WriteAttributeString("key", Key.ToString());
        writer.WriteAttributeString("mousebutton", ((int)MouseButton).ToString());
        writer.WriteAttributeString("wheelscroll", WheelScroll.ToString());
        writer.WriteAttributeString("wheelup", WheelUp.ToString());
        writer.WriteAttributeString("alt", Alt.ToString());
        writer.WriteAttributeString("ctrl", Ctrl.ToString());
        writer.WriteAttributeString("shift", Shift.ToString());
        writer.WriteStartElement("actions");
        for (MacroObject action = (MacroObject)Items; action != null; action = (MacroObject)action.Next) {
            writer.WriteStartElement("action");
            writer.WriteAttributeString("code", ((int)action.Code).ToString());
            writer.WriteAttributeString("subcode", ((int)action.SubCode).ToString());
            writer.WriteAttributeString("submenutype", action.SubMenuType.ToString());
            if (action.HasString())
                writer.WriteAttributeString("text", ((MacroObjectString)action).Text);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    public void Load(XmlElement xml) {
        if (xml == null) return;
        Key = int.Parse(xml.GetAttribute("key"));
        Alt = bool.Parse(xml.GetAttribute("alt"));
        Ctrl = bool.Parse(xml.GetAttribute("ctrl"));
        Shift = bool.Parse(xml.GetAttribute("shift"));
        if (xml.HasAttribute("mousebutton"))
            MouseButton = (MouseButtonType)int.Parse(xml.GetAttribute("mousebutton"));
        if (xml.HasAttribute("wheelscroll"))
            WheelScroll = bool.Parse(xml.GetAttribute("wheelscroll"));
        if (xml.HasAttribute("wheelup"))
            WheelUp = bool.Parse(xml.GetAttribute("wheelup"));
        XmlElement actions = xml["actions"];
        if (actions == null) return;
        foreach (XmlElement xmlAction in actions.GetElementsByTagName("action")) {
            MacroType code = (MacroType)int.Parse(xmlAction.GetAttribute("code"));
            MacroSubType sub = (MacroSubType)int.Parse(xmlAction.GetAttribute("subcode"));
            // MovePlayer (61) is Walk. Top, left, down, and right become NW, SW, SE, and NE.
            if ((int)code == 61) {
                code = MacroType.Walk;
                switch ((int)sub) {
                    case 211: sub = MacroSubType.NW; break;
                    case 214: sub = MacroSubType.SW; break;
                    case 213: sub = MacroSubType.SE; break;
                    case 212: sub = MacroSubType.NE; break;
                }
            }
            sbyte subMenuType = sbyte.Parse(xmlAction.GetAttribute("submenutype"));
            MacroObject m;
            if (xmlAction.HasAttribute("text"))
                m = new MacroObjectString(code, sub, xmlAction.GetAttribute("text"));
            else
                m = new MacroObject(code, sub);
            m.SubMenuType = subMenuType;
            PushToBack(m);
        }
    }

    public static MacroObject Create(MacroType code) {
        switch (code) {
            case MacroType.Say:
            case MacroType.Emote:
            case MacroType.Whisper:
            case MacroType.Yell:
            case MacroType.Delay:
            case MacroType.SetUpdateRange:
            case MacroType.ModifyUpdateRange:
            case MacroType.RazorMacro:
            case MacroType.UseCounterBarSlot:
                return new MacroObjectString(code, MacroSubType.MSC_NONE);
            default:
                return new MacroObject(code, MacroSubType.MSC_NONE);
        }
    }

    public static Macro CreateEmptyMacro(string name) {
        Macro macro = new Macro(name, 0, false, false, false);
        macro.PushToBack(new MacroObject(MacroType.None, MacroSubType.MSC_NONE));
        return macro;
    }

    public static Macro CreateFastMacro(string name, MacroType type, MacroSubType sub) {
        Macro macro = new Macro(name, 0, false, false, false);
        macro.PushToBack(new MacroObject(type, sub));
        return macro;
    }

    public static void GetBoundByCode(MacroType code, ref int count, ref int offset) {
        switch (code) {
            case MacroType.Walk:
                offset = (int)MacroSubType.NW;
                count = (int)MacroSubType.Configuration - (int)MacroSubType.NW;
                break;
            case MacroType.Open:
            case MacroType.Close:
            case MacroType.Minimize:
            case MacroType.Maximize:
                offset = (int)MacroSubType.Configuration;
                count = (int)MacroSubType.Anatomy - (int)MacroSubType.Configuration;
                break;
            case MacroType.UseSkill:
                offset = (int)MacroSubType.Anatomy;
                count = (int)MacroSubType.LeftHand - (int)MacroSubType.Anatomy;
                break;
            case MacroType.ArmDisarm:
                offset = (int)MacroSubType.LeftHand;
                count = (int)MacroSubType.Honor - (int)MacroSubType.LeftHand;
                break;
            case MacroType.InvokeVirtue:
                offset = (int)MacroSubType.Honor;
                count = (int)MacroSubType.Clumsy - (int)MacroSubType.Honor;
                break;
            case MacroType.CastSpell:
                offset = (int)MacroSubType.Clumsy;
                count = (int)MacroSubType.Hostile - (int)MacroSubType.Clumsy;
                break;
            case MacroType.SelectNext:
            case MacroType.SelectPrevious:
            case MacroType.SelectNearest:
                offset = (int)MacroSubType.Hostile;
                count = (int)MacroSubType.MscTotalCount - (int)MacroSubType.Hostile;
                break;
            case MacroType.UsePotion:
                offset = (int)MacroSubType.ConfusionBlastPotion;
                count = (int)MacroSubType.DefaultZoom - (int)MacroSubType.ConfusionBlastPotion;
                break;
            case MacroType.Zoom:
                offset = (int)MacroSubType.DefaultZoom;
                count = 1 + (int)MacroSubType.ZoomOut - (int)MacroSubType.DefaultZoom;
                break;
            case MacroType.UseObject:
                offset = (int)MacroSubType.BestHealPotion;
                count = 1 + (int)MacroSubType.SpellStone - (int)MacroSubType.BestHealPotion;
                break;
            case MacroType.LookAtMouse:
                offset = (int)MacroSubType.LookForwards;
                count = 1 + (int)MacroSubType.LookBackwards - (int)MacroSubType.LookForwards;
                break;
        }
    }
}

public class MacroObject : LinkedObject {
    public MacroObject(MacroType code, MacroSubType sub) {
        Code = code;
        SubCode = sub;
        switch (code) {
            case MacroType.Walk:
            case MacroType.Open:
            case MacroType.Close:
            case MacroType.Minimize:
            case MacroType.Maximize:
            case MacroType.UseSkill:
            case MacroType.ArmDisarm:
            case MacroType.InvokeVirtue:
            case MacroType.CastSpell:
            case MacroType.SelectNext:
            case MacroType.SelectPrevious:
            case MacroType.SelectNearest:
            case MacroType.UsePotion:
            case MacroType.Zoom:
            case MacroType.UseObject:
            case MacroType.LookAtMouse:
                if (sub == MacroSubType.MSC_NONE) {
                    int count = 0;
                    int offset = 0;
                    Macro.GetBoundByCode(code, ref count, ref offset);
                    SubCode = (MacroSubType)offset;
                }
                SubMenuType = 1;
                break;
            case MacroType.Say:
            case MacroType.Emote:
            case MacroType.Whisper:
            case MacroType.Yell:
            case MacroType.Delay:
            case MacroType.SetUpdateRange:
            case MacroType.ModifyUpdateRange:
            case MacroType.RazorMacro:
            case MacroType.UseCounterBarSlot:
                SubMenuType = 2;
                break;
            default:
                SubMenuType = 0;
                break;
        }
    }

    public MacroType Code { get; set; }
    public MacroSubType SubCode { get; set; }
    public sbyte SubMenuType { get; set; }

    public virtual bool HasString() { return false; }
}

public class MacroObjectString : MacroObject {
    public MacroObjectString(MacroType code, MacroSubType sub, string str = "") : base(code, sub) {
        Text = str;
    }

    public string Text { get; set; }

    public override bool HasString() { return true; }
}

public enum MacroType {
    None = 0,
    Say,
    Emote,
    Whisper,
    Yell,
    Walk,
    WarPeace,
    Paste,
    Open,
    Close,
    Minimize,
    Maximize,
    OpenDoor,
    UseSkill,
    LastSkill,
    CastSpell,
    LastSpell,
    LastObject,
    Bow,
    Salute,
    QuitGame,
    AllNames,
    LastTarget,
    TargetSelf,
    ArmDisarm,
    WaitForTarget,
    TargetNext,
    AttackLast,
    Delay,
    CircleTrans,
    CloseGump,
    AlwaysRun,
    SaveDesktop,
    KillGumpOpen,
    PrimaryAbility,
    SecondaryAbility,
    EquipLastWeapon,
    SetUpdateRange,
    ModifyUpdateRange,
    IncreaseUpdateRange,
    DecreaseUpdateRange,
    MaxUpdateRange,
    MinUpdateRange,
    DefaultUpdateRange,
    EnableRangeColor,
    DisableRangeColor,
    ToggleRangeColor,
    InvokeVirtue,
    SelectNext,
    SelectPrevious,
    SelectNearest,
    AttackSelectedTarget,
    UseSelectedTarget,
    CurrentTarget,
    TargetSystemOnOff,
    ToggleBuffIconGump,
    BandageSelf,
    BandageTarget,
    ToggleGargoyleFly,
    Zoom,
    ToggleChatVisibility,
    INVALID,
    Aura = 62,
    AuraOnOff,
    Grab,
    SetGrabBag,
    NamesOnOff,
    UseItemInHand,
    UsePotion,
    CloseAllHealthBars,
    RazorMacro,
    ToggleDrawRoofs,
    ToggleTreeStumps,
    ToggleVegetation,
    ToggleCaveTiles,
    CloseInactiveHealthBars,
    CloseCorpses,
    UseObject,
    LookAtMouse,
    UseCounterBarSlot
}

public enum MacroSubType {
    MSC_NONE = 0,
    NW,
    N,
    NE,
    E,
    SE,
    S,
    SW,
    W,
    Configuration,
    Paperdoll,
    Status,
    Journal,
    Skills,
    MageSpellbook,
    Chat,
    Backpack,
    Overview,
    WorldMap,
    Mail,
    PartyManifest,
    PartyChat,
    NecroSpellbook,
    PaladinSpellbook,
    CombatBook,
    BushidoSpellbook,
    NinjitsuSpellbook,
    Guild,
    SpellWeavingSpellbook,
    QuestLog,
    MysticismSpellbook,
    RacialAbilitiesBook,
    BardSpellbook,
    Anatomy,
    AnimalLore,
    AnimalTaming,
    ArmsLore,
    Begging,
    Cartography,
    DetectingHidden,
    Discordance,
    EvaluatingIntelligence,
    ForensicEvaluation,
    Hiding,
    Imbuing,
    Inscription,
    ItemIdentification,
    Meditation,
    Peacemaking,
    Poisoning,
    Provocation,
    RemoveTrap,
    SpiritSpeak,
    Stealing,
    Stealth,
    TasteIdentification,
    Tracking,
    LeftHand,
    // Arm/Disarm group
    RightHand,
    Honor,
    Sacrifice,
    Valor,
    Clumsy,
    CreateFood,
    Feeblemind,
    Heal,
    MagicArrow,
    NightSight,
    ReactiveArmor,
    Weaken,
    Agility,
    Cunning,
    Cure,
    Harm,
    MagicTrap,
    MagicUntrap,
    Protection,
    Strength,
    Bless,
    Fireball,
    MagicLock,
    Poison,
    Telekinesis,
    Teleport,
    Unlock,
    WallOfStone,
    ArchCure,
    ArchProtection,
    Curse,
    FireField,
    GreaterHeal,
    Lightning,
    ManaDrain,
    Recall,
    BladeSpirits,
    DispellField,
    Incognito,
    MagicReflection,
    MindBlast,
    Paralyze,
    PoisonField,
    SummonCreature,
    Dispel,
    EnergyBolt,
    Explosion,
    Invisibility,
    Mark,
    MassCurse,
    ParalyzeField,
    Reveal,
    ChainLightning,
    EnergyField,
    FlameStrike,
    GateTravel,
    ManaVampire,
    MassDispel,
    MeteorSwarm,
    Polymorph,
    Earthquake,
    EnergyVortex,
    Resurrection,
    AirElemental,
    SummonDaemon,
    EarthElemental,
    FireElemental,
    WaterElemental,
    AnimateDead,
    BloodOath,
    CorpseSkin,
    CurseWeapon,
    EvilOmen,
    HorrificBeast,
    LichForm,
    MindRot,
    PainSpike,
    PoisonStrike,
    Strangle,
    SummonFamilar,
    VampiricEmbrace,
    VengefulSpirit,
    Wither,
    WraithForm,
    Exorcism,
    CleanceByFire,
    CloseWounds,
    ConsecrateWeapon,
    DispelEvil,
    DivineFury,
    EnemyOfOne,
    HolyLight,
    NobleSacrifice,
    RemoveCurse,
    SacredJourney,
    HonorableExecution,
    Confidence,
    Evasion,
    CounterAttack,
    LightingStrike,
    MomentumStrike,
    FocusAttack,
    DeathStrike,
    AnimalForm,
    KiAttack,
    SurpriceAttack,
    Backstab,
    Shadowjump,
    MirrorImage,
    ArcaneCircle,
    GiftOfRenewal,
    ImmolatingWeapon,
    Attunement,
    Thunderstorm,
    NaturesFury,
    SummonFey,
    SummonFiend,
    ReaperForm,
    Wildfire,
    EssenceOfWind,
    DryadAllure,
    EtherealVoyage,
    WordOfDeath,
    GiftOfLife,
    ArcaneEmpowermen,
    NetherBolt,
    HealingStone,
    PurgeMagic,
    Enchant,
    Sleep,
    EagleStrike,
    AnimatedWeapon,
    StoneForm,
    SpellTrigger,
    MassSleep,
    CleansingWinds,
    Bombard,
    SpellPlague,
    HailStorm,
    NetherCyclone,
    RisingColossus,
    Inspire,
    Invigorate,
    Resilience,
    Perseverance,
    Tribulation,
    Despair,
    Hostile,
    Party,
    Follower,
    Object,
    Mobile,
    MscTotalCount,
    INVALID_0,
    INVALID_1,
    INVALID_2,
    INVALID_3,
    ConfusionBlastPotion = 215,
    CurePotion,
    AgilityPotion,
    StrengthPotion,
    PoisonPotion,
    RefreshPotion,
    HealPotion,
    ExplosionPotion,
    DefaultZoom,
    ZoomIn,
    ZoomOut,
    BestHealPotion,
    BestCurePotion,
    BestRefreshPotion,
    BestStrengthPotion,
    BestAgiPotion,
    BestExplosionPotion,
    BestConflagPotion,
    EnchantedApple,
    PetalsOfTrinsic,
    OrangePetals,
    TrappedBox,
    SmokeBomb,
    HealStone,
    SpellStone,
    LookForwards,
    LookBackwards
}

/// <summary>ClassicUO ScanTypeObject. Hostile, party, follower, object, then any mobile.</summary>
public enum ScanTypeObject {
    Hostile = 0,
    Party,
    Followers,
    Objects,
    Mobiles
}

/// <summary>ClassicUO ScanModeObject. Next, previous, then nearest.</summary>
public enum ScanModeObject {
    Next = 0,
    Previous,
    Nearest
}
