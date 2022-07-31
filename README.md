
# ![EvoIcon](https://user-images.githubusercontent.com/14081996/182005448-5beb0373-a1d0-4b8e-82f1-a371edcfdc59.png) EvoEdit 


## _The Starship Evo Editor_

This program permits the importing and editing of ship blueprints for use in [Starship Evo](https://store.steampowered.com/app/711980/Starship_EVO/).

## Features
### Starship Evo 
- Dynamic resizer; allows you to apply a scalar to your blueprint to make the entire object larger or smaller by a factor, includes mechanisms, permitting 1/8th scale rotors etc.
    - Ship stats will increase/decrease with scale maintaining the balance. 
    - Permits ships far larger than normally feasible and certain bricks in sizes smaller than normally permitted.
    - **(note)** Tsuna has altered the blueprints a number of times. If child enties appear out of place after resizing. Copy the original blueprint onto a new entity, save and rescale that one instead. 
- Color Configurator; Swap out your colors and materials easily with the simple to use color configurator. Allowing you to try new paint schemes in single click. 
    - Load your own Paints.xml file to use your custom paint-sheets. 
    - Use the advanced mode to add your own custom colors to your ships.
    - Swap out the underlying material of a block, or remove blocks entirely from the ship with the prune option. 
    - Decals included.
- Reposition Tool; Is something not in the right place on a build, would something look brilliant but requires clipping? Fix it with the repositional tool!
    - Mark the blocks you wish to move in game with a particular paint (with no underlying base material) , then, with the blueprint loaded, select that color in the Reposition tool. You'll then be able to move all matching/nonmatching blocks by a relative vector. 
-  Weapon designer; 4 block long lasers cramping your style? Go all the way with the weapon designer. 
    - Supports lasers with barrel mods and beams up to 16 units long.

### Starmade
- Bring your favourite creations from Starmade into the new decade!
- Porting both SMD2 and SMD3 Based Starmade ship, station, and addon blueprints. This includes blueprints from the present to around 2014.
- Dynamically optimizes blocks to reduce the resultant brick count
- Currently ports paint, hull, corners, wedges, slabs and hepta/tetras.
- Mirror tools are provided to ensure the blueprint is workable in Starship Evo

### 3D
- Supports obj and stl files. 
- Dynamically optimizes blocks to reduce the resultant brick count.
- Mirror tools are provided to ensure the blueprint is workable in Starship Evo

## Tech
EvoEdit uses two opensource tools to function properly, they come pre-packaged in the release:
- [stl2obj](https://github.com/baserinia/stl2obj) - Corrects structural issues with stls before voxelizing.
- [obj2voxel](https://github.com/Eisenwave/obj2voxel) - Voxelizer.

EvoEdit is open source with a [public repository](https://github.com/pinesh/EvoEdit) on GitHub.

## Installation

EvoEdit requires Windows to run.

Just grab the latest [release](https://github.com/pinesh/EvoEdit/releases/), unzip and run *EvoEdit.exe*

## Development
This application was built in C# with WPF and will require Visual Studio to build.

Want to contribute? Great!

You can get reach me in the offical [Starship Evo Discord](https://discord.gg/starshipevo) @Nuc1earLemons


## License

MIT


