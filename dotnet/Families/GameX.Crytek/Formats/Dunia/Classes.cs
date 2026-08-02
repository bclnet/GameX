using System.Numerics;

namespace GameX.Crytek.Formats.Dunia;

#region Pre
// Base classes referenced via 'extends' that are not themselves defined in the XML

public class CAIEvent { }
public class CBaseEntity { }
public class CBaseFact { }
public class CClientInfo { }
public class CCommandCBParam { }
public class CEntity { }
public class CGOStateEvent { }
public class CGRQueryParams { }
public class CGRState { }
public class CGameFile { }
public class CGameFileHeader { }
public class CGameMessageParser { }
public class CGameMode { }
public class CGameOperation { }
public class CGameOperationBuilder { }
public class CGameSetting { }
public class CInputDriver { }
public class CNetDataContainer { }
public class CNetGameCtrlState { }
public class CNetworkSetting { }
public class CNomadObject { }
public class COmniMapEntity { }
public class COperation { }
public class COperationData { }
public class CResource { }
public class CSessionInfo { }
public class CSpawnPoint { }
public class CSpawnPointBlue { }
public class CSpawnPointRed { }
public class CUIPageBase { }
public class CUISettingBase { }
public class IFile { }
public class IGOStateContext { }
public class INetEvent { }
public class IOperation { }
public class IPlayer { }

/// <summary>
/// x
/// </summary>

public class IAuthorizationService : IGameModeService { }
public class ICountersService : IGameModeService { }
public class IGameModeService : CNomadObject { }
public class IGameMessageService : IGameModeService { }
public class IGameSoundService : IGameModeService { }
public class IGameStatsService : IGameModeService { }
public class IHostAdminService : IGameModeService { }
public class IMagmaDebugTextService : IGameModeService { }
public class IPlayerService : IGameModeService { }
public class IShapeEntity : COmniMapEntity { }
public class IShapeComponent : CEntityComponent { }
public class ISpawnPointService : IGameModeService { }


public class CPlan : CAction { }
public class CPersonality : CNomadObject { }
public class CPhysComponent : CEntityComponent {
    public string X_527E7674;
    public uint hidResourceId;
}
public class CPhysNetworkComponent : CNetworkComponent { }
public class CPickupNetworkComponent : CPhysNetworkComponent { }
public class CPlayer : IPlayer { }
public class CPlayerService : IPlayerService { }

public class CResourceContainer : CResource { }
public class CRenderBaseConfig : CNomadConfigObject { }
public class CRenderableComponent : CEntityComponent { }
public class CRandomPathFollower : CPathFollower { }
public class CResourceNotifier : CResourceContainer { }
public class CRendezVousOperation : CSessionOperation { }

public class CScanner : CTask { }
public class CScriptEvent : CEntityEvent { }
public class CScoreboardService : IGameModeService { }
public class CSectorSpawnCategory : CResourceNotifier { }
public class CSessionOperation : COperation { }
public class CSettingsPage : CListMenuPage { }
public class CSingletonEntity : COmniEntity { }
public class CSmartTerrain : CGameAIObject { }
public class CSoundEvent : CEntityEvent { }
public class CSpawnPointService : ISpawnPointService { }

public class CTaskRoot : CNomadObject { }
public class CTask : CTaskRoot { }
public class CTDMSpawnPointService : CDMSpawnPointService { }
public class CTrackingService : IGameModeService { }


public class CValueListSettingbool : CGenericUISettingbool { }
public class CValueListSettingCryString : CGenericUISettingCryString { }
public class CValueListSettingunsigned_long : CGenericUISettingunsigned_long { }

public class CWorldSector : CResource { }
public class CWeapon : CEquipmentBase { }
public class CWeaponsService : IGameModeService { }

#endregion

public class X_256A1FF9 {
    public string Name;
}

public class X_E0BDB3DB {
    public string Name;
}

// 	The following definitions need to be moved to their proper place in native classes.They're here from when there wasn't any subclass handling.

public class WorldSector {
    public uint Id;
    public uint X;
    public uint Y;
}

public class hidEffectBones { }

public class enumCollisionLayer { }

public class Impact {
    public string X_67F06359;
}

public class Effect {
    public string X_D986CE26;
    public uint sEffectName;
}

public class Entity {
    public string hidName;
    public ulong disEntityId;
    public string X_D2B3429E;
    public uint hidEntityClass;
    public uint hidResourceCount;
    public Vector3 hidPos;
    public Vector3 hidAngles;
    public Vector3 hidPos_precise;
    public bool hidConstEntity;
}

public class Components { }

public class enum_ {
    public string Value;
}

public class enumAnimalSpecies { }

public class World {
    public byte[] Objective;
    public byte[] PGP;
    public byte[] SafeHouse;
    public byte[] CellTower;
}

public class Area { }

public class KeyLocation { }

public class StimsToExplode { }

public class _Stim {
    public bool bPierceStim;
    public bool bCrushStim;
    public bool bBurnStim;
    public float fBulletImpulseScale;
    public float fExplosionImpulseScale;
    public uint selType;
    public uint selStimType;
    public string hidEventName;
    public uint eventMask;
    public ulong hidTargetEntityId;
    public string X_FC25E1F1;
    public uint sDetail;
    public uint nLevel;
    public float fRadius;
    public bool bFalloff;
    public uint nFalloffMinLevel;
    public byte[] hidShowType;
    public byte[] hidShowRadius;
    public float fPhysImpulse;
}

public class Stim_ImpactDamage : _Stim { }

public class Stim : _Stim { }

public class enumStimType { }

public class enumType { }

public class Sound {
    public string sndActive;
    public uint sndtpActive;
    public string X_EDFBC3D2;
    public uint matimpExplosionFx;
}

public class DustBlast {
    public string sndLeftSound;
    public string sndRightSound;
    public uint sndtpSoundType;
    public float fDistance;
    public float fSoundDuration;
}

public class ExplosionSound {
    public string sndExplosionSound;
    public uint sndtpExplosionSoundType;
    public string sndLandSoundStart;
    public string sndLandSoundStop;
    public uint sndtpLandSoundType;
}

public class enumDelayExplodeType { }

public class CameraShakeAndRumble { }

public class ExplodeStims { }

public class Stims { }

public class RemainStims { }

public class Particles {
    public string X_6D980293;
    public uint psTrail;
    public string X_38680A74;
    public uint psLand;
    public string X_8DF2AAC6;
    public uint psExplosion;
    public string X_8E6613D3;
    public uint psExplosionUnderwater;
    public string X_BC45A121;
    public uint psRemains;
}

public class Light { }

public class LightExplosion { }

public class LightRemains { }

public class Stages {
    public float fMaxDistance;
    public float fMaxLifeTime;
}

public class Malfunction {
    public float fFireSpeed;
    public float fFireTime;
    public float fGravity;
    public float fMalfunctionInAirProbability;
}

public class enumCategory { }

public class MuzzleStims { }

public class WeaponStims { }

public class ImpactStims { }

public class VictimStims { }

public class _Stage {
    public float fImpulse;
    public float fTime;
    public float fSpeed;
    public float fGravity;
    public float fMinTimeSpinning;
    public float fMaxTimeSpinning;
    public float fTimeStartSpinOnAir;
    public float fForce;
    public float fOnAirTurnSpeed;
    public float fSpinChangeDestTime;
    public Vector3 vectorPropellerStartPoint;
    public Vector3 vectorPropellerEndPoint;
    public uint psStartPS;
    public uint psLoopPS;
    public string sndStartSound;
    public uint sndtpStartSound;
    public string sndLoopSound;
    public uint sndtpLoopSound;
    public string sndLoopEndSound;
    public uint sndtpLoopEndSound;
}

public class Fire : _Stage { }

public class Ignite : _Stage { }

public class Fall : _Stage { }

public class Spin : _Stage { }

public class FireStrategy { }

public class ReliabilityLevelsData { }

public class _ReliabilityLevelData {
    public float fHorizontalRecoilPerShot;
    public float fVerticalRecoilPerShot;
    public float fBulletDeviationMax;
    public float fJamProbabilityPerReload;
}

public class Failure : _ReliabilityLevelData { }

public class Low : _ReliabilityLevelData { }

public class Medium : _ReliabilityLevelData { }

public class High : _ReliabilityLevelData { }
public class enumLevel { }

public class CommonProperties {
    public string sName;
    public string sDisplayName;
    public float fReloadTime;
    public bool bAutoReload;
    public bool bIsSilent;
    public bool bVisibleHolstered;
    public bool bEmitLight;
    public uint selReloadType;
    public uint selWeaponClass;
    public uint selFireStrategy;
    public uint selReticleType;
    public string crosshairMagmaAreaName;
    public uint iBaseAccuracyLevel;
    public float fRange;
    public Vector2 vectorEffectiveRange;
    public Vector2 vectorEffectiveRangeIS;
    public float fUnjamTime;
    public uint selJamType;
    public uint iClipsForSelfDestruct;
    public bool bIsIndestructible;
    public bool bIsBreakable;
    public float fLookSensitivityFactor;
    public float fMoveSpeedFactor;
    public float fForcedReliability;
    public float fInitialJamCounter;
    public string archPickupArchetype;
    public float fShootingAngle;
    public float fShootingIronsightAngle;
    public bool bSingleHitHealthFailure;
    public float fHealthFailureChanceModifier;
    public uint selHitLocation_Torso_Severity;
    public uint selHitLocation_Limb_Severity;
    public uint selCategory;
    public string X_E0FF29E0;
    public uint HolsterHandle;
}
public class enumReloadType { }

public class enumWeaponClass { }

public class enumFireStrategy { }

public class enumReticleType { }

public class FireRate {
    public float fBusyDuration;
    public float iFireRate;
    public uint selFireRateMode;
}

public class enumFireRateMode { }

public class FireStrategyProperties {
    public string X_A58AA772;
    public uint StartBone;
    public float fConsumeAmmoRate;
    public bool bUseAngleSpread;
    public uint iBulletsShot;
    public uint iBurstLength;
    public float fAngleYawBulletSpread;
    public float fAnglePitchBulletSpread;
    public bool bHasMuzzleLight;
    public string X_F8F5F0F8;
    public uint matimpShellImpactFx;
    public string X_EB8DE264;
    public uint matimpBulletImpactFx;
    public string X_74A94828;
    public uint matimpSecondaryBulletImpactFx;
    public string archProjectileArchetype;
    public float fInitialImpulse;
    public float fMalfunctionImpulse;
    public float fMalfunctionDetonateAfterHit;
    public bool bActivateOnLaunch;
    public bool bProjectileBoundOnWeapon;
    public string X_16E19113;
    public uint sShootBone;
    public string sndMalfunctionLoopSound;
    public uint sndtpMalfunctionLoopSound;
    public string sndMalfunctionEndLoopSound;
    public uint sndtpMalfunctionEndLoopSound;
    public string sndMalfunctionLoopTPSound;
    public uint sndtpMalfunctionLoopTPSound;
    public string sndMalfunctionEndTPLoopSound;
    public uint sndtpMalfunctionEndTPLoopSound;
    public bool bRotateBaril;
}

public class Network {
    public string strControllerNetobjectType;
}

public class FuelGauge {
    public string X_F7A0C8D5;
    public uint sNeedleBone;
    public float fNeedleMaxRotationInDegrees;
}

public class FlameMesh {
    public float fSize;
    public float fSplineTension;
    public float fSplineContinuity;
    public float fSplineBias;
    public float fPSSpawnTime;
    public string archSpawnTimeAngularSpeedRatioCurve;
    public float fSegmentLength;
    public float fRestitutionInterpolationDist;
    public float fSizeGrowInterpolationDist;
    public float fSizeShrinkInterpolationDist;
    public float fGravityScalePlayerPitch;
    public float fGravityInterpolationDist;
    public float iRingNVertex;
    public float fRingStartAngle;
    public float fTeselation;
    public float fSpeed;
    public bool bInterpolate;
    public string X_93D2AFB5;
    public uint psParticleSystem;
    public string X_3924E150;
    public uint texTexture;
    public float fTextureFrames;
    public float fTextureChangeTime;
}

public class Sounds {
    public string sndPickupGrabSound_1st;
    public uint sndtpPickupGrabSoundType_1st;
    public string sndPickupGrabSound_3rd;
    public uint sndtpPickupGrabSoundType_3rd;
    public string sndPickupEquipSound_1st;
    public uint sndtpPickupEquipSoundType_1st;
    public string sndPickupEquipSound_3rd;
    public uint sndtpPickupEquipSoundType_3rd;
}

public class SoundsWeapon {
    public string sndPickAmmo;
    public uint sndtpPickAmmoSoundType;
}

// Why isn't this part of CCurve
public class curveCurve {
    public uint hidNumKnots;
    public class Knots {
        public class Knot {
            public Vector4 Value;
            public Vector4 Info;
            public uint Type;
        }
    }
}

// Native classes

public class CAABBPartitionManager : CSingletonEntity { }
public class CAccountService : IGameModeService { }
public class CAction : CTask { }
public class CAddSEFactEvent : CEntityEvent { }

public class CAgent : CAIObject {
    public string X_24B313D8;
    public byte[] Brain;
    public string X_071B548C;
    public uint aiwsBrainWorkspace;

    public class PersonalityComponent {
        public string X_2B928622;
        public uint Type;
    }
}

public class CAgentAction : CAction { }
public class CAgentDecision : CDecision { }
public class CAgentScanner : CScanner { }
public class CAIAlertedNearby : CEntityEvent { }
public class CAIBuilding : CGameAIObject { }

public class CAIComponent : CEntityComponent {
    public string X_2B928622;
    public uint Type;
    public class AIObject {
        public class DensityManagement {
            public bool bNeverDelete;
            public bool bLastToBeDeleted;
        }
    }
}

public class CAIMountedWeapon : CGameAIObject { }

public class CAIObject : CAIObjectRoot {
    // AIObjectID
}

public class CAIObjectRoot : CNomadObject { }
public class CAIOcclusionVolumeComponent : CEntityComponent { }
public class CAIShootMeEvent : CEntityEvent { }
public class CAIShootMeObject : CEntityComponent { }
public class CAISoundAndFXComponent : CPawnSoundAndFXComponent { }
public class CAIToggleNavmeshComponent : CEntityComponent { }
public class CAIWorkspaceResource : CResourceContainer { }
public class CAIWorld : CCollective { }
public class CAlwaysLoaded : CSingletonEntity { }
public class CAmbxComponent : CEntityComponent { }

public class CAnimal : CGameObject {
    public string X_4E784950;
}

public class CAnimalAgent : CGameAgent { }
public class CAnimalBeautifierSelector : CEntityComponent { }
public class CAnimalPersonality : CLivingCreature { }

public class CAnimationComponent : CEntityComponent {
    public string X_F9F2D5F4;
    public uint fileSkeleton;
    public string X_E0AAD6E5;
    public uint fileFacialFile;

    public class MercKitFacialFiles {
        public class Faces {
            public string X_0AF17627;
            public uint sHeadTag;
            public string X_89CE658A;
            public uint fileFacialActor;
        }
    }
}

public class CAnimationPackageResource : CResourceContainer { }
public class CAnimationResource : CResourceContainer { }
public class CAnimFacialEvent : CEntityEvent { }
public class CAnimFacialPoseEvent : CEntityEvent { }
public class CAnimPoseEvent : CEntityEvent { }
public class CAntiPortalConfig : CRenderBaseConfig { }
public class CArchiveFile : IFile { }
public class CArmy : CCollective { }
public class CAuthorizationService : IAuthorizationService { }
public class CBargeDelimiter : CGameAIObject { }
public class CBarkManagerService : IGameModeService { }
public class CBarkResourceContainer : CResourceContainer { }
public class CBaseEvent : CNomadObject { }
public class CBaseGraphicComponent : CRenderableComponent { }
public class CBaseMission : CNomadObject { }
public class CBaseSessionParam : COperationData { }
public class CBaseTriggerComponent : CEntityComponent { }
public class CBasicRegionEntity : CBasicShapeEntity { }
public class CBasicShapeComponent : IShapeComponent { }
public class CBasicShapeEntity : IShapeEntity { }
public class CBazaarComputer : CEntityComponent { }
public class CBeautifierRepository : CEntityComponent { }
public class CBedroll : CEntityComponent { }
public class CBinaryResource : CResource { }
public class CBindingComponent : CEntityComponent { }
public class CBinkResource : CResource { }
public class CBlueprintDecision : CDecision { }
public class CBoidsComponent : CEntityComponent { }
public class CBonusService : IGameModeService { }
public class CBonusServiceMP : CBonusService { }
public class CBoundaryRegion : CBasicRegionEntity { }
public class CBrain : CPlan { }
public class CBrainAnimal : CBrain { }
public class CBrainAnimalAlert : CBrain { }
public class CBrainAnimalIdle : CBrain { }
public class CBrainBlackboardSelector : CBrain { }
public class CBrainBoat : CBrain { }
public class CBrainBuddyBase : CBrain { }
public class CBrainDomino : CBrain { }
public class CBrainDrone : CBrain { }
public class CBrainLayeredPatrol : CBrain { }
public class CBrainMerc : CBrain { }
public class CBrainMercAlert : CBrain { }
public class CBrainMercCombat : CBrain { }
public class CBrainMercDead : CBrain { }
public class CBrainMercIdle : CBrain { }
public class CBrainMercSocial : CBrain { }
public class CBrainMercSocialBehavior : CBrain { }
public class CBrainMercSpecial : CBrain { }
public class CBrainMercThreshold : CBrain { }
public class CBrainMercThresholdHealthRescuer : CBrain { }
public class CBrainMercThresholdHealthVictim : CBrain { }
public class CBrainMercVehicle : CBrain { }
public class CBrainRescueBuddy : CBrain { }
public class CBrainSimple : CBrain { }
public class CBrainSmartTerrain : CBrain { }
public class CBrainSpecialCharacter : CBrain { }
public class CBrainStoopidMerc : CBrain { }
public class CBrainVehicle : CBrain { }
public class CBrainVehicleCombat : CBrain { }
public class CBranchPathFollower : CRandomPathFollower { }
public class CBuddiesManager : IGameModeService { }
public class CBuddyDown : CNomadObject { }
public class CBuddyRescueEvent : CEntityEvent { }
public class CBuildingEvent : CEntityEvent { }
public class CBuildingInfoComponent : CEntityComponent { }
public class CBulletTracerManager : CSingletonEntity { }
public class CBurnableRegion : CBasicShapeEntity { }

public class CCameraBoneComponent : CCameraComponent {
    public string X_920A6E7C;
    public uint Bone;
    public bool Cinematic;
}

public class CCameraComponent : CEntityComponent {
    public float fCameraBlendTime;
    public float fNearDistance;
    public float fFarDistance;
    public float fFOV;

    // FocusEntityID
    // Active
}

public class CCameraEditorComponent : CCameraComponent { }

public class CCameraFreeComponent : CCameraNetworkComponent {
    public float fSpeed;
}

public class CCameraGameComponent : CCameraNetworkComponent { }
public class CCameraGhostComponent : CCameraFreeComponent { }
public class CCameraNetworkComponent : CCameraComponent { }

public class CCameraPawnComponent : CCameraGameComponent {
    public string X_920A6E7C;
    public uint Bone;
    public Vector3 DebugOffset;

    // NoiseFOVEnabled
    // NoiseFOVTimeCount
    // NoiseFOVTarget
    // NoiseFOVCurrent
}

public class CCameraShakeAndPadRumbleComponent : CEntityComponent { }
public class CCameraShakeAndPadRumbleEvent : CEntityEvent { }

public class CCameraSpectatorComponent : CCameraNetworkComponent {
    public float fSpeed;
    public float fFastSpeed;
    public float fMaxHeight;
}

public class CCameraThirdComponent : CCameraNetworkComponent {
    public float fDistance;
}

public class CCampaignGameFile : CGameFile { }
public class CCampaignGameFileHeader : CGameFileHeader { }
public class CCanBeStabbed : CEntityEvent { }
public class CCapturePoint : CGameObject { }
public class CCapturePointNetworkComponent : CNetworkComponent { }
public class CChallenge : CNomadObject { }
public class CChallengeComponent : CEntityComponent { }
public class CChallengeProjectile : CChallenge { }
public class CChallengeWeapon : CChallenge { }

public class CPhysCharacterControllerStanceDimensions {
    public Vector3 vecStandCapsulePointA;
    public Vector3 vecStandCapsulePointB;
    public float fStandCapsuleRadius;
}

public class CPhysCharacterControllerEntityCreateParams {
    public float fMass;
    public bool bUpdateRotation;
    public bool bUseRigidBased;
    public float fMaxSlope;
    public float fMaxTerrainSlope;

    public class StandDimensions : CPhysCharacterControllerStanceDimensions { }
    public class CrouchDimensions : CPhysCharacterControllerStanceDimensions { }
    public class SwimDimensions : CPhysCharacterControllerStanceDimensions { }
}

public class CCharacterPhysComponent : CPhysComponent {
    public float RagdollCollideSpeedLimit;
    public string X_041E4C28;
    public uint LockBone;

    public class CharacterParams : CPhysCharacterControllerEntityCreateParams { }
}

public class CCheckScoutEvent : CBaseEvent { }
public class CClientDescriptor : CNetDescriptor { }
public class CClientDescriptor_Agora : CClientDescriptor { }
public class CClientInfo_Agora : CClientInfo { }
public class CClusterComponent : CRenderableComponent { }
public class CCollectionComponent : CEntityComponent { }
public class ICollectionIgnitorComponent : CEntityComponent { }
public class CCollectionIgnitorComponent : ICollectionIgnitorComponent { }
public class CCollectionManager : CSingletonEntity { }
public class CCollective : CAIObject { }
public class CCompassObjectives : CEntityComponent { }
public class CCompoundPhysChangeStateEvent : CEntityEvent { }
public class CCompoundPhysComponent : CPhysComponent { }
public class CCompoundPhysComponentBreakableNode : CCompoundPhysComponentNode { }
public class CCompoundPhysComponentListNode : CCompoundPhysComponentNode { }
public class CCompoundPhysComponentNode : CNomadObject { }
public class CCompoundPhysComponentSingleBodyNode : CCompoundPhysComponentNode { }
public class CCompoundPhysComponentStateNode : CCompoundPhysComponentNode { }
public class CCompoundPhysDestroyEvent : CEntityEvent { }
public class CCompoundPhysForceStateEvent : CEntityEvent { }
public class CCompoundPhysNetworkComponent : CNetworkComponent { }
public class CCompoundPhysOnDamageEvent : CEntityEvent { }
public class CCompoundPhysOnDamageLastStateEvent : CEntityEvent { }
public class CCompoundPhysOnDamageStateChangeEvent : CEntityEvent { }
public class CCompoundPhysOnDestroyEvent : CEntityEvent { }
public class CCompoundPhysOnEventLastStateEvent : CEntityEvent { }
public class CCompoundPhysOnPartBreakOffEvent : CEntityEvent { }
public class CCompoundPhysOnPostStateChangeEvent : CEntityEvent { }
public class CCompoundPhysOnStateChangeEvent : CEntityEvent { }
public class CCompoundSetDamageableEvent : CGenericEntityEventbool { }
public class CConsoleService : IGameModeService { }
public class CConvoyMission : CGameAIObject { }
public class CCorpseComponent : CEntityComponent { }
public class CCounterEvent : CEntityEvent { }

public class CCountersComponent : CEntityComponent {
    public string archStimEffectTable;
}

public class CCountersComponentGO : CCountersComponent { }
public class CCounterThresholdCrossedEvent : CEntityEvent { }
public class CCounterTriggerComponent : CBaseTriggerComponent { }
public class CCreateGameSessionParam : COperationData { }
public class CCreateMatchMakingServiceOperation : CSessionOperation { }
public class CCreateNetObjectOperation : CNetObjectOperation { }
public class CCreateSessionParam : CBaseSessionParam { }
public class CCreatureSoundAndFXComponent : CEntityComponent { }
public class CCurve : CBaseEntity { }
public class CCurveObj : CNomadObject { }
public class CCustomMapGameFile : CGameFile { }
public class CCustomMaterialComponent : CEntityComponent { }
public class CDataBaseItemManager : CSingletonEntity { }
public class CDayCycleScale : CNomadObject { }
public class CDecision : CTask { }
public class CDecompressedArchiveFile : CMemoryStreamFile { }
public class CDelayTriggerComponent : CBaseTriggerComponent { }
public class CDeleteGameSessionParam : COperationData { }
public class CDeleteNetObjectOperation : CNetObjectOperation { }
public class CDeleteSessionParam : COperationData { }
public class CDemonwareLoginOperation : CLoginOperation { }
public class CDependenciesService : IGameModeService { }
public class CDestroyEvent : CEntityEvent { }
public class CDestructibleBridge : CEntityComponent { }
public class CDialogEvent : CSoundEvent { }
public class CDiamondPickedEvent : CEntityEvent { }
public class CDiamondsManager : CSingletonEntity { }
public class CDisableNavMeshVolumeEvent : CEntityEvent { }
public class CDispatcher : CAgent { }
public class CDispatcherConvoy : CDispatcher { }
public class CDispatcherSocial : CDispatcher { }
public class CDispatcherSquadLieutenant : CDispatcher { }
public class CDispatcherVehicle : CDispatcher { }
public class CDisplayApplyPopPup : CGameMessageBox { }
public class CDlcService : IGameModeService { }
public class CDMSpawnPointService : CSpawnPointService { }
public class CDominoBoxInstance : CResourceContainer { }
public class CDominoBoxResource : CResourceContainer { }

public class CDominoComponent : CEntityComponent {
    public string fileBoxPath;
    public bool hidStartOnLoad;
}

public class CDominoEvent : CEntityEvent { }
public class CDominoManager : CSingletonEntity { }
public class CDominoService : IGameModeService { }
public class CDoor : CEntityComponent { }
public class CDoubleFusionComponent : COnlineAdComponent { }
public class CDynamicDeploadComponent : CEntityComponent { }
public class CDynamicLightComponent : CEntityComponent { }
public class CDynLoadComponent : CEntityComponent { }
public class CEconomyComponent : CEntityComponent { }
public class CEditableEventComponent : CEntityComponent { }
public class CEnableBuddyDown : CEntityEvent { }
public class CEnableBuddyDownSuccess : CEntityEvent { }
public class CEnableNavMeshVolumeEvent : CEntityEvent { }
public class CEndOfGameLogosPage : CMenuPage { }
public class CEndOfGamePage : CMenuPage { }

public class CEntityComponent : CNomadObject {
    public bool hidHasAliasName;
    public string hidComponentClassName;
}

public class CEntityDieEvent : CEntityEvent { }
public class CEntityEvent : CBaseEvent { }
public class CEntityEventAddContainer : CEntityEvent { }
public class CEntityEventBlackboardUpdate : CEntityEvent { }
public class CEntityEventCanContain : CEntityEvent { }
public class CEntityEventGetAggressiveState : CEntityEvent { }
public class CEntityEventIsASpecialCharacter : CEntityEvent { }
public class CEntityEventIsUsable : CEntityEvent { }
public class CEntityEventOnUsed : CEntityEvent { }
public class CEntityEventOnUsing : CEntityEvent { }
public class CEntityEventStims : CEntityEvent { }
public class CEntitySpawner : CEntityComponent { }
public class CEntitySystemService : IGameModeService { }
public class CEntityUsableStateEvent : CEntityEvent { }
public class CEntranceInfoComponent : CEntityComponent { }
public class CEnvironmentAdaptiveBloom : CNomadObject { }
public class CEnvironmentAtmosphericScattering : CNomadObject { }
public class CEnvironmentCloud : CNomadObject { }
public class CEnvironmentDepthOfField : CNomadObject { }
public class CEnvironmentFog : CNomadObject { }
public class CEnvironmentLighting : CNomadObject { }
public class CEnvironmentSky : CNomadObject { }
public class CEnvironmentTransition : CNomadObject { }
public class CEnvironmentWeather : CNomadObject { }
public class CEnvironmentWind : CNomadObject { }
public class CEquipmentBase : CGameObject { }
public class CEquipmentUseStrategy : CNomadObject { }
public class CEventComponent : CEntityComponent { }
public class CEventDriveReportLostOccupant : CAIEvent { }

public class CExplosive : CGameObject {
    public string sUseString;
    public string sCategory;
    public byte[] selDelayExplodeType;
    public float fPenetrateDistance;
    public float fDelayRemoveAfterExplosion;
    public float fDelaySendStimsRemain;
    public bool bApplyRemainStimsOnlyOnce;
    public float fTimerSendRemainStims;
    public float fHealthFailureChanceModifier;
    public byte[] ExplodeSendEvent;
    public bool bShouldExplodeUnderwater;
    public bool bShotJustMissedIsUsed;
    public float fShotJustMissedDistance;
    public byte[] archStickyFireFlame;
}

public class CExplosiveEvent : CEntityEvent { }
public class CExportWorldDependenciesEvent : CEntityEvent { }
public class CFaceActorResource : CResource { }
public class CFaceAnimResource : CResource { }
public class CFactAIObjectId : CBaseFact { }
public class CFactbool : CBaseFact { }
public class CFactCNoCaseStringID : CBaseFact { }
public class CFactCSmartPosition : CBaseFact { }
public class CFactCStringID : CBaseFact { }
public class CFactEAimStrategy : CBaseFact { }
public class CFactEEmotionStrategy : CBaseFact { }
public class CFactEFireRange : CBaseFact { }
public class CFactEFireStrategy : CBaseFact { }
public class CFactEIdleBehavior : CBaseFact { }
public class CFactELookStrategy : CBaseFact { }
public class CFactENeedType : CBaseFact { }
public class CFactEntityId : CBaseFact { }
public class CFactEOccupation : CBaseFact { }
public class CFactEPatrolType : CBaseFact { }
public class CFactESocialBehaviorType : CBaseFact { }
public class CFactESpecialStrategy : CBaseFact { }
public class CFactESpeed : CBaseFact { }
public class CFactfloat : CBaseFact { }
public class CFactndAngle3F : CBaseFact { }
public class CFactndQuat : CBaseFact { }
public class CFactndVec2 : CBaseFact { }
public class CFactndVec3 : CBaseFact { }
public class CFactsigned_int : CBaseFact { }
public class CFactsigned_long : CBaseFact { }
public class CFactunsigned_int : CBaseFact { }
public class CFactunsigned_long_long : CBaseFact { }
public class CFactunsigned_long : CBaseFact { }
public class CFakeWeapon : CEntityComponent { }
public class CFanComponent : CEntityComponent { }

public class CFCXActivatePresenceOperation : CFCXGameOperation { }
public class CFCXAIBehaviorService : IGameModeService { }
public class CFCXAIComponent : CAIComponent { }
public class CFcxAIEventDesertChange : CEntityEvent { }
public class CFCXAIEventMercDied : CEntityEvent { }
public class CFCXAntiCheatService : IGameModeService { }
public class CFCXArbitrationEnd : CFCXGameOperation { }
public class CFCXArbitrationStart : CFCXGameOperation { }
public class CFCXArbitrationStartResult : CFCXGameOperation { }
public class CFCXBarkManagerService : CBarkManagerService { }
public class CFCXBaseOptionPage : CSettingsPage { }
public class CFCXBenchmarkService : IGameModeService { }
public class CFCXBrightnessPage : CMenuPage { }
public class CFCXClassService : IGameModeService { }
public class CFCXClearMessageBoxManager : CFCXGameOperation { }
public class CFCXCompassObjectives : CCompassObjectives { }
public class CFCXConsoleService : CConsoleService { }
public class CFCXControllerOptionPage : CListMenuPage { }
public class CFCXCountersComponent : CCountersComponentGO { }
public class CFCXCountersComponentAI : CFCXCountersComponent { }
public class CFCXCountersComponentAIBuddy : CFCXCountersComponentAI { }
public class CFCXCountersComponentAnimal : CFCXCountersComponent { }
public class CFCXCountersComponentPlayer : CFCXCountersComponent { }
public class CFCXCountersComponentPlayerMP : CFCXCountersComponentPlayer { }
public class CFCXCountersComponentPlayerSP : CFCXCountersComponentPlayer { }
public class CFCXCountersService : ICountersService { }
public class CFCXCreateGameModeOperation : CGameOperationContainer { }
public class CFCXCreateSessionOpCtn : CGameOperationContainer { }
public class CFCXCreateSessionOperation : CFCXGameOperation { }
public class CFCXCustomMapDownloadService : IGameModeService { }
public class CFCXCustomMapService : IGameModeService { }
public class CFCXDeleteGameModeOperation : CFCXGameOperation { }
public class CFCXDeleteSessionOpCtn : CGameOperationContainer { }
public class CFCXDeleteSessionOperation : CFCXGameOperation { }
public class CFCXDifficultyPage : CListMenuPage { }
public class CFCXDLCPage : CListMenuPage { }
public class CFCXDMSpawnPointService : CDMSpawnPointService { }
public class CFCXDownloadCustomMapOperation : CFCXGameOperation { }
public class CFCXDuniaPage : CFCXMoviePage { }
public class CFCXEditorConfigService : IGameModeService { }
public class CFCXEditorGameFilesService : CGameFilesService { }
public class CFCXEditorUiService : CFCXUiService { }
public class CFCXEndSession : CFCXGameOperation { }
public class CFCXEnumerateCustomMapsOperation : CFCXGameOperation { }
public class CFCXESRBRatingPage : CFCXRatingPage { }
public class CFCXExclusiveContentMenuPage : CListMenuPage { }
public class CFCXGameMessageParser_Rank : CGameMessageParser { }
public class CFCXGameMessageParser_RankText : CFCXGameMessageParser_Rank { }
public class CFCXGameMessageParser_RankTitle : CFCXGameMessageParser_Rank { }
public class CFCXGameMessageService : CGameMessageService { }
public class CFCXGameModeChange : CFCXGameOperation { }
public class CFCXGameModeInitNetworkOperation : CFCXGameOperation { }
public class CFCXGameModeParamNode : CGameModeParamNode { }
public class CFCXGameModeShutdownNetworkOperation : CFCXGameOperation { }
public class CFCXGameModeSingle : CGameModeSingle { }
public class CFCXGameOperation : CGameOperation { }
public class CFCXGameplayManager : CGameplayManager { }
public class CFCXGameSettingsService : CGameSettingsService { }
public class CFCXGameSoundService : CGameSoundService { }
public class CFCXGameStartOperation : CFCXGameOperation { }
public class CFCXGameStatsSynchronize : CFCXGameOperation { }
public class CFCXGOBuilderCommon : CGameOperationBuilder { }
public class CFCXGOBuilderBenchmark : CFCXGOBuilderCommon { }
public class CFCXGOBuilderConsole : CGameOperationBuilder { }
public class CFCXGOBuilderEditor : CFCXGOBuilderCommon { }
public class CFCXGOBuilderInGameConsole : CGameOperationBuilder { }
public class CFCXGOBuilderMainMenu : CGameOperationBuilder { }
public class CFCXGOBuilderMultiCreateMatch : CGameOperationBuilder { }
public class CFCXGOBuilderMultiEndMatch : CGameOperationBuilder { }
public class CFCXGOBuilderMultiJoinMatch : CGameOperationBuilder { }
public class CFCXGOBuilderMultiNextRankedMatch : CGameOperationBuilder { }
public class CFCXGOBuilderMultiSetupNextRankedMatch : CGameOperationBuilder { }
public class CFCXGOBuilderMultiStartMatch : CGameOperationBuilder { }
public class CFCXGOBuilderMultiUpdateMatch : CGameOperationBuilder { }
public class CFCXGOBuilderSingle : CFCXGOBuilderCommon { }
public class CFCXGOBuilderSingleLoad : CFCXGOBuilderSingle { }
public class CFCXGOCustomGroupNode : CGOCustomGroupNode { }
public class CFCXGOMainMenuNode : CFCXGOCustomGroupNode { }
public class CFCXGOSetUpdateFlags : CGameOperation { }
public class CFCXGOSingleMatchNode : CFCXGOCustomGroupNode { }
public class CFCXGRStateLoadPlayer : CGRStateLoadPlayer { }
public class CFCXGRStateMain : CGRStateMenu { }
public class CFCXGRStateMultiMenu : CGRStateMenu { }
public class CFCXGRStateSingleInGame : CGRStateSingle { }
public class CFCXGRStateSinglePreGame : CGRStateLoad { }
public class CFCXHudService : CHudService { }
public class CFCXInitializeTerminalsOperation : CFCXGameOperation { }
public class CFCXInitNatTraversal : CFCXGameOperation { }
public class CFCXInteractionUIService : IGameModeService { }
public class CFCXJoinSessionOpCtn : CGameOperationContainer { }
public class CFCXJoinSessionOperation : CFCXGameOperation { }
public class CFCXKeyboardControllerOptionPage : CMenuPage { }
public class CFCXLeaderboardSubmitStats : CFCXGameOperation { }
public class CFCXLoadCustomMapGameFileOperation : CFCXGameOperation { }
public class CFCXLoadGameOperation : CFCXGameOperation { }
public class CFCXLoadGamePage : CLoadGamePage { }
public class CFCXLoadGameStartOperation : CFCXGameOperation { }
public class CFCXLoadMessageBoxPackage : CFCXGameOperation { }
public class CFCXLoadOutService : IGameModeService { }
public class CFCXLoadWorldOp : CFCXGameOperation { }
public class CFCXLoadWorldOperation : CGameOperationContainer { }
public class CFCXLoadWorldSynchOp : CFCXGameOperation { }
public class CFCXLobbyService : CLobbyService { }
public class CFCXLoginOperation : CFCXGameOperation { }
public class CFCXLogoutOperation : CFCXGameOperation { }
public class CFCXMainCreditsPage : CMenuPage { }
public class CFCXMainMenu : CGameMode { }
public class CFCXMainPage : CListMenuPage { }
public class CFCXMapListPopup : CGameMessageBox { }
public class CFCXMapProgressPage : CMenuPage { }
public class CFCXMapService : CMapService { }
public class CFCXMatchService : CMatchService { }
public class CFCXMissionManager : IGameModeService { }
public class CFCXMoviePage : CMenuPage { }
public class CFCXMultiBaseMapRotationPage : CMenuPage { }
public class CFCXMultiCreateHostPage : CFCXMultiMatchOptionsPage { }
public class CFCXMultiCreateMapRotationPage : CFCXMultiBaseMapRotationPage { }
public class CFCXMultiCreateMatchAdvancedOptionsMenuPageOffline : CFCXMultiCreateMatchAdvancedOptionsPageOffline { }
public class CFCXMultiCreateMatchAdvancedOptionsMenuPageRanked : CFCXMultiCreateMatchAdvancedOptionsPageRanked { }
public class CFCXMultiCreateMatchAdvancedOptionsMenuPageUnranked : CFCXMultiCreateMatchAdvancedOptionsPageUnranked { }
public class CFCXMultiCreateMatchAdvancedOptionsPage : CFCXMultiMatchOptionsPage { }
public class CFCXMultiCreateMatchAdvancedOptionsPageOffline : CFCXMultiCreateMatchAdvancedOptionsPageUnranked { }
public class CFCXMultiCreateMatchAdvancedOptionsPageRanked : CFCXMultiCreateMatchAdvancedOptionsPage { }
public class CFCXMultiCreateMatchAdvancedOptionsPageUnranked : CFCXMultiCreateMatchAdvancedOptionsPage { }
public class CFCXMultiCreateMatchPage : CFCXMultiCreateHostPage { }
public class CFCXMultiCreateOfflineProfilePage : CMenuPage { }
public class CFCXMultiCreateOnlineProfilePage : CMenuPage { }
public class CFCXMultiCustomBrowserPage : CFCXMultiMatchBrowserPage { }
public class CFCXMultiCustomCreatePage : CFCXMultiCreateMatchPage { }
public class CFCXMultiCustomPage : CFCXMultiMainMatchBasePage { }
public class CFCXMultiEditOfflineProfilePage : CFCXMultiEditProfilePage { }
public class CFCXMultiEditOnlineProfilePage : CFCXMultiEditProfilePage { }
public class CFCXMultiEditorOnlinePage : CListMenuPage { }
public class CFCXMultiEditProfilePage : CMenuPage { }
public class CFCXMultiLANBrowserPage : CFCXMultiMatchBrowserPage { }
public class CFCXMultiLANCreatePage : CFCXMultiCreateMatchPage { }
public class CFCXMultiLANPage : CListMenuPage { }
public class CFCXMultiLeaderboardPage : CMenuPage { }
public class CFCXMultiLeaderboardTypesPage : CListMenuPage { }
public class CFCXMultiMainMatchBasePage : CListMenuPage { }
public class CFCXMultiMainPage : CListMenuPage { }
public class CFCXMultiMatchBrowserPage : CSettingsPage { }
public class CFCXMultiMatchOptionsPage : CSettingsPage { }
public class CFCXMultiOnlinePrivacyStatementPage : CMenuPage { }
public class CFCXMultiPlayerProfilePage : CMenuPage { }
public class CFCXMultiProfileTypePage : CListMenuPage { }
public class CFCXMultiRankedBrowserPage : CFCXMultiMatchBrowserPage { }
public class CFCXMultiRankedCreatePage : CFCXMultiCreateMatchPage { }
public class CFCXMultiRankedPage : CFCXMultiMainMatchBasePage { }
public class CFCXMultiRegisterOnlineProfilePage : CMenuPage { }
public class CFCXMultiSelectProfilePage : CMenuPage { }
public class CFCXMultiServerCustomPage : CSettingsPage { }
public class CFCXMultiServerDeleteMapInfoListPage : CFCXMultiServerMapInfoPage { }
public class CFCXMultiServerDeleteMapListPage : CFCXMultiServerMapListPage { }
public class CFCXMultiServerDownloadMapInfoListPage : CFCXMultiServerMapInfoPage { }
public class CFCXMultiServerDownloadMapListPage : CFCXMultiServerMapListPage { }
public class CFCXMultiServerMapInfoPage : CMenuPage { }
public class CFCXMultiServerMapListPage : CMenuPage { }
public class CFCXMultiServerOperationProgressPage : CMenuPage { }
public class CFCXMultiServerQuickSearchOptionsPage : CSettingsPage { }
public class CFCXMultiServerUploadMapInfoListPage : CFCXMultiServerMapInfoPage { }
public class CFCXMultiServerUploadMapListPage : CFCXMultiServerMapListPage { }
public class CFCXNetEngineIdleOperation : CFCXGameOperation { }
public class CFCXNetEngineShutdownOperation : CFCXGameOperation { }
public class CFCXNetEngineStartupOperation : CFCXGameOperation { }
public class CFCXNetGameCtrlOnEndMatchSync : CNetGameCtrlStateBaseSynchOp { }
public class CFCXNetGameCtrlOnStartMatchSync : CNetGameCtrlStateBaseSynchOp { }
public class CFCXOnlineMapService : IGameModeService { }
public class CFCXOnLoadWorldOp : CFCXGameOperation { }
public class CFCXOnPostLoadWorldOp : CFCXGameOperation { }
public class CFCXOnPreLoadWorldOp : CFCXGameOperation { }
public class CFCXOptionDisplayPage : CMenuPage { }
public class CFCXOptionGamePage : CFCXBaseOptionPage { }
public class CFCXOptionNetworkPage : CFCXBaseOptionPage { }
public class CFCXOptionPage : CListMenuPage { }
public class CFCXOptionSoundPage : CFCXBaseOptionPage { }
public class CFCXParticleAmbianceComponent : CParticleAmbianceComponent { }
public class CFCXPartnersPage : CFCXMoviePage { }
public class CFCXPauseBuddiesPage : CMenuPage { }
public class CFCXPauseGameStatsPage : CMenuPage { }
public class CFCXPauseJackalFilesPage : CMenuPage { }
public class CFCXPauseLegendPage : CMenuPage { }
public class CFCXPauseMenuPage : CListMenuPage { }
public class CFCXPauseMultiService : IGameModeService { }
public class CFCXPausePartnerFilesPage : CMenuPage { }
public class CFCXPausePlayerStatsPage : CMenuPage { }
public class CFCXPlayer : CPlayer { }
public class CFCXPlayerService : CPlayerService { }
public class CFCXPostGameModeChange : CFCXGameOperation { }
public class CFCXPostLoadWorldOp : CFCXGameOperation { }
public class CFCXPreLoadWorldOp : CFCXGameOperation { }
public class CFCXPrepareLoadingScreenOperation : CFCXGameOperation { }
public class CFCXPrepareRendererOperation : CFCXGameOperation { }
public class CFCXPrepareUnloadWorldOperation : CFCXGameOperation { }
public class CFCXPresentationPage : CFCXMoviePage { }
public class CFCXRankService : IGameModeService { }
public class CFCXRatingPage : CFCXMoviePage { }
public class CFCXRemoveEntityFromListOperation : CGameOperation { }
public class CFCXReputationPage : CMenuPage { }
public class CFCXRetrieveLeaderboardStatsOperation : CFCXGameOperation { }
public class CFCXRunBatchFileOperation : CGameOperation { }
public class CFCXRunEditor : CGameOperationContainer { }
public class CFCXScoreboardService : CScoreboardService { }
public class CFCXScoreboardServiceFFA : CFCXScoreboardService { }
public class CFCXScoreboardServiceTeam : CFCXScoreboardService { }
public class CFCXSearchSessionOperation : CFCXGameOperation { }
public class CFCXServeCustomMapOperation : CFCXGameOperation { }
public class CFCXSingleGameFilesService : CGameFilesService { }
public class CFCXSkipFramesOperation : CFCXGameOperation { }
public class CFCXSplashPage : CMenuPage { }
public class CFcxSpline : CNomadObject { }
public class CFcxSplineCollection : CNomadObject { }
public class CFcxSplineCollectionEntity : COmniMapEntity { }
public class CFCXStartCustomMapOperations : CGameOperationContainer { }
public class CFCXStartEditor : CGameOperationContainer { }
public class CFCXStartNetworkOperation : CFCXGameOperation { }
public class CFCXStartSession : CFCXGameOperation { }
public class CFCXStopCustomMapOperations : CGameOperationContainer { }
public class CFCXStopDownloadCustomMapOperation : CFCXGameOperation { }
public class CFCXStopEditor : CGameOperationContainer { }
public class CFCXStopServeCustomMapOperation : CFCXGameOperation { }
public class CFCXStoryAvatarSelectionPage : CMenuPage { }
public class CFCXStoryModePage : CListMenuPage { }
public class CFCXTDMSpawnPointService : CTDMSpawnPointService { }
public class CFCXTeleportEntityOperation : CFCXGameOperation { }
public class CFCXTrackingService : CTrackingService { }
public class CFCXUbisoftPage : CFCXMoviePage { }
public class CFCXUiService : IGameModeService { }
public class CFCXUnloadCustomMapGameFileOperation : CFCXGameOperation { }
public class CFCXUnloadLoadingScreenOperation : CFCXGameOperation { }
public class CFCXUnloadWorldOperation : CFCXGameOperation { }
public class CFCXWaitDownloadCustomMapOperation : CFCXGameOperation { }
public class CFCXWaitForEmptySessionOperation : CFCXGameOperation { }

public class CFCXWeapon : CWeapon {
    public uint iAnimationValue;
    public uint sndswtpWeaponStatusSoundSwitchType;
    public byte[] WeaponStatusSwitchValues;
    public bool bUseHiResScope;
    public float fHiResLowResScopeSwitchTransitionPoint;
}

public class CFCXWeaponsService : CWeaponsService { }
public class CFCXWorldDemoManager : CSingletonEntity { }
public class CFetchPrivilegesOperation : CRendezVousOperation { }

public class CFileDescriptorComponent : CEntityComponent {
    public string X_2A7BCA49;
    public uint fileName;
    public byte[] SerializationEvent;
    public byte[] hidDescriptor;
}

public class CFireComponent : CEntityComponent { }
public class CFireManager : CSingletonEntity { }
public class CFireNode : CNomadObject { }
public class CFireObjectComponent : CFireComponent { }
public class CFireObjectNode : CFireNode { }
public class CFireRealtreeComponent : CFireComponent { }
public class CFireRealtreeElementComponent : CFireRealtreeComponent { }
public class CFireRealtreeNode : CFireNode { }
public class CFireRegionComponent : CFireComponent { }
public class CFireStickyStreamComponent : CFireComponent { }
public class CFireStickyStreamNetworkComponent : CNetworkComponent { }
public class CFireStickyStreamNode : CFireObjectNode { }
public class CFirstHitEvent : CEntityEvent { }
public class CFlag : CGameObject { }
public class CFlagNetworkComponent : CNetworkComponent { }
public class CFlagStation : CGameObject { }
public class CFlagStationNetworkComponent : CNetworkComponent { }
public class CFlare : CGameObject { }
public class CFlareExplosionEvent : CEntityEvent { }

public class CFrankensteinComponent : CEntityComponent {
    // ScriptEventOverrideID
    // Enable
    // LookatEntityTargetIds, TargetId, TargetId
    public bool bCheatKnees;
}

public class CFrankensteinEvent : CEntityEvent { }
public class CFrankensteinPoseResource : CResourceContainer { }
public class CFriendListService : IGameModeService { }
public class CGadget : CEquipmentBase { }
public class CGadgetEventSetProjectileVelocity : CEntityEvent { }
public class CGadgetMapStrategy : CGadgetUseStrategy { }
public class CGadgetNetworkComponent : CNetworkComponent { }
public class CGadgetUseBinocularsStrategy : CGadgetUseStrategy { }
public class CGadgetUseCompassSingleStrategy : CGadgetUseStrategy { }
public class CGadgetUsePhoneStrategy : CGadgetUseStrategy { }
public class CGadgetUseStrategy : CEquipmentUseStrategy { }
public class CGadgetUseThrowStrategy : CGadgetUseStrategy { }
public class CGadgetUseWatchStrategy : CGadgetUseStrategy { }

public class CGameAgent : CAgent {
    public byte[] FlagField;
    public bool bIsScripted;
    public float fAccelerationsSlow;
    public float fAccelerationsNormal;
    public float fAccelerationsFast;
    public float fDecelerationsSlow;
    public float fDecelerationsNormal;
    public float fDecelerationsFast;
    public float fSpeedsBabyStep;
    public float fSpeedsWalk;
    public float fSpeedsJog;
    public float fSpeedsRun;
    public float fSpeedsSprint;
    public float fVariationBabyStep;
    public float fVariationWalk;
    public float fVariationJog;
    public float fVariationRun;
    public float fVariationSprint;
    public byte[] JustStarted;
    public byte[] Destination;
    public byte[] PathInfos;
    public byte[] PatrolPathFollower;
    public byte[] DensityManagement;
    public bool bNeverDelete;
    public bool bLastToBeDeleted;
}

public class CGameAIObject : CAIObject { }
public class CGameConfig : CNomadConfigObject { }
public class CGameConnectOperation : CSessionOperation { }
public class CGameElementEntity : COmniMapEntity { }
public class CGameFilesListPage : CMenuPage { }
public class CGameFilesService : IGameModeService { }
public class CGameFireConfig : CNomadConfigObject { }
public class CGameMessageBox : CUIPageBase { }
public class CGameMessageBoxCustomPopUpTutorial : CGameMessageBoxPopUpTutorial { }
public class CGameMessageBoxDZMessage : CGameMessageBox { }
public class CGameMessageBoxEditBox : CGameMessageBox { }
public class CGameMessageBoxEvent : CGameMessageBox { }
public class CGameMessageBoxFloatingTutorial : CGameMessageBox { }
public class CGameMessageBoxList : CGameMessageBox { }
public class CGameMessageBoxListSingleButton : CGameMessageBox { }
public class CGameMessageBoxPasswordEditBox : CGameMessageBoxEditBox { }
public class CGameMessageBoxPopUpConfirmation : CGameMessageBox { }
public class CGameMessageBoxPopUpTutorial : CGameMessageBox { }
public class CGameMessageBoxQuickMatchStatus : CGameMessageBoxSpinner { }
public class CGameMessageBoxSpinner : CGameMessageBox { }
public class CGameMessageParser_BonusPlanGranted : CGameMessageParser { }
public class CGameMessageParser_BonusPlanGrantedText : CGameMessageParser_BonusPlanGranted { }
public class CGameMessageParser_BonusPlanGrantedTitle : CGameMessageParser_BonusPlanGranted { }
public class CGameMessageParser_Generic : CGameMessageParser { }
public class CGameMessageParser_JoinGame : CGameMessageParser { }
public class CGameMessageParser_LeftGame : CGameMessageParser { }
public class CGameMessageService : IGameMessageService { }
public class CGameMission : CBaseMission { }
public class CGameModeBaseParamNode : CNomadObject { }
public class CGameModeComponent : CEntityComponent { }
public class CGameModeEntity : COmniEntity { }
public class CGameModeParamNode : CGameModeBaseParamNode { }
public class CGameModeServiceEvent : CNomadObject { }
public class CGameModeServiceNetEngineEvent : CGameModeServiceEvent { }
public class CGameModeSingle : CGameMode { }
public class CGameObject : CEntityComponent { }
public class CGameOperationContainer : CGameOperation { }
public class CGameOperationSimpleBuilder : CGameOperationBuilder { }
public class CGameOpNode : CNomadObject { }
public class CGameOverLoadPage : CLoadGamePage { }
public class CGameOverPage : CListMenuPage { }
public class CGameplayManager : IGameModeService { }
public class CGameRegion : CBasicRegionEntity { }
public class CGameSettingsContainer : CGameSetting { }
public class CGameSettingsService : IGameModeService { }
public class CGameSoundService : IGameSoundService { }
public class CGameStatsService : IGameStatsService { }
public class CGameValueListSettingbool : CValueListSettingbool { }
public class CGameValueListSettingCryString : CValueListSettingCryString { }
public class CGameValueListSettingunsigned_long : CValueListSettingunsigned_long { }
public class CGenericEntityEventbool : CEntityEvent { }
public class CGenericEntityEventCTerminalPTR : CEntityEvent { }
public class CGenericUISettingbool : CUISettingBase { }
public class CGenericUISettingCMapCycle : CUISettingBase { }
public class CGenericUISettingCryString : CUISettingBase { }
public class CGenericUISettingunsigned_long : CUISettingBase { }
public class CGeometryResource : CResourceContainer { }
public class CGhostComponent : CEntityComponent { }
public class CGhostEntity : COmniMapEntity { }
public class CGhostEvent : CBaseEvent { }
public class CGOBuilderNode : CGameOpNode { }
public class CGOCreateMatchNode : CGOCustomGroupNode { }
public class CGOCriticalSectionEnd : CGameOperation { }
public class CGOCriticalSectionStart : CGameOperation { }
public class CGOCustomGroupNode : CGameOpNode { }
public class CGOExternalState : CGOState { }
public class CGOSMBarkEvent : CEntityEvent { }
public class CGOState : CNomadObject { }
public class CGOStateAnim : CGOState { }
public class CGOStateAnimRotation : CGOStateAnim { }
public class CGOStateApproachPosition : CGOStateAnim { }
public class CGOStateBriefing : CGOStateAnim { }
public class CGOStateBriefingReaction : CGOStateAnim { }
public class CGOStateContextCGameObject : IGOStateContext { }
public class CGOStateEquipment : CGOStateAnim { }
public class CGOStateEventAnimal : CGOStateEvent { }
public class CGOStateEventBark : CGOStateEvent { }
public class CGOStateEventBazaarComputer : CGOStateEvent { }
public class CGOStateEventBeautifier : CGOStateEvent { }
public class CGOStateEventBedroll : CGOStateEvent { }
public class CGOStateEventBuddyDown : CGOStateEvent { }
public class CGOStateEventCamera : CGOStateEvent { }
public class CGOStateEventCapturePoint : CGOStateEvent { }
public class CGOStateEventEquipment : CGOStateEvent { }
public class CGOStateEventFCXPawn : CGOStateEvent { }
public class CGOStateEventFCXUseEquipment : CGOStateEvent { }
public class CGOStateEventGameRules : CGOStateEvent { }
public class CGOStateEventHeal : CGOStateEvent { }
public class CGOStateEventInput : CGOStateEvent { }
public class CGOStateEventInventory : CGOStateEvent { }
public class CGOStateEventMovie : CGOStateEvent { }
public class CGOStateEventPawn : CGOStateEvent { }
public class CGOStateEventPickupDiamond : CGOStateEvent { }
public class CGOStateEventRescue : CGOStateEvent { }
public class CGOStateEventSM : CGOStateEvent { }
public class CGOStateEventSound : CGOStateEvent { }
public class CGOStateEventTakeFlag : CGOStateEvent { }
public class CGOStateEventVehicle : CGOStateEvent { }
public class CGOStateExitVehicle : CGOStateAnim { }
public class CGOStateGameSetting : CGOState { }
public class CGOStateLadderTransition : CGOStateAnim { }
public class CGOStateMachineTrack : CNomadObject { }
public class CGOStateSmartTerrain : CGOStateAnim { }
public class CGradientColor : CNomadObject { }
public class CGRAmmoPilesRespawn : CGRQueryParams { }
public class CGRAmmoPilesSpawnProjectiles : CGRQueryParams { }
public class CGraphicClusterComponent : CClusterComponent { }

public class CGraphicComponent : CBaseGraphicComponent {
    public bool bCastShadow;
    public bool bReceiveShadow;
    public bool bCastAmbientShadow;
    public uint olgLightGroup;
    public bool bAllowCullBySize;
    public uint agAmbientGroup;
    public bool bBehaveLikeAPickup;
    public bool bShowInReflection;
    public bool bAlwaysShowInReflection;
    public bool bOverrideLODSphere;
    public float fLODSphereRadius;
    public uint hidSkyOcclusion0;
    public uint hidSkyOcclusion1;
    public uint hidSkyOcclusion2;
    public uint hidSkyOcclusion3;
    public uint hidGroundColor;
    public float hidObjectHeight;
    public byte[] hidHeightAbove;
    public bool hidHasAmbientValues;

    public class object_ {
        public uint hidIndex;
        public string X_BF9B3A5C;
        public uint objModel;
        public string hidMeshName;
        public string X_E1A0EE56;
        public uint hidNodeName;
        public string X_0D9C8B1A;
        public uint hidNodeNameLOD0;
        public bool hidDetailObject;
    }
}

public class SPartOverwrite {
    public string X_CE56B704;
    public uint PartID;
    public uint TextureIndex;
    public uint ColorIndex;
}

public class CGraphicKitComponent : CEntityComponent {
    public bool bRadomize;
    public class Tags {
        public class SpecializationTag {
            public string X_9B35862A;
            public uint sTag;
        }
        public class PartOverwrite {
            public class ActivePartOverwrite : SPartOverwrite { }
        }
    }
}

public class CGrassDisplacementComponent : CEntityComponent { }
public class CGRCanBackstab : CGRQueryParams { }
public class CGRCanRemoveInventoryEntities : CGRQueryParams { }
public class CGRCanStab : CGRQueryParams { }
public class CGRDropInventoryWhenRagdoll : CGRQueryParams { }
public class CGREvent : CCommandCBParam { }
public class CGREventEndGameStatsReceived : CGREvent { }
public class CGREventOnEntityReady : CGREvent { }
public class CGREventOnPawnReady : CGREvent { }
public class CGRGenericEvent : CGREvent { }
public class CGRGenericEventWithParamEntityId : CGRGenericEvent { }
public class CGRGenericEventWithParamPlayerId : CGRGenericEvent { }
public class CGRQueryCanDoDamage : CGRQueryParams { }
public class CGRQueryCanJamEquipment : CGRQueryParams { }
public class CGRQueryCanModifyHealth : CGRQueryParams { }
public class CGRQueryCanRevive : CGRQueryParams { }
public class CGRQueryCanVoiceChat : CGRQueryParams { }
public class CGRQueryDisplayAccountErrors : CGRQueryParams { }
public class CGRQueryDisplayConfirmDestructiveAction : CGRQueryParams { }
public class CGRQueryGetMenuContext : CGRQueryParams { }
public class CGRQueryGetStateTimeLeft : CGRQueryParams { }
public class CGRQueryInMultiMenu : CGRQueryParams { }
public class CGRQueryIsGameInLobby : CGRQueryParams { }
public class CGRQueryIsGameInMainMenu : CGRQueryParams { }
public class CGRQueryIsGameInPreRound : CGRQueryParams { }
public class CGRQueryIsGameInProgress : CGRQueryParams { }
public class CGRQueryJoinAsSpectator : CGRQueryParams { }
public class CGRQuerySystemPresence : CGRQueryParams { }
public class CGRStateIdle : CGRState { }
public class CGRStateLoad : CGRState { }
public class CGRStateLoadPlayer : CGRStateLoad { }
public class CGRStateMenu : CGRState { }
public class CGRStateSingle : CGRState { }
public class CGRSwitchStateEvent : CGREvent { }
public class CGRUsePawnEquipmentFromArchetype : CGRQueryParams { }
public class CHandleNetBroadcast : CNetObjectProtocolEvent { }
public class CHandleNetMessage : CNetObjectProtocolEvent { }
public class CHandleNetUnicast : CNetObjectProtocolEvent { }
public class CHealthFailureEscalationEvent : CEntityEvent { }
public class CHostAdminService : IHostAdminService { }
public class CHudComponent : CEntityComponent { }
public class CHudService : IGameModeService { }
public class CHumanPersonality : CLivingCreature { }
public class CIEDPlacedEvent : CEntityEvent { }
public class CIgnitorComponent : CEntityComponent { }
public class CIgnitorNetworkComponent : CNetworkComponent { }
public class CInputConfig : CNomadConfigObject { }
public class CInputDriverGamepad : CInputDriver { }
public class CInputDriverGamepad_Win32 : CInputDriverGamepad { }
public class CInputDriverKeyboard : CInputDriver { }
public class CInputDriverKeyboard_Win32 : CInputDriverKeyboard { }
public class CInputDriverMouse : CInputDriver { }
public class CInputDriverMouse_Win32 : CInputDriverMouse { }
public class CInventoryItem : CNomadObject { }
public class CInventoryItemAmmoPouch : CInventoryItem { }
public class CInventoryItemEmbeddedGadget : CInventoryItemGadget { }
public class CInventoryItemEquipment : CInventoryItem { }
public class CInventoryItemEquippedGadget : CInventoryItemGadget { }
public class CInventoryItemGadget : CInventoryItemEquipment { }
public class CInventoryItemWeapon : CInventoryItemEquipment { }
public class CInvisibleWall : CBasicRegionEntity { }
public class CJackalTapeManager : CSingletonEntity { }
public class CKeyFramedGradientColor : CNomadObject { }
public class CKickBanService : IGameModeService { }
public class CLadder : CGameObject { }
public class CLadderNetworkComponent : CNetworkComponent { }
public class CLandmarkFarCategory : CSectorSpawnCategory { }
public class CLandmarkNearCategory : CSectorSpawnCategory { }
public class CLANLoginOperation : CLoginOperation { }
public class CLayerResource : CResourceContainer { }
public class CLeaderboardService : IGameModeService { }
public class CLightEvent : CEntityEvent { }
public class CLinearPathFollower : CPathFollower { }
public class CLiquidPropaneTank : CEntityComponent { }
public class CListMenuPage : CMenuPage { }
public class CLivingCreature : CPersonality { }
public class CLoadGamePage : CGameFilesListPage { }
public class CLobbyService : IGameModeService { }
public class CLoginOperation : CSessionOperation { }
public class CLoginSessionParam : COperationData { }
public class CLookAtTriggerComponent : CBaseTriggerComponent { }
public class CLoopingPathFollower : CPathFollower { }
public class CLuaResource : CResource { }
public class CMacheteEvent : CEntityEvent { }
public class CMagicCrate : CEntityComponent { }
public class CMagmaConfigUIResource : CMagmaResourceContainer { }
public class CMagmaDebugTextService : IMagmaDebugTextService { }
public class CMagmaResourceContainer : CResourceContainer { }
public class CMagmaUIResource : CMagmaResourceContainer { }
public class CMajorLocationEntity : CEntity { }
public class CMalariaEvent : CEntityEvent { }
public class CMapElementComponent : CEntityComponent { }
public class CMapElementEvent : CEntityEvent { }
public class CMapElementStateChangedEvent : CEntityEvent { }
public class CMapIntelligence : CEntityComponent { }
public class CMapMarkerManager : CSingletonEntity { }
public class CMapOverrideTextureEvent : CEntityEvent { }
public class CMapService : IGameModeService { }
public class CMassiveComponent : COnlineAdComponent { }
public class CMatchService : IGameModeService { }
public class CMaterialImpactFx : CNomadObject { }
public class CMaterialResource : CResourceContainer { }
public class CMedicStation : COpeningPickup { }
public class CMedicStationNetworkComponent : CPickupNetworkComponent { }
public class CMemoryStreamFile : IFile { }
public class CMenuPage : CUIPageBase { }
public class CMetaSector : CWorldSector { }

public class CMissionComponent : CEntityComponent {
    public string X_7AF1FD74;
    public uint hidMissionLayerPath;
    public string X_27B31D2E;
    public uint hidCategory;
    public bool ForceMerge;
}

public class CMissionHandlerEvent : CScriptEvent { }
public class CMortarIncoming : CEntityEvent { }
public class CMountedWeapon : CEntityComponent { }
public class CMountedWeaponNetworkComponent : CNetworkComponent { }
public class CMountedWeaponSmartTerrain : CSmartTerrain { }
public class CMovementResource : CResourceContainer { }
public class CMPBase : CEntityComponent { }
public class CMusicAIInfoManager : CSingletonEntity { }
public class CMusicManager : CSingletonEntity { }
public class CMuzzleFlashManager : IGameModeService { }
public class CNavMeshGenComponent : CGameAIObject { }
public class CNavMeshSectorResource : CResourceContainer { }
public class CNetDescriptor : CNetDataContainer { }
public class CNetGameContextResolvedOperation : CGameOperation { }
public class CNetGameCtrlEnterGame : CNetGRStateProceedOperation { }
public class CNetGameCtrlEnterLobby : CNetGRStateProceedOperation { }
public class CNetGameCtrlOnGameModeChange : CGameOperation { }
public class CNetGameCtrlOnLoadWorldSync : CNetGameCtrlStateBaseSynchOp { }
public class CNetGameCtrlOnUnloadWorldSync : CNetGameCtrlStateBaseSynchOp { }
public class CNetGameCtrlStateBaseSynchOp : CGameOperation { }
public class CNetGameCtrlStateChangeContext : CNetGameCtrlStateGameContext { }
public class CNetGameCtrlStateGameContext : CNetGameCtrlState { }
public class CNetGameCtrlStateLocalPresence : CNetGameCtrlStatePresence { }
public class CNetGameCtrlStatePresence : CNetGameCtrlState { }
public class CNetGameCtrlStateUpdate : CNetGameCtrlState { }
public class CNetGameCtrlStateUpdateInGame : CNetGameCtrlStateUpdate { }
public class CNetGameCtrlStateUpdateLobby : CNetGameCtrlStateUpdate { }
public class CNetGRStateProceedOperation : CGameOperation { }
public class CNetObjectEvent : INetEvent { }
public class CNetObjectMonitoringEvent : CNetObjectEvent { }
public class CNetObjectOperation : IOperation { }
public class CNetObjectProtocolEvent : CNetObjectEvent { }
public class CNetObjectReady : CNetObjectEvent { }
public class CNetObjectResolved : CNetObjectMonitoringEvent { }
public class CNetObjectResolvedLegacy : CNetObjectEvent { }
public class CNetObjectUnresolved : CNetObjectMonitoringEvent { }
public class CNetworkComponent : CEntityComponent { }
public class CNetworkConfig : CNomadConfigObject { }
public class CNetworkLogConfig : CNomadConfigObject { }
public class CNetworkResource : CResource { }
public class CNetworkSettingGenericbool : CNetworkSetting { }
public class CNetworkSettingGenericCryString : CNetworkSetting { }
public class CNetworkSettingGenericunsigned_long : CNetworkSetting { }
public class CNetworkSettingsCollection : CNetworkSetting { }
public class CNewParticlesComponent : CEntityComponent { }
public class CNewParticlesSystemCleanEvent : CEntityEvent { }
public class CNewParticlesSystemPauseEvent : CEntityEvent { }
public class CNewParticlesSystemStartEvent : CEntityEvent { }
public class CNewParticlesSystemStopEvent : CEntityEvent { }
public class CNewsGetChannelOperation_RdV : CNewsOperation_RdV { }
public class CNewsGetHeadersOperation_RdV : CNewsOperation_RdV { }
public class CNewsGetHeadersParams : COperationData { }
public class CNewsGetNewsHeadersOperation_RdV : CNewsOperation_RdV { }
public class CNewsGetNumberOfNewsOperation_RdV : CNewsOperation_RdV { }
public class CNewsGetNumberOfNewsParams : COperationData { }
public class CNewsOperation_RdV : CRendezVousOperation { }
public class CNewsSetLocalizationOperation_RdV : CSessionOperation { }
public class CNomadConfigObject : CNomadObject { }
public class CNomadDbObject : CNomadObject { }
public class CNomadDbObjectNamed : CNomadDbObject { }
public class CObjectIgnitorCreatorComponent : CEntityComponent { }
public class CObjectSoundAndFXComponent : CEntityComponent { }
public class COcclusionQueryComponent : CEntityComponent { }
public class COmniEntity : CEntity { }
public class COneDayCompletedEvent : CEntityEvent { }
public class COnlineAdComponent : CEntityComponent { }
public class COnScreenPopup : CNomadObject { }
public class COpeningPickup : CPickup { }
public class CParticleAmbianceComponent : CEntityComponent { }
public class CParticleFXComponent : CEntityComponent { }
public class CParticleFXEvent : CEntityEvent { }
public class CParticlePhysComponent : CPhysComponent { }
public class CParticleRegion : CNomadObject { }
public class CParticlesEmitterParamResource : CResourceContainer { }
public class CParticlesSystemParamResource : CResourceContainer { }
public class CPartyService : IGameModeService { }
public class CPathFindTester : CAgent { }
public class CPatrolBrain : CBrain { }
public class CPathFollower : CNomadObject { }

public class CPawn : CGameObject {
    // public byte[] Implementation;
    public bool bIsAI;
    // Skills
    public string X_502D1B6A;
    public uint filePawnStateMachine;
    // Inventory
    // DesiredData
    // EffectiveData
    // JumpHeight
    // SavedMoveState
    // BonusPlans
    // StateDriver
    // PawnBlackboard
    // SerializationEvent
    // SerializationEvent
    public bool Usable;
    public bool Enabled;
    public bool IsUsableOrientationNeeded;

    public class Body : CPawnBody { }
    public class Skills { }
    public class Inventory : SInventoryViewPawnImpl { }
    public class IdleCycleBreaker {
        public float fMinTime;
        public float fMaxTime;
    }
}

public class CPawnAction : CAgentAction { }

public class CPawnAgent : CGameAgent {
    // RescueAttempt
    // RescueCooldown
    // IsDead
    // FlareCooldown
    // IsUsingMountedWeapon
    // CurrentArmyMemberState
    // PreviousArmyMemberState
    // CurrentArmyMemberRole
    // DesiredArmyMemberRole
    // CurrentArmyMemberRoleAction
    // DesiredArmyMemberRoleAction
    // GotoFireRange
    // FireStrategy
    // LookStrategy
    // EmotionStrategy
    // AimStrategy
    // SpecialStrategy
    // CurrentAttackZone
    // ThreatLevelTimeCounter
    // ThreatLevel
    // PillarThresholdCross
    // RescueState
    // ThreatEventTimeStamp
    // ThreatLevelCounter
    // ThreatPriority
    // HealthFailureWhileHealing
    // ThresholdStartTime
    // TimeSinceLastShot
    // TimeSinceHMRFailure
    // MercBrain
    // MercBrainST
    // RescueSafe
    // ThresholdLevel
    // CurrentBuildingId
    // BumpAngle
    // BumpSpeed
    // PreviousBestTarget
    // CurrentBestTarget
    // SawSomethingLevel
    // AlertLevel
    // BlindCombatLevel
    // FuzzyVisibility
    // ClearVisibility
    // TimeOfDeath
    // IsPlayerInAIvsAIZone
    // InitialReinforcementRegionId
    // InitialStrategicZoneId
    // WeaponReadyTimer
    // LastMuzzleFlashTime
    // AiShootMeObjectId
    // LastBlindCombatNotification
    // HighestSocialRegionType
    // AllSocialRegionType
    // IsPlayer
    // LastTimeHurt
    // AlertLostTargetRushType
    // ProjEscapeType
    // WagerHandle
    // IsSpecialMissionBehaviourMerc
    // IsSafeHouseMerc
    // OutsideWagerLifeTime
    // WeaponCurrentClass
    // WeaponPreviousClass
    // WeaponLastTransitionTime
    // WeaponSwitchTo
    // ReservedEntrance
    // ReadyForMoveCallback
    // MoveCallbackLayer
    // ShineLensCounter
    // DominoDataArray, DominiData, DominoData
    // AutomaticScriptedScenePrefab
    // PlayingAnim
    // NextAnim
    // IntuitionTimer
    // GotIntuition
    // BulletJustMissed
    // MustDieNow
    // VehicleFallBackPosTimer
    // VehicleFallBackPositions
    // VariationID
    // VariationID2
    public bool bHasALongRangeWeapon;
    public bool bOppositeArmy;
    public float m_IdleFuzzyVal;
    public float m_IdleClearVal;
    public float m_SocialFuzzyVal;
    public float m_SocialClearVal;
    public float m_AlertFuzzyVal;
    public float m_AlertClearVal;
    public float m_CombatFuzzyVal;
    public float m_CombatClearVal;
    public float m_ThresholdFuzzyVal;
    public float m_ThresholdClearVal;
    public float m_SpecialFuzzyVal;
    public float m_SpecialClearVal;
    public float m_DeadFuzzyVal;
    public float m_DeadClearVal;
    public float m_VehicleFuzzyVal;
    public float m_VehicleClearVal;
    public uint selArmy;
    public uint selODU;
    public uint selSpecialCharacterType;
    public uint selAIInfamyMode;

    public class enumArmy { }
    public class enumODU { }
    public class enumSpecialCharacterType { }
    public class enumAIInfamyMode { }

    public class ShootingSystem {
        public string archGroupNumberCurve;
        // TargetStatus
        // AimingDot
        public float fMissWidth;
        public float fMissHeight;
        public float fTimerToMissTarget;
        public float fPointBlankDistance;
        public float fTimerToPointBlank;

        public class ShooterStatus : CPawnFactorParam { }
        public class TargetStatus : CPawnFactorParam { }
    }

    public class SensorySystem : CSensorySystem { }
}

public class CPawnAgentRescueEvent : CEntityEvent { }
public class CPawnBarkEvent : CEntityEvent { }
public class CPawnBeautifier : CEntityComponent { }
public class CPawnBeautifierAI : CPawnBeautifier { }
public class CPawnBeautifierAICinematic : CPawnBeautifier { }
public class CPawnBeautifierBuddyDownAI : CPawnBeautifier { }
public class CPawnBeautifierCinematicFirst : CPawnBeautifierFirst { }
public class CPawnBeautifierComponent : CEntityComponent { }
public class CPawnBeautifierDominoPlayer : CPawnBeautifier { }
public class CPawnBeautifierFirst : CPawnBeautifier { }
public class CPawnBeautifierFirstNoControl : CPawnBeautifierFirst { }
public class CPawnBeautifierHMRAI : CPawnBeautifier { }
public class CPawnBeautifierLadder : CPawnBeautifier { }
public class CPawnBeautifierMeleeAI : CPawnBeautifier { }
public class CPawnBeautifierMountedWeapon : CPawnBeautifier { }
public class CPawnBeautifierNetPlayer : CPawnBeautifier { }
public class CPawnBeautifierPickupPlayer : CPawnBeautifier { }
public class CPawnBeautifierPlantedWeapon : CPawnBeautifier { }
public class CPawnBeautifierPlayer : CPawnBeautifier { }
public class CPawnBeautifierRagdoll : CPawnBeautifier { }
public class CPawnBeautifierRescueAI : CPawnBeautifier { }
public class CPawnBeautifierRescuePlayer : CPawnBeautifier { }
public class CPawnBeautifierRevive : CPawnBeautifier { }
public class CPawnBeautifierSlide : CPawnBeautifier { }
public class CPawnBeautifierStorm : CPawnBeautifier { }
public class CPawnBeautifierSwim : CPawnBeautifier { }
public class CPawnBeautifierThird : CPawnBeautifier { }
public class CPawnBeautifierVehicle : CPawnBeautifier { }
public class CPawnBeautifierVehiclePassenger : CPawnBeautifier { }
public class CPawnBeautifierVehicleRide : CPawnBeautifierVehicle { }
public class CPawnBeautifierVehicleRidePassenger : CPawnBeautifierDominoPlayer { }

public class CPawnBody {
    public float fJumpHeight;
    public float fJumpHeightExhausted;
    public float fGravity;
    public float fWalkingMaxSpeed;
    public float fWalkingMaxSpeedCrouch;
    public float fWalkingAcceleration;
    public float fWalkingDeceleration;
    public string archSprintCurve;
    public float fSprintingDeceleration;
    public float fClimbSpeed;
    public float fSwimmingMinDepth;
    public float fSwimmingMaxSpeed;
    public float fSwimmingAcceleration;
    public float fSwimmingDeceleration;
    public float fDivingMaxSpeed;
    public float fDivingAcceleration;
    public float fDivingDeceleration;
    public float fSprintingTurnModifier;
    public float fSprintingStrafeLimit;
    public float SwimmingClimbMinHeight;
    public float SwimmingClimbMaxHeight;
}

public class CPawnBonusPlanManager : CNomadObject { }
public class CPawnDecision : CAgentDecision { }
public class CPawnEnemyMonitor : CEntityComponent { }
public class CPawnEntity : CEntity { }
public class CPawnEvent : CEntityEvent { }
public class CPawnEventFakeBullet : CEntityEvent { }
public class CPawnEventInstantKill : CEntityEvent { }
public class CPawnEventProcessLanding : CEntityEvent { }

public class CPawnFactorParam {
    public float fStandingFactor;
    public float fCrouchingFactor;
    public float fMoveSpeedBabyStepFactor;
    public float fMoveSpeedWalkFactor;
    public float fMoveSpeedJogFactor;
    public float fMoveSpeedRunFactor;
    public float fMoveSpeedSprintFactor;
    public float fDrivingFactor;
    public float fSwimmingFactor;
    public float fIronsightFactor;
    public uint uiMaxHitPerSecondFactor;
}

public class CPawnInteractionMonitor : CEntityComponent { }
public class CPawnMagicCrate : CEntityComponent { }
public class CPawnNetworkComponent : CNetworkComponent { }
public class CPawnPlayerAchievementsComponent : CEntityComponent { }
public class CPawnPushPlayerEvent : CEntityEvent { }
public class CPawnScanner : CAgentScanner { }
public class CPawnSoundAndFXComponent : CCreatureSoundAndFXComponent { }

public class CPersistComponent : CEntityComponent {
    public uint selLevel;
}

































public class SInventoryViewPawnImpl : CNomadObject {
    public string X_8C965C28;
    public uint packInventoryPack;
    public byte[] archGPSVehicleArchetype;
    public bool bUnlimitedAmmo;
    public bool bAutoReload;
    public bool bAutoDraw;
    public string X_130CDED8;
    public uint sInitialWeaponCategory;
}

public class CPersistenceMgr : IGameModeService { }
public class CPhoneCallEvent : CEntityEvent { }
public class CPhysStim : CEntityEventStims { }
public class CPhysBulletHitStim : CPhysStim { }
public class CPhysCollisionStim : CPhysStim { }
public class CPhysEntityCreateParams : CNomadObject { }
public class CPhysExplosionStim : CPhysStim { }
public class CPhysicalFile : IFile { }
public class CPhysicConfig : CNomadConfigObject { }
public class CPhysOutOfWorldEvent : CEntityEvent { }
public class CPhysPhantomComponent : CEntityComponent { }
public class CPhysRayPhantomComponent : CEntityComponent { }
public class CPhysResource : CResource { }
public class CPhysSimulationEntityCreateParams : CPhysEntityCreateParams { }
public class CPhysRigidEntityCreateParams : CPhysSimulationEntityCreateParams { }
public class CPhysVehicleEntityCreateParams : CPhysRigidEntityCreateParams { }
public class CPhysWheeledVehicleEntityCreateParams : CPhysVehicleEntityCreateParams { }
public class CPickAmmoEvent : CEntityEvent { }
public class CPickup : CEntityComponent { }
public class CPickupAmmo : CPickup { }
public class CPickupContainer : CPickup { }
public class CPickupContainerNetworkComponent : CNetworkComponent { }
public class CPickupDiamond : COpeningPickup { }
public class CPickupEvent : CEntityEvent { }
public class CPickupGadget : CPickup { }
public class CPickupHealth : CPickup { }
public class CPickupMissionItem : CPickup { }
public class CPickupMultipleAmmo : CPickup { }
public class CPickupPile : CPickup { }
public class CPickupPileNetworkComponent : CCompoundPhysNetworkComponent { }
public class CPickupScoutedEvent : CEntityEvent { }

public class CPickupWeapon : CPickup {
    public bool bEnable;
    public float fRespawnTime;
    public bool bCustomBoundingBox;
    public Vector3 vectorBoundingBoxSize;
    public Vector3 vectorBoundingBoxOffset;
    public bool bAffectedLightPickup;
    public bool bPickable;
    public string sUsageString;
    public string objGeometryPreload;
    public uint Priority;
    public bool bCanBeScouted;
    public string archWeapon;
    public uint iMinAmmo;
    public uint iMaxAmmo;
}

public class CPierAnchor : CGameAIObject { }

public class CPlayActionBrain : CBrain { }

public class CPlaybackComponent : CEntityComponent { }

public class CPlayerPopupMenu : CGameMessageBoxList { }

public class CPlayerSoundAndFXComponent : CPawnSoundAndFXComponent { }

public class CPlayerSoundEvent : CEntityEvent { }

public class CPositionLoggerComponent : CEntityComponent { }

public class CPostFxDatabase : CEntityComponent { }

public class CPostFxManager : CSingletonEntity { }

public class CPostFxService : IGameModeService { }

public class CPostLoginOperation : CSessionOperation { }

public class CPrefabDescription : CNomadObject { }

public class CPrefabEntity : CEntity { }

public class CPrefabManager : CSingletonEntity { }

public class CProjectileNetworkComponent : CNetworkComponent { }

public class CProximityTriggerComponent : CBaseTriggerComponent { }

public class CPusher : CEntityComponent { }

public class CQueryProjectileSynchroEvent : CBaseEvent { }

public class CQuickMatchGatherOpCtn : CGameOperationContainer { }

public class CQuickMatchJoinCandidateOp : CGameOperation { }

public class CQuickMatchJoinOpCtn : CGameOperationContainer { }

public class CQuickMatchPingCandidatesOp : CGameOperation { }

public class CQuickMatchRetrieveCandidatesOp : CGameOperation { }

public class CQuickMatchSelectCandidateOp : CGameOperation { }

public class CRadio : CEntityComponent { }

public class CRadioManager : CSingletonEntity { }

public class CRainComponent : CEntityComponent { }

public class CRandomShooterComponent : CEntityComponent { }

public class CReadNetMemento : CNetObjectProtocolEvent { }

public class CRealtreeClusterComponent : CClusterComponent { }

public class CRealtreeComponent : CRenderableComponent { }

public class CRealtreeFx : CNomadDbObject { }

public class CRealtreeFxManager : CSingletonEntity { }

public class CRealtreeResource : CResourceContainer { }

public class CReinforcementEntityLoadedEvent : CEntityEvent { }

public class CReinforcementMercLoadedEvent : CReinforcementEntityLoadedEvent { }

public class CReinforcementPoint : CGameAIObject { }

public class CRelayTriggerComponent : CBaseTriggerComponent { }

public class CRemoveSEFactEvent : CEntityEvent { }

public class CRenderAmbientConfig : CRenderBaseConfig { }

public class CRenderConfig : CNomadConfigObject { }

public class CRenderEnvironmentConfig : CRenderBaseConfig { }

public class CRenderGeometryConfig : CRenderBaseConfig { }

public class CRenderPostFxConfig : CRenderBaseConfig { }

public class CRenderQualityConfig : CRenderBaseConfig { }

public class CRenderShadowConfig : CRenderBaseConfig { }

public class CRenderTerrainConfig : CRenderBaseConfig { }

public class CRenderTextureConfig : CRenderBaseConfig { }

public class CRenderVegetationConfig : CRenderBaseConfig { }

public class CRenderWaterConfig : CRenderBaseConfig { }

public class CRescueManager : IGameModeService { }

public class CResourceWatch : CResourceNotifier { }

public class CStaticGraphicComponent : CBaseGraphicComponent { }

public class CRigidGraphicComponent : CStaticGraphicComponent { }

public class CRigidPhysComponent : CPhysComponent {
    public string X_527E7674;
    public uint hidResourceId;
    public bool bDisabledAtStart;
    public bool bAlwaysStatic;
    public bool bCreateAsStatic;
    public bool bUseFastCollision;
    public bool bDisappearOnDeath;
    public bool bUseMaxTerrainSlope;
    public string sndDestructionSound;
    public float fHealth;
    public float fSelfCollOverrideSpeed;
    public uint selCollisionLayer;
    public uint ResourceIndex;
    public Vector3 vectorCenterOfMassOffset;
    public float fFloatingScale;
    public float fWaterFriction;
    public uint sndtpDestructionSoundType;
}

public class CRigidPhysOnDamageEvent : CEntityEvent { }

public class CRigidPhysOnDieEvent : CEntityEvent { }

public class CRigidPhysOnStateChangeEvent : CEntityEvent { }

public class CRoadSign : CEntityComponent { }

public class CRoadSignManager : CSingletonEntity { }

public class CRocket : CGameObject {
    public uint sFXBone;
}

public class CSafeHouseComponent : CEntityComponent { }

public class CSaveAtNextUpdateEvent : CEntityEvent { }

public class CSaveGamePage : CGameFilesListPage { }

public class CSavePointCheckPage : CMenuPage { }

public class CSavePointSaveGamePage : CSaveGamePage { }

public class CScannerAgentAimingAt : CPawnScanner { }

public class CScannerAgentHasRaisedWeapon : CPawnScanner { }

public class CScannerAgentIsVisible : CPawnScanner { }

public class CScannerAgentSocialProximity : CPawnScanner { }

public class CScannerAgentStaredown : CPawnScanner { }

public class CScannerAimStrategy : CPawnScanner { }

public class CScannerAnimalObstacleAhead : CAgentScanner { }

public class CScannerAnimalThreatChanged : CAgentScanner { }

public class CScannerAnimalThreatened : CAgentScanner { }

public class CScannerArmyMemberRole : CPawnScanner { }

public class CScannerArmyMemberState : CPawnScanner { }

public class CScannerBestTargetChangedPos : CPawnScanner { }

public class CScannerBlackboardFact : CAgentScanner { }

public class CScannerCanDisableSTPDynamicAvoidance : CPawnScanner { }

public class CScannerCheckValue : CAgentScanner { }

public class CScannerDead : CAgentScanner { }

public class CScannerDominoEvent : CPawnScanner { }

public class CScannerEmotionStrategy : CPawnScanner { }

public class CScannerFactExist : CAgentScanner { }

public class CScannerFireProximity : CAgentScanner { }

public class CScannerFireStrategy : CPawnScanner { }

public class CScannerInFOV : CPawnScanner { }

public class CScannerInterestLookAtType : CPawnScanner { }

public class CScannerIsAIShootMeObjectValid : CPawnScanner { }

public class CScannerIsInBuilding : CPawnScanner { }

public class CScannerIsInDistance : CPawnScanner { }

public class CScannerIsInVehicle : CPawnScanner { }

public class CScannerIsPosOnBarge : CAgentScanner { }

public class CScannerIsRotatedTowardsPos : CPawnScanner { }

public class CScannerIsUnderFire : CPawnScanner { }

public class CScannerLookStrategy : CPawnScanner { }

public class CScannerMovingPosition : CAgentScanner { }

public class CScannerMutualGreeting : CPawnScanner { }

public class CScannerNewTargetNeeded : CPawnScanner { }

public class CScannerPawnSenses : CPawnScanner { }

public class CScannerRiskPoint : CPawnScanner { }

public class CScannerSideLookOpening : CPawnScanner { }

public class CScannerSocialBehavior : CPawnScanner { }

public class CScannerSocialRegion : CPawnScanner { }

public class CScannerSpecialStrategy : CPawnScanner { }

public class CScannerTargetVisible : CPawnScanner { }

public class CScannerThresholdCross : CPawnScanner { }

public class CVehicleScanner : CAgentScanner { }

public class CScannerVehicleIntruderAboard : CVehicleScanner { }

public class CScannerVehicleIsFunctional : CVehicleScanner { }

public class CScannerVehicleMergePosReached : CVehicleScanner { }

public class CScannerVehiclePierAnchor : CVehicleScanner { }

public class CScannerVehicleStandBy : CAgentScanner { }

public class CScannerVisualThreat : CPawnScanner { }

public class CScannerWalkDistance : CAgentScanner { }

public class CSceneObjectComponentCSceneAdaptiveBloom : CEntityComponent { }

public class CSceneObjectComponentCScenePostFxDepthOfField : CEntityComponent { }

public class CScoutIntelsManager : CSingletonEntity { }

public class CScriptCallbackComponent : CEntityComponent { }

public class CScriptedScenePrefabEntity : CPrefabEntity { }

public class CScriptService : IGameModeService { }

public class CSectorDataResource : CResource { }

public class CSectorDescriptorResource : CResource { }

public class CSectorEntity : CEntity { }

public class CSectorPreloadResource : CResource { }

public class CSectorResource : CResourceContainer { }
public class CSensorySystem : CNomadObject { }

// nested under: CSensorySystem
public class CSensorySystem_FOVParameters { }

// nested under: CSensorySystem_FOVParameters
public class CSensorySystem_FOVParameters_FOVMultipliers {
    public float fPreCombatMultiplier;
    public float fCombatMultiplier;
    public float fPostCombatMultiplier;
    public float fPlayerInVehicleMultiplier;
    public float fNightTimeMultiplier;
    public float fSniperLengthMultiplier;
    public float fSniperAngleMultiplier;
}

// nested under: CSensorySystem_FOVParameters
public class CSensorySystem_FOVParameters__RegionFOV { }

// nested under: CSensorySystem_FOVParameters__RegionFOV
public class CSensorySystem_FOVParameters__RegionFOV__FOV {
    public float fLength;
    public float fAngle;
}

// nested under: CSensorySystem_FOVParameters__RegionFOV
public class CSensorySystem_FOVParameters__RegionFOV_FocusFOV : CSensorySystem_FOVParameters__RegionFOV__FOV { }

// nested under: CSensorySystem_FOVParameters__RegionFOV
public class CSensorySystem_FOVParameters__RegionFOV_PeripheralFOV : CSensorySystem_FOVParameters__RegionFOV__FOV { }

// nested under: CSensorySystem_FOVParameters
public class CSensorySystem_FOVParameters_DesertFOV : CSensorySystem_FOVParameters__RegionFOV { }

// nested under: CSensorySystem_FOVParameters
public class CSensorySystem_FOVParameters_SavannahFOV : CSensorySystem_FOVParameters__RegionFOV { }

// nested under: CSensorySystem_FOVParameters
public class CSensorySystem_FOVParameters_JungleFOV : CSensorySystem_FOVParameters__RegionFOV { }

// nested under: CSensorySystem
public class CSensorySystem_VisibilityEvaluatorParameters { }

// nested under: CSensorySystem_VisibilityEvaluatorParameters
public class CSensorySystem_VisibilityEvaluatorParameters_Weights {
    public float fDistanceEvaluatorWeight;
    public float fFOVEvaluatorWeight;
    public float fPawnSamplingEvaluatorWeight;
    public float fOcclusionEvaluatorWeight;
    public float fVegetationEvaluatorWeight;
    public float fStanceEvaluatorWeight;
    public float fSpeedEvaluatorWeight;
    public float fAmbientLightEvaluatorWeight;
}

// nested under: CSensorySystem_VisibilityEvaluatorParameters
public class CSensorySystem_VisibilityEvaluatorParameters_InternalValues {
    public float fDistanceEvaluator_FullVisibilityRatio;
    public float fDistanceEvaluator_MinVisibilityAtMaxFOVRange;
    public float fSpeedEvaluator_StandingStillVisibilityFactor;
    public float fFOVEvaluator_VisibilityFactorAtFOVLimit;
}

// nested under: CSensorySystem
public class CSensorySystem_SocialMechanic {
    public float fStareDetectionTime;
    public float fAimAtDetectionTime;
    public float fIntrusionDistanceInnerRing;
    public float fIntrusionDistanceMidRing;
    public float fIntrusionDistanceOuterRing;
    public float fMaxChargingDistance;
    public float fMaxChargingAngle;
}

public class CSessionCreateGameOperation : CSessionOperation { }

public class CSessionCreateOperation : CSessionOperation { }

public class CSessionCreateServiceOperation : CSessionOperation { }

public class CSessionCreateOperation_Agora : CSessionCreateServiceOperation { }

public class CSessionDeleteGameOperation : CSessionOperation { }

public class CSessionDeleteOperation : CSessionOperation { }

public class CSessionDeleteServiceOperation : CSessionOperation { }

public class CSessionDescriptor : CNetDescriptor { }

public class CSessionDescriptor_Agora : CSessionDescriptor { }

public class CSessionFetchOnlineConfigOperation : CRendezVousOperation { }

public class CSessionInfo_Agora : CSessionInfo { }

public class CSessionJoinGameOperation : CSessionOperation { }

public class CSessionJoinOperation : CSessionOperation { }

public class CSessionJoinServiceOperation : CSessionOperation { }

public class CSessionLoginOperation : CSessionOperation { }

public class CSessionLogoutOperation : CSessionOperation { }

public class CSessionUpdateOperation : CSessionOperation { }

public class CSetInvincibleEvent : CEntityEvent { }

public class CSetNetInstanceIdEvent : CBaseEvent { }

public class CShortRangeResource : CSectorResource { }

public class CSimpleAnimationComponent : CEntityComponent {
    public string X_F9F2D5F4;
    public uint fileSkeleton;
    public string sPartName;
}

public class CSimpleEntityEvent : CEntityEvent { }

public class CSimpleNetworkComponent : CNetworkComponent { }

public class CSimplePrimitiveComponent : CRenderableComponent { }

public class CSimpleSettingCMapCycle : CGenericUISettingCMapCycle { }

public class CSkeletonResource : CResource { }

public class CSmartTerrainEvent : CEntityEvent { }

public class CSmartTerrainManager : CSingletonEntity { }

public class CSniperPoint : CGameAIObject { }

public class CSocialRegion : CBasicRegionEntity { }

public class CSomeoneTalked : CEntityEvent { }

public class CSoundComponent : CEntityComponent {
    public uint sndptSoundPoint;
}

public class CSoundLineComponent : CBasicShapeComponent { }

public class CSoundManager : CSingletonEntity { }

public class CSoundResource : CResourceContainer { }

public class CSoundShapeComponent : IShapeComponent { }

public class CSpawnPointBlueStart : CSpawnPointBlue { }

public class CSpawnPointBuddy : CSpawnPoint { }

public class CSpawnPointRedStart : CSpawnPointRed { }

public class CSpawnPointSpectator : CSpawnPoint { }

public class CSpecialEventPoint : CEntity { }

public class CSpectatorPlayer : IPlayer { }

public class CSplinePrimitiveComponent : CRenderableComponent { }

public class CSRLResource : CResource { }

public class CStateMachineBlobResource : CResourceContainer { }

public class CStateMachineResource : CResourceContainer { }

public class CStaticClusterPhysComponent : CPhysComponent { }

public class CStaticDecalComponent : CRenderableComponent { }

public class CStaticPhysComponent : CPhysComponent { }

public class CStealthComponent : CEntityComponent { }

public class CStickyFlameEvent : CEntityEvent { }

public class CStimArray : CNomadObject { }

public class CStimEffectTable : CBaseEntity { }

public class CStimsEmitterComponent : CEntityComponent { }

public class CStopDialogEvent : CEntityEvent { }

public class CTagPoint : CEntity { }

public class CStrategicPoint : CTagPoint { }

public class CSuicideComponent : CEntityComponent { }

public class CTaskActivateInfamyPose : CPawnAction { }

public class CTaskActivateSocialSTP : CAgentAction { }

public class CTaskAimAt : CAgentAction { }

public class CTaskAnimalPathFollow : CAgentAction { }

public class CTaskAttackStrategy : CPawnDecision { }

public class CTaskBreakSocialPair : CPawnAction { }

public class CTaskBroadcastStims : CAgentAction { }

public class CTaskBuddyDown : CPawnDecision { }

public class CTaskCalcLineDist : CAgentAction { }

public class CTaskChase : CAgentAction { }

public class CTaskCheckActionSignal : CAgentAction { }

public class CTaskCheckAimStrategy : CPawnDecision { }

public class CTaskCheckAmmoStatus : CPawnDecision { }

public class CTaskCheckAnimalCanTryAnotherRunAwayDestination : CAgentDecision { }

public class CTaskCheckAnimalThreaten : CAgentDecision { }

public class CTaskCheckArmyRole : CPawnDecision { }

public class CTaskCheckArmyRoleAction : CPawnDecision { }

public class CTaskCheckBargeSide : CAgentDecision { }

public class CTaskCheckBlindCombatLevel : CPawnDecision { }

public class CTaskCheckBuildingEntry : CAgentDecision { }

public class CTaskCheckCanRescue : CPawnDecision { }

public class CTaskCheckCombatMercInRadius : CPawnDecision { }

public class CTaskCheckCoverDist : CPawnDecision { }

public class CTaskCheckCurrentSocialOccupation : CPawnDecision { }

public class CTaskCheckCurrentWeapon : CPawnDecision { }

public class CTaskCheckDifficultyLevel : CAgentDecision { }

public class CTaskCheckDisturbanceType : CPawnDecision { }

public class CTaskCheckDominoData : CPawnDecision { }

public class CTaskCheckEmotionStrategy : CPawnDecision { }

public class CTaskCheckFactExist : CAgentDecision { }

public class CTaskCheckFireProximity : CAgentDecision { }

public class CTaskCheckFireRange : CPawnDecision { }

public class CTaskCheckFireStrategy : CPawnDecision { }

public class CTaskCheckIdleBehavior : CPawnDecision { }

public class CTaskCheckInterestLookAtType : CPawnDecision { }

public class CTaskCheckIsInBuilding : CPawnDecision { }

public class CTaskCheckIsInDistance : CPawnDecision { }

public class CTaskCheckIsInFOV : CPawnAction { }

public class CTaskCheckIsPlayingBark : CPawnAction { }

public class CTaskCheckLookStrategy : CPawnDecision { }

public class CTaskCheckMovingFire : CPawnDecision { }

public class CTaskCheckObjectBlockingPath : CAgentDecision { }

public class CTaskCheckObstaclesInRegion : CAgentDecision { }

public class CTaskCheckODUType : CPawnDecision { }

public class CTaskCheckPillarDepleted : CPawnDecision { }

public class CTaskCheckPillarThreshold : CPawnDecision { }

public class CTaskCheckPlayerAction : CPawnDecision { }

public class CTaskCheckPlayerInfamy : CPawnDecision { }

public class CTaskCheckPosInLoadedSector : CAgentDecision { }

public class CTaskCheckPosOnSpline : CAgentDecision { }

public class CTaskCheckProjEscapeType : CPawnDecision { }

public class CTaskCheckProximity : CAgentAction { }

public class CTaskCheckQueryRange : CPawnDecision { }

public class CTaskCheckRegionTransition : CPawnDecision { }

public class CTaskCheckRegionType : CPawnDecision { }

public class CTaskCheckRelativeInfamy : CPawnDecision { }

public class CTaskCheckRescueState : CPawnDecision { }

public class CTaskCheckSawSomethingLevel : CPawnDecision { }

public class CTaskCheckSeeFriendNearby : CPawnDecision { }

public class CTaskCheckSmartTerrainType : CAgentDecision { }

public class CTaskCheckSocialProximity : CPawnDecision { }

public class CTaskCheckSpecialMissionBehaviour : CPawnDecision { }

public class CTaskCheckSpecialStrategy : CPawnDecision { }

public class CTaskCheckSquadAction : CPawnDecision { }

public class CTaskCheckSquadRole : CPawnDecision { }

public class CTaskCheckStressLevel : CPawnDecision { }

public class CTaskCheckTargetHeightDiff : CPawnDecision { }

public class CTaskCheckTargetRange : CPawnDecision { }

public class CTaskCheckTargetType : CPawnDecision { }

public class CTaskCheckTargetVisible : CPawnDecision { }

public class CTaskCheckThreatDistance : CAgentDecision { }

public class CTaskCheckThresholdLevel : CPawnDecision { }

public class CTaskCheckUnderFire : CPawnDecision { }

public class CTaskCheckUsingCover : CPawnDecision { }

public class CTaskCheckViewBlocked : CPawnDecision { }

public class CTaskCheckVisibleByPlayer : CPawnDecision { }

public class CTaskChooseCoverAttack : CPawnDecision { }

public class CTaskChurchAssault : CPawnAction { }

public class CTaskCleanBriefingAnim : CPawnAction { }

public class CTaskClearMoveToDynamics : CPawnAction { }

public class CTaskComputeInterpolatedPos : CAgentAction { }

public class CTaskComputeLeapFrogStep : CAgentAction { }

public class CTaskComputeProjectileTrajectory : CPawnAction { }

public class CTaskComputeSynchActionPosition : CPawnAction { }

public class CTaskCoverAttack : CPawnAction { }

public class CTaskDebugSetCurrentBehavior : CPawnAction { }

public class CTaskDisableSTPDynamicAvoidance : CPawnAction { }

public class CTaskDisplayError : CAgentAction { }

public class CTaskDisplaySTPClippingError : CAgentAction { }

public class CTaskDropItem : CPawnAction { }

public class CTaskEmitBark : CPawnAction { }

public class CTaskFindAIShootMeObject : CPawnDecision { }

public class CTaskFindCover : CAgentAction { }

public class CTaskFindCoverAttack : CAgentAction { }

public class CTaskFindEscapePos : CAgentAction { }

public class CTaskFindInterestLookAt : CPawnAction { }

public class CTaskFindLeapFrogStep : CAgentDecision { }

public class CTaskFindMountedWeapon : CPawnAction { }

public class CTaskFindProtectionPoint : CAgentAction { }

public class CTaskFindRandomDest : CAgentAction { }

public class CTaskFindRescueDest : CAgentAction { }

public class CTaskFindRiskPoints : CPawnAction { }

public class CTaskFindSocialFleePos : CPawnAction { }

public class CTaskFindStrategicPoint : CPawnDecision { }

public class CTaskFindVisualThreat : CPawnAction { }

public class CTaskFindWorldEntity : CAgentAction { }

public class CTaskFireStrategySelector : CPawnDecision { }

public class CTaskFuzzyChoice : CPawnDecision { }

public class CTaskGetBuildingEntry : CPawnAction { }

public class CTaskGetClosestSplinePos : CAgentAction { }

public class CTaskGetNextPathPos : CAgentAction { }

public class CTaskGetPatrolPath : CAgentAction { }

public class CTaskGetPosOnNavMesh : CAgentAction { }

public class CTaskGetRescuePositions : CPawnAction { }

public class CTaskGetSniperPoint : CPawnAction { }

public class CTaskGetStraightPath : CAgentAction { }

public class CTaskHighTargetAttackPos : CPawnDecision { }

public class CTaskIncreaseSawSomethingLevel : CPawnAction { }

public class CTaskIncrementPathPos : CAgentAction { }

public class CTaskLookAround : CPawnAction { }

public class CTaskLookAroundTarget : CPawnAction { }

public class CTaskLookAt : CAgentAction { }

public class CTaskLookAtVehicle : CPawnAction { }

public class CTaskLookRandom : CPawnAction { }

public class CTaskManageAnchor : CAgentAction { }

public class CTaskManageArmy : CPawnAction { }

public class CTaskMoveStrategy : CPawnDecision { }

public class CTaskMoveTo : CAgentAction { }

public class CTaskNextWeapon : CPawnAction { }

public class CTaskNotifyUnreachablePos : CPawnAction { }

public class CTaskOperateOnFlagField : CAgentDecision { }

public class CTaskOrientToward : CPawnAction { }

public class CTaskPathAnalyzer : CPawnDecision { }

public class CTaskPathFind : CAgentAction { }

public class CTaskPathFindAndMoveTo : CAgentAction { }

public class CTaskPathFollow : CAgentAction { }

public class CTaskPatrol : CAgentAction { }

public class CTaskPlayAnim : CAgentAction { }

public class CTaskPlayBriefingAnim : CPawnAction { }

public class CTaskPlaySound : CAgentAction { }

public class CTaskPredictImpactPos : CPawnAction { }

public class CTaskPrepareSynchActionPosition : CPawnAction { }

public class CTaskPushPlayer : CPawnAction { }

public class CTaskRequestVehicle : CPawnAction { }

public class CTaskReserveProtectionPoint : CAgentAction { }

public class CTaskReserveSniperPoint : CPawnAction { }

public class CTaskResourceManager : CAgentAction { }

public class CTaskSavePosInFact : CAgentAction { }

public class CTaskSearchOpponents : CPawnAction { }

public class CTaskSelectBestOpponents : CPawnDecision { }

public class CTaskSelectBestTarget : CPawnDecision { }

public class CTaskSelectRiskPoint : CPawnDecision { }

public class CTaskSelectWeapon : CPawnAction { }

public class CTaskSendActionSignal : CAgentAction { }

public class CTaskSendBrainEvent : CAgentAction { }

public class CTaskSendDominoEvent : CPawnAction { }

public class CTaskSendHMREvent : CPawnAction { }

public class CTaskSendReport : CAgentAction { }

public class CTaskSendSocialReport : CPawnAction { }

public class CTaskSetAimStrategy : CPawnAction { }

public class CTaskSetCurrentState : CPawnAction { }

public class CTaskSetEmotionStrategy : CPawnAction { }

public class CTaskSetFacialEmotion : CPawnAction { }

public class CTaskSetFireStrategy : CPawnAction { }

public class CTaskSetForcedLookAtEntity : CPawnAction { }

public class CTaskSetLookStrategy : CPawnAction { }

public class CTaskSetPathPointPosition : CAgentAction { }

public class CTaskSetPawnAttribute : CPawnAction { }

public class CTaskSetPawnTarget : CPawnAction { }

public class CTaskSetPostureAttribute : CPawnAction { }

public class CTaskSetPostureIntention : CPawnAction { }

public class CTaskSetSocialEngageMode : CPawnAction { }

public class CTaskSetSpecialStrategy : CPawnAction { }

public class CTaskSetSpeed : CAgentAction { }

public class CTaskSetStanceOnSniperPoint : CPawnAction { }

public class CTaskSetSyncState : CPawnAction { }

public class CTaskShoot : CPawnAction { }

public class CTaskShootMortar : CPawnAction { }

public class CTaskShootMountedWeapon : CPawnAction { }

public class CTaskSmartTerrainExecutor : CAgentAction { }

public class CTaskSmartTerrainFinder : CAgentAction { }

public class CTaskSpecialVehicleDetach : CPawnAction { }

public class CTaskSplinePathFind : CAgentAction { }

public class CTaskStopBark : CPawnAction { }

public class CTaskStopBarkGesture : CPawnAction { }

public class CTaskSwitchWeapon : CPawnAction { }

public class CTaskTeleportInVehicleSeat : CPawnAction { }

public class CTaskUnReserveCover : CAgentAction { }

public class CTaskUpdateBlackboard : CAgentAction { }

public class CTaskUpdateBuddyAiming : CPawnAction { }

public class CTaskUpdatePathPos : CAgentAction { }

public class CTaskUseAIBuilding : CPawnAction { }

public class CTaskUseMountedWeapon : CPawnAction { }

public class CTaskUseSniperPoint : CPawnAction { }

public class CVehicleAction : CAgentAction { }

public class CTaskVehicleAccost : CVehicleAction { }

public class CTaskVehicleAggressiveMove : CVehicleAction { }

public class CTaskVehicleBoostFactor : CVehicleAction { }

public class CTaskVehicleChase : CVehicleAction { }

public class CVehicleDecision : CAgentDecision { }

public class CTaskVehicleCheckExitOnLand : CVehicleDecision { }

public class CTaskVehicleCheckSpeed : CVehicleDecision { }

public class CTaskVehicleCheckUserPriority : CVehicleDecision { }

public class CTaskVehicleEnableSteeringEngine : CVehicleAction { }

public class CTaskVehicleEscapeProjectile : CVehicleAction { }

public class CTaskVehicleGetBargePos : CVehicleAction { }

public class CTaskVehicleGetMergePos : CVehicleAction { }

public class CTaskVehicleGetPierAnchor : CVehicleAction { }

public class CTaskVehicleOrientToward : CVehicleAction { }

public class CTaskVehiclePathFollow : CVehicleAction { }

public class CTaskVehicleSetUserRolePriority : CVehicleAction { }

public class CTaskVehicleSink : CVehicleAction { }

public class CTaskVehicleStop : CVehicleAction { }

public class CTaskVehicleTurnAround : CVehicleAction { }

public class CTaskVehicleTurnCheat : CVehicleAction { }

public class CTaskVehicleUpdatePathFollow : CVehicleAction { }

public class CTaskWait : CAgentAction { }

public class CTaskWaitFactExist : CAgentAction { }

public class CTaskWatchFlyingProjectile : CPawnAction { }

public class ITeamManager : IGameModeService { }

public class CTeamManager : ITeamManager { }

public class CTerm : CNomadObject { }

public class CTermFactList : CTerm { }

public class CTermSingleFact : CTerm { }

public class CTextureMipResource : CResource { }

public class CTextureResource : CResource { }

public class CThinPropaneTank : CEntityComponent { }

public class CThreadingConfig : CNomadConfigObject { }

public class CTimeOfDayTriggerComponent : CBaseTriggerComponent { }

public class CTravelStartOperation : CGameOperation { }

public class CTravelStopOperation : CGameOperation { }

public class CTriggerChangeCountEvent : CEntityEvent { }

public class CTriggerComponent : CEntityComponent {
    public bool static_;
}

public class CTriggerEnableEvent : CEntityEvent { }

public class CTriggerEvent : CEntityEvent { }

public class CTriggerSimpleEvent : CEntityEvent { }

public class CTutorial : CChallenge { }

public class CUbisoftLoginOperation : CRendezVousOperation { }

public class CUGCLoginOperation : CLoginOperation { }

public class CUnreachableLocationsManager : CSingletonEntity { }

public class CUsableComponent : CEntityComponent { }

public class CValidEntityToAttachExplosive : CEntityEvent { }

public class CVegetationObstructionEvent : CEntityEvent { }

public class CVegetationSlowdownComponent : CEntityComponent { }

public class CVehicle : CGameObject { }

public class CVehicleAgent : CGameAgent { }

public class CVehicleDamagedPartEvent : CEntityEvent { }

public class CVehicleEngineFloodedEvent : CEntityEvent { }

public class CVehicleEventExplosion : CEntityEvent { }

public class CVehicleEventIsDestructable : CEntityEvent { }

public class CVehicleEventSetEngineBroken : CEntityEvent { }

public class CVehiclePhysComponent : CPhysComponent { }

public class CVehicleFloatingPhysComponent : CVehiclePhysComponent { }

public class CVehicleMaterialComponent : CCustomMaterialComponent { }

public class CVehicleNetworkComponent : CNetworkComponent { }

public class CVehicleParagliderPhysComponent : CVehiclePhysComponent { }

public class CVehicleSmartTerrain : CSmartTerrain { }

public class CVehicleSoundAndFXComponent : CObjectSoundAndFXComponent { }

public class CVehicleStateChangeEvent : CEntityEvent { }

public class CVehicleUserAccepted : CEntityEvent { }

public class CVehicleWheeledPhysComponent : CVehiclePhysComponent { }

public class CVisibilityOcclusionVolumeComponent : CEntityComponent { }

public class CVisibleObject : CGameAIObject { }

public class CVolumeCheckManager : CSingletonEntity { }

public class CVotingService : IGameModeService { }

public class CWagerRegion : CBasicRegionEntity { }

public class CWaterSoundManager : CSingletonEntity { }

public class CWeaponBazaar : IGameModeService { }

public class CWeaponEventBulletShot : CEntityEvent { }

public class CWeaponEventFireBullet : CEntityEvent { }

public class CWeaponEventReload : CEntityEvent { }

public class CWeaponFireProperties : CNomadObject { }

public class CWeaponFireBulletProperties : CWeaponFireProperties { }

public class CWeaponFireStrategy : CEquipmentUseStrategy { }

public class CWeaponFireBulletStrategy : CWeaponFireStrategy { }

public class CWeaponFireFlameProperties : CWeaponFireProperties { }

public class CWeaponFireFlameStrategy : CWeaponFireStrategy { }

public class CWeaponFireMeleeProperties : CWeaponFireProperties { }

public class CWeaponFireMeleeStrategy : CWeaponFireStrategy { }

public class CWeaponFireProjectileProperties : CWeaponFireBulletProperties { }

public class CWeaponFireProjectileStrategy : CWeaponFireBulletStrategy { }

public class CWeaponNetworkComponent : CNetworkComponent { }

public class CWeaponProperties : CEntityComponent { }

public class CWeaponStimsCEntityEventStims : CEntityEventStims { }

public class CWeaponUsedEvent : CEntityEvent { }

public class CXmlResource : CResource { }

public class CZoneInfoComponent : CEntityComponent {
    public float fSamplingRadius;
    public uint uiGridSubdivisions;
    public float fDensityAdjustmentSpeed;
    public float fWeightScale;
    public float fWeightDistributionPower;
}

public class CZoneLogicManager : CSingletonEntity { }

public class CZoneLogicRegion : CBasicRegionEntity { }

public class CZoneSectorResource : CResource { }

public class SDecalDescription : CNomadDbObjectNamed { }

public class SMixingPreset : CNomadObject { }

public class SPhysMaterial : CNomadDbObjectNamed { }

public class SSettings : CNomadDbObjectNamed { }

public class SSoundPoint : CNomadDbObject { }

public class StCollectionResInfo : CNomadObject { }
