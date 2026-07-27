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
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Orquestra seleção → câmera → menu contextual → painel de ação.
    /// </summary>
    public sealed class BuildingSelectionPresenter
    {
        private readonly CityController _city;
        private readonly IDragonGateway? _dragons;
        private readonly VisualElement _panelRoot;
        private readonly BuildingContextMenu _contextMenu;
        private readonly VisualElement _actionPanel;
        private readonly Label _actionTitle;
        private readonly Label _actionBody;
        private readonly VisualElement _actionButtons;
        private readonly Label _feedback;
        private readonly Action? _goToWorldMap;
        private CityCameraController? _cameraController;
        private Camera? _camera;
        private BuildingInstance? _current;
        private BuildingContextAction? _openPanelAction;

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
            _actionPanel = BuildActionPanel(out _actionTitle, out _actionBody, out _actionButtons, out _feedback);
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
                    _contextMenu.Reposition(_panelRoot, _camera, anchor);
                }
            }

            HandleOutsideClick();
        }

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

        public void Dispose()
        {
            _city.Selection.SelectionChanged -= OnSelectionChanged;
            _city.BuildingChanged -= RefreshCurrent;
        }

        private void OnSelectionChanged(BuildingInstance? selected)
        {
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
                _contextMenu.Reposition(_panelRoot, _camera, anchor);
            }
        }

        private List<BuildingContextActionInfo> BuildActions(
            BuildingInstance building,
            BuildingDefinition definition)
        {
            var list = new List<BuildingContextActionInfo>(8)
            {
                new(BuildingContextAction.Details, "Detalhes", true)
            };

            var upgradeLabel = building.State == BuildingState.Available && building.Level <= 0
                ? "Construir"
                : "Atualizar";
            var canUpgrade = _city.CanUpgrade(building, definition);
            var upgradeBlock = _city.GetUpgradeBlockReason(building, definition);
            list.Add(new BuildingContextActionInfo(
                BuildingContextAction.Upgrade,
                upgradeLabel,
                canUpgrade,
                upgradeBlock));

            var hasProduction = ProductionCatalog.TryGet(building.DefinitionId, out _);
            if (hasProduction)
            {
                var canCollect = _city.Economy.Production.TryGetState(building.DefinitionId, out var state) &&
                                 state.HasCollectable;
                list.Add(new BuildingContextActionInfo(
                    BuildingContextAction.Collect,
                    "Coletar",
                    canCollect,
                    canCollect ? null : "Nada acumulado ainda."));
                list.Add(new BuildingContextActionInfo(BuildingContextAction.Produce, "Produzir", true));
            }

            if (string.Equals(building.DefinitionId, "arena", StringComparison.Ordinal))
            {
                list.Add(new BuildingContextActionInfo(BuildingContextAction.Train, "Treinar", true));
            }

            if (string.Equals(building.DefinitionId, "laboratory", StringComparison.Ordinal) ||
                string.Equals(building.DefinitionId, "academy", StringComparison.Ordinal))
            {
                list.Add(new BuildingContextActionInfo(
                    BuildingContextAction.Research,
                    "Pesquisar",
                    building.Level >= 1 || string.Equals(building.DefinitionId, "laboratory", StringComparison.Ordinal),
                    "Melhore o edifício para pesquisar."));
            }

            if (string.Equals(building.DefinitionId, "dragon-tower", StringComparison.Ordinal))
            {
                list.Add(new BuildingContextActionInfo(BuildingContextAction.Open, "Abrir", true));
                list.Add(new BuildingContextActionInfo(BuildingContextAction.Send, "Enviar", true));
            }

            return list;
        }

        private void OnContextAction(BuildingContextAction action)
        {
            if (_current == null)
            {
                return;
            }

            switch (action)
            {
                case BuildingContextAction.Collect:
                    ExecuteCollect();
                    break;
                case BuildingContextAction.Send:
                    ExecuteSend();
                    break;
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
            ShowTransientFeedback();
            RefreshCurrent();
        }

        private void ExecuteSend()
        {
            _city.Persist();
            if (_goToWorldMap == null)
            {
                _feedback.text = "Mapa indisponível.";
                ShowTransientFeedback();
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
            _actionTitle.text = ActionTitle(action, definition);
            _actionBody.text = BuildPanelBody(action, _current, definition);
            _feedback.text = string.Empty;

            switch (action)
            {
                case BuildingContextAction.Details:
                    AddPanelButton("Fechar", HideActionPanel);
                    break;
                case BuildingContextAction.Upgrade:
                    AddPanelButton(
                        _current.State == BuildingState.Available && _current.Level <= 0 ? "Construir" : "Confirmar atualização",
                        ExecuteUpgrade);
                    AddPanelButton("Voltar", HideActionPanel);
                    break;
                case BuildingContextAction.Produce:
                    AddPanelButton("Coletar agora", ExecuteCollect);
                    AddPanelButton("Voltar", HideActionPanel);
                    break;
                case BuildingContextAction.Train:
                    AddPanelButton("Entendi", HideActionPanel);
                    break;
                case BuildingContextAction.Research:
                    AddPanelButton("Voltar", HideActionPanel);
                    if (string.Equals(_current.DefinitionId, "laboratory", StringComparison.Ordinal) &&
                        _city.CanUpgrade(_current, definition))
                    {
                        AddPanelButton("Melhorar laboratório", ExecuteUpgrade);
                    }

                    break;
                case BuildingContextAction.Open:
                    AddPanelButton("Alimentar", ExecuteFeed);
                    AddPanelButton("Chocar ovo", ExecuteHatch);
                    AddPanelButton("Evoluir", ExecuteEvolve);
                    AddPanelButton("Voltar", HideActionPanel);
                    break;
                default:
                    AddPanelButton("Voltar", HideActionPanel);
                    break;
            }

            _actionPanel.style.display = DisplayStyle.Flex;
        }

        private string BuildPanelBody(
            BuildingContextAction action,
            BuildingInstance building,
            BuildingDefinition definition)
        {
            var sb = new StringBuilder();
            switch (action)
            {
                case BuildingContextAction.Details:
                    sb.AppendLine(definition.DisplayName);
                    sb.AppendLine($"Nível {building.Level}/{definition.MaxLevel}");
                    sb.AppendLine($"Estado: {FriendlyState(building.State)}");
                    if (building.State == BuildingState.Upgrading && building.UpgradeCompletesAtUtc.HasValue)
                    {
                        var remaining = building.UpgradeCompletesAtUtc.Value - _city.Economy.Clock.UtcNow;
                        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
                        sb.AppendLine($"Melhorando → Nv.{building.Level + 1} ({(int)remaining.TotalSeconds}s)");
                    }

                    var block = _city.GetUpgradeBlockReason(building, definition);
                    if (!string.IsNullOrEmpty(block))
                    {
                        sb.AppendLine(block);
                    }

                    sb.AppendLine(BuildProductionBlock(building));
                    if (string.Equals(building.DefinitionId, "dragon-tower", StringComparison.Ordinal) &&
                        _dragons != null)
                    {
                        sb.AppendLine($"Ninho: {_dragons.RoostOccupantCount}/{_dragons.RoostCapacity}");
                    }

                    break;
                case BuildingContextAction.Upgrade:
                    sb.AppendLine($"{definition.DisplayName}");
                    sb.AppendLine($"Nv.{building.Level} → Nv.{Math.Min(definition.MaxLevel, building.Level + 1)}");
                    var reason = _city.GetUpgradeBlockReason(building, definition);
                    sb.AppendLine(string.IsNullOrEmpty(reason)
                        ? $"Tempo: {(int)definition.GetUpgradeDuration(building.Level).TotalSeconds}s"
                        : reason);
                    break;
                case BuildingContextAction.Produce:
                    sb.AppendLine(BuildProductionBlock(building));
                    if (string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        sb.Append("Este edifício não produz recursos passivos.");
                    }

                    break;
                case BuildingContextAction.Train:
                    sb.Append("Treino de unidades estará disponível em breve neste edifício.");
                    break;
                case BuildingContextAction.Research:
                    sb.AppendLine(BetaProgress.Describe());
                    sb.AppendLine(string.Equals(building.DefinitionId, "laboratory", StringComparison.Ordinal)
                        ? "Melhore o Laboratório para desbloquear Coleta +."
                        : "A Academia prepara pesquisas futuras.");
                    break;
                case BuildingContextAction.Open:
                    sb.AppendLine("Torre dos Dragões");
                    if (_dragons != null)
                    {
                        sb.AppendLine($"Ninho: {_dragons.RoostOccupantCount}/{_dragons.RoostCapacity}");
                        foreach (var status in _dragons.GetDragonStatuses())
                        {
                            sb.AppendLine($"· {status.DisplayName}: {status.StateLabel}");
                        }
                    }

                    break;
            }

            return sb.ToString().Trim();
        }

        private void ExecuteUpgrade()
        {
            var building = _city.Selection.Selected;
            if (building == null)
            {
                return;
            }

            var definition = _city.GetDefinition(building);
            if (_city.TryUpgradeSelected())
            {
                var duration = definition.GetUpgradeDuration(building.Level);
                _feedback.text = building.State == BuildingState.Upgrading
                    ? $"{definition.DisplayName}: melhoria iniciada ({(int)duration.TotalSeconds}s)"
                    : $"{definition.DisplayName} → Nv.{building.Level}";
            }
            else
            {
                _feedback.text = _city.GetUpgradeBlockReason(building, definition) ?? "Não foi possível atualizar.";
            }

            RefreshCurrent();
        }

        private void ExecuteFeed()
        {
            if (_dragons == null)
            {
                _feedback.text = "Dragões indisponíveis.";
                return;
            }

            foreach (var status in _dragons.GetDragonStatuses())
            {
                if (status.StateLabel is "HUNGRY" or "RESTING" or "READY" or "JUVENILE")
                {
                    if (_dragons.TryFeed(status.DragonId, out var error))
                    {
                        _feedback.text = "Dragão alimentado.";
                        BetaJourneyGuide.NotifyDragonFed();
                    }
                    else
                    {
                        _feedback.text = error;
                    }

                    RefreshCurrent();
                    return;
                }
            }

            _feedback.text = "Nenhum dragão disponível para alimentar.";
        }

        private void ExecuteHatch()
        {
            if (_dragons == null)
            {
                return;
            }

            _feedback.text = _dragons.TryUnlockAndHatch("ash-drake", out var error)
                ? "Incubação iniciada."
                : error;
            RefreshCurrent();
        }

        private void ExecuteEvolve()
        {
            if (_dragons == null)
            {
                return;
            }

            foreach (var status in _dragons.GetDragonStatuses())
            {
                if (_dragons.TryEvolve(status.DragonId, out var error))
                {
                    _feedback.text = $"{status.DisplayName} evoluiu!";
                    RefreshCurrent();
                    return;
                }

                if (!string.Equals(error, "Esta espécie não possui evolução.", StringComparison.Ordinal) &&
                    !string.Equals(error, "Evolução indisponível neste estado.", StringComparison.Ordinal))
                {
                    _feedback.text = error;
                    return;
                }
            }

            _feedback.text = "Nenhum dragão elegível para evoluir.";
        }

        private void HandleOutsideClick()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasReleasedThisFrame)
            {
                return;
            }

            EnsureCamera();
            if (_camera == null)
            {
                return;
            }

            // Clique em UI do menu/painel — não fecha.
            var pos = mouse.position.ReadValue();
            if (IsScreenOverElement(_contextMenu.Root, pos) ||
                (_actionPanel.style.display == DisplayStyle.Flex && IsScreenOverElement(_actionPanel, pos)))
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var hit, 500f))
            {
                if (hit.collider != null && hit.collider.GetComponentInParent<BuildingView>() != null)
                {
                    return; // outro/mesmo edifício — seleção cuida
                }
            }

            _city.Selection.Deselect();
        }

        private bool TryGetWorldAnchor(BuildingInstance building, out Vector3 anchor)
        {
            if (_city.TryGetView(building, out var view))
            {
                anchor = view.transform.position + Vector3.up * 2.4f;
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
                    ? _cameraController.GetComponent<Camera>()
                    : Camera.main;
            }
        }

        private void ResolveCamera() => EnsureCamera();

        private void HideActionPanel()
        {
            _openPanelAction = null;
            _actionPanel.style.display = DisplayStyle.None;
            _actionButtons.Clear();
        }

        private void ShowTransientFeedback()
        {
            if (_actionPanel.style.display != DisplayStyle.Flex)
            {
                _openPanelAction = BuildingContextAction.Details;
                OpenActionPanel(BuildingContextAction.Details);
            }
        }

        private void AddPanelButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginTop = 8;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.paddingTop = 8;
            button.style.paddingBottom = 8;
            button.style.backgroundColor = BetaVisualTheme.ButtonFace;
            button.style.color = BetaVisualTheme.TextPrimary;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = BetaVisualTheme.ButtonBorder;
            button.style.borderBottomColor = BetaVisualTheme.ButtonBorder;
            button.style.borderLeftColor = BetaVisualTheme.ButtonBorder;
            button.style.borderRightColor = BetaVisualTheme.ButtonBorder;
            button.style.fontSize = 13;
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

        private static string ActionTitle(BuildingContextAction action, BuildingDefinition definition) =>
            action switch
            {
                BuildingContextAction.Details => $"Detalhes — {definition.DisplayName}",
                BuildingContextAction.Upgrade => $"Atualizar — {definition.DisplayName}",
                BuildingContextAction.Produce => $"Produção — {definition.DisplayName}",
                BuildingContextAction.Train => $"Treinar — {definition.DisplayName}",
                BuildingContextAction.Research => $"Pesquisar — {definition.DisplayName}",
                BuildingContextAction.Open => $"Abrir — {definition.DisplayName}",
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
            ResourceType.DragonEssence => "Essência",
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
            out Label body,
            out VisualElement buttons,
            out Label feedback)
        {
            var panel = new VisualElement { name = "building-action-panel" };
            panel.style.position = Position.Absolute;
            panel.style.right = 16;
            panel.style.top = 64;
            panel.style.width = 320;
            panel.style.maxHeight = 420;
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
            panel.pickingMode = PickingMode.Position;

            title = new Label();
            title.style.color = BetaVisualTheme.AgedGoldBright;
            title.style.fontSize = 15;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            panel.Add(title);

            body = new Label();
            body.style.color = BetaVisualTheme.TextPrimary;
            body.style.fontSize = 13;
            body.style.whiteSpace = WhiteSpace.Normal;
            panel.Add(body);

            feedback = new Label();
            feedback.style.color = BetaVisualTheme.AgedGoldBright;
            feedback.style.marginTop = 8;
            feedback.style.whiteSpace = WhiteSpace.Normal;
            panel.Add(feedback);

            buttons = new VisualElement { name = "action-panel-buttons" };
            panel.Add(buttons);
            return panel;
        }
    }
}
