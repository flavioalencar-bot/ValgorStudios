using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Valgor.City.Buildings;
using Valgor.City.Camera;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Production;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.City.Input;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Orquestra seleção → câmera → menu contextual → painel de ação.
    /// Primeira entrega: Castelo, Fazenda, Armazém.
    /// </summary>
    public sealed class BuildingSelectionPresenter
    {
        private readonly CityController _city;
        private readonly IDragonGateway? _dragons;
        private readonly VisualElement _panelRoot;
        private readonly BuildingContextMenu _contextMenu;
        private readonly BuildingDetailsPanel _detailsPanel;
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

            _contextMenu = new BuildingContextMenu(panelRoot);
            _detailsPanel = new BuildingDetailsPanel(panelRoot);
            _actionPanel = BuildActionPanel(out _actionTitle, out _actionBodyHost, out _actionButtons, out _feedback);
            panelRoot.Add(_actionPanel);

            _city.Selection.SelectionChanged += OnSelectionChanged;
            _city.BuildingChanged += RefreshCurrent;
            ResolveCamera();
        }

        public void Tick()
        {
            if (_current == null)
            {
                return;
            }

            if (TryGetWorldAnchor(_current, out var anchor))
            {
                EnsureCamera();
                if (_camera != null)
                {
                    _contextMenu.Reposition(
                        _panelRoot,
                        _camera,
                        anchor,
                        reserveRightPanel: IsAnyPanelOpen());
                }
            }

            if (_openPanelAction == BuildingContextAction.Upgrade &&
                _actionPanel.style.display == DisplayStyle.Flex)
            {
                RebuildUpgradeBody(_current);
            }

            HandleOutsideClick();
        }

        private bool IsAnyPanelOpen() =>
            _detailsPanel.IsVisible || _actionPanel.style.display == DisplayStyle.Flex;

        public void RefreshCurrent()
        {
            if (_current == null)
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

        public void Dispose()
        {
            _city.Selection.SelectionChanged -= OnSelectionChanged;
            _city.BuildingChanged -= RefreshCurrent;
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
            OpenContextFor(selected, refocusCamera: true);
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
                _cameraController?.FocusOn(view.transform.position, 0.35f);
            }

            var title = $"{definition.DisplayName}\nNv.{Math.Max(0, building.Level)}";
            var actions = BuildActions(building, definition);
            _contextMenu.Show(title, actions, OnContextAction);
            if (TryGetWorldAnchor(building, out var anchor) && _camera != null)
            {
                _contextMenu.Reposition(
                    _panelRoot,
                    _camera,
                    anchor,
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
                    new(BuildingContextAction.Details, "Detalhes", true),
                    UpgradeAction(building, definition)
                };
            }

            if (string.Equals(id, "farm", StringComparison.Ordinal))
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

            // Demais edifícios: Detalhes + Atualizar (sem menu central).
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

            if (string.Equals(id, "dragon-tower", StringComparison.Ordinal))
            {
                list.Add(new BuildingContextActionInfo(BuildingContextAction.Open, "Abrir", true));
                list.Add(new BuildingContextActionInfo(BuildingContextAction.Send, "Enviar", true));
            }

            return list;
        }

        private BuildingContextActionInfo UpgradeAction(BuildingInstance building, BuildingDefinition definition)
        {
            var upgradeLabel = building.State == BuildingState.Available && building.Level <= 0
                ? "Construir"
                : "Atualizar";
            var canUpgrade = _city.CanUpgrade(building, definition) &&
                             string.IsNullOrEmpty(_city.GetUpgradeBlockReason(building, definition));
            // Ainda mostra o botão se só faltar recurso — painel explica.
            var softEnable = building.State != BuildingState.Upgrading &&
                             building.CanUpgrade(definition) &&
                             (_city.GetActiveConstructionCount() < CityController.ConstructionQueueSlots ||
                              building.State == BuildingState.Upgrading);
            return new BuildingContextActionInfo(
                BuildingContextAction.Upgrade,
                upgradeLabel,
                softEnable || canUpgrade,
                _city.GetUpgradeBlockReason(building, definition));
        }

        private void OnContextAction(BuildingContextAction action)
        {
            if (_current == null)
            {
                return;
            }

            // Impede que o mesmo release do mouse selecione o prédio de novo e feche o painel.
            CityBuildingPointerInput.SuppressWorldClicks(0.35f);
            _ignoreOutsideClickUntil = Time.unscaledTime + 0.35f;

            switch (action)
            {
                case BuildingContextAction.Collect:
                    ExecuteCollect();
                    break;
                case BuildingContextAction.Send:
                    ExecuteSend();
                    break;
                case BuildingContextAction.Details:
                case BuildingContextAction.Open:
                case BuildingContextAction.Upgrade:
                default:
                    _openPanelAction = action;
                    OpenActionPanel(action);
                    break;
            }
        }

        private void ExecuteCollect()
        {
            var amount = _city.CollectSelected();
            _feedback.text = amount > 0 ? $"+{amount} coletado!" : "Nada para coletar agora.";
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
                _actionPanel.style.display = DisplayStyle.None;
                var model = BuildingDetailsViewModel.From(
                    _city,
                    _current,
                    definition,
                    openMode: action == BuildingContextAction.Open);
                _detailsPanel.Show(model, HideActionPanel);
                BetaJourneyGuide.NotifyCityModalOpen(true, panelOnRight: true);
                Debug.Log($"[Valgor] Painel Detalhes aberto: {definition.DisplayName}");
                return;
            }

            _detailsPanel.Hide();
            _actionTitle.text = ActionTitle(action, definition);

            switch (action)
            {
                case BuildingContextAction.Upgrade:
                    RebuildUpgradeBody(_current);
                    var canConfirmUpgrade = _city.CanUpgrade(_current, definition) &&
                                            string.IsNullOrEmpty(_city.GetUpgradeBlockReason(_current, definition));
                    AddPanelButton(
                        _current.State == BuildingState.Available && _current.Level <= 0 ? "Construir" : "Atualizar",
                        ExecuteUpgrade,
                        enabled: canConfirmUpgrade);
                    AddPanelButton("Concluir Agora", ExecuteInstantComplete);
                    AddPanelButton("Fechar", HideActionPanel);
                    break;
                default:
                    AppendBodyText(BuildPanelBodyLegacy(action, _current, definition));
                    AddPanelButton("Fechar", HideActionPanel);
                    break;
            }

            _actionPanel.style.display = DisplayStyle.Flex;
            _actionPanel.style.visibility = Visibility.Visible;
            _actionPanel.style.opacity = 1f;
            _actionPanel.BringToFront();
            BetaJourneyGuide.NotifyCityModalOpen(true, panelOnRight: true);
        }

        private void RebuildUpgradeBody(BuildingInstance building)
        {
            if (_openPanelAction != BuildingContextAction.Upgrade)
            {
                return;
            }

            _actionBodyHost.Clear();
            var definition = _city.GetDefinition(building);
            var next = Math.Min(definition.MaxLevel, building.Level + 1);
            var duration = definition.GetUpgradeDuration(building.Level);

            AppendBodyText(
                $"{definition.DisplayName}\n" +
                $"Nível atual: {building.Level}\n" +
                $"Próximo nível: {next}\n" +
                $"Benefício: {DescribeUpgradeBenefit(building, definition)}\n" +
                $"Duração: {(int)duration.TotalSeconds}s\n" +
                (building.State == BuildingState.Upgrading && building.UpgradeCompletesAtUtc.HasValue
                    ? $"Em andamento — resta {FormatRemaining(building.UpgradeCompletesAtUtc.Value)}\n"
                    : string.Empty) +
                $"Construtor: {_city.GetActiveConstructionCount()}/{CityController.ConstructionQueueSlots}\n" +
                "Pré-requisitos:");

            foreach (var dep in _city.GetDependencyChecks(building))
            {
                _actionBodyHost.Add(BuildDependencyRow(dep));
            }

            AppendBodyText("Recursos:");

            foreach (var req in _city.GetUpgradeRequirements(building))
            {
                _actionBodyHost.Add(BuildRequirementRow(req));
            }

            var diamonds = BuildingUpgradeRequirements.InstantCompleteDiamondCost(
                building.State == BuildingState.Upgrading && building.UpgradeCompletesAtUtc.HasValue
                    ? building.UpgradeCompletesAtUtc.Value - _city.Economy.Clock.UtcNow
                    : duration);
            AppendBodyText($"\nConcluir Agora: {diamonds} diamante(s) · saldo {_city.Economy.Wallet.Get(ResourceType.Diamonds)}");
        }

        private VisualElement BuildDependencyRow(BuildingDependencyCheck check)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginTop = 3;
            row.style.paddingLeft = 6;
            row.style.paddingRight = 6;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 0.85f);

            var name = new Label(check.Label);
            name.style.color = BetaVisualTheme.TextPrimary;
            name.style.fontSize = 12;
            name.style.flexGrow = 1;
            name.style.flexShrink = 1;

            var detail = new Label(check.Detail);
            detail.style.fontSize = 11;
            detail.style.color = check.Satisfied
                ? new Color(0.45f, 0.85f, 0.5f)
                : new Color(0.9f, 0.35f, 0.32f);
            detail.style.unityTextAlign = TextAnchor.MiddleRight;
            detail.style.flexGrow = 1;
            detail.style.flexShrink = 1;

            var mark = new Label(check.Satisfied ? "✓" : "✗");
            mark.style.fontSize = 13;
            mark.style.marginLeft = 8;
            mark.style.color = detail.style.color;
            mark.style.unityFontStyleAndWeight = FontStyle.Bold;

            row.Add(name);
            row.Add(detail);
            row.Add(mark);

            if (!check.Satisfied && !string.IsNullOrEmpty(check.JumpToDefinitionId))
            {
                var targetId = check.JumpToDefinitionId!;
                var go = new Button(() => GoToRequirementBuilding(targetId)) { text = "Ir" };
                go.style.marginLeft = 8;
                go.style.paddingLeft = 10;
                go.style.paddingRight = 10;
                go.style.paddingTop = 4;
                go.style.paddingBottom = 4;
                go.style.fontSize = 12;
                go.style.backgroundColor = BetaVisualTheme.ButtonFace;
                go.style.color = BetaVisualTheme.TextPrimary;
                go.style.borderTopWidth = 1;
                go.style.borderBottomWidth = 1;
                go.style.borderLeftWidth = 1;
                go.style.borderRightWidth = 1;
                go.style.borderTopColor = BetaVisualTheme.ButtonBorder;
                go.style.borderBottomColor = BetaVisualTheme.ButtonBorder;
                go.style.borderLeftColor = BetaVisualTheme.ButtonBorder;
                go.style.borderRightColor = BetaVisualTheme.ButtonBorder;
                row.Add(go);
            }

            return row;
        }

        private void GoToRequirementBuilding(string definitionId)
        {
            CityBuildingPointerInput.SuppressWorldClicks(0.35f);
            _ignoreOutsideClickUntil = Time.unscaledTime + 0.35f;

            if (!_city.TryGetBuildingByDefinitionId(definitionId, out var target))
            {
                _feedback.text = "Edifício exigido não encontrado na cidade.";
                return;
            }

            HideActionPanel();
            _openPanelAction = null;
            _city.Selection.Select(target);
        }

        private static VisualElement BuildRequirementRow(UpgradeResourceRequirement req)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginTop = 3;
            row.style.paddingLeft = 6;
            row.style.paddingRight = 6;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 0.85f);

            var name = new Label(FriendlyResource(req.Resource));
            name.style.color = BetaVisualTheme.TextPrimary;
            name.style.fontSize = 12;
            name.style.flexGrow = 1;

            var amounts = new Label($"{req.Available} / {req.Required}");
            amounts.style.fontSize = 12;
            amounts.style.color = req.Satisfied
                ? new Color(0.45f, 0.85f, 0.5f)
                : new Color(0.9f, 0.35f, 0.32f);
            amounts.style.unityTextAlign = TextAnchor.MiddleRight;
            amounts.style.minWidth = 90;

            var mark = new Label(req.Satisfied ? "✓" : "✗");
            mark.style.fontSize = 13;
            mark.style.marginLeft = 8;
            mark.style.color = amounts.style.color;
            mark.style.unityFontStyleAndWeight = FontStyle.Bold;

            row.Add(name);
            row.Add(amounts);
            row.Add(mark);
            return row;
        }

        private string BuildDetailsBody(BuildingInstance building, BuildingDefinition definition, bool openMode)
        {
            var sb = new StringBuilder();
            sb.AppendLine(definition.DisplayName);
            sb.AppendLine($"Nível: {building.Level}/{definition.MaxLevel}");
            sb.AppendLine($"Estado: {FriendlyState(building.State)}");

            if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal))
            {
                sb.AppendLine("Função: coração da cidade — limita o nível dos demais edifícios.");
                sb.AppendLine($"Bônus atuais: Castelo Nv.{building.Level} (teto de upgrade).");
                sb.AppendLine($"Próximo nível: {Math.Min(definition.MaxLevel, building.Level + 1)}");
                sb.AppendLine($"Duração upgrade: {(int)definition.GetUpgradeDuration(building.Level).TotalSeconds}s");
                sb.AppendLine(DescribeRequirementsShort(building));
            }
            else if (string.Equals(building.DefinitionId, "farm", StringComparison.Ordinal))
            {
                sb.AppendLine(BuildProductionBlock(building));
                sb.AppendLine($"Próximo benefício: +produção de comida no Nv.{building.Level + 1}");
            }
            else if (string.Equals(building.DefinitionId, "warehouse", StringComparison.Ordinal))
            {
                sb.AppendLine($"Capacidade: {WarehouseRules.GetCapacity(building.Level):N0}");
                sb.AppendLine($"Proteção de recursos: {WarehouseRules.GetProtection(building.Level):N0}");
                sb.AppendLine(
                    $"Próximo benefício: capacidade {WarehouseRules.GetNextCapacity(building.Level):N0} · " +
                    $"proteção {WarehouseRules.GetNextProtection(building.Level):N0}");
                sb.AppendLine(DescribeRequirementsShort(building));
                if (openMode)
                {
                    sb.AppendLine();
                    sb.AppendLine("Armazém aberto — estoque e proteção da cidade.");
                }
            }
            else
            {
                sb.AppendLine(BuildProductionBlock(building));
            }

            if (building.State == BuildingState.Upgrading && building.UpgradeCompletesAtUtc.HasValue)
            {
                sb.AppendLine($"Melhorando → Nv.{building.Level + 1} ({FormatRemaining(building.UpgradeCompletesAtUtc.Value)})");
            }

            var block = _city.GetUpgradeBlockReason(building, definition);
            if (!string.IsNullOrEmpty(block))
            {
                sb.AppendLine(block);
            }

            if (!string.IsNullOrEmpty(_city.LastUpgradeFeedback))
            {
                sb.AppendLine(_city.LastUpgradeFeedback);
            }

            return sb.ToString().Trim();
        }

        private string DescribeRequirementsShort(BuildingInstance building)
        {
            var sb = new StringBuilder("Requisitos: ");
            var first = true;
            foreach (var req in _city.GetUpgradeRequirements(building))
            {
                if (req.Required <= 0)
                {
                    continue;
                }

                if (!first) sb.Append(", ");
                sb.Append($"{FriendlyResource(req.Resource)} {req.Required}");
                first = false;
            }

            return first ? "Requisitos: —" : sb.ToString();
        }

        private static string DescribeUpgradeBenefit(BuildingInstance building, BuildingDefinition definition)
        {
            if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal))
            {
                return $"Eleva o teto da cidade para Nv.{building.Level + 1}";
            }

            if (string.Equals(building.DefinitionId, "farm", StringComparison.Ordinal))
            {
                return "Aumenta taxa e capacidade de comida";
            }

            if (string.Equals(building.DefinitionId, "warehouse", StringComparison.Ordinal))
            {
                return
                    $"Capacidade {WarehouseRules.GetNextCapacity(building.Level):N0} · " +
                    $"proteção {WarehouseRules.GetNextProtection(building.Level):N0}";
            }

            return $"Melhora {definition.DisplayName}";
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
                RebuildUpgradeBody(building);
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
                (_actionPanel.style.display == DisplayStyle.Flex && IsScreenOverElement(_actionPanel, pos)))
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var hit, 500f))
            {
                if (hit.collider != null &&
                    (hit.collider.GetComponentInParent<BuildingView>() != null ||
                     hit.collider.GetComponentInParent<BuildingCollectableClickProxy>() != null))
                {
                    return;
                }
            }

            _city.Selection.Deselect();
        }

        private bool TryGetWorldAnchor(BuildingInstance building, out Vector3 anchor)
        {
            if (_city.TryGetView(building, out var view))
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

        private string BuildProductionBlock(BuildingInstance building)
        {
            if (!ProductionCatalog.TryGet(building.DefinitionId, out var productionDef))
            {
                return string.Empty;
            }

            var rate = _city.Economy.Production.GetRatePerHour(building);
            var capacity = _city.Economy.Production.GetCapacity(building);
            _city.Economy.Production.TryGetState(building.DefinitionId, out var state);
            var accumulated = state?.Accumulated ?? 0;
            return $"Produção: {rate:0.#}/h · Acumulado {accumulated}/{capacity} ({FriendlyResource(productionDef.Resource)})";
        }

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
                BuildingContextAction.Open => $"Abrir — {definition.DisplayName}",
                BuildingContextAction.Collect => $"Coletar — {definition.DisplayName}",
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
