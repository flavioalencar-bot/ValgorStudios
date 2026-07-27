using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Filters;
using Valgor.WorldMap.Marches;
using Valgor.WorldMap.Territory;
using Valgor.UI;
using Valgor.Core;

namespace Valgor.WorldMap.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class WorldMapHudController : MonoBehaviour
    {
        private UIDocument _document = null!;
        private WorldMapController _map = null!;
        private CityEconomy? _economy;
        private WorldMapFilterPanel? _filterPanel;
        private Label _title = null!;
        private Label _power = null!;
        private Label _wallet = null!;
        private Label _march = null!;
        private Label _territory = null!;
        private VisualElement _panel = null!;
        private Label _details = null!;
        private Button _dispatchButton = null!;
        private Button _collectButton = null!;
        private Button _engageButton = null!;
        private Button _resolveButton = null!;
        private Button _returnButton = null!;
        private Button _cancelButton = null!;
        private Label _feedback = null!;
        private VisualElement _encounterPanel = null!;
        private Label _encounterTitle = null!;
        private Label _encounterBody = null!;
        private Label _encounterResult = null!;

        public void Initialize(WorldMapController map, CityEconomy? economy)
        {
            _map = map;
            _economy = economy;
            _document = GetComponent<UIDocument>();
            EnsurePanelSettings();
            Build();
            _map.Session.Selection.SelectionChanged += _ => Refresh();
            _map.Session.RegionSelection.SelectionChanged += _ => Refresh();
            _map.Changed += Refresh;
            _map.Session.EnergyWallet.Changed += (_, _) => RefreshWallet();
            if (_economy != null)
            {
                _economy.Wallet.Changed += (_, _) =>
                {
                    _economy.PersistWallet();
                    RefreshWallet();
                };
            }
        }

        private void EnsurePanelSettings()
        {
            BetaUiPanels.ApplyTo(_document);
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;

            // Barra superior compacta (sem painéis técnicos densos).
            var top = new VisualElement();
            top.style.position = Position.Absolute;
            top.style.left = 10;
            top.style.right = 10;
            top.style.top = 6;
            top.style.height = 42;
            top.style.paddingLeft = 12;
            top.style.paddingRight = 12;
            top.style.paddingTop = 8;
            top.style.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 0.9f);
            top.style.borderBottomWidth = 2;
            top.style.borderBottomColor = BetaVisualTheme.AgedGold;
            root.Add(top);

            _title = new Label("Mapa Mundial");
            _title.style.fontSize = 14;
            _title.style.color = BetaVisualTheme.AgedGoldBright;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            top.Add(_title);

            _wallet = new Label();
            _wallet.style.position = Position.Absolute;
            _wallet.style.left = 12;
            _wallet.style.top = 22;
            _wallet.style.fontSize = 12;
            _wallet.style.color = BetaVisualTheme.TextPrimary;
            top.Add(_wallet);

            _power = new Label();
            _power.style.display = DisplayStyle.None;
            root.Add(_power);

            _march = new Label();
            _march.style.position = Position.Absolute;
            _march.style.left = 14;
            _march.style.top = 52;
            _march.style.fontSize = 11;
            _march.style.color = new Color(0.75f, 0.88f, 0.95f);
            _march.style.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 0.75f);
            _march.style.paddingLeft = 8;
            _march.style.paddingRight = 8;
            _march.style.paddingTop = 4;
            _march.style.paddingBottom = 4;
            root.Add(_march);

            _territory = new Label();
            _territory.style.position = Position.Absolute;
            _territory.style.left = 14;
            _territory.style.top = 78;
            _territory.style.fontSize = 11;
            _territory.style.color = new Color(0.9f, 0.85f, 0.7f);
            _territory.style.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 0.75f);
            _territory.style.paddingLeft = 8;
            _territory.style.paddingRight = 8;
            _territory.style.paddingTop = 4;
            _territory.style.paddingBottom = 4;
            root.Add(_territory);

            _filterPanel = new WorldMapFilterPanel(_map.Session.Filters, root);

            // Localizar compacto sob filtros.
            var locate = new VisualElement();
            locate.style.position = Position.Absolute;
            locate.style.right = 14;
            locate.style.top = 100;
            locate.style.width = 188;
            locate.style.paddingLeft = 8;
            locate.style.paddingRight = 8;
            locate.style.paddingTop = 6;
            locate.style.paddingBottom = 6;
            locate.style.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 0.9f);
            root.Add(locate);

            var locateTitle = new Label("Localizar");
            locateTitle.style.color = Color.white;
            locateTitle.style.fontSize = 12;
            locateTitle.style.marginBottom = 2;
            locate.Add(locateTitle);
            locate.Add(CreateCompactButton("Cidade", OnLocateHome));
            locate.Add(CreateCompactButton("Marcha", OnLocateMarch));
            locate.Add(CreateCompactButton("Selecionado", OnLocateSelected));

            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.left = 14;
            _panel.style.bottom = 78;
            _panel.style.width = 300;
            _panel.style.maxHeight = 340;
            _panel.style.paddingLeft = 12;
            _panel.style.paddingRight = 12;
            _panel.style.paddingTop = 10;
            _panel.style.paddingBottom = 10;
            _panel.style.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 0.94f);
            _panel.style.borderTopWidth = 2;
            _panel.style.borderBottomWidth = 2;
            _panel.style.borderLeftWidth = 2;
            _panel.style.borderRightWidth = 2;
            _panel.style.borderTopColor = BetaVisualTheme.AgedGold;
            _panel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _panel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _panel.style.borderRightColor = BetaVisualTheme.AgedGold;
            _details = new Label();
            _details.style.color = Color.white;
            _details.style.whiteSpace = WhiteSpace.Normal;
            _details.style.fontSize = 12;
            _panel.Add(_details);

            _dispatchButton = CreateButton("Enviar marcha", OnDispatch);
            _collectButton = CreateButton("Coletar", OnCollect);
            _engageButton = CreateButton("Engajar", OnEngage);
            _resolveButton = CreateButton("Resolver", OnResolve);
            _returnButton = CreateButton("Retornar", OnReturn);
            _cancelButton = CreateButton("Cancelar marcha", OnCancel);
            _panel.Add(_dispatchButton);
            _panel.Add(_collectButton);
            _panel.Add(_engageButton);
            _panel.Add(_resolveButton);
            _panel.Add(_returnButton);
            _panel.Add(_cancelButton);
            _panel.Add(CreateButton("Fechar", () => _map.Session.Selection.Deselect()));

            _feedback = new Label();
            _feedback.style.color = BetaVisualTheme.AgedGoldBright;
            _feedback.style.marginTop = 6;
            _feedback.style.whiteSpace = WhiteSpace.Normal;
            _feedback.style.fontSize = 11;
            _panel.Add(_feedback);

            root.Add(_panel);
            BuildEncounterPanel(root);
            BetaJourneyGuide.NotifyWorldMapOpened();
            BetaJourneyGuide.AttachOrRefresh(root);
            Refresh();
        }

        private void BuildEncounterPanel(VisualElement root)
        {
            _encounterPanel = new VisualElement();
            _encounterPanel.style.position = Position.Absolute;
            _encounterPanel.style.right = 14;
            _encounterPanel.style.bottom = 78;
            _encounterPanel.style.width = 260;
            _encounterPanel.style.paddingLeft = 14;
            _encounterPanel.style.paddingRight = 14;
            _encounterPanel.style.paddingTop = 12;
            _encounterPanel.style.paddingBottom = 12;
            _encounterPanel.style.backgroundColor = new Color(0.12f, 0.06f, 0.08f, 0.94f);
            _encounterPanel.style.borderTopWidth = 2;
            _encounterPanel.style.borderBottomWidth = 2;
            _encounterPanel.style.borderLeftWidth = 2;
            _encounterPanel.style.borderRightWidth = 2;
            _encounterPanel.style.borderTopColor = new Color(0.85f, 0.45f, 0.25f);
            _encounterPanel.style.borderBottomColor = new Color(0.85f, 0.45f, 0.25f);
            _encounterPanel.style.borderLeftColor = new Color(0.85f, 0.45f, 0.25f);
            _encounterPanel.style.borderRightColor = new Color(0.85f, 0.45f, 0.25f);
            _encounterPanel.style.display = DisplayStyle.None;

            _encounterTitle = new Label("Encontro");
            _encounterTitle.style.color = new Color(1f, 0.82f, 0.45f);
            _encounterTitle.style.fontSize = 18;
            _encounterTitle.style.marginBottom = 6;
            _encounterPanel.Add(_encounterTitle);

            _encounterBody = new Label();
            _encounterBody.style.color = Color.white;
            _encounterBody.style.whiteSpace = WhiteSpace.Normal;
            _encounterBody.style.marginBottom = 8;
            _encounterPanel.Add(_encounterBody);

            _encounterResult = new Label();
            _encounterResult.style.color = new Color(0.7f, 0.95f, 0.75f);
            _encounterResult.style.whiteSpace = WhiteSpace.Normal;
            _encounterResult.style.marginBottom = 8;
            _encounterPanel.Add(_encounterResult);

            _encounterPanel.Add(CreateButton("Engajar", OnEngage));
            _encounterPanel.Add(CreateButton("Resolver combate", OnResolve));
            root.Add(_encounterPanel);
        }

        private Button CreateButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginTop = 8;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.paddingTop = 7;
            button.style.paddingBottom = 7;
            return button;
        }

        private static Button CreateCompactButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginTop = 4;
            button.style.paddingLeft = 8;
            button.style.paddingRight = 8;
            button.style.paddingTop = 4;
            button.style.paddingBottom = 4;
            return button;
        }

        private void OnLocateHome()
        {
            _feedback.text = _map.TryLocatePlayerHome(out var error) ? "Cidade localizada." : error;
            Refresh();
        }

        private void OnLocateMarch()
        {
            _feedback.text = _map.TryLocateActiveMarch(out var error) ? "Marcha localizada." : error;
            Refresh();
        }

        private void OnLocateSelected()
        {
            _feedback.text = _map.TryLocateSelectedNode(out var error) ? "Nó centralizado." : error;
            Refresh();
        }

        private void OnLocateCreature()
        {
            var selected = _map.Session.Selection.Selected;
            var id = selected != null && _map.Session.GetDefinition(selected.DefinitionId).Kind == WorldNodeKind.Creature
                ? selected.DefinitionId
                : _map.Session.Nodes.Keys.FirstOrDefault(nodeId =>
                    _map.Session.GetDefinition(nodeId).Kind == WorldNodeKind.Creature &&
                    _map.Session.IsNodeVisible(nodeId));

            if (string.IsNullOrEmpty(id))
            {
                _feedback.text = "Nenhuma criatura visível.";
                Refresh();
                return;
            }

            _feedback.text = _map.TryLocateCreature(id, out var error) ? "Criatura localizada." : error;
            Refresh();
        }

        private void OnLocateResource()
        {
            var selected = _map.Session.Selection.Selected;
            var id = selected != null && _map.Session.GetDefinition(selected.DefinitionId).Kind == WorldNodeKind.Resource
                ? selected.DefinitionId
                : _map.Session.Nodes.Keys.FirstOrDefault(nodeId =>
                    _map.Session.GetDefinition(nodeId).Kind == WorldNodeKind.Resource &&
                    _map.Session.IsNodeVisible(nodeId));

            if (string.IsNullOrEmpty(id))
            {
                _feedback.text = "Nenhum recurso visível.";
                Refresh();
                return;
            }

            _feedback.text = _map.TryLocateResource(id, out var error) ? "Recurso localizado." : error;
            Refresh();
        }

        private void OnDispatch()
        {
            if (_map.Session.TryDispatchToSelected(out var error))
            {
                var prefix = _map.Session.LastDispatchWasQueued ? "Marcha enfileirada." : "Marcha enviada.";
                _feedback.text = string.IsNullOrEmpty(_map.Session.LastDispatchDetail)
                    ? prefix
                    : $"{prefix} {_map.Session.LastDispatchDetail}";
                BetaJourneyGuide.NotifyMarchOrGatherAction();
                BetaJourneyGuide.AttachOrRefresh(_document.rootVisualElement);
            }
            else
            {
                _feedback.text = error;
            }

            Refresh();
        }

        private void OnCollect()
        {
            if (_map.Session.TryCollectSelected(_economy?.Wallet, out var error, out var collected))
            {
                _feedback.text = collected > 0
                    ? $"Coletando... +{collected} na carga."
                    : "Coleta iniciada.";
                BetaJourneyGuide.NotifyMarchOrGatherAction();
                BetaJourneyGuide.AttachOrRefresh(_document.rootVisualElement);
            }
            else
            {
                _feedback.text = error;
            }

            Refresh();
        }

        private void OnReturn()
        {
            if (_map.Session.TryReturnMarch(out var error))
            {
                _feedback.text = "Retorno iniciado.";
            }
            else
            {
                _feedback.text = error;
            }

            Refresh();
        }

        private void OnCancel()
        {
            if (_map.Session.TryCancelMarch(out var error))
            {
                _feedback.text = "Marcha cancelada.";
            }
            else
            {
                _feedback.text = error;
            }

            Refresh();
        }

        private void OnEngage()
        {
            if (_map.Session.TryEngageSelectedCreature(out var error))
            {
                _feedback.text = "Encontro iniciado — prepare o combate.";
                _encounterResult.text = "Em combate. Resolva quando estiver pronto.";
                _encounterResult.style.color = new Color(1f, 0.85f, 0.45f);
            }
            else
            {
                _feedback.text = error;
                _encounterResult.text = error;
                _encounterResult.style.color = new Color(1f, 0.45f, 0.4f);
            }

            Refresh();
        }

        private void OnResolve()
        {
            if (_map.Session.TryResolveSelectedCreature(_economy?.Wallet, out var error, out var band))
            {
                _economy?.PersistWallet();
                var power = _map.Session.GetAttackerPower();
                _feedback.text = $"Vitória ({band}). Poder {power}.";
                _encounterResult.text = $"Vitória — faixa {band}.\nFormação: {_map.Session.DescribeHeroFormation()}\nPoder total: {power}";
                _encounterResult.style.color = new Color(0.55f, 0.95f, 0.6f);
            }
            else
            {
                _feedback.text = error;
                _encounterResult.text = $"Derrota / bloqueio: {error}";
                _encounterResult.style.color = new Color(1f, 0.45f, 0.4f);
            }

            Refresh();
        }

        private void Refresh()
        {
            RefreshWallet();
            RefreshPower();
            RefreshMarch();
            RefreshTerritory();
            var deposit = _map.Session.ConsumeDepositMessage();
            if (!string.IsNullOrEmpty(deposit))
            {
                _feedback.text = deposit;
            }

            _filterPanel?.SyncFromState();

            var selected = _map.Session.Selection.Selected;
            _panel.style.display = selected == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (_encounterPanel != null)
            {
                _encounterPanel.style.display = DisplayStyle.None;
            }

            if (selected == null)
            {
                return;
            }

            var definition = _map.Session.GetDefinition(selected.DefinitionId);
            var travel = _map.Session.Marches.EstimateTravel(
                _map.Session.Settings.PlayerHomeNodeId,
                definition.Id);

            var builder = new StringBuilder();
            builder.AppendLine(definition.DisplayName);
            builder.AppendLine($"Tipo: {definition.Kind}");
            builder.AppendLine($"Status: {selected.Status}");
            builder.AppendLine(definition.Description);
            builder.AppendLine($"Região: {definition.RegionId}");
            builder.AppendLine($"Deslocamento estimado: {FormatDuration(travel)}");

            switch (definition)
            {
                case WorldCityNode city:
                    builder.AppendLine(city.IsPlayerHome ? "Base do jogador." : "Cidade do mundo.");
                    break;
                case WorldVillageNode village:
                    builder.AppendLine($"População: {village.Population}");
                    break;
                case WorldResourceNode resource:
                    builder.AppendLine($"Recurso: {resource.ResourceType}");
                    builder.AppendLine($"Nível: {resource.Level}");
                    builder.AppendLine($"Taxa: {resource.GetGatherRatePerHour():0.#}/h");
                    builder.AppendLine($"Acumulado: {selected.RemainingAmount} / {resource.MaxAmount}");
                    builder.AppendLine($"Estado recurso: {selected.ResourceState}");
                    if (selected.RespawnAt.HasValue)
                    {
                        var left = selected.RespawnAt.Value - _map.Session.Clock.UtcNow;
                        if (left < TimeSpan.Zero)
                        {
                            left = TimeSpan.Zero;
                        }

                        builder.AppendLine($"Respawn em: {FormatDuration(left)}");
                    }

                    if (_map.Session.Marches.Active != null &&
                        string.Equals(_map.Session.Marches.Active.TargetNodeId, selected.DefinitionId, StringComparison.Ordinal))
                    {
                        var march = _map.Session.Marches.Active;
                        builder.AppendLine($"Carga da marcha: {march.ResourceLoad} / {march.Capacity}");
                        var eta = ResourceGatherCalculator.EstimateTimeToFillOrDeplete(
                            resource.GetGatherRatePerHour(),
                            selected.RemainingAmount,
                            march.ResourceLoad,
                            march.Capacity);
                        if (eta.HasValue && march.State == MarchState.Gathering)
                        {
                            builder.AppendLine($"Tempo p/ encher/esgotar: {FormatDuration(eta.Value)}");
                        }
                    }

                    break;
                case WorldCreatureNode creature:
                    builder.AppendLine($"Código do nó: {creature.CreatureCode}");
                    if (_map.Session.TryGetCreature(creature.Id, out var instance) &&
                        WorldCreatureCatalog.TryGet(creature.Id, out var creatureDef))
                    {
                        builder.AppendLine($"Estado: {instance.State}");
                        builder.AppendLine($"Tipo: {creatureDef.Type}");
                        builder.AppendLine($"Nível: {creatureDef.Level}");
                        builder.AppendLine($"Poder recomendado: {creatureDef.RecommendedPower}");
                        builder.AppendLine($"Seu poder: {_map.Session.GetAttackerPower()} (Vortex {_map.Session.GetHeroMarchPower()} + dragões)");
                        builder.AppendLine($"Custo de energia: {_map.Session.EnergyCosts.ResolveCreature(creature.Id)}");
                        builder.AppendLine($"Respawn: {creatureDef.RespawnDuration.TotalHours:0.#} h");
                        builder.AppendLine($"Posição: ({creatureDef.X:0.#}, {creatureDef.Z:0.#})");
                        if (instance.RespawnAtUtc.HasValue)
                        {
                            var left = instance.RespawnAtUtc.Value - _map.Session.Clock.UtcNow;
                            if (left < TimeSpan.Zero)
                            {
                                left = TimeSpan.Zero;
                            }

                            builder.AppendLine($"Respawn em: {FormatDuration(left)}");
                        }

                        RefreshEncounterPanel(creatureDef, instance);
                    }

                    break;
                case WorldDragonNode dragon:
                    builder.AppendLine($"Código do nó: {dragon.DragonCode}");
                    if (_map.Session.Dragons.TryGetStatusByWorldCode(
                            dragon.DragonCode,
                            out var dragonName,
                            out var dragonState))
                    {
                        builder.AppendLine($"Espécie: {dragonName}");
                        builder.AppendLine($"Estado no ninho: {dragonState}");
                    }

                    builder.AppendLine($"Dragões READY: {_map.Session.Dragons.GetReadyDragonCount()}");
                    builder.AppendLine($"Poder em missão: {_map.Session.Dragons.GetProvisionalDragonPower()}");
                    break;
                case WorldLandmarkNode landmark:
                    builder.AppendLine($"Marco: {landmark.LandmarkCode}");
                    break;
            }

            if (_map.Session.TryGetTerritoryByRegion(definition.RegionId, out var territoryRuntime) &&
                WorldTerritoryCatalog.TryGetByRegion(definition.RegionId, out var territoryDef))
            {
                builder.AppendLine($"Território: {territoryDef.DisplayName} ({territoryRuntime.State})");
            }

            _details.text = builder.ToString();

            var canDispatch = selected.Status != WorldNodeStatus.Locked &&
                              definition is not WorldCityNode { IsPlayerHome: true };
            var canCollect = definition is WorldResourceNode &&
                             _map.Session.Marches.Active != null &&
                             _map.Session.Gathering.CanStart(
                                 selected,
                                 definition,
                                 _map.Session.Marches.Active);
            var canEngage = definition is WorldCreatureNode &&
                            _map.Session.Encounters.CanEngage(
                                selected.DefinitionId,
                                _map.Session.EnergyWallet.CurrentEnergy,
                                out _);
            var canResolve = definition is WorldCreatureNode &&
                             _map.Session.TryGetCreature(selected.DefinitionId, out var creatureState) &&
                             creatureState.State == WorldCreatureState.Engaged;
            var canReturn = _map.Session.Marches.Active?.State is MarchState.Arrived or MarchState.Gathering;
            var canCancel = _map.Session.Marches.Active != null &&
                            _map.Session.Marches.StateMachine.CanCancel(_map.Session.Marches.Active.State);

            _dispatchButton.SetEnabled(canDispatch);
            _collectButton.SetEnabled(canCollect);
            _engageButton.SetEnabled(canEngage);
            _resolveButton.SetEnabled(canResolve);
            _returnButton.SetEnabled(canReturn);
            _cancelButton.SetEnabled(canCancel);
            _collectButton.style.backgroundColor = canCollect
                ? new Color(0.25f, 0.55f, 0.3f)
                : new Color(0.2f, 0.2f, 0.2f);
            _engageButton.style.backgroundColor = canEngage
                ? new Color(0.55f, 0.3f, 0.25f)
                : new Color(0.2f, 0.2f, 0.2f);

            if (_encounterPanel != null && _encounterPanel.style.display == DisplayStyle.Flex)
            {
                foreach (var child in _encounterPanel.Children())
                {
                    if (child is Button { text: var text } btn)
                    {
                        if (text.StartsWith("Engajar", StringComparison.Ordinal)) btn.SetEnabled(canEngage);
                        else if (text.StartsWith("Resolver", StringComparison.Ordinal)) btn.SetEnabled(canResolve);
                    }
                }
            }
        }

        private void RefreshEncounterPanel(WorldCreatureDefinition creatureDef, WorldCreatureInstance instance)
        {
            if (_encounterPanel == null) return;
            _encounterPanel.style.display = DisplayStyle.Flex;
            _encounterTitle.text = instance.State == WorldCreatureState.Engaged
                ? "Combate em andamento"
                : $"Encontro · {creatureDef.DisplayName}";

            var power = _map.Session.GetAttackerPower();
            var band = CreatureDifficultyResolver.Resolve(power, creatureDef.RecommendedPower);
            _encounterBody.text =
                $"{creatureDef.DisplayName} (Nv.{creatureDef.Level})\n" +
                $"Estado: {instance.State}\n" +
                $"Poder inimigo: {creatureDef.RecommendedPower}\n" +
                $"Seu poder: {power}\n" +
                $"Formação: {_map.Session.DescribeHeroFormation()}\n" +
                $"Previsão: {band}";

            if (instance.State != WorldCreatureState.Engaged &&
                string.IsNullOrEmpty(_encounterResult.text))
            {
                _encounterResult.text = band == CreatureDifficultyBand.Impossible
                    ? "Poder insuficiente — melhore heróis/dragões."
                    : "Pronto para engajar.";
                _encounterResult.style.color = band == CreatureDifficultyBand.Impossible
                    ? new Color(1f, 0.5f, 0.4f)
                    : new Color(0.75f, 0.9f, 1f);
            }
        }

        private void RefreshPower()
        {
            // Poder embutido na barra de carteira — evita bloco técnico no topo.
        }

        private void RefreshWallet()
        {
            var energyWallet = _map.Session.EnergyWallet;
            var energy = $"Energia {energyWallet.CurrentEnergy}/{energyWallet.MaxEnergy}";
            var power = _map.Session.GetAttackerPower();
            if (_economy == null)
            {
                _wallet.text = $"{energy} · Poder {power}";
                return;
            }

            var w = _economy.Wallet;
            _wallet.text =
                $"{energy} · Poder {power} · " +
                $"Ouro {w.Get(ResourceType.Gold)} · Comida {w.Get(ResourceType.Food)} · " +
                $"Madeira {w.Get(ResourceType.Wood)} · Pedra {w.Get(ResourceType.Stone)} · Ferro {w.Get(ResourceType.Iron)}";
        }

        private void RefreshMarch()
        {
            var marches = _map.Session.Marches;
            var march = marches.Active;
            if (march == null)
            {
                _march.text = marches.HasQueuedMarch
                    ? $"Marcha na fila → {marches.QueuedTargetNodeId}"
                    : "Sem marcha ativa";
                return;
            }

            var remaining = (march.State == MarchState.Returning
                    ? march.ReturnAt ?? march.ArrivalAt
                    : march.ArrivalAt) - _map.Session.Clock.UtcNow;
            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            _march.text = $"Marcha {march.State} · resta {FormatDuration(remaining)}";
        }

        private void RefreshTerritory()
        {
            var region = _map.Session.RegionSelection.Selected;
            if (region != null &&
                _map.Session.TryGetTerritoryByRegion(region.DefinitionId, out var runtime) &&
                WorldTerritoryCatalog.TryGetByRegion(region.DefinitionId, out var definition))
            {
                _territory.text = $"Território: {definition.DisplayName} · {runtime.State}";
                return;
            }

            var selected = _map.Session.Selection.Selected;
            if (selected != null)
            {
                var def = _map.Session.GetDefinition(selected.DefinitionId);
                if (_map.Session.TryGetTerritoryByRegion(def.RegionId, out var nodeTerritory) &&
                    WorldTerritoryCatalog.TryGetByRegion(def.RegionId, out var territoryDef))
                {
                    _territory.text = $"Território: {territoryDef.DisplayName} · {nodeTerritory.State}";
                    return;
                }
            }

            _territory.text = "Território: selecione uma região ou nó.";
        }

        private static string FormatDuration(TimeSpan span)
        {
            if (span.TotalHours >= 1)
            {
                return $"{span.TotalHours:0.00} h";
            }

            return $"{span.TotalMinutes:0.0} min";
        }
    }
}
