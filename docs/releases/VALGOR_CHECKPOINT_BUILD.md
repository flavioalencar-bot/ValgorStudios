# VALGOR — Checkpoint Build (Windows Executável)

Data: **2026-07-26**  
Fonte de verdade da auditoria: `docs/audits/VALGOR_CHECKPOINT_AUDITORIA_2026-07-26.html`

## Resultado

**SUCESSO** — existe e foi executado:

```text
C:\Valgor_Studio\builds\windows\Valgor-Checkpoint\Valgor.exe
```

| Campo | Valor |
|-------|--------|
| Commit HEAD (repo) | `efb14db1204b9947b47b293a2551322945b35f91` |
| Nota | Working tree com alterações de recuperação **não commitadas** no momento da build |
| Caminho do `.exe` | `builds/windows/Valgor-Checkpoint/Valgor.exe` |
| Tamanho do stub `.exe` | 672 256 bytes |
| Tamanho total da pasta build | **~112,7 MB** (168 arquivos) |
| Tipo | Development Build · Script Debugging · Windows 64-bit · janela 1600×900 |
| Projeto fonte | **somente** `client/` |
| Scaffold | `builds/_unity-beta-project` marcado **OBSOLETO** (`OBSOLETE.md`) — não usado |

## Resultado da execução

| Checagem | Resultado |
|----------|-----------|
| Abrir `Valgor.exe` | OK |
| Splash/Loading → MainMenu | OK (log + captura) |
| City | OK (`[CheckpointSmoke] City OK`) |
| HeroesDemo (roster + Vortex no catálogo) | OK (`HeroesDemo OK`) |
| WorldMap + seleção de nó | OK (`WorldMap OK` + `Nó selecionado`) |
| Retorno à City | OK |
| Fechar (Application.Quit) | exit code **0** |
| Reabrir | OK (captura `07-relaunch.png`) |
| Jornada automatizada | `-checkpointSmoke` |

Logs salvos em:

- `docs/releases/checkpoint-logs/Player-smoke-journey.log`
- `docs/releases/checkpoint-logs/Player.log` (quando disponível em `%USERPROFILE%\AppData\LocalLow\Valgor Studios\Valgor\`)
- Build Unity: `builds/windows/checkpoint-build.log` / `checkpoint-build-2.log`

Capturas:

- `docs/releases/checkpoint-screenshots/02-mainmenu.png`
- `docs/releases/checkpoint-screenshots/03-city.png`
- `docs/releases/checkpoint-screenshots/04-heroes.png`
- `docs/releases/checkpoint-screenshots/05-worldmap.png`
- `docs/releases/checkpoint-screenshots/07-relaunch.png`

## Cenas acessíveis nesta build

1. Bootstrap  
2. Loading  
3. MainMenu  
4. City  
5. HeroesDemo  
6. WorldMap  

DragonTower **sem cena dedicada** — permanece botão de navegação/foco na City (fora do fluxo mínimo de cenas).

## Testes aprovados

| Suíte | Resultado |
|-------|-----------|
| `tools/Valgor.GameLogic.Tests` | **114/114** OK |
| `server` (Domain+Application+Api) | **23/23** OK |
| Unity EditMode Heroes | Não reexecutados nesta sessão (Editor fechado para batch build) |
| Unity PlayMode | Não bloqueantes (conforme escopo) |
| Compilação Unity player | **Build Successful** (sem `error CS`) |

## Correções feitas para destravar a build

1. **Pacotes:** removido `com.unity.textmeshpro` 3.2.0-pre.2 (incompatível com Unity 6); alinhado `com.unity.test-framework` a **1.5.1**; URP **17.0.4** + UGUI **2.0.0** via PackageCache do `client`.  
2. **Build path:** saída `Valgor-Checkpoint`; Development + AllowDebugging; Mono; 1600×900 Windowed.  
3. **Script:** `scripts/build-windows-checkpoint.ps1` (fecha Editor, usa só `client/`).  
4. **NRE WorldMap:** null-check em `WorldMapController.ApplyNodeVisibility` (views destruídas após unload).  
5. **QA:** `CheckpointSmokeDriver` ativo apenas com `-checkpointSmoke`.

## Warnings

- Centenas de `CS8632` (nullable annotations) em WorldMap/City — não bloqueiam build.  
- Licensing: `Access token is unavailable` (não impediu build).  
- HeroesDemo: aviso de fallback técnico do Vortex se o preview não resolver o prefab Addressable no player (`HeroPreview` log).  
- Capturas automatizadas às vezes incluem janelas vizinhas no desktop (BlueStacks/ads); as cenas Valgor estão identificáveis nas PNGs listadas.

## Erros restantes (não bloqueantes do fluxo mínimo)

- Integração client ↔ API ainda inexistente (offline/PlayerPrefs).  
- Arte City/WorldMap continua provisional (silhuetas).  
- Vortex 3D no player pode cair em fallback técnico dependendo de Addressables/path.  
- `MarchRepository` em memória; Workers/admin além do escopo deste checkpoint.

## Primeira limitação encontrada

**Build Windows dependia de fechar o Unity Editor** (lockfile em `client/Temp`) e de **não usar** o scaffold `builds/_unity-beta-project` (sem Assets). Tentativas anteriores falhavam por Package Manager ENOENT em cópia/temp; a resolução correta foi buildar o **`client/` real** com PackageCache já populado (URP/UGUI presentes).

## Como regenerar

```powershell
powershell -ExecutionPolicy Bypass -File scripts/build-windows-checkpoint.ps1
```

Validação rápida:

```powershell
& builds\windows\Valgor-Checkpoint\Valgor.exe -checkpointSmoke
```
