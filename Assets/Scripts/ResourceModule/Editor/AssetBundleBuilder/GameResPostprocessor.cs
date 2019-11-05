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
            var modelImporter = assetImporter as ModelImporter;
            if (modelImporter != null)
            {
                if (assetPath.Contains("Assets/GameRes/Actors") || assetPath.Contains("Assets/GameRes/FX/Mesh"))
                {
                    modelImporter.importMaterials = true;
                    var defaultMat = AssetDatabase.LoadMainAssetAtPath("Assets/GameRes/Actors/ActorRef/mat_actor_ref.mat");
                    var remap = modelImporter.GetExternalObjectMap();
                    foreach (var pair in remap)
                    {
                        if (pair.Value != defaultMat)
                            modelImporter.RemoveRemap(pair.Key);
                    }

                    var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(modelImporter.assetPath);
                    foreach (var subAsset in objs)
                    {
                        var mat = subAsset as Material;
                        if (mat != null)
                        {
                            var sourceId = new AssetImporter.SourceAssetIdentifier(mat);
                            modelImporter.AddRemap(sourceId, defaultMat);
                        }
                    }
                }
            }
        }

        void OnPreprocessTexture()
        {
            if (assetPath.Contains("Assets/BundleResources/RawImages"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.mipmapEnabled = false;
                textureImporter.textureCompression = TextureImporterCompression.Compressed;
                Debug.Log("Update RawImages Textures:" + assetPath);
            }
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