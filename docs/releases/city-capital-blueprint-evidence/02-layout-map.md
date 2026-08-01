# 02 — Mapa de layout

## Eixo monumental

```text
Portão (S, z≈-20.5)
  → Estrada Imperial
  → Praça (z≈-3.2)
  → Castelo (0,0)
  → Caminho Norte
  → Torre dos Dragões (platô z≈17.6)
```

## Distritos (17 oficiais)

```text
Norte elevado     → Dragões + Academia / Mina / Pedreira
NW / NE internos  → Sagrado (Hospital / Templo) + Salão dos Heróis (hero-hall)
Leste             → Militar (Arena + Campo de Treinamento)
SW / S internos   → Econômico (Mercado / Armazém / Residência)
Sul externo       → Industrial (Serraria / Fundição) + Rural (Fazenda)
Extremo sul       → Acesso (Portão / Muralha)
```

## Hierarquia runtime

```text
CityRoot
├── Terrain / Water / Roads / Walls / Districts
├── Decorations / Vegetation / AmbientFX / TierGroups
├── Buildings (17 slots canônicos)
└── NPCs / Navigation
```
