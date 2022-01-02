# 0.6.3.0
Features:
- Added setting to Status Effects Lists to disable mouse interactions.
- Added target border thickness setting to the Enemy List.
- Added Bard's Radiant Finale as a trackable party cooldown.

Fixes:
- Fixed Party Frames border color not working.
- Fixed party members health label not visible on preview mode.

# 0.6.2.1
- Fixed slide cast being drawn on top of the cast progress.

# 0.6.2.0
Features:
- Party Frames layout is now configured with the new Rows and Column settings:
    + Party Frames are not longer draggable / resizable when the config window is opened.
    + This is a breaking change in the config. When updating, the Party Frames will likely be in a bad position and with an incorrect layout. Use the Position, Rows and Columns settings to correct it. The actual elements inside the frames should remain exactly the same as before the update.
- Added Strata Level settings to most UI elements (these allow the user to choose which elements are drawn on top of others).
- Castbars have been given the option to set a Fill Direction.
- Castbars now has a Reverse Fill option to use in place of normal Background Color settings.
- Added more settings to Party Frames health bars so they are more in line with other unit frames.
- Added "dead" status icon in the Party Frames.
- Added Thresholds for Bard Songs with the recommended song rotation (43, 34, 43).

Fixes:
- Fixed Dancer Proc Bars so they can be individually disabled.
- Fixed Reaper and Sage not appearing in Party Frames on preview mode.

# 0.6.1.2
Features:
- Astrologian Minor Arcana Bar now has a label to track cooldown of Minor Arcana whilst a Crown Card is drawn. Note that this will not work for users of the XIVCombo plugin that has selected the option to turn Minor Arcana into Crown Play.
- Dark Knight's' Delirium Bar now shows the amount of stacks along with the duration of the buff.
- Added tooltips for party cooldowns.
- Added Rescue, Swiftcast and Tank's invulnerabilities as trackable party cooldowns (disabled by default).

Fixes:
- Fixed job and role colors not working on some job hud bars.
- Fixed Summoner's Ifrit, Titan and Garuda bars "Hide When Inactive" not working.
- Dark Knight's Mana Bar now shows correct mana thresholds on the non-chunked version.
- Fixed Missing Health color when using transparent bars.
- Fixed Party Cooldowns not saving properly in some situations.

# 0.6.1.1
Features:
- Sage Kerachole tracker now also tracks Holos uptime and the config option for this bar was renamed to show that.

Fixes:
- Fixed some game windows covering DelvUI elements.
- Fixed Summoner's Rekindle not working with mouseover on automatic mode.
- Fixed "Hide when inactive" not working properly for Astrologian Minor Arcana Bar.
- Fixed Party Cooldowns not working properly when "Show When Solo" is enabled.

# 0.6.1.0
Features:
- Added Party Cooldowns tracker:
    + You can choose which spells to track (only party-wide effects enabled for now).
    + The hud displays a grid with all the tracked spells for every member.
    + You can organize them by priorty and column.
- Added a Minor Arcana Bar for Astrologian.
- Added a third label for unit frames that is empty by default.

Fixes:
- Fixed Bard Coda bar never showing when "Hide When Inactive" is checked.
- Fixed mana bar not being visible by default for Summoner.
- Fixed threshold values for some bars being incorrect.
- Fixed disable for Sage Addersting and White Mage Blood Lily bars.
- Fixed Tank Invulnerability Background Color so it now also works for unit frame profiles that use "Missing Health Color".

# 0.6.0.3
Features:
- Updated Paladin's hud to accommodate with the latest changes:
    + Paladin hud mana bar removed since it doesn't have a special purpose anymore (the generic mana bar can be used instead).
    + Requiescat Bar now shows the amount of stacks along with the duration of the buff.

Fixes:
- Fixed Astrologian card stacks number when at max stacks.

# 0.6.0.2
Features:
- Added a Sacred Soil Bar for Scholar.
- Separated Surging Tempest and Inner Release into 2 separate bars to better accomodate for the new Warrior changes.
- New Warrior Inner Release Bar Features:
    + Shows Inner Release (or Berserk) Stacks
    + Option to show Inner Release buff duration
    + Option to track Inner Release ability cooldown
    + Optional bar glow when Primal Rend buff is gained by Inner Release at level 90.

Fixes:
- Fixed Bard's Bloodletter Bar showing 2 charges instead of 3 with the level 84 trait.
- Fixed "[time-till-max-gp]" text tag not working properly.
- Fixed Border Color on chunked bars.

# 0.6.0.1
Fixes:
- Fixed Storm's Eye Bar not tracking the new Surging Tempest buff (renamed to Surging Tempest Bar).
- Fixed Bard's DoTs and songs durations.
- Fixed mana bar not being visible by default for Sage.
- Fixed "Hide When Inactive" option of Summoner's Trance Bar. Also added an option to hide the Trance Bar when primals are active (i.e. to only show trance bar for bahamut and phoenix).
- Fixed blacklist shortcut not working on status effects lists without buffs.
- Fixed mouseover not working properly when automatic mode is disabled.

# 0.6.0.0
Endwalker Beta Release:
- Reworked job huds according to the new job changes.
- Added job huds for Reaper and Sage.
- Added a 3rd color option to "Color Based On Health Value" setting.
- Added a Use Role Color option for labels.

Fixes:
- Fixed enmity in party frames for Trust and GC parties.

# 0.5.1.0
- Improved UI of the main config window.
- Font sizes can now go up to 100. Be aware that fonts are shared between all plugins. If you add too many big fonts it may cause issues with Dalamud or straight up crash the game.
- Fixed game cursor sometimes getting stuck when interacting with DelvUI frames.
- Fixed possible crash when loading textures with a wrong format or corrupted files.

# 0.5.0.3
- Fixed more buff related crashes.

# 0.5.0.2
- Fixed crash when using the "Pet As Own Effect" setting on buff lists.

# 0.5.0.1
- Fixed castbars not drawing properly when anchored to unit frames.

# 0.5.0.0
Features:
- Added Enemy List hud.
- Added more border options for bars.
- Added tank stance indicator for the player's unit frame.
- Added "Offline Status" tracking for party frames.
- Added window clipping settings. These settings are targetted for people that are experiencing performance issues or crashes. Until we find a definitive solution, disabling window clipping might help with these issues.
- Added "Start Angle" and "Counter Clock Wise Rotation" settings for the circular GCD Indicator.
- Added GCD Threshold to GCD Indicator.
- Added sub-options to "Hide only JobPack HUD outside of combat" to always show in duties and/or when weapons are drawn.
- Implement new tag `[time-till-max-gp]`, this will show you the time until your GP hits max again.
- Added Raise Tracker and Invuln Tracker to the party frames preview mode.
- Added more settings for all castbars.
- Added "Show Source Name" to status tooltips.
- Added option to show pet effects as your own status.

Fixes: 
- Fixed Bard's Soul Voice threshold not working.
- Fixed settings not saving in some situations.
- Fixed Death Indicator color showing for players that are not reachable instead of Out of Range color.
- Fixed some more game windows not covering DelvUI elements
- Fixed losing the original positions of the game's default cast bar and job gauges when using the DelvUI hide options for them and also using multiple HUD layouts.

# 0.4.0.2
Fixes:
- Fixed some game windows not covering DelvUI elements.
- Fixed mouseover getting "stuck" with automatic mode turned off.

# 0.4.0.1
Features:
- Added a command to switch profiles: `/delvui profile ProfileName`, no quotation marks needed.
- Partial Fill Color is now used for bars when Show In Chunks is unchecked

Fixes:
- Fixed BLU Job Specific Bar config not saving and only showing when actually on the BLU job.
- Fixed AST cards not working with mouseover.
- Fixed party frames not updating properly in some cases when showing chocobo.

# 0.4.0.0
Features:
- Added "Player Status" tracking for party frames. Currently only "Viewing Cutscene" is supported.
- Added role color setting to unit frames.
- Added role/job icon to unit frames.
- Added role color settings for DPS types (melee / ranged / caster).
- Added setting to hide health related labels when appropiate in unit frames.
- Added a number format setting for labels in job specific bars.
- Added thickness settings for party frames healthbars borders.
- Added support for Trust and Squadron Command parties.
- Added Enable Combat Hotbars sub-option to use with Cross Hotbar instead.
- Added option to change background color and opacity of the Castbars.
- Added an option to display mana bar for jobs with raise (in addition to healers) in party frames.
- Added basic BLU implementation. Only supports tracking of DPS skills so far.

Fixes:
- Fixed unit and party frames not being interactable when using XivAlexander.
- Fixed some offensive spells not working when the cursor is on top of a player frame with mouseover in automatic mode, but the target is valid (ie GNB's Continuation or SMN's Egi Assault).
- Fixed some bars not displaying the remaining duration properly.
- Fixed delay with DRG Disembowel bar.
- Fixed DRG Disembowel bar hiding itself for a second when re-applying buff with "Hide When Inactive" option checked.
- Fixed "invisible" buffs.
- Fixed status whitelists/blacklists not working properly if the imported profile was created with a different game language.
- Fixed chocobo icon not showing in party frames.
- Fixed unit frame colors being incorrect for NPCs in some situations.
- Fixed castbar progress not being accurate when showing the ability icon.
- Fixed castbar's slidecast default value.
- Fixed game being frozen when importing or exporting profiles to .delvui files.
- Fixed right click sometimes not working on party frames.

# 0.3.2.0
Features:
- You can now attach HUD Layouts to be loaded alongside profiles.
- Cropped status effects icons now always use the texture with 1 stack (This will make having texture mods for buffs and debuffs easier in the future).
- Added option to dim DelvUI's setting window when not being focused.
- Added option to automatically disable the preview mode on HUD elements when the DelvUI's setting window is closed.
- Added alpha bar to color selections.
- Making party frames and status effects list areas more clear now. When you open DelvUI's settings window, these areas will automatically show reflecting the real size of the hud elements.
- Added a cleanse tracker for party frames.
- Added sub-option to Hide DelvUI outside of combat to show when weapons are drawn.
- Added sub-option to Enable Combat Hotbars to always show in duties.
- Added sub-option to Enable Combat Hotbars to show when weapons are drawn.
- When adding a status effect to a filter list, if there are multiple effects with the same name, all of them will be added to the list at once.
- Change Alpha Based on Range now also applies to the Death Indicator Background Color.
- DelvUI windows are now using Dalamus' window system.

Fixes:
- Fixed clicks not working on game windows when they are on top of unit frames or party frames.
- Fixed unit frame background color and death indicator background color being linked together.
- Fixed storm's eye bar for warrior not tracking the buff duration properly.
- Fixed mana bars not being visible when previewing party frames.
- Fixed health and mana values not being correctly displayed when previewing party frames.
- Fixed mouse clicks sometimes not responding at all.
- Fixed hud not being properly centered when resizing the game window.

# 0.3.1.2
Features:
- Added a Text Tags list when editing a label:
    + Shows a list with all available tag formats.
    + The tags can be added to the label by clicking on them.
    + Hovering with the cursor on top of a tag shows an example of what they do.

- Added option to set a Death Indicator Background Color for unit frames and party frames. Disabled by default.
- Added Buff/Debuffs for the Focus Target.
- Added a sub-option to Hide DelvUI outside of combat to always show in duties.
- Re-enabled labels for chunked bars.
- When previewing buffs or debuffs, there will always be at least one effect with stacks now.
- Added show border option on tooltips.

Fixes:
- Fixed crashes when logging out and then logging back in.
- Fixed Target of Target and Focus Target castbars not showing damage type colors.
- Fixed stacks on uncropped status effects.
- Fixed non-latin characters encoding error in player names.

# 0.3.1.1
- Fixed mouseover not working in some situations.

# 0.3.1.0
Features:
- Added a mouseover settings under Misc > HUD Options. Three modes supported:
    1. Mouseover completely disabled for DelvUI frames.
    2. Automatic mode: All your actions will automatically assume mouseover when your cursor is on top of a unit frame. Mouseover macros or other mouseover plugins are not necessary and WON'T WORK with DelvUI in this mode!
    3. Regular mode: DelvUI unit frames will behave like the game's ones. You'll need to use mouseover macros or other mouseover related plugins in this mode.

- Added a command to forcibly switch the active Job Pack to the specified job:
    + Turn on with `/delvui forcejob job`
    + Turn off with `/delvui forcejob off`

Fixes:
- Fixed some threshold or timers not working properly in some job specific bars.
- Fixed mouseover not working for actions bound to mouse buttons (ie M3, M4, etc).
- Fixed smooth HP in party frames not being smooth enough.
- Fixed fonts not properly reloading when switching profiles.
- Fixed tooltips missing characters / not working in other languages with a non-standard latin alphabet.
- Fixed tooltips not showing when the config window is opened.
- Fixed tank invuln breaking when two tanks used invuln at the same time.
- Fixed tank invuln breaking when an actor died with invuln up.
- Fixed bugs with the mana bars.

# 0.3.0.1
- Fixed smooth HP in party frames.
- Fixed tooltips doing weird things when the config window is opened.
- Fixed BLM's Polyglot Bar Glow not hiding.
- Fixed DRK mana bar showing label when chunked.
- Fixed DRK Dark Arts proc color not showing.
- Fixed DRK mana bar partial fill color when Dark Arts has procced.
- Removed glow option from DRK Dark Arts proc.
- Fixed party frames preview not showing accurate values for health and mana.
- Fixed PLD Invulnerability not being properly tracked.
- Fixed some crashes.

# 0.3.0.0
This version brings A LOT of changes. Chances are your settings will be invalid / broken. We recommend starting from a clean state.

Features:
- Added party frames. Currently they support:
    + Resizable area and fill order. The frames will adapt to the area and grow in the desired direction.
    + Light Parties and Full Parties. Alliances are not supported (you will only see your party).
    + "Regular" parties and Cross-World parties.
    + Chocobo (GC and Trust systems not supported).
    + Their own health bar, mana bar and cast bar settings.
    + Their own buffs and debuffs settings with their individual filter settings.
    + The game's party sorting settings. You can use the game's sorting options and change player's order in the game's social window, and it will be reflected in DelvUI's party frames.
    + Overriding your own position in the frames through the settings or by Ctrl+Alt+Shift+Clicking on another player (this will move you to that position in the list).
    + Raise Tracker. Tracks Raise casts and Raise buffs on the party.
    + Tank Invulnerability Tracker.
    + Mouseover. All abilities automatically assume "mouseover" if the cursor is on top of a unit frame when the ability is used.

- Added Experience Bar hud.
- Added Pull timer hud.
- Added Limit Break hud.
- Every label is now customizable (font, size, color, etc).
- Added mana bars for Target, Target of Target and Focus Target.
- Added anchor options for basically every UI element.
- Castbars, Mana Bars and Buff/Debuffs can now be anchored to their respective unit frame.
- Big re-work on all job packs (more settings for all bars and labels).
- Re-worked "Export" and added "Reset" feature. You can right-click in a section or tab to export or reset.
- Re-worked "Import". Import is now a separated section. You can import any string and select exactly which parts you want to bring in.
- Implemented profiles. You can clone, export and import profiles. Also, 
- Implemented auto-switch feature for profiles. You can define which profile you want for each job.
- DelvUI's elements should now properly be covered by the game's windows (most of the time).
- Added more color options for unit frames.
- Added more text tags.
- Added support for Penumbra texture mods.
- Added option to show the id of status effects on the tooltips.
- Added font options fot tooltips.
- Added option to show status effects with their original shape and dispellable indicator.
- Added BLM specific options for the MP Ticker.
- Smooth HP transitions for unit- and party frames.

Fixes:
- Fixed several issues in the HUD unlocked mode.
- Fixed crash related to the DNC job pack.
- Fixed several issues related to buffs and debuffs.
- Fixed and re-added hide options for some of the game's default UI elements.
- Fonts now use a user-defined path instead of the plugin's directory to prevent the font files from being deleted when the plugin is updated.
- Lots of fixes and improvements to the config window.
- Lots of general bug fixes and improvements.

# 0.2.1.1
- Temporarily disabled options to hide some of the game's default UI elements because it was causing crashes.

# 0.2.1.0
- Added mouseover support for unit frames. All abilities automatically assume "mouseover" if the cursor is on top of a unit frame when the ability is used.
- Added buffs and debuffs lists.
- Added "Unlock HUD" mode where you can freely drag any DelvUI element on the screen.
- Added fonts and text size support
- Many improvements in the configuration window.
- General bug fixes and improvements.

# 0.1.0.0
- First public beta version.
