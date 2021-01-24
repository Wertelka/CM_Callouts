﻿using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
    public enum ShowWoundLevel
    {
        None,
        Destroyed,
        Major,
        Serious,
        All
    }

    public class CalloutModSettings : ModSettings
    {
        public bool attachCalloutText = true;
        //public bool showWounds = true;
        public ShowWoundLevel showWoundLevel = ShowWoundLevel.All;
        public bool drawLabelBackgroundForTextMotes = true;
        public float baseCalloutChance = 0.2f;

        public bool showDebugLogMessages = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref attachCalloutText, "attachCalloutText", true);
            //Scribe_Values.Look(ref showWounds, "showWounds", true);
            Scribe_Values.Look(ref drawLabelBackgroundForTextMotes, "drawLabelBackgroundForTextMotes", true);
            Scribe_Values.Look(ref baseCalloutChance, "baseCalloutChance", 0.2f);
            Scribe_Values.Look(ref showWoundLevel, "showWoundLevel", ShowWoundLevel.All);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 34f) / 2f;

            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Attach_Callout_Text_Label".Translate(), ref attachCalloutText, "CM_Callouts_Settings_Attach_Callout_Text_Description".Translate());
            //listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Show_Wounds_Label".Translate(), ref showWounds, "CM_Callouts_Settings_Show_Wounds_Description".Translate());

            if (listing_Standard.RadioButton_NewTemp("CM_Callouts_Settings_Show_Wounds_None_Label".Translate(), showWoundLevel == ShowWoundLevel.None, 8f, "CM_Callouts_Settings_Show_Wounds_None_Description".Translate()))
                showWoundLevel = ShowWoundLevel.None;
            if (listing_Standard.RadioButton_NewTemp("CM_Callouts_Settings_Show_Wounds_Destroyed_Label".Translate(), showWoundLevel == ShowWoundLevel.Destroyed, 8f, "CM_Callouts_Settings_Show_Wounds_Destroyed_Description".Translate()))
                showWoundLevel = ShowWoundLevel.Destroyed;
            if (listing_Standard.RadioButton_NewTemp("CM_Callouts_Settings_Show_Wounds_Major_Label".Translate(), showWoundLevel == ShowWoundLevel.Major, 8f, "CM_Callouts_Settings_Show_Wounds_Major_Description".Translate()))
                showWoundLevel = ShowWoundLevel.Major;
            if (listing_Standard.RadioButton_NewTemp("CM_Callouts_Settings_Show_Wounds_Serious_Label".Translate(), showWoundLevel == ShowWoundLevel.Serious, 8f, "CM_Callouts_Settings_Show_Wounds_Serious_Description".Translate()))
                showWoundLevel = ShowWoundLevel.Serious;
            if (listing_Standard.RadioButton_NewTemp("CM_Callouts_Settings_Show_Wounds_All_Label".Translate(), showWoundLevel == ShowWoundLevel.All, 8f, "CM_Callouts_Settings_Show_Wounds_All_Description".Translate()))
                showWoundLevel = ShowWoundLevel.All;           

            listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Draw_Label_Background_For_Text_Motes_Label".Translate(), ref drawLabelBackgroundForTextMotes, "CM_Callouts_Settings_Draw_Label_Background_For_Text_Motes_Description".Translate());

            listing_Standard.Label("CM_Callouts_Settings_Base_Callout_Chance_Label".Translate(), -1, "CM_Callouts_Settings_Base_Callout_Chance_Description".Translate());
            listing_Standard.Label(baseCalloutChance.ToString("P0"));
            baseCalloutChance = listing_Standard.Slider(baseCalloutChance, 0.0f, 1.0f);

            if (Prefs.DevMode)
            {
                listing_Standard.CheckboxLabeled("Show debug messages", ref showDebugLogMessages);
            }

            listing_Standard.End();
        }

        public void UpdateSettings()
        {
        }
    }
}