using System;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;

namespace Valgor.WorldMap.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class WorldMapHudController : MonoBehaviour
    {
        private UIDocument _document = null!;
        private WorldMapController _map = null!;
        private CityEconomy? _economy;
        private Label _title = null!;
        private Label _wallet = null!;
        private Label _march = null!;
        private VisualElement _panel = null!;
        private Label _details = null!;
        private Button _dispatchButton = null!;
        private Button _collectButton = null!;
        private Button _engageButton = null!;
        private Button _resolveButton = null!;
        private Button _returnButton = null!;
        private Button _cancelButton = null!;
        private Label _feedback = null!;

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
            if (_document.panelSettings != null)
            {
                return;
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            _document.panelSettings = settings;
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;

            _title = new Label("World Map");
            _title.style.position = Position.Absolute;
            _title.style.left = 18;
            _title.style.top = 18;
            _title.style.fontSize = 28;
            _title.style.color = Color.white;
            root.Add(_title);

            _wallet = new Label();
            _wallet.style.position = Position.Absolute;
            _wallet.style.left = 18;
            _wallet.style.top = 56;
            _wallet.style.color = new Color(0.9f, 0.92f, 0.85f);
            _wallet.style.fontSize = 14;
            root.Add(_wallet);

            _march = new Label();
            _march.style.position = Position.Absolute;
            _march.style.left = 18;
            _march.style.top = 84;
            _march.style.color = new Color(0.75f, 0.9f, 1f);
            _march.style.fontSize = 13;
            _march.style.whiteSpace = WhiteSpace.Normal;
            _march.style.width = 420;
            root.Add(_march);

            var actions = new VisualElement();
            actions.style.position = Position.Absolute;
            actions.style.right = 18;
            actions.style.top = 18;
            root.Add(actions);
            actions.Add(CreateButton("Voltar para a Cidade", () =>
                StartCoroutine(GameBootstrap.Game.Navigator.GoToCity())));

            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.left = 18;
            _panel.style.bottom = 18;
            _panel.style.width = 380;
            _panel.style.paddingLeft = 14;
            _panel.style.paddingRight = 14;
            _panel.style.paddingTop = 12;
            _panel.style.paddingBottom = 12;
            _panel.style.backgroundColor = new Color(0.04f, 0.08f, 0.06f, 0.92f);
            _details = new Label();
            _details.style.color = Color.white;
            _details.style.whiteSpace = WhiteSpace.Normal;
            _panel.Add(_details);

            _dispatchButton = CreateButton("Enviar marcha", OnDispatch);
            _collectButton = CreateButton("Coletar recursos", OnCollect);
            _engageButton = CreateButton("Engajar criatura", OnEngage);
            _resolveButton = CreateButton("Resolver encontro", OnResolve);
            _returnButton = CreateButton("Retornar à cidade", OnReturn);
            _cancelButton = CreateButton("Cancelar marcha", OnCancel);
            _panel.Add(_dispatchButton);
            _panel.Add(_collectButton);
            _panel.Add(_engageButton);
            _panel.Add(_resolveButton);
            _panel.Add(_returnButton);
            _panel.Add(_cancelButton);
            _panel.Add(CreateButton("Fechar", () => _map.Session.Selection.Deselect()));

            _feedback = new Label();
            _feedback.style.color = new Color(1f, 0.85f, 0.45f);
            _feedback.style.marginTop = 8;
            _feedback.style.whiteSpace = WhiteSpace.Normal;
            _panel.Add(_feedback);

            root.Add(_panel);
            Refresh();
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

        private void OnDispatch()
        {
            if (_map.Session.TryDispatchToSelected(out var error))
            {
                _feedback.text = "Marcha enviada.";
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
                _feedback.text = "Encontro iniciado.";
            }
            else
            {
                _feedback.text = error;
            }

            Refresh();
        }

        private void OnResolve()
        {
            if (_map.Session.TryResolveSelectedCreature(_economy?.Wallet, out var error, out var band))
            {
                _economy?.PersistWallet();
                _feedback.text = $"Vitória provisória ({band}). Recompensas coletadas.";
            }
            else
            {
                _feedback.text = error;
            }

            Refresh();
        }

        private void Refresh()
        {
            RefreshWallet();
            RefreshMarch();

            var selected = _map.Session.Selection.Selected;
            _panel.style.display = selected == null ? DisplayStyle.None : DisplayStyle.Flex;
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
                        builder.AppendLine($"Custo de energia: {creatureDef.EnergyCost}");
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
                    }

                    break;
                case WorldDragonNode dragon:
                    builder.AppendLine($"Dragão: {dragon.DragonCode}");
                    break;
                case WorldLandmarkNode landmark:
                    builder.AppendLine($"Marco: {landmark.LandmarkCode}");
                    break;
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
                            _map.Session.Encounters.CanEngage(selected.DefinitionId, _map.Session.Energy, out _);
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
        }

        private void RefreshWallet()
        {
            var energy = $"Energia {_map.Session.Energy}/{_map.Session.Settings.MaxEnergy}";
            if (_economy == null)
            {
                _wallet.text = $"{energy} · Carteira: visite a Cidade primeiro para sincronizar recursos.";
                return;
            }

            var w = _economy.Wallet;
            _wallet.text =
                $"{energy} · Gold {w.Get(ResourceType.Gold)} · Food {w.Get(ResourceType.Food)} · Wood {w.Get(ResourceType.Wood)} · Stone {w.Get(ResourceType.Stone)} · Iron {w.Get(ResourceType.Iron)}";
        }

        private void RefreshMarch()
        {
            var march = _map.Session.Marches.Active;
            if (march == null)
            {
                _march.text = "Marcha: nenhuma ativa.";
                return;
            }

            var remaining = (march.State == MarchState.Returning
                    ? march.ReturnAt ?? march.ArrivalAt
                    : march.ArrivalAt) - _map.Session.Clock.UtcNow;
            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            _march.text =
                $"Marcha: {march.State} → {march.TargetNodeId} · carga {march.ResourceLoad}/{march.Capacity} · resta {FormatDuration(remaining)}";
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
