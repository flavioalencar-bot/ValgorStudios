# 04 — Validação de câmera (preview V2)

## Ajustes no preview

| Parâmetro | V1 | V2 Preview |
|---|---:|---:|
| Overview zoom (ortho size) | 20 | 24 |
| Pitch | 50° | 48° |
| Yaw | 18° | 28° |
| Height | 20 | 26 |
| Focus | origem | Castelo (leve offset para NE) |

## Critérios overview

Deve caber na mesma vista:

- Castelo (foco)
- Praça / eixo sul
- Portão
- Parte da muralha
- Torre dos Dragões (NE) **sem cortar**
- Distritos principais

## Resoluções

| Captura | Resolução |
|---|---|
| `10-overview-1080x640.png` | 1080×640 (mobile landscape) |
| `11-overview-1920x1080.png` | 1920×1080 |

## Bounds

`CityBounds` permanece (−28…28). Zoom máx. sobe automaticamente se `OverviewZoom` exigir.

## Etapa B

Recalibrar HUD/safe areas com o layout oficial promovido; não alterar defaults do player normal nesta etapa.
