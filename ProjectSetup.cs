using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static System.IO.Path;
using System.Threading.Tasks;
using static UnityEditor.AssetDatabase;

public static class ProjectSetup
{
    [MenuItem("Tools/Setup/Import Essential Assets")]
    static void ImportEssentialAssets()
    {
        Assets.ImportAsset("Odin Inspector and Serializer.unitypackage", "Sirenix/Editor ExtensionSystem");
    }

    [MenuItem("Tools/Setup/Import Packages")]
    static void InstallPackages()
    {
        string[] packages =
        {
            "git+https://github.com/adammyhre/Unity-Utils.git",
            "git+https://github.com/adammyhre/Unity-Improved-Timers.git",
            "https://github.com/Ryan-Pierce-Shoelace/Shoelace-SOAP-Variables.git#main",
            "https://github.com/Ryan-Pierce-Shoelace/Shoelace-SOAP-Events.git#main",
            "https://github.com/Ryan-Pierce-Shoelace/Shoelace-Audio-System.git#main",
            "https://github.com/Ryan-Pierce-Shoelace/Shoelace-Grid-System.git#main",
            "https://github.com/Ryan-Pierce-Shoelace/Shoelace-Region-System.git#main"
        };

        Packages.InstallPackages(packages);
    }

    [MenuItem("Tools/Setup/Create Folder Structure")]
    static void CreateFolderStructure()
    {
        Folders.Create("_Project", 
            "_Scripts", 
            "Prefabs", "Prefabs/UI", "Prefabs/Gameplay",
            "Art", 
            "Art/Sprites", "Art/Sprites/Tilemaps","Art/Sprites/Objects","Art/Sprites/Characters",
            "Art/Sprites/Characters/Example", "Art/Sprites/Characters/Example/Spritesheet","Art/Sprites/Characters/Example/Animations","Art/Sprites/Characters/Example/Animator",
            "Fonts",
            "Models",
            "Models/Objects", "Models/Objects/Clutter", "Models/Objects/Item", "Models/Objects/Structure",
            "Models/Objects/Item/Example", "Models/Objects/Item/Example/Textures", "Models/Objects/Item/Example/Shader", "Models/Objects/Item/Example/Material",
            "Models/Character", "Models/Character/Example",
            "Models/Character/Example/Model", 
            "Models/Character/Example/Animation", "Models/Character/Example/Animation/Idle", "Models/Character/Example/Animation/Run", "Models/Character/Example/Animation/Attack",
            "Models/Character/Example/Materials", "Models/Character/Example/Materials/Textures", "Models/Character/Example/Materials/Shader", "Models/Character/Example/Materials/MaterialAssets",
            "UI",
            "UI/Images",
            "UI/Images/Panels",
            "UI/Images/Icons"
            );
        Refresh();
        Folders.Move("_Project", "Scenes");
        Folders.Move("_Project", "Settings");
        Folders.Create("_Project", 
            "Scenes/Development",
            "Scenes/Levels",
            "Scenes/SubScenes",
            "Scenes/Manager");
        
        Folders.Delete("TutorialInfo");
        Refresh();
        const string pathToInputActions = "Assets/InputSystem_Actions.inputactions";
        string destination = "Assets/_Project/Settings/InputSystem_Actions.inputactions";
        MoveAsset(pathToInputActions, destination);
        const string pathToReadme = "Assets/Readme.asset";
        DeleteAsset(pathToReadme);
        Refresh();
    }
    
    static class Assets
    {
        public static void ImportAsset(string asset, string folder)
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string assetsFolder = Combine(basePath, "Unity/Asset Store-5.x");

            ImportPackage(Combine(assetsFolder, folder, asset), false);
        }
    }
    static class Packages
    {
        static AddRequest request;
        private static Queue<string> packagesToInstall = new();

        static async void StartNextPackageInstallation()
        {
            request = Client.Add(packagesToInstall.Dequeue());

            while (!request.IsCompleted)
            {
                Debug.Log("Downloading : " + packagesToInstall.Count +" packages remaining");
                await Task.Delay(500);
            }

            if (request.Status == StatusCode.Success) Debug.Log("Installed : " + request.Result.packageId);
            else if (request.Status == StatusCode.Failure)
                Debug.LogError("Failed to install : " + request.Result.packageId + " ||| " + request.Error.message);

            if (packagesToInstall.Count != 0)
            {
                await Task.Delay(1000);
                StartNextPackageInstallation();
            }
        }

        public static void InstallPackages(string[] packages)
        {
            foreach (string package in packages)
            {
                packagesToInstall.Enqueue(package);
            }

            if (packagesToInstall.Count > 0)
            {
                StartNextPackageInstallation();
            }
        }

        
    }
    static class Folders
    {
        public static void Delete(string folderName)
        {
            string pathToDelete = $"Assets/{folderName}";

            if (IsValidFolder(pathToDelete))
            {
                DeleteAsset(pathToDelete);
            }
        }
            
        public static void Move(string newParent, string folderName)
        {
            string sourcePath = $"Assets/{folderName}";
            if (IsValidFolder(sourcePath))
            {
                string destinationPath = $"Assets/{newParent}/{folderName}";
                string error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                }
            }
        }

        static void CreateSubFolders(string rootPath, string folderHierarchy)
        {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;

            foreach (var folder in folders)
            {
                currentPath = Combine(currentPath, folder);

                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }

        public static void Create(string root, params string[] folders)
        {
            var fullpath = Path.Combine(Application.dataPath, root);
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }

            foreach (var folder in folders)
            {
                CreateSubFolders(fullpath, folder);
            }
        }
    }
}