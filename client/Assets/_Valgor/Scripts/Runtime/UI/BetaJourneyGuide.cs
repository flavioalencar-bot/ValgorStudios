using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Core;

namespace Valgor.UI
{
    /// <summary>
    /// Tutorial mínimo da Beta 0.1 (canto) — guia a jornada inicial.
    /// </summary>
    public static class BetaJourneyGuide
    {
        public const string OverlayName = "beta-journey-guide";

        public static void AttachOrRefresh(VisualElement root, System.Action onPrimary = null)
        {
            if (root == null)
            {
                return;
            }

            if (LocalPlayerProfile.TutorialComplete)
            {
                root.Q(OverlayName)?.RemoveFromHierarchy();
                return;
            }

            var overlay = root.Q(OverlayName) ?? BuildOverlay();
            if (overlay.parent != root)
            {
                root.Add(overlay);
            }

            Refresh(overlay, onPrimary);
        }

        public static void NotifyCastleSelected()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.SelectCastle)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyFarmSelected()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.SelectFarm)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyHeroesOpened()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.OpenHeroes)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyVortexViewed()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.ViewVortex)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyDragonTowerFocused()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.OpenDragons)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyDragonFed()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.FeedDragon)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyWorldMapOpened()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.OpenMap)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyResourceNodeSelected()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.SelectResource)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyMarchOrGatherAction()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.SendMarch)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyRewardReceived()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.ReceiveReward)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyReturnedToCity()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.ReturnCity)
            {
                LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
                PlayerPrefs.Save();
            }
        }

        private static VisualElement BuildOverlay()
        {
            var overlay = new VisualElement { name = OverlayName };
            overlay.style.position = Position.Absolute;
            overlay.style.right = 16;
            overlay.style.top = 72;
            overlay.style.width = 300;
            overlay.style.maxWidth = 320;
            overlay.style.paddingLeft = 12;
            overlay.style.paddingRight = 12;
            overlay.style.paddingTop = 10;
            overlay.style.paddingBottom = 10;
            overlay.style.backgroundColor = new Color(0.1f, 0.11f, 0.12f, 0.92f);
            overlay.style.borderTopWidth = 2;
            overlay.style.borderBottomWidth = 2;
            overlay.style.borderLeftWidth = 2;
            overlay.style.borderRightWidth = 2;
            overlay.style.borderTopColor = BetaVisualTheme.AgedGold;
            overlay.style.borderBottomColor = BetaVisualTheme.AgedGold;
            overlay.style.borderLeftColor = BetaVisualTheme.AgedGold;
            overlay.style.borderRightColor = BetaVisualTheme.AgedGold;
            // Só o botão captura pointer — o restante da cidade permanece clicável.
            overlay.pickingMode = PickingMode.Ignore;

            var title = new Label { name = "guide-title" };
            title.pickingMode = PickingMode.Ignore;
            title.style.color = BetaVisualTheme.AgedGoldBright;
            title.style.fontSize = 13;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            overlay.Add(title);

            var body = new Label { name = "guide-body" };
            body.pickingMode = PickingMode.Ignore;
            body.style.color = BetaVisualTheme.TextPrimary;
            body.style.fontSize = 12;
            body.style.whiteSpace = WhiteSpace.Normal;
            body.style.marginTop = 4;
            overlay.Add(body);

            var row = new VisualElement { name = "guide-row" };
            row.pickingMode = PickingMode.Ignore;
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 8;
            overlay.Add(row);

            var primary = new Button { name = "guide-primary", text = "Entendi" };
            primary.pickingMode = PickingMode.Position;
            StyleButton(primary);
            row.Add(primary);

            return overlay;
        }

        private static void Refresh(VisualElement overlay, System.Action onPrimary)
        {
            var step = LocalPlayerProfile.TutorialStep;
            GetCopy(step, out var title, out var body, out var primaryLabel);
            overlay.Q<Label>("guide-title").text = title;
            overlay.Q<Label>("guide-body").text = body;

            var primary = overlay.Q<Button>("guide-primary");
            var row = overlay.Q("guide-row");
            var index = row.IndexOf(primary);
            primary.RemoveFromHierarchy();
            var fresh = new Button { name = "guide-primary", text = primaryLabel };
            fresh.pickingMode = PickingMode.Position;
            StyleButton(fresh);
            fresh.clicked += () =>
            {
                onPrimary?.Invoke();
                if (step == LocalPlayerProfile.TutorialSteps.ReturnCity)
                {
                    LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
                    PlayerPrefs.Save();
                    overlay.RemoveFromHierarchy();
                }
                else
                {
                    Refresh(overlay, onPrimary);
                }
            };
            row.Insert(index < 0 ? 0 : index, fresh);
        }

        private static void GetCopy(int step, out string title, out string body, out string primary)
        {
            primary = "Entendi";
            switch (step)
            {
                case LocalPlayerProfile.TutorialSteps.SelectCastle:
                    title = "Castelo";
                    body = "Selecione o Castelo — o coração da sua cidade.";
                    break;
                case LocalPlayerProfile.TutorialSteps.SelectFarm:
                    title = "Fazenda";
                    body = "Selecione a Fazenda para produzir comida.";
                    break;
                case LocalPlayerProfile.TutorialSteps.OpenHeroes:
                    title = "Heróis";
                    body = "Abra Heróis na barra inferior.";
                    break;
                case LocalPlayerProfile.TutorialSteps.ViewVortex:
                    title = "Vortex";
                    body = "Observe Vortex, o Rei dos Dragões — seu guia.";
                    break;
                case LocalPlayerProfile.TutorialSteps.OpenDragons:
                    title = "Dragões";
                    body = "Abra Dragões para visitar a Torre dos Dragões.";
                    break;
                case LocalPlayerProfile.TutorialSteps.FeedDragon:
                    title = "Alimentar";
                    body = "Alimente o dragão inicial na Torre.";
                    break;
                case LocalPlayerProfile.TutorialSteps.OpenMap:
                    title = "Mapa Mundial";
                    body = "Abra o Mapa na barra inferior.";
                    break;
                case LocalPlayerProfile.TutorialSteps.SelectResource:
                    title = "Recurso";
                    body = "Selecione um nó de recurso no mapa.";
                    break;
                case LocalPlayerProfile.TutorialSteps.SendMarch:
                    title = "Marcha";
                    body = "Envie a marcha para coletar.";
                    break;
                case LocalPlayerProfile.TutorialSteps.ReceiveReward:
                    title = "Recompensa";
                    body = "Aguarde o retorno e receba a recompensa.";
                    break;
                case LocalPlayerProfile.TutorialSteps.ReturnCity:
                    title = "Retorno";
                    body = "Volte à Cidade pela barra inferior.";
                    primary = "Concluir";
                    break;
                default:
                    title = "Jornada";
                    body = "Continue explorando Valgor.";
                    break;
            }
        }

        private static void StyleButton(Button button)
        {
            button.style.paddingLeft = 10;
            button.style.paddingRight = 10;
            button.style.paddingTop = 6;
            button.style.paddingBottom = 6;
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
            button.style.fontSize = 12;
        }
    }
}
