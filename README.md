# EvoEdit
The Starmade - Starship Evo Voxel Converter

This program is the first Starship Evo blueprint editor and permits the conversion of .sment files from Starmade and obj/stl 3d files to .sevo for use in Starship Evo.

Currently Supports:
- Porting both SMD2 and SMD3 Based Starmade ship, station, and addon blueprints. This includes blueprints from the present to around 2014. 
- Dynamically optimizes blocks to reduce the resultant brick count
- Currently ports paint, hull, corners, wedges, slabs and hepta/tetras.
- Permits a flat scale multiplier based on the Starship Evo scales (1-32)
- Importing of 3d models (obj stl) over a variable resolution. 
- Repainting an existing sevo blueprint using the color configurator. 

Upcoming updates will bring further porting control, UI improvements and .sevo manipulation. 

How to use (recomended):
1. Run the Exe
2. [File - > Set Path] in order to establish the output directory if not done when prompted
4. FOR STARMADE: [Import - > Starmade -> .sment/folder] in order to load in a blueprint, select your chosen files and press Read
4. FOR 3D: [Import - > model] in order to load in an object, select your chosen resolution and press Read Model
5. Choose an output scale (default 1:1) and press Export[x] Blueprints.
6. When done, check your output folder and move the new .sevo(s) file to your Starship Evo Blueprints folder (found in \AppData\LocalLow\Moonfire Entertainment\Starship EVO\Save_Data\Blueprints).
7. Spawn the blueprint as you would a regular blueprint, then save it to ensure it has the correct modeldata. You can then discard the .sevo.

 