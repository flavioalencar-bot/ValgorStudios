using System;

namespace Valgor.WorldMap.Filters
{
    /// <summary>
    /// Seleção de filtros do mapa. Afeta apenas visibilidade.
    /// </summary>
    public sealed class WorldMapFilterState
    {
        public bool ShowCities { get; set; } = true;
        public bool ShowVillages { get; set; } = true;
        public bool ShowResources { get; set; } = true;
        public bool ShowCreatures { get; set; } = true;
        public bool ShowDragons { get; set; } = true;
        public bool ShowLandmarks { get; set; } = true;
        public bool ShowOccupied { get; set; } = true;
        public bool ShowAvailable { get; set; } = true;

        public static WorldMapFilterState CreateDefault() => new();

        public void ClearToDefault()
        {
            ShowCities = true;
            ShowVillages = true;
            ShowResources = true;
            ShowCreatures = true;
            ShowDragons = true;
            ShowLandmarks = true;
            ShowOccupied = true;
            ShowAvailable = true;
        }

        public WorldMapFilterState Clone() =>
            new()
            {
                ShowCities = ShowCities,
                ShowVillages = ShowVillages,
                ShowResources = ShowResources,
                ShowCreatures = ShowCreatures,
                ShowDragons = ShowDragons,
                ShowLandmarks = ShowLandmarks,
                ShowOccupied = ShowOccupied,
                ShowAvailable = ShowAvailable
            };

        public void CopyFrom(WorldMapFilterState source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ShowCities = source.ShowCities;
            ShowVillages = source.ShowVillages;
            ShowResources = source.ShowResources;
            ShowCreatures = source.ShowCreatures;
            ShowDragons = source.ShowDragons;
            ShowLandmarks = source.ShowLandmarks;
            ShowOccupied = source.ShowOccupied;
            ShowAvailable = source.ShowAvailable;
        }
    }
}
