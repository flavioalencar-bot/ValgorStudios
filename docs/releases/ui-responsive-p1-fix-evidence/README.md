# UI Responsive P1 Fix — evidências

**Build:** `builds/windows/Valgor-QA-Responsive-P1-Fix/Valgor.exe`  
**Flag:** `-responsiveUiTest`  
**Auto-teste:** 16 pass / 0 fail (2026-07-29)

## Causa do corte

1. **Main Menu:** `ScrollView` com `flexGrow=0` + `justifyContent: Center` — o card crescia com o conteúdo e estourava a viewport; botões inferiores (**Sair** / **Confirmar**) saíam da área visível sem scroll efetivo.
2. **PanelSettings:** `ScaleWithScreenSize` sem `match=0.5` (MatchWidthOrHeight) agravava overflow vertical em 1080×640 vs ref 1920×1080.
3. **Modais / QA / Missões:** `maxHeight` fixo ou painéis sem scroll interno em altura curta.

## Correção (só layout)

- `ValgorResponsiveUi` — safe pad, modal shell %, scroll viewport-bound, compact metrics
- `BetaUiPanels` — `match = 0.5`
- Main Menu, HUD City, nav, Missões, modais (Detalhes/Atualizar/Obter/Auto-refill), painel QA, Loading

## Capturas 1080×640 (obrigatórias)

| Arquivo | Tela |
|---------|------|
| `00-main-menu-1080x640.png` | Menu principal (Jogar…Sair visíveis) |
| `city-hud-1080x640.png` | City HUD |
| `city-context-1080x640.png` | Contextual edifício |
| `details-1080x640.png` | Detalhes |
| `upgrade-1080x640.png` | Atualizar (footer Atualizar visível) |
| `obtain-1080x640.png` | Obter mais |
| `city-qa-panel-1080x640.png` | Painel QA |
| `1080x640-construction-world.png` | Construção em andamento |
| `1080x640-missions.png` | Missões |

Comparativo 5 resoluções: ver `COMPARATIVE.md` + PNGs `*-1920x1080` … `*-1080x640`.

## P1

**Encerrado** para o critério “botões obrigatórios cortados em ~1080×640” no fluxo menu + City + modais de evolução, com evidência visual e auto-teste.
