# 01 — Auditoria técnica (atualizada — 17 oficiais)

**Projeto:** `C:\Valgor_Studio` · **Branch:** `master`  
**Base:** `0804fc4` · Fase 5 `6331d7b` · Blueprint `d3845d1`

## Tabela de reconciliação

| Edifício oficial | ID atual | Entidade lógica | Selecionável | Persistido | Situação |
| ---------------- | -------- | --------------: | -----------: | ---------: | -------- |
| Castelo | castle | sim | sim | sim | OK |
| Muralha | wall | sim (+ visual anel) | sim | sim | OK |
| Fazenda | farm | sim | sim | sim | OK |
| Serraria | lumbermill | sim | sim | sim | OK (alias sawmill) |
| Pedreira | quarry | sim | sim | sim | OK |
| Mina | mine | sim | sim | sim | OK |
| Armazém | warehouse | sim | sim | sim | OK |
| Residência | residence | sim | sim | sim | **NOVO** |
| Mercado | market | sim | sim | sim | OK |
| Campo de Treinamento | training-ground | sim | sim | sim | **migrado de laboratory** |
| Fundição | foundry | sim | sim | sim | **NOVO** |
| Arena | arena | sim | sim | sim | OK |
| Academia | academy | sim | sim | sim | OK |
| Hospital | hospital | sim | sim | sim | OK |
| Templo | temple | sim | sim | sim | OK |
| Salão dos Heróis | hero-hall | sim | sim | sim | **migrado de institute** |
| Torre dos Dragões | dragon-tower | sim | sim | sim | OK |

## Cena / layout

- Cena: `City.unity` + spawn runtime
- Layout: `CityCapitalLayoutConfig` (17 entradas)
- Migração: `BuildingIdMigration`
