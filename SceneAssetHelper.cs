using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shoelace.Utilities
{
	public static class SceneAssetHelper
	{
		public static string SceneDataFolder => "Assets/Scenes/SceneData/";

		public static string GetSceneFolder()
		{
			string sceneName = SceneManager.GetActiveScene().name;
			return Path.Combine(SceneDataFolder, sceneName);
		}

		public static T GetOrCreateAsset<T>(string name) where T : ScriptableObject
		{
			string folder = GetSceneFolder();
			if (!AssetDatabase.IsValidFolder(folder))
				AssetDatabase.CreateFolder(SceneDataFolder.TrimEnd('/'), SceneManager.GetActiveScene().name);

			string path = Path.Combine(folder, name + ".asset");
			T asset = AssetDatabase.LoadAssetAtPath<T>(path);
			if (asset == null)
			{
				asset = ScriptableObject.CreateInstance<T>();
				AssetDatabase.CreateAsset(asset, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Debug.Log($"Created {typeof(T).Name} for scene at {path}");
			}
			return asset;
		}
	}

}
