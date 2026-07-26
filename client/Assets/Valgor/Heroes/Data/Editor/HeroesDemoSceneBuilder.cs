#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Heroes.Data;
using Valgor.Heroes.Factions;
using Valgor.Heroes.Preview360;
using Valgor.Heroes.UI;

namespace Valgor.Heroes.EditorTools
{
    public static class HeroesDemoSceneBuilder
    {
        public const string ScenePath = "Assets/Valgor/Heroes/Scenes/HeroesDemo.unity";
        private const string PanelSettingsPath = "Assets/Valgor/Heroes/UI/HeroesPanelSettings.asset";
        private const string CatalogPath = "Assets/Valgor/Heroes/Data/Generated/HeroCatalog.asset";
        private const string FactionPath = "Assets/Valgor/Heroes/Data/Generated/FactionConfig.asset";
        private const string UxmlPath = "Assets/Valgor/Heroes/UI/HeroesDemo.uxml";
        private const string UssPath = "Assets/Valgor/Heroes/UI/HeroesDemo.uss";

        [MenuItem("Valgor/Heroes/Open Heroes Demo Scene")]
        public static void OpenDemoScene()
        {
            BuildDemoScene();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"HeroesDemo aberta: {scene.path}");
        }

        [MenuItem("Valgor/Heroes/Rebuild Catalog And Demo Scene")]
        public static void RebuildCatalogAndDemo()
        {
            HeroesCatalogBuilder.RebuildFromSeed();
            BuildDemoScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Catálogo e cena HeroesDemo prontos.");
        }

        public static void BuildFromCommandLine()
        {
            HeroesCatalogBuilder.RebuildFromSeed();
            BuildDemoScene();
            Debug.Log($"[Valgor] HeroesDemo scene ready at {ScenePath}");
        }

        public static void BuildDemoScene()
        {
            Directory.CreateDirectory("Assets/Valgor/Heroes/Scenes");
            EnsurePanelSettings();
            HumanoidDummyPrefabBuilder.EnsureHeroPreviewLayer();
            var dummyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HumanoidDummyPrefabBuilder.PrefabPath);
            if (dummyPrefab == null)
            {
                dummyPrefab = HumanoidDummyPrefabBuilder.CreateOrUpdatePrefab();
            }

            var catalog = AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(CatalogPath);
            var factions = AssetDatabase.LoadAssetAtPath<FactionConfigSO>(FactionPath);
            if (catalog == null || factions == null)
            {
                HeroesCatalogBuilder.RebuildFromSeed();
                catalog = AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(CatalogPath);
                factions = AssetDatabase.LoadAssetAtPath<FactionConfigSO>(FactionPath);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var previewLayer = HumanoidDummyFactory.ResolveLayer();

            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0f, 1.2f, -4f);
                camera.transform.rotation = Quaternion.identity;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
                // Main camera must NOT render the isolated preview layer.
                camera.cullingMask = ~(1 << previewLayer);
            }

            var light = Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
                light.intensity = 0.35f;
                light.cullingMask = ~(1 << previewLayer);
            }

            var previewGo = new GameObject("HeroPreview");
            var preview = previewGo.AddComponent<HeroPreviewController>();
            preview.SetDummyPrefab(dummyPrefab);

            // Force rig creation in edit mode via ShowHero.
            preview.ShowHero("HERO_VORTEX_000", HeroFaction.GuardaDaOrdem);

            var uiGo = new GameObject("HeroesDemoUI");
            var uiDocument = uiGo.AddComponent<UIDocument>();
            uiDocument.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            uiDocument.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            var controller = uiGo.AddComponent<HeroesDemoController>();
            controller.BindPreview(preview);

            var so = new SerializedObject(controller);
            so.Update();
            so.FindProperty("catalog").objectReferenceValue = catalog;
            so.FindProperty("factionConfig").objectReferenceValue = factions;
            so.FindProperty("previewController").objectReferenceValue = preview;
            so.ApplyModifiedPropertiesWithoutUndo();

            var previewSo = new SerializedObject(preview);
            previewSo.Update();
            previewSo.FindProperty("dummyPrefab").objectReferenceValue = dummyPrefab;
            previewSo.ApplyModifiedPropertiesWithoutUndo();

            var uiSo = new SerializedObject(uiDocument);
            uiSo.Update();
            uiSo.FindProperty("m_PanelSettings").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            uiSo.FindProperty("sourceAsset").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            uiSo.ApplyModifiedPropertiesWithoutUndo();

            var styleHost = uiGo.AddComponent<HeroesDemoStyleHost>();
            var styleSo = new SerializedObject(styleHost);
            styleSo.Update();
            styleSo.FindProperty("styleSheet").objectReferenceValue = style;
            styleSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            PatchSceneAssetReferences();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Cena salva em {ScenePath} com preview 3D (layer={previewLayer}, prefab={dummyPrefab != null}).");
        }

        private static void PatchSceneAssetReferences()
        {
            if (!File.Exists(ScenePath)) return;

            var catalogGuid = AssetDatabase.AssetPathToGUID(CatalogPath);
            var factionGuid = AssetDatabase.AssetPathToGUID(FactionPath);
            if (string.IsNullOrEmpty(catalogGuid) || string.IsNullOrEmpty(factionGuid)) return;

            var yaml = File.ReadAllText(ScenePath);
            yaml = yaml.Replace(
                "catalog: {fileID: 0}",
                $"catalog: {{fileID: 11400000, guid: {catalogGuid}, type: 2}}");
            yaml = yaml.Replace(
                "factionConfig: {fileID: 0}",
                $"factionConfig: {{fileID: 11400000, guid: {factionGuid}, type: 2}}");
            File.WriteAllText(ScenePath, yaml);
            AssetDatabase.ImportAsset(ScenePath);
        }

        private static void EnsurePanelSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null) return;

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
