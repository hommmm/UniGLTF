﻿using System.IO;
using UnityEditor;
using UnityEngine;


namespace UniGLTF
{
    public static class ImporterMenu
    {
        [MenuItem(UniGLTFVersion.UNIGLTF_VERSION + "/Import", priority = 1)]
        public static void ImportMenu()
        {
            var path = UnityEditor.EditorUtility.OpenFilePanel("open gltf", "", "gltf,glb,zip");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Application.isPlaying)
            {
                // load into scene
                var context = new ImporterContext();
                context.Load(path);
                context.ShowMeshes();
                Selection.activeGameObject = context.Root;
            }
            else
            {
                if (path.StartsWithUnityAssetPath())
                {
                    Debug.LogWarningFormat("disallow import from folder under the Assets");
                    return;
                }

                var assetPath = UnityEditor.EditorUtility.SaveFilePanel("save prefab", "Assets", Path.GetFileNameWithoutExtension(path), "prefab");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                if (!assetPath.StartsWithUnityAssetPath())
                {
                    Debug.LogWarningFormat("out of asset path: {0}", assetPath);
                    return;
                }

                // import as asset
                Import(path, UnityPath.FromUnityPath(assetPath));
            }
        }

        static void Import(string readPath, UnityPath prefabPath)
        {
            var context = new ImporterContext();
            context.Parse(readPath, File.ReadAllBytes(readPath));
            context.SaveTexturesAsPng(prefabPath);

            EditorApplication.delayCall += () =>
            {
                // delay and can import png texture
                context.Load();
                context.SaveAsAsset(prefabPath);
                context.Destroy(false);
            };

        }
    }
}
