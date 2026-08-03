# 02 — Versionamento do layout

**Status Etapa A:** versionamento implementado; **migração de save bloqueada**.

| Constante | Valor | Uso |
|---|---:|---|
| `CapitalLayoutVersion` / `CityCapitalLayoutRuntime.Version1` | 1 | Oficial (default) |
| `CityCapitalLayoutRuntime.Version2` | 2 | Preview QA apenas |

## API

| Peça | Comportamento |
|---|---|
| `CityCapitalLayoutRuntime.ActiveVersion` | Versão efetiva em memória |
| `UseOfficialV1()` | Default seguro |
| `UsePreviewV2()` | Ativa V2 só para homologação (`-capitalLayoutV2Preview`) |
| `CityCapitalLayoutConfig.All` / `TryGet` / `GetPosition` | Roteia V1 ou V2 conforme `ActiveVersion` |
| `CityLayout.WorldPosition` | Continua fonte única → Logic Root → Visual Root |
| `CityCapitalLayoutMigration.TryMigrate` | **Sempre retorna false** na Etapa A |

## Persistência

- Etapa A: **não** grava `CapitalLayoutVersion = 2` no save.
- Etapa B (após aprovação visual): migrar, persistir versão 2, rollback QA para V1.

## Princípio

Offsets só no VisualRoot **não** são a solução definitiva. A fonte oficial de posição volta a ser o config versionado.
