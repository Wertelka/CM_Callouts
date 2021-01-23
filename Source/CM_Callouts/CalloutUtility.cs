﻿using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace CM_Callouts
{
    public static class CalloutUtility
    {
        public static void AttemptDraftedCallout(Pawn pawn)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                if (calloutTracker.CanCalloutNow(pawn))
                {
                    RulePackDef rulePack = CalloutDefOf.CM_Callouts_RulePack_Drafted;

                    GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };
                    CollectPawnRules(pawn, "INITIATOR", ref grammarRequest);

                    calloutTracker.RequestCallout(pawn, rulePack, grammarRequest);
                }
            }
        }

        public static void AttemptMeleeAttackCallout(PendingCallout pendingCallout)
        {
            AttemptMeleeAttackCallout(pendingCallout.initiator, pendingCallout.recipient);
        }

        public static void AttemptMeleeAttackCallout(Pawn initiator, Pawn recipient)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                if (calloutTracker.CanCalloutNow(initiator))
                {
                    RulePackDef rulePack = CalloutDefOf.CM_Callouts_RulePack_Melee_Attack;

                    GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };

                    CollectPawnRules(initiator, "INITIATOR", ref grammarRequest);

                    if (recipient != null)
                        CollectPawnRules(recipient, "RECIPIENT", ref grammarRequest);

                    calloutTracker.RequestCallout(initiator, rulePack, grammarRequest);
                }
            }
        }

        public static void AttemptMeleeAttackLandedCallout(PendingCallout pendingCallout, BodyDef body, List<BodyPartRecord> bodyParts, List<bool> bodyPartsDestroyed)
        {
            AttemptMeleeAttackLandedCallout(pendingCallout.initiator, pendingCallout.recipient, body, bodyParts, bodyPartsDestroyed);
        }

        public static void AttemptMeleeAttackLandedCallout(Pawn initiator, Pawn recipient, BodyDef body, List<BodyPartRecord> bodyParts, List<bool> bodyPartsDestroyed)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                bool recipientCallout = Rand.Bool;

                if ((recipientCallout && calloutTracker.CanCalloutNow(recipient)) || (!recipientCallout && calloutTracker.CanCalloutNow(initiator)))
                {
                    RulePackDef rulePack = CalloutDefOf.CM_Callouts_RulePack_Melee_Attack_Landed;
                    if (recipientCallout)
                        rulePack = CalloutDefOf.CM_Callouts_RulePack_Melee_Attack_Received;

                    GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };

                    CollectPawnRules(initiator, "INITIATOR", ref grammarRequest);

                    if (recipient != null)
                    {
                        CollectPawnRules(recipient, "RECIPIENT", ref grammarRequest);
                        grammarRequest.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("recipient_part", body, bodyParts, bodyPartsDestroyed, grammarRequest.Constants));
                    }

                    if (!recipientCallout)
                        calloutTracker.RequestCallout(initiator, rulePack, grammarRequest);
                    else
                        calloutTracker.RequestCallout(recipient, rulePack, grammarRequest);
                }
            }
        }

        public static void AttemptRangedAttackCallout(Pawn pawn, Verb_LaunchProjectile verb)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                if (calloutTracker.CanCalloutNow(pawn))
                {
                    RulePackDef rulePack = CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack;

                    GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };

                    CollectPawnRules(pawn, "INITIATOR", ref grammarRequest);

                    if (verb.CurrentTarget.Pawn != null)
                    {
                        CollectCoverRules(verb.CurrentTarget.Pawn, pawn, "INITIATOR_COVER", verb, ref grammarRequest);

                        CollectPawnRules(verb.CurrentTarget.Pawn, "RECIPIENT", ref grammarRequest);
                        CollectCoverRules(pawn, verb.CurrentTarget.Pawn, "RECIPIENT_COVER", verb, ref grammarRequest);
                    }

                    calloutTracker.RequestCallout(pawn, rulePack, grammarRequest);
                }
            }
        }

        public static void AttemptRangedAttackImpactCallout(PendingCallout pendingCallout, BodyDef body, List<BodyPartRecord> bodyParts, List<bool> bodyPartsDestroyed)
        {
            AttemptRangedAttackImpactCallout(pendingCallout.initiator, pendingCallout.recipient, pendingCallout.originalTarget,
                                             pendingCallout.weaponDef, pendingCallout.projectileDef, pendingCallout.coverDef,
                                             body, bodyParts, bodyPartsDestroyed);
        }

        public static void AttemptRangedAttackImpactCallout(Pawn initiator, Pawn recipient, Pawn originalTarget, ThingDef weaponDef, ThingDef projectileDef, ThingDef coverDef, BodyDef body, List<BodyPartRecord> bodyParts, List<bool> bodyPartsDestroyed)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                if (calloutTracker.CanCalloutNow(initiator))
                {
                    RulePackDef rulePack = CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack_Impact;

                    GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };

                    CollectPawnRules(initiator, "INITIATOR", ref grammarRequest);

                    if (recipient != null)
                    {
                        CollectPawnRules(recipient, "RECIPIENT", ref grammarRequest);
                        grammarRequest.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("recipient_part", body, bodyParts, bodyPartsDestroyed, grammarRequest.Constants));

                        if (coverDef != null)
                            grammarRequest.Rules.AddRange(GrammarUtility.RulesForDef("RECIPIENT_COVER", coverDef));
                    }

                    calloutTracker.RequestCallout(initiator, rulePack, grammarRequest);
                }
            }
        }

        private static void CollectPawnRules(Pawn pawn, string symbol, ref GrammarRequest grammarRequest)
        {
            grammarRequest.Rules.AddRange(GrammarUtility.RulesForPawn(symbol, pawn));
            CollectWeaponRules(pawn, symbol + "_WEAPON", ref grammarRequest);
        }

        private static void CollectWeaponRules(Pawn pawn, string symbol, ref GrammarRequest grammarRequest)
        {
            if (pawn.equipment != null && pawn.equipment.Primary != null)
                grammarRequest.Rules.AddRange(GetRulesForWeapon(symbol, pawn.equipment.Primary));
        }

        private static void CollectCoverRules(Pawn pawn, Pawn target, string symbol, Verb_LaunchProjectile verb, ref GrammarRequest grammarRequest)
        {
            ShotReport shotReport = ShotReport.HitReportFor(pawn, verb, target);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = randomCoverToMissInto?.def;

            if (targetCoverDef != null)
                grammarRequest.Rules.AddRange(GrammarUtility.RulesForDef(symbol, targetCoverDef));
        }

        private static IEnumerable<Rule> GetRulesForWeapon(string prefix, Thing thing)
        {
            ThingDef projectileDef = null;
            if (thing.def.Verbs != null && thing.def.Verbs.Count > 0)
                projectileDef = thing.def.Verbs[0].defaultProjectile;
            foreach (Rule_String defRule in PlayLogEntryUtility.RulesForOptionalWeapon(prefix, thing.def, projectileDef))
                yield return defRule;

            //yield return new Rule_String(prefix + "_label", thing.def.label);
            //yield return new Rule_String(prefix + "_definite", Find.ActiveLanguageWorker.WithDefiniteArticle(thing.def.label));
            //yield return new Rule_String(prefix + "_indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.def.label));
            if (thing.Stuff != null)
            {
                yield return new Rule_String(prefix + "_stuffLabel", thing.Stuff.label);
            }
            CompArt compArt = thing.TryGetComp<CompArt>();
            if (compArt != null && compArt.Active)
            {
                yield return new Rule_String(prefix + "_title", compArt.Title);
            }
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                yield return new Rule_String(prefix + "_quality", compQuality.Quality.GetLabel());
            }
        }

    }
}
