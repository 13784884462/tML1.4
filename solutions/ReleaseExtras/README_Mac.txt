Automatic Install:
To install tModLoader, extract the zip achive containing this README.txt file to a temporary folder and then simply run the tModLoaderInstaller.jar file in that folder. Java 1.7 or higher is required for the installer to run. The installer will make a window pop up, informing you of either success or failure. If you have changed the steam library location or are on GOG, you will need to do the Manual Install instructions.

Manual Install:
If the installer for some reason does not work, or if you do not want to install/upgrade Java, you may also do a manual install:

Steam Manual Install:
1.  Go to the Terraria install folder (the one containing Terraria.exe, not the Saves folder). For most people, this will be "Library/Application Support/Steam/steamapps/common/Terraria/Terraria.app/Contents/MacOS". To enter your hidden Library folder, in the Finder open the "Go" menu, then hold down the "Alt" key. The hidden Library will become visible. To enter the Terraria.app folder, right-click it then click on "Show Package Contents". If you can't find it, view this video: https://gfycat.com/SelfreliantAssuredIsabellineshrike

2.  Back-up your vanilla Terraria.exe if you haven't already. To do this, rename the "Terraria.exe" file in the install folder to "Terraria_v1.3.5.3.exe".

3.  Copy all of the files from the temporary folder (or zip archive) into the install folder using the special merge copy and paste (This is necessary since Mac will delete folders rather than merge folders when copy and pasting). To do this, hold down the Option key as you drag-and-drop all the files and folders from this folder and drop them into the folder mentioned above. If you do this, you’ll see a dialog with a Merge button and you can click it to merge folders like you would on Windows or Linux. You'll need to make sure to replace files if asked: https://gfycat.com/EqualFaithfulAfricanharrierhawk

3.B  If you messed up the merge copy and paste, you'll need to first "verify game integrity" to get the files back (see below) and then try step 3 again.

4.  If you are updating from tModLoader v0.10.1.5 or earlier, you'll need to delete "MP3Sharp.dll", "Ionic.Zip.Reduced.dll", and "Mono.Cecil.dll" from the install folder.

5. You need to give execute permissions to 2 files. Open a terminal in the install directory and run the following 2 commands:
    chmod +x tModLoaderServer
    chmod +x Terraria

GOG Install:
For GOG installs, follow the Steam Manual Install instructions above but with the location of your GOG install of Terraria. Don't skip the backing up vanilla "Terraria.exe" step or it won't work.

Run tModLoader:
To run tModLoader, run Terraria as normal from Steam. Do not attempt to run it from the temporary folder.

Why are there EXE files?:
Don't worry about it, you didn't download the wrong thing.

Uninstall/Switch back to un-modded Terraria:
If you want to play un-modded Terraria or join an un-modded Terraria server, you can't use tModLoader. You need to use steam to "verify game integrity" which will restore the files back to the un-modded Terraria. (You'll need to reinstall tModLoader later.) See this video to see how to do that: https://gfycat.com/RapidBelovedFrog

Dedicated tModLoader server:
If you are installing tModLoader onto a machine that will be running a dedicated server, you'll need to install the un-modded Terraria server first: http://terraria.org/server/terraria-server-1353.zip  After installing, you can follow the manual install instructions to copy the tModLoader files into the folder you installed the server into.