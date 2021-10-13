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
    + Tank Invulnerabilty Tracker.
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
- Added more textags.
- Added support for Penumbra texture mods.
- Added option to show the id of status effects on the tooltips.
- Added font options fot tooltips.
- Added option to show status effects with their original shape and dispellable indicator.
- Added BLM specific options for the MP Ticker.

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
- Added "Unlock HUD" mode where you can freely drag any DelvUI element on the scren.
- Added fonts and text size support
- Many improvements in the configuration window.
- General bug fixes and improvements.

# 0.1.0.0
- First public beta version.