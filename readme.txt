========================================================================================
               Mullet Media-Strike 6.9 - Game State Integration
========================================================================================

This software integrates with **Counter-Strike 2 & CSGO** using
a custom Game State Integration (GSI) configuration file.

Follow these quick steps to get everything working smoothly.

(You can also just run the auto-configuration and not have to worry about anything.)

----------------------------------------------------------------------------------------
1. PLACE THE CONFIG FILE
----------------------------------------------------------------------------------------
   - Turn off Counter-Strike if its running.

   - Copy the provided **game_stateintegration_media.cfg** file
     (Its in the same folder as this readme)

   - Paste it into the following folder:
     ```
     [...]\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg\
     ```

----------------------------------------------------------------------------------------
2. YOU ARE DONE!
----------------------------------------------------------------------------------------
   - Start Counter-Strike again.

   - Start Mullet Media-Strike 6.9.

----------------------------------------------------------------------------------------
3. (OPTIONAL) REPLACE ANY EXISTING CONFIG - FOOLPROOFING!
----------------------------------------------------------------------------------------
   - If a file named **gamestate_integration.cfg** already exists in that folder:
     → Delete it.
     → Replace it with **game_stateintegration_media.cfg**.

----------------------------------------------------------------------------------------
4. (OPTIONAL) CONFIGURE LOCALHOST - skip if you want to use defaults.
----------------------------------------------------------------------------------------
   - Skip this if you just want to use the default port: **1337**
   
   - If you want to use a different port:

     → Open **game_stateintegration.cfg**
     → Make sure the `"uri"` line uses the correct port your software listens on.
       Example:
       ```
       "uri" "http://localhost:1337/"
       ```
     → Set whatever you port you chose in Mullet Media-Strike 6.9 to the same number.
       You can change it in menu option [6] "Change Webhook Port"

----------------------------------------------------------------------------------------
IMPORTANT NOTES
----------------------------------------------------------------------------------------
 - You may need to run the software as administrator on some systems.

 - This setup is **VAC-safe**. It uses the official GSI webhook that Valve
   provides for tournament overlays.

 - That said, I take **zero responsibility** if you get banned (even though that 
   would make absolutely no sense).

 - If something isn’t working, double-check:
     → File paths
     → Port numbers, they NEED to match for it to work.
       Example: 
       The gamestate_integration_media.cfg file has "uri" set to "http://localhost:1337/"
       this means that Mullet Media-Strike 6.9 needs to listen for port 1337 too.
  
  - Every time you make changes to the .cfg file Counter-Strike requires a restart.
       
========================================================================================
               Click some heads and enjoy your jams. //80smullet
========================================================================================