# LRLRelativity: You can change how fast TIME passes with this. (Windows)

PLEASE READ THE WHOLE DOC BEFORE USING. DONT USE IF UNSURE.


I have created this LRLRelativity mod/patcher to change how fast time moves in the game.

How? There is a value called 'SecondsInHour'. This value controls how many real world seconds there are in each game hour. The default (original) value is 55.0. So in each hour of the game, 55 real world seconds pass. This value is present in the file 'GameAssembly.dll' of the game. This patcher changes that value to your provided value. Use at your own risk. Source code provided (at the end). Recommended values: 75, 110. Only for game version 1.0.4. Find known issues towards the end.


IMPORTANT
IMPORTANT

1. This patcher modifies the GameAssembly.dll game file (a backup is also created). Till a more robust mod (which doesnt change the game files) is created, this can work well. If you are not comfortable with that, skip this.
2. DO NOT use this patch for your first experience of the game and only use it if you really dislike the time passage. Because this will change the player experience. The game design (and content) is probably optimized for 55.0 real world seconds in one game hour.
3. This patch only works with the 1.0.4 version. Basically I am checking if the dll file is same or not. If the file changes in the future(or you use it on something else), this patch will deliberately not execute. I may update it for newer versions whenever they release...but cannot promise that.
4. If your game is installed in a restricted folder (which requires run as administrator), I request you move the game install to another steam folder...otherwise it may fail to create the backup dll file. I don't recommend running the patcher as administrator just to be safe.
5. If the devs tell me to remove this patch (for whatever reason), I will remove it. DONT ask me for the removed patch in those cases.

Thanks

IMPORTANT END
IMPORTANT END


INSTRUCTIONS TO INSTALL

1. When you download the zip file (LRLRelativity.zip) from the google drive link provided (never use untrusted links), it will have 3 files. Put these 3 LRL files in the game's install folder (where LittleRocketLab.exe executable is).

2. Make sure the game is not running: Then run the LRLRelativity.exe file (the executable of the patch...not the game). A command prompt window will open. First it will check hash of the dll. Then it will ask you for a new value (I recommend 75 or 110 for example). Then it will do the patching.

3. It will also create the ""GameAssembly.dll.DONT_DELETE.bak"" file which is the original file backup just with a different name.

4. While patching, if anything goes wrong, it will tell you what went wrong. (It will also tell you if you need to reinstall the game or if no changes were made).

5. If the patch runs somewhat, but then no error is displayed, and the patch just stops, better to reinstall.

6. If the patch's exe doesnt even open then something has gone wrong. Usually, in those cases, no changes have been made, so the game should still work normally.

7. After patching you can actually remove the 3 LRLRelativity files but dont remove the backup file as that is the original dll file (backup filename: GameAssembly.dll.DONT_DELETE.bak).

8. ONLY WHEN YOU HAVE USED THE PATCH at least once (and the backup file is present): Suppose you want to restore the game to normal, either you can just delete the game folder (uninstall) and reinstall the game from steam; OR OR you can delete the 'GameAssembly.dll' (which is the patched version) and remove the '.DONT_DELETE.bak' part of the ""GameAssembly.dll.DONT_DELETE.bak"" file's filename (this file is the original dll)...basically you need to remove the patched dll and restore the original dll's name; that will restore the game to normal.

9. Suppose after patching to a value (say 75), you feel that it is not right still, and you want to re-use the patch to enter a new value (say 110)...in that case first restore the game to normal (as described in in step 8) and then re-run the patcher (inputting the new value).

INSTRUCTIONS TO INSTALL END


WARNING
THIS PATCH IS PROVIDED AS-IS. I cannot guarantee that it will work perfectly. Use it at your own risk.
WARNING END


When you download it, if windows asks for a security scan and there are hits, don't run it if you are unsure.


EXTRA DETAILS (TECHNICAL)
For those who want details: It seems the game has a DayController : MonoBehaviour which has a static float variable "SecondsInHour"...that variable gets IL2CPP compiled to the data section of the GameAssembly.dll of the game. We are changing that value with this patcher.

Why didn't I use a normal mod loader like either MelonLoader or BepinEx? First, because the stable latest versions of both of them didn't work for the game.
Then I tried the cutting edge (built from source) version of both of them.

MelonLoader cutting edge version worked! So in the future, if modders want to choose...there is only one choice (for now): MelonLoader. I still didn't use it for this patch because I was encountering some minor issues with it....once the cutting edge version of MelonLoader is stable it should be fine...I think.

Only if you know what you are doing (|CAUTION|): Lastly, you can actually, manually perform the patch using a hex editor (and without using my provided LRLRelativity patcher)....A bit of a pain though..Basically you change the value 'SecondsInHour' which is at 0x02AD8280 file offset in the dll. For finding it, you can search for this hex sequence: ""0x00 0x00 0xB8 0x41 0x00 0x00 0x5C 0x42""....as you can see the last 4 bytes is a float (IEEE 754) of value 55.0...in little endian, I think it was. When you find it, you can edit those 4 bytes and set it to any other value. BUT the location (0x02AD8280) may only be valid for game dll version 1.0.4; future versions may change that location because the dll will change. (Note that if while searching, you find multiple places with the same hex sequences...better not change anything).

EXTRA DETAILS END


KNOWN ISSUES USING THIS PATCH

1. The *evening* lighting breaks for many values when changing SecondsInHour (The float and color anim curves which drive the bg lighting dont work correctly for different values above 55.0...it seems).
2. The character wakes up at 6AM when slept correctly, but when the character passes out from exhaustion, instead of waking at 10AM (like in original), they wake up at different times (for eg. if SecondsInHour is 110, the character wakes up late at 2PM instead of 10 AM. Another example, if SecondsInHour is set to 75, the character wakes up at 11:25 AM instead of 10 AM). It is basically a formula instead of a fixed value, that's why.

KNOWN ISSUES END
