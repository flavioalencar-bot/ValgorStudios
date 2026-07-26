using System;

namespace Valgor.WorldMap.Filters
{
    public sealed class WorldMapFilterService
    {
        private readonly WorldMapFilterState _state = WorldMapFilterState.CreateDefault();
        private readonly IWorldMapFilterPersistenceRepository _repository;

        public WorldMapFilterService(IWorldMapFilterPersistenceRepository? repository = null)
        {
            _repository = repository ?? new WorldMapFilterPersistenceRepository("valgor.worldmap.filters.v1");
        }

        public WorldMapFilterState State => _state;
        public event Action? Changed;

        public void LoadOrInitialize()
        {
            var snapshot = _repository.Load();
            if (snapshot == null)
            {
                _state.ClearToDefault();
                Persist();
                return;
            }

            ApplySnapshot(snapshot);
        }

        public void SetShowCities(bool value) => Set(refState => refState.ShowCities = value);
        public void SetShowVillages(bool value) => Set(refState => refState.ShowVillages = value);
        public void SetShowResources(bool value) => Set(refState => refState.ShowResources = value);
        public void SetShowCreatures(bool value) => Set(refState => refState.ShowCreatures = value);
        public void SetShowDragons(bool value) => Set(refState => refState.ShowDragons = value);
        public void SetShowLandmarks(bool value) => Set(refState => refState.ShowLandmarks = value);
        public void SetShowOccupied(bool value) => Set(refState => refState.ShowOccupied = value);
        public void SetShowAvailable(bool value) => Set(refState => refState.ShowAvailable = value);

        public void ClearFilters()
        {
            _state.ClearToDefault();
            Persist();
            Changed?.Invoke();
        }

        public void Persist()
        {
            _repository.Save(ToSnapshot());
        }

        private void Set(Action<WorldMapFilterState> mutate)
        {
            mutate(_state);
            Persist();
            Changed?.Invoke();
        }

        private void ApplySnapshot(WorldMapFilterSnapshot snapshot)
        {
            _state.ShowCities = snapshot.ShowCities;
            _state.ShowVillages = snapshot.ShowVillages;
            _state.ShowResources = snapshot.ShowResources;
            _state.ShowCreatures = snapshot.ShowCreatures;
            _state.ShowDragons = snapshot.ShowDragons;
            _state.ShowLandmarks = snapshot.ShowLandmarks;
            _state.ShowOccupied = snapshot.ShowOccupied;
            _state.ShowAvailable = snapshot.ShowAvailable;
        }

        private WorldMapFilterSnapshot ToSnapshot() =>
            new()
            {
                ShowCities = _state.ShowCities,
                ShowVillages = _state.ShowVillages,
                ShowResources = _state.ShowResources,
                ShowCreatures = _state.ShowCreatures,
                ShowDragons = _state.ShowDragons,
                ShowLandmarks = _state.ShowLandmarks,
                ShowOccupied = _state.ShowOccupied,
                ShowAvailable = _state.ShowAvailable
            };
    }
}
