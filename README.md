# Valgor Studios

Cliente Unity + backend .NET do ecossistema **Valgor**.

> **Beta 0.1 (offline)** · repositório [flavioalencar-bot/ValgorStudios](https://github.com/flavioalencar-bot/ValgorStudios)

---

## Estado atual da Beta (leia isto)

### O que funciona no executável

- **Main Menu** offline (Jogar / Continuar / Configurações / Sair)
- **City** com todos os edifícios atuais: seleção, menu contextual, Detalhes, Atualizar, pré-requisitos, botão Ir, coleta nos produtores, Torre com Dragões/Alimentar
- **Heroes** (roster + preview Vortex)
- **Dragons** via Torre / módulo existente (fome, vínculo, ninho)
- **World Map** (nós, marcha, filtros) — regras estáveis; arte provisória
- Save/reload local (`PlayerPrefs` / perfil local)

### O que é offline

- Sem login online obrigatório nesta beta
- Sem multiplayer, alianças, PvP, SvS
- Backend/API existem no monorepo, mas o **Player** da Beta 0.1 roda a jornada local

### O que é placeholder

- Silhuetas procedurais modulares (P0 Castelo/Torre/Fazenda/Armazém/Academia melhorados; demais ainda kit básico)
- Texturas noise provisórias (não atlas final)
- Ninho de dragões em primitivos
- Ver inventário: [`docs/project-control/VALGOR_PLACEHOLDER_INVENTORY.md`](docs/project-control/VALGOR_PLACEHOLDER_INVENTORY.md)

### O que NÃO faz parte da Beta 0.1

- Monetização / loja
- Comércio entre jogadores
- Árvore de pesquisa completa / religião / facção
- Novos heróis ou novos dragões além do módulo atual
- Menu central administrativo de construções

Build de referência: `builds/windows/Valgor-Beta-0.1/Valgor.exe`

---

## Arquitetura (monorepo)

```
client/   Unity 6 LTS (jogo)
server/   .NET 9 (API / domínio)
tools/    Testes de lógica do cliente
docs/     Controle de projeto e evidências
```

Fluxo do cliente: **Bootstrap → Loading → MainMenu → City ⇄ Heroes ⇄ World Map**.

---

## Testes rápidos

```powershell
dotnet test tools/Valgor.GameLogic.Tests/Valgor.GameLogic.Tests.csproj
dotnet test server/Valgor.sln
powershell -File scripts/build-windows-beta.ps1
powershell -File scripts/capture-checkpoint-evidence.ps1
```

---

## Documentação de controle

- [`docs/project-control/VALGOR_IMPLEMENTATION_STATUS.md`](docs/project-control/VALGOR_IMPLEMENTATION_STATUS.md)
- [`docs/project-control/VALGOR_NEXT_SPRINT.md`](docs/project-control/VALGOR_NEXT_SPRINT.md)
- [`docs/project-control/VALGOR_PLACEHOLDER_INVENTORY.md`](docs/project-control/VALGOR_PLACEHOLDER_INVENTORY.md)
- [`docs/architecture/city-building-context-ux.md`](docs/architecture/city-building-context-ux.md)
- [`CHANGELOG.md`](CHANGELOG.md)

---

## Stack

| Camada | Tecnologia |
|--------|------------|
| Client | Unity 6 LTS · URP · UI Toolkit · Input System |
| Backend | .NET 9 · EF Core · PostgreSQL · Redis |
| Testes | xUnit (GameLogic + Server) |
