﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;

namespace DelvUI {
    public class PluginConfiguration : IPluginConfiguration {

        public event EventHandler<EventArgs> ConfigChangedEvent;

        public int Version { get; set; }
        public bool HideHud = false;
        public bool LockHud = true;
        public int HealthBarHeight { get; set; } = 50;
        public int HealthBarWidth { get; set; } = 270;
        public int HealthBarXOffset { get; set; } = 160;
        public int HealthBarYOffset { get; set; } = 460;
        public int PrimaryResourceBarHeight { get; set; } = 13;
        public int PrimaryResourceBarWidth { get; set; } = 254;
        public int PrimaryResourceBarXOffset { get; set; } = 160;
        public int PrimaryResourceBarYOffset { get; set; } = 460;
        public int TargetBarHeight { get; set; } = 50;
        public int TargetBarWidth { get; set; } = 270;
        public int TargetBarXOffset { get; set; } = 160;
        public int TargetBarYOffset { get; set; } = 460;
        public int ToTBarHeight { get; set; } = 20;
        public int ToTBarWidth { get; set; } = 120;
        public int ToTBarXOffset { get; set; } = 164;
        public int ToTBarYOffset { get; set; } = 460;
        public int FocusBarHeight { get; set; } = 20;
        public int FocusBarWidth { get; set; } = 120;
        public int FocusBarXOffset { get; set; } = 164;
        public int FocusBarYOffset { get; set; } = 460;
        public int ShieldHeight { get; set; } = 10;

        public bool ShieldHeightPixels = true;

        public bool ShieldEnabled = true;

        public int TankStanceIndicatorWidth { get; set; } = 2;
        public bool TankStanceIndicatorEnabled = true;
        public bool CustomHealthBarColorEnabled = false;

        public string HealthBarTextLeft = "[name:abbreviate]";
        public string HealthBarTextRight = "[health:max-short] | [health:percent]";
        public int HealthBarTextLeftXOffset { get; set; } = 0;
        public int HealthBarTextLeftYOffset { get; set; } = 0;
        public int HealthBarTextRightXOffset { get; set; } = 0;
        public int HealthBarTextRightYOffset { get; set; } = 0;
        
        public string TargetBarTextLeft = "[health:max-short] | [health:percent]";
        public string TargetBarTextRight = "[name:abbreviate]";
        public int TargetBarTextLeftXOffset { get; set; } = 0;
        public int TargetBarTextLeftYOffset { get; set; } = 0;
        public int TargetBarTextRightXOffset { get; set; } = 0;
        public int TargetBarTextRightYOffset { get; set; } = 0;
        
        public string ToTBarText = "[name:abbreviate]";
        public string FocusBarText = "[name:abbreviate]";
        public int ToTBarTextXOffset { get; set; } = 0;
        public int ToTBarTextYOffset { get; set; } = 0;
        public int FocusBarTextXOffset { get; set; } = 0;
        public int FocusBarTextYOffset { get; set; } = 0;

        public bool MPTickerEnabled = false;
        public int MPTickerHeight { get; set; } = 4;
        public int MPTickerWidth { get; set; } = 254;
        public int MPTickerXOffset { get; set; } = 0;
        public int MPTickerYOffset { get; set; } = 470;
        public bool MPTickerShowBorder = false;
        public bool MPTickerHideOnFullMp = false;

        public bool GCDIndicatorEnabled = false;
        public int GCDIndicatorHeight { get; set; } = 4;
        public int GCDIndicatorWidth { get; set; } = 254;
        public int GCDIndicatorXOffset { get; set; } = 0;
        public int GCDIndicatorYOffset { get; set; } = 480;
        public bool GCDIndicatorShowBorder = false;

        public int CastBarHeight { get; set; } = 25;
        public int CastBarWidth { get; set; } = 254;
        public int CastBarXOffset { get; set; } = 0;
        public int CastBarYOffset { get; set; } = 460;


        public bool ShowCastBar = true;
        public bool ShowActionIcon = true;
        public bool ShowActionName = true;
        public bool ShowCastTime = true;
        public bool SlideCast = false;
        public bool ColorCastBarByJob = false;
        public float SlideCastTime = 500;
        
        public int TargetCastBarHeight { get; set; } = 25;
        public int TargetCastBarWidth { get; set; } = 254;
        public int TargetCastBarXOffset { get; set; } = 0;
        public int TargetCastBarYOffset { get; set; } = 320;


        public bool ShowTargetCastBar = true;
        public bool ShowTargetActionIcon = true;
        public bool ShowTargetActionName = true;
        public bool ShowTargetCastTime = true;
        public bool ShowTargetInterrupt = true;
        public bool ColorCastBarByDamageType = false;

        public Vector4 CustomHealthBarColor = new Vector4(0f/255f, 145f/255f, 6f/255f, 100f/100f);
        public Vector4 CastBarColor = new Vector4(255f/255f,158f/255f,208f/255f,100f/100f);
        public Vector4 TargetCastBarColor = new Vector4(255f/255f,158f/255f,208f/255f,100f/100f);
        public Vector4 TargetCastBarPhysicalColor = new Vector4(255f/255f,0/255f,0f/255f,100f/100f);
        public Vector4 TargetCastBarMagicalColor = new Vector4(0f/255f,0/255f,255f/255f,100f/100f);
        public Vector4 TargetCastBarDarknessColor = new Vector4(255f/255f,0/255f,255f/255f,100f/100f);
        public Vector4 TargetCastBarInterruptColor = new Vector4(255f/255f,0/255f,255f/255f,100f/100f);
        public Vector4 SlideCastColor = new Vector4(255f/255f,0f/255f,0f/255f,100f/100f);
        public Vector4 ShieldColor = new Vector4(255f/255f,255f/255f,0f/255f,100f/100f);
        public Vector4 MPTickerColor = new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f);
        public Vector4 GCDIndicatorColor = new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f);

        public Vector4 JobColorPLD = new Vector4(21f/255f,28f/255f,100f/255f,100f/100f);
        public Vector4 JobColorWAR = new Vector4(153f/255f,23f/255f,23f/255f,100f/100f);
        public Vector4 JobColorDRK = new Vector4(136f/255f,14f/255f,79f/255f,100f/100f);
        public Vector4 JobColorGNB = new Vector4(78f/255f,52f/255f,46f/255f,100f/100f);

        public Vector4 JobColorWHM = new Vector4(150f/255f,150f/255f,150f/255f,100f/100f);
        public Vector4 JobColorSCH = new Vector4(121f/255f,134f/255f,203f/255f,100f/100f);
        public Vector4 JobColorAST = new Vector4(121f/255f,85f/255f,72f/255f,100f/100f);

        public Vector4 JobColorMNK = new Vector4(78f/255f,52f/255f,46f/255f,100f/100f);
        public Vector4 JobColorDRG = new Vector4(63f/255f,81f/255f,181f/255f,100f/100f);
        public Vector4 JobColorNIN = new Vector4(211f/255f,47f/255f,47f/255f,100f/100f);
        public Vector4 JobColorSAM = new Vector4(255f/255f,202f/255f,40f/255f,100f/100f);

        public Vector4 JobColorBRD = new Vector4(158f/255f,157f/255f,36f/255f,100f/100f);
        public Vector4 JobColorMCH = new Vector4(0f/255f,151f/255f,167f/255f,100f/100f);
        public Vector4 JobColorDNC = new Vector4(244f/255f,143f/255f,177f/255f,100f/100f);

        public Vector4 JobColorBLM = new Vector4(126f/255f,87f/255f,194f/255f,100f/100f);
        public Vector4 JobColorSMN = new Vector4(46f/255f,125f/255f,50f/255f,100f/100f);
        public Vector4 JobColorRDM = new Vector4(233f/255f,30f/255f,99f/255f,100f/100f);
        public Vector4 JobColorBLU = new Vector4(0f/255f,185f/255f,247f/255f,100f/100f);

        public Vector4 NPCColorHostile = new Vector4(205f/255f, 25f/255f, 25f/255f, 100f/100f);
        public Vector4 NPCColorNeutral = new Vector4(214f/255f, 145f/255f, 64f/255f, 100f/100f);
        public Vector4 NPCColorFriendly = new Vector4(0f/255f, 145f/255f, 6f/255f, 100f/100f);

        #region WAR Configuration

        public int WARStormsEyeHeight { get; set; } = 20;
        public int WARStormsEyeWidth { get; set; } = 254;
        public int WARBaseXOffset { get; set; } = 127;
        public int WARBaseYOffset { get; set; } = 416;
        public int WARBeastGaugeHeight { get; set; } = 20;
        public int WARBeastGaugeWidth { get; set; } = 254;
        public int WARBeastGaugePadding { get; set; } = 2;
        public int WARBeastGaugeXOffset { get; set; }
        public int WARBeastGaugeYOffset { get; set; }
        public int WARInterBarOffset { get; set; } = 2;
        public Vector4 WARInnerReleaseColor = new Vector4(255f/255f, 0f/255f, 0f/255f, 100f/100f);
        public Vector4 WARStormsEyeColor = new Vector4(255f/255f, 136f/255f, 146f/255f, 100f/100f);
        public Vector4 WARFellCleaveColor = new Vector4(201f/255f, 13f/255f, 13f/255f, 100f/100f);
        public Vector4 WARNascentChaosColor = new Vector4(240f/255f, 176f/255f, 0f/255f, 100f/100f);
        public Vector4 WAREmptyColor = new Vector4(143f/255f, 141f/255f, 142f/255f, 100f/100f);

        #endregion

        #region SCH Configuration

        public int FairyBarHeight { get; set; } = 20;
        public int FairyBarWidth { get; set; } = 254;
        public int FairyBarX { get; set; } = 127;
        public int FairyBarY { get; set; } = 450;
        public int SchAetherBarHeight { get; set; } = 20;
        public int SchAetherBarWidth { get; set; } = 250;
        public int SchAetherBarX { get; set; } = -42;
        public int SchAetherBarY { get; set; } = 450;
        public int SchAetherBarPad { get; set; } = 2;
        public int SCHBioBarHeight { get; set; } = 20;
        public int SCHBioBarWidth { get; set; } = 254;
        public int SCHBioBarX { get; set; } = 127;
        public int SCHBioBarY { get; set; } = 423;

        public bool SCHShowBioBar = true;
        public bool SCHShowAetherBar = true;
        public bool SCHShowFairyBar = true;
        public bool SCHShowPrimaryResourceBar = true;

        public Vector4 SchAetherColor = new Vector4(0f/255f, 255f/255f, 0f/255f, 100f/100f);
        public Vector4 SchFairyColor = new Vector4(94f/255f, 250f/255f, 154f/255f, 100f/100f);
        public Vector4 SchEmptyColor = new Vector4(0f/255f, 0f/255f, 0f/255f, 53f/100f);
        public Vector4 SCHBioColor = new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 1f);

        #endregion

        #region WHM Configuration
        public int LillyBarHeight { get; set; } = 20;
        public int LillyBarWidth { get; set; } = 254;
        public int LillyBarX { get; set; } = 127;
        public int LillyBarY { get; set; } = 421;
        public int LillyBarPad { get; set; } = 2;
        public int BloodLillyBarHeight { get; set; } = 20;
        public int BloodLillyBarWidth { get; set; } = 254;
        public int BloodLillyBarX { get; set; } = 42;
        public int BloodLillyBarY { get; set; } = 421;
        public int BloodLillyBarPad { get; set; } = 2;
        public int DiaBarHeight { get; set; } = 20;
        public int DiaBarWidth { get; set; } = 254;
        public int DiaBarX { get; set; } = 127;
        public int DiaBarY { get; set; } = 423;

        public bool WHMShowDiaBar = true;
        public bool WHMShowLillyBar = true;
        //public bool WHMShowBloodLillyBar = true;
        public bool WHMShowPrimaryResourceBar = true;

        public Vector4 WhmLillyColor = new Vector4(0f / 255f, 64f / 255f, 1f, 1f);
        public Vector4 WhmBloodLillyColor = new Vector4(199f / 255f, 40f / 255f, 9f / 255f, 1f);
        public Vector4 WhmLillyChargingColor = new Vector4(141f / 255f, 141f / 255f, 141f / 255f, 1f);
        public Vector4 WhmDiaColor = new Vector4(0f / 255f, 64f / 255f, 1f, 1f);
        public Vector4 WhmEmptyColor = new Vector4(0f, 0f, 0f, 136f / 255f);

        #endregion

        #region AST Configuration

        public int ASTDrawBarHeight { get; set; } = 20;
        public int ASTDrawBarWidth { get; set; } = 254;
        public int ASTDrawBarX { get; set; } = 127;
        public int ASTDrawBarY { get; set; } = 438;
        public int ASTDivinationHeight { get; set; } = 20;
        public int ASTDivinationWidth { get; set; } = 253;
        public int ASTDivinationBarX { get; set; } = -42;
        public int ASTDivinationBarY { get; set; } = 438;
        public int ASTDivinationBarPad { get; set; } = 1;
        public int ASTDotBarHeight { get; set; } = 20;
        public int ASTDotBarWidth { get; set; } = 254;
        public int ASTDotBarX { get; set; } = 33;
        public int ASTDotBarY { get; set; } = -38;
        public bool ASTShowDivinationBar = true;
        public bool ASTShowDrawBar = true;
        public bool ASTShowDotBar = true;
        public bool ASTShowPrimaryResourceBar = true;
        public Vector4 ASTEmptyColor = new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 53f / 100f);
        public Vector4 ASTSealSunColor = new Vector4(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f);
        public Vector4 ASTSealLunarColor = new Vector4(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f);
        public Vector4 ASTSealCelestialColor = new Vector4(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f);
        public Vector4 ASTDotColor = new Vector4(20f / 255f, 80f / 255f, 168f / 255f, 100f / 100f);

        #endregion

        #region SMN Configuration

        public int SmnRuinBarX { get; set; } = 127;
        public int SmnRuinBarY { get; set; } = 460;
        public int SmnRuinBarHeight { get; set; } = 10;
        public int SmnRuinBarWidth { get; set; } = 254;
        public int SmnDotBarX { get; set; } = 127;
        public int SmnDotBarY { get; set; } = 460;
        public int SmnDotBarHeight { get; set; } = 10;
        public int SmnDotBarWidth { get; set; } = 254;
        public int SmnAetherBarHeight { get; set; } = 20;
        public int SmnAetherBarWidth { get; set; } = 254;
        public int SmnAetherBarX { get; set; } = -42;
        public int SmnAetherBarY { get; set; } = 460;

        public Vector4 SmnAetherColor = new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f);
        public Vector4 SmnRuinColor = new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f);
        public Vector4 SmnEmptyColor = new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 136f / 255f);

        public Vector4 SmnMiasmaColor = new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f);
        public Vector4 SmnBioColor = new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f);
        public Vector4 SmnExpiryColor = new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f);

        #endregion

        #region SAM Configuration

        public bool SAMGaugeEnabled { get; set; } = true;
        public bool SAMSenEnabled { get; set; } = true;
        public bool SAMMeditationEnabled { get; set; } = true;
        public bool SAMHiganbanaEnabled { get; set; } = true;
        public bool SAMBuffsEnabled { get; set; } = true;

        public bool SAMHiganbanaText { get; set; } = true;
        public bool SAMBuffText { get; set; } = true;
        public bool SAMKenkiText { get; set; } = true;
        public int SamHiganbanaBarX { get; set; } = 127;
        public int SamHiganbanaBarY { get; set; } = 370;
        public int SamHiganbanaBarHeight { get; set; } = 20;
        public int SamHiganbanaBarWidth { get; set; } = 254;
        public int SamBuffsBarX { get; set; } = 127;
        public int SamBuffsBarY { get; set; } = 392;
        public int SamBuffsBarHeight { get; set; } = 20;
        public int SamBuffsBarWidth { get; set; } = 254;
        public int SamTimeShifuXOffset { get; set; } = 63;
        public int SamTimeShifuYOffset { get; set; } = 390;
        public int SamTimeJinpuXOffset { get; set; } = -63;
        public int SamTimeJinpuYOffset { get; set; } = 390;
        public int SamKenkiBarX { get; set; } = 127;
        public int SamKenkiBarY { get; set; } = 414;
        public int SamKenkiBarHeight { get; set; } = 20;
        public int SamKenkiBarWidth { get; set; } = 254;
        public int SAMSenPadding { get; set; } = 2;
        public int SamSenBarX { get; set; } = 127;
        public int SamSenBarY { get; set; } = 436;
        public int SamSenBarHeight { get; set; } = 10;
        public int SamSenBarWidth { get; set; } = 254;
        public int SamMeditationBarX { get; set; } = 127;
        public int SamMeditationBarY { get; set; } = 448;
        public int SamMeditationBarHeight { get; set; } = 10;
        public int SamMeditationBarWidth { get; set; } = 254;
        public int SAMMeditationPadding { get; set; } = 2;
        public int SAMBuffsPadding { get; set; } = 2;


        public Vector4 SamHiganbanaColor = new Vector4(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f);
        public Vector4 SamShifuColor = new Vector4(219f / 255f, 211f / 255f, 136f / 255f, 100f / 100f);
        public Vector4 SamJinpuColor = new Vector4(136f / 255f, 146f / 255f, 219f / 255f, 100f / 100f);

        public Vector4 SamSetsuColor = new Vector4(89f / 255f, 234f / 255f, 247f / 255f, 100f / 100f);
        public Vector4 SamGetsuColor = new Vector4(89f / 255f, 126f / 255f, 247f / 255f, 100f / 100f);
        public Vector4 SamKaColor = new Vector4(247f / 255f, 89f / 255f, 89f / 255f, 100f / 100f);

        public Vector4 SamMeditationColor = new Vector4(247f / 255f, 163f / 255f, 89f / 255f, 100f / 100f);
        public Vector4 SamKenkiColor = new Vector4(255f / 255f, 82f / 255f, 82f / 255f, 53f / 100f);

        public Vector4 SamExpiryColor = new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f);
        public Vector4 SamEmptyColor = new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 136f / 255f);

        #endregion

        #region MCH Configuration

        public int MCHOverheatHeight { get; set; } = 20;
        public int MCHOverheatWidth { get; set; } = 254;
        public int MCHBaseXOffset { get; set; } = 127;
        public int MCHBaseYOffset { get; set; } = 395;
        public int MCHHeatGaugeHeight { get; set; } = 20;
        public int MCHHeatGaugeWidth { get; set; } = 254;
        public int MCHHeatGaugePadding { get; set; } = 2;
        public int MCHHeatGaugeXOffset { get; set; }
        public int MCHHeatGaugeYOffset { get; set; }
        public int MCHBatteryGaugeHeight { get; set; } = 20;
        public int MCHBatteryGaugeWidth { get; set; } = 254;
        public int MCHBatteryGaugePadding { get; set; } = 2;
        public int MCHBatteryGaugeXOffset { get; set; }
        public int MCHBatteryGaugeYOffset { get; set; }
        public bool MCHWildfireEnabled { get; set; }
        public int MCHWildfireHeight { get; set; } = 20;
        public int MCHWildfireWidth { get; set; } = 254;
        public int MCHWildfireXOffset { get; set; }
        public int MCHWildfireYOffset { get; set; }
        public int MCHInterBarOffset { get; set; } = 2;
        public Vector4 MCHHeatColor = new Vector4(201f/255f, 13f/255f, 13f/255f, 100f/100f);
        public Vector4 MCHBatteryColor = new Vector4(106f/255f, 255f/255f, 255f/255f, 100f/100f);
        public Vector4 MCHRobotColor = new Vector4(153f/255f, 0f/255f, 255f/255f, 100f/100f);
        public Vector4 MCHOverheatColor = new Vector4(255f/255f, 239f/255f, 14f/255f, 100f/100f);
        public Vector4 MCHWildfireColor = new Vector4(255f/255f, 0f/255f, 0f/255f, 100f/100f);
        public Vector4 MCHEmptyColor = new Vector4(143f/255f, 141f/255f, 142f/255f, 100f/100f);

        #endregion


        #region NIN Configuration

        public int NINBaseXOffset { get; set; } = 127;
        public int NINBaseYOffset { get; set; } = 420;
        public int NINHutonGaugeHeight { get; set; } = 20;
        public int NINHutonGaugeWidth { get; set; } = 254;
        public int NINNinkiGaugeHeight { get; set; } = 20;
        public int NINNinkiGaugeWidth { get; set; } = 254;
        public int NINNinkiGaugePadding { get; set; } = 2;
        public int NINNinkiGaugeXOffset { get; set; }
        public int NINNinkiGaugeYOffset { get; set; }

        public int NINInterBarOffset { get; set; } = 2;
        public Vector4 NINHutonColor = new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f);
        public Vector4 NINNinkiColor = new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f);
        public Vector4 NINEmptyColor = new Vector4(143f / 255f, 141f / 255f, 142f / 255f, 100f / 100f);

        #endregion

        #region DRK Configuration

        public int DRKBaseXOffset { get; set; } = 127;
        public int DRKBaseYOffset { get; set; } = 415;
        public bool DRKManaBarEnabled { get; set; } = true;
        public bool DRKManaBarOverflowEnabled { get; set; }
        public int DRKManaBarHeight { get; set; } = 10;
        public int DRKManaBarWidth { get; set; } = 254;
        public int DRKManaBarPadding { get; set; } = 1;
        public int DRKManaBarXOffset { get; set; }
        public int DRKManaBarYOffset { get; set; }
        public bool DRKBloodGaugeEnabled { get; set; } = true;
        public bool DRKBloodGaugeSplit { get; set; } = false;
        public bool DRKBloodGaugeThreshold { get; set; } = false;
        public int DRKBloodGaugeHeight { get; set; } = 10;
        public int DRKBloodGaugeWidth { get; set; } = 254;
        public int DRKBloodGaugePadding { get; set; } = 2;
        public int DRKBloodGaugeXOffset { get; set; }
        public int DRKBloodGaugeYOffset { get; set; }
        public bool DRKBuffBarEnabled { get; set; } = true;
        public int DRKBuffBarHeight { get; set; } = 20;
        public int DRKBuffBarWidth { get; set; } = 254;
        public int DRKBuffBarPadding { get; set; } = 2;
        public int DRKBuffBarXOffset { get; set; }
        public int DRKBuffBarYOffset { get; set; }
        public bool DRKLivingShadowBarEnabled { get; set; }
        public int DRKLivingShadowBarHeight { get; set; } = 20;
        public int DRKLivingShadowBarWidth { get; set; } = 254;
        public int DRKLivingShadowBarPadding { get; set; } = 2;
        public int DRKLivingShadowBarXOffset { get; set; }
        public int DRKLivingShadowBarYOffset { get; set; }
        public int DRKInterBarOffset { get; set; } = 2;
        public Vector4 DRKManaColor = new Vector4(0f/255f, 142f/255f, 254f/255f, 100f/100f);
        public Vector4 DRKBloodColorLeft = new Vector4(196f/255f, 20f/255f, 122f/255f, 100f/100f);
        public Vector4 DRKBloodColorRight = new Vector4(216f/255f, 0f/255f, 73f/255f, 100f/100f);
        public Vector4 DRKDarkArtsColor = new Vector4(210f/255f, 33f/255f, 33f/255f, 100f/100f);
        public Vector4 DRKBloodWeaponColor = new Vector4(160f/255f, 0f/255f, 0f/255f, 100f/100f);
        public Vector4 DRKDeliriumColor = new Vector4(255f/255f, 255f/255f, 255f/255f, 100f/100f);
        public Vector4 DRKLivingShadowColor = new Vector4(225f/255f, 105f/255f, 205f/255f, 100f/100f);
        public Vector4 DRKEmptyColor = new Vector4(143f/255f, 141f/255f, 142f/255f, 100f/100f);

        #endregion

        #region PLD Configuration

        public int PLDManaHeight { get; set; } = 20;
        public int PLDManaWidth { get; set; } = 254;
        public int PLDManaPadding { get; set; } = 2;
        public int PLDBaseXOffset { get; set; } = 127;
        public int PLDBaseYOffset { get; set; } = 373;
        public int PLDOathGaugeHeight { get; set; } = 20;
        public int PLDOathGaugeWidth { get; set; } = 254;
        public int PLDOathGaugePadding { get; set; } = 2;
        public int PLDOathGaugeXOffset {get; set;}
        public int PLDOathGaugeYOffset {get; set;}
        public bool PLDOathGaugeText { get; set; } = false;
        public int PLDBuffBarHeight { get; set; } = 20;
        public int PLDBuffBarWidth { get; set; } = 254;
        public int PLDBuffBarXOffset {get; set;}
        public int PLDBuffBarYOffset {get; set;}
        public int PLDAtonementBarHeight { get; set; } = 20;
        public int PLDAtonementBarWidth { get; set; } = 254;
        public int PLDAtonementBarPadding { get; set; } = 2;
        public int PLDAtonementBarXOffset {get; set;}
        public int PLDAtonementBarYOffset {get; set;}
        public int PLDInterBarOffset { get; set; } = 2;
        public Vector4 PLDManaColor = new Vector4(0f/255f, 203f/255f, 230f/255f, 100f/100f);
        public Vector4 PLDOathGaugeColor = new Vector4(24f/255f, 80f/255f, 175f/255f, 100f/100f);
        public Vector4 PLDFightOrFlightColor = new Vector4(240f/255f, 50f/255f, 0f/255f, 100f/100f);
        public Vector4 PLDRequiescatColor = new Vector4(61f/255f, 61f/255f, 255f/255f, 100f/100f);
        public Vector4 PLDEmptyColor = new Vector4(143f/255f, 141f/255f, 142f/255f, 100f/100f);
        public Vector4 PLDAtonementColor = new Vector4(240f/255f, 176f/255f, 0f/255f, 100f/100f);

        #endregion

        #region MNK Configuration
        
        public bool ShowDemolishTime = true;
        public bool ShowBuffTime = true;
        public int MNKDemolishHeight { get; set; } = 20;
        public int MNKDemolishWidth { get; set; } = 254;
        public int MNKDemolishXOffset { get; set; } = 127;
        public int MNKDemolishYOffset { get; set; } = 370;
        public int MNKChakraHeight { get; set; } = 20;
        public int MNKChakraWidth { get; set; } = 254;
        public int MNKChakraXOffset { get; set; } = 127;
        public int MNKChakraYOffset { get; set; } = 370;
        public int MNKBuffHeight { get; set; } = 20;
        public int MNKBuffWidth { get; set; } = 254;
        public int MNKBuffXOffset { get; set; } = 127;
        public int MNKBuffYOffset { get; set; } = 370;
        public int MNKTimeTwinXOffset { get; set; } = 60;
        public int MNKTimeTwinYOffset { get; set; } = 2;
        public int MNKTimeLeadenXOffset { get; set; } = 60;
        public int MNKTimeLeadenYOffset { get; set; } = 2;
        public int MNKTimeDemoXOffset { get; set; } = 0;
        public int MNKTimeDemoYOffset { get; set; } = 2;
        public Vector4 MNKDemolishColor = new Vector4(147f/255f, 0f, 83f/255f, 100f);
        public Vector4 MNKChakraColor = new Vector4(204f/255f, 115f/255f, 0f, 100f);
        public Vector4 MNKLeadenFistColor = new Vector4(255f/255f, 234f/255f, 0f, 100f);
        public Vector4 MNKTwinSnakesColor = new Vector4(121f/255f, 0f, 96f/255f, 100f);
        
        #endregion
        
        #region BLM Configuration

        public int BLMVerticalOffset { get; set; } = -2;
        public int BLMHorizontalOffset { get; set; } = 0;
        public int BLMVerticalSpaceBetweenBars { get; set; } = 2;
        public int BLMHorizontalSpaceBetweenBars { get; set; } = 2;
        public int BLMManaBarHeight { get; set; } = 20;
        public int BLMManaBarWidth { get; set; } = 253;
        public int BLMUmbralHeartHeight { get; set; } = 16;
        public int BLMUmbralHeartWidth { get; set; } = 83;
        public int BLMPolyglotHeight { get; set; } = 18;
        public int BLMPolyglotWidth { get; set; } = 18;
        
        public bool BLMShowManaValue = false;
        
        public bool BLMShowManaThresholdMarker = true;
        public int BLMManaThresholdValue { get; set; } = 2600;

        public bool BLMShowTripleCast = true;
        public int BLMTripleCastHeight { get; set; } = 16;
        public int BLMTripleCastWidth { get; set; } = 83;

        public bool BLMShowFirestarterProcs = true;
        public bool BLMShowThundercloudProcs = true;
        public int BLMProcsHeight { get; set; } = 7;
        public bool BLMShowDotTimer = true;
        public int BLMDotTimerHeight { get; set; } = 10;

        public Vector4 BLMManaBarNoElementColor = new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f);
        public Vector4 BLMManaBarIceColor = new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f);
        public Vector4 BLMManaBarFireColor = new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f);
        public Vector4 BLMUmbralHeartColor = new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f);
        public Vector4 BLMPolyglotColor = new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f);
        public Vector4 BLMTriplecastColor = new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f);
        public Vector4 BLMFirestarterColor = new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 90f / 100f);
        public Vector4 BLMThundercloudColor = new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 90f / 100f);
        public Vector4 BLMDotColor = new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f);
        public Vector4 BLMEmptyColor = new Vector4(143f / 255f, 141f / 255f, 142f / 255f, 100f / 100f);

        #endregion

        #region RDM Configuration

        public int RDMVerticalOffset { get; set; } = -2;
        public int RDMHorizontalOffset { get; set; } = 0;
        public int RDMHorizontalSpaceBetweenBars { get; set; } = 2;
        public int RDMManaBarHeight { get; set; } = 18;
        public int RDMManaBarWidth { get; set; } = 253;
        public int RDMManaBarXOffset { get; set; } = 0;
        public int RDMManaBarYOffset { get; set; } = 0;
        public int RDMWhiteManaBarHeight { get; set; } = 20;
        public int RDMWhiteManaBarWidth { get; set; } = 114;
        public int RDMWhiteManaBarXOffset { get; set; } = -13;
        public int RDMWhiteManaBarYOffset { get; set; } = -40;
        public bool RDMWhiteManaBarInversed = true;
        public bool RDMShowWhiteManaValue = true;
        public int RDMBlackManaBarHeight { get; set; } = 20;
        public int RDMBlackManaBarWidth { get; set; } = 114;
        public int RDMBlackManaBarXOffset { get; set; } = 13;
        public int RDMBlackManaBarYOffset { get; set; } = -40;
        public bool RDMBlackManaBarInversed = false;
        public bool RDMShowBlackManaValue = true;
        public int RDMBalanceBarHeight { get; set; } = 20;
        public int RDMBalanceBarWidth { get; set; } = 21;
        public int RDMBalanceBarXOffset { get; set; } = 0;
        public int RDMBalanceBarYOffset { get; set; } = -40;
        public int RDMAccelerationBarHeight { get; set; } = 12;
        public int RDMAccelerationBarWidth { get; set; } = 83;
        public int RDMAccelerationBarXOffset { get; set; } = 0;
        public int RDMAccelerationBarYOffset { get; set; } = -54;
        public bool RDMShowManaValue = true;
        public bool RDMShowManaThresholdMarker = true;
        public int RDMManaThresholdValue { get; set; } = 2600;
        public bool RDMShowDualCast = true;
        public int RDMDualCastHeight { get; set; } = 16;
        public int RDMDualCastWidth { get; set; } = 16;
        public int RDMDualCastXOffset { get; set; } = 0;
        public int RDMDualCastYOffset { get; set; } = -72;
        public bool RDMShowVerstoneProcs = true;
        public bool RDMShowVerfireProcs = true;
        public int RDMProcsHeight { get; set; } = 7;

        public Vector4 RDMManaBarColor = new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f);
        public Vector4 RDMManaBarBelowThresholdColor = new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f);
        public Vector4 RDMWhiteManaBarColor = new Vector4(221f / 255f, 212f / 255f, 212f / 255f, 100f / 100f);
        public Vector4 RDMBlackManaBarColor = new Vector4(60f / 255f, 81f / 255f, 197f / 255f, 100f / 100f);
        public Vector4 RDMBalanceBarColor = new Vector4(195f / 255f, 35f / 255f, 35f / 255f, 100f / 100f);
        public Vector4 RDMAccelerationBarColor = new Vector4(194f / 255f, 74f / 255f, 74f / 255f, 100f / 100f);
        public Vector4 RDMDualcastBarColor = new Vector4(204f / 255f, 17f / 255f, 255f / 95f, 100f / 100f);
        public Vector4 RDMVerstoneBarColor = new Vector4(228f / 255f, 188f / 255f, 145 / 255f, 90f / 100f);
        public Vector4 RDMVerfireBarColor = new Vector4(238f / 255f, 119f / 255f, 17 / 255f, 90f / 100f);

        #endregion

        #region DRG Configuration

        public int DRGEyeOfTheDragonHeight { get; set; } = 20;
        public int DRGEyeOfTheDragonBarWidth { get; set; } = 125;
        public int DRGEyeOfTheDragonPadding { get; set; } = 2;
        public int DRGBaseXOffset { get; set; } = 127;
        public int DRGBaseYOffset { get; set; } = 373;
        public int DRGBloodBarHeight { get; set; } = 20;

        public int DRGDisembowelBarHeight { get; set; } = 20;
        public int DRGChaosThrustBarHeight { get; set; } = 20;
        public int DRGInterBarOffset { get; set; } = 2;
        public bool DRGShowChaosThrustTimer = true;
        public bool DRGShowDisembowelBuffTimer = true;
        public bool DRGShowChaosThrustText = true;
        public bool DRGShowBloodText = true;
        public bool DRGShowDisembowelText = true;

        public Vector4 DRGEyeOfTheDragonColor = new Vector4(1f, 182f / 255f, 194f / 255f, 1f);
        public Vector4 DRGBloodOfTheDragonColor = new Vector4(78f / 255f, 198f / 255f, 238f / 255f, 1f);
        public Vector4 DRGLifeOfTheDragonColor = new Vector4(139f / 255f, 24f / 255f, 24f / 255f, 1f);
        public Vector4 DRGDisembowelColor = new Vector4(244f / 255f, 206f / 255f, 191f / 255f, 1f);
        public Vector4 DRGChaosThrustColor = new Vector4(106f / 255f, 82f / 255f, 148f / 255f, 1f);
        public Vector4 DRGEmptyColor = new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 1f);

        #endregion

        [JsonIgnore] private DalamudPluginInterface _pluginInterface;
        [JsonIgnore] public ImFontPtr BigNoodleTooFont = null;
        [JsonIgnore] public Dictionary<uint, Dictionary<string, uint>> JobColorMap;
        [JsonIgnore] public Dictionary<string, Dictionary<string, uint>> NPCColorMap;
        [JsonIgnore] public Dictionary<string, Dictionary<string, uint>> MiscColorMap;
        [JsonIgnore] public Dictionary<string, Dictionary<string, uint>> CastBarColorMap;

        public void Init(DalamudPluginInterface pluginInterface) {
            _pluginInterface = pluginInterface;
            BuildColorMap();
        }

        public void Save() {
            _pluginInterface.SavePluginConfig(this);

            // call event when the config changes
            if (ConfigChangedEvent != null)
            {
                ConfigChangedEvent(this, null);
            }
        }

        public void BuildColorMap() {
            JobColorMap = new Dictionary<uint, Dictionary<string, uint>>
            {
                [Jobs.PLD] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(.1f))
                },
                
                [Jobs.PLD * 1000] = new Dictionary<string, uint> // PLD Mana
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(PLDManaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(PLDManaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(PLDManaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(PLDManaColor.AdjustColor(.1f))
                },
                
                [Jobs.PLD * 1000 + 1] = new Dictionary<string, uint> // Oath Gauge
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(PLDOathGaugeColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(PLDOathGaugeColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(PLDOathGaugeColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(PLDOathGaugeColor.AdjustColor(.1f))
                },
                
                [Jobs.PLD * 1000 + 2] = new Dictionary<string, uint> // Fight Or Flight
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(PLDFightOrFlightColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(PLDFightOrFlightColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(PLDFightOrFlightColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(PLDFightOrFlightColor.AdjustColor(.1f))
                },
                
                [Jobs.PLD * 1000 + 3] = new Dictionary<string, uint> // Requiescat
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(PLDRequiescatColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(PLDRequiescatColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(PLDRequiescatColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(PLDRequiescatColor.AdjustColor(.1f))
                },
                
                [Jobs.PLD * 1000 + 4] = new Dictionary<string, uint> // PLD Empty
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(PLDEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(PLDEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(PLDEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(PLDEmptyColor.AdjustColor(.1f))
                },
                
                [Jobs.PLD * 1000 + 5] = new Dictionary<string, uint> // Atonement
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(PLDAtonementColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(PLDAtonementColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(PLDAtonementColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(PLDAtonementColor.AdjustColor(.1f))
                },
                
                [Jobs.WAR] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(.1f))
                },

                [Jobs.WAR * 1000] = new Dictionary<string, uint> // Inner Release
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WARInnerReleaseColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WARInnerReleaseColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WARInnerReleaseColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WARInnerReleaseColor.AdjustColor(.1f))
                },

                [Jobs.WAR * 1000 + 1] = new Dictionary<string, uint> // Storm's Eye
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WARStormsEyeColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WARStormsEyeColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WARStormsEyeColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WARStormsEyeColor.AdjustColor(.1f))
                },

                [Jobs.WAR * 1000 + 2] = new Dictionary<string, uint> // Fell Cleave
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WARFellCleaveColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WARFellCleaveColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WARFellCleaveColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WARFellCleaveColor.AdjustColor(.1f))
                },

                [Jobs.WAR * 1000 + 3] = new Dictionary<string, uint> // Nascent Chaos
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WARNascentChaosColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WARNascentChaosColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WARNascentChaosColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WARNascentChaosColor.AdjustColor(.1f))
                },

                [Jobs.WAR * 1000 + 4] = new Dictionary<string, uint> // WAR Empty
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WAREmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WAREmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WAREmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WAREmptyColor.AdjustColor(.1f))
                },

                [Jobs.DRK] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000] = new Dictionary<string, uint> // Mana
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKManaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKManaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKManaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKManaColor.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000 + 1] = new Dictionary<string, uint> // Blood Left
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorLeft),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorLeft.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorLeft.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorLeft.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000 + 2] = new Dictionary<string, uint> // Blood Right
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorRight),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorRight.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorRight.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKBloodColorRight.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000 + 3] = new Dictionary<string, uint> // Dark Arts
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKDarkArtsColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKDarkArtsColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKDarkArtsColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKDarkArtsColor.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000 + 4] = new Dictionary<string, uint> // Blood Weapon
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKBloodWeaponColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKBloodWeaponColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKBloodWeaponColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKBloodWeaponColor.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000 + 5] = new Dictionary<string, uint> // Delirium
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000 + 6] = new Dictionary<string, uint> // Living Shadow
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKDeliriumColor.AdjustColor(.1f))
                },

                [Jobs.DRK * 1000 + 7] = new Dictionary<string, uint> // Bar not ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRKEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRKEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRKEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRKEmptyColor.AdjustColor(.1f))
                },

                [Jobs.GNB] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB.AdjustColor(.1f))
                },

                [Jobs.WHM] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM.AdjustColor(.1f))
                },

                [Jobs.WHM * 1000] = new Dictionary<string, uint> // White mage Lilly Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WhmLillyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WhmLillyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WhmLillyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WhmLillyColor.AdjustColor(.1f))
                },

                [Jobs.WHM * 1000 + 1] = new Dictionary<string, uint> // White mage Blood Lilly Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WhmBloodLillyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WhmBloodLillyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WhmBloodLillyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WhmBloodLillyColor.AdjustColor(.1f))
                },

                [Jobs.WHM * 1000 + 2] = new Dictionary<string, uint> // White mage Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WhmEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WhmEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WhmEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WhmEmptyColor.AdjustColor(.1f))
                },

                [Jobs.WHM * 1000 + 3] = new Dictionary<string, uint> // White mage Lilly gauge charging color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WhmLillyChargingColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WhmLillyChargingColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WhmLillyChargingColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WhmLillyChargingColor.AdjustColor(.1f))
                },

                [Jobs.WHM * 1000 + 4] = new Dictionary<string, uint> // White mage Dia bar color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(WhmDiaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(WhmDiaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(WhmDiaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(WhmDiaColor.AdjustColor(.1f))
                },

                [Jobs.SCH] = new Dictionary<string, uint> // Scholar job color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(.1f))
                },

                [Jobs.SCH * 1000] = new Dictionary<string, uint> // Scholar Aether Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor.AdjustColor(.1f))
                },

                [Jobs.SCH * 1000 + 1] = new Dictionary<string, uint> // Scholar Fairy Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor.AdjustColor(.1f))
                },

                [Jobs.SCH * 1000 + 2] = new Dictionary<string, uint> // Scholar Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SchEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SchEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SchEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SchEmptyColor.AdjustColor(.1f))
                },

                [Jobs.SCH * 1000 + 3] = new Dictionary<string, uint> // Scholar Biolysis Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor.AdjustColor(.1f))
                },

                [Jobs.SMN] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(.1f))
                },

                [Jobs.SMN * 1000] = new Dictionary<string, uint> // Aether Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor.AdjustColor(.1f))
                },

                [Jobs.SMN * 1000 + 1] = new Dictionary<string, uint> // Ruin Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor.AdjustColor(.1f))
                },

                [Jobs.SMN * 1000 + 2] = new Dictionary<string, uint> // Empty Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnEmptyColor.AdjustColor(.1f))
                },

                [Jobs.SMN * 1000 + 3] = new Dictionary<string, uint> // Miasma Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor.AdjustColor(.1f))
                },

                [Jobs.SMN * 1000 + 4] = new Dictionary<string, uint> // Bio Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor.AdjustColor(.1f))
                },

                [Jobs.SMN * 1000 + 5] = new Dictionary<string, uint> // Dot Expiry
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor.AdjustColor(.1f))
                },

                [Jobs.AST] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorAST),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorAST.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorAST.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorAST.AdjustColor(.1f))
                },

                [Jobs.AST * 1000] = new Dictionary<string, uint> // Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(ASTEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(ASTEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(ASTEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(ASTEmptyColor.AdjustColor(.1f))
                },

                [Jobs.AST * 1000 + 1] = new Dictionary<string, uint> // Seal Color [Sun]
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(ASTSealSunColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(ASTSealSunColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(ASTSealSunColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(ASTSealSunColor.AdjustColor(.1f))
                },

                [Jobs.AST * 1000 + 2] = new Dictionary<string, uint> // Seal Color [Lunar]
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(ASTSealLunarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(ASTSealLunarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(ASTSealLunarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(ASTSealLunarColor.AdjustColor(.1f))
                },

                [Jobs.AST * 1000 + 3] = new Dictionary<string, uint> // Seal Color [Celestial]
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(ASTSealCelestialColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(ASTSealCelestialColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(ASTSealCelestialColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(ASTSealCelestialColor.AdjustColor(.1f))
                },

                [Jobs.AST * 1000 + 4] = new Dictionary<string, uint> // Dots
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(ASTDotColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(ASTDotColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(ASTDotColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(ASTDotColor.AdjustColor(.1f))
                },

                [Jobs.MNK] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK.AdjustColor(.1f))
                },

                [Jobs.MNK * 1000] = new Dictionary<string, uint> // Scholar Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor.AdjustColor(.1f))
                },

                [Jobs.MNK * 1000 + 1] = new Dictionary<string, uint> // Scholar Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor.AdjustColor(.1f))
                },

                [Jobs.MNK * 1000 + 2] = new Dictionary<string, uint> // Scholar Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor.AdjustColor(.1f))
                },

                [Jobs.MNK * 1000 + 3] = new Dictionary<string, uint> // Scholar Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor.AdjustColor(.1f))
                },
                
                [Jobs.DRG] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG.AdjustColor(.1f))
                },

                [Jobs.DRG * 1000] = new Dictionary<string, uint> // Eye of the Dragon
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRGEyeOfTheDragonColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRGEyeOfTheDragonColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRGEyeOfTheDragonColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRGEyeOfTheDragonColor.AdjustColor(.1f))
                },

                [Jobs.DRG * 1000 + 1] = new Dictionary<string, uint> // Blood of the Dragon
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRGBloodOfTheDragonColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRGBloodOfTheDragonColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRGBloodOfTheDragonColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRGBloodOfTheDragonColor.AdjustColor(.1f))
                },

                [Jobs.DRG * 1000 + 2] = new Dictionary<string, uint> // Life of the Dragon
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRGLifeOfTheDragonColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRGLifeOfTheDragonColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRGLifeOfTheDragonColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRGLifeOfTheDragonColor.AdjustColor(.1f))
                },

                [Jobs.DRG * 1000 + 3] = new Dictionary<string, uint> // DRG Disembowel
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRGDisembowelColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRGDisembowelColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRGDisembowelColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRGDisembowelColor.AdjustColor(.1f))
                },

                [Jobs.DRG * 1000 + 4] = new Dictionary<string, uint> // DRG Disembowel
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRGChaosThrustColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRGChaosThrustColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRGChaosThrustColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRGChaosThrustColor.AdjustColor(.1f))
                },

                [Jobs.DRG * 1000 + 5] = new Dictionary<string, uint> // DRG Empty
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(DRGEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(DRGEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(DRGEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(DRGEmptyColor.AdjustColor(.1f))
                },

                [Jobs.NIN] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(.1f))
                },

                [Jobs.SAM] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 0] = new Dictionary<string, uint> // Higanbana Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 1] = new Dictionary<string, uint> // Shifu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 2] = new Dictionary<string, uint> // Jinpu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 3] = new Dictionary<string, uint> // Setsu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 4] = new Dictionary<string, uint> // Getsu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 5] = new Dictionary<string, uint> // Ka Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamKaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamKaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamKaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamKaColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 6] = new Dictionary<string, uint> // Meditation Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 7] = new Dictionary<string, uint> // Kenki Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 8] = new Dictionary<string, uint> // Empty Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamEmptyColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 9] = new Dictionary<string, uint> // Dot Expiry Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor.AdjustColor(.1f))
                },

                [Jobs.NIN] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(.1f))
                },

                [Jobs.NIN * 1000] = new Dictionary<string, uint> // Bar not ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NINEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NINEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NINEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NINEmptyColor.AdjustColor(.1f))
                },

                [Jobs.NIN * 1000 + 1] = new Dictionary<string, uint> // Battery gauge ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NINHutonColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NINHutonColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NINHutonColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NINHutonColor.AdjustColor(.1f))
                },

                [Jobs.NIN * 1000 + 2] = new Dictionary<string, uint> // Robot summoned
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NINNinkiColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NINNinkiColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NINNinkiColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NINNinkiColor.AdjustColor(.1f))
                },                

                [Jobs.BRD] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD.AdjustColor(.1f))
                },

                [Jobs.MCH] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH.AdjustColor(.1f))
                },

                [Jobs.MCH * 1000] = new Dictionary<string, uint> // Heat gauge ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor.AdjustColor(.1f))
                },

                [Jobs.MCH * 1000 + 1] = new Dictionary<string, uint> // Battery gauge ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor.AdjustColor(.1f))
                },

                [Jobs.MCH * 1000 + 2] = new Dictionary<string, uint> // Robot summoned
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor.AdjustColor(.1f))
                },

                [Jobs.MCH * 1000 + 3] = new Dictionary<string, uint> // Overheated
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor.AdjustColor(.1f))
                },

                [Jobs.MCH * 1000 + 4] = new Dictionary<string, uint> // Bar not ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHEmptyColor.AdjustColor(.1f))
                },

                [Jobs.MCH * 1000 + 5] = new Dictionary<string, uint> // Wildfire Active
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor.AdjustColor(.1f))
                },

                [Jobs.DNC] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC.AdjustColor(.1f))
                },

                [Jobs.BLM] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000] = new Dictionary<string, uint> // Mana Bar no element
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarNoElementColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarNoElementColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarNoElementColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarNoElementColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 1] = new Dictionary<string, uint> // Mana bar ice
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarIceColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarIceColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarIceColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarIceColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 2] = new Dictionary<string, uint> // Mana bar fire
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarFireColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarFireColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarFireColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMManaBarFireColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 3] = new Dictionary<string, uint> // Umbral heart
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMUmbralHeartColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMUmbralHeartColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMUmbralHeartColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMUmbralHeartColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 4] = new Dictionary<string, uint> // Polyglot
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMPolyglotColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMPolyglotColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMPolyglotColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMPolyglotColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 5] = new Dictionary<string, uint> // Triplecast
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMTriplecastColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMTriplecastColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMTriplecastColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMTriplecastColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 6] = new Dictionary<string, uint> // Firestarter
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMFirestarterColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMFirestarterColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMFirestarterColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMFirestarterColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 7] = new Dictionary<string, uint> // Thundercloud
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMThundercloudColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMThundercloudColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMThundercloudColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMThundercloudColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 8] = new Dictionary<string, uint> // DoT
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMDotColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMDotColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMDotColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMDotColor.AdjustColor(.1f))
                },

                [Jobs.BLM * 1000 + 9] = new Dictionary<string, uint> // Empty
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(BLMEmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(BLMEmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(BLMEmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(BLMEmptyColor.AdjustColor(.1f))
                },

                [Jobs.RDM] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000] = new Dictionary<string, uint> // Mana Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 1] = new Dictionary<string, uint> // Mana bar threshold
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 2] = new Dictionary<string, uint> // White mana bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 3] = new Dictionary<string, uint> // Black mana bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor.AdjustColor(-.5f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 4] = new Dictionary<string, uint> // Balance
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 5] = new Dictionary<string, uint> // Acceleration
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 6] = new Dictionary<string, uint> // Dualcast
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 7] = new Dictionary<string, uint> // Verstone
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor.AdjustColor(.1f))
                },

                [Jobs.RDM * 1000 + 8] = new Dictionary<string, uint> // Verfire
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor.AdjustColor(.1f))
                },

                [Jobs.BLU] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU.AdjustColor(.1f))
                },
            };

            JobColorMap.Add(Jobs.GLD, JobColorMap[Jobs.PLD]);
            JobColorMap.Add(Jobs.PGL, JobColorMap[Jobs.MNK]);
            JobColorMap.Add(Jobs.MRD, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.LNC, JobColorMap[Jobs.DRG]);
            JobColorMap.Add(Jobs.ROG, JobColorMap[Jobs.NIN]);
            JobColorMap.Add(Jobs.ARC, JobColorMap[Jobs.BRD]);
            JobColorMap.Add(Jobs.CNJ, JobColorMap[Jobs.WHM]);
            JobColorMap.Add(Jobs.THM, JobColorMap[Jobs.BLM]);
            JobColorMap.Add(Jobs.ACN, JobColorMap[Jobs.SMN]);

            JobColorMap.Add(Jobs.CRP, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.BSM, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.ARM, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.GSM, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.LTW, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.WVR, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.ALC, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.CUL, JobColorMap[Jobs.WAR]);

            JobColorMap.Add(Jobs.MIN, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.BOT, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.FSH, JobColorMap[Jobs.WAR]);

            NPCColorMap = new Dictionary<string, Dictionary<string, uint>>
            {
                ["hostile"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile.AdjustColor(.1f))
                },

                ["neutral"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral.AdjustColor(.1f))
                },

                ["friendly"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly.AdjustColor(.1f))
                },
            };

            MiscColorMap = new Dictionary<string, Dictionary<string, uint>>
            {
                ["customhealth"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor.AdjustColor(.1f))
                },
                ["shield"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(ShieldColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(ShieldColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(ShieldColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(ShieldColor.AdjustColor(.1f))
                },
                ["mpTicker"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(.1f))
                },
                ["gcd"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(.1f))
                }
            };

            CastBarColorMap = new Dictionary<string, Dictionary<string, uint>>
            {
                ["castbar"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(CastBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(CastBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(CastBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(CastBarColor.AdjustColor(.1f))
                },
                ["targetcastbar"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor.AdjustColor(.1f))
                },
                ["targetphysicalcastbar"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor.AdjustColor(.1f))
                },
                ["targetmagicalcastbar"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor.AdjustColor(.1f))
                },
                ["targetdarknesscastbar"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor.AdjustColor(.1f))
                },
                ["targetinterruptcastbar"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor.AdjustColor(.1f))
                },
                ["slidecast"] = new Dictionary<string, uint>
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor.AdjustColor(.1f))
                }
            };
        }
    }

    public static class Jobs {
        public const uint GLD = 1;
        public const uint MRD = 3;
        public const uint PLD = 19;
        public const uint WAR = 21;
        public const uint DRK = 32;
        public const uint GNB = 37;

        public const uint CNJ = 6;
        public const uint WHM = 24;
        public const uint SCH = 28;
        public const uint AST = 33;

        public const uint PGL = 2;
        public const uint LNC = 4;
        public const uint ROG = 29;
        public const uint MNK = 20;
        public const uint DRG = 22;
        public const uint NIN = 30;
        public const uint SAM = 34;

        public const uint ARC = 5;
        public const uint BRD = 23;
        public const uint MCH = 31;
        public const uint DNC = 38;

        public const uint THM = 7;
        public const uint ACN = 26;
        public const uint BLM = 25;
        public const uint SMN = 27;
        public const uint RDM = 35;
        public const uint BLU = 36;

        public const uint CRP = 8;
        public const uint BSM = 9;
        public const uint ARM = 10;
        public const uint GSM = 11;
        public const uint LTW = 12;
        public const uint WVR = 13;
        public const uint ALC = 14;
        public const uint CUL = 15;

        public const uint MIN = 16;
        public const uint BOT = 17;
        public const uint FSH = 18;
    }
}