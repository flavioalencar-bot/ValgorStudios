using UnityEngine;

namespace Valgor.Heroes.Characters.Vortex
{
    public enum VortexPipelinePhase
    {
        WaitingForSourceModel = 0,
        SourcePresent = 1,
        PrefabBuilt = 2,
        Validated = 3,
        AddressableReady = 4
    }

    [CreateAssetMenu(menuName = "Valgor/Heroes/Vortex Pipeline Status", fileName = "Vortex_PipelineStatus")]
    public sealed class VortexPipelineStatusSO : ScriptableObject
    {
        public VortexPipelinePhase Phase = VortexPipelinePhase.WaitingForSourceModel;
        public bool UsingTechnicalFallback = true;
        public string LastValidationReport;
        public string PrefabPath = VortexAssetPaths.HeroPrefab;
        public string AddressableKey = VortexAssetPaths.AddressablePrefabKey;
        public string SourceModelPath;
    }
}
