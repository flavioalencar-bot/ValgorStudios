# Valgor — Sprint recuperação visual da City (2026-07-26)

## Commit
Ver `git log -1` após o commit desta sprint.

## Executável
`C:\Valgor_Studio\builds\windows\Valgor-Checkpoint\Valgor.exe`

Assemblies atualizadas em `Valgor_Data\Managed\` (Valgor.City / Valgor.Runtime).

## Erros corrigidos
- Development Console oculto por padrão (`DeveloperConsoleGate`; F10 ou `-showDevConsole`/`-debug`).
- Localization/Addressables sem catálogo: não inicializa LocalizationSettings → elimina `InvalidKeyException` / Nyx / SpecialLocaleSelector na tela.
- Addressables: fallback silencioso se `StreamingAssets/aa/settings.json` ausente.
- StackOverflow na City: `RefreshSelection` → `ForceApply` → `Production.Changed` (ciclo removido).
- HUD: barra superior só com nome/nível/recursos/energia; nav única inferior; tutorial compacto.
- Rótulos de edifício só na seleção; bolhas de coleta sem texto técnico.
- Meshes medievais provisórias + câmera/céu/névoa.

## Capturas
`C:\Valgor_Studio\builds\windows\Valgor-Checkpoint\evidence\`
- `01-mainmenu.png`
- `02-city-clean.png`
- `03-city-building-selected.png`
- `04-heroes-vortex.png`
- `05-worldmap.png`

Smoke: `Valgor.exe -checkpointSmoke -captureEvidence`

## Limitações restantes
- Arte ainda provisional (primitivas compostas), não final.
- Heróis: preview 360° pode cair em fallback magenta; IDs de facção em snake_case na lista.
- WorldMap fora do escopo desta sprint (HUD técnico / labels grandes ainda presentes).
- Watermark Unity “Development Build” permanece (build Dev).
- Missões = stub na nav.
