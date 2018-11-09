using UnityEditor;
using UnityEngine;

namespace ResourceModule
{
	/// <summary>
	/// 项目资源导入后处理,自动设置游戏中大部分资源BundleName,对于其依赖资源不做处理
	/// 这样做的好处就是不用每次新增资源时都要手动更新一下所有的BundleName,才能在Editor模式下进行加载
	/// 等到要真正打包的时候在做一次全面的检查操作
	/// </summary>
	public class GameResPostprocessor : AssetPostprocessor
	{
		void OnPreprocessModel()
		{
			var modelImporter = (ModelImporter)assetImporter;
			modelImporter.importMaterials = false;
		}

		void OnPreprocessTexture()
		{
			//if (assetPath.Contains("Assets/ArtResource/Animations"))
			//{
			//	TextureImporter textureImporter = (TextureImporter)assetImporter;
			//	textureImporter.textureType = TextureImporterType.Default;
			//	textureImporter.mipmapEnabled = false;
			//	textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			//	textureImporter.spritePixelsPerUnit = 100;
			//	textureImporter.isReadable = true;
			//	Debug.Log("Update Animations Textures:" + assetPath);
			//}
			//else if (assetPath.Contains("Assets/MyTemp"))
			//{
			//	TextureImporter textureImporter = (TextureImporter)assetImporter;
			//	textureImporter.textureType = TextureImporterType.Default;
			//	textureImporter.mipmapEnabled = false;
			//	textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			//	textureImporter.spritePixelsPerUnit = 100;
			//	textureImporter.isReadable = true;
			//	Debug.Log("Update MyTemp Textures:" + assetPath);
			//}
		}
	}
}