# VALGOR — Revalidação P0/P1 · Beta 0.2.1

**Data:** 2026-07-27  
**Build:** `C:\Valgor_Studio\builds\windows\Valgor-Beta-0.2.1\Valgor.exe`  
**Base:** auditoria `VALGOR_BETA_0_2_*` + patch `f2eefe4` + fechamento 0.2.1  
**Smoke:** exit **0** · `FormatException=0` · `NullReferenceException=0` · `WorldMap OK`  
**Evidência:** `docs/audits/beta-0.2.1-revalidation-evidence/`  
**Preservados:** `Valgor-Beta-0.1` · `Valgor-Beta-0.2`

Estados: **CORRIGIDO** · **PARCIAL** · **NÃO CORRIGIDO** · **NÃO REPRODUZIDO**

---

## Matriz P0 / P1

| Bug | Prioridade | Estado | Evidência | Observação |
|-----|------------|--------|-----------|------------|
| B0.2-001 World Map `FormatException` energia | P0 | CORRIGIDO | `smoke-0.2.1.log` (`FormatException=0`, `WorldMap OK`); `04-worldmap.png` Energia 100/100 | TryParse + seed completo; domínio numérico |
| B0.2-002 World Map viewport preto / NRE bootstrap | P0 | CORRIGIDO | `04-worldmap.png` ~147 KB (mapa legível); `NullReferenceException=0` | Antes ~63 KB tela preta na auditoria 0.2 |
| B0.2-003 Novo Jogo / Confirmar cortados ~1080×640 | P0 | CORRIGIDO | `00-main-menu.png`; ScrollView/centragem no menu (patch) | Sem botões cortados no smoke 1600×900; layout responsivo no código |
| B0.2-010 Barra Cidade/Heróis no Menu Principal | P1 | CORRIGIDO | `00-main-menu.png` — só card do menu, sem nav inferior | Nav só em City / Heróis / World Map |
| B0.2-011 PlayerPrefs Editor ≠ Player | P1 | CORRIGIDO | `continue-check.log` `hasProfile=True` `playerName=BetaSmoke`; registry `Software\Valgor Studios\Valgor` | Stores **permanecem separados** (Editor ≠ Player); `SaveDiagnostics` no log; exe usa store Player |
| B0.2-012 Missões stub (“em breve”) | P1 | CORRIGIDO | `08-missions-panel.png` — capítulo, objetivos, progresso, Recolher | 8 objetivos mínimos; sem campanha nova |
| B0.2-013 Códigos técnicos no HUD do mapa | P1 | CORRIGIDO | `04-worldmap.png` — “Veio de Ferro”, sem `tide-crab`/`ash-drake` | Labels de display |
| B0.2-014 Cards de heróis com títulos em vez de nomes | P1 | CORRIGIDO | `02-heroes-vortex.png` — **Lyra / Nyx / Selene** | `PendingNamePlaceholder` estava igual ao nome real; ResolveDisplayName endurecido |
| B0.2-015 Instabilidade World Map (OK numa run, preto noutra) | P1 | CORRIGIDO | Duas smokes 0.2.1 com `WorldMap OK` + mapa legível | Causa raiz: prefs parciais de energia |

### P0 abertos

**Nenhum.**

### Itens P2 tratados no patch (fora da matriz P0/P1, referência)

| Bug | Estado | Nota |
|-----|--------|------|
| B0.2-023 Development Build watermark | CORRIGIDO | Build sem `BuildOptions.Development`; ausente em `00-main-menu.png` |
| B0.2-025 Splash/Loading pouco evidenciados | PARCIAL | Smoke inicia após Splash/Loading; log: captura omitida. Fluxo existe nas cenas Bootstrap/Loading |

---

## Regressão jornada (smoke + Continuar)

| Etapa | Resultado |
|-------|-----------|
| Splash → Loading → Menu | OK (transição; Splash não PNG — timing smoke) |
| Menu · Beta 0.2.1 · Continuar | OK (`00-main-menu.png`, perfil BetaSmoke) |
| City · Castelo · Muralha · Fazenda · coleta · Ir · upgrade | OK (série `ux-*` / `art-*`) |
| Missões | OK (`08-missions-panel.png`) |
| Heróis · Vortex | OK (`02-heroes-vortex.png`) |
| Dragões · Alimentar | OK (`DebugFeedDragon` no log; `03-dragons-tower.png`) |
| World Map · nó · marcha · retorno City | OK (`04`–`07`) |
| Fechar → abrir → Continuar (save Player) | OK (`continue-check.log` hasProfile + name) |

---

## Parecer

**Beta 0.2.1 aprovada para fechamento funcional P0/P1.**  
Próximo foco: vertical slice **visual** (P2 arte) — sem novos sistemas de jogo.
