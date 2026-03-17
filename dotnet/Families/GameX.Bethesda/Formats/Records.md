# TES3.Main File Header
TES3:
  wbRecord(TES3, 'Main File Header', [
    wbStruct(HEDR, 'Header', [
      wbFloat('Version', cpNormal, False, 1, 2).IncludeFlag(dfInternalEditOnly, not wbAllowEditHEDRVersion),
      wbRecordFlags,
      wbString('Author', 32),
      wbString('Description', 256),
      wbInteger('Number of Records', itU32)
    ]).SetRequired,
    wbRArray('Master Files',
      wbRStruct('Master File', [
        wbStringForward(MAST, 'Filename').SetRequired,
        wbInteger(DATA, 'Master Size', itU64, nil, cpIgnore, True)
    ])).IncludeFlag(dfInternalEditOnly, not wbAllowMasterFilesEdit)
  ], False, nil, cpNormal, True)
    .SetGetFormIDCallback(function(const aMainRecord: IwbMainRecord; out aFormID: TwbFormID): Boolean begin
       Result := True;
       aFormID := TwbFormID.Null;
     end)
     .SetAfterLoad(wbTES3AfterLoad);

# TES4.Main File Header
TES4:
  wbRecord(TES4, 'Main File Header',
    wbFlags(wbFlagsList([
      0, 'ESM',
      4, 'Optimized'
    ])), [
    wbHEDR,
    IfThen(wbSimpleRecords,
      wbByteArray(OFST, 'Offset Load Order', 0, cpIgnore),
      wbArray(OFST, 'Offset Load Order',
        wbStruct('Form', [
          wbInteger('Index', itU8),
          wbUnused(3),
          wbString('Form Type', 4),
          wbInteger('Offset (Unused)', itU32)
        ]), 0, nil, nil, cpIgnore).IncludeFlag(dfCollapsed, wbCollapseOther)),
    wbByteArray(DELE, 'Version Control (Unused)', 8, cpIgnore),
    wbString(CNAM, 'Author', 0, cpTranslate).SetRequired,
    wbString(SNAM, 'Description', 0, cpTranslate),
    wbRArray('Master Files',
      wbRStruct('Master File', [
        wbStringForward(MAST, 'Filename').SetRequired,
        wbUnused(DATA, 8).SetRequired
      ])).IncludeFlag(dfInternalEditOnly, not wbAllowMasterFilesEdit)
  ], False, nil, cpNormal, True);

# AACT.Action

# ACHR.Placed NPC
TES4:
  wbRefRecord(ACHR, 'Placed NPC',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      11, 'Initially Disabled',
      15, 'Visible When Distant'
    ])), [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [NPC_]).SetRequired,
    wbRStruct('Unused', [
      wbFormIDCk(XPCI, 'Unused', [CELL]),
      wbString(FULL, 'Unused')
    ]),
    wbXLOD,
    wbXESP,
    wbFormIDCk(XMRC, 'Merchant container', [REFR], True),
    wbFormIDCk(XHRS, 'Horse', [ACRE], True),
    wbRagdoll,
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo)
          .SetAfterLoad(wbREFRAfterLoad);

# ACRE.Placed Creature
TES4:
  wbRefRecord(ACRE, 'Placed Creature',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      11, 'Initially Disabled',
      15, 'Visible When Distant'
    ])), [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [CREA]).SetRequired,
    wbOwnership,
    wbRagdoll,
    wbXLOD,
    wbXESP,
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo);

# ACTI.Activator
TES3:
  wbRecord(ACTI, 'Activator',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName.SetRequired,
    wbScript //[SCPT]
  ]).SetFormIDBase($40);
TES4:
  wbRecord(ACTI, 'Activator',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      17, 'Dangerous'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbSCRI,
    wbFormIDCk(SNAM, 'Sound', [SOUN])
  ]);

# ADDN.Addon Node

# ALCH.Alchemy
TES3:
  wbRecord(ALCH, 'Alchemy',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel,
    wbString(TEXT, 'Icon Filename'),
    wbScript, //[SCPT]
    wbFullName,
    wbStruct(ALDT, 'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2),
      wbInteger('Potion Value', itU32),
      wbInteger('Auto Calculate Value', itU32, wbBoolEnum).SetDefaultNativeValue(1)
    ]).SetRequired,
    wbEffects
  ]).SetFormIDBase($40);
TES4:
  wbRecord(ALCH, 'Potion',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    {wbStruct(OBME, 'Oblivion Magic Extender', [
      wbInteger('Record Version', itU8),
      wbOBMEVersion,
      wbUnused($1C)
    ]).SetDontShow(wbOBMEDontShow),}
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbFloat(DATA, 'Weight').SetRequired,
    wbStruct(ENIT, 'Data', [
      wbInteger('Value', itS32),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'No Auto-Calculate',
          {1} 'Food Item'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired,
    wbEffects.SetRequired
  ]);

# AMMO.Ammunition
TES4:
  wbRecord(AMMO, 'Ammunition',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbEnchantment(True),
    wbStruct(DATA, 'Data', [
      wbFloat('Speed'),
      wbInteger('Ignores Normal Weapon Resistance', itU8, wbBoolEnum),
      wbUnused(3),
      wbInteger('Value', itU32),
      wbFloat('Weight'),
      wbInteger('Damage', itU16)
    ]).SetRequired
  ]);

# ANIO.Animated Object
TES4:
  wbRecord(ANIO, 'Animated Object', [
    wbEDID,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbFormIDCk(DATA, 'Idle Animation', [IDLE])
      .SetDefaultNativeValue($0003ECAB)
      .SetRequired
  ]).SetSummaryKey([1, 2])
    .IncludeFlag(dfSummaryMembersNoName);

# APPA.Apparatus
TES3:
  wbRecord(APPA, 'Apparatus',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName.SetRequired,
    wbScript, //[SCPT]
    wbStruct(AADT, 'Data', [
      wbInteger('Type', itU32,
        wbEnum([
        {0} 'Mortar & Pestle',
        {1} 'Alembic',
        {2} 'Calcinator',
        {3} 'Retort'
        ])).SetDefaultNativeValue(1),
      wbFloat('Quality', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1)
    ]).SetRequired,
    wbIcon
  ]).SetFormIDBase($40);
TES4:
  wbRecord(APPA, 'Apparatus', [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU8,
        wbEnum([
          {0} 'Mortar & Pestle',
          {1} 'Alembic',
          {2} 'Calcinator',
          {3} 'Retort'
        ])),
      wbInteger('Value', itU32),
      wbFloat('Weight', cpNormal, False, 1, 4),
      wbFloat('Quality', cpNormal, False, 1, 0)
    ]).SetRequired
  ]);

# ARMA.Armature (Model)

# ARMO.Armor
TES3:
  wbRecord(ARMO, 'Armor',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName.SetRequired,
    wbScript, //[SCPT]
    wbStruct(AODT, 'Data', [
      wbInteger('Type', itU32,
        wbEnum([
        {0}  'Helmet',
        {1}  'Cuirass',
        {2}  'Left Pauldron',
        {3}  'Right Pauldron',
        {4}  'Greaves',
        {5}  'Boots',
        {6}  'Left Gauntlet',
        {7}  'Right Gauntlet',
        {8}  'Shield',
        {9}  'Left Bracer',
        {10} 'Right Bracer'
        ])).SetDefaultNativeValue(5),
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      wbInteger('Health', itU32).SetDefaultNativeValue(100),
      wbInteger('Enchanting Charge', itU32).SetDefaultNativeValue(100),
      wbInteger('Armor Rating', itU32).SetDefaultNativeValue(1)
    ]).SetRequired,
    wbIcon,
    wbBipedObjects,
    wbEnchantment //[ENCH]
  ]).SetFormIDBase($40);
TES4:
  wbRecord(ARMO, 'Armor',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbSCRI,
    wbEnchantment(True),
    wbStruct(BMDT, 'Flags', [
      wbInteger('Biped Flags', itU16, wbBipedFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('General Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'Hide Rings',
          1, 'Hide Amulets',
          6, 'Non-Playable',
          7, 'Heavy armor'
        ], False, 8))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(1)
    ]).SetRequired,
    wbRStruct('Male', [
      wbTexturedModel('Biped Model', [MODL, MODB, MODT], []),
      wbTexturedModel('World Model', [MOD2, MO2B, MO2T], []),
      wbString(ICON, 'Icon Image')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('Female', [
      wbTexturedModel('Biped Model', [MOD3, MO3B, MO3T], []),
      wbTexturedModel('World Model', [MOD4, MO4B, MO4T], []),
      wbString(ICO2, 'Icon Image')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbStruct(DATA, 'Data', [
      wbInteger('Armor', itU16, wbDiv(100)),
      wbInteger('Value', itU32),
      wbInteger('Health', itU32),
      wbFloat('Weight')
    ]).SetRequired
  ]);

# ARTO.Art Object

# ASPC.Acoustic Space

# ASTP.Association Type

# AVIF.Actor Values / Perk Tree Graphics

# BNDS.Bendable Spline

# BODY.Body Part
TES3:
  wbRecord(BODY, 'Body Part', @wbKnownSubRecordSignaturesNoFNAM,
    wbFlags(wbFlagsList([
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbString(FNAM, 'Skin Race')  //[RACE]
      .SetDefaultNativeValue('Argonian')
      .SetRequired,
    wbStruct(BYDT, 'Data', [
      wbInteger('Body Part', itU8,
        wbEnum([
        {0} 'Head',
        {1} 'Hair',
        {2} 'Neck',
        {3} 'Chest',
        {4} 'Groin',
        {5} 'Hand',
        {6} 'Wrist',
        {7} 'Forearm',
        {8} 'Upperarm',
        {9} 'Foot',
        {10} 'Ankle',
        {11} 'Knee',
        {12} 'Upperleg',
        {13} 'Clavicle',
        {14} 'Tail'
        ])).SetDefaultNativeValue(10),
      wbInteger('Skin Type', itU8,
        wbEnum([
        {0} 'Normal',
        {1} 'Vampire'
        ])),
      wbInteger('Flags', itU8,
        wbFlags([
        {0} 'Female',
        {1} 'Not Playable'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Part Type', itU8,
        wbEnum([
        {0} 'Skin',
        {1} 'Clothing',
        {2} 'Armor'
        ]))
    ]).SetRequired
  ]).SetFormIDBase($20)
    .SetSummaryKey([2]);

# BOIM.Biome

# BOOK.Book
TES3:
  wbRecord(BOOK, 'Book',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(BKDT, 'Book Data', [
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      wbInteger('Is Scroll', itU32, wbBoolEnum),
      wbInteger('Teaches', itS32, wbSkillEnum).SetDefaultNativeValue(-1), //[SKIL]
      wbInteger('Enchantment Charge', itU32).SetDefaultNativeValue(100)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon,
    wbLStringKC(TEXT, 'Book Text', 0, cpTranslate),
    wbEnchantment //[ENCH]
  ]).SetFormIDBase($40);
TES4:
  wbRecord(BOOK, 'Book',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbEnchantment(True),
    wbDESC,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Scroll',
          {1} 'Can''t be taken'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Teaches', itS8, wbSkillEnum).SetDefaultNativeValue(255),
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ]).SetRequired
  ], True);

# BPTD.Body Part Data

# BSGN.Birthsign
TES3:
  wbRecord(BSGN, 'Birthsign', [
    wbDeleted,
    wbEditorID,
    wbFullName,
    wbString(TNAM, 'Constellation Filename'),
    wbDescription,
    wbSpells
  ]).SetFormIDBase($10);
TES4:
  wbRecord(BSGN, 'Birthsign', [
    wbEDID.SetRequired,
    wbFULL,
    wbString(ICON, 'Constellation Filename'),
    wbDESC.SetRequired,
    wbSPLOs
  ]);

# CAMS.Camera Shot

# CELL.Cell
TES3:
  wbRecord(CELL, 'Cell', [
    wbString(NAME, 'Location').SetRequired,
    wbDeleted,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
        0, 'Is Interior Cell',
        1, 'Has Water',
        2, 'Illegal To Sleep Here',
        6, 'Has Map Color',
        7, 'Behave Like Exterior'
        ], False, 8))).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbStruct('Grid', [
        wbInteger('X', itS32),
        wbInteger('Y', itS32)
      ]).SetSummaryKey([0,1])
        .SetSummaryMemberPrefixSuffix(0, '(', ')')
        .SetSummaryMemberPrefixSuffix(1, '', ')')
        .SetSummaryDelimiter(', ')
        .SetDontShow(wbCellInteriorDontShow)
    ]).SetRequired,
    wbInteger(INTV, 'Water Height', itS32, nil, cpIgnore).SetDontShow(wbCellExteriorDontShow),
    wbString(RGNN, 'Region'),  //[REGN]
    wbByteColors(NAM5, 'Region Map Color').SetDontShow(wbCellInteriorDontShow),
    wbFloat(WHGT, 'Water Height').SetDontShow(wbCellExteriorDontShow),
    wbStruct(AMBI, 'Ambience', [
      wbByteColors('Ambient Color'),
      wbByteColors('Sunlight Color'),
      wbByteColors('Fog Color'),
      wbFloat('Fog Density', cpNormal, False, 1, 2).SetDefaultNativeValue(1)
    ]).SetDontShow(wbCellExteriorDontShow)
  ]).SetFormIDBase($B0)
    .SetGetGridCellCallback(function(const aSubRecord: IwbSubRecord; out aGridCell: TwbGridCell): Boolean begin
      with aGridCell, aSubRecord do begin
        Result := not (ElementNativeValues['Flags\Is Interior Cell'] = True);
        if Result then begin
          X := ElementNativeValues['Grid\X'];
          Y := ElementNativeValues['Grid\Y'];
        end;
      end;
    end)
    .SetGetFormIDCallback(function(const aMainRecord: IwbMainRecord; out aFormID: TwbFormID): Boolean begin
      var GridCell: TwbGridCell;
      Result := aMainRecord.GetGridCell(GridCell) and wbGridCellToFormID($A0, GridCell, aFormID);
    end)
    .SetIdentityCallback(function(const aMainRecord: IwbMainRecord): string begin
      var GridCell: TwbGridCell;
      if aMainRecord.GetGridCell(GridCell) then
        Result := '<Exterior>' + GridCell.SortKey
      else
        Result := aMainRecord.EditorID;
    end)
    .SetAfterLoad(wbCELLAfterLoad);
TES4:
wbRecord(CELL, 'Cell',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      17, 'Off Limits',
      19, 'Can''t Wait'
    ])), [
    wbEDID,
    wbFULL,
    wbInteger(DATA, 'Flags', itU8,
      wbFlags(wbSparseFlags([
        0, 'Is Interior Cell',
        1, 'Has Water',
        2, 'Can''t Travel From Here',
        3, 'Force Hide Land (Exterior) / Oblivion Interior (Interior)',
        5, 'Public Area',
        6, 'Hand Changed',
        7, 'Behave Like Exterior'
      ], False, 8))
    ).SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbStruct(XCLL, 'Lighting', [
      wbByteColors('Ambient Color'),
      wbByteColors('Directional Color'),
      wbByteColors('Fog Color'),
      wbFloat('Fog Near', cpNormal, True, 1, 4, nil, wbNormalizeToRange(-163840, 163840)),
      wbFloat('Fog Far', cpNormal, True, 1, 4, nil, wbNormalizeToRange(-163840, 163840)),
      wbInteger('Directional Rotation XY', itS32),
      wbInteger('Directional Rotation Z', itS32),
      wbFloat('Directional Fade', cpNormal, True, 1, 4, nil, wbNormalizeToRange(0, 10), 1),
      wbFloat('Fog Clip Dist', cpNormal, True, 1, 4, nil, wbNormalizeToRange(0, 163840))
    ]).SetDontShow(wbCellExteriorDontShow)
      .SetIsRemovable(wbCellLightingIsRemovable),
    wbArrayS(XCLR, 'Regions',
      wbFormIDCk('Region', [REGN])
    ).SetDontShow(wbCellInteriorDontShow),
    wbInteger(XCMT, 'Music', itU8, wbMusicEnum),
    wbFloat(XCLW, 'Water Height', cpBenign),
    wbFormIDCk(XCCM, 'Climate', [CLMT])
      .SetDefaultNativeValue(351)
      .SetDontShow(wbCellExteriorDontShow),
    wbFormIDCk(XCWT, 'Water', [WATR]).SetDefaultNativeValue(24),
    wbOwnership([XCCM, XCLW, XCMT]),
    IsTES4R(
      wbInteger(XTLI, 'Threat Level', itU32,
        wbEnum([],[
        1, 'Easy',
        2, 'Medium',
        3, 'Hard'
        ])).SetDefaultNativeValue(2),
      nil),
    IsTES4R(
      IfThen(wbSimpleRecords,
        wbUnknown(XLRL),
        wbArray(XLRL, 'Unknown',
          wbStruct('Unknown', [
            wbInteger('Unknown', itu32),
            wbInteger('Unknown', itu32),
            wbUnknown(4),
            wbUnknown(4),
            wbInteger('Unknown', itu32)
          ]))),
      nil),
    wbStruct(XCLC, 'Grid', [
      wbInteger('X', itS32),
      wbInteger('Y', itS32)
    ]).SetDontShow(wbCellInteriorDontShow)
      .SetIsRemovable(wbCellGridIsRemovable)
  ], True).SetAddInfo(wbCellAddInfo)
          .SetAfterLoad(wbCELLAfterLoad);

# CLAS.Class
TES3:
  wbRecord(CLAS, 'Class', [
    wbEditorID,
    wbDeleted,
    wbFullName.SetRequired,
    wbStruct(CLDT, 'Data', [
      wbArray('Primary Attributes',
        wbInteger('Attribute', itS32, wbAttributeEnum),
      2),
      wbInteger('Specialization', itU32, wbSpecializationEnum),
      wbArray('Major & Minor Skill Sets',
        wbStruct('Skill Set', [
          wbInteger('Minor Skill', itS32, wbSkillEnum), //[SKIL]
          wbInteger('Major Skill', itS32, wbSkillEnum) //[SKIL]
        ]),
      5),
      wbInteger('Playable', itU32, wbBoolEnum),
      wbInteger('Service Flags', itU32, wbServiceFlags).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbDescription
  ]).SetFormIDBase($18);
TES4:
  wbRecord(CLAS, 'Class', [
    wbEDID.SetRequired,
    wbFULL,
    wbDESC.SetRequired,
    wbString(ICON, 'Image Filename'),
    wbStruct(DATA, 'Data', [
      wbStruct('Primary Attributes', [
        wbInteger('Attribute #1', itU32, wbAttributeEnum),
        wbInteger('Attribute #2', itU32, wbAttributeEnum).SetDefaultNativeValue(1)
      ]),
      wbInteger('Specialization', itU32, wbSpecializationEnum),
      wbStruct('Major Skills', [
        wbInteger('Skill #1', itS32, wbMajorSkillEnum).SetDefaultNativeValue(12),
        wbInteger('Skill #2', itS32, wbMajorSkillEnum).SetDefaultNativeValue(13),
        wbInteger('Skill #3', itS32, wbMajorSkillEnum).SetDefaultNativeValue(14),
        wbInteger('Skill #4', itS32, wbMajorSkillEnum).SetDefaultNativeValue(15),
        wbInteger('Skill #5', itS32, wbMajorSkillEnum).SetDefaultNativeValue(16),
        wbInteger('Skill #6', itS32, wbMajorSkillEnum).SetDefaultNativeValue(17),
        wbInteger('Skill #7', itS32, wbMajorSkillEnum).SetDefaultNativeValue(18)
      ]),
      wbInteger('Flags', itU32,
        wbFlags([
          {0} 'Playable',
          {1} 'Guard'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Buys/Sells and Services', itU32, wbServiceFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Teaches', itS8, wbSkillEnum),
      wbInteger('Maximum training level', itU8),
      wbInteger('Unused', itU16)
    ], cpNormal, True, nil, 5)
  ]);

# CLFM.Color

# CLMT.Climate
TES4:
  wbRecord(CLMT, 'Climate', [
    wbEDID.SetRequired,
    wbArrayS(WLST, 'Weather Types',
      wbStructSK([0], 'Weather Type', [
        wbFormIDCk('Weather', [WTHR]),
        wbInteger('Chance', itS32)
      ])),
    wbString(FNAM, 'Sun Texture'),
    wbString(GNAM, 'Sun Glare Texture'),
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbClimateTiming(wbClmtTime, wbClmtMoonsPhaseLength)
  ]);

# CLOT.Clothing
TES3:
  wbRecord(CLOT, 'Clothing',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(CTDT, 'Data', [
      wbInteger('Type', itU32, wbEnum([
      {0} 'Pants',
      {1} 'Shoes',
      {2} 'Shirt',
      {3} 'Belt',
      {4} 'Robe',
      {5} 'Right Glove',
      {6} 'Left Glove',
      {7} 'Skirt',
      {8} 'Ring',
      {9} 'Amulet'
      ])).SetDefaultNativeValue(9),
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU16).SetDefaultNativeValue(1),
      wbInteger('Enchantment Charge', itU16).SetDefaultNativeValue(100)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon,
    wbBipedObjects,
    wbEnchantment //[ENCH]
  ]).SetFormIDBase($40);
TES4:
  wbRecord(CLOT, 'Clothing',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbSCRI,
    wbEnchantment(True),
    wbStruct(BMDT, 'Flags', [
      wbInteger('Biped Flags', itU16, wbBipedFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('General Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'Hide Rings',
          1, 'Hide Amulets',
          6, 'Non-Playable'
        ], False, 7))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(1)
    ]).SetRequired,
    wbRStruct('Male', [
      wbTexturedModel('Biped Model', [MODL, MODB, MODT], []),
      wbTexturedModel('World Model', [MOD2, MO2B, MO2T], []),
      wbString(ICON, 'Icon Image')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('Female', [
      wbTexturedModel('Biped Model', [MOD3, MO3B, MO3T], []),
      wbTexturedModel('World Model', [MOD4, MO4B, MO4T], []),
      wbString(ICO2, 'Icon Image')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ]).SetRequired
  ]);

# COBJ.Constructible Object

# COLL.Collision Layer

# CONT.Container
TES3:
  wbRecord(CONT, 'Container',
    wbFlags(wbFlagsList([
      10, 'Corpses Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbFloat(CNDT, 'Weight', cpNormal, False, 1, 2).SetRequired,
    wbInteger(FLAG, 'Flags', itU32,
      wbFlags(wbSparseFlags([
      0, 'Organic',
      1, 'Respawns',
      3, 'Can Hold Items'
      ], False, 4))
    ).SetDefaultNativeValue(4)
     .SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbScript, //[SCPT]
    wbInventory
  ]).SetFormIDBase($40);
TES4:
  wbRecord(CONT, 'Container',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbSCRI,
    wbCNTOs,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          1, 'Respawns'
        ], False, 2))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat('Weight')
    ]).SetRequired,
    wbFormIDCk(SNAM, 'Open Sound', [SOUN]),
    wbFormIDCk(QNAM, 'Close Sound', [SOUN])
  ]);

# CPTH.Camera Path

# CREA.Creature
TES3:
  wbRecord(CREA, 'Creature',
    wbFlags(wbFlagsList([
      10, 'Corpses Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbString(CNAM, 'Sound Generator Creature'), //[CREA]
    wbFullName,
    wbScript, //[SCPT]
    wbStruct(NPDT, 'Data', [
      wbInteger('Type', itU32,
        wbEnum([
        {0} 'Creature',
        {1} 'Daedra',
        {2} 'Undead',
        {3} 'Humanoid'
        ])),
      wbInteger('Level', itU32).SetDefaultNativeValue(1),
      wbStruct('Attributes', [
        wbInteger('Strength', itU32).SetDefaultNativeValue(50),
        wbInteger('Intelligence', itU32).SetDefaultNativeValue(50),
        wbInteger('Willpower', itU32).SetDefaultNativeValue(50),
        wbInteger('Agility', itU32).SetDefaultNativeValue(50),
        wbInteger('Speed', itU32).SetDefaultNativeValue(50),
        wbInteger('Endurance', itU32).SetDefaultNativeValue(50),
        wbInteger('Personality', itU32).SetDefaultNativeValue(50),
        wbInteger('Luck', itU32).SetDefaultNativeValue(50)
      ]),
      wbInteger('Health', itU32).SetDefaultNativeValue(50),
      wbInteger('Magicka', itU32).SetDefaultNativeValue(50),
      wbInteger('Fatigue', itU32).SetDefaultNativeValue(50),
      wbInteger('Soul', itU32).SetDefaultNativeValue(50),
      wbStruct('Skills', [
        wbInteger('Combat', itU32).SetDefaultNativeValue(50),
        wbInteger('Magic', itU32).SetDefaultNativeValue(50),
        wbInteger('Stealth', itU32).SetDefaultNativeValue(50)
      ]),
      wbArray('Attack Sets',
        wbStruct('Attack Set', [
          wbInteger('Minimum', itS32).SetDefaultNativeValue(1),
          wbInteger('Maximum', itS32).SetDefaultNativeValue(5)
        ]),
      3),
      wbInteger('Barter Gold', itU32)
    ]).SetRequired,
    wbInteger(FLAG, 'Flags', itU32,
      wbFlags(wbSparseFlags([
      0,  'Biped',
      1,  'Respawn',
      2,  'Weapon & Shield',
      3,  'Can Hold Items',
      4,  'Swims',
      5,  'Flies',
      6,  'Walks',
      7,  'Essential',
      10, 'Skeleton Blood',
      11, 'Metal Blood'
      ], False, 12))
    ).SetDefaultNativeValue(48)
     .SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbFloat(XSCL, 'Scale', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
    wbInventory,
    wbSpells,
    wbAIData,
    wbTravelServices,
    wbPackages
  ]).SetFormIDBase($40);
TES4:
  wbRecord(CREA, 'Creature',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      19, 'Starts Dead'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbCNTOs,
    wbSPLOs,
    wbArrayS(NIFZ, 'Model List', wbStringLC('Model')),
    wbModelInfos(NIFT, 'Model List Textures').SetRequired,
    wbStruct(ACBS, 'Configuration', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Biped',
          1,  'Essential',
          2,  'Weapon & Shield',
          3,  'Respawn',
          4,  'Swims',
          5,  'Flies',
          6,  'Walks',
          7,  'PC Level Offset',
          9,  'No Low Level Processing',
          11, 'No Blood Spray',
          12, 'No Blood Decal',
          15, 'No Head',
          16, 'No Right Arm',
          17, 'No Left Arm',
          18, 'No Combat in Water',
          19, 'No Shadow',
          20, 'No Corpse Check'
        ], False, 21), True)
      ).SetDefaultNativeValue(576)
       .IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Base spell points', itU16).SetDefaultNativeValue(50),
      wbInteger('Fatigue', itU16).SetDefaultNativeValue(50),
      wbInteger('Barter gold', itU16),
      wbInteger('Level (offset)', itS16).SetDefaultNativeValue(1),
      wbInteger('Calc min', itU16),
      wbInteger('Calc max', itU16)
    ]).SetRequired,
    wbRArrayS('Factions', wbFaction),
    wbFormIDCk(INAM, 'Death item', [LVLI]),
    wbSCRI,
    wbStruct(AIDT, 'AI Data', [
      wbInteger('Aggression', itU8).SetDefaultNativeValue(70),
      wbInteger('Confidence', itU8).SetDefaultNativeValue(50),
      wbInteger('Energy Level', itU8).SetDefaultNativeValue(50),
      wbInteger('Responsibility', itU8).SetDefaultNativeValue(50),
      wbInteger('Buys/Sells and Services', itU32, wbServiceFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Teaches', itS8, wbSkillEnum),
      wbInteger('Maximum training level', itU8),
      wbUnused(2)
    ]).SetRequired,
    wbRArray('AI Packages', wbFormIDCk(PKID, 'AI Package', [PACK])),
    wbArrayS(KFFZ, 'Animations', wbStringLC('Animation')),
    wbStruct(DATA, 'Creature Data', [
      wbInteger('Type', itU8,
        wbEnum([
          {0} 'Creature',
          {1} 'Daedra',
          {2} 'Undead',
          {3} 'Humanoid',
          {4} 'Horse',
          {5} 'Giant'
        ])),
      wbInteger('Combat Skill', itU8).SetDefaultNativeValue(50),
      wbInteger('Magic Skill', itU8).SetDefaultNativeValue(50),
      wbInteger('Stealth Skill', itU8).SetDefaultNativeValue(50),
      wbInteger('Soul', itU8, wbSoulGemEnum).SetDefaultNativeValue(3),
      wbUnused(1),
      wbInteger('Health', itU16).SetDefaultNativeValue(50),
      wbUnused(2),
      wbInteger('Attack Damage', itU16),
      wbInteger('Strength', itU8).SetDefaultNativeValue(50),
      wbInteger('Intelligence', itU8).SetDefaultNativeValue(50),
      wbInteger('Willpower', itU8).SetDefaultNativeValue(50),
      wbInteger('Agility', itU8).SetDefaultNativeValue(50),
      wbInteger('Speed', itU8).SetDefaultNativeValue(50),
      wbInteger('Endurance', itU8).SetDefaultNativeValue(50),
      wbInteger('Personality', itU8).SetDefaultNativeValue(50),
      wbInteger('Luck', itU8).SetDefaultNativeValue(50)
    ]).SetRequired,
    wbInteger(RNAM, 'Attack reach', itU8)
      .SetDefaultNativeValue(32)
      .SetRequired,
    wbFormIDCk(ZNAM, 'Combat Style', [CSTY]),
    wbFloat(TNAM, 'Turning Speed').SetRequired,
    wbFloat(BNAM, 'Base Scale')
      .SetDefaultNativeValue(1)
      .SetRequired,
    wbFloat(WNAM, 'Foot Weight')
      .SetDefaultNativeValue(3)
      .SetRequired,
    wbString(NAM0, 'Blood Spray'),
    wbString(NAM1, 'Blood Decal'),
    wbFormIDCk(CSCR, 'Inherits Sounds from', [CREA]),
    wbRArrayS('Sound Types',
      wbRStructSK([0], 'Sound Type', [
        wbInteger(CSDT, 'Type', itU32,
          wbEnum([
            {0} 'Left Foot',
            {1} 'Right Foot',
            {2} 'Left Back Foot',
            {3} 'Right Back Foot',
            {4} 'Idle',
            {5} 'Aware',
            {6} 'Attack',
            {7} 'Hit',
            {8} 'Death',
            {9} 'Weapon'
        ])),
        wbSoundTypeSounds
      ]))
  ], True);

# CSTY.Combat Style
TES4:
  wbRecord(CSTY, 'Combat Style', [
    wbEDID,
    wbStruct(CSTD, 'Standard', [
      wbInteger('Dodge % Chance', itU8).SetDefaultNativeValue(75),
      wbInteger('Left/Right % Chance', itU8).SetDefaultNativeValue(50),
      wbUnused(2),
      wbStruct('Dodge', [
        wbFloat('L/R Timer Min').SetDefaultNativeValue(0.5),
        wbFloat('L/R Timer Max').SetDefaultNativeValue(1.5),
        wbFloat('Forward Timer Min').SetDefaultNativeValue(0.5),
        wbFloat('Forward Timer Max').SetDefaultNativeValue(1),
        wbFloat('Back Timer Min').SetDefaultNativeValue(0.25),
        wbFloat('Back Timer Max').SetDefaultNativeValue(0.75)
      ]),
      wbFloat('Idle Timer Min').SetDefaultNativeValue(0.5),
      wbFloat('Idle Timer Max').SetDefaultNativeValue(1.5),
      wbInteger('Block % Chance', itU8).SetDefaultNativeValue(30),
      wbInteger('Attack % Chance', itU8).SetDefaultNativeValue(40),
      wbUnused(2),
      wbFloat('Recoil/Stagger Bonus to Attack').SetDefaultNativeValue(30),
      wbFloat('Unconscious Bonus to Attack').SetDefaultNativeValue(5),
      wbFloat('Hand-To-Hand Bonus to Attack').SetDefaultNativeValue(5),
      wbInteger('Power Attack % Chance', itU8).SetDefaultNativeValue(25),
      wbUnused(3),
      wbFloat('Recoil/Stagger Bonus to Power Attack').SetDefaultNativeValue(5),
      wbFloat('Unconscious Bonus to Power Attack').SetDefaultNativeValue(5),
      wbStruct('Power Attack', [
        wbInteger('Normal', itU8).SetDefaultNativeValue(20),
        wbInteger('Forward', itU8).SetDefaultNativeValue(20),
        wbInteger('Back', itU8).SetDefaultNativeValue(20),
        wbInteger('Left', itU8).SetDefaultNativeValue(20),
        wbInteger('Right', itU8).SetDefaultNativeValue(20)
      ]),
      wbUnused(3),
      wbFloat('Hold Timer Min').SetDefaultNativeValue(0.5),
      wbFloat('Hold Timer Max').SetDefaultNativeValue(1.5),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Advanced',
          {1} 'Choose Attack using % Chance',
          {2} 'Ignore Allies in Area',
          {3} 'Will Yield',
          {4} 'Rejects Yields',
          {5} 'Fleeing Disabled',
          {6} 'Prefers Ranged',
          {7} 'Melee Alert OK'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Acrobatic Dodge % Chance', itU8),
      wbUnused(2),
      wbFloat('Range Mult (Optimal)').SetDefaultNativeValue(1),
      wbFloat('Range Mult (Max)').SetDefaultNativeValue(1),
      wbFloat('Switch Distance (Melee)').SetDefaultNativeValue(250),
      wbFloat('Switch Distance (Ranged)').SetDefaultNativeValue(1000),
      wbFloat('Buff standoff Distance').SetDefaultNativeValue(325),
      wbFloat('Ranged standoff Distance').SetDefaultNativeValue(500),
      wbFloat('Group standoff Distance').SetDefaultNativeValue(325),
      wbInteger('Rushing Attack % Chance', itU8).SetDefaultNativeValue(25),
      wbUnused(3),
      wbFloat('Rushing Attack Distance Mult').SetDefaultNativeValue(1),
      wbInteger('Do Not Acquire', itU32, wbBoolEnum)
    ], cpNormal, True, nil, 30),
    wbStruct(CSAD, 'Advanced', [
      wbFloat('Dodge Fatigue Mod Mult').SetDefaultNativeValue(-20),
      wbFloat('Dodge Fatigue Mod Base'),
      wbFloat('Encumbered Speed Mod Base').SetDefaultNativeValue(-110),
      wbFloat('Encumbered Speed Mod Mult').SetDefaultNativeValue(1),
      wbStruct('Dodge', [
        wbFloat('While Under Attack Mult').SetDefaultNativeValue(1),
        wbFloat('Not Under Attack Mult').SetDefaultNativeValue(0.75),
        wbFloat('Back While Under Attack Mult').SetDefaultNativeValue(1),
        wbFloat('Back Not Under Attack Mult').SetDefaultNativeValue(0.7),
        wbFloat('Forward While Attacking Mult').SetDefaultNativeValue(1),
        wbFloat('Forward Not Attacking Mult').SetDefaultNativeValue(0.5)
      ]),
      wbStruct('Block', [
        wbFloat('Skill Modifier Mult').SetDefaultNativeValue(20),
        wbFloat('Skill Modifier Base'),
        wbFloat('While Under Attack Mult').SetDefaultNativeValue(2),
        wbFloat('Not Under Attack Mult').SetDefaultNativeValue(1)
      ]),
      wbStruct('Attack', [
        wbFloat('Skill Modifier Mult').SetDefaultNativeValue(20),
        wbFloat('Skill Modifier Base'),
        wbFloat('While Under Attack Mult').SetDefaultNativeValue(0.75),
        wbFloat('Not Under Attack Mult').SetDefaultNativeValue(1),
        wbFloat('During Block Mult').SetDefaultNativeValue(0.5)
      ]),
      wbFloat('Power Attack Fatigue Mod Base').SetDefaultNativeValue(5),
      wbFloat('Power Attack Fatigue Mod Mult').SetDefaultNativeValue(-10)
    ])
  ]);

# DEBR.Debris

# DIAL.Dialog Topic
TES3:
  wbRecord(DIAL, 'Dialog Topic', [
    wbEditorID,
    wbStruct(DATA, 'Data', [
      wbInteger('Dialog Type', itU8, wbDialogTypeEnum),
      wbUnused(3)
    ]).SetRequired,
    wbDeleted
  ]).SetFormIDBase($80)
    .SetSummaryKey([1]);
TES4:
  wbRecord(DIAL, 'Dialog Topic', [
    wbEDID,
    wbQSTI,
    wbQSTR,
    wbFULL
      .SetAfterLoad(wbDialogueTextAfterLoad)
      .SetAfterSet(wbDialogueTextAfterSet),
    wbInteger(DATA, 'Type', itU8, wbDialogueTypeEnum).SetRequired,
    wbINOM,
    wbINOA
  ]);

# DLVW.Dialog View

# DOBJ.Default Object Manager

# DMGT.Damage Type

# DOOR.Door
TES3:
  wbRecord(DOOR, 'Door',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel,
    wbFullName,
    wbScript, //[SCPT]
    wbString(SNAM, 'Open Sound'), //[SOUN]
    wbString(ANAM, 'Close Sound') //[SOUN]
  ]).SetFormIDBase($40);
TES4:
  wbRecord(DOOR, 'Door',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbSCRI,
    wbFormIDCk(SNAM, 'Open Sound', [SOUN]),
    wbFormIDCk(ANAM, 'Close Sound', [SOUN]),
    wbFormIDCk(BNAM, 'Loop Sound', [SOUN]),
    wbInteger(FNAM, 'Flags', itU8,
      wbFlags([
        {0} 'Oblivion Gate',
        {1} 'Automatic Door',
        {2} 'Hidden',
        {3} 'Minimal Use'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Random Teleport Destinations',
      wbFormIDCk(TNAM, 'Destination', [CELL, WRLD]))
  ]);

# DUEL.Dual Cast Data

# ECZN.Encounter Zone

# EFSH.Effect Shader
TES4:

  wbRecord(EFSH, 'Effect Shader', [
    wbEDID,
    wbString(ICON, 'Fill Texture').SetRequired,
    wbString(ICO2, 'Particle Shader Texture').SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'No Membrane Shader',
          3, 'No Particle Shader',
          4, 'Edge Effect - Inverse',
          5, 'Membrane Shader - Affect Skin Only'
        ], False, 6))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbStruct('Membrane Shader', [
        wbInteger('Source Blend Mode', itU32, wbBlendModeEnum).SetDefaultNativeValue(5),
        wbInteger('Blend Operation', itU32, wbBlendOpEnum).SetDefaultNativeValue(1),
        wbInteger('Z Test Function', itU32, wbZTestFuncEnum).SetDefaultNativeValue(3)
      ]),
      wbStruct('Fill/Texture Effect', [
        wbByteColors('Color'),
        wbFloat('Alpha Fade In Time'),
        wbFloat('Full Alpha Time'),
        wbFloat('Alpha Fade Out Time'),
        wbFloat('Persistent Alpha Ratio'),
        wbFloat('Alpha Pulse Amplitude'),
        wbFloat('Alpha Pulse Frequency').SetDefaultNativeValue(1),
        wbFloat('Texture Animation Speed (U)'),
        wbFloat('Texture Animation Speed (V)')
      ]),
      wbStruct('Edge Effect', [
        wbFloat('Fall Off').SetDefaultNativeValue(1),
        wbByteColors('Color'),
        wbFloat('Alpha Fade In Time'),
        wbFloat('Full Alpha Time'),
        wbFloat('Alpha Fade Out Time'),
        wbFloat('Persistent Alpha Ratio'),
        wbFloat('Alpha Pulse Amplitude'),
        wbFloat('Alpha Pusle Frequence').SetDefaultNativeValue(1)
      ]),
      wbFloat('Fill/Texture Effect - Full Alpha Ratio').SetDefaultNativeValue(1),
      wbFloat('Edge Effect - Full Alpha Ratio').SetDefaultNativeValue(1),
      wbInteger('Membrane Shader - Dest Blend Mode', itU32, wbBlendModeEnum).SetDefaultNativeValue(6),
      wbStruct('Particle Shader', [
        wbInteger('Source Blend Mode', itU32, wbBlendModeEnum).SetDefaultNativeValue(5),
        wbInteger('Blend Operation', itU32, wbBlendOpEnum).SetDefaultNativeValue(1),
        wbInteger('Z Test Function', itU32, wbZTestFuncEnum).SetDefaultNativeValue(4),
        wbInteger('Dest Blend Mode', itU32, wbBlendModeEnum).SetDefaultNativeValue(6),
        wbFloat('Particle Birth Ramp Up Time'),
        wbFloat('Full Particle Birth Time'),
        wbFloat('Particle Birth Ramp Down Time'),
        wbFloat('Full Particle Birth Ratio').SetDefaultNativeValue(1),
        wbFloat('Persistant Particle Birth Ratio').SetDefaultNativeValue(1),
        wbFloat('Particle Lifetime').SetDefaultNativeValue(1),
        wbFloat('Particle Lifetime +/-'),
        wbFloat('Initial Speed Along Normal'),
        wbFloat('Acceleration Along Normal'),
        wbFloat('Initial Velocity #1'),
        wbFloat('Initial Velocity #2'),
        wbFloat('Initial Velocity #3'),
        wbFloat('Acceleration #1'),
        wbFloat('Acceleration #2'),
        wbFloat('Acceleration #3'),
        wbFloat('Scale Key 1').SetDefaultNativeValue(1),
        wbFloat('Scale Key 2').SetDefaultNativeValue(1),
        wbFloat('Scale Key 1 Time'),
        wbFloat('Scale Key 2 Time').SetDefaultNativeValue(1)
      ]),
      wbByteColors('Color Key 1 - Color'),
      wbByteColors('Color Key 2 - Color'),
      wbByteColors('Color Key 3 - Color'),
      wbFloat('Color Key 1 - Color Alpha').SetDefaultNativeValue(1),
      wbFloat('Color Key 2 - Color Alpha').SetDefaultNativeValue(1),
      wbFloat('Color Key 3 - Color Alpha').SetDefaultNativeValue(1),
      wbFloat('Color Key 1 - Color Key Time'),
      wbFloat('Color Key 2 - Color Key Time').SetDefaultNativeValue(0.5),
      wbFloat('Color Key 3 - Color Key Time').SetDefaultNativeValue(1)
    ], cpNormal, True, nil, 8)
  ]);

# ENCH.Enchantment
TES3:
  wbRecord(ENCH, 'Enchantment',
    wbFlags(wbFlagsList([
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbStruct(ENDT, 'Data', [
      wbInteger('Cast Type', itU32,
        wbEnum([
        {0} 'Cast Once',
        {1} 'Cast Strikes',
        {2} 'Cast When Used',
        {3} 'Constant Effect'
        ])),
      wbInteger('Enchantment Cost', itU32),
      wbInteger('Charge Amount', itU32),
      wbInteger('Flags', itU8,
        wbFlags([
        {0} 'Auto Calculate'
        ], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired,
    wbEffects
  ]).SetFormIDBase($04).SetSummaryKey([3]);
TES4:
  wbRecord(ENCH, 'Enchantment', [
    wbEDID,
    {wbStruct(OBME, 'Oblivion Magic Extender', [
      wbInteger('Record Version', itU8),
      wbOBMEVersion,
      wbUnused($1C)
    ]).SetDontShow(wbOBMEDontShow),}
    wbFULL,
    wbStruct(ENIT, 'Data', [
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'Scroll',
          {1} 'Staff',
          {2} 'Weapon',
          {3} 'Apparel'
        ])).SetDefaultNativeValue(2),
      wbInteger('Charge Amount', itU32),
      wbInteger('Enchant Cost', itU32),
      wbInteger('No Autocalc Cost', itU8, wbBoolEnum),
      wbUnused(3)
    ]).SetRequired,
    wbEffects
  ]);

# EQUP.Equip Slots

# EXPL.Explosion

# EYES.Eyes
TES4:
  wbRecord(EYES, 'Eyes', [
    wbEDID.SetRequired,
    wbFULL,
    wbString(ICON, 'Texture').SetRequired,
    wbInteger(DATA, 'Playable', itU8, wbBoolEnum).SetRequired
  ]);

# FACT.Faction
TES3:
  wbRecord(FACT, 'Faction', [
    wbEditorID,
    wbDeleted,
    wbFullName.SetRequired,
    wbRArray('Ranks', wbStringForward(RNAM, 'Rank', 32)),
    wbStruct(FADT, 'Data', [
      wbArray('Favored Attributes', wbInteger('Attribute', itS32, wbAttributeEnum), 2),
      wbArray('Rank Requirements',
        wbStruct('Rank', [
          wbInteger('Attribute 1', itS32),
          wbInteger('Attribute 2', itS32),
          wbInteger('Primary Skills', itS32),
          wbInteger('Favored Skills', its32),
          wbInteger('Faction Reputation', itU32)
        ]).SetSummaryKey([0,1,2,3,4])
          .SetSummaryMemberPrefixSuffix(0, 'Attribute 1: ', ',')
          .SetSummaryMemberPrefixSuffix(1, 'Attribute 2: ', ',')
          .SetSummaryMemberPrefixSuffix(2, 'Primary Skills: ', ',')
          .SetSummaryMemberPrefixSuffix(3, 'Favored Skills: ', ',')
          .SetSummaryMemberPrefixSuffix(4, 'Faction Reputation: ', '')
          .IncludeFlag(dfCollapsed, wbCollapseFactionRanks)
          .IncludeFlag(dfSummaryMembersNoName),
      10),
      wbArray('Favored Skills', wbInteger('Skill', itS32, wbSkillEnum), 7),
      wbInteger('Hidden From Player', itU32, wbBoolEnum)
    ]).SetRequired,
    wbRArrayS('Relations',
      wbRStructSK([0], 'Relation', [
        wbString(ANAM, 'Faction'), //[FACT]
        wbInteger(INTV, 'Reaction', itS32)
      ])).SetToStr(wbFactionReactionToStr)
  ]).SetFormIDBase($1C);
TES4:
  wbRecord(FACT, 'Faction', [
    wbEDID.SetRequired,
    wbFULL,
    wbFactionRelations,
    wbInteger(DATA, 'Flags', itU8,
      wbFlags([
        {0} 'Hidden from Player',
        {1} 'Evil',
        {2} 'Special Combat'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbFloat(CNAM, 'Crime Gold Multiplier')
      .SetDefaultNativeValue(1)
      .SetRequired,
    wbRArrayS('Ranks',
      wbRStructSK([0], 'Rank', [
        wbInteger(RNAM, 'Rank#', itS32),
        wbString(MNAM, 'Male', 0, cpTranslate),
        wbString(FNAM, 'Female', 0, cpTranslate),
        wbString(INAM, 'Insignia')
      ]))
  ]);

# FLOR.Flora
TES4:
  wbRecord(FLOR, 'Flora', [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbSCRI,
    wbFormIDCk(PFIG, 'Ingredient', [INGR]),
    wbSeasons
  ]);

# FLST.Form List (non-leveled list)

# FSTP.Footstep

# FSTS.Footstep Set

# FURN.Furniture
TES4:
  wbRecord(FURN, 'Furniture',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbSCRI,
    wbByteArray(MNAM, 'Marker Flags', 4).SetRequired
  ]);

# GLOB.Global
TES3:
  wbRecord(GLOB, 'Global', @wbKnownSubRecordSignaturesNoFNAM,  [
    wbEditorID,
    wbDeleted,
    wbInteger(FNAM, 'Variable Type', itU8,
      wbEnum([], [
      $66, 'Float',
      $6C, 'Long',
      $73, 'Short'
      ])).SetDefaultNativeValue($73),
    wbFloat(FLTV, 'Value', cpNormal, False, 1, 2)
  ]).SetFormIDBase($58)
    .SetSummaryKey([3])
    .SetAfterLoad(wbGlobalAfterLoad);
TES4:
  wbRecord(GLOB, 'Global', [
    wbEDID.SetRequired,
    wbInteger(FNAM, 'Type', itU8,
      wbEnum([], [
        Ord('s'), 'Short',
        Ord('l'), 'Long',
        Ord('f'), 'Float'
      ])).SetDefaultEditValue('Short')
         .SetRequired,
    wbFloat(FLTV, 'Value').SetRequired
  ]).SetSummaryKey([2]);

# GMST.Game Setting
TES3:
  wbRecord(GMST, 'Game Setting', [
    wbEditorID,
    wbRUnion('Value', [
      wbString(STRV, 'String Value'),
      wbInteger(INTV, 'Integer Value', itS32),
      wbFloat(FLTV, 'Float Value', cpNormal, False, 1, 4)
    ])
  ]).SetFormIDBase($50)
    .SetSummaryKey([1])
    .IncludeFlag(dfIndexEditorID);
TES4:
  wbRecord(GMST, 'Game Setting', [
    wbEDID.SetRequired,
    wbUnion(DATA, 'Value', wbGMSTUnionDecider, [
      wbString('Name', 0, cpTranslate),
      wbInteger('Int', itS32),
      wbFloat('Float')
    ]).SetRequired
  ]).SetSummaryKey([1])
    .IncludeFlag(dfIndexEditorID);

# GRAS.Grass
TES4:
  wbRecord(GRAS, 'Grass', [
    wbEDID,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbStruct(DATA, 'Data', [
      wbInteger('Density', itU8).SetDefaultNativeValue(30),
      wbInteger('Min Slope', itU8),
      wbInteger('Max Slope', itU8).SetDefaultNativeValue(90),
      wbUnused(1),
      wbInteger('Unit from water amount', itU16),
      wbUnused(2),
      wbInteger('Unit from water type', itU32,
        wbEnum([
          {0} 'Above - At Least',
          {1} 'Above - At Most',
          {2} 'Below - At Least',
          {3} 'Below - At Most',
          {4} 'Either - At Least',
          {5} 'Either - At Most',
          {6} 'Either - At Most Above',
          {7} 'Either - At Most Below'
        ])),
      wbFloat('Position Range').SetDefaultNativeValue(32),
      wbFloat('Height Range').SetDefaultNativeValue(0.2),
      wbFloat('Color Range').SetDefaultNativeValue(0.5),
      wbFloat('Wave Period').SetDefaultNativeValue(10),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Vertex Lighting',
          {1} 'Uniform Scaling',
          {2} 'Fit to Slope'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired
  ]).SetSummaryKey([1]);

# GRUP.Form Group

# HAIR.Hair
TES4:
  wbRecord(HAIR, 'Hair', [
    wbEDID.SetRequired,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbString(ICON, 'Texture').SetRequired,
    wbInteger(DATA, 'Flags', itU8,
      wbFlags([
        {0} 'Playable',
        {1} 'Not Male',
        {2} 'Not Female',
        {3} 'Fixed'
      ])
    ).SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);

# HAZD.Hazard

# HDPT.Head Part

# IDLE.Idle Animations
TES4:
  wbRecord(IDLE, 'Idle Animation', [
    wbEDID.SetRequired,
    wbTexturedModel('Model', [MODL, MODB, MODT], []).SetRequired,
    wbConditions,
    wbInteger(ANAM, 'Animation Group Section', itU8, wbIdleAnam).SetRequired,
    wbStruct(DATA, 'Animations', [
      wbFormIDCk('Parent', [IDLE, NULL], False, cpBenign),
      wbFormIDCk('Previous', [IDLE, NULL], False, cpBenign)
    ]).SetRequired
  ]).SetSummaryKey([1]);

# IDLM.Idle Marker

# IMAD.Image Space Modifier

# IMGS.Image Space

# INFO.Dialog Response
TES3:
  wbRecord(INFO, 'Dialog Response', @wbKnownSubRecordSignaturesINFO, [
    wbString(INAM, 'Response ID').SetRequired,
    wbString(PNAM, 'Previous Response ID').SetRequired,
    wbString(NNAM, 'Next Response ID').SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Dialog Type', itU32, wbDialogTypeEnum),
      wbInteger('Disposition/Index', itU32),
      wbInteger('Speaker Faction Rank', itS8).SetDefaultNativeValue(-1),
      wbInteger('Sex', itS8, wbSexEnum).SetDefaultNativeValue(-1),
      wbInteger('Player Faction Rank', itS8).SetDefaultNativeValue(-1),
      wbUnused(1)
    ]).SetRequired,
    wbString(ONAM, 'Speaker'), //[NPC_]
    wbString(RNAM, 'Speaker Race'), //[RACE]
    wbString(CNAM, 'Speaker Class'), //[CLAS]
    wbString(FNAM, 'Speaker Faction'), //[FACT]
    wbString(ANAM, 'Speaker Cell'), //[CELL]
    wbString(DNAM, 'Player Faction'), //[FACT]
    wbString(SNAM, 'Sound Filename'),
    wbString(NAME, 'Response'),
    wbDeleted,
    wbRArray('Conditions',
      wbRStruct('Condition', [
        wbStruct(SCVR, 'Condition', [
          wbInteger('Position', itU8,
            wbEnum([], [
            48, '1st', //0
            49, '2nd', //1
            50, '3rd', //2
            51, '4th', //3
            52, '5th', //4
            53, '6th' //5
            ])),
          wbInteger('Type', itU8,
            wbEnum([], [
            49, 'Function',
            50, 'Global',
            51, 'Local',
            52, 'Journal',
            53, 'Item',
            54, 'Dead',
            55, 'Not ID',
            56, 'Not Faction',
            57, 'Not Class',
            65, 'Not Race',
            66, 'Not Cell',
            67, 'Not Local'
            ])),
          wbUnion('Function', wbConditionFunctionDecider, [
            wbInteger('Function', itU16,
              wbEnum([], [
              12336, 'Reaction Low',
              12337, 'PC Strength',
              12338, 'PC Enchant',
              12339, 'PC Sneak',
              12340, 'PC Common Disease',
              12341, 'Choice',
              12342, 'PC Vampire',
              12343, 'Flee',
              12592, 'Reaction High',
              12593, 'PC Block',
              12594, 'PC Destruction',
              12595, 'PC Acrobatics',
              12596, 'PC Blight Disease',
              12597, 'PC Intelligence',
              12598, 'Level',
              12599, 'Should Attack',
              12848, 'Rank Requirement',
              12849, 'PC Armorer',
              12850, 'PC Alteration',
              12851, 'PC Light Armor',
              12852, 'PC Clothing Modifier',
              12853, 'PC Willpower',
              12854, 'Attacked',
              12855, 'Werewolf',
              13104, 'Reputation',
              13105, 'PC Medium Armor',
              13106, 'PC Illusion',
              13107, 'PC Short Blade',
              13108, 'PC Crime Level',
              13109, 'PC Agility',
              13110, 'Talked To PC',
              13111, 'PC Werewolf Kills',
              13360, 'Health Percent',
              13361, 'PC Heavy Armor',
              13362, 'PC Conjuration',
              13363, 'PC Marksman',
              13364, 'Same Sex',
              13365, 'PC Speed',
              13366, 'PC Health',
              13616, 'PC Reputation',
              13617, 'PC Blunt Weapon',
              13619, 'PC Mysticism',
              13619, 'PC Mercantile',
              13620, 'Same Race',
              13621, 'PC Endurance',
              13622, 'Creature Target',
              13872, 'PC Level',
              13873, 'PC Long Blade',
              13874, 'PC Restoration',
              13875, 'PC Speechcraft',
              13876, 'Same Faction',
              13877, 'PC Personality',
              13878, 'Friend Hit',
              14128, 'PC Health Percent',
              14129, 'PC Axe',
              14130, 'PC Alchemy',
              14131, 'PC Hand To Hand',
              14132, 'Faction Rank Difference',
              14133, 'PC Luck',
              14134, 'Fight',
              14384, 'PC Magicka',
              14385, 'PC Spear',
              14386, 'PC Unarmored',
              14387, 'PC Sex',
              14388, 'Detected',
              14389, 'PC Corpus',
              14390, 'Hello',
              14640, 'PC Fatigue',
              14641, 'PC Athletics',
              16462, 'PC Security',
              14643, 'PC Expelled',
              14644, 'Alarmed',
              14645, 'Weather',
              14646, 'Alarm'
              ])).SetDefaultNativeValue(14646),
            wbInteger('Function', itU16,
              wbEnum([], [
              22630, 'Float',
              22636, 'Long',
              22643, 'Short'
              ])).SetDefaultNativeValue(22630),
            wbInteger('Function', itU16,
              wbEnum([], [
              22602, 'Journal'
              ])).SetDefaultNativeValue(22602),
            wbInteger('Function', itU16,
              wbEnum([], [
              22601, 'Item'
              ])).SetDefaultNativeValue(22601),
            wbInteger('Function', itU16,
              wbEnum([], [
              22596, 'Dead'
              ])).SetDefaultNativeValue(22596),
            wbInteger('Function', itU16,
              wbEnum([], [
              22616, 'Not ID'
              ])).SetDefaultNativeValue(22616),
            wbInteger('Function', itU16,
              wbEnum([], [
              22598, 'Not Faction'
              ])).SetDefaultNativeValue(22598),
            wbInteger('Function', itU16,
              wbEnum([], [
              22595, 'Not Class'
              ])).SetDefaultNativeValue(22595),
            wbInteger('Function', itU16,
              wbEnum([], [
              22610, 'Not Race'
              ])).SetDefaultNativeValue(22610),
            wbInteger('Function', itU16,
              wbEnum([], [
              22604, 'Not Cell'
              ])).SetDefaultNativeValue(22604)
            ]),
          wbInteger('Operator', itU8,
            wbEnum([], [
            48, 'Equal To',
            49, 'Not Equal To',
            50, 'Less Than',
            51, 'Less Than Or Equal To',
            52, 'Greater Than',
            53, 'Greater Than Or Equal To'
            ])),
          wbString('Variable/Object')
        ]),
        wbRUnion('Value', [
          wbInteger(INTV, 'Value', itS32),
          wbFloat(FLTV, 'Value')
        ])
      ])),
    wbRStruct('Quest Data', [
      wbInteger(QSTN, 'Quest Named', itU8, wbBoolEnum).SetDefaultNativeValue(1),
      wbInteger(QSTF, 'Quest Finished', itU8, wbBoolEnum).SetDefaultNativeValue(1),
      wbInteger(QSTR, 'Quest Restarted', itU8, wbBoolEnum).SetDefaultNativeValue(1)
    ], [], cpNormal, False, nil, True),
    wbString(BNAM, 'Result')
  ]).SetFormIDBase($90);
TES4:
  wbRecord(INFO, 'Dialog response', [
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU8, wbDialogueTypeEnum),
      wbNextSpeaker,
      wbInteger('Flags', itU8,
        wbFlags([
        {0} 'Goodbye',
        {1} 'Random',
        {2} 'Say Once',
        {3} 'Run Immediately',
        {4} 'Info Refusal',
        {5} 'Random End',
        {6} 'Run for Rumors'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True, nil, 2),
    wbFormIDCkNoReach(QSTI, 'Quest', [QUST]).SetRequired,
    wbFormIDCkNoReach(TPIC, 'Previous Topic', [DIAL]),
    wbFormIDCkNoReach(PNAM, 'Previous Info', [INFO,NULL]),
    wbRArray('Add Topics', wbFormIDCk(NAME, 'Topic', [DIAL])),
    wbRArray('Responses',
      wbRStruct('Response', [
        wbStruct(TRDT, 'Response Data', [
          wbInteger('Emotion Type', itU32,
            wbEnum([
            {0} 'Neutral',
            {1} 'Anger',
            {2} 'Disgust',
            {3} 'Fear',
            {4} 'Sad',
            {5} 'Happy',
            {6} 'Surprise'
            ])),
          wbInteger('Emotion Value', itS32),
          wbUnused(4),
          wbInteger('Response Number', itU8),
          wbUnused(3)
        ]),
        wbStringKC(NAM1, 'Response Text', 0, cpTranslate)
          .SetAfterLoad(wbDialogueTextAfterLoad)
          .SetAfterSet(wbDialogueTextAfterSet)
          .SetRequired,
        wbString(NAM2, 'Actor Notes', 0, cpTranslate)
      ]).SetSummaryKey([1])
        .IncludeFlag(dfCollapsed)
    ),
    wbConditions,
    wbRArray('Choices', wbFormIDCk(TCLT, 'Choice', [DIAL])),
    wbRArray('Link From', wbFormIDCk(TCLF, 'Topic', [DIAL])),
    wbResultScript
  ]).SetAddInfo(wbINFOAddInfo);

# INGR.Ingredient
TES3:
  wbRecord(INGR, 'Ingredient',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(IRDT, 'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      wbStruct('Effects', [
        wbArray('Magic Effects',
          wbInteger('Magic Effect', itS32, wbMagicEffectEnum).SetDefaultNativeValue(-1),
        4),
        wbArray('Skills',
          wbInteger('Skill', itS32, wbSkillEnum).SetDefaultNativeValue(-1),
        4),
        wbArray('Attributes',
          wbInteger('Attribute', itS32, wbAttributeEnum).SetDefaultNativeValue(-1),
        4)
      ])
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon
  ]).SetFormIDBase($40)
    .SetAfterLoad(wbIngredientAfterLoad);
TES4:
  wbRecord(INGR, 'Ingredient',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    {wbStruct(OBME, 'Oblivion Magic Extender', [
      wbInteger('Record Version', itU8),
      wbOBMEVersion,
      wbUnused($1C)
    ]).SetDontShow(wbOBMEDontShow),}
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbFloat(DATA, 'Weight').SetRequired,
    wbStruct(ENIT, 'Data', [
      wbInteger('Value', itS32),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'No Auto-Calculate',
          {1} 'Food Item'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired,
    wbEffects
  ]);

# IPCT.Impact Data

# IPDS.Impact Data Set

# KEYM.Key
TES4:
  wbRecord(KEYM, 'Key',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ]).SetRequired
  ]);

# KYWD.Keyword

# LAND.Landscape
TES3:
  wbRecord(LAND, 'Landscape', @wbKnownSubRecordSignaturesLAND, [
    wbStruct(INTV, 'Grid', [
      wbInteger('X', itS32),
      wbInteger('Y', itS32)
    ], cpCritical).SetSummaryKeyOnValue([0,1])
                  .SetSummaryPrefixSuffixOnValue(0, '(', '')
                  .SetSummaryPrefixSuffixOnValue(1, '', ')')
                  .SetRequired,
    wbInteger(DATA, 'Flags', itU32,
      wbFlags([
      {0} 'Has Vertex Normals/Height Map',
      {1} 'Has Vertex Colors',
      {2} 'Has Landscape Textures',
      {3} 'User Created/Edited'
      ])).SetDefaultNativeValue(8)
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    IfThen(wbSimpleRecords,
      wbByteArray(VNML, 'Vertex Normals'),
      wbArray(VNML, 'Vertex Normals',
        wbArray('Row',
          wbStruct('Column', [
            wbInteger('X', itS8, nil, cpBenign, False, nil, nil, 0, wbLandNormalsGetCP),
            wbInteger('Y', itS8, nil, cpBenign, False, nil, nil, 0, wbLandNormalsGetCP),
            wbInteger('Z', itS8, nil, cpBenign, False, nil, nil, 0, wbLandNormalsGetCP)
          ]).SetSummaryKey([0,1,2])
            .SetSummaryMemberPrefixSuffix(0, '(', '')
            .SetSummaryMemberPrefixSuffix(2, '', ')')
            .IncludeFlag(dfCollapsed, wbCollapseVec3)
            .IncludeFlag(dfSummaryMembersNoName),
        65).SetSummaryName('Columns')
           .IncludeFlag(dfCollapsed, wbCollapseVertices),
      65).SetSummaryName('Rows')
         .IncludeFlag(dfCollapsed, wbCollapseVertices)),
    IfThen(wbSimpleRecords,
      wbByteArray(VHGT, 'Vertex Height Map'),
      wbStruct(VHGT, 'Vertex Height Map', [
        wbFloat('Offset'),
        wbUnused(1),
        wbArray('Height Map',
          wbArray('Row',
            wbInteger('Column', itS8),
          65).SetSummaryName('Columns')
             .IncludeFlag(dfCollapsed, wbCollapseVertices),
        65).SetSummaryName('Rows')
           .IncludeFlag(dfCollapsed, wbCollapseVertices),
        wbUnused(2)
      ])),
    IfThen(wbSimpleRecords,
      wbByteArray(WNAM, 'World Map Colors'),
      wbArray(WNAM, 'World Map Colors',
        wbArray('Row',
          wbInteger('Column', itS8),
        9).SetSummaryName('Columns')
          .IncludeFlag(dfCollapsed, wbCollapseOther),
      9).SetSummaryName('Rows')
        .IncludeFlag(dfCollapsed, wbCollapseOther)),
    IfThen(wbSimpleRecords,
      wbByteArray(VCLR, 'Vertex Colors'),
      wbArray(VCLR, 'Vertex Colors',
        wbArray('Row',
          wbStruct('Column', [
            wbInteger('Red', itU8),
            wbInteger('Green', itU8),
            wbInteger('Blue', itU8)
          ]).SetToStr(wbRGBAToStr)
            .IncludeFlag(dfCollapsed, wbCollapseRGBA),
        65).SetSummaryName('Columns')
           .IncludeFlag(dfCollapsed, wbCollapseVertices),
      65).SetSummaryName('Rows')
         .IncludeFlag(dfCollapsed, wbCollapseVertices)),
    IfThen(wbSimpleRecords,
      wbByteArray(VTEX, 'Textures'),
      wbArray(VTEX, 'Textures',
        wbArray('Row',
          wbInteger('Column', itU16), //[LTEX]
        16).SetSummaryName('Columns')
           .IncludeFlag(dfCollapsed, wbCollapseOther),
      16).SetSummaryName('Rows')
         .IncludeFlag(dfCollapsed, wbCollapseOther))
  ]).SetFormIDBase($D0)
    .SetFormIDNameBase($B0)
    .SetGetFormIDCallback(function(const aMainRecord: IwbMainRecord; out aFormID: TwbFormID): Boolean begin
      var GridCell: TwbGridCell;
      Result := aMainRecord.GetGridCell(GridCell) and wbGridCellToFormID($C0, GridCell, aFormID);
    end)
    .SetIdentityCallback(function(const aMainRecord: IwbMainRecord): string begin
      Result := '';
      var GridCell: TwbGridCell;
      if aMainRecord.GetGridCell(GridCell) then
        Result := GridCell.SortKey
    end);
TES4:
  wbRecord(LAND, 'Landscape',
    wbFlags(wbFlagsList([
      18, 'Compressed'
    ])), [
    wbInteger(DATA, 'Flags', itU32,
      wbFlags(wbSparseFlags([
        0,  'Has Vertex Normals/Height Map',
        1,  'Has Vertex Colours',
        2,  'Has Layers',
        3,  'Unknown 4',
        4,  'Auto-Calc Normals',
        10, 'Ignored'
      ], False, 11))
    ).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbLandNormals,
    wbLandHeights,
    wbLandColors,
    wbLandLayers,
    wbArray(VTEX, 'Landscape Textures', wbFormIDCk('Texture', [LTEX, NULL]))
  ]).SetAddInfo(wbLANDAddInfo);

# LCRT.Location Reference Type

# LCTN.Location

# LEVC.Leveled Creature
TES3:
  wbRecord(LEVC, 'Leveled Creature',
    wbFlags(wbFlagsList([
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbInteger(DATA, 'Leveled Flags', itU32, wbLeveledFlags)
      .SetRequired
      .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbInteger(NNAM, 'Chance None', itU8).SetRequired,
    wbInteger(INDX, 'Entry Count', itU32).IncludeFlag(dfSkipImplicitEdit),
    wbRArrayS('Leveled Creature Entries',
      wbRStructSK([1], 'Leveled Creature Entry', [
        wbString(CNAM, 'Creature'), //[CREA]
        wbInteger(INTV, 'Level', itU16)
      ]).SetSummaryKey([1,0])
        .SetSummaryMemberPrefixSuffix(1, '[Level: ', ']')
        .SetSummaryMemberPrefixSuffix(0, '', ' x1')
        .IncludeFlag(dfSummaryMembersNoName)
        .IncludeFlag(dfSummaryNoSortKey)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    ).SetCountPath(INDX)
  ]).SetFormIDBase($40)
    .SetSummaryKey([5]);

# LEVI.Leveled Item
TES3:
  wbRecord(LEVI, 'Leveled Item',
    wbFlags(wbFlagsList([
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbInteger(DATA, 'Leveled Flags', itU32, wbLeveledFlags)
      .SetRequired
      .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbInteger(NNAM, 'Chance None', itU8).SetRequired,
    wbInteger(INDX, 'Entry Count', itU32).IncludeFlag(dfSkipImplicitEdit),
    wbRArrayS('Leveled Item Entries',
      wbRStructSK([1], 'Leveled Item Entry', [
        wbString(INAM, 'Item'), //[ALCH, APPA, ARMO, BOOK, CLOT, INGR, LEVI, LIGH, LOCK, MISC, PROB, REPA, WEAP]
        wbInteger(INTV, 'Player Level', itU16)
      ]).SetSummaryKey([1,0])
        .SetSummaryMemberPrefixSuffix(1, '[Level: ', ']')
        .SetSummaryMemberPrefixSuffix(0, '', ' x1')
        .IncludeFlag(dfSummaryMembersNoName)
        .IncludeFlag(dfSummaryNoSortKey)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    ).SetCountPath(INDX)
  ]).SetFormIDBase($40)
    .SetSummaryKey([5]);

# LGTM.Lighting Template

# LIGH.Light
TES3:
  wbRecord(LIGH, 'Light',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbIcon,
    wbStruct(LHDT, 'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2),
      wbInteger('Value', itU32),
      wbInteger('Time', itS32).SetDefaultNativeValue(-1),
      wbInteger('Radius', itU32).SetDefaultNativeValue(1000),
      wbByteColors,
      wbInteger('Flags', itU32,
        wbFlags([
        {0} 'Dynamic',
        {1} 'Can Carry',
        {2} 'Negative',
        {3} 'Flicker',
        {4} 'Fire',
        {5} 'Off By Default',
        {6} 'Flicker Slow',
        {7} 'Pulse',
        {8} 'Pulse Slow'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbString(SNAM, 'Looping Sound') //[SOUN]
  ]).SetFormIDBase($40);
TES4:
  wbRecord(LIGH, 'Light',
    wbFlags(wbFlagsList([
      10, 'Quest'
    ])), [
    wbEDID,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbSCRI,
    wbFULL,
    wbICON,
    wbStruct(DATA, 'Data', [
      wbInteger('Time', itS32).SetDefaultNativeValue(-1),
      wbInteger('Radius', itU32).SetDefaultNativeValue(16),
      wbByteColors('Color'),
      wbInteger('Flags', itU32,
        wbFlags([
          {0}  'Dynamic',
          {1}  'Can be Carried',
          {2}  'Negative',
          {3}  'Flicker',
          {4}  'Unused',
          {5}  'Off By Default',
          {6}  'Flicker Slow',
          {7}  'Pulse',
          {8}  'Pulse Slow',
          {9}  'Spot Light',
          {10} 'Spot Shadow'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat('Falloff Exponent').SetDefaultNativeValue(0.001),
      wbFloat('FOV').SetDefaultNativeValue(90),
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ], cpNormal, True, nil, 6),
    wbFloat(FNAM, 'Fade value')
      .SetDefaultNativeValue(1.0)
      .SetRequired,
    wbFormIDCk(SNAM, 'Sound', [SOUN])
  ]);

# LOCK.Lockpick
TES3:
  wbRecord(LOCK, 'Lockpick',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(LKDT, 'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      wbFloat('Quality', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Uses', itU32).SetDefaultNativeValue(10)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon
  ]).SetFormIDBase($40);

# LSCR.Load Screen
TES4:
  wbRecord(LSCR, 'Load Screen', [
    wbEDID,
    wbICON,
    wbDESC.SetRequired,
    wbRArrayS('Locations',
      wbStructSK(LNAM, [0, 1], 'Location', [
        wbFormIDCkNoReach('Direct', [CELL, WRLD, NULL]),
        wbStructSK([0, 1], 'Indirect', [
          wbFormIDCkNoReach('World', [WRLD, NULL]),
          wbStructSK([0,1], 'Grid', [
            wbInteger('Y', itS16),
            wbInteger('X', itS16)
          ])
        ])
      ]))
  ]).SetSummaryKey([2]);

# LTEX.Landscape Texture
TES3:
  wbRecord(LTEX, 'Landscape Texture', [
    wbDeleted,
    wbEditorID,
    wbInteger(INTV, 'Texture ID', itU32).SetRequired,
    wbString(DATA, 'Texture Filename').SetRequired
  ]).SetFormIDBase($60)
    .SetSummaryKey([3]);
TES4:
  wbRecord(LTEX, 'Landscape Texture', [
    wbEDID.SetRequired,
    wbICON,
    wbStruct(HNAM, 'Havok Data', [
      wbInteger('Material Type', itU8,
        wbEnum([
          {0}  'Stone',
          {1}  'Cloth',
          {2}  'Dirt',
          {3}  'Glass',
          {4}  'Grass',
          {5}  'Metal',
          {6}  'Organic',
          {7}  'Skin',
          {8}  'Water',
          {9}  'Wood',
          {10} 'Heavy Stone',
          {11} 'Heavy Metal',
          {12} 'Heavy Wood',
          {13} 'Chain',
          {14} 'Snow',
          {15} 'Stone Stairs',
          {16} 'Cloth Stairs',
          {17} 'Dirt Stairs',
          {18} 'Glass Stairs',
          {19} 'Grass Stairs',
          {20} 'Metal Stairs',
          {21} 'Organic Stairs',
          {22} 'Skin Stairs',
          {23} 'Water Stairs',
          {24} 'Wood Stairs',
          {25} 'Heavy Stone Stairs',
          {26} 'Heavy Metal Stairs',
          {27} 'Heavy Wood Stairs',
          {28} 'Chain Stairs',
          {29} 'Snow Stairs',
          {30} 'Elevator'
        ])).SetDefaultNativeValue(2),
      wbInteger('Friction', itU8).SetDefaultNativeValue(30),
      wbInteger('Restitution', itU8).SetDefaultNativeValue(30)
    ]).SetRequired,
    wbInteger(SNAM, 'Texture Specular Exponent', itU8)
      .SetDefaultNativeValue(30)
      .SetRequired,
    wbRArrayS('Grasses', wbFormIDCk(GNAM, 'Grass', [GRAS]))
  ]).SetSummaryKey([1]);

# LVLC.Leveled Creature
TES4:
  wbRecord(LVLC, 'Leveled Creature', [
    wbEDID,
    wbInteger(LVLD, 'Chance none', itU8).SetRequired,
    wbInteger(LVLF, 'Flags', itU8,
      wbFlags([
        {0} 'Calculate from all levels <= player''s level',
        {1} 'Calculate for each item in count'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Leveled List Entries',
      wbLeveledListEntry('Creature', [CREA, LVLC, NPC_])
    ),
    wbSCRI,
    wbFormIDCk(TNAM, 'Creature template', [CREA, NPC_])
  ], True).SetSummaryKey([3])
          .SetAfterLoad(wbLVLAfterLoad);

# LVLI.Leveled Item
TES4:
  wbRecord(LVLI, 'Leveled Item', [
    wbEDID,
    wbInteger(LVLD, 'Chance none', itU8).SetRequired,
    wbInteger(LVLF, 'Flags', itU8,
      wbFlags([
        {0} 'Calculate from all levels <= player''s level',
        {1} 'Calculate for each item in count'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Leveled List Entries',
      wbLeveledListEntry('Item', [ALCH, AMMO, APPA, ARMO, BOOK, CLOT, INGR, KEYM, LIGH, LVLI, MISC, SGST, SLGM, WEAP])
    ),
    wbUnused(DATA, 1)
  ]).SetSummaryKey([3])
    .SetAfterLoad(wbLVLAfterLoad);

# LVLN.Leveled Actor

# LVSP.Leveled Spell
TES4:
  wbRecord(LVSP, 'Leveled Spell', [
    wbEDID,
    wbInteger(LVLD, 'Chance none', itU8).SetRequired,
    wbInteger(LVLF, 'Flags', itU8,
      wbFlags([
        {0} 'Calculate from all levels <= player''s level',
        {1} 'Calculate for each item in count',
        {2} 'Use all spells'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Leveled List Entries',
      wbLeveledListEntry('Spell', [LVSP, SPEL])
    )
  ]).SetSummaryKey([3])
    .SetAfterLoad(wbLVLAfterLoad);

# MATO.Material Object

# MATT.Material Type

# MESG.Messag

# MGEF.Magic Effect
TES3:
  wbRecord(MGEF, 'Magic Effect', @wbKnownSubRecordSignaturesINDX, [
    wbInteger(INDX, 'Effect', itU32, wbMagicEffectEnum),
    wbDeleted,
    wbStruct(MEDT, 'Data', [
      wbInteger('School', itU32,
        wbEnum([
        {0} 'Alteration',
        {1} 'Conjuration',
        {2} 'Destruction',
        {3} 'Illusion',
        {4} 'Mysticism',
        {5} 'Restoration'
        ])),
      wbFloat('Base Cost', cpNormal, False, 1, 2),
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
        9,  'Spellmaking',
        10, 'Enchanting',
        11, 'Negative'
        ], False, 12))).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbStruct('Lighting Effect', [
        wbInteger('Red', itU32),
        wbInteger('Green', itU32),
        wbInteger('Blue', itU32)
      ]).SetToStr(wbRGBAToStr)
        .IncludeFlag(dfCollapsed, wbCollapseRGBA),
      wbFloat('Size Multiplier', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbFloat('Speed Multiplier', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbFloat('Size Cap', cpNormal, False, 1, 2)
    ]).SetRequired,
    wbString(ITEX, 'Effect Texture Filename'),
    wbString(PTEX, 'Particle Texture Filename'),
    wbString(BSND, 'Bolt Sound'), //[SOUN]
    wbString(CSND, 'Cast Sound'), //[SOUN]
    wbString(HSND, 'Hit Sound'), //[SOUN]
    wbString(ASND, 'Area Sound'), //[SOUN]
    wbString(CVFX, 'Casting Visual'), //[STAT]
    wbString(BVFX, 'Bolt Visual'), //[WEAP]
    wbString(HVFX, 'Hit Visual'), //[STAT]
    wbString(AVFX, 'Area Visual'), //[STAT]
    wbDescription
  ]).SetFormIDBase($02);
TES4:
  wbRecord(MGEF, 'Magic Effect', [
    wbStringMgefCode(EDID, 'Magic Effect Code', 4).SetRequired,
    {wbStruct(OBME, 'Oblivion Magic Extender', [
      wbInteger('Record Version', itU8),
      wbOBMEVersion,
      wbInteger('Param A Info', itU8, wbOBMEResolutionEnum),
      wbInteger('Param B Info', itU8, wbOBMEResolutionEnum),
      wbUnused(2),
      wbString('Handler', 4),
      wbInteger('Flag Overrides', itU32,
        wbFlags(wbSparseFlags([
          2,  'ParamFlagA',
          3,  'Beneficial',
          16, 'ParamFlagB',
          17, 'Magnitude Is Range',
          18, 'Atomic Resistance',
          19, 'ParamFlagC',
          20, 'ParamFlagD',
          30, 'Hidden'
        ], False, 31))).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbByteArray('Param B', 4), //Needs a union based on Handler.
      wbUnused($1C)
    ]).SetDontShow(wbOBMEDontShow),
    wbString(EDDX, 'EditorID').SetDontShow(wbEDDXDontShow),}
    wbFULL.SetRequired,
    wbDESC.SetRequired,
    wbICON,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Hostile',
          1,  'Recover',
          2,  'Detrimental',
          3,  'Magnitude %',
          4,  'Self',
          5,  'Touch',
          6,  'Target',
          7,  'No duration',
          8,  'No magnitude',
          9,  'No area',
          10, 'FX persist',
          11, 'Spellmaking',
          12, 'Enchanting',
          13, 'No Ingredient',
          16, 'Use weapon',
          17, 'Use armor',
          18, 'Use creature',
          19, 'Use skill',
          20, 'Use attribute',
          24, 'Use actor value',
          25, 'Spray projectile type (or Fog if Bolt is specified as well)',
          26, 'Bolt projectile type',
          27, 'No hit effect'
        ], False, 28), True)
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat('Base cost'),
      wbUnion('Assoc. Item', wbMGEFFAssocItemDecider, [
        //wbByteArray('Param A', 4).SetDontShow(wbOBMEDontShow), //Needs a union based on Handler.
        wbByteArray('Unknown', 4),
        wbFormIDCk('Assoc. Weapon', [WEAP]),
        wbFormIDCk('Assoc. Armor', [ARMO, NULL{?}]),
        wbFormIDCk('Assoc. Creature', [CREA, LVLC, NPC_]),
        wbInteger('Assoc. Actor Value', itS32, wbActorValueEnum)
      ]),
      wbInteger('Magic School', itU32, wbMagicSchoolEnum),
      wbInteger('Resist value', itS32,
        wbEnum([], [
          -1, 'None',
          61, 'Resist Fire',
          62, 'Resist Frost',
          63, 'Resist Disease',
          64, 'Resist Magic',
          65, 'Resist Normal Weapons',
          66, 'Resist Paralysis',
          67, 'Resist Poison',
          68, 'Resist Shock'{,
         255, 'None (OBME)'}
        ])),
      wbInteger('Counter Effect Count', itU16), //!!! must be updated automatically when ESCE length changes!
      wbUnused(2),
      wbFormIDCk('Light', [LIGH, NULL]),
      wbFloat('Projectile speed'),
      wbFormIDCk('Effect Shader', [EFSH, NULL]),
      wbFormIDCk('Enchant effect', [EFSH, NULL]),
      wbFormIDCk('Casting sound', [SOUN, NULL]),
      wbFormIDCk('Bolt sound', [SOUN, NULL]),
      wbFormIDCk('Hit sound', [SOUN, NULL]),
      wbFormIDCk('Area sound', [SOUN, NULL]),
      wbFloat('Constant Effect enchantment factor'),
      wbFloat('Constant Effect barter factor')
    ], cpNormal, True, nil, 10).SetRequired,
    wbArrayS(ESCE, 'Counter Effects', wbInteger('Counter Effect Code', itU32, wbChar4))
      .SetCountPathOnValue('DATA\Counter Effect Count', False)
  ]).SetAfterLoad(wbMGEFAfterLoad)
    .IncludeFlag(dfIndexEditorID);

# MICN.Menu Icon

# MISC.Misc Item
TES3:
  wbRecord(MISC, 'Misc. Item',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(MCDT,'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      //This bool is only set true if the object is used in a KNAM on a REFR.
      wbInteger('Is Key', itU32, wbBoolEnum)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon
  ]).SetFormIDBase($40);
TES4:
  wbRecord(MISC, 'Misc. Item',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbStruct(DATA, '', [
      wbUnion('', wbMISCActorValueDecider, [
        wbInteger('Value', itS32),
        wbFormIDCk('Actor Value', [ACVA])
      ]),
      wbUnion('', wbMISCActorValueDecider, [
        wbFloat('Weight'),
        wbInteger('Group', itU32,
          wbEnum([], [
            $40E00000, ' [NONE]',
            $40400000, 'AI',
            $00000000, 'Attribute',
            $40C00000, 'Combat',
            $40A00000, 'Misc',
            $40000000, 'Skill',
            $40800000, 'Social',
            $3F800000, 'Stat'
          ]))
      ])
    ]).SetRequired
  ]);

# MOVT.Movement Type

# MSTT.Movable Static

# MUSC.Music Type

# MUST.Music Track

# NAVI.Navigation

# NAVM.NavMesh

# NOTE.Note

# NPC_.Non-Player Character
TES3:
  wbRecord(NPC_, 'Non-Player Character',
    wbFlags(wbFlagsList([
      10, 'Corpses Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel,
    wbFullName,
    wbString(RNAM, 'Race') //[RACE]
      .SetDefaultEditValue('Argonian')
      .SetRequired,
    wbString(CNAM, 'Class') //[CLAS]
      .SetDefaultEditValue('Acrobat')
      .SetRequired,
    wbString(ANAM, 'Faction').SetRequired, //[FACT]
    wbString(BNAM, 'Head Body Part') //[BODY]
      .SetDefaultEditValue('b_n_argonian_m_head_02')
      .SetRequired,
    wbString(KNAM, 'Hair Body Part') //[BODY]
      .SetDefaultEditValue('b_n_argonian_m_hair01')
      .SetRequired,
    wbScript, //[SCPT]
    wbUnion(NPDT, 'Data', wbNPCDataDecider, [
        wbStruct('Non-Auto', [
          wbInteger('Level', itU16),
          wbStruct('Attributes', [
            wbInteger('Strength', itU8).SetDefaultNativeValue(50),
            wbInteger('Intelligence', itU8).SetDefaultNativeValue(50),
            wbInteger('Willpower', itU8).SetDefaultNativeValue(50),
            wbInteger('Agility', itU8).SetDefaultNativeValue(50),
            wbInteger('Speed', itU8).SetDefaultNativeValue(50),
            wbInteger('Endurance', itU8).SetDefaultNativeValue(50),
            wbInteger('Personality', itU8).SetDefaultNativeValue(50),
            wbInteger('Luck', itU8).SetDefaultNativeValue(50)
          ]),
          wbStruct('Skills', [
            wbInteger('Block', itU8).SetDefaultNativeValue(5),
            wbInteger('Armorer', itU8).SetDefaultNativeValue(5),
            wbInteger('Medium Armor', itU8).SetDefaultNativeValue(5),
            wbInteger('Heavy Armor', itU8).SetDefaultNativeValue(5),
            wbInteger('Blunt Weapon', itU8).SetDefaultNativeValue(5),
            wbInteger('Long Blade', itU8).SetDefaultNativeValue(5),
            wbInteger('Axe', itU8).SetDefaultNativeValue(5),
            wbInteger('Spear', itU8).SetDefaultNativeValue(5),
            wbInteger('Athletics', itU8).SetDefaultNativeValue(5),
            wbInteger('Enchant', itU8).SetDefaultNativeValue(5),
            wbInteger('Destruction', itU8).SetDefaultNativeValue(5),
            wbInteger('Alteration', itU8).SetDefaultNativeValue(5),
            wbInteger('Illusion', itU8).SetDefaultNativeValue(5),
            wbInteger('Conjuration', itU8).SetDefaultNativeValue(5),
            wbInteger('Mysticism', itU8).SetDefaultNativeValue(5),
            wbInteger('Restoration', itU8).SetDefaultNativeValue(5),
            wbInteger('Alchemy', itU8).SetDefaultNativeValue(5),
            wbInteger('Unarmored', itU8).SetDefaultNativeValue(5),
            wbInteger('Security', itU8).SetDefaultNativeValue(5),
            wbInteger('Sneak', itU8).SetDefaultNativeValue(5),
            wbInteger('Acrobatics', itU8).SetDefaultNativeValue(5),
            wbInteger('Light Armor', itU8).SetDefaultNativeValue(5),
            wbInteger('Short Blade', itU8).SetDefaultNativeValue(5),
            wbInteger('Marksman', itU8).SetDefaultNativeValue(5),
            wbInteger('Speechcraft', itU8).SetDefaultNativeValue(5),
            wbInteger('Mercantile', itU8).SetDefaultNativeValue(5),
            wbInteger('Hand-to-Hand', itU8).SetDefaultNativeValue(5)
          ]),
          wbUnused(1),
          wbInteger('Health', itU16).SetDefaultNativeValue(50),
          wbInteger('Magicka', itU16).SetDefaultNativeValue(100),
          wbInteger('Fatigue', itU16).SetDefaultNativeValue(200),
          wbInteger('Disposition', itU8).SetDefaultNativeValue(50),
          wbInteger('Reputation', itU8),
          wbInteger('Rank', itU8),
          wbUnused(1),
          wbInteger('Gold', itU32)
        ]),
        wbStruct('Auto', [
          wbInteger('Level', itU16).SetDefaultNativeValue(1),
          wbInteger('Disposition', itU8).SetDefaultNativeValue(50),
          wbInteger('Reputation', itU8),
          wbInteger('Rank', itU8),
          wbUnused(3),
          wbInteger('Gold', itU32)
        ])
    ]).SetRequired,
    wbInteger(FLAG, 'Flags', itU32,
      wbFlags(wbSparseFlags([
      0, 'Female',
      1, 'Essential',
      2, 'Respawn',
      3, 'Can Hold Items',
      4, 'Auto Calculate Stats',
      10, 'Skeleton Blood',
      11, 'Metal Blood'
      ], False, 12))
    ).SetDefaultNativeValue(18)
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbInventory,
    wbSpells,
    wbAIData.SetRequired,
    wbTravelServices,
    wbPackages.SetRequired,
    wbFloat(XSCL, 'Scale', cpNormal, False, 1, 2).SetDefaultNativeValue(1)
  ]).SetFormIDBase($40);
TES4:
  wbRecord(NPC_, 'Non-Player Character',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      18, 'Compressed',
      19, 'Starts Dead'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []).SetRequired,
    wbStruct(ACBS, 'Configuration', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0, 'Female',
          1, 'Essential',
          3, 'Respawn',
          4, 'Auto-calc stats',
          7, 'PC Level Offset',
          9, 'No Low Level Processing',
          13, 'No Rumors',
          14, 'Summonable',
          15, 'No Persuasion',
          20, 'Can Corpse Check'
      ], False, 21))).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Base spell points', itU16).SetDefaultNativeValue(50),
      wbInteger('Fatigue', itU16).SetDefaultNativeValue(50),
      wbInteger('Barter gold', itU16),
      wbInteger('Level (offset)', itS16).SetDefaultNativeValue(1),
      wbInteger('Calc min', itU16),
      wbInteger('Calc max', itU16)
    ]).SetRequired,
    wbRArrayS('Factions', wbFaction),
    wbFormIDCk(INAM, 'Death item', [LVLI]),
    wbFormIDCk(RNAM, 'Race', [RACE]).SetDefaultNativeValue($19)
      .SetRequired,
    wbSPLOs,
    wbSCRI,
    wbCNTOs,
    wbStruct(AIDT, 'AI Data', [
      wbInteger('Aggression', itU8).SetDefaultNativeValue(5),
      wbInteger('Confidence', itU8).SetDefaultNativeValue(50),
      wbInteger('Energy Level', itU8).SetDefaultNativeValue(50),
      wbInteger('Responsibility', itU8).SetDefaultNativeValue(50),
      wbInteger('Buys/Sells and Services', itU32, wbServiceFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Teaches', itS8, wbSkillEnum),
      wbInteger('Maximum training level', itU8),
      wbUnused(2)
    ]).SetRequired,
    wbRArray('AI Packages', wbFormIDCk(PKID, 'AI Package', [PACK])),
    wbArrayS(KFFZ, 'Animations', wbString('Animation')),
    wbFormIDCk(CNAM, 'Class', [CLAS])
      .SetDefaultNativeValue($000230E6)
      .SetRequired,
    wbStruct(DATA, 'Stats', [
      wbInteger('Armorer', itU8).SetDefaultNativeValue(5),
      wbInteger('Athletics', itU8).SetDefaultNativeValue(5),
      wbInteger('Blade', itU8).SetDefaultNativeValue(5),
      wbInteger('Block', itU8).SetDefaultNativeValue(5),
      wbInteger('Blunt', itU8).SetDefaultNativeValue(5),
      wbInteger('Hand to Hand', itU8).SetDefaultNativeValue(5),
      wbInteger('Heavy Armor', itU8).SetDefaultNativeValue(5),
      wbInteger('Alchemy', itU8).SetDefaultNativeValue(5),
      wbInteger('Alteration', itU8).SetDefaultNativeValue(5),
      wbInteger('Conjuration', itU8).SetDefaultNativeValue(5),
      wbInteger('Destruction', itU8).SetDefaultNativeValue(5),
      wbInteger('Illusion', itU8).SetDefaultNativeValue(5),
      wbInteger('Mysticism', itU8).SetDefaultNativeValue(5),
      wbInteger('Restoration', itU8).SetDefaultNativeValue(5),
      wbInteger('Acrobatics', itU8).SetDefaultNativeValue(5),
      wbInteger('Light Armor', itU8).SetDefaultNativeValue(5),
      wbInteger('Marksman', itU8).SetDefaultNativeValue(5),
      wbInteger('Mercantile', itU8).SetDefaultNativeValue(5),
      wbInteger('Security', itU8).SetDefaultNativeValue(5),
      wbInteger('Sneak', itU8).SetDefaultNativeValue(5),
      wbInteger('Speechcraft', itU8).SetDefaultNativeValue(5),
      wbInteger('Health', itU16).SetDefaultNativeValue(50),
      wbUnused(2),
      wbInteger('Strength', itU8).SetDefaultNativeValue(50),
      wbInteger('Intelligence', itU8).SetDefaultNativeValue(50),
      wbInteger('Willpower', itU8).SetDefaultNativeValue(50),
      wbInteger('Agility', itU8).SetDefaultNativeValue(50),
      wbInteger('Speed', itU8).SetDefaultNativeValue(50),
      wbInteger('Endurance', itU8).SetDefaultNativeValue(50),
      wbInteger('Personality', itU8).SetDefaultNativeValue(50),
      wbInteger('Luck', itU8).SetDefaultNativeValue(50)
    ]).SetRequired,
    wbFormIDCk(HNAM, 'Hair', [HAIR]),
    wbFloat(LNAM, 'Hair length'),
    wbArray(ENAM, 'Eyes', wbFormIDCk('Eyes', [EYES])),
    wbByteColors(HCLR, 'Hair color').SetRequired,
    wbFormIDCk(ZNAM, 'Combat Style', [CSTY]),
    wbFaceGen,
    wbByteArray(FNAM, 'Unknown', 2, cpBenign).SetRequired
  ], True);

# OTFT.Outfit

# PACK.AI Package
TES4:
  wbRecord(PACK, 'Package',
    wbFlags(wbFlagsList([
      14, 'Unknown 14',
      15, 'Unknown 15'
    ])), [
    wbEDID,
    wbUnion(PKDT, 'General', wbPACKPKDTDecider, [
      wbStruct('', [
        wbInteger('Flags', itU16, wbPackageFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type', itU8, wbPackageTypeEnum),
        wbUnused(1)
      ]).SetSummaryKey([1])
        .IncludeFlag(dfSummaryMembersNoName),
      wbStruct('', [
        wbInteger('Flags', itU32, wbPackageFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type', itU8, wbPackageTypeEnum),
        wbUnused(3)
      ]).SetSummaryKey([1])
        .IncludeFlag(dfSummaryMembersNoName)
    ]).SetRequired
      .IncludeFlag(dfSummaryMembersNoName),
    wbStruct(PLDT, 'Location', [
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'Near Reference',
          {1} 'In Cell',
          {2} 'Near Current Location',
          {3} 'Near Editor Location',
          {4} 'Object ID',
          {5} 'Object Type'
        ])),
      wbUnion('Location', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [ACHR, ACRE, PLYR, REFR], True),
        wbFormIDCkNoReach('Cell', [CELL]),
        wbFormIDCk('Unused', [NULL]),
        wbFormIDCk('Unused', [NULL]),
        wbFormIDCkNoReach('Object ID', [ACTI, ALCH, AMMO, APPA, ARMO, BOOK, CLOT, CONT, CREA, DOOR, FLOR, FURN, INGR, KEYM, LIGH, MISC, NPC_, SGST, SLGM, SPEL, STAT, WEAP]),
        wbInteger('Object type', itU32)
      ]),
      wbInteger('Radius', itS32)
    ]).SetSummaryKeyOnValue([0, 1])
      .IncludeFlagOnValue(dfSummaryMembersNoName)
      .IncludeFlag(dfSummaryMembersNoName),
    wbStruct(PSDT, 'Schedule', [
      wbInteger('Month', itU8,
        wbEnum([
          {0}  'January',
          {1}  'February',
          {2}  'March',
          {3}  'April',
          {4}  'May',
          {5}  'June',
          {6}  'July',
          {7}  'August',
          {8}  'September',
          {9}  'October',
          {10} 'November',
          {11} 'December',
          {12} 'Spring (MAM)',
          {13} 'Summer (JJA)',
          {14} 'Autumn (SON)',
          {15} 'Winter (DJF)'
        ], [
          255, 'Any'
        ])).SetDefaultNativeValue(255),
      wbInteger('Day of week', itU8,
        wbEnum([
          {0}  'Sunday',
          {1}  'Monday',
          {2}  'Tuesday',
          {3}  'Wednesday',
          {4}  'Thursday',
          {5}  'Friday',
          {6}  'Saturday',
          {7}  'Weekdays',
          {8}  'Weekends',
          {9}  'Monday, Wednesday, Friday',
          {10} 'Tuesday, Thursday'
        ], [
          255, 'Any'
        ])).SetDefaultNativeValue(255),
      wbInteger('Date', itU8),
      wbInteger('Time', itU8),
      wbInteger('Duration', itU32)
    ]).SetRequired,
    wbStruct(PTDT, 'Target', [
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'Specific Reference',
          {1} 'Object ID',
          {2} 'Object Type'
        ])),
      wbUnion('Target', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [ACHR, ACRE, REFR, PLYR], True),
        wbFormIDCkNoReach('Object ID', [ACTI, ALCH, AMMO, APPA, ARMO, BOOK, CLOT, CONT, CREA, DOOR, FLOR, FURN, GRAS, INGR, KEYM, LIGH, LVLC, LVLI, LVSP, MISC, NPC_, SBSP, SGST, SLGM, SPEL, STAT, TREE, WEAP]),
        wbInteger('Object type', itU32,
          wbEnum([
            {0}  'None',
            {1}  'Activators',
            {2}  'Apparatus',
            {3}  'Armor',
            {4}  'Books',
            {5}  'Clothing',
            {6}  'Containers',
            {7}  'Doors',
            {8}  'Ingredients',
            {9}  'Lights',
            {10} 'Miscellaneous',
            {11} 'Flora',
            {12} 'Furniture',
            {13} 'Weapons: All',
            {14} 'Ammo',
            {15} 'NPCs',
            {16} 'Creatures',
            {17} 'Soul Gems',
            {18} 'Keys',
            {19} 'Alchemy',
            {20} 'Food',
            {21} 'All: Combat Wearable',
            {22} 'All: Wearable',
            {23} 'Weapons: None',
            {24} 'Weapons: Melee',
            {25} 'Weapons: Ranged',
            {26} 'Spells: Any',
            {27} 'Spells: Range Target',
            {28} 'Spells: Range Touch',
            {29} 'Spells: Range Self',
            {30} 'Spells: School Alteration',
            {31} 'Spells: School Conjuration',
            {32} 'Spells: School Destruction',
            {33} 'Spells: School Illusion',
            {34} 'Spells: School Mysticism',
            {35} 'Spells: School Restoration'
          ]))
      ]),
      wbInteger('Count', itS32)
    ]).SetSummaryKeyOnValue([0, 1])
      .IncludeFlagOnValue(dfSummaryMembersNoName)
      .IncludeFlag(dfSummaryMembersNoName),
    wbConditions
  ]).SetSummaryKey([1, 2, 4, 5])
    .SetSummaryMemberPrefixSuffix(5, 'if ', '')
    .IncludeFlag(dfSummaryMembersNoName);

# PERK.Perk

# PGRD.Path Grid
TES3:
  wbRecord(PGRD, 'Path Grid', [
    wbStruct(DATA, 'Data', [
      wbStruct('Grid', [
        wbInteger('X', itS32),
        wbInteger('Y', itS32)
      ], cpCritical).SetSummaryKey([0,1])
                    .SetSummaryMemberPrefixSuffix(0, '(', '')
                    .SetSummaryMemberPrefixSuffix(1, '', ')')
                    .SetSummaryDelimiter(', '),
      wbInteger('Granularity', itU16).SetDefaultNativeValue(1024),
      wbInteger('Point Count', itU16)
    ]).SetRequired,
    wbString(NAME, 'Location ID', 0, cpIgnore).SetRequired,
    IfThen(wbSimpleRecords,
      wbArray(PGRP, 'Points',
        wbByteArray('Point', 16)
      ).SetCountPathOnValue('DATA\Point Count', False),
      wbArray(PGRP, 'Points',
        wbStruct('Point', [
          wbStruct('Position', [
            wbInteger('X', itS32),
            wbInteger('Y', itS32),
            wbInteger('Z', itS32)
          ]).IncludeFlag(dfCollapsed, wbCollapseVec3),
          wbInteger('User Created', itU8, wbBoolEnum),
          wbInteger('Connection Count', itU8),
          wbUnused(2)
        ]).SetSummaryKey([0,2])
          .SetSummaryMemberPrefixSuffix(0, '', '')
          .SetSummaryMemberPrefixSuffix(2, 'Connections: ', '')
          .IncludeFlag(dfCollapsed, wbCollapseNavmesh)
      ).SetCountPathOnValue('DATA\Point Count', False)),
    IfThen(wbSimpleRecords,
      wbByteArray(PGRC, 'Point Connections'),
      wbArray(PGRC, 'Point Connections',
        wbArrayS('Point Connection',
          wbInteger('Point', itU32),
        wbCalcPGRCSize)).IncludeFlag(dfCollapsed, wbCollapseNavmesh))
  ]).SetFormIDBase($F0)
    .SetFormIDNameBase($B0).SetGetGridCellCallback(function(const aSubRecord: IwbSubRecord; out aGridCell: TwbGridCell): Boolean begin
      with aGridCell, aSubRecord do begin
        X := ElementNativeValues['Grid\X'];
        Y := ElementNativeValues['Grid\Y'];
        Result := not ((X = 0) and (Y = 0));
      end;
    end)
    .SetGetFormIDCallback(function(const aMainRecord: IwbMainRecord; out aFormID: TwbFormID): Boolean begin
      var GridCell: TwbGridCell;
      Result := aMainRecord.GetGridCell(GridCell) and wbGridCellToFormID($E0, GridCell, aFormID);
    end)
    .SetIdentityCallback(function(const aMainRecord: IwbMainRecord): string begin
      var GridCell: TwbGridCell;
      if aMainRecord.GetGridCell(GridCell) then
        Result := '<Exterior>' + GridCell.SortKey
      else
        Result := aMainRecord.EditorID;
    end);
TES4:
  wbRecord(PGRD, 'Path Grid',
    wbFlags(wbFlagsList([
      18, 'Compressed'
    ])), [
    wbInteger(DATA, 'Point Count', itU16).SetRequired,
    wbPGRP,
    wbArray(PGAG, 'Auto-Generated Point Sets',
      wbInteger('Set', itU8, wbPGAGFlags, cpIgnore).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ),
    wbArray(PGRR, 'Point-to-Point Connections',
      wbArrayS('Point', wbInteger('Point', itS16), wbCalcPGRRSize)),
    wbArrayS(PGRI, 'Inter-Cell Connections',
      wbStructSK([0,2,3,4], 'Inter-Cell Connection', [
        wbInteger('Point', itU16),
        wbUnused(2),
        wbFloat('X'),
        wbFloat('Y'),
        wbFloat('Z')
      ])).SetAfterLoad(wbPGRIPointerAfterLoad),
    wbRArrayS('Point-to-Reference Mappings',
      wbStructSK(PGRL, [0], 'Point-to-Reference Mapping', [
        wbFormIDCk('Reference', [REFR]),
        wbArrayS('Points', wbInteger('Point', itU32))
      ]))
  ]).SetAddInfo(wbPGRDAddInfo)
    .SetAfterLoad(wbPGRDAfterLoad);

# PGRE.Placed Grenade

# PHZD.Placed Hazard

# PLYR.Player Reference #NEW
TES4:
  wbRecord(PLYR, 'Player Reference', [
    wbEDID,
    wbFormID(PLYR, 'Player')
      .SetDefaultNativeValue($7)
      .SetRequired
  ]).IncludeFlag(dfInternalEditOnly);

# PROB.Probe
TES3:
  wbRecord(PROB, 'Probe',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(PBDT, 'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      wbFloat('Quality', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Uses', itU32).SetDefaultNativeValue(10)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon
  ]).SetFormIDBase($40);

# PROJ.Projectile

# QUST.Quest
TES4:
  wbRecord(QUST, 'Quest', [
    wbEDID.SetRequired,
    wbSCRI,
    wbFULL,
    wbICON,
    wbStruct(DATA, 'General', [
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'Start game enabled',
          2, 'Allow repeated conversation topics',
          3, 'Allow repeated stages'
        ], False, 4))
      ).SetDefaultNativeValue(1).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Priority', itU8)
    ]).SetRequired,
    wbConditions,
    wbRArrayS('Stages',
      wbRStructSK([0], 'Stage', [
        wbInteger(INDX, 'Stage index', itS16),
        wbRArray('Log Entries',
          wbRStruct('Log Entry', [
            wbInteger(QSDT, 'Complete Quest', itU8, wbBoolEnum),
            wbConditions,
            wbStringKC(CNAM, 'Log Entry', 0, cpTranslate),
            wbResultScript
          ]).SetSummaryKey([2, 1]))
      ]).SetSummaryKey([1])),
    wbRArray('Targets',
      wbRStruct('Target', [
        wbStructSK(QSTA, [0], 'Target', [
          wbFormIDCkNoReach('Target', [ACHR, ACRE, REFR], True),
          wbInteger('Compass Marker Ignores Locks', itU8, wbBoolEnum),
          wbUnused(3)
        ]),
        wbConditions
      ]).SetSummaryKey([0, 1]))
  ]);

# RACE.Race
TES3:
  wbRecord(RACE, 'Race', [
    wbEditorID,
    wbDeleted,
    wbFullName.SetRequired,
    wbStruct(RADT, 'Data', [
      wbArrayS('Skill Bonuses',
        wbStructSK([0], 'Skill Bonus', [
          wbInteger('Skill', itS32, wbSkillEnum).SetDefaultNativeValue(-1),
          wbInteger('Bonus', itU32)
        ]).SetSummaryKey([1,0])
          .SetSummaryMemberPrefixSuffix(1, '+', '')
          .SetSummaryMemberPrefixSuffix(0, '', '')
          .IncludeFlag(dfSummaryMembersNoName)
          .IncludeFlag(dfSummaryNoSortKey)
          .IncludeFlag(dfCollapsed, wbCollapseOther),
      7),
      wbStruct('Base Attributes', [
        wbStruct('Strength', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ]),
        wbStruct('Intelligence', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ]),
        wbStruct('Willpower', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ]),
        wbStruct('Agility', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ]),
        wbStruct('Speed', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ]),
        wbStruct('Endurance', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ]),
        wbStruct('Personality', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ]),
        wbStruct('Luck', [
          wbInteger('Male', itU32).SetDefaultNativeValue(50),
          wbInteger('Female', itU32).SetDefaultNativeValue(50)
        ])
      ]),
      wbStruct('Height', [
        wbFloat('Male', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
        wbFloat('Female', cpNormal, False, 1, 2).SetDefaultNativeValue(1)
      ]),
      wbStruct('Weight', [
        wbFloat('Male', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
        wbFloat('Female', cpNormal, False, 1, 2).SetDefaultNativeValue(1)
      ]),
      wbInteger('Flags', itU32,
        wbFlags([
        {0} 'Playable',
        {1} 'Beast Race'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbSpells,
    wbDescription
  ]).SetFormIDBase($14);
TES4:
  wbRecord(RACE, 'Race', [
    wbEDID.SetRequired,
    wbFULL,
    wbDESC.SetRequired,
    wbSPLOs,
    wbFactionRelations,
    wbStruct(DATA, '', [
      wbArrayS('Skill Boosts',
        wbStructSK([0], 'Skill Boost', [
          wbInteger('Skill', itS8, wbMajorSkillEnum).SetDefaultNativeValue(255),
          wbInteger('Boost', itS8)
        ]).SetSummaryKey([1, 0])
          .SetSummaryMemberPrefixSuffix(1, '+', '')
          .SetSummaryMemberPrefixSuffix(0, '', '')
          .SetSummaryDelimiter(' ')
          .IncludeFlag(dfSummaryNoSortKey)
          .IncludeFlag(dfCollapsed, wbCollapseOther),
      7),
      wbUnused(2),
      wbFloat('Male Height').SetDefaultNativeValue(1),
      wbFloat('Female Height').SetDefaultNativeValue(1),
      wbFloat('Male Weight').SetDefaultNativeValue(1),
      wbFloat('Female Weight').SetDefaultNativeValue(1),
      wbInteger('Playable', itU32, wbBoolEnum)
    ]).SetRequired,
    wbStruct(VNAM, 'Voice', [
      wbFormIDCk('Male', [RACE, NULL]),
      wbFormIDCk('Female', [RACE, NULL])
    ]),
    wbStruct(DNAM, 'Default Hair', [
      wbFormIDCk('Male', [HAIR]),
      wbFormIDCk('Female', [HAIR])
    ]),
    wbInteger(CNAM, 'Default Hair Color', itU8).SetRequired,
    wbFloat(PNAM, 'FaceGen - Main clamp').SetRequired,
    wbFloat(UNAM, 'FaceGen - Face clamp').SetRequired,
    wbStruct(ATTR, 'Base Attributes', [
      wbStruct('Male', [
        wbInteger('Strength', itU8).SetDefaultNativeValue(50),
        wbInteger('Intelligence', itU8).SetDefaultNativeValue(50),
        wbInteger('Willpower', itU8).SetDefaultNativeValue(50),
        wbInteger('Agility', itU8).SetDefaultNativeValue(50),
        wbInteger('Speed', itU8).SetDefaultNativeValue(50),
        wbInteger('Endurance', itU8).SetDefaultNativeValue(50),
        wbInteger('Personality', itU8).SetDefaultNativeValue(50),
        wbInteger('Luck', itU8).SetDefaultNativeValue(50)
      ]),
      wbStruct('Female', [
        wbInteger('Strength', itU8).SetDefaultNativeValue(50),
        wbInteger('Intelligence', itU8).SetDefaultNativeValue(50),
        wbInteger('Willpower', itU8).SetDefaultNativeValue(50),
        wbInteger('Agility', itU8).SetDefaultNativeValue(50),
        wbInteger('Speed', itU8).SetDefaultNativeValue(50),
        wbInteger('Endurance', itU8).SetDefaultNativeValue(50),
        wbInteger('Personality', itU8).SetDefaultNativeValue(50),
        wbInteger('Luck', itU8).SetDefaultNativeValue(50)
      ])
    ]).SetRequired,
    wbRStruct('Face Data', [
      wbEmpty(NAM0, 'Face Data Marker'),
      wbRArrayS('Parts',
        wbHeadPart(
          wbEnum([
            {0} 'Head',
            {1} 'Ear (Male)',
            {2} 'Ear (Female)',
            {3} 'Mouth',
            {4} 'Teeth (Lower)',
            {5} 'Teeth (Upper)',
            {6} 'Tongue',
            {7} 'Eye (Left)',
            {8} 'Eye (Right)'
          ]),
          wbTexturedModel('Model', [MODL, MODB, MODT], []),
          nil))
    ]).SetRequired,
    wbEmpty(NAM1, 'Body Data Marker').SetRequired,
    wbRStruct('Male Body Data', [
      wbEmpty(MNAM, 'Male Body Data Marker'),
      wbTexturedModel('Model', [MODL, MODB, MODT], []),
      wbBodyParts
    ]).SetRequired,
    wbRStruct('Female Body Data', [
      wbEmpty(FNAM, 'Female Body Data Marker'),
      wbTexturedModel('Model', [MODL, MODB, MODT], []),
      wbBodyParts
    ]).SetRequired,
    wbArrayS(HNAM, 'Hairs', wbFormIDCk('Hair', [HAIR])).SetRequired,
    wbArrayS(ENAM, 'Eyes', wbFormIDCk('Eye', [EYES])).SetRequired,
    wbFaceGen,
    wbByteArray(SNAM, 'Unknown', 2).SetRequired
  ], True);

# REFR.Placed Object
TES3:
  wbRecord(REFR, 'Placed Object', @wbKnownSubRecordSignaturesREFR, [
    wbStruct(CNDT, 'New Cell Cell', [
      wbInteger('X', itS32),
      wbInteger('Y', itS32)
    ]).SetSummaryKeyOnValue([0,1])
      .SetSummaryPrefixSuffixOnValue(0, '(', '')
      .SetSummaryPrefixSuffixOnValue(1, '', ')')
      .SetSummaryDelimiterOnValue(', '),
    wbInteger(FRMR, 'Object Index', itU32, wbFRMRToString, nil, cpIgnore, True)
      .SetRequired
      .IncludeFlag(dfInternalEditOnly),
    wbString(NAME, 'Base Object'), //[ACTI, ALCH, APPA, ARMO, BODY, BOOK, CLOT, CONT, CREA, DOOR, INGR, LEVC, LOCK, MISC, NPC_, PROB, REPA, STAT, WEAP]
    wbInteger(UNAM, 'Reference Blocked', itU8, wbEnum(['True'])),
    wbFloat(XSCL, 'Scale', cpNormal, False, 1, 2),
    wbRStructSK([], 'Owner Data', [
      wbString(ANAM, 'Owner'), //[NPC_]
      wbString(BNAM, 'Global Variable'), //[GLOB]
      wbString(CNAM, 'Faction Owner'), //[FACT]
      wbInteger(INDX, 'Faction Rank', itU32)
    ], [], cpNormal, False, nil, True),
    wbFloat(XCHG, 'Enchantment Charge', cpNormal, False, 1, 0),
    wbString(XSOL, 'Soul'), //[CREA]
    wbInteger(INTV, 'Health', itU32),
    wbInteger(NAM9, 'Count', itU32),
    wbRStructSK([], 'Teleport Data', [
      wbVec3PosRot(DODT),
      wbString(DNAM, 'Cell') //[CELL]
    ]),
    wbRStructSK([], 'Lock Data', [
      wbInteger(FLTV, 'Lock Level', itU32).SetRequired,
      wbString(KNAM, 'Key'), //[MISC]
      wbString(TNAM, 'Trap') //[ENCH]
    ], [], cpNormal, False, nil, True),
    wbDeleted,
    wbVec3PosRot(DATA, 'Reference Data')
  ]).SetGetFormIDCallback(function(const aMainRecord: IwbMainRecord; out aFormID: TwbFormID): Boolean begin
      var lFRMR := aMainRecord.RecordBySignature[FRMR];
      Result := Assigned(lFRMR);
      if Result then begin
        aFormID := TwbFormID.FromCardinal(lFRMR.NativeValue);
        if aFormID.FileID.FullSlot = 0 then
          aFormID.FileID := TwbFileID.CreateFull($FF);
      end;
    end)
    .SetAfterLoad(wbDeletedAfterLoad);
TES4:
  wbRefRecord(REFR, 'Placed Object',
    wbFlags(wbFlagsList([
       7, 'Turn Off Fire',
       9, 'Cast Shadows',
      10, 'Persistent',
      11, 'Initially Disabled',
      15, 'Visible When Distant'
    ])), [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [ACTI, ALCH, AMMO, APPA, ARMO, BOOK, CLOT, CONT, DOOR, FLOR, FURN, GRAS, INGR, KEYM, LIGH, LVLC, MISC, SBSP, SGST, SLGM, SOUN, STAT, TREE, WEAP], False, cpNormal, True),
    wbStruct(XTEL, 'Teleport Destination', [
      wbFormIDCk('Door', [REFR], True),
      wbPosRot
    ]),
    wbStruct(XLOC, 'Lock information', [
      wbInteger('Lock Level', itU8),
      wbUnused(3),
      wbFormIDCk('Key', [KEYM, NULL]),
      wbUnion('Unused', wbXLOCFillerDecider, [
        wbUnused(),
        wbUnused(4)
      ]),
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          2, 'Leveled Lock'
        ], False, 3))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]),
    wbOwnership([XLOC]),
    wbXESP,
    wbFormIDCk(XTRG, 'Target', [ACHR, ACRE, REFR], True),
    wbStruct(XSED, 'Speed Tree', [
      wbInteger('Seed', itU8),
      wbUnused(0)
    ]),
    wbXLOD,
    wbFloat(XCHG, 'Charge'),
    wbInteger(XHLT, 'Health', itS32),
    wbRStruct('Unused', [
      wbFormIDCk(XPCI, 'Unused', [CELL]),
      wbString(FULL, 'Unused')
    ]),
    wbInteger(XLCM, 'Level Modifier', itS32),
    wbFormIDCk(XRTM, 'Reference Teleport Marker', [REFR]),
    wbActionFlag,
    wbInteger(XCNT, 'Count', itU32),
    wbRStruct('Map Marker', [
      wbEmpty(XMRK, 'Map Marker Data'),
      wbInteger(FNAM, 'Map Flags', itU8,
        wbFlags([
          {0} 'Visible',
          {1} 'Can Travel To'
        ])).SetRequired
           .IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFULLReq,
      wbStruct(TNAM, '', [
        wbInteger('Type', itU8,
          wbEnum([
            {0}  'None',
            {1}  'Camp',
            {2}  'Cave',
            {3}  'City',
            {4}  'Elven Ruin',
            {5}  'Fort Ruin',
            {6}  'Mine',
            {7}  'Landmark',
            {8}  'Tavern',
            {9}  'Settlement',
            {10} 'Daedric Shrine',
            {11} 'Oblivion Gate',
            {12} 'Unknown? (door icon)'
          ])),
        wbUnused(1)
      ]).SetRequired
    ]),
    wbEmpty(ONAM, 'Open by Default'),
    wbRagdoll,
    wbXSCL,
    wbInteger(XSOL, 'Contained Soul', itU8, wbSoulGemEnum),
    IsTES4R(wbGUID(XAAG), nil),
    IsTES4R(wbStringForward(XACN, 'Unknown', 128).IncludeFlag(dfHasZeroTerminator), nil),
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo)
          .SetAfterLoad(wbREFRAfterLoad);

# REGN.Region
TES3:
  wbRecord(REGN, 'Region', [
    wbDeleted,
    wbEditorID,
    wbFullName.SetRequired,
    wbStruct(WEAT, 'Weather Chances', [
      wbInteger('Clear', itU8).SetDefaultNativeValue(5),
      wbInteger('Cloudy', itU8).SetDefaultNativeValue(25),
      wbInteger('Foggy', itU8).SetDefaultNativeValue(35),
      wbInteger('Overcast', itU8).SetDefaultNativeValue(20),
      wbInteger('Rain', itU8).SetDefaultNativeValue(10),
      wbInteger('Thunder', itU8).SetDefaultNativeValue(5),
      wbInteger('Ash', itU8),
      wbInteger('Blight', itU8),
      wbInteger('Snow', itU8),
      wbInteger('Blizzard', itU8)
    ], cpNormal, True, nil, 8),
    wbString(BNAM, 'Sleep Creature'), //[LEVC]
    wbByteColors(CNAM, 'Region Map Color').SetRequired,
    wbRArray('Region Sounds',
      wbStruct(SNAM, 'Region Sound', [
        wbString(True, 'Sound', 32).SetAfterLoad(wbForwardForReal), //[SOUN]
        wbInteger('Chance', itU8).SetDefaultNativeValue(50)
      ]).SetSummaryKeyOnValue([0,1])
        .SetSummaryPrefixSuffixOnValue(0, 'Sound: ', ',')
        .SetSummaryPrefixSuffixOnValue(1, 'Chance: ', '')
        .IncludeFlag(dfCollapsed, wbCollapseSounds)
      )
  ]).SetFormIDBase($70);
TES4:
  wbRecord(REGN, 'Region',
    wbFlags(wbFlagsList([
      6, 'Border Region'
    ])), [
    wbEDID.SetRequired,
    wbICON,
    wbByteColors(RCLR, 'Map Color').SetRequired,
    wbFormIDCkNoReach(WNAM, 'Worldspace', [WRLD]),
    wbRegionAreas,
    wbRArrayS('Region Data Entries',
      wbRStructSK([0], 'Region Data Entry', [
        wbStructSK(RDAT, [0], 'Data Header', [
          wbInteger('Type', itU32,
            wbEnum([], [
              2, 'Objects',
              3, 'Weather',
              4, 'Map',
              5, 'Land',
              6, 'Grass',
              7, 'Sound'
            ])),
          wbInteger('Override', itU8, wbBoolEnum),
          wbInteger('Priority', itU8),
          wbUnused(2)
        ], cpNormal, True, nil, 3),
        wbArray(RDOT, 'Objects',
          wbStruct('Object', [
            wbFormIDCk('Object', [FLOR, LTEX, STAT, TREE]),
            wbInteger('Parent Index', itU16, wbHideFFFF),
            wbUnused(2),
            wbFloat('Density'),
            wbInteger('Clustering', itU8),
            wbInteger('Min Slope', itU8),
            wbInteger('Max Slope', itU8),
            wbInteger('Flags', itU8,
              wbFlags([
                {0} 'Conform to slope',
                {1} 'Paint Vertices',
                {2} 'Size Variance +/-',
                {3} 'X +/-',
                {4} 'Y +/-',
                {5} 'Z +/-',
                {6} 'Tree',
                {7} 'Huge Rock'
              ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
            wbInteger('Radius wrt Parent', itU16),
            wbInteger('Radius', itU16),
            wbFloat('Min Height'),
            wbFloat('Max Height'),
            wbFloat('Sink'),
            wbFloat('Sink Variance'),
            wbFloat('Size Variance'),
            wbStruct('Angle Variance', [
              wbInteger('X', itU16),
              wbInteger('Y', itU16),
              wbInteger('Z', itU16)
            ]).SetToStr(wbVec3ToStr)
              .IncludeFlag(dfCollapsed, wbCollapseVec3),
            wbUnused(2),
            wbUnused(4)
          ])),
        wbString(RDMP, 'Map Name', 0, cpTranslate),
        wbArrayS(RDGS, 'Grasses',
          wbStructSK([0], 'Grass', [
            wbFormIDCk('Grass', [GRAS]),
            wbUnused(4)
          ])),
        wbInteger(RDMD, 'Music Type', itU32, wbMusicEnum),
        wbRegionSounds,
        wbArrayS(RDWT, 'Weather Types',
          wbStructSK([0], 'Weather Type', [
            wbFormIDCk('Weather', [WTHR]),
            wbInteger('Chance', itU32)
          ]))
      ]))
  ], True).SetSummaryKey([3, 4])
          .IncludeFlag(dfSummaryMembersNoName);

# RELA.Relationship

# REPA.Repair Item
TES3:
  wbRecord(REPA, 'Repair Item',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(RIDT, 'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      wbInteger('Uses', itU32).SetDefaultNativeValue(10),
      wbFloat('Quality', cpNormal, False, 1, 2).SetDefaultNativeValue(1)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon
  ]).SetFormIDBase($40);

# REVB.Reverb Parameters

# RFCT.Visual Effect

# ROAD.Road
TES4:
  wbRecord(ROAD, 'Road', [
    wbPGRP,
    wbArray(PGRR, 'Point-to-Point Connections',
      wbArray('Point', wbVec3('Point'), wbCalcPGRRSize)).SetRequired
  ]).SetAddInfo(wbROADAddInfo);

# SBSP.Subspace
TES4:
  wbRecord(SBSP, 'Subspace', [
    wbEDID,
    wbStruct(DNAM, 'Bounds', [
      wbFloat('X').SetDefaultNativeValue(400),
      wbFloat('Y').SetDefaultNativeValue(400),
      wbFloat('Z').SetDefaultNativeValue(200)
    ]).SetRequired
  ]).SetSummaryKey([1]);

# SCEN.Scene

# SCPT.Script
TES3:
  wbRecord(SCPT, 'Script', @wbKnownSubRecordSignaturesSCPT, [
    wbStruct(SCHD, 'Script Header', [
      //Name can be saved with 36 characters in the CS, but it collides with Number of Shorts.
      wbString('Name', 32),
      wbInteger('Number of Shorts', itU32),
      wbInteger('Number of Longs', itU32),
      wbInteger('Number of Floats', itU32),
      wbInteger('Compiled Size', itU32),
      wbInteger('Local Variable Size', itU32)
    ]).SetSummaryKeyOnValue([4,5,2,1,3])
      .SetSummaryPrefixSuffixOnValue(4, '{Compiled Size: ', ',')
      .SetSummaryPrefixSuffixOnValue(5, 'Local Var Size: ', ',')
      .SetSummaryPrefixSuffixOnValue(2, 'Shorts: ', ',')
      .SetSummaryPrefixSuffixOnValue(1, 'Longs: ', ',')
      .SetSummaryPrefixSuffixOnValue(3, 'Floats: ', '}')
      .SetRequired
      .IncludeFlag(dfSummaryMembersNoName)
      .IncludeFlag(dfCollapsed, wbCollapseScriptData),
    wbDeleted,
    wbArrayS(SCVR, 'Script Variables', wbString('Script Variable', 0, cpCritical)),
    wbByteArray(SCDT, 'Compiled Script'),
    wbStringScript(SCTX, 'Script Source').SetRequired
  ]).SetFormIDBase($30)
    .SetGetEditorIDCallback(function (const aSubRecord: IwbSubRecord): string begin
      Result := aSubRecord.ElementEditValues['Name'];
    end)
    .SetSetEditorIDCallback(procedure (const aSubRecord: IwbSubRecord; const aEditorID: string) begin
      aSubRecord.ElementEditValues['Name'] := aEditorID;
    end)
    .SetToStr(wbScriptToStr);
TES4:
  wbRecord(SCPT, 'Script', [
    wbEDID.SetRequired,
    wbByteArray(SCHD, 'Unknown (Script Header?)'),
    wbSCHR.SetRequired,
    wbByteArray(SCDA, 'Compiled Script').SetRequired,
    wbStringScript(SCTX, 'Script Source').SetRequired,
    wbRArrayS('Local Variables',
      wbRStructSK([0], 'Local Variable', [
        wbStructSK(SLSD, [0], 'Local Variable Data', [
          wbInteger('Index', itU32),
          wbUnused(12),
          wbInteger('IsLongOrShort', itU8, wbBoolEnum, cpCritical),
          wbByteArray('Unused')
        ]).IncludeFlag(dfSummaryMembersNoName),
        wbString(SCVR, 'Name', 0, cpCritical)
      ]).SetSummaryKey([1])
        .IncludeFlag(dfSummaryMembersNoName)),
    wbSCROs
  ]).SetToStr(wbScriptToStr);

# SCRL.Scroll

# SGST.Sigil Stone
TES4:
  wbRecord(SGST, 'Sigil Stone', [
    wbEDID,
    {wbStruct(OBME, 'Oblivion Magic Extender', [
      wbInteger('Record Version', itU8),
      wbOBMEVersion,
      wbUnused($1C)
    ]).SetDontShow(wbOBMEDontShow),}
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbEffects.SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Uses ', itU8),
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ]).SetRequired
  ]);

# SHOU.Shout

# SKIL.Skill
TES3:
  wbRecord(SKIL, 'Skill', @wbKnownSubRecordSignaturesINDX, [
    wbInteger(INDX, 'Name', itU32, wbSkillEnum).SetRequired,
    wbDeleted,
    wbStruct(SKDT, 'Data', [
      wbInteger('Governing Attribute', itS32, wbAttributeEnum),
      wbInteger('Type', itU32, wbSpecializationEnum),
      wbUnion('Actions', wbSkillDecider, [
        wbStruct('Block', [
          wbFloat('Successful Block'),
          wbUnused(12)
        ]),
        wbStruct('Armorer', [
          wbFloat('Successful Repair'),
          wbUnused(12)
        ]),
        wbStruct('Armor', [
          wbFloat('Hit By Opponent'),
          wbUnused(12)
        ]),
        wbStruct('Weapon', [
          wbFloat('Successful Attack'),
          wbUnused(12)
        ]),
        wbStruct('Athletics', [
          wbFloat('Seconds of Running'),
          wbFloat('Seconds of Swimming'),
          wbUnused(8)
        ]),
        wbStruct('Enchant', [
          wbFloat('Recharge Item'),
          wbFloat('Use Magic Item'),
          wbFloat('Create Magic Item'),
          wbFloat('Cast When Strikes')
        ]),
        wbStruct('Magic School', [
          wbFloat('Successful Cast'),
          wbUnused(12)
        ]),
        wbStruct('Alchemy', [
          wbFloat('Potion Creation'),
          wbFloat('Ingredient Use'),
          wbUnused(8)
        ]),
        wbStruct('Security', [
          wbFloat('Defeat Trap'),
          wbFloat('Pick Lock'),
          wbUnused(8)
        ]),
        wbStruct('Sneak', [
          wbFloat('Avoid Notice'),
          wbFloat('Successful Pick-Pocket'),
          wbUnused(8)
        ]),
        wbStruct('Acrobatics', [
          wbFloat('Jump'),
          wbFloat('Fall'),
          wbUnused(8)
        ]),
        wbStruct('Mercantile', [
          wbFloat('Successful Bargain'),
          wbFloat('Successful Bribe'),
          wbUnused(8)
        ]),
        wbStruct('Speechcraft', [
          wbFloat('Successful Persuasion'),
          wbFloat('Failed Persuasion'),
          wbUnused(8)
        ])
      ])
    ]).SetRequired,
    wbDescription
  ]).SetFormIDBase($01);
TES4:
  wbRecord(SKIL, 'Skill', [
    wbEDID.SetRequired,
    wbInteger(INDX, 'Skill', itS32, wbMajorSkillEnum).SetRequired,
    wbDESC.SetRequired,
    wbICON.SetRequired,
    wbStruct(DATA, 'Skill Data', [
      wbInteger('Action', itS32, wbMajorSkillEnum),
      wbInteger('Attribute', itU32, wbAttributeEnum),
      wbInteger('Specialization', itU32, wbSpecializationEnum),
      wbArray('Use Values', wbFloat('Use Value'), 2)
    ]).SetRequired,
    wbString(ANAM, 'Apprentice Text', 0, cpTranslate).SetRequired,
    wbString(JNAM, 'Journeyman Text', 0, cpTranslate).SetRequired,
    wbString(ENAM, 'Expert Text', 0, cpTranslate).SetRequired,
    wbString(MNAM, 'Master Text', 0, cpTranslate).SetRequired
  ]).SetSummaryKey([2]);

# SLGM.Soul Gem
TES4:
  wbRecord(SLGM, 'Soul Gem',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ]).SetRequired,
    wbInteger(SOUL, 'Contained Soul', itU8, wbSoulGemEnum).SetRequired,
    wbInteger(SLCP, 'Maximum Capacity', itU8, wbSoulGemEnum).SetRequired
  ]);

# SMBN.Story Manager Branch Node

# SMEN.Story Manager Event Node

# SMNQ.Story Manager Quest Node

# SNCT.Sound Category

# SNDG.Sound Generator
TES3:
  wbRecord(SNDG, 'Sound Generator', [
    wbEditorID,
    wbInteger(DATA, 'Type', itU32,
      wbEnum([
      {0} 'Left Foot',
      {1} 'Right Foot',
      {2} 'Swim Left',
      {3} 'Swim Right',
      {4} 'Moan',
      {5} 'Roar',
      {6} 'Scream',
      {7} 'Land'
      ])).SetDefaultNativeValue(7)
         .SetRequired,
    wbString(CNAM, 'Creature'), //[CREA]
    wbString(SNAM, 'Sound')
      .SetDefaultNativeValue('Body Fall Medium')
      .SetRequired, //[SOUN]
    wbDeleted
  ]).SetFormIDBase($28)
    .SetSummaryKey([3]);

# SNDR.Sound Reference

# SOPM.Sound Output Model

# SOUN.Sound
TES3:
  wbRecord(SOUN, 'Sound', @wbKnownSubRecordSignaturesNoFNAM, [
    wbEditorID,
    wbDeleted,
    wbString(FNAM, 'Sound Filename').SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Volume', itU8, wbDiv(255, 2)).SetDefaultNativeValue(1),
      wbInteger('Minimum Range', itU8),
      wbInteger('Maximum Range', itU8)
    ]).SetRequired
  ]).SetFormIDBase($40);
TES4:
  wbRecord(SOUN, 'Sound', [
    wbEDID,
    wbString(FNAM, 'Sound Filename'),
    wbStruct(SNDX, 'Sound Data', wbSoundDataMembers).SetRequired,
    wbStruct(SNDD, 'Sound Data', wbSoundDataMembers, cpNormal, False, nil, 6).SetDontShow(wbAlwaysDontShow)
  ]).SetSummaryKey([1])
    .SetAfterLoad(wbSOUNAfterLoad);

# SPEL.Spell
TES3:
  wbRecord(SPEL, 'Spellmaking',
    wbFlags(wbFlagsList([
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbFullName,
    wbStruct(SPDT, 'Data', [
      wbInteger('Type', itU32,
        wbEnum([
        {0} 'Spell',
        {1} 'Ability',
        {2} 'Blight',
        {3} 'Disease',
        {4} 'Curse',
        {5} 'Power'
        ])),
      wbInteger('Spell Cost', itU32),
      wbInteger('Flags', itU32,
        wbFlags([
        {0} 'Auto Calculate Cost',
        {1} 'Player Start Spell',
        {2} 'Always Succeeds'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbEffects
  ]).SetFormIDBase($0A);
TES4:
  wbRecord(SPEL, 'Spell', [
    wbEDID,
    {wbStruct(OBME, 'Oblivion Magic Extender', [
      wbInteger('Record Version', itU8),
      wbOBMEVersion,
      wbUnused($1C)
    ]).SetDontShow(wbOBMEDontShow),}
    wbFULL,
    wbStruct(SPIT, 'Data', [
      wbInteger('Type', itU8,
        wbEnum([
          {0} 'Spell',
          {1} 'Disease',
          {2} 'Power',
          {3} 'Lesser Power',
          {4} 'Ability',
          {5} 'Poison'
        ])),
      wbUnused(3),
      wbInteger('Cost', itU32),
      wbInteger('Level', itU8,
        wbEnum([
          {0} 'Novice',
          {1} 'Apprentice',
          {2} 'Journeyman',
          {3} 'Expert',
          {4} 'Master'
        ])),
      wbUnused(3),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Manual Spell Cost',
          {1} 'Immune to Silence 1',
          {2} 'Player Start Spell',
          {3} 'Immune to Silence 2',
          {4} 'Area Effect Ignores LOS',
          {5} 'Script Effect Always Applies',
          {6} 'Disallow Spell Absorb/Reflect',
          {7} 'Touch Spell Explodes w/ no Target'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired,
    wbEffects.SetRequired
  ]);

# SPGD.Shader Particle Geometry

# SSCR.Start Script
TES3:
  wbRecord(SSCR, 'Start Script', @wbKnownSubRecordSignaturesSSCR, [
    wbDeleted,
    wbString(DATA, 'Numerical ID').SetRequired,
    wbString(NAME, 'Script').SetRequired //[SCPT]
  ]).SetFormIDBase($3F)
    .SetAfterLoad(wbDeletedAfterLoad);

# STAT.Static
TES3:
  wbRecord(STAT, 'Static',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired
  ]).SetFormIDBase($40)
    .SetSummaryKey([2]);
TES4:
  wbRecord(STAT, 'Static',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbArray(DMTL, 'Distant Model Texture List',
      wbStruct('Texture', [
        wbInteger('File Hash (PC)', itU64, wbFileHashCallback),
        wbInteger('File Hash (Console)', itU64, wbFileHashCallback),
        wbInteger('Folder Hash', itU64, wbFolderHashCallback)
      ]))
  ]).SetSummaryKey([1]);

# STDT.xx

# SUNP.xx

# TACT.Talking Activator

# TERM.Computer Terminals

# TMLM.Terminal Menus

# TREE.Tree
TES4:
  wbRecord(TREE, 'Tree',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbTexturedModel('SPT File', [MODL, MODB, MODT], []),
    wbString(ICON, 'Leaf Texture'),
    wbArrayS(SNAM, 'SpeedTree Seeds', wbInteger('SpeedTree Seed', itU32)),
    wbStruct(CNAM, 'Tree Data', [
      wbFloat('Leaf Curvature').SetDefaultNativeValue(2.5),
      wbFloat('Minimum Leaf Angle').SetDefaultNativeValue(5),
      wbFloat('Maximum Leaf Angle').SetDefaultNativeValue(85),
      wbFloat('Branch Dimming Value').SetDefaultNativeValue(0.5),
      wbFloat('Leaf Dimming Value').SetDefaultNativeValue(0.7),
      wbInteger('Shadow Radius', itS32).SetDefaultNativeValue(-842150464),
      wbFloat('Rock Speed').SetDefaultNativeValue(1),
      wbFloat('Rustle Speed').SetDefaultNativeValue(1)
    ]).SetRequired,
    wbStruct(BNAM, 'Billboard Dimensions', [
      wbFloat('Width'),
      wbFloat('Height')
    ]).SetRequired
  ]).SetSummaryKey([1]);

# TRNS.TRNS Record

# TXST.Texture Set

# VTYP.Voice Type

# WATR.Water Type
TES4:
  wbRecord(WATR, 'Water',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbString(TNAM, 'Texture').SetRequired,
    wbInteger(ANAM, 'Opacity', itU8)
      .SetDefaultNativeValue(75)
      .SetRequired,
    wbInteger(FNAM, 'Flags', itU8,
      wbFlags([
        {0} 'Causes Damage',
        {1} 'Reflective'
     ])).SetRequired
        .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbString(MNAM, 'Material ID').SetRequired,
    wbFormIDCk(SNAM, 'Sound', [SOUN]),
    wbStruct(DATA, 'Data', [
      wbFloat('Wind Velocity').SetDefaultNativeValue(0.1),
      wbFloat('Wind Direction').SetDefaultNativeValue(90),
      wbFloat('Wave Amplitude').SetDefaultNativeValue(0.5),
      wbFloat('Wave Frequency').SetDefaultNativeValue(1),
      wbFloat('Sun Power').SetDefaultNativeValue(50),
      wbFloat('Reflectivity Amount').SetDefaultNativeValue(0.5),
      wbFloat('Fresnel Amount').SetDefaultNativeValue(0.025),
      wbFloat('Scroll X Speed'),
      wbFloat('Scroll Y Speed'),
      wbStruct('Fog Distance', [
        wbFloat('Near').SetDefaultNativeValue(27852.800782),
        wbFloat('Far').SetDefaultNativeValue(163840)
      ]),
      wbByteColors('Shallow Color', 0, 128, 128),
      wbByteColors('Deep Color', 0, 0, 25),
      wbByteColors('Reflection Color', 255, 255, 255),
      wbInteger('Texture Blend', itU8).SetDefaultNativeValue(50),
      wbUnused(3),
      wbStruct('Rain Simulator', [
        wbFloat('Force').SetDefaultNativeValue(0.1),
        wbFloat('Velocity').SetDefaultNativeValue(0.6),
        wbFloat('Falloff').SetDefaultNativeValue(0.985),
        wbFloat('Dampner').SetDefaultNativeValue(2),
        wbFloat('Starting Size').SetDefaultNativeValue(0.01)
      ], cpNormal, True, nil, 0),
      wbStruct('Displacement Simulator', [
        wbFloat('Force').SetDefaultNativeValue(0.4),
        wbFloat('Velocity').SetDefaultNativeValue(0.6),
        wbFloat('Falloff').SetDefaultNativeValue(0.985),
        wbFloat('Dampner').SetDefaultNativeValue(10),
        wbFloat('Starting Size').SetDefaultNativeValue(0.05)
      ], cpNormal, True, nil, 0),
      wbInteger('Damage', itU16)
    ], cpNormal, True, nil, 0),
    wbStruct(GNAM, 'Related Waters', [
      wbFormIDCk('Daytime', [WATR, NULL]),
      wbFormIDCk('Nighttime', [WATR, NULL]),
      wbFormIDCk('Underwater', [WATR, NULL])
    ]).SetRequired
  ]).SetSummaryKey([1]);

# WEAP.Weapon
TES3:
  wbRecord(WEAP, 'Weapon',
    wbFlags(wbFlagsList([
      10, 'References Persist',
      13, 'Blocked'
    ])), [
    wbEditorID,
    wbDeleted,
    wbModel.SetRequired,
    wbFullName,
    wbStruct(WPDT, 'Data', [
      wbFloat('Weight', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Value', itU32).SetDefaultNativeValue(1),
      wbInteger('Type', itU16,
        wbEnum([
        {0}  'Short Blade One Hand',
        {1}  'Long Blade One Hand',
        {2}  'Long Blade Two Close',
        {3}  'Blunt One Hand',
        {4}  'Blunt Two Close',
        {5}  'Blunt Two Wide',
        {6}  'Spear Two Wide',
        {7}  'Axe One Hand',
        {8}  'Axe Two Hand',
        {9}  'Marksman Bow',
        {10} 'Marksman Crossbow',
        {11} 'Marksman Thrown',
        {12} 'Arrow',
        {13} 'Bolt'
        ])).SetDefaultNativeValue(12),
      wbInteger('Health', itU16).SetDefaultNativeValue(100),
      wbFloat('Speed', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbFloat('Reach', cpNormal, False, 1, 2).SetDefaultNativeValue(1),
      wbInteger('Enchanting Charge', itU16).SetDefaultNativeValue(100),
      wbStruct('Damage Types', [
        wbStruct('Chop', [
          wbInteger('Minimum', itU8).SetDefaultNativeValue(1),
          wbInteger('Maximum', itU8).SetDefaultNativeValue(5)
        ]),
        wbStruct('Slash', [
          wbInteger('Minimum', itU8).SetDefaultNativeValue(1),
          wbInteger('Maximum', itU8).SetDefaultNativeValue(5)
        ]),
        wbStruct('Thrust', [
          wbInteger('Minimum', itU8).SetDefaultNativeValue(1),
          wbInteger('Maximum', itU8).SetDefaultNativeValue(5)
        ])
      ]),
      wbInteger('Flags', itU32,
        wbFlags([
        {0} 'Silver Weapon',
        {1} 'Ignore Normal Weapon Resistance'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbScript, //[SCPT]
    wbIcon,
    wbEnchantment //[ENCH]
  ]).SetFormIDBase($40);
TES4:
  wbRecord(WEAP, 'Weapon',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDID,
    wbFULL,
    wbTexturedModel('Model', [MODL, MODB, MODT], []),
    wbICON,
    wbSCRI,
    wbEnchantment(True),
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU8,
        wbEnum([
          {0} 'Blade One Hand',
          {1} 'Blade Two Hand',
          {2} 'Blunt One Hand',
          {3} 'Blunt Two Hand',
          {4} 'Staff',
          {5} 'Bow'
        ])),
      wbUnused(3),
      wbFloat('Speed'),
      wbFloat('Reach'),
      wbInteger('Ignores Normal Weapon Resistance', itU32, wbBoolEnum),
      wbInteger('Value', itU32),
      wbInteger('Health', itU32),
      wbFloat('Weight'),
      wbInteger('Damage', itU16)
    ]).SetRequired
  ]);

# WOOP.Word Of Power

# WRLD.Worldspace
TES4:
  wbRecord(WRLD, 'Worldspace',
    wbFlags(wbFlagsList([
      19, 'Can''t Wait'
    ])), [
    wbEDID.SetRequired,
    wbFULL,
    wbFormIDCk(WNAM, 'Parent Worldspace', [WRLD]),
    wbFormIDCk(CNAM, 'Climate', [CLMT])
      .SetDefaultNativeValue(351)
      .SetIsRemovable(wbWorldClimateIsRemovable),
    wbFormIDCk(NAM2, 'Water', [WATR])
      .SetDefaultNativeValue(24)
      .SetIsRemovable(wbWorldWaterIsRemovable),
    wbString(ICON, 'Map Image'),
    wbWorldMapData,
    wbInteger(DATA, 'Flags', itU8,
      wbFlags(wbSparseFlags([
        0, 'Small world',
        1, 'Can''t fast travel',
        2, 'Oblivion worldspace',
        4, 'No LOD water'
      ], False, 5), True)
    ).SetDefaultNativeValue(1)
     .SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbWorldObjectBounds,
    wbInteger(SNAM, 'Music', itU32, wbMusicEnum),
    wbWorldOffsetData
  ]).SetAfterLoad(wbWorldAfterLoad)
    .SetAfterSet(wbWorldAfterSet);

# WTHR.Weather
TES4:
  wbRecord(WTHR, 'Weather', [
    wbEDID.SetRequired,
    wbString(CNAM, 'Cloud Texture Lower Layer'),
    wbString(DNAM, 'Cloud Texture Upper Layer'),
    wbTexturedModel('Precipitation Model', [MODL, MODB, MODT], []),
    wbWeatherColors,
    wbWeatherFogDistance,
    wbStruct(HNAM, 'HDR Data', [
      wbFloat('Eye Adapt Speed'),
      wbFloat('Blur Radius'),
      wbFloat('Blur Passes'),
      wbFloat('Emissive Mult'),
      wbFloat('Target LUM'),
      wbFloat('Upper LUM Clamp'),
      wbFloat('Bright Scale'),
      wbFloat('Bright Clamp'),
      wbFloat('LUM Ramp No Tex'),
      wbFloat('LUM Ramp Min'),
      wbFloat('LUM Ramp Max'),
      wbFloat('Sunlight Dimmer'),
      wbFloat('Grass Dimmer'),
      wbFloat('Tree Dimmer')
    ]).SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Wind Speed', itU8),
      wbInteger('Cloud Speed (Lower)', itU8),
      wbInteger('Cloud Speed (Upper)', itU8),
      wbInteger('Trans Delta', itU8),
      wbInteger('Sun Glare', itU8),
      wbInteger('Sun Damage', itU8),
      wbInteger('Precipitation - Begin Fade In', itU8),
      wbInteger('Precipitation - End Fade Out', itU8),
      wbInteger('Thunder/Lightning - Begin Fade In', itU8),
      wbInteger('Thunder/Lightning - End Fade Out', itU8),
      wbInteger('Thunder/Lightning - Frequency', itU8),
      wbInteger('Flags ', itU8,
        wbFlags([
          {0} 'Weather - Pleasant',
          {1} 'Weather - Cloudy',
          {2} 'Weather - Rainy',
          {3} 'Weather - Snow'
        ], True)
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbWeatherLightningColor
    ]).SetRequired,
    wbWeatherSounds
  ]).SetSummaryKey([1,2,3]);