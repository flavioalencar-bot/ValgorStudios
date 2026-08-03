# 06 — Validação da muralha

## Lógica (inalterada)

| Item | Valor |
|---|---|
| BuildingId | `wall` (único) |
| Âncora V2 | ~(0.4, 0, −20.5) sul |
| Seleção / upgrade / reparo / save | apontam para `wall` |

## Visual

- Forma pode ser orgânica (relevo, rios, penhasco, entrada sul).
- **Não** exigir círculo perfeito nos testes de preview/V2.
- `WallRadius` V1 permanece para compatibilidade do layout oficial atual.

## Etapa A

Validação: clique em segmentos → `wall`; âncora sul; captura `09-wall-organic.png`.

## Etapa B

Atualizar `CityWallVisualTests` para invariantes orgânicas sem perder cobertura de seleção.
