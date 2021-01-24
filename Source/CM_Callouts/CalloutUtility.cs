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
        public static PendingCalloutEvent pendingCallout = null;

        public static bool CanCalloutNow(Thing thing)
        {
            Pawn caller = thing as Pawn;

            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (caller != null && calloutTracker != null)
                return calloutTracker.CanCalloutNow(caller);

            return false;
        }

        public static bool CanCalloutAtTarget(Thing target)
        {
            Pawn targetPawn = target as Pawn;

            return (targetPawn != null && (CalloutMod.settings.allowCalloutsWhenTargetingAnimals || targetPawn.RaceProps.Humanlike));
        }

        public static void AttemptDraftedCallout(Pawn pawn)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                if (CalloutUtility.CanCalloutNow(pawn))
                {
                    RulePackDef rulePack = CalloutDefOf.CM_Callouts_RulePack_Drafted;

                    GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };
                    CollectPawnRules(pawn, "INITIATOR", ref grammarRequest);

                    calloutTracker.RequestCallout(pawn, rulePack, grammarRequest);
                }
            }
        }

        public static void AttemptMeleeAttackCallout(Pawn initiator, Pawn recipient)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                RulePackDef rulePack = CalloutDefOf.CM_Callouts_RulePack_Melee_Attack;

                GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };

                CollectPawnRules(initiator, "INITIATOR", ref grammarRequest);

                if (recipient != null)
                    CollectPawnRules(recipient, "RECIPIENT", ref grammarRequest);

                calloutTracker.RequestCallout(initiator, rulePack, grammarRequest);
            }
        }

        public static void AttemptRangedAttackCallout(Pawn pawn, Verb_LaunchProjectile verb)
        {
            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
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

        public static void CollectPawnRules(Pawn pawn, string symbol, ref GrammarRequest grammarRequest)
        {
            grammarRequest.Rules.AddRange(GrammarUtility.RulesForPawn(symbol, pawn));
            CollectWeaponRules(pawn, symbol + "_WEAPON", ref grammarRequest);
        }

        public static void CollectWeaponRules(Pawn pawn, string symbol, ref GrammarRequest grammarRequest)
        {
            if (pawn.equipment != null && pawn.equipment.Primary != null)
                grammarRequest.Rules.AddRange(GetRulesForWeapon(symbol, pawn.equipment.Primary));
        }

        public static void CollectCoverRules(Pawn pawn, Pawn target, string symbol, Verb_LaunchProjectile verb, ref GrammarRequest grammarRequest)
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

        public static void DrawText(Vector2 worldPos, string text, Color textColor)
        {
            Vector3 position = new Vector3(worldPos.x, 0f, worldPos.y);
            Vector2 vector = Find.Camera.WorldToScreenPoint(position) / Prefs.UIScale;
            vector.y = (float)UI.screenHeight - vector.y;
            Text.Font = GameFont.Tiny;
            float x = Text.CalcSize(text).x;
            GUI.DrawTexture(new Rect(vector.x - x / 2f - 4f, vector.y, x + 8f, 12f), TexUI.GrayTextBG);
            GUI.color = textColor;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(new Rect(vector.x - x / 2f, vector.y - 2f, x, 999f), text);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
