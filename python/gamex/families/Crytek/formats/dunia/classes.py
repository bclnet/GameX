from __future__ import annotations
from dataclasses import dataclass, field
from numpy import ndarray

# types
type Vector2 = ndarray
type Vector3 = ndarray
type Vector4 = ndarray

#region forwards

class CSingletonEntity: pass
class IGameModeService: pass
class CTask: pass
class CEntityEvent: pass
class CAIObject: pass
class CDecision: pass
class CScanner: pass
class CGameAIObject: pass
class CEntityComponent: pass
class CAIObjectRoot: pass
class CPawnSoundAndFXComponent: pass
class CResourceContainer: pass
class CCollective: pass
class CGameObject: pass
class CGameAgent: pass
class CLivingCreature: pass
class CRenderBaseConfig: pass
class IAuthorizationService: pass
class CRenderableComponent: pass
class IShapeEntity: pass
class IShapeComponent: pass
class CPlan: pass
class CRandomPathFollower: pass
class CCameraComponent: pass
class CCameraNetworkComponent: pass
class CNetworkComponent: pass
class CPhysComponent: pass
class CNetDescriptor: pass
class ICollectionIgnitorComponent: pass
class CGenericEntityEventbool: pass
class CSessionOperation: pass
class CNetObjectOperation: pass
class CMemoryStreamFile: pass
class CLoginOperation: pass
class CSoundEvent: pass
class CGameMessageBox: pass
class CSpawnPointService: pass
class CMenuPage: pass
class CFCXGameOperation: pass
class CSettingsPage: pass
class CListMenuPage: pass
class ICountersService: pass
class CGameOperationContainer: pass
class CFCXMoviePage: pass
class CGameFilesService: pass
class CFCXUiService: pass
class CGameMessageService: pass
class CGameModeParamNode: pass
class CGameModeSingle: pass
class CGameplayManager: pass
class CGameSettingsService: pass
class CGameSoundService: pass
class CFCXGOBuilderCommon: pass
class CGOCustomGroupNode: pass
class CGRStateLoadPlayer: pass
class CGRStateMenu: pass
class CGRStateSingle: pass
class CGRStateLoad: pass
class CHudService: pass
class CLoadGamePage: pass
class CLobbyService: pass
class CMapService: pass
class CMatchService: pass
class CFCXMultiMatchOptionsPage: pass
class CFCXMultiMatchBrowserPage: pass
class CFCXMultiMainMatchBasePage: pass
class CNetGameCtrlStateBaseSynchOp: pass
class CParticleAmbianceComponent: pass
class CPlayer: pass
class CPlayerService: pass
class CScoreboardService: pass
class CTDMSpawnPointService: pass
class CTrackingService: pass
class CWeapon: pass
class CWeaponsService: pass
class CRendezVousOperation: pass
class CGadgetUseStrategy: pass
class CNomadConfigObject: pass
class CGameMessageBoxPopUpTutorial: pass
class CGameMessageBoxSpinner: pass
class IGameMessageService: pass
class COmniEntity: pass
class IGameSoundService: pass
class IGameStatsService: pass
class CValueListSettingbool: pass
class CValueListSettingCryString: pass
class CValueListSettingunsigned_long: pass
class CGOState: pass
class CNetObjectProtocolEvent: pass
class IHostAdminService: pass
class CSectorSpawnCategory: pass
class CPathFollower: pass
class CPersonality: pass
class CMagmaResourceContainer: pass
class IMagmaDebugTextService: pass
class COpeningPickup: pass
class CPickupNetworkComponent: pass
class CWorldSector: pass
class CScriptEvent: pass
class CSmartTerrain: pass
class CNetGRStateProceedOperation: pass
class CNetGameCtrlStateGameContext: pass
class CNetGameCtrlStatePresence: pass
class CNewsOperation_RdV: pass
class CPickup: pass
class CPawnBody: pass
class SInventoryViewPawnImpl: pass
class CPawnFactorParam: pass
class CSensorySystem: pass
class CPhysStim: pass
class CPhysSimulationEntityCreateParams: pass
class IPlayerService: pass
class CStaticGraphicComponent: pass
class CVehicleScanner: pass
class CSessionCreateServiceOperation: pass
class ISpawnPointService: pass
class CTagPoint: pass
class CTaskRoot: pass
class CVehicleAction: pass
class CVehicleDecision: pass
class ITeamManager: pass
class CVehiclePhysComponent: pass
class CWeaponFireProperties: pass
class CWeaponFireStrategy: pass

def forward(obj, base) -> None:
	pass

#endregion

#region Base classes
# Base classes referenced via 'extends' that are not themselves defined in the XML

class CAIEvent:	pass
class CBaseEntity: pass
class CBaseFact: pass
class CClientInfo: pass
class CCommandCBParam: pass
class CEntity: pass
class CGOStateEvent: pass
class CGRQueryParams: pass
class CGRState: pass
class CGameFile: pass
class CGameFileHeader: pass
class CGameMessageParser: pass
class CGameMode: pass
class CGameOperation: pass
class CGameOperationBuilder: pass
class CGameSetting: pass
class CInputDriver: pass
class CNetDataContainer: pass
class CNetGameCtrlState: pass
class CNetworkSetting: pass
class CNomadObject: pass
class COmniMapEntity: pass
class COperation: pass
class COperationData: pass
class CResource: pass
class CSessionInfo: pass
class CSpawnPoint: pass
class CSpawnPointBlue: pass
class CSpawnPointRed: pass
class CUIPageBase: pass
class CUISettingBase: pass
class IFile: pass
class IGOStateContext: pass
class INetEvent: pass
class IOperation: pass
class IPlayer: pass

#endregion

#region Move later

@dataclass
class X_256A1FF9(object):
	Name: str = field(default="")

@dataclass
class X_E0BDB3DB(object):
	Name: str = field(default="")

# The following definitions need to be moved to their proper place in native classes.They're here from when there wasn't any subclass handling.

@dataclass
class WorldSector(object):
	Id: int = field(default=0)
	X: int = field(default=0)
	Y: int = field(default=0)

@dataclass
class hidEffectBones(object): pass

@dataclass
class enumCollisionLayer(object): pass

@dataclass
class Impact(object):
	x_67F06359: str = field(default="")

@dataclass
class Effect(object):
	x_D986CE26: str = field(default="")
	sEffectName: int = field(default=0)

@dataclass
class Entity(object):
	hidName: str = field(default="")
	disEntityId: int = field(default=0)
	x_D2B3429E: str = field(default="")
	hidEntityClass: int = field(default=0)
	hidResourceCount: int = field(default=0)
	hidPos: Vector3 = field(default_factory=Vector3)
	hidAngles: Vector3 = field(default_factory=Vector3)
	hidPos_precise: Vector3 = field(default_factory=Vector3)
	hidConstEntity: bool = field(default=False)

@dataclass
class Components(object): pass

@dataclass
class enum_(object):
	Value: str = field(default="")

@dataclass
class enumAnimalSpecies(object): pass

@dataclass
class World(object):
	Objective: bytes = field(default_factory=bytes)
	PGP: bytes = field(default_factory=bytes)
	SafeHouse: bytes = field(default_factory=bytes)
	CellTower: bytes = field(default_factory=bytes)

@dataclass
class Area(object): pass

@dataclass
class KeyLocation(object): pass

@dataclass
class StimsToExplode(object): pass

@dataclass
class _Stim(object):
	bPierceStim: bool = field(default=False)
	bCrushStim: bool = field(default=False)
	bBurnStim: bool = field(default=False)
	fBulletImpulseScale: float = field(default=0.0)
	fExplosionImpulseScale: float = field(default=0.0)
	selType: int = field(default=0)
	selStimType: int = field(default=0)
	hidEventName: str = field(default="")
	eventMask: int = field(default=0)
	hidTargetEntityId: int = field(default=0)
	x_FC25E1F1: str = field(default="")
	sDetail: int = field(default=0)
	nLevel: int = field(default=0)
	fRadius: float = field(default=0.0)
	bFalloff: bool = field(default=False)
	nFalloffMinLevel: int = field(default=0)
	hidShowType: bytes = field(default_factory=bytes)
	hidShowRadius: bytes = field(default_factory=bytes)
	fPhysImpulse: float = field(default=0.0)

@dataclass
class Stim_ImpactDamage(_Stim): pass

@dataclass
class Stim(_Stim): pass

@dataclass
class enumStimType(object): pass

@dataclass
class enumType(object): pass

@dataclass
class Sound(object):
	sndActive: str = field(default="")
	sndtpActive: int = field(default=0)
	x_EDFBC3D2: str = field(default="")
	matimpExplosionFx: int = field(default=0)

@dataclass
class DustBlast(object):
	sndLeftSound: str = field(default="")
	sndRightSound: str = field(default="")
	sndtpSoundType: int = field(default=0)
	fDistance: float = field(default=0.0)
	fSoundDuration: float = field(default=0.0)

@dataclass
class ExplosionSound(object):
	sndExplosionSound: str = field(default="")
	sndtpExplosionSoundType: int = field(default=0)
	sndLandSoundStart: str = field(default="")
	sndLandSoundStop: str = field(default="")
	sndtpLandSoundType: int = field(default=0)

@dataclass
class enumDelayExplodeType(object): pass

@dataclass
class CameraShakeAndRumble(object): pass

@dataclass
class ExplodeStims(object): pass

@dataclass
class Stims(object): pass

@dataclass
class RemainStims(object): pass

@dataclass
class Particles(object):
	x_6D980293: str = field(default="")
	psTrail: int = field(default=0)
	x_38680A74: str = field(default="")
	psLand: int = field(default=0)
	x_8DF2AAC6: str = field(default="")
	psExplosion: int = field(default=0)
	x_8E6613D3: str = field(default="")
	psExplosionUnderwater: int = field(default=0)
	x_BC45A121: str = field(default="")
	psRemains: int = field(default=0)

@dataclass
class Light(object): pass

@dataclass
class LightExplosion(object): pass

@dataclass
class LightRemains(object): pass

@dataclass
class Stages(object):
	fMaxDistance: float = field(default=0.0)
	fMaxLifeTime: float = field(default=0.0)

@dataclass
class Malfunction(object):
	fFireSpeed: float = field(default=0.0)
	fFireTime: float = field(default=0.0)
	fGravity: float = field(default=0.0)
	fMalfunctionInAirProbability: float = field(default=0.0)

@dataclass
class enumCategory(object): pass

@dataclass
class MuzzleStims(object): pass

@dataclass
class WeaponStims(object): pass

@dataclass
class ImpactStims(object): pass

@dataclass
class VictimStims(object): pass

@dataclass
class _Stage(object):
	fImpulse: float = field(default=0.0)
	fTime: float = field(default=0.0)
	fSpeed: float = field(default=0.0)
	fGravity: float = field(default=0.0)
	fMinTimeSpinning: float = field(default=0.0)
	fMaxTimeSpinning: float = field(default=0.0)
	fTimeStartSpinOnAir: float = field(default=0.0)
	fForce: float = field(default=0.0)
	fOnAirTurnSpeed: float = field(default=0.0)
	fSpinChangeDestTime: float = field(default=0.0)
	vectorPropellerStartPoint: Vector3 = field(default_factory=Vector3)
	vectorPropellerEndPoint: Vector3 = field(default_factory=Vector3)
	psStartPS: int = field(default=0)
	psLoopPS: int = field(default=0)
	sndStartSound: str = field(default="")
	sndtpStartSound: int = field(default=0)
	sndLoopSound: str = field(default="")
	sndtpLoopSound: int = field(default=0)
	sndLoopEndSound: str = field(default="")
	sndtpLoopEndSound: int = field(default=0)

@dataclass
class Fire(_Stage): pass

@dataclass
class Ignite(_Stage): pass

@dataclass
class Fall(_Stage): pass

@dataclass
class Spin(_Stage): pass

@dataclass
class FireStrategy(object): pass

@dataclass
class ReliabilityLevelsData(object): pass

@dataclass
class _ReliabilityLevelData(object):
	fHorizontalRecoilPerShot: float = field(default=0.0)
	fVerticalRecoilPerShot: float = field(default=0.0)
	fBulletDeviationMax: float = field(default=0.0)
	fJamProbabilityPerReload: float = field(default=0.0)

@dataclass
class Failure(_ReliabilityLevelData): pass

@dataclass
class Low(_ReliabilityLevelData): pass

@dataclass
class Medium(_ReliabilityLevelData): pass

@dataclass
class High(_ReliabilityLevelData): pass
@dataclass
class enumLevel(object): pass

@dataclass
class CommonProperties(object):
	sName: str = field(default="")
	sDisplayName: str = field(default="")
	fReloadTime: float = field(default=0.0)
	bAutoReload: bool = field(default=False)
	bIsSilent: bool = field(default=False)
	bVisibleHolstered: bool = field(default=False)
	bEmitLight: bool = field(default=False)
	selReloadType: int = field(default=0)
	selWeaponClass: int = field(default=0)
	selFireStrategy: int = field(default=0)
	selReticleType: int = field(default=0)
	crosshairMagmaAreaName: str = field(default="")
	iBaseAccuracyLevel: int = field(default=0)
	fRange: float = field(default=0.0)
	vectorEffectiveRange: Vector2 = field(default_factory=Vector2)
	vectorEffectiveRangeIS: Vector2 = field(default_factory=Vector2)
	fUnjamTime: float = field(default=0.0)
	selJamType: int = field(default=0)
	iClipsForSelfDestruct: int = field(default=0)
	bIsIndestructible: bool = field(default=False)
	bIsBreakable: bool = field(default=False)
	fLookSensitivityFactor: float = field(default=0.0)
	fMoveSpeedFactor: float = field(default=0.0)
	fForcedReliability: float = field(default=0.0)
	fInitialJamCounter: float = field(default=0.0)
	archPickupArchetype: str = field(default="")
	fShootingAngle: float = field(default=0.0)
	fShootingIronsightAngle: float = field(default=0.0)
	bSingleHitHealthFailure: bool = field(default=False)
	fHealthFailureChanceModifier: float = field(default=0.0)
	selHitLocation_Torso_Severity: int = field(default=0)
	selHitLocation_Limb_Severity: int = field(default=0)
	selCategory: int = field(default=0)
	x_E0FF29E0: str = field(default="")
	HolsterHandle: int = field(default=0)
@dataclass
class enumReloadType(object): pass

@dataclass
class enumWeaponClass(object): pass

@dataclass
class enumFireStrategy(object): pass

@dataclass
class enumReticleType(object): pass

@dataclass
class FireRate(object):
	fBusyDuration: float = field(default=0.0)
	iFireRate: float = field(default=0.0)
	selFireRateMode: int = field(default=0)

@dataclass
class enumFireRateMode(object): pass

@dataclass
class FireStrategyProperties(object):
	x_A58AA772: str = field(default="")
	StartBone: int = field(default=0)
	fConsumeAmmoRate: float = field(default=0.0)
	bUseAngleSpread: bool = field(default=False)
	iBulletsShot: int = field(default=0)
	iBurstLength: int = field(default=0)
	fAngleYawBulletSpread: float = field(default=0.0)
	fAnglePitchBulletSpread: float = field(default=0.0)
	bHasMuzzleLight: bool = field(default=False)
	x_F8F5F0F8: str = field(default="")
	matimpShellImpactFx: int = field(default=0)
	x_EB8DE264: str = field(default="")
	matimpBulletImpactFx: int = field(default=0)
	x_74A94828: str = field(default="")
	matimpSecondaryBulletImpactFx: int = field(default=0)
	archProjectileArchetype: str = field(default="")
	fInitialImpulse: float = field(default=0.0)
	fMalfunctionImpulse: float = field(default=0.0)
	fMalfunctionDetonateAfterHit: float = field(default=0.0)
	bActivateOnLaunch: bool = field(default=False)
	bProjectileBoundOnWeapon: bool = field(default=False)
	x_16E19113: str = field(default="")
	sShootBone: int = field(default=0)
	sndMalfunctionLoopSound: str = field(default="")
	sndtpMalfunctionLoopSound: int = field(default=0)
	sndMalfunctionEndLoopSound: str = field(default="")
	sndtpMalfunctionEndLoopSound: int = field(default=0)
	sndMalfunctionLoopTPSound: str = field(default="")
	sndtpMalfunctionLoopTPSound: int = field(default=0)
	sndMalfunctionEndTPLoopSound: str = field(default="")
	sndtpMalfunctionEndTPLoopSound: int = field(default=0)
	bRotateBaril: bool = field(default=False)

@dataclass
class Network(object):
	strControllerNetobjectType: str = field(default="")

@dataclass
class FuelGauge(object):
	x_F7A0C8D5: str = field(default="")
	sNeedleBone: int = field(default=0)
	fNeedleMaxRotationInDegrees: float = field(default=0.0)

@dataclass
class FlameMesh(object):
	fSize: float = field(default=0.0)
	fSplineTension: float = field(default=0.0)
	fSplineContinuity: float = field(default=0.0)
	fSplineBias: float = field(default=0.0)
	fPSSpawnTime: float = field(default=0.0)
	archSpawnTimeAngularSpeedRatioCurve: str = field(default="")
	fSegmentLength: float = field(default=0.0)
	fRestitutionInterpolationDist: float = field(default=0.0)
	fSizeGrowInterpolationDist: float = field(default=0.0)
	fSizeShrinkInterpolationDist: float = field(default=0.0)
	fGravityScalePlayerPitch: float = field(default=0.0)
	fGravityInterpolationDist: float = field(default=0.0)
	iRingNVertex: float = field(default=0.0)
	fRingStartAngle: float = field(default=0.0)
	fTeselation: float = field(default=0.0)
	fSpeed: float = field(default=0.0)
	bInterpolate: bool = field(default=False)
	x_93D2AFB5: str = field(default="")
	psParticleSystem: int = field(default=0)
	x_3924E150: str = field(default="")
	texTexture: int = field(default=0)
	fTextureFrames: float = field(default=0.0)
	fTextureChangeTime: float = field(default=0.0)

@dataclass
class Sounds(object):
	sndPickupGrabSound_1st: str = field(default="")
	sndtpPickupGrabSoundType_1st: int = field(default=0)
	sndPickupGrabSound_3rd: str = field(default="")
	sndtpPickupGrabSoundType_3rd: int = field(default=0)
	sndPickupEquipSound_1st: str = field(default="")
	sndtpPickupEquipSoundType_1st: int = field(default=0)
	sndPickupEquipSound_3rd: str = field(default="")
	sndtpPickupEquipSoundType_3rd: int = field(default=0)

@dataclass
class SoundsWeapon(object):
	sndPickAmmo: str = field(default="")
	sndtpPickAmmoSoundType: int = field(default=0)

# Why isn't this part of CCurve
@dataclass
class curveCurve(object):
	hidNumKnots: int = field(default=0)
	@dataclass
	class Knots(object):
		@dataclass
		class Knot(object):
			Value: Vector4 = field(default_factory=Vector4)
			Info: Vector4 = field(default_factory=Vector4)
			Type: int = field(default=0)

#endregion

#region Native classes

@dataclass
class CAABBPartitionManager(CSingletonEntity): pass
forward(CAABBPartitionManager, CSingletonEntity)
@dataclass
class CAccountService(IGameModeService): pass
@dataclass
class CAction(CTask): pass
@dataclass
class CAddSEFactEvent(CEntityEvent): pass

@dataclass
class CAgent(CAIObject):
	x_24B313D8: str = field(default="")
	Brain: bytes = field(default_factory=bytes)
	x_071B548C: str = field(default="")
	aiwsBrainWorkspace: int = field(default=0)

	@dataclass
	class PersonalityComponent(object):
		x_2B928622: str = field(default="")
		Type: int = field(default=0)

@dataclass
class CAgentAction(CAction): pass
@dataclass
class CAgentDecision(CDecision): pass
@dataclass
class CAgentScanner(CScanner): pass
@dataclass
class CAIAlertedNearby(CEntityEvent): pass
@dataclass
class CAIBuilding(CGameAIObject): pass

@dataclass
class CAIComponent(CEntityComponent):
	x_2B928622: str = field(default="")
	Type: int = field(default=0)
	@dataclass
	class AIObject(object):
		@dataclass
		class DensityManagement(object):
			bNeverDelete: bool = field(default=False)
			bLastToBeDeleted: bool = field(default=False)

@dataclass
class CAIMountedWeapon(CGameAIObject): pass

@dataclass
class CAIObject(CAIObjectRoot):
	# AIObjectID
	pass

@dataclass
class CAIObjectRoot(CNomadObject): pass
@dataclass
class CAIOcclusionVolumeComponent(CEntityComponent): pass
@dataclass
class CAIShootMeEvent(CEntityEvent): pass
@dataclass
class CAIShootMeObject(CEntityComponent): pass
@dataclass
class CAISoundAndFXComponent(CPawnSoundAndFXComponent): pass
@dataclass
class CAIToggleNavmeshComponent(CEntityComponent): pass
@dataclass
class CAIWorkspaceResource(CResourceContainer): pass
@dataclass
class CAIWorld(CCollective): pass
@dataclass
class CAlwaysLoaded(CSingletonEntity): pass
forward(CAlwaysLoaded, CSingletonEntity)
@dataclass
class CAmbxComponent(CEntityComponent): pass

@dataclass
class CAnimal(CGameObject):
	x_4E784950: str = field(default="")

@dataclass
class CAnimalAgent(CGameAgent): pass
@dataclass
class CAnimalBeautifierSelector(CEntityComponent): pass
@dataclass
class CAnimalPersonality(CLivingCreature): pass

class CAnimationComponent(CEntityComponent):
	x_F9F2D5F4: str = field(default="")
	fileSkeleton: int = field(default=0)
	x_E0AAD6E5: str = field(default="")
	fileFacialFile: int = field(default=0)

	@dataclass
	class MercKitFacialFiles(object):
		@dataclass
		class Faces(object):
			x_0AF17627: str = field(default="")
			sHeadTag: int = field(default=0)
			x_89CE658A: str = field(default="")
			fileFacialActor: int = field(default=0)

@dataclass
class CAnimationPackageResource(CResourceContainer): pass
@dataclass
class CAnimationResource(CResourceContainer): pass
@dataclass
class CAnimFacialEvent(CEntityEvent): pass
@dataclass
class CAnimFacialPoseEvent(CEntityEvent): pass
@dataclass
class CAnimPoseEvent(CEntityEvent): pass
@dataclass
class CAntiPortalConfig(CRenderBaseConfig): pass
@dataclass
class CArchiveFile(IFile): pass
@dataclass
class CArmy(CCollective): pass
@dataclass
class CAuthorizationService(IAuthorizationService): pass
@dataclass
class CBargeDelimiter(CGameAIObject): pass
@dataclass
class CBarkManagerService(IGameModeService): pass
@dataclass
class CBarkResourceContainer(CResourceContainer): pass
@dataclass
class CBaseEvent(CNomadObject): pass
@dataclass
class CBaseGraphicComponent(CRenderableComponent): pass
@dataclass
class CBaseMission(CNomadObject): pass
@dataclass
class CBaseSessionParam(COperationData): pass
@dataclass
class CBaseTriggerComponent(CEntityComponent): pass
@dataclass
class CBasicShapeEntity(IShapeEntity): pass
@dataclass
class CBasicRegionEntity(CBasicShapeEntity): pass
@dataclass
class CBasicShapeComponent(IShapeComponent): pass
@dataclass
class CBazaarComputer(CEntityComponent): pass
@dataclass
class CBeautifierRepository(CEntityComponent): pass
@dataclass
class CBedroll(CEntityComponent): pass
@dataclass
class CBinaryResource(CResource): pass
@dataclass
class CBindingComponent(CEntityComponent): pass
@dataclass
class CBinkResource(CResource): pass
@dataclass
class CBlueprintDecision(CDecision): pass
@dataclass
class CBoidsComponent(CEntityComponent): pass
@dataclass
class CBonusService(IGameModeService): pass
@dataclass
class CBonusServiceMP(CBonusService): pass
@dataclass
class CBoundaryRegion(CBasicRegionEntity): pass
@dataclass
class CBrain(CPlan): pass
@dataclass
class CBrainAnimal(CBrain): pass
@dataclass
class CBrainAnimalAlert(CBrain): pass
@dataclass
class CBrainAnimalIdle(CBrain):	pass
@dataclass
class CBrainBlackboardSelector(CBrain): pass
@dataclass
class CBrainBoat(CBrain): pass
@dataclass
class CBrainBuddyBase(CBrain): pass
@dataclass
class CBrainDomino(CBrain): pass
@dataclass
class CBrainDrone(CBrain): pass
@dataclass
class CBrainLayeredPatrol(CBrain): pass
@dataclass
class CBrainMerc(CBrain): pass
@dataclass
class CBrainMercAlert(CBrain): pass
@dataclass
class CBrainMercCombat(CBrain): pass
@dataclass
class CBrainMercDead(CBrain): pass
@dataclass
class CBrainMercIdle(CBrain): pass
@dataclass
class CBrainMercSocial(CBrain): pass
@dataclass
class CBrainMercSocialBehavior(CBrain): pass
@dataclass
class CBrainMercSpecial(CBrain): pass
@dataclass
class CBrainMercThreshold(CBrain): pass
@dataclass
class CBrainMercThresholdHealthRescuer(CBrain): pass
@dataclass
class CBrainMercThresholdHealthVictim(CBrain): pass
@dataclass
class CBrainMercVehicle(CBrain): pass
@dataclass
class CBrainRescueBuddy(CBrain): pass
@dataclass
class CBrainSimple(CBrain): pass
@dataclass
class CBrainSmartTerrain(CBrain): pass
@dataclass
class CBrainSpecialCharacter(CBrain): pass
@dataclass
class CBrainStoopidMerc(CBrain): pass
@dataclass
class CBrainVehicle(CBrain): pass
@dataclass
class CBrainVehicleCombat(CBrain): pass
@dataclass
class CBranchPathFollower(CRandomPathFollower): pass
@dataclass
class CBuddiesManager(IGameModeService): pass
@dataclass
class CBuddyDown(CNomadObject): pass
@dataclass
class CBuddyRescueEvent(CEntityEvent): pass
@dataclass
class CBuildingEvent(CEntityEvent): pass
@dataclass
class CBuildingInfoComponent(CEntityComponent): pass
@dataclass
class CBulletTracerManager(CSingletonEntity): pass
forward(CBulletTracerManager, CSingletonEntity)
@dataclass
class CBurnableRegion(CBasicShapeEntity): pass

@dataclass
class CCameraBoneComponent(CCameraComponent):
	x_920A6E7C: str = field(default="")
	Bone: int = field(default=0)
	Cinematic: bool = field(default=False)

@dataclass
class CCameraComponent(CEntityComponent):
	fCameraBlendTime: float = field(default=0.0)
	fNearDistance: float = field(default=0.0)
	fFarDistance: float = field(default=0.0)
	fFOV: float = field(default=0.0)
    # FocusEntityID
    # Active

@dataclass
class CCameraEditorComponent(CCameraComponent): pass

@dataclass
class CCameraFreeComponent(CCameraNetworkComponent):
	fSpeed: float = field(default=0.0)

@dataclass
class CCameraGameComponent(CCameraNetworkComponent): pass
@dataclass
class CCameraGhostComponent(CCameraFreeComponent): pass
@dataclass
class CCameraNetworkComponent(CCameraComponent): pass

@dataclass
class CCameraPawnComponent(CCameraGameComponent):
	x_920A6E7C: str = field(default="")
	Bone: int = field(default=0)
	DebugOffset: Vector3 = field(default_factory=Vector3)
    # NoiseFOVEnabled
    # NoiseFOVTimeCount
    # NoiseFOVTarget
    # NoiseFOVCurrent

@dataclass
class CCameraShakeAndPadRumbleComponent(CEntityComponent): pass
@dataclass
class CCameraShakeAndPadRumbleEvent(CEntityEvent): pass

@dataclass
class CCameraSpectatorComponent(CCameraNetworkComponent):
	fSpeed: float = field(default=0.0)
	fFastSpeed: float = field(default=0.0)
	fMaxHeight: float = field(default=0.0)

@dataclass
class CCameraThirdComponent(CCameraNetworkComponent):
	fDistance: float = field(default=0.0)

@dataclass
class CCampaignGameFile(CGameFile): pass
@dataclass
class CCampaignGameFileHeader(CGameFileHeader): pass
@dataclass
class CCanBeStabbed(CEntityEvent): pass
@dataclass
class CCapturePoint(CGameObject): pass
@dataclass
class CCapturePointNetworkComponent(CNetworkComponent): pass
@dataclass
class CChallenge(CNomadObject): pass
@dataclass
class CChallengeComponent(CEntityComponent): pass
@dataclass
class CChallengeProjectile(CChallenge): pass
@dataclass
class CChallengeWeapon(CChallenge): pass

@dataclass
class CPhysCharacterControllerStanceDimensions(object):
	vecStandCapsulePointA: Vector3 = field(default_factory=Vector3)
	vecStandCapsulePointB: Vector3 = field(default_factory=Vector3)
	fStandCapsuleRadius: float = field(default=0.0)

@dataclass
class CPhysCharacterControllerEntityCreateParams(object):
	fMass: float = field(default=0.0)
	bUpdateRotation: bool = field(default=False)
	bUseRigidBased: bool = field(default=False)
	fMaxSlope: float = field(default=0.0)
	fMaxTerrainSlope: float = field(default=0.0)

	@dataclass
	class StandDimensions(CPhysCharacterControllerStanceDimensions): pass
	@dataclass
	class CrouchDimensions(CPhysCharacterControllerStanceDimensions): pass
	@dataclass
	class SwimDimensions(CPhysCharacterControllerStanceDimensions): pass

@dataclass
class CCharacterPhysComponent(CPhysComponent):
	RagdollCollideSpeedLimit: float = field(default=0.0)
	x_041E4C28: str = field(default="")
	LockBone: int = field(default=0)

	@dataclass
	class CharacterParams(CPhysCharacterControllerEntityCreateParams): pass

@dataclass
class CCheckScoutEvent(CBaseEvent): pass
@dataclass
class CClientDescriptor(CNetDescriptor): pass
@dataclass
class CClientDescriptor_Agora(CClientDescriptor): pass
@dataclass
class CClientInfo_Agora(CClientInfo): pass
@dataclass
class CClusterComponent(CRenderableComponent): pass
@dataclass
class CCollectionComponent(CEntityComponent): pass
@dataclass
class CCollectionIgnitorComponent(ICollectionIgnitorComponent): pass
@dataclass
class CCollectionManager(CSingletonEntity): pass
forward(CCollectionManager, CSingletonEntity)
@dataclass
class CCollective(CAIObject): pass
@dataclass
class CCompassObjectives(CEntityComponent): pass
@dataclass
class CCompoundPhysChangeStateEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysComponent(CPhysComponent): pass
@dataclass
class CCompoundPhysComponentNode(CNomadObject): pass
@dataclass
class CCompoundPhysComponentBreakableNode(CCompoundPhysComponentNode): pass
@dataclass
class CCompoundPhysComponentListNode(CCompoundPhysComponentNode): pass
@dataclass
class CCompoundPhysComponentSingleBodyNode(CCompoundPhysComponentNode): pass
@dataclass
class CCompoundPhysComponentStateNode(CCompoundPhysComponentNode): pass
@dataclass
class CCompoundPhysDestroyEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysForceStateEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysNetworkComponent(CNetworkComponent): pass
@dataclass
class CCompoundPhysOnDamageEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysOnDamageLastStateEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysOnDamageStateChangeEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysOnDestroyEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysOnEventLastStateEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysOnPartBreakOffEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysOnPostStateChangeEvent(CEntityEvent): pass
@dataclass
class CCompoundPhysOnStateChangeEvent(CEntityEvent): pass
@dataclass
class CCompoundSetDamageableEvent(CGenericEntityEventbool): pass
@dataclass
class CConsoleService(IGameModeService): pass
@dataclass
class CConvoyMission(CGameAIObject): pass
@dataclass
class CCorpseComponent(CEntityComponent): pass
@dataclass
class CCounterEvent(CEntityEvent): pass

@dataclass
class CCountersComponent(CEntityComponent):
	archStimEffectTable: str = field(default="")

@dataclass
class CCountersComponentGO(CCountersComponent): pass
@dataclass
class CCounterThresholdCrossedEvent(CEntityEvent): pass
@dataclass
class CCounterTriggerComponent(CBaseTriggerComponent): pass
@dataclass
class CCreateGameSessionParam(COperationData): pass
@dataclass
class CCreateMatchMakingServiceOperation(CSessionOperation): pass
@dataclass
class CCreateNetObjectOperation(CNetObjectOperation): pass
@dataclass
class CCreateSessionParam(CBaseSessionParam): pass
@dataclass
class CCreatureSoundAndFXComponent(CEntityComponent): pass
@dataclass
class CCurve(CBaseEntity): pass
@dataclass
class CCurveObj(CNomadObject): pass
@dataclass
class CCustomMapGameFile(CGameFile): pass
@dataclass
class CCustomMaterialComponent(CEntityComponent): pass
@dataclass
class CDataBaseItemManager(CSingletonEntity): pass
forward(CDataBaseItemManager, CSingletonEntity)
@dataclass
class CDayCycleScale(CNomadObject): pass
@dataclass
class CDecision(CTask): pass
@dataclass
class CDecompressedArchiveFile(CMemoryStreamFile): pass
@dataclass
class CDelayTriggerComponent(CBaseTriggerComponent): pass
@dataclass
class CDeleteGameSessionParam(COperationData): pass
@dataclass
class CDeleteNetObjectOperation(CNetObjectOperation): pass
@dataclass
class CDeleteSessionParam(COperationData): pass
@dataclass
class CDemonwareLoginOperation(CLoginOperation): pass
@dataclass
class CDependenciesService(IGameModeService): pass
@dataclass
class CDestroyEvent(CEntityEvent): pass
@dataclass
class CDestructibleBridge(CEntityComponent): pass
@dataclass
class CDialogEvent(CSoundEvent): pass
@dataclass
class CDiamondPickedEvent(CEntityEvent): pass
@dataclass
class CDiamondsManager(CSingletonEntity): pass
forward(CDiamondsManager, CSingletonEntity)
@dataclass
class CDisableNavMeshVolumeEvent(CEntityEvent): pass
@dataclass
class CDispatcher(CAgent): pass
@dataclass
class CDispatcherConvoy(CDispatcher): pass
@dataclass
class CDispatcherSocial(CDispatcher): pass
@dataclass
class CDispatcherSquadLieutenant(CDispatcher): pass
@dataclass
class CDispatcherVehicle(CDispatcher): pass
@dataclass
class CDisplayApplyPopPup(CGameMessageBox): pass
@dataclass
class CDlcService(IGameModeService): pass
@dataclass
class CDMSpawnPointService(CSpawnPointService): pass
@dataclass
class CDominoBoxInstance(CResourceContainer): pass
@dataclass
class CDominoBoxResource(CResourceContainer): pass

@dataclass
class CDominoComponent(CEntityComponent):
	fileBoxPath: str = field(default="")
	hidStartOnLoad: bool = field(default=False)

@dataclass
class CDominoEvent(CEntityEvent): pass
@dataclass
class CDominoManager(CSingletonEntity): pass
forward(CDominoManager, CSingletonEntity)
@dataclass
class CDominoService(IGameModeService): pass
@dataclass
class CDoor(CEntityComponent): pass
@dataclass
class COnlineAdComponent(CEntityComponent): pass
@dataclass
class CDoubleFusionComponent(COnlineAdComponent): pass
@dataclass
class CDynamicDeploadComponent(CEntityComponent): pass
@dataclass
class CDynamicLightComponent(CEntityComponent): pass
@dataclass
class CDynLoadComponent(CEntityComponent): pass
@dataclass
class CEconomyComponent(CEntityComponent): pass
@dataclass
class CEditableEventComponent(CEntityComponent): pass
@dataclass
class CEnableBuddyDown(CEntityEvent): pass
@dataclass
class CEnableBuddyDownSuccess(CEntityEvent): pass
@dataclass
class CEnableNavMeshVolumeEvent(CEntityEvent): pass
@dataclass
class CEndOfGameLogosPage(CMenuPage): pass
@dataclass
class CEndOfGamePage(CMenuPage): pass

@dataclass
class CEntityComponent(CNomadObject):
	hidHasAliasName: bool = field(default=False)
	hidComponentClassName: str = field(default="")

@dataclass
class CEntityDieEvent(CEntityEvent): pass
@dataclass
class CEntityEvent(CBaseEvent): pass
@dataclass
class CEntityEventAddContainer(CEntityEvent): pass
@dataclass
class CEntityEventBlackboardUpdate(CEntityEvent): pass
@dataclass
class CEntityEventCanContain(CEntityEvent): pass
@dataclass
class CEntityEventGetAggressiveState(CEntityEvent): pass
@dataclass
class CEntityEventIsASpecialCharacter(CEntityEvent): pass
@dataclass
class CEntityEventIsUsable(CEntityEvent): pass
@dataclass
class CEntityEventOnUsed(CEntityEvent): pass
@dataclass
class CEntityEventOnUsing(CEntityEvent): pass
@dataclass
class CEntityEventStims(CEntityEvent): pass
@dataclass
class CEntitySpawner(CEntityComponent): pass
@dataclass
class CEntitySystemService(IGameModeService): pass
@dataclass
class CEntityUsableStateEvent(CEntityEvent): pass
@dataclass
class CEntranceInfoComponent(CEntityComponent): pass
@dataclass
class CEnvironmentAdaptiveBloom(CNomadObject): pass
@dataclass
class CEnvironmentAtmosphericScattering(CNomadObject): pass
@dataclass
class CEnvironmentCloud(CNomadObject): pass
@dataclass
class CEnvironmentDepthOfField(CNomadObject): pass
@dataclass
class CEnvironmentFog(CNomadObject): pass
@dataclass
class CEnvironmentLighting(CNomadObject): pass
@dataclass
class CEnvironmentSky(CNomadObject): pass
@dataclass
class CEnvironmentTransition(CNomadObject): pass
@dataclass
class CEnvironmentWeather(CNomadObject): pass
@dataclass
class CEnvironmentWind(CNomadObject): pass
@dataclass
class CEquipmentBase(CGameObject): pass
@dataclass
class CEquipmentUseStrategy(CNomadObject): pass
@dataclass
class CEventComponent(CEntityComponent): pass
@dataclass
class CEventDriveReportLostOccupant(CAIEvent): pass

@dataclass
class CExplosive(CGameObject):
	sUseString: str = field(default="")
	sCategory: str = field(default="")
	selDelayExplodeType: bytes = field(default_factory=bytes)
	fPenetrateDistance: float = field(default=0.0)
	fDelayRemoveAfterExplosion: float = field(default=0.0)
	fDelaySendStimsRemain: float = field(default=0.0)
	bApplyRemainStimsOnlyOnce: bool = field(default=False)
	fTimerSendRemainStims: float = field(default=0.0)
	fHealthFailureChanceModifier: float = field(default=0.0)
	ExplodeSendEvent: bytes = field(default_factory=bytes)
	bShouldExplodeUnderwater: bool = field(default=False)
	bShotJustMissedIsUsed: bool = field(default=False)
	fShotJustMissedDistance: float = field(default=0.0)
	archStickyFireFlame: bytes = field(default_factory=bytes)

@dataclass
class CExplosiveEvent(CEntityEvent): pass
@dataclass
class CExportWorldDependenciesEvent(CEntityEvent): pass
@dataclass
class CFaceActorResource(CResource): pass
@dataclass
class CFaceAnimResource(CResource): pass
@dataclass
class CFactAIObjectId(CBaseFact): pass
@dataclass
class CFactbool(CBaseFact): pass
@dataclass
class CFactCNoCaseStringID(CBaseFact): pass
@dataclass
class CFactCSmartPosition(CBaseFact): pass
@dataclass
class CFactCStringID(CBaseFact): pass
@dataclass
class CFactEAimStrategy(CBaseFact): pass
@dataclass
class CFactEEmotionStrategy(CBaseFact): pass
@dataclass
class CFactEFireRange(CBaseFact): pass
@dataclass
class CFactEFireStrategy(CBaseFact): pass
@dataclass
class CFactEIdleBehavior(CBaseFact): pass
@dataclass
class CFactELookStrategy(CBaseFact): pass
@dataclass
class CFactENeedType(CBaseFact): pass
@dataclass
class CFactEntityId(CBaseFact): pass
@dataclass
class CFactEOccupation(CBaseFact): pass
@dataclass
class CFactEPatrolType(CBaseFact): pass
@dataclass
class CFactESocialBehaviorType(CBaseFact): pass
@dataclass
class CFactESpecialStrategy(CBaseFact): pass
@dataclass
class CFactESpeed(CBaseFact): pass
@dataclass
class CFactfloat(CBaseFact): pass
@dataclass
class CFactndAngle3F(CBaseFact): pass
@dataclass
class CFactndQuat(CBaseFact): pass
@dataclass
class CFactndVec2(CBaseFact): pass
@dataclass
class CFactndVec3(CBaseFact): pass
@dataclass
class CFactsigned_int(CBaseFact): pass
@dataclass
class CFactsigned_long(CBaseFact): pass
@dataclass
class CFactunsigned_int(CBaseFact): pass
@dataclass
class CFactunsigned_long_long(CBaseFact): pass
@dataclass
class CFactunsigned_long(CBaseFact): pass
@dataclass
class CFakeWeapon(CEntityComponent): pass
@dataclass
class CFanComponent(CEntityComponent): pass
@dataclass
class CFCXActivatePresenceOperation(CFCXGameOperation): pass
@dataclass
class CFCXAIBehaviorService(IGameModeService): pass
@dataclass
class CFCXAIComponent(CAIComponent): pass
@dataclass
class CFcxAIEventDesertChange(CEntityEvent): pass
@dataclass
class CFCXAIEventMercDied(CEntityEvent): pass
@dataclass
class CFCXAntiCheatService(IGameModeService): pass
@dataclass
class CFCXArbitrationEnd(CFCXGameOperation): pass
@dataclass
class CFCXArbitrationStart(CFCXGameOperation): pass
@dataclass
class CFCXArbitrationStartResult(CFCXGameOperation): pass
@dataclass
class CFCXBarkManagerService(CBarkManagerService): pass
@dataclass
class CFCXBaseOptionPage(CSettingsPage): pass
@dataclass
class CFCXBenchmarkService(IGameModeService): pass
@dataclass
class CFCXBrightnessPage(CMenuPage): pass
@dataclass
class CFCXClassService(IGameModeService): pass
@dataclass
class CFCXClearMessageBoxManager(CFCXGameOperation): pass
@dataclass
class CFCXCompassObjectives(CCompassObjectives): pass
@dataclass
class CFCXConsoleService(CConsoleService): pass
@dataclass
class CFCXControllerOptionPage(CListMenuPage): pass
@dataclass
class CFCXCountersComponent(CCountersComponentGO): pass
@dataclass
class CFCXCountersComponentAI(CFCXCountersComponent): pass
@dataclass
class CFCXCountersComponentAIBuddy(CFCXCountersComponentAI): pass
@dataclass
class CFCXCountersComponentAnimal(CFCXCountersComponent): pass
@dataclass
class CFCXCountersComponentPlayer(CFCXCountersComponent): pass
@dataclass
class CFCXCountersComponentPlayerMP(CFCXCountersComponentPlayer): pass
@dataclass
class CFCXCountersComponentPlayerSP(CFCXCountersComponentPlayer): pass
@dataclass
class CFCXCountersService(ICountersService): pass
@dataclass
class CFCXCreateGameModeOperation(CGameOperationContainer): pass
@dataclass
class CFCXCreateSessionOpCtn(CGameOperationContainer): pass
@dataclass
class CFCXCreateSessionOperation(CFCXGameOperation): pass
@dataclass
class CFCXCustomMapDownloadService(IGameModeService): pass
@dataclass
class CFCXCustomMapService(IGameModeService): pass
@dataclass
class CFCXDeleteGameModeOperation(CFCXGameOperation): pass
@dataclass
class CFCXDeleteSessionOpCtn(CGameOperationContainer): pass
@dataclass
class CFCXDeleteSessionOperation(CFCXGameOperation): pass
@dataclass
class CFCXDifficultyPage(CListMenuPage): pass
@dataclass
class CFCXDLCPage(CListMenuPage): pass
@dataclass
class CFCXDMSpawnPointService(CDMSpawnPointService): pass
@dataclass
class CFCXDownloadCustomMapOperation(CFCXGameOperation): pass
@dataclass
class CFCXDuniaPage(CFCXMoviePage): pass
@dataclass
class CFCXEditorConfigService(IGameModeService): pass
@dataclass
class CFCXEditorGameFilesService(CGameFilesService): pass
@dataclass
class CFCXEditorUiService(CFCXUiService): pass
@dataclass
class CFCXEndSession(CFCXGameOperation): pass
@dataclass
class CFCXEnumerateCustomMapsOperation(CFCXGameOperation): pass
@dataclass
class CFCXRatingPage(CFCXMoviePage): pass
@dataclass
class CFCXESRBRatingPage(CFCXRatingPage): pass
@dataclass
class CFCXExclusiveContentMenuPage(CListMenuPage): pass
@dataclass
class CFCXGameMessageParser_Rank(CGameMessageParser): pass
@dataclass
class CFCXGameMessageParser_RankText(CFCXGameMessageParser_Rank): pass
@dataclass
class CFCXGameMessageParser_RankTitle(CFCXGameMessageParser_Rank): pass
@dataclass
class CFCXGameMessageService(CGameMessageService): pass
@dataclass
class CFCXGameModeChange(CFCXGameOperation): pass
@dataclass
class CFCXGameModeInitNetworkOperation(CFCXGameOperation): pass
@dataclass
class CFCXGameModeParamNode(CGameModeParamNode): pass
@dataclass
class CFCXGameModeShutdownNetworkOperation(CFCXGameOperation): pass
@dataclass
class CFCXGameModeSingle(CGameModeSingle): pass
@dataclass
class CFCXGameOperation(CGameOperation): pass
@dataclass
class CFCXGameplayManager(CGameplayManager): pass
@dataclass
class CFCXGameSettingsService(CGameSettingsService): pass
@dataclass
class CFCXGameSoundService(CGameSoundService): pass
@dataclass
class CFCXGameStartOperation(CFCXGameOperation): pass
@dataclass
class CFCXGameStatsSynchronize(CFCXGameOperation): pass
@dataclass
class CFCXGOBuilderBenchmark(CFCXGOBuilderCommon): pass
@dataclass
class CFCXGOBuilderCommon(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderConsole(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderEditor(CFCXGOBuilderCommon): pass
@dataclass
class CFCXGOBuilderInGameConsole(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMainMenu(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMultiCreateMatch(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMultiEndMatch(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMultiJoinMatch(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMultiNextRankedMatch(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMultiSetupNextRankedMatch(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMultiStartMatch(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderMultiUpdateMatch(CGameOperationBuilder): pass
@dataclass
class CFCXGOBuilderSingle(CFCXGOBuilderCommon): pass
@dataclass
class CFCXGOBuilderSingleLoad(CFCXGOBuilderSingle): pass
@dataclass
class CFCXGOCustomGroupNode(CGOCustomGroupNode): pass
@dataclass
class CFCXGOMainMenuNode(CFCXGOCustomGroupNode): pass
@dataclass
class CFCXGOSetUpdateFlags(CGameOperation): pass
@dataclass
class CFCXGOSingleMatchNode(CFCXGOCustomGroupNode): pass
@dataclass
class CFCXGRStateLoadPlayer(CGRStateLoadPlayer): pass
@dataclass
class CFCXGRStateMain(CGRStateMenu): pass
@dataclass
class CFCXGRStateMultiMenu(CGRStateMenu): pass
@dataclass
class CFCXGRStateSingleInGame(CGRStateSingle): pass
@dataclass
class CFCXGRStateSinglePreGame(CGRStateLoad): pass
@dataclass
class CFCXHudService(CHudService): pass
@dataclass
class CFCXInitializeTerminalsOperation(CFCXGameOperation): pass
@dataclass
class CFCXInitNatTraversal(CFCXGameOperation): pass
@dataclass
class CFCXInteractionUIService(IGameModeService): pass
@dataclass
class CFCXJoinSessionOpCtn(CGameOperationContainer): pass
@dataclass
class CFCXJoinSessionOperation(CFCXGameOperation): pass
@dataclass
class CFCXKeyboardControllerOptionPage(CMenuPage): pass
@dataclass
class CFCXLeaderboardSubmitStats(CFCXGameOperation): pass
@dataclass
class CFCXLoadCustomMapGameFileOperation(CFCXGameOperation): pass
@dataclass
class CFCXLoadGameOperation(CFCXGameOperation): pass
@dataclass
class CFCXLoadGamePage(CLoadGamePage): pass
@dataclass
class CFCXLoadGameStartOperation(CFCXGameOperation): pass
@dataclass
class CFCXLoadMessageBoxPackage(CFCXGameOperation): pass
@dataclass
class CFCXLoadOutService(IGameModeService): pass
@dataclass
class CFCXLoadWorldOp(CFCXGameOperation): pass
@dataclass
class CFCXLoadWorldOperation(CGameOperationContainer): pass
@dataclass
class CFCXLoadWorldSynchOp(CFCXGameOperation): pass
@dataclass
class CFCXLobbyService(CLobbyService): pass
@dataclass
class CFCXLoginOperation(CFCXGameOperation): pass
@dataclass
class CFCXLogoutOperation(CFCXGameOperation): pass
@dataclass
class CFCXMainCreditsPage(CMenuPage): pass
@dataclass
class CFCXMainMenu(CGameMode): pass
@dataclass
class CFCXMainPage(CListMenuPage): pass
@dataclass
class CFCXMapListPopup(CGameMessageBox): pass
@dataclass
class CFCXMapProgressPage(CMenuPage): pass
@dataclass
class CFCXMapService(CMapService): pass
@dataclass
class CFCXMatchService(CMatchService): pass
@dataclass
class CFCXMissionManager(IGameModeService): pass
@dataclass
class CFCXMoviePage(CMenuPage): pass
@dataclass
class CFCXMultiBaseMapRotationPage(CMenuPage): pass
@dataclass
class CFCXMultiCreateHostPage(CFCXMultiMatchOptionsPage): pass
@dataclass
class CFCXMultiCreateMapRotationPage(CFCXMultiBaseMapRotationPage): pass
@dataclass
class CFCXMultiCreateMatchAdvancedOptionsPage(CFCXMultiMatchOptionsPage): pass
@dataclass
class CFCXMultiCreateMatchAdvancedOptionsPageUnranked(CFCXMultiCreateMatchAdvancedOptionsPage): pass
@dataclass
class CFCXMultiCreateMatchAdvancedOptionsPageOffline(CFCXMultiCreateMatchAdvancedOptionsPageUnranked): pass
@dataclass
class CFCXMultiCreateMatchAdvancedOptionsMenuPageOffline(CFCXMultiCreateMatchAdvancedOptionsPageOffline): pass
@dataclass
class CFCXMultiCreateMatchAdvancedOptionsPageRanked(CFCXMultiCreateMatchAdvancedOptionsPage): pass
@dataclass
class CFCXMultiCreateMatchAdvancedOptionsMenuPageRanked(CFCXMultiCreateMatchAdvancedOptionsPageRanked): pass
@dataclass
class CFCXMultiCreateMatchAdvancedOptionsMenuPageUnranked(CFCXMultiCreateMatchAdvancedOptionsPageUnranked): pass
@dataclass
class CFCXMultiCreateMatchPage(CFCXMultiCreateHostPage): pass
@dataclass
class CFCXMultiCreateOfflineProfilePage(CMenuPage): pass
@dataclass
class CFCXMultiCreateOnlineProfilePage(CMenuPage): pass
@dataclass
class CFCXMultiCustomBrowserPage(CFCXMultiMatchBrowserPage): pass
@dataclass
class CFCXMultiCustomCreatePage(CFCXMultiCreateMatchPage): pass
@dataclass
class CFCXMultiCustomPage(CFCXMultiMainMatchBasePage): pass
@dataclass
class CFCXMultiEditProfilePage(CMenuPage): pass
@dataclass
class CFCXMultiEditOfflineProfilePage(CFCXMultiEditProfilePage): pass
@dataclass
class CFCXMultiEditOnlineProfilePage(CFCXMultiEditProfilePage): pass
@dataclass
class CFCXMultiEditorOnlinePage(CListMenuPage): pass
@dataclass
class CFCXMultiLANBrowserPage(CFCXMultiMatchBrowserPage): pass
@dataclass
class CFCXMultiLANCreatePage(CFCXMultiCreateMatchPage): pass
@dataclass
class CFCXMultiLANPage(CListMenuPage): pass
@dataclass
class CFCXMultiLeaderboardPage(CMenuPage): pass
@dataclass
class CFCXMultiLeaderboardTypesPage(CListMenuPage): pass
@dataclass
class CFCXMultiMainMatchBasePage(CListMenuPage): pass
@dataclass
class CFCXMultiMainPage(CListMenuPage): pass
@dataclass
class CFCXMultiMatchBrowserPage(CSettingsPage): pass
@dataclass
class CFCXMultiMatchOptionsPage(CSettingsPage): pass
@dataclass
class CFCXMultiOnlinePrivacyStatementPage(CMenuPage): pass
@dataclass
class CFCXMultiPlayerProfilePage(CMenuPage): pass
@dataclass
class CFCXMultiProfileTypePage(CListMenuPage): pass
@dataclass
class CFCXMultiRankedBrowserPage(CFCXMultiMatchBrowserPage): pass
@dataclass
class CFCXMultiRankedCreatePage(CFCXMultiCreateMatchPage): pass
@dataclass
class CFCXMultiRankedPage(CFCXMultiMainMatchBasePage): pass
@dataclass
class CFCXMultiRegisterOnlineProfilePage(CMenuPage): pass
@dataclass
class CFCXMultiSelectProfilePage(CMenuPage): pass
@dataclass
class CFCXMultiServerCustomPage(CSettingsPage): pass
@dataclass
class CFCXMultiServerMapInfoPage(CMenuPage): pass
@dataclass
class CFCXMultiServerDeleteMapInfoListPage(CFCXMultiServerMapInfoPage): pass
@dataclass
class CFCXMultiServerMapListPage(CMenuPage): pass
@dataclass
class CFCXMultiServerDeleteMapListPage(CFCXMultiServerMapListPage): pass
@dataclass
class CFCXMultiServerDownloadMapInfoListPage(CFCXMultiServerMapInfoPage): pass
@dataclass
class CFCXMultiServerDownloadMapListPage(CFCXMultiServerMapListPage): pass
@dataclass
class CFCXMultiServerOperationProgressPage(CMenuPage): pass
@dataclass
class CFCXMultiServerQuickSearchOptionsPage(CSettingsPage): pass
@dataclass
class CFCXMultiServerUploadMapInfoListPage(CFCXMultiServerMapInfoPage): pass
@dataclass
class CFCXMultiServerUploadMapListPage(CFCXMultiServerMapListPage): pass
@dataclass
class CFCXNetEngineIdleOperation(CFCXGameOperation): pass
@dataclass
class CFCXNetEngineShutdownOperation(CFCXGameOperation): pass
@dataclass
class CFCXNetEngineStartupOperation(CFCXGameOperation): pass
@dataclass
class CFCXNetGameCtrlOnEndMatchSync(CNetGameCtrlStateBaseSynchOp): pass
@dataclass
class CFCXNetGameCtrlOnStartMatchSync(CNetGameCtrlStateBaseSynchOp): pass
@dataclass
class CFCXOnlineMapService(IGameModeService): pass
@dataclass
class CFCXOnLoadWorldOp(CFCXGameOperation): pass
@dataclass
class CFCXOnPostLoadWorldOp(CFCXGameOperation): pass
@dataclass
class CFCXOnPreLoadWorldOp(CFCXGameOperation): pass
@dataclass
class CFCXOptionDisplayPage(CMenuPage): pass
@dataclass
class CFCXOptionGamePage(CFCXBaseOptionPage): pass
@dataclass
class CFCXOptionNetworkPage(CFCXBaseOptionPage): pass
@dataclass
class CFCXOptionPage(CListMenuPage): pass
@dataclass
class CFCXOptionSoundPage(CFCXBaseOptionPage): pass
@dataclass
class CFCXParticleAmbianceComponent(CParticleAmbianceComponent): pass
@dataclass
class CFCXPartnersPage(CFCXMoviePage): pass
@dataclass
class CFCXPauseBuddiesPage(CMenuPage): pass
@dataclass
class CFCXPauseGameStatsPage(CMenuPage): pass
@dataclass
class CFCXPauseJackalFilesPage(CMenuPage): pass
@dataclass
class CFCXPauseLegendPage(CMenuPage): pass
@dataclass
class CFCXPauseMenuPage(CListMenuPage): pass
@dataclass
class CFCXPauseMultiService(IGameModeService): pass
@dataclass
class CFCXPausePartnerFilesPage(CMenuPage): pass
@dataclass
class CFCXPausePlayerStatsPage(CMenuPage): pass
@dataclass
class CFCXPlayer(CPlayer): pass
@dataclass
class CFCXPlayerService(CPlayerService): pass
@dataclass
class CFCXPostGameModeChange(CFCXGameOperation): pass
@dataclass
class CFCXPostLoadWorldOp(CFCXGameOperation): pass
@dataclass
class CFCXPreLoadWorldOp(CFCXGameOperation): pass
@dataclass
class CFCXPrepareLoadingScreenOperation(CFCXGameOperation): pass
@dataclass
class CFCXPrepareRendererOperation(CFCXGameOperation): pass
@dataclass
class CFCXPrepareUnloadWorldOperation(CFCXGameOperation): pass
@dataclass
class CFCXPresentationPage(CFCXMoviePage): pass
@dataclass
class CFCXRankService(IGameModeService): pass
@dataclass
class CFCXRemoveEntityFromListOperation(CGameOperation): pass
@dataclass
class CFCXReputationPage(CMenuPage): pass
@dataclass
class CFCXRetrieveLeaderboardStatsOperation(CFCXGameOperation): pass
@dataclass
class CFCXRunBatchFileOperation(CGameOperation): pass
@dataclass
class CFCXRunEditor(CGameOperationContainer): pass
@dataclass
class CFCXScoreboardService(CScoreboardService): pass
@dataclass
class CFCXScoreboardServiceFFA(CFCXScoreboardService): pass
@dataclass
class CFCXScoreboardServiceTeam(CFCXScoreboardService): pass
@dataclass
class CFCXSearchSessionOperation(CFCXGameOperation): pass
@dataclass
class CFCXServeCustomMapOperation(CFCXGameOperation): pass
@dataclass
class CFCXSingleGameFilesService(CGameFilesService): pass
@dataclass
class CFCXSkipFramesOperation(CFCXGameOperation): pass
@dataclass
class CFCXSplashPage(CMenuPage): pass
@dataclass
class CFcxSpline(CNomadObject): pass
@dataclass
class CFcxSplineCollection(CNomadObject): pass
@dataclass
class CFcxSplineCollectionEntity(COmniMapEntity): pass
@dataclass
class CFCXStartCustomMapOperations(CGameOperationContainer): pass
@dataclass
class CFCXStartEditor(CGameOperationContainer): pass
@dataclass
class CFCXStartNetworkOperation(CFCXGameOperation): pass
@dataclass
class CFCXStartSession(CFCXGameOperation): pass
@dataclass
class CFCXStopCustomMapOperations(CGameOperationContainer): pass
@dataclass
class CFCXStopDownloadCustomMapOperation(CFCXGameOperation): pass
@dataclass
class CFCXStopEditor(CGameOperationContainer): pass
@dataclass
class CFCXStopServeCustomMapOperation(CFCXGameOperation): pass
@dataclass
class CFCXStoryAvatarSelectionPage(CMenuPage): pass
@dataclass
class CFCXStoryModePage(CListMenuPage): pass
@dataclass
class CFCXTDMSpawnPointService(CTDMSpawnPointService): pass
@dataclass
class CFCXTeleportEntityOperation(CFCXGameOperation): pass
@dataclass
class CFCXTrackingService(CTrackingService): pass
@dataclass
class CFCXUbisoftPage(CFCXMoviePage): pass
@dataclass
class CFCXUiService(IGameModeService): pass
@dataclass
class CFCXUnloadCustomMapGameFileOperation(CFCXGameOperation): pass
@dataclass
class CFCXUnloadLoadingScreenOperation(CFCXGameOperation): pass
@dataclass
class CFCXUnloadWorldOperation(CFCXGameOperation): pass
@dataclass
class CFCXWaitDownloadCustomMapOperation(CFCXGameOperation): pass
@dataclass
class CFCXWaitForEmptySessionOperation(CFCXGameOperation): pass

@dataclass
class CFCXWeapon(CWeapon):
	iAnimationValue: int = field(default=0)
	sndswtpWeaponStatusSoundSwitchType: int = field(default=0)
	WeaponStatusSwitchValues: bytes = field(default_factory=bytes)
	bUseHiResScope: bool = field(default=False)
	fHiResLowResScopeSwitchTransitionPoint: float = field(default=0.0)

@dataclass
class CFCXWeaponsService(CWeaponsService): pass
@dataclass
class CFCXWorldDemoManager(CSingletonEntity): pass
forward(CFCXWorldDemoManager, CSingletonEntity)
@dataclass
class CFetchPrivilegesOperation(CRendezVousOperation): pass

@dataclass
class CFileDescriptorComponent(CEntityComponent):
	x_2A7BCA49: str = field(default="")
	fileName: int = field(default=0)
	SerializationEvent: bytes = field(default_factory=bytes)
	hidDescriptor: bytes = field(default_factory=bytes)

@dataclass
class CFireComponent(CEntityComponent): pass
@dataclass
class CFireManager(CSingletonEntity): pass
forward(CFireManager, CSingletonEntity)
@dataclass
class CFireNode(CNomadObject): pass
@dataclass
class CFireObjectComponent(CFireComponent): pass
@dataclass
class CFireObjectNode(CFireNode): pass
@dataclass
class CFireRealtreeComponent(CFireComponent): pass
@dataclass
class CFireRealtreeElementComponent(CFireRealtreeComponent): pass
@dataclass
class CFireRealtreeNode(CFireNode): pass
@dataclass
class CFireRegionComponent(CFireComponent): pass
@dataclass
class CFireStickyStreamComponent(CFireComponent): pass
@dataclass
class CFireStickyStreamNetworkComponent(CNetworkComponent): pass
@dataclass
class CFireStickyStreamNode(CFireObjectNode): pass
@dataclass
class CFirstHitEvent(CEntityEvent): pass
@dataclass
class CFlag(CGameObject): pass
@dataclass
class CFlagNetworkComponent(CNetworkComponent): pass
@dataclass
class CFlagStation(CGameObject): pass
@dataclass
class CFlagStationNetworkComponent(CNetworkComponent): pass
@dataclass
class CFlare(CGameObject): pass
@dataclass
class CFlareExplosionEvent(CEntityEvent): pass

@dataclass
class CFrankensteinComponent(CEntityComponent):
    # ScriptEventOverrideID
    # Enable
    # LookatEntityTargetIds, TargetId, TargetId
	bCheatKnees: bool = field(default=False)

@dataclass
class CFrankensteinEvent(CEntityEvent): pass
@dataclass
class CFrankensteinPoseResource(CResourceContainer): pass
@dataclass
class CFriendListService(IGameModeService): pass
@dataclass
class CGadget(CEquipmentBase): pass
@dataclass
class CGadgetEventSetProjectileVelocity(CEntityEvent): pass
@dataclass
class CGadgetMapStrategy(CGadgetUseStrategy): pass
@dataclass
class CGadgetNetworkComponent(CNetworkComponent): pass
@dataclass
class CGadgetUseBinocularsStrategy(CGadgetUseStrategy): pass
@dataclass
class CGadgetUseCompassSingleStrategy(CGadgetUseStrategy): pass
@dataclass
class CGadgetUsePhoneStrategy(CGadgetUseStrategy): pass
@dataclass
class CGadgetUseStrategy(CEquipmentUseStrategy): pass
@dataclass
class CGadgetUseThrowStrategy(CGadgetUseStrategy): pass
@dataclass
class CGadgetUseWatchStrategy(CGadgetUseStrategy): pass

@dataclass
class CGameAgent(CAgent):
	FlagField: bytes = field(default_factory=bytes)
	bIsScripted: bool = field(default=False)
	fAccelerationsSlow: float = field(default=0.0)
	fAccelerationsNormal: float = field(default=0.0)
	fAccelerationsFast: float = field(default=0.0)
	fDecelerationsSlow: float = field(default=0.0)
	fDecelerationsNormal: float = field(default=0.0)
	fDecelerationsFast: float = field(default=0.0)
	fSpeedsBabyStep: float = field(default=0.0)
	fSpeedsWalk: float = field(default=0.0)
	fSpeedsJog: float = field(default=0.0)
	fSpeedsRun: float = field(default=0.0)
	fSpeedsSprint: float = field(default=0.0)
	fVariationBabyStep: float = field(default=0.0)
	fVariationWalk: float = field(default=0.0)
	fVariationJog: float = field(default=0.0)
	fVariationRun: float = field(default=0.0)
	fVariationSprint: float = field(default=0.0)
	JustStarted: bytes = field(default_factory=bytes)
	Destination: bytes = field(default_factory=bytes)
	PathInfos: bytes = field(default_factory=bytes)
	PatrolPathFollower: bytes = field(default_factory=bytes)
	DensityManagement: bytes = field(default_factory=bytes)
	bNeverDelete: bool = field(default=False)
	bLastToBeDeleted: bool = field(default=False)

@dataclass
class CGameAIObject(CAIObject): pass
@dataclass
class CGameConfig(CNomadConfigObject): pass
@dataclass
class CGameConnectOperation(CSessionOperation): pass
@dataclass
class CGameElementEntity(COmniMapEntity): pass
@dataclass
class CGameFilesListPage(CMenuPage): pass
@dataclass
class CGameFilesService(IGameModeService): pass
@dataclass
class CGameFireConfig(CNomadConfigObject): pass
@dataclass
class CGameMessageBox(CUIPageBase): pass
@dataclass
class CGameMessageBoxCustomPopUpTutorial(CGameMessageBoxPopUpTutorial): pass
@dataclass
class CGameMessageBoxDZMessage(CGameMessageBox): pass
@dataclass
class CGameMessageBoxEditBox(CGameMessageBox): pass
@dataclass
class CGameMessageBoxEvent(CGameMessageBox): pass
@dataclass
class CGameMessageBoxFloatingTutorial(CGameMessageBox): pass
@dataclass
class CGameMessageBoxList(CGameMessageBox): pass
@dataclass
class CGameMessageBoxListSingleButton(CGameMessageBox): pass
@dataclass
class CGameMessageBoxPasswordEditBox(CGameMessageBoxEditBox): pass
@dataclass
class CGameMessageBoxPopUpConfirmation(CGameMessageBox): pass
@dataclass
class CGameMessageBoxPopUpTutorial(CGameMessageBox): pass
@dataclass
class CGameMessageBoxQuickMatchStatus(CGameMessageBoxSpinner): pass
@dataclass
class CGameMessageBoxSpinner(CGameMessageBox): pass
@dataclass
class CGameMessageParser_BonusPlanGranted(CGameMessageParser): pass
@dataclass
class CGameMessageParser_BonusPlanGrantedText(CGameMessageParser_BonusPlanGranted): pass
@dataclass
class CGameMessageParser_BonusPlanGrantedTitle(CGameMessageParser_BonusPlanGranted): pass
@dataclass
class CGameMessageParser_Generic(CGameMessageParser): pass
@dataclass
class CGameMessageParser_JoinGame(CGameMessageParser): pass
@dataclass
class CGameMessageParser_LeftGame(CGameMessageParser): pass
@dataclass
class CGameMessageService(IGameMessageService): pass
@dataclass
class CGameMission(CBaseMission): pass
@dataclass
class CGameModeBaseParamNode(CNomadObject): pass
@dataclass
class CGameModeComponent(CEntityComponent): pass
@dataclass
class CGameModeEntity(COmniEntity): pass
@dataclass
class CGameModeParamNode(CGameModeBaseParamNode): pass
@dataclass
class CGameModeServiceEvent(CNomadObject): pass
@dataclass
class CGameModeServiceNetEngineEvent(CGameModeServiceEvent): pass
@dataclass
class CGameModeSingle(CGameMode): pass
@dataclass
class CGameObject(CEntityComponent): pass
@dataclass
class CGameOperationContainer(CGameOperation): pass
@dataclass
class CGameOperationSimpleBuilder(CGameOperationBuilder): pass
@dataclass
class CGameOpNode(CNomadObject): pass
@dataclass
class CGameOverLoadPage(CLoadGamePage): pass
@dataclass
class CGameOverPage(CListMenuPage): pass
@dataclass
class CGameplayManager(IGameModeService): pass
@dataclass
class CGameRegion(CBasicRegionEntity): pass
@dataclass
class CGameSettingsContainer(CGameSetting): pass
@dataclass
class CGameSettingsService(IGameModeService): pass
@dataclass
class CGameSoundService(IGameSoundService): pass
@dataclass
class CGameStatsService(IGameStatsService): pass
@dataclass
class CGameValueListSettingbool(CValueListSettingbool): pass
@dataclass
class CGameValueListSettingCryString(CValueListSettingCryString): pass
@dataclass
class CGameValueListSettingunsigned_long(CValueListSettingunsigned_long): pass
@dataclass
class CGenericEntityEventbool(CEntityEvent): pass
@dataclass
class CGenericEntityEventCTerminalPTR(CEntityEvent): pass
@dataclass
class CGenericUISettingbool(CUISettingBase): pass
@dataclass
class CGenericUISettingCMapCycle(CUISettingBase): pass
@dataclass
class CGenericUISettingCryString(CUISettingBase): pass
@dataclass
class CGenericUISettingunsigned_long(CUISettingBase): pass
@dataclass
class CGeometryResource(CResourceContainer): pass
@dataclass
class CGhostComponent(CEntityComponent): pass
@dataclass
class CGhostEntity(COmniMapEntity): pass
@dataclass
class CGhostEvent(CBaseEvent): pass
@dataclass
class CGOBuilderNode(CGameOpNode): pass
@dataclass
class CGOCreateMatchNode(CGOCustomGroupNode): pass
@dataclass
class CGOCriticalSectionEnd(CGameOperation): pass
@dataclass
class CGOCriticalSectionStart(CGameOperation): pass
@dataclass
class CGOCustomGroupNode(CGameOpNode): pass
@dataclass
class CGOExternalState(CGOState): pass
@dataclass
class CGOSMBarkEvent(CEntityEvent): pass
@dataclass
class CGOState(CNomadObject): pass
@dataclass
class CGOStateAnim(CGOState): pass
@dataclass
class CGOStateAnimRotation(CGOStateAnim): pass
@dataclass
class CGOStateApproachPosition(CGOStateAnim): pass
@dataclass
class CGOStateBriefing(CGOStateAnim): pass
@dataclass
class CGOStateBriefingReaction(CGOStateAnim): pass
@dataclass
class CGOStateContextCGameObject(IGOStateContext): pass
@dataclass
class CGOStateEquipment(CGOStateAnim): pass
@dataclass
class CGOStateEventAnimal(CGOStateEvent): pass
@dataclass
class CGOStateEventBark(CGOStateEvent): pass
@dataclass
class CGOStateEventBazaarComputer(CGOStateEvent): pass
@dataclass
class CGOStateEventBeautifier(CGOStateEvent): pass
@dataclass
class CGOStateEventBedroll(CGOStateEvent): pass
@dataclass
class CGOStateEventBuddyDown(CGOStateEvent): pass
@dataclass
class CGOStateEventCamera(CGOStateEvent): pass
@dataclass
class CGOStateEventCapturePoint(CGOStateEvent): pass
@dataclass
class CGOStateEventEquipment(CGOStateEvent): pass
@dataclass
class CGOStateEventFCXPawn(CGOStateEvent): pass
@dataclass
class CGOStateEventFCXUseEquipment(CGOStateEvent): pass
@dataclass
class CGOStateEventGameRules(CGOStateEvent): pass
@dataclass
class CGOStateEventHeal(CGOStateEvent): pass
@dataclass
class CGOStateEventInput(CGOStateEvent): pass
@dataclass
class CGOStateEventInventory(CGOStateEvent): pass
@dataclass
class CGOStateEventMovie(CGOStateEvent): pass
@dataclass
class CGOStateEventPawn(CGOStateEvent): pass
@dataclass
class CGOStateEventPickupDiamond(CGOStateEvent): pass
@dataclass
class CGOStateEventRescue(CGOStateEvent): pass
@dataclass
class CGOStateEventSM(CGOStateEvent): pass
@dataclass
class CGOStateEventSound(CGOStateEvent): pass
@dataclass
class CGOStateEventTakeFlag(CGOStateEvent): pass
@dataclass
class CGOStateEventVehicle(CGOStateEvent): pass
@dataclass
class CGOStateExitVehicle(CGOStateAnim): pass
@dataclass
class CGOStateGameSetting(CGOState): pass
@dataclass
class CGOStateLadderTransition(CGOStateAnim): pass
@dataclass
class CGOStateMachineTrack(CNomadObject): pass
@dataclass
class CGOStateSmartTerrain(CGOStateAnim): pass
@dataclass
class CGradientColor(CNomadObject): pass
@dataclass
class CGRAmmoPilesRespawn(CGRQueryParams): pass
@dataclass
class CGRAmmoPilesSpawnProjectiles(CGRQueryParams): pass
@dataclass
class CGraphicClusterComponent(CClusterComponent): pass

@dataclass
class CGraphicComponent(CBaseGraphicComponent):
	bCastShadow: bool = field(default=False)
	bReceiveShadow: bool = field(default=False)
	bCastAmbientShadow: bool = field(default=False)
	olgLightGroup: int = field(default=0)
	bAllowCullBySize: bool = field(default=False)
	agAmbientGroup: int = field(default=0)
	bBehaveLikeAPickup: bool = field(default=False)
	bShowInReflection: bool = field(default=False)
	bAlwaysShowInReflection: bool = field(default=False)
	bOverrideLODSphere: bool = field(default=False)
	fLODSphereRadius: float = field(default=0.0)
	hidSkyOcclusion0: int = field(default=0)
	hidSkyOcclusion1: int = field(default=0)
	hidSkyOcclusion2: int = field(default=0)
	hidSkyOcclusion3: int = field(default=0)
	hidGroundColor: int = field(default=0)
	hidObjectHeight: float = field(default=0.0)
	hidHeightAbove: bytes = field(default_factory=bytes)
	hidHasAmbientValues: bool = field(default=False)

	@dataclass
	class object_(object):
		hidIndex: int = field(default=0)
		x_BF9B3A5C: str = field(default="")
		objModel: int = field(default=0)
		hidMeshName: str = field(default="")
		x_E1A0EE56: str = field(default="")
		hidNodeName: int = field(default=0)
		x_0D9C8B1A: str = field(default="")
		hidNodeNameLOD0: int = field(default=0)
		hidDetailObject: bool = field(default=False)

@dataclass
class SPartOverwrite(object):
	x_CE56B704: str = field(default="")
	PartID: int = field(default=0)
	TextureIndex: int = field(default=0)
	ColorIndex: int = field(default=0)

@dataclass
class CGraphicKitComponent(CEntityComponent):
	bRadomize: bool = field(default=False)
	@dataclass
	class Tags(object):
		@dataclass
		class SpecializationTag(object):
			x_9B35862A: str = field(default="")
			sTag: int = field(default=0)
	@dataclass
	class PartOverwrite(object):
		@dataclass
		class ActivePartOverwrite(SPartOverwrite): pass

@dataclass
class CGrassDisplacementComponent(CEntityComponent): pass
@dataclass
class CGRCanBackstab(CGRQueryParams): pass
@dataclass
class CGRCanRemoveInventoryEntities(CGRQueryParams): pass
@dataclass
class CGRCanStab(CGRQueryParams): pass
@dataclass
class CGRDropInventoryWhenRagdoll(CGRQueryParams): pass
@dataclass
class CGREvent(CCommandCBParam): pass
@dataclass
class CGREventEndGameStatsReceived(CGREvent): pass
@dataclass
class CGREventOnEntityReady(CGREvent): pass
@dataclass
class CGREventOnPawnReady(CGREvent): pass
@dataclass
class CGRGenericEvent(CGREvent): pass
@dataclass
class CGRGenericEventWithParamEntityId(CGRGenericEvent): pass
@dataclass
class CGRGenericEventWithParamPlayerId(CGRGenericEvent): pass
@dataclass
class CGRQueryCanDoDamage(CGRQueryParams): pass
@dataclass
class CGRQueryCanJamEquipment(CGRQueryParams): pass
@dataclass
class CGRQueryCanModifyHealth(CGRQueryParams): pass
@dataclass
class CGRQueryCanRevive(CGRQueryParams): pass
@dataclass
class CGRQueryCanVoiceChat(CGRQueryParams): pass
@dataclass
class CGRQueryDisplayAccountErrors(CGRQueryParams): pass
@dataclass
class CGRQueryDisplayConfirmDestructiveAction(CGRQueryParams): pass
@dataclass
class CGRQueryGetMenuContext(CGRQueryParams): pass
@dataclass
class CGRQueryGetStateTimeLeft(CGRQueryParams): pass
@dataclass
class CGRQueryInMultiMenu(CGRQueryParams): pass
@dataclass
class CGRQueryIsGameInLobby(CGRQueryParams): pass
@dataclass
class CGRQueryIsGameInMainMenu(CGRQueryParams): pass
@dataclass
class CGRQueryIsGameInPreRound(CGRQueryParams): pass
@dataclass
class CGRQueryIsGameInProgress(CGRQueryParams): pass
@dataclass
class CGRQueryJoinAsSpectator(CGRQueryParams): pass
@dataclass
class CGRQuerySystemPresence(CGRQueryParams): pass
@dataclass
class CGRStateIdle(CGRState): pass
@dataclass
class CGRStateLoad(CGRState): pass
@dataclass
class CGRStateLoadPlayer(CGRStateLoad): pass
@dataclass
class CGRStateMenu(CGRState): pass
@dataclass
class CGRStateSingle(CGRState): pass
@dataclass
class CGRSwitchStateEvent(CGREvent): pass
@dataclass
class CGRUsePawnEquipmentFromArchetype(CGRQueryParams): pass
@dataclass
class CHandleNetBroadcast(CNetObjectProtocolEvent): pass
@dataclass
class CHandleNetMessage(CNetObjectProtocolEvent): pass
@dataclass
class CHandleNetUnicast(CNetObjectProtocolEvent): pass
@dataclass
class CHealthFailureEscalationEvent(CEntityEvent): pass
@dataclass
class CHostAdminService(IHostAdminService): pass
@dataclass
class CHudComponent(CEntityComponent): pass
@dataclass
class CHudService(IGameModeService): pass
@dataclass
class CHumanPersonality(CLivingCreature): pass
@dataclass
class CIEDPlacedEvent(CEntityEvent): pass
@dataclass
class CIgnitorComponent(CEntityComponent): pass
@dataclass
class CIgnitorNetworkComponent(CNetworkComponent): pass
@dataclass
class CInputConfig(CNomadConfigObject): pass
@dataclass
class CInputDriverGamepad(CInputDriver): pass
@dataclass
class CInputDriverGamepad_Win32(CInputDriverGamepad): pass
@dataclass
class CInputDriverKeyboard(CInputDriver): pass
@dataclass
class CInputDriverKeyboard_Win32(CInputDriverKeyboard): pass
@dataclass
class CInputDriverMouse(CInputDriver): pass
@dataclass
class CInputDriverMouse_Win32(CInputDriverMouse): pass
@dataclass
class CInventoryItem(CNomadObject): pass
@dataclass
class CInventoryItemAmmoPouch(CInventoryItem): pass
@dataclass
class CInventoryItemEquipment(CInventoryItem): pass
@dataclass
class CInventoryItemGadget(CInventoryItemEquipment): pass
@dataclass
class CInventoryItemEmbeddedGadget(CInventoryItemGadget): pass
@dataclass
class CInventoryItemEquippedGadget(CInventoryItemGadget): pass
@dataclass
class CInventoryItemWeapon(CInventoryItemEquipment): pass
@dataclass
class CInvisibleWall(CBasicRegionEntity): pass
@dataclass
class CJackalTapeManager(CSingletonEntity): pass
forward(CJackalTapeManager, CSingletonEntity)
@dataclass
class CKeyFramedGradientColor(CNomadObject): pass
@dataclass
class CKickBanService(IGameModeService): pass
@dataclass
class CLadder(CGameObject): pass
@dataclass
class CLadderNetworkComponent(CNetworkComponent): pass
@dataclass
class CLandmarkFarCategory(CSectorSpawnCategory): pass
@dataclass
class CLandmarkNearCategory(CSectorSpawnCategory): pass
@dataclass
class CLANLoginOperation(CLoginOperation): pass
@dataclass
class CLayerResource(CResourceContainer): pass
@dataclass
class CLeaderboardService(IGameModeService): pass
@dataclass
class CLightEvent(CEntityEvent): pass
@dataclass
class CLinearPathFollower(CPathFollower): pass
@dataclass
class CLiquidPropaneTank(CEntityComponent): pass
@dataclass
class CLivingCreature(CPersonality): pass
@dataclass
class CListMenuPage(CMenuPage): pass
@dataclass
class CLoadGamePage(CGameFilesListPage): pass
@dataclass
class CLobbyService(IGameModeService): pass
@dataclass
class CLoginOperation(CSessionOperation): pass
@dataclass
class CLoginSessionParam(COperationData): pass
@dataclass
class CLookAtTriggerComponent(CBaseTriggerComponent): pass
@dataclass
class CLoopingPathFollower(CPathFollower): pass
@dataclass
class CLuaResource(CResource): pass
@dataclass
class CMacheteEvent(CEntityEvent): pass
@dataclass
class CMagicCrate(CEntityComponent): pass
@dataclass
class CMagmaConfigUIResource(CMagmaResourceContainer): pass
@dataclass
class CMagmaDebugTextService(IMagmaDebugTextService): pass
@dataclass
class CMagmaResourceContainer(CResourceContainer): pass
@dataclass
class CMagmaUIResource(CMagmaResourceContainer): pass
@dataclass
class CMajorLocationEntity(CEntity): pass
@dataclass
class CMalariaEvent(CEntityEvent): pass
@dataclass
class CMapElementComponent(CEntityComponent): pass
@dataclass
class CMapElementEvent(CEntityEvent): pass
@dataclass
class CMapElementStateChangedEvent(CEntityEvent): pass
@dataclass
class CMapIntelligence(CEntityComponent): pass
@dataclass
class CMapMarkerManager(CSingletonEntity): pass
forward(CMapMarkerManager, CSingletonEntity)
@dataclass
class CMapOverrideTextureEvent(CEntityEvent): pass
@dataclass
class CMapService(IGameModeService): pass
@dataclass
class CMassiveComponent(COnlineAdComponent): pass
@dataclass
class CMatchService(IGameModeService): pass
@dataclass
class CMaterialImpactFx(CNomadObject): pass
@dataclass
class CMaterialResource(CResourceContainer): pass
@dataclass
class CMedicStation(COpeningPickup): pass
@dataclass
class CMedicStationNetworkComponent(CPickupNetworkComponent): pass
@dataclass
class CMemoryStreamFile(IFile): pass
@dataclass
class CMenuPage(CUIPageBase): pass
@dataclass
class CMetaSector(CWorldSector): pass

@dataclass
class CMissionComponent(CEntityComponent):
	x_7AF1FD74: str = field(default="")
	hidMissionLayerPath: int = field(default=0)
	x_27B31D2E: str = field(default="")
	hidCategory: int = field(default=0)
	ForceMerge: bool = field(default=False)

@dataclass
class CMissionHandlerEvent(CScriptEvent): pass
@dataclass
class CMortarIncoming(CEntityEvent): pass
@dataclass
class CMountedWeapon(CEntityComponent): pass
@dataclass
class CMountedWeaponNetworkComponent(CNetworkComponent): pass
@dataclass
class CMountedWeaponSmartTerrain(CSmartTerrain): pass
@dataclass
class CMovementResource(CResourceContainer): pass
@dataclass
class CMPBase(CEntityComponent): pass
@dataclass
class CMusicAIInfoManager(CSingletonEntity): pass
forward(CMusicAIInfoManager, CSingletonEntity)
@dataclass
class CMusicManager(CSingletonEntity): pass
forward(CMusicManager, CSingletonEntity)
@dataclass
class CMuzzleFlashManager(IGameModeService): pass
@dataclass
class CNavMeshGenComponent(CGameAIObject): pass
@dataclass
class CNavMeshSectorResource(CResourceContainer): pass
@dataclass
class CNetDescriptor(CNetDataContainer): pass
@dataclass
class CNetGameContextResolvedOperation(CGameOperation): pass
@dataclass
class CNetGameCtrlEnterGame(CNetGRStateProceedOperation): pass
@dataclass
class CNetGameCtrlEnterLobby(CNetGRStateProceedOperation): pass
@dataclass
class CNetGameCtrlOnGameModeChange(CGameOperation): pass
@dataclass
class CNetGameCtrlOnLoadWorldSync(CNetGameCtrlStateBaseSynchOp): pass
@dataclass
class CNetGameCtrlOnUnloadWorldSync(CNetGameCtrlStateBaseSynchOp): pass
@dataclass
class CNetGameCtrlStateBaseSynchOp(CGameOperation): pass
@dataclass
class CNetGameCtrlStateChangeContext(CNetGameCtrlStateGameContext): pass
@dataclass
class CNetGameCtrlStateGameContext(CNetGameCtrlState): pass
@dataclass
class CNetGameCtrlStateLocalPresence(CNetGameCtrlStatePresence): pass
@dataclass
class CNetGameCtrlStatePresence(CNetGameCtrlState): pass
@dataclass
class CNetGameCtrlStateUpdate(CNetGameCtrlState): pass
@dataclass
class CNetGameCtrlStateUpdateInGame(CNetGameCtrlStateUpdate): pass
@dataclass
class CNetGameCtrlStateUpdateLobby(CNetGameCtrlStateUpdate): pass
@dataclass
class CNetGRStateProceedOperation(CGameOperation): pass
@dataclass
class CNetObjectEvent(INetEvent): pass
@dataclass
class CNetObjectMonitoringEvent(CNetObjectEvent): pass
@dataclass
class CNetObjectOperation(IOperation): pass
@dataclass
class CNetObjectProtocolEvent(CNetObjectEvent): pass
@dataclass
class CNetObjectReady(CNetObjectEvent): pass
@dataclass
class CNetObjectResolved(CNetObjectMonitoringEvent): pass
@dataclass
class CNetObjectResolvedLegacy(CNetObjectEvent): pass
@dataclass
class CNetObjectUnresolved(CNetObjectMonitoringEvent): pass
@dataclass
class CNetworkComponent(CEntityComponent): pass
@dataclass
class CNetworkConfig(CNomadConfigObject): pass
@dataclass
class CNetworkLogConfig(CNomadConfigObject): pass
@dataclass
class CNetworkResource(CResource): pass
@dataclass
class CNetworkSettingGenericbool(CNetworkSetting): pass
@dataclass
class CNetworkSettingGenericCryString(CNetworkSetting): pass
@dataclass
class CNetworkSettingGenericunsigned_long(CNetworkSetting): pass
@dataclass
class CNetworkSettingsCollection(CNetworkSetting): pass
@dataclass
class CNewParticlesComponent(CEntityComponent): pass
@dataclass
class CNewParticlesSystemCleanEvent(CEntityEvent): pass
@dataclass
class CNewParticlesSystemPauseEvent(CEntityEvent): pass
@dataclass
class CNewParticlesSystemStartEvent(CEntityEvent): pass
@dataclass
class CNewParticlesSystemStopEvent(CEntityEvent): pass
@dataclass
class CNewsGetChannelOperation_RdV(CNewsOperation_RdV): pass
@dataclass
class CNewsGetHeadersOperation_RdV(CNewsOperation_RdV): pass
@dataclass
class CNewsGetHeadersParams(COperationData): pass
@dataclass
class CNewsGetNewsHeadersOperation_RdV(CNewsOperation_RdV): pass
@dataclass
class CNewsGetNumberOfNewsOperation_RdV(CNewsOperation_RdV): pass
@dataclass
class CNewsGetNumberOfNewsParams(COperationData): pass
@dataclass
class CNewsOperation_RdV(CRendezVousOperation): pass
@dataclass
class CNewsSetLocalizationOperation_RdV(CSessionOperation): pass
@dataclass
class CNomadConfigObject(CNomadObject): pass
@dataclass
class CNomadDbObject(CNomadObject): pass
@dataclass
class CNomadDbObjectNamed(CNomadDbObject): pass
@dataclass
class CObjectIgnitorCreatorComponent(CEntityComponent): pass
@dataclass
class CObjectSoundAndFXComponent(CEntityComponent): pass
@dataclass
class COcclusionQueryComponent(CEntityComponent): pass
@dataclass
class COmniEntity(CEntity): pass
@dataclass
class COneDayCompletedEvent(CEntityEvent): pass
@dataclass
class COnScreenPopup(CNomadObject): pass
@dataclass
class COpeningPickup(CPickup): pass
@dataclass
class CParticleAmbianceComponent(CEntityComponent): pass
@dataclass
class CParticleFXComponent(CEntityComponent): pass
@dataclass
class CParticleFXEvent(CEntityEvent): pass
@dataclass
class CParticlePhysComponent(CPhysComponent): pass
@dataclass
class CParticleRegion(CNomadObject): pass
@dataclass
class CParticlesEmitterParamResource(CResourceContainer): pass
@dataclass
class CParticlesSystemParamResource(CResourceContainer): pass
@dataclass
class CPartyService(IGameModeService): pass
@dataclass
class CPathFindTester(CAgent): pass
@dataclass
class CPathFollower(CNomadObject): pass
@dataclass
class CPatrolBrain(CBrain): pass

@dataclass
class CPawn(CGameObject):
	# Implementation
	bIsAI: bool = field(default=False)
	# Skills
	x_502D1B6A: str = field(default="")
	filePawnStateMachine: int = field(default=0)
    # Inventory
    # DesiredData
    # EffectiveData
    # JumpHeight
    # SavedMoveState
    # BonusPlans
    # StateDriver
    # PawnBlackboard
    # SerializationEvent
    # SerializationEvent
	Usable: bool = field(default=False)
	Enabled: bool = field(default=False)
	IsUsableOrientationNeeded: bool = field(default=False)

	@dataclass
	class Body(CPawnBody): pass
	@dataclass
	class Skills(object): pass
	@dataclass
	class Inventory(SInventoryViewPawnImpl): pass
	@dataclass
	class IdleCycleBreaker(object):
		fMinTime: float = field(default=0.0)
		fMaxTime: float = field(default=0.0)

@dataclass
class CPawnAction(CAgentAction): pass

@dataclass
class CPawnAgent(CGameAgent):
    # RescueAttempt
    # RescueCooldown
    # IsDead
    # FlareCooldown
    # IsUsingMountedWeapon
    # CurrentArmyMemberState
    # PreviousArmyMemberState
    # CurrentArmyMemberRole
    # DesiredArmyMemberRole
    # CurrentArmyMemberRoleAction
    # DesiredArmyMemberRoleAction
    # GotoFireRange
    # FireStrategy
    # LookStrategy
    # EmotionStrategy
    # AimStrategy
    # SpecialStrategy
    # CurrentAttackZone
    # ThreatLevelTimeCounter
    # ThreatLevel
    # PillarThresholdCross
    # RescueState
    # ThreatEventTimeStamp
    # ThreatLevelCounter
    # ThreatPriority
    # HealthFailureWhileHealing
    # ThresholdStartTime
    # TimeSinceLastShot
    # TimeSinceHMRFailure
    # MercBrain
    # MercBrainST
    # RescueSafe
    # ThresholdLevel
    # CurrentBuildingId
    # BumpAngle
    # BumpSpeed
    # PreviousBestTarget
    # CurrentBestTarget
    # SawSomethingLevel
    # AlertLevel
    # BlindCombatLevel
    # FuzzyVisibility
    # ClearVisibility
    # TimeOfDeath
    # IsPlayerInAIvsAIZone
    # InitialReinforcementRegionId
    # InitialStrategicZoneId
    # WeaponReadyTimer
    # LastMuzzleFlashTime
    # AiShootMeObjectId
    # LastBlindCombatNotification
    # HighestSocialRegionType
    # AllSocialRegionType
    # IsPlayer
    # LastTimeHurt
    # AlertLostTargetRushType
    # ProjEscapeType
    # WagerHandle
    # IsSpecialMissionBehaviourMerc
    # IsSafeHouseMerc
    # OutsideWagerLifeTime
    # WeaponCurrentClass
    # WeaponPreviousClass
    # WeaponLastTransitionTime
    # WeaponSwitchTo
    # ReservedEntrance
    # ReadyForMoveCallback
    # MoveCallbackLayer
    # ShineLensCounter
    # DominoDataArray, DominiData, DominoData
    # AutomaticScriptedScenePrefab
    # PlayingAnim
    # NextAnim
    # IntuitionTimer
    # GotIntuition
    # BulletJustMissed
    # MustDieNow
    # VehicleFallBackPosTimer
    # VehicleFallBackPositions
    # VariationID
    # VariationID2
	bHasALongRangeWeapon: bool = field(default=False)
	bOppositeArmy: bool = field(default=False)
	m_IdleFuzzyVal: float = field(default=0.0)
	m_IdleClearVal: float = field(default=0.0)
	m_SocialFuzzyVal: float = field(default=0.0)
	m_SocialClearVal: float = field(default=0.0)
	m_AlertFuzzyVal: float = field(default=0.0)
	m_AlertClearVal: float = field(default=0.0)
	m_CombatFuzzyVal: float = field(default=0.0)
	m_CombatClearVal: float = field(default=0.0)
	m_ThresholdFuzzyVal: float = field(default=0.0)
	m_ThresholdClearVal: float = field(default=0.0)
	m_SpecialFuzzyVal: float = field(default=0.0)
	m_SpecialClearVal: float = field(default=0.0)
	m_DeadFuzzyVal: float = field(default=0.0)
	m_DeadClearVal: float = field(default=0.0)
	m_VehicleFuzzyVal: float = field(default=0.0)
	m_VehicleClearVal: float = field(default=0.0)
	selArmy: int = field(default=0)
	selODU: int = field(default=0)
	selSpecialCharacterType: int = field(default=0)
	selAIInfamyMode: int = field(default=0)

	@dataclass
	class enumArmy(object): pass
	@dataclass
	class enumODU(object): pass
	@dataclass
	class enumSpecialCharacterType(object): pass
	@dataclass
	class enumAIInfamyMode(object): pass

	@dataclass
	class ShootingSystem(object):
		archGroupNumberCurve: str = field(default="")
        # TargetStatus
        # AimingDot
		fMissWidth: float = field(default=0.0)
		fMissHeight: float = field(default=0.0)
		fTimerToMissTarget: float = field(default=0.0)
		fPointBlankDistance: float = field(default=0.0)
		fTimerToPointBlank: float = field(default=0.0)

		@dataclass
		class ShooterStatus(CPawnFactorParam): pass
		@dataclass
		class TargetStatus(CPawnFactorParam): pass

	@dataclass
	class SensorySystem(CSensorySystem): pass

@dataclass
class CPawnAgentRescueEvent(CEntityEvent): pass
@dataclass
class CPawnBarkEvent(CEntityEvent): pass
@dataclass
class CPawnBeautifier(CEntityComponent): pass
@dataclass
class CPawnBeautifierAI(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierAICinematic(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierBuddyDownAI(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierFirst(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierCinematicFirst(CPawnBeautifierFirst): pass
@dataclass
class CPawnBeautifierComponent(CEntityComponent): pass
@dataclass
class CPawnBeautifierDominoPlayer(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierFirstNoControl(CPawnBeautifierFirst): pass
@dataclass
class CPawnBeautifierHMRAI(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierLadder(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierMeleeAI(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierMountedWeapon(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierNetPlayer(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierPickupPlayer(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierPlantedWeapon(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierPlayer(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierRagdoll(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierRescueAI(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierRescuePlayer(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierRevive(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierSlide(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierStorm(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierSwim(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierThird(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierVehicle(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierVehiclePassenger(CPawnBeautifier): pass
@dataclass
class CPawnBeautifierVehicleRide(CPawnBeautifierVehicle): pass
@dataclass
class CPawnBeautifierVehicleRidePassenger(CPawnBeautifierDominoPlayer): pass

@dataclass
class CPawnBody(object):
	fJumpHeight: float = field(default=0.0)
	fJumpHeightExhausted: float = field(default=0.0)
	fGravity: float = field(default=0.0)
	fWalkingMaxSpeed: float = field(default=0.0)
	fWalkingMaxSpeedCrouch: float = field(default=0.0)
	fWalkingAcceleration: float = field(default=0.0)
	fWalkingDeceleration: float = field(default=0.0)
	archSprintCurve: str = field(default="")
	fSprintingDeceleration: float = field(default=0.0)
	fClimbSpeed: float = field(default=0.0)
	fSwimmingMinDepth: float = field(default=0.0)
	fSwimmingMaxSpeed: float = field(default=0.0)
	fSwimmingAcceleration: float = field(default=0.0)
	fSwimmingDeceleration: float = field(default=0.0)
	fDivingMaxSpeed: float = field(default=0.0)
	fDivingAcceleration: float = field(default=0.0)
	fDivingDeceleration: float = field(default=0.0)
	fSprintingTurnModifier: float = field(default=0.0)
	fSprintingStrafeLimit: float = field(default=0.0)
	SwimmingClimbMinHeight: float = field(default=0.0)
	SwimmingClimbMaxHeight: float = field(default=0.0)

@dataclass
class CPawnBonusPlanManager(CNomadObject): pass
@dataclass
class CPawnDecision(CAgentDecision): pass
@dataclass
class CPawnEnemyMonitor(CEntityComponent): pass
@dataclass
class CPawnEntity(CEntity): pass
@dataclass
class CPawnEvent(CEntityEvent): pass
@dataclass
class CPawnEventFakeBullet(CEntityEvent): pass
@dataclass
class CPawnEventInstantKill(CEntityEvent): pass
@dataclass
class CPawnEventProcessLanding(CEntityEvent): pass

@dataclass
class CPawnFactorParam(object):
	fStandingFactor: float = field(default=0.0)
	fCrouchingFactor: float = field(default=0.0)
	fMoveSpeedBabyStepFactor: float = field(default=0.0)
	fMoveSpeedWalkFactor: float = field(default=0.0)
	fMoveSpeedJogFactor: float = field(default=0.0)
	fMoveSpeedRunFactor: float = field(default=0.0)
	fMoveSpeedSprintFactor: float = field(default=0.0)
	fDrivingFactor: float = field(default=0.0)
	fSwimmingFactor: float = field(default=0.0)
	fIronsightFactor: float = field(default=0.0)
	uiMaxHitPerSecondFactor: int = field(default=0)

@dataclass
class CPawnInteractionMonitor(CEntityComponent): pass
@dataclass
class CPawnMagicCrate(CEntityComponent): pass
@dataclass
class CPawnNetworkComponent(CNetworkComponent): pass
@dataclass
class CPawnPlayerAchievementsComponent(CEntityComponent): pass
@dataclass
class CPawnPushPlayerEvent(CEntityEvent): pass
@dataclass
class CPawnScanner(CAgentScanner): pass
@dataclass
class CPawnSoundAndFXComponent(CCreatureSoundAndFXComponent): pass

@dataclass
class CPersistComponent(CEntityComponent):
	selLevel: int = field(default=0)

@dataclass
class CPersistenceMgr(IGameModeService): pass
@dataclass
class CPersonality(CNomadObject): pass
@dataclass
class CPhoneCallEvent(CEntityEvent): pass
@dataclass
class CPhysBulletHitStim(CPhysStim): pass
@dataclass
class CPhysCollisionStim(CPhysStim): pass

@dataclass
class CPhysComponent(CEntityComponent):
	x_527E7674: str = field(default="")
	hidResourceId: int = field(default=0)
    # PartType
    # Velocity
    # CollisionSystemGroup
    # Enable

@dataclass
class CPhysEntityCreateParams(CNomadObject): pass
@dataclass
class CPhysExplosionStim(CPhysStim): pass
@dataclass
class CPhysicalFile(IFile): pass
@dataclass
class CPhysicConfig(CNomadConfigObject): pass
@dataclass
class CPhysNetworkComponent(CNetworkComponent): pass
@dataclass
class CPhysOutOfWorldEvent(CEntityEvent): pass
@dataclass
class CPhysPhantomComponent(CEntityComponent): pass
@dataclass
class CPhysRayPhantomComponent(CEntityComponent): pass
@dataclass
class CPhysResource(CResource): pass
@dataclass
class CPhysRigidEntityCreateParams(CPhysSimulationEntityCreateParams): pass
@dataclass
class CPhysSimulationEntityCreateParams(CPhysEntityCreateParams): pass
@dataclass
class CPhysStim(CEntityEventStims): pass
@dataclass
class CPhysVehicleEntityCreateParams(CPhysRigidEntityCreateParams): pass
@dataclass
class CPhysWheeledVehicleEntityCreateParams(CPhysVehicleEntityCreateParams): pass
@dataclass
class CPickAmmoEvent(CEntityEvent): pass
@dataclass
class CPickup(CEntityComponent): pass
@dataclass
class CPickupAmmo(CPickup): pass
@dataclass
class CPickupContainer(CPickup): pass
@dataclass
class CPickupContainerNetworkComponent(CNetworkComponent): pass
@dataclass
class CPickupDiamond(COpeningPickup): pass
@dataclass
class CPickupEvent(CEntityEvent): pass
@dataclass
class CPickupGadget(CPickup): pass
@dataclass
class CPickupHealth(CPickup): pass
@dataclass
class CPickupMissionItem(CPickup): pass
@dataclass
class CPickupMultipleAmmo(CPickup): pass
@dataclass
class CPickupNetworkComponent(CPhysNetworkComponent): pass
@dataclass
class CPickupPile(CPickup): pass
@dataclass
class CPickupPileNetworkComponent(CCompoundPhysNetworkComponent): pass
@dataclass
class CPickupScoutedEvent(CEntityEvent): pass

@dataclass
class CPickupWeapon(CPickup):
	bEnable: bool = field(default=False)
	fRespawnTime: float = field(default=0.0)
	bCustomBoundingBox: bool = field(default=False)
	vectorBoundingBoxSize: Vector3 = field(default_factory=Vector3)
	vectorBoundingBoxOffset: Vector3 = field(default_factory=Vector3)
	bAffectedLightPickup: bool = field(default=False)
	bPickable: bool = field(default=False)
	sUsageString: str = field(default="")
	objGeometryPreload: str = field(default="")
	Priority: int = field(default=0)
	bCanBeScouted: bool = field(default=False)
	archWeapon: str = field(default="")
	iMinAmmo: int = field(default=0)
	iMaxAmmo: int = field(default=0)

@dataclass
class CPierAnchor(CGameAIObject): pass
@dataclass
class CPlan(CAction): pass
@dataclass
class CPlayActionBrain(CBrain): pass
@dataclass
class CPlaybackComponent(CEntityComponent): pass
@dataclass
class CPlayer(IPlayer): pass
@dataclass
class CPlayerPopupMenu(CGameMessageBoxList): pass
@dataclass
class CPlayerService(IPlayerService): pass
@dataclass
class CPlayerSoundAndFXComponent(CPawnSoundAndFXComponent): pass
@dataclass
class CPlayerSoundEvent(CEntityEvent): pass
@dataclass
class CPositionLoggerComponent(CEntityComponent): pass
@dataclass
class CPostFxDatabase(CEntityComponent): pass
@dataclass
class CPostFxManager(CSingletonEntity): pass
forward(CPostFxManager, CSingletonEntity)
@dataclass
class CPostFxService(IGameModeService): pass
@dataclass
class CPostLoginOperation(CSessionOperation): pass
@dataclass
class CPrefabDescription(CNomadObject): pass
@dataclass
class CPrefabEntity(CEntity): pass
@dataclass
class CPrefabManager(CSingletonEntity): pass
forward(CPrefabManager, CSingletonEntity)
@dataclass
class CProjectileNetworkComponent(CNetworkComponent): pass
@dataclass
class CProximityTriggerComponent(CBaseTriggerComponent): pass
@dataclass
class CPusher(CEntityComponent): pass
@dataclass
class CQueryProjectileSynchroEvent(CBaseEvent): pass
@dataclass
class CQuickMatchGatherOpCtn(CGameOperationContainer): pass
@dataclass
class CQuickMatchJoinCandidateOp(CGameOperation): pass
@dataclass
class CQuickMatchJoinOpCtn(CGameOperationContainer): pass
@dataclass
class CQuickMatchPingCandidatesOp(CGameOperation): pass
@dataclass
class CQuickMatchRetrieveCandidatesOp(CGameOperation): pass
@dataclass
class CQuickMatchSelectCandidateOp(CGameOperation): pass
@dataclass
class CRadio(CEntityComponent): pass
@dataclass
class CRadioManager(CSingletonEntity): pass
forward(CRadioManager, CSingletonEntity)
@dataclass
class CRainComponent(CEntityComponent): pass
@dataclass
class CRandomPathFollower(CPathFollower): pass
@dataclass
class CRandomShooterComponent(CEntityComponent): pass
@dataclass
class CReadNetMemento(CNetObjectProtocolEvent): pass
@dataclass
class CRealtreeClusterComponent(CClusterComponent): pass
@dataclass
class CRealtreeComponent(CRenderableComponent): pass
@dataclass
class CRealtreeFx(CNomadDbObject): pass
@dataclass
class CRealtreeFxManager(CSingletonEntity): pass
forward(CRealtreeFxManager, CSingletonEntity)
@dataclass
class CRealtreeResource(CResourceContainer): pass
@dataclass
class CReinforcementEntityLoadedEvent(CEntityEvent): pass
@dataclass
class CReinforcementMercLoadedEvent(CReinforcementEntityLoadedEvent): pass
@dataclass
class CReinforcementPoint(CGameAIObject): pass
@dataclass
class CRelayTriggerComponent(CBaseTriggerComponent): pass
@dataclass
class CRemoveSEFactEvent(CEntityEvent): pass
@dataclass
class CRenderableComponent(CEntityComponent): pass
@dataclass
class CRenderAmbientConfig(CRenderBaseConfig): pass
@dataclass
class CRenderBaseConfig(CNomadConfigObject): pass
@dataclass
class CRenderConfig(CNomadConfigObject): pass
@dataclass
class CRenderEnvironmentConfig(CRenderBaseConfig): pass
@dataclass
class CRenderGeometryConfig(CRenderBaseConfig): pass
@dataclass
class CRenderPostFxConfig(CRenderBaseConfig): pass
@dataclass
class CRenderQualityConfig(CRenderBaseConfig): pass
@dataclass
class CRenderShadowConfig(CRenderBaseConfig): pass
@dataclass
class CRenderTerrainConfig(CRenderBaseConfig): pass
@dataclass
class CRenderTextureConfig(CRenderBaseConfig): pass
@dataclass
class CRenderVegetationConfig(CRenderBaseConfig): pass
@dataclass
class CRenderWaterConfig(CRenderBaseConfig): pass
@dataclass
class CRendezVousOperation(CSessionOperation): pass
@dataclass
class CRescueManager(IGameModeService): pass
@dataclass
class CResourceContainer(CResource): pass
@dataclass
class CResourceNotifier(CResourceContainer): pass
@dataclass
class CResourceWatch(CResourceNotifier): pass
@dataclass
class CRigidGraphicComponent(CStaticGraphicComponent): pass

@dataclass
class CRigidPhysComponent(CPhysComponent):
	x_527E7674: str = field(default="")
	hidResourceId: int = field(default=0)
	bDisabledAtStart: bool = field(default=False)
	bAlwaysStatic: bool = field(default=False)
	bCreateAsStatic: bool = field(default=False)
	bUseFastCollision: bool = field(default=False)
	bDisappearOnDeath: bool = field(default=False)
	bUseMaxTerrainSlope: bool = field(default=False)
	sndDestructionSound: str = field(default="")
	fHealth: float = field(default=0.0)
	fSelfCollOverrideSpeed: float = field(default=0.0)
	selCollisionLayer: int = field(default=0)
	ResourceIndex: int = field(default=0)
	vectorCenterOfMassOffset: Vector3 = field(default_factory=Vector3)
	fFloatingScale: float = field(default=0.0)
	fWaterFriction: float = field(default=0.0)
	sndtpDestructionSoundType: int = field(default=0)

@dataclass
class CRigidPhysOnDamageEvent(CEntityEvent): pass
@dataclass
class CRigidPhysOnDieEvent(CEntityEvent): pass
@dataclass
class CRigidPhysOnStateChangeEvent(CEntityEvent): pass
@dataclass
class CRoadSign(CEntityComponent): pass
@dataclass
class CRoadSignManager(CSingletonEntity): pass
forward(CRoadSignManager, CSingletonEntity)

@dataclass
class CRocket(CGameObject):
	sFXBone: int = field(default=0)

@dataclass
class CSafeHouseComponent(CEntityComponent): pass
@dataclass
class CSaveAtNextUpdateEvent(CEntityEvent): pass
@dataclass
class CSaveGamePage(CGameFilesListPage): pass
@dataclass
class CSavePointCheckPage(CMenuPage): pass
@dataclass
class CSavePointSaveGamePage(CSaveGamePage): pass
@dataclass
class CScanner(CTask): pass
@dataclass
class CScannerAgentAimingAt(CPawnScanner): pass
@dataclass
class CScannerAgentHasRaisedWeapon(CPawnScanner): pass
@dataclass
class CScannerAgentIsVisible(CPawnScanner): pass
@dataclass
class CScannerAgentSocialProximity(CPawnScanner): pass
@dataclass
class CScannerAgentStaredown(CPawnScanner): pass
@dataclass
class CScannerAimStrategy(CPawnScanner): pass
@dataclass
class CScannerAnimalObstacleAhead(CAgentScanner): pass
@dataclass
class CScannerAnimalThreatChanged(CAgentScanner): pass
@dataclass
class CScannerAnimalThreatened(CAgentScanner): pass
@dataclass
class CScannerArmyMemberRole(CPawnScanner): pass
@dataclass
class CScannerArmyMemberState(CPawnScanner): pass
@dataclass
class CScannerBestTargetChangedPos(CPawnScanner): pass
@dataclass
class CScannerBlackboardFact(CAgentScanner): pass
@dataclass
class CScannerCanDisableSTPDynamicAvoidance(CPawnScanner): pass
@dataclass
class CScannerCheckValue(CAgentScanner): pass
@dataclass
class CScannerDead(CAgentScanner): pass
@dataclass
class CScannerDominoEvent(CPawnScanner): pass
@dataclass
class CScannerEmotionStrategy(CPawnScanner): pass
@dataclass
class CScannerFactExist(CAgentScanner): pass
@dataclass
class CScannerFireProximity(CAgentScanner): pass
@dataclass
class CScannerFireStrategy(CPawnScanner): pass
@dataclass
class CScannerInFOV(CPawnScanner): pass
@dataclass
class CScannerInterestLookAtType(CPawnScanner): pass
@dataclass
class CScannerIsAIShootMeObjectValid(CPawnScanner): pass
@dataclass
class CScannerIsInBuilding(CPawnScanner): pass
@dataclass
class CScannerIsInDistance(CPawnScanner): pass
@dataclass
class CScannerIsInVehicle(CPawnScanner): pass
@dataclass
class CScannerIsPosOnBarge(CAgentScanner): pass
@dataclass
class CScannerIsRotatedTowardsPos(CPawnScanner): pass
@dataclass
class CScannerIsUnderFire(CPawnScanner): pass
@dataclass
class CScannerLookStrategy(CPawnScanner): pass
@dataclass
class CScannerMovingPosition(CAgentScanner): pass
@dataclass
class CScannerMutualGreeting(CPawnScanner): pass
@dataclass
class CScannerNewTargetNeeded(CPawnScanner): pass
@dataclass
class CScannerPawnSenses(CPawnScanner): pass
@dataclass
class CScannerRiskPoint(CPawnScanner): pass
@dataclass
class CScannerSideLookOpening(CPawnScanner): pass
@dataclass
class CScannerSocialBehavior(CPawnScanner): pass
@dataclass
class CScannerSocialRegion(CPawnScanner): pass
@dataclass
class CScannerSpecialStrategy(CPawnScanner): pass
@dataclass
class CScannerTargetVisible(CPawnScanner): pass
@dataclass
class CScannerThresholdCross(CPawnScanner): pass
@dataclass
class CScannerVehicleIntruderAboard(CVehicleScanner): pass
@dataclass
class CScannerVehicleIsFunctional(CVehicleScanner): pass
@dataclass
class CScannerVehicleMergePosReached(CVehicleScanner): pass
@dataclass
class CScannerVehiclePierAnchor(CVehicleScanner): pass
@dataclass
class CScannerVehicleStandBy(CAgentScanner): pass
@dataclass
class CScannerVisualThreat(CPawnScanner): pass
@dataclass
class CScannerWalkDistance(CAgentScanner): pass
@dataclass
class CSceneObjectComponentCSceneAdaptiveBloom(CEntityComponent): pass
@dataclass
class CSceneObjectComponentCScenePostFxDepthOfField(CEntityComponent): pass
@dataclass
class CScoreboardService(IGameModeService): pass
@dataclass
class CScoutIntelsManager(CSingletonEntity): pass
forward(CScoutIntelsManager, CSingletonEntity)
@dataclass
class CScriptCallbackComponent(CEntityComponent): pass
@dataclass
class CScriptedScenePrefabEntity(CPrefabEntity): pass
@dataclass
class CScriptEvent(CEntityEvent): pass
@dataclass
class CScriptService(IGameModeService): pass
@dataclass
class CSectorDataResource(CResource): pass
@dataclass
class CSectorDescriptorResource(CResource): pass
@dataclass
class CSectorEntity(CEntity): pass
@dataclass
class CSectorPreloadResource(CResource): pass
@dataclass
class CSectorResource(CResourceContainer): pass
@dataclass
class CSectorSpawnCategory(CResourceNotifier): pass

class CSensorySystem_FOVParameters__RegionFOV__FOV: pass
class CSensorySystem_FOVParameters__RegionFOV: pass
@dataclass
class CSensorySystem(CNomadObject):
	@dataclass
	class FOVParameters(object):
		@dataclass
		class FOVMultipliers(object):
			fPreCombatMultiplier: float = field(default=0.0)
			fCombatMultiplier: float = field(default=0.0)
			fPostCombatMultiplier: float = field(default=0.0)
			fPlayerInVehicleMultiplier: float = field(default=0.0)
			fNightTimeMultiplier: float = field(default=0.0)
			fSniperLengthMultiplier: float = field(default=0.0)
			fSniperAngleMultiplier: float = field(default=0.0)

		@dataclass
		class _RegionFOV(object):
			@dataclass
			class _FOV(object):
				fLength: float = field(default=0.0)
				fAngle: float = field(default=0.0)

			@dataclass
			class FocusFOV(CSensorySystem_FOVParameters__RegionFOV__FOV): pass
			@dataclass
			class PeripheralFOV(CSensorySystem_FOVParameters__RegionFOV__FOV): pass
		@dataclass
		class DesertFOV(CSensorySystem_FOVParameters__RegionFOV): pass
		@dataclass
		class SavannahFOV(CSensorySystem_FOVParameters__RegionFOV): pass
		@dataclass
		class JungleFOV(CSensorySystem_FOVParameters__RegionFOV): pass

	@dataclass
	class VisibilityEvaluatorParameters(object):
		@dataclass
		class Weights(object):
			fDistanceEvaluatorWeight: float = field(default=0.0)
			fFOVEvaluatorWeight: float = field(default=0.0)
			fPawnSamplingEvaluatorWeight: float = field(default=0.0)
			fOcclusionEvaluatorWeight: float = field(default=0.0)
			fVegetationEvaluatorWeight: float = field(default=0.0)
			fStanceEvaluatorWeight: float = field(default=0.0)
			fSpeedEvaluatorWeight: float = field(default=0.0)
			fAmbientLightEvaluatorWeight: float = field(default=0.0)
		@dataclass
		class InternalValues(object):
			fDistanceEvaluator_FullVisibilityRatio: float = field(default=0.0)
			fDistanceEvaluator_MinVisibilityAtMaxFOVRange: float = field(default=0.0)
			fSpeedEvaluator_StandingStillVisibilityFactor: float = field(default=0.0)
			fFOVEvaluator_VisibilityFactorAtFOVLimit: float = field(default=0.0)

	@dataclass
	class SocialMechanic(object):
		fStareDetectionTime: float = field(default=0.0)
		fAimAtDetectionTime: float = field(default=0.0)
		fIntrusionDistanceInnerRing: float = field(default=0.0)
		fIntrusionDistanceMidRing: float = field(default=0.0)
		fIntrusionDistanceOuterRing: float = field(default=0.0)
		fMaxChargingDistance: float = field(default=0.0)
		fMaxChargingAngle: float = field(default=0.0)
CSensorySystem.FOVParameters._RegionFOV.FocusFOV.__bases__ = (CSensorySystem.FOVParameters._RegionFOV._FOV,)
CSensorySystem.FOVParameters._RegionFOV.PeripheralFOV.__bases__ = (CSensorySystem.FOVParameters._RegionFOV._FOV,)
CSensorySystem.FOVParameters.DesertFOV.__bases__ = (CSensorySystem.FOVParameters._RegionFOV,)
CSensorySystem.FOVParameters.SavannahFOV.__bases__ = (CSensorySystem.FOVParameters._RegionFOV,)
CSensorySystem.FOVParameters.JungleFOV.__bases__ = (CSensorySystem.FOVParameters._RegionFOV,)

@dataclass
class CSessionCreateGameOperation(CSessionOperation): pass
@dataclass
class CSessionCreateOperation(CSessionOperation): pass
@dataclass
class CSessionCreateOperation_Agora(CSessionCreateServiceOperation): pass
@dataclass
class CSessionCreateServiceOperation(CSessionOperation): pass
@dataclass
class CSessionDeleteGameOperation(CSessionOperation): pass
@dataclass
class CSessionDeleteOperation(CSessionOperation): pass
@dataclass
class CSessionDeleteServiceOperation(CSessionOperation): pass
@dataclass
class CSessionDescriptor(CNetDescriptor): pass
@dataclass
class CSessionDescriptor_Agora(CSessionDescriptor): pass
@dataclass
class CSessionFetchOnlineConfigOperation(CRendezVousOperation): pass
@dataclass
class CSessionInfo_Agora(CSessionInfo): pass
@dataclass
class CSessionJoinGameOperation(CSessionOperation): pass
@dataclass
class CSessionJoinOperation(CSessionOperation): pass
@dataclass
class CSessionJoinServiceOperation(CSessionOperation): pass
@dataclass
class CSessionLoginOperation(CSessionOperation): pass
@dataclass
class CSessionLogoutOperation(CSessionOperation): pass
@dataclass
class CSessionOperation(COperation): pass
@dataclass
class CSessionUpdateOperation(CSessionOperation): pass
@dataclass
class CSetInvincibleEvent(CEntityEvent): pass
@dataclass
class CSetNetInstanceIdEvent(CBaseEvent): pass
@dataclass
class CSettingsPage(CListMenuPage): pass
@dataclass
class CShortRangeResource(CSectorResource): pass

@dataclass
class CSimpleAnimationComponent(CEntityComponent):
	x_F9F2D5F4: str = field(default="")
	fileSkeleton: int = field(default=0)
	sPartName: str = field(default="")

@dataclass
class CSimpleEntityEvent(CEntityEvent): pass
@dataclass
class CSimpleNetworkComponent(CNetworkComponent): pass
@dataclass
class CSimplePrimitiveComponent(CRenderableComponent): pass
@dataclass
class CSimpleSettingCMapCycle(CGenericUISettingCMapCycle): pass
@dataclass
class CSingletonEntity(COmniEntity): pass

@dataclass
class CSkeletonResource(CResource): pass
@dataclass
class CSmartTerrain(CGameAIObject): pass
@dataclass
class CSmartTerrainEvent(CEntityEvent): pass
@dataclass
class CSmartTerrainManager(CSingletonEntity): pass
forward(CSmartTerrainManager, CSingletonEntity)
@dataclass
class CSniperPoint(CGameAIObject): pass
@dataclass
class CSocialRegion(CBasicRegionEntity): pass
@dataclass
class CSomeoneTalked(CEntityEvent): pass

@dataclass
class CSoundComponent(CEntityComponent):
	sndptSoundPoint: int = field(default=0)

@dataclass
class CSoundEvent(CEntityEvent): pass
@dataclass
class CSoundLineComponent(CBasicShapeComponent): pass
@dataclass
class CSoundManager(CSingletonEntity): pass
forward(CSoundManager, CSingletonEntity)
@dataclass
class CSoundResource(CResourceContainer): pass
@dataclass
class CSoundShapeComponent(IShapeComponent): pass
@dataclass
class CSpawnPointBlueStart(CSpawnPointBlue): pass
@dataclass
class CSpawnPointBuddy(CSpawnPoint): pass
@dataclass
class CSpawnPointRedStart(CSpawnPointRed): pass
@dataclass
class CSpawnPointService(ISpawnPointService): pass
@dataclass
class CSpawnPointSpectator(CSpawnPoint): pass
@dataclass
class CSpecialEventPoint(CEntity): pass
@dataclass
class CSpectatorPlayer(IPlayer): pass
@dataclass
class CSplinePrimitiveComponent(CRenderableComponent): pass
@dataclass
class CSRLResource(CResource): pass
@dataclass
class CStateMachineBlobResource(CResourceContainer): pass
@dataclass
class CStateMachineResource(CResourceContainer): pass
@dataclass
class CStaticClusterPhysComponent(CPhysComponent): pass
@dataclass
class CStaticDecalComponent(CRenderableComponent): pass
@dataclass
class CStaticGraphicComponent(CBaseGraphicComponent): pass
@dataclass
class CStaticPhysComponent(CPhysComponent): pass
@dataclass
class CStealthComponent(CEntityComponent): pass
@dataclass
class CStickyFlameEvent(CEntityEvent): pass
@dataclass
class CStimArray(CNomadObject): pass
@dataclass
class CStimEffectTable(CBaseEntity): pass
@dataclass
class CStimsEmitterComponent(CEntityComponent): pass
@dataclass
class CStopDialogEvent(CEntityEvent): pass
@dataclass
class CStrategicPoint(CTagPoint): pass
@dataclass
class CSuicideComponent(CEntityComponent): pass
@dataclass
class CTagPoint(CEntity): pass
@dataclass
class CTask(CTaskRoot): pass
@dataclass
class CTaskActivateInfamyPose(CPawnAction): pass
@dataclass
class CTaskActivateSocialSTP(CAgentAction): pass
@dataclass
class CTaskAimAt(CAgentAction): pass
@dataclass
class CTaskAnimalPathFollow(CAgentAction): pass
@dataclass
class CTaskAttackStrategy(CPawnDecision): pass
@dataclass
class CTaskBreakSocialPair(CPawnAction): pass
@dataclass
class CTaskBroadcastStims(CAgentAction): pass
@dataclass
class CTaskBuddyDown(CPawnDecision): pass
@dataclass
class CTaskCalcLineDist(CAgentAction): pass
@dataclass
class CTaskChase(CAgentAction): pass
@dataclass
class CTaskCheckActionSignal(CAgentAction): pass
@dataclass
class CTaskCheckAimStrategy(CPawnDecision): pass
@dataclass
class CTaskCheckAmmoStatus(CPawnDecision): pass
@dataclass
class CTaskCheckAnimalCanTryAnotherRunAwayDestination(CAgentDecision): pass
@dataclass
class CTaskCheckAnimalThreaten(CAgentDecision): pass
@dataclass
class CTaskCheckArmyRole(CPawnDecision): pass
@dataclass
class CTaskCheckArmyRoleAction(CPawnDecision): pass
@dataclass
class CTaskCheckBargeSide(CAgentDecision): pass
@dataclass
class CTaskCheckBlindCombatLevel(CPawnDecision): pass
@dataclass
class CTaskCheckBuildingEntry(CAgentDecision): pass
@dataclass
class CTaskCheckCanRescue(CPawnDecision): pass
@dataclass
class CTaskCheckCombatMercInRadius(CPawnDecision): pass
@dataclass
class CTaskCheckCoverDist(CPawnDecision): pass
@dataclass
class CTaskCheckCurrentSocialOccupation(CPawnDecision): pass
@dataclass
class CTaskCheckCurrentWeapon(CPawnDecision): pass
@dataclass
class CTaskCheckDifficultyLevel(CAgentDecision): pass
@dataclass
class CTaskCheckDisturbanceType(CPawnDecision): pass
@dataclass
class CTaskCheckDominoData(CPawnDecision): pass
@dataclass
class CTaskCheckEmotionStrategy(CPawnDecision): pass
@dataclass
class CTaskCheckFactExist(CAgentDecision): pass
@dataclass
class CTaskCheckFireProximity(CAgentDecision): pass
@dataclass
class CTaskCheckFireRange(CPawnDecision): pass
@dataclass
class CTaskCheckFireStrategy(CPawnDecision): pass
@dataclass
class CTaskCheckIdleBehavior(CPawnDecision): pass
@dataclass
class CTaskCheckInterestLookAtType(CPawnDecision): pass
@dataclass
class CTaskCheckIsInBuilding(CPawnDecision): pass
@dataclass
class CTaskCheckIsInDistance(CPawnDecision): pass
@dataclass
class CTaskCheckIsInFOV(CPawnAction): pass
@dataclass
class CTaskCheckIsPlayingBark(CPawnAction): pass
@dataclass
class CTaskCheckLookStrategy(CPawnDecision): pass
@dataclass
class CTaskCheckMovingFire(CPawnDecision): pass
@dataclass
class CTaskCheckObjectBlockingPath(CAgentDecision): pass
@dataclass
class CTaskCheckObstaclesInRegion(CAgentDecision): pass
@dataclass
class CTaskCheckODUType(CPawnDecision): pass
@dataclass
class CTaskCheckPillarDepleted(CPawnDecision): pass
@dataclass
class CTaskCheckPillarThreshold(CPawnDecision): pass
@dataclass
class CTaskCheckPlayerAction(CPawnDecision): pass
@dataclass
class CTaskCheckPlayerInfamy(CPawnDecision): pass
@dataclass
class CTaskCheckPosInLoadedSector(CAgentDecision): pass
@dataclass
class CTaskCheckPosOnSpline(CAgentDecision): pass
@dataclass
class CTaskCheckProjEscapeType(CPawnDecision): pass
@dataclass
class CTaskCheckProximity(CAgentAction): pass
@dataclass
class CTaskCheckQueryRange(CPawnDecision): pass
@dataclass
class CTaskCheckRegionTransition(CPawnDecision): pass
@dataclass
class CTaskCheckRegionType(CPawnDecision): pass
@dataclass
class CTaskCheckRelativeInfamy(CPawnDecision): pass
@dataclass
class CTaskCheckRescueState(CPawnDecision): pass
@dataclass
class CTaskCheckSawSomethingLevel(CPawnDecision): pass
@dataclass
class CTaskCheckSeeFriendNearby(CPawnDecision): pass
@dataclass
class CTaskCheckSmartTerrainType(CAgentDecision): pass
@dataclass
class CTaskCheckSocialProximity(CPawnDecision): pass
@dataclass
class CTaskCheckSpecialMissionBehaviour(CPawnDecision): pass
@dataclass
class CTaskCheckSpecialStrategy(CPawnDecision): pass
@dataclass
class CTaskCheckSquadAction(CPawnDecision): pass
@dataclass
class CTaskCheckSquadRole(CPawnDecision): pass
@dataclass
class CTaskCheckStressLevel(CPawnDecision): pass
@dataclass
class CTaskCheckTargetHeightDiff(CPawnDecision): pass
@dataclass
class CTaskCheckTargetRange(CPawnDecision): pass
@dataclass
class CTaskCheckTargetType(CPawnDecision): pass
@dataclass
class CTaskCheckTargetVisible(CPawnDecision): pass
@dataclass
class CTaskCheckThreatDistance(CAgentDecision): pass
@dataclass
class CTaskCheckThresholdLevel(CPawnDecision): pass
@dataclass
class CTaskCheckUnderFire(CPawnDecision): pass
@dataclass
class CTaskCheckUsingCover(CPawnDecision): pass
@dataclass
class CTaskCheckViewBlocked(CPawnDecision): pass
@dataclass
class CTaskCheckVisibleByPlayer(CPawnDecision): pass
@dataclass
class CTaskChooseCoverAttack(CPawnDecision): pass
@dataclass
class CTaskChurchAssault(CPawnAction): pass
@dataclass
class CTaskCleanBriefingAnim(CPawnAction): pass
@dataclass
class CTaskClearMoveToDynamics(CPawnAction): pass
@dataclass
class CTaskComputeInterpolatedPos(CAgentAction): pass
@dataclass
class CTaskComputeLeapFrogStep(CAgentAction): pass
@dataclass
class CTaskComputeProjectileTrajectory(CPawnAction): pass
@dataclass
class CTaskComputeSynchActionPosition(CPawnAction): pass
@dataclass
class CTaskCoverAttack(CPawnAction): pass
@dataclass
class CTaskDebugSetCurrentBehavior(CPawnAction): pass
@dataclass
class CTaskDisableSTPDynamicAvoidance(CPawnAction): pass
@dataclass
class CTaskDisplayError(CAgentAction): pass
@dataclass
class CTaskDisplaySTPClippingError(CAgentAction): pass
@dataclass
class CTaskDropItem(CPawnAction): pass
@dataclass
class CTaskEmitBark(CPawnAction): pass
@dataclass
class CTaskFindAIShootMeObject(CPawnDecision): pass
@dataclass
class CTaskFindCover(CAgentAction): pass
@dataclass
class CTaskFindCoverAttack(CAgentAction): pass
@dataclass
class CTaskFindEscapePos(CAgentAction): pass
@dataclass
class CTaskFindInterestLookAt(CPawnAction): pass
@dataclass
class CTaskFindLeapFrogStep(CAgentDecision): pass
@dataclass
class CTaskFindMountedWeapon(CPawnAction): pass
@dataclass
class CTaskFindProtectionPoint(CAgentAction): pass
@dataclass
class CTaskFindRandomDest(CAgentAction): pass
@dataclass
class CTaskFindRescueDest(CAgentAction): pass
@dataclass
class CTaskFindRiskPoints(CPawnAction): pass
@dataclass
class CTaskFindSocialFleePos(CPawnAction): pass
@dataclass
class CTaskFindStrategicPoint(CPawnDecision): pass
@dataclass
class CTaskFindVisualThreat(CPawnAction): pass
@dataclass
class CTaskFindWorldEntity(CAgentAction): pass
@dataclass
class CTaskFireStrategySelector(CPawnDecision): pass
@dataclass
class CTaskFuzzyChoice(CPawnDecision): pass
@dataclass
class CTaskGetBuildingEntry(CPawnAction): pass
@dataclass
class CTaskGetClosestSplinePos(CAgentAction): pass
@dataclass
class CTaskGetNextPathPos(CAgentAction): pass
@dataclass
class CTaskGetPatrolPath(CAgentAction): pass
@dataclass
class CTaskGetPosOnNavMesh(CAgentAction): pass
@dataclass
class CTaskGetRescuePositions(CPawnAction): pass
@dataclass
class CTaskGetSniperPoint(CPawnAction): pass
@dataclass
class CTaskGetStraightPath(CAgentAction): pass
@dataclass
class CTaskHighTargetAttackPos(CPawnDecision): pass
@dataclass
class CTaskIncreaseSawSomethingLevel(CPawnAction): pass
@dataclass
class CTaskIncrementPathPos(CAgentAction): pass
@dataclass
class CTaskLookAround(CPawnAction): pass
@dataclass
class CTaskLookAroundTarget(CPawnAction): pass
@dataclass
class CTaskLookAt(CAgentAction): pass
@dataclass
class CTaskLookAtVehicle(CPawnAction): pass
@dataclass
class CTaskLookRandom(CPawnAction): pass
@dataclass
class CTaskManageAnchor(CAgentAction): pass
@dataclass
class CTaskManageArmy(CPawnAction): pass
@dataclass
class CTaskMoveStrategy(CPawnDecision): pass
@dataclass
class CTaskMoveTo(CAgentAction): pass
@dataclass
class CTaskNextWeapon(CPawnAction): pass
@dataclass
class CTaskNotifyUnreachablePos(CPawnAction): pass
@dataclass
class CTaskOperateOnFlagField(CAgentDecision): pass
@dataclass
class CTaskOrientToward(CPawnAction): pass
@dataclass
class CTaskPathAnalyzer(CPawnDecision): pass
@dataclass
class CTaskPathFind(CAgentAction): pass
@dataclass
class CTaskPathFindAndMoveTo(CAgentAction): pass
@dataclass
class CTaskPathFollow(CAgentAction): pass
@dataclass
class CTaskPatrol(CAgentAction): pass
@dataclass
class CTaskPlayAnim(CAgentAction): pass
@dataclass
class CTaskPlayBriefingAnim(CPawnAction): pass
@dataclass
class CTaskPlaySound(CAgentAction): pass
@dataclass
class CTaskPredictImpactPos(CPawnAction): pass
@dataclass
class CTaskPrepareSynchActionPosition(CPawnAction): pass
@dataclass
class CTaskPushPlayer(CPawnAction): pass
@dataclass
class CTaskRequestVehicle(CPawnAction): pass
@dataclass
class CTaskReserveProtectionPoint(CAgentAction): pass
@dataclass
class CTaskReserveSniperPoint(CPawnAction): pass
@dataclass
class CTaskResourceManager(CAgentAction): pass
@dataclass
class CTaskRoot(CNomadObject): pass
@dataclass
class CTaskSavePosInFact(CAgentAction): pass
@dataclass
class CTaskSearchOpponents(CPawnAction): pass
@dataclass
class CTaskSelectBestOpponents(CPawnDecision): pass
@dataclass
class CTaskSelectBestTarget(CPawnDecision): pass
@dataclass
class CTaskSelectRiskPoint(CPawnDecision): pass
@dataclass
class CTaskSelectWeapon(CPawnAction): pass
@dataclass
class CTaskSendActionSignal(CAgentAction): pass
@dataclass
class CTaskSendBrainEvent(CAgentAction): pass
@dataclass
class CTaskSendDominoEvent(CPawnAction): pass
@dataclass
class CTaskSendHMREvent(CPawnAction): pass
@dataclass
class CTaskSendReport(CAgentAction): pass
@dataclass
class CTaskSendSocialReport(CPawnAction): pass
@dataclass
class CTaskSetAimStrategy(CPawnAction): pass
@dataclass
class CTaskSetCurrentState(CPawnAction): pass
@dataclass
class CTaskSetEmotionStrategy(CPawnAction): pass
@dataclass
class CTaskSetFacialEmotion(CPawnAction): pass
@dataclass
class CTaskSetFireStrategy(CPawnAction): pass
@dataclass
class CTaskSetForcedLookAtEntity(CPawnAction): pass
@dataclass
class CTaskSetLookStrategy(CPawnAction): pass
@dataclass
class CTaskSetPathPointPosition(CAgentAction): pass
@dataclass
class CTaskSetPawnAttribute(CPawnAction): pass
@dataclass
class CTaskSetPawnTarget(CPawnAction): pass
@dataclass
class CTaskSetPostureAttribute(CPawnAction): pass
@dataclass
class CTaskSetPostureIntention(CPawnAction): pass
@dataclass
class CTaskSetSocialEngageMode(CPawnAction): pass
@dataclass
class CTaskSetSpecialStrategy(CPawnAction): pass
@dataclass
class CTaskSetSpeed(CAgentAction): pass
@dataclass
class CTaskSetStanceOnSniperPoint(CPawnAction): pass
@dataclass
class CTaskSetSyncState(CPawnAction): pass
@dataclass
class CTaskShoot(CPawnAction): pass
@dataclass
class CTaskShootMortar(CPawnAction): pass
@dataclass
class CTaskShootMountedWeapon(CPawnAction): pass
@dataclass
class CTaskSmartTerrainExecutor(CAgentAction): pass
@dataclass
class CTaskSmartTerrainFinder(CAgentAction): pass
@dataclass
class CTaskSpecialVehicleDetach(CPawnAction): pass
@dataclass
class CTaskSplinePathFind(CAgentAction): pass
@dataclass
class CTaskStopBark(CPawnAction): pass
@dataclass
class CTaskStopBarkGesture(CPawnAction): pass
@dataclass
class CTaskSwitchWeapon(CPawnAction): pass
@dataclass
class CTaskTeleportInVehicleSeat(CPawnAction): pass
@dataclass
class CTaskUnReserveCover(CAgentAction): pass
@dataclass
class CTaskUpdateBlackboard(CAgentAction): pass
@dataclass
class CTaskUpdateBuddyAiming(CPawnAction): pass
@dataclass
class CTaskUpdatePathPos(CAgentAction): pass
@dataclass
class CTaskUseAIBuilding(CPawnAction): pass
@dataclass
class CTaskUseMountedWeapon(CPawnAction): pass
@dataclass
class CTaskUseSniperPoint(CPawnAction): pass
@dataclass
class CTaskVehicleAccost(CVehicleAction): pass
@dataclass
class CTaskVehicleAggressiveMove(CVehicleAction): pass
@dataclass
class CTaskVehicleBoostFactor(CVehicleAction): pass
@dataclass
class CTaskVehicleChase(CVehicleAction): pass
@dataclass
class CTaskVehicleCheckExitOnLand(CVehicleDecision): pass
@dataclass
class CTaskVehicleCheckSpeed(CVehicleDecision): pass
@dataclass
class CTaskVehicleCheckUserPriority(CVehicleDecision): pass
@dataclass
class CTaskVehicleEnableSteeringEngine(CVehicleAction): pass
@dataclass
class CTaskVehicleEscapeProjectile(CVehicleAction): pass
@dataclass
class CTaskVehicleGetBargePos(CVehicleAction): pass
@dataclass
class CTaskVehicleGetMergePos(CVehicleAction): pass
@dataclass
class CTaskVehicleGetPierAnchor(CVehicleAction): pass
@dataclass
class CTaskVehicleOrientToward(CVehicleAction): pass
@dataclass
class CTaskVehiclePathFollow(CVehicleAction): pass
@dataclass
class CTaskVehicleSetUserRolePriority(CVehicleAction): pass
@dataclass
class CTaskVehicleSink(CVehicleAction): pass
@dataclass
class CTaskVehicleStop(CVehicleAction): pass
@dataclass
class CTaskVehicleTurnAround(CVehicleAction): pass
@dataclass
class CTaskVehicleTurnCheat(CVehicleAction): pass
@dataclass
class CTaskVehicleUpdatePathFollow(CVehicleAction): pass
@dataclass
class CTaskWait(CAgentAction): pass
@dataclass
class CTaskWaitFactExist(CAgentAction): pass
@dataclass
class CTaskWatchFlyingProjectile(CPawnAction): pass
@dataclass
class CTDMSpawnPointService(CDMSpawnPointService): pass
@dataclass
class CTeamManager(ITeamManager): pass
@dataclass
class CTerm(CNomadObject): pass
@dataclass
class CTermFactList(CTerm): pass
@dataclass
class CTermSingleFact(CTerm): pass
@dataclass
class CTextureMipResource(CResource): pass
@dataclass
class CTextureResource(CResource): pass
@dataclass
class CThinPropaneTank(CEntityComponent): pass
@dataclass
class CThreadingConfig(CNomadConfigObject): pass
@dataclass
class CTimeOfDayTriggerComponent(CBaseTriggerComponent): pass
@dataclass
class CTrackingService(IGameModeService): pass
@dataclass
class CTravelStartOperation(CGameOperation): pass
@dataclass
class CTravelStopOperation(CGameOperation): pass
@dataclass
class CTriggerChangeCountEvent(CEntityEvent): pass

@dataclass
class CTriggerComponent(CEntityComponent):
	static_: bool = field(default=False)

@dataclass
class CTriggerEnableEvent(CEntityEvent): pass
@dataclass
class CTriggerEvent(CEntityEvent): pass
@dataclass
class CTriggerSimpleEvent(CEntityEvent): pass
@dataclass
class CTutorial(CChallenge): pass
@dataclass
class CUbisoftLoginOperation(CRendezVousOperation): pass
@dataclass
class CUGCLoginOperation(CLoginOperation): pass
@dataclass
class CUnreachableLocationsManager(CSingletonEntity): pass
forward(CUnreachableLocationsManager, CSingletonEntity)
@dataclass
class CUsableComponent(CEntityComponent): pass
@dataclass
class CValidEntityToAttachExplosive(CEntityEvent): pass
@dataclass
class CValueListSettingbool(CGenericUISettingbool): pass
@dataclass
class CValueListSettingCryString(CGenericUISettingCryString): pass
@dataclass
class CValueListSettingunsigned_long(CGenericUISettingunsigned_long): pass
@dataclass
class CVegetationObstructionEvent(CEntityEvent): pass
@dataclass
class CVegetationSlowdownComponent(CEntityComponent): pass
@dataclass
class CVehicle(CGameObject): pass
@dataclass
class CVehicleAction(CAgentAction): pass
@dataclass
class CVehicleAgent(CGameAgent): pass
@dataclass
class CVehicleDamagedPartEvent(CEntityEvent): pass
@dataclass
class CVehicleDecision(CAgentDecision): pass
@dataclass
class CVehicleEngineFloodedEvent(CEntityEvent): pass
@dataclass
class CVehicleEventExplosion(CEntityEvent): pass
@dataclass
class CVehicleEventIsDestructable(CEntityEvent): pass
@dataclass
class CVehicleEventSetEngineBroken(CEntityEvent): pass
@dataclass
class CVehicleFloatingPhysComponent(CVehiclePhysComponent): pass
@dataclass
class CVehicleMaterialComponent(CCustomMaterialComponent): pass
@dataclass
class CVehicleNetworkComponent(CNetworkComponent): pass
@dataclass
class CVehicleParagliderPhysComponent(CVehiclePhysComponent): pass
@dataclass
class CVehiclePhysComponent(CPhysComponent): pass
@dataclass
class CVehicleScanner(CAgentScanner): pass
@dataclass
class CVehicleSmartTerrain(CSmartTerrain): pass
@dataclass
class CVehicleSoundAndFXComponent(CObjectSoundAndFXComponent): pass
@dataclass
class CVehicleStateChangeEvent(CEntityEvent): pass
@dataclass
class CVehicleUserAccepted(CEntityEvent): pass
@dataclass
class CVehicleWheeledPhysComponent(CVehiclePhysComponent): pass
@dataclass
class CVisibilityOcclusionVolumeComponent(CEntityComponent): pass
@dataclass
class CVisibleObject(CGameAIObject): pass
@dataclass
class CVolumeCheckManager(CSingletonEntity): pass
forward(CVolumeCheckManager, CSingletonEntity)
@dataclass
class CVotingService(IGameModeService): pass
@dataclass
class CWagerRegion(CBasicRegionEntity): pass
@dataclass
class CWaterSoundManager(CSingletonEntity): pass
forward(CWaterSoundManager, CSingletonEntity)
@dataclass
class CWeapon(CEquipmentBase): pass
@dataclass
class CWeaponBazaar(IGameModeService): pass
@dataclass
class CWeaponEventBulletShot(CEntityEvent): pass
@dataclass
class CWeaponEventFireBullet(CEntityEvent): pass
@dataclass
class CWeaponEventReload(CEntityEvent): pass
@dataclass
class CWeaponFireBulletProperties(CWeaponFireProperties): pass
@dataclass
class CWeaponFireBulletStrategy(CWeaponFireStrategy): pass
@dataclass
class CWeaponFireFlameProperties(CWeaponFireProperties): pass
@dataclass
class CWeaponFireFlameStrategy(CWeaponFireStrategy): pass
@dataclass
class CWeaponFireMeleeProperties(CWeaponFireProperties): pass
@dataclass
class CWeaponFireMeleeStrategy(CWeaponFireStrategy): pass
@dataclass
class CWeaponFireProjectileProperties(CWeaponFireBulletProperties): pass
@dataclass
class CWeaponFireProjectileStrategy(CWeaponFireBulletStrategy): pass
@dataclass
class CWeaponFireProperties(CNomadObject): pass
@dataclass
class CWeaponFireStrategy(CEquipmentUseStrategy): pass
@dataclass
class CWeaponNetworkComponent(CNetworkComponent): pass
@dataclass
class CWeaponProperties(CEntityComponent): pass
@dataclass
class CWeaponsService(IGameModeService): pass
@dataclass
class CWeaponStimsCEntityEventStims(CEntityEventStims): pass
@dataclass
class CWeaponUsedEvent(CEntityEvent): pass
@dataclass
class CWorldSector(CResource): pass
@dataclass
class CXmlResource(CResource): pass

@dataclass
class CZoneInfoComponent(CEntityComponent):
	fSamplingRadius: float = field(default=0.0)
	uiGridSubdivisions: int = field(default=0)
	fDensityAdjustmentSpeed: float = field(default=0.0)
	fWeightScale: float = field(default=0.0)
	fWeightDistributionPower: float = field(default=0.0)

@dataclass
class CZoneLogicManager(CSingletonEntity): pass
forward(CZoneLogicManager, CSingletonEntity)
@dataclass
class CZoneLogicRegion(CBasicRegionEntity): pass
@dataclass
class CZoneSectorResource(CResource): pass
@dataclass
class IAuthorizationService(IGameModeService): pass
@dataclass
class ICollectionIgnitorComponent(CEntityComponent): pass
@dataclass
class ICountersService(IGameModeService): pass
@dataclass
class IGameMessageService(IGameModeService): pass
@dataclass
class IGameModeService(CNomadObject): pass
@dataclass
class IGameSoundService(IGameModeService): pass
@dataclass
class IGameStatsService(IGameModeService): pass
@dataclass
class IHostAdminService(IGameModeService): pass
@dataclass
class IMagmaDebugTextService(IGameModeService): pass
@dataclass
class IPlayerService(IGameModeService): pass
@dataclass
class IShapeComponent(CEntityComponent): pass
@dataclass
class IShapeEntity(COmniMapEntity): pass
@dataclass
class ISpawnPointService(IGameModeService): pass
@dataclass
class ITeamManager(IGameModeService): pass
@dataclass
class SDecalDescription(CNomadDbObjectNamed): pass
@dataclass
class SInventoryViewPawnImpl(CNomadObject):
	x_8C965C28: str = field(default="")
	packInventoryPack: int = field(default=0)
	archGPSVehicleArchetype: bytes = field(default_factory=bytes)
	bUnlimitedAmmo: bool = field(default=False)
	bAutoReload: bool = field(default=False)
	bAutoDraw: bool = field(default=False)
	x_130CDED8: str = field(default="")
	sInitialWeaponCategory: int = field(default=0)
@dataclass
class SMixingPreset(CNomadObject): pass
@dataclass
class SPhysMaterial(CNomadDbObjectNamed): pass
@dataclass
class SSettings(CNomadDbObjectNamed): pass
@dataclass
class SSoundPoint(CNomadDbObject): pass
@dataclass
class StCollectionResInfo(CNomadObject): pass

#endregion


#xx