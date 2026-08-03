# 06 — Performance

## Metas

Medium estável em 1080×640 e 1920×1080.

## Medidas aplicadas

- UIToolkit (sem rebuild uGUI global)
- refresh HUD a 0,5s (não por frame)
- labels: tick 0,2s
- shared materials Art Pass + densidade sem colliders
- LOD/static batch existentes
- sombra soft com strength controlada
- UrbanDensity layer Ignore Raycast

## Profiler

Executar no player QA após boot (separar spike de boot):

| Métrica | Alvo |
|---------|------|
| FPS médio contínuo | ≥ 30 Medium |
| FPS mínimo pós-boot | ≥ 20 |
| Labels ativos | ≤ 17 oficiais |

Ver `08-player-audit.md` após captura.
