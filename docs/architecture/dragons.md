# Dragons Foundation

Sistema de dragões em `Assets/Valgor/Dragons`.

## Objetivo

**Fase 1 (oficial):** Castelo Nv.20 → desbloqueio → missão do Ovo → conquista → incubação com cuidados → nascimento Nv.1.

Fundação adicional (já no módulo, fora do escopo de combate completo): ninho na Torre, alimentação, recuperação e destaque opcional em marchas.

## Limite

Fronteira de módulo: não reimplementar heróis dentro de Dragons. Integração via `IHeroesGateway` e `IDragonGateway`. O agente único (D022) pode editar `Heroes/**` e `Dragons/**`, sem duplicar lógica entre pastas.

**Fora da Fase 1:** combate completo, PvP, montaria, habilidades avançadas, múltiplos dragões.

## Jornada do ovo (Fase 1)

```text
LOCKED → UNLOCKED (Castelo ≥ 20)
→ MISSION_ACTIVE (Aceitar missão)
→ EGG_OWNED (Buscar/conquistar ovo)
→ INCUBATING (iniciar + cuidados ≥ N)
→ BORN (Juvenile Nv.1)
```

| API | Papel |
|-----|--------|
| `SyncCastleLevel` | Espelha Castelo da City |
| `TryAcceptEggMission` | Desbloqueia missão |
| `TryConquerEgg` | Locked → Egg |
| `TryBeginIncubation` | Egg → Hatching |
| `TryCareIncubation` | Gasta comida; exige `CareRequiredForHatch` para nascer |

UI: painel **Dragões** na Torre dos Dragões.

## Entidades

| Tipo | Papel |
|------|--------|
| `DragonEggJourneyPhase` | Fase da jornada do ovo |
| `DragonDefinition` / `DragonCatalog` | Catálogo estático |
| `DragonInstance` | Instância (`DragonLevel`, `CareCount`) |
| `DragonStateMachine` | Grafo de transições |
| `DragonRepository` | Memória + PlayerPrefs (`valgor.dragons.v4`) |
| `DragonService` | Fachada `IDragonGateway` |
| `DragonRoost` | Ninho vinculado à `dragon-tower` |

## Estados oficiais

```text
LOCKED → EGG → HATCHING → JUVENILE → RESTING ⇄ READY
READY → DEPLOYED → EXHAUSTED|INJURED → RECOVERING → RESTING
```

## Seed (Fase 1)

- `dragon-ember-1` (`ember-whelp`) — **LOCKED** (ovo pendente da jornada)
- Um único dragão; sem seed Ready pré-nascido

## Integração

| Área | Comportamento |
|------|----------------|
| City | `SyncCastleLevel` no tick/bind; Torre: missão / conquistar / incubar / cuidar / alimentar |
| Recursos | Care usa Food; feed pós-nascimento usa Food + Essence |
| World Map | Despacho destaca 1 dragão READY (após maturação pós-nascimento) |
| Persistência | `valgor.dragons.v4` (fase, nível, cuidados) |

## Contratos Runtime

`IDragonModule`, `IDragonGateway`, `IDragonResourceWallet`, `DragonStatusInfo`, `ProvisionalDragonGateway`.
