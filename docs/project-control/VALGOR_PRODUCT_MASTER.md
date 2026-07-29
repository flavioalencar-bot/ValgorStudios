# VALGOR — Plano Mestre do Produto

**Documento de controle:** `docs/project-control/VALGOR_PRODUCT_MASTER.md`  
**Data de consolidação:** 2026-07-27  
**Fontes:** README, CHANGELOG, `docs/architecture/*`, `docs/game-design/heroes/*`, `docs/design-references/*`, `docs/releases/*`, `docs/audits/VALGOR_CHECKPOINT_AUDITORIA_2026-07-26.html`, código em `client/`, `server/`, `admin/`, `production/`, `builds/`, `scripts/`  
**Regra:** mensagens de chat **não** são prova técnica.

---

## 1. Visão

| Dimensão | Definição aprovada |
|----------|-------------------|
| Gênero | Jogo mobile de **estratégia** medieval-fantástica |
| Núcleo | Cidade · Heróis · Dragões · Mapa mundial · Alianças (longo prazo) |
| Plataformas | **Android** e **iOS** (alvo de produto); Windows player usado para validação técnica da beta |
| Idioma | Primeira versão em **português do Brasil** |
| Ícone da franquia | **Vortex** — herói principal, Rei dos Dragões |
| Referência de sistemas | *Last Z* como inspiração de **loop** apenas (`docs/design-references/LAST_Z_SYSTEMS_MAP.md`) — **sem** copiar arte, nomes, zumbis ou shooter |
| Diferencial declarado | Torre dos Dragões + dragões como eixo (não existe em Last Z) |

### Fora da visão de produto (explícito)

- Shooter / stages de ação vertical  
- Estética zumbi / militar contemporânea / armas modernas  
- Gacha como núcleo da beta  
- Protótipos descartáveis (README: “base definitiva”)

---

## 2. Jornada principal do jogador (alvo de produto)

```text
Splash
→ Login / Novo Jogo
→ Cidade
→ edifícios (interação contextual)
→ heróis
→ dragões
→ mapa mundial
→ recursos
→ criaturas
→ cidades (nós / ocupação)
→ alianças
→ guerra territorial
→ Capital
→ Rei do Reino
```

### Estado da jornada na Beta Técnica 0.1 (evidência)

Fluxo **implementado e no executável Windows** (`docs/releases/VALGOR_BETA_0_1_CLEANUP_VALIDATION.md`, `PLAYER_JOURNEY_BETA_0_1.md`):

```text
Splash (Loading brand)
→ Main Menu (Novo Jogo / Continuar)
→ Intro Vortex
→ Cidade
→ Heróis (HeroesDemo)
→ Torre dos Dragões (na City)
→ World Map (recursos / marcha)
→ retorno City
→ Continuar (última tela)
```

**Ainda não na jornada jogável:** Login online, alianças, guerra territorial, Capital, Rei do Reino, PvP/SvS.

---

## 3. Cidade (direção aprovada)

| Princípio | Status no plano |
|-----------|-----------------|
| Cidade como **interface principal** / tela inicial pós-intro | Aprovado |
| Interação **direta** no edifício | Aprovado |
| Menu **contextual** no item selecionado | Aprovado (`city-building-context-ux.md`, commit `ac83ebe`) |
| Coleta e upgrade **sobre o próprio prédio** | Aprovado |
| Tempo e progresso visíveis no edifício | Aprovado (label 3D + painel) |
| **Nenhum** menu administrativo central de construções | Aprovado — proibido na beta |

### Edifícios (catálogo beta — 14 slots)

Castelo, Fazenda, Serraria, Pedreira, Mina, Armazém, Academia, Instituto, Hospital, Mercado, Templo, Torre dos Dragões, Arena, Laboratório  
Fonte: `BuildingCatalog` / `docs/architecture/player-city.md`.

---

## 4. Heróis (direção aprovada)

| Princípio | Fonte |
|-----------|--------|
| Vortex = único masculino do elenco principal atual; mais poderoso | `VALGOR_HEROES_MASTER.md` |
| Dez heroínas adultas, estilo medieval-fantástico | Seed / catálogo Heroes |
| Três facções (Rosa de Sangue, Asas do Amanhecer, Guarda da Ordem) | Game design + server |
| Poderes especiais com duração e recarga | Master + API battle special |
| Skins, progressão, formação, combate | Planejado; parcialmente no cliente/demo |
| Modelos 3D reais substituem dummies **progressivamente**; após aprovação, **não** reverter para dummy | Sprint Vortex + decisões |

---

## 5. Dragões (direção aprovada)

Ciclo de produto (arquitetura `docs/architecture/dragons.md`):

desbloqueio → ovo → crescimento → alimentação → fome → stamina → vínculo → evolução → recuperação → deploy → combate → presença visual no mapa

Persistência atual: `valgor.dragons.v4`. Fase 1: ovo via Castelo ≥ 20 → missão → conquista → incubação com cuidados → nascimento Nv.1 (`ember-whelp`). Catálogo: ember-whelp, ash-drake, portal-wyrm.

---

## 6. World Map (direção aprovada)

| Camada | No plano de produto |
|--------|---------------------|
| Regiões / territórios | Sim |
| Cidades / vilarejos / recursos / criaturas / dragões | Sim |
| Marchas | Sim (núcleo da beta) |
| Alianças / ocupação / guerra | Sim (pós-beta / longo prazo) |
| Capital / Rei do Reino | Sim (endgame) |

Beta 0.1: nós tipados, marcha, energia, gather, território **visual** fundação — sem guerra de alianças.

---

## 7. Identidade visual

| Elemento | Direção |
|----------|---------|
| Tom | Medieval-fantástico |
| Cores | Preto, dourado envelhecido, azul profundo (`BetaVisualTheme`) |
| Materiais | Pedra, madeira, pergaminho |
| Motivos | Dragões, reinos, heróis |
| Proibido | Zumbis, armas modernas, estética militar contemporânea |

Arte de produção atual: **Vortex** (FBX real em `production/Vortex` + pipeline Unity). City / World Map / Dragons: silhuetas / placeholders admitidos na beta técnica.

---

## 8. Arquitetura de plataforma (alvo)

```text
Unity (client) → Valgor.Api (.NET) → PostgreSQL / Redis
Admin React → Api
Workers (background)
```

Fonte: `README.md`.  
**Realidade Beta 0.1:** cliente **offline** (PlayerPrefs); API Heroes/Auth existe no monorepo mas **não** integra o fluxo jogável (`docs/audits/...2026-07-26.html`).

---

## 9. Critério de evolução do projeto

> Evolução é medida pelo **executável jogável** e evidências (smoke, capturas, logs), **não** pela quantidade de classes ou docs.

Bloqueios visuais críticos (magenta, missing scripts, splash quebrado) **congelam** novas features até correção — ver `VALGOR_DECISIONS_LOG.md`.

---

## 10. Governança de agente (D022)

A partir de 2026-07-27, **um único agente** é responsável por todo o monorepo (Game Core, City, Heroes, Dragons, World Map, save, backend, admin, builds, docs, testes, production).

O segundo agente (heróis dedicado) está **descontinuado**.  
Detalhes e checklist: `VALGOR_SINGLE_AGENT.md`.

Fronteiras de pasta (`Heroes/**`, `Dragons/**`, etc.) continuam como **arquitetura de módulos**, não como divisão entre agentes.

---

## 11. Documentos irmãos

| Arquivo | Função |
|---------|--------|
| `VALGOR_IMPLEMENTATION_STATUS.md` | Matriz épico×funcionalidade + auditoria de aderência |
| `VALGOR_DECISIONS_LOG.md` | Decisões oficiais e impacto |
| `VALGOR_NEXT_SPRINT.md` | Próximo foco após esta consolidação |
| `VALGOR_SINGLE_AGENT.md` | Carta do agente único (anti-conflito) |
