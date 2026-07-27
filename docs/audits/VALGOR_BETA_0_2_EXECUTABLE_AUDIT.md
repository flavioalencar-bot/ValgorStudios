# VALGOR — Auditoria do executável Beta 0.2

**Data:** 2026-07-27  
**Commit:** `f732589` (`master`)  
**Executável:** `C:\Valgor_Studio\builds\windows\Valgor-Beta-0.2\Valgor.exe`  
**Build:** 2026-07-27 18:41:38 · 672256 bytes  
**Beta 0.1:** preservada (não sobrescrita)  
**Escopo:** auditoria de experiência — **sem correções de código**

## Método e evidências

| Fonte | Caminho | Notas |
|-------|---------|--------|
| Smoke integrado do exe 0.2 | `docs/audits/beta-0.2-executable-evidence/` (97 PNG + `smoke-audit.log`) | `-checkpointSmoke -captureEvidence` · exit **0** · 19:18–19:23 |
| Evidência anterior mesma build | `docs/releases/beta-0.2-evidence/vertical-slice/` | World Map ainda legível às 18:47 |
| Perfil Editor (sessão do jogador) | `HKCU\Software\Unity\UnityEditor\Valgor Studios\Valgor` | Perfil `flavio`, tutorialStep=6, wallet seed 5000/3000/… |
| Perfil Player (exe) | `HKCU\Software\Valgor Studios\Valgor` | Após smoke: `BetaSmoke`, lastScene=`City` |

**Importante:** Unity Editor e o Player Windows usam **PlayerPrefs separados**. A City com perfil novo no Editor **não** é o mesmo save do `Valgor.exe`.

## Jornada validada (Player exe)

```text
Splash/Loading → MainMenu → (perfil smoke) → City
→ edifícios / Detalhes / Atualizar / Coletar / Ir / Muralha
→ Heróis (Vortex) → Torre Dragões / Alimentar
→ World Map (FALHA VISUAL nesta corrida) → retorno City
→ quit → reopen (prefs persistem)
```

Smoke marca tutorial completo e pula UI de Novo Jogo; Novo Jogo/UI foi auditado à parte (ver bug list).

---

## Resultados por área

### HUD City — APROVADO (com provisos)

- Nome, Nv, Ouro/Comida/Madeira/Pedra/Ferro/Essência/Diamantes, Energia, **Construção 0/1 · livre**.
- Evidência: `01-city.png`, `ux-10-upgrade-complete.png`, `ux-32-wall-details.png`.

### Edifícios — APROVADO (parcial)

Catálogo (15): castle, farm, lumbermill, quarry, mine, warehouse, academy, institute, hospital, market, temple, dragon-tower, arena, laboratory, wall.

| Edifício | Seleção | Detalhes | Atualizar | Observação |
|----------|---------|----------|-----------|------------|
| Castelo | OK | OK | OK / bloqueado | `ux-01`, `vis-08`, `vis-09` |
| Fazenda | OK | — | bloqueio+Ir | Coletar OK `ux-03`/`ux-04` |
| Serraria | OK | OK | OK | `ux-15`–`17` |
| Pedreira | OK | — | OK | `ux-18`–`19` |
| Mina | OK | — | OK + upgrade válido→Nv.1 | `ux-08`–`10` |
| Armazém | OK | OK | bloqueio+Ir | `ux-05`, `ux-11`/`12` |
| Academia | OK | OK | OK | `ux-22`–`24` |
| Arena | OK | OK | bloqueado+Ir | `ux-25-*` |
| Hospital | OK | OK | bloqueado+Ir | `ux-26-*` |
| Torre Dragões | OK | OK | bloqueado+Ir | `ux-27-*` + Feed |
| Templo | OK | OK | bloqueado+Ir | `ux-28-*` |
| Mercado | OK | OK | bloqueado+Ir | `ux-29-*` |
| Laboratório | OK | OK | bloqueado+Ir | `ux-30-*` |
| Muralha | OK | OK | bloqueado+Ir | `ux-31`–`35`, log `Building clicked: wall` |
| Instituto | **SEM EVIDÊNCIA UX** | — | — | Existe no catálogo/save; **não** coberto pelo smoke |

### Muralha — APROVADO (funcional) / REPROVADO (visual)

- Seleção por `BuildingView` e por **proxy de segmento** → `Building clicked: wall`.
- Detalhes: defesa/HP/resistência; bloqueio Castelo Nv.2; botão **Ir** foca Castelo (`ux-35`).
- Visual: segmentos soltos / anel pouco legível.

### Coleta / upgrade / Ir — APROVADO

- Coleta Fazenda: `ux-03` → `ux-04`.
- Upgrade válido (Mina 0→1) com progresso/conclusão: `ux-08`–`10`.
- Upgrade bloqueado com pré-req + **Ir**: Mina/`ux-10`, Fazenda/`ux-13b`, Armazém/`ux-12`, Muralha/`ux-35`.
- **Concluir Agora** aparece desabilitado fora de construção ativa (`ux-10`).

### Heróis / Vortex — APROVADO (provisório)

- Log: `HeroesDemo OK`.
- Vortex listado e selecionado (não dummy de lista).
- Retratos = iniciais; preview 3D = blocos; vários cards ainda mostram **título** em vez de nome curto.
- Evidência: `02-heroes-vortex.png`, `vis-10-heroes.png`.

### Dragões / Alimentar — APROVADO (integrado)

- Menu: Dragões / Alimentar / Detalhes / Atualizar.
- Painel com **Alimentar** + status (ex.: Filhote READY/hunger).
- Log: `DebugFeedDragon` / `DragonService.TryFeed`.
- Evidência: `ux-27-dragon-tower-open.png`, `ux-27-dragon-tower-feed.png`.

### World Map / marcha — REPROVADO nesta corrida (lógica parcial)

- Log às 19:23:
  - `FormatException` em `EnergyPersistenceRepository.LoadFromPrefs`
  - `NullReferenceException` em `WorldMapSceneHost` / bootstrap
  - Capturas `04`/`05`/`06`/`vis-12` ≈ **tela preta** (só nav inferior)
- Às **18:47** (mesma build) o mapa ainda era legível (`vertical-slice/04-worldmap.png` ~135 KB).
- Prefs de worldmap/meta gravados mesmo assim; smoke registrou `WorldMap OK` e concluiu a jornada.
- Integração de marcha: **instável** sob falha de energia; evidência visual boa só na corrida das 18:47.

### Retorno à City — APROVADO (cena)

- `07-city-return.png` mostra City após mapa; nav inferior OK.
- HUD superior pode aparecer incompleto em alguns frames (provisório).

### Save / fechar / reabrir / Continuar — APROVADO (Player)

Após smoke + quit:

| Chave | Valor |
|-------|--------|
| name | `BetaSmoke` |
| lastScene | `City` |
| tutorialStep | 11 |
| city / dragons / worldmap meta | presentes |

Continuar no menu: esperado com perfil; captura visual de reopen foi contaminada por sobreposição de janelas — **persistência confirmada via registry**, não por PNG.

### Missões — REPROVADO

- Botão existe; ação = toast “Missões em breve.” (`BetaNavigationBar`).

### Menu / Novo Jogo — REPROVADO (robustez)

- Nav inferior vaza no Main Menu (`00-main-menu.png`).
- Em janelas ~1080×640 o card cola à direita e **corta** botões (Sair / Confirmar).
- Fundo preto; watermark Development Build.

---

## Telas — veredito

| Tela | Veredito |
|------|----------|
| Splash / Loading | Não evidenciado de forma isolada |
| Menu Principal | **Reprovado** (nav leak + clipping) |
| City + HUD | **Aprovado** (arte provisória) |
| Painéis Detalhes / Atualizar | **Aprovado** |
| Heróis | **Aprovado** (provisório) |
| Dragões / Torre | **Aprovado** |
| World Map | **Reprovado** (preto + exceptions nesta corrida) |
| Missões | **Reprovado** (stub) |
| Continuar / save Player | **Aprovado** (prefs) |

## Falhas na jornada

1. World Map: `FormatException` energia → NRE → viewport preto (19:23).  
2. Instituto sem passagem UX automatizada.  
3. Novo Jogo frágil em resolução baixa.  
4. Save Editor ≠ Save Player (risco de confusão na auditoria “perfil novo”).

## Parecer final

A Beta 0.2 entrega um **vertical slice jogável na City** (edifícios, muralha clicável, coleta, upgrades, Ir, HUD de construtores, Heróis com Vortex real, Torre/Alimentar) e **persiste save no Player**.  

Não está pronta como fatia ponta-a-ponta estável: **World Map quebrou nesta corrida**, Missões é stub, menu principal vaza HUD e a arte permanece silhueta.  

**Parecer:** *condicional / não go* para chamar a jornada Beta 0.2 “completa e estável” até corrigir P0/P1 (mapa/energia, menu, dual-save Editor×Player, Missões ou remoção do botão).

Ver também: `VALGOR_BETA_0_2_BUG_LIST.md`, `VALGOR_BETA_0_2_VISUAL_GAP.md`.
