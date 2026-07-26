#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Valgor.Heroes.Preview360;

namespace Valgor.Heroes.EditorTools
{
    public static class HumanoidDummyPrefabBuilder
    {
        public const string PrefabPath = HumanoidDummyFactory.PrefabPath;
        private const string MaterialPath = "Assets/Valgor/Heroes/Prefabs/HumanoidDummy_Mat.mat";

        [MenuItem("Valgor/Heroes/Create Humanoid Dummy Prefab")]
        public static GameObject CreateOrUpdatePrefab()
        {
            Directory.CreateDirectory("Assets/Valgor/Heroes/Prefabs");
            EnsureHeroPreviewLayer();

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = HumanoidDummyFactory.CreateUrpCompatibleMaterial(HeroPreviewFactionColors.GuardaDaOrdem);
                if (material != null)
                {
                    AssetDatabase.CreateAsset(material, MaterialPath);
                }
            }
            else
            {
                HumanoidDummyFactory.ApplyColor(material, HeroPreviewFactionColors.GuardaDaOrdem);
                EditorUtility.SetDirty(material);
            }

            var root = HumanoidDummyFactory.Create(null, material);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"HumanoidDummy prefab salvo em {PrefabPath}");
            return prefab;
        }

        public static void EnsureHeroPreviewLayer()
        {
            const string layerName = HumanoidDummyFactory.LayerName;
            if (LayerMask.NameToLayer(layerName) >= 0) return;

            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            var tagManager = new SerializedObject(assets[0]);
            var layers = tagManager.FindProperty("layers");
            for (var i = 8; i < layers.arraySize; i++)
            {
                var slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue)) continue;
                slot.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"Layer '{layerName}' criada no índice {i}.");
                return;
            }

            Debug.LogWarning($"Não foi possível criar a layer '{layerName}' (slots cheios).");
        }
    }
}
#endif
