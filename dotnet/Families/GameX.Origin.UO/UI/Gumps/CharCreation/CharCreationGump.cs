// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CharCreationGump. Not CharacterCreationGump: that shell stays the login step.
/// Finish calls UOLoginNet.CreateCharacter. It does not build a 0x00 or 0xF8.
/// </summary>
public class CharCreationGump : Gump {
    public const uint LockedT2A = 0x00000001;
    public const uint LockedAOS = 0x00000010;
    public const uint LockedSE = 0x00000040;
    public const uint LockedML = 0x00000080;
    public const uint LockedSA = 0x00010000;

    readonly LoginScene _scene;
    CharDraft _character;
    int _cityIndex;
    CharCreationStep _step;
    ProfessionInfo _profession;
    CharCreationMessage _message;

    public CharCreationGump(World world, LoginScene scene) : base(world, 0, 0) {
        _scene = scene;
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        Add(new CreateCharAppearanceGump(world), 1);
        SetStep(CharCreationStep.Appearence);
    }

    public static ClientVersion Version {
        get {
            var uo = UltimaOnline.Current;
            if (uo == null) return 0;
            if (uo.Version != 0) return uo.Version;
            return ClientVersionHelper.ValidateClientVersion(uo.VersionText) ?? 0;
        }
    }

    public static int SkillsCount => Version >= ClientVersion.CV_70160 ? 4 : 3;

    public CharDraft Character => _character;
    public ProfessionInfo SelectedProfession => _profession;

    public static string Cliloc(int id, string fallback = null) {
        var text = UOGamePackets.ClilocText(id);
        if (string.IsNullOrEmpty(text) || text == "#" + id) return fallback ?? text ?? "";
        return text;
    }

    public static CharCreationGump Shell(Control from) {
        for (var p = from; p != null; p = p.Parent)
            if (p is CharCreationGump g) return g;
        return UIManager.GetGump<CharCreationGump>();
    }

    /// <summary>0xB9 locked flags. Zero during character creation uses the client version instead.</summary>
    public static bool Expansion(uint bit, ClientVersion since) {
        var flags = UOLoginNet.Features;
        if (flags != 0) return (flags & bit) != 0;
        return Version >= since;
    }

    public void SetCharacter(CharDraft character) {
        _character = character;
        SetStep(CharCreationStep.ChooseProfession);
    }

    public void SetAttributes(bool force = false) {
        SetStep(_profession != null && (_profession.DescriptionIndex >= 0 || force)
            ? CharCreationStep.ChooseCity
            : CharCreationStep.ChooseTrade);
    }

    public void SetCity(int cityIndex) => _cityIndex = cityIndex;

    public void SetProfession(ProfessionInfo info) {
        if (_character == null || info == null) return;
        for (var i = 0; i < _character.Skills.Length; i++) _character.Skills[i] = 0;
        var count = SkillsCount;
        for (var i = 0; i < count; i++) {
            var skillIndex = info.SkillDefVal[i, 0];
            if (skillIndex < 0 || skillIndex >= _character.Skills.Length) continue;
            if (!Expansion(LockedSE, ClientVersion.CV_405A) && (skillIndex == 52 || skillIndex == 53)) {
                for (var k = 0; k < i; k++) {
                    var prior = info.SkillDefVal[k, 0];
                    if (prior >= 0 && prior < _character.Skills.Length) _character.Skills[prior] = 0;
                }
                ShowMessage(Cliloc(1063016, "That profession is not available."));
                return;
            }
            _character.Skills[skillIndex] = (ushort)info.SkillDefVal[i, 1];
        }
        _profession = info;
        _character.Strength = (ushort)info.StatsVal[0];
        _character.Intelligence = (ushort)info.StatsVal[1];
        _character.Dexterity = (ushort)info.StatsVal[2];
        SetAttributes();
        SetStep(info.DescriptionIndex > 0 ? CharCreationStep.ChooseCity : CharCreationStep.ChooseTrade);
    }

    public void CreateCharacter(byte profession) {
        var name = _character?.Name;
        if (string.IsNullOrWhiteSpace(name)) return;
        UOLoginNet.CreateCharacter(name, _character.Female, _scene.Characters?.Length ?? 0);
        _scene.CurrentLoginStep = LoginSteps.CharacterCreationDone;
    }

    public void StepBack(int steps = 1) {
        if (_step == CharCreationStep.Appearence) _scene.CurrentLoginStep = LoginSteps.CharacterSelection;
        else SetStep((CharCreationStep)((int)_step - steps));
    }

    public void ShowMessage(string message) {
        var page = ActivePage;
        if (_message != null && !_message.IsDisposed) {
            _message.Parent = null;
            _message.Dispose();
        }
        _message = new CharCreationMessage(message, () => {
            if (!IsDisposed) ActivePage = page;
        });
        Add(_message, 5);
        ActivePage = 5;
    }

    void SetStep(CharCreationStep step) {
        _step = step;
        switch (step) {
            case CharCreationStep.ChooseProfession:
                Replace(2, new CreateCharProfessionGump(World));
                ActivePage = 2;
                break;
            case CharCreationStep.ChooseTrade:
                Replace(3, new CreateCharTradeGump(World, _character, _profession));
                ActivePage = 3;
                break;
            case CharCreationStep.ChooseCity:
                var index = _profession == null ? (byte)0 : (byte)_profession.DescriptionIndex;
                var cityPage = new CreateCharSelectionCityGump(World, index, _scene);
                Replace(4, cityPage);
                ActivePage = 4;
                if (!cityPage.HasCity)
                    ShowMessage("No city found. Something wrong with the received cities.");
                break;
            default:
                ActivePage = 1;
                break;
        }
    }

    void Replace(int page, Gump next) {
        for (var i = Children.Count - 1; i >= 0; i--) {
            var child = Children[i];
            if (child.Page != page) continue;
            child.Parent = null;
            child.Dispose();
        }
        Add(next, page);
    }

    enum CharCreationStep {
        Appearence = 0,
        ChooseProfession = 1,
        ChooseTrade = 2,
        ChooseCity = 3,
    }

    sealed class CharCreationMessage : Control {
        readonly System.Action _close;

        public CharCreationMessage(string message, System.Action close) {
            _close = close;
            X = 135;
            Y = 56;
            Width = 400;
            Height = 300;
            CanMove = false;
            WantUpdateSize = false;
            AcceptMouseInput = false;
            Add(new ResizePic(0x0A28) { Width = 400, Height = 300, AcceptMouseInput = false });
            Add(new Label(message ?? "", false, 0x0386, 2) { X = 20, Y = 40 });
            var ok = new Button(0, 0x0817, 0x0818, 0x0819) { X = 170, Y = 240, ButtonAction = ButtonAction.Activate };
            if (!ok.IsDisposed) Add(ok);
        }

        public override void OnButtonClick(int buttonID) {
            Parent = null;
            Dispose();
            _close?.Invoke();
        }
    }
}

/// <summary>Creation choices. The create packet still uses only the name, sex, and slot.</summary>
public sealed class CharDraft {
    public string Name = "";
    public bool Female;
    public byte Race = 1;
    public ushort Hue;
    public ushort Strength = 60;
    public ushort Intelligence = 10;
    public ushort Dexterity = 10;
    public ushort HairGraphic;
    public ushort HairHue;
    public ushort BeardGraphic;
    public ushort BeardHue;
    public ushort ShirtHue = 2;
    public ushort PantsHue = 2;
    public readonly ushort[] Skills = new ushort[58];
}

/// <summary>One Prof.txt row, plus the Advanced entry ClassicUO appends.</summary>
public sealed class ProfessionInfo {
    public ProfessionInfo(ClientVersion version) {
        (SkillDefVal, StatsVal) = GetDefaults(version);
    }

    public static (int[,] defaultSkillsValues, int[] defaultStatsValues) GetDefaults(ClientVersion version) {
        var initial = version >= ClientVersion.CV_70160 ? 30 : 50;
        var remain = version >= ClientVersion.CV_70160 ? 15 : 10;
        var third = version < ClientVersion.CV_70160 ? 0 : initial;
        return (new[,] { { 0, initial }, { 0, initial }, { 0, third }, { 0, initial } }, new[] { 60, remain, remain });
    }

    public string Name { get; set; } = "";
    public string TrueName { get; set; } = "";
    public int Localization { get; set; }
    public int Description { get; set; }
    public int DescriptionIndex { get; set; }
    public ProfessionKind Type { get; set; }
    public ushort Graphic { get; set; }
    public bool TopLevel { get; set; }
    public int[,] SkillDefVal { get; set; }
    public int[] StatsVal { get; set; }
    public List<string> Children { get; set; }
}

public enum ProfessionKind { None = 0, Category = 1, Profession = 2 }
