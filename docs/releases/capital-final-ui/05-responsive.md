# 05 — Responsividade

Referência: **1920×1080**.

| Resolução | Status esperado |
|-----------|-----------------|
| 1920×1080 | válido |
| 1600×900 | válido |
| 1366×768 | válido |
| 1280×720 | válido |
| 1080×640 | válido (compact via `ValgorResponsiveUi`) |

Regras:

- root HUD `pickingMode=Ignore` (cidade clicável)
- chat e atalhos com picking local
- painéis usam `ValgorResponsiveUi.Compact`
- barra inferior flexShrink para não cortar botões
