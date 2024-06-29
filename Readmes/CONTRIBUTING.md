## Sure

### What 

`./Assets/` holds the asset bundle and assets used to build this mod. Keep it light.  
It holds exclusively image assets for the scene previews and a prefab version of said scene preview

`./Code/` you will never guess what is in this folder

`./Deps/` holds dependencies for this mod. This is primarily done since I work on this mod on a mac half the time and 
have to sync the dlls between machines. This is where I put them. Do NOT upload DLLs to git.

Basic guidelines

### Structurally, emulate the game's own codebase. 

This makes it easy for someone familiar with the game's codebase to look at your code's structure and understand it quickly.

#### Managers

Managers are singletons, stored under the `MANAGER_MASTER` GameObject in the DontDestroyOnLoad scene.

Manager MonoBehaviors in this game are named with the convention `Mgr_MyManagerName`, with the GameObjects named `MyManagerName`.

It's best practice to stick with this convention. 

Injecting your own is done as so:

```csharp
// Setup stuff for our mod 
var managerMaster = GameObject.Find("MANAGER_MASTER");
var myModMaster = new GameObject("MOD_MyModName");
myModMaster.transform.SetParent(managerMaster.transform);

...

// Add a manager like so
var gameAssetLoader = new GameObject("CS_GameAssetLoader"); // see below for naming conventions
DontDestroyOnLoad(gameAssetLoader);
gameAssetLoader.transform.SetParent(myModMaster.transform);
gameAssetLoader.AddComponent<Mgr_CS_GameAssetLoader>().enabled = true;
```

Here, we do the following to help with organization:
* Store a mod specific gameobject under the `MANAGER_MASTER` named `MOD_ModName`
* Store all of our custom managers under this gameobject
* Name the primary manager for this mod `Mgr_ModName`
* Name the rest with the convention `Mgr_MD_MyManagerName` where `MD` is a 2-3 letter abbreviation for the mod. Here we use `CS`


