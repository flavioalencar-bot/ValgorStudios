using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Core;

namespace Valgor.UI
{
    /// <summary>
    /// Painel compacto de tutorial (canto) — não cobre a cidade.
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

        public static void NotifyHeroesOpened()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.OpenHeroes)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        public static void NotifyDragonTowerFocused()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.DragonTower)
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

        public static void NotifyReturnedToCity()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.ReturnCity)
            {
                LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
                PlayerPrefs.Save();
            }
        }

        public static void NotifyMarchOrGatherAction()
        {
            if (LocalPlayerProfile.TutorialStep == LocalPlayerProfile.TutorialSteps.MarchGather)
            {
                LocalPlayerProfile.AdvanceTutorial();
            }
        }

        private static VisualElement BuildOverlay()
        {
            var overlay = new VisualElement { name = OverlayName };
            overlay.style.position = Position.Absolute;
            overlay.style.right = 16;
            overlay.style.top = 72;
            overlay.style.width = 280;
            overlay.style.maxWidth = 300;
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

            var title = new Label { name = "guide-title" };
            title.style.color = BetaVisualTheme.AgedGoldBright;
            title.style.fontSize = 13;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            overlay.Add(title);

            var body = new Label { name = "guide-body" };
            body.style.color = BetaVisualTheme.TextPrimary;
            body.style.fontSize = 12;
            body.style.whiteSpace = WhiteSpace.Normal;
            body.style.marginTop = 4;
            overlay.Add(body);

            var row = new VisualElement { name = "guide-row" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 8;
            overlay.Add(row);

            var primary = new Button { name = "guide-primary", text = "Avançar" };
            StyleButton(primary);
            row.Add(primary);

            var skip = new Button { name = "guide-skip", text = "Ignorar" };
            StyleButton(skip);
            skip.style.marginLeft = 8;
            skip.clicked += () =>
            {
                LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
                PlayerPrefs.Save();
                overlay.RemoveFromHierarchy();
            };
            row.Add(skip);

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
            StyleButton(fresh);
            fresh.clicked += () =>
            {
                onPrimary?.Invoke();
                if (step == LocalPlayerProfile.TutorialSteps.Welcome)
                {
                    LocalPlayerProfile.AdvanceTutorial();
                    Refresh(overlay, onPrimary);
                }
                else if (step == LocalPlayerProfile.TutorialSteps.ReturnCity)
                {
                    LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
                    PlayerPrefs.Save();
                    overlay.RemoveFromHierarchy();
                }
            };
            row.Insert(index < 0 ? 0 : index, fresh);
        }

        private static void GetCopy(int step, out string title, out string body, out string primary)
        {
            switch (step)
            {
                case LocalPlayerProfile.TutorialSteps.Welcome:
                    title = "Bem-vindo";
                    body = "Esta é a sua cidade. Recursos no topo; navegue pela barra inferior.";
                    primary = "Avançar";
                    break;
                case LocalPlayerProfile.TutorialSteps.OpenHeroes:
                    title = "Heróis";
                    body = "Abra Heróis na barra inferior e conheça Vortex.";
                    primary = "Avançar";
                    break;
                case LocalPlayerProfile.TutorialSteps.DragonTower:
                    title = "Dragões";
                    body = "Toque em Dragões ou selecione a Torre dos Dragões na cidade.";
                    primary = "Avançar";
                    break;
                case LocalPlayerProfile.TutorialSteps.OpenMap:
                    title = "Mapa";
                    body = "Abra o Mapa na barra inferior para explorar e coletar.";
                    primary = "Avançar";
                    break;
                case LocalPlayerProfile.TutorialSteps.MarchGather:
                    title = "Marcha";
                    body = "Selecione um nó de recurso, envie a marcha e colete.";
                    primary = "Avançar";
                    break;
                case LocalPlayerProfile.TutorialSteps.ReturnCity:
                    title = "Retorno";
                    body = "Volte à Cidade pela barra inferior quando terminar.";
                    primary = "Concluir";
                    break;
                default:
                    title = "Tutorial";
                    body = "Pronto.";
                    primary = "Ok";
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
