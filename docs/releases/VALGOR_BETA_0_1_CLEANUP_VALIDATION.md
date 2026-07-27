# Validação Beta 0.1 — limpeza final (2026-07-27)

Commit: `d9f2e3bef165dd58c2a5391099404866071e93c7`

## Parte 1 — Pendências técnicas

| Item | Resolução |
|------|-----------|
| Missing Scripts (2× GO `<null>`) | Assets URP stubados com GUIDs inválidos (`UniversalRenderPipelineAsset` / `UniversalRenderer`). GUIDs corrigidos para URP 17. **Zero** missing script no smoke novo. |
| Localização Addressables | PT-BR embutido; `LocalizationBootstrap` não inicializa o package; warning de catálogo removido. |
| `XInput1_3.dll` | `activeInputHandler: 1` (Input System only). Sem DLL legado. Teclado/mouse OK. Doc: `INPUT_SYSTEM_BETA_0_1.md`. |

## Parte 2 — Jornada

| Fluxo | Status |
|-------|--------|
| Splash → Loading → Main Menu | OK |
| Novo Jogo (nome 3–20) + recursos iniciais | OK |
| Intro Vortex (4 frases) | OK |
| Tutorial 11 passos | OK (código) |
| City / Heróis / Dragões / Mapa | OK (smoke) |
| Save + Continuar (última tela) | OK (código) |

## Build

`C:\Valgor_Studio\builds\windows\Valgor-Beta-0.1\Valgor.exe`  
672 256 bytes · 27/07/2026 12:19 · `Build Successful`

## Evidências

`docs/releases/beta-0.1-evidence/`

## Avisos restantes (não bloqueantes)

- Smoke: `Dispatch não encontrado` (reflexão do smoke; UI de marcha permanece no jogo)
- Licensing token ausente no batchmode Unity
- Build Development (marca d'água Unity em builds Dev)
