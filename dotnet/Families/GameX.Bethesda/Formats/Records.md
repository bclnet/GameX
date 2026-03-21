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
FO3:
  wbRecord(ACTI, 'Activator',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD',
      9, 'On Local Map',
     10, 'Quest Item',
     15, 'Visible When Distant',
     16, 'Random Anim Start',
     17, 'Dangerous',
     19, 'Has Platform Specific Textures',
     25, 'Obstacle',
     26, 'Navmesh - Filter',
     27, 'Navmesh - Bounding Box',
     29, 'Child Can Use',
     30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbSCRI,
    wbDEST,
    wbFormIDCk(SNAM, 'Sound - Looping', [SOUN]),
    wbFormIDCk(VNAM, 'Sound - Activation', [SOUN]),
    wbFormIDCk(RNAM, 'Radio Station', [TACT]),
    wbFormIDCk(WNAM, 'Water Type', [WATR])
  ]);
FNV:
  wbRecord(ACTI, 'Activator',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD',
      9, 'On Local Map',
     10, 'Quest Item',
     15, 'Visible When Distant',
     16, 'Random Anim Start',
     17, 'Dangerous',
     19, 'Has Platform Specific Textures',
     25, 'Obstacle',
     26, 'Navmesh - Filter',
     27, 'Navmesh - Bounding Box',
     29, 'Child Can Use',
     30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbSCRI,
    wbDEST,
    wbFormIDCk(SNAM, 'Sound - Looping', [SOUN]),
    wbFormIDCk(VNAM, 'Sound - Activation', [SOUN]),
    wbFormIDCk(INAM, 'Radio Template', [SOUN]),
    wbFormIDCk(RNAM, 'Radio Station', [TACT]),
    wbFormIDCk(WNAM, 'Water Type', [WATR]),
    wbStringKC(XATO, 'Activation Prompt')
  ]);
TES5:
  wbRecord(ACTI, 'Activator',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD',
      8, 'Must Update Anims',
      9, 'Hidden From Local Map',
     15, 'Has Distant LOD',
     16, 'Random Anim Start',
     17, 'Dangerous',
     20, 'Ignore Object Interaction',
     23, 'Is Marker',
     25, 'Obstacle',
     26, 'Navmesh - Filter',
     27, 'Navmesh - Bounding Box',
     29, 'Child Can Use',
     30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDID,
    wbVMAD,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbDEST,
    wbKeywords,
    wbByteColors(PNAM, 'Marker Color').SetRequired,
    wbFormIDCk(SNAM, 'Sound - Looping', [SNDR]),
    wbFormIDCk(VNAM, 'Sound - Activation', [SNDR]),
    wbFormIDCk(WNAM, 'Water Type', [WATR]),
    wbLString(RNAM, 'Activate Text Override', 0, cpTranslate),
    wbInteger(FNAM, 'Flags', itU16, wbFlags([
      'No Displacement',
      'Ignored by Sandbox'
    ])).SetRequired.IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbFormIDCk(KNAM, 'Interaction Keyword', [KYWD])
  ]);

# ADDN.Addon Node
FO3:
  wbRecord(ADDN, 'Addon Node', [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbInteger(DATA, 'Node Index', itS32).SetRequired,
    wbFormIDCk(SNAM, 'Sound', [SOUN]),
    wbStruct(DNAM, 'Data', [
      wbInteger('Master Particle System Cap', itU16),
      wbByteArray('Unknown', 2)
    ]).SetRequired
  ]);
FNV:
  wbRecord(ADDN, 'Addon Node', [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbInteger(DATA, 'Node Index', itS32, nil, cpNormal, True),
    wbFormIDCk(SNAM, 'Sound', [SOUN]),
    wbStruct(DNAM, 'Data', [
      wbInteger('Master Particle System Cap', itU16),
      wbByteArray('Unknown', 2)
    ], cpNormal, True)
  ]);

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
FO3:
  wbRecord(ALCH, 'Ingestible',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      29, 'Unknown 29'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULLReq,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbETYPReq,
    wbFloat(DATA, 'Weight').SetRequired,
    wbStruct(ENIT, 'Effect Data', [
      wbInteger('Value', itS32),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'No Auto-Calc',
          {1} 'Food Item',
          {2} 'Medicine'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbFormIDCk('Withdrawal Effect', [SPEL, NULL]),
      wbFloat('Addiction Chance'),
      wbFormIDCk('Sound - Consume', [SOUN])
    ]).SetRequired,
    wbEffectsReq
  ]);
FNV:
  wbRecord(ALCH, 'Ingestible',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      29, 'Unknown 29'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULLReq,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbETYPReq,
    wbFloat(DATA, 'Weight', cpNormal, True),
    wbStruct(ENIT, 'Effect Data', [
      wbInteger('Value', itS32),
      wbInteger('Flags?', itU8, wbFlags([
        'No Auto-Calc (Unused)',
        'Food Item',
        'Medicine'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbFormIDCk('Withdrawal Effect', [SPEL, NULL]),
      wbFloat('Addiction Chance'),
      wbFormIDCk('Sound - Consume', [SOUN, NULL])
    ], cpNormal, True),
    wbEffectsReq
  ]);
TES5:
  wbRecord(ALCH, 'Ingestible',
    wbFlags(wbFlagsList([
      29, 'Medicine'
    ])), [
    wbEDID,
    wbOBND(True),
    wbFULL,
    wbKeywords,
    wbDESC,
    wbGenericModel,
    wbDEST,
    wbICON,
    wbYNAM,
    wbZNAM,
    wbETYP,
    wbFloat(DATA, 'Weight', cpNormal, True),
    wbStruct(ENIT, 'Effect Data', [
      wbInteger('Value', itS32),
      wbInteger('Flags', itU32, wbFlags([
        {0x00000001} 'No Auto-Calc',
        {0x00000002} 'Food Item',
        {0x00000004} 'Unknown 3',
        {0x00000008} 'Unknown 4',
        {0x00000010} 'Unknown 5',
        {0x00000020} 'Unknown 6',
        {0x00000040} 'Unknown 7',
        {0x00000080} 'Unknown 8',
        {0x00000100} 'Unknown 9',
        {0x00000200} 'Unknown 10',
        {0x00000400} 'Unknown 11',
        {0x00000800} 'Unknown 12',
        {0x00001000} 'Unknown 13',
        {0x00002000} 'Unknown 14',
        {0x00004000} 'Unknown 15',
        {0x00008000} 'Unknown 16',
        {0x00010000} 'Medicine',
        {0x00020000} 'Poison'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFormID('Addiction'),
      wbFloat('Addiction Chance'),
      wbFormIDCk('Sound - Consume', [SNDR,NULL])
    ], cpNormal, True),
    wbEffectsReq
  ]);

# ALOC.Media Location Controller
FNV:
  wbRecord(ALOC, 'Media Location Controller', [
    wbEDIDReq,
    wbFULL,
    wbByteArray(NAM1, 'Flags and Enums, messily combined'),
    wbUnknown(NAM2),
    wbUnknown(NAM3),
    wbFloat(NAM4, 'Location Delay'),
    wbInteger(NAM5, 'Day Start', itU32, wbAlocTime),
    wbInteger(NAM6, 'Night Start', itU32, wbAlocTime),
    wbFloat(NAM7, 'Retrigger Delay'),
    wbRArrayS('Neutral Sets',
      wbFormIDCk(HNAM, 'Media Set', [MSET])
    ),
    wbRArrayS('Ally Sets',
      wbFormIDCk(ZNAM, 'Media Set', [MSET])
    ),
    wbRArrayS('Friend Sets',
      wbFormIDCk(XNAM, 'Media Set', [MSET])
    ),
    wbRArrayS('Enemy Sets',
      wbFormIDCk(YNAM, 'Media Set', [MSET])
    ),
    wbRArrayS('Location Sets',
      wbFormIDCk(LNAM, 'Media Set', [MSET])
    ),
    wbRArrayS('Battle Sets',
      wbFormIDCk(GNAM, 'Media Set', [MSET])
    ),
    wbFormIDCk(RNAM, 'Conditional Faction', [FACT]),
    wbUnknown(FNAM)
  ]);

# AMEF.Ammo Effect
  wbRecord(AMEF, 'Ammo Effect', [
    wbEDIDReq,
    wbFULL,
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU32, wbEnum([
        'Damage Mod',
        'DR Mod',
        'DT Mod',
        'Spread Mod',
        'Weapon Condition Mod',
        'Fatigue Mod'
      ])),
      wbInteger('Operation', itU32, wbEnum([
        'Add',
        'Multiply',
        'Subtract'
      ])),
      wbFloat('Value')
    ])
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
FO3:
  wbRecord(AMMO, 'Ammunition', [
    wbEDIDReq,
    wbOBND(True),
    wbFULLReq,
    wbGenericModel,
    wbICON,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, 'Data', [
      wbFloat('Speed'),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Ignores Normal Weapon Resistance',
          {1} 'Non-Playable'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbInteger('Value', itS32),
      wbInteger('Clip Rounds', itU8)
    ]).SetRequired,
    wbStringKC(ONAM, 'Short Name', 0, cpTranslate)
  ]);
FNV:
  wbRecord(AMMO, 'Ammunition', [
    wbEDIDReq,
    wbOBND(True),
    wbFULLReq,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, 'Data', [
      wbFloat('Speed'),
      wbInteger('Flags', itU8, wbFlags([
        'Ignores Normal Weapon Resistance',
        'Non-Playable'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbInteger('Value', itS32),
      wbInteger('Clip Rounds', itU8)
    ], cpNormal, True),
    wbStruct(DAT2, 'Data 2', [
      wbInteger('Proj. per Shot', itU32),
      wbFormIDCk('Projectile', [PROJ, NULL]),
      wbFloat('Weight'),
      wbFormIDCk('Consumed Ammo', [AMMO, MISC, NULL]),
      wbFloat('Consumed Percentage')
    ], cpNormal, False, nil, 3),
    wbStringKC(ONAM, 'Short Name', 0, cpTranslate),
    wbStringKC(QNAM, 'Abbrev.', 0, cpTranslate),
    wbRArray('Ammo Effects',
      wbFormIDCk(RCIL, 'Effect', [AMEF])
    )
  ]);
TES5:
  wbRecord(AMMO, 'Ammunition',
    wbFlags(wbFlagsList([
      2, 'Non-Playable'
    ])), [
    wbEDID,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbDESC,
    wbKeywords,
    IsSSE(
      wbStruct(DATA, 'Data', [
        wbFormIDCk('Projectile', [PROJ, NULL]),
        wbInteger('Flags', itU32, wbFlags([
          'Ignores Normal Weapon Resistance',
          'Non-Playable',
          'Non-Bolt'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbFloat('Damage'),
        wbInteger('Value', itU32),
        wbFloat('Weight')
      ], cpNormal, True, nil, 4),
      wbStruct(DATA, 'Data', [
        wbFormIDCk('Projectile', [PROJ, NULL]),
        wbInteger('Flags', itU32, wbFlags([
          'Ignores Normal Weapon Resistance',
          'Non-Playable',
          'Non-Bolt'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbFloat('Damage'),
        wbInteger('Value', itU32)
      ], cpNormal, True)
    ),
    wbString(ONAM, 'Short Name')
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
FO3:
  wbRecord(ANIO, 'Animated Object', [
    wbEDIDReq,
    wbGenericModel(True),
    wbFormIDCk(DATA, 'Animation', [IDLE]).SetRequired
  ]);
FNV:
  wbRecord(ANIO, 'Animated Object', [
    wbEDIDReq,
    wbGenericModel(True),
    wbFormIDCk(DATA, 'Animation', [IDLE], False, cpNormal, True)
  ]);
TES5:
  wbRecord(ANIO, 'Animated Object',
    wbFlags(wbFlagsList([
      9, 'Unknown 9' // always present in updated records, not in Skyrim.esm
    ]), [9]), [
    wbEDID,
    wbGenericModel,
    wbString(BNAM, 'Unload Event')
  ]).SetSummaryKey([1]);

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

# ARMA.Armor Addon
FO3:
  wbRecord(ARMA, 'Armor Addon', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbBMDT,
    wbRStruct('Male', [
      wbTexturedModel('Biped Model', [MODL, MODT], [wbMODS, wbMODD]),
      wbTexturedModel('World Model', [MOD2, MO2T], [wbMO2S, nil]),
      wbString(ICON, 'Icon Image'),
      wbString(MICO, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('Female', [
      wbTexturedModel('Biped Model', [MOD3, MO3T], [wbMO3S, wbMOSD]),
      wbTexturedModel('World Model', [MOD4, MO4T], [wbMO4S, nil]),
      wbString(ICO2, 'Icon Image'),
      wbString(MIC2, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbETYPReq,
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itS32),
      wbInteger('Max Condition', itS32),
      wbFloat('Weight')
    ]).SetRequired,
    wbStruct(DNAM, '', [
      wbInteger('DR', itS16, wbDiv(100)),
      wbInteger('Modulates Voice', itU16, wbBoolEnum)
    ]).SetRequired
  ]);
FNV:
  wbRecord(ARMA, 'Armor Addon', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbBMDT,
    wbRStruct('Male', [
      wbTexturedModel('Biped Model', [MODL, MODT], [wbMODS, wbMODD]),
      wbTexturedModel('World Model', [MOD2, MO2T], [wbMO2S, nil]),
      wbString(ICON, 'Icon Image'),
      wbString(MICO, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('Female', [
      wbTexturedModel('Biped Model', [MOD3, MO3T], [wbMO3S, wbMOSD]),
      wbTexturedModel('World Model', [MOD4, MO4T], [wbMO4S, nil]),
      wbString(ICO2, 'Icon Image'),
      wbString(MIC2, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbETYPReq,
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itS32),
      wbInteger('Max Condition', itS32),
      wbFloat('Weight')
    ], cpNormal, True),
    wbStruct(DNAM, '', [
      wbInteger('DR', itS16, wbDiv(100)),
      wbInteger('Flags', itU16, wbFlags([ // Only a byte or 2 distincts byte
        'Modulates Voice'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat('DT'),
      wbUnused(4)
    ], cpNormal, True, nil, 2)
  ]);
TES5:
  wbRecord(ARMA, 'Armor Addon', [
    wbEDID,
    wbBODTBOD2,
    wbFormIDCk(RNAM, 'Race', [RACE]),
    wbStruct(DNAM, 'Data', [
      wbInteger('Male Priority', itU8),
      wbInteger('Female Priority', itU8),
      // essentialy a number of world models for different weights (Enabled = 2 models _0.nif and _1.nif)
      wbInteger('Weight slider - Male', itU8, wbFlags([
        {0x01} 'Unknown 0',
        {0x02} 'Enabled'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Weight slider - Female', itU8, wbFlags([
        {0x01} 'Unknown 0',
        {0x02} 'Enabled'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbByteArray('Unknown', 2),
      wbInteger('Detection Sound Value', itU8),
      wbByteArray('Unknown', 1),
      wbFloat('Weapon Adjust')
    ], cpNormal, True),
    wbRStruct('Biped Model', [
      wbTexturedModel('Male', [MOD2, MO2T], [wbMO2S]),
      wbTexturedModel('Female', [MOD3, MO3T], [wbMO3S])
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('1st Person', [
      wbTexturedModel('Male', [MOD4, MO4T], [wbMO4S]),
      wbTexturedModel('Female', [MOD5, MO5T], [wbMO5S])
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbFormIDCK(NAM0, 'Male Skin Texture', [TXST, NULL]),
    wbFormIDCK(NAM1, 'Female Skin texture', [TXST, NULL]),
    wbFormIDCK(NAM2, 'Male Skin Texture Swap List', [FLST, NULL]),
    wbFormIDCK(NAM3, 'Female Skin Texture Swap List', [FLST, NULL]),
    wbRArrayS('Additional Races', wbFormIDCK(MODL, 'Race', [RACE, NULL])),
    wbFormIDCk(SNDD, 'Footstep Sound', [FSTS, NULL]),
    wbFormIDCk(ONAM, 'Art Object', [ARTO])
  ], False, nil, cpNormal, False, wbARMAAfterLoad).SetSummaryKey([4]);

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
FO3:
  wbRecord(ARMO, 'Armor',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      19, 'Has Platform Specific Textures'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbSCRI,
    wbEnchantment,
    wbBMDT,
    wbRStruct('Male', [
      wbTexturedModel('Biped Model', [MODL, MODT], [wbMODS, wbMODD]),
      wbTexturedModel('World Model', [MOD2, MO2T], [wbMO2S, nil]),
      wbString(ICON, 'Icon Image'),
      wbString(MICO, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('Female', [
      wbTexturedModel('Biped Model', [MOD3, MO3T], [wbMO3S, wbMOSD]),
      wbTexturedModel('World Model', [MOD4, MO4T], [wbMO4S, nil]),
      wbString(ICO2, 'Icon Image'),
      wbString(MIC2, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbString(BMCT, 'Ragdoll Constraint Template'),
    wbDEST,
    wbREPL,
    wbBIPL,
    wbETYPReq,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itS32),
      wbInteger('Max Condition', itS32),
      wbFloat('Weight')
    ]).SetRequired,
    wbStruct(DNAM, '', [
      wbInteger('DR', itS16, wbDiv(100)),
      wbInteger('Modulates Voice', itU16, wbBoolEnum)
    ]).SetRequired
  ]);
FNV:
  wbRecord(ARMO, 'Armor',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      19, 'Has Platform Specific Textures'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbSCRI,
    wbEnchantment,
    wbBMDT,
    wbRStruct('Male', [
      wbTexturedModel('Biped Model', [MODL, MODT], [wbMODS, wbMODD]),
      wbTexturedModel('World Model', [MOD2, MO2T], [wbMO2S, nil]),
      wbString(ICON, 'Icon Image'),
      wbString(MICO, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('Female', [
      wbTexturedModel('Biped Model', [MOD3, MO3T], [wbMO3S, wbMOSD]),
      wbTexturedModel('World Model', [MOD4, MO4T], [wbMO4S, nil]),
      wbString(ICO2, 'Icon Image'),
      wbString(MIC2, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbString(BMCT, 'Ragdoll Constraint Template'),
    wbDEST,
    wbREPL,
    wbBIPL,
    wbETYPReq,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itS32),
      wbInteger('Health', itS32),
      wbFloat('Weight')
    ], cpNormal, True),
    wbStruct(DNAM, '', [
      wbInteger('DR', itS16, wbDiv(100)),
      wbUnused(2),
      wbFloat('DT'),
      wbInteger('Flags', itU16, wbFlags([
        'Modulates Voice'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(2)
    ], cpNormal, True, nil, 2),
    wbInteger(BNAM, 'Overrides Animation Sounds', itU32, wbBoolEnum),
    wbRArray('Animation Sounds',
      wbStruct(SNAM, 'Animation Sound', [
        wbFormIDCk('Sound', [SOUN]),
        wbInteger('Chance', itU8),
        wbUnused(3),
        wbInteger('Type', itU32, wbEnum([], [
          19, 'Run',
          21, 'Run (Armor)',
          18, 'Sneak',
          20, 'Sneak (Armor)',
          17, 'Walk',
          22, 'Walk (Armor)'
        ]))
      ])
    ),
    wbFormIDCk(TNAM, 'Animation Sounds Template', [ARMO])
  ]);
TES5:
  wbRecord(ARMO, 'Armor',
    wbFlags(wbFlagsList([
      2, 'Non-Playable',
      6, 'Shield',
     10, 'Unknown 10',
     15, 'Visible When Distant'
    ])), [
    wbEDID,
    wbVMAD,
    wbOBND(True),
    wbFULL,
    wbEnchantment,
    wbRStruct('Male', [
      wbTexturedModel('World Model', [MOD2, MO2T], [wbMO2S]),
      wbString(ICON, 'Icon Image'),
      wbString(MICO, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbRStruct('Female', [
      wbTexturedModel('World Model', [MOD4, MO4T], [wbMO4S]),
      wbString(ICO2, 'Icon Image'),
      wbString(MIC2, 'Message Icon')
    ]).IncludeFlag(dfAllowAnyMember)
      .IncludeFlag(dfStructFirstNotRequired),
    wbBODTBOD2,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbString(BMCT, 'Ragdoll Constraint Template'),
    wbETYP,
    wbFormIDCk(BIDS, 'Bash Impact Data Set', [IPDS]),
    wbFormIDCk(BAMT, 'Alternate Block Material', [MATT]),
    wbFormIDCk(RNAM, 'Race', [RACE]),
    wbKeywords,
    wbDESC.SetRequired,
    wbRArray('Armature', wbFormIDCK(MODL, 'Model Filename', [ARMA, NULL])),
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ], cpNormal, True),
    wbInteger(DNAM, 'Armor Rating', itS32, wbDiv(100), cpNormal, True),
    wbFormIDCk(TNAM, 'Template Armor', [ARMO])
  ], False, nil, cpNormal, False, wbARMOAfterLoad);

# ARTO.Art Object

# ASPC.Acoustic Space
FO3:
  wbRecord(ASPC, 'Acoustic Space', [
    wbEDIDReq,
    wbOBND(True),
    wbFormIDCk(SNAM, 'Sound - Looping', [SOUN]),
    wbFormIDCk(RDAT, 'Use Sound from Region (Interiors Only)', [REGN]),
    wbInteger(ANAM, 'Environment Type', itU32,
      wbEnum([
        {0}  'None',
        {1}  'Default',
        {2}  'Generic',
        {3}  'Padded Cell',
        {4}  'Room',
        {5}  'Bathroom',
        {6}  'Livingroom',
        {7}  'Stone Room',
        {8}  'Auditorium',
        {9}  'Concerthall',
        {10} 'Cave',
        {11} 'Arena',
        {12} 'Hangar',
        {13} 'Carpeted Hallway',
        {14} 'Hallway',
        {15} 'Stone Corridor',
        {16} 'Alley',
        {17} 'Forest',
        {18} 'City',
        {19} 'Mountains',
        {20} 'Quarry',
        {21} 'Plain',
        {22} 'Parkinglot',
        {23} 'Sewerpipe',
        {24} 'Underwater',
        {25} 'Small Room',
        {26} 'Medium Room',
        {27} 'Large Room',
        {28} 'Medium Hall',
        {29} 'Large Hall',
        {30} 'Plate'
      ])).SetRequired
  ]);
FNV:
  wbRecord(ASPC, 'Acoustic Space', [
    wbEDIDReq,
    wbOBND(True),

    wbFormIDCk(SNAM, 'Dawn / Default Loop', [NULL, SOUN], False, cpNormal, True),
    wbFormIDCk(SNAM, 'Afternoon', [NULL, SOUN], False, cpNormal, True),
    wbFormIDCk(SNAM, 'Dusk', [NULL, SOUN], False, cpNormal, True),
    wbFormIDCk(SNAM, 'Night', [NULL, SOUN], False, cpNormal, True),
    wbFormIDCk(SNAM, 'Walla', [NULL, SOUN], False, cpNormal, True),

    wbInteger(WNAM, 'Walla Trigger Count', itU32, nil, cpNormal, True),
    wbFormIDCk(RDAT, 'Use Sound from Region (Interiors Only)', [REGN]),
    wbInteger(ANAM, 'Environment Type', itU32, wbEnum([
      'None',
      'Default',
      'Generic',
      'Padded Cell',
      'Room',
      'Bathroom',
      'Livingroom',
      'Stone Room',
      'Auditorium',
      'Concerthall',
      'Cave',
      'Arena',
      'Hangar',
      'Carpeted Hallway',
      'Hallway',
      'Stone Corridor',
      'Alley',
      'Forest',
      'City',
      'Mountains',
      'Quarry',
      'Plain',
      'Parkinglot',
      'Sewerpipe',
      'Underwater',
      'Small Room',
      'Medium Room',
      'Large Room',
      'Medium Hall',
      'Large Hall',
      'Plate'
    ]), cpNormal, True),
    wbInteger(INAM, 'Is Interior', itU32, wbBoolEnum, cpNormal, True)
  ]);

# ASTP.Association Type

# AVIF.Actor Values / Perk Tree Graphics
FO3:
  wbRecord(AVIF, 'ActorValue Information', [
    wbEDIDReq,
    wbFULL,
    wbDESCReq,
    wbICON,
    wbStringKC(ANAM, 'Short Name', 0, cpTranslate)
  ]);
FNV:
  wbRecord(AVIF, 'ActorValue Information', [
    wbEDIDReq,
    wbFULL,
    wbDESCReq,
    wbICON,
    wbStringKC(ANAM, 'Short Name', 0, cpTranslate)
  ]);

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
FO3:
  wbRecord(BOOK, 'Book',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDESCReq,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Scroll',
          {1} 'Can''t be Taken'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Skill', itS8, wbSkillEnum),
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ]).SetRequired
  ]);
FNV:
  wbRecord(BOOK, 'Book',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDESCReq,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU8, wbFlags([
        'Scroll',
        'Can''t be Taken'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Skill', itS8, wbSkillEnum),
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ], cpNormal, True)
  ]);
TES5:
  wbRecord(BOOK, 'Book', [
    wbEDID,
    wbVMAD,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbLStringKC(DESC, 'Book Text', 0, cpTranslate, True),
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbKeywords,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU8, wbFlags([
       {0x01} 'Teaches Skill',
       {0x02} 'Can''t be Taken',
       {0x04} 'Teaches Spell',
       {0x08} 'Unknown 4',
       {0x10} 'Unknown 5',
       {0x20} 'Unknown 6',
       {0x40} 'Unknown 7',
       {0x80} 'Unknown 8'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Type', itU8, wbEnum([], [
        0, 'Book/Tome', 255, 'Note/Scroll'
      ])),
      wbUnused(2),
      wbUnion('Teaches', wbBOOKTeachesDecider, [
        wbInteger('Skill', itS32, wbSkillEnum),
        wbFormIDCk('Spell', [SPEL])
      ]),
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ], cpNormal, True),
    wbFormIDCk(INAM, 'Inventory Art', [STAT]),
    wbLString(CNAM, 'Description', 0, cpTranslate)
  ]);

# BPTD.Body Part Data
FO3:
  wbRecord(BPTD, 'Body Part Data', [
    wbEDIDReq,
    wbGenericModel(True),
    wbRArrayS('Body Parts',
      wbRStructSK([1], 'Body Part', [
        wbString(BPTN, 'Part Name'),
        wbString(BPNN, 'Part Node').SetRequired,
        wbString(BPNT, 'VATS Target').SetRequired,
        wbString(BPNI, 'IK Data - Start Node').SetRequired,
        wbStruct(BPND, 'Node Data', [
          wbFloat('Damage Mult'),
          wbInteger('Flags', itU8,
            wbFlags([
              {0} 'Severable',
              {1} 'IK Data',
              {2} 'IK Data - Biped Data',
              {3} 'Explodable',
              {4} 'IK Data - Is Head',
              {5} 'IK Data - Headtracking',
              {6} 'To Hit Chance - Absolute'
            ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
          wbInteger('Part Type', itS8, wbBodyLocationEnum),
          wbInteger('Health Percent', itU8),
          wbInteger('Actor Value', itS8, wbActorValueEnum),
          wbInteger('To Hit Chance', itU8),
          wbInteger('Explodable - Explosion Chance %', itU8),
          wbInteger('Explodable - Debris Count', itU16),
          wbFormIDCk('Explodable - Debris', [DEBR, NULL]),
          wbFormIDCk('Explodable - Explosion', [EXPL, NULL]),
          wbFloat('Tracking Max Angle'),
          wbFloat('Explodable - Debris Scale'),
          wbInteger('Severable - Debris Count', itS32),
          wbFormIDCk('Severable - Debris', [DEBR, NULL]),
          wbFormIDCk('Severable - Explosion', [EXPL, NULL]),
          wbFloat('Severable - Debris Scale'),
          wbVec3PosRot('Gore Effects Positioning'),
          wbFormIDCk('Severable - Impact DataSet', [IPDS, NULL]),
          wbFormIDCk('Explodable - Impact DataSet', [IPDS, NULL]),
          wbInteger('Severable - Decal Count', itU8),
          wbInteger('Explodable - Decal Count', itU8),
          wbUnused(2),
          wbFloat('Limb Replacement Scale')
        ]).SetRequired,
        wbString(NAM1, 'Limb Replacement Model').SetRequired,
        wbString(NAM4, 'Gore Effects - Target Bone').SetRequired,
        wbModelInfo(NAM5)
      ]).SetSummaryKey([1])
        .IncludeFlag(dfAllowAnyMember)
        .IncludeFlag(dfSummaryMembersNoName)
        .IncludeFlag(dfSummaryNoSortKey)
        .IncludeFlag(dfStructFirstNotRequired)
    ).SetRequired,
    wbFormIDCk(RAGA, 'Ragdoll', [RGDL])
  ]).SetSummaryKey([1])
    .IncludeFlag(dfSummaryMembersNoName);
FNV:
  wbRecord(BPTD, 'Body Part Data', [
    wbEDIDReq,
    wbGenericModel(True),
    wbRArrayS('Body Parts',
      wbRStructSK([1], 'Body Part', [
        wbString(BPTN, 'Part Name'),
        wbString(BPNN, 'Part Node').SetRequired,
        wbString(BPNT, 'VATS Target').SetRequired,
        wbString(BPNI, 'IK Data - Start Node').SetRequired,
        wbStruct(BPND, 'Node Data', [
          wbFloat('Damage Mult'),
          wbInteger('Flags', itU8,
            wbFlags([
              {0} 'Severable',
              {1} 'IK Data',
              {2} 'IK Data - Biped Data',
              {3} 'Explodable',
              {4} 'IK Data - Is Head',
              {5} 'IK Data - Headtracking',
              {6} 'To Hit Chance - Absolute'
            ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
          wbInteger('Part Type', itS8, wbBodyLocationEnum),
          wbInteger('Health Percent', itU8),
          wbInteger('Actor Value', itS8, wbActorValueEnum),
          wbInteger('To Hit Chance', itU8),
          wbInteger('Explodable - Explosion Chance %', itU8),
          wbInteger('Explodable - Debris Count', itU16),
          wbFormIDCk('Explodable - Debris', [DEBR, NULL]),
          wbFormIDCk('Explodable - Explosion', [EXPL, NULL]),
          wbFloat('Tracking Max Angle'),
          wbFloat('Explodable - Debris Scale'),
          wbInteger('Severable - Debris Count', itS32),
          wbFormIDCk('Severable - Debris', [DEBR, NULL]),
          wbFormIDCk('Severable - Explosion', [EXPL, NULL]),
          wbFloat('Severable - Debris Scale'),
          wbVec3PosRot('Gore Effects Positioning'),
          wbFormIDCk('Severable - Impact DataSet', [IPDS, NULL]),
          wbFormIDCk('Explodable - Impact DataSet', [IPDS, NULL]),
          wbInteger('Severable - Decal Count', itU8),
          wbInteger('Explodable - Decal Count', itU8),
          wbUnused(2),
          wbFloat('Limb Replacement Scale')
        ]).SetRequired,
        wbString(NAM1, 'Limb Replacement Model').SetRequired,
        wbString(NAM4, 'Gore Effects - Target Bone').SetRequired,
        wbModelInfo(NAM5)
      ]).SetSummaryKey([1])
        .IncludeFlag(dfAllowAnyMember)
        .IncludeFlag(dfSummaryMembersNoName)
        .IncludeFlag(dfSummaryNoSortKey)
        .IncludeFlag(dfStructFirstNotRequired)
    ).SetRequired,
    wbFormIDCk(RAGA, 'Ragdoll', [RGDL])
  ]).SetSummaryKey([1])
    .IncludeFlag(dfSummaryMembersNoName);

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
FO3:
  wbRecord(CAMS, 'Camera Shot', [
    wbEDIDReq,
    wbGenericModel,
    wbStruct(DATA, 'Data', [
      {00} wbInteger('Action', itU32,
             wbEnum([
               {0} 'Shoot',
               {1} 'Fly',
               {2} 'Hit',
               {3} 'Zoom'
             ])),
      {04} wbInteger('Location', itU32,
             wbEnum([
               {0} 'Attacker',
               {1} 'Projectile',
               {2} 'Target'
             ])),
      {08} wbInteger('Target', itU32,
             wbEnum([
               {0} 'Attacker',
               {1} 'Projectile',
               {2} 'Target'
             ])),
      {12} wbInteger('Flags', itU32,
             wbFlags([
               {0} 'Position Follows Location',
               {1} 'Rotation Follows Target',
               {2} 'Don''t Follow Bone',
               {3} 'First Person Camera',
               {4} 'No Tracer',
               {5} 'Start At Time Zero'
             ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbStruct('Time Multipliers', [
        {16} wbFloat('Player'),
        {20} wbFloat('Target'),
        {24} wbFloat('Global')
      ]),
      {28} wbFloat('Max Time'),
      {32} wbFloat('Min Time'),
      {36} wbFloat('Target % Between Actors')
    ], cpNormal, True, nil, 7),
    wbFormIDCk(MNAM, 'Image Space Modifier', [IMAD])
  ]);
FNV:
  wbRecord(CAMS, 'Camera Shot', [
    wbEDIDReq,
    wbGenericModel,
    wbStruct(DATA, 'Data', [
      {00} wbInteger('Action', itU32, wbEnum([
        'Shoot',
        'Fly',
        'Hit',
        'Zoom'
      ])),
      {04} wbInteger('Location', itU32, wbEnum([
        'Attacker',
        'Projectile',
        'Target'
      ])),
      {08} wbInteger('Target', itU32, wbEnum([
        'Attacker',
        'Projectile',
        'Target'
      ])),
      {12} wbInteger('Flags', itU32, wbFlags([
        'Position Follows Location',
        'Rotation Follows Target',
        'Don''t Follow Bone',
        'First Person Camera',
        'No Tracer',
        'Start At Time Zero'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbStruct('Time Multipliers', [
        {16} wbFloat('Player'),
        {20} wbFloat('Target'),
        {24} wbFloat('Global')
      ]),
      {28} wbFloat('Max Time'),
      {32} wbFloat('Min Time'),
      {36} wbFloat('Target % Between Actors')
    ], cpNormal, True, nil, 7),
    wbFormIDCk(MNAM, 'Image Space Modifier', [IMAD])
  ]);

# CCRD.Caravan Card
FNV:
  wbRecord(CCRD, 'Caravan Card', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbYNAM,
    wbZNAM,
    wbRStruct('High Res Image', [
      wbString(TX00, 'Face'),
      wbString(TX01, 'Back')
    ]),
    wbRStruct('Card', [
      wbInteger(INTV, 'Suit', itU32, wbEnum([
        '',
        'Hearts',
        'Spades',
        'Diamonds',
        'Clubs',
        'Joker'
      ])),
      wbInteger(INTV, 'Value', itU32, wbEnum([
        '',
        'Ace',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        '10',
        '',
        'Jack',
        'Queen',
        'King',
        'Joker'
      ]))
    ]),
    wbInteger(DATA, 'Value', itU32)
  ]);

# CDCK.Caravan Deck
FNV:
  wbRecord(CDCK, 'Caravan Deck', [
    wbEDIDReq,
    wbFULL,
    wbRArrayS('Cards',
      wbFormIDCk(CARD, 'Card', [CCRD])
    ),
    wbInteger(DATA, 'Count (broken)', itU32)
  ]);

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
FO3:
  wbRecord(CELL, 'Cell',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      17, 'Off Limits',
      19, 'Can''t Wait'
    ])), [
    wbEDID,
    wbFULL,
    wbInteger(DATA, 'Flags', itU8,
      wbFlags([
        {0} 'Is Interior Cell',
        {1} 'Has water',
        {2} 'Can Travel From Here',
        {3} 'No LOD Water',
        {5} 'Public Area',
        {6} 'Hand changed',
        {7} 'Behave like exterior'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbCellGrid,
    wbStruct(XCLL, 'Lighting', [
      wbByteColors('Ambient Color'),
      wbByteColors('Directional Color'),
      wbByteColors('Fog Color'),
      wbFloat('Fog Near'),
      wbFloat('Fog Far'),
      wbInteger('Directional Rotation XY', itS32),
      wbInteger('Directional Rotation Z', itS32),
      wbFloat('Directional Fade'),
      wbFloat('Fog Clip Dist'),
      wbFloat('Fog Power')
    ], cpNormal, False, nil, 7)
      .SetDontShow(wbCellExteriorDontShow)
      .SetIsRemovable(wbCellLightingIsRemovable),
    wbArray(IMPF, 'Footstep Materials', wbString('Unknown', 30), [
      'ConcSolid',
      'ConcBroken',
      'MetalSolid',
      'MetalHollow',
      'MetalSheet',
      'Wood',
      'Sand',
      'Dirt',
      'Grass',
      'Water'
    ]),
    wbRStruct('Light Template', [
      wbFormIDCk(LTMP, 'Template', [LGTM, NULL]),
      wbInteger(LNAM, 'Inherit', itU32,
        wbFlags([
          {0} 'Ambient Color',
          {1} 'Directional Color',
          {2} 'Fog Color',
          {3} 'Fog Near',
          {4} 'Fog Far',
          {5} 'Directional Rotation',
          {6} 'Directional Fade',
          {7} 'Clip Distance',
          {8} 'Fog Power'
        ])).SetRequired
           .IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbFloat(XCLW, 'Water Height'),
    wbString(XNAM, 'Water Noise Texture'),
    wbArrayS(XCLR, 'Regions', wbFormIDCk('Region', [REGN])),
    wbFormIDCk(XCIM, 'Image Space', [IMGS]),
    wbUnused(XCET, 1),
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),
    wbFormIDCk(XCCM, 'Climate', [CLMT]),
    wbFormIDCk(XCWT, 'Water', [WATR]),
    wbOwnership([XCMT, XCMO]),
    wbFormIDCk(XCAS, 'Acoustic Space', [ASPC]),
    wbByteArray(XCMT, 'Unused', 1, cpIgnore),
    wbFormIDCk(XCMO, 'Music Type', [MUSC])
  ], True)
    .SetAddInfo(wbCellAddInfo)
    .SetAfterLoad(wbCELLAfterLoad);
FNV:
  wbRecord(CELL, 'Cell',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      17, 'Off Limits',
      19, 'Can''t Wait'
    ])), [
    wbEDID,
    wbFULL,
    wbInteger(DATA, 'Flags', itU8, wbFlags([
      {0x01} 'Is Interior Cell',
      {0x02} 'Has water',
      {0x04} 'Invert Fast Travel behavior',
      {0x08} 'No LOD Water',
      {0x10} '',
      {0x20} 'Public place',
      {0x40} 'Hand changed',
      {0x80} 'Behave like exterior'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbCellGrid,
    wbStruct(XCLL, 'Lighting', [
      wbByteColors('Ambient Color'),
      wbByteColors('Directional Color'),
      wbByteColors('Fog Color'),
      wbFloat('Fog Near'),
      wbFloat('Fog Far'),
      wbInteger('Directional Rotation XY', itS32),
      wbInteger('Directional Rotation Z', itS32),
      wbFloat('Directional Fade'),
      wbFloat('Fog Clip Dist'),
      wbFloat('Fog Power')
    ], cpNormal, False, nil, 7)
      .SetDontShow(wbCellExteriorDontShow)
      .SetIsRemovable(wbCellLightingIsRemovable),
    wbArray(IMPF, 'Footstep Materials', wbString('Unknown', 30), [
      'ConcSolid',
      'ConcBroken',
      'MetalSolid',
      'MetalHollow',
      'MetalSheet',
      'Wood',
      'Sand',
      'Dirt',
      'Grass',
      'Water'
    ]),
    wbRStruct('Light Template', [
      wbFormIDCk(LTMP, 'Template', [LGTM, NULL]),
      wbInteger(LNAM, 'Inherit', itU32, wbFlags([
        {0x00000001}'Ambient Color',
        {0x00000002}'Directional Color',
        {0x00000004}'Fog Color',
        {0x00000008}'Fog Near',
        {0x00000010}'Fog Far',
        {0x00000020}'Directional Rotation',
        {0x00000040}'Directional Fade',
        {0x00000080}'Clip Distance',
        {0x00000100}'Fog Power'
      ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], [], cpNormal, True ),
    wbFloat(XCLW, 'Water Height'),
    wbString(XNAM, 'Water Noise Texture'),
    wbArrayS(XCLR, 'Regions', wbFormIDCk('Region', [REGN])),
    wbFormIDCk(XCIM, 'Image Space', [IMGS]),
    wbUnused(XCET, 1),
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),
    wbFormIDCk(XCCM, 'Climate', [CLMT]),
    wbFormIDCk(XCWT, 'Water', [WATR]),
    wbOwnership([XCMT, XCMO]),
    wbFormIDCk(XCAS, 'Acoustic Space', [ASPC]),
    wbByteArray(XCMT, 'Unused', 1, cpIgnore),
    wbFormIDCk(XCMO, 'Music Type', [MUSC])
  ], True)
    .SetAddInfo(wbCellAddInfo)
    .SetAfterLoad(wbCELLAfterLoad);
TES5:
  wbRecord(CELL, 'Cell',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      14, 'Partial Form',
      17, 'Off Limits',
      18, 'Compressed',
      19, 'Can''t Wait'
    ]), [14, 18])
      .SetFlagHasDontShow(14, wbFlagPartialFormDontShow),
  [
    wbEDID,
    wbFULL,
    {>>>
    Flags can be itU8, but CELL\DATA has a critical role in various wbImplementation.pas routines
    and replacing it with wbUnion generates error when setting for example persistent flag in REFR.
    So let it always be an integer
    <<<}
    wbInteger(DATA, 'Flags', itU16,
      wbFlags(wbSparseFlags([
      0, 'Is Interior Cell',
      1, 'Has Water',
      2, 'Can Travel From Here',
      3, 'No LOD Water',
      5, 'Public Area',
      6, 'Hand Changed',
      7, 'Show Sky',
      8, 'Use Sky Lighting',
      15, IsCS('Sunlight Shadows', '')
      ], False, 16))
    ).SetAfterSet(wbCELLDATAAfterSet)
     .SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbCellGrid,
    wbStruct(XCLL, 'Lighting', [
      wbByteColors('Ambient Color'),
      wbByteColors('Directional Color'),
      wbByteColors('Fog Color Near'),
      wbFloat('Fog Near'),
      wbFloat('Fog Far'),
      wbInteger('Directional Rotation XY', itS32),
      wbInteger('Directional Rotation Z', itS32),
      wbFloat('Directional Fade'),
      wbFloat('Fog Clip Distance'),
      wbFloat('Fog Power'),
      wbAmbientColors('Ambient Colors'),
      wbByteColors('Fog Color Far'),
      wbFloat('Fog Max'),
      wbFloat('Light Fade Begin'),
      wbFloat('Light Fade End'),
      wbInteger('Inherits', itU32, wbFlags([
        {0x00000001}'Ambient Color',
        {0x00000002}'Directional Color',
        {0x00000004}'Fog Color',
        {0x00000008}'Fog Near',
        {0x00000010}'Fog Far',
        {0x00000020}'Directional Rotation',
        {0x00000040}'Directional Fade',
        {0x00000080}'Clip Distance',
        {0x00000100}'Fog Power',
        {0x00000200}'Fog Max',
        {0x00000400}'Light Fade Distances'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, False, nil, 11)
      .SetDontShow(wbCellExteriorDontShow)
      .SetIsRemovable(wbCellLightingIsRemovable),

    wbTVDT,
    wbMHDTCELL,
    wbFormIDCk(LTMP, 'Lighting Template', [LGTM, NULL], False, cpNormal, True),
    wbByteArray(LNAM, 'Unknown', 0, cpIgnore), // leftover flags, they are now in XCLC

    {>>> XCLW sometimes has $FF7FFFFF and causes invalid floation point <<<}
    wbFloat(XCLW, 'Water Height', cpNormal, False, 1, -1, nil, nil, 0, wbCELLXCLWGetConflictPriority),
    //wbByteArray(XCLW, 'Water Height', 4),
    wbString(XNAM, 'Water Noise Texture'),
    wbArrayS(XCLR, 'Regions', wbFormIDCk('Region', [REGN])),
    wbFormIDCk(XLCN, 'Location', [LCTN]),
    wbRStruct('Water Current Velocities', [
      wbRUnion('', [
        wbInteger(XWCN, 'Velocity Count', itU32, nil, cpBenign),
        wbInteger(XWCS, 'Velocity Count', itU32, nil, cpBenign)
      ]).IncludeFlag(dfUnionStaticResolve),
      wbArray(XWCU, 'Velocities',
        wbStruct('Current', [
          wbVec3('Velocity'),
          wbFloat
        ])
      ).SetCountPathOnValue('[0]', False)
       .SetRequired
       .IncludeFlag(dfCollapsed, wbCollapseOther)
       .IncludeFlag(dfNotAlignable)
    ]),
    wbFormIDCk(XCWT, 'Water', [WATR]),
    wbOwnership([XRGD]),
    wbFormIDCk(XILL, 'Lock List', [FLST, NPC_]),
    wbString(XWEM, 'Water Environment Map').SetDontShow(wbCellExteriorDontShow),
    wbFormIDCk(XCCM, 'Sky/Weather from Region', [REGN]),
    wbFormIDCk(XCAS, 'Acoustic Space', [ASPC]),
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),
    wbFormIDCk(XCMO, 'Music Type', [MUSC]),
    wbFormIDCk(XCIM, 'Image Space', [IMGS])
  ], True)
    .SetAddInfo(wbCellAddInfo)
    .SetAfterLoad(wbCELLAfterLoad);

# CHAL.Challenge
FNV:
  wbRecord(CHAL, 'Challenge', [
    wbEDIDReq,
    wbFULL,
    wbICON,
    wbSCRI,
    wbDESC,
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU32, wbEnum([
        {00} 'Kill from a Form List',
        {01} 'Kill a specific FormID',
        {02} 'Kill any in a category',
        {03} 'Hit an Enemy',
        {04} 'Discover a Map Marker',
        {05} 'Use an Item',
        {06} 'Acquire an Item',
        {07} 'Use a Skill',
        {08} 'Do Damage',
        {09} 'Use an Item from a List',
        {10} 'Acquire an Item from a List',
        {11} 'Miscellaneous Stat',
        {12} 'Craft Using an Item',
        {13} 'Scripted Challenge'
      ])),
      wbInteger('Threshold', itU32),
      wbInteger('Flags', itU32, wbFlags([
        'Start Disabled',
        'Recurring',
        'Show Zero Progress'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Interval', itU32),
      wbByteArray('(depends on type)', 2),
      wbByteArray('(depends on type)', 2),
      wbByteArray('(depends on type)', 4)
    ]),
    wbFormID(SNAM, '(depends on type)'),
    wbFormID(XNAM, '(depends on type)')
  ]);

# CHIP.Casino Chip
FNV:
  wbRecord(CHIP, 'Casino Chip', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbDEST,
    wbYNAM,
    wbZNAM
  ]);

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
FO3:
  wbRecord(CLAS, 'Class', [
    wbEDIDReq,
    wbFULLReq,
    wbDESCReq,
    wbICON,
    wbStruct(DATA, '', [
      wbArray('Tag Skills', wbInteger('Tag Skill', itS32, wbActorValueEnum), 4),
      wbInteger('Flags', itU32,
        wbFlags([
          {0} 'Playable',
          {1} 'Guard'
        ], True)
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Buys/Sells and Services', itU32, wbServiceFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Teaches', itS8, wbSkillEnum),
      wbInteger('Maximum training level', itU8),
      wbUnused(2)
    ]).SetRequired,
    wbArray(ATTR, 'Attributes', wbInteger('Attribute', itU8), [
      'Strength',
      'Perception',
      'Endurance',
      'Charisma',
      'Intelligence',
      'Agility',
      'Luck'
    ]).SetRequired
  ]);
FNV:
  wbRecord(CLAS, 'Class', [
    wbEDIDReq,
    wbFULLReq,
    wbDESCReq,
    wbICON,
    wbStruct(DATA, '', [
      wbArray('Tag Skills', wbInteger('Tag Skill', itS32, wbActorValueEnum), 4),
      wbInteger('Flags', itU32, wbFlags(['Playable', 'Guard'], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Buys/Sells and Services', itU32, wbServiceFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Teaches', itS8, wbSkillEnum),
      wbInteger('Maximum training level', itU8),
      wbUnused(2)
    ], cpNormal, True),
    wbArray(ATTR, 'Attributes', wbInteger('Attribute', itU8), [
      'Strength',
      'Perception',
      'Endurance',
      'Charisma',
      'Intelligence',
      'Agility',
      'Luck'
    ], cpNormal, True)
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
FO3:
  wbRecord(CLMT, 'Climate', [
    wbEDIDReq,
    wbArrayS(WLST, 'Weather Types', wbStructSK([0], 'Weather Type', [
      wbFormIDCk('Weather', [WTHR, NULL]),
      wbInteger('Chance', itS32),
      wbFormIDCk('Global', [GLOB, NULL])
    ])),
    wbString(FNAM, 'Sun Texture'),
    wbString(GNAM, 'Sun Glare Texture'),
    wbGenericModel,
    wbClimateTiming(wbClmtTime, wbClmtMoonsPhaseLength)
  ]);
FNV:
  wbRecord(CLMT, 'Climate', [
    wbEDIDReq,
    wbArrayS(WLST, 'Weather Types', wbStructSK([0], 'Weather Type', [
      wbFormIDCk('Weather', [WTHR, NULL]),
      wbInteger('Chance', itS32),
      wbFormIDCk('Global', [GLOB, NULL])
    ])),
    wbString(FNAM, 'Sun Texture'),
    wbString(GNAM, 'Sun Glare Texture'),
    wbGenericModel,
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

# CMNY.Caravan Money
FNV:
  wbRecord(CMNY, 'Caravan Money', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbYNAM,
    wbZNAM,
    wbInteger(DATA, 'Absolute Value', itU32)
  ]);

# COBJ.Constructible Object
FO3:
  wbRecord(COBJ, 'Constructible Object', [
    wbEDID,
    wbOBND,
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ]).SetRequired
  ]);
FNV:
  wbRecord(COBJ, 'Constructible Object', [
    wbEDID,
    wbOBND,
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ], cpNormal, True)
  ]);

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
FO3:
  wbRecord(CONT, 'Container',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start',
      25, 'Obstacle',
      26, 'Navmesh - Filter',
      27, 'Navmesh - Bounding Box',
      30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbSCRI,
    wbCNTOs,
    wbDEST,
    wbStruct(DATA, '', [
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          1, 'Respawns'
        ], False, 2)
      )).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat('Weight')
    ]).SetRequired,
    wbFormIDCk(SNAM, 'Sound - Open', [SOUN]),
    wbFormIDCk(QNAM, 'Sound - Close', [SOUN])
  ], True);
FNV:
  wbRecord(CONT, 'Container',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start',
      25, 'Obstacle',
      26, 'Navmesh - Filter',
      27, 'Navmesh - Bounding Box',
      30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbSCRI,
    wbCNTOs,
    wbDEST,
    wbStruct(DATA, '', [
      wbInteger('Flags', itU8, wbFlags(['', 'Respawns'])),
      wbFloat('Weight')
    ], cpNormal, True),
    wbFormIDCk(SNAM, 'Sound - Open', [SOUN]),
    wbFormIDCk(QNAM, 'Sound - Close', [SOUN]),
    wbFormIDCk(RNAM, 'Sound - Random/Looping', [SOUN])
  ], True);

# CPTH.Camera Path
FO3:
  wbRecord(CPTH, 'Camera Path', [
    wbEDIDReq,
    wbConditions,
    wbStruct(ANAM, 'Camera Paths', [
      wbFormIDCk('Parent', [CPTH, NULL], False, cpBenign),
      wbFormIDCk('Previous', [CPTH, NULL], False, cpBenign)
    ]).SetRequired,
    wbInteger(DATA, 'Camera Zoom', itU8,
      wbEnum([
        {0} 'Default',
        {1} 'Disable',
        {2} 'Shot List'
      ])).SetRequired,
    wbRArray('Camera Shots', wbFormIDCk(SNAM, 'Camera Shot', [CAMS]))
  ]);
FNV:
  wbRecord(CPTH, 'Camera Path', [
    wbEDIDReq,
    wbConditions,
    wbStruct(ANAM, 'Camera Paths', [
      wbFormIDCk('Parent', [CPTH, NULL], False, cpBenign),
      wbFormIDCk('Previous', [CPTH, NULL], False, cpBenign)
    ]).SetRequired,
    wbInteger(DATA, 'Camera Zoom', itU8, wbEnum([
      'Default',
      'Disable',
      'Shot List'
    ]), cpNormal, True),
    wbRArray('Camera Shots', wbFormIDCk(SNAM, 'Camera Shot', [CAMS]))
  ]);

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
FO3:
  wbRecord(CREA, 'Creature',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      19, 'Unknown 19',
      29, 'Unknown 29'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL.SetDontShow(wbActorTemplateUseBaseData),
    wbGenericModel(False, wbActorTemplateUseModelAnimation),
    wbSPLOs,
    wbFormIDCk(EITM, 'Unarmed Attack Effect', [ENCH, SPEL]).SetDontShow(wbActorTemplateUseActorEffectList),
    wbInteger(EAMT, 'Unarmed Attack Animation', itU16, wbAttackAnimationEnum)
      .SetDontShow(wbActorTemplateUseActorEffectList)
      .SetRequired,
    wbArrayS(NIFZ, 'Model List', wbStringLC('Model')).SetDontShow(wbActorTemplateUseModelAnimation),
    wbModelInfos(NIFT, 'Model List Textures').SetDontShow(wbActorTemplateUseModelAnimation),
    wbStruct(ACBS, 'Configuration', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Biped',
          1,  'Essential',
          2,  'Weapon & Shield?',
          3,  'Respawn',
          4,  'Swims',
          5,  'Flies',
          6,  'Walks',
          7,  'PC Level Mult',
          9,  'No Low Level Processing',
          11, 'No Blood Spray',
          12, 'No Blood Decal',
          15, 'No Head',
          16, 'No Right Arm',
          17, 'No Left Arm',
          18, 'No Combat in Water',
          19, 'No Shadow',
          20, 'No VATS Melee',
          21, 'Allow PC Dialogue',
          22, 'Can''t Open Doors',
          23, 'Immobile',
          24, 'Tilt Front/Back',
          25, 'Tilt Left/Right',
          26, 'No Knockdowns',
          27, 'Not Pushable',
          28, 'Allow Pickpocket',
          29, 'Is Ghost',
          30, 'No Rotating To Head-track',
          31, 'Invulnerable'
        ])).SetFlagHasDontShow(0,  wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(1,  wbActorTemplateUseBaseData)
           .SetFlagHasDontShow(3,  wbActorTemplateUseBaseData)
           .SetFlagHasDontShow(4,  wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(5,  wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(6,  wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(7,  wbActorTemplateUseStats)
           .SetFlagHasDontShow(9,  wbActorTemplateUseBaseData)
           .SetFlagHasDontShow(10, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(11, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(15, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(16, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(17, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(18, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(19, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(21, wbActorTemplateUseBaseData)
           .SetFlagHasDontShow(22, wbActorTemplateUseBaseData)
           .SetFlagHasDontShow(23, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(24, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(25, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(27, wbActorTemplateUseModelAnimation)
           .SetFlagHasDontShow(28, wbActorTemplateUseBaseData)
           .SetFlagHasDontShow(30, wbActorTemplateUseModelAnimation)
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {04} wbInteger('Fatigue', itU16).SetDontShow(wbActorTemplateUseStats),
      {06} wbInteger('Barter gold', itU16).SetDontShow(wbActorTemplateUseAIData),
      {08} wbUnion('Level', wbACBSLevelDecider, [
             wbInteger('Level', itU16),//.SetDontShow(wbActorTemplateUseStats),
             wbInteger('Level Mult', itU16, wbDiv(1000, 2))
               .SetAfterLoad(wbACBSLevelMultAfterLoad)
               .SetDefaultNativeValue(1000)
               //.SetDontShow(wbActorTemplateUseStats)
           ]).SetAfterSet(wbACBSLevelMultAfterSet)
             .SetDontShow(wbActorTemplateUseStats),
      {10} wbInteger('Calc min', itU16).SetDontShow(wbActorTemplateUseStats),
      {12} wbInteger('Calc max', itU16).SetDontShow(wbActorTemplateUseStats),
      {14} wbInteger('Speed Multiplier', itU16).SetDontShow(wbActorTemplateUseStats),
      {16} wbFloat('Karma (Alignment)').SetDontShow(wbActorTemplateUseTraits),
      {20} wbInteger('Disposition Base', itS16).SetDontShow(wbActorTemplateUseTraits),
      {22} wbInteger('Template Flags', itU16, wbTemplateFlags).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbRArrayS('Factions', wbFaction).SetDontShow(wbActorTemplateUseFactions),
    wbFormIDCk(INAM, 'Death item', [LVLI]).SetDontShow(wbActorTemplateUseTraits),
    wbFormIDCk(VTCK, 'Voice', [VTYP]).SetDontShow(wbActorTemplateUseTraits),
    wbFormIDCk(TPLT, 'Template', [CREA, LVLC]),
    wbDEST.SetDontShow(wbActorTemplateUseModelAnimation),
    wbSCRI.SetDontShow(wbActorTemplateUseScript),
    wbCNTOs.SetDontShow(wbActorTemplateUseInventory),
    wbAIDT,
    wbRArray('Packages', wbFormIDCk(PKID, 'Package', [PACK])).SetDontShow(wbActorTemplateUseAIPackages),
    wbArrayS(KFFZ, 'Animations', wbStringLC('Animation')).SetDontShow(wbActorTemplateUseModelAnimation),
    wbStruct(DATA, '', [
      {00} wbInteger('Type', itU8, wbCreatureTypeEnum).SetDontShow(wbActorTemplateUseTraits),
      {01} wbInteger('Combat Skill', itU8).SetDontShow(wbActorTemplateUseStats),
      {02} wbInteger('Magic Skill', itU8).SetDontShow(wbActorTemplateUseStats),
      {03} wbInteger('Stealth Skill', itU8).SetDontShow(wbActorTemplateUseStats),
      {04} wbInteger('Health', itS16).SetDontShow(wbActorTemplateUseStats),
      {06} wbUnused(2),
      {08} wbInteger('Damage', itS16).SetDontShow(wbActorTemplateUseStats),
      {10} wbArray('Attributes', wbInteger('Attribute', itU8), [
            'Strength',
            'Perception',
            'Endurance',
            'Charisma',
            'Intelligence',
            'Agility',
            'Luck'
          ]).SetDontShow(wbActorTemplateUseStats)
    ]).SetRequired,
    wbInteger(RNAM, 'Attack reach', itU8)
      .SetDontShow(wbActorTemplateUseTraits)
      .SetRequired,
    wbFormIDCk(ZNAM, 'Combat Style', [CSTY]).SetDontShow(wbActorTemplateUseTraits),
    wbFormIDCk(PNAM, 'Body Part Data', [BPTD]).SetDontShow(wbActorTemplateUseModelAnimation).SetRequired,
    wbFloat(TNAM, 'Turning Speed')
      .SetDontShow(wbActorTemplateUseStats)
      .SetRequired,
    wbFloat(BNAM, 'Base Scale')
      .SetDontShow(wbActorTemplateUseStats)
      .SetRequired,
    wbFloat(WNAM, 'Foot Weight')
      .SetDontShow(wbActorTemplateUseStats)
      .SetRequired,
    wbInteger(NAM4, 'Impact Material Type', itU32, wbActorImpactMaterialEnum)
      .SetDontShow(wbActorTemplateUseModelAnimation)
      .SetRequired,
    wbInteger(NAM5, 'Sound Level', itU32, wbSoundLevelEnum)
      .SetDontShow(wbActorTemplateUseModelAnimation)
      .SetRequired,
    wbFormIDCk(CSCR, 'Inherits Sounds from', [CREA]).SetDontShow(wbActorTemplateUseModelAnimation),
    wbRArrayS('Sound Types',
      wbRStructSK([0], 'Sound Type', [
        wbInteger(CSDT, 'Type', itU32,
          wbEnum([
            {0}  'Left Foot',
            {1}  'Right Foot',
            {2}  'Left Back Foot',
            {3}  'Right Back Foot',
            {4}  'Idle',
            {5}  'Aware',
            {6}  'Attack',
            {7}  'Hit',
            {8}  'Death',
            {9}  'Weapon',
            {10} 'Movement',
            {11} 'Conscious'
          ])),
        wbSoundTypeSounds
      ])).SetDontShow(wbActorTemplateUseModelAnimation),
    wbFormIDCk(CNAM, 'Impact Dataset', [IPDS]).SetDontShow(wbActorTemplateUseModelAnimation),
    wbFormIDCk(LNAM, 'Melee Weapon List', [FLST]).SetDontShow(wbActorTemplateUseTraits)
  ], True);
FNV:
  wbRecord(CREA, 'Creature',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      19, 'Unknown 19',
      29, 'Unknown 29'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULLActor,
    wbGenericModel(False, wbActorTemplateUseModelAnimation),
    wbSPLOs,
    wbFormIDCk(EITM, 'Unarmed Attack Effect', [ENCH, SPEL], False, cpNormal, False, wbActorTemplateUseActorEffectList),
    wbInteger(EAMT, 'Unarmed Attack Animation', itU16, wbAttackAnimationEnum, cpNormal, True, False, wbActorTemplateUseActorEffectList),
    wbArrayS(NIFZ, 'Model List', wbStringLC('Model'), 0, cpNormal, False, nil, nil, wbActorTemplateUseModelAnimation),
    wbModelInfos(NIFT, 'Model List Textures', wbActorTemplateUseModelAnimation),
    wbStruct(ACBS, 'Configuration', [
      {00} wbInteger('Flags', itU32, wbFlags([
             {0x000001} 'Biped',
             {0x000002} 'Essential',
             {0x000004} 'Weapon & Shield?',
             {0x000008} 'Respawn',
             {0x000010} 'Swims',
             {0x000020} 'Flies',
             {0x000040} 'Walks',
             {0x000080} 'PC Level Mult',
             {0x000100} 'Unknown 8',
             {0x000200} 'No Low Level Processing',
             {0x000400} '',
             {0x000800} 'No Blood Spray',
             {0x001000} 'No Blood Decal',
             {0x002000} '',
             {0x004000} '',
             {0x008000} 'No Head',
             {0x010000} 'No Right Arm',
             {0x020000} 'No Left Arm',
             {0x040000} 'No Combat in Water',
             {0x080000} 'No Shadow',
             {0x100000} 'No VATS Melee',
           {0x00200000} 'Allow PC Dialogue',
           {0x00400000} 'Can''t Open Doors',
           {0x00800000} 'Immobile',
           {0x01000000} 'Tilt Front/Back',
           {0x02000000} 'Tilt Left/Right',
           {0x03000000} 'No Knockdowns',
           {0x08000000} 'Not Pushable',
           {0x10000000} 'Allow Pickpocket',
           {0x20000000} 'Is Ghost',
           {0x40000000} 'No Rotating To Head-track',
           {0x80000000} 'Invulnerable'
           ], [
             {0x000001 Biped} wbActorTemplateUseModelAnimation,
             {0x000002 Essential} wbActorTemplateUseBaseData,
             {0x000004 Weapon & Shield} nil,
             {0x000008 Respawn} wbActorTemplateUseBaseData,
             {0x000010 Swims} wbActorTemplateUseModelAnimation,
             {0x000020 Flies} wbActorTemplateUseModelAnimation,
             {0x000040 Walks} wbActorTemplateUseModelAnimation,
             {0x000080 PC Level Mult} wbActorTemplateUseStats,
             {0x000100 Unknown 8} nil,
             {0x000200 No Low Level Processing} wbActorTemplateUseBaseData,
             {0x000400 } nil,
             {0x000800 No Blood Spray} wbActorTemplateUseModelAnimation,
             {0x001000 No Blood Decal} wbActorTemplateUseModelAnimation,
             {0x002000 } nil,
             {0x004000 } nil,
             {0x008000 No Head} wbActorTemplateUseModelAnimation,
             {0x010000 No Right Arm} wbActorTemplateUseModelAnimation,
             {0x020000 No Left Arm} wbActorTemplateUseModelAnimation,
             {0x040000 No Combat in Water} wbActorTemplateUseModelAnimation,
             {0x080000 No Shadow} wbActorTemplateUseModelAnimation,
             {0x100000 No VATS Melee} nil,
           {0x00200000 Allow PC Dialogue} wbActorTemplateUseBaseData,
           {0x00400000 Can''t Open Doors} wbActorTemplateUseBaseData,
           {0x00800000 Immobile} wbActorTemplateUseModelAnimation,
           {0x01000000 Tilt Front/Back} wbActorTemplateUseModelAnimation,
           {0x02000000 Tilt Left/Right} wbActorTemplateUseModelAnimation,
           {0x03000000 No Knockdowns} nil,
           {0x08000000 Not Pushable} wbActorTemplateUseModelAnimation,
           {0x10000000 Allow Pickpocket} wbActorTemplateUseBaseData,
           {0x20000000 Is Ghost} nil,
           {0x40000000 No Rotating To Head-track} wbActorTemplateUseModelAnimation,
           {0x80000000 Invulnerable} nil
           ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {04} wbInteger('Fatigue', itU16, nil, cpNormal, False, wbActorTemplateUseStats),
      {06} wbInteger('Barter gold', itU16, nil, cpNormal, False, wbActorTemplateUseAIData),
      {08} wbUnion('Level', wbACBSLevelDecider, [
             wbInteger('Level', itU16),
             wbInteger('Level Mult', itU16, wbDiv(1000, 2))
               .SetAfterLoad(wbACBSLevelMultAfterLoad)
               .SetDefaultNativeValue(1000)
           ]).SetAfterSet(wbACBSLevelMultAfterSet)
             .SetDontShow(wbActorTemplateUseStats),
      {10} wbInteger('Calc min', itU16, nil, cpNormal, False, wbActorTemplateUseStats),
      {12} wbInteger('Calc max', itU16, nil, cpNormal, False, wbActorTemplateUseStats),
      {14} wbInteger('Speed Multiplier', itU16, nil, cpNormal, False, wbActorTemplateUseStats),
      {16} wbFloat('Karma (Alignment)', cpNormal, False, 1, -1, wbActorTemplateUseTraits),
      {20} wbInteger('Disposition Base', itS16, nil, cpNormal, False, wbActorTemplateUseTraits),
      {22} wbInteger('Template Flags', itU16, wbTemplateFlags).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True),
    wbRArrayS('Factions', wbFaction, cpNormal, False, nil, nil, wbActorTemplateUseFactions),
    wbFormIDCk(INAM, 'Death item', [LVLI], False, cpNormal, False, wbActorTemplateUseTraits),
    wbFormIDCk(VTCK, 'Voice', [VTYP], False, cpNormal, False, wbActorTemplateUseTraits),
    wbFormIDCk(TPLT, 'Template', [CREA, LVLC]),
    wbDESTActor,
    wbSCRIActor,
    wbRArrayS('Items', wbCNTO, cpNormal, False, nil, nil, wbActorTemplateUseInventory),
    wbAIDT,
    wbRArray('Packages', wbFormIDCk(PKID, 'Package', [PACK]), cpNormal, False, nil, nil, wbActorTemplateUseAIPackages),
    wbArrayS(KFFZ, 'Animations', wbStringLC('Animation'), 0, cpNormal, False, nil, nil, wbActorTemplateUseModelAnimation),
    wbStruct(DATA, '', [
      {00} wbInteger('Type', itU8, wbCreatureTypeEnum, cpNormal, False, wbActorTemplateUseTraits),
      {01} wbInteger('Combat Skill', itU8, nil, cpNormal, False, wbActorTemplateUseStats),
      {02} wbInteger('Magic Skill', itU8, nil, cpNormal, False, wbActorTemplateUseStats),
      {03} wbInteger('Stealth Skill', itU8, nil, cpNormal, False, wbActorTemplateUseStats),
      {04} wbInteger('Health', itS16, nil, cpNormal, False, wbActorTemplateUseStats),
      {06} wbUnused(2),
      {08} wbInteger('Damage', itS16, nil, cpNormal, False, wbActorTemplateUseStats),
      {10} wbArray('Attributes', wbInteger('Attribute', itU8), [
            'Strength',
            'Perception',
            'Endurance',
            'Charisma',
            'Intelligence',
            'Agility',
            'Luck'
          ], cpNormal, False, wbActorTemplateUseStats)
    ], cpNormal, True),
    wbInteger(RNAM, 'Attack reach', itU8, nil, cpNormal, True, False, wbActorTemplateUseTraits),
    wbFormIDCk(ZNAM, 'Combat Style', [CSTY], False, cpNormal, False, wbActorTemplateUseTraits),
    wbFormIDCk(PNAM, 'Body Part Data', [BPTD], False, cpNormal, True, wbActorTemplateUseModelAnimation),
    wbFloat(TNAM, 'Turning Speed', cpNormal, True, 1, -1, wbActorTemplateUseStats),
    wbFloat(BNAM, 'Base Scale', cpNormal, True, 1, -1, wbActorTemplateUseStats),
    wbFloat(WNAM, 'Foot Weight', cpNormal, True, 1, -1, wbActorTemplateUseStats),
    wbInteger(NAM4, 'Impact Material Type', itU32, wbActorImpactMaterialEnum, cpNormal, True, False, wbActorTemplateUseModelAnimation),
    wbInteger(NAM5, 'Sound Level', itU32, wbSoundLevelEnum, cpNormal, True, False, wbActorTemplateUseModelAnimation),
    wbFormIDCk(CSCR, 'Inherits Sounds from', [CREA], False, cpNormal, False, wbActorTemplateUseModelAnimation),
    wbCSDTs,
    wbFormIDCk(CNAM, 'Impact Dataset', [IPDS], False, cpNormal, False, wbActorTemplateUseModelAnimation),
    wbFormIDCk(LNAM, 'Melee Weapon List', [FLST], False, cpNormal, False, wbActorTemplateUseTraits)
  ], True);

# CSNO.Casino
FNV:
  wbRecord(CSNO, 'Casino', [
    wbEDIDReq,
    wbFULL,
    wbStruct(DATA, 'Data', [
      wbFloat('Decks % Before Shuffle'),
      wbFloat('BlackJack Payout Ratio'),
      wbArray('Slot Reel Stops', wbInteger('Reel', itU32),[
        'Symbol 1',
        'Symbol 2',
        'Symbol 3',
        'Symbol 4',
        'Symbol 5',
        'Symbol 6',
        'Symbol W'
      ]),
      wbInteger('Number of Decks', itU32),
      wbInteger('Max Winnings', itU32),
      wbFormIDCk('Currency', [CHIP]),
      wbFormIDCk('Casino Winnings Quest', [QUST]),
      wbInteger('Flags', itU32, wbFlags([
        'Dealer Stay on Soft 17'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]),
    wbRStruct('Casino Chip Models', [
      wbString(MODL, '$1 Chip'),
      wbString(MODL, '$5 Chip'),
      wbString(MODL, '$10 Chip'),
      wbString(MODL, '$25 Chip'),
      wbString(MODL, '$100 Chip'),
      wbString(MODL, '$500 Chip'),
      wbString(MODL, 'Roulette Chip')
    ]),
    wbString(MODL, 'Slot Machine Model'),
    wbString(MOD2, 'Slot Machine Model (again?)'),
    wbString(MOD3, 'BlackJack Table Model'),
    wbString(MODT, 'BlackJack Table Model related'),
    wbString(MOD4, 'Roulette Table Model'),
    wbRStruct('Slot Reel Textures', [
      wbString(ICON, 'Symbol 1'),
      wbString(ICON, 'Symbol 2'),
      wbString(ICON, 'Symbol 3'),
      wbString(ICON, 'Symbol 4'),
      wbString(ICON, 'Symbol 5'),
      wbString(ICON, 'Symbol 6'),
      wbString(ICON, 'Symbol W')
    ]),
      wbRStruct('BlackJack Decks', [
      wbString(ICO2, 'Deck 1'),
      wbString(ICO2, 'Deck 2'),
      wbString(ICO2, 'Deck 3'),
      wbString(ICO2, 'Deck 4')
    ])
  ]);

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
FO3:
  wbRecord(CSTY, 'Combat Style', [
    wbEDIDReq,
    wbStruct(CSTD, 'Advanced - Standard', [
      {000}wbInteger('Maneuver Decision - Dodge % Chance', itU8),
      {001}wbInteger('Maneuver Decision - Left/Right % Chance', itU8),
      {002}wbUnused(2),
      {004}wbFloat('Maneuver Decision - Dodge L/R Timer (min)'),
      {008}wbFloat('Maneuver Decision - Dodge L/R Timer (max)'),
      {012}wbFloat('Maneuver Decision - Dodge Forward Timer (min)'),
      {016}wbFloat('Maneuver Decision - Dodge Forward Timer (max)'),
      {020}wbFloat('Maneuver Decision - Dodge Back Timer Min'),
      {024}wbFloat('Maneuver Decision - Dodge Back Timer Max'),
      {028}wbFloat('Maneuver Decision - Idle Timer min'),
      {032}wbFloat('Maneuver Decision - Idle Timer max'),
      {036}wbInteger('Melee Decision - Block % Chance', itU8),
      {037}wbInteger('Melee Decision - Attack % Chance', itU8),
      {038}wbUnused(2),
      {040}wbFloat('Melee Decision - Recoil/Stagger Bonus to Attack'),
      {044}wbFloat('Melee Decision - Unconscious Bonus to Attack'),
      {048}wbFloat('Melee Decision - Hand-To-Hand Bonus to Attack'),
      {052}wbInteger('Melee Decision - Power Attacks - Power Attack % Chance', itU8),
      {053}wbUnused(3),
      {056}wbFloat('Melee Decision - Power Attacks - Recoil/Stagger Bonus to Power'),
      {060}wbFloat('Melee Decision - Power Attacks - Unconscious Bonus to Power Attack'),
      {064}wbInteger('Melee Decision - Power Attacks - Normal', itU8),
      {065}wbInteger('Melee Decision - Power Attacks - Forward', itU8),
      {066}wbInteger('Melee Decision - Power Attacks - Back', itU8),
      {067}wbInteger('Melee Decision - Power Attacks - Left', itU8),
      {068}wbInteger('Melee Decision - Power Attacks - Right', itU8),
      {069}wbUnused(3),
      {072}wbFloat('Melee Decision - Hold Timer (min)'),
      {076}wbFloat('Melee Decision - Hold Timer (max)'),
      {080}wbInteger('Flags', itU16,
             wbFlags(wbSparseFlags([
               0, 'Choose Attack using % Chance',
               1, 'Melee Alert OK',
               2, 'Flee Based on Personal Survival',
               4, 'Ignore Threats',
               5, 'Ignore Damaging Self',
               6, 'Ignore Damaging Group',
               7, 'Ignore Damaging Spectators',
               8, 'Cannot Use Stealthboy'
             ], False, 9))
           ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {082}wbUnused(2),
      {085}wbInteger('Maneuver Decision - Acrobatic Dodge % Chance', itU8),
      {085}wbInteger('Melee Decision - Power Attacks - Rushing Attack % Chance', itU8),
      {086}wbUnused(2),
      {088}wbFloat('Melee Decision - Power Attacks - Rushing Attack Distance Mult')
    ]).SetRequired,
    wbStruct(CSAD, 'Advanced - Advanced', [
      wbFloat('Dodge Fatigue Mod Mult'),
      wbFloat('Dodge Fatigue Mod Base'),
      wbFloat('Encumb. Speed Mod Base'),
      wbFloat('Encumb. Speed Mod Mult'),
      wbFloat('Dodge While Under Attack Mult'),
      wbFloat('Dodge Not Under Attack Mult'),
      wbFloat('Dodge Back While Under Attack Mult'),
      wbFloat('Dodge Back Not Under Attack Mult'),
      wbFloat('Dodge Forward While Attacking Mult'),
      wbFloat('Dodge Forward Not Attacking Mult'),
      wbFloat('Block Skill Modifier Mult'),
      wbFloat('Block Skill Modifier Base'),
      wbFloat('Block While Under Attack Mult'),
      wbFloat('Block Not Under Attack Mult'),
      wbFloat('Attack Skill Modifier Mult'),
      wbFloat('Attack Skill Modifier Base'),
      wbFloat('Attack While Under Attack Mult'),
      wbFloat('Attack Not Under Attack Mult'),
      wbFloat('Attack During Block Mult'),
      wbFloat('Power Att. Fatigue Mod Base'),
      wbFloat('Power Att. Fatigue Mod Mult')
    ]).SetRequired,
    wbStruct(CSSD, 'Simple', [
      {00} wbFloat('Cover Search Radius'),
      {04} wbFloat('Take Cover Chance'),
      {08} wbFloat('Wait Timer (min)'),
      {12} wbFloat('Wait Timer (max)'),
      {16} wbFloat('Wait to Fire Timer (min)'),
      {20} wbFloat('Wait to Fire Timer (max)'),
      {24} wbFloat('Fire Timer (min)'),
      {28} wbFloat('Fire Timer (max)'),
      {32} wbFloat('Ranged Weapon Range Mult (min)'),
      {36} wbUnused(4),
      {40} wbInteger('Weapon Restrictions', itU32,
             wbEnum([
               {0} 'None',
               {1} 'Melee Only',
               {2} 'Ranged Only'
             ])),
      {44} wbFloat('Ranged Weapon Range Mult (max)'),
      {48} wbFloat('Max Targeting FOV'),
      {52} wbFloat('Combat Radius'),
      {56} wbFloat('Semi-Auto Firing Delay Mult (min)'),
      {60} wbFloat('Semi-Auto Firing Delay Mult (max)')
    ]).SetRequired
  ]);
FNV:
  wbRecord(CSTY, 'Combat Style', [
    wbEDIDReq,
    wbStruct(CSTD, 'Advanced - Standard', [
      {000}wbInteger('Maneuver Decision - Dodge % Chance', itU8),
      {001}wbInteger('Maneuver Decision - Left/Right % Chance', itU8),
      {002}wbUnused(2),
      {004}wbFloat('Maneuver Decision - Dodge L/R Timer (min)'),
      {008}wbFloat('Maneuver Decision - Dodge L/R Timer (max)'),
      {012}wbFloat('Maneuver Decision - Dodge Forward Timer (min)'),
      {016}wbFloat('Maneuver Decision - Dodge Forward Timer (max)'),
      {020}wbFloat('Maneuver Decision - Dodge Back Timer Min'),
      {024}wbFloat('Maneuver Decision - Dodge Back Timer Max'),
      {028}wbFloat('Maneuver Decision - Idle Timer min'),
      {032}wbFloat('Maneuver Decision - Idle Timer max'),
      {036}wbInteger('Melee Decision - Block % Chance', itU8),
      {037}wbInteger('Melee Decision - Attack % Chance', itU8),
      {038}wbUnused(2),
      {040}wbFloat('Melee Decision - Recoil/Stagger Bonus to Attack'),
      {044}wbFloat('Melee Decision - Unconscious Bonus to Attack'),
      {048}wbFloat('Melee Decision - Hand-To-Hand Bonus to Attack'),
      {052}wbInteger('Melee Decision - Power Attacks - Power Attack % Chance', itU8),
      {053}wbUnused(3),
      {056}wbFloat('Melee Decision - Power Attacks - Recoil/Stagger Bonus to Power'),
      {060}wbFloat('Melee Decision - Power Attacks - Unconscious Bonus to Power Attack'),
      {064}wbInteger('Melee Decision - Power Attacks - Normal', itU8),
      {065}wbInteger('Melee Decision - Power Attacks - Forward', itU8),
      {066}wbInteger('Melee Decision - Power Attacks - Back', itU8),
      {067}wbInteger('Melee Decision - Power Attacks - Left', itU8),
      {068}wbInteger('Melee Decision - Power Attacks - Right', itU8),
      {069}wbUnused(3),
      {072}wbFloat('Melee Decision - Hold Timer (min)'),
      {076}wbFloat('Melee Decision - Hold Timer (max)'),
      {080}wbInteger('Flags', itU16, wbFlags([
             'Choose Attack using % Chance',
             'Melee Alert OK',
             'Flee Based on Personal Survival',
             '',
             'Ignore Threats',
             'Ignore Damaging Self',
             'Ignore Damaging Group',
             'Ignore Damaging Spectators',
             'Cannot Use Stealthboy'
           ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {082}wbUnused(2),
      {085}wbInteger('Maneuver Decision - Acrobatic Dodge % Chance', itU8),
      {085}wbInteger('Melee Decision - Power Attacks - Rushing Attack % Chance', itU8),
      {086}wbUnused(2),
      {088}wbFloat('Melee Decision - Power Attacks - Rushing Attack Distance Mult')
    ], cpNormal, True),
    wbStruct(CSAD, 'Advanced - Advanced', [
      wbFloat('Dodge Fatigue Mod Mult'),
      wbFloat('Dodge Fatigue Mod Base'),
      wbFloat('Encumb. Speed Mod Base'),
      wbFloat('Encumb. Speed Mod Mult'),
      wbFloat('Dodge While Under Attack Mult'),
      wbFloat('Dodge Not Under Attack Mult'),
      wbFloat('Dodge Back While Under Attack Mult'),
      wbFloat('Dodge Back Not Under Attack Mult'),
      wbFloat('Dodge Forward While Attacking Mult'),
      wbFloat('Dodge Forward Not Attacking Mult'),
      wbFloat('Block Skill Modifier Mult'),
      wbFloat('Block Skill Modifier Base'),
      wbFloat('Block While Under Attack Mult'),
      wbFloat('Block Not Under Attack Mult'),
      wbFloat('Attack Skill Modifier Mult'),
      wbFloat('Attack Skill Modifier Base'),
      wbFloat('Attack While Under Attack Mult'),
      wbFloat('Attack Not Under Attack Mult'),
      wbFloat('Attack During Block Mult'),
      wbFloat('Power Att. Fatigue Mod Base'),
      wbFloat('Power Att. Fatigue Mod Mult')
    ], cpNormal, True),
    wbStruct(CSSD, 'Simple', [
      {00} wbFloat('Cover Search Radius'),
      {04} wbFloat('Take Cover Chance'),
      {08} wbFloat('Wait Timer (min)'),
      {12} wbFloat('Wait Timer (max)'),
      {16} wbFloat('Wait to Fire Timer (min)'),
      {20} wbFloat('Wait to Fire Timer (max)'),
      {24} wbFloat('Fire Timer (min)'),
      {28} wbFloat('Fire Timer (max)'),
      {32} wbFloat('Ranged Weapon Range Mult (min)'),
      {36} wbUnused(4),
      {40} wbInteger('Weapon Restrictions', itU32, wbEnum([
        'None',
        'Melee Only',
        'Ranged Only'
      ])),
      {44} wbFloat('Ranged Weapon Range Mult (max)'),
      {48} wbFloat('Max Targeting FOV'),
      {52} wbFloat('Combat Radius'),
      {56} wbFloat('Semi-Auto Firing Delay Mult (min)'),
      {60} wbFloat('Semi-Auto Firing Delay Mult (max)')
    ], cpNormal, True)
  ]);

# DEBR.Debris
FO3:
  wbRecord(DEBR, 'Debris', [
    wbEDIDReq,
    wbRArray('Models', wbDebrisModel(wbMODT)).SetRequired
  ]);
FNV:
  wbRecord(DEBR, 'Debris', [
    wbEDIDReq,
    wbRArray('Models', wbDebrisModel(wbMODT), cpNormal, True)
  ]);

# DEHY.Dehydration Stage
FNV:
  wbRecord(DEHY, 'Dehydration Stage', [
    wbEDIDReq,
    wbStruct(DATA, '', [
      wbInteger('Trigger Threshold', itU32),
      wbFormIDCk('Actor Effect', [SPEL])
    ], cpNormal, True)
  ]);

# HUNG.Hunger Stage
FNV:
  wbRecord(HUNG, 'Hunger Stage', [
    wbEDIDReq,
    wbStruct(DATA, '', [
      wbInteger('Trigger Threshold', itU32),
      wbFormIDCk('Actor Effect', [SPEL])
    ], cpNormal, True)
  ]);

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
FO3:
  wbRecord(DIAL, 'Dialog Topic', [
    wbEDIDReqKC,
    wbQSTI,
    wbQSTR,
    wbFULL
      .SetAfterLoad(wbDialogueTextAfterLoad)
      .SetAfterSet(wbDialogueTextAfterSet),
    wbFloat(PNAM, 'Priority')
      .SetDefaultNativeValue(50)
      .SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU8,
        wbEnum([
        {0} 'Topic',
        {1} 'Conversation',
        {2} 'Combat',
        {3} 'Persuasion',
        {4} 'Detection',
        {5} 'Service',
        {6} 'Miscellaneous',
        {7} 'Radio'
        ])),
      wbInteger('Flags', itU8,
        wbFlags([
        {0} 'Rumors',
        {1} 'Top-level'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True, nil, 1),
    wbINOM,
    wbINOA
  ]);
FNV:
  wbRecord(DIAL, 'Dialog Topic', [
    wbEDIDReqKC,
    wbRArrayS('Added Quests',
      wbRStructSK([0], 'Added Quest', [
        wbFormIDCkNoReach(QSTI, 'Quest', [QUST], False, cpBenign),
        wbRArray('Shared Infos',
          wbRStruct('Shared Info', [
            wbFormIDCk(INFC, 'Info Connection', [INFO], False, cpBenign),
            wbInteger(INFX, 'Info Index', itS32, nil, cpBenign)
          ]))
      ])),
    // no QSTR in FNV, but keep it just in case
    wbQSTR,
    // some records have INFC INFX (with absent formids) but no QSTI, probably error in GECK
    // i.e. [DIAL:001287C6] and [DIAL:000E9084]
    wbRArray('Unused',
      wbRStruct('Unused', [
        wbUnknown(INFC, cpIgnore),
        wbUnknown(INFX, cpIgnore)
      ]), cpIgnore).SetDontShow(wbNeverShow),
    wbFULL
      .SetAfterLoad(wbDialogueTextAfterLoad)
      .SetAfterSet(wbDialogueTextAfterSet),
    wbFloat(PNAM, 'Priority')
      .SetDefaultNativeValue(50)
      .SetRequired,
    wbStringKC(TDUM, 'Dumb Response'),
    wbStruct(DATA, '', [
      wbInteger('Type', itU8,
        wbEnum([
        {0} 'Topic',
        {1} 'Conversation',
        {2} 'Combat',
        {3} 'Persuasion',
        {4} 'Detection',
        {5} 'Service',
        {6} 'Miscellaneous',
        {7} 'Radio'
        ])),
      wbInteger('Flags', itU8,
        wbFlags([
        {0} 'Rumors',
        {1} 'Top-level'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True, nil, 1),
    wbINOM,
    wbINOA
  ]);

# DLVW.Dialog View

# DOBJ.Default Object Manager
FO3:
  wbRecord(DOBJ, 'Default Object Manager', [
    wbString(EDID, 'Editor ID')
      .SetDefaultNativeValue('DefaultObjectManager')
      .SetRequired
      .IncludeFlag(dfInternalEditOnly),
    wbStruct(DATA, 'Default Objects', [
      wbFormIDCk('Stimpak', [ALCH,NULL]),
      wbFormIDCk('Super Stimpak', [ALCH,NULL]),
      wbFormIDCk('Rad X', [ALCH,NULL]),
      wbFormIDCk('Rad Away', [ALCH,NULL]),
      wbFormIDCk('Morphine', [ALCH,NULL]),
      wbFormIDCk('Perk Paralysis', [SPEL,NULL]),
      wbFormIDCk('Player Faction', [FACT,NULL]),
      wbFormIDCk('Mysterious Stranger NPC', [NPC_,NULL]),
      wbFormIDCk('Mysterious Stranger Faction', [FACT,NULL]),
      wbFormIDCk('Default Music', [MUSC,NULL]),
      wbFormIDCk('Battle Music', [MUSC,NULL]),
      wbFormIDCk('Death Music', [MUSC,NULL]),
      wbFormIDCk('Success Music', [MUSC,NULL]),
      wbFormIDCk('Level Up Music', [MUSC,NULL]),
      wbFormIDCk('Player Voice (Male)', [VTYP,NULL]),
      wbFormIDCk('Player Voice (Male Child)', [VTYP,NULL]),
      wbFormIDCk('Player Voice (Female)', [VTYP,NULL]),
      wbFormIDCk('Player Voice (Female Child)', [VTYP,NULL]),
      wbFormIDCk('Eat Package Default Food', [FLST,NULL]),
      wbFormIDCk('Every Actor Ability', [SPEL,NULL]),
      wbFormIDCk('Drug Wears Off Image Space', [IMAD,NULL])
    ]).SetRequired
  ]);
FNV:
  wbRecord(DOBJ, 'Default Object Manager', [
    wbString(EDID, 'Editor ID')
      .SetDefaultNativeValue('DefaultObjectManager')
      .SetRequired
      .IncludeFlag(dfInternalEditOnly),
    wbStruct(DATA, 'Default Objects', [
      wbFormIDCk('Stimpak', [ALCH,NULL]),
      wbFormIDCk('Super Stimpak', [ALCH,NULL]),
      wbFormIDCk('Rad X', [ALCH,NULL]),
      wbFormIDCk('Rad Away', [ALCH,NULL]),
      wbFormIDCk('Morphine', [ALCH,NULL]),
      wbFormIDCk('Perk Paralysis', [SPEL,NULL]),
      wbFormIDCk('Player Faction', [FACT,NULL]),
      wbFormIDCk('Mysterious Stranger NPC', [NPC_,NULL]),
      wbFormIDCk('Mysterious Stranger Faction', [FACT,NULL]),
      wbFormIDCk('Default Music', [MUSC,NULL]),
      wbFormIDCk('Battle Music', [MUSC,NULL]),
      wbFormIDCk('Death Music', [MUSC,NULL]),
      wbFormIDCk('Success Music', [MUSC,NULL]),
      wbFormIDCk('Level Up Music', [MUSC,NULL]),
      wbFormIDCk('Player Voice (Male)', [VTYP,NULL]),
      wbFormIDCk('Player Voice (Male Child)', [VTYP,NULL]),
      wbFormIDCk('Player Voice (Female)', [VTYP,NULL]),
      wbFormIDCk('Player Voice (Female Child)', [VTYP,NULL]),
      wbFormIDCk('Eat Package Default Food', [FLST,NULL]),
      wbFormIDCk('Every Actor Ability', [SPEL,NULL]),
      wbFormIDCk('Drug Wears Off Image Space', [IMAD,NULL]),
      wbFormIDCk('Doctor''s Bag', [ALCH,NULL]),
      wbFormIDCk('Miss Fortune NPC', [NPC_,NULL]),
      wbFormIDCk('Miss Fortune Faction', [FACT,NULL]),
      wbFormIDCk('Meltdown Explosion', [EXPL,NULL]),
      wbFormIDCk('Unarmed Forward PA', [SPEL,NULL]),
      wbFormIDCk('Unarmed Backward PA', [SPEL,NULL]),
      wbFormIDCk('Unarmed Left PA', [SPEL,NULL]),
      wbFormIDCk('Unarmed Right PA', [SPEL,NULL]),
      wbFormIDCk('Unarmed Crouch PA', [SPEL,NULL]),
      wbFormIDCk('Unarmed Counter PA', [SPEL,NULL]),
      wbFormIDCk('Spotter Effect', [EFSH,NULL]),
      wbFormIDCk('Item  Detected Effect', [EFSH,NULL]),
      wbFormIDCk('Cateye Mobile Effect (NYI)', [EFSH,NULL])
    ]).SetRequired
  ]);

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
FO3:
  wbRecord(DOOR, 'Door',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      15, 'Visible When Distant',
      16, 'Random Anim Start'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbSCRI,
    wbDEST,
    wbFormIDCk(SNAM, 'Sound - Open', [SOUN]),
    wbFormIDCk(ANAM, 'Sound - Close', [SOUN]),
    wbFormIDCk(BNAM, 'Sound - Looping', [SOUN]),
    wbInteger(FNAM, 'Flags', itU8,
      wbFlags(wbSparseFlags([
        1, 'Automatic Door',
        2, 'Hidden',
        3, 'Minimal Use',
        4, 'Sliding Door'
      ], False, 5))
    ).SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);
FNV:
  wbRecord(DOOR, 'Door',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      15, 'Visible When Distant',
      16, 'Random Anim Start'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbSCRI,
    wbDEST,
    wbFormIDCk(SNAM, 'Sound - Open', [SOUN]),
    wbFormIDCk(ANAM, 'Sound - Close', [SOUN]),
    wbFormIDCk(BNAM, 'Sound - Looping', [SOUN]),
    wbInteger(FNAM, 'Flags', itU8, wbFlags([
      '',
      'Automatic Door',
      'Hidden',
      'Minimal Use',
      'Sliding Door'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);

# DUEL.Dual Cast Data

# ECZN.Encounter Zone
FO3:
  wbRecord(ECZN, 'Encounter Zone', [
    wbEDIDReq,
    wbStruct(DATA, '', [
      wbFormIDCkNoReach('Owner', [NPC_, FACT, NULL]),
      wbInteger('Rank', itS8),
      wbInteger('Minimum Level', itS8),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Never Resets',
          {1} 'Match PC Below Minimum Level'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(1)
    ]).SetRequired
  ]);
FNV:
  wbRecord(ECZN, 'Encounter Zone', [
    wbEDIDReq,
    wbStruct(DATA, '', [
      wbFormIDCkNoReach('Owner', [NPC_, FACT, NULL]),
      wbInteger('Rank', itS8),
      wbInteger('Minimum Level', itS8),
      wbInteger('Flags', itU8, wbFlags([
        'Never Resets',
        'Match PC Below Minimum Level'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(1)
    ], cpNormal, True)
  ]);

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
FO3:
  wbRecord(EFSH, 'Effect Shader', [
    wbEDID,
    wbString(ICON, 'Fill Texture'),
    wbString(ICO2, 'Particle Shader Texture'),
    wbString(NAM7, 'Holes Texture'),
    wbStruct(DATA, '', [
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'No Membrane Shader',
          3, 'No Particle Shader',
          4, 'Edge Effect - Inverse',
          5, 'Membrane Shader - Affect Skin Only'
        ], False, 6))).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbInteger('Membrane Shader - Source Blend Mode', itU32, wbBlendModeEnum),
      wbInteger('Membrane Shader - Blend Operation', itU32, wbBlendOpEnum),
      wbInteger('Membrane Shader - Z Test Function', itU32, wbZTestFuncEnum),
      wbByteColors('Fill/Texture Effect - Color'),
      wbFloat('Fill/Texture Effect - Alpha Fade In Time'),
      wbFloat('Fill/Texture Effect - Full Alpha Time'),
      wbFloat('Fill/Texture Effect - Alpha Fade Out Time'),
      wbFloat('Fill/Texture Effect - Presistent Alpha Ratio'),
      wbFloat('Fill/Texture Effect - Alpha Pulse Amplitude'),
      wbFloat('Fill/Texture Effect - Alpha Pulse Frequency'),
      wbFloat('Fill/Texture Effect - Texture Animation Speed (U)'),
      wbFloat('Fill/Texture Effect - Texture Animation Speed (V)'),
      wbFloat('Edge Effect - Fall Off'),
      wbByteColors('Edge Effect - Color'),
      wbFloat('Edge Effect - Alpha Fade In Time'),
      wbFloat('Edge Effect - Full Alpha Time'),
      wbFloat('Edge Effect - Alpha Fade Out Time'),
      wbFloat('Edge Effect - Persistent Alpha Ratio'),
      wbFloat('Edge Effect - Alpha Pulse Amplitude'),
      wbFloat('Edge Effect - Alpha Pusle Frequence'),
      wbFloat('Fill/Texture Effect - Full Alpha Ratio'),
      wbFloat('Edge Effect - Full Alpha Ratio'),
      wbInteger('Membrane Shader - Dest Blend Mode', itU32, wbBlendModeEnum),
      wbInteger('Particle Shader - Source Blend Mode', itU32, wbBlendModeEnum),
      wbInteger('Particle Shader - Blend Operation', itU32, wbBlendOpEnum),
      wbInteger('Particle Shader - Z Test Function', itU32, wbZTestFuncEnum),
      wbInteger('Particle Shader - Dest Blend Mode', itU32, wbBlendModeEnum),
      wbFloat('Particle Shader - Particle Birth Ramp Up Time'),
      wbFloat('Particle Shader - Full Particle Birth Time'),
      wbFloat('Particle Shader - Particle Birth Ramp Down Time'),
      wbFloat('Particle Shader - Full Particle Birth Ratio'),
      wbFloat('Particle Shader - Persistant Particle Birth Ratio'),
      wbFloat('Particle Shader - Particle Lifetime'),
      wbFloat('Particle Shader - Particle Lifetime +/-'),
      wbFloat('Particle Shader - Initial Speed Along Normal'),
      wbFloat('Particle Shader - Acceleration Along Normal'),
      wbFloat('Particle Shader - Initial Velocity #1'),
      wbFloat('Particle Shader - Initial Velocity #2'),
      wbFloat('Particle Shader - Initial Velocity #3'),
      wbFloat('Particle Shader - Acceleration #1'),
      wbFloat('Particle Shader - Acceleration #2'),
      wbFloat('Particle Shader - Acceleration #3'),
      wbFloat('Particle Shader - Scale Key 1'),
      wbFloat('Particle Shader - Scale Key 2'),
      wbFloat('Particle Shader - Scale Key 1 Time'),
      wbFloat('Particle Shader - Scale Key 2 Time'),
      wbByteColors('Color Key 1 - Color'),
      wbByteColors('Color Key 2 - Color'),
      wbByteColors('Color Key 3 - Color'),
      wbFloat('Color Key 1 - Color Alpha'),
      wbFloat('Color Key 2 - Color Alpha'),
      wbFloat('Color Key 3 - Color Alpha'),
      wbFloat('Color Key 1 - Color Key Time'),
      wbFloat('Color Key 2 - Color Key Time'),
      wbFloat('Color Key 3 - Color Key Time'),
      wbFloat('Particle Shader - Initial Speed Along Normal +/-'),
      wbFloat('Particle Shader - Initial Rotation (deg)'),
      wbFloat('Particle Shader - Initial Rotation (deg) +/-'),
      wbFloat('Particle Shader - Rotation Speed (deg/sec)'),
      wbFloat('Particle Shader - Rotation Speed (deg/sec) +/-'),
      wbFormIDCk('Addon Models', [DEBR, NULL]),
      wbFloat('Holes - Start Time'),
      wbFloat('Holes - End Time'),
      wbFloat('Holes - Start Val'),
      wbFloat('Holes - End Val'),
      wbFloat('Edge Width (alpha units)'),
      wbByteColors('Edge Color'),
      wbFloat('Explosion Wind Speed'),
      wbInteger('Texture Count U', itU32),
      wbInteger('Texture Count V', itU32),
      wbFloat('Addon Models - Fade In Time'),
      wbFloat('Addon Models - Fade Out Time'),
      wbFloat('Addon Models - Scale Start'),
      wbFloat('Addon Models - Scale End'),
      wbFloat('Addon Models - Scale In Time'),
      wbFloat('Addon Models - Scale Out Time')
    ], cpNormal, True, nil, 57)
  ]).SetAfterLoad(wbEFSHAfterLoad);
FNV:
  wbRecord(EFSH, 'Effect Shader', [
    wbEDID,
    wbString(ICON, 'Fill Texture'),
    wbString(ICO2, 'Particle Shader Texture'),
    wbString(NAM7, 'Holes Texture'),
    wbStruct(DATA, '', [
      wbInteger('Flags', itU8, wbFlags([
        {0} 'No Membrane Shader',
        {1} '',
        {2} '',
        {3} 'No Particle Shader',
        {4} 'Edge Effect - Inverse',
        {5} 'Membrane Shader - Affect Skin Only'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbInteger('Membrane Shader - Source Blend Mode', itU32, wbBlendModeEnum),
      wbInteger('Membrane Shader - Blend Operation', itU32, wbBlendOpEnum),
      wbInteger('Membrane Shader - Z Test Function', itU32, wbZTestFuncEnum),
      wbByteColors('Fill/Texture Effect - Color'),
      wbFloat('Fill/Texture Effect - Alpha Fade In Time'),
      wbFloat('Fill/Texture Effect - Full Alpha Time'),
      wbFloat('Fill/Texture Effect - Alpha Fade Out Time'),
      wbFloat('Fill/Texture Effect - Presistent Alpha Ratio'),
      wbFloat('Fill/Texture Effect - Alpha Pulse Amplitude'),
      wbFloat('Fill/Texture Effect - Alpha Pulse Frequency'),
      wbFloat('Fill/Texture Effect - Texture Animation Speed (U)'),
      wbFloat('Fill/Texture Effect - Texture Animation Speed (V)'),
      wbFloat('Edge Effect - Fall Off'),
      wbByteColors('Edge Effect - Color'),
      wbFloat('Edge Effect - Alpha Fade In Time'),
      wbFloat('Edge Effect - Full Alpha Time'),
      wbFloat('Edge Effect - Alpha Fade Out Time'),
      wbFloat('Edge Effect - Persistent Alpha Ratio'),
      wbFloat('Edge Effect - Alpha Pulse Amplitude'),
      wbFloat('Edge Effect - Alpha Pusle Frequence'),
      wbFloat('Fill/Texture Effect - Full Alpha Ratio'),
      wbFloat('Edge Effect - Full Alpha Ratio'),
      wbInteger('Membrane Shader - Dest Blend Mode', itU32, wbBlendModeEnum),
      wbInteger('Particle Shader - Source Blend Mode', itU32, wbBlendModeEnum),
      wbInteger('Particle Shader - Blend Operation', itU32, wbBlendOpEnum),
      wbInteger('Particle Shader - Z Test Function', itU32, wbZTestFuncEnum),
      wbInteger('Particle Shader - Dest Blend Mode', itU32, wbBlendModeEnum),
      wbFloat('Particle Shader - Particle Birth Ramp Up Time'),
      wbFloat('Particle Shader - Full Particle Birth Time'),
      wbFloat('Particle Shader - Particle Birth Ramp Down Time'),
      wbFloat('Particle Shader - Full Particle Birth Ratio'),
      wbFloat('Particle Shader - Persistant Particle Birth Ratio'),
      wbFloat('Particle Shader - Particle Lifetime'),
      wbFloat('Particle Shader - Particle Lifetime +/-'),
      wbFloat('Particle Shader - Initial Speed Along Normal'),
      wbFloat('Particle Shader - Acceleration Along Normal'),
      wbFloat('Particle Shader - Initial Velocity #1'),
      wbFloat('Particle Shader - Initial Velocity #2'),
      wbFloat('Particle Shader - Initial Velocity #3'),
      wbFloat('Particle Shader - Acceleration #1'),
      wbFloat('Particle Shader - Acceleration #2'),
      wbFloat('Particle Shader - Acceleration #3'),
      wbFloat('Particle Shader - Scale Key 1'),
      wbFloat('Particle Shader - Scale Key 2'),
      wbFloat('Particle Shader - Scale Key 1 Time'),
      wbFloat('Particle Shader - Scale Key 2 Time'),
      wbByteColors('Color Key 1 - Color'),
      wbByteColors('Color Key 2 - Color'),
      wbByteColors('Color Key 3 - Color'),
      wbFloat('Color Key 1 - Color Alpha'),
      wbFloat('Color Key 2 - Color Alpha'),
      wbFloat('Color Key 3 - Color Alpha'),
      wbFloat('Color Key 1 - Color Key Time'),
      wbFloat('Color Key 2 - Color Key Time'),
      wbFloat('Color Key 3 - Color Key Time'),
      wbFloat('Particle Shader - Initial Speed Along Normal +/-'),
      wbFloat('Particle Shader - Initial Rotation (deg)'),
      wbFloat('Particle Shader - Initial Rotation (deg) +/-'),
      wbFloat('Particle Shader - Rotation Speed (deg/sec)'),
      wbFloat('Particle Shader - Rotation Speed (deg/sec) +/-'),
      wbFormIDCk('Addon Models', [DEBR, NULL]),
      wbFloat('Holes - Start Time'),
      wbFloat('Holes - End Time'),
      wbFloat('Holes - Start Val'),
      wbFloat('Holes - End Val'),
      wbFloat('Edge Width (alpha units)'),
      wbByteColors('Edge Color'),
      wbFloat('Explosion Wind Speed'),
      wbInteger('Texture Count U', itU32),
      wbInteger('Texture Count V', itU32),
      wbFloat('Addon Models - Fade In Time'),
      wbFloat('Addon Models - Fade Out Time'),
      wbFloat('Addon Models - Scale Start'),
      wbFloat('Addon Models - Scale End'),
      wbFloat('Addon Models - Scale In Time'),
      wbFloat('Addon Models - Scale Out Time')
    ], cpNormal, True, nil, 57)
  ], False, nil, cpNormal, False, wbEFSHAfterLoad);

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
FO3:
  wbRecord(ENCH, 'Object Effect', [
    wbEDIDReq,
    wbFULL,
    wbStruct(ENIT, 'Effect Data', [
      wbInteger('Type', itU32,
        wbEnum([], [
          2, 'Weapon',
          3, 'Apparel'
        ])),
      wbUnused(4),
      wbUnused(4),
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'No Auto-Calc',
          2, 'Hide Effect'
        ], False, 3))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired,
    wbEffectsReq
  ]);
FNV:
  wbRecord(ENCH, 'Object Effect', [
    wbEDIDReq,
    wbFULL,
    wbStruct(ENIT, 'Effect Data', [
      wbInteger('Type', itU32, wbEnum([
        {0} '',
        {1} '',
        {2} 'Weapon',
        {3} 'Apparel'
      ])),
      wbUnused(4),
      wbUnused(4),
      wbInteger('Flags', itU8, wbFlags([
        'No Auto-Calc',
        'Auto Calculate',
        'Hide Effect'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ], cpNormal, True),
    wbEffectsReq
  ]);

# EQUP.Equip Slots

# EXPL.Explosion
FO3:
   wbRecord(EXPL, 'Explosion', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbEnchantment,
    wbFormIDCk(MNAM, 'Image Space Modifier', [IMAD]),
    wbStruct(DATA, 'Data', [
      {00} wbFloat('Force'),
      {04} wbFloat('Damage'),
      {08} wbFloat('Radius'),
      {12} wbFormIDCk('Light', [LIGH, NULL]),
      {16} wbFormIDCk('Sound 1', [SOUN, NULL]),
      {20} wbInteger('Flags', itU32,
             wbFlags(wbSparseFlags([
               1, 'Always Uses World Orientation',
               2, 'Knock Down - Always',
               3, 'Knock Down - By Formula',
               4, 'Ignore LOS Check',
               5, 'Push Explosion Source Ref Only',
               6, 'Ignore Image Space Swap'
             ], False, 7), True)
           ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {24} wbFloat('IS Radius'),
      {28} wbFormIDCk('Impact DataSet', [IPDS, NULL]),
      {32} wbFormIDCk('Sound 2', [SOUN, NULL]),
           wbStruct('Radiation', [
             {36} wbFloat('Level'),
             {40} wbFloat('Dissipation Time'),
             {44} wbFloat('Radius')
           ]),
      {48} wbInteger('Sound Level', itU32, wbSoundLevelEnum)
    ]).SetRequired,
    wbFormIDCk(INAM, 'Placed Impact Object', [TREE, SOUN, ACTI, DOOR, STAT, FURN,
          CONT, ARMO, AMMO, LVLN, LVLC, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, GRAS,
          ASPC, IDLM, ARMA, MSTT, NOTE, PWAT, SCOL, TACT, TERM, TXST])
  ]);
FNV:
   wbRecord(EXPL, 'Explosion', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbEnchantment,
    wbFormIDCk(MNAM, 'Image Space Modifier', [IMAD]),
    wbStruct(DATA, 'Data', [
      {00} wbFloat('Force'),
      {04} wbFloat('Damage'),
      {08} wbFloat('Radius'),
      {12} wbFormIDCk('Light', [LIGH, NULL]),
      {16} wbFormIDCk('Sound 1', [SOUN, NULL]),
      {20} wbInteger('Flags', itU32,
             wbFlags(wbSparseFlags([
               1, 'Always Uses World Orientation',
               2, 'Knock Down - Always',
               3, 'Knock Down - By Formula',
               4, 'Ignore LOS Check',
               5, 'Push Explosion Source Ref Only',
               6, 'Ignore Image Space Swap'
             ], False, 7), True)
           ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {24} wbFloat('IS Radius'),
      {28} wbFormIDCk('Impact DataSet', [IPDS, NULL]),
      {32} wbFormIDCk('Sound 2', [SOUN, NULL]),
           wbStruct('Radiation', [
             {36} wbFloat('Level'),
             {40} wbFloat('Dissipation Time'),
             {44} wbFloat('Radius')
           ]),
      {48} wbInteger('Sound Level', itU32, wbSoundLevelEnum, cpNormal, True)
    ], cpNormal, True),
    wbFormIDCk(INAM, 'Placed Impact Object', [TREE, SOUN, ACTI, DOOR, STAT, FURN,
          CONT, ARMO, AMMO, LVLN, LVLC, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, GRAS,
          ASPC, IDLM, ARMA, MSTT, NOTE, PWAT, SCOL, TACT, TERM, TXST, CHIP, CMNY,
          CCRD, IMOD])
  ]);

# EYES.Eyes
TES4:
  wbRecord(EYES, 'Eyes', [
    wbEDID.SetRequired,
    wbFULL,
    wbString(ICON, 'Texture').SetRequired,
    wbInteger(DATA, 'Playable', itU8, wbBoolEnum).SetRequired
  ]);
FO3:
  wbRecord(EYES, 'Eyes', [
    wbEDIDReq,
    wbFULLReq,
    wbString(ICON, 'Texture'),
    wbInteger(DATA, 'Flags', itU8,
      wbFlags([
        {0} 'Playable',
        {1} 'Not Male',
        {2} 'Not Female'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);
FNV:
  wbRecord(EYES, 'Eyes', [
    wbEDIDReq,
    wbFULLReq,
    wbString(ICON, 'Texture', 0{, cpNormal, True??}),
    wbInteger(DATA, 'Flags', itU8, wbFlags([
      'Playable',
      'Not Male',
      'Not Female'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags)
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
FO3:
  wbRecord(FACT, 'Faction', [
    wbEDIDReq,
    wbFULL,
    wbFactionRelations,
    wbStruct(DATA, '', [
      wbInteger('Flags 1', itU8,
        wbFlags([
          {0} 'Hidden from PC',
          {1} 'Evil',
          {2} 'Special Combat'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Flags 2', itU8,
        wbFlags([
          {0} 'Track Crime',
          {1} 'Allow Sell'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(2)
    ], cpNormal, True, nil, 1),
    wbFloat(CNAM, 'Unused'),
    wbRArrayS('Ranks',
      wbRStructSK([0], 'Rank', [
        wbInteger(RNAM, 'Rank#', itS32),
        wbString(MNAM, 'Male', 0, cpTranslate),
        wbString(FNAM, 'Female', 0, cpTranslate),
        wbString(INAM, 'Insignia (Unused)')
      ]))
  ]).SetAfterLoad(wbFACTAfterLoad);
FNV:
  wbRecord(FACT, 'Faction', [
    wbEDIDReq,
    wbFULL,
    wbFactionRelations,
    wbStruct(DATA, '', [
      wbInteger('Flags 1', itU8, wbFlags([
        'Hidden from PC',
        'Evil',
        'Special Combat'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Flags 2', itU8, wbFlags([
        'Track Crime',
        'Allow Sell'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(2)
    ], cpNormal, True, nil, 1),
    wbFloat(CNAM, 'Unused'),
    wbRArrayS('Ranks', wbFactionRank),
    wbFormIDCk(WMI1, 'Reputation', [REPU])
  ], False, nil, cpNormal, False, wbFACTAfterLoad);

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
FO3:
  wbRecord(FLST, 'FormID List', [
    wbString(EDID, 'Editor ID', 0, cpBenign)
      .SetAfterSet(wbFLSTEDIDAfterSet)
      .SetRequired,
    wbRArrayS('FormIDs', wbFormID(LNAM, 'FormID'), cpNormal, False, nil, nil, nil, wbFLSTLNAMIsSorted)
  ]);
FNV:
  wbRecord(FLST, 'FormID List', [
    wbString(EDID, 'Editor ID', 0, cpBenign, True, nil, wbFLSTEDIDAfterSet),
    wbRArrayS('FormIDs', wbFormID(LNAM, 'FormID'), cpNormal, False, nil, nil, nil, wbFLSTLNAMIsSorted)
  ]);

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
FO3:
  wbRecord(FURN, 'Furniture',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start',
      29, 'Child Can Use'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbSCRI,
    wbDEST,
    wbByteArray(MNAM, 'Marker Flags').SetRequired
  ]);
FNV:
  wbRecord(FURN, 'Furniture',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start',
      29, 'Child Can Use'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbSCRI,
    wbDEST,
    wbByteArray(MNAM, 'Marker Flags', 0, cpNormal, True)
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
FO3:
  wbRecord(GLOB, 'Global',
    wbFlags(wbFlagsList([
      6, 'Constant'
    ])), [
    wbEDIDReq,
    wbInteger(FNAM, 'Type', itU8,
      wbEnum([], [
        Ord('s'), 'Short',
        Ord('l'), 'Long',
        Ord('f'), 'Float'
      ])).SetDefaultEditValue('Float').SetRequired,
    wbFloat(FLTV, 'Value').SetRequired
  ]);
FNV:
  wbRecord(GLOB, 'Global',
    wbFlags(wbFlagsList([
      6, 'Constant'
    ])), [
    wbEDIDReq,
    wbInteger(FNAM, 'Type', itU8, wbEnum([], [
      Ord('s'), 'Short',
      Ord('l'), 'Long',
      Ord('f'), 'Float'
    ]), cpNormal, True).SetDefaultEditValue('Float'),
    wbFloat(FLTV, 'Value', cpNormal, True)
  ]);

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
FO3:
  wbRecord(GMST, 'Game Setting', [
    wbString(EDID, 'Editor ID', 0, cpCritical)
      .SetAfterSet(wbGMSTEDIDAfterSet)
      .SetRequired,
    wbUnion(DATA, 'Value', wbGMSTUnionDecider, [
      wbString('Name', 0, cpTranslate),
      wbInteger('Int', itS32),
      wbFloat('Float')
    ]).SetRequired
  ]).SetSummaryKey([1])
    .IncludeFlag(dfIndexEditorID);
FNV:
  wbRecord(GMST, 'Game Setting', [
    wbString(EDID, 'Editor ID', 0, cpCritical, True, nil, wbGMSTEDIDAfterSet),
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
FO3:
  wbRecord(GRAS, 'Grass', [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbStruct(DATA, '', [
      wbInteger('Density', itU8),
      wbInteger('Min Slope', itU8),
      wbInteger('Max Slope', itU8),
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
      wbFloat('Position Range'),
      wbFloat('Height Range'),
      wbFloat('Color Range'),
      wbFloat('Wave Period'),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Vertex Lighting',
          {1} 'Uniform Scaling',
          {2} 'Fit to Slope'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired
  ]);
FNV:
  wbRecord(GRAS, 'Grass', [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbStruct(DATA, '', [
      wbInteger('Density', itU8),
      wbInteger('Min Slope', itU8),
      wbInteger('Max Slope', itU8),
      wbUnused(1),
      wbInteger('Unit from water amount', itU16),
      wbUnused(2),
      wbInteger('Unit from water type', itU32, wbEnum([
        'Above - At Least',
        'Above - At Most',
        'Below - At Least',
        'Below - At Most',
        'Either - At Least',
        'Either - At Most',
        'Either - At Most Above',
        'Either - At Most Below'
      ])),
      wbFloat('Position Range'),
      wbFloat('Height Range'),
      wbFloat('Color Range'),
      wbFloat('Wave Period'),
      wbInteger('Flags', itU8, wbFlags([
        'Vertex Lighting',
        'Uniform Scaling',
        'Fit to Slope'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ], cpNormal, True)
  ]);

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
FO3:
  wbRecord(HAIR, 'Hair', [
    wbEDIDReq,
    wbFULLReq,
    wbGenericModel(True),
    wbString(ICON, 'Texture').SetRequired,
    wbInteger(DATA, 'Flags', itU8,
      wbFlags([
        {0} 'Playable',
        {1} 'Not Male',
        {2} 'Not Female',
        {3} 'Fixed'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);
FNV:
  wbRecord(HAIR, 'Hair', [
    wbEDIDReq,
    wbFULLReq,
    wbGenericModel(True),
    wbString(ICON, 'Texture', 0, cpNormal, True),
    wbInteger(DATA, 'Flags', itU8, wbFlags([
      'Playable',
      'Not Male',
      'Not Female',
      'Fixed'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);


# HAZD.Hazard

# HDPT.Head Part
FO3:
  wbRecord(HDPT, 'Head Part', [
    wbEDIDReq,
    wbFULLReq,
    wbGenericModel,
    wbInteger(DATA, 'Playable', itU8, wbBoolEnum).SetRequired,
    wbRArrayS('Extra Parts',
      wbFormIDCk(HNAM, 'Part', [HDPT])
    )
  ]);
FNV:
  wbRecord(HDPT, 'Head Part', [
    wbEDIDReq,
    wbFULLReq,
    wbGenericModel,
    wbInteger(DATA, 'Playable', itU8, wbBoolEnum).SetRequired,
    wbRArrayS('Extra Parts',
      wbFormIDCk(HNAM, 'Part', [HDPT])
    )
  ]);

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
FO3:
  wbRecord(IDLE, 'Idle Animation', [
    wbEDID,
    wbGenericModel(True),
    wbConditions,
    wbStruct(ANAM, 'Animations', [
      wbFormIDCk('Parent', [IDLE, NULL], False, cpBenign),
      wbFormIDCk('Previous', [IDLE, NULL], False, cpBenign)
    ]).SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Animation Group Section', itU8, wbIdleAnam),
      wbStruct('Looping', [
        wbInteger('Min', itU8),
        wbInteger('Max', itU8)
      ]),
      wbUnused(1),
      wbInteger('Replay Delay', itS16),
      wbInteger('No Attacking', itU8, wbBoolEnum),
      wbUnused(1)
    ], cpNormal, True, nil, 4)
  ]);
FNV:
  wbRecord(IDLE, 'Idle Animation', [
    wbEDID,
    wbGenericModel(True),
    wbConditions,
    wbStruct(ANAM, 'Animations', [
      wbFormIDCk('Parent', [IDLE, NULL], False, cpBenign),
      wbFormIDCk('Previous', [IDLE, NULL], False, cpBenign)
    ]).SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Animation Group Section', itU8, wbIdleAnam),
      wbStruct('Looping', [
        wbInteger('Min', itU8),
        wbInteger('Max', itU8)
      ]),
      wbUnused(1),
      wbInteger('Replay Delay', itS16),
      wbInteger('Flags', itU8, wbFlags([
        'No attacking'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(1)
    ], cpNormal, True, nil, 4)
  ]);


# IDLM.Idle Marker
FO3:
  wbRecord(IDLM, 'Idle Marker',
    wbFlags(wbFlagsList([
    10, 'Quest Item',
    29, 'Child Can Use'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbIdleAnimation
  ]);
FNV:
  wbRecord(IDLM, 'Idle Marker',
    wbFlags(wbFlagsList([
    10, 'Quest Item',
    29, 'Child Can Use'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbIdleAnimation
  ]);

# IMAD.Image Space Modifier
FO3:
  wbRecord(IMAD, 'Image Space Adapter', [
    wbEDID.SetRequired,
    wbStruct(DNAM, 'Data', [
      wbInteger('Animatable', itU32, wbBoolEnum),
      wbFloat('Duration'),
      wbStruct('HDR', [
        wbIMADMultAddCount('Eye Adapt Speed'),
        wbIMADMultAddCount('Blur Radius'),
        wbIMADMultAddCount('Skin Dimmer'),
        wbIMADMultAddCount('Emissive Mult'),
        wbIMADMultAddCount('Target Lum'),
        wbIMADMultAddCount('Upper Lum Clamp'),
        wbIMADMultAddCount('Bright Scale'),
        wbIMADMultAddCount('Bright Clamp'),
        wbIMADMultAddCount('LUM Ramp No Tex'),
        wbIMADMultAddCount('LUM Ramp Min'),
        wbIMADMultAddCount('LUM Ramp Max'),
        wbIMADMultAddCount('Sunlight Dimmer'),
        wbIMADMultAddCount('Grass Dimmer'),
        wbIMADMultAddCount('Tree Dimmer')
      ]),
      wbStruct('Bloom', [
        wbIMADMultAddCount('Blur Radius'),
        wbIMADMultAddCount('Alpha Mult Interior'),
        wbIMADMultAddCount('Alpha Mult Exterior')
      ]),
      wbStruct('Cinematic', [
        wbIMADMultAddCount('Saturation'),
        wbIMADMultAddCount('Contrast'),
        wbIMADMultAddCount('Contrast Avg Lum'),
        wbIMADMultAddCount('Brightness')
      ]),
      wbInteger('Tint Color', itU32),
      wbInteger('Blur Radius', itU32),
      wbInteger('Double Vision Strength', itU32),
      wbInteger('Radial Blur Strength', itU32),
      wbInteger('Radial Blur Ramp Up', itU32),
      wbInteger('Radial Blur Start', itU32),
      wbInteger('Radial Blur - Use Target', itU32, wbBoolEnum),
      wbFloat('Radial Blur Center X'),
      wbFloat('Radial Blur Center Y'),
      wbInteger('DoF Strength', itU32),
      wbInteger('DoF Distance', itU32),
      wbInteger('DoF Range', itU32),
      wbInteger('DoF - Use Target', itU8, wbBoolEnum),
      wbInteger('DoF Flags', itU8,
        wbFlags([
        {0} 'Mode - Front',
        {1} 'Mode - Back',
        {2} 'No Sky',
        {3} 'Unknown 3',
        {4} 'Unknown 4',
        {5} 'Unknown 5'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(2),
      wbInteger('Radial Blur Ramp Down', itU32),
      wbInteger('Radial Blur Down Start', itU32),
      wbInteger('Fade Color', itU32),
      wbInteger('Motion Blur Strength', itU32)
    ], cpNormal, True, nil, 8),
    wbTimeInterpolators(BNAM, 'Blur Radius'),
    wbTimeInterpolators(VNAM, 'Double Vision Strength'),
    wbArray(TNAM, 'Tint Color', wbColorInterpolator).SetRequired,
    wbArray(NAM3, 'Fade Color', wbColorInterpolator).SetRequired,
    wbRStruct('Radial Blur', [
      wbTimeInterpolators(RNAM, 'Strength'),
      wbTimeInterpolators(SNAM, 'Ramp Up'),
      wbTimeInterpolators(UNAM, 'Start'),
      wbTimeInterpolators(NAM1, 'Ramp Down'),
      wbTimeInterpolators(NAM2, 'Down Start')
    ]).SetRequired,
    wbRStruct('Depth of Field', [
      wbTimeInterpolators(WNAM, 'Strength'),
      wbTimeInterpolators(XNAM, 'Distance'),
      wbTimeInterpolators(YNAM, 'Range')
    ]).SetRequired,
    wbTimeInterpolators(NAM4, 'Motion Blur Strength'),
    wbRStruct('HDR', [
      wbTimeInterpolatorsMultAdd(_00_IAD, _40_IAD, 'Eye Adapt Speed'),
      wbTimeInterpolatorsMultAdd(_01_IAD, _41_IAD, 'Blur Radius'),
      wbTimeInterpolatorsMultAdd(_02_IAD, _42_IAD, 'Skin Dimmer'),
      wbTimeInterpolatorsMultAdd(_03_IAD, _43_IAD, 'Emissive Mult'),
      wbTimeInterpolatorsMultAdd(_04_IAD, _44_IAD, 'Target LUM'),
      wbTimeInterpolatorsMultAdd(_05_IAD, _45_IAD, 'Upper LUM Clamp'),
      wbTimeInterpolatorsMultAdd(_06_IAD, _46_IAD, 'Bright Scale'),
      wbTimeInterpolatorsMultAdd(_07_IAD, _47_IAD, 'Bright Clamp'),
      wbTimeInterpolatorsMultAdd(_08_IAD, _48_IAD, 'LUM Ramp No Tex'),
      wbTimeInterpolatorsMultAdd(_09_IAD, _49_IAD, 'LUM Ramp Min'),
      wbTimeInterpolatorsMultAdd(_0A_IAD, _4A_IAD, 'LUM Ramp Max'),
      wbTimeInterpolatorsMultAdd(_0B_IAD, _4B_IAD, 'Sunlight Dimmer'),
      wbTimeInterpolatorsMultAdd(_0C_IAD, _4C_IAD, 'Grass Dimmer'),
      wbTimeInterpolatorsMultAdd(_0D_IAD, _4D_IAD, 'Tree Dimmer')
    ]).SetRequired,
    wbRStruct('Bloom', [
      wbTimeInterpolatorsMultAdd(_0E_IAD, _4E_IAD, 'Blur Radius'),
      wbTimeInterpolatorsMultAdd(_0F_IAD, _4F_IAD, 'Alpha Mult Interior'),
      wbTimeInterpolatorsMultAdd(_10_IAD, _50_IAD, 'Alpha Mult Exterior')
    ]).SetRequired,
    wbRStruct('Cinematic', [
      wbTimeInterpolatorsMultAdd(_11_IAD, _51_IAD, 'Saturation'),
      wbTimeInterpolatorsMultAdd(_12_IAD, _52_IAD, 'Contrast'),
      wbTimeInterpolatorsMultAdd(_13_IAD, _53_IAD, 'Contrast Avg Lum'),
      wbTimeInterpolatorsMultAdd(_14_IAD, _54_IAD, 'Brightness')
    ]).SetRequired
  ]);
FNV:
  wbRecord(IMAD, 'Image Space Adapter', [
    wbEDID.SetRequired,
    wbStruct(DNAM, 'Data', [
      wbInteger('Animatable', itU32, wbBoolEnum),
      wbFloat('Duration'),
      wbStruct('HDR', [
        wbIMADMultAddCount('Eye Adapt Speed'),
        wbIMADMultAddCount('Blur Radius'),
        wbIMADMultAddCount('Skin Dimmer'),
        wbIMADMultAddCount('Emissive Mult'),
        wbIMADMultAddCount('Target Lum'),
        wbIMADMultAddCount('Upper Lum Clamp'),
        wbIMADMultAddCount('Bright Scale'),
        wbIMADMultAddCount('Bright Clamp'),
        wbIMADMultAddCount('LUM Ramp No Tex'),
        wbIMADMultAddCount('LUM Ramp Min'),
        wbIMADMultAddCount('LUM Ramp Max'),
        wbIMADMultAddCount('Sunlight Dimmer'),
        wbIMADMultAddCount('Grass Dimmer'),
        wbIMADMultAddCount('Tree Dimmer')
      ]),
      wbStruct('Bloom', [
        wbIMADMultAddCount('Blur Radius'),
        wbIMADMultAddCount('Alpha Mult Interior'),
        wbIMADMultAddCount('Alpha Mult Exterior')
      ]),
      wbStruct('Cinematic', [
        wbIMADMultAddCount('Saturation'),
        wbIMADMultAddCount('Contrast'),
        wbIMADMultAddCount('Contrast Avg Lum'),
        wbIMADMultAddCount('Brightness')
      ]),
      wbInteger('Tint Color', itU32),
      wbInteger('Blur Radius', itU32),
      wbInteger('Double Vision Strength', itU32),
      wbInteger('Radial Blur Strength', itU32),
      wbInteger('Radial Blur Ramp Up', itU32),
      wbInteger('Radial Blur Start', itU32),
      wbInteger('Radial Blur - Use Target', itU32, wbBoolEnum),
      wbFloat('Radial Blur Center X'),
      wbFloat('Radial Blur Center Y'),
      wbInteger('DoF Strength', itU32),
      wbInteger('DoF Distance', itU32),
      wbInteger('DoF Range', itU32),
      wbInteger('DoF - Use Target', itU8, wbBoolEnum),
      wbInteger('DoF Flags', itU8,
        wbFlags([
        {0} 'Mode - Front',
        {1} 'Mode - Back',
        {2} 'No Sky',
        {3} 'Unknown 3',
        {4} 'Unknown 4',
        {5} 'Unknown 5'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(2),
      wbInteger('Radial Blur Ramp Down', itU32),
      wbInteger('Radial Blur Down Start', itU32),
      wbInteger('Fade Color', itU32),
      wbInteger('Motion Blur Strength', itU32)
    ], cpNormal, True, nil, 8),
    wbTimeInterpolators(BNAM, 'Blur Radius'),
    wbTimeInterpolators(VNAM, 'Double Vision Strength'),
    wbArray(TNAM, 'Tint Color', wbColorInterpolator).SetRequired,
    wbArray(NAM3, 'Fade Color', wbColorInterpolator).SetRequired,
    wbRStruct('Radial Blur', [
      wbTimeInterpolators(RNAM, 'Strength'),
      wbTimeInterpolators(SNAM, 'Ramp Up'),
      wbTimeInterpolators(UNAM, 'Start'),
      wbTimeInterpolators(NAM1, 'Ramp Down'),
      wbTimeInterpolators(NAM2, 'Down Start')
    ]).SetRequired,
    wbRStruct('Depth of Field', [
      wbTimeInterpolators(WNAM, 'Strength'),
      wbTimeInterpolators(XNAM, 'Distance'),
      wbTimeInterpolators(YNAM, 'Range')
    ]).SetRequired,
    wbTimeInterpolators(NAM4, 'Motion Blur Strength'),
    wbRStruct('HDR', [
      wbTimeInterpolatorsMultAdd(_00_IAD, _40_IAD, 'Eye Adapt Speed'),
      wbTimeInterpolatorsMultAdd(_01_IAD, _41_IAD, 'Blur Radius'),
      wbTimeInterpolatorsMultAdd(_02_IAD, _42_IAD, 'Skin Dimmer'),
      wbTimeInterpolatorsMultAdd(_03_IAD, _43_IAD, 'Emissive Mult'),
      wbTimeInterpolatorsMultAdd(_04_IAD, _44_IAD, 'Target LUM'),
      wbTimeInterpolatorsMultAdd(_05_IAD, _45_IAD, 'Upper LUM Clamp'),
      wbTimeInterpolatorsMultAdd(_06_IAD, _46_IAD, 'Bright Scale'),
      wbTimeInterpolatorsMultAdd(_07_IAD, _47_IAD, 'Bright Clamp'),
      wbTimeInterpolatorsMultAdd(_08_IAD, _48_IAD, 'LUM Ramp No Tex'),
      wbTimeInterpolatorsMultAdd(_09_IAD, _49_IAD, 'LUM Ramp Min'),
      wbTimeInterpolatorsMultAdd(_0A_IAD, _4A_IAD, 'LUM Ramp Max'),
      wbTimeInterpolatorsMultAdd(_0B_IAD, _4B_IAD, 'Sunlight Dimmer'),
      wbTimeInterpolatorsMultAdd(_0C_IAD, _4C_IAD, 'Grass Dimmer'),
      wbTimeInterpolatorsMultAdd(_0D_IAD, _4D_IAD, 'Tree Dimmer')
    ]).SetRequired,
    wbRStruct('Bloom', [
      wbTimeInterpolatorsMultAdd(_0E_IAD, _4E_IAD, 'Blur Radius'),
      wbTimeInterpolatorsMultAdd(_0F_IAD, _4F_IAD, 'Alpha Mult Interior'),
      wbTimeInterpolatorsMultAdd(_10_IAD, _50_IAD, 'Alpha Mult Exterior')
    ]).SetRequired,
    wbRStruct('Cinematic', [
      wbTimeInterpolatorsMultAdd(_11_IAD, _51_IAD, 'Saturation'),
      wbTimeInterpolatorsMultAdd(_12_IAD, _52_IAD, 'Contrast'),
      wbTimeInterpolatorsMultAdd(_13_IAD, _53_IAD, 'Contrast Avg Lum'),
      wbTimeInterpolatorsMultAdd(_14_IAD, _54_IAD, 'Brightness')
    ]).SetRequired,
    wbFormIDCk(RDSD, 'Sound - Intro', [SOUN]),
    wbFormIDCk(RDSI, 'Sound - Outro', [SOUN])
  ]);

# IMGS.Image Space
FO3:
  wbRecord(IMGS, 'Image Space', [
    wbEDIDReq,
    wbStruct(DNAM, '', [
      wbStruct('HDR', [
        {00} wbFloat('Eye Adapt Speed'),
        {04} wbFloat('Blur Radius'),
        {08} wbFloat('Blur Passes'),
        {12} wbFloat('Emissive Mult'),
        {16} wbFloat('Target LUM'),
        {20} wbFloat('Upper LUM Clamp'),
        {24} wbFloat('Bright Scale'),
        {28} wbFloat('Bright Clamp'),
        {32} wbFloat('LUM Ramp No Tex'),
        {36} wbFloat('LUM Ramp Min'),
        {40} wbFloat('LUM Ramp Max'),
        {44} wbFloat('Sunlight Dimmer'),
        {48} wbFloat('Grass Dimmer'),
        {52} wbFloat('Tree Dimmer'),
        {56} wbFromVersion(10, wbFloat('Skin Dimmer'))
      ], cpNormal, False, nil, 14),
      wbStruct('Bloom', [
        {60} wbFloat('Blur Radius'),
        {64} wbFloat('Alpha Mult Interior'),
        {68} wbFloat('Alpha Mult Exterior')
      ]),
      wbStruct('Get Hit', [
        {72} wbFloat('Blur Radius'),
        {76} wbFloat('Blur Damping Constant'),
        {80} wbFloat('Damping Constant')
      ]),
      wbStruct('Night Eye', [
        wbFloatColors('Tint Color'),
      {96} wbFloat('Brightness')
      ]),
      wbStruct('Cinematic', [
        {100} wbFloat('Saturation'),
        wbStruct('Contrast', [
          {104} wbFloat('Avg Lum Value'),
          {108} wbFloat('Value')
        ]),
        {112} wbFloat('Cinematic - Brightness - Value'),
        wbStruct('Tint', [
          wbFloatColors('Color'),
        {128} wbFloat('Value')
        ])
      ]),
      wbByteArray('Unknown', 4),
      wbFromVersion(10, wbUnused(4)),
      wbFromVersion(10, wbUnused(4)),
      wbFromVersion(10, wbUnused(4)),
      wbFromVersion(13, wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Saturation',
          {1} 'Contrast',
          {2} 'Tint',
          {3} 'Brightness'
        ], True))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFromVersion(13, wbUnused(3))
    ], cpNormal, True, nil, 5)
  ]);
FNV:
  wbRecord(IMGS, 'Image Space', [
    wbEDIDReq,
    wbStruct(DNAM, '', [
      wbStruct('HDR', [
        {00} wbFloat('Eye Adapt Speed'),
        {04} wbFloat('Blur Radius'),
        {08} wbFloat('Blur Passes'),
        {12} wbFloat('Emissive Mult'),
        {16} wbFloat('Target LUM'),
        {20} wbFloat('Upper LUM Clamp'),
        {24} wbFloat('Bright Scale'),
        {28} wbFloat('Bright Clamp'),
        {32} wbFloat('LUM Ramp No Tex'),
        {36} wbFloat('LUM Ramp Min'),
        {40} wbFloat('LUM Ramp Max'),
        {44} wbFloat('Sunlight Dimmer'),
        {48} wbFloat('Grass Dimmer'),
        {52} wbFloat('Tree Dimmer'),
        {56} wbFromVersion(10, wbFloat('Skin Dimmer'))
      ], cpNormal, False, nil, 14),
      wbStruct('Bloom', [
        {60} wbFloat('Blur Radius'),
        {64} wbFloat('Alpha Mult Interior'),
        {68} wbFloat('Alpha Mult Exterior')
      ]),
      wbStruct('Get Hit', [
        {72} wbFloat('Blur Radius'),
        {76} wbFloat('Blur Damping Constant'),
        {80} wbFloat('Damping Constant')
      ]),
      wbStruct('Night Eye', [
        wbFloatColors('Tint Color'),
      {96} wbFloat('Brightness')
      ]),
      wbStruct('Cinematic', [
        {100} wbFloat('Saturation'),
        wbStruct('Contrast', [
          {104} wbFloat('Avg Lum Value'),
          {108} wbFloat('Value')
        ]),
        {112} wbFloat('Cinematic - Brightness - Value'),
        wbStruct('Tint', [
          wbFloatColors('Color'),
        {128} wbFloat('Value')
        ])
      ]),
      wbByteArray('Unknown', 4),
      wbFromVersion(10, wbUnused(4)),
      wbFromVersion(10, wbUnused(4)),
      wbFromVersion(10, wbUnused(4)),
      wbFromVersion(13, wbInteger('Flags', itU8, wbFlags([
        'Saturation',
        'Contrast',
        'Tint',
        'Brightness'
      ], True))).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFromVersion(13, wbUnused(3))
    ], cpNormal, True, nil, 5)
  ]);

# IMOD.Item Mode
FNV:
  wbRecord(IMOD, 'Item Mod', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDESC,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, 'Data', [
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ])
  ]);

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
FO3:
  wbRecord(INFO, 'Dialog response',
    wbFlags(wbFlagsList([
    13, 'Unknown 13'
    ])), [
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU8,
        wbEnum([
        {0} 'Topic',
        {1} 'Conversation',
        {2} 'Combat',
        {3} 'Persuasion',
        {4} 'Detection',
        {5} 'Service',
        {6} 'Miscellaneous',
        {7} 'Radio'
        ])),
      wbNextSpeaker,
      wbInteger('Flags 1', itU8,
        wbFlags([
        {0} 'Goodbye',
        {1} 'Random',
        {2} 'Say Once',
        {3} 'Run Immediately',
        {4} 'Info Refusal',
        {5} 'Random End',
        {6} 'Run for Rumors',
        {7} 'Speech Challenge'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Flags 2', itU8,
        wbFlags([
        {0} 'Say Once a Day',
        {1} 'Always Darken'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True, nil, 3),
    wbFormIDCkNoReach(QSTI, 'Quest', [QUST]).SetRequired,
    wbFormIDCkNoReach(TPIC, 'Previous Topic', [DIAL]),
    wbFormIDCkNoReach(PNAM, 'Previous INFO', [INFO,NULL]),
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
            {6} 'Surprise',
            {7} 'Pained'
            ])),
          wbInteger('Emotion Value', itS32),
          wbUnused(4),
          wbInteger('Response number', itU8),
          wbUnused(3),
          wbFormIDCk('Sound', [SOUN,NULL]),
          wbInteger('Use Emotion Animation', itU8, wbBoolEnum),
          wbUnused(3)
        ], cpNormal, False, nil, 5),
        wbStringKC(NAM1, 'Response Text', 0, cpTranslate)
          .SetAfterLoad(wbDialogueTextAfterLoad)
          .SetAfterSet(wbDialogueTextAfterSet)
          .SetRequired,
        wbString(NAM2, 'Script Notes', 0, cpTranslate).SetRequired,
        wbString(NAM3, 'Edits'),
        wbFormIDCk(SNAM, 'Speaker Animation', [IDLE]),
        wbFormIDCk(LNAM, 'Listener Animation', [IDLE])
      ]).SetSummaryKey([1])
        .IncludeFlag(dfCollapsed)
    ),
    wbConditions,
    wbRArray('Choices', wbFormIDCk(TCLT, 'Choice', [DIAL])),
    wbRArray('Link From', wbFormIDCk(TCLF, 'Topic', [DIAL])),
    wbRStruct('Script (Begin)', [
      wbEmbeddedScriptReq
    ]).SetRequired,
    wbRStruct('Script (End)', [
      wbEmpty(NEXT, 'Marker').SetRequired,
      wbEmbeddedScriptReq
    ]).SetRequired,
    wbFormIDCk(SNDD, 'Unused', [SOUN]),
    wbStringKC(RNAM, 'Prompt', 0, cpTranslate)
      .SetAfterLoad(wbDialogueTextAfterLoad)
      .SetAfterSet(wbDialogueTextAfterSet),
    wbFormIDCk(ANAM, 'Speaker', [CREA,NPC_]),
    wbFormIDCk(KNAM, 'ActorValue/Perk', [AVIF,PERK]),
    wbInteger(DNAM, 'Speech Challenge', itU32,
      wbEnum([
      {0} 'None',
      {1} 'Very Easy',
      {2} 'Easy',
      {3} 'Average',
      {4} 'Hard',
      {5} 'Very Hard'
      ]))
  ]).SetAddInfo(wbINFOAddInfo)
    .SetAfterLoad(wbINFOAfterLoad);
FNV:
  wbRecord(INFO, 'Dialog response',
    wbFlags(wbFlagsList([
    13, 'Unknown 13'
    ])), [
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU8,
        wbEnum([
        {0} 'Topic',
        {1} 'Conversation',
        {2} 'Combat',
        {3} 'Persuasion',
        {4} 'Detection',
        {5} 'Service',
        {6} 'Miscellaneous',
        {7} 'Radio'
        ])),
      wbNextSpeaker,
      wbInteger('Flags 1', itU8,
        wbFlags([
        {0} 'Goodbye',
        {1} 'Random',
        {2} 'Say Once',
        {3} 'Run Immediately',
        {4} 'Info Refusal',
        {5} 'Random End',
        {6} 'Run for Rumors',
        {7} 'Speech Challenge'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Flags 2', itU8,
        wbFlags([
        {0} 'Say Once a Day',
        {1} 'Always Darken',
        {2} 'Unknown 2',
        {3} 'Unknown 3',
        {4} 'Low Intelligence',
        {5} 'High Intelligence'
        ])
      ).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True, nil, 3),
    wbFormIDCkNoReach(QSTI, 'Quest', [QUST]).SetRequired,
    wbFormIDCkNoReach(TPIC, 'Previous Topic', [DIAL]),  // The GECK ignores it for ESM
    wbFormIDCkNoReach(PNAM, 'Previous INFO', [INFO,NULL]),
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
            {6} 'Surprise',
            {7} 'Pained'
            ])),
          wbInteger('Emotion Value', itS32),
          wbUnused(4),
          wbInteger('Response number', itU8),
          wbUnused(3),
          wbFormIDCk('Sound', [SOUN, NULL]),
          wbInteger('Use Emotion Animation', itU8, wbBoolEnum),
          wbUnused(3)
        ], cpNormal, False, nil, 5),
        wbStringKC(NAM1, 'Response Text', 0, cpTranslate)
          .SetAfterLoad(wbDialogueTextAfterLoad)
          .SetAfterSet(wbDialogueTextAfterSet)
          .SetRequired,
        wbString(NAM2, 'Script Notes', 0, cpTranslate).SetRequired,
        wbString(NAM3, 'Edits'),
        wbFormIDCk(SNAM, 'Speaker Animation', [IDLE]),
        wbFormIDCk(LNAM, 'Listener Animation', [IDLE])
      ]).SetSummaryKey([1])
        .IncludeFlag(dfCollapsed)
    ),
    wbConditions,
    wbRArray('Link To', wbFormIDCk(TCLT, 'Topic', [DIAL])),
    wbRArray('Link From', wbFormIDCk(TCLF, 'Topic', [DIAL])),
    wbRArray('Follow Up', wbFormIDCk(TCFU, 'Info', [INFO] )),
    wbRStruct('Script (Begin)', [
      wbEmbeddedScriptReq
    ]).SetRequired,
    wbRStruct('Script (End)', [
      wbEmpty(NEXT, 'Marker').SetRequired,
      wbEmbeddedScriptReq
    ]).SetRequired,
    wbFormIDCk(SNDD, 'Unused', [SOUN]),
    wbStringKC(RNAM, 'Prompt', 0, cpTranslate)
      .SetAfterLoad(wbDialogueTextAfterLoad)
      .SetAfterSet(wbDialogueTextAfterSet),
    wbFormIDCk(ANAM, 'Speaker', [CREA,NPC_]),
    wbFormIDCk(KNAM, 'ActorValue/Perk', [AVIF,PERK]),
    wbInteger(DNAM, 'Speech Challenge', itU32,
      wbEnum([
      {0} 'None',
      {1} 'Very Easy',
      {2} 'Easy',
      {3} 'Average',
      {4} 'Hard',
      {5} 'Very Hard'
      ]))
  ]).SetAddInfo(wbINFOAddInfo)
    .SetAfterLoad(wbINFOAfterLoad);

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
FO3:
  wbRecord(INGR, 'Ingredient', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbETYPReq,
    wbFloat(DATA, 'Weight').SetRequired,
    wbStruct(ENIT, 'Effect Data', [
      wbInteger('Value', itS32),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'No auto-calculation',
          {1} 'Food item'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired,
    wbEffectsReq
  ]);
FNV:
  wbRecord(INGR, 'Ingredient', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbETYPReq,
    wbFloat(DATA, 'Weight', cpNormal, True),
    wbStruct(ENIT, 'Effect Data', [
      wbInteger('Value', itS32),
      wbInteger('Flags', itU8, wbFlags(['No auto-calculation', 'Food item'])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ], cpNormal, True),
    wbEffectsReq
  ]);


# IPCT.Impact Data
FO3:
  wbRecord(IPCT, 'Impact', [
    wbEDIDReq,
    wbGenericModel,
    wbStruct(DATA, '', [
      wbFloat('Effect - Duration'),
      wbInteger('Effect - Orientation', itU32,
        wbEnum([
          {0} 'Surface Normal',
          {1} 'Projectile Vector',
          {2} 'Projectile Reflection'
        ])),
      wbFloat('Angle Threshold'),
      wbFloat('Placement Radius'),
      wbInteger('Sound Level', itU32, wbSoundLevelEnum),
      wbInteger('No Decal Data', itU32, wbBoolEnum)
    ]).SetRequired,
    wbDODT,
    wbFormIDCk(DNAM, 'Texture Set', [TXST]),
    wbFormIDCk(SNAM, 'Sound 1', [SOUN]),
    wbFormIDCk(NAM1, 'Sound 2', [SOUN])
  ]);
FNV:
  wbRecord(IPCT, 'Impact', [
    wbEDIDReq,
    wbGenericModel,
    wbStruct(DATA, '', [
      wbFloat('Effect - Duration'),
      wbInteger('Effect - Orientation', itU32, wbEnum([
        'Surface Normal',
        'Projectile Vector',
        'Projectile Reflection'
      ])),
      wbFloat('Angle Threshold'),
      wbFloat('Placement Radius'),
      wbInteger('Sound Level', itU32, wbSoundLevelEnum),
      wbInteger('Flags', itU32, wbFlags([
        'No Decal Data'
      ]))
    ], cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbDODT,
    wbFormIDCk(DNAM, 'Texture Set', [TXST]),
    wbFormIDCk(SNAM, 'Sound 1', [SOUN]),
    wbFormIDCk(NAM1, 'Sound 2', [SOUN])
  ]);

# IPDS.Impact Data Set
FO3:
  wbRecord(IPDS, 'Impact DataSet', [
    wbEDIDReq,
    wbStruct(DATA, 'Impacts', [
      wbFormIDCk('Stone', [IPCT, NULL]),
      wbFormIDCk('Dirt', [IPCT, NULL]),
      wbFormIDCk('Grass', [IPCT, NULL]),
      wbFormIDCk('Glass', [IPCT, NULL]),
      wbFormIDCk('Metal', [IPCT, NULL]),
      wbFormIDCk('Wood', [IPCT, NULL]),
      wbFormIDCk('Organic', [IPCT, NULL]),
      wbFormIDCk('Cloth', [IPCT, NULL]),
      wbFormIDCk('Water', [IPCT, NULL]),
      wbFormIDCk('Hollow Metal', [IPCT, NULL]),
      wbFormIDCk('Organic Bug', [IPCT, NULL]),
      wbFormIDCk('Organic Glow', [IPCT, NULL])
    ], cpNormal, True, nil, 9)
  ]);
FNV:
  wbRecord(IPDS, 'Impact DataSet', [
    wbEDIDReq,
    wbStruct(DATA, 'Impacts', [
      wbFormIDCk('Stone', [IPCT, NULL]),
      wbFormIDCk('Dirt', [IPCT, NULL]),
      wbFormIDCk('Grass', [IPCT, NULL]),
      wbFormIDCk('Glass', [IPCT, NULL]),
      wbFormIDCk('Metal', [IPCT, NULL]),
      wbFormIDCk('Wood', [IPCT, NULL]),
      wbFormIDCk('Organic', [IPCT, NULL]),
      wbFormIDCk('Cloth', [IPCT, NULL]),
      wbFormIDCk('Water', [IPCT, NULL]),
      wbFormIDCk('Hollow Metal', [IPCT, NULL]),
      wbFormIDCk('Organic Bug', [IPCT, NULL]),
      wbFormIDCk('Organic Glow', [IPCT, NULL])
    ], cpNormal, True, nil, 9)
  ]);

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
FO3:
  wbRecord(KEYM, 'Key',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULLReq,
    wbGenericModel,
    wbICONReq,
    wbSCRI,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ]).SetRequired
  ]);
FNV:
  wbRecord(KEYM, 'Key',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULLReq,
    wbGenericModel,
    wbICONReq,
    wbSCRI,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ], cpNormal, True),
    wbFormIDCk(RNAM, 'Sound - Random/Looping', [SOUN])
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
FO3:
  wbRecord(LAND, 'Landscape',
    wbFlags(wbFlagsList([
      18, 'Compressed'
    ])), [
    wbInteger(DATA, 'Flags', itU32,
      wbFlags(wbSparseFlags([
        0,  'Has Vertex Normals/Height Map',
        1,  'Has Vertex Colours',
        2,  'Has Layers',
        3,  'Unknown 3',
        4,  'Auto-Calc Normals',
        10, 'Ignored'
      ], False, 11))
    ).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbLandNormals,
    wbLandHeights,
    wbLandColors,
    wbLandLayers
  ]).SetAddInfo(wbLANDAddInfo);
FNV:
  wbRecord(LAND, 'Landscape',
    wbFlags(wbFlagsList([
      18, 'Compressed'
    ])), [
    wbInteger(DATA, 'Flags', itU32, wbFlags([
      {0x001} 'Has Vertex Normals/Height Map',
      {0x002} 'Has Vertex Colours',
      {0x004} 'Has Layers',
      {0x008} 'Unknown 4',
      {0x010} 'Auto-Calc Normals',
      {0x020} '',
      {0x040} '',
      {0x080} '',
      {0x100} '',
      {0x200} '',
      {0x400} 'Ignored'
    ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbLandNormals,
    wbLandHeights,
    wbLandColors,
    wbLandLayers
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
FO3:
  wbRecord(LGTM, 'Lighting Template', [
    wbEDIDReq,
    wbStruct(DATA, 'Lighting', [
      wbByteColors('Ambient Color'),
      wbByteColors('Directional Color'),
      wbByteColors('Fog Color'),
      wbFloat('Fog Near'),
      wbFloat('Fog Far'),
      wbInteger('Directional Rotation XY', itS32),
      wbInteger('Directional Rotation Z', itS32),
      wbFloat('Directional Fade'),
      wbFloat('Fog Clip Dist'),
      wbFloat('Fog Power')
    ]).SetRequired
  ]);
FNV:
  wbRecord(LGTM, 'Lighting Template', [
    wbEDIDReq,
    wbStruct(DATA, 'Lighting', [
      wbByteColors('Ambient Color'),
      wbByteColors('Directional Color'),
      wbByteColors('Fog Color'),
      wbFloat('Fog Near'),
      wbFloat('Fog Far'),
      wbInteger('Directional Rotation XY', itS32),
      wbInteger('Directional Rotation Z', itS32),
      wbFloat('Directional Fade'),
      wbFloat('Fog Clip Dist'),
      wbFloat('Fog Power')
    ], cpNormal, True)
  ]);

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
FO3:
  wbRecord(LIGH, 'Light',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start',
      25, 'Obstacle'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel,
    wbSCRI,
    wbDEST,
    wbFULL,
    wbICON,
    wbStruct(DATA, '', [
      wbInteger('Time', itS32),
      wbInteger('Radius', itU32),
      wbByteColors('Color'),
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Dynamic',
          1,  'Can Carry',
          2,  'Negative',
          3,  'Flicker',
          5,  'Off By Default',
          6,  'Flicker Slow',
          7,  'Pulse',
          8,  'Pulse Slow',
          9,  'Spot Light',
          10, 'Spot Shadow'
        ], False, 11))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat('Falloff Exponent').SetDefaultNativeValue(1),
      wbFloat('FOV').SetDefaultNativeValue(90),
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ]).SetRequired,
    wbFloat(FNAM, 'Fade value')
      .SetDefaultNativeValue(1.0)
      .SetRequired,
    wbFormIDCk(SNAM, 'Sound', [SOUN])
  ]);
FNV:
  wbRecord(LIGH, 'Light',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start',
      25, 'Obstacle'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel,
    wbSCRI,
    wbDEST,
    wbFULL,
    wbICON,
    wbStruct(DATA, '', [
      wbInteger('Time', itS32),
      wbInteger('Radius', itU32),
      wbByteColors('Color'),
      wbInteger('Flags', itU32, wbFlags([
        {0x00000001} 'Dynamic',
        {0x00000002} 'Can be Carried',
        {0x00000004} 'Negative',
        {0x00000008} 'Flicker',
        {0x00000010} 'Unused',
        {0x00000020} 'Off By Default',
        {0x00000040} 'Flicker Slow',
        {0x00000080} 'Pulse',
        {0x00000100} 'Pulse Slow',
        {0x00000200} 'Spot Light',
        {0x00000400} 'Spot Shadow'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat('Falloff Exponent')
        .SetDefaultNativeValue(1),
      wbFloat('FOV')
        .SetDefaultNativeValue(90),
      wbInteger('Value', itU32),
      wbFloat('Weight')
    ], cpNormal, True),
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
FO3:
  wbRecord(LSCR, 'Load Screen',
    wbFlags(wbFlagsList([
      10, 'Displays In Main Menu'
    ])), [
    wbEDIDReq,
    wbICONReq,
    wbDESCReq,
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
  ]);
FNV:
  wbRecord(LSCR, 'Load Screen',
    wbFlags(wbFlagsList([
      10, 'Displays In Main Menu'
    ])), [
    wbEDIDReq,
    wbICONReq,
    wbDESCReq,
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
      ])),
    wbFormIDCk(WMI1, 'Load Screen Type', [LSCT])
  ]);

# LSCT.Load Screen Type
FNV:
  wbRecord(LSCT, 'Load Screen Type', [
    wbEDIDReq,
    wbStruct(DATA, 'Data', [
      wbInteger('Type', itU32, wbEnum([
        'None',
        'XP Progress',
        'Objective',
        'Tip',
        'Stats'
      ])),
      wbStruct('Data 1', [
        wbInteger('X', itU32),
        wbInteger('Y', itU32),
        wbInteger('Width', itU32),
        wbInteger('Height', itU32),
        wbFloatAngle('Orientation', cpNormal, True),
        wbInteger('Font', itU32, wbEnum([
          '',
          '2',
          '3',
          '4',
          '5',
          '6',
          '7',
          '8'
        ])),
        wbStruct('Font Color', [
          wbFloat('R'),
          wbFloat('G'),
          wbFloat('B')
        ]),
        wbInteger('Font', itU32, wbEnum([
          '',
          'Left',
          'Center',
          '',
          'Right'
        ]))
      ]),
      wbByteArray('Unknown', 20),
      wbStruct('Data 2', [
        wbInteger('Font', itU32, wbEnum([
          '',
          '2',
          '3',
          '4',
          '5',
          '6',
          '7',
          '8'
        ])),
        wbStruct('Font Color', [
          wbFloat('R'),
          wbFloat('G'),
          wbFloat('B')
        ]),
        wbByteArray('', 4),
        wbInteger('Stats', itU32, wbEnum([
          '',
          '2',
          '3',
          '4',
          '5',
          '6',
          '7',
          '8'
        ]))
      ])
    ])
  ]);

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
FO3:
  wbRecord(LTEX, 'Landscape Texture', [
    wbEDIDReq,
    wbICON,
    wbFormIDCk(TNAM, 'Texture', [TXST]).SetRequired,
    wbStruct(HNAM, 'Havok Data', [
      wbInteger('Material Type', itU8,
        wbEnum([
          {0}  'STONE',
          {1}  'CLOTH',
          {2}  'DIRT',
          {3}  'GLASS',
          {4}  'GRASS',
          {5}  'METAL',
          {6}  'ORGANIC',
          {7}  'SKIN',
          {8}  'WATER',
          {9}  'WOOD',
          {10} 'HEAVY STONE',
          {11} 'HEAVY METAL',
          {12} 'HEAVY WOOD',
          {13} 'CHAIN',
          {14} 'SNOW',
          {15} 'ELEVATOR',
          {16} 'HOLLOW METAL',
          {17} 'SHEET METAL',
          {18} 'SAND',
          {19} 'BRIKEN CONCRETE',
          {20} 'VEHILCE BODY',
          {21} 'VEHILCE PART SOLID',
          {22} 'VEHILCE PART HOLLOW',
          {23} 'BARREL',
          {24} 'BOTTLE',
          {25} 'SODA CAN',
          {26} 'PISTOL',
          {27} 'RIFLE',
          {28} 'SHOPPING CART',
          {29} 'LUNCHBOX',
          {30} 'BABY RATTLE',
          {31} 'RUBER BALL'
        ])),
      wbInteger('Friction', itU8),
      wbInteger('Restitution', itU8)
    ]).SetRequired,
    wbInteger(SNAM, 'Texture Specular Exponent', itU8).SetRequired,
    wbRArrayS('Grasses', wbFormIDCk(GNAM, 'Grass', [GRAS]))
  ]);
FNV:
  wbRecord(LTEX, 'Landscape Texture', [
    wbEDIDReq,
    wbICON,
    wbFormIDCk(TNAM, 'Texture', [TXST], False, cpNormal, True),
    wbStruct(HNAM, 'Havok Data', [
      wbInteger('Material Type', itU8, wbEnum([
        {00} 'STONE',
        {01} 'CLOTH',
        {02} 'DIRT',
        {03} 'GLASS',
        {04} 'GRASS',
        {05} 'METAL',
        {06} 'ORGANIC',
        {07} 'SKIN',
        {08} 'WATER',
        {09} 'WOOD',
        {10} 'HEAVY STONE',
        {11} 'HEAVY METAL',
        {12} 'HEAVY WOOD',
        {13} 'CHAIN',
        {14} 'SNOW',
        {15} 'ELEVATOR',
        {16} 'HOLLOW METAL',
        {17} 'SHEET METAL',
        {18} 'SAND',
        {19} 'BRIKEN CONCRETE',
        {20} 'VEHILCE BODY',
        {21} 'VEHILCE PART SOLID',
        {22} 'VEHILCE PART HOLLOW',
        {23} 'BARREL',
        {24} 'BOTTLE',
        {25} 'SODA CAN',
        {26} 'PISTOL',
        {27} 'RIFLE',
        {28} 'SHOPPING CART',
        {29} 'LUNCHBOX',
        {30} 'BABY RATTLE',
        {31} 'RUBER BALL'
      ])),
      wbInteger('Friction', itU8),
      wbInteger('Restitution', itU8)
    ], cpNormal, True),
    wbInteger(SNAM, 'Texture Specular Exponent', itU8, nil, cpNormal, True),
    wbRArrayS('Grasses', wbFormIDCk(GNAM, 'Grass', [GRAS]))
  ]);

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
FO3:
  wbRecord(LVLC, 'Leveled Creature', [
    wbEDIDReq,
    wbOBND(True),
    wbInteger(LVLD, 'Chance none', itU8).SetRequired,
    wbInteger(LVLF, 'Flags', itU8,
      wbFlags([
        {0} 'Calculate from all levels <= player''s level',
        {1} 'Calculate for each item in count'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Leveled List Entries',
      wbRStructExSK([0], [1], 'Leveled List Entry', [
        wbLeveledListEntry('Creature', [CREA, LVLC]),
        wbCOED
      ]).SetSummaryMemberMaxDepth(0, 1)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    ),
    wbGenericModel
  ]);
FNV:
  wbRecord(LVLC, 'Leveled Creature', [
    wbEDIDReq,
    wbOBND(True),
    wbInteger(LVLD, 'Chance none', itU8, nil, cpNormal, True),
    wbInteger(LVLF, 'Flags', itU8, wbFlags([
      {0x01} 'Calculate from all levels <= player''s level',
      {0x02} 'Calculate for each item in count'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Leveled List Entries',
      wbRStructExSK([0], [1], 'Leveled List Entry', [
        wbLeveledListEntry('Creature', [CREA, LVLC]),
        wbCOED
      ]).SetSummaryMemberMaxDepth(0, 1)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    ),
    wbGenericModel
  ]);

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
FO3:
   wbRecord(LVLI, 'Leveled Item', [
    wbEDIDReq,
    wbOBND(True),
    wbInteger(LVLD, 'Chance none', itU8).SetRequired,
    wbInteger(LVLF, 'Flags', itU8,
      wbFlags([
        {0} 'Calculate from all levels <= player''s level',
        {1} 'Calculate for each item in count',
        {2} 'Use All'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbFormIDCk(LVLG, 'Global', [GLOB]),
    wbRArrayS('Leveled List Entries',
      wbRStructExSK([0], [1], 'Leveled List Entry', [
        wbLeveledListEntry('Item', [ALCH, AMMO, ARMO, BOOK, KEYM, LVLI, MISC, NOTE, WEAP]),
        wbCOED
      ]).SetSummaryMemberMaxDepth(0, 1)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    )
  ]);
FNV:
   wbRecord(LVLI, 'Leveled Item', [
    wbEDIDReq,
    wbOBND(True),
    wbInteger(LVLD, 'Chance none', itU8, nil, cpNormal, True),
    wbInteger(LVLF, 'Flags', itU8, wbFlags([
      {0x01} 'Calculate from all levels <= player''s level',
      {0x02} 'Calculate for each item in count',
      {0x04} 'Use All'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbFormIDCk(LVLG, 'Global', [GLOB]),
    wbRArrayS('Leveled List Entries',
      wbRStructExSK([0], [1], 'Leveled List Entry', [
        wbLeveledListEntry('Item', [ALCH, AMMO, ARMO, BOOK, CCRD, CHIP, CMNY, IMOD, KEYM, LVLI, MISC, NOTE, WEAP]),
        wbCOED
      ]).SetSummaryMemberMaxDepth(0, 1)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    )
  ]);

# LVLN.Leveled Actor
FO3:
  wbRecord(LVLN, 'Leveled NPC', [
    wbEDIDReq,
    wbOBND(True),
    wbInteger(LVLD, 'Chance none', itU8).SetRequired,
    wbInteger(LVLF, 'Flags', itU8,
      wbFlags([
        {0} 'Calculate from all levels <= player''s level',
        {1} 'Calculate for each item in count'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Leveled List Entries',
      wbRStructExSK([0], [1], 'Leveled List Entry', [
        wbLeveledListEntry('NPC', [LVLN, NPC_]),
        wbCOED
      ]).SetSummaryMemberMaxDepth(0, 1)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    ),
    wbGenericModel
  ]);
FNV:
  wbRecord(LVLN, 'Leveled NPC', [
    wbEDIDReq,
    wbOBND(True),
    wbInteger(LVLD, 'Chance none', itU8, nil, cpNormal, True),
    wbInteger(LVLF, 'Flags', itU8, wbFlags([
      {0x01} 'Calculate from all levels <= player''s level',
      {0x02} 'Calculate for each item in count'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbRArrayS('Leveled List Entries',
      wbRStructExSK([0], [1], 'Leveled List Entry', [
        wbLeveledListEntry('NPC', [LVLN, NPC_]),
        wbCOED
      ]).SetSummaryMemberMaxDepth(0, 1)
        .IncludeFlag(dfCollapsed, wbCollapseLeveledItems)
    ),
    wbGenericModel
  ]);

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

# MESG.Message
FO3:
  wbRecord(MESG, 'Message', [
    wbEDIDReq,
    wbDESCReq,
    wbFULL,
    wbFormIDCk(INAM, 'Icon', [MICN, NULL]).SetRequired,
    wbByteArray(NAM0, 'Unused', 0, cpIgnore),
    wbByteArray(NAM1, 'Unused', 0, cpIgnore),
    wbByteArray(NAM2, 'Unused', 0, cpIgnore),
    wbByteArray(NAM3, 'Unused', 0, cpIgnore),
    wbByteArray(NAM4, 'Unused', 0, cpIgnore),
    wbByteArray(NAM5, 'Unused', 0, cpIgnore),
    wbByteArray(NAM6, 'Unused', 0, cpIgnore),
    wbByteArray(NAM7, 'Unused', 0, cpIgnore),
    wbByteArray(NAM8, 'Unused', 0, cpIgnore),
    wbByteArray(NAM9, 'Unused', 0, cpIgnore),
    wbInteger(DNAM, 'Flags', itU32,
      wbFlags([
        {0} 'Message Box',
        {1} 'Auto Display'
      ])).SetAfterSet(wbMESGDNAMAfterSet)
         .SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbInteger(TNAM, 'Display Time', itU32).SetDontShow(wbMESGTNAMDontShow),
    wbRArray('Menu Buttons',
      wbRStruct('Menu Button', [
        wbStringKC(ITXT, 'Button Text', 0, cpTranslate),
        wbConditions
      ]))
  ]).SetAfterLoad(wbMESGAfterLoad);
FNV:
  wbRecord(MESG, 'Message', [
    wbEDIDReq,
    wbDESCReq,
    wbFULL,
    wbFormIDCk(INAM, 'Icon', [MICN, NULL], False, cpNormal, True),
    wbByteArray(NAM0, 'Unused', 0, cpIgnore),
    wbByteArray(NAM1, 'Unused', 0, cpIgnore),
    wbByteArray(NAM2, 'Unused', 0, cpIgnore),
    wbByteArray(NAM3, 'Unused', 0, cpIgnore),
    wbByteArray(NAM4, 'Unused', 0, cpIgnore),
    wbByteArray(NAM5, 'Unused', 0, cpIgnore),
    wbByteArray(NAM6, 'Unused', 0, cpIgnore),
    wbByteArray(NAM7, 'Unused', 0, cpIgnore),
    wbByteArray(NAM8, 'Unused', 0, cpIgnore),
    wbByteArray(NAM9, 'Unused', 0, cpIgnore),
    wbInteger(DNAM, 'Flags', itU32, wbFlags([
      'Message Box',
      'Auto Display'
    ]), cpNormal, True, False, nil, wbMESGDNAMAfterSet).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbInteger(TNAM, 'Display Time', itU32, nil, cpNormal, False, False, wbMESGTNAMDontShow),
    wbRArray('Menu Buttons', wbMenuButton)
  ], False, nil, cpNormal, False, wbMESGAfterLoad);

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
FO3:
  wbRecord(MGEF, 'Base Effect', [
    wbEDIDReq,
    wbFULL,
    wbDESCReq,
    wbICON,
    wbGenericModel,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Hostile',
          1,  'Recover',
          2,  'Detrimental',
          4,  'Self',
          5,  'Touch',
          6,  'Target',
          7,  'No Duration',
          8,  'No Magnitude',
          9,  'No Area',
          10, 'FX Persist',
          12, 'Gory Visuals',
          13, 'Display Name Only',
          15, 'Radio Broadcast ??',
          19, 'Use skill',
          20, 'Use attribute',
          24, 'Painless',
          25, 'Spray projectile type (or Fog if Bolt is specified as well)',
          26, 'Bolt projectile type (or Fog if Spray is specified as well)',
          27, 'No Hit Effect',
          28, 'No Death Dispel',
          29, '????'
        ], False, 30))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {04} wbFloat('Base cost (Unused)'),
      {08} wbUnion('Assoc. Item', wbMGEFFAssocItemDecider, [
             wbFormID('Unused', cpIgnore),
             wbFormID('Assoc. Item'),
             wbFormIDCk('Assoc. Script', [SCPT, NULL]), //Script
             wbFormIDCk('Assoc. Item', [WEAP, ARMO, NULL]), //Bound Item
             wbFormIDCk('Assoc. Creature', [CREA]) //Summon Creature
           ]).SetAfterSet(wbMGEFAssocItemAfterSet),
      {12} wbByteArray('Magic School (Unused)', 4),
      {16} wbInteger('Resistance Type', itS32, wbActorValueEnum),
      {20} wbInteger('Counter Effect Count', itU16),
      {22} wbUnused(2),
      {24} wbFormIDCk('Light', [LIGH, NULL]),
      {28} wbFloat('Projectile speed'),
      {32} wbFormIDCk('Effect Shader', [EFSH, NULL]),
      {36} wbFormIDCk('Object Display Shader', [EFSH, NULL]),
      {40} wbFormIDCk('Effect sound', [NULL, SOUN]),
      {44} wbFormIDCk('Bolt sound', [NULL, SOUN]),
      {48} wbFormIDCk('Hit sound', [NULL, SOUN]),
      {52} wbFormIDCk('Area sound', [NULL, SOUN]),
      {56} wbFloat('Constant Effect enchantment factor  (Unused)'),
      {60} wbFloat('Constant Effect barter factor (Unused)'),
      {64} wbInteger('Archtype', itU32, wbArchtypeEnum).SetAfterSet(wbMGEFArchtypeAfterSet),
      {68} wbActorValue
    ]).SetRequired,
    wbRArrayS('Counter Effects',
      wbFormIDCk(ESCE, 'Effect', [MGEF])
    ).SetCountPath('DATA\Counter Effect Count')
  ]).SetAfterLoad(wbMGEFAfterLoad);
FNV:
  wbRecord(MGEF, 'Base Effect', [
    wbEDIDReq,
    wbFULL,
    wbDESCReq,
    wbICON,
    wbGenericModel,
    wbStruct(DATA, 'Data', [
      wbInteger('Flags', itU32, wbFlags([
        {0x00000001} 'Hostile',
        {0x00000002} 'Recover',
        {0x00000004} 'Detrimental',
        {0x00000008} '',
        {0x00000010} 'Self',
        {0x00000020} 'Touch',
        {0x00000040} 'Target',
        {0x00000080} 'No Duration',
        {0x00000100} 'No Magnitude',
        {0x00000200} 'No Area',
        {0x00000400} 'FX Persist',
        {0x00000800} '',
        {0x00001000} 'Gory Visuals',
        {0x00002000} 'Display Name Only',
        {0x00004000} '',
        {0x00008000} 'Radio Broadcast ??',
        {0x00010000} '',
        {0x00020000} '',
        {0x00040000} '',
        {0x00080000} 'Use skill',
        {0x00100000} 'Use attribute',
        {0x00200000} '',
        {0x00400000} '',
        {0x00800000} '',
        {0x01000000} 'Painless',
        {0x02000000} 'Spray projectile type (or Fog if Bolt is specified as well)',
        {0x04000000} 'Bolt projectile type (or Fog if Spray is specified as well)',
        {0x08000000} 'No Hit Effect',
        {0x10000000} 'No Death Dispel',
        {0x20000000} '????'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {04} wbFloat('Base cost (Unused)'),
      {08} wbUnion('Assoc. Item', wbMGEFFAssocItemDecider, [
             wbFormID('Unused', cpIgnore),
             wbFormID('Assoc. Item'),
             wbFormIDCk('Assoc. Script', [SCPT, NULL]), //Script
             wbFormIDCk('Assoc. Item', [WEAP, ARMO, NULL]), //Bound Item
             wbFormIDCk('Assoc. Creature', [CREA]) //Summon Creature
           ], cpNormal, false, nil, wbMGEFFAssocItemAfterSet),
      {12} wbInteger('Magic School (Unused)', itS32, wbEnum([
      ], [
        -1, 'None'
      ])),
      {16} wbInteger('Resistance Type', itS32, wbActorValueEnum),
      {20} wbInteger('Counter Effect Count', itU16),
      {22} wbUnused(2),
      {24} wbFormIDCk('Light', [LIGH, NULL]),
      {28} wbFloat('Projectile speed'),
      {32} wbFormIDCk('Effect Shader', [EFSH, NULL]),
      {36} wbFormIDCk('Object Display Shader', [EFSH, NULL]),
      {40} wbFormIDCk('Effect sound', [NULL, SOUN]),
      {44} wbFormIDCk('Bolt sound', [NULL, SOUN]),
      {48} wbFormIDCk('Hit sound', [NULL, SOUN]),
      {52} wbFormIDCk('Area sound', [NULL, SOUN]),
      {56} wbFloat('Constant Effect enchantment factor  (Unused)'),
      {60} wbFloat('Constant Effect barter factor (Unused)'),
      {64} wbInteger('Archtype', itU32, wbArchtypeEnum, cpNormal, False, nil, wbMGEFArchtypeAfterSet),
      {68} wbActorValue
    ], cpNormal, True),
    wbRArrayS('Counter Effects',
      wbFormIDCk(ESCE, 'Effect', [MGEF])
    ).SetCountPath('DATA\Counter Effect Count')
  ], False, nil, cpNormal, False, wbMGEFAfterLoad);

# MICN.Menu Icon
FO3:
  wbRecord(MICN, 'Menu Icon', [
    wbEDIDReq,
    wbICONReq
  ]);
FNV:
  wbRecord(MICN, 'Menu Icon', [
    wbEDIDReq,
    wbICONReq
  ]);

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
FO3:
  wbRecord(MISC, 'Misc. Item',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ]).SetRequired
  ]);
FNV:
  wbRecord(MISC, 'Misc. Item',
    wbFlags(wbFlagsList([
      10, 'Quest Item'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbDEST,
    wbYNAM,
    wbZNAM,
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbFloat('Weight')
    ], cpNormal, True),
    wbFormIDCk(RNAM, 'Sound - Random/Looping', [SOUN])
  ]);

# MOVT.Movement Type

# MSET.Media Set
FNV:
  wbRecord(MSET, 'Media Set', [
    wbEDIDReq,
    wbFULL,
    wbInteger(NAM1, 'Type', itU32, wbEnum([
      'Battle Set',
      'Location Set',
      'Dungeon Set',
      'Incidental Set'
    ], [
      -1, 'No Set'
    ])),
    wbString(NAM2, 'Loop (B) / Battle (D) / Day Outer (L)'),
    wbString(NAM3, 'Explore (D) / Day Middle (L)'),
    wbString(NAM4, 'Suspense (D) / Day Inner (L)'),
    wbString(NAM5, 'Night Outer (L)'),
    wbString(NAM6, 'Night Middle (L)'),
    wbString(NAM7, 'Night Inner (L)'),
    wbFloat(NAM8, 'Loop dB (B) / Battle dB (D) / Day Outer dB (L)'),
    wbFloat(NAM9, 'Explore dB (D) / Day Middle dB (L)'),
    wbFloat(NAM0, 'Suspense dB (D) / Day Inner dB (L)'),
    wbFloat(ANAM, 'Night Outer dB (L)'),
    wbFloat(BNAM, 'Night Middle dB (L)'),
    wbFloat(CNAM, 'Night Inner dB (L)'),
    wbFloat(JNAM, 'Day Outer Boundary % (L)'),
    wbFloat(KNAM, 'Day Middle Boundary % (L)'),
    wbFloat(LNAM, 'Day Inner Boundary % (L)'),
    wbFloat(MNAM, 'Night Outer Boundary % (L)'),
    wbFloat(NNAM, 'Night Middle Boundary % (L)'),
    wbFloat(ONAM, 'Night Inner Boundary % (L)'),
    wbInteger(PNAM, 'Enable Flags', itU8,
      wbFlags(wbSparseFlags([
        0, 'Day Outer',
        1, 'Day Middle',
        2, 'Day Inner',
        3, 'Night Outer',
        4, 'Night Middle',
        5, 'Night Inner',
        6, 'Unknown 6',
        7, 'Unknown 7'
    ]))).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbFloat(DNAM, 'Wait Time (B) / Minimum Time On (D,L) / Daytime Min (I)'),
    wbFloat(ENAM, 'Loop Fade Out (B) / Looping/Random Crossfade Overlap (D,L) / Nighttime Min (I)'),
    wbFloat(FNAM, 'Recovery Time (B) / Layer Crossfade Time (D,L) / Daytime Max (I)'),
    wbFloat(GNAM, 'Nighttime Max (I)'),
    wbFormIDCk(HNAM, 'Intro (B,D) / Daytime (I)', [SOUN]),
    wbFormIDCk(INAM, 'Outro (B,D) / Nighttime (I)', [SOUN]),
    wbUnknown(DATA)
  ]);

# MSTT.Movable Static
FO3:
  wbRecord(MSTT, 'Moveable Static',
    wbFlags(wbFlagsList([
       9, 'On Local Map',
      10, 'Quest Item',
      16, 'Random Anim Start',
      25, 'Obstacle'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbDEST,
    wbInteger(DATA, 'On Local Map', itU8, wbBoolEnum).SetRequired,
    wbFormIDCk(SNAM, 'Sound', [SOUN])
  ]);
FNV:
  wbRecord(MSTT, 'Moveable Static',
    wbFlags(wbFlagsList([
       9, 'On Local Map',
      10, 'Quest Item',
      16, 'Random Anim Start',
      25, 'Obstacle'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbDEST,
    wbInteger(DATA, 'On Local Map', itU8, wbBoolEnum).SetRequired,
    wbFormIDCk(SNAM, 'Sound', [SOUN])
  ]);

# MUSC.Music Type
FO3:
  wbRecord(MUSC, 'Music Type', [
    wbEDIDReq,
    wbString(FNAM, 'FileName')
  ]);
FNV:
  wbRecord(MUSC, 'Music Type', [
    wbEDIDReq,
    wbString(FNAM, 'FileName'),
    wbFloat(ANAM, 'dB (positive = Loop)')
  ]);

# MUST.Music Track

# NAVI.Navigation
FO3:
  wbRecord(NAVI, 'Navmesh Info Map', [
    wbEDID,
    wbInteger(NVER, 'Version', itU32),
    wbRArrayS('Navmesh Infos',
      wbStructSK(NVMI, [1], 'Navmesh Info', [
        wbInteger('Flags', itU32,
          wbFlags(wbSparseFlags([
            4, 'Initially Disabled',
            5, 'Is Island'
          ], False, 6))).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbFormIDCk('Navmesh', [NAVM]).IncludeFlag(dfSummaryNoName),
        wbFormIDCk('Location', [CELL, WRLD]),
        wbStruct('Coordinates', [
          wbInteger('Grid Y', itS16),
          wbInteger('Grid X', itS16)
        ]).SetSummaryKey([1, 0])
          .SetSummaryMemberPrefixSuffix(0, 'Y: ', '>')
          .SetSummaryMemberPrefixSuffix(1, '<X: ', '')
          .SetSummaryDelimiter(', ')
          .IncludeFlag(dfCollapsed, wbCollapsePlacement)
          .IncludeFlag(dfSummaryMembersNoName),
        wbVec3('Approx Location'),
        wbUnion('Island Data', wbNAVINVMIDecider, [
          wbStruct('Unused', [wbEmpty('Unused')])
            .SetDontShow(wbNeverShow)
            .IncludeFlag(dfCollapsed, wbCollapseOther),
          wbStruct('Island Data', [
            wbStruct('Navmesh Bounds', [
              wbVec3('Min'),
              wbVec3('Max')
            ]),
            wbInteger('Vertex Count', itU16),
            wbInteger('Triangle Count', itU16),
            wbArray('Vertices',
              wbVec3('Vertex')
            ).SetCountPath('Vertex Count', True)
             .IncludeFlag(dfCollapsed, wbCollapseVertices)
             .IncludeFlag(dfNotAlignable),
            wbArray('Triangles',
              wbStruct('Triangle', [
                wbInteger('Vertex 0', itU16),
                wbInteger('Vertex 1', itU16),
                wbInteger('Vertex 2', itU16)
              ]).IncludeFlag(dfCollapsed, wbCollapseVertices)
            ).SetCountPath('Triangle Count', True)
             .IncludeFlag(dfCollapsed, wbCollapseVertices)
             .IncludeFlag(dfNotAlignable)
          ]).SetSummaryKey([4])
            .IncludeFlag(dfCollapsed, wbCollapseNavmesh)
        ]).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
        wbFloat('Preferred %')
      ]).SetSummaryKeyOnValue([1,2,5])
        .SetSummaryPrefixSuffixOnValue(1, '', '')
        .SetSummaryPrefixSuffixOnValue(2, 'in ', '')
        .SetSummaryPrefixSuffixOnValue(5, 'is island with ', '')
        .IncludeFlag(dfCollapsed, wbCollapseNavmesh)
    ).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
    wbRArrayS('Navmesh Connections',
      wbStructSK(NVCI, [0], 'Connection', [
        wbFormIDCk('Navmesh', [NAVM]),
        wbArrayS('Standard', wbFormIDCk('Navmesh', [NAVM]), -1).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
        wbArrayS('Preferred', wbFormIDCk('Navmesh', [NAVM]), -1).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
        wbArrayS('Door Links', wbFormIDCk('Door', [REFR]), -1).IncludeFlag(dfCollapsed, wbCollapseNavmesh)
      ]).IncludeFlag(dfCollapsed, wbCollapseNavmesh)
    ).IncludeFlag(dfCollapsed, wbCollapseNavmesh)
  ]);
FNV:
  wbRecord(NAVI, 'Navmesh Info Map', [
    wbEDID,
    wbInteger(NVER, 'Version', itU32),
    wbRArrayS('Navmesh Infos',
      wbStructSK(NVMI, [1], 'Navmesh Info', [
        wbInteger('Flags', itU32,
          wbFlags(wbSparseFlags([
            4, 'Initially Disabled',
            5, 'Is Island'
          ], False, 6))).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbFormIDCk('Navmesh', [NAVM]).IncludeFlag(dfSummaryNoName),
        wbFormIDCk('Location', [CELL, WRLD]),
        wbStruct('Coordinates', [
          wbInteger('Grid Y', itS16),
          wbInteger('Grid X', itS16)
        ]).SetSummaryKey([1, 0])
          .SetSummaryMemberPrefixSuffix(0, 'Y: ', '>')
          .SetSummaryMemberPrefixSuffix(1, '<X: ', '')
          .SetSummaryDelimiter(', ')
          .IncludeFlag(dfCollapsed, wbCollapsePlacement)
          .IncludeFlag(dfSummaryMembersNoName),
        wbVec3('Approx Location'),
        wbUnion('Island Data', wbNAVINVMIDecider, [
          wbStruct('Unused', [wbEmpty('Unused')])
            .SetDontShow(wbNeverShow)
            .IncludeFlag(dfCollapsed, wbCollapseOther),
          wbStruct('Island Data', [
            wbStruct('Navmesh Bounds', [
              wbVec3('Min'),
              wbVec3('Max')
            ]),
            wbInteger('Vertex Count', itU16),
            wbInteger('Triangle Count', itU16),
            wbArray('Vertices',
              wbVec3('Vertex')
            ).SetCountPath('Vertex Count', True)
             .IncludeFlag(dfCollapsed, wbCollapseVertices)
             .IncludeFlag(dfNotAlignable),
            wbArray('Triangles',
              wbStruct('Triangle', [
                wbInteger('Vertex 0', itU16),
                wbInteger('Vertex 1', itU16),
                wbInteger('Vertex 2', itU16)
              ]).IncludeFlag(dfCollapsed, wbCollapseVertices)
            ).SetCountPath('Triangle Count', True)
             .IncludeFlag(dfCollapsed, wbCollapseVertices)
             .IncludeFlag(dfNotAlignable)
          ]).SetSummaryKey([4])
            .IncludeFlag(dfCollapsed, wbCollapseNavmesh)
        ]).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
        wbFloat('Preferred %')
      ]).SetSummaryKeyOnValue([1,2,5])
        .SetSummaryPrefixSuffixOnValue(1, '', '')
        .SetSummaryPrefixSuffixOnValue(2, 'in ', '')
        .SetSummaryPrefixSuffixOnValue(5, 'is island with ', '')
        .IncludeFlag(dfCollapsed, wbCollapseNavmesh)
    ).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
    wbRArrayS('Navmesh Connections',
      wbStructSK(NVCI, [0], 'Connection', [
        wbFormIDCk('Navmesh', [NAVM]),
        wbArrayS('Standard', wbFormIDCk('Navmesh', [NAVM]), -1).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
        wbArrayS('Preferred', wbFormIDCk('Navmesh', [NAVM]), -1).IncludeFlag(dfCollapsed, wbCollapseNavmesh),
        wbArrayS('Door Links', wbFormIDCk('Door', [REFR]), -1).IncludeFlag(dfCollapsed, wbCollapseNavmesh)
      ]).IncludeFlag(dfCollapsed, wbCollapseNavmesh)
    ).IncludeFlag(dfCollapsed, wbCollapseNavmesh)
  ]);

# NAVM.NavMesh
FO3:
  wbRecord(NAVM, 'Navmesh',
    wbFlags(wbFlagsList([
      11, 'Initially Disabled',
      26, 'Autogen'
    ])), [
    wbEDID,
    wbInteger(NVER, 'Version', itU32),
    wbStruct(DATA, 'Data', [
      wbFormIDCk('Cell', [CELL]),
      wbInteger('Vertex Count', itU32),
      wbInteger('Triangle Count', itU32),
      wbInteger('Edge Link Count', itU32),
      wbInteger('Cover Triangle Count', itU32),
      wbInteger('Door Link Count', itU32)
    ]),
    IfThen(wbSimpleRecords,
      wbArray(NVVX, 'Vertices',
        wbByteArray('Vertex', 12)
      ).SetCountPathOnValue('DATA\Vertex Count', False)
       .IncludeFlag(dfNotAlignable),
      wbArray(NVVX, 'Vertices',
        wbVec3('Vertex')
      ).SetCountPathOnValue('DATA\Vertex Count', False)
       .IncludeFlag(dfNotAlignable)
    ),
    IfThen(wbSimpleRecords,
      wbArray(NVTR, 'Triangles',
        wbByteArray('Triangle', 16)
      ).SetCountPathOnValue('DATA\Triangle Count', False)
       .IncludeFlag(dfNotAlignable),
      wbArray(NVTR, 'Triangles',
        wbStruct('Triangle', [
          wbInteger('Vertex 0', itU16),
          wbInteger('Vertex 1', itU16),
          wbInteger('Vertex 2', itU16),
          wbInteger('Edge 0-1', itS16, wbNVTREdgeToStr, wbNVTREdgeToInt),
          wbInteger('Edge 1-2', itS16, wbNVTREdgeToStr, wbNVTREdgeToInt),
          wbInteger('Edge 2-0', itS16, wbNVTREdgeToStr, wbNVTREdgeToInt),
          wbInteger('Flags', itU16, wbNavmeshTriangleFlags)
            .IncludeFlag(dfCollapsed, wbCollapseFlags),
          wbInteger('Cover Flags', itU16, wbNavmeshCoverFlags)
            .IncludeFlag(dfCollapsed, wbCollapseFlags)
        ])
      ).SetCountPathOnValue('DATA\Triangle Count', False)
       .IncludeFlag(dfNotAlignable)
    ),
    IfThen(wbSimpleRecords,
      wbArray(NVCA, 'Cover Triangles',
        wbByteArray('Cover Triangle', 2)
      ).SetCountPathOnValue('DATA\Cover Triangle Count', False)
       .IncludeFlag(dfNotAlignable),
      wbArray(NVCA, 'Cover Triangles',
        wbInteger('Cover Triangle', itU16)
      ).SetCountPathOnValue('DATA\Cover Triangle Count', False)
       .IncludeFlag(dfNotAlignable)
    ),
    wbArrayS(NVDP, 'Door Links',
      wbStructSK([1, 0], 'Door Link', [
        wbFormIDCk('Door Ref', [REFR]),
        wbInteger('Triangle', itU16),
        wbUnused(2)
      ])
    ).SetCountPathOnValue('DATA\Door Link Count', False)
     .IncludeFlag(dfNotAlignable),
    wbStruct(NVGD, 'Navmesh Grid', [
      wbInteger('Divisor', itU32),
      wbFloat('Max X Distance'),
      wbFloat('Max Y Distance'),
      wbStruct('Navmesh Bounds', [
        wbVec3('Min'),
        wbVec3('Max')
      ]),
      IfThen(wbSimpleRecords,
        wbArray('Cells',
          wbArray('Cell',
            wbByteArray('Triangle', 2),
          -2).IncludeFlag(dfNotAlignable)
        ).IncludeFlag(dfNotAlignable),
        wbArray('Cells',
          wbArray('Cell',
            wbInteger('Triangle', itU16),
          -2).IncludeFlag(dfNotAlignable)
        ).IncludeFlag(dfNotAlignable)
      )
    ]),
    wbArray(NVEX, 'Edge Links',
      wbStruct('Edge Link', [
        wbInteger('Type', itU32, wbNavmeshEdgeLinkEnum, cpIgnore),
        wbFormIDCk('Navmesh', [NAVM]),
        wbInteger('Triangle', itU16)
      ])
    ).SetCountPathOnValue('DATA\Edge Link Count', False)
     .IncludeFlag(dfNotAlignable)
  ]).SetAddInfo(wbNAVMAddInfo);
FNV:
  wbRecord(NAVM, 'Navmesh',
    wbFlags(wbFlagsList([
      11, 'Initially Disabled',
      26, 'AutoGen'
    ])), [
    wbEDID,
    wbInteger(NVER, 'Version', itU32),
    wbStruct(DATA, 'Data', [
      wbFormIDCk('Cell', [CELL]),
      wbInteger('Vertex Count', itU32),
      wbInteger('Triangle Count', itU32),
      wbInteger('Edge Link Count', itU32),
      wbInteger('Cover Triangle Count', itU32),
      wbInteger('Door Link Count', itU32)
    ]),
    IfThen(wbSimpleRecords,
      wbArray(NVVX, 'Vertices',
        wbByteArray('Vertex', 12)
      ).SetCountPathOnValue('DATA\Vertex Count', False)
       .IncludeFlag(dfNotAlignable),
      wbArray(NVVX, 'Vertices',
        wbVec3('Vertex')
      ).SetCountPathOnValue('DATA\Vertex Count', False)
       .IncludeFlag(dfNotAlignable)
    ),
    IfThen(wbSimpleRecords,
      wbArray(NVTR, 'Triangles',
        wbByteArray('Triangle', 16)
      ).SetCountPathOnValue('DATA\Triangle Count', False)
       .IncludeFlag(dfNotAlignable),
      wbArray(NVTR, 'Triangles',
        wbStruct('Triangle', [
          wbInteger('Vertex 0', itU16),
          wbInteger('Vertex 1', itU16),
          wbInteger('Vertex 2', itU16),
          wbInteger('Edge 0-1', itS16, wbNVTREdgeToStr, wbNVTREdgeToInt),
          wbInteger('Edge 1-2', itS16, wbNVTREdgeToStr, wbNVTREdgeToInt),
          wbInteger('Edge 2-0', itS16, wbNVTREdgeToStr, wbNVTREdgeToInt),
          wbInteger('Flags', itU16, wbNavmeshTriangleFlags)
            .IncludeFlag(dfCollapsed, wbCollapseFlags),
          wbInteger('Cover Flags', itU16, wbNavmeshCoverFlags)
            .IncludeFlag(dfCollapsed, wbCollapseFlags)
        ])
      ).SetCountPathOnValue('DATA\Triangle Count', False)
       .IncludeFlag(dfNotAlignable)
    ),
    IfThen(wbSimpleRecords,
      wbArray(NVCA, 'Cover Triangles',
        wbByteArray('Cover Triangle', 2)
      ).SetCountPathOnValue('DATA\Cover Triangle Count', False)
       .IncludeFlag(dfNotAlignable),
      wbArray(NVCA, 'Cover Triangles',
        wbInteger('Cover Triangle', itU16)
      ).SetCountPathOnValue('DATA\Cover Triangle Count', False)
       .IncludeFlag(dfNotAlignable)
    ),
    wbArrayS(NVDP, 'Door Links',
      wbStructSK([1, 0], 'Door Link', [
        wbFormIDCk('Door Ref', [REFR]),
        wbInteger('Triangle', itU16),
        wbUnused(2)
      ])
    ).SetCountPathOnValue('DATA\Door Link Count', False)
     .IncludeFlag(dfNotAlignable),
    wbStruct(NVGD, 'Navmesh Grid', [
      wbInteger('Divisor', itU32),
      wbFloat('Max X Distance'),
      wbFloat('Max Y Distance'),
      wbStruct('Navmesh Bounds', [
        wbVec3('Min'),
        wbVec3('Max')
      ]),
      IfThen(wbSimpleRecords,
        wbArray('Cells',
          wbArray('Cell',
            wbByteArray('Triangle', 2),
          -2).IncludeFlag(dfNotAlignable)
        ).IncludeFlag(dfNotAlignable),
        wbArray('Cells',
          wbArray('Cell',
            wbInteger('Triangle', itU16),
          -2).IncludeFlag(dfNotAlignable)
        ).IncludeFlag(dfNotAlignable)
      )
    ]),
    wbArray(NVEX, 'Edge Links',
      wbStruct('Edge Link', [
        wbInteger('Type', itU32, wbNavmeshEdgeLinkEnum, cpIgnore),
        wbFormIDCk('Navmesh', [NAVM]),
        wbInteger('Triangle', itU16)
      ])
    ).SetCountPathOnValue('DATA\Edge Link Count', False)
     .IncludeFlag(dfNotAlignable)
  ]).SetAddInfo(wbNAVMAddInfo);

# NOTE.Note
FO3:
  wbRecord(NOTE, 'Note', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbYNAM,
    wbZNAM,
    wbInteger(DATA, 'Type', itU8,
      wbEnum([
        {0} 'Sound',
        {1} 'Text',
        {2} 'Image',
        {3} 'Voice'
      ])).SetRequired,
    wbRArrayS('Quests', wbFormIDCkNoReach(ONAM, 'Quest', [QUST])),
    wbString(XNAM, 'Texture'),
    wbUnion(TNAM, 'Text / Topic', wbNOTETNAMDecide, [
      wbStringKC('Text'),
      wbFormIDCk('Topic', [DIAL])
    ]),
    wbUnion(SNAM, 'Sound / NPC', wbNOTESNAMDecide, [
      wbFormIDCk('Sound', [SOUN]),
      wbFormIDCk('NPC', [NPC_])
    ])
  ]);
FNV:
  wbRecord(NOTE, 'Note', [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbYNAM,
    wbZNAM,
    wbInteger(DATA, 'Type', itU8, wbEnum([
      'Sound',
      'Text',
      'Image',
      'Voice'
    ]), cpNormal, True),
    wbRArrayS('Quests',
      wbFormIDCkNoReach(ONAM, 'Quest', [QUST])
    ),
    wbString(XNAM, 'Texture'),
    wbUnion(TNAM, 'Text / Topic', wbNOTETNAMDecide, [
      wbStringKC('Text'),
      wbFormIDCk('Topic', [DIAL])
    ]),
    wbUnion(SNAM, 'Sound / NPC', wbNOTESNAMDecide, [
      wbFormIDCk('Sound', [SOUN]),
      wbFormIDCk('Actor', [NPC_, CREA])
    ])
  ]);

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
FO3:
  wbRecord(NPC_, 'Non-Player Character',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      18, 'Compressed',
      19, 'Unknown 19'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL.SetDontShow(wbActorTemplateUseBaseData),
    wbGenericModel(False, wbActorTemplateUseModelAnimation),
    wbStruct(ACBS, 'Configuration', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Female',
          1,  'Essential',
          2,  'Is CharGen Face Preset',
          3,  'Respawn',
          4,  'Auto-calc stats',
          7,  'PC Level Mult',
          8,  'Use Template',
          9,  'No Low Level Processing',
          11, 'No Blood Spray',
          12, 'No Blood Decal',
          20, 'No VATS Melee',
          22, 'Can be all races',
          26, 'No Knockdowns',
          27, 'Not Pushable',
          30, 'No Rotating To Head-track'
        ], False, 31))
          .SetFlagHasDontShow(0,  wbActorTemplateUseTraits)
          .SetFlagHasDontShow(1,  wbActorTemplateUseBaseData)
          .SetFlagHasDontShow(3,  wbActorTemplateUseBaseData)
          .SetFlagHasDontShow(4,  wbActorTemplateUseStats)
          .SetFlagHasDontShow(7,  wbActorTemplateUseStats)
          .SetFlagHasDontShow(9,  wbActorTemplateUseBaseData)
          .SetFlagHasDontShow(11, wbActorTemplateUseModelAnimation)
          .SetFlagHasDontShow(12, wbActorTemplateUseModelAnimation)
          .SetFlagHasDontShow(27, wbActorTemplateUseModelAnimation)
          .SetFlagHasDontShow(30, wbActorTemplateUseModelAnimation)
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {04} wbInteger('Fatigue', itU16).SetDontShow(wbActorTemplateUseStats),
      {06} wbInteger('Barter gold', itU16).SetDontShow(wbActorTemplateUseAIData),
      {08} wbUnion('Level', wbACBSLevelDecider, [
             wbInteger('Level', itU16),
             wbInteger('Level Mult', itU16, wbDiv(1000, 2))
               .SetAfterLoad(wbACBSLevelMultAfterLoad)
               .SetDefaultNativeValue(1000)
           ]).SetAfterSet(wbACBSLevelMultAfterSet)
             .SetDontShow(wbActorTemplateUseStats),
      {10} wbInteger('Calc min', itU16).SetDontShow(wbActorTemplateUseStats),
      {12} wbInteger('Calc max', itU16).SetDontShow(wbActorTemplateUseStats),
      {14} wbInteger('Speed Multiplier', itU16).SetDontShow(wbActorTemplateUseStats),
      {16} wbFloat('Karma (Alignment)').SetDontShow(wbActorTemplateUseTraits),
      {20} wbInteger('Disposition Base', itS16).SetDontShow(wbActorTemplateUseTraits),
      {22} wbInteger('Template Flags', itU16, wbTemplateFlags).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbRArrayS('Factions', wbFaction).SetDontShow(wbActorTemplateUseFactions),
    wbFormIDCk(INAM, 'Death item', [LVLI]).SetDontShow(wbActorTemplateUseTraits),
    wbFormIDCk(VTCK, 'Voice', [VTYP])
      .SetDontShow(wbActorTemplateUseTraits)
      .SetRequired,
    wbFormIDCk(TPLT, 'Template', [LVLN, NPC_]),
    wbFormIDCk(RNAM, 'Race', [RACE])
      .SetDontShow(wbActorTemplateUseTraits)
      .SetRequired,
    wbSPLOs,
    wbFormIDCk(EITM, 'Unarmed Attack Effect', [ENCH, SPEL]).SetDontShow(wbActorTemplateUseActorEffectList),
    wbInteger(EAMT, 'Unarmed Attack Animation', itU16, wbAttackAnimationEnum)
      .SetDontShow(wbActorTemplateUseActorEffectList)
      .SetRequired,
    wbDEST.SetDontShow(wbActorTemplateUseModelAnimation),
    wbSCRI.SetDontShow(wbActorTemplateUseScript),
    wbCNTOs.SetDontShow(wbActorTemplateUseInventory),
    wbAIDT,
    wbRArray('Packages', wbFormIDCk(PKID, 'Package', [PACK])).SetDontShow(wbActorTemplateUseAIPackages),
    wbArrayS(KFFZ, 'Animations', wbStringLC('Animation')).SetDontShow(wbActorTemplateUseModelAnimation),
    wbFormIDCk(CNAM, 'Class', [CLAS])
      .SetDontShow(wbActorTemplateUseTraits)
      .SetRequired,
    wbStruct(DATA, '', [
      {00} wbInteger('Base Health', itS32),
      {04} wbArray('Attributes', wbInteger('Attribute', itU8), [
            'Strength',
            'Perception',
            'Endurance',
            'Charisma',
            'Intelligence',
            'Agility',
            'Luck'
          ]).SetDontShow(wbActorAutoCalcDontShow),
          wbByteArray('Unused'{, 14 - only present in old record versions})
    ]).SetDontShow(wbActorTemplateUseStats)
      .SetRequired,
    wbStruct(DNAM, '', [
      {00} wbArray('Skill Values', wbInteger('Skill', itU8), [
             'Barter',
             'Big Guns',
             'Energy Weapons',
             'Explosives',
             'Lockpick',
             'Medicine',
             'Melee Weapons',
             'Repair',
             'Science',
             'Small Guns',
             'Sneak',
             'Speech',
             'Throwing (unused)',
             'Unarmed'
           ]),
      {14} wbArray('Skill Offsets', wbInteger('Skill', itU8), [
             'Barter',
             'Big Guns',
             'Energy Weapons',
             'Explosives',
             'Lockpick',
             'Medicine',
             'Melee Weapons',
             'Repair',
             'Science',
             'Small Guns',
             'Sneak',
             'Speech',
             'Throwing (unused)',
             'Unarmed'
           ])
    ]).SetDontShow(wbActorTemplateUseStatsAutoCalc),
    wbRArrayS('Head Parts',
      wbFormIDCk(PNAM, 'Head Part', [HDPT])
    ).SetDontShow(wbActorTemplateUseModelAnimation),
    wbFormIDCk(HNAM, 'Hair', [HAIR]).SetDontShow(wbActorTemplateUseModelAnimation),
    wbFloat(LNAM, 'Hair length').SetDontShow(wbActorTemplateUseModelAnimation),
    wbFormIDCk(ENAM, 'Eyes', [EYES]).SetDontShow(wbActorTemplateUseModelAnimation),
    wbByteColors(HCLR, 'Hair color')
      .SetDontShow(wbActorTemplateUseModelAnimation)
      .SetRequired,
    wbFormIDCk(ZNAM, 'Combat Style', [CSTY]).SetDontShow(wbActorTemplateUseTraits),
    wbInteger(NAM4, 'Impact Material Type', itU32, wbActorImpactMaterialEnum).SetDontShow(wbActorTemplateUseModelAnimation).SetRequired,
    wbFaceGen.SetDontShow(wbActorTemplateUseModelAnimation),
    wbInteger(NAM5, 'Unknown', itU16)
      .SetDefaultNativeValue(255)
      .SetRequired,
    wbFloat(NAM6, 'Height')
      .SetDontShow(wbActorTemplateUseTraits)
      .SetRequired,
    wbFloat(NAM7, 'Weight')
      .SetDontShow(wbActorTemplateUseTraits)
      .SetRequired
  ], True).SetAfterLoad(wbNPCAfterLoad);
FNV:
  wbRecord(NPC_, 'Non-Player Character',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      18, 'Compressed',
      19, 'Unknown 19'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULLActor,
    wbGenericModel(False, wbActorTemplateUseModelAnimation),
    wbStruct(ACBS, 'Configuration', [
      {00} wbInteger('Flags', itU32, wbFlags([
             {0x000001} 'Female',
             {0x000002} 'Essential',
             {0x000004} 'Is CharGen Face Preset',
             {0x000008} 'Respawn',
             {0x000010} 'Auto-calc stats',
             {0x000020} '',
             {0x000040} '',
             {0x000080} 'PC Level Mult',
             {0x000100} 'Use Template',
             {0x000200} 'No Low Level Processing',
             {0x000400} '',
             {0x000800} 'No Blood Spray',
             {0x001000} 'No Blood Decal',
             {0x002000} '',
             {0x004000} '',
             {0x008000} '',
             {0x010000} '',
             {0x020000} '',
             {0x040000} '',
             {0x080000} '',
             {0x100000} 'No VATS Melee',
           {0x00200000} '',
           {0x00400000} 'Can be all races',
           {0x00800000} 'Autocalc Service',
           {0x01000000} '',
           {0x02000000} '',
           {0x04000000} 'No Knockdowns',
           {0x08000000} 'Not Pushable',
           {0x10000000} 'Unknown 28',
           {0x20000000} '',
           {0x40000000} 'No Rotating To Head-track',
           {0x80000000} ''
           ], [
             {0x000001 Female} wbActorTemplateUseTraits,
             {0x000002 Essential} wbActorTemplateUseBaseData,
             {0x000004 Is CharGen Face Preset} nil,
             {0x000008 Respawn} wbActorTemplateUseBaseData,
             {0x000010 Auto-calc stats} wbActorTemplateUseStats,
             {0x000020 } nil,
             {0x000040 } nil,
             {0x000080 PC Level Mult} wbActorTemplateUseStats,
             {0x000100 Use Template} nil,
             {0x000200 No Low Level Processing} wbActorTemplateUseBaseData,
             {0x000400 } nil,
             {0x000800 No Blood Spray} wbActorTemplateUseModelAnimation,
             {0x001000 No Blood Decal} wbActorTemplateUseModelAnimation,
             {0x002000 } nil,
             {0x004000 } nil,
             {0x008000 } nil,
             {0x010000 } nil,
             {0x020000 } nil,
             {0x040000 } nil,
             {0x080000 } nil,
             {0x100000 No VATS Melee} nil,
           {0x00200000 } nil,
           {0x00400000 Can be all races} nil,
           {0x00800000 } nil,
           {0x01000000 } nil,
           {0x02000000 } nil,
           {0x04000000 No Knockdowns} nil,
           {0x08000000 Not Pushable} wbActorTemplateUseModelAnimation,
           {0x10000000 } nil,
           {0x20000000 } nil,
           {0x40000000 No Rotating To Head-track} wbActorTemplateUseModelAnimation,
           {0x80000000 } nil
           ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {04} wbInteger('Fatigue', itU16, nil, cpNormal, True, wbActorTemplateUseStats),
      {06} wbInteger('Barter gold', itU16, nil, cpNormal, False, wbActorTemplateUseAIData),
      {08} wbUnion('Level', wbACBSLevelDecider, [
             wbInteger('Level', itU16),
             wbInteger('Level Mult', itU16, wbDiv(1000, 2))
               .SetAfterLoad(wbACBSLevelMultAfterLoad)
               .SetDefaultNativeValue(1000)
           ]).SetAfterSet(wbACBSLevelMultAfterSet)
             .SetDontShow(wbActorTemplateUseStats),
      {10} wbInteger('Calc min', itU16, nil, cpNormal, True, wbActorTemplateUseStats),
      {12} wbInteger('Calc max', itU16, nil, cpNormal, True, wbActorTemplateUseStats),
      {14} wbInteger('Speed Multiplier', itU16, nil, cpNormal, True, wbActorTemplateUseStats),
      {16} wbFloat('Karma (Alignment)', cpNormal, False, 1, -1, wbActorTemplateUseTraits),
      {20} wbInteger('Disposition Base', itS16, nil, cpNormal, False, wbActorTemplateUseTraits),
      {22} wbInteger('Template Flags', itU16, wbTemplateFlags).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True),
    wbRArrayS('Factions', wbFaction, cpNormal, False, nil, nil, wbActorTemplateUseFactions),
    wbFormIDCk(INAM, 'Death item', [LVLI], False, cpNormal, False, wbActorTemplateUseTraits),
    wbFormIDCk(VTCK, 'Voice', [VTYP], False, cpNormal, True, wbActorTemplateUseTraits),
    wbFormIDCk(TPLT, 'Template', [LVLN, NPC_]),
    wbFormIDCk(RNAM, 'Race', [RACE], False, cpNormal, True, wbActorTemplateUseTraits),
    wbSPLOs,
    wbFormIDCk(EITM, 'Unarmed Attack Effect', [ENCH, SPEL], False, cpNormal, False, wbActorTemplateUseActorEffectList),
    wbInteger(EAMT, 'Unarmed Attack Animation', itU16, wbAttackAnimationEnum, cpNormal, True, False, wbActorTemplateUseActorEffectList),
    wbDESTActor,
    wbSCRIActor,
    wbRArrayS('Items', wbCNTO, cpNormal, False, nil, nil, wbActorTemplateUseInventory),
    wbAIDT,
    wbRArray('Packages', wbFormIDCk(PKID, 'Package', [PACK]), cpNormal, False, nil, nil, wbActorTemplateUseAIPackages),
    wbArrayS(KFFZ, 'Animations', wbStringLC('Animation'), 0, cpNormal, False, nil, nil, wbActorTemplateUseModelAnimation),
    wbFormIDCk(CNAM, 'Class', [CLAS], False, cpNormal, True, wbActorTemplateUseTraits),
    wbStruct(DATA, '', [
      {00} wbInteger('Base Health', itS32),
      {04} wbArray('Attributes', wbInteger('Attribute', itU8), [
            'Strength',
            'Perception',
            'Endurance',
            'Charisma',
            'Intelligence',
            'Agility',
            'Luck'
          ], cpNormal, False, wbActorAutoCalcDontShow),
          wbByteArray('Unused'{, 14 - only present in old record versions})
    ], cpNormal, True, wbActorTemplateUseStats),
    wbStruct(DNAM, '', [
      {00} wbArray('Skill Values', wbInteger('Skill', itU8), [
             'Barter',
             'Big Guns',
             'Energy Weapons',
             'Explosives',
             'Lockpick',
             'Medicine',
             'Melee Weapons',
             'Repair',
             'Science',
             'Guns',
             'Sneak',
             'Speech',
             'Survival',
             'Unarmed'
           ]),
      {14} wbArray('Skill Offsets', wbInteger('Skill', itU8), [
             'Barter',
             'Big Guns',
             'Energy Weapons',
             'Explosives',
             'Lockpick',
             'Medicine',
             'Melee Weapons',
             'Repair',
             'Science',
             'Guns',
             'Sneak',
             'Speech',
             'Survival',
             'Unarmed'
           ])
    ], cpNormal, False, wbActorTemplateUseStatsAutoCalc),
    wbRArrayS('Head Parts',
      wbFormIDCk(PNAM, 'Head Part', [HDPT]),
    cpNormal, False, nil, nil, wbActorTemplateUseModelAnimation),
    wbFormIDCk(HNAM, 'Hair', [HAIR], False, cpNormal, False, wbActorTemplateUseModelAnimation),
    wbFloat(LNAM, 'Hair length', cpNormal, False, 1, -1, wbActorTemplateUseModelAnimation),
    wbFormIDCk(ENAM, 'Eyes', [EYES], False, cpNormal, False, wbActorTemplateUseModelAnimation),
    wbByteColors(HCLR, 'Hair color').SetRequired.SetDontShow(wbActorTemplateUseModelAnimation),
    wbFormIDCk(ZNAM, 'Combat Style', [CSTY], False, cpNormal, False, wbActorTemplateUseTraits),
    wbInteger(NAM4, 'Impact Material Type', itU32, wbActorImpactMaterialEnum, cpNormal, True, False, wbActorTemplateUseModelAnimation),
    wbFaceGen.SetDontShow(wbActorTemplateUseModelAnimation),
    wbInteger(NAM5, 'Unknown', itU16, nil, cpNormal, True, False, nil, nil, 255),
    wbFloat(NAM6, 'Height', cpNormal, True, 1, -1, wbActorTemplateUseTraits),
    wbFloat(NAM7, 'Weight', cpNormal, True, 1, -1, wbActorTemplateUseTraits)
  ], True, nil, cpNormal, False, wbNPCAfterLoad);

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
FO3:
  wbRecord(PACK, 'Package',
    wbFlags(wbFlagsList([
      27, 'Unknown 27'
    ])), [
    wbEDIDReq,
    wbStruct(PKDT, 'General', [
      wbInteger('General Flags', itU32, wbPackageFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Type', itU8, wbPackageTypeEnum),
      wbUnused(1),
      wbInteger('Fallout Behavior Flags', itU16,
        wbFlags(wbSparseFlags([
          0, 'Hellos To Player',
          1, 'Random Conversations',
          2, 'Observe Combat Behavior',
          4, 'Reaction To Player Actions',
          5, 'Friendly Fire Comments',
          6, 'Aggro Radius Behavior',
          7, 'Allow Idle Chatter',
          8, 'Avoid Radiation'
        ], False, 9), True)
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnion('Type Specific Flags', wbPKDTSpecificFlagsDecider, [
        wbEmpty('Type Specific Flags (missing)', cpIgnore, False, nil, True),
        wbInteger('Type Specific Flags - Find', itU16,
          wbFlags(wbSparseFlags([
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
          ], False, 11), True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Follow', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Escort', itU16,
          wbFlags(wbSparseFlags([
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
          ], False, 11), True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Eat', itU16,
          wbFlags(wbSparseFlags([
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
          ], False, 11), True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Sleep', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Wander', itU16,
          wbFlags([
            {0} 'No Eating',
            {1} 'No Sleeping',
            {2} 'No Conversation',
            {3} 'No Idle Markers',
            {4} 'No Furniture',
            {5} 'No Wandering'
          ], True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Travel', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Accompany', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Use Item At', itU16,
          wbFlags(wbSparseFlags([
            2, 'Sit Down',
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
          ], False, 11), True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Ambush', itU16,
          wbFlags(wbSparseFlags([
            0, 'Hide While Ambushing'
          ], False, 1), True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Flee Not Combat', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Cast Magic', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Sandbox', itU16,
          wbFlags([
            {0} 'No Eating',
            {1} 'No Sleeping',
            {2} 'No Conversation',
            {3} 'No Idle Markers',
            {4} 'No Furniture',
            {5} 'No Wandering'
          ], True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Patrol', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Guard', itU16,
          wbFlags(wbSparseFlags([
            3, 'Remain Near Reference to Guard'
          ], False, 4), True)
        ).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Dialogue', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Use Weapon', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ]),
      wbUnused(2)
    ], cpNormal, True, nil, 2),
    wbRStruct('Locations', [
      wbStruct(PLDT, 'Location 1', [
        wbInteger('Type', itU32,
          wbEnum([     // Byte + filler
            {0} 'Near reference',
            {1} 'In cell',
            {2} 'Near current location',
            {3} 'Near editor location',
            {4} 'Object ID',
            {5} 'Object Type',
            {6} 'Near linked reference',
            {7} 'At package location'
          ])),
        wbUnion('Location', wbPxDTLocationDecider, [
          wbFormIDCkNoReach('Reference', [REFR, PGRE, PMIS, PBEA, ACHR, ACRE, PLYR], True),
          wbFormIDCkNoReach('Cell', [CELL]),
          wbUnused(4),
          wbUnused(4),
          wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH]),
          wbInteger('Object Type', itU32, wbObjectTypeEnum),
          wbUnused(4),
          wbUnused(4)
        ]),
        wbInteger('Radius', itS32)
      ]),
      wbStruct(PLD2, 'Location 2', [
        wbInteger('Type', itU32,
          wbEnum([
            {0} 'Near reference',
            {1} 'In cell',
            {2} 'Near current location',
            {3} 'Near editor location',
            {4} 'Object ID',
            {5} 'Object Type',
            {6} 'Near linked reference',
            {7} 'At package location'
          ])),
        wbUnion('Location', wbPxDTLocationDecider, [
          wbFormIDCkNoReach('Reference', [REFR, PGRE, PMIS, PBEA, ACHR, ACRE, PLYR], True),
          wbFormIDCkNoReach('Cell', [CELL]),
          wbUnused(4),
          wbUnused(4),
          wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH]),
          wbInteger('Object Type', itU32, wbObjectTypeEnum),
          wbUnused(4),
          wbUnused(4)
        ]),
        wbInteger('Radius', itS32)
      ])
    ], [], cpNormal, False, nil, True),
    wbStruct(PSDT, 'Schedule', [
      wbInteger('Month', itS8),
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
        ])),
      wbInteger('Date', itU8),
      wbInteger('Time', itS8),
      wbInteger('Duration', itS32)
    ]).SetRequired,
    wbStruct(PTDT, 'Target 1', [
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'Specific Reference',
          {1} 'Object ID',
          {2} 'Object Type',
          {3} 'Linked Reference'
        ])).SetDefaultNativeValue(2),
      wbUnion('Target', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [ACHR, ACRE, REFR, PGRE, PMIS, PBEA, PLYR], True),
        wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, LVLN, LVLC, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, FACT, FLST]),
        wbInteger('Object Type', itU32, wbObjectTypeEnum),
        wbUnused(4)
      ]),
      wbInteger('Count / Distance', itS32),
      wbFloat('Unknown')
    ], cpNormal, False, nil, 3),
    wbConditions,
    wbIdleAnimation,
    wbFormIDCk(CNAM, 'Combat Style', [CSTY]),
    wbEmpty(PKED, 'Eat Marker'),
    wbInteger(PKE2, 'Escort Distance', itU32),
    wbFloat(PKFD, 'Follow - Start Location - Trigger Radius'),
    wbStruct(PKPT, 'Patrol Flags', [
      wbInteger('Repeatable', itU8, wbBoolEnum, cpNormal, False, nil, nil, 1),
      wbUnused(1)
    ], cpNormal, False, nil, 1),
    wbStruct(PKW3, 'Use Weapon Data', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Always Hit',
          8,  'Do No Damage',
          16, 'Crouch To Reload',
          24, 'Hold Fire When Blocked'
        ], False, 25))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Fire Rate', itU8,
        wbEnum([
          {0} 'Auto Fire',
          {1} 'Volley Fire'
        ])),
      wbInteger('Fire Count', itU8,
        wbEnum([
          {0} 'Number of Bursts',
          {1} 'Repeat Fire'
        ])),
      wbInteger('Number of Bursts', itU16),
      wbStruct('Shoots Per Volleys', [
        wbInteger('Min', itU16),
        wbInteger('Max', itU16)
      ]),
      wbStruct('Pause Between Volleys', [
        wbFloat('Min'),
        wbFloat('Max')
      ]),
      wbUnused(4)
    ]),
    wbStruct(PTD2, 'Target 2', [
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'Specific reference',
          {1} 'Object ID',
          {2} 'Object Type',
          {3} 'Linked Reference'
        ])),
      wbUnion('Target', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [ACHR, ACRE, REFR, PGRE, PMIS, PBEA, PLYR], True),
        wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, LVLN, LVLC, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, FACT, FLST]),
        wbInteger('Object Type', itU32, wbObjectTypeEnum),
        wbUnused(4)
      ]),
      wbInteger('Count / Distance', itS32),
      wbFloat('Unknown')
    ], cpNormal, False, nil, 3),
    wbEmpty(PUID, 'Use Item Marker'),
    wbEmpty(PKAM, 'Ambush Marker'),
    wbStruct(PKDD, 'Dialogue Data', [
      wbFloat('FOV'),
      wbFormIDCk('Topic', [DIAL, NULL]),
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0, 'No Headtracking',
          8, 'Don''t Control Target Movement'
        ], False, 9))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(4),
      wbInteger('Dialogue Type', itU32,
        wbEnum([
          {0} 'Conversation',
          {1} 'Say To'
        ])),
      wbByteArray('Unknown', 4)
    ], cpNormal, False, nil, 3),
    wbStruct(PLD2, 'Location 2 (again??)', [
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'Near reference',
          {1} 'In cell',
          {2} 'Near current location',
          {3} 'Near editor location',
          {4} 'Object ID',
          {5} 'Object Type',
          {6} 'Near linked reference',
          {7} 'At package location'
        ])),
      wbUnion('Location', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [REFR, PGRE, PMIS, PBEA, ACHR, ACRE, PLYR], True),
        wbFormIDCkNoReach('Cell', [CELL]),
        wbUnused(4),
        wbUnused(4),
        wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH]),
        wbInteger('Object Type', itU32, wbObjectTypeEnum),
        wbUnused(4),
        wbUnused(4)
      ]),
      wbInteger('Radius', itS32)
    ]),
    wbRStruct('OnBegin', [
      wbEmpty(POBA, 'OnBegin Marker').SetRequired,
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL]).SetRequired,
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL]).SetRequired
    ]).SetRequired,
    wbRStruct('OnEnd', [
      wbEmpty(POEA, 'OnEnd Marker').SetRequired,
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL]).SetRequired,
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL]).SetRequired
    ]).SetRequired,
    wbRStruct('OnChange', [
      wbEmpty(POCA, 'OnChange Marker').SetRequired,
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL]).SetRequired,
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL]).SetRequired
    ]).SetRequired
  ]).SetAfterLoad(wbPACKAfterLoad);
FNV:
  wbRecord(PACK, 'Package',
    wbFlags(wbFlagsList([
      27, 'Unknown 27'
    ])), [
    wbEDIDReq,
    wbStruct(PKDT, 'General', [
      wbInteger('General Flags', itU32, wbPackageFlags).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Type', itU8, wbPackageTypeEnum),
      wbUnused(1),
      wbInteger('Fallout Behavior Flags', itU16, wbFlags([
        {0x00000001}'Hellos To Player',
        {0x00000002}'Random Conversations',
        {0x00000004}'Observe Combat Behavior',
        {0x00000008}'Unknown 4',
        {0x00000010}'Reaction To Player Actions',
        {0x00000020}'Friendly Fire Comments',
        {0x00000040}'Aggro Radius Behavior',
        {0x00000080}'Allow Idle Chatter',
        {0x00000100}'Avoid Radiation'
      ], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnion('Type Specific Flags', wbPKDTSpecificFlagsDecider, [
        wbEmpty('Type Specific Flags (missing)', cpIgnore, False, nil, True),
        wbInteger('Type Specific Flags - Find', itU16,
          wbFlags(wbSparseFlags([
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
        ], False, 11), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Follow', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Escort', itU16,
          wbFlags(wbSparseFlags([
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
        ], False, 11), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Eat', itU16,
          wbFlags(wbSparseFlags([
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
        ], False, 11), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Sleep', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Wander', itU16,
          wbFlags(wbSparseFlags([
            0, 'No Eating',
            1, 'No Sleeping',
            2, 'No Conversation',
            3, 'No Idle Markers',
            4, 'No Furniture',
            5, 'No Wandering'
        ], False, 6), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Travel', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Accompany', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Use Item At', itU16,
          wbFlags(wbSparseFlags([
            2, 'Sit Down',
            8, 'Allow Buying',
            9, 'Allow Killing',
           10, 'Allow Stealing'
        ], False, 11), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Ambush', itU16,
          wbFlags(wbSparseFlags([
            0, 'Hide While Ambushing'
        ], False, 1), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Flee Not Combat', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Cast Magic', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Sandbox', itU16,
          wbFlags(wbSparseFlags([
            0, 'No Eating',
            1, 'No Sleeping',
            2, 'No Conversation',
            3, 'No Idle Markers',
            4, 'No Furniture',
            5, 'No Wandering'
        ], False, 6), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Patrol', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Guard', itU16,
          wbFlags(wbSparseFlags([
            3, 'Remain Near Reference to Guard'
        ], False, 3), True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Dialogue', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Type Specific Flags - Use Weapon', itU16, wbFlags([], True)).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ]),
      wbUnused(2)
    ], cpNormal, True, nil, 2),
    wbRStruct('Locations', [
      wbStruct(PLDT, 'Location 1', [
        wbInteger('Type', itS32, wbEnum([     // Byte + filler
          {0} 'Near reference',
          {1} 'In cell',
          {2} 'Near current location',
          {3} 'Near editor location',
          {4} 'Object ID',
          {5} 'Object Type',
          {6} 'Near linked reference',
          {7} 'At package location'
        ])),
        wbUnion('Location', wbPxDTLocationDecider, [
          wbFormIDCkNoReach('Reference', [REFR, PGRE, PMIS, PBEA, ACHR, ACRE, PLYR], True),
          wbFormIDCkNoReach('Cell', [CELL]),
          wbUnused(4),
          wbUnused(4),
          wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, CHIP, CMNY, CCRD, IMOD]),
          wbInteger('Object Type', itU32, wbObjectTypeEnum),
          wbUnused(4),
          wbUnused(4)
        ]),
        wbInteger('Radius', itS32)
      ], cpNormal{, True}),
      wbStruct(PLD2, 'Location 2', [
        wbInteger('Type', itS32, wbEnum([
          {0} 'Near reference',
          {1} 'In cell',
          {2} 'Near current location',
          {3} 'Near editor location',
          {4} 'Object ID',
          {5} 'Object Type',
          {6} 'Near linked reference',
          {7} 'At package location'
        ])),
        wbUnion('Location', wbPxDTLocationDecider, [
          wbFormIDCkNoReach('Reference', [REFR, PGRE, PMIS, PBEA, ACHR, ACRE, PLYR], True),
          wbFormIDCkNoReach('Cell', [CELL]),
          wbUnused(4),
          wbUnused(4),
          wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, CHIP, CMNY, CCRD, IMOD]),
          wbInteger('Object Type', itU32, wbObjectTypeEnum),
          wbUnused(4),
          wbUnused(4)
        ]),
        wbInteger('Radius', itS32)
      ])
    ], [], cpNormal, False, nil, True),
    wbStruct(PSDT, 'Schedule', [
      wbInteger('Month', itS8),
      wbInteger('Day of week', itS8, wbEnum([
        'Sunday',
        'Monday',
        'Tuesday',
        'Wednesday',
        'Thursday',
        'Friday',
        'Saturday',
        'Weekdays',
        'Weekends',
        'Monday, Wednesday, Friday',
        'Tuesday, Thursday'
      ], [
        -1, 'Any'
      ])),
      wbInteger('Date', itU8),
      wbInteger('Time', itS8),
      wbInteger('Duration', itS32)
    ], cpNormal, True),
    wbStruct(PTDT, 'Target 1', [
      wbInteger('Type', itS32, wbEnum([
        {0} 'Specific Reference',
        {1} 'Object ID',
        {2} 'Object Type',
        {3} 'Linked Reference'
      ]), cpNormal, False, nil, nil, 2),
      wbUnion('Target', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [ACHR, ACRE, REFR, PGRE, PMIS, PBEA, PLYR], True),
        wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, LVLN, LVLC, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, FACT, FLST, IDLM, CHIP, CMNY, CCRD, IMOD]),
        wbInteger('Object Type', itU32, wbObjectTypeEnum),
        wbUnused(4)
      ]),
      wbInteger('Count / Distance', itS32),
      wbFloat('Unknown')
    ], cpNormal, False, nil, 3),
    wbConditions,
    wbIdleAnimation,
    wbFormIDCk(CNAM, 'Combat Style', [CSTY]),
    wbEmpty(PKED, 'Eat Marker'),
    wbInteger(PKE2, 'Escort Distance', itU32),
    wbFloat(PKFD, 'Follow - Start Location - Trigger Radius'),
    wbStruct(PKPT, 'Patrol Flags', [
      wbInteger('Repeatable', itU8, wbBoolEnum, cpNormal, False, nil, nil, 1),
      wbUnused(1)
    ], cpNormal, False, nil, 1),
    wbStruct(PKW3, 'Use Weapon Data', [
      wbInteger('Flags', itU32, wbFlags([
        'Always Hit',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        'Do No Damage',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        'Crouch To Reload',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        'Hold Fire When Blocked'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Fire Rate', itU8, wbEnum([
        'Auto Fire',
        'Volley Fire'
      ])),
      wbInteger('Fire Count', itU8, wbEnum([
        'Number of Bursts',
        'Repeat Fire'
      ])),
      wbInteger('Number of Bursts', itU16),
      wbStruct('Shoots Per Volleys', [
        wbInteger('Min', itU16),
        wbInteger('Max', itU16)
      ]),
      wbStruct('Pause Between Volleys', [
        wbFloat('Min'),
        wbFloat('Max')
      ]),
      wbUnused(4)
    ]),
    wbStruct(PTD2, 'Target 2', [
      wbInteger('Type', itS32, wbEnum([
        {0} 'Specific reference',
        {1} 'Object ID',
        {2} 'Object Type',
        {3} 'Linked Reference'
      ])),
      wbUnion('Target', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [ACHR, ACRE, REFR, PGRE, PMIS, PBEA, PLYR], True),
        wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, LVLN, LVLC, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, FACT, FLST, CHIP, CMNY, CCRD, IMOD]),
        wbInteger('Object Type', itU32, wbObjectTypeEnum),
        wbUnused(4)
      ]),
      wbInteger('Count / Distance', itS32),
      wbFloat('Unknown')
    ], cpNormal, False, nil, 3),
    wbEmpty(PUID, 'Use Item Marker'),
    wbEmpty(PKAM, 'Ambush Marker'),
    wbStruct(PKDD, 'Dialogue Data', [
      wbFloat('FOV'),
      wbFormIDCk('Topic', [DIAL, NULL]),
      wbInteger('Flags', itU32, wbFlags([
        'No Headtracking',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        'Don''t Control Target Movement'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(4),
      wbInteger('Dialogue Type', itU32, wbEnum([
        'Conversation',
        'Say To'
      ])),
      wbByteArray('Unknown', 4)
    ], cpNormal, False, nil, 3),
    wbStruct(PLD2, 'Location 2 (again??)', [
      wbInteger('Type', itS32, wbEnum([
        {0} 'Near reference',
        {1} 'In cell',
        {2} 'Near current location',
        {3} 'Near editor location',
        {4} 'Object ID',
        {5} 'Object Type',
        {6} 'Near linked reference',
        {7} 'At package location'
      ])),
      wbUnion('Location', wbPxDTLocationDecider, [
        wbFormIDCkNoReach('Reference', [REFR, PGRE, PMIS, PBEA, ACHR, ACRE, PLYR], True),
        wbFormIDCkNoReach('Cell', [CELL]),
        wbUnused(4),
        wbUnused(4),
        wbFormIDCkNoReach('Object ID', [ACTI, DOOR, STAT, FURN, CREA, SPEL, NPC_, CONT, ARMO, AMMO, MISC, WEAP, BOOK, KEYM, ALCH, LIGH, CHIP, CMNY, CCRD, IMOD]),
        wbInteger('Object Type', itU32, wbObjectTypeEnum),
        wbUnused(4),
        wbUnused(4)
      ]),
      wbInteger('Radius', itS32)
    ]),
    wbRStruct('OnBegin', [
      wbEmpty(POBA, 'OnBegin Marker', cpNormal, True),
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL], False, cpNormal, True),
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL], False, cpNormal, True)
    ], [], cpNormal, True),
    wbRStruct('OnEnd', [
      wbEmpty(POEA, 'OnEnd Marker', cpNormal, True),
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL], False, cpNormal, True),
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL], False, cpNormal, True)
    ], [], cpNormal, True),
    wbRStruct('OnChange', [
      wbEmpty(POCA, 'OnChange Marker', cpNormal, True),
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL], False, cpNormal, True),
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL], False, cpNormal, True)
    ], [], cpNormal, True)
  ], False, nil, cpNormal, False, wbPACKAfterLoad);

# PARW.Placed Arrow
FO5:
  ReferenceRecord(PARW, 'Placed Arrow');

# PBAR.Placed Barrier
FO5:
  ReferenceRecord(PBAR, 'Placed Barrier');

# PBEA.Placed Beam
FO3:
  wbRefRecord(PBEA, 'Placed Beam', [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [PROJ]).SetRequired,
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    wbRagdoll,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time').SetRequired,
      wbEmpty(XPPA, 'Patrol Script Marker').SetRequired,
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL]).SetRequired,
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL]).SetRequired
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32,
          wbFlags([
            {0} 'Reflection',
            {1} 'Refraction'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Parent Activate Only', itU8, wbBoolEnum),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo);
FNV:
  wbRefRecord(PBEA, 'Placed Beam', [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [PROJ], False, cpNormal, True),
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    wbRagdoll,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time', cpNormal, True),
      wbEmpty(XPPA, 'Patrol Script Marker', cpNormal, True),
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL], False, cpNormal, True),
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL], False, cpNormal, True)
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32, wbFlags([
          'Reflection',
          'Refraction'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])
    ),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Flags', itU8, wbFlags([
        'Parent Activate Only'
      ], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    wbStringKC(XATO, 'Activation Prompt'),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo);
FO5:
  ReferenceRecord(PBEA, 'Placed Beam');

# PCON.Placed Cone/Voice
FO5:
  ReferenceRecord(PCON, 'Placed Cone/Voice');

# PFLA.Placed Flame
FO5:
  ReferenceRecord(PFLA, 'Placed Flame');

# PERK.Perk
FO3:
  wbRecord(PERK, 'Perk', [
    wbEDIDReq,
    wbFULL,
    wbDESCReq,
    wbICON,
    wbConditions,
    wbStruct(DATA, 'Data', [
      wbInteger('Trait', itU8, wbBoolEnum),
      wbInteger('Min Level', itU8),
      wbInteger('Ranks', itU8),
      wbInteger('Playable', itU8, wbBoolEnum),
      wbInteger('Hidden', itU8, wbBoolEnum)
    ], cpNormal, True, nil, 4),
    wbRArrayS('Effects',
      wbRStructSK([0, 1], 'Effect', [
        wbStructSK(PRKE, [1, 2, 0], 'Header', [
          wbPerkEffectType(wbPERKPRKETypeAfterSet),
          wbInteger('Rank', itU8),
          wbInteger('Priority', itU8)
        ]),
        wbUnion(DATA, 'Effect Data', wbPerkDATADecider, [
          wbStructSK([0, 1], 'Quest + Stage', [
            wbFormIDCk('Quest', [QUST]),
            wbInteger('Quest Stage', itU8, wbPerkDATAQuestStageToStr, wbQuestStageToInt),
            wbUnused(3)
          ]),
          wbFormIDCk('Ability', [SPEL]),
          wbStructSK([0, 1], 'Entry Point', [
            wbInteger('Entry Point', itU8,
              wbEnum([
                {0}  'Calculate Weapon Damage',
                {1}  'Calculate My Critical Hit Chance',
                {2}  'Calculate My Critical Hit Damage',
                {3}  'Calculate Weapon Attack AP Cost',
                {4}  'Calculate Mine Explode Chance',
                {5}  'Adjust Range Penalty',
                {6}  'Adjust Limb Damage',
                {7}  'Calculate Weapon Range',
                {8}  'Calculate To Hit Chance',
                {9}  'Adjust Experience Points',
                {10} 'Adjust Gained Skill Points',
                {11} 'Adjust Book Skill Points',
                {12} 'Modify Recovered Health',
                {13} 'Calculate Inventory AP Cost',
                {14} 'Get Disposition',
                {15} 'Get Should Attack',
                {16} 'Get Should Assist',
                {17} 'Calculate Buy Price',
                {18} 'Get Bad Karma',
                {19} 'Get Good Karma',
                {20} 'Ignore Locked Terminal',
                {21} 'Add Leveled List On Death',
                {22} 'Get Max Carry Weight',
                {23} 'Modify Addiction Chance',
                {24} 'Modify Addiction Duration',
                {25} 'Modify Positive Chem Duration',
                {26} 'Adjust Drinking Radiation',
                {27} 'Activate',
                {28} 'Mysterious Stranger',
                {29} 'Has Paralyzing Palm',
                {30} 'Hacking Science Bonus',
                {31} 'Ignore Running During Detection',
                {32} 'Ignore Broken Lock',
                {33} 'Has Concentrated Fire',
                {34} 'Calculate Gun Spread',
                {35} 'Player Kill AP Reward',
                {36} 'Modify Enemy Critical Hit Chance'
              ])).SetAfterSet(wbPERKEntryPointAfterSet),
            wbInteger('Function', itU8, wbPerkDATAFunctionToStr, wbPerkDATAFunctionToInt).SetAfterSet(wbPerkDATAFunctionAfterSet),
            wbInteger('Perk Condition Tab Count', itU8, nil, cpIgnore)
          ])
        ]).SetRequired,
        wbRArrayS('Perk Conditions',
          wbRStructSK([0], 'Perk Condition', [
            wbInteger(PRKC, 'Run On', itS8, wbPRKCToStr, wbPRKCToInt),
            wbConditions.SetRequired
          ]).SetDontShow(wbPERKPRKCDontShow)),
        wbRStruct('Entry Point Function Parameters', [
          wbInteger(EPFT, 'Type', itU8, wbPerkEPFTToStr, wbPerkEPFTToInt, cpIgnore).SetAfterSet(wbPerkEPFTAfterSet),
          wbUnion(EPFD, 'Data', wbEPFDDecider, [
            wbByteArray('Unknown'),
            wbFloat('Float'),
            wbStruct('Float, Float', [
              wbFloat('Float 1'),
              wbFloat('Float 2')
            ]),
            wbFormIDCk('Leveled Item', [LVLI]),
            wbEmpty('None (Script)'),
            wbStruct('Actor Value, Float', [
              wbInteger('Actor Value', itU32, wbEPFDActorValueToStr, wbEPFDActorValueToInt),
              wbFloat('Float')
            ])
          ]).SetDontShow(wbEPFDDontShow),
          wbStringKC(EPF2, 'Button Label').SetDontShow(wbEPF2DontShow),
          wbInteger(EPF3, 'Run Immediately', itU16, wbBoolEnum).SetDontShow(wbEPF2DontShow),
          wbEmbeddedScript.SetDontShow(wbEPF2DontShow)
        ]).SetDontShow(wbPERKPRKCDontShow),
        wbEmpty(PRKF, 'End Marker', cpIgnore).SetRequired
      ]))
  ]);
FNV:
  wbRecord(PERK, 'Perk', [
    wbEDIDReq,
    wbFULL,
    wbDESCReq,
    wbICON,
    wbConditions,
    wbStruct(DATA, 'Data', [
      wbInteger('Trait', itU8, wbBoolEnum),
      wbInteger('Min Level', itU8),
      wbInteger('Ranks', itU8),
      wbInteger('Playable', itU8, wbBoolEnum),
      wbInteger('Hidden', itU8, wbBoolEnum)
    ], cpNormal, True, nil, 4),
    wbRArrayS('Effects', wbPerkEffect)
  ]);

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
FO3:
  wbRefRecord(PGRE, 'Placed Grenade',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      11, 'Initially Disabled'
    ])), [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [PROJ]).SetRequired,
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    wbRagdoll,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time').SetRequired,
      wbEmpty(XPPA, 'Patrol Script Marker').SetRequired,
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL]).SetRequired,
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL]).SetRequired
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32,
          wbFlags([
            {0} 'Reflection',
            {1} 'Refraction'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Parent Activate Only', itU8, wbBoolEnum),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo);
FNV:
  wbRefRecord(PGRE, 'Placed Grenade',
    wbFlags(wbFlagsList([
      10, 'Persistent',
      11, 'Initially Disabled'
    ])), [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [PROJ], False, cpNormal, True),
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    wbRagdoll,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time', cpNormal, True),
      wbEmpty(XPPA, 'Patrol Script Marker', cpNormal, True),
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL], False, cpNormal, True),
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL], False, cpNormal, True)
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32, wbFlags([
          'Reflection',
          'Refraction'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])
    ),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Flags', itU8, wbFlags([
        'Parent Activate Only'
      ], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    wbStringKC(XATO, 'Activation Prompt'),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo);
FO5:
  ReferenceRecord(PGRE, 'Placed Projectile');

# PHZD.Placed Hazard
FO5:
  ReferenceRecord(PHZD, 'Placed Hazard');

# PLYR.Player Reference #NEW
TES4:
  wbRecord(PLYR, 'Player Reference', [
    wbEDID,
    wbFormID(PLYR, 'Player')
      .SetDefaultNativeValue($7)
      .SetRequired
  ]).IncludeFlag(dfInternalEditOnly);
FO3:
  wbRecord(PLYR, 'Player Reference', [
    wbEDID,
    wbFormID(PLYR, 'Player')
      .SetDefaultNativeValue($7)
      .SetRequired
  ]).IncludeFlag(dfInternalEditOnly);
FNV:
  wbRecord(PLYR, 'Player Reference', [
    wbEDID,
    wbFormID(PLYR, 'Player', cpNormal, True).SetDefaultNativeValue($7)
  ]).IncludeFlag(dfInternalEditOnly);

# PMIS.Placed Missile
FO3:
  wbRefRecord(PMIS, 'Placed Missile', [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [PROJ]).SetRequired,
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    wbRagdoll,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time').SetRequired,
      wbEmpty(XPPA, 'Patrol Script Marker').SetRequired,
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL]).SetRequired,
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL]).SetRequired
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32,
          wbFlags([
            {0} 'Reflection',
            {1} 'Refraction'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Parent Activate Only', itU8, wbBoolEnum),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo);
FNV:
  wbRefRecord(PMIS, 'Placed Missile', [
    wbEDID,
    wbFormIDCk(NAME, 'Base', [PROJ], False, cpNormal, True),
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    wbRagdoll,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time', cpNormal, True),
      wbEmpty(XPPA, 'Patrol Script Marker', cpNormal, True),
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL], False, cpNormal, True),
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL], False, cpNormal, True)
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32, wbFlags([
          'Reflection',
          'Refraction'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])
    ),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Flags', itU8, wbFlags([
        'Parent Activate Only'
      ], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    wbStringKC(XATO, 'Activation Prompt'),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True).SetAddInfo(wbPlacedAddInfo);
FO5:
  ReferenceRecord(PMIS, 'Placed Missile');

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
FO3:
  wbRecord(PROJ, 'Projectile',
    wbFlags(wbFlagsList([
      27, 'Unknown 27'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbDEST,
    wbStruct(DATA, 'Data', [
      {00} wbInteger('Flags', itU16,
             wbFlags(wbSparseFlags([
               0, 'Hitscan',
               1, 'Explosion',
               2, 'Alt. Trigger',
               3, 'Muzzle Flash',
               5, 'Can Be Disabled',
               6, 'Can Be Picked Up',
               7, 'Supersonic',
               8, 'Pins Limbs',
               9, 'Pass Through Small Transparent'
             ], False, 10)
           )).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {00} wbInteger('Type', itU16,
             wbEnum([], [
               1, 'Missile',
               2, 'Lobber',
               4, 'Beam',
               8, 'Flame'
             ])),
      {04} wbFloat('Gravity'),
      {08} wbFloat('Speed'),
      {12} wbFloat('Range'),
      {16} wbFormIDCk('Light', [LIGH, NULL]),
      {20} wbFormIDCk('Muzzle Flash - Light', [LIGH, NULL]),
      {24} wbFloat('Tracer Chance'),
      {28} wbFloat('Explosion - Alt. Trigger - Proximity'),
      {32} wbFloat('Explosion - Alt. Trigger - Timer'),
      {36} wbFormIDCk('Explosion', [EXPL, NULL]),
      {40} wbFormIDCk('Sound', [SOUN, NULL]),
      {44} wbFloat('Muzzle Flash - Duration'),
      {48} wbFloat('Fade Duration'),
      {52} wbFloat('Impact Force'),
      {56} wbFormIDCk('Sound - Countdown', [SOUN, NULL]),
      {60} wbFormIDCk('Sound - Disable', [SOUN, NULL]),
      {64} wbFormIDCk('Default Weapon Source', [WEAP, NULL])
    ]).SetRequired,
    wbRStructSK([0], 'Muzzle Flash Model', [
      wbString(NAM1, 'Model FileName'),
      wbModelInfo(NAM2)
    ]).SetSummaryKey([0])
      .SetRequired
      .IncludeFlag(dfCollapsed, wbCollapseModels),
    wbInteger(VNAM, 'Sound Level', itU32, wbSoundLevelEnum).SetRequired
  ]);
FNV:
  wbRecord(PROJ, 'Projectile',
    wbFlags(wbFlagsList([
      27, 'Unknown 27'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbDEST,
    wbStruct(DATA, 'Data', [
      {00} wbInteger('Flags', itU16, wbFlags([
        'Hitscan',
        'Explosion',
        'Alt. Trigger',
        'Muzzle Flash',
        '',
        'Can Be Disabled',
        'Can Be Picked Up',
        'Supersonic',
        'Pins Limbs',
        'Pass Through Small Transparent',
        'Detonates',
        'Rotation'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {02} wbInteger('Type', itU16, wbEnum([
        {00} '',
        {01} 'Missile',
        {02} 'Lobber',
        {03} '',
        {04} 'Beam',
        {05} '',
        {06} '',
        {07} '',
        {08} 'Flame',
        {09} '',
        {10} '',
        {11} '',
        {12} '',
        {13} '',
        {14} '',
        {15} '',
        {16} 'Continuous Beam'
      ])),
      {04} wbFloat('Gravity'),
      {08} wbFloat('Speed'),
      {12} wbFloat('Range'),
      {16} wbFormIDCk('Light', [LIGH, NULL]),
      {20} wbFormIDCk('Muzzle Flash - Light', [LIGH, NULL]),
      {24} wbFloat('Tracer Chance'),
      {28} wbFloat('Explosion - Alt. Trigger - Proximity'),
      {32} wbFloat('Explosion - Alt. Trigger - Timer'),
      {36} wbFormIDCk('Explosion', [EXPL, NULL]),
      {40} wbFormIDCk('Sound', [SOUN, NULL]),
      {44} wbFloat('Muzzle Flash - Duration'),
      {48} wbFloat('Fade Duration'),
      {52} wbFloat('Impact Force'),
      {56} wbFormIDCk('Sound - Countdown', [SOUN, NULL]),
      {60} wbFormIDCk('Sound - Disable', [SOUN, NULL]),
      {64} wbFormIDCk('Default Weapon Source', [WEAP, NULL]),
      {68} wbVec3('Rotation'),
      {80} wbFloat('Bouncy Mult')
    ], cpNormal, True, nil, 18),
    wbRStructSK([0], 'Muzzle Flash Model', [
      wbString(NAM1, 'Model FileName'),
      wbModelInfo(NAM2)
    ], [], cpNormal, True)
    .SetSummaryKey([0])
    .IncludeFlag(dfCollapsed, wbCollapseModels),
    wbInteger(VNAM, 'Sound Level', itU32, wbSoundLevelEnum, cpNormal, True)
  ]);

# PWAT.Placeable Water
FO3:
  wbRecord(PWAT, 'Placeable Water', [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbStruct(DNAM, '', [
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0,  'Reflects',
          1,  'Reflects - Actors',
          2,  'Reflects - Land',
          3,  'Reflects - LOD Land',
          4,  'Reflects - LOD Buildings',
          5,  'Reflects - Trees',
          6,  'Reflects - Sky',
          7,  'Reflects - Dynamic Objects',
          8,  'Reflects - Dead Bodies',
          9,  'Refracts',
          10, 'Refracts - Actors',
          11, 'Refracts - Land',
          16, 'Refracts - Dynamic Objects',
          17, 'Refracts - Dead Bodies',
          18, 'Silhouette Reflections',
          28, 'Depth',
          29, 'Object Texture Coordinates',
          31, 'No Underwater Fog'
        ]))).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFormIDCk('Water', [WATR])
    ]).SetRequired
  ]);
FNV:
  wbRecord(PWAT, 'Placeable Water', [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbStruct(DNAM, '', [
      wbInteger('Flags', itU32, wbFlags([
        {0x00000001}'Reflects',
        {0x00000002}'Reflects - Actors',
        {0x00000004}'Reflects - Land',
        {0x00000008}'Reflects - LOD Land',
        {0x00000010}'Reflects - LOD Buildings',
        {0x00000020}'Reflects - Trees',
        {0x00000040}'Reflects - Sky',
        {0x00000080}'Reflects - Dynamic Objects',
        {0x00000100}'Reflects - Dead Bodies',
        {0x00000200}'Refracts',
        {0x00000400}'Refracts - Actors',
        {0x00000800}'Refracts - Land',
        {0x00001000}'',
        {0x00002000}'',
        {0x00004000}'',
        {0x00008000}'',
        {0x00010000}'Refracts - Dynamic Objects',
        {0x00020000}'Refracts - Dead Bodies',
        {0x00040000}'Silhouette Reflections',
        {0x00080000}'',
        {0x00100000}'',
        {0x00200000}'',
        {0x00400000}'',
        {0x00800000}'',
        {0x01000000}'',
        {0x02000000}'',
        {0x03000000}'',
        {0x08000000}'',
        {0x10000000}'Depth',
        {0x20000000}'Object Texture Coordinates',
        {0x40000000}'',
        {0x80000000}'No Underwater Fog'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFormIDCk('Water', [WATR])
    ], cpNormal, True)
  ]);

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
FO3:
  wbRecord(QUST, 'Quest', [
    wbEDIDReq,
    wbSCRI,
    wbFULL,
    wbICON,
    wbStruct(DATA, 'General', [
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'Start game enabled',
          2, 'Allow repeated conversation topics',
          3, 'Allow repeated stages',
          4, 'Default Script Processing Delay'
        ], False, 5))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Priority', itU8),
      wbUnused(2),
      wbFloat('Quest Delay')
    ], cpNormal, True, nil, 3),
    wbConditions,
    wbRArrayS('Stages', wbRStructSK([0], 'Stage', [
      wbInteger(INDX, 'Stage Index', itS16),
      wbRArray('Log Entries', wbRStruct('Log Entry', [
        wbInteger(QSDT, 'Stage Flags', itU8,
          wbFlags([
            {0} 'Complete Quest',
            {1} 'Fail Quest'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbConditions,
        wbStringKC(CNAM, 'Log Entry', 0, cpTranslate),
        wbEmbeddedScriptReq,
        wbFormIDCk(NAM0, 'Next Quest', [QUST])
      ]))
    ])),
    wbRArray('Objectives', wbRStruct('Objective', [
      wbInteger(QOBJ, 'Objective Index', itS32),
      wbStringKC(NNAM, 'Description').SetRequired,
      wbRArray('Targets', wbRStruct('Target', [
        wbStruct(QSTA, 'Target', [
          wbFormIDCkNoReach('Target', [REFR, PGRE, PMIS, PBEA, ACRE, ACHR], True),
          wbInteger('Compass Marker Ignores Locks', itU8, wbBoolEnum),
          wbUnused(3)
        ]),
        wbConditions
      ]))
    ]))
  ]);
FNV:
  wbRecord(QUST, 'Quest', [
    wbEDIDReq,
    wbSCRI,
    wbFULL,
    wbICON,
    wbStruct(DATA, 'General', [
      wbInteger('Flags', itU8, wbFlags([
        {0x01} 'Start game enabled',
        {0x02} '',
        {0x04} 'Allow repeated conversation topics',
        {0x08} 'Allow repeated stages',
        {0x10} 'Default Script Processing Delay'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('Priority', itU8),
      wbUnused(2),
      wbFloat('Quest Delay')
    ], cpNormal, True, nil, 3),
    wbConditions,
    wbRArrayS('Stages', wbRStructSK([0], 'Stage', [
      wbInteger(INDX, 'Stage Index', itS16),
      wbRArray('Log Entries', wbRStruct('Log Entry', [
        wbInteger(QSDT, 'Stage Flags', itU8, wbFlags([
          {0x01} 'Complete Quest',
          {0x02} 'Fail Quest'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbConditions,
        wbStringKC(CNAM, 'Log Entry', 0, cpTranslate),
        wbEmbeddedScriptReq,
        wbFormIDCk(NAM0, 'Next Quest', [QUST])
      ]))
    ])),
    wbRArray('Objectives', wbRStruct('Objective', [
      wbInteger(QOBJ, 'Objective Index', itS32),
      wbStringKC(NNAM, 'Description', 0, cpNormal, True),
      wbRArray('Targets', wbRStruct('Target', [
        wbStruct(QSTA, 'Target', [
          wbFormIDCkNoReach('Target', [REFR, PGRE, PMIS, PBEA, ACRE, ACHR], True),
          wbInteger('Flags', itU8, wbFlags([
            {0x01} 'Compass Marker Ignores Locks'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
          wbUnused(3)
        ]),
        wbConditions
      ]))
    ]))
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
FO3:
  wbRecord(RACE, 'Race', [
    wbEDIDReq,
    wbFULLReq,
    wbDESCReq,
    wbFactionRelations,
    wbStruct(DATA, '', [
      wbArrayS('Skill Boosts', wbStructSK([0], 'Skill Boost', [
        wbInteger('Skill', itS8, wbActorValueEnum),
        wbInteger('Boost', itS8)
      ]).SetSummaryKey([1, 0])
        .SetSummaryMemberPrefixSuffix(1, '+', '')
        .SetSummaryMemberPrefixSuffix(0, '', '')
        .SetSummaryDelimiter(' ')
        .IncludeFlag(dfSummaryNoSortKey)
        .IncludeFlag(dfSummaryMembersNoName)
        .IncludeFlag(dfCollapsed, wbCollapseObjectProperties), 7),
      wbUnused(2),
      wbFloat('Male Height'),
      wbFloat('Female Height'),
      wbFloat('Male Weight'),
      wbFloat('Female Weight'),
      wbInteger('Flags', itU32,
        wbFlags(wbSparseFlags([
          0, 'Playable',
          2, 'Child'
        ], False, 3))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]).SetRequired,
    wbFormIDCk(ONAM, 'Older', [RACE]),
    wbFormIDCk(YNAM, 'Younger', [RACE]),
    wbEmpty(NAM2, 'Unknown Marker').SetRequired,
    wbArray(VTCK, 'Voices', wbFormIDCk('Voice', [VTYP]), ['Male', 'Female']).SetRequired,
    wbArray(DNAM, 'Default Hair Styles', wbFormIDCk('Default Hair Style', [HAIR, NULL]), ['Male', 'Female']).SetRequired,
    wbArray(CNAM, 'Default Hair Colors',
      wbInteger('Default Hair Color', itU8,
        wbEnum([
          {0}  'Bleached',
          {1}  'Brown',
          {2}  'Chocolate',
          {3}  'Platinum',
          {4}  'Cornsilk',
          {5}  'Suede',
          {6}  'Pecan',
          {7}  'Auburn',
          {8}  'Ginger',
          {9}  'Honey',
          {10} 'Gold',
          {11} 'Rosewood',
          {12} 'Black',
          {13} 'Chestnut',
          {14} 'Steel',
          {15} 'Champagne'
        ])), [
          {0} 'Male',
          {1} 'Female'
    ]).SetRequired,
    wbFloat(PNAM, 'FaceGen - Main clamp').SetRequired,
    wbFloat(UNAM, 'FaceGen - Face clamp').SetRequired,
    wbByteArray(ATTR, 'Unused').SetRequired,
    wbRStruct('Head Data', [
      wbEmpty(NAM0, 'Head Data Marker').SetRequired,
      wbRStruct('Male Head Data', [
        wbEmpty(MNAM, 'Male Data Marker').SetRequired,
        wbHeadParts
      ]).SetRequired,
      wbRStruct('Female Head Data', [
        wbEmpty(FNAM, 'Female Data Marker').SetRequired,
        wbHeadParts
      ]).SetRequired
    ]).SetRequired,
    wbRStruct('Body Data', [
      wbEmpty(NAM1, 'Body Data Marker').SetRequired,
      wbRStruct('Male Body Data', [
        wbEmpty(MNAM, 'Male Data Marker'),
        wbBodyParts
      ]).SetRequired,
      wbRStruct('Female Body Data', [
        wbEmpty(FNAM, 'Female Data Marker').SetRequired,
        wbBodyParts
      ]).SetRequired
    ]).SetRequired,
    wbArrayS(HNAM, 'Hairs', wbFormIDCk('Hair', [HAIR])).SetRequired,
    wbArrayS(ENAM, 'Eyes', wbFormIDCk('Eye', [EYES])).SetRequired,
    wbRStruct('FaceGen Data', [
      wbRStruct('Male FaceGen Data', [
        wbEmpty(MNAM, 'Male Data Marker').SetRequired,
        wbFaceGen,
        wbUnknown(SNAM).SetRequired
      ]).SetRequired,
      wbRStruct('Female FaceGen Data', [
        wbEmpty(FNAM, 'Female Data Marker').SetRequired,
        wbFaceGen,
        wbUnknown(SNAM).SetRequired
      ]).SetRequired
    ]).SetRequired
  ]);
FNV:
    wbRecord(RACE, 'Race', [
    wbEDIDReq,
    wbFULLReq,
    wbDESCReq,
    wbFactionRelations,
    wbStruct(DATA, '', [
      wbArrayS('Skill Boosts', wbStructSK([0], 'Skill Boost', [
        wbInteger('Skill', itS8, wbActorValueEnum),
        wbInteger('Boost', itS8)
      ])
      .SetSummaryKey([1, 0])
      .SetSummaryMemberPrefixSuffix(1, '+', '')
      .SetSummaryMemberPrefixSuffix(0, '', '')
      .SetSummaryDelimiter(' ')
      .IncludeFlag(dfSummaryNoSortKey)
      .IncludeFlag(dfSummaryMembersNoName)
      .IncludeFlag(dfCollapsed, wbCollapseObjectProperties), 7),
      wbUnused(2),
      wbFloat('Male Height'),
      wbFloat('Female Height'),
      wbFloat('Male Weight'),
      wbFloat('Female Weight'),
      wbInteger('Flags', itU32, wbFlags([
        'Playable',
        '',
        'Child'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ], cpNormal, True),
    wbFormIDCk(ONAM, 'Older', [RACE]),
    wbFormIDCk(YNAM, 'Younger', [RACE]),
    wbEmpty(NAM2, 'Unknown Marker', cpNormal, True),
    wbArray(VTCK, 'Voices', wbFormIDCk('Voice', [VTYP]), ['Male', 'Female'], cpNormal, True),
    wbArray(DNAM, 'Default Hair Styles', wbFormIDCk('Default Hair Style', [HAIR, NULL]), ['Male', 'Female'], cpNormal, True),
    wbArray(CNAM, 'Default Hair Colors', wbInteger('Default Hair Color', itU8, wbEnum([
      'Bleached',
      'Brown',
      'Chocolate',
      'Platinum',
      'Cornsilk',
      'Suede',
      'Pecan',
      'Auburn',
      'Ginger',
      'Honey',
      'Gold',
      'Rosewood',
      'Black',
      'Chestnut',
      'Steel',
      'Champagne'
    ])), ['Male', 'Female'], cpNormal, True),
    wbFloat(PNAM, 'FaceGen - Main clamp', cpNormal, True),
    wbFloat(UNAM, 'FaceGen - Face clamp', cpNormal, True),
    wbByteArray(ATTR, 'Unused', 0, cpNormal, True),
    wbRStruct('Head Data', [
      wbEmpty(NAM0, 'Head Data Marker', cpNormal, True),
      wbRStruct('Male Head Data', [
        wbEmpty(MNAM, 'Male Data Marker', cpNormal, True),
        wbHeadParts
      ], [], cpNormal, True),
      wbRStruct('Female Head Data', [
        wbEmpty(FNAM, 'Female Data Marker', cpNormal, True),
        wbHeadParts
      ], [], cpNormal, True)
    ], [], cpNormal, True),
    wbRStruct('Body Data', [
      wbEmpty(NAM1, 'Body Data Marker', cpNormal, True),
      wbRStruct('Male Body Data', [
        wbEmpty(MNAM, 'Male Data Marker'),
        wbBodyParts
      ], [], cpNormal, True),
      wbRStruct('Female Body Data', [
        wbEmpty(FNAM, 'Female Data Marker', cpNormal, True),
        wbBodyParts
      ], [], cpNormal, True)
    ], [], cpNormal, True),
    wbArrayS(HNAM, 'Hairs', wbFormIDCk('Hair', [HAIR]), 0, cpNormal, True),
    wbArrayS(ENAM, 'Eyes', wbFormIDCk('Eye', [EYES]),  0,  cpNormal, True),
    wbRStruct('FaceGen Data', [
      wbRStruct('Male FaceGen Data', [
        wbEmpty(MNAM, 'Male Data Marker', cpNormal, True),
        wbFaceGen,
        wbInteger(SNAM, 'Unknown', itU16, nil, cpNormal, True)
      ], [], cpNormal, True),
      wbRStruct('Female FaceGen Data', [
        wbEmpty(FNAM, 'Female Data Marker', cpNormal, True),
        wbFaceGen,
        wbInteger(SNAM, 'Unknown', itU16, nil, cpNormal, True)  // will effectivly overwrite the SNAM from the male :)
      ], [], cpNormal, True)
    ], [], cpNormal, True)
  ]);

# RADS.Radiation Stage
FO3:
  wbRecord(RADS, 'Radiation Stage', [
    wbEDIDReq,
    wbStruct(DATA, '', [
      wbInteger('Trigger Threshold', itU32),
      wbFormIDCk('Actor Effect', [SPEL])
    ]).SetRequired
  ]);
FNV:
  wbRecord(RADS, 'Radiation Stage', [
    wbEDIDReq,
    wbStruct(DATA, '', [
      wbInteger('Trigger Threshold', itU32),
      wbFormIDCk('Actor Effect', [SPEL])
    ], cpNormal, True)
  ]);

# RCCT.Recipe Category
FNV:
  wbRecord(RCCT, 'Recipe Category', [
    wbEDIDReq,
    wbFULL,
    wbInteger(DATA, 'Flags', itU8,
      wbFlags(wbSparseFlags([
        0, 'Subcategory?',
        1, 'Unknown 1',
        2, 'Unknown 2',
        3, 'Unknown 3',
        4, 'Unknown 4',
        5, 'Unknown 5',
        6, 'Unknown 6',
        7, 'Unknown 7'
      ]))).IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);

# RCPE.Recipe
FNV:
  wbRecord(RCPE, 'Recipe', [
    wbEDIDReq,
    wbFULL,
    wbConditions,
    wbStruct(DATA, 'Data', [
      wbInteger('Skill', itS32, wbActorValueEnum),
      wbInteger('Level', itU32),
      wbFormIDCk('Category', [RCCT, NULL]),   // Some of DeadMoney are NULL
      wbFormIDCk('Sub-Category', [RCCT])
    ]),
    wbRArray('Ingredients', wbIngredient),
    wbRArray('Outputs', wbOutput)
  ]);

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
FO3:
  wbRefRecord(REFR, 'Placed Object',
    wbFlags(wbFlagsList([
       6, 'Hidden From Local Map',
       7, 'Turn Off Fire',  //Only MSTT placing FXSmokeMed01 [00071FED]?
       8, 'Inaccessible',
       9, 'Casts Shadows/Motion Blur',
      10, 'Persistent',
      11, 'Initially Disabled',
      15, 'Visible When Distant',
      16, 'High Priority LOD',  //Requires Visible When Distant
      25, 'No AI Acquire',
      26, 'Navmesh - Filter',
      27, 'Navmesh - Bounding Box',
      28, 'Reflected By Auto Water', //Only REFRs placed in Exterior?
      29, 'Refracted by Auto Water', //Only REFRs placed in Exterior?
      30, 'Navmesh - Ground',
      31, 'Multibound'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDID,
    {
    wbStruct(RCLR, 'Linked Reference Color (Old Format?)', [
      wbStruct('Link Start Color', [
        wbInteger('Red', itU8),
        wbInteger('Green', itU8),
        wbInteger('Blue', itU8),
        wbUnused(1)
      ]),
      wbStruct('Link End Color', [
        wbInteger('Red', itU8),
        wbInteger('Green', itU8),
        wbInteger('Blue', itU8),
        wbUnused(1)
      ])
    ], cpIgnore),}
    wbByteArray(RCLR, 'Unused', 0, cpIgnore),
    wbFormIDCk(NAME, 'Base', [TREE, SOUN, ACTI, DOOR, STAT, FURN, CONT, ARMO, AMMO, LVLN, LVLC,
                              MISC, WEAP, BOOK, KEYM, ALCH, LIGH, GRAS, ASPC, IDLM, ARMA,
                              MSTT, NOTE, PWAT, SCOL, TACT, TERM, TXST, ADDN]).SetRequired,
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    {--- ?? ---}
    wbRagdoll,

    {--- Primitive ---}
    wbStruct(XPRM, 'Primitive', [
      wbStruct('Bounds', [
        wbFloat('X', cpNormal, True, 2, 4),
        wbFloat('Y', cpNormal, True, 2, 4),
        wbFloat('Z', cpNormal, True, 2, 4)
      ]).SetToStr(wbVec3ToStr).IncludeFlag(dfCollapsed, wbCollapseVec3),
      wbFloatColors('Color'),
      wbUnknown(4),
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'None',
          {1} 'Box',
          {2} 'Sphere',
          {3} 'Portal Box'
        ]))
    ]),
    wbInteger(XTRI, 'Collision Layer', itU32,
      wbEnum([
        {0}  'Unidentified',
        {1}  'Static',
        {2}  'AnimStatic',
        {3}  'Transparent',
        {4}  'Clutter',
        {5}  'Weapon',
        {6}  'Projectile',
        {7}  'Spell',
        {8}  'Biped',
        {9}  'Trees',
        {10} 'Props',
        {11} 'Water',
        {12} 'Trigger',
        {13} 'Terrain',
        {14} 'Trap',
        {15} 'Non Collidable',
        {16} 'Cloud Trap',
        {17} 'Ground',
        {18} 'Portal',
        {19} 'Debris Small',
        {20} 'Debris Large',
        {21} 'Acoustic Space',
        {22} 'Actor Zone',
        {23} 'Projectile Zone',
        {24} 'Gas Trap',
        {25} 'Shell Casing',
        {26} 'Transparent Small',
        {27} 'Invisible Wall',
        {28} 'Transparent Small Anim',
        {29} 'Dead Bip',
        {30} 'Char Controller',
        {31} 'Avoid Box',
        {32} 'Collision Box',
        {33} 'Camera Sphere',
        {34} 'Door Detection',
        {35} 'Camera Pick',
        {36} 'Item Pick',
        {37} 'Line Of Sight',
        {38} 'Path Pick',
        {39} 'Custom Pick 1',
        {40} 'Custom Pick 2',
        {41} 'Spell Explosion',
        {42} 'Dropping Pick'
      ])),
    wbEmpty(XMBP, 'MultiBound Primitive Marker'),

    {--- Bound Contents ---}

    {--- Bound Data ---}
    wbVec3(XMBO, 'Bound Half Extents'),

    {--- Teleport ---}
    wbStruct(XTEL, 'Teleport Destination', [
      wbFormIDCk('Door', [REFR], True),
      wbPosRot,
      wbInteger('No Alarm', itU32, wbBoolEnum)
    ]),

    {--- Map Data ---}
    wbRStruct('Map Marker', [
      wbEmpty(XMRK, 'Map Marker Data'),
      wbInteger(FNAM, 'Flags', itU8,
        wbFlags([
          {0} 'Visible',
          {1} 'Can Travel To',
          {2} '"Show All" Hidden'
        ])).SetRequired
           .IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFULLReq,
      wbStruct(TNAM, '', [
        wbInteger('Type', itU8,
          wbEnum([
            {0}  'None',
            {1}  'City',
            {2}  'Settlement',
            {3}  'Encampment',
            {4}  'Natural Landmark',
            {5}  'Cave',
            {6}  'Factory',
            {7}  'Monument',
            {8}  'Military',
            {9}  'Office',
            {1}  'Town Ruins',
            {10} 'Urban Ruins',
            {11} 'Sewer Ruins',
            {12} 'Metro',
            {13} 'Vault'
          ])),
        wbUnused(1)
      ]).SetRequired
    ]),

    wbInteger(XSRF, 'Special Rendering Flags', itU32,
      wbFlags(wbSparseFlags([
        1, 'Imposter',
        2, 'Use Full Shader in LOD'
      ], False, 3))
    ).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbByteArray(XSRD, 'Special Rendering Data', 4),

    {--- X Target Data ---}
    wbFormIDCk(XTRG, 'Target', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA], True),

    {--- Leveled Actor ----}
    wbXLCM,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time').SetRequired,
      wbEmpty(XPPA, 'Patrol Script Marker').SetRequired,
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL]).SetRequired,
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL]).SetRequired
    ]),

    {--- Radio ---}
    wbStruct(XRDO, 'Radio Data', [
      wbFloat('Range Radius'),
      wbInteger('Broadcast Range Type', itU32,
        wbEnum([
          {0} 'Radius',
          {1} 'Everywhere',
          {2} 'Worldspace and Linked Interiors',
          {3} 'Linked Interiors',
          {4} 'Current Cell Only'
        ])),
      wbFloat('Static Percentage'),
      wbFormIDCkNoReach('Position Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, NULL])
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Lock ---}
    wbStruct(XLOC, 'Lock Data', [
      wbInteger('Level', itU8),
      wbUnused(3),
      wbFormIDCkNoReach('Key', [KEYM, NULL]),
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          2, 'Leveled Lock'
        ], False, 3))
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbByteArray('Unknown', 8)
    ], cpNormal, False, nil, 5),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),
    wbFloat(XRAD, 'Radiation'),
    wbFloat(XCHG, 'Charge'),
    wbRStruct('Ammo', [
      wbFormIDCk(XAMT, 'Type', [AMMO]).SetRequired,
      wbInteger(XAMC, 'Count', itS32).SetRequired
    ]),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32,
          wbFlags([
            {0} 'Reflection',
            {1} 'Refraction'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])),

    {--- Lit Water ---}
    wbRArrayS('Lit Water',
      wbFormIDCk(XLTW, 'Water', [REFR])
    ),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Parent Activate Only', itU8, wbBoolEnum),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbActionFlag,
    wbEmpty(ONAM, 'Open by Default'),
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- Generated Data ---}
    wbStruct(XNDP, 'Navmesh Door Link', [
      wbFormIDCk('Navmesh', [NAVM]),
      wbInteger('Triangle', itS16, wbREFRNavmeshTriangleToStr, wbStringToInt),
      wbUnused(2)
    ]),

    wbArray(XPOD, 'Portal Data', wbFormIDCk('Room', [REFR, NULL]), 2),
    wbSizePosRot(XPTL, 'Portal Data'),

    wbInteger(XSED, 'SpeedTree Seed', itU8),

    wbRStruct('Room Data', [
      wbStruct(XRMR, 'Header', [
        wbInteger('Linked Rooms Count', itU16),
        wbByteArray('Unknown', 2)
      ]),
      wbRArrayS('Linked Rooms',
        wbFormIDCk(XLRM, 'Linked Room', [REFR])
      ).SetCountPath('XRMR\Linked Rooms Count')
    ]),

    wbSizePosRot(XOCP, 'Occlusion Plane Data'),
    wbArray(XORD, 'Linked Occlusion Planes', wbFormIDCk('Plane', [REFR, NULL]), [
      'Right',
      'Left',
      'Bottom',
      'Top'
    ]),

    wbXLOD,

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True)
    .SetAddInfo(wbPlacedAddInfo)
    .SetAfterLoad(wbREFRAfterLoad);
FNV:
  wbRefRecord(REFR, 'Placed Object',
    wbFlags(wbFlagsList([
       6, 'Hidden From Local Map',
       7, 'Turn Off Fire',  //Only MSTT placing FXSmokeMed01 [00071FED]?
       8, 'Inaccessible',
       9, 'Casts Shadows/Motion Blur',
      10, 'Persistent',
      11, 'Initially Disabled',
      15, 'Visible When Distant',
      16, 'High Priority LOD',  //Requires Visible When Distant
      25, 'No AI Acquire',
      26, 'Navmesh - Filter',
      27, 'Navmesh - Bounding Box',
      28, 'Reflected By Auto Water', //Only REFRs placed in Exterior?
      29, 'Refracted by Auto Water', //Only REFRs placed in Exterior?
      30, 'Navmesh - Ground',
      31, 'Multibound'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDID,
    {
    wbStruct(RCLR, 'Linked Reference Color (Old Format?)', [
      wbStruct('Link Start Color', [
        wbInteger('Red', itU8),
        wbInteger('Green', itU8),
        wbInteger('Blue', itU8),
        wbUnused(1)
      ]),
      wbStruct('Link End Color', [
        wbInteger('Red', itU8),
        wbInteger('Green', itU8),
        wbInteger('Blue', itU8),
        wbUnused(1)
      ])
    ], cpIgnore),}
    wbByteArray(RCLR, 'Unused', 0, cpIgnore),
    wbFormIDCk(NAME, 'Base', [TREE, SOUN, ACTI, DOOR, STAT, FURN, CONT, ARMO, AMMO, LVLN, LVLC,
                              MISC, WEAP, BOOK, KEYM, ALCH, LIGH, GRAS, ASPC, IDLM, ARMA, CHIP,
                              MSTT, NOTE, PWAT, SCOL, TACT, TERM, TXST, CCRD, IMOD, CMNY], False, cpNormal, True),
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),

    {--- ?? ---}
    wbRagdoll,

    {--- Primitive ---}
    wbStruct(XPRM, 'Primitive', [
      wbStruct('Bounds', [
        wbFloat('X', cpNormal, True, 2, 4),
        wbFloat('Y', cpNormal, True, 2, 4),
        wbFloat('Z', cpNormal, True, 2, 4)
      ]).SetToStr(wbVec3ToStr).IncludeFlag(dfCollapsed, wbCollapseVec3),
      wbFloatColors('Color'),
      wbUnknown(4),
      wbInteger('Type', itU32, wbEnum([
        'None',
        'Box',
        'Sphere',
        'Portal Box'
      ]))
    ]),
    wbInteger(XTRI, 'Collision Layer', itU32, wbEnum([
      'Unidentified',
      'Static',
      'AnimStatic',
      'Transparent',
      'Clutter',
      'Weapon',
      'Projectile',
      'Spell',
      'Biped',
      'Trees',
      'Props',
      'Water',
      'Trigger',
      'Terrain',
      'Trap',
      'Non Collidable',
      'Cloud Trap',
      'Ground',
      'Portal',
      'Debris Small',
      'Debris Large',
      'Acoustic Space',
      'Actor Zone',
      'Projectile Zone',
      'Gas Trap',
      'Shell Casing',
      'Transparent Small',
      'Invisible Wall',
      'Transparent Small Anim',
      'Dead Bip',
      'Char Controller',
      'Avoid Box',
      'Collision Box',
      'Camera Sphere',
      'Door Detection',
      'Camera Pick',
      'Item Pick',
      'Line Of Sight',
      'Path Pick',
      'Custom Pick 1',
      'Custom Pick 2',
      'Spell Explosion',
      'Dropping Pick'
    ])),
    wbEmpty(XMBP, 'MultiBound Primitive Marker'),

    {--- Bound Contents ---}

    {--- Bound Data ---}
    wbVec3(XMBO, 'Bound Half Extents'),

    {--- Teleport ---}
    wbStruct(XTEL, 'Teleport Destination', [
      wbFormIDCk('Door', [REFR], True),
      wbPosRot,
      wbInteger('Flags', itU32, wbFlags([
        'No Alarm'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]),

    {--- Map Data ---}
    wbRStruct('Map Marker', [
      wbEmpty(XMRK, 'Map Marker Data'),
      wbInteger(FNAM, 'Flags', itU8, wbFlags([
        {0x01} 'Visible',
        {0x02} 'Can Travel To',
        {0x04} '"Show All" Hidden'
      ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFULLReq,
      wbStruct(TNAM, '', [
        wbInteger('Type', itU8, wbEnum([
          'None',
          'City',
          'Settlement',
          'Encampment',
          'Natural Landmark',
          'Cave',
          'Factory',
          'Monument',
          'Military',
          'Office',
          'Town Ruins',
          'Urban Ruins',
          'Sewer Ruins',
          'Metro',
          'Vault'
        ])),
        wbUnused(1)
      ], cpNormal, True),
      wbFormIDCk(WMI1, 'Reputation', [REPU])
    ]),

    {--- Audio Data ---}
    wbRStruct('Audio Data', [
      wbEmpty(MMRK, 'Audio Marker'),
      wbString(FULL, 'Audio Marker Location Name'),
      wbFormIDCk(CNAM, 'Audio Location', [ALOC]),
      wbInteger(BNAM, 'Flags', itU32, wbFlags(['Use Controller Values'])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbFloat(MNAM, 'Layer 2 Trigger %', cpNormal, True, 100),
      wbFloat(NNAM, 'Layer 3 Trigger %', cpNormal, True, 100)
    ]),

    wbInteger(XSRF, 'Special Rendering Flags', itU32,
      wbFlags(wbSparseFlags([
        1, 'Imposter',
        2, 'Use Full Shader in LOD'
      ], True))
    ).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbByteArray(XSRD, 'Special Rendering Data', 4),

    {--- X Target Data ---}
    wbFormIDCk(XTRG, 'Target', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA], True),

    {--- Leveled Actor ----}
    wbXLCM,

    {--- Patrol Data ---}
    wbRStruct('Patrol Data', [
      wbFloat(XPRD, 'Idle Time', cpNormal, True),
      wbEmpty(XPPA, 'Patrol Script Marker', cpNormal, True),
      wbFormIDCk(INAM, 'Idle', [IDLE, NULL], False, cpNormal, True),
      wbEmbeddedScriptReq,
      wbFormIDCk(TNAM, 'Topic', [DIAL, NULL], False, cpNormal, True)
    ]),

    {--- Radio ---}
    wbStruct(XRDO, 'Radio Data', [
      wbFloat('Range Radius'),
      wbInteger('Broadcast Range Type', itU32, wbEnum([
        'Radius',
        'Everywhere',
        'Worldspace and Linked Interiors',
        'Linked Interiors',
        'Current Cell Only'
      ])),
      wbFloat('Static Percentage'),
      wbFormIDCkNoReach('Position Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, NULL])
    ]),

    {--- Ownership ---}
    wbOwnership([XCMT, XCMO]),

    {--- Lock ---}
    wbStruct(XLOC, 'Lock Data', [
      wbInteger('Level', itU8),
      wbUnused(3),
      wbFormIDCkNoReach('Key', [KEYM, NULL]),
      wbInteger('Flags', itU8, wbFlags(['', '', 'Leveled Lock'])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3),
      wbByteArray('Unknown', 8)
    ], cpNormal, False, nil, 5),

    {--- Extra ---}
    wbInteger(XCNT, 'Count', itS32),
    wbFloat(XRDS, 'Radius'),
    wbFloat(XHLP, 'Health'),
    wbFloat(XRAD, 'Radiation'),
    wbFloat(XCHG, 'Charge'),
    wbRStruct('Ammo', [
      wbFormIDCk(XAMT, 'Type', [AMMO], False, cpNormal, True),
      wbInteger(XAMC, 'Count', itS32, nil, cpNormal, True)
    ]),

    {--- Reflected By / Refracted By ---}
    wbRArrayS('Reflected/Refracted By',
      wbStructSK(XPWR, [0], 'Water', [
        wbFormIDCk('Reference', [REFR]),
        wbInteger('Type', itU32, wbFlags([
          'Reflection',
          'Refraction'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags)
      ])
    ),

    {--- Lit Water ---}
    wbRArrayS('Lit Water',
      wbFormIDCk(XLTW, 'Water', [REFR])
    ),

    {--- Decals ---}
    wbRArrayS('Linked Decals',
      wbStructSK(XDCR, [0], 'Decal', [
        wbFormIDCk('Reference', [REFR]),
        wbUnknown
      ])
    ),

    {--- Linked Ref ---}
    wbFormIDCk(XLKR, 'Linked Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
    wbStruct(XCLP, 'Linked Reference Color', [
      wbByteColors('Link Start Color'),
      wbByteColors('Link End Color')
    ]),

    {--- Activate Parents ---}
    wbRStruct('Activate Parents', [
      wbInteger(XAPD, 'Flags', itU8, wbFlags([
        'Parent Activate Only'
      ], True)).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbRArrayS('Activate Parent Refs',
        wbStructSK(XAPR, [0], 'Activate Parent Ref', [
          wbFormIDCk('Reference', [REFR, ACRE, ACHR, PGRE, PMIS, PBEA, PLYR]),
          wbFloat('Delay')
        ])
      )
    ]),

    wbStringKC(XATO, 'Activation Prompt'),

    {--- Enable Parent ---}
    wbXESP,

    {--- Emittance ---}
    wbFormIDCk(XEMI, 'Emittance', [LIGH, REGN]),

    {--- MultiBound ---}
    wbFormIDCk(XMBR, 'MultiBound Reference', [REFR]),

    {--- Flags ---}
    wbActionFlag,
    wbEmpty(ONAM, 'Open by Default'),
    wbEmpty(XIBS, 'Ignored By Sandbox'),

    {--- Generated Data ---}
    wbStruct(XNDP, 'Navmesh Door Link', [
      wbFormIDCk('Navmesh', [NAVM]),
      wbInteger('Triangle', itS16, wbREFRNavmeshTriangleToStr, wbStringToInt),
      wbUnused(2)
    ]),

    wbArray(XPOD, 'Portal Data', wbFormIDCk('Room', [REFR, NULL]), 2),
    wbSizePosRot(XPTL, 'Portal Data'),

    wbInteger(XSED, 'SpeedTree Seed', itU8),

    wbRStruct('Room Data', [
      wbStruct(XRMR, 'Header', [
        wbInteger('Linked Rooms Count', itU16),
        wbByteArray('Unknown', 2)
      ]),
      wbRArrayS('Linked Rooms',
        wbFormIDCk(XLRM, 'Linked Room', [REFR])
      ).SetCountPath('XRMR\Linked Rooms Count')
    ]),

    wbSizePosRot(XOCP, 'Occlusion Plane Data'),
    wbArray(XORD, 'Linked Occlusion Planes', wbFormIDCk('Plane', [REFR, NULL]), [
      'Right',
      'Left',
      'Bottom',
      'Top'
    ]),

    wbXLOD,

    {--- 3D Data ---}
    wbXSCL,
    wbDATAPosRot
  ], True)
    .SetAddInfo(wbPlacedAddInfo)
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
FO3:
  wbRecord(REGN, 'Region',
    wbFlags(wbFlagsList([
      6, 'Border Region'
    ])), [
    wbEDID,
    wbICON,
    wbByteColors(RCLR, 'Map Color').SetRequired,
    wbFormIDCkNoReach(WNAM, 'Worldspace', [WRLD]),
    wbRegionAreas,

    wbRArrayS('Region Data Entries', wbRStructSK([0], 'Region Data Entry', [
      {always starts with an RDAT}
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
        wbByteArray('Unused')
      ]).SetRequired,

      {followed by one of these: }

      {--- Objects ---}
      wbArray(RDOT, 'Objects',
      wbStruct('Object', [
        wbFormIDCk('Object', [TREE, STAT, LTEX]),
        wbInteger('Parent Index', itU16, wbHideFFFF),
        wbUnused(2),
        wbFloat('Density'),
        wbInteger('Clustering', itU8),
        wbInteger('Min Slope', itU8),
        wbInteger('Max Slope', itU8),
        wbInteger('Flags', itU8,
          wbFlags([
            {0}'Conform to slope',
            {1}'Paint Vertices',
            {2}'Size Variance +/-',
            {3}'X +/-',
            {4}'Y +/-',
            {5}'Z +/-',
            {6}'Tree',
            {7}'Huge Rock'
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
        wbByteArray('Unknown', 4)
      ])).SetDontShow(wbREGNObjectsDontShow),

      {--- Map ---}
      wbString(RDMP, 'Map Name', 0, cpTranslate).SetDontShow(wbREGNMapDontShow),

      {--- Grass ---}
      wbArrayS(RDGS, 'Grasses', wbStructSK([0], 'Grass', [
        wbFormIDCk('Grass', [GRAS]),
        wbByteArray('Unknown',4)
      ])).SetDontShow(wbREGNGrassDontShow),

      {--- Sound ---}
      wbInteger(RDMD, 'Music Type', itU32, wbMusicEnum, cpIgnore).SetDontShow(wbNeverShow),
      wbFormIDCk(RDMO, 'Music', [MUSC]).SetDontShow(wbREGNSoundDontShow),
      wbRegionSounds,

      {--- Weather ---}
      wbArrayS(RDWT, 'Weather Types', wbStructSK([0], 'Weather Type', [
        wbFormIDCk('Weather', [WTHR]),
        wbInteger('Chance', itU32),
        wbFormIDCk('Global', [GLOB, NULL])
      ])).SetDontShow(wbREGNWeatherDontShow)
    ]))
  ], True);
FNV:
  wbRecord(REGN, 'Region',
    wbFlags(wbFlagsList([
      6, 'Border Region'
    ])), [
    wbEDID,
    wbICON,
    wbByteColors(RCLR, 'Map Color'),
    wbFormIDCkNoReach(WNAM, 'Worldspace', [WRLD]),
    wbRegionAreas,

    wbRArrayS('Region Data Entries', wbRStructSK([0], 'Region Data Entry', [
      {always starts with an RDAT}
      wbStructSK(RDAT, [0], 'Data Header', [
        wbInteger('Type', itU32,
          wbEnum([], [
            2, 'Objects',
            3, 'Weather',
            4, 'Map',
            5, 'Land',
            6, 'Grass',
            7, 'Sound',
            8, 'Imposter'
          ])
        ),
        wbInteger('Override', itU8, wbBoolEnum),
        wbInteger('Priority', itU8),
        wbByteArray('Unused')
      ], cpNormal, True),

      {followed by one of these: }

      {--- Objects ---}
      wbArray(RDOT, 'Objects', wbStruct('Object', [
        wbFormIDCk('Object', [TREE, STAT, LTEX]),
        wbInteger('Parent Index', itU16, wbHideFFFF),
        wbUnused(2),
        wbFloat('Density'),
        wbInteger('Clustering', itU8),
        wbInteger('Min Slope', itU8),
        wbInteger('Max Slope', itU8),
        wbInteger('Flags', itU8, wbFlags([
          {0}'Conform to slope',
          {1}'Paint Vertices',
          {2}'Size Variance +/-',
          {3}'X +/-',
          {4}'Y +/-',
          {5}'Z +/-',
          {6}'Tree',
          {7}'Huge Rock'
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
        ]).SetToStr(wbVec3ToStr).IncludeFlag(dfCollapsed, wbCollapseVec3),
        wbUnused(2),
        wbByteArray('Unknown', 4)
      ]), 0, nil, nil, cpNormal, False, wbREGNObjectsDontShow),

      {--- Map ---}
      wbString(RDMP, 'Map Name', 0, cpTranslate, False, wbREGNMapDontShow),

      {--- Grass ---}
      wbArrayS(RDGS, 'Grasses', wbStructSK([0], 'Grass', [
        wbFormIDCk('Grass', [GRAS]),
        wbByteArray('Unknown',4)
      ]), 0, cpNormal, False, nil, nil, wbREGNGrassDontShow),

      {--- Sound ---}
      wbInteger(RDMD, 'Music Type', itU32, wbMusicEnum, cpIgnore, False, False, wbNeverShow),
      wbFormIDCk(RDMO, 'Music', [MUSC], False, cpNormal, False, wbREGNSoundDontShow),
      wbFormIDCk(RDSI, 'Incidental MediaSet', [MSET], False, cpNormal, False, wbREGNSoundDontShow),
      wbRArray('Battle MediaSets', wbFormIDCk(RDSB, 'Battle MediaSet', [MSET]), cpNormal, False, nil, nil, wbREGNSoundDontShow),
      wbRegionSounds,

      {--- Weather ---}
      wbArrayS(RDWT, 'Weather Types', wbStructSK([0], 'Weather Type', [
        wbFormIDCk('Weather', [WTHR]),
        wbInteger('Chance', itU32),
        wbFormIDCk('Global', [GLOB, NULL])
      ]), 0, cpNormal, False, nil, nil, wbREGNWeatherDontShow),

      {--- Imposter ---}
      wbArrayS(RDID, 'Imposters', wbFormIDCk('Imposter', [REFR]), 0, cpNormal, False, nil, nil, wbREGNImposterDontShow)
    ]))
  ], True);

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

# REPU.Reputation
FNV:
  wbRecord(REPU, 'Reputation', [
    wbEDIDReq,
    wbFULL,
    wbICON,
    wbFloat(DATA, 'Value')
  ]);

# REVB.Reverb Parameters

# RFCT.Visual Effect

# RGDL.Ragdoll
FO3:
  wbRecord(RGDL, 'Ragdoll', [
    wbEDIDReq,
    wbInteger(NVER, 'Version', itU32).SetRequired,
    wbStruct(DATA, 'General Data', [
      wbInteger('Dynamic Bone Count', itU32),
      wbUnused(4),
      wbStruct('Enabled', [
        wbInteger('Feedback', itU8, wbBoolEnum),
        wbInteger('Foot IK (broken, don''t use)', itU8, wbBoolEnum),
        wbInteger('Look IK (broken, don''t use)', itU8, wbBoolEnum),
        wbInteger('Grab IK (broken, don''t use)', itU8, wbBoolEnum),
        wbInteger('Pose Matching', itU8, wbBoolEnum)
      ]),
      wbUnused(1)
    ]).SetRequired,
    wbFormIDCk(XNAM, 'Actor Base', [CREA, NPC_]).SetRequired,
    wbFormIDCk(TNAM, 'Body Part Data', [BPTD]).SetRequired,
    wbStruct(RAFD, 'Feedback Data', [
    {00} wbFloat('Dynamic/Keyframe Blend Amount'),
    {04} wbFloat('Hierarchy Gain'),
    {08} wbFloat('Position Gain'),
    {12} wbFloat('Velocity Gain'),
    {16} wbFloat('Acceleration Gain'),
    {20} wbFloat('Snap Gain'),
    {24} wbFloat('Velocity Damping'),
         wbStruct('Snap Max Settings', [
           {28} wbFloat('Linear Velocity'),
           {32} wbFloat('Angular Velocity'),
           {36} wbFloat('Linear Distance'),
           {40} wbFloat('Angular Distance')
         ]),
         wbStruct('Position Max Velocity', [
           {44} wbFloat('Linear'),
           {48} wbFloat('Angular')
         ]),
         wbStruct('Position Max Velocity', [
           {52} wbInteger('Projectile', itS32, wbDiv(1000)),
           {56} wbInteger('Melee', itS32, wbDiv(1000))
         ])
    ]).SetRequired,
    wbArray(RAFB, 'Feedback Dynamic Bones', wbInteger('Bone', itU16)).SetRequired,
    wbStruct(RAPS, 'Pose Matching Data', [
    {00} wbArray('Match Bones', wbInteger('Bone', itU16, wbHideFFFF), 3),
    {06} wbInteger('Disable On Move', itU8, wbBoolEnum),
    {07} wbUnused(1),
    {08} wbFloat('Motors Strength'),
    {12} wbFloat('Pose Activation Delay Time'),
    {16} wbFloat('Match Error Allowance'),
    {20} wbFloat('Displacement To Disable')
    ]).SetRequired,
    wbString(ANAM, 'Death Pose')
  ]);
FNV:
  wbRecord(RGDL, 'Ragdoll', [
    wbEDIDReq,
    wbInteger(NVER, 'Version', itU32, nil, cpNormal, True),
    wbStruct(DATA, 'General Data', [
      wbInteger('Dynamic Bone Count', itU32),
      wbUnused(4),
      wbStruct('Enabled', [
        wbInteger('Feedback', itU8, wbBoolEnum),
        wbInteger('Foot IK (broken, don''t use)', itU8, wbBoolEnum),
        wbInteger('Look IK (broken, don''t use)', itU8, wbBoolEnum),
        wbInteger('Grab IK (broken, don''t use)', itU8, wbBoolEnum),
        wbInteger('Pose Matching', itU8, wbBoolEnum)
      ]),
      wbUnused(1)
    ], cpNormal, True),
    wbFormIDCk(XNAM, 'Actor Base', [CREA, NPC_], False, cpNormal, True),
    wbFormIDCk(TNAM, 'Body Part Data', [BPTD], False, cpNormal, True),
    wbStruct(RAFD, 'Feedback Data', [
    {00} wbFloat('Dynamic/Keyframe Blend Amount'),
    {04} wbFloat('Hierarchy Gain'),
    {08} wbFloat('Position Gain'),
    {12} wbFloat('Velocity Gain'),
    {16} wbFloat('Acceleration Gain'),
    {20} wbFloat('Snap Gain'),
    {24} wbFloat('Velocity Damping'),
         wbStruct('Snap Max Settings', [
           {28} wbFloat('Linear Velocity'),
           {32} wbFloat('Angular Velocity'),
           {36} wbFloat('Linear Distance'),
           {40} wbFloat('Angular Distance')
         ]),
         wbStruct('Position Max Velocity', [
           {44} wbFloat('Linear'),
           {48} wbFloat('Angular')
         ]),
         wbStruct('Position Max Velocity', [
           {52} wbInteger('Projectile', itS32, wbDiv(1000)),
           {56} wbInteger('Melee', itS32, wbDiv(1000))
         ])
    ], cpNormal, False),
    wbArray(RAFB, 'Feedback Dynamic Bones', wbInteger('Bone', itU16), 0, nil, nil, cpNormal, False),
    wbStruct(RAPS, 'Pose Matching Data', [
    {00} wbArray('Match Bones', wbInteger('Bone', itU16, wbHideFFFF), 3),
    {06} wbInteger('Flags', itU8, wbFlags([
           'Disable On Move'
         ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
    {07} wbUnused(1),
    {08} wbFloat('Motors Strength'),
    {12} wbFloat('Pose Activation Delay Time'),
    {16} wbFloat('Match Error Allowance'),
    {20} wbFloat('Displacement To Disable')
    ], cpNormal, True),
    wbString(ANAM, 'Death Pose')
  ]);

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

# SCOL.Static Collection
FO3:
  wbRecord(SCOL, 'Static Collection',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD',
      9, 'On Local Map',
     10, 'Quest Item',
     15, 'Visible When Distant',
     25, 'Obstacle',
     26, 'Navmesh - Filter',
     27, 'Navmesh - Bounding Box',
     30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbRArray('Parts',
      wbRStruct('Part', [
        wbFormIDCk(ONAM, 'Static', [STAT]),
        wbStaticPartPlacements
      ]).SetRequired
    ).SetRequired
  ]);
FNV:
  wbRecord(SCOL, 'Static Collection',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD',
      9, 'On Local Map',
     10, 'Quest Item',
     15, 'Visible When Distant',
     25, 'Obstacle',
     26, 'Navmesh - Filter',
     27, 'Navmesh - Bounding Box',
     30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbRArray('Parts', wbStaticPart)
  ]);

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
FO3:
  wbRecord(SCPT, 'Script', [
    wbEDIDReq,
    wbSCHRReq,
    wbByteArray(SCDA, 'Compiled Script'),
    wbStringScript(SCTX, 'Script Source'),
    wbRArrayS('Local Variables', wbRStructSK([0], 'Local Variable', [
      wbSLSD,
      wbString(SCVR, 'Name', 0, cpCritical).SetRequired
    ])),
    wbSCROs
  ]).SetToStr(wbScriptToStr);
FNV:
  wbRecord(SCPT, 'Script', [
    wbEDIDReq,
    wbSCHRReq,
    wbByteArray(SCDA, 'Compiled Script'),
    wbStringScript(SCTX, 'Script Source', 0, cpNormal{, True}),
    wbRArrayS('Local Variables', wbRStructSK([0], 'Local Variable', [
      wbSLSD,
      wbString(SCVR, 'Name', 0, cpCritical, True)
    ])),
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

# SLPD.Sleep Deprivation Stage
FNV:
  wbRecord(SLPD, 'Sleep Deprivation Stage', [
    wbEDIDReq,
    wbStruct(DATA, '', [
      wbInteger('Trigger Threshold', itU32),
      wbFormIDCk('Actor Effect', [SPEL])
    ], cpNormal, True)
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
FO3:
  wbRecord(SOUN, 'Sound', [
    wbEDIDReq,
    wbOBND(True),
    wbString(FNAM, 'Sound FileName'),
    wbRUnion('Sound Data', [
      wbStruct(SNDD, 'Sound Data', [
        wbInteger('Minimum Attenuation Distance', itU8, wbMul(5)),
        wbInteger('Maximum Attenuation Distance', itU8, wbMul(100)),
        wbInteger('Frequency Adjustment %', itS8),
        wbUnused(1),
        wbInteger('Flags', itU32,
          wbFlags([
            {0}  'Random Frequency Shift',
            {1}  'Play At Random',
            {2}  'Environment Ignored',
            {3}  'Random Location',
            {4}  'Loop',
            {5}  'Menu Sound',
            {6}  '2D',
            {7}  '360 LFE',
            {8}  'Dialogue Sound',
            {9}  'Envelope Fast',
            {10} 'Envelope Slow',
            {11} '2D Radius',
            {12} 'Mute When Submerged'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Static attenuation cdB', itS16),
        wbInteger('Stop time ', itU8),
        wbInteger('Start time ', itU8),
        wbArray('Attenuation Curve', wbInteger('Point', itS16), 5),
        wbInteger('Reverb Attenuation Control', itS16),
        wbInteger('Priority', itS32),
        wbByteArray('Unknown', 8)
      ]).SetRequired,
      wbStruct(SNDX, 'Sound Data', [
        wbInteger('Minimum attenuation distance', itU8, wbMul(5)),
        wbInteger('Maximum attenuation distance', itU8, wbMul(100)),
        wbInteger('Frequency adjustment %', itS8),
        wbUnused(1),
        wbInteger('Flags', itU32,
          wbFlags([
            {0}  'Random Frequency Shift',
            {1}  'Play At Random',
            {2}  'Environment Ignored',
            {3}  'Random Location',
            {4}  'Loop',
            {5}  'Menu Sound',
            {6}  '2D',
            {7}  '360 LFE',
            {8}  'Dialogue Sound',
            {9}  'Envelope Fast',
            {10} 'Envelope Slow',
            {11} '2D Radius',
            {12} 'Mute When Submerged'
          ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Static attenuation cdB', itS16),
        wbInteger('Stop time ', itU8),
        wbInteger('Start time ', itU8)
      ]).SetRequired
    ]).SetRequired,
    wbArray(ANAM, 'Attenuation Curve', wbInteger('Point', itS16), 5).SetDontShow(wbNeverShow),
    wbInteger(GNAM, 'Reverb Attenuation Control', itS16).SetDontShow(wbNeverShow),
    wbInteger(HNAM, 'Priority', itS32).SetDontShow(wbNeverShow)
  ]).SetAfterLoad(wbSOUNAfterLoad);
FNV:
  wbRecord(SOUN, 'Sound', [
    wbEDIDReq,
    wbOBND(True),
    wbString(FNAM, 'Sound FileName'),
    wbInteger(RNAM, 'Random Chance %', itU8),
    wbRUnion('Sound Data', [
      wbStruct(SNDD, 'Sound Data', [
        wbInteger('Minimum Attenuation Distance', itU8, wbMul(5)),
        wbInteger('Maximum Attenuation Distance', itU8, wbMul(100)),
        wbInteger('Frequency Adjustment %', itS8),
        wbUnused(1),
        wbInteger('Flags', itU32, wbFlags([
          {0x0001} 'Random Frequency Shift',
          {0x0002} 'Play At Random',
          {0x0004} 'Environment Ignored',
          {0x0008} 'Random Location',
          {0x0010} 'Loop',
          {0x0020} 'Menu Sound',
          {0x0040} '2D',
          {0x0080} '360 LFE',
          {0x0100} 'Dialogue Sound',
          {0x0200} 'Envelope Fast',
          {0x0400} 'Envelope Slow',
          {0x0800} '2D Radius',
          {0x1000} 'Mute When Submerged',
          {0x2000} 'Start at Random Position'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Static attenuation cdB', itS16),
        wbInteger('Stop time ', itU8, wbAlocTime),
        wbInteger('Start time ', itU8, wbAlocTime),
        wbArray('Attenuation Curve', wbInteger('Point', itS16), 5),
        wbInteger('Reverb Attenuation Control', itS16),
        wbInteger('Priority', itS32),
        wbStruct('Loop Points', [
          wbInteger('Begin', itS32),
          wbInteger('End', itS32)
        ])

      ], cpNormal, True),
      wbStruct(SNDX, 'Sound Data', [
        wbInteger('Minimum attenuation distance', itU8, wbMul(5)),
        wbInteger('Maximum attenuation distance', itU8, wbMul(100)),
        wbInteger('Frequency adjustment %', itS8),
        wbUnused(1),
        wbInteger('Flags', itU32, wbFlags([
          {0x0001} 'Random Frequency Shift',
          {0x0002} 'Play At Random',
          {0x0004} 'Environment Ignored',
          {0x0008} 'Random Location',
          {0x0010} 'Loop',
          {0x0020} 'Menu Sound',
          {0x0040} '2D',
          {0x0080} '360 LFE',
          {0x0100} 'Dialogue Sound',
          {0x0200} 'Envelope Fast',
          {0x0400} 'Envelope Slow',
          {0x0800} '2D Radius',
          {0x1000} 'Mute When Submerged'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbInteger('Static attenuation cdB', itS16),
        wbInteger('Stop time ', itU8),
        wbInteger('Start time ', itU8)
      ], cpNormal, True)
    ], [], cpNormal, True),
    wbArray(ANAM, 'Attenuation Curve', wbInteger('Point', itS16), 5, nil, nil, cpNormal, False, wbNeverShow),
    wbInteger(GNAM, 'Reverb Attenuation Control', itS16, nil, cpNormal, False, False, wbNeverShow),
    wbInteger(HNAM, 'Priority', itS32, nil, cpNormal, False, False, wbNeverShow)
  ], False, nil, cpNormal, False, wbSOUNAfterLoad);

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
FO3:
  wbRecord(SPEL, 'Actor Effect', [
    wbEDIDReq,
    wbFULL,
    wbStruct(SPIT, '', [
      wbInteger('Type', itU32,
        wbEnum([
          {0} 'Actor Effect',
          {1} 'Disease',
          {2} 'Power',
          {3} 'Lesser Power',
          {4} 'Ability',
          {5} 'Poison'
        ], [
          10, 'Addiction'
        ])),
      wbInteger('Cost (Unused)', itU32),
      wbInteger('Level (Unused)', itU32),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'No Auto-Calc',
          {1} 'Immune to Silence 1?',
          {2} 'PC Start Effect',
          {3} 'Immune to Silence 2?',
          {4} 'Area Effect Ignores LOS',
          {5} 'Script Effect Always Applies',
          {6} 'Disable Absorb/Reflect',
          {7} 'Force Touch Explode'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ]).SetRequired,
    wbEffectsReq
  ]);
FNV:
  wbRecord(SPEL, 'Actor Effect', [
    wbEDIDReq,
    wbFULL,
    wbStruct(SPIT, '', [
      wbInteger('Type', itU32, wbEnum([
        {0} 'Actor Effect',
        {1} 'Disease',
        {2} 'Power',
        {3} 'Lesser Power',
        {4} 'Ability',
        {5} 'Poison',
        {6} '',
        {7} '',
        {8} '',
        {9} '',
       {10} 'Addiction'
      ])),
      wbInteger('Cost (Unused)', itU32),
      wbInteger('Level (Unused)', itU32, wbEnum([
        {0} 'Unused'
      ])),
      wbInteger('Flags', itU8, wbFlags([
        {0x00000001} 'No Auto-Calc',
        {0x00000002} 'Immune to Silence 1?',
        {0x00000004} 'PC Start Effect',
        {0x00000008} 'Immune to Silence 2?',
        {0x00000010} 'Area Effect Ignores LOS',
        {0x00000020} 'Script Effect Always Applies',
        {0x00000040} 'Disable Absorb/Reflect',
        {0x00000080} 'Force Touch Explode'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbUnused(3)
    ], cpNormal, True),
    wbEffectsReq
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
FO3:
  wbRecord(STAT, 'Static',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD',
      9, 'On Local Map',
      10, 'Quest Item',
      15, 'Visible When Distant',
      25, 'Obstacle',
      26, 'Navmesh - Filter',
      27, 'Navmesh - Bounding Box',
      30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel
  ]);
FNV:
  wbRecord(STAT, 'Static',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD',
      9, 'On Local Map',
      10, 'Quest Item',
      15, 'Visible When Distant',
      25, 'Obstacle',
      26, 'Navmesh - Filter',
      27, 'Navmesh - Bounding Box',
      30, 'Navmesh - Ground'
    ])).SetFlagHasDontShow(26, wbFlagNavmeshFilterDontShow)
       .SetFlagHasDontShow(27, wbFlagNavmeshBoundingBoxDontShow)
       .SetFlagHasDontShow(30, wbFlagNavmeshGroundDontShow), [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel,
    wbInteger(BRUS, 'Passthrough Sound', itS8, wbEnum([
      'BushA',
      'BushB',
      'BushC',
      'BushD',
      'BushE',
      'BushF',
      'BushG',
      'BushH',
      'BushI',
      'BushJ'
    ], [
      -1, 'NONE',
      11, 'Unknown 11'
    ])),
    wbFormIDCk(RNAM, 'Sound - Looping/Random', [SOUN])
  ]);

# STDT.xx

# SUNP.xx

# TACT.Talking Activator
FO3:
  wbRecord(TACT, 'Talking Activator',
    wbFlags(wbFlagsList([
      9, 'On Local Map',
     10, 'Quest Item',
     13, 'No Voice Filter',
     16, 'Random Anim Start',
     17, 'Radio Station',
     28, 'Non-Pipboy',     //Requires Radio Station
     30, 'Cont. Broadcast' //Requires Radio Station
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbSCRI,
    wbDEST,
    wbFormIDCk(SNAM, 'Sound', [SOUN]),
    wbFormIDCk(VNAM, 'Voice Type', [VTYP])
  ]);
FNV:
  wbRecord(TACT, 'Talking Activator',
    wbFlags(wbFlagsList([
      9, 'On Local Map',
     10, 'Quest Item',
     13, 'No Voice Filter',
     16, 'Random Anim Start',
     17, 'Radio Station',
     28, 'Non-Pipboy',     //Requires Radio Station
     30, 'Cont. Broadcast' //Requires Radio Station
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel(True),
    wbSCRI,
    wbDEST,
    wbFormIDCk(SNAM, 'Looping Sound', [SOUN]),
    wbFormIDCk(VNAM, 'Voice Type', [VTYP]),
    wbFormIDCk(INAM, 'Radio Template', [SOUN])
  ]);
TES5:
  wbRecord(TACT, 'Talking Activator',
    wbFlags(wbFlagsList([
      9, 'Hidden From Local Map',
     16, 'Random Anim Start',
     17, 'Radio Station'
    ]), [17]), [
    wbEDID,
    wbVMAD,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbDEST,
    wbKeywords,
    wbUnknown(PNAM, cpIgnore, True),
    wbFormIDCk(SNAM, 'Looping Sound', [SNDR]),
    wbUnknown(FNAM, cpIgnore, True),
    wbFormIDCk(VNAM, 'Voice Type', [VTYP])
  ]);

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
FO3:
  wbRecord(TES4, 'Main File Header',
    wbFlags(wbFlagsList([
      0, 'ESM',
      4, 'Optimized'
    ])), [
    wbHEDR,
    wbByteArray(OFST, 'Unknown', 0, cpIgnore),
    wbByteArray(DELE, 'Unknown', 0, cpIgnore),
    wbString(CNAM, 'Author', 0, cpTranslate).SetRequired,
    wbString(SNAM, 'Description', 0, cpTranslate),
    wbRArray('Master Files', wbRStruct('Master File', [
      wbStringForward(MAST, 'FileName').SetRequired,
      wbByteArray(DATA, 'Unused', 8, cpIgnore).SetRequired
    ], [ONAM])).IncludeFlag(dfInternalEditOnly, not wbAllowMasterFilesEdit),
    wbArray(ONAM, 'Overridden Forms',
      wbFormIDCk('Form', [REFR, ACHR, ACRE, PMIS, PBEA, PGRE, LAND, NAVM])
    ).SetDontShow(wbTES4ONAMDontShow),
    wbByteArray(SCRN, 'Screenshot')
  ], True, nil, cpNormal, True);
FNV:
  wbRecord(TES4, 'Main File Header',
    wbFlags(wbFlagsList([
      0, 'ESM',
      4, 'Optimized'
    ])), [
    wbHEDR,
    wbByteArray(OFST, 'Unknown', 0, cpIgnore),
    wbByteArray(DELE, 'Unknown', 0, cpIgnore),
    wbString(CNAM, 'Author', 0, cpTranslate, True),
    wbString(SNAM, 'Description', 0, cpTranslate),
    wbRArray('Master Files', wbRStruct('Master File', [
      wbStringForward(MAST, 'FileName', 0, cpNormal, True),
      wbByteArray(DATA, 'Unused', 8, cpIgnore, True)
    ], [ONAM])).IncludeFlag(dfInternalEditOnly, not wbAllowMasterFilesEdit),
    wbArray(ONAM, 'Overridden Forms', wbFormIDCk('Form', [REFR, ACHR, ACRE, PMIS, PBEA, PGRE, LAND, NAVM]), 0, nil, nil, cpNormal, False, wbTES4ONAMDontShow),
    wbByteArray(SCRN, 'Screenshot')
  ], True, nil, cpNormal, True);

# TERM.Computer Terminals
FO3:
  wbRecord(TERM, 'Terminal',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbSCRI,
    wbDEST,
    wbDESCReq,
    wbFormIDCk(SNAM, 'Sound - Looping', [SOUN]),
    wbFormIDCk(PNAM, 'Password Note', [NOTE]),
    wbStruct(DNAM, '', [
      wbInteger('Base Hacking Difficulty', itU8,
        wbEnum([
          {0} 'Very Easy',
          {1} 'Easy',
          {2} 'Average',
          {3} 'Hard',
          {4} 'Very Hard',
          {5} 'Requires Key'
        ])),
      wbInteger('Flags', itU8,
        wbFlags([
          {0} 'Leveled',
          {1} 'Unlocked',
          {2} 'Alternate Colors',
          {3} 'Hide Welcome Text when displaying Image'
        ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('ServerType', itU8,
        wbEnum([
          {0} '-Server 1-',
          {1} '-Server 2-',
          {2} '-Server 3-',
          {3} '-Server 4-',
          {4} '-Server 5-',
          {5} '-Server 6-',
          {6} '-Server 7-',
          {7} '-Server 8-',
          {8} '-Server 9-',
          {9} '-Server 10-'
        ])),
      wbUnused(1)
    ]).SetRequired,
    wbRArray('Menu Items',
      wbRStruct('Menu Item', [
        wbStringKC(ITXT, 'Item Text', 0, cpTranslate),
        wbStringKC(RNAM, 'Result Text', 0, cpTranslate).SetRequired,
        wbInteger(ANAM, 'Flags', itU8,
          wbFlags([
            {0 }'Add Note',
            {1} 'Force Redraw'
          ])).SetRequired
             .IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbFormIDCk(INAM, 'Display Note', [NOTE]),
        wbFormIDCk(TNAM, 'Sub Menu', [TERM]),
        wbEmbeddedScriptReq,
        wbConditions
      ])
    )
  ]);
FNV:
  wbRecord(TERM, 'Terminal',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      16, 'Random Anim Start'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbSCRI,
    wbDEST,
    wbDESCReq,
    wbFormIDCk(SNAM, 'Sound - Looping', [SOUN]),
    wbFormIDCk(PNAM, 'Password Note', [NOTE]),
    wbStruct(DNAM, '', [
      wbInteger('Base Hacking Difficulty', itU8, wbEnum([
        'Very Easy',
        'Easy',
        'Average',
        'Hard',
        'Very Hard',
        'Requires Key'
      ])),
      wbInteger('Flags', itU8, wbFlags([
        'Leveled',
        'Unlocked',
        'Alternate Colors',
        'Hide Welcome Text when displaying Image'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbInteger('ServerType', itU8, wbEnum([
        '-Server 1-',
        '-Server 2-',
        '-Server 3-',
        '-Server 4-',
        '-Server 5-',
        '-Server 6-',
        '-Server 7-',
        '-Server 8-',
        '-Server 9-',
        '-Server 10-'
      ])),
      wbUnused(1)
    ], cpNormal, True),
    wbRArray('Menu Items',
      wbRStruct('Menu Item', [
        wbStringKC(ITXT, 'Item Text', 0, cpTranslate),
        wbStringKC(RNAM, 'Result Text', 0, cpTranslate, True),
        wbInteger(ANAM, 'Flags', itU8, wbFlags([
          'Add Note',
          'Force Redraw'
        ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
        wbFormIDCk(INAM, 'Display Note', [NOTE]),
        wbFormIDCk(TNAM, 'Sub Menu', [TERM]),
        wbEmbeddedScriptReq,
        wbConditions
      ])
    )
  ]);

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
FO3:
  wbRecord(TREE, 'Tree', [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbICONReq,
    wbArrayS(SNAM, 'SpeedTree Seeds', wbInteger('SpeedTree Seed', itU32)).SetRequired,
    wbStruct(CNAM, 'Tree Data', [
      wbFloat('Leaf Curvature'),
      wbFloat('Minimum Leaf Angle'),
      wbFloat('Maximum Leaf Angle'),
      wbFloat('Branch Dimming Value'),
      wbFloat('Leaf Dimming Value'),
      wbInteger('Shadow Radius', itS32),
      wbFloat('Rock Speed'),
      wbFloat('Rustle Speed')
    ]).SetRequired,
    wbStruct(BNAM, 'Billboard Dimensions', [
      wbFloat('Width'),
      wbFloat('Height')
    ]).SetRequired
  ]);
FNV:
  wbRecord(TREE, 'Tree',
    wbFlags(wbFlagsList([
      6, 'Has Tree LOD'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbGenericModel(True),
    wbICONReq,
    wbDEST,
    wbArrayS(SNAM, 'SpeedTree Seeds', wbInteger('SpeedTree Seed', itU32), 0, cpNormal, True),
    wbStruct(CNAM, 'Tree Data', [
      wbFloat('Leaf Curvature'),
      wbFloat('Minimum Leaf Angle'),
      wbFloat('Maximum Leaf Angle'),
      wbFloat('Branch Dimming Value'),
      wbFloat('Leaf Dimming Value'),
      wbInteger('Shadow Radius', itS32),
      wbFloat('Rock Speed'),
      wbFloat('Rustle Speed')
    ], cpNormal, True),
    wbStruct(BNAM, 'Billboard Dimensions', [
      wbFloat('Width'),
      wbFloat('Height')
    ], cpNormal, True)
  ]);

# TRNS.TRNS Record

# TXST.Texture Set
FO3:
  wbRecord(TXST, 'Texture Set', [
    wbEDIDReq,
    wbOBND(True),
    wbRStruct('Textures (RGB/A)', [
      wbString(TX00,'Base Image / Transparency'),
      wbString(TX01,'Normal Map / Specular'),
      wbString(TX02,'Environment Map Mask / ?'),
      wbString(TX03,'Glow Map / Unused'),
      wbString(TX04,'Parallax Map / Unused'),
      wbString(TX05,'Environment Map / Unused')
    ]),
    wbDODT,
    wbInteger(DNAM, 'No Specular', itU16, wbBoolEnum).SetRequired
  ]);
FNV:
  wbRecord(TXST, 'Texture Set', [
    wbEDIDReq,
    wbOBND(True),
    wbRStruct('Textures (RGB/A)', [
      wbString(TX00,'Base Image / Transparency'),
      wbString(TX01,'Normal Map / Specular'),
      wbString(TX02,'Environment Map Mask / ?'),
      wbString(TX03,'Glow Map / Unused'),
      wbString(TX04,'Parallax Map / Unused'),
      wbString(TX05,'Environment Map / Unused')
    ]),
    wbDODT,
    wbInteger(DNAM, 'Flags', itU16, wbFlags([
      'No Specular Map'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);

# VTYP.Voice Type
FO3:
  wbRecord(VTYP, 'Voice Type', [
    wbEDIDReq,
    wbInteger(DNAM, 'Flags', itU8,
      wbFlags([
        {0} 'Allow Default Dialog',
        {1} 'Female'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);
FNV:
  wbRecord(VTYP, 'Voice Type', [
    wbEDIDReq,
    wbInteger(DNAM, 'Flags', itU8, wbFlags([
      'Allow Default Dialog',
      'Female'
    ]), cpNormal, False).IncludeFlag(dfCollapsed, wbCollapseFlags)
  ]);

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
FO3:
  wbRecord(WATR, 'Water', [
    wbEDIDReq,
    wbFULL,
    wbString(NNAM, 'Noise Map').SetRequired,
    wbInteger(ANAM, 'Opacity', itU8)
      .SetDefaultNativeValue(75)
      .SetRequired,
    wbInteger(FNAM, 'Flags', itU8,
      wbFlags([
        {0}'Causes Damage',
        {1}'Reflective'
      ])).SetRequired
         .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbString(MNAM, 'Material ID').SetRequired,
    wbFormIDCk(SNAM, 'Sound', [SOUN]),
    wbFormIDCk(XNAM, 'Actor Effect', [SPEL]),
    wbInteger(DATA, 'Damage', itU16, nil, cpNormal, True, True),
    wbStruct(DNAM, 'Visual Data', [
      wbUnused(16),
      wbFloat('Water Properties - Sun Power').SetDefaultNativeValue(50),
      wbFloat('Water Properties - Reflectivity Amount').SetDefaultNativeValue(0.5),
      wbFloat('Water Properties - Fresnel Amount').SetDefaultNativeValue(0.025),
      wbUnused(4),
      wbFloat('Fog Properties - Above Water - Fog Distance - Near Plane'),
      wbFloat('Fog Properties - Above Water - Fog Distance - Far Plane'),
      wbByteColors('Shallow Color', 0, 128, 128),
      wbByteColors('Deep Color', 0, 0, 25),
      wbByteColors('Reflection Color', 255, 255, 255),
      wbUnused(4),
      wbFloat('Rain Simulator - Force').SetDefaultNativeValue(0.1),
      wbFloat('Rain Simulator - Velocity').SetDefaultNativeValue(0.6),
      wbFloat('Rain Simulator - Falloff').SetDefaultNativeValue(0.985),
      wbFloat('Rain Simulator - Dampner').SetDefaultNativeValue(2),
      wbFloat('Displacement Simulator - Starting Size').SetDefaultNativeValue(0.01),
      wbFloat('Displacement Simulator - Force').SetDefaultNativeValue(0.4),
      wbFloat('Displacement Simulator - Velocity').SetDefaultNativeValue(0.6),
      wbFloat('Displacement Simulator - Falloff').SetDefaultNativeValue(0.985),
      wbFloat('Displacement Simulator - Dampner').SetDefaultNativeValue(10),
      wbFloat('Rain Simulator - Starting Size').SetDefaultNativeValue(0.05),
      wbFloat('Noise Properties - Normals - Noise Scale').SetDefaultNativeValue(1),
      wbFloat('Noise Properties - Noise Layer One - Wind Direction'),
      wbFloat('Noise Properties - Noise Layer Two - Wind Direction'),
      wbFloat('Noise Properties - Noise Layer Three - Wind Direction'),
      wbFloat('Noise Properties - Noise Layer One - Wind Speed'),
      wbFloat('Noise Properties - Noise Layer Two - Wind Speed'),
      wbFloat('Noise Properties - Noise Layer Three - Wind Speed'),
      wbFloat('Noise Properties - Normals - Depth Falloff Start'),
      wbFloat('Noise Properties - Normals - Depth Falloff End'),
      wbFloat('Fog Properties - Above Water - Fog Amount').SetDefaultNativeValue(1),
      wbFloat('Noise Properties - Normals - UV Scale').SetDefaultNativeValue(500),
      wbFloat('Fog Properties - Under Water - Fog Amount').SetDefaultNativeValue(1),
      wbFloat('Fog Properties - Under Water - Fog Distance - Near Plane'),
      wbFloat('Fog Properties - Under Water - Fog Distance - Far Plane').SetDefaultNativeValue(1000),
      wbFloat('Water Properties - Distortion Amount').SetDefaultNativeValue(250),
      wbFloat('Water Properties - Shininess').SetDefaultNativeValue(100),
      wbFloat('Water Properties - Reflection HDR Multiplier').SetDefaultNativeValue(1),
      wbFloat('Water Properties - Light Radius').SetDefaultNativeValue(10000),
      wbFloat('Water Properties - Light Brightness').SetDefaultNativeValue(1),
      wbFloat('Noise Properties - Noise Layer One - UV Scale').SetDefaultNativeValue(100),
      wbFloat('Noise Properties - Noise Layer Two - UV Scale').SetDefaultNativeValue(100),
      wbFloat('Noise Properties - Noise Layer Three - UV Scale').SetDefaultNativeValue(100),
      wbFloat('Noise Properties - Noise Layer One - Amplitude Scale'),
      wbFloat('Noise Properties - Noise Layer Two - Amplitude Scale'),
      wbFloat('Noise Properties - Noise Layer Three - Amplitude Scale')
    ], cpNormal, True, nil, 43),
    wbStruct(DATA, 'Visual Data', [
      wbUnused(16),
      wbFloat('Water Properties - Sun Power'),
      wbFloat('Water Properties - Reflectivity Amount'),
      wbFloat('Water Properties - Fresnel Amount'),
      wbUnused(4),
      wbFloat('Fog Properties - Above Water - Fog Distance - Near Plane'),
      wbFloat('Fog Properties - Above Water - Fog Distance - Far Plane'),
      wbByteColors('Shallow Color'),
      wbByteColors('Deep Color'),
      wbByteColors('Reflection Color'),
      wbUnused(4),
      wbFloat('Rain Simulator - Force'),
      wbFloat('Rain Simulator - Velocity'),
      wbFloat('Rain Simulator - Falloff'),
      wbFloat('Rain Simulator - Dampner'),
      wbFloat('Displacement Simulator - Starting Size'),
      wbFloat('Displacement Simulator - Force'),
      wbFloat('Displacement Simulator - Velocity'),
      wbFloat('Displacement Simulator - Falloff'),
      wbFloat('Displacement Simulator - Dampner'),
      wbFloat('Rain Simulator - Starting Size'),
      wbFloat('Noise Properties - Normals - Noise Scale'),
      wbFloat('Noise Properties - Noise Layer One - Wind Direction'),
      wbFloat('Noise Properties - Noise Layer Two - Wind Direction'),
      wbFloat('Noise Properties - Noise Layer Three - Wind Direction'),
      wbFloat('Noise Properties - Noise Layer One - Wind Speed'),
      wbFloat('Noise Properties - Noise Layer Two - Wind Speed'),
      wbFloat('Noise Properties - Noise Layer Three - Wind Speed'),
      wbFloat('Noise Properties - Normals - Depth Falloff Start'),
      wbFloat('Noise Properties - Normals - Depth Falloff End'),
      wbFloat('Fog Properties - Above Water - Fog Amount'),
      wbFloat('Noise Properties - Normals - UV Scale'),
      wbFloat('Fog Properties - Under Water - Fog Amount'),
      wbFloat('Fog Properties - Under Water - Fog Distance - Near Plane'),
      wbFloat('Fog Properties - Under Water - Fog Distance - Far Plane'),
      wbFloat('Water Properties - Distortion Amount'),
      wbFloat('Water Properties - Shininess'),
      wbFloat('Water Properties - Reflection HDR Multiplier'),
      wbFloat('Water Properties - Light Radius'),
      wbFloat('Water Properties - Light Brightness'),
      wbFloat('Noise Properties - Noise Layer One - UV Scale'),
      wbFloat('Noise Properties - Noise Layer Two - UV Scale'),
      wbFloat('Noise Properties - Noise Layer Three - UV Scale'),
      wbInteger('Damage (Old Format)', itU16)
    ]).SetDontShow(wbAlwaysDontShow),
    wbUnused(GNAM, 12).SetRequired
  ]).SetAfterLoad(wbWATRAfterLoad);
FNV:
  wbRecord(WATR, 'Water', [
    wbEDIDReq,
    wbFULL,
    wbString(NNAM, 'Noise Map', 0, cpNormal, True),
    wbInteger(ANAM, 'Opacity', itU8, nil, cpNormal, True),
    wbInteger(FNAM, 'Flags', itU8, wbFlags([
      {0}'Causes Damage',
      {1}'Reflective'
    ]), cpNormal, True).IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbString(MNAM, 'Material ID', 0, cpNormal, True),
    wbFormIDCk(SNAM, 'Sound', [SOUN]),
    wbFormIDCk(XNAM, 'Actor Effect', [SPEL]),
    wbInteger(DATA, 'Damage', itU16, nil, cpNormal, True, True),
    wbRUnion('Visual Data', [
      wbStruct(DNAM, 'Visual Data', [
        wbFloat('Unknown'),
        wbFloat('Unknown'),
        wbFloat('Unknown'),
        wbFloat('Unknown'),
        wbFloat('Water Properties - Sun Power'),
        wbFloat('Water Properties - Reflectivity Amount'),
        wbFloat('Water Properties - Fresnel Amount'),
        wbUnused(4),
        wbFloat('Fog Properties - Above Water - Fog Distance - Near Plane'),
        wbFloat('Fog Properties - Above Water - Fog Distance - Far Plane'),
        wbByteColors('Shallow Color'),
        wbByteColors('Deep Color'),
        wbByteColors('Reflection Color'),
        wbUnused(4),
        wbFloat('Rain Simulator - Force'),
        wbFloat('Rain Simulator - Velocity'),
        wbFloat('Rain Simulator - Falloff'),
        wbFloat('Rain Simulator - Dampner'),
        wbFloat('Displacement Simulator - Starting Size'),
        wbFloat('Displacement Simulator - Force'),
        wbFloat('Displacement Simulator - Velocity'),
        wbFloat('Displacement Simulator - Falloff'),
        wbFloat('Displacement Simulator - Dampner'),
        wbFloat('Rain Simulator - Starting Size'),
        wbFloat('Noise Properties - Normals - Noise Scale'),
        wbFloat('Noise Properties - Noise Layer One - Wind Direction'),
        wbFloat('Noise Properties - Noise Layer Two - Wind Direction'),
        wbFloat('Noise Properties - Noise Layer Three - Wind Direction'),
        wbFloat('Noise Properties - Noise Layer One - Wind Speed'),
        wbFloat('Noise Properties - Noise Layer Two - Wind Speed'),
        wbFloat('Noise Properties - Noise Layer Three - Wind Speed'),
        wbFloat('Noise Properties - Normals - Depth Falloff Start'),
        wbFloat('Noise Properties - Normals - Depth Falloff End'),
        wbFloat('Fog Properties - Above Water - Fog Amount'),
        wbFloat('Noise Properties - Normals - UV Scale'),
        wbFloat('Fog Properties - Under Water - Fog Amount'),
        wbFloat('Fog Properties - Under Water - Fog Distance - Near Plane'),
        wbFloat('Fog Properties - Under Water - Fog Distance - Far Plane'),
        wbFloat('Water Properties - Distortion Amount'),
        wbFloat('Water Properties - Shininess'),
        wbFloat('Water Properties - Reflection HDR Multiplier'),
        wbFloat('Water Properties - Light Radius'),
        wbFloat('Water Properties - Light Brightness'),
        wbFloat('Noise Properties - Noise Layer One - UV Scale'),
        wbFloat('Noise Properties - Noise Layer Two - UV Scale'),
        wbFloat('Noise Properties - Noise Layer Three - UV Scale'),
        wbFloat('Noise Properties - Noise Layer One - Amplitude Scale'),
        wbFloat('Noise Properties - Noise Layer Two - Amplitude Scale'),
        wbFloat('Noise Properties - Noise Layer Three - Amplitude Scale')
      ], cpNormal, True, nil, 46),
      wbStruct(DATA, 'Visual Data', [
        wbFloat('Unknown'),
        wbFloat('Unknown'),
        wbFloat('Unknown'),
        wbFloat('Unknown'),
        wbFloat('Water Properties - Sun Power'),
        wbFloat('Water Properties - Reflectivity Amount'),
        wbFloat('Water Properties - Fresnel Amount'),
        wbUnused(4),
        wbFloat('Fog Properties - Above Water - Fog Distance - Near Plane'),
        wbFloat('Fog Properties - Above Water - Fog Distance - Far Plane'),
        wbByteColors('Shallow Color'),
        wbByteColors('Deep Color'),
        wbByteColors('Reflection Color'),
        wbUnused(4),
        wbFloat('Rain Simulator - Force'),
        wbFloat('Rain Simulator - Velocity'),
        wbFloat('Rain Simulator - Falloff'),
        wbFloat('Rain Simulator - Dampner'),
        wbFloat('Displacement Simulator - Starting Size'),
        wbFloat('Displacement Simulator - Force'),
        wbFloat('Displacement Simulator - Velocity'),
        wbFloat('Displacement Simulator - Falloff'),
        wbFloat('Displacement Simulator - Dampner'),
        wbFloat('Rain Simulator - Starting Size'),
        wbFloat('Noise Properties - Normals - Noise Scale'),
        wbFloat('Noise Properties - Noise Layer One - Wind Direction'),
        wbFloat('Noise Properties - Noise Layer Two - Wind Direction'),
        wbFloat('Noise Properties - Noise Layer Three - Wind Direction'),
        wbFloat('Noise Properties - Noise Layer One - Wind Speed'),
        wbFloat('Noise Properties - Noise Layer Two - Wind Speed'),
        wbFloat('Noise Properties - Noise Layer Three - Wind Speed'),
        wbFloat('Noise Properties - Normals - Depth Falloff Start'),
        wbFloat('Noise Properties - Normals - Depth Falloff End'),
        wbFloat('Fog Properties - Above Water - Fog Amount'),
        wbFloat('Noise Properties - Normals - UV Scale'),
        wbFloat('Fog Properties - Under Water - Fog Amount'),
        wbFloat('Fog Properties - Under Water - Fog Distance - Near Plane'),
        wbFloat('Fog Properties - Under Water - Fog Distance - Far Plane'),
        wbFloat('Water Properties - Distortion Amount'),
        wbFloat('Water Properties - Shininess'),
        wbFloat('Water Properties - Reflection HDR Multiplier'),
        wbFloat('Water Properties - Light Radius'),
        wbFloat('Water Properties - Light Brightness'),
        wbFloat('Noise Properties - Noise Layer One - UV Scale'),
        wbFloat('Noise Properties - Noise Layer Two - UV Scale'),
        wbFloat('Noise Properties - Noise Layer Three - UV Scale'),
        wbEmpty('Noise Properties - Noise Layer One - Amplitude Scale'),
        wbEmpty('Noise Properties - Noise Layer Two - Amplitude Scale'),
        wbEmpty('Noise Properties - Noise Layer Three - Amplitude Scale'),
        wbInteger('Damage (Old Format)', itU16)
      ], cpNormal, True)
    ], [], cpNormal, True),
    wbStruct(GNAM, 'Related Waters (Unused)', [
      wbFormIDCk('Daytime', [WATR, NULL]),
      wbFormIDCk('Nighttime', [WATR, NULL]),
      wbFormIDCk('Underwater', [WATR, NULL])
    ], cpNormal, True)
  ], False, nil, cpNormal, False, wbWATRAfterLoad);

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
FO3:
  wbRecord(WEAP, 'Weapon',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      27, 'Unknown 27',
      29, 'Unknown 29'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbEnchantment(True),
    wbFormIDCkNoReach(NAM0, 'Ammo', [AMMO, FLST]),
    wbDEST,
    wbREPL,
    wbETYPReq,
    wbBIPL,
    wbYNAM,
    wbZNAM,
    wbRStruct('Shell Casing Model', [
      wbString(MOD2, 'Model FileName'),
      wbModelInfo(MO2T),
      wbMO2S
    ])
    .SetSummaryKey([0])
    .IncludeFlag(dfCollapsed, wbCollapseModels),
    wbRStruct('Scope Model', [
      wbString(MOD3, 'Model FileName'),
      wbModelInfo(MO3T),
      wbMO3S
    ])
    .SetSummaryKey([0])
    .IncludeFlag(dfCollapsed, wbCollapseModels),
    wbFormIDCK(EFSD, 'Scope Effect', [EFSH]),
    wbRStruct('World Model', [
      wbString(MOD4, 'Model FileName'),
      wbModelInfo(MO4T),
      wbMO4S
    ])
    .SetSummaryKey([0])
    .IncludeFlag(dfCollapsed, wbCollapseModels),
    wbString(NNAM, 'Embedded Weapon Node'),
    wbFormIDCk(INAM, 'Impact DataSet', [IPDS]),
    wbFormIDCk(WNAM, '1st Person Model', [STAT]),
    wbFormIDCk(SNAM, 'Sound - Gun - Shoot 3D', [SOUN]),
    wbFormIDCk(XNAM, 'Sound - Gun - Shoot 2D', [SOUN]),
    wbFormIDCk(NAM7, 'Sound - Gun - Shoot 3D Looping', [SOUN]),
    wbFormIDCk(TNAM, 'Sound - Melee - Swing / Gun - No Ammo', [SOUN]),
    wbFormIDCk(NAM6, 'Sound - Block', [SOUN]),
    wbFormIDCk(UNAM, 'Sound - Idle', [SOUN]),
    wbFormIDCk(NAM9, 'Sound - Equip', [SOUN]),
    wbFormIDCk(NAM8, 'Sound - Unequip', [SOUN]),
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbInteger('Health', itS32),
      wbFloat('Weight'),
      wbInteger('Base Damage', itS16),
      wbInteger('Clip Size', itU8)
    ]).SetRequired,
    wbStruct(DNAM, '', [
      {00} wbInteger('Animation Type', itU32, wbWeaponAnimTypeEnum),
      {04} wbFloat('Animation Multiplier'),
      {08} wbFloat('Reach'),
      {12} wbInteger('Flags 1', itU8,
             wbFlags([
               {0} 'Ignores Normal Weapon Resistance',
               {1} 'Automatic',
               {2} 'Has Scope',
               {3} 'Can''t Drop',
               {4} 'Hide Backpack',
               {5} 'Embedded Weapon',
               {6} 'Don''t Use 1st Person IS Animations',
               {7} 'Non-Playable'
             ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {13} wbInteger('Grip Animation', itU8,
             wbEnum([], [
               171, 'HandGrip1',
               172, 'HandGrip2',
               173, 'HandGrip3',
               255, 'DEFAULT'
             ])),
      {14} wbInteger('Ammo Use', itU8),
      {15} wbInteger('Reload Animation', itU8,
             wbEnum([
               {0}  'ReloadA',
               {1}  'ReloadB',
               {2}  'ReloadC',
               {3}  'ReloadD',
               {4}  'ReloadE',
               {5}  'ReloadF',
               {6}  'ReloadG',
               {7}  'ReloadH',
               {8}  'ReloadI',
               {9}  'ReloadJ',
               {10} 'ReloadK'
             ],[
                255, 'None'
             ])),
      {16} wbFloat('Min Spread'),
      {20} wbFloat('Spread'),
      {24} wbUnused(4),
      {28} wbFloat('Sight FOV'),
      {32} wbUnused(4),
      {36} wbFormIDCk('Projectile', [PROJ, NULL]),
      {40} wbInteger('Base VATS To-Hit Chance', itU8),
      {41} wbInteger('Attack Animation', itU8,
             wbEnum([], [
               26,  'AttackLeft',
               32,  'AttackRight',
               38,  'Attack3',
               44,  'Attack4',
               50,  'Attack5',
               56,  'Attack6',
               62,  'Attack7',
               68,  'Attack8',
               74,  'AttackLoop',
               80,  'AttackSpin',
               86,  'AttackSpin2',
               97,  'PlaceMine',
               103, 'PlaceMine2',
               109, 'AttackThrow',
               115, 'AttackThrow2',
               121, 'AttackThrow3',
               127, 'AttackThrow4',
               133, 'AttackThrow5',
               255, 'DEFAULT'
             ])),
      {42} wbInteger('Projectile Count', itU8),
      {43} wbInteger('Embedded Weapon - Actor Value', itU8,
             wbEnum([
               {0} 'Perception',
               {1} 'Endurance',
               {2} 'Left Attack',
               {3} 'Right Attack',
               {4} 'Left Mobility',
               {5} 'Right Mobility',
               {6} 'Brain'
             ])),
      {44} wbFloat('Min Range'),
      {48} wbFloat('Max Range'),
      {52} wbInteger('On Hit', itU32,
             wbEnum([
               {0} 'Normal formula behavior',
               {1} 'Dismember Only',
               {2} 'Explode Only',
               {3} 'No Dismember/Explode'
             ])),
      {56} wbInteger('Flags 2', itU32,
            wbFlags([
              {0}  'Player Only',
              {1}  'NPCs Use Ammo',
              {2}  'No Jam After Reload',
              {3}  'Override - Action Points',
              {4}  'Minor Crime',
              {5}  'Range - Fixed',
              {6}  'Not Used In Normal Combat',
              {7}  'Override - Damage to Weapon Mult',
              {8}  'Don''t Use 3rd Person IS Animations',
              {9}  'Short Burst',
              {10} 'Rumble Alternate',
              {11} 'Long Burst'
            ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {60} wbFloat('Animation Attack Multiplier'),
      {64} wbFloat('Fire Rate'),
      {68} wbFloat('Override - Action Points'),
      {72} wbFloat('Rumble - Left Motor Strength'),
      {76} wbFloat('Rumble - Right Motor Strength'),
      {80} wbFloat('Rumble - Duration'),
      {84} wbFloat('Override - Damage to Weapon Mult'),
      {88} wbFloat('Attack Shots/Sec'),
      {92} wbFloat('Reload Time'),
      {96} wbFloat('Jam Time'),
     {100} wbFloat('Aim Arc'),
     {104} wbInteger('Skill', itS32, wbActorValueEnum),
     {108} wbInteger('Rumble - Pattern', itU32,
             wbEnum([
               {0} 'Constant',
               {1} 'Square',
               {2} 'Triangle',
               {3} 'Sawtooth'
             ])),
     {112} wbFloat('Rumble - Wavelength'),
     {116} wbFloat('Limb Dmg Mult'),
     {120} wbInteger('Resist Type', itS32, wbActorValueEnum),
     {124} wbFloat('Sight Usage'),
     {128} wbFloat('Semi-Automatic Fire Delay Min'),
     {132} wbFloat('Semi-Automatic Fire Delay Max')
    ], cpNormal, True, nil, 36),

   wbStruct(CRDT, 'Critical Data', [
      {00} wbInteger('Critical Damage', itU16),
      {09} wbUnused(2),
      {04} wbFloat('Crit % Mult'),
      {08} wbInteger('On Death', itU8, wbBoolEnum),
      {09} wbUnused(3),
      {12} wbFormIDCk('Effect', [SPEL, NULL])
    ], cpNormal, True),
    wbInteger(VNAM, 'Sound Level', itU32, wbSoundLevelEnum).SetRequired
  ]).SetAfterLoad(wbWEAPAfterLoad);
FNV:
  wbRecord(WEAP, 'Weapon',
    wbFlags(wbFlagsList([
      10, 'Quest Item',
      27, 'Unknown 27',
      29, 'Unknown 29'
    ])), [
    wbEDIDReq,
    wbOBND(True),
    wbFULL,
    wbGenericModel,
    wbICON,
    wbSCRI,
    wbEnchantment(True),
    wbFormIDCkNoReach(NAM0, 'Ammo', [AMMO, FLST]),
    wbDEST,
    wbREPL,
    wbETYPReq,
    wbBIPL,
    wbYNAM,
    wbZNAM,
    wbTexturedModel('Shell Casing Model', [MOD2, MO2T], [wbMO2S, nil]),
    wbTexturedModel('Scope Model', [MOD3, MO3T], [wbMO3S, nil]),
    wbFormIDCK(EFSD, 'Scope Effect', [EFSH]),
    wbTexturedModel('World Model', [MOD4, MO4T], [wbMO4S, nil]),
    wbString(MWD1, 'Model - Mod 1'),
    wbString(MWD2, 'Model - Mod 2'),
    wbString(MWD3, 'Model - Mod 1 and 2'),
    wbString(MWD4, 'Model - Mod 3'),
    wbString(MWD5, 'Model - Mod 1 and 3'),
    wbString(MWD6, 'Model - Mod 2 and 3'),
    wbString(MWD7, 'Model - Mod 1, 2 and 3'),
    {wbRStruct( 'Model with Mods', [
      wbString(MWD1, 'Mod 1'),
      wbString(MWD2, 'Mod 2'),
      wbString(MWD3, 'Mod 1 and 2'),
      wbString(MWD4, 'Mod 3'),
      wbString(MWD5, 'Mod 1 and 3'),
      wbString(MWD6, 'Mod 2 and 3'),
      wbString(MWD7, 'Mod 1, 2 and 3')
    ], [], cpNormal, False, nil, True),}

    wbString(VANM, 'VATS Attack Name', 0, cpTranslate),
    wbString(NNAM, 'Embedded Weapon Node'),

    wbFormIDCk(INAM, 'Impact DataSet', [IPDS]),
    wbFormIDCk(WNAM, '1st Person Model', [STAT]),
    wbFormIDCk(WNM1, '1st Person Model - Mod 1', [STAT]),
    wbFormIDCk(WNM2, '1st Person Model - Mod 2', [STAT]),
    wbFormIDCk(WNM3, '1st Person Model - Mod 1 and 2', [STAT]),
    wbFormIDCk(WNM4, '1st Person Model - Mod 3', [STAT]),
    wbFormIDCk(WNM5, '1st Person Model - Mod 1 and 3', [STAT]),
    wbFormIDCk(WNM6, '1st Person Model - Mod 2 and 3', [STAT]),
    wbFormIDCk(WNM7, '1st Person Model - Mod 1, 2 and 3', [STAT]),
    {wbRStruct('1st Person Models with Mods', [
      wbFormIDCk(WNM1, 'Mod 1', [STAT]),
      wbFormIDCk(WNM2, 'Mod 2', [STAT]),
      wbFormIDCk(WNM3, 'Mod 1 and 2', [STAT]),
      wbFormIDCk(WNM4, 'Mod 3', [STAT]),
      wbFormIDCk(WNM5, 'Mod 1 and 3', [STAT]),
      wbFormIDCk(WNM6, 'Mod 2 and 3', [STAT]),
      wbFormIDCk(WNM7, 'Mod 1, 2 and 3', [STAT])
    ], [], cpNormal, False, nil, True),}
    wbFormIDCk(WMI1, 'Weapon Mod 1', [IMOD]),
    wbFormIDCk(WMI2, 'Weapon Mod 2', [IMOD]),
    wbFormIDCk(WMI3, 'Weapon Mod 3', [IMOD]),
    {wbRStruct('Weapon Mods', [
      wbFormIDCk(WMI1, 'Mod 1', [IMOD]),
      wbFormIDCk(WMI2, 'Mod 2', [IMOD]),
      wbFormIDCk(WMI3, 'Mod 3', [IMOD])
    ], [], cpNormal, False, nil, True),}
    wbRStruct('Sound - Gun', [
      wbFormIDCk(SNAM, 'Shoot 3D', [SOUN]),
      wbFormIDCk(SNAM, 'Shoot Dist', [SOUN])
    ]),
    //wbFormIDCk(SNAM, 'Sound - Gun - Shoot 3D', [SOUN]),
    //wbFormIDCk(SNAM, 'Sound - Gun - Shoot Dist', [SOUN]),
    wbFormIDCk(XNAM, 'Sound - Gun - Shoot 2D', [SOUN]),
    wbFormIDCk(NAM7, 'Sound - Gun - Shoot 3D Looping', [SOUN]),
    wbFormIDCk(TNAM, 'Sound - Melee - Swing / Gun - No Ammo', [SOUN]),
    wbFormIDCk(NAM6, 'Sound - Block', [SOUN]),
    wbFormIDCk(UNAM, 'Sound - Idle', [SOUN]),
    wbFormIDCk(NAM9, 'Sound - Equip', [SOUN]),
    wbFormIDCk(NAM8, 'Sound - Unequip', [SOUN]),
    wbRStruct('Sound - Mod 1', [
      wbFormIDCk(WMS1, 'Shoot 3D', [SOUN]),
      wbFormIDCk(WMS1, 'Shoot Dist', [SOUN])
    ]),
    //wbFormIDCk(WMS1, 'Sound - Mod 1 - Shoot 3D', [SOUN]),
    //wbFormIDCk(WMS1, 'Sound - Mod 1 - Shoot Dist', [SOUN]),
    wbFormIDCk(WMS2, 'Sound - Mod 1 - Shoot 2D', [SOUN]),
    wbStruct(DATA, '', [
      wbInteger('Value', itS32),
      wbInteger('Health', itS32),
      wbFloat('Weight'),
      wbInteger('Base Damage', itS16),
      wbInteger('Clip Size', itU8)
    ], cpNormal, True),
    wbStruct(DNAM, '', [
      {00} wbInteger('Animation Type', itU32, wbWeaponAnimTypeEnum),
      {04} wbFloat('Animation Multiplier'),
      {08} wbFloat('Reach'),
      {12} wbInteger('Flags 1', itU8, wbFlags([
        'Ignores Normal Weapon Resistance',
        'Is Automatic',
        'Has Scope',
        'Can''t Drop',
        'Hide Backpack',
        'Embedded Weapon',
        'Don''t Use 1st Person IS Animations',
        'Non-Playable'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {13} wbInteger('Grip Animation', itU8, wbEnum([
      ], [
        172, 'Unknown 172',
        230, 'HandGrip1',
        231, 'HandGrip2',
        232, 'HandGrip3',
        233, 'HandGrip4',
        234, 'HandGrip5',
        235, 'HandGrip6',
        255, 'DEFAULT'
      ])),
      {14} wbInteger('Ammo Use', itU8),
      {15} wbInteger('Reload Animation', itU8, wbReloadAnimEnum),
      {16} wbFloat('Min Spread'),
      {20} wbFloat('Spread'),
      {24} wbUnused(4),
      {28} wbFloat('Sight FOV'),
      {32} wbUnused(4),
      {36} wbFormIDCk('Projectile', [PROJ, NULL]),
      {40} wbInteger('Base VATS To-Hit Chance', itU8),
      {41} wbInteger('Attack Animation', itU8, wbEnum([
           ], [
             26, 'AttackLeft',
             32, 'AttackRight',
             38, 'Attack3',
             44, 'Attack4',
             50, 'Attack5',
             56, 'Attack6',
             62, 'Attack7',
             68, 'Attack8',
             74, 'AttackLoop',
             80, 'AttackSpin',
             86, 'AttackSpin2',
            102, 'PlaceMine',
            108, 'PlaceMine2',
            114, 'AttackThrow',
            120, 'AttackThrow2',
            126, 'AttackThrow3',
            132, 'AttackThrow4',
            138, 'AttackThrow5',
            144, 'Attack9',
            150, 'AttackThrow6',
            156, 'AttackThrow7',
            162, 'AttackThrow8',
            255, 'DEFAULT'
           ])),
      {42} wbInteger('Projectile Count', itU8),
      {43} wbInteger('Embedded Weapon - Actor Value', itU8, wbEnum([
        {00} 'Perception',
        {01} 'Endurance',
        {02} 'Left Attack',
        {03} 'Right Attack',
        {04} 'Left Mobility',
        {05} 'Right Mobility',
        {06} 'Brain'
      ])),
      {44} wbFloat('Min Range'),
      {48} wbFloat('Max Range'),
      {52} wbInteger('On Hit', itU32, wbEnum([
        'Normal formula behavior',
        'Dismember Only',
        'Explode Only',
        'No Dismember/Explode'
      ])),
      {56} wbInteger('Flags 2', itU32, wbFlags([
        {0x00000001}'Player Only',
        {0x00000002}'NPCs Use Ammo',
        {0x00000004}'No Jam After Reload',
        {0x00000008}'Override - Action Points',
        {0x00000010}'Minor Crime',
        {0x00000020}'Range - Fixed',
        {0x00000040}'Not Used In Normal Combat',
        {0x00000080}'Override - Damage to Weapon Mult',
        {0x00000100}'Don''t Use 3rd Person IS Animations',
        {0x00000200}'Short Burst',
        {0x00000400}'Rumble Alternate',
        {0x00000800}'Long Burst',
        {0x00001000}'Scope has NightVision',
        {0x00002000}'Scope from Mod'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {60} wbFloat('Animation Attack Multiplier'),
      {64} wbFloat('Fire Rate'),
      {68} wbFloat('Override - Action Points'),
      {72} wbFloat('Rumble - Left Motor Strength'),
      {76} wbFloat('Rumble - Right Motor Strength'),
      {80} wbFloat('Rumble - Duration'),
      {84} wbFloat('Override - Damage to Weapon Mult'),
      {88} wbFloat('Attack Shots/Sec'),
      {92} wbFloat('Reload Time'),
      {96} wbFloat('Jam Time'),
     {100} wbFloat('Aim Arc'),
     {104} wbInteger('Skill', itS32, wbActorValueEnum),
     {108} wbInteger('Rumble - Pattern', itU32, wbEnum([
       'Constant',
       'Square',
       'Triangle',
       'Sawtooth'
     ])),
     {112} wbFloat('Rumble - Wavelength'),
     {116} wbFloat('Limb Dmg Mult'),
     {120} wbInteger('Resist Type', itS32, wbActorValueEnum),
     {124} wbFloat('Sight Usage'),
     {128} wbFloat('Semi-Automatic Fire Delay Min'),
     {132} wbFloat('Semi-Automatic Fire Delay Max'),
     wbFloat,
     wbInteger('Effect - Mod 1', itU32, wbModEffectEnum),
     wbInteger('Effect - Mod 2', itU32, wbModEffectEnum),
     wbInteger('Effect - Mod 3', itU32, wbModEffectEnum),
     wbFloat('Value A - Mod 1'),
     wbFloat('Value A - Mod 2'),
     wbFloat('Value A - Mod 3'),
     wbInteger('Power Attack Animation Override', itU32, wbEnum([
     ], [
        0, '0?',
       97, 'AttackCustom1Power',
       98, 'AttackCustom2Power',
       99, 'AttackCustom3Power',
      100, 'AttackCustom4Power',
      101, 'AttackCustom5Power',
      255, 'DEFAULT'
     ])),
     wbInteger('Strength Req', itU32),
     wbByteArray('Unknown', 1),
     wbInteger('Reload Animation - Mod', itU8, wbReloadAnimEnum),
     wbByteArray('Unknown', 2),
     wbFloat('Regen Rate'),
     wbFloat('Kill Impulse'),
     wbFloat('Value B - Mod 1'),
     wbFloat('Value B - Mod 2'),
     wbFloat('Value B - Mod 3'),
     wbFloat('Impulse Dist'),
     wbInteger('Skill Req', itU32)
    ], cpNormal, True, nil, 36),

   wbStruct(CRDT, 'Critical Data', [
      {00} wbInteger('Critical Damage', itU16),
      {09} wbUnused(2),
      {04} wbFloat('Crit % Mult'),
      {08} wbInteger('Flags', itU8, wbFlags([
        'On Death'
      ])).IncludeFlag(dfCollapsed, wbCollapseFlags),
      {09} wbUnused(3),
      {12} wbFormIDCk('Effect', [SPEL, NULL])
    ], cpNormal, True),
    wbStruct(VATS, 'VATS', [
      wbFormIDCk('Effect', [SPEL, NULL]),
      wbFloat('Skill'),
      wbFloat('Dam. Mult'),
      wbFloat('AP'),
      wbInteger('Silent', itU8, wbBoolEnum),
      wbInteger('Mod Required', itU8, wbBoolEnum),
      wbUnused(2)
    ], cpNormal, False, nil, 4),
    wbInteger(VNAM, 'Sound Level', itU32, wbSoundLevelEnum, cpNormal, True)
  ], True, nil, cpNormal, False, wbWEAPAfterLoad);

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
FO3:
  wbRecord(WRLD, 'Worldspace',
    wbFlags(wbFlagsList([
      19, 'Can''t Wait'
    ])), [
    wbEDIDReq,
    wbFULL,
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),
    wbRStruct('Parent Worldspace', [
      wbFormIDCk(WNAM, 'World', [WRLD]),
      wbInteger(PNAM, 'Flags', itU16,
        wbFlags([
          {0} 'Use Land Data',
          {1} 'Use LOD Data',
          {2} 'Use Map Data',
          {3} 'Use Water Data',
          {4} 'Use Climate Data',
          {5} 'Use Image Space Data'
        ], True)
      ).SetRequired
       .IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]),
    wbFormIDCk(CNAM, 'Climate', [CLMT])
      .SetDefaultNativeValue(351)
      .SetIsRemovable(wbWorldClimateIsRemovable),
    wbFormIDCk(NAM2, 'Water', [WATR])
      .SetDefaultNativeValue(24)
      .SetIsRemovable(wbWorldWaterIsRemovable),
    wbWorldLODData,
    wbWorldLandData,
    wbICON,
    wbWorldMapData,
    wbWorldMapOffset,
    wbFormIDCk(INAM, 'Image Space', [IMGS]).SetDefaultNativeValue(353),
    wbInteger(DATA, 'Flags', itU8,
      wbFlags(wbSparseFlags([
        0, 'Small World',
        1, 'Can''t Fast Travel',
        4, 'No LOD Water',
        5, 'No LOD Noise',
        6, 'Don''t Allow NPC Fall Damage',
        7, 'Needs Water Adjustment'
      ], False, 8), True)
    ).SetDefaultNativeValue(1)
     .SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbWorldObjectBounds,
    wbFormIDCk(ZNAM, 'Music', [MUSC]),
    wbString(NNAM, 'Canopy Shadow').SetRequired,
    wbString(XNAM, 'Water Noise Texture').SetRequired,
    wbWorldSwapsImpactData,
    wbWorldOffsetData
  ]).SetAfterLoad(wbWorldAfterLoad)
    .SetAfterSet(wbWorldAfterSet);
FNV:
  wbRecord(WRLD, 'Worldspace',
    wbFlags(wbFlagsList([
      19, 'Can''t Wait'
    ])), [
    wbEDIDReq,
    wbFULL,
    wbFormIDCk(XEZN, 'Encounter Zone', [ECZN]),
    wbRStruct('Parent Worldspace', [
      wbFormIDCk(WNAM, 'World', [WRLD]),
      wbInteger(PNAM, 'Flags', itU16,
        wbFlags([
          {0} 'Use Land Data',
          {1} 'Use LOD Data',
          {2} 'Use Map Data',
          {3} 'Use Water Data',
          {4} 'Use Climate Data',
          {5} 'Use Image Space Data'
        ], True)
      ).SetRequired
       .IncludeFlag(dfCollapsed, wbCollapseFlags)
    ]),
    wbFormIDCk(CNAM, 'Climate', [CLMT])
      .SetDefaultNativeValue(351)
      .SetIsRemovable(wbWorldClimateIsRemovable),
    wbFormIDCk(NAM2, 'Water', [WATR])
      .SetDefaultNativeValue(24)
      .SetIsRemovable(wbWorldWaterIsRemovable),
    wbWorldLODData,
    wbWorldLandData,
    wbICON,
    wbWorldMapData,
    wbWorldMapOffset,
    wbFormIDCk(INAM, 'Image Space', [IMGS])
      .SetDefaultNativeValue(353),
    wbInteger(DATA, 'Flags', itU8,
      wbFlags(wbSparseFlags([
        0, 'Small World',
        1, 'Can''t Fast Travel',
        4, 'No LOD Water',
        5, 'No LOD Noise',
        6, 'Don''t Allow NPC Fall Damage',
        7, 'Needs Water Adjustment'
      ], False, 8), True)
    ).SetDefaultNativeValue(1)
     .SetRequired
     .IncludeFlag(dfCollapsed, wbCollapseFlags),
    wbWorldObjectBounds,
    wbFormIDCk(ZNAM, 'Music', [MUSC]),
    wbString(NNAM, 'Canopy Shadow')
      .SetRequired,
    wbString(XNAM, 'Water Noise Texture')
      .SetRequired,
    wbWorldSwapsImpactData,
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
FO3:
  wbRecord(WTHR, 'Weather', [
    wbEDIDReq,
    wbFormIDCk(_00_IAD, 'Sunrise', [IMAD]),
    wbFormIDCk(_01_IAD, 'Day', [IMAD]),
    wbFormIDCk(_02_IAD, 'Sunset', [IMAD]),
    wbFormIDCk(_03_IAD, 'Night', [IMAD]),
    wbWeatherCloudTextures,
    wbRStruct('Precipitation', [
      wbGenericModel
    ]),
    wbInteger(LNAM, 'Max Cloud Layers', itU32)
      .SetDefaultNativeValue(4)
      .SetRequired,
    wbWeatherCloudSpeed,
    wbWeatherCloudColors,
    wbWeatherColors,
    wbWeatherFogDistance,
    wbUnused(INAM, 304).SetRequired,
    wbStruct(DATA, 'Data', [
      wbInteger('Wind Speed', itU8),
      wbUnused(2),
      wbInteger('Trans Delta', itU8),
      wbInteger('Sun Glare', itU8),
      wbInteger('Sun Damage', itU8),
      wbInteger('Precipitation - Begin Fade In', itU8),
      wbInteger('Precipitation - End Fade Out', itU8),
      wbInteger('Thunder/Lightning - Begin Fade In', itU8),
      wbInteger('Thunder/Lightning - End Fade Out', itU8),
      wbInteger('Thunder/Lightning - Frequency', itU8),
      wbInteger('Flags', itU8,
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
  ]);
  FNV:
    wbRecord(WTHR, 'Weather', [
    wbEDIDReq,
    wbFormIDCk(_00_IAD, 'Sunrise Image Space Adapter', [IMAD]),
    wbFormIDCk(_01_IAD, 'Day Image Space Adapter', [IMAD]),
    wbFormIDCk(_02_IAD, 'Sunset Image Space Adapter', [IMAD]),
    wbFormIDCk(_03_IAD, 'Night Image Space Adapter', [IMAD]),
    wbFormIDCk(_04_IAD, 'High Noon Image Space Adapter', [IMAD]),
    wbFormIDCk(_05_IAD, 'Midnight Image Space Adapter', [IMAD]),
    wbWeatherCloudTextures,
    wbRStruct('Precipitation', [
      wbGenericModel
    ]),
    wbInteger(LNAM, 'Max Cloud Layers', itU32)
      .SetDefaultNativeValue(4)
      .SetRequired,
    wbWeatherCloudSpeed,
    wbWeatherCloudColors,
    wbWeatherColors,
    wbWeatherFogDistance,
    wbUnused(INAM, 304, True),
    wbStruct(DATA, 'Data', [
      wbInteger('Wind Speed', itU8),
      wbUnused(2),
      wbInteger('Trans Delta', itU8),
      wbInteger('Sun Glare', itU8),
      wbInteger('Sun Damage', itU8),
      wbInteger('Precipitation - Begin Fade In', itU8),
      wbInteger('Precipitation - End Fade Out', itU8),
      wbInteger('Thunder/Lightning - Begin Fade In', itU8),
      wbInteger('Thunder/Lightning - End Fade Out', itU8),
      wbInteger('Thunder/Lightning - Frequency', itU8),
      wbInteger('Flags', itU8,
        wbFlags(wbSparseFlags([
          0, 'Weather - Pleasant',
          1, 'Weather - Cloudy',
          2, 'Weather - Rainy',
          3, 'Weather - Snow'
        ], False, 4), True)
      ).IncludeFlag(dfCollapsed, wbCollapseFlags),
      wbWeatherLightningColor
    ]).SetRequired,
    wbWeatherSounds
  ]);