# Penguin Quest
A penguin on a quest to eat a bunch of fish and slide, jump, and walk his way to victory!

Can play in-browser at itch.io!: https://jeffreypersons.itch.io/penguin-quest (EDIT: As of 2022 there is something causing the web player to hang, so avoid for now!)

_Status: still in development_

Ongoing development of a platformer using C# and Unity with a focus on skeletal animation featuring a jumping and sliding penguin in a spline-based world complete with slope alignment, camera following, HUD, and root motion.



## Project Setup
### Prerequisites
* Unity Hub installed
* Git LFS installed
* *Recommended* IDE with first-class Unity integration (Rider, Visual Studio) installed

## Steps
1) Download project
  a) Clone repo
  b) Enable Git LFS for repo (otherwise art may fail to show in editor later on)
2) Open in Unity
  a) Install project's current Unity Version (make sure PC/WebGL are included)
  b) Once installation is finished, open as existing project in Unity Hub
3) Open IDE
  a) Select desired IDE in editor preferences
  b) Right click assets folder and select 'Open as C# Project'


## Developer Intro

**UNDER CONSTRUCTION**

### General Project Structure

* `Assets` root folders are breaken down by disciplinedo not reference each other
* `Prefabs` root folder is where above assets are all hooked together
* `Scenes` is where prefabs are hooked together to form levels

* `_Experimental` is where features can be developed and prototyped outside the context of the main game
  * `SandBox_<CategoryName>` is the root folder with shared assets and scene templates for the specific type of content
    * `Feature_NNN__<ScenarioName>` is the specific folder containing the minimal code needed to showcase a specific piece of functionality
* `Tests` is where
* `UnitySettings` is for Unity-specific configs such as settings for input and rendering

todo: consider moving the more detailed breakdowns into the readmes found in each asset subfolder and linking to them here


### Code
* todo: add code architecture breakdown of common/game/etc


### Art Guide

#### General
* Inkscape used for sprites
* Photopea used for exporting sprites to PSD
* PSD files and Unity animation used for rigs
* Sprite spline shapes used for terrain
* Cinemachine used for cameras

#### Character rig pipeline
* todo: add breakdown of tools used (Photopea for PSD, Unity animation skinning, etc)

#### Creating Terrain
* Inkscape used for sprites
* Photopea used for exporting sprites to PSD
* PSD files and Unity animation used for rigs
* Sprite spline shapes used for terrain
* Cinemachine used for cameras



### Design Guide
* todo: breakdown of tools and workflow for creating levels, etc



## Principles
todo: consolidate and move things around for clarity, principles should be a small section, maybe we need a new 'rationale' or similar section

* Standardized world scales at **1000 pixels denotes 1 meter**
  * Unity Physics is tuned to this scale
  * 1000 PPU gives a crisper, non-pixelated, HD look in alignment with the art direction
  * Avoids slight visual glitches that were previously noticable when less pixels were on screen
  * Avoids having to modify scale, which is significant performance hit for character rigs
    * Using anything but 1,1,1 for transform.scale proprogates the computation to every object in the heirarchy, every frame
* Minimize dependencies
  * No ThirdParty plugins needed, Unity's first-party modules were sufficient to replace external dependencies previously used during initial prototype
* Custom movement scripting
  * The platformer mechanics in this game requires very tight movement control
* Fast Iteration Time
  * Keep build sizes small
      * `_` prefix is used to denote editor specific folders to be excluded from build (eg experimental)
   * Fast scene loads in editor
      * **Disable domain reloading** making scenes play near-instantly (at expense of static loads fail on first re-run)
   * Sandbox-driven feature development
      * Develop gameplay features in isolated test scenes
      * Creating new test scenes is easy thanks to conventions (see later) and usage existing scene templates


## Development History
### Early development shots (will add more professional looking ones and descriptions soon)
![standing-up](https://user-images.githubusercontent.com/8084757/90597527-2df3e200-e1a6-11ea-8724-219b50025dac.png)

![standing-up-on-slope](https://user-images.githubusercontent.com/8084757/90597529-2e8c7880-e1a6-11ea-8ff8-75531ca798fa.png)

![sliding-down-on-slope](https://user-images.githubusercontent.com/8084757/90597522-2cc2b500-e1a6-11ea-8882-8a5c46956e28.png)

The principles were born from painful months of rework:
* Lack of standardized scale led to unplayable state with glitchy physics, meshes, animation
* Lack of sandbox meant new feature development and debugging was overly time-intensive
* Useage of dynamic rigidbody physics was too difficult to tune, and required a lot of working around limitations as opposed to defining how movement should work
* Polishing animation too early meant that they had to be redone often
