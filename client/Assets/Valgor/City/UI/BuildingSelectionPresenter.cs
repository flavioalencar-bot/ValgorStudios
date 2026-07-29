using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Valgor.City.Buildings;
using Valgor.City.Camera;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Economy;
using Valgor.City.Production;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.City.Input;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Orquestra seleção → câmera → menu contextual → modais de Detalhes/Atualizar/Obter.
    /// </summary>
    public sealed class BuildingSelectionPresenter
    {
        private readonly CityController _city;
        private readonly IDragonGateway? _dragons;
        private readonly VisualElement _panelRoot;
        private readonly BuildingContextMenu _contextMenu;
        private readonly BuildingContextToast _toast;
        private readonly BuildingDetailsPanel _detailsPanel;
        private readonly BuildingUpgradeModal _upgradeModal;
        private readonly BuildingDetailsModal _detailsModal;
        private readonly ObtainMoreResourcesModal _obtainModal;
        private readonly AutoRefillConfirmModal _autoRefillModal;
        private readonly ResourceItemInventory _inventory;
        private readonly VisualElement _actionPanel;
        private readonly Label _actionTitle;
        private readonly VisualElement _actionBodyHost;
        private readonly VisualElement _actionButtons;
        private readonly Label _feedback;
        private readonly Action? _goToWorldMap;
        private CityCameraController? _cameraController;
        private UnityEngine.Camera? _camera;
        private BuildingInstance? _current;
        private BuildingContextAction? _openPanelAction;
        private float _ignoreOutsideClickUntil;
        private string? _returnToDefinitionId;
        private ResourceRequirementView? _pendingObtainResource;
        private bool _reopenUpgradeAfterSelect;
        private float _nextUpgradeModalRefresh;

        public BuildingSelectionPresenter(
            CityController city,
            VisualElement panelRoot,
            IDragonGateway? dragons,
            Action? goToWorldMap = null)
        {
            _city = city ?? throw new ArgumentNullException(nameof(city));
            _panelRoot = panelRoot ?? throw new ArgumentNullException(nameof(panelRoot));
            _dragons = dragons;
            _goToWorldMap = goToWorldMap;
            _inventory = CityResourceItems.Shared;
            _inventory.EnsureLoaded();

            _contextMenu = new BuildingContextMenu(panelRoot);
            _toast = new BuildingContextToast(panelRoot);
            _detailsPanel = new BuildingDetailsPanel(panelRoot);
            _upgradeModal = new BuildingUpgradeModal(panelRoot);
            _detailsModal = new BuildingDetailsModal(panelRoot);
            _obtainModal = new ObtainMoreResourcesModal(panelRoot);
            _autoRefillModal = new AutoRefillConfirmModal(panelRoot);
            _actionPanel = BuildActionPanel(out _actionTitle, out _actionBodyHost, out _actionButtons, out _feedback);
            panelRoot.Add(_actionPanel);

            WireModals();
            _inventory.Changed += OnInventoryChanged;
            _city.Selection.SelectionChanged += OnSelectionChanged;
            _city.BuildingChanged += OnBuildingChanged;
            ResolveCamera();
        }

        public ResourceItemInventory Inventory => _inventory;

        public void Tick()
        {
            if (_current == null)
            {
                return;
            }

            if (_city.TryGetView(_current, out var liveView) && liveView != null)
            {
                EnsureCamera();
                if (_camera != null)
                {
                    _contextMenu.Reposition(
                        _panelRoot,
                        _camera,
                        liveView.GetScreenRect(_camera),
                        reserveRightPanel: IsAnyPanelOpen());
                }
            }

            if (_openPanelAction == BuildingContextAction.Upgrade &&
                _upgradeModal.IsVisible &&
                _current != null &&
                (_current.State == BuildingState.Upgrading || Time.unscaledTime >= _nextUpgradeModalRefresh))
            {
                _nextUpgradeModalRefresh = Time.unscaledTime + 0.5f;
                RefreshUpgradeModal();
            }

            HandleOutsideClick();
        }

        private bool IsAnyPanelOpen() =>
            _detailsPanel.IsVisible ||
            _detailsModal.IsVisible ||
            _upgradeModal.IsVisible ||
            _obtainModal.IsVisible ||
            _autoRefillModal.IsVisible ||
            _actionPanel.style.display == DisplayStyle.Flex;

        public void RefreshCurrent()
        {
            if (_current == null)
            {
                return;
            }

            // Evita NRE se a view foi destruída (ex.: tick de produção no Awake da City).
            if (!TryGetWorldAnchor(_current, out _))
            {
                return;
            }

            OpenContextFor(_current, refocusCamera: false);
            if (_openPanelAction.HasValue)
            {
                OpenActionPanel(_openPanelAction.Value);
            }
        }

        /// <summary>API de smoke/QA: abre o painel Atualizar do selecionado.</summary>
        public void DebugOpenUpgradePanel()
        {
            if (_current == null)
            {
                return;
            }

            _openPanelAction = BuildingContextAction.Upgrade;
            OpenActionPanel(BuildingContextAction.Upgrade);
        }

        /// <summary>API de smoke: toast do gancho Decoração.</summary>
        public void DebugShowDecorationPlaceholder() => ExecuteDecorationPlaceholder();

        /// <summary>API de smoke/QA: abre o painel Abrir/Dragões do selecionado.</summary>
        public void DebugOpenOpenPanel()
        {
            if (_current == null)
            {
                return;
            }

            _openPanelAction = BuildingContextAction.Open;
            OpenActionPanel(BuildingContextAction.Open);
        }

        /// <summary>API de smoke/QA: alimenta via Torre.</summary>
        public void DebugFeedDragon() => ExecuteFeedDragon();

        /// <summary>API de smoke/QA: clica no primeiro pré-requisito não cumprido (botão Ir).</summary>
        public void DebugGoToFirstUnmetRequirement()
        {
            if (_current == null)
            {
                return;
            }

            foreach (var check in _city.GetDependencyChecks(_current))
            {
                if (!check.Satisfied && !string.IsNullOrEmpty(check.JumpToDefinitionId))
                {
                    GoToRequirementBuilding(check.JumpToDefinitionId!);
                    return;
                }
            }
        }

        public void DebugOpenDetailsPanel()
        {
            if (_current == null)
            {
                return;
            }

            _openPanelAction = BuildingContextAction.Details;
            OpenActionPanel(BuildingContextAction.Details);
        }

        public void DebugOpenObtainForFirstMissing()
        {
            if (_current == null)
            {
                return;
            }

            RefreshUpgradeModal(forceShow: true);
            var presentation = BuildingUpgradePresentationBuilder.Build(_city, _current, _inventory);
            foreach (var res in presentation.ResourceCosts)
            {
                if (!res.IsSatisfied)
                {
                    OpenObtainFor(res);
                    return;
                }
            }
        }

        public void DebugOpenObtainForResource(ResourceType resource)
        {
            if (_current == null)
            {
                return;
            }

            RefreshUpgradeModal(forceShow: true);
            var presentation = BuildingUpgradePresentationBuilder.Build(_city, _current, _inventory);
            foreach (var res in presentation.ResourceCosts)
            {
                if (res.ResourceId == resource)
                {
                    OpenObtainFor(res);
                    return;
                }
            }

            OpenObtainFor(new ResourceRequirementView
            {
                ResourceId = resource,
                DisplayName = BuildingUpgradePresentationBuilder.FriendlyResource(resource),
                Available = _city.Economy.Wallet.Get(resource),
                Required = Math.Max(_city.Economy.Wallet.Get(resource) + 10_000, 10_000),
                CanAutoRefill = true
            });
        }

        public void DebugUseFirstInventoryItem()
        {
            if (_pendingObtainResource == null)
            {
                return;
            }

            var stacks = _inventory.GetStacksFor(_pendingObtainResource.ResourceId);
            if (stacks.Count == 0)
            {
                return;
            }

            UseInventoryItem(stacks[0].ItemId, 1);
        }

        public void DebugOpenAutoRefill()
        {
            if (_pendingObtainResource == null)
            {
                AutoRefillConfirmModal.SkipConfirmThisSession = false;
                DebugOpenObtainForFirstMissing();
            }

            BeginAutoRefill();
        }

        public void DebugConfirmAutoRefill()
        {
            if (_pendingObtainResource == null)
            {
                return;
            }

            var resource = _pendingObtainResource.ResourceId;
            var missing = Math.Max(0, _pendingObtainResource.Required - _city.Economy.Wallet.Get(resource));
            var plan = AutoRefillPlanner.Plan(_inventory, resource, missing);
            plan.BeforeAmount = _city.Economy.Wallet.Get(resource);
            plan.AfterAmount = plan.BeforeAmount + plan.TotalObtained;
            plan.RequiredAmount = _pendingObtainResource.Required;
            _autoRefillModal.Hide();
            ApplyAutoRefill(plan);
        }

        public void DebugConfirmUpgrade() => ExecuteUpgrade();

        public void DebugReturnToOriginIfAny()
        {
            if (!string.IsNullOrEmpty(_returnToDefinitionId))
            {
                ReturnToOriginBuilding();
            }
        }

        public void Dispose()
        {
            _inventory.Changed -= OnInventoryChanged;
            _city.Selection.SelectionChanged -= OnSelectionChanged;
            _city.BuildingChanged -= OnBuildingChanged;
        }

        private void WireModals()
        {
            _upgradeModal.Bind(
                onClose: OnPremiumModalClosed,
                onUpgrade: ExecuteUpgrade,
                onInstant: ExecuteInstantComplete,
                onGo: req => GoToRequirementBuilding(req.TargetBuildingId),
                onObtain: OpenObtainFor,
                onInfo: () =>
                {
                    if (_current != null)
                    {
                        OpenDetailsModal(_current);
                    }
                },
                onSatisfyQa: req =>
                {
                    var qa = UnityEngine.Object.FindFirstObjectByType<Valgor.City.Qa.CityProgressionQaController>();
                    qa?.RequestSatisfyRequirement(req.TargetBuildingId, req.RequiredLevel);
                },
                onReturn: ReturnToOriginBuilding);

            _detailsModal.Bind(
                onClose: OnPremiumModalClosed,
                onUpgrade: () =>
                {
                    if (_current != null)
                    {
                        _openPanelAction = BuildingContextAction.Upgrade;
                        OpenActionPanel(BuildingContextAction.Upgrade);
                    }
                });

            _obtainModal.Bind(
                onClose: () =>
                {
                    _pendingObtainResource = null;
                    if (_openPanelAction == BuildingContextAction.Upgrade && _current != null)
                    {
                        RefreshUpgradeModal();
                    }
                    else
                    {
                        OnPremiumModalClosed();
                    }
                },
                onUse: UseInventoryItem,
                onAutoRefill: BeginAutoRefill,
                onSimulateShortageQa: SimulateResourceShortageQa,
                onRestoreResourcesQa: RestoreResourcesQa);

            _autoRefillModal.Bind(
                onCancel: () => { },
                onConfirm: (plan, _) => ApplyAutoRefill(plan));
        }

        private void OnPremiumModalClosed()
        {
            if (_upgradeModal.IsVisible || _detailsModal.IsVisible || _obtainModal.IsVisible || _autoRefillModal.IsVisible)
            {
                return;
            }

            if (_actionPanel.style.display != DisplayStyle.Flex)
            {
                _openPanelAction = null;
                BetaJourneyGuide.NotifyCityModalOpen(false);
            }
        }

        private void OnInventoryChanged()
        {
            if (_obtainModal.IsVisible && _pendingObtainResource != null)
            {
                var wallet = _city.Economy.Wallet;
                _obtainModal.Refresh(
                    _inventory,
                    wallet.Get(_pendingObtainResource.ResourceId),
                    _pendingObtainResource.Required);
            }

            if (_upgradeModal.IsVisible)
            {
                RefreshUpgradeModal();
            }

            var hud = UnityEngine.Object.FindFirstObjectByType<CityHudController>();
            hud?.ForceRefreshResources();
        }

        private void OnBuildingChanged()
        {
            EnsureCamera();
            _cameraController?.SuppressFocus(0.85f);
            _cameraController?.CancelFocus();

            var feedback = _city.LastUpgradeFeedback;
            if (!string.IsNullOrEmpty(feedback) &&
                feedback.IndexOf("→ Nv.", StringComparison.Ordinal) >= 0)
            {
                BetaMissions.Notify(MissionEvent.UpgradeComplete);
            }

            RefreshCurrent();
        }

        private void OnSelectionChanged(BuildingInstance? selected)
        {
            // Re-clique no mesmo prédio (fallthrough do Input System após Detalhes) — não fecha o painel.
            if (selected != null && ReferenceEquals(selected, _current))
            {
                OpenContextFor(selected, refocusCamera: false);
                if (_openPanelAction.HasValue)
                {
                    OpenActionPanel(_openPanelAction.Value);
                }

                return;
            }

            _openPanelAction = null;
            HideActionPanel();
            if (selected == null)
            {
                _current = null;
                _contextMenu.Hide();
                return;
            }

            _current = selected;
            if (string.Equals(selected.DefinitionId, "castle", StringComparison.Ordinal))
            {
                BetaMissions.Notify(MissionEvent.SelectCastle);
            }

            var reopenUpgrade = _reopenUpgradeAfterSelect;
            _reopenUpgradeAfterSelect = false;
            OpenContextFor(selected, refocusCamera: true);
            if (reopenUpgrade)
            {
                _openPanelAction = BuildingContextAction.Upgrade;
                OpenActionPanel(BuildingContextAction.Upgrade);
            }
        }

        private void OpenContextFor(BuildingInstance building, bool refocusCamera)
        {
            if (!_city.TryGetView(building, out var view))
            {
                _contextMenu.Hide();
                return;
            }

            var definition = _city.GetDefinition(building);
            if (refocusCamera)
            {
                EnsureCamera();
                // Sem override de zoom — enquadramento por tier não deve alterar a câmera
                // em seleção/upgrade (evita salto ao cruzar faixa).
                _cameraController?.FocusOn(view.transform.position, 0.35f, orthographicSize: null);
            }

            var actions = BuildActions(building, definition);
            _contextMenu.Show(actions, OnContextAction, _openPanelAction);
            if (_camera != null)
            {
                _contextMenu.Reposition(
                    _panelRoot,
                    _camera,
                    view.GetScreenRect(_camera),
                    reserveRightPanel: IsAnyPanelOpen());
            }
        }

        private List<BuildingContextActionInfo> BuildActions(
            BuildingInstance building,
            BuildingDefinition definition)
        {
            var id = building.DefinitionId;

            // Primeira entrega: ações explícitas por edifício.
            if (string.Equals(id, "castle", StringComparison.Ordinal))
            {
                return new List<BuildingContextActionInfo>
                {
                    new(BuildingContextAction.Decoration, "Decoração", true),
                    new(BuildingContextAction.Details, "Detalhes", true),
                    UpgradeAction(building, definition)
                };
            }

            if (string.Equals(id, "farm", StringComparison.Ordinal) ||
                string.Equals(id, "lumbermill", StringComparison.Ordinal) ||
                string.Equals(id, "quarry", StringComparison.Ordinal) ||
                string.Equals(id, "mine", StringComparison.Ordinal))
            {
                var canCollect = _city.Economy.Production.TryGetState(building.DefinitionId, out var state) &&
                                 state.HasCollectable;
                return new List<BuildingContextActionInfo>
                {
                    new(BuildingContextAction.Collect, "Coletar", canCollect,
                        canCollect ? null : "Nada acumulado ainda."),
                    new(BuildingContextAction.Details, "Detalhes", true),
                    UpgradeAction(building, definition)
                };
            }

            if (string.Equals(id, "warehouse", StringComparison.Ordinal))
            {
                return new List<BuildingContextActionInfo>
                {
                    new(BuildingContextAction.Open, "Abrir", true),
                    new(BuildingContextAction.Details, "Detalhes", true),
                    UpgradeAction(building, definition)
                };
            }

            if (string.Equals(id, "academy", StringComparison.Ordinal) ||
                string.Equals(id, "wall", StringComparison.Ordinal))
            {
                return new List<BuildingContextActionInfo>
                {
                    new(BuildingContextAction.Details, "Detalhes", true),
                    UpgradeAction(building, definition)
                };
            }

            if (string.Equals(id, "arena", StringComparison.Ordinal) ||
                string.Equals(id, "hospital", StringComparison.Ordinal) ||
                string.Equals(id, "temple", StringComparison.Ordinal) ||
                string.Equals(id, "market", StringComparison.Ordinal) ||
                string.Equals(id, "laboratory", StringComparison.Ordinal))
            {
                return new List<BuildingContextActionInfo>
                {
                    new(BuildingContextAction.Open, "Abrir", true),
                    new(BuildingContextAction.Details, "Detalhes", true),
                    UpgradeAction(building, definition)
                };
            }

            if (string.Equals(id, "dragon-tower", StringComparison.Ordinal))
            {
                var canFeed = _dragons != null && _dragons.RoostOccupantCount > 0;
                return new List<BuildingContextActionInfo>
                {
                    new(BuildingContextAction.Open, "Dragões", true),
                    new(BuildingContextAction.Feed, "Alimentar", canFeed,
                        canFeed ? null : "Nenhum dragão no ninho."),
                    new(BuildingContextAction.Details, "Detalhes", true),
                    UpgradeAction(building, definition)
                };
            }

            // Demais edifícios (fora do bloco contextual): Detalhes + Atualizar.
            var list = new List<BuildingContextActionInfo>
            {
                new(BuildingContextAction.Details, "Detalhes", true),
                UpgradeAction(building, definition)
            };

            if (ProductionCatalog.TryGet(building.DefinitionId, out _))
            {
                var canCollect = _city.Economy.Production.TryGetState(building.DefinitionId, out var state) &&
                                 state.HasCollectable;
                list.Insert(0, new BuildingContextActionInfo(
                    BuildingContextAction.Collect,
                    "Coletar",
                    canCollect,
                    canCollect ? null : "Nada acumulado ainda."));
            }

            return list;
        }

        private BuildingContextActionInfo UpgradeAction(BuildingInstance building, BuildingDefinition definition)
        {
            if (building.Level >= definition.MaxLevel &&
                building.State != BuildingState.Available)
            {
                return new BuildingContextActionInfo(
                    BuildingContextAction.Upgrade,
                    "Nível Máximo",
                    enabled: false,
                    disabledReason: "Este edifício atingiu o nível máximo.",
                    icon: BuildingContextIcon.Crown);
            }

            var upgradeLabel = building.State == BuildingState.Available && building.Level <= 0
                ? "Construir"
                : "Atualizar";
            var canUpgrade = _city.CanUpgrade(building, definition) &&
                             string.IsNullOrEmpty(_city.GetUpgradeBlockReason(building, definition));
            var softEnable = building.State != BuildingState.Upgrading &&
                             building.CanUpgrade(definition) &&
                             (_city.GetActiveConstructionCount() < CityController.ConstructionQueueSlots ||
                              building.State == BuildingState.Upgrading);
            return new BuildingContextActionInfo(
                BuildingContextAction.Upgrade,
                upgradeLabel,
                softEnable || canUpgrade,
                _city.GetUpgradeBlockReason(building, definition),
                BuildingContextIcon.Upgrade);
        }

        private void OnContextAction(BuildingContextAction action)
        {
            if (_current == null)
            {
                return;
            }

            CityBuildingPointerInput.SuppressWorldClicks(0.35f);
            _ignoreOutsideClickUntil = Time.unscaledTime + 0.35f;

            switch (action)
            {
                case BuildingContextAction.Collect:
                    ExecuteCollect();
                    break;
                case BuildingContextAction.Feed:
                    ExecuteFeedDragon();
                    break;
                case BuildingContextAction.Send:
                    ExecuteSend();
                    break;
                case BuildingContextAction.Decoration:
                    ExecuteDecorationPlaceholder();
                    break;
                case BuildingContextAction.Upgrade:
                {
                    var definition = _city.GetDefinition(_current);
                    if (_current.Level >= definition.MaxLevel)
                    {
                        _toast.Show("Este edifício atingiu o nível máximo.");
                        return;
                    }

                    _openPanelAction = action;
                    _contextMenu.SetSelectedAction(action);
                    OpenActionPanel(action);
                    break;
                }
                case BuildingContextAction.Details:
                case BuildingContextAction.Open:
                default:
                    _openPanelAction = action;
                    _contextMenu.SetSelectedAction(action);
                    OpenActionPanel(action);
                    break;
            }
        }

        private void ExecuteDecorationPlaceholder()
        {
            // Gancho futuro: BuildingDecorationCatalog.ListSkins(_current.DefinitionId)
            _contextMenu.SetSelectedAction(BuildingContextAction.Decoration);
            _toast.Show(Valgor.City.Decoration.BuildingDecorationCatalog.ComingSoonMessage);
            Debug.Log(
                $"[Valgor.City] decoration action id={Valgor.City.Decoration.BuildingDecorationCatalog.ActionId} " +
                $"(placeholder — skins em breve)");
        }

        private void ExecuteFeedDragon()
        {
            if (_dragons == null)
            {
                _feedback.text = "Módulo de dragões indisponível.";
                _feedback.style.display = DisplayStyle.Flex;
                return;
            }

            string? fedName = null;
            string lastError = "Nenhum dragão precisa de comida agora.";
            foreach (var status in _dragons.GetDragonStatuses())
            {
                if (status.Hunger >= status.MaxHunger)
                {
                    continue;
                }

                if (_dragons.TryFeed(status.DragonId, out var error))
                {
                    fedName = status.DisplayName;
                    break;
                }

                lastError = error;
            }

            _feedback.text = fedName != null
                ? $"{fedName} alimentado."
                : lastError;
            _feedback.style.display = DisplayStyle.Flex;
            if (fedName != null)
            {
                BetaMissions.Notify(MissionEvent.FeedDragon);
            }

            _city.Persist();
            RefreshCurrent();
        }

        private void ExecuteCollect()
        {
            var amount = _city.CollectSelected();
            _feedback.text = amount > 0 ? $"+{amount} coletado!" : "Nada para coletar agora.";
            if (amount > 0 &&
                _current != null &&
                string.Equals(_current.DefinitionId, "farm", StringComparison.Ordinal))
            {
                BetaMissions.Notify(MissionEvent.CollectFarm);
            }

            if (_actionPanel.style.display != DisplayStyle.Flex)
            {
                _openPanelAction = BuildingContextAction.Details;
                OpenActionPanel(BuildingContextAction.Details);
            }

            _feedback.style.display = DisplayStyle.Flex;
            RefreshCurrent();
        }

        private void ExecuteSend()
        {
            _city.Persist();
            if (_goToWorldMap == null)
            {
                _feedback.text = "Mapa indisponível.";
                return;
            }

            _goToWorldMap.Invoke();
        }

        private void OpenActionPanel(BuildingContextAction action)
        {
            if (_current == null)
            {
                return;
            }

            var definition = _city.GetDefinition(_current);
            _actionButtons.Clear();
            _actionBodyHost.Clear();
            _feedback.text = string.Empty;

            if (action is BuildingContextAction.Details or BuildingContextAction.Open)
            {
                // Torre: painel acionável (status + Alimentar), não só texto.
                if (action == BuildingContextAction.Open &&
                    string.Equals(definition.Id, "dragon-tower", StringComparison.Ordinal))
                {
                    HidePremiumModals();
                    _detailsPanel.Hide();
                    _actionTitle.text = ActionTitle(action, definition);
                    var model = BuildingDetailsViewModel.From(_city, _current, definition, openMode: true);
                    AppendBodyText(model.Body);
                    var canFeed = _dragons != null && _dragons.RoostOccupantCount > 0;
                    AddPanelButton("Alimentar", ExecuteFeedDragon, enabled: canFeed);
                    AddPanelButton("Fechar", HideActionPanel);
                    _actionPanel.style.display = DisplayStyle.Flex;
                    _actionPanel.style.visibility = Visibility.Visible;
                    _actionPanel.style.opacity = 1f;
                    _actionPanel.BringToFront();
                    BetaJourneyGuide.NotifyCityModalOpen(true, panelOnRight: true);
                    Debug.Log("[Valgor] Painel Dragões aberto (acionável).");
                    return;
                }

                if (action == BuildingContextAction.Details)
                {
                    _actionPanel.style.display = DisplayStyle.None;
                    _detailsPanel.Hide();
                    OpenDetailsModal(_current);
                    Debug.Log($"[Valgor] Painel Detalhes aberto: {definition.DisplayName}");
                    return;
                }

                // Abrir (não-torre): painel legado de detalhes à direita.
                _actionPanel.style.display = DisplayStyle.None;
                HidePremiumModals();
                var detailsModel = BuildingDetailsViewModel.From(
                    _city,
                    _current,
                    definition,
                    openMode: true);
                _detailsPanel.Show(detailsModel, HideActionPanel);
                BetaJourneyGuide.NotifyCityModalOpen(true, panelOnRight: true);
                Debug.Log($"[Valgor] Painel Abrir aberto: {definition.DisplayName}");
                return;
            }

            _detailsPanel.Hide();
            _detailsModal.HideWithoutCallback();
            _actionTitle.text = ActionTitle(action, definition);

            switch (action)
            {
                case BuildingContextAction.Upgrade:
                    _actionPanel.style.display = DisplayStyle.None;
                    RefreshUpgradeModal(forceShow: true);
                    break;
                default:
                    HidePremiumModals();
                    AppendBodyText(BuildPanelBodyLegacy(action, _current, definition));
                    AddPanelButton("Fechar", HideActionPanel);
                    _actionPanel.style.display = DisplayStyle.Flex;
                    _actionPanel.style.visibility = Visibility.Visible;
                    _actionPanel.style.opacity = 1f;
                    _actionPanel.BringToFront();
                    BetaJourneyGuide.NotifyCityModalOpen(true, panelOnRight: true);
                    break;
            }
        }

        private void OpenDetailsModal(BuildingInstance building)
        {
            HidePremiumModals();
            var presentation = BuildingUpgradePresentationBuilder.BuildDetails(_city, building);
            _detailsModal.Show(presentation);
            _openPanelAction = BuildingContextAction.Details;
        }

        private void RefreshUpgradeModal(bool forceShow = false, string? feedback = null)
        {
            if (_current == null)
            {
                return;
            }

            if (!forceShow && !_upgradeModal.IsVisible && _openPanelAction != BuildingContextAction.Upgrade)
            {
                return;
            }

            var presentation = BuildingUpgradePresentationBuilder.Build(_city, _current, _inventory);
            var showReturn = !string.IsNullOrEmpty(_returnToDefinitionId) &&
                             !string.Equals(_returnToDefinitionId, _current.DefinitionId, StringComparison.Ordinal);
            _obtainModal.HideWithoutCallback();
            _detailsModal.HideWithoutCallback();
            _upgradeModal.Show(presentation, feedback ?? _feedback.text, showReturn);
            _openPanelAction = BuildingContextAction.Upgrade;
        }

        private void HidePremiumModals()
        {
            _upgradeModal.HideWithoutCallback();
            _detailsModal.HideWithoutCallback();
            _obtainModal.HideWithoutCallback();
            _autoRefillModal.Hide();
        }

        private void OpenObtainFor(ResourceRequirementView resource)
        {
            _pendingObtainResource = resource;
            _obtainModal.Show(
                resource.ResourceId,
                resource.Available,
                resource.Required,
                _inventory);
        }

        private void UseInventoryItem(string itemId, int quantity)
        {
            if (!_inventory.TryUse(itemId, _city.Economy.Wallet, out var credited, out var error, quantity))
            {
                _toast.Show(error);
                return;
            }

            _city.Economy.PersistWallet();
            var hud = UnityEngine.Object.FindFirstObjectByType<CityHudController>();
            hud?.ForceRefreshResources();
            _toast.Show($"+{BuildingUpgradePresentationBuilder.FormatAmount(credited)} creditado");
            OnInventoryChanged();
            if (_current != null)
            {
                RefreshUpgradeModal();
            }
        }

        private void BeginAutoRefill()
        {
            if (_pendingObtainResource == null)
            {
                return;
            }

            var resource = _pendingObtainResource.ResourceId;
            var missing = Math.Max(0, _pendingObtainResource.Required - _city.Economy.Wallet.Get(resource));
            var plan = AutoRefillPlanner.Plan(_inventory, resource, missing);
            plan.BeforeAmount = _city.Economy.Wallet.Get(resource);
            plan.AfterAmount = plan.BeforeAmount + plan.TotalObtained;
            plan.RequiredAmount = _pendingObtainResource.Required;

            if (plan.Lines.Length == 0)
            {
                _toast.Show("Nenhum item disponível para reabastecer.");
                return;
            }

            if (AutoRefillConfirmModal.SkipConfirmThisSession)
            {
                ApplyAutoRefill(plan);
                return;
            }

            _autoRefillModal.Show(plan);
        }

        private void ApplyAutoRefill(AutoRefillPlan plan)
        {
            if (!_inventory.TryApplyAutoRefill(plan, _city.Economy.Wallet, out var error))
            {
                _toast.Show(error);
                return;
            }

            _city.Economy.PersistWallet();
            var hud = UnityEngine.Object.FindFirstObjectByType<CityHudController>();
            hud?.ForceRefreshResources();
            _toast.Show(
                $"+{BuildingUpgradePresentationBuilder.FormatAmount(plan.TotalObtained)} via reabastecimento");
            OnInventoryChanged();
            if (_current != null)
            {
                RefreshUpgradeModal();
            }
        }

        private void SimulateResourceShortageQa()
        {
            if (!CityProgressionQa.IsActive || _pendingObtainResource == null)
            {
                return;
            }

            var resource = _pendingObtainResource.ResourceId;
            var required = _pendingObtainResource.Required;
            var low = Math.Max(0, required / 20);
            _city.Economy.Wallet.SetAmount(resource, low);
            _city.Economy.PersistWallet();
            var hud = UnityEngine.Object.FindFirstObjectByType<CityHudController>();
            hud?.ForceRefreshResources();
            _toast.Show($"QA: {BuildingUpgradePresentationBuilder.FriendlyResource(resource)} → {low}");
            OnInventoryChanged();
            RefreshUpgradeModal();
        }

        private void RestoreResourcesQa()
        {
            if (!CityProgressionQa.IsActive)
            {
                return;
            }

            var qa = UnityEngine.Object.FindFirstObjectByType<Valgor.City.Qa.CityProgressionQaController>();
            qa?.TopUpNow();
            _inventory.SeedQaControlled();
            _toast.Show("QA: recursos e inventário restaurados");
            OnInventoryChanged();
            RefreshUpgradeModal();
        }

        private void ReturnToOriginBuilding()
        {
            if (string.IsNullOrEmpty(_returnToDefinitionId))
            {
                return;
            }

            var originId = _returnToDefinitionId;
            _returnToDefinitionId = null;
            if (!_city.TryGetBuildingByDefinitionId(originId!, out var origin))
            {
                return;
            }

            CityBuildingPointerInput.SuppressWorldClicks(0.35f);
            _ignoreOutsideClickUntil = Time.unscaledTime + 0.35f;
            HidePremiumModals();
            _reopenUpgradeAfterSelect = true;
            _city.Selection.Select(origin);
        }

        private void GoToRequirementBuilding(string definitionId)
        {
            CityBuildingPointerInput.SuppressWorldClicks(0.35f);
            _ignoreOutsideClickUntil = Time.unscaledTime + 0.35f;

            if (!_city.TryGetBuildingByDefinitionId(definitionId, out var target))
            {
                _feedback.text = "Edifício exigido não encontrado na cidade.";
                _upgradeModal.SetFeedback(_feedback.text);
                return;
            }

            if (_current != null)
            {
                _returnToDefinitionId = _current.DefinitionId;
            }

            HidePremiumModals();
            _detailsPanel.Hide();
            _actionPanel.style.display = DisplayStyle.None;
            _openPanelAction = null;
            BetaJourneyGuide.NotifyCityModalOpen(false);
            _city.Selection.Select(target);
        }

        private string BuildPanelBodyLegacy(
            BuildingContextAction action,
            BuildingInstance building,
            BuildingDefinition definition)
        {
            if (action == BuildingContextAction.Produce)
            {
                return BuildProductionBlock(building);
            }

            return definition.DisplayName;
        }

        private void ExecuteUpgrade()
        {
            var building = _city.Selection.Selected;
            if (building == null)
            {
                return;
            }

            var definition = _city.GetDefinition(building);
            if (building.State == BuildingState.Upgrading)
            {
                _feedback.text = "Melhoria já em andamento.";
                RefreshUpgradeModal(feedback: _feedback.text);
                return;
            }

            if (_city.TryUpgradeSelected())
            {
                var duration = definition.GetUpgradeDuration(building.Level);
                _feedback.text = $"{definition.DisplayName}: melhoria iniciada ({(int)duration.TotalSeconds}s)";
            }
            else
            {
                _feedback.text = _city.GetUpgradeBlockReason(building, definition)
                                 ?? "Recursos insuficientes ou construtor ocupado.";
            }

            RefreshCurrent();
            if (_openPanelAction == BuildingContextAction.Upgrade)
            {
                RefreshUpgradeModal(forceShow: true, feedback: _feedback.text);
            }
        }

        private void ExecuteInstantComplete()
        {
            if (_city.TryInstantCompleteSelected(out var error))
            {
                _feedback.text = _city.LastUpgradeFeedback ?? "Upgrade concluído!";
            }
            else
            {
                _feedback.text = error;
            }

            RefreshCurrent();
            if (_openPanelAction == BuildingContextAction.Upgrade)
            {
                RefreshUpgradeModal(forceShow: true, feedback: _feedback.text);
            }
        }

        private void HandleOutsideClick()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasReleasedThisFrame)
            {
                return;
            }

            if (CityCameraController.ShouldSuppressBuildingClick ||
                CityBuildingPointerInput.IsWorldClickSuppressed ||
                Time.unscaledTime < _ignoreOutsideClickUntil)
            {
                return;
            }

            EnsureCamera();
            if (_camera == null)
            {
                return;
            }

            var pos = mouse.position.ReadValue();
            if (IsScreenOverElement(_contextMenu.Root, pos) ||
                IsScreenOverElement(_detailsPanel.Root, pos) ||
                IsScreenOverElement(_detailsModal.Root, pos) ||
                IsScreenOverElement(_upgradeModal.Root, pos) ||
                IsScreenOverElement(_obtainModal.Root, pos) ||
                IsScreenOverElement(_autoRefillModal.Root, pos) ||
                (_actionPanel.style.display == DisplayStyle.Flex && IsScreenOverElement(_actionPanel, pos)))
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var hit, 500f))
            {
                if (hit.collider != null &&
                    (hit.collider.GetComponentInParent<BuildingView>() != null ||
                     hit.collider.GetComponentInParent<BuildingCollectableClickProxy>() != null ||
                     hit.collider.GetComponentInParent<BuildingSelectionClickProxy>() != null))
                {
                    return;
                }
            }

            _city.Selection.Deselect();
        }

        private bool TryGetWorldAnchor(BuildingInstance building, out Vector3 anchor)
        {
            if (_city.TryGetView(building, out var view) && view != null)
            {
                // Offset lateral + altura — menu não cobre o centro visual do prédio.
                anchor = view.transform.position + Vector3.up * 2.8f + view.transform.right * 1.35f;
                return true;
            }

            anchor = default;
            return false;
        }

        private void EnsureCamera()
        {
            if (_cameraController == null)
            {
                _cameraController = UnityEngine.Object.FindFirstObjectByType<CityCameraController>();
            }

            if (_camera == null)
            {
                _camera = _cameraController != null
                    ? _cameraController.GetComponent<UnityEngine.Camera>()
                    : UnityEngine.Camera.main;
            }
        }

        private void ResolveCamera() => EnsureCamera();

        private void HideActionPanel()
        {
            _openPanelAction = null;
            _detailsPanel.Hide();
            HidePremiumModals();
            _actionPanel.style.display = DisplayStyle.None;
            _actionButtons.Clear();
            _actionBodyHost.Clear();
            BetaJourneyGuide.NotifyCityModalOpen(false);
        }

        private void AppendBodyText(string text)
        {
            var label = new Label(text);
            label.style.color = BetaVisualTheme.TextPrimary;
            label.style.fontSize = 13;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 6;
            _actionBodyHost.Add(label);
        }

        private void AddPanelButton(string text, Action action, bool enabled = true)
        {
            var button = new Button(action) { text = text };
            button.SetEnabled(enabled);
            button.style.marginTop = 8;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.paddingTop = 8;
            button.style.paddingBottom = 8;
            button.style.backgroundColor = enabled
                ? BetaVisualTheme.ButtonFace
                : new Color(0.18f, 0.18f, 0.2f, 0.9f);
            button.style.color = enabled
                ? BetaVisualTheme.TextPrimary
                : new Color(0.55f, 0.55f, 0.58f);
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = BetaVisualTheme.ButtonBorder;
            button.style.borderBottomColor = BetaVisualTheme.ButtonBorder;
            button.style.borderLeftColor = BetaVisualTheme.ButtonBorder;
            button.style.borderRightColor = BetaVisualTheme.ButtonBorder;
            button.style.fontSize = 13;
            button.style.opacity = enabled ? 1f : 0.55f;
            _actionButtons.Add(button);
        }

        private string BuildProductionBlock(BuildingInstance building) =>
            ProductionBuildingDetails.BuildBlock(building, _city.Economy.Production);

        private string FormatRemaining(DateTime completesAtUtc)
        {
            var remaining = completesAtUtc - _city.Economy.Clock.UtcNow;
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
            return $"{Math.Max(0, (int)remaining.TotalSeconds)}s";
        }

        private static string ActionTitle(BuildingContextAction action, BuildingDefinition definition) =>
            action switch
            {
                BuildingContextAction.Details => $"Detalhes — {definition.DisplayName}",
                BuildingContextAction.Upgrade => $"Atualizar — {definition.DisplayName}",
                BuildingContextAction.Decoration => $"Decoração — {definition.DisplayName}",
                BuildingContextAction.Open when string.Equals(definition.Id, "dragon-tower", StringComparison.Ordinal)
                    => $"Dragões — {definition.DisplayName}",
                BuildingContextAction.Open => $"Abrir — {definition.DisplayName}",
                BuildingContextAction.Collect => $"Coletar — {definition.DisplayName}",
                BuildingContextAction.Feed => $"Alimentar — {definition.DisplayName}",
                _ => definition.DisplayName
            };

        private static string FriendlyState(BuildingState state) => state switch
        {
            BuildingState.Ready => "Pronto",
            BuildingState.Available => "Disponível",
            BuildingState.Locked => "Bloqueado",
            BuildingState.Upgrading => "Melhorando",
            _ => state.ToString()
        };

        private static string FriendlyResource(ResourceType resource) => resource switch
        {
            ResourceType.Gold => "Ouro",
            ResourceType.Food => "Comida",
            ResourceType.Wood => "Madeira",
            ResourceType.Stone => "Pedra",
            ResourceType.Iron => "Ferro",
            ResourceType.DragonEssence => "Essência de Dragão",
            ResourceType.Diamonds => "Diamantes",
            _ => resource.ToString()
        };

        private static bool IsScreenOverElement(VisualElement element, Vector2 screenPos)
        {
            if (element == null || element.style.display == DisplayStyle.None || element.panel == null)
            {
                return false;
            }

            var panelPos = RuntimePanelUtils.ScreenToPanel(element.panel, screenPos);
            return element.worldBound.Contains(panelPos);
        }

        private static VisualElement BuildActionPanel(
            out Label title,
            out VisualElement bodyHost,
            out VisualElement buttons,
            out Label feedback)
        {
            var panel = new VisualElement { name = "building-action-panel" };
            panel.style.position = Position.Absolute;
            panel.style.right = 16;
            panel.style.top = 56;
            panel.style.bottom = 80;
            panel.style.width = 360;
            panel.style.maxWidth = 380;
            panel.style.maxHeight = 760;
            panel.style.paddingLeft = 14;
            panel.style.paddingRight = 14;
            panel.style.paddingTop = 12;
            panel.style.paddingBottom = 12;
            panel.style.backgroundColor = new Color(0.1f, 0.11f, 0.12f, 0.96f);
            panel.style.borderTopWidth = 2;
            panel.style.borderBottomWidth = 2;
            panel.style.borderLeftWidth = 2;
            panel.style.borderRightWidth = 2;
            panel.style.borderTopColor = BetaVisualTheme.AgedGold;
            panel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            panel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            panel.style.borderRightColor = BetaVisualTheme.AgedGold;
            panel.style.display = DisplayStyle.None;
            panel.style.flexDirection = FlexDirection.Column;
            panel.pickingMode = PickingMode.Position;

            title = new Label();
            title.style.color = BetaVisualTheme.AgedGoldBright;
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            title.style.flexShrink = 0;
            title.style.whiteSpace = WhiteSpace.Normal;
            panel.Add(title);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 120;
            scroll.style.maxHeight = StyleKeyword.None;
            bodyHost = new VisualElement { name = "action-body-host" };
            scroll.Add(bodyHost);
            panel.Add(scroll);

            feedback = new Label();
            feedback.style.color = BetaVisualTheme.AgedGoldBright;
            feedback.style.marginTop = 8;
            feedback.style.whiteSpace = WhiteSpace.Normal;
            feedback.style.flexShrink = 0;
            panel.Add(feedback);

            buttons = new VisualElement { name = "action-panel-buttons" };
            buttons.style.flexShrink = 0;
            buttons.style.marginTop = 4;
            panel.Add(buttons);
            return panel;
        }
    }
}
