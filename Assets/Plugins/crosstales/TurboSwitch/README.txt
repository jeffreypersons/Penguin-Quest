# Turbo Switch PRO 2020.2.3

Thank you for buying our asset "Turbo Switch PRO" (TPS)! 
If you have questions about this asset, send us an email at [tps@crosstales.com](mailto:tps@crosstales.com). 
Please don't forget to rate it or even better write a little review – it's very much appreciated.



## Description
"Turbo Switch" is a Unity editor extension to reduce the time for assets to import during platform switches.
We measured speed improvements up to 100x faster than the built-in switch in Unity.

After importing TPS from the "Asset Store", open the Window menu and click TPS.

You should now switch platforms using TPS exclusively and not rely on the "Build Settings" platform list.

Pick a platform and hit the "Switch" button. That's it. The plugin will do the rest.
TPS will close Unity, save and restore the necessary files and then restart Unity. 
If the platform was selected for the first time, Unity has to import the assets which will take some time.

Finally, because it creates and operates the "TPS_cache" folder in your project directory, we've also included convenient 
methods to ignore this folder for popular version control mechanisms (Git, SVN and Mercurial).

Please read the "TurboSwitch-doc.pdf" and "TurboSwitch-api.pdf" for more details.



## Important
* Please be patient - TPS is working as fast as your machine can but if you have large projects, it will need some time!
  After you hit the "Switch"-button, Unity closes and TPS does all the work and restarts Unity. Wait until it's finished or 
  you risk a corrupt project.

* Because TPS caches data for each platform at switch time, it takes up valuable disk space which can become 
  quite large depending on your project size. If you run on low disk space, please delete some caches from unused platforms.

* TPS isn't meant to replace Unity's Cache Server. It's a personal caching utility for individuals and small teams. 
  If you're already using Unity's Cache Server you should not use TPS, because you would be caching your data twice and would probably lose time and disk space.

* Always backup your project. TPS was carefully designed and extensively tested. Nevertheless, it works on your filesystem and something 
  could go wrong. If your project is corrupted, close Unity and delete the "Library"- and "TPS_cache"-folders in your project.
  Unity will then re-import all assets for your current platform.
  


## Upgrade to new version
Follow these steps to upgrade your version of "TPS PRO":

1. Update "TPS PRO" to the latest version from the "Unity AssetStore"
2. Delete the "Assets/Plugins/crosstales/TurboSwitch" folder from the Project-view
3. Import the latest version from the "Unity AssetStore"



## Release notes

See "VERSIONS.txt" for details.



## Credits

The icons are based on [Font Awesome](http://fontawesome.io/).



## Contact

crosstales LLC
Schanzeneggstrasse 1
CH-8002 Zürich

* [Homepage](https://www.crosstales.com/en/portfolio/tps/)
* [Email](mailto:tps@crosstales.com)

### Social media
* [Discord](https://discord.gg/ZbZ2sh4)
* [Facebook](https://www.facebook.com/crosstales/)
* [Twitter](https://twitter.com/crosstales)
* [LinkedIN](https://www.linkedin.com/company/crosstales)



## More information
* [AssetStore](https://assetstore.unity.com/lists/crosstales-42213?aid=1011lNGT)
* [Forum](https://forum.unity3d.com/threads/turbo-platform-switch.434860/)
* [Documentation](https://www.crosstales.com/media/data/assets/tps/TurboSwitch-doc.pdf)
* [API](http://www.crosstales.com/en/assets/tps/api/)

### Videos
[Youtube-channel](https://www.youtube.com/c/Crosstales)

* [Promotion](https://youtu.be/rb1cqypznEg?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S)
* [Tutorial](https://youtu.be/J2zh0EjmrjQ?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S)


`Version: 24.05.2020`