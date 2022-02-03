# EvoEdit
The Starmade - Starship Evo Voxel Converter


This program is the first iteration of a full Starship Evo blueprint editor and permits the conversion of .sment files from Starmade to .sevo files For Starship Evo.

Currently Supporting:
- Porting both SMD2 and SMD3 Based Starmade ship, station, and addon blueprints. This includes blueprints from the present to around 2014. 
- Dynamically optimizes blocks to reduce the resultant brick count
- Currently ports paint, hull, corners, wedges, and hepta/tetras.
- Permits a flat scale multiplier based on the Starship Evo scales (1-32)

Upcoming updates will bring further porting control, user color sets as well as .sevo/.OBJ support. 

How to use (recomended):
1. Run the Exe
2. [File - > Set Path] in order to establish the output directory.
4. [File - > Import - > blueprint] in order to load in a blueprint.
5. Choose an output scale (default 1:1) and press Import[x] Blueprints.
6. When done, check your output folder and move the new .sevo(s) file to your Starship Evo Blueprints folder (found in \AppData\LocalLow\Moonfire Entertainment\Starship EVO\Save_Data\Blueprints).
7. Spawn the blueprint as you would a regular blueprint, then save it to ensure it has the correct modeldata. You can then discard the .sevo.


To Import attached entities:
While the EVOEditor can handle top level sub entities, it can only output them to seperate blueprints. It's recomended that use the Import->Folder option on an ATTACHED, using it on each subsequent child folder until you have what you're after.
 