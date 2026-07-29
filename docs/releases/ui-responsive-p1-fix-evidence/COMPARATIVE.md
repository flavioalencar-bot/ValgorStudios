# Comparativo resoluções — Responsive P1

| Resolução | Capturas |
|---|---|
| 1920x1080 | city-hud / city-context / details / upgrade / obtain / city-qa-panel |
| 1600x900 | city-hud / city-context / details / upgrade / obtain / city-qa-panel |
| 1366x768 | city-hud / city-context / details / upgrade / obtain / city-qa-panel |
| 1280x720 | city-hud / city-context / details / upgrade / obtain / city-qa-panel |
| 1080x640 | city-hud / city-context / details / upgrade / obtain / city-qa-panel |

Foco 1080×640: `1080x640-construction-world.png`, `1080x640-missions.png`, `00-main-menu-1080x640.png`.

## Causa do corte (pré-fix)
- Menu: ScrollView com flexGrow=0 + justify center → conteúdo estourava a viewport e botões inferiores sumiam.
- PanelSettings: ScaleWithScreenSize sem match balanceado (0.5) agravava overflow vertical.
- Modais/QA/Missões: maxHeight fixo ou painel sem scroll interno em altura curta.
