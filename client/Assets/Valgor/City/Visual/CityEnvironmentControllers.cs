using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Controlador visual da muralha (único edifício lógico). Sem regras de combate.
    /// </summary>
    public sealed class CityWallVisualController : MonoBehaviour
    {
        private Transform _cityRoot = null!;
        private int _appliedLevel = int.MinValue;

        public void Bind(Transform cityRoot)
        {
            _cityRoot = cityRoot;
        }

        public void ApplyLevel(int wallLevel)
        {
            if (_cityRoot == null)
            {
                return;
            }

            if (_appliedLevel == wallLevel && CityEnvironmentBuilder.FindFortifications(_cityRoot) != null)
            {
                return;
            }

            CityEnvironmentBuilder.ApplyWallLevel(_cityRoot, wallLevel);
            _appliedLevel = wallLevel;
        }

        public void SetHighlighted(bool selected) =>
            CityEnvironmentBuilder.SetFortificationsHighlighted(selected);
    }

    /// <summary>
    /// Ativa grupos de ambientação por tier do Castelo (1–6).
    /// </summary>
    public sealed class CityEnvironmentTierController : MonoBehaviour
    {
        private Transform[] _tierRoots = Array.Empty<Transform>();
        private int _activeTier = 1;

        public int ActiveTier => _activeTier;

        public void Bind(Transform environmentRoot)
        {
            if (environmentRoot == null)
            {
                return;
            }

            _tierRoots = new Transform[7];
            for (var t = 1; t <= 6; t++)
            {
                var child = environmentRoot.Find($"TierGroups/Tier{t}");
                _tierRoots[t] = child;
            }
        }

        public void ApplyCastleLevel(int castleLevel)
        {
            _activeTier = CityCapitalLayoutConfig.ResolveEnvironmentTier(castleLevel);
            for (var t = 1; t <= 6; t++)
            {
                var root = _tierRoots.Length > t ? _tierRoots[t] : null;
                if (root != null)
                {
                    root.gameObject.SetActive(t <= _activeTier);
                }
            }
        }
    }
}
