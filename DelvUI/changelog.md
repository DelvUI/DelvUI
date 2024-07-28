# 2.2.0.9
- Changed Mug to Dokumori in Party Cooldowns.

# 2.2.0.8
- Fixed chat being spammed with hotbar commands.
- Fixed Chocobo showing up as a Viper in the party list.
- Fixed Dark Knight's Blood Weapon Stacks being 3 instead of the previous 5.

# 2.2.0.7
- Fixed Honorific Title integration.
- Fixed chat being spammed with hotbar commands.
- Fixed Viper's Vipersight Bar glow not working.

# 2.2.0.6
- Fixed default job gauge flickering when "Hide Default Job Gauge" is enabled.
- Fixed Viper's Rattling Coil and Anguine Tribute bars not working properly with "Hide when Inactive".
- Fixed Viper's Serpent's Offering gauge showing Awakened Timer even when "Enable Awakened Timer" is disabled.
- Fixed Party Frames Ready Check icons not working.

# 2.2.0.5
- Updated Monk's Fury Stacks Bar for 7.01 patch changes.
- Fix Bard's Radiant Finale duration in Party Cooldowns.
- Fixed Who's Talking Indicator in Party Frames.
- Fixed default job gauges sometimes getting stuck after disabling `Misc > HUD Options > Hide Default Job Gauge`.

# 2.2.0.4
- Added duration labels for Pictomancer's Hammer Time and Hyperphantasia Bars.
- Fixed Sage's Eukrasian Dyskrasia not being tracked in the Eukrasian Dosis Bar.
- Fixed Monk's Masterful Blitz colors.

# 2.2.0.3
- Added `[name:abbreviate]`, `health:percent-hidden` and `level:hidden` text tags.
  * For more info check https://github.com/DelvUI/DelvUI/wiki/Text-Formatting

- Added several improvements to the Viper Hud:
  * Added hind/rear skill colors.
  * Serpent Offering can now be shown in chunks.
  * Added reawakened timer toggle.
  * Added color option to show when Reawakened is ready to be used.

- Attempt to fix inputs from not working when the plugin is updated.
- Fixed Ninja's Kunai's Bane not being tracked properly in the Trick Attack Bar.
- Fixed right click context menu not working on the Party Frames.

# 2.2.0.2
- Added subtractive color options to the Pictomancer's Pallete Bar.
- Added setting to the Pictomancer's Creature Canvas bar to show an empty drawing section when it contains Pom + Wings.
- Fixed shadows not working on chunked bars.
- Fixer Monk's Masterful Blitz Bar colors.

# 2.2.0.1
- Fixed `[health:current-percent]` and `[mana:current-percent]` not working properly.
- Fixed mouseover not working when using other plugins like MOAction or ReAction.
- Fixed Party Frames not working when using the Trust party system.

# 2.2.0.0
- Added support for Dawntrail and Dalamud API 10.

# 2.1.3.1
- Fixed "Hide Default Castbar" and "Hide Default Job Gauges" not working properly.

# 2.1.3.0
- Added "Always show while target exists" visibilty setting.
- Fixed the default castbar, job gauges and pull timer sometimes being off screen when DelvUI is unloaded.
- Fixed the chat getting spammed with hotbar commands during dialogues.

# 2.1.2.1
- Fixed fonts not reloading properly when switching profiles.

# 2.1.2.0
- Added support for the "new" Dalamund Fonts API.
- Added support for multi-line text in all labels.
- Potential fixes for some crashes.

# 2.1.1.3
- Fixed Limit Break bar not working.

# 2.1.1.2
- Attempting to fix crashes / exceptions.

# 2.1.1.1
- Fixed support for cyrillic characters.

# 2.1.1.0
- Updated support for the Pet Nicknames plugin.
- Added support for cyrillic characters in fonts.

# 2.1.0.2
- Fixed weird artifacts in some parts of the config window.

# 2.1.0.1
- Fixed status effects sometimes not showing.

# 2.1.0.0
- Added support for patch 6.5 and Dalamud API 9.
- Fixed Castbar's slide cast not adjusting properly to all fill directions.

# 2.0.3.1
- Fixed Paladin's Requiescat Bar fill direction not working properly.
- Added `/dui` command to open DelvUI's settings.

# 2.0.3.0
- Added support for the Honorific plugin:
  * Note that only the custom Titles are used and not the colors.

- Added Party Title Label to Party Frames.

# 2.0.2.1
- Invulnerability and Raise trackers now use a numeric label:
  * This means the amount of decimals shown can be now configured.

- Added "Change Labels Colors When Active" setting to Party Cooldowns Bars / Icons.
- Added "Show Effect Duration" and "Show Remaining Cooldown" to Party Cooldowns Time Labels.
- Fixed `-short` Text Tags not using the correct regional formatting.

# 2.0.2.0
- Fonts can now be applied globally to all elements:
  * They will replace the font of the same size in all labels.

- The "Sort By Duration" setting in Status Effect Lists can now be set to Ascending or Descending.

# 2.0.1.0
- Added a "User Regional Number Format" setting in the Misc section:
  * This can be set to use the system's settings, or default to English.

- Added new `-formatted` Text Tags for numeric values.
- Corrected a lot of numeric Text Tags that were using the wrong formatting.
- Added Gunbreaker's No Mercy to the tracked Party Cooldowns.
- Added Bard's Raging Strikes to the tracked Party Cooldowns.
- Party Cooldowns priority can now be edited with double click.
- Fixed tooltips showing "()" when the source name is empty.
- Fixed tooltips making other Dalamud windows to lose focus.

# 2.0.0.0
- Added a Bar Texture setting for all bars:
  * Each bar can use a different Bar Texture and Draw Mode.
  * Supported Draw Modes are: Stretch, Repeat Horizontal, Repeat Vertical and Repeat.
  * DelvUI comes with several textures to choose from.
  * There's a new section under Customization > Bar Textures where custom textures can be added.
  * Textures can be previewed in Customization > Bar Textures.
  * Also in this section it is possible to apply a particular Bar Texture and Draw Mode to all bars with one click.

- Party Cooldowns can now be tracked per player directly in the Party Frames:
  * Tracked cooldowns are configured in Party Cooldowns > Tracked Cooldowns.
  * Cooldowns can be set to be shown in the Party Cooldowns section or in the Party Frames (or both).
  * Cooldowns can also be configured so they are only visible for certain jobs (For example making Swiftcast only visible for healers).
  * Added a lot of cooldowns that were missing in the Tracked Cooldowns list.

- Added support for the Wotsit plugin:
  * You can search for DelvUI settings by name to open the config window in the relevant section.
  * For example searching for `black mage` will yield a result named `DelvUI Settings: Job Specific Bars > Caster > Black Mage`, and selecting it will open DelvUI's Black Mage HUD configuration.

- Added support for the Who's Talking plugin:
  * Can be configured in Party Frames > Icons > Who's Talking.
  * Requires v0.6.0.0+ of the Who's Talking plugin.

- Added settings for Enemy Nameplate Castbars to match the width or height of the Health Bar:
  * These are intended to be used when the Health Bar size changes for targeted units.
    
- Added an auto backup feature that runs every time DelvUI gets an update.
  * Automatic backups can be found in `%APPDATA%\XIVLauncher\pluginConfigs\DelvUI\Backups`.

- Added support for the Pet Nicknames plugin (by Glyceri). 
- Added a setting to all Castbars to separate the icon from the bar.
- Added setting to the Player State Icon in Nameplates to only show for the disconnected state (aka pokeball).
- Added Use Job Colors setting for Party Cooldown bars.
- Added Sort by Duration setting for all Status Effect Lists.
- Added minor improvements to the Party Frames preview.
- Added new possible Growth Directions for Status Effect Lists and Party Frames Cooldowns.
- Added `[job-full]` text tag.
- Added `[exp:required-to-level]` and `[exp:required-to-level-short]` text tags.
- Moved "Experience Bar", "GCD Indicator", "Pull Timer", "Limit Break" and "MP Ticker" from "Misc" to the new "Other Elements" section.
- Moved "Fonts" from "Misc" to the new "Customization" section.
- Updated and improved the default profile that comes with DelvUI.

- Fixed Nameplates with interaction enabled sometimes making the mouse clicks to be stuck as if they were being held down.
- Fixed DelvUI sometimes making Dalamud windows lose focus.
- Fixed targeted unit Nameplate not drawing on top of other Nameplates.
- Fixed Order Label not working for Enemy Nameplates.
- Fixed preview not working for Sign Icons.
- Fixed DelvUI trying to show/hide hotbars during Chocobo races which resulted in chat error messages.

# 1.7.1.5
- Fixed Sign Icons not working.
- Attempt to fix DelvUI exception that causes ImGUI's fonts to be broken.

# 1.7.1.4
- Attempt to fix DelvUI exception that causes ImGUI's fonts to be broken.

# 1.7.1.3
- Attempt to fix DelvUI exception that causes ImGUI's fonts to be broken.

# 1.7.1.2
- Attempt to fix DelvUI exception that causes ImGUI's fonts to be broken.

# 1.7.1.1
- Fixed Scholar's Fairy Gauge Bar not working.

# 1.7.1.0
- Added "Different Size when targeted" setting for Nameplates.
- Added "Custom Color when being targeted" setting for Enemy Nameplates.
- Fixed "Full" Occlusion Mode for Nameplates.

# 1.7.0.0
- Added support for Patch 6.4.

# 1.6.2.1
- Fixed Nameplates special Window Clipping settings affecting other elements.
- Fixed Default Target Castbar setting not working properly.

# 1.6.2.0
- Added Window Clipping settings to enable some special behaviors for Nameplates.
- Fixed the game's focus target UI clipping DelvUI elements.
- Fixed Duty Finder window not clipping DelvUI elements properly.

# 1.6.1.0
- Added a new "Occlusion Type" setting for Nameplates:
  * This lets you control which kind of objects in the world will cover nameplates.
  * This setting's default value is "Walls".
  * In previous versions, the default was "Walls and Objects" which was causing a lot of unexpected behaviors.

- Fixed released minions in the Island Sanctuary not having a nameplate.
- Fixed elements in the enemies nameplates not being visible if both the Health Bar and Name Label were hidden.
- Fixed some game windows not covering DelvUI elements.
- Fixed Status Lists layout not being calculated correctly with high padding.

# 1.6.0.1
- Added "Disable Interaction" setting to Nameplates' Health Bars.
- Fixed Nameplates eating mouse inputs even when their Health Bars are not visible.
- Fixed Nameplates Occlusion not working for some players.
- Fixed some elements not anchoring properly to enemy Nameplates when the Health Bar is not visible.
- Fixed some object names showing as "?" in their Nameplates.
- Fixed Warrior's Style 3 job icon.

# 1.6.0.0
- Added custom Nameplates:
  * Like other DelvUI components, enabling this feature won't automatically disable the game's counterpart.
  * You are supposed to manually hide the types of nameplates you don't want from either the game or DelvUI.

- Unit frames for NPCs with 1 max health points wont show health values if the "Hide Health if Possible" setting is enabled for the label (previously it only did it for units with no health points).
- Fixed issues with colors in unit frames for NPCs.

# 1.5.4.0
- Implemented format groups for labels:
  * This allows you to have entirely different formats for players and NPCs.
  * The format is `{unitType=<text>}`. Anything inside the brackets will only be shown for units of that `unitType`.
  * Valid unitTypes are `player` and `npc`.
  * The text inside a group can have text tags.
  * Example `{player=Lv [level]  [name:initials]}{npc=[distance]y  [name]}`:
    * This will display "Lv 90  J. D." for a level 90 player named "John Doe".
    * This will display "30y  Striking Dummy" for a Striking Dummy NPC that is 30 yalms away from you.

- Removed some text tags: `[health:current-max]`, `[health:current-max-short]`, `[mana:current-max]`, `[mana:current-max-short]`.
  * These can easily be replaced by combining other existing tags.
  * Example: `[health:current] | [health:max]`.

- Updated text tag `[time-till-max-gp]`:
  * New format is `mm:ss` (the parenthesis were removed).

- Fixed experience text tags not showing in the text tags list.

# 1.5.3.2
- Fixed castbar disappearing mid-cast for some enemy abilities.
- Fixed Enemy List sign icons preview not being disabled automatically.

# 1.5.3.1
- Fixed name tags length cap not working properly.

# 1.5.3.0
- Reworked name text tags:
  * A lot of existing text tags have been removed.
  * You can now specify the type of unit for a name text tag.
    * This allows to display player names and npc names differently on the same label.
    * Example: `[player_name:initials][npc_name]` would display the initials for a player, and the full name for an npc.
    * You can still use tags that start with `[name:` that work for both types of units.
  * Additionally you can now specify a cap for the length of the names by adding `.#` at the end.
    * Example: `[name.5]` would display `Tisch` for the name `Tischel`.

- Disabled Sign Icons from unit frames for non-combat NPCs (until I find a better fix).

# 1.5.2.1
- Added the '[chocobo-time]' text tag.
- Fixed Ignore setting in the Custom Mouseover Areas.

# 1.5.2.0
- Added visibility settings for PvP.
- Added the ability to disable mouseover functionality in individual frames.
  * To use this you have to enable the Custom Mouseover Area and enable the Ignore setting.

# 1.5.1.1
- Fixed Text Tags popup not scrolling properly.

# 1.5.1.0
Features:
- Added Sign Icons to all Unit Frames, Party Frames and Enemy List.
- Reworked Machinist's Overheat Bar:
  * It now uses chunks for the 5 stacks.
  * The label displays the remaining duration of the buff.
  * Due to these changes, the settings for this bar will be reset.

Fixes:
- Fixed tooltips sometimes cutting off.
- Fixed Machinist Automaton Queen Bar's choppiness.

# 1.5.0.2
- Fixed Enemy List castbars not working properly.

# 1.5.0.1
- Added Machinist's Dismantle to Party Cooldowns.
- Fixed Invulnerabilities and Raise Trackers' icons not being disableable.

# 1.5.0.0
- Added support for Patch 6.3 and Dalamud Api8.
- Removed the DoT Bar from Paladin's hud.
- Made improvements to better handle obfuscated abilities and status effect names in future raids.

# 1.4.5.2
- Fixed job and role colors not working properly for the Party Frames Health Bar's order label.

# 1.4.5.1
- Fixed Enemy List castbars not working properly.

# 1.4.5.0
- Fixed DelvUI not working properly with Penumbra v0.5.8.0 and their new Interface Collection.

# 1.4.4.1
- Fixed abilities and status effect names for German, French and Japanese in Abyssos (Savage).
- Fixed tooltips being shown when mouseovering on hidden elements.
- Fixed Party Frames preview looking too dark.
- Fixed Enemy List Shadows.

# 1.4.4.0
- Added a Ready Check Status Icon for the Party Frames.
- Added a "Count Swiftcast" setting to Black Mage's Triplecast Bar, which will add an extra charge to the bar when the buff is active.
- Fixed Party Cooldowns not resetting after a wipe.

# 1.4.3.0
- Added a "First of my role" setting to the player overridden position for the Party Frames:
  * It will place the player as the first of their current role, respecting the game's party list sorting settings.
  * In cross-world parties the party is not sorted so this setting will not work properly.

# 1.4.2.3
- Fixed Focus Target Castbar not showing sometimes.
- Fixed Target of Target Castbar not showing sometimes.

# 1.4.2.2
- Fixed abilities and status effects names in P8S (English only for now).

# 1.4.2.1
- Fixed abilities and status effects names in P5S, P6S and P7S (English only for now).

# 1.4.2.0
- Fixed some enemy abilities showing a castbar when they shouldn't.

# 1.4.1.1
Fixes:
- Fixed Target of Target (again).
- Fixed Astrologian's Minor Arcana cooldown (again).

# 1.4.1.0
Features:
- Added visibility settings for Island Sanctuary.

Fixes:
- Fixed Ninja Job Hud not working properly.
- Fixed Astrologian's Minor Arcana cooldown.
- Fixed job icons not working on cross-world parties.
- Fixed Target of Target not working.
- Fixed name hide logic for the Party Frames.
- Fixed enmity indicators not working in Party Frames.

# 1.4.0.0
Features:
- Added support for Patch 6.2 and Dalamud Api7.

Fixes:
- Fixed the Echo status effect not being removed when right clicking on it.

# 1.3.1.1
- Fixed crash related to obfuscated texts for Actions or Status Effects.

# 1.3.1.0
Features:
- Added a setting to hide the player name while casting on the Party Frames.
- Added shadow options to all bars and added shadow thickness for labels:
  + Due to these changes, all labels' shadow settings will be reset.

Fixes:
- Blood Weapon and Delirium fill directions now follow the correct direction.

# 1.3.0.0
- Attempt to fix "seemingly random" crashes (mostly related to Window Clipping).

# 1.2.1.0
Features:
- Added "Hide when in duty" option to Visibility settings.
- Added an icon to the experience bar that shows when the player is in a sanctuary.
- Order Labels for Party Frames and Enemy List now uses the same symbols from the game:
  * This means it will no longer be a text like '[A]' or '[1]'.
  * These labels will no longer use a custom font, instead there's a Scale setting to make them bigger.
  * Due to these changes, these label's settings will be reset.
- Reworked Monk's Forms bar:
  * It now uses chunks for the 3 forms.
  * A different color can be assigned for each form and also Formless Fist.
  * When Formless Fist is active, it will show a progress bar with the duration of the buff instead of the chunks.
  * Due to these changes, the settings for this bar will be reset.

Fixes:
- Fixed cooldown for Troubadour, Shield Samba and Tactician in Party Cooldowns:
  * It now adapts to the level of each player to account for the level 88 trait that reduces the cooldown from 120s to 90s.
  * The cooldown will now read as "90-120" in the list for these actions.
- Fixed hotbar commands being spammed while doing PvP.

# 1.2.0.1
Fixes:
- Fixed DelVUI sometimes crashing on startup.
- Fixed hotbar commands being spammed in the chat.

# 1.2.0.0
Features:
- Completely reworked visibility options for DelvUI elements and the game's hotbars:
  * Most DelvUI elements now have their own visibility settings and can be changed individually.
  * A global setting can be applied to all elements in 'Visibility > Global'.
  * Hotbar visibility settings were moved from 'Misc > HUD Options' to 'Visibility > Hotbars'.
  * Due to the change in the structure, all visibility related settings will be reset.
- Updated the main config window so it adapts better to higher Dalamud Global Font Scales.
- Added a setting to prevent DelvUI from overriding the global Dalamud style (Misc > HUD Options > Use DelvUI style).

Fixes:
- Fixed tooltips still sometimes not working properly with high Dalamud Global Font Scales.

# 1.1.5.0
Fixes:
- Fixed tooltips not working properly with high Dalamud Global Font Scales.

# 1.1.4.1
Fixes:
- Fixed DelvUI not loading.

# 1.1.4.0
Fixes:
- Fixed mouse cursor sometimes becoming unresponsive on certain Window Clipping settings.

# 1.1.3.0
Fixes:
- Fixed actions and status effects names in Dragonsong's Reprise.
- Fixed cast times being inaccurate for some abilities in Dragonsong's Reprise and other encounters.

# 1.1.2.0
Features:
- Improved performance when switching profiles in some situations.
- Added sub-setting to show when crafting when DelvUI is hidden out of combat.
- Gathering nodes integrity will be shown as health in the target unit frame:
  * This DOES NOT include text tags support.
  * Only the health bar will reflect the integrity, no health values will be visible in the labels.

Fixes:
- Fixed Addersting Bar stacks appearing full when below level 66.
- Fixed unintentional delay when hiding the target castbar after a successful cast.

# 1.1.1.1
- Fixed mouse inputs sometimes getting stuck (bug introduced in v1.1.1.0).

# 1.1.1.0
Features:
- Added support for in-game fonts.
- Added support for soft targeting in the Party Frames and Enemy List (units can be highlighted when soft targeting them).
- Changed Dark Knight's Blood Weapon Bar to a chunked bar to reflect the changes in patch 6.1.

Fixes: 
- Fixed several game windows not being covered by DelvUI elements with Window Clipping enabled.
- Fixed Party Cooldowns not resetting on a wipe.
- Fixed Flourish buffs not working properly for Dancer.

# 1.1.0.0
Features:
- Updated for patch 6.1 support.
- DelvUI option was removed from the in-game system menu:
  * This feature used to conflict with Dalamud's system menu options which was not ideal.
  * We ran into some issues with this feature and were not really comfortable with the way it was implemented so we decided to remove it.
  * You can use "/delvui" to access the config window. We might add a shorter alias for this command in the future for convenience.

Fixes:
- Fixed party sorting for Trust and Command Missions parties.
- Fixed party leader icon for Trust and Command Missions parties.

# 1.0.1.3
Fixes:
- Fixed the duration of Dancer Flourishing buffs.
- Fixed Dancer "Fan Dance III" proc not showing on the Feather Gauge if "Hide When Inactive" was selected and no other feathers were available.

# 1.0.1.2
Features:
- Added Partial Fill Color options for Sages' "Addersgall" bar and White Mages' "Lily" bar.
- Added options for Glow to White Mages' "Blood Lily" bar when filled.
- Added White Mages' "Liturgy of the Bell" to the Party Cooldowns tracker.
- Added a sub-option to the Experience Bars' "Hide When Inactive" to hide the experience bar in downsynced content when on a max level job.
- Added new text tags for Health and Mana percentages to also show for whole numbers (ie 78.0% instead of 78%).

Fixes:
- Reaper "Death Gauge" bar now has Fill Direction options.

# 1.0.1.1
Features:
- Added a sub-option to keep hiding Player Unitframe outside of combat when not full health if "Hide DelvUI outside of combat" is enabled.
- Added a separate option to always hide the Player Unitframe when at full health.

Fixes:
- Fixed status effects names and descriptions on P4S (English only for now).

# 1.0.1.0
Features:
- Added Rounding Mode, an option to choose in what way labels are handled (truncate, floor, ceil, round).
- Added a command to toggle the default job gauges `/delvui toggledefaulthud`.

Fixes:
- Fixed cast names on P4S (English only for now).
- Fixed "My Effects First" for Buffs and Debuffs that was broken in the latest patch.
- Fixed weird crashes when the plugin is unloading.
- Fixed crash when manually setting a Party Cooldown section to an invalid value.

# 1.0.0.3
Features:
- Added "Use Job Color" and "Use Role Color" options when using "Use Max Health Color".
- Added option to sort permanent buffs/debuffs first.

Fixes:
- Fixed Machinist's Automaton Queen/Rook Autoturret bar not showing progress. 
- Removed duplicate proc bars for Dancer and renamed them as appropriate.

# 1.0.0.2
Features:
- Added "Right" and "Left" growth directions for Party Cooldowns:
  * "Columns" renamed to "Sections". When using a vertical growth direction the Section of a cooldown would be the column. On horizontal directions, it would be the row.
  * Due to adjustments in the positioning logic, the list might be slightly moved when updating.
- Added an option to show mana up to 10k on Dark Knight's mana bar. Note that this will break thresholds.
- Separated the Automaton Queen/Rook Autoturret duration tracker from the Battery Gauge into its own bar.

Fixes:
- Fixed "Use Job Color" and "Use Role Color" for status effects duration and stacks labels.
- Fixed Job Huds strata level not working properly.
- Fixed status effects durations when they are 1 hour or longer.
- Fixed border thickness not working on some chunked bars.

# 1.0.0.1
Fixes:
- Fixed Status Effect Lists interactions not working with Window Clipping disabled or in Performance Mode.
- Fixed GCD Indicator and MP Ticker strata levels not working properly.
- Fixed strata levels not saving properly for some elements.
- Fixed Death Indicator Color when using Missing Health Color for unit and party frames.
- Fixed Player Castbar freezing when interacting with some objects.
- Fixed targets in the Enemy List not being targetable when "Highlight When Hovering With Cursor" was disabled.
- Fixed "Change Alpha Based on Range" for Missing Health Color and Background Color on Enemy List frames.

# 1.0.0.0
Features:
- Several changes made to Window Clipping:
  * Moved from Misc > HUD Options to its own tab under Misc.
  * Will be disabled by default since it is known to cause random crashes to a small portion of users.
  * It can still be manually enabled through the config window.
  * A new "Performance" mode was added which has the clipping functionality reduced in favor of FPS.
  * Details on all the modes can be found in Misc > Window Clipping.
- Added "Change Alpha Based on Range" options for Target, Target of Target and Focus Frames separated into Friendly and Enemy settings.

Fixes:
- Fixed "Change Alpha Based on Range" for Missing Health Color and Background Color on party frames.
- Updated the pre-populated white list for the Custom Effects. Note that this will not update existing profiles.
- Fixed Bard's Troubadour and Dancer's Shield Samba not being tracked correctly in some situations.
- Fixed Dancer's Technical Finish not being tracked properly.

# 0.6.3.4
- Fixed window clipping not working.

# 0.6.3.3
Features:
- Added element background colors for Black Mage's custom mana bar.
- Added element colors for Black Mage's Paradox bar.

Fixes:
- GCD Indicator now works for melee classes under level 30.
- Fixed Black Mage's mana bar "Use Element Color" setting not disabling properly.

# 0.6.3.2
- Fixed crashes caused by setting Missing Health Color to use Job or Role Colors.

# 0.6.3.1
Features:
- Added option to show Total Casttime on top of Current Cast Time for Castbars.
- Added option to use Job and Role Color as Background Color in Party Frames.
- Added option to use Job and Role Color as Missing Health Color in Party Frames.
- Added option to use Role Color as Background Color in Unit Frames.
- Added option to use Job and Role Color as Missing Health Color in Unit Frames.

Fixes:
- Fixed positioning of the label on Dark Knight's Delirium Bar.
- Fixed positioning of the label on Warrior's Inner Release Bar.
- Spearfishing window will now be drawn on top of DelvUI.
- Fixed Party Frames not updating properly when resizing the health bars.
- Fixed Party Frames Tank Invulnerability Background Color when using Missing Health Color.

# 0.6.3.0
Features:
- Added setting to Status Effects Lists to disable mouse interactions.
- Added target border thickness setting to the Enemy List.
- Added Bard's Radiant Finale as a trackable party cooldown.
- Added Dancer's Technical Finish as a trackable party cooldown.
- Added a Custom Mouseover Area setting too all types of unit frames.

Fixes:
- Fixed Party Frames border color not working.
- Fixed party members health label not visible on preview mode.

# 0.6.2.1
- Fixed slide cast being drawn on top of the cast progress.

# 0.6.2.0
Features:
- Party Frames layout is now configured with the new Rows and Column settings:
  * Party Frames are not longer draggable / resizable when the config window is opened.
  * This is a breaking change in the config. When updating, the Party Frames will likely be in a bad position and with an incorrect layout. Use the Position, Rows and Columns settings to correct it. The actual elements inside the frames should remain exactly the same as before the update.
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
  * You can choose which spells to track (only party-wide effects enabled for now).
  * The hud displays a grid with all the tracked spells for every member.
  * You can organize them by priority and column.
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
  * Paladin hud mana bar removed since it doesn't have a special purpose anymore (the generic mana bar can be used instead).
  * Requiescat Bar now shows the amount of stacks along with the duration of the buff.

Fixes:
- Fixed Astrologian card stacks number when at max stacks.

# 0.6.0.2
Features:
- Added a Sacred Soil Bar for Scholar.
- Separated Surging Tempest and Inner Release into 2 separate bars to better accommodate for the new Warrior changes.
- New Warrior Inner Release Bar Features:
  * Shows Inner Release (or Berserk) Stacks
  * Option to show Inner Release buff duration
  * Option to track Inner Release ability cooldown
  * Optional bar glow when Primal Rend buff is gained by Inner Release at level 90.

Fixes:
- Fixed Bard's Bloodletter Bar showing 2 charges instead of 3 with the level 84 trait.
- Fixed `[time-till-max-gp]` text tag not working properly.
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
- Added window clipping settings. These settings are targeted for people that are experiencing performance issues or crashes. Until we find a definitive solution, disabling window clipping might help with these issues.
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
- Added setting to hide health related labels when appropriate in unit frames.
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
- DelvUI windows are now using Dalamud's window system.

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
  * Shows a list with all available tag formats.
  * The tags can be added to the label by clicking on them.
  * Hovering with the cursor on top of a tag shows an example of what they do.

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
  * Mouseover completely disabled for DelvUI frames.
  * Automatic mode: All your actions will automatically assume mouseover when your cursor is on top of a unit frame. Mouseover macros or other mouseover plugins are not necessary and WON'T WORK with DelvUI in this mode!
  * Regular mode: DelvUI unit frames will behave like the game's ones. You'll need to use mouseover macros or other mouseover related plugins in this mode.

- Added a command to forcibly switch the active Job Pack to the specified job:
  * Turn on with `/delvui forcejob job`
  * Turn off with `/delvui forcejob off`

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
  * Resizable area and fill order. The frames will adapt to the area and grow in the desired direction.
  * Light Parties and Full Parties. Alliances are not supported (you will only see your party).
  * "Regular" parties and Cross-World parties.
  * Chocobo (GC and Trust systems not supported).
  * Their own health bar, mana bar and cast bar settings.
  * Their own buffs and debuffs settings with their individual filter settings.
  * The game's party sorting settings. You can use the game's sorting options and change player's order in the game's social window, and it will be reflected in DelvUI's party frames.
  * Overriding your own position in the frames through the settings or by Ctrl+Alt+Shift+Clicking on another player (this will move you to that position in the list).
  * Raise Tracker. Tracks Raise casts and Raise buffs on the party.
  * Tank Invulnerability Tracker.
  * Mouseover. All abilities automatically assume "mouseover" if the cursor is on top of a unit frame when the ability is used.

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
- Added font options for tooltips.
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
