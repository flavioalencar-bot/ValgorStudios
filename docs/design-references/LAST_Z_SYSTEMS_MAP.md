# Last Z → Valgor: mapa de sistemas

**Fonte:** informação pública de guias/wikis de *Last Z: Survival Shooter* (2025–2026).  
**Uso:** direção de produto/gameplay do Valgor — **não copiar arte, nomes, facções nem UI pixel-a-pixel**.  
**Data:** 2026-07-26.

---

## 1. O que é o Last Z (núcleo)

Loop dual:

| Modo Last Z | Função | Analogia Valgor |
|-------------|--------|-----------------|
| **Shooter stages** (ação vertical) | Limpa névoa, ganha XP/recursos, “abre” território | **Fora da beta 0.1** (não portamos shooter) |
| **Base management** | Constrói/upgrades, desbloqueia poder | **Cidade do jogador** |
| **World map** | Marchas, coleta, PvE/PvP, aliança | **World Map** |
| **Heroes + facções** | Time, passivas, gacha, seasons | **Heróis + facções Valgor** |
| **Creatures / seasons** | Zumbis → dinos etc. | **Dragões + criaturas de mapa** |

Regra de ouro Last Z: *fight to build, build to fight*.  
No Valgor beta: *cidade fortalece heróis/dragões → mapa gera recursos/pressão → volta à cidade*.

---

## 2. Inventário de sistemas (Last Z)

### 2.1 Base / HQ

| Sistema LZ | Notas públicas | Valgor hoje | Beta 0.1 | Próximo |
|------------|----------------|-------------|----------|---------|
| Headquarters (castelo/HQ) | Cap de nível de prédios e heróis | Castelo + níveis | ✅ parcial | Cap global por nível do castelo |
| Laboratory | Obrigatório p/ HQ alto; tech tree | Laboratório (slot) | ✅ slot | Pesquisa real |
| Resource buildings (farm, lumber, power…) | ROI fraco vs gathering | Fazenda/Serraria/etc. | ✅ produção passiva | Manter como complemento, não core |
| Warehouse | Capacidade | Armazém | ✅ | Soft cap + overflow |
| Camps (Assaulter/Shooter/Rider) | Tropas / capacidade de marcha | — | ❌ | Tropas **ou** só heróis+dragões (decidir) |
| City Walls | Pré-requisito HQ | — | ❌ | Cosmético / defesa PvP depois |
| Alliance Center | Ajuda/reforço | — | ❌ | Pós-beta |
| Military / Formation / Rally | Poder de marcha | — | ❌ | Formação de marcha (heróis) |
| Production Center | Speed/load de gather | — | ❌ | Buff de coleta |
| Pub / Bar (gacha) | Recruta heróis | — | ❌ | Fora da beta; catálogo fixo |
| Hospital | Cura tropas | Hospital (slot) | ✅ slot | Cura / downtime |
| Market / Trade | Economia secundária | Mercado | ✅ slot | Depois |
| Hall of Honor (por facção) | Buff de facção | Instituto/Academia | ✅ slots | Buffs de facção |
| Decorations (76+ / 27) | Power cosmético | — | ❌ | Nunca na beta |

**Prioridade LZ (early):** HQ → Lab → pré-requisitos rotativos → camps/aliança/muros.  
**Prioridade Valgor beta:** Castelo → produção mínima → Torre dos Dragões → Armazém → resto cosmético/placeholder.

### 2.2 Recursos

| LZ | Valgor |
|----|--------|
| Food, Wood, Electricity/Power | Food, Wood, Stone, Iron, Gold, Essence, Diamonds (seed) |
| Steel (late) | — (depois) |
| Diamonds / soft premium | Diamonds (placeholder) |
| Hero XP | — (ligar a Torneio/missões depois) |
| Speed-ups | — (fora da beta) |

**Insight LZ:** gathering no **mapa** >> produção na base.  
**Valgor:** manter produção na cidade para tutorial; **mapa = fonte principal** de recurso “sério”.

### 2.3 Heróis

| LZ | Valgor |
|----|--------|
| ~31 heróis, 3 facções sazonais | Catálogo Heroes + Vortex real |
| Roles: Defender / DPS / Support | Roles no data model (parcial) |
| Faction synergy (+stats / gather) | Facções no módulo Heroes |
| Stars / 4 skills / gear | Skills/poder Vortex; resto stub |
| Gacha + shards + seasons | **Não** na beta — unlock por jornada |
| Passives “even when not deployed” | Depois (Sofia-like → buff construção) |
| Focus 3–5 main heroes | **Vortex + 1–2** na demo |

**Beta:** roster navegável + Vortex jogável no preview; marcha usa poder provisório até gateway real.

### 2.4 World map

| LZ | Valgor |
|----|--------|
| Nós de recurso (nível, gather overnight) | Nós Resource + coleta/marcha |
| Creatures / NPC raids | Creature nodes + encounter texto |
| Cities / alliance territory | Territory overlay parcial |
| Fog of war (via shooter) | Filtros / locked nodes |
| Multiple marches | 1 marcha ativa (beta) |
| Trucks / radar / bounties | — |
| Alliance PvP / city capture | — |

**Já no Valgor:** marcha FSM, energia, gather, retorno, marca visual no mapa.  
**Falta “cheiro de LZ”:** multi-marcha, carga (load), tempo overnight legível, encontro com feedback, território com buff.

### 2.5 Combate / ação

| LZ | Valgor |
|----|--------|
| Shooter stages (core) | **Fora de escopo** da linha strategy |
| Troop combat no mapa | Encounter provisório (texto) |
| Arena / duels | — |
| Boss / season events | Domínio do Rei (VFX herói) só |

**Decisão de produto:** Valgor **não** clona o shooter. Combate de mapa = resolução tática leve (heróis + dragões), não bullet-hell.

### 2.6 Social / meta

| LZ | Valgor beta |
|----|-------------|
| Alliance, helps, shop, duels | ❌ |
| Seasons / battle pass | ❌ |
| VIP / trucks | ❌ |
| Events calendar | ❌ |
| Local profile + Continuar | ✅ |

---

## 3. Loop jogável alvo (Valgor, inspirado em LZ)

```text
[Cidade] upgrade / coleta passiva / Torre (dragões)
    ↓
[Heróis] ver roster / Vortex (poder)
    ↓
[Mapa] marcha → (encontro?) → coletar → retornar
    ↓
[Cidade] gastar recursos / próximo upgrade
```

Espelho LZ sem shooter:

| LZ | Valgor |
|----|--------|
| Clear stage → resources | Marcha/coleta → resources |
| Upgrade HQ → stronger heroes | Upgrade castelo/torre → mais poder |
| Deploy gather heroes | Deploy herói+dragão na marcha |
| Alliance pressure | (depois) |

---

## 4. Matriz: o que entra agora

### ✅ Já na Beta Técnica 0.1 (manter / polir)

1. Fluxo Bootstrap → Loading → Menu → City → Heroes → Torre → Map → City  
2. Perfil local (nome, intro, tutorial, Continuar)  
3. Cidade com 14 slots + produção/coleta/upgrade  
4. Silhuetas de cidade + ambiente  
5. HeroesDemo + Vortex  
6. DragonService (estados, ninho, fome)  
7. World map: nós, marcha, energia, gather, território parcial  
8. Marcha **visível** no mundo  

### 🟡 Próximo slice (“cheira a Last Z strategy”)

| # | Entrega | Por quê (LZ) |
|---|---------|--------------|
| 1 | Formação de marcha com herói (Vortex) + dragão opcional | Deploy de time no mapa |
| 2 | Feedback de encontro (painel + resultado, não só label) | Combate de mapa legível |
| 3 | Placeholder 3D no ninho da Torre | Criatura “viva” na base |
| 4 | Cap de upgrade pelo Castelo (HQ gate) | HQ como coração da base |
| 5 | Multiplicador de gather por nível do nó + herói | Gathering > farm | ✅ beta: Vortex x1.10 + Lab +5% |
| 6 | 2ª marcha **ou** fila clara “próxima marcha” | Sensação de idle map | ✅ fila 1+1 no `MarchService` |

### 🔴 Explicitamente fora da beta 0.1

- Shooter stages / fog via stages  
- Gacha, seasons, battle pass, VIP  
- Alliance / PvP / city war  
- 76 prédios / decorações  
- Trucks, radar, arena  
- Steel age / T10 troops  

---

## 5. Tradução de “edifícios” (LZ → Valgor)

| Prioridade | Last Z | Valgor ID | Papel na beta |
|------------|--------|-----------|---------------|
| P0 | HQ | `castle` | Cap + identidade |
| P0 | — (fantasy) | `dragon-tower` | Core Valgor (não existe em LZ) |
| P1 | Lab | `laboratory` | Pesquisa Coleta (+5% gather) ao Nv.1 |
| P0 | HQ queues | construção 1/1 + pesquisa 1/1 | Filas no HUD cidade (timer beta curto) |
| P0 | Floating collect | bolhas sobre fazenda/etc. | Valor + seta de upgrade no mundo |
| P1 | Farm / Lumber / Power | `farm` `lumbermill` `quarry` `mine` | Tutorial economia |
| P1 | Warehouse | `warehouse` | Capacidade |
| P2 | Hospital | `hospital` | Stub |
| P2 | Camps / Military | — ou `arena` | Depois: tropas/formação |
| P2 | Alliance Center | — | Pós-beta |
| P2 | Pub | — | Pós-beta |
| P3 | Hall of Honor | `academy` `institute` `temple` | Facção/lore |
| P3 | Market / Trade | `market` | Stub |

**Diferencial Valgor (não copiar LZ):** Torre dos Dragões + dragões como unidade de mapa/poder — o “Hatch of Legends” do LZ S4 é a prova de que **criar criatura na base** vende; Valgor coloca isso no centro desde a beta.

---

## 6. UX / sensação (o que roubar como *feel*, não como skin)

1. **Sempre há um próximo botão óbvio** (tutorial LZ = campaign; Valgor = `BetaJourneyGuide`).  
2. **Mapa deve mostrar movimento** (marcha no mundo — já iniciado).  
3. **Power score** mental: nível castelo + heróis + dragões (HUD “Poder” depois).  
4. **Idle útil:** mandar marcha e ver progresso (timer + marker).  
5. **Facção/identidade** visual na cidade e no roster (ouro envelhecido Valgor ≠ UI militar LZ).  

Identidade Valgor: reinos, dragões, heróis — **não** apocalipse zumbi / biker.

---

## 7. Fontes públicas (consulta)

- [LDShop beginner guide](https://www.ldshop.gg/blog/last-z/lz-beginner-guide.html)  
- [Building upgrade order](https://lastzguides.com/base-building-order.html)  
- [Resources / gathering](https://lastzguides.com/resources.html)  
- [Heroes / factions](https://lastzwiki.com/en/heroes.html)  
- [Season 4 creatures](https://lastzwiki.com/en/season-4.html)  

> Limite: guias de jogador ≠ GDD oficial. Revalidar com prints do build que você joga.

---

## 8. Ação imediata recomendada (implementação)

Ordem sugerida no Cursor (já alinhada ao repo):

1. ~~Ninho 3D na Torre + limpar PLACEHOLDER de dragão na HUD~~ ✅
2. ~~Encontro de mapa com painel de resultado~~ ✅
3. ~~Gateway de heróis real → poder na marcha (Vortex)~~ ✅ (`BetaHeroesGateway`, poder 280)
4. ~~Gate de upgrade pelo nível do Castelo~~ ✅

Quando houver prints do seu Last Z, anexar em `docs/design-references/` (city/world/ui) com nota “mood only”.
