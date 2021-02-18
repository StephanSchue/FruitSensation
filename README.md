# MAG Interactive - Job Application Test
### Author: Stephan Schüritz
### Unity Version: 2019.4.10f1

**Please make sure to initialize Git LFS to download all assets of the project.**

## Description
This project represents a work example of a linker game in style of a classic match three game.
The time invested in the project was taken after my daily job, when there was time.
The invested time is within the constrained 1 week work duration.

## Repository-Features
- 3 Levels to play with different difficulties and tile variations
- Completly configuratable board rules, animation tweens and game settings using profiles
- Usage of Addressables for Scene and Prefab loading
- Easy extendable and reliable code structure
- Everything has smooth transitions and timing
- Fast booting
- playtested on Android

## Game-Features
- The board consists of at least 5 different coloured tiles and may be of various sizes (5x5, 7x7, 6x9 etc.)
- Adjacent tiles of the same colour can be linked
- Tiles can be linked horizontally, vertically or diagonally
- At least 3 tiles in a link are required to make a match
- Matched tiles are removed from the board
- The board collapses downward to fill empty slots and new tiles fall in from the top
- The board reshuffles the tiles if there are no valid links available to make

## Unity-Project
The app is intented to be played in portait-mode, so setup your game view with a portait-view like 9:16, 10:19, 3:2, 3:4 or simular. The camera adjust to the view in playmode.

The build starts via the 'Assets/Scenes/Preloader.unity' scene.
You can also start the specific level 'Assets/Scenes/Level01-03.unity' directly.

## Ready to Play Version
A ready to play Android Build can be found in the 'Build'-Folder. Please share and enjoy.

## Configuration
All profiles to configure the games and assets can be found in 'Assets/Config' folder.
The Tile visuals can be adjusted at 'Assets/Prefabs/TilePack/TilePack_01'.

## Plugins

| Plugin | Description |
| ------ | ------ |
| DoTween | Tweening Plugin |

## License
GNU GENERAL PUBLIC LICENSE

Programming by Stephan Schüritz
All Graphics used are licene Free and created by Nicole Tietze
