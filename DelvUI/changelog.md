# 0.4.0.0
Features:
- Added "Player Status" tracking for party frames. Currently only "Viewing Cutscene" is supported.
- Added role color setting to unit frames.
- Added role/job icon to unit frames.
- Added setting to hide health related labels when appropiate in unit frames.
- Enable Combat Hotbars sub-option to use with Cross Hotbar instead.

Fixes:
- Fixed some offensive spells not working when the cursor is on top of a player frame with mouseover in automatic mode, but the target is valid (ie GNB's Continuation or SMN's Egi Assault).
- Fixed some bars not displaying the remaining duration properly.
- Fixed delay with DRG Disembowel bar.
- Fixed DRG Disembowel bar hiding itself for a second when re-applying buff with "Hide When Inactive" option checked.
- Fixed "invisible" buffs.
- Fixed status whitelists/blacklists not working properly if the imported profile was created with a different game language.

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
