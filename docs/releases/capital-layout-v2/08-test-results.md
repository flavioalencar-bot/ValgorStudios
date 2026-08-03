# 08 — Resultados de testes (Etapa A)

## Suite

```text
dotnet test tools/Valgor.GameLogic.Tests --filter "FullyQualifiedName~CityCapital"
```

## Resultados (última execução local)

| Grupo | Resultado |
|---|---|
| `CityCapitalLayoutV2Tests` | Aprovado |
| `CityCapitalAxisAlignmentTests` | Aprovado (reset V1) |
| Demais `CityCapital*` | Aprovado |
| Filtro `CityCapital*` | **13/13** |
| Filtro ampliado (layout/selection/camera/wall/official) | **26/26** |
| Player preview (`-capitalLayoutV2Preview`) | Capturas `02`–`11` geradas; log confirma status de aprovação visual |
| Migração de save | Bloqueada (nenhuma versão 2 persistida) |

## Cobertura V2 preview

| Teste | Garante |
|---|---|
| `V1_RemainsDefault_AndPassesLegacyAxis` | Default oficial = V1 |
| `V2_Preview_PassesOrganicInvariants` | Torre NE, Academia O, Campo SO, Mercado S/C, Arena E, Castelo recuado, Portão S |
| `V2_HasExactlyOfficialBuildings` | 17 IDs únicos |
| `Migration_IsBlocked_UntilPreviewApproved` | Sem migração de save |

## Não removido

Cobertura legacy de eixo V1 permanece. Invariantes orgânicas vivem só sob preview V2.
