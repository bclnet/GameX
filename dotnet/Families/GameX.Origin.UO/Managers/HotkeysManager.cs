// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO HotKeyCombination. Key and Mod are ints, the same values OnKeyDown receives.
/// </summary>
public class HotKeyCombination {
    public int Key { get; set; }
    public int Mod { get; set; }
    public HotkeyAction KeyAction { get; set; }
}

/// <summary>
/// ClassicUO HotkeysManager. The spell handlers are the in-memory action table.
/// Key rows stay empty until Bind. This type does not subscribe to a key hook.
/// </summary>
public class HotkeysManager {
    // Profile path is not ported.
    readonly Dictionary<HotkeyAction, Action> _actions = new Dictionary<HotkeyAction, Action>();
    readonly List<HotKeyCombination> _hotkeys = new List<HotKeyCombination>();

    public HotkeysManager() {
        Add(HotkeyAction.CastClumsy, () => UOLoginNet.CastSpell(1));
        Add(HotkeyAction.CastCreateFood, () => UOLoginNet.CastSpell(2));
        Add(HotkeyAction.CastFeeblemind, () => UOLoginNet.CastSpell(3));
        Add(HotkeyAction.CastHeal, () => UOLoginNet.CastSpell(4));
        Add(HotkeyAction.CastMagicArrow, () => UOLoginNet.CastSpell(5));
        Add(HotkeyAction.CastNightSight, () => UOLoginNet.CastSpell(6));
        Add(HotkeyAction.CastReactiveArmor, () => UOLoginNet.CastSpell(7));
        Add(HotkeyAction.CastWeaken, () => UOLoginNet.CastSpell(8));
        Add(HotkeyAction.CastAgility, () => UOLoginNet.CastSpell(9));
        Add(HotkeyAction.CastCunning, () => UOLoginNet.CastSpell(10));
        Add(HotkeyAction.CastCure, () => UOLoginNet.CastSpell(11));
        Add(HotkeyAction.CastHarm, () => UOLoginNet.CastSpell(12));
        Add(HotkeyAction.CastMagicTrap, () => UOLoginNet.CastSpell(13));
        Add(HotkeyAction.CastMagicUntrap, () => UOLoginNet.CastSpell(14));
        Add(HotkeyAction.CastProtection, () => UOLoginNet.CastSpell(15));
        Add(HotkeyAction.CastStrength, () => UOLoginNet.CastSpell(16));
        Add(HotkeyAction.CastBless, () => UOLoginNet.CastSpell(17));
        Add(HotkeyAction.CastFireball, () => UOLoginNet.CastSpell(18));
        Add(HotkeyAction.CastMagicLock, () => UOLoginNet.CastSpell(19));
        Add(HotkeyAction.CastPosion, () => UOLoginNet.CastSpell(20));
        Add(HotkeyAction.CastTelekinesis, () => UOLoginNet.CastSpell(21));
        Add(HotkeyAction.CastTeleport, () => UOLoginNet.CastSpell(22));
        Add(HotkeyAction.CastUnlock, () => UOLoginNet.CastSpell(23));
        Add(HotkeyAction.CastWallOfStone, () => UOLoginNet.CastSpell(24));
        Add(HotkeyAction.CastArchCure, () => UOLoginNet.CastSpell(25));
        Add(HotkeyAction.CastArchProtection, () => UOLoginNet.CastSpell(26));
        Add(HotkeyAction.CastCurse, () => UOLoginNet.CastSpell(27));
        Add(HotkeyAction.CastFireField, () => UOLoginNet.CastSpell(28));
        Add(HotkeyAction.CastGreaterHeal, () => UOLoginNet.CastSpell(29));
        Add(HotkeyAction.CastLightning, () => UOLoginNet.CastSpell(30));
        Add(HotkeyAction.CastManaDrain, () => UOLoginNet.CastSpell(31));
        Add(HotkeyAction.CastRecall, () => UOLoginNet.CastSpell(32));
        Add(HotkeyAction.CastBladeSpirits, () => UOLoginNet.CastSpell(33));
        Add(HotkeyAction.CastDispelField, () => UOLoginNet.CastSpell(34));
        Add(HotkeyAction.CastIncognito, () => UOLoginNet.CastSpell(35));
        Add(HotkeyAction.CastMagicReflection, () => UOLoginNet.CastSpell(36));
        Add(HotkeyAction.CastMindBlast, () => UOLoginNet.CastSpell(37));
        Add(HotkeyAction.CastParalyze, () => UOLoginNet.CastSpell(38));
        Add(HotkeyAction.CastPoisonField, () => UOLoginNet.CastSpell(39));
        Add(HotkeyAction.CastSummonCreature, () => UOLoginNet.CastSpell(40));
        Add(HotkeyAction.CastDispel, () => UOLoginNet.CastSpell(41));
        Add(HotkeyAction.CastEnergyBolt, () => UOLoginNet.CastSpell(42));
        Add(HotkeyAction.CastExplosion, () => UOLoginNet.CastSpell(43));
        Add(HotkeyAction.CastInvisibility, () => UOLoginNet.CastSpell(44));
        Add(HotkeyAction.CastMark, () => UOLoginNet.CastSpell(45));
        Add(HotkeyAction.CastMassCurse, () => UOLoginNet.CastSpell(46));
        Add(HotkeyAction.CastParalyzeField, () => UOLoginNet.CastSpell(47));
        Add(HotkeyAction.CastReveal, () => UOLoginNet.CastSpell(48));
        Add(HotkeyAction.CastChainLightning, () => UOLoginNet.CastSpell(49));
        Add(HotkeyAction.CastEnergyField, () => UOLoginNet.CastSpell(50));
        Add(HotkeyAction.CastFlamestrike, () => UOLoginNet.CastSpell(51));
        Add(HotkeyAction.CastGateTravel, () => UOLoginNet.CastSpell(52));
        Add(HotkeyAction.CastManaVampire, () => UOLoginNet.CastSpell(53));
        Add(HotkeyAction.CastMassDispel, () => UOLoginNet.CastSpell(54));
        Add(HotkeyAction.CastMeteorSwam, () => UOLoginNet.CastSpell(55));
        Add(HotkeyAction.CastPolymorph, () => UOLoginNet.CastSpell(56));
        Add(HotkeyAction.CastEarthquake, () => UOLoginNet.CastSpell(57));
        Add(HotkeyAction.CastEnergyVortex, () => UOLoginNet.CastSpell(58));
        Add(HotkeyAction.CastResurrection, () => UOLoginNet.CastSpell(59));
        Add(HotkeyAction.CastAirElemental, () => UOLoginNet.CastSpell(60));
        Add(HotkeyAction.CastSummonDaemon, () => UOLoginNet.CastSpell(61));
        Add(HotkeyAction.CastEarthElemental, () => UOLoginNet.CastSpell(62));
        Add(HotkeyAction.CastFireElemental, () => UOLoginNet.CastSpell(63));
        Add(HotkeyAction.CastWaterElemental, () => UOLoginNet.CastSpell(64));

        Add(HotkeyAction.CastAnimatedDead, () => UOLoginNet.CastSpell(101));
        Add(HotkeyAction.CastBloodOath, () => UOLoginNet.CastSpell(102));
        Add(HotkeyAction.CastCorpseSkin, () => UOLoginNet.CastSpell(103));
        Add(HotkeyAction.CastCurseWeapon, () => UOLoginNet.CastSpell(104));
        Add(HotkeyAction.CastEvilOmen, () => UOLoginNet.CastSpell(105));
        Add(HotkeyAction.CastHorrificBeast, () => UOLoginNet.CastSpell(106));
        Add(HotkeyAction.CastLichForm, () => UOLoginNet.CastSpell(107));
        Add(HotkeyAction.CastMindRot, () => UOLoginNet.CastSpell(108));
        Add(HotkeyAction.CastPainSpike, () => UOLoginNet.CastSpell(109));
        Add(HotkeyAction.CastPoisonStrike, () => UOLoginNet.CastSpell(110));
        Add(HotkeyAction.CastStrangle, () => UOLoginNet.CastSpell(111));
        Add(HotkeyAction.CastSummonFamiliar, () => UOLoginNet.CastSpell(112));
        Add(HotkeyAction.CastVampiricEmbrace, () => UOLoginNet.CastSpell(113));
        Add(HotkeyAction.CastVangefulSpririt, () => UOLoginNet.CastSpell(114));
        Add(HotkeyAction.CastWither, () => UOLoginNet.CastSpell(115));
        Add(HotkeyAction.CastWraithForm, () => UOLoginNet.CastSpell(116));
        Add(HotkeyAction.CastExorcism, () => UOLoginNet.CastSpell(117));

        Add(HotkeyAction.CastCleanseByFire, () => UOLoginNet.CastSpell(201));
        Add(HotkeyAction.CastCloseWounds, () => UOLoginNet.CastSpell(202));
        Add(HotkeyAction.CastConsecrateWeapon, () => UOLoginNet.CastSpell(203));
        Add(HotkeyAction.CastDispelEvil, () => UOLoginNet.CastSpell(204));
        Add(HotkeyAction.CastDivineFury, () => UOLoginNet.CastSpell(205));
        Add(HotkeyAction.CastEnemyOfOne, () => UOLoginNet.CastSpell(206));
        Add(HotkeyAction.CastHolyLight, () => UOLoginNet.CastSpell(207));
        Add(HotkeyAction.CastNobleSacrifice, () => UOLoginNet.CastSpell(208));
        Add(HotkeyAction.CastRemoveCurse, () => UOLoginNet.CastSpell(209));
        Add(HotkeyAction.CastSacredJourney, () => UOLoginNet.CastSpell(210));

        Add(HotkeyAction.CastHonorableExecution, () => UOLoginNet.CastSpell(401));
        Add(HotkeyAction.CastConfidence, () => UOLoginNet.CastSpell(402));
        Add(HotkeyAction.CastEvasion, () => UOLoginNet.CastSpell(403));
        Add(HotkeyAction.CastCounterAttack, () => UOLoginNet.CastSpell(404));
        Add(HotkeyAction.CastLightningStrike, () => UOLoginNet.CastSpell(405));
        Add(HotkeyAction.CastMomentumStrike, () => UOLoginNet.CastSpell(406));

        Add(HotkeyAction.CastFocusAttack, () => UOLoginNet.CastSpell(501));
        Add(HotkeyAction.CastDeathStrike, () => UOLoginNet.CastSpell(502));
        Add(HotkeyAction.CastAnimalForm, () => UOLoginNet.CastSpell(503));
        Add(HotkeyAction.CastKiAttack, () => UOLoginNet.CastSpell(504));
        Add(HotkeyAction.CastSurpriseAttack, () => UOLoginNet.CastSpell(505));
        Add(HotkeyAction.CastBackstab, () => UOLoginNet.CastSpell(506));
        Add(HotkeyAction.CastShadowjump, () => UOLoginNet.CastSpell(507));
        Add(HotkeyAction.CastMirrorImage, () => UOLoginNet.CastSpell(508));

        Add(HotkeyAction.CastArcaneCircle, () => UOLoginNet.CastSpell(601));
        Add(HotkeyAction.CastGiftOfRenewal, () => UOLoginNet.CastSpell(602));
        Add(HotkeyAction.CastImmolatingWeapon, () => UOLoginNet.CastSpell(603));
        Add(HotkeyAction.CastAttuneWeapon, () => UOLoginNet.CastSpell(604));
        Add(HotkeyAction.CastThinderstorm, () => UOLoginNet.CastSpell(605));
        Add(HotkeyAction.CastNaturesFury, () => UOLoginNet.CastSpell(606));
        Add(HotkeyAction.CastSummonFey, () => UOLoginNet.CastSpell(607));
        Add(HotkeyAction.CastSummonFiend, () => UOLoginNet.CastSpell(608));
        Add(HotkeyAction.CastReaperForm, () => UOLoginNet.CastSpell(609));
        Add(HotkeyAction.CastWildFire, () => UOLoginNet.CastSpell(610));
        Add(HotkeyAction.CastEssenceOfWind, () => UOLoginNet.CastSpell(611));
        Add(HotkeyAction.CastDryadAllure, () => UOLoginNet.CastSpell(612));
        Add(HotkeyAction.CastEtherealVoyage, () => UOLoginNet.CastSpell(613));
        Add(HotkeyAction.CastWordOfDeath, () => UOLoginNet.CastSpell(614));
        Add(HotkeyAction.CastGiftOfLife, () => UOLoginNet.CastSpell(615));

        Add(HotkeyAction.CastNetherBolt, () => UOLoginNet.CastSpell(678));
        Add(HotkeyAction.CastHealingStone, () => UOLoginNet.CastSpell(679));
        Add(HotkeyAction.CastPurgeMagic, () => UOLoginNet.CastSpell(680));
        Add(HotkeyAction.CastEnchant, () => UOLoginNet.CastSpell(681));
        Add(HotkeyAction.CastSleep, () => UOLoginNet.CastSpell(682));
        Add(HotkeyAction.CastEagleStrike, () => UOLoginNet.CastSpell(683));
        Add(HotkeyAction.CastAnimatedWeapon, () => UOLoginNet.CastSpell(684));
        Add(HotkeyAction.CastStoneForm, () => UOLoginNet.CastSpell(685));
        Add(HotkeyAction.CastSpellTrigger, () => UOLoginNet.CastSpell(686));
        Add(HotkeyAction.CastMassSleep, () => UOLoginNet.CastSpell(687));
        Add(HotkeyAction.CastCleansingWinds, () => UOLoginNet.CastSpell(688));
        Add(HotkeyAction.CastBombard, () => UOLoginNet.CastSpell(689));
        Add(HotkeyAction.CastSpellPlague, () => UOLoginNet.CastSpell(690));
        Add(HotkeyAction.CastHailStorm, () => UOLoginNet.CastSpell(691));
        Add(HotkeyAction.CastNetherCyclone, () => UOLoginNet.CastSpell(692));
        Add(HotkeyAction.CastRisingColossus, () => UOLoginNet.CastSpell(693));

        Add(HotkeyAction.CastInspire, () => UOLoginNet.CastSpell(701));
        Add(HotkeyAction.CastInvigorate, () => UOLoginNet.CastSpell(702));
        Add(HotkeyAction.CastResilience, () => UOLoginNet.CastSpell(703));
        Add(HotkeyAction.CastPerseverance, () => UOLoginNet.CastSpell(704));
        Add(HotkeyAction.CastTribulation, () => UOLoginNet.CastSpell(705));
        Add(HotkeyAction.CastDespair, () => UOLoginNet.CastSpell(706));

        Add(HotkeyAction.CastDeathRay, () => UOLoginNet.CastSpell(707));
        Add(HotkeyAction.CastEtherealBurst, () => UOLoginNet.CastSpell(708));
        Add(HotkeyAction.CastNetherBlast, () => UOLoginNet.CastSpell(709));
        Add(HotkeyAction.CastMysticWeapon, () => UOLoginNet.CastSpell(710));
        Add(HotkeyAction.CastCommandUndead, () => UOLoginNet.CastSpell(711));
        Add(HotkeyAction.CastConduit, () => UOLoginNet.CastSpell(712));
        Add(HotkeyAction.CastManaShield, () => UOLoginNet.CastSpell(713));
        Add(HotkeyAction.CastSummonReaper, () => UOLoginNet.CastSpell(714));
        Add(HotkeyAction.CastEnchantedSummoning, () => UOLoginNet.CastSpell(715));
        Add(HotkeyAction.CastAnticipateHit, () => UOLoginNet.CastSpell(716));
        Add(HotkeyAction.CastWarcry, () => UOLoginNet.CastSpell(717));
        Add(HotkeyAction.CastIntuition, () => UOLoginNet.CastSpell(718));
        Add(HotkeyAction.CastRejuvenate, () => UOLoginNet.CastSpell(719));
        Add(HotkeyAction.CastHolyFist, () => UOLoginNet.CastSpell(720));
        Add(HotkeyAction.CastShadow, () => UOLoginNet.CastSpell(721));
        Add(HotkeyAction.CastWhiteTigerForm, () => UOLoginNet.CastSpell(722));
        Add(HotkeyAction.CastFlamingShot, () => UOLoginNet.CastSpell(723));
        Add(HotkeyAction.CastPlayingTheOdds, () => UOLoginNet.CastSpell(724));
        Add(HotkeyAction.CastThrust, () => UOLoginNet.CastSpell(725));
        Add(HotkeyAction.CastPierce, () => UOLoginNet.CastSpell(726));
        Add(HotkeyAction.CastStagger, () => UOLoginNet.CastSpell(727));
        Add(HotkeyAction.CastToughness, () => UOLoginNet.CastSpell(728));
        Add(HotkeyAction.CastOnslaught, () => UOLoginNet.CastSpell(729));
        Add(HotkeyAction.CastFocusedEye, () => UOLoginNet.CastSpell(730));
        Add(HotkeyAction.CastElementalFury, () => UOLoginNet.CastSpell(731));
        Add(HotkeyAction.CastCalledShot, () => UOLoginNet.CastSpell(732));
        Add(HotkeyAction.CastWarriorsGifts, () => UOLoginNet.CastSpell(733));
        Add(HotkeyAction.CastShieldBash, () => UOLoginNet.CastSpell(734));
        Add(HotkeyAction.CastBodyguard, () => UOLoginNet.CastSpell(735));
        Add(HotkeyAction.CastHeightenSenses, () => UOLoginNet.CastSpell(736));
        Add(HotkeyAction.CastTolerance, () => UOLoginNet.CastSpell(737));
        Add(HotkeyAction.CastInjectedStrike, () => UOLoginNet.CastSpell(738));
        Add(HotkeyAction.CastPotency, () => UOLoginNet.CastSpell(739));
        Add(HotkeyAction.CastRampage, () => UOLoginNet.CastSpell(740));
        Add(HotkeyAction.CastFistsofFury, () => UOLoginNet.CastSpell(741));
        Add(HotkeyAction.CastKnockout, () => UOLoginNet.CastSpell(742));
        Add(HotkeyAction.CastWhispering, () => UOLoginNet.CastSpell(743));
        Add(HotkeyAction.CastCombatTraining, () => UOLoginNet.CastSpell(744));
        Add(HotkeyAction.CastBoarding, () => UOLoginNet.CastSpell(745));
    }

    public bool Bind(HotkeyAction action, int key, int mod) {
        foreach (HotKeyCombination h in _hotkeys) {
            if (h.Key == key && h.Mod == mod) return false;
        }
        _hotkeys.Add(new HotKeyCombination {
            Key = key,
            Mod = mod,
            KeyAction = action
        });
        return true;
    }

    public void UnBind(HotkeyAction action) {
        for (var i = 0; i < _hotkeys.Count; i++) {
            HotKeyCombination h = _hotkeys[i];
            if (h.KeyAction == action) {
                _hotkeys.RemoveAt(i);
                break;
            }
        }
    }

    public bool TryExecuteIfBinded(int key, int mod, out Action action) {
        for (var i = 0; i < _hotkeys.Count; i++) {
            HotKeyCombination h = _hotkeys[i];
            if (h.Key == key && h.Mod == mod) {
                if (_actions.TryGetValue(h.KeyAction, out action)) return true;
                break;
            }
        }
        action = null;
        return false;
    }

    public Dictionary<HotkeyAction, Action> GetValues() {
        return _actions;
    }

    void Add(HotkeyAction action, Action handler) {
        _actions.Add(action, handler);
    }
}

public enum HotkeyAction {
    None,

    #region Magery

    CastClumsy,
    CastCreateFood,
    CastFeeblemind,
    CastHeal,
    CastMagicArrow,
    CastNightSight,
    CastReactiveArmor,
    CastWeaken,
    CastAgility,
    CastCunning,
    CastCure,
    CastHarm,
    CastMagicTrap,
    CastMagicUntrap,
    CastProtection,
    CastStrength,
    CastBless,
    CastFireball,
    CastMagicLock,
    CastPosion,
    CastTelekinesis,
    CastTeleport,
    CastUnlock,
    CastWallOfStone,
    CastArchCure,
    CastArchProtection,
    CastCurse,
    CastFireField,
    CastGreaterHeal,
    CastLightning,
    CastManaDrain,
    CastRecall,
    CastBladeSpirits,
    CastDispelField,
    CastIncognito,
    CastMagicReflection,
    CastMindBlast,
    CastParalyze,
    CastPoisonField,
    CastSummonCreature,
    CastDispel,
    CastEnergyBolt,
    CastExplosion,
    CastInvisibility,
    CastMark,
    CastMassCurse,
    CastParalyzeField,
    CastReveal,
    CastChainLightning,
    CastEnergyField,
    CastFlamestrike,
    CastGateTravel,
    CastManaVampire,
    CastMassDispel,
    CastMeteorSwam,
    CastPolymorph,
    CastEarthquake,
    CastEnergyVortex,
    CastResurrection,
    CastAirElemental,
    CastSummonDaemon,
    CastEarthElemental,
    CastFireElemental,
    CastWaterElemental,

    #endregion

    #region Necro

    CastAnimatedDead,
    CastBloodOath,
    CastCorpseSkin,
    CastCurseWeapon,
    CastEvilOmen,
    CastHorrificBeast,
    CastLichForm,
    CastMindRot,
    CastPainSpike,
    CastPoisonStrike,
    CastStrangle,
    CastSummonFamiliar,
    CastVampiricEmbrace,
    CastVangefulSpririt,
    CastWither,
    CastWraithForm,
    CastExorcism,

    #endregion

    #region Chivalry

    CastCleanseByFire,
    CastCloseWounds,
    CastConsecrateWeapon,
    CastDispelEvil,
    CastDivineFury,
    CastEnemyOfOne,
    CastHolyLight,
    CastNobleSacrifice,
    CastRemoveCurse,
    CastSacredJourney,

    #endregion

    #region Bushido

    CastHonorableExecution,
    CastConfidence,
    CastEvasion,
    CastCounterAttack,
    CastLightningStrike,
    CastMomentumStrike,

    #endregion

    #region Ninja

    CastFocusAttack,
    CastDeathStrike,
    CastAnimalForm,
    CastKiAttack,
    CastSurpriseAttack,
    CastBackstab,
    CastShadowjump,
    CastMirrorImage,

    #endregion

    #region Spellweaving

    CastArcaneCircle,
    CastGiftOfRenewal,
    CastImmolatingWeapon,
    CastAttuneWeapon,
    CastThinderstorm,
    CastNaturesFury,
    CastSummonFey,
    CastSummonFiend,
    CastReaperForm,
    CastWildFire,
    CastEssenceOfWind,
    CastDryadAllure,
    CastEtherealVoyage,
    CastWordOfDeath,
    CastGiftOfLife,
    CastArcaneEmpowerment,

    #endregion

    #region Mysticism

    CastNetherBolt,
    CastHealingStone,
    CastPurgeMagic,
    CastEnchant,
    CastSleep,
    CastEagleStrike,
    CastAnimatedWeapon,
    CastStoneForm,
    CastSpellTrigger,
    CastMassSleep,
    CastCleansingWinds,
    CastBombard,
    CastSpellPlague,
    CastHailStorm,
    CastNetherCyclone,
    CastRisingColossus,

    #endregion

    #region Bardic

    CastInspire,
    CastInvigorate,
    CastResilience,
    CastPerseverance,
    CastTribulation,
    CastDespair,

    #endregion

    #region Other mastery spells

    CastDeathRay,
    CastEtherealBurst,
    CastNetherBlast,
    CastMysticWeapon,
    CastCommandUndead,
    CastConduit,
    CastManaShield,
    CastSummonReaper,
    CastEnchantedSummoning,
    CastAnticipateHit,
    CastWarcry,
    CastIntuition,
    CastRejuvenate,
    CastHolyFist,
    CastShadow,
    CastWhiteTigerForm,
    CastFlamingShot,
    CastPlayingTheOdds,
    CastThrust,
    CastPierce,
    CastStagger,
    CastToughness,
    CastOnslaught,
    CastFocusedEye,
    CastElementalFury,
    CastCalledShot,
    CastWarriorsGifts,
    CastShieldBash,
    CastBodyguard,
    CastHeightenSenses,
    CastTolerance,
    CastInjectedStrike,
    CastPotency,
    CastRampage,
    CastFistsofFury,
    CastKnockout,
    CastWhispering,
    CastCombatTraining,
    CastBoarding,

    #endregion

    #region Skills

    UseSkillAnatomy,
    UseSkillAnimalLore,
    UseSkillAnimalTaming,
    UseSkillArmsLore,
    UseSkillBegging,
    UseSkillCartography,
    UseSkillDetectingHidden,
    UseSkillEnticement,
    UseSkillEvaluatingIntelligence,
    UseSkillForensicEvaluation,
    UseSkillHiding,
    UseSkillImbuing,
    UseSkillInscription,
    UseSkillItemIdentificator,
    UseSkillMeditation,
    UseSkillPeacemaking,
    UseSkillPoisoning,
    UseSkillProvocation,
    UseSkillRemoveTrap,
    UseSkillSpiritSpeak,
    UseSkillStealing,
    UseSkillStealth,
    UseSkillTasteIdentification,
    UseSkillTracking,

    #endregion

    #region Virtues

    UseVirtueHonor,
    UseVirtueSacrifice,
    UseVirtueValor,

    #endregion

    #region WalkDir

    WalkToNW,
    WalkToN,
    WalkToNE,
    WalkToE,
    WalkToSE,
    WalkToS,
    WalkToSW,
    WalkToW,

    #endregion

    #region GumpAction

    OpenSettings,
    OpenPaperdoll,
    OpenStatus,
    OpenJournal,
    OpenSkills,
    OpenMageSpellbook,
    OpenNecroSpellbook,
    OpenChivaSpellbook,
    OpenBushidoSpellbook,
    OpenNinjaSpellbook,
    OpenSpellweaverSpellbook,
    OpenMysticSpellbook,
    OpenRacialAbilitiesBook,
    OpenChat,
    OpenBackpack,
    OpenMinimap,
    OpenParty,
    OpenPartyChat,
    OpenGuild,
    OpenQuestLog,
    ToggleBuffGump,
    QuitGame,
    SaveGumps,

    #endregion

    #region Abilities

    UsePrimaryAbility,
    UseSecondaryAbility,
    ClearCurrentAbility,
    ToggleGargoyleFly,

    #endregion

    #region UseItems

    UseSelectedItem,
    UseCurrentTarget,
    BandageSelf,
    BandageTarget,

    #endregion

    #region Speech

    Say,
    Emote,
    Whisper,
    Yell,

    #endregion

    #region Targetting

    TargetNext,
    TargetClosest,
    TargetSelf,

    #endregion

    #region Attack

    AttackLast,
    AttackSelected,

    #endregion

    #region SelectTarget

    SelectNext,
    SelectPrevious,
    SelectNearest,

    #endregion

    #region Misc

    ArmDisarm,
    AllNames,
    Bow,
    Salute,
    AlwaysRun,
    EquipLastWeapon,

    #endregion
}
