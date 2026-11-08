// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO OptionsGump. The left column is the twelve pages plus Ignore List.
/// Cancel, Apply, Default, and OK sit on the bottom. Apply copies the controls
/// into the fields below. Profile path is not ported.
/// ShowPublicHouseContent is not sent. The host window, camera zoom, and world light are not changed.
/// ClassicUO draws these rows with font 0xFF (its unicode sentinel). DrawText here is fonts.mul, so the rows use font 1.
/// </summary>
public sealed class OptionsGump : Gump {
    const int WidthPx = 700;
    const int HeightPx = 500;
    const int TextBoxHeight = 25;
    const int ScreenZoomSteps = 20;
    const byte FontIndex = 1;
    const ushort HueFont = 0xFFFF;
    const int FillKey = 0x57100010;
    const int LineKey = 0x57100011;
    const int ButtonKey = 0x57100014;
    const int ButtonOnKey = 0x57100015;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.95).
    static readonly byte[] Dark = { 0x84, 0x90 };
    // r = g = b = 16, opaque.
    static readonly byte[] Gray = { 0x10, 0xC2 };
    static readonly byte[] ButtonPx = { 0x08, 0xA1 };
    static readonly byte[] ButtonOnPx = { 0x8C, 0xB5 };
    static readonly Random HueRandom = new Random();
    static readonly string[] InfoBarVarNames = {
        "HP", "Mana", "Stamina", "Weight", "Followers", "Gold", "Damage", "Armor", "Luck",
        "FireResist", "ColdResist", "PoisonResist", "EnergyResist", "LowerReagentCost", "SpellDamageInc",
        "FasterCasting", "FasterCastRecovery", "HitChanceInc", "DefenseChanceInc", "LowerManaCost",
        "DamageChanceInc", "SwingSpeedInc", "StatsCap", "NameNotoriety", "TithingPoints",
    };

    // Profile.cs defaults. Apply writes the controls back here. Nothing is saved.
    public bool EnableSound = true;
    public int SoundVolume = 100;
    public bool EnableMusic = true;
    public int MusicVolume = 100;
    public bool EnableFootstepsSound = true;
    public bool EnableCombatMusic = true;
    public bool ReproduceSoundsInBackground;
    public bool LoginMusic = true;
    public int LoginMusicVolume = 70;
    public byte ChatFont = 1;
    public int SpeechDelay = 100;
    public bool ScaleSpeechDelay = true;
    public bool SaveJournalToFile = true;
    public bool ForceUnicodeJournal;
    public bool IgnoreAllianceMessages;
    public bool IgnoreGuildMessages;
    public int MaxJournalFiles = 100;
    public bool JournalFileWithSerial;
    public ushort SpeechHue = 0x02B2;
    public ushort WhisperHue = 0x0033;
    public ushort EmoteHue = 0x0021;
    public ushort YellHue = 0x0021;
    public ushort PartyMessageHue = 0x0044;
    public ushort GuildMessageHue = 0x0044;
    public ushort AllyMessageHue = 0x0057;
    public ushort ChatMessageHue = 0x0256;
    public ushort InnocentHue = 0x005A;
    public ushort PartyAuraHue = 0x0044;
    public ushort FriendHue = 0x0044;
    public ushort CriminalHue = 0x03B2;
    public ushort CanAttackHue = 0x03B2;
    public ushort EnemyHue = 0x0031;
    public ushort MurdererHue = 0x0023;
    public ushort BeneficHue = 0x0059;
    public ushort HarmfulHue = 0x0020;
    public ushort NeutralHue = 0x03B1;
    public bool EnabledSpellHue;
    public bool EnabledSpellFormat;
    public string SpellDisplayFormat = "{power} [{spell}]";
    public ushort PoisonHue = 0x0044;
    public ushort ParalyzedHue = 0x014C;
    public ushort InvulnerableHue = 0x0030;
    public bool EnabledCriminalActionQuery = true;
    public bool EnabledBeneficialCriminalActionQuery;
    public bool UseOldStatusGump;
    public bool StatusGumpBarMutuallyExclusive = true;
    public int BackpackStyle;
    public bool HighlightGameObjects;
    public bool HighlightMobilesByParalize = true;
    public bool HighlightMobilesByPoisoned = true;
    public bool HighlightMobilesByInvul = true;
    public bool ShowMobilesHP;
    public int MobileHPType;
    public int MobileHPShowWhen;
    public bool DrawRoofs = true;
    public bool TreeToStumps;
    public bool EnableCaveBorder;
    public bool HideVegetation;
    public int FieldsType;
    public bool NoColorObjectsOutOfRange;
    public bool UseCircleOfTransparency;
    public int CircleOfTransparencyRadius = 100;
    public int CircleOfTransparencyType;
    public float DefaultScale = 1f;
    public int ZoomIndex;
    public bool EnableMousewheelScaleZoom;
    public bool RestoreScaleAfterUnpressCtrl;
    public bool EnableDeathScreen = true;
    public bool EnableBlackWhiteEffect = true;
    public bool UseTooltip = true;
    public ushort TooltipTextHue = 0xFFFF;
    public int TooltipDelayBeforeDisplay = 250;
    public int TooltipDisplayZoom = 100;
    public int TooltipBackgroundOpacity = 70;
    public byte TooltipFont = 1;
    public bool EnablePathfind;
    public bool UseShiftToPathfind;
    public bool AlwaysRun;
    public bool AlwaysRunUnlessHidden;
    public bool FastRotation;
    public bool HoldDownKeyTab = true;
    public bool HoldShiftForContext;
    public bool HoldShiftToSplitStack;
    public int GameWindowX = 10;
    public int GameWindowY = 10;
    public bool GameWindowLock;
    public bool GameWindowFullSize;
    public bool WindowBorderless;
    public int GameWindowWidth = 600;
    public int GameWindowHeight = 480;
    public bool TopbarGumpIsDisabled;
    public bool UseAlternativeLights;
    public bool UseCustomLightLevel;
    public byte LightLevel;
    public int LightLevelType;
    public bool UseColoredLights = true;
    public bool UseDarkNights;
    public int CloseHealthBarType;
    public bool ActivateChatAfterEnter;
    public bool ActivateChatAdditionalButtons = true;
    public bool ActivateChatShiftEnterSupport = true;
    public bool UseObjectsFading = true;
    public bool HoldDownKeyAltToCloseAnchored = true;
    public bool CloseAllAnchoredGumpsInGroupWithRightClick;
    public bool HoldAltToMoveGumps;
    public bool HideScreenshotStoredInMessage;
    public bool CastSpellsByOneClick;
    public bool BuffBarTime;
    public bool FastSpellsAssign;
    public bool AutoOpenDoors;
    public bool SmoothDoors;
    public bool AutoOpenCorpses;
    public int AutoOpenCorpseRange = 2;
    public int CorpseOpenOptions = 3;
    public bool SkipEmptyCorpse;
    public bool DisableDefaultHotkeys;
    public bool DisableArrowBtn;
    public bool DisableTabBtn;
    public bool DisableCtrlQWBtn;
    public bool DisableAutoMove;
    public bool EnableDragSelect;
    public int DragSelectModifierKey;
    public bool OverrideContainerLocation;
    public int OverrideContainerLocationSetting;
    public bool HueContainerGumps = true;
    public bool DragSelectHumanoidsOnly;
    public bool DragSelectHostileOnly;
    public int DragSelectStartX = 100;
    public int DragSelectStartY = 100;
    public bool DragSelectAsAnchor;
    public bool NameOverheadToggled;
    public bool NameOverheadShowHpBar = true;
    public bool ShowTargetRangeIndicator;
    public bool PartyInviteGump;
    public bool CustomBarsToggled;
    public bool CBBlackBGToggled;
    public bool ShowInfoBar;
    public int InfoBarHighlightType;
    public bool CounterBarEnabled;
    public bool CounterBarHighlightOnChange = true;
    public bool CounterBarHighlightOnAmount;
    public bool CounterBarDisplayAbbreviatedAmount;
    public int CounterBarAbbreviatedAmount = 1000;
    public int CounterBarHighlightAmount = 5;
    public int CounterBarCellSize = 40;
    public bool ShowSkillsChangedMessage = true;
    public int ShowSkillsChangedDeltaValue = 1;
    public bool ShowStatsChangedMessage = true;
    public bool ShadowsEnabled = true;
    public bool ShadowsStatics = true;
    public int TerrainShadowsLevel = 15;
    public int AuraUnderFeetType;
    public bool AuraOnMouse = true;
    public bool AnimatedWaterEffect;
    public bool PartyAura;
    public bool HideChatGradient;
    public bool StandardSkillsGump = true;
    public bool ShowNewMobileNameIncoming = true;
    public bool ShowNewCorpseNameIncoming = true;
    public int GridLootType;
    public bool ReduceFPSWhenInactive = true;
    public int Fps = 60;
    public float ScreenScale = 1f;
    public bool OverrideAllFonts;
    public bool OverrideAllFontsIsUnicode = true;
    public bool SallosEasyGrab;
    public int ContainersScale = 100;
    public bool ScaleItemsInsideContainers;
    public bool DoubleClickToLootInsideContainers;
    public bool UseLargeContainerGumps;
    public bool RelativeDragAndDropItems;
    public bool HighlightContainerWhenSelected;
    public bool UseNewTargetSystem = true;
    public bool ShowHouseContent;
    public bool SaveHealthbars;
    public bool TextFading = true;
    public bool UseSmoothBoatMovement;
    public bool ShowDPSWithDamageNumbers = true;
    public bool UseAlternateJournal;
    public bool OverheadPartyMessages;
    public bool RunMouseInSeparateThread = true;
    public readonly List<InfoBarEntry> InfoBars = new List<InfoBarEntry> {
        new InfoBarEntry("", 23, 0x3D2),
        new InfoBarEntry("Hits", 0, 0x1B6),
        new InfoBarEntry("Mana", 1, 0x1ED),
        new InfoBarEntry("Stam", 2, 0x22E),
        new InfoBarEntry("Weight", 3, 0x3D2),
    };
    public readonly List<string> MacroNames = new List<string>();

    readonly List<OptionsTextButton> _pageButtons = new List<OptionsTextButton>();
    readonly List<InfoBarRow> _infoRows = new List<InfoBarRow>();
    int _selectedMacro = -1;

    Checkbox _autoOpenDoors, _autoOpenCorpse, _skipEmptyCorpse, _disableTabBtn, _disableCtrlQWBtn;
    Checkbox _disableDefaultHotkeys, _disableArrowBtn, _disableAutoMove, _overrideContainerLocation, _smoothDoors;
    Checkbox _showTargetRangeIndicator, _customBars, _customBarsBBG, _saveHealthbars;
    Checkbox _nameOverheadAlwaysOn, _nameOverheadShowHpBar;
    HSliderBar _cellSize;
    Checkbox _containerScaleItems, _containerDoubleClickToLoot, _relativeDragAndDropItems, _useLargeContainers;
    Checkbox _highlightContainersWhenMouseIsOver, _hueContainerGumps;
    HSliderBar _containersScale;
    Combobox _cotType, _autoOpenCorpseOptions, _dragSelectModifierKey, _backpackStyle;
    HSliderBar _delayBeforeTooltip, _tooltipZoom, _tooltipOpacity;
    Checkbox _enableCounters, _highlightOnChange, _highlightOnAmount, _enableAbbreviatedAmount;
    Checkbox _enableDragSelect, _dragSelectHumanoidsOnly, _dragSelectHostileOnly, _dragSelectAsAnchor;
    Checkbox _enableSounds, _enableMusic, _footStepsSound, _combatMusic, _musicInBackground, _loginMusic;
    OptionsFontSelector _fontSelectorChat, _tooltipFontSelector;
    Checkbox _forceUnicodeJournal, _maxJournalFilesEnabled, _journalFileWithSerial;
    OptionsInput _gameWindowHeight, _gameWindowPositionX, _gameWindowPositionY, _gameWindowWidth, _maxJournalFiles;
    Checkbox _gameWindowLock, _gameWindowFullsize, _hideScreenshotStoredInMessage;
    Checkbox _highlightObjects, _enablePathfind, _useShiftPathfind, _alwaysRun, _alwaysRunUnlessHidden, _fastRotation;
    Checkbox _showHpMobile, _highlightByPoisoned, _highlightByParalyzed, _highlightByInvul, _drawRoofs, _treeToStumps;
    Checkbox _hideVegetation, _noColorOutOfRangeObjects, _useCircleOfTransparency, _enableTopbar, _holdDownKeyTab;
    Checkbox _holdDownKeyAlt, _closeAllAnchoredGumpsWithRClick, _chatAfterEnter, _chatAdditionalButtonsCheckbox;
    Checkbox _chatShiftEnterCheckbox, _enableCaveBorder, _holdShiftForContext, _holdShiftToSplitStack;
    Checkbox _reduceFPSWhenInactive, _sallosEasyGrab, _partyInviteGump, _objectsFading, _textFading, _holdAltToMoveGumps;
    Combobox _hpComboBox, _healtbarType, _fieldsType, _hpComboBoxShowWhen, _infoBarHighlightType;
    OptionsHue _innocentColor, _friendColor, _criminalColor, _canAttackColor, _enemyColor, _murdererColor;
    OptionsHue _neutralColor, _beneficColor, _harmfulColor, _poisonColor, _paralyzedColor, _invulnerableColor;
    HSliderBar _lightBar;
    Checkbox _buffBarTime, _uiButtonsSingleClick, _queryBeforeAttack, _queryBeforeBeneficial, _spellColoring;
    Checkbox _spellFormatCheckbox, _enableFastSpellsAssign, _newTargetSystem, _showDPS;
    Combobox _overrideAllFontsIsUnicode, _overrideContainerLocationSetting;
    Checkbox _overrideAllFonts, _restoreZoom, _zoomCheckbox, _scaleSpeechDelay, _saveJournalCheckBox;
    OptionsInput _highlightAmount, _abbreviatedAmount, _rows, _columns, _autoOpenCorpseRange, _spellFormatBox, _macroName;
    Checkbox _showHouseContent, _showInfoBar, _ignoreAllianceMessages, _ignoreGuildMessages;
    Checkbox _useAlternateJournal, _partyMessagesOverhead;
    HSliderBar _sliderFPS, _circleOfTranspRadius, _sliderSpeechDelay, _sliderScreenZoom, _sliderZoom;
    HSliderBar _soundsVolume, _musicVolume, _loginMusicVolume, _dragSelectStartX, _dragSelectStartY, _showSkillsMessageDelta;
    HSliderBar _terrainShadowLevel;
    OptionsHue _speechColor, _emoteColor, _yellColor, _whisperColor, _partyMessageColor, _guildMessageColor;
    OptionsHue _allyMessageColor, _chatMessageColor, _partyAuraColor, _tooltipFontHue;
    Checkbox _useOldStatus, _statusGumpBarMutuallyExclusive, _windowBorderless, _enableDeathScreen, _enableBlackWhiteEffect;
    Checkbox _altLights, _enableLight, _enableShadows, _enableShadowsStatics, _auraMouse, _runMouseInSeparateThread;
    Checkbox _useColoredLights, _darkNights, _partyAura, _hideChatGradient, _animatedWaterEffect, _useSmoothBoat;
    Combobox _lightLevelType, _auraType, _gridLoot;
    Checkbox _useTooltip, _useStandardSkillsGump, _showMobileNameIncoming, _showCorpseNameIncoming;
    Checkbox _showStatsMessage, _showSkillsMessage;
    ScrollArea _macroList;
    OptionsStack _infoStack;

    public OptionsGump(World world) : base(world, 0, 0) {
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        Width = WidthPx;
        Height = HeightPx;
        var screen = Mouse.Current;
        var bw = screen.BufW > 0 ? screen.BufW : 800;
        var bh = screen.BufH > 0 ? screen.BufH : 600;
        X = Math.Max(0, (bw - WidthPx) / 2);
        Y = Math.Max(0, (bh - HeightPx) / 2);

        var pages = new[] {
            "General", "Sound", "Video", "Macros", "Tooltip", "Fonts", "Speech", "Combat-Spells",
            "Counters", "Info Bar", "Containers", "Experimental",
        };
        for (var i = 0; i < pages.Length; i++)
            _pageButtons.Add(Add(new OptionsTextButton(10, 10 + 30 * i, 140, 25, pages[i], i + 1, true)));
        Add(new OptionsTextButton(10, 10 + 30 * pages.Length, 140, 25, "Ignore List", (int)Buttons.OpenIgnoreList, false));

        Add(new Button((int)Buttons.Cancel, 0x00F3, 0x00F1, 0x00F2) {
            X = 214, Y = 465, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Apply, 0x00EF, 0x00F0, 0x00EE) {
            X = 308, Y = 465, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Default, 0x00F6, 0x00F4, 0x00F5) {
            X = 406, Y = 465, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Ok, 0x00F9, 0x00F8, 0x00F7) {
            X = 503, Y = 465, ButtonAction = ButtonAction.Activate,
        });

        var macros = new MacroManager(World);
        macros.Load();
        var loaded = macros.GetAllMacros();
        for (var i = 0; i < loaded.Count; i++)
            if (!string.IsNullOrEmpty(loaded[i].Name)) MacroNames.Add(loaded[i].Name);
        if (MacroNames.Count > 0) _selectedMacro = 0;

        BuildGeneral();
        BuildSounds();
        BuildVideo();
        BuildMacros();
        BuildTooltip();
        BuildFonts();
        BuildSpeech();
        BuildCombat();
        BuildCounters();
        BuildInfoBar();
        BuildContainers();
        BuildExperimental();
        ChangePage(1);
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        // The ClassicUO logo texture is not drawn.
        var sink = GumpArt.Sink;
        if (sink != null && Width > 2 && Height > 2) {
            sink.DrawRaw(FillKey, x + 1, y + 1, Width - 2, Height - 2, Dark, 1, 1);
            sink.DrawRaw(LineKey, x, y, Width, 1, Gray, 1, 1);
            sink.DrawRaw(LineKey, x, y + Height - 1, Width, 1, Gray, 1, 1);
            sink.DrawRaw(LineKey, x, y, 1, Height, Gray, 1, 1);
            sink.DrawRaw(LineKey, x + Width - 1, y, 1, Height, Gray, 1, 1);
            sink.DrawRaw(LineKey, x + 160, y + 5, 1, Height - 10, Gray, 1, 1);
            sink.DrawRaw(LineKey, x + 160, y + 441, Width - 160, 1, Gray, 1, 1);
            if (ActivePage == 4) {
                sink.DrawRaw(LineKey, x + 190, y + 79, 150, 1, Gray, 1, 1);
                sink.DrawRaw(LineKey, x + 341, y + 21, 1, 418, Gray, 1, 1);
            }
        }
        return base.Draw(x, y);
    }

    public void ChangePage(int page) {
        ActivePage = page;
        for (var i = 0; i < _pageButtons.Count; i++)
            _pageButtons[i].IsSelected = _pageButtons[i].ButtonParameter == page;
    }

    /// <summary>Writes the stored window numbers back into the video page. There is no camera.</summary>
    public void UpdateVideo() {
        _gameWindowPositionX?.SetText(GameWindowX.ToString());
        _gameWindowPositionY?.SetText(GameWindowY.ToString());
        _gameWindowWidth?.SetText(GameWindowWidth.ToString());
        _gameWindowHeight?.SetText(GameWindowHeight.ToString());
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.Cancel:
                Dispose();
                break;
            case Buttons.Apply:
                Apply();
                break;
            case Buttons.Default:
                SetDefault();
                break;
            case Buttons.Ok:
                Apply();
                Dispose();
                break;
            case Buttons.OpenIgnoreList:
                UIManager.GetGump<IgnoreManagerGump>()?.Dispose();
                UIManager.Add(new IgnoreManagerGump(World));
                break;
            case Buttons.NewMacro:
                AddMacro();
                break;
            case Buttons.DeleteMacro:
                AskDeleteMacro();
                break;
        }
    }

    void BuildGeneral() {
        var area = PageArea(1);
        var section = AddSection(area, "General", 5);
        section.Add(_highlightObjects = Check("Highlight game objects", HighlightGameObjects));
        section.Add(_enablePathfind = Check("Enable pathfinding", EnablePathfind));
        section.AddRight(_useShiftPathfind = Check("Use SHIFT for pathfinding", UseShiftToPathfind));
        section.Add(_alwaysRun = Check("Always run", AlwaysRun));
        section.AddRight(_alwaysRunUnlessHidden = Check("Unless hidden", AlwaysRunUnlessHidden));
        section.Add(_fastRotation = Check("Fast rotation", FastRotation));
        section.Add(_autoOpenDoors = Check("Auto Open Doors", AutoOpenDoors));
        section.AddRight(_smoothDoors = Check("Smooth doors", SmoothDoors));
        section.Add(_autoOpenCorpse = Check("Auto Open Corpses", AutoOpenCorpses));
        section.PushIndent();
        section.Add(Caption("Corpse Open Range:"));
        section.AddRight(_autoOpenCorpseRange = Input(50, AutoOpenCorpseRange.ToString(), 5, true));
        section.Add(_skipEmptyCorpse = Check("Skip empty corpses", SkipEmptyCorpse));
        section.Add(Caption("Corpse Open Options:"));
        section.AddRight(_autoOpenCorpseOptions = Combo(new[] { "None", "Not Targeting", "Not Hiding", "Both" }, CorpseOpenOptions, 150), 2);
        section.PopIndent();
        section.Add(_noColorOutOfRangeObjects = Check("No color for object out of range", NoColorObjectsOutOfRange));
        section.Add(_sallosEasyGrab = Check("Sallos easy grab", SallosEasyGrab));
        section.Add(_showHouseContent = Check("Show houses content", ShowHouseContent));
        section.Add(_useSmoothBoat = Check("Smooth boat movements", UseSmoothBoatMovement));
        var version = UltimaOnline.Current?.Version ?? 0;
        _showHouseContent.IsVisible = version >= ClientVersion.CV_70796;
        _useSmoothBoat.IsVisible = version >= ClientVersion.CV_7090;

        var mobiles = AddSection(area, "Mobiles", section.Y + section.Height + 40);
        mobiles.Add(_showHpMobile = Check("Show HP", ShowMobilesHP));
        mobiles.AddRight(_hpComboBox = Combo(new[] { "Percentage", "Line", "Both" }, MobileHPType, 100));
        mobiles.AddRight(Caption("mode:"));
        mobiles.AddRight(_hpComboBoxShowWhen = Combo(new[] { "Always", "Less than 100 %", "Smart" }, MobileHPShowWhen, 100), 2);
        mobiles.Add(_highlightByPoisoned = Check("Highlight poisoned", HighlightMobilesByPoisoned));
        mobiles.PushIndent();
        mobiles.Add(_poisonColor = HueBox(PoisonHue));
        mobiles.AddRight(Caption("Poisoned Color"), 2);
        mobiles.PopIndent();
        mobiles.Add(_highlightByParalyzed = Check("Highlight paralyzed", HighlightMobilesByParalize));
        mobiles.PushIndent();
        mobiles.Add(_paralyzedColor = HueBox(ParalyzedHue));
        mobiles.AddRight(Caption("Paralyzed Color"), 2);
        mobiles.PopIndent();
        mobiles.Add(_highlightByInvul = Check("Highlight invulnerable", HighlightMobilesByInvul));
        mobiles.PushIndent();
        mobiles.Add(_invulnerableColor = HueBox(InvulnerableHue));
        mobiles.AddRight(Caption("Invulnerable Color"), 2);
        mobiles.PopIndent();
        mobiles.Add(_showMobileNameIncoming = Check("Show incoming new mobiles", ShowNewMobileNameIncoming));
        mobiles.Add(_showCorpseNameIncoming = Check("Show incoming new corpses", ShowNewCorpseNameIncoming));
        mobiles.Add(Caption("Aura under feet"));
        mobiles.AddRight(_auraType = Combo(new[] { "None", "Warmode", "Ctrl+Shift", "Always" }, AuraUnderFeetType, 100), 2);
        mobiles.PushIndent();
        mobiles.Add(_partyAura = Check("Custom color aura for party members", PartyAura));
        mobiles.PushIndent();
        mobiles.Add(_partyAuraColor = HueBox(PartyAuraHue));
        mobiles.AddRight(Caption("Party Aura Color"));
        mobiles.PopIndent();
        mobiles.PopIndent();

        var gumps = AddSection(area, "Gumps & Context", mobiles.Y + mobiles.Height + 40);
        gumps.Add(_enableTopbar = Check("Disable the Menu Bar", TopbarGumpIsDisabled));
        gumps.Add(_holdDownKeyAlt = Check("Hold ALT key + right click to close Anchored gumps", HoldDownKeyAltToCloseAnchored));
        gumps.Add(_holdAltToMoveGumps = Check("Hold ALT key to move gumps", HoldAltToMoveGumps));
        gumps.Add(_closeAllAnchoredGumpsWithRClick = Check("Close all Anchored gumps when right click on a group", CloseAllAnchoredGumpsInGroupWithRightClick));
        gumps.Add(_useStandardSkillsGump = Check("Use standard skills gump", StandardSkillsGump));
        gumps.Add(_useOldStatus = Check("Use old status gump", UseOldStatusGump));
        gumps.Add(_statusGumpBarMutuallyExclusive = Check("Status Gump and Health Bar are mutually exclusive", StatusGumpBarMutuallyExclusive));
        gumps.Add(_partyInviteGump = Check("Show gump for party invites", PartyInviteGump));
        gumps.Add(_customBars = Check("Use custom healthbars gump", CustomBarsToggled));
        gumps.AddRight(_customBarsBBG = Check("Opaque background", CBBlackBGToggled));
        gumps.Add(_saveHealthbars = Check("Save healthbars on logout", SaveHealthbars));
        gumps.PushIndent();
        gumps.Add(Caption("Close healthbar gump when:"));
        gumps.AddRight(_healtbarType = Combo(new[] { "None", "Mobile out of range", "Mobile is dead" }, CloseHealthBarType, 150));
        gumps.PopIndent();
        gumps.Add(Caption("Grid loot"));
        gumps.AddRight(_gridLoot = Combo(new[] { "None", "Grid loot only", "Both" }, GridLootType, 120), 2);
        gumps.Add(_holdShiftForContext = Check("Hold Shift for Context Menus", HoldShiftForContext));
        gumps.Add(_holdShiftToSplitStack = Check("Hold Shift to split stack of items", HoldShiftToSplitStack));

        var misc = AddSection(area, "Miscellaneous", gumps.Y + gumps.Height + 40);
        misc.Add(_useCircleOfTransparency = Check("Enable circle of transparency", UseCircleOfTransparency));
        misc.AddRight(_circleOfTranspRadius = Slider(50, 200, CircleOfTransparencyRadius, 200));
        misc.PushIndent();
        misc.Add(Caption("Transparency type:"));
        misc.AddRight(_cotType = Combo(new[] { "Full", "Gradient" }, CircleOfTransparencyType, 150), 2);
        misc.PopIndent();
        misc.Add(_hideScreenshotStoredInMessage = Check("Hide \"Screenshot stored in\" message", HideScreenshotStoredInMessage));
        misc.Add(_objectsFading = Check("Objects alpha fading", UseObjectsFading));
        misc.Add(_textFading = Check("Text alpha fading", TextFading));
        misc.Add(_showTargetRangeIndicator = Check("Show target range indicator", ShowTargetRangeIndicator));
        misc.Add(_nameOverheadAlwaysOn = Check("Always show name overheads", NameOverheadToggled));
        misc.Add(_nameOverheadShowHpBar = Check("Show HP bar on name overheads", NameOverheadShowHpBar));
        misc.Add(_enableDragSelect = Check("Enable drag-select to open health bars", EnableDragSelect));
        misc.PushIndent();
        misc.Add(Caption("Drag-select modifier key"));
        misc.AddRight(_dragSelectModifierKey = Combo(new[] { "None", "Ctrl", "Shift" }, DragSelectModifierKey, 100));
        misc.Add(_dragSelectHumanoidsOnly = Check("Select humanoids only", DragSelectHumanoidsOnly));
        misc.Add(_dragSelectHostileOnly = Check("Select hostiles only", DragSelectHostileOnly));
        misc.Add(Caption("Starting X position of health bars"));
        misc.Add(_dragSelectStartX = Slider(0, ScreenW(), DragSelectStartX, 200));
        misc.Add(Caption("Starting Y position of health bars"));
        misc.Add(_dragSelectStartY = Slider(0, ScreenH(), DragSelectStartY, 200));
        misc.Add(_dragSelectAsAnchor = Check("Anchored Healthbar", DragSelectAsAnchor));
        misc.PopIndent();
        misc.Add(_showStatsMessage = Check("Inform when stats change", ShowStatsChangedMessage));
        misc.Add(_showSkillsMessage = Check("Inform when skills change by (in tenths)", ShowSkillsChangedMessage));
        misc.PushIndent();
        misc.AddRight(_showSkillsMessageDelta = Slider(0, 100, ShowSkillsChangedDeltaValue, 150));
        misc.PopIndent();
        var saveDefault = new OptionsTextButton(0, 0, Math.Max(40, misc.Width - 18), 25, "Save current settings as defaults for new users", -1, false);
        saveDefault.IsSelected = true;
        saveDefault.Clicked = () => {
            // Profile path is not ported.
        };
        misc.Add(saveDefault);

        var terrain = AddSection(area, "Terrain & Statics", misc.Y + misc.Height + 40);
        terrain.Add(_drawRoofs = Check("Hide roof tiles", !DrawRoofs));
        terrain.Add(_treeToStumps = Check("Trees to stumps", TreeToStumps));
        terrain.Add(_hideVegetation = Check("Hide vegetation", HideVegetation));
        terrain.Add(_enableCaveBorder = Check("Mark cave tiles", EnableCaveBorder));
        terrain.Add(Caption("Fields:"));
        terrain.AddRight(_fieldsType = Combo(new[] { "Normal fields", "Static fields", "Tile fields" }, FieldsType, 150));
    }

    void BuildSounds() {
        var area = PageArea(2);
        var y = 5;
        Place(area, _enableSounds = Check("Sounds", EnableSound), 5, y);
        Place(area, _soundsVolume = Slider(0, 100, SoundVolume, 200), 120, y + 2);
        y += Row(_enableSounds) + 2;
        Place(area, _enableMusic = Check("Music", EnableMusic), 5, y);
        Place(area, _musicVolume = Slider(0, 100, MusicVolume, 200), 120, y + 2);
        y += Row(_enableMusic) + 2;
        Place(area, _loginMusic = Check("Login music", LoginMusic), 5, y);
        Place(area, _loginMusicVolume = Slider(0, 100, LoginMusicVolume, 200), 120, y + 2);
        y += Row(_loginMusic) + 4;
        Place(area, _footStepsSound = Check("Play Footsteps", EnableFootstepsSound), 5, y);
        y += Row(_footStepsSound) + 2;
        Place(area, _combatMusic = Check("Combat music", EnableCombatMusic), 5, y);
        y += Row(_combatMusic) + 2;
        Place(area, _musicInBackground = Check("Reproduce sounds and music when ClassicUO is not focused", ReproduceSoundsInBackground), 5, y);
    }

    void BuildVideo() {
        var area = PageArea(3);
        var fpsLabel = Caption("- FPS:");
        Place(area, fpsLabel, 5, 5);
        Place(area, _sliderFPS = Slider(12, 250, Fps, 250), 5 + fpsLabel.Width + 5, 5);
        var y = 5 + Row(fpsLabel) + 5;
        Place(area, _reduceFPSWhenInactive = Check("Reduce FPS when game is inactive", ReduceFPSWhenInactive), 5 + fpsLabel.Width + 5, y);
        y += Row(_reduceFPSWhenInactive) + 22;

        var window = AddSection(area, "Game window", y);
        window.Add(_gameWindowFullsize = Check("Always use fullsize game window", GameWindowFullSize));
        window.Add(_windowBorderless = Check("Borderless window", WindowBorderless));
        window.Add(_gameWindowLock = Check("Lock game window moving/resizing", GameWindowLock));
        window.Add(Caption("Game Play Window Position:"));
        window.AddRight(_gameWindowPositionX = Input(50, GameWindowX.ToString(), 6, true), 4);
        window.AddRight(_gameWindowPositionY = Input(50, GameWindowY.ToString(), 6, true));
        window.Add(Caption("Game Play Window Size:"));
        window.AddRight(_gameWindowWidth = Input(50, GameWindowWidth.ToString(), 6, true));
        window.AddRight(_gameWindowHeight = Input(50, GameWindowHeight.ToString(), 6, true));

        var zoom = AddSection(area, "Zoom", window.Y + window.Height + 40);
        zoom.Add(Caption("Screen zoom:"));
        zoom.AddRight(_sliderScreenZoom = Slider(-ScreenZoomSteps, ScreenZoomSteps, GetScreenZoom(ScreenScale), 250));
        zoom.Add(Caption("Default zoom:"));
        zoom.AddRight(_sliderZoom = Slider(0, ScreenZoomSteps, ZoomIndex, 100));
        zoom.Add(_zoomCheckbox = Check("Enable mousewheel for in game zoom scaling (Ctrl + Scroll)", EnableMousewheelScaleZoom));
        zoom.Add(_restoreZoom = Check("Releasing Ctrl Restores Scale", RestoreScaleAfterUnpressCtrl));

        var lights = AddSection(area, "Lights", zoom.Y + zoom.Height + 40);
        lights.Add(_altLights = Check("Alternative lights", UseAlternativeLights));
        lights.Add(_enableLight = Check("Light level", UseCustomLightLevel));
        lights.AddRight(_lightBar = Slider(0, 0x1E, 0x1E - LightLevel, 250));
        lights.Add(Caption("Light Level Setting Type"));
        lights.AddRight(_lightLevelType = Combo(new[] { "Absolute", "Minimum" }, LightLevelType, 150));
        lights.Add(_darkNights = Check("Dark nights", UseDarkNights));
        lights.Add(_useColoredLights = Check("Use colored lights", UseColoredLights));

        var misc = AddSection(area, "Misc", lights.Y + lights.Height + 40);
        misc.Add(_enableDeathScreen = Check("Enable Death Screen", EnableDeathScreen));
        misc.AddRight(_enableBlackWhiteEffect = Check("Black & White mode for dead player", EnableBlackWhiteEffect));
        misc.Add(_runMouseInSeparateThread = Check("Run mouse in a separate thread", RunMouseInSeparateThread));
        misc.Add(_auraMouse = Check("Aura on mouse target", AuraOnMouse));
        misc.Add(_animatedWaterEffect = Check("Animated water effect", AnimatedWaterEffect));

        var shadows = AddSection(area, "Shadows", misc.Y + misc.Height + 40);
        shadows.Add(_enableShadows = Check("Shadows", ShadowsEnabled));
        shadows.PushIndent();
        shadows.Add(_enableShadowsStatics = Check("Show tree and rock shadows", ShadowsStatics));
        shadows.PopIndent();
        shadows.Add(Caption("Terrain shadows level:"));
        shadows.AddRight(_terrainShadowLevel = Slider(5, 25, TerrainShadowsLevel, 200));
    }

    void BuildMacros() {
        var add = new OptionsTextButton(190, 20, 130, 20, "New macro", (int)Buttons.NewMacro, false);
        Add(add, 4);
        var del = new OptionsTextButton(190, 52, 130, 20, "Delete macro", (int)Buttons.DeleteMacro, false);
        Add(del, 4);
        _macroList = new ScrollArea(190, 81, 150, 360, true) { WantUpdateSize = false };
        if (_macroList.Width < 1) _macroList.Width = 150;
        if (_macroList.Height < 1) _macroList.Height = 360;
        Add(_macroList, 4);
        var name = Caption("Macro name:");
        name.X = 360;
        name.Y = 20;
        Add(name, 4);
        _macroName = Input(160, "", 32, false);
        _macroName.X = 360;
        _macroName.Y = 40;
        Add(_macroName, 4);
        // MacroControl is not in this gump. The list is the name of each macro.
        FillMacros();
    }

    void BuildTooltip() {
        var area = PageArea(5);
        var y = 5;
        Place(area, _useTooltip = Check("Use tooltip", UseTooltip), 5, y);
        y += Row(_useTooltip) + 2;
        var x = 45;
        var delay = Caption("Delay before display:");
        Place(area, delay, x, y);
        Place(area, _delayBeforeTooltip = Slider(0, 1000, TooltipDelayBeforeDisplay, 200), x + delay.Width + 5, y);
        y += Row(delay) + 2;
        var zoom = Caption("Tooltip zoom:");
        Place(area, zoom, x, y);
        Place(area, _tooltipZoom = Slider(100, 200, TooltipDisplayZoom, 200), x + zoom.Width + 5, y);
        y += Row(zoom) + 2;
        var opacity = Caption("Tooltip background opacity:");
        Place(area, opacity, x, y);
        Place(area, _tooltipOpacity = Slider(0, 100, TooltipBackgroundOpacity, 200), x + opacity.Width + 5, y);
        y += Row(opacity) + 2;
        Place(area, _tooltipFontHue = HueBox(TooltipTextHue), x, y);
        Place(area, Caption("Tooltip font hue"), x + 32, y);
        y += 30;
        var font = Caption("Tooltip font:");
        Place(area, font, x, y);
        y += Row(font) + 2;
        _tooltipFontSelector = new OptionsFontSelector(7, TooltipFont, "Tooltip font!");
        Place(area, _tooltipFontSelector, x + 40, y);
    }

    void BuildFonts() {
        var area = PageArea(6);
        Place(area, _overrideAllFonts = Check("Override game font", OverrideAllFonts), 5, 5);
        Place(area, _overrideAllFontsIsUnicode = Combo(new[] { "ASCII", "Unicode" }, OverrideAllFontsIsUnicode ? 1 : 0, 100), 5 + _overrideAllFonts.Width + 5, 5);
        var y = 5 + Row(_overrideAllFonts) + 2;
        Place(area, _forceUnicodeJournal = Check("Force Unicode in journal", ForceUnicodeJournal), 5, y);
        y += Row(_forceUnicodeJournal) + 2;
        var font = Caption("Speech font:");
        Place(area, font, 5, y);
        y += Row(font) + 2;
        _fontSelectorChat = new OptionsFontSelector(20, ChatFont, "That's ClassicUO!");
        Place(area, _fontSelectorChat, 45, y);
    }

    void BuildSpeech() {
        var area = PageArea(7);
        var y = 5;
        Place(area, _scaleSpeechDelay = Check("Scale speech delay", ScaleSpeechDelay), 5, y);
        Place(area, _sliderSpeechDelay = Slider(0, 1000, SpeechDelay, 180), 5 + _scaleSpeechDelay.Width + 5, y);
        y += Row(_scaleSpeechDelay) + 2;
        Place(area, _saveJournalCheckBox = Check("Save Journal to file in game folder", SaveJournalToFile), 5, y);
        y += Row(_saveJournalCheckBox) + 2;
        var x = 45;
        Place(area, _maxJournalFilesEnabled = Check("Keep at most this many journal files:", MaxJournalFiles >= 0), x, y);
        _maxJournalFiles = Input(50, MaxJournalFiles < 0 ? "100" : MaxJournalFiles.ToString(), 6, true);
        Place(area, _maxJournalFiles, x + _maxJournalFilesEnabled.Width + 5, y);
        y += Row(_maxJournalFiles) + 7;
        Place(area, _journalFileWithSerial = Check("Add speaker serial to journal entries", JournalFileWithSerial), x, y);
        y += Row(_journalFileWithSerial) + 7;
        Place(area, _chatAfterEnter = Check("Active chat when pressing ENTER", ActivateChatAfterEnter), 5, y);
        y += Row(_chatAfterEnter) + 2;
        Place(area, _chatAdditionalButtonsCheckbox = Check("Use additional buttons to activate chat: ! ; : / \\ , . [ | -", ActivateChatAdditionalButtons), x, y);
        y += Row(_chatAdditionalButtonsCheckbox) + 2;
        Place(area, _chatShiftEnterCheckbox = Check("Use `Shift+Enter` to send message without closing chat", ActivateChatShiftEnterSupport), x, y);
        y += Row(_chatShiftEnterCheckbox) + 2;
        Place(area, _hideChatGradient = Check("Hide chat gradient", HideChatGradient), 5, y);
        y += Row(_hideChatGradient) + 2;
        Place(area, _ignoreGuildMessages = Check("Hide guild chat", IgnoreGuildMessages), 5, y);
        y += Row(_ignoreGuildMessages) + 2;
        Place(area, _ignoreAllianceMessages = Check("Hide alliance chat", IgnoreAllianceMessages), 5, y);
        y += Row(_ignoreAllianceMessages) + 2;
        Place(area, _useAlternateJournal = Check("Use alternative journal", UseAlternateJournal), 5, y);
        y += Row(_useAlternateJournal) + 2;
        Place(area, _partyMessagesOverhead = Check("Show party messages overhead", OverheadPartyMessages), 5, y);
        y += 35;
        var random = new OptionsTextButton(5, y, 140, 25, "Randomize speech hues", -1, false);
        random.Clicked = RandomizeSpeech;
        area.Add(random);
        y += 47;
        PlaceHue(area, _speechColor = HueBox(SpeechHue), "Speech Color", 5, y);
        PlaceHue(area, _emoteColor = HueBox(EmoteHue), "Emote Color", 205, y);
        y += 24;
        PlaceHue(area, _yellColor = HueBox(YellHue), "Yell Color", 5, y);
        PlaceHue(area, _whisperColor = HueBox(WhisperHue), "Whisper Color", 205, y);
        y += 24;
        PlaceHue(area, _partyMessageColor = HueBox(PartyMessageHue), "Party Message Color", 5, y);
        PlaceHue(area, _guildMessageColor = HueBox(GuildMessageHue), "Guild Message Color", 205, y);
        y += 24;
        PlaceHue(area, _allyMessageColor = HueBox(AllyMessageHue), "Alliance Message Color", 5, y);
        PlaceHue(area, _chatMessageColor = HueBox(ChatMessageHue), "Chat Message Color", 205, y);
    }

    void BuildCombat() {
        var area = PageArea(8);
        var y = 5;
        Place(area, _newTargetSystem = Check("Use new target system", UseNewTargetSystem), 5, y);
        y += Row(_newTargetSystem) + 2;
        Place(area, _holdDownKeyTab = Check("Hold TAB key for combat", HoldDownKeyTab), 5, y);
        y += Row(_holdDownKeyTab) + 2;
        Place(area, _queryBeforeAttack = Check("Query before attack", EnabledCriminalActionQuery), 5, y);
        y += Row(_queryBeforeAttack) + 2;
        Place(area, _queryBeforeBeneficial = Check("Query before performing beneficial acts on\nMurderers, Criminals, Grays (Monsters/Animals)", EnabledBeneficialCriminalActionQuery), 5, y);
        y += Row(_queryBeforeBeneficial) + 2;
        Place(area, _spellFormatCheckbox = Check("Enable Overhead Spell Format", EnabledSpellFormat), 5, y);
        y += Row(_spellFormatCheckbox) + 2;
        Place(area, _spellColoring = Check("Enable Overhead Spell Hue", EnabledSpellHue), 5, y);
        y += Row(_spellColoring) + 2;
        Place(area, _uiButtonsSingleClick = Check("Single-click UI buttons", CastSpellsByOneClick), 5, y);
        y += Row(_uiButtonsSingleClick) + 2;
        Place(area, _buffBarTime = Check("Show buff duration", BuffBarTime), 5, y);
        y += Row(_buffBarTime) + 2;
        Place(area, _enableFastSpellsAssign = Check("Enable Fast Spells Assign (Ctrl + Alt)", FastSpellsAssign), 5, y);
        y += Row(_enableFastSpellsAssign) + 2;
        Place(area, _showDPS = Check("Show DPS with damage numbers", ShowDPSWithDamageNumbers), 5, y);
        y += 30;
        var left = y;
        PlaceHue(area, _innocentColor = HueBox(InnocentHue), "Innocent Color", 5, y);
        y += 24;
        PlaceHue(area, _friendColor = HueBox(FriendHue), "Friend Color", 5, y);
        y += 24;
        PlaceHue(area, _criminalColor = HueBox(CriminalHue), "Criminal Color", 5, y);
        y += 24;
        PlaceHue(area, _canAttackColor = HueBox(CanAttackHue), "Can Be Attacked Color", 5, y);
        y += 24;
        PlaceHue(area, _murdererColor = HueBox(MurdererHue), "Murderer Color", 5, y);
        y += 24;
        PlaceHue(area, _enemyColor = HueBox(EnemyHue), "Enemy Color", 5, y);
        y = left;
        PlaceHue(area, _beneficColor = HueBox(BeneficHue), "Benefic Spell Hue", 205, y);
        y += 24;
        PlaceHue(area, _harmfulColor = HueBox(HarmfulHue), "Harmful Spell Hue", 205, y);
        y += 24;
        PlaceHue(area, _neutralColor = HueBox(NeutralHue), "Neutral Spell Hue", 205, y);
        y += 24 * 5;
        var format = Caption("Spell Overhead format: ({power} for powerword - {spell} for spell name)");
        Place(area, format, 5, y);
        y += Row(format) + 2;
        Place(area, _spellFormatBox = Input(200, SpellDisplayFormat, 30, false), 5, y);
    }

    void BuildCounters() {
        var area = PageArea(9);
        var y = 5;
        Place(area, _enableCounters = Check("Enable Counters", CounterBarEnabled), 5, y);
        y += Row(_enableCounters) + 2;
        Place(area, _highlightOnChange = Check("Highlight On Change", CounterBarHighlightOnChange), 45, y);
        y += Row(_highlightOnChange) + 2;
        Place(area, _enableAbbreviatedAmount = Check("Enable abbreviated amount values\nwhen amount is or exceeds", CounterBarDisplayAbbreviatedAmount), 45, y);
        Place(area, _abbreviatedAmount = Input(50, CounterBarAbbreviatedAmount.ToString(), 6, true), 45 + _enableAbbreviatedAmount.Width + 5, y);
        y += Row(_enableAbbreviatedAmount) + 2;
        Place(area, _highlightOnAmount = Check("Highlight red when amount is below", CounterBarHighlightOnAmount), 45, y);
        Place(area, _highlightAmount = Input(50, CounterBarHighlightAmount.ToString(), 6, true), 45 + _highlightOnAmount.Width + 5, y);
        y += Row(_highlightAmount) + 45;
        var layout = Caption("Counter Layout:");
        Place(area, layout, 45, y);
        y += Row(layout) + 2;
        var cell = Caption("Cell size:");
        Place(area, cell, 85, y);
        Place(area, _cellSize = Slider(30, 80, CounterBarCellSize, 80), 85 + cell.Width + 5, y);
        // ClassicUO declares these and only SetDefault writes them.
        _rows = Input(50, "1", 4, true);
        _columns = Input(50, "1", 4, true);
    }

    void BuildInfoBar() {
        var area = PageArea(10);
        var y = 5;
        Place(area, _showInfoBar = Check("Show Info Bar", ShowInfoBar), 5, y);
        y += Row(_showInfoBar) + 2;
        var kind = Caption("Data highlight type:");
        Place(area, kind, 45, y);
        Place(area, _infoBarHighlightType = Combo(new[] { "Text color", "Colored bars" }, InfoBarHighlightType, 150), 45 + kind.Width + 5, y);
        y += 30;
        var add = new OptionsTextButton(5, y, 90, 20, "+ Add item", -1, false) { IsSelected = true };
        add.Clicked = () => AddInfoRow(new InfoBarEntry("", 0, 0x3B9));
        area.Add(add);
        y += 40;
        Place(area, Caption("Label"), 5, y);
        Place(area, Caption("Color"), 155, y);
        Place(area, Caption("Data"), 210, y);
        y += 38;
        _infoStack = new OptionsStack { X = 5, Y = y, Width = 460 };
        area.Add(_infoStack);
        for (var i = 0; i < InfoBars.Count; i++) AddInfoRow(InfoBars[i]);
    }

    void BuildContainers() {
        var area = PageArea(11);
        var version = UltimaOnline.Current?.Version ?? 0;
        var hasBackpacks = version >= ClientVersion.CV_705301;
        var y = 5;
        var x = 5;
        if (hasBackpacks) {
            var style = Caption("Select Character Backpack Style");
            Place(area, style, x, y);
            x += style.Width + 5;
        }
        Place(area, _backpackStyle = Combo(new[] { "Default", "Suede", "PolarBear", "GhoulSkin" }, BackpackStyle, 200), x, y);
        _backpackStyle.IsVisible = hasBackpacks;
        if (hasBackpacks) y += Row(_backpackStyle) + 12;
        x = 5;
        var scale = Caption("- Container scale:");
        Place(area, scale, x, y);
        Place(area, _containersScale = Slider(50, 200, ContainersScale, 200), x + scale.Width + 5, y);
        y += Row(scale) + 2;
        Place(area, _containerScaleItems = Check("Scale items inside containers", ScaleItemsInsideContainers), x, y);
        y += Row(_containerScaleItems) + 2;
        Place(area, _useLargeContainers = Check("Use large containers gump", UseLargeContainerGumps), x, y);
        _useLargeContainers.IsVisible = version >= ClientVersion.CV_706000;
        if (_useLargeContainers.IsVisible) y += Row(_useLargeContainers) + 2;
        Place(area, _containerDoubleClickToLoot = Check("Double click to loot items inside containers", DoubleClickToLootInsideContainers), x, y);
        y += Row(_containerDoubleClickToLoot) + 2;
        Place(area, _relativeDragAndDropItems = Check("Relative drag and drop items in containers", RelativeDragAndDropItems), x, y);
        y += Row(_relativeDragAndDropItems) + 2;
        Place(area, _highlightContainersWhenMouseIsOver = Check("Highlight container when mouse is over a container gump", HighlightContainerWhenSelected), x, y);
        y += Row(_highlightContainersWhenMouseIsOver) + 2;
        Place(area, _hueContainerGumps = Check("Recolor container gump by item hue", HueContainerGumps), x, y);
        y += Row(_hueContainerGumps) + 2;
        Place(area, _overrideContainerLocation = Check("Override container gump location", OverrideContainerLocation), x, y);
        Place(area, _overrideContainerLocationSetting = Combo(new[] {
            "Near container position", "Top right", "Last dragged position", "Remember every container",
        }, OverrideContainerLocationSetting, 200), x + _overrideContainerLocation.Width + 5, y);
        y += Row(_overrideContainerLocation) + 12;
        var rebuild = new OptionsTextButton(x, y, 130, 30, "Rebuild containers.txt", -1, false) { IsSelected = true };
        rebuild.Clicked = () => {
            // ContainerManager is not held on World. Profile path is not ported.
        };
        area.Add(rebuild);
    }

    void BuildExperimental() {
        var area = PageArea(12);
        var y = 5;
        Place(area, _disableDefaultHotkeys = Check("Disable default UO hotkeys", DisableDefaultHotkeys), 5, y);
        y += Row(_disableDefaultHotkeys) + 2;
        Place(area, _disableArrowBtn = Check("Disable arrows & numlock arrows (player moving)", DisableArrowBtn), 45, y);
        y += Row(_disableArrowBtn) + 2;
        Place(area, _disableTabBtn = Check("Disable TAB (toggle warmode)", DisableTabBtn), 45, y);
        y += Row(_disableTabBtn) + 2;
        Place(area, _disableCtrlQWBtn = Check("Disable Ctrl + Q/W (messageHistory)", DisableCtrlQWBtn), 45, y);
        y += Row(_disableCtrlQWBtn) + 2;
        Place(area, _disableAutoMove = Check("Disable Right+Left Click Automove", DisableAutoMove), 45, y);
        y += Row(_disableAutoMove) + 12;
        var reload = new OptionsTextButton(5, y, 190, 20, "Reload our art, gumps, sounds and strings", -1, false);
        reload.Clicked = () => {
            // Art reload is not on this client.
        };
        area.Add(reload);
    }

    void Apply() {
        HighlightGameObjects = On(_highlightObjects, HighlightGameObjects);
        ReduceFPSWhenInactive = On(_reduceFPSWhenInactive, ReduceFPSWhenInactive);
        EnablePathfind = On(_enablePathfind, EnablePathfind);
        UseShiftToPathfind = On(_useShiftPathfind, UseShiftToPathfind);
        AlwaysRun = On(_alwaysRun, AlwaysRun);
        AlwaysRunUnlessHidden = On(_alwaysRunUnlessHidden, AlwaysRunUnlessHidden);
        FastRotation = On(_fastRotation, FastRotation);
        ShowMobilesHP = On(_showHpMobile, ShowMobilesHP);
        HighlightMobilesByPoisoned = On(_highlightByPoisoned, HighlightMobilesByPoisoned);
        HighlightMobilesByParalize = On(_highlightByParalyzed, HighlightMobilesByParalize);
        HighlightMobilesByInvul = On(_highlightByInvul, HighlightMobilesByInvul);
        PoisonHue = HueOf(_poisonColor, PoisonHue);
        ParalyzedHue = HueOf(_paralyzedColor, ParalyzedHue);
        InvulnerableHue = HueOf(_invulnerableColor, InvulnerableHue);
        MobileHPType = Index(_hpComboBox, MobileHPType);
        MobileHPShowWhen = Index(_hpComboBoxShowWhen, MobileHPShowWhen);
        UseNewTargetSystem = On(_newTargetSystem, UseNewTargetSystem);
        HoldDownKeyTab = On(_holdDownKeyTab, HoldDownKeyTab);
        HoldDownKeyAltToCloseAnchored = On(_holdDownKeyAlt, HoldDownKeyAltToCloseAnchored);
        CloseAllAnchoredGumpsInGroupWithRightClick = On(_closeAllAnchoredGumpsWithRClick, CloseAllAnchoredGumpsInGroupWithRightClick);
        HoldShiftForContext = On(_holdShiftForContext, HoldShiftForContext);
        HoldAltToMoveGumps = On(_holdAltToMoveGumps, HoldAltToMoveGumps);
        HoldShiftToSplitStack = On(_holdShiftToSplitStack, HoldShiftToSplitStack);
        CloseHealthBarType = Index(_healtbarType, CloseHealthBarType);
        HideScreenshotStoredInMessage = On(_hideScreenshotStoredInMessage, HideScreenshotStoredInMessage);
        if (_drawRoofs != null && DrawRoofs == _drawRoofs.IsChecked) DrawRoofs = !_drawRoofs.IsChecked;
        if (TopbarGumpIsDisabled != On(_enableTopbar, TopbarGumpIsDisabled)) {
            TopbarGumpIsDisabled = _enableTopbar.IsChecked;
            if (TopbarGumpIsDisabled) UIManager.GetGump<TopBarGump>()?.Dispose();
            else if (UIManager.GetGump<TopBarGump>() == null) UIManager.Add(new TopBarGump(World));
        }
        EnableCaveBorder = On(_enableCaveBorder, EnableCaveBorder);
        TreeToStumps = On(_treeToStumps, TreeToStumps);
        FieldsType = Index(_fieldsType, FieldsType);
        HideVegetation = On(_hideVegetation, HideVegetation);
        NoColorObjectsOutOfRange = On(_noColorOutOfRangeObjects, NoColorObjectsOutOfRange);
        UseCircleOfTransparency = On(_useCircleOfTransparency, UseCircleOfTransparency);
        CircleOfTransparencyRadius = Val(_circleOfTranspRadius, CircleOfTransparencyRadius);
        CircleOfTransparencyType = Index(_cotType, CircleOfTransparencyType);
        StandardSkillsGump = On(_useStandardSkillsGump, StandardSkillsGump);
        ShowNewMobileNameIncoming = On(_showMobileNameIncoming, ShowNewMobileNameIncoming);
        ShowNewCorpseNameIncoming = On(_showCorpseNameIncoming, ShowNewCorpseNameIncoming);
        GridLootType = Index(_gridLoot, GridLootType);
        SallosEasyGrab = On(_sallosEasyGrab, SallosEasyGrab);
        PartyInviteGump = On(_partyInviteGump, PartyInviteGump);
        UseObjectsFading = On(_objectsFading, UseObjectsFading);
        TextFading = On(_textFading, TextFading);
        UseSmoothBoatMovement = On(_useSmoothBoat, UseSmoothBoatMovement);
        // ShowPublicHouseContent is not sent.
        ShowHouseContent = On(_showHouseContent, ShowHouseContent);

        EnableSound = On(_enableSounds, EnableSound);
        EnableMusic = On(_enableMusic, EnableMusic);
        EnableFootstepsSound = On(_footStepsSound, EnableFootstepsSound);
        EnableCombatMusic = On(_combatMusic, EnableCombatMusic);
        ReproduceSoundsInBackground = On(_musicInBackground, ReproduceSoundsInBackground);
        SoundVolume = Val(_soundsVolume, SoundVolume);
        MusicVolume = Val(_musicVolume, MusicVolume);
        LoginMusicVolume = Val(_loginMusicVolume, LoginMusicVolume);
        LoginMusic = On(_loginMusic, LoginMusic);

        ScaleSpeechDelay = On(_scaleSpeechDelay, ScaleSpeechDelay);
        SpeechDelay = Val(_sliderSpeechDelay, SpeechDelay);
        SpeechHue = HueOf(_speechColor, SpeechHue);
        EmoteHue = HueOf(_emoteColor, EmoteHue);
        YellHue = HueOf(_yellColor, YellHue);
        WhisperHue = HueOf(_whisperColor, WhisperHue);
        PartyMessageHue = HueOf(_partyMessageColor, PartyMessageHue);
        GuildMessageHue = HueOf(_guildMessageColor, GuildMessageHue);
        AllyMessageHue = HueOf(_allyMessageColor, AllyMessageHue);
        ChatMessageHue = HueOf(_chatMessageColor, ChatMessageHue);
        ActivateChatAfterEnter = On(_chatAfterEnter, ActivateChatAfterEnter);
        ActivateChatAdditionalButtons = On(_chatAdditionalButtonsCheckbox, ActivateChatAdditionalButtons);
        ActivateChatShiftEnterSupport = On(_chatShiftEnterCheckbox, ActivateChatShiftEnterSupport);
        SaveJournalToFile = On(_saveJournalCheckBox, SaveJournalToFile);
        OverheadPartyMessages = On(_partyMessagesOverhead, OverheadPartyMessages);
        if (_maxJournalFilesEnabled != null && !_maxJournalFilesEnabled.IsChecked) MaxJournalFiles = -1;
        else if (_maxJournalFiles != null && int.TryParse(_maxJournalFiles.Text, out var files)) MaxJournalFiles = files;
        else MaxJournalFiles = -1;
        JournalFileWithSerial = On(_journalFileWithSerial, JournalFileWithSerial);

        EnableDeathScreen = On(_enableDeathScreen, EnableDeathScreen);
        EnableBlackWhiteEffect = On(_enableBlackWhiteEffect, EnableBlackWhiteEffect);
        ScreenScale = _sliderScreenZoom == null ? ScreenScale : GetScreenScale(_sliderScreenZoom.Value);
        ZoomIndex = Val(_sliderZoom, ZoomIndex);
        // Camera zoom step is not on this client.
        if (ZoomIndex == 0) DefaultScale = 1f;
        EnableMousewheelScaleZoom = On(_zoomCheckbox, EnableMousewheelScaleZoom);
        RestoreScaleAfterUnpressCtrl = On(_restoreZoom, RestoreScaleAfterUnpressCtrl);
        UseOldStatusGump = On(_useOldStatus, UseOldStatusGump);
        StatusGumpBarMutuallyExclusive = On(_statusGumpBarMutuallyExclusive, StatusGumpBarMutuallyExclusive);
        if (_gameWindowWidth != null && int.TryParse(_gameWindowWidth.Text, out var gw)) GameWindowWidth = gw;
        if (_gameWindowHeight != null && int.TryParse(_gameWindowHeight.Text, out var gh)) GameWindowHeight = gh;
        if (_gameWindowPositionX != null && int.TryParse(_gameWindowPositionX.Text, out var gx)) GameWindowX = gx;
        if (_gameWindowPositionY != null && int.TryParse(_gameWindowPositionY.Text, out var gy)) GameWindowY = gy;
        GameWindowLock = On(_gameWindowLock, GameWindowLock);
        if (_gameWindowFullsize != null && GameWindowFullSize != _gameWindowFullsize.IsChecked) {
            GameWindowFullSize = _gameWindowFullsize.IsChecked;
            if (GameWindowFullSize) {
                GameWindowWidth = ScreenW();
                GameWindowHeight = ScreenH();
                GameWindowX = -5;
                GameWindowY = -5;
            }
            else {
                GameWindowWidth = 600;
                GameWindowHeight = 480;
                GameWindowX = 20;
                GameWindowY = 20;
            }
            UpdateVideo();
        }
        // The host window is not changed.
        WindowBorderless = On(_windowBorderless, WindowBorderless);
        UseAlternativeLights = On(_altLights, UseAlternativeLights);
        UseCustomLightLevel = On(_enableLight, UseCustomLightLevel);
        if (_lightBar != null) {
            var level = 0x1E - _lightBar.Value;
            if (level < 0) level = 0;
            if (level > 0x1E) level = 0x1E;
            LightLevel = (byte)level;
        }
        LightLevelType = Index(_lightLevelType, LightLevelType);
        // World light is not on this client.
        UseColoredLights = On(_useColoredLights, UseColoredLights);
        UseDarkNights = On(_darkNights, UseDarkNights);
        ShadowsEnabled = On(_enableShadows, ShadowsEnabled);
        ShadowsStatics = On(_enableShadowsStatics, ShadowsStatics);
        TerrainShadowsLevel = Val(_terrainShadowLevel, TerrainShadowsLevel);
        AuraUnderFeetType = Index(_auraType, AuraUnderFeetType);
        RunMouseInSeparateThread = On(_runMouseInSeparateThread, RunMouseInSeparateThread);
        AuraOnMouse = On(_auraMouse, AuraOnMouse);
        AnimatedWaterEffect = On(_animatedWaterEffect, AnimatedWaterEffect);
        PartyAura = On(_partyAura, PartyAura);
        PartyAuraHue = HueOf(_partyAuraColor, PartyAuraHue);
        HideChatGradient = On(_hideChatGradient, HideChatGradient);
        IgnoreGuildMessages = On(_ignoreGuildMessages, IgnoreGuildMessages);
        IgnoreAllianceMessages = On(_ignoreAllianceMessages, IgnoreAllianceMessages);
        UseAlternateJournal = On(_useAlternateJournal, UseAlternateJournal);

        ForceUnicodeJournal = On(_forceUnicodeJournal, ForceUnicodeJournal);
        if (_fontSelectorChat != null) {
            var font = _fontSelectorChat.GetSelectedFont();
            if (font != 0xFF) ChatFont = font;
        }
        OverrideAllFonts = On(_overrideAllFonts, OverrideAllFonts);
        OverrideAllFontsIsUnicode = _overrideAllFontsIsUnicode != null && _overrideAllFontsIsUnicode.SelectedIndex == 1;

        InnocentHue = HueOf(_innocentColor, InnocentHue);
        FriendHue = HueOf(_friendColor, FriendHue);
        CriminalHue = HueOf(_criminalColor, CriminalHue);
        CanAttackHue = HueOf(_canAttackColor, CanAttackHue);
        EnemyHue = HueOf(_enemyColor, EnemyHue);
        MurdererHue = HueOf(_murdererColor, MurdererHue);
        EnabledCriminalActionQuery = On(_queryBeforeAttack, EnabledCriminalActionQuery);
        EnabledBeneficialCriminalActionQuery = On(_queryBeforeBeneficial, EnabledBeneficialCriminalActionQuery);
        CastSpellsByOneClick = On(_uiButtonsSingleClick, CastSpellsByOneClick);
        BuffBarTime = On(_buffBarTime, BuffBarTime);
        FastSpellsAssign = On(_enableFastSpellsAssign, FastSpellsAssign);
        BeneficHue = HueOf(_beneficColor, BeneficHue);
        HarmfulHue = HueOf(_harmfulColor, HarmfulHue);
        NeutralHue = HueOf(_neutralColor, NeutralHue);
        EnabledSpellHue = On(_spellColoring, EnabledSpellHue);
        EnabledSpellFormat = On(_spellFormatCheckbox, EnabledSpellFormat);
        if (_spellFormatBox != null) SpellDisplayFormat = _spellFormatBox.Text ?? "";
        ShowDPSWithDamageNumbers = On(_showDPS, ShowDPSWithDamageNumbers);

        var countersWere = CounterBarEnabled;
        CounterBarEnabled = On(_enableCounters, CounterBarEnabled);
        CounterBarCellSize = Val(_cellSize, CounterBarCellSize);
        CounterBarHighlightOnChange = On(_highlightOnChange, CounterBarHighlightOnChange);
        if (_highlightAmount == null || !int.TryParse(_highlightAmount.Text, out var highlight)) {
            highlight = 5;
            _highlightAmount?.SetText("5");
        }
        CounterBarHighlightAmount = highlight;
        if (_abbreviatedAmount == null || !int.TryParse(_abbreviatedAmount.Text, out var abbreviated)) {
            abbreviated = 1000;
            _abbreviatedAmount?.SetText("1000");
        }
        CounterBarAbbreviatedAmount = abbreviated;
        CounterBarHighlightOnAmount = On(_highlightOnAmount, CounterBarHighlightOnAmount);
        CounterBarDisplayAbbreviatedAmount = On(_enableAbbreviatedAmount, CounterBarDisplayAbbreviatedAmount);
        var counter = UIManager.GetGump<CounterBarGump>();
        if (countersWere != CounterBarEnabled) {
            if (counter == null) {
                if (CounterBarEnabled) UIManager.Add(new CounterBarGump(World) { X = 200, Y = 200 });
            }
            else {
                counter.IsEnabled = CounterBarEnabled;
                counter.IsVisible = CounterBarEnabled;
            }
        }

        if (_disableDefaultHotkeys != null && !_disableDefaultHotkeys.IsChecked) {
            if (_disableArrowBtn != null) _disableArrowBtn.IsChecked = false;
            if (_disableTabBtn != null) _disableTabBtn.IsChecked = false;
            if (_disableCtrlQWBtn != null) _disableCtrlQWBtn.IsChecked = false;
            if (_disableAutoMove != null) _disableAutoMove.IsChecked = false;
        }
        DisableDefaultHotkeys = On(_disableDefaultHotkeys, DisableDefaultHotkeys);
        DisableArrowBtn = On(_disableArrowBtn, DisableArrowBtn);
        DisableTabBtn = On(_disableTabBtn, DisableTabBtn);
        DisableCtrlQWBtn = On(_disableCtrlQWBtn, DisableCtrlQWBtn);
        DisableAutoMove = On(_disableAutoMove, DisableAutoMove);
        AutoOpenDoors = On(_autoOpenDoors, AutoOpenDoors);
        SmoothDoors = On(_smoothDoors, SmoothDoors);
        AutoOpenCorpses = On(_autoOpenCorpse, AutoOpenCorpses);
        if (_autoOpenCorpseRange != null && int.TryParse(_autoOpenCorpseRange.Text, out var range)) AutoOpenCorpseRange = range;
        CorpseOpenOptions = Index(_autoOpenCorpseOptions, CorpseOpenOptions);
        SkipEmptyCorpse = On(_skipEmptyCorpse, SkipEmptyCorpse);
        EnableDragSelect = On(_enableDragSelect, EnableDragSelect);
        DragSelectModifierKey = Index(_dragSelectModifierKey, DragSelectModifierKey);
        DragSelectHumanoidsOnly = On(_dragSelectHumanoidsOnly, DragSelectHumanoidsOnly);
        DragSelectHostileOnly = On(_dragSelectHostileOnly, DragSelectHostileOnly);
        DragSelectStartX = Val(_dragSelectStartX, DragSelectStartX);
        DragSelectStartY = Val(_dragSelectStartY, DragSelectStartY);
        DragSelectAsAnchor = On(_dragSelectAsAnchor, DragSelectAsAnchor);
        ShowSkillsChangedMessage = On(_showSkillsMessage, ShowSkillsChangedMessage);
        ShowSkillsChangedDeltaValue = Val(_showSkillsMessageDelta, ShowSkillsChangedDeltaValue);
        ShowStatsChangedMessage = On(_showStatsMessage, ShowStatsChangedMessage);
        OverrideContainerLocation = On(_overrideContainerLocation, OverrideContainerLocation);
        OverrideContainerLocationSetting = Index(_overrideContainerLocationSetting, OverrideContainerLocationSetting);
        ShowTargetRangeIndicator = On(_showTargetRangeIndicator, ShowTargetRangeIndicator);
        NameOverheadToggled = On(_nameOverheadAlwaysOn, NameOverheadToggled);
        NameOverheadShowHpBar = On(_nameOverheadShowHpBar, NameOverheadShowHpBar);
        CustomBarsToggled = On(_customBars, CustomBarsToggled);
        CBBlackBGToggled = On(_customBarsBBG, CBBlackBGToggled);
        SaveHealthbars = On(_saveHealthbars, SaveHealthbars);

        ShowInfoBar = On(_showInfoBar, ShowInfoBar);
        InfoBarHighlightType = Index(_infoBarHighlightType, InfoBarHighlightType);
        InfoBars.Clear();
        for (var i = 0; i < _infoRows.Count; i++) {
            var row = _infoRows[i];
            if (row != null && !row.IsDisposed)
                InfoBars.Add(new InfoBarEntry(row.LabelText, row.Var, row.Hue));
        }
        var info = UIManager.GetGump<InfoBarGump>();
        if (ShowInfoBar) {
            if (info == null) UIManager.Add(new InfoBarGump(World) { X = 300, Y = 300 });
        }
        else info?.Dispose();

        ContainersScale = Val(_containersScale, ContainersScale);
        ScaleItemsInsideContainers = On(_containerScaleItems, ScaleItemsInsideContainers);
        UseLargeContainerGumps = On(_useLargeContainers, UseLargeContainerGumps);
        DoubleClickToLootInsideContainers = On(_containerDoubleClickToLoot, DoubleClickToLootInsideContainers);
        RelativeDragAndDropItems = On(_relativeDragAndDropItems, RelativeDragAndDropItems);
        HighlightContainerWhenSelected = On(_highlightContainersWhenMouseIsOver, HighlightContainerWhenSelected);
        HueContainerGumps = On(_hueContainerGumps, HueContainerGumps);
        BackpackStyle = Index(_backpackStyle, BackpackStyle);

        UseTooltip = On(_useTooltip, UseTooltip);
        TooltipTextHue = HueOf(_tooltipFontHue, TooltipTextHue);
        TooltipDelayBeforeDisplay = Val(_delayBeforeTooltip, TooltipDelayBeforeDisplay);
        TooltipBackgroundOpacity = Val(_tooltipOpacity, TooltipBackgroundOpacity);
        TooltipDisplayZoom = Val(_tooltipZoom, TooltipDisplayZoom);
        if (_tooltipFontSelector != null) {
            var font = _tooltipFontSelector.GetSelectedFont();
            if (font != 0xFF) TooltipFont = font;
        }
        Fps = Val(_sliderFPS, Fps);
        // Profile path is not ported.
    }

    void SetDefault() {
        switch (ActivePage) {
            case 1:
                SetSlider(_sliderFPS, 60);
                SetCheck(_reduceFPSWhenInactive, true);
                SetCheck(_highlightObjects, true);
                SetCheck(_enableTopbar, false);
                SetCheck(_newTargetSystem, true);
                SetCheck(_holdDownKeyTab, true);
                SetCheck(_holdDownKeyAlt, true);
                SetCheck(_closeAllAnchoredGumpsWithRClick, false);
                SetCheck(_holdShiftForContext, false);
                SetCheck(_holdAltToMoveGumps, false);
                SetCheck(_holdShiftToSplitStack, false);
                SetCheck(_enablePathfind, false);
                SetCheck(_useShiftPathfind, false);
                SetCheck(_alwaysRun, false);
                SetCheck(_alwaysRunUnlessHidden, false);
                SetCheck(_showHpMobile, false);
                SetIndex(_hpComboBox, 0);
                SetIndex(_hpComboBoxShowWhen, 0);
                SetCheck(_highlightByPoisoned, true);
                SetCheck(_highlightByParalyzed, true);
                SetCheck(_highlightByInvul, true);
                SetHue(_poisonColor, 0x0044);
                SetHue(_paralyzedColor, 0x014C);
                SetHue(_invulnerableColor, 0x0030);
                SetCheck(_drawRoofs, false);
                SetCheck(_enableCaveBorder, false);
                SetCheck(_treeToStumps, false);
                SetCheck(_hideVegetation, false);
                SetCheck(_noColorOutOfRangeObjects, false);
                SetSlider(_circleOfTranspRadius, 50);
                SetIndex(_cotType, 0);
                SetCheck(_useCircleOfTransparency, false);
                SetIndex(_healtbarType, 0);
                SetIndex(_fieldsType, 0);
                SetCheck(_useStandardSkillsGump, true);
                SetCheck(_showCorpseNameIncoming, true);
                SetCheck(_showMobileNameIncoming, true);
                SetIndex(_gridLoot, 0);
                SetCheck(_sallosEasyGrab, false);
                SetCheck(_partyInviteGump, false);
                SetCheck(_showHouseContent, false);
                SetCheck(_objectsFading, true);
                SetCheck(_textFading, true);
                SetCheck(_enableDragSelect, false);
                SetCheck(_dragSelectHumanoidsOnly, false);
                SetCheck(_dragSelectHostileOnly, false);
                SetCheck(_showTargetRangeIndicator, false);
                SetCheck(_nameOverheadAlwaysOn, false);
                SetCheck(_nameOverheadShowHpBar, true);
                SetCheck(_customBars, false);
                SetCheck(_customBarsBBG, false);
                SetCheck(_autoOpenCorpse, false);
                SetCheck(_autoOpenDoors, false);
                SetCheck(_smoothDoors, false);
                SetCheck(_skipEmptyCorpse, false);
                SetCheck(_saveHealthbars, false);
                SetCheck(_useSmoothBoat, false);
                SetCheck(_hideScreenshotStoredInMessage, false);
                SetCheck(_useOldStatus, false);
                SetCheck(_statusGumpBarMutuallyExclusive, true);
                SetIndex(_auraType, 0);
                SetCheck(_showSkillsMessage, true);
                SetSlider(_showSkillsMessageDelta, 1);
                SetCheck(_showStatsMessage, true);
                SetSlider(_dragSelectStartX, 100);
                SetSlider(_dragSelectStartY, 100);
                SetCheck(_dragSelectAsAnchor, false);
                break;
            case 2:
                SetCheck(_enableSounds, true);
                SetCheck(_enableMusic, true);
                SetCheck(_combatMusic, true);
                SetSlider(_soundsVolume, 100);
                SetSlider(_musicVolume, 100);
                SetCheck(_musicInBackground, false);
                SetCheck(_footStepsSound, true);
                SetSlider(_loginMusicVolume, 100);
                SetCheck(_loginMusic, true);
                if (_soundsVolume != null) _soundsVolume.IsVisible = _enableSounds != null && _enableSounds.IsChecked;
                if (_musicVolume != null) _musicVolume.IsVisible = _enableMusic != null && _enableMusic.IsChecked;
                break;
            case 3:
                SetCheck(_windowBorderless, false);
                SetCheck(_zoomCheckbox, false);
                SetCheck(_restoreZoom, false);
                _gameWindowWidth?.SetText("600");
                _gameWindowHeight?.SetText("480");
                _gameWindowPositionX?.SetText("20");
                _gameWindowPositionY?.SetText("20");
                SetCheck(_gameWindowLock, false);
                SetCheck(_gameWindowFullsize, false);
                SetCheck(_enableDeathScreen, true);
                SetCheck(_enableBlackWhiteEffect, true);
                DefaultScale = 1f;
                ZoomIndex = 0;
                SetSlider(_sliderZoom, 0);
                SetSlider(_sliderScreenZoom, 0);
                SetSlider(_lightBar, 0);
                SetCheck(_enableLight, false);
                SetIndex(_lightLevelType, 0);
                SetCheck(_useColoredLights, false);
                SetCheck(_darkNights, false);
                SetCheck(_enableShadows, true);
                SetCheck(_enableShadowsStatics, true);
                SetSlider(_terrainShadowLevel, 15);
                SetCheck(_runMouseInSeparateThread, true);
                SetCheck(_auraMouse, true);
                SetCheck(_partyAura, true);
                SetCheck(_animatedWaterEffect, false);
                SetHue(_partyAuraColor, 0x0044);
                break;
            case 5:
                SetCheck(_useTooltip, true);
                SetHue(_tooltipFontHue, 0xFFFF);
                SetSlider(_delayBeforeTooltip, 200);
                SetSlider(_tooltipOpacity, 70);
                SetSlider(_tooltipZoom, 100);
                _tooltipFontSelector?.SetSelectedFont(1);
                break;
            case 6:
                _fontSelectorChat?.SetSelectedFont(0);
                SetCheck(_overrideAllFonts, false);
                SetIndex(_overrideAllFontsIsUnicode, 1);
                break;
            case 7:
                SetCheck(_scaleSpeechDelay, true);
                SetSlider(_sliderSpeechDelay, 100);
                SetHue(_speechColor, 0x02B2);
                SetHue(_emoteColor, 0x0021);
                SetHue(_yellColor, 0x0021);
                SetHue(_whisperColor, 0x0033);
                SetHue(_partyMessageColor, 0x0044);
                SetHue(_guildMessageColor, 0x0044);
                SetHue(_allyMessageColor, 0x0057);
                SetHue(_chatMessageColor, 0x0256);
                SetCheck(_chatAfterEnter, false);
                SetCheck(_chatAdditionalButtonsCheckbox, true);
                SetCheck(_chatShiftEnterCheckbox, true);
                SetCheck(_saveJournalCheckBox, false);
                SetCheck(_hideChatGradient, false);
                SetCheck(_ignoreGuildMessages, false);
                SetCheck(_ignoreAllianceMessages, false);
                SetCheck(_useAlternateJournal, false);
                SetCheck(_partyMessagesOverhead, false);
                break;
            case 8:
                SetCheck(_showDPS, false);
                SetHue(_innocentColor, 0x005A);
                SetHue(_friendColor, 0x0044);
                SetHue(_criminalColor, 0x03B2);
                SetHue(_canAttackColor, 0x03B2);
                SetHue(_murdererColor, 0x0023);
                SetHue(_enemyColor, 0x0031);
                SetCheck(_queryBeforeAttack, true);
                SetCheck(_queryBeforeBeneficial, false);
                SetCheck(_uiButtonsSingleClick, false);
                SetCheck(_buffBarTime, false);
                SetCheck(_enableFastSpellsAssign, false);
                SetHue(_beneficColor, 0x0059);
                SetHue(_harmfulColor, 0x0020);
                SetHue(_neutralColor, 0x03B2);
                _spellFormatBox?.SetText("{power} [{spell}]");
                SetCheck(_spellColoring, false);
                SetCheck(_spellFormatCheckbox, false);
                break;
            case 9:
                SetCheck(_enableCounters, false);
                SetCheck(_highlightOnChange, false);
                SetCheck(_enableAbbreviatedAmount, false);
                _columns?.SetText("1");
                _rows?.SetText("1");
                SetSlider(_cellSize, 40);
                SetCheck(_highlightOnAmount, false);
                _highlightAmount?.SetText("5");
                _abbreviatedAmount?.SetText("1000");
                break;
            case 11:
                SetSlider(_containersScale, 100);
                SetCheck(_containerScaleItems, false);
                SetCheck(_useLargeContainers, false);
                SetCheck(_containerDoubleClickToLoot, false);
                SetCheck(_relativeDragAndDropItems, false);
                SetCheck(_highlightContainersWhenMouseIsOver, false);
                SetCheck(_overrideContainerLocation, false);
                SetIndex(_overrideContainerLocationSetting, 0);
                SetIndex(_backpackStyle, 0);
                SetCheck(_hueContainerGumps, true);
                break;
            case 12:
                SetCheck(_disableDefaultHotkeys, false);
                SetCheck(_disableArrowBtn, false);
                SetCheck(_disableTabBtn, false);
                SetCheck(_disableCtrlQWBtn, false);
                SetCheck(_disableAutoMove, false);
                break;
        }
    }

    void AddMacro() {
        var name = _macroName == null ? "" : (_macroName.Text ?? "").Trim();
        if (name.Length == 0) return;
        for (var i = 0; i < MacroNames.Count; i++)
            if (MacroNames[i] == name) return;
        MacroNames.Add(name);
        _selectedMacro = MacroNames.Count - 1;
        _macroName.SetText("");
        FillMacros();
    }

    void AskDeleteMacro() {
        if (_selectedMacro < 0 || _selectedMacro >= MacroNames.Count) return;
        var index = _selectedMacro;
        UIManager.Add(new QuestionGump(World, "Do you want\ndelete it?", yes => {
            if (!yes || index < 0 || index >= MacroNames.Count) return;
            MacroNames.RemoveAt(index);
            _selectedMacro = MacroNames.Count == 0 ? -1 : Math.Min(index, MacroNames.Count - 1);
            FillMacros();
        }));
    }

    void FillMacros() {
        if (_macroList == null) return;
        while (_macroList.Children.Count > 0) {
            var child = _macroList.Children[0];
            child.Parent = null;
            child.Dispose();
        }
        for (var i = 0; i < MacroNames.Count; i++) {
            var index = i;
            var button = new OptionsTextButton(0, i * 26, 130, 25, MacroNames[i], -1, false) {
                IsSelected = i == _selectedMacro,
            };
            button.Clicked = () => {
                _selectedMacro = index;
                FillMacros();
            };
            _macroList.Add(button);
        }
    }

    void AddInfoRow(InfoBarEntry item) {
        if (_infoStack == null) return;
        var row = new InfoBarRow(this, item);
        _infoRows.Add(row);
        _infoStack.Add(row);
        ReflowInfo();
    }

    void RemoveInfoRow(InfoBarRow row) {
        row.Parent = null;
        row.Dispose();
        _infoRows.Remove(row);
        ReflowInfo();
    }

    void ReflowInfo() {
        if (_infoStack == null) return;
        var y = 0;
        for (var i = 0; i < _infoRows.Count; i++) {
            var row = _infoRows[i];
            if (row.IsDisposed) continue;
            row.X = 0;
            row.Y = y;
            y += (row.Height > 0 ? row.Height : 26) + 2;
        }
        _infoStack.Height = y;
    }

    void RandomizeSpeech() {
        SpeechHue = NextHue();
        EmoteHue = NextHue();
        YellHue = NextHue();
        WhisperHue = NextHue();
        SetHue(_speechColor, SpeechHue);
        SetHue(_emoteColor, EmoteHue);
        SetHue(_yellColor, YellHue);
        SetHue(_whisperColor, WhisperHue);
    }

    static ushort NextHue() => (ushort)HueRandom.Next(2, 0x03B2);

    ScrollArea PageArea(int page) {
        var area = new ScrollArea(190, 20, WidthPx - 210, 420, true) { WantUpdateSize = false };
        if (area.Width < 1) area.Width = WidthPx - 210;
        if (area.Height < 1) area.Height = 420;
        Add(area, page);
        return area;
    }

    SettingsSection AddSection(ScrollArea area, string title, int y) {
        var section = new SettingsSection(title, 470) { X = 5, Y = y };
        area.Add(section);
        return section;
    }

    Checkbox Check(string text, bool on) {
        var box = new Checkbox(0x00D2, 0x00D3, text, FontIndex, HueFont, true) { IsChecked = on };
        if (box.Height < 1) box.Height = 16;
        if (box.Width < 1) box.Width = (text == null ? 0 : text.Length) * 6 + 16;
        return box;
    }

    Label Caption(string text) => new Label(text, true, HueFont, FontIndex);

    Combobox Combo(string[] items, int index, int width) {
        if (items == null || items.Length == 0) items = new[] { "" };
        if (index < 0 || index >= items.Length) index = 0;
        var box = new Combobox(0, 0, width, items, index);
        if (box.Height < 1) box.Height = TextBoxHeight;
        if (box.Width < 1) box.Width = width;
        return box;
    }

    HSliderBar Slider(int min, int max, int value, int width) {
        if (max < min) max = min;
        if (value < min) value = min;
        if (value > max) value = max;
        var bar = new HSliderBar(0, 0, width, min, max, value, HSliderBarStyle.MetalWidgetRecessedBar, true, FontIndex, HueFont);
        if (bar.Height < 1) bar.Height = 16;
        if (bar.Width < 1) bar.Width = width;
        return bar;
    }

    OptionsInput Input(int width, string text, int maxChars, bool numbers) {
        var field = new OptionsInput(0, 0, width, TextBoxHeight, maxChars, numbers);
        field.SetText(text);
        return field;
    }

    OptionsHue HueBox(ushort hue) => new OptionsHue(World, hue);

    static void Place(Control parent, Control child, int x, int y) {
        child.X = x;
        child.Y = y;
        parent.Add(child);
    }

    void PlaceHue(Control parent, OptionsHue box, string text, int x, int y) {
        Place(parent, box, x, y);
        Place(parent, Caption(text), x + box.Width + 10, y);
    }

    static int Row(Control c) => c == null || c.Height < 1 ? 16 : c.Height;

    static int ScreenW() {
        var w = Mouse.Current.BufW;
        return w > 0 ? w : 800;
    }

    static int ScreenH() {
        var h = Mouse.Current.BufH;
        return h > 0 ? h : 600;
    }

    static bool On(Checkbox box, bool fallback) => box == null ? fallback : box.IsChecked;

    static int Index(Combobox box, int fallback) {
        if (box == null || box.SelectedIndex < 0) return fallback;
        return box.SelectedIndex;
    }

    static int Val(HSliderBar bar, int fallback) => bar == null ? fallback : bar.Value;

    static ushort HueOf(OptionsHue box, ushort fallback) => box == null ? fallback : box.Hue;

    static void SetCheck(Checkbox box, bool value) {
        if (box != null) box.IsChecked = value;
    }

    static void SetSlider(HSliderBar bar, int value) {
        if (bar != null) bar.Value = value;
    }

    static void SetIndex(Combobox box, int index) {
        if (box != null) box.SelectedIndex = index;
    }

    static void SetHue(OptionsHue box, ushort hue) {
        if (box != null) box.Hue = hue;
    }

    float GetScreenScale(int zoom) {
        var steps = (float)ScreenZoomSteps;
        var x = (float)zoom;
        if (zoom > 0) return x / steps + 1f;
        return 1f + 0.025f * x;
    }

    int GetScreenZoom(float scale) {
        var steps = (float)ScreenZoomSteps;
        if (scale > 1f) return (int)Math.Round(steps * (scale - 1f));
        return (int)Math.Round(-steps * 2f * (1f - scale));
    }

    enum Buttons {
        Disabled,
        Cancel,
        Apply,
        Default,
        Ok,
        OpenIgnoreList,
        NewMacro,
        DeleteMacro,
    }

    /// <summary>One info-bar row. Var is the ClassicUO InfoBarVars ordinal. NameNotoriety is 23.</summary>
    public struct InfoBarEntry {
        public string Label;
        public int Var;
        public ushort Hue;

        public InfoBarEntry(string label, int var, ushort hue) {
            Label = label ?? "";
            Var = var;
            Hue = hue;
        }
    }

    sealed class SettingsSection : Control {
        int _indent;
        int _bottom;

        public SettingsSection(string title, int width) {
            CanMove = false;
            AcceptMouseInput = false;
            WantUpdateSize = false;
            Width = width;
            var label = new Label(title, true, HueFont, FontIndex) { X = 5, Y = 0 };
            base.Add(label);
            _bottom = (label.Height > 0 ? label.Height : 16) + 4;
            Height = _bottom;
        }

        public void PushIndent() => _indent += 40;

        public void PopIndent() {
            if (_indent >= 40) _indent -= 40;
        }

        public void AddRight(Control c, int offset = 15) {
            if (c.Height < 1) c.Height = 16;
            Control last = null;
            for (var i = Children.Count - 1; i >= 1; i--) {
                if (Children[i].IsVisible) {
                    last = Children[i];
                    break;
                }
            }
            c.X = last == null ? _indent : last.X + last.Width + offset;
            c.Y = last == null ? _bottom : last.Y;
            var bottom = c.Y + c.Height + 2;
            if (bottom > _bottom) _bottom = bottom;
            Height = _bottom;
            base.Add(c);
        }

        public override T Add<T>(T c, int page = 0) {
            if (c.Height < 1) c.Height = 16;
            c.X = _indent;
            c.Y = _bottom;
            _bottom += c.Height + 2;
            Height = _bottom;
            return base.Add(c, page);
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var labelH = Children.Count > 0 && Children[0].Height > 0 ? Children[0].Height : 16;
            GumpArt.Sink?.DrawRaw(LineKey, x, y + labelH, Math.Max(1, Width - 30), 1, Gray, 1, 1);
            return base.Draw(x, y);
        }
    }

    sealed class OptionsTextButton : Control {
        readonly int _fill;

        public OptionsTextButton(int x, int y, int w, int h, string text, int parameter, bool switchPage) {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Text = text ?? "";
            ButtonParameter = parameter;
            SwitchPage = switchPage;
            AcceptMouseInput = true;
            CanMove = false;
            CanCloseWithRightClick = false;
            WantUpdateSize = false;
            _fill = parameter >= 0 ? ButtonKey + (parameter & 15) : ButtonKey;
        }

        public string Text { get; }
        public int ButtonParameter { get; }
        public bool SwitchPage { get; }
        public bool IsSelected { get; set; }
        public Action Clicked { get; set; }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            GumpArt.Sink?.DrawRaw(IsSelected ? ButtonOnKey : _fill, x, y, Width, Height, IsSelected ? ButtonOnPx : ButtonPx, 1, 1);
            GumpArt.Sink?.DrawText(x + 4, y + 4, Text, HueFont, FontIndex);
            return base.Draw(x, y);
        }

        public override void OnMouseClick(int x, int y) {
            if (Clicked != null) {
                Clicked();
                return;
            }
            if (Parent is OptionsGump gump) {
                if (SwitchPage) gump.ChangePage(ButtonParameter);
                else gump.OnButtonClick(ButtonParameter);
            }
        }
    }

    sealed class OptionsInput : Control {
        readonly TextBox _box;

        public OptionsInput(int x, int y, int width, int height, int maxChars, bool numbersOnly) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            WantUpdateSize = false;
            AcceptMouseInput = true;
            CanMove = false;
            NumbersOnly = numbersOnly;
            Add(new ResizePic(0x0BB8) {
                Width = width, Height = height, CanMove = false, AcceptMouseInput = false,
            });
            _box = new TextBox {
                X = 4,
                Y = 4,
                Width = Math.Max(1, width - 8),
                Height = Math.Max(1, height - 8),
                MaxLength = maxChars < 0 ? 256 : maxChars,
                Hue = HueFont,
            };
            Add(_box);
        }

        public bool NumbersOnly { get; }

        public string Text => _box.Text;

        public void SetText(string text) => _box.Text = text ?? "";

        public override void Update() {
            if (NumbersOnly && _box.Text.Length > 0) {
                var source = _box.Text;
                var kept = "";
                for (var i = 0; i < source.Length; i++)
                    if (source[i] >= '0' && source[i] <= '9') kept += source[i];
                if (kept.Length != source.Length) _box.Text = kept;
            }
            base.Update();
        }
    }

    sealed class OptionsHue : Control {
        public OptionsHue(World world, ushort hue) {
            World = world;
            Hue = hue;
            Width = 22;
            Height = 22;
            if (GumpMetrics.TrySize(0x00D4, out var w, out var h) && w > 0 && h > 0) {
                Width = w;
                Height = h;
            }
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
        }

        public World World { get; }
        public ushort Hue { get; set; }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            GumpArtDraw.Pic(0x00D4, x, y, Width, Height, false);
            var inset = 3;
            var ramp = (ushort)(Hue & 31);
            var pixel = (ushort)(0x8000 | (ramp << 10) | (ramp << 5) | ramp);
            var swatch = new[] { (byte)pixel, (byte)(pixel >> 8) };
            GumpArt.Sink?.DrawRaw(unchecked((int)(0x5F100000 + Hue)), x + inset, y + inset, Math.Max(1, Width - inset * 2), Math.Max(1, Height - inset * 2), swatch, 1, 1);
            return base.Draw(x, y);
        }

        public override void OnMouseClick(int x, int y) {
            UIManager.GetGump<ColorPickerGump>()?.Dispose();
            UIManager.Add(new HuePick(World, hue => Hue = hue));
        }

        sealed class HuePick : ColorPickerGump {
            readonly Action<ushort> _set;

            public HuePick(World world, Action<ushort> set) : base(world, 0, 0, 200, 200) => _set = set;

            protected override void OnOk(ushort hue) => _set?.Invoke(hue);
        }
    }

    sealed class OptionsFontSelector : Control {
        readonly Checkbox[] _boxes;

        public OptionsFontSelector(int maxFont, int current, string markup) {
            CanMove = false;
            CanCloseWithRightClick = false;
            WantUpdateSize = false;
            _boxes = new Checkbox[maxFont];
            var y = 0;
            for (var i = 0; i < maxFont; i++) {
                var index = i;
                var box = new Checkbox(0x00D0, 0x00D1, markup, (byte)i, HueFont, true) {
                    X = 0, Y = y,
                };
                if (box.Height < 1) box.Height = 20;
                if (box.Width < 1) box.Width = (markup == null ? 0 : markup.Length) * 6 + 16;
                box.ValueChanged += (_, __) => {
                    if (box.IsChecked) {
                        for (var j = 0; j < _boxes.Length; j++)
                            if (j != index && _boxes[j] != null) _boxes[j].IsChecked = false;
                    }
                    else {
                        var any = false;
                        for (var j = 0; j < _boxes.Length; j++)
                            if (_boxes[j] != null && _boxes[j].IsChecked) any = true;
                        if (!any) box.IsChecked = true;
                    }
                };
                _boxes[i] = box;
                Add(box);
                y += 25;
            }
            Width = 240;
            Height = y;
            SetSelectedFont(current);
        }

        public byte GetSelectedFont() {
            for (byte i = 0; i < _boxes.Length; i++)
                if (_boxes[i] != null && _boxes[i].IsChecked) return i;
            return 0xFF;
        }

        public void SetSelectedFont(int index) {
            if (index < 0 || index >= _boxes.Length || _boxes[index] == null) return;
            _boxes[index].IsChecked = true;
            for (var j = 0; j < _boxes.Length; j++)
                if (j != index && _boxes[j] != null) _boxes[j].IsChecked = false;
        }
    }

    sealed class OptionsStack : Control {
        public OptionsStack() {
            WantUpdateSize = false;
            AcceptMouseInput = false;
        }
    }

    sealed class InfoBarRow : Control {
        readonly OptionsInput _label;
        readonly OptionsHue _hue;
        readonly Combobox _var;

        public InfoBarRow(OptionsGump owner, InfoBarEntry item) {
            WantUpdateSize = false;
            CanMove = false;
            Width = 460;
            Height = 26;
            _label = owner.Input(130, item.Label, 10, false);
            Add(_label);
            _hue = owner.HueBox(item.Hue);
            _hue.X = 150;
            Add(_hue);
            _var = owner.Combo(InfoBarVarNames, item.Var, 170);
            _var.X = 200;
            Add(_var);
            var delete = new OptionsTextButton(390, 0, 60, 25, "Delete", -1, false);
            delete.Clicked = () => owner.RemoveInfoRow(this);
            Add(delete);
        }

        public string LabelText => _label.Text;
        public int Var => _var.SelectedIndex < 0 ? 0 : _var.SelectedIndex;
        public ushort Hue => _hue.Hue;
    }
}
