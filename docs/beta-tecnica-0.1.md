# Beta Técnica 0.1

Fluxo navegável para validação visual/funcional.

```text
Bootstrap → Loading → Main Menu → City → Heroes → Torre dos Dragões → World Map → City
```

## Como jogar no Editor

1. Abra `client` no Unity 6000.0.58f2.
2. Play na cena `Assets/_Valgor/Scenes/Bootstrap.unity`.
3. Use a barra superior (Menu / Cidade / Heróis / Dragões / Mapa) ou os botões da City.

## Build Windows

Feche o Unity Editor neste projeto e execute:

```powershell
pwsh -File scripts/build-windows-beta.ps1
```

Saída esperada:

```text
builds/windows/Valgor-Beta-0.1/Valgor.exe
```

Menu no Editor: `Valgor/Build/Windows Beta Técnica 0.1`.

## Placeholders

Edifícios, logotipo e modelos de dragão estão marcados como **PLACEHOLDER**. Heróis usam Vortex real no preview quando disponível; demais usam fallback do módulo de heróis.
