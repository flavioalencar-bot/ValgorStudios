using System;
using System.Collections.Generic;
using UnityEngine;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Visual
{
    /// <summary>
    /// Entrada data-driven do visual por estágio (prefab substituível + placeholder).
    /// </summary>
    [Serializable]
    public sealed class DragonStageVisualConfig
    {
        public DragonVisualStage Stage;
        public string DisplayNamePt = string.Empty;

        /// <summary>Caminho Resources (ex.: Valgor/Dragons/Visuals/Hatchling). Vazio = placeholder.</summary>
        public string PrefabResourcePath = string.Empty;

        public Vector3 LocalPosition = Vector3.zero;
        public Vector3 LocalRotationEuler = Vector3.zero;
        public Vector3 LocalScale = Vector3.one;
        public Vector3 PreviewCameraOffset = new(0f, 1.2f, -2.4f);
        public string LightPreset = "ember-soft";
        public string AnimatorControllerResourcePath = string.Empty;
        public string TransitionVfxResourcePath = string.Empty;
        public bool PlaceholderFlag = true;

        /// <summary>Tint do placeholder procedural (diferenciação visível entre estágios).</summary>
        public Color PlaceholderTint = Color.white;

        public Quaternion LocalRotation => Quaternion.Euler(LocalRotationEuler);
    }
}
