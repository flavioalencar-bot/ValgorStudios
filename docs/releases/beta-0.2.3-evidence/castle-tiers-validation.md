# Castle Tiers 1–6 — validação pré-integração

Gerado a partir de `production/City/Castle/reports/prepare_all_tiers.json`.

## Fonte GLB

| Arquivo | Bytes | Magic | Textura embutida | Mesh único | Geometria solta |
|---------|------:|-------|-----------------|------------|-----------------|
| Castle_Tier1.glb | 10 235 652 | glTF | sim (BaseColor) | 1 | não |
| Castle_Tier2.glb | 11 028 808 | glTF | sim | 1 | não |
| Castle_Tier3.glb | 10 973 640 | glTF | sim | 1 | não |
| Castle_Tier4.glb | 10 917 588 | glTF | sim | 1 | não |
| Castle_Tier5.glb | 11 406 624 | glTF | sim | 1 | não |
| Castle_Tier6.glb | 11 253 408 | glTF | sim | 1 | não |

Datas de modificação (source): 2026-07-28 ~18:12–18:14.

## Normalização staging

| Tier | Footprint (m) | Altura (m) | Pivô | Orientação export |
|-----:|--------------:|-----------:|------|-------------------|
| 1 | 7.5 | ~4.81 | base centro Y=0 | FBX Y-up, forward −Z → portão Unity +Z |
| 2 | 8.1 | ~7.30 | idem | idem |
| 3 | 8.7 | ~7.93 | idem | idem |
| 4 | 9.4 | ~7.71 | idem | idem |
| 5 | 10.1 | ~8.60 | idem | idem |
| 6 | 10.8 | ~10.69 | idem | idem |

Crescimento visual progressivo intencional (não homogeneizado).

## Faixas de nível

- 1–5 → Tier1 · 6–10 → Tier2 · 11–15 → Tier3
- 16–20 → Tier4 · 21–25 → Tier5 · 26–30 → Tier6

## Runtime

- `CastleRealVisualLoader` troca só o filho visual sob `Visual`
- `BuildingView.ApplyColors` não recoloriza assets `Castle_Tier*_Real`
- Materiais URP/Lit com BaseMap por tier
- Transição encolher→troca→crescer ao cruzar faixa
