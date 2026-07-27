# VALGOR — Agente Único (Carta de Responsabilidade)

**Documento:** `docs/project-control/VALGOR_SINGLE_AGENT.md`  
**Vigência:** 2026-07-27 · Decisão **D022**  
**Status:** O segundo agente (heróis dedicado) está **descontinuado**.

---

## Princípio

Um único agente Cursor é dono de **todo** o monorepo Valgor.  
Não há mais divisão “agente do jogo” × “agente de heróis”.

Objetivo: preservar o que já existe, evitar conflitos de edição, duplicações e regressões.

---

## Fonte única da verdade (ler antes de alterar código)

```text
docs/project-control/VALGOR_PRODUCT_MASTER.md
docs/project-control/VALGOR_IMPLEMENTATION_STATUS.md
docs/project-control/VALGOR_DECISIONS_LOG.md
docs/project-control/VALGOR_NEXT_SPRINT.md
docs/project-control/VALGOR_SINGLE_AGENT.md
docs/audits/VALGOR_CHECKPOINT_AUDITORIA_2026-07-26.html
CHANGELOG.md
README.md
```

Mensagens de chat **não** são prova técnica. Em conflito: prevalecem estes docs + executável + commits.

---

## Escopo sob responsabilidade

| Área | Paths / notas |
|------|----------------|
| Game Core / navegação | `Assets/_Valgor/**`, `Valgor.Core*` |
| City / edifícios / produção / coleta / upgrades / filas | `Assets/Valgor/City/**` |
| Heroes / Vortex / heroínas / poderes | `Assets/Valgor/Heroes/**`, `docs/game-design/heroes/**`, `production/Vortex/**` |
| Dragons | `Assets/Valgor/Dragons/**` |
| World Map / marchas | `Assets/Valgor/WorldMap/**`, cenas mapa em `_Valgor` |
| Save | PlayerPrefs / perfis locais (beta) |
| Backend / integração | `server/**` |
| Admin | `admin/**` |
| Builds / scripts / testes | `builds/`, `scripts/`, `tools/`, CI |
| Documentação | `docs/**` |
| Assets / pipeline | `production/`, `assets/`, import Unity |

---

## Regras anti-conflito (mesmo com agente único)

1. **Não duplicar** lógica de heróis fora de `Assets/Valgor/Heroes/**` — usar `IHeroesGateway`.
2. **Não duplicar** lógica de dragões fora de `Assets/Valgor/Dragons/**` — usar `IDragonGateway`.
3. City / World Map consomem módulos via contratos em `Valgor.Core.Modules`.
4. Modelos reais aprovados (ex.: Vortex) **não** voltam a dummy (D010).
5. Features fora da Beta 0.1 (alianças, PvP, SvS, gacha, shooter) só com decisão nova no log.
6. Bloqueio visual crítico congela features novas (D012).
7. Evolução = executável + evidência, não quantidade de classes (D011).
8. Seguir `VALGOR_NEXT_SPRINT.md` até nova priorização explícita.

---

## O que mudou vs modelo antigo

| Antes | Agora |
|-------|--------|
| Agente A: jogo (proibido editar Heroes interno) | Agente único edita Heroes quando necessário |
| Agente B: heróis (`game-design/heroes` + `Heroes/**`) | Descontinuado |
| Risco de PRs paralelas / overlap | Uma linha de trabalho |

A **fronteira de módulos** (assemblies/pastas) permanece — é arquitetura, não divisão de agentes.

---

## Checklist pré-alteração

- [ ] Li os docs de `project-control/` relevantes à tarefa
- [ ] A mudança está no escopo do próximo sprint (ou há pedido explícito do usuário)
- [ ] Não cria segundo caminho paralelo para a mesma feature
- [ ] Não reverte Vortex / arte aprovada
- [ ] Após mudança relevante: atualizar CHANGELOG e, se status mudar, `VALGOR_IMPLEMENTATION_STATUS.md`
