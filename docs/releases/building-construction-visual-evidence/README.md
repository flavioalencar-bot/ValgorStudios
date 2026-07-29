# Building Construction Visual — evidências

Sprint: estado visual de construção/evolução (andaimes, poeira, cronômetro world-space).

## Build QA

`builds/windows/Valgor-QA-Building-Construction-Visual/Valgor.exe`

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-qa-building-construction-visual.ps1
```

## Capturas

| Arquivo | Conteúdo |
|---------|----------|
| `01-construction-in-progress.png` | Mundo sem modal: andaime + WorldUI no castelo |
| `01b-upgrade-modal-during-build.png` | Modal com upgrade em andamento (Atualizar bloqueado) |
| `02-construction-complete.png` | Após conclusão (sem obra) |
| `03-tier-after-build.png` | Tier pós-evolução |
| `auto-test-report.txt` | Relatório do auto-teste |

## Limitações desta sprint

- Andaimes procedurais Valgor em runtime (madeira/cordas/plataformas/escadas).
- Prefabs baked em `Art/Construction/Resources` existem para iteração de arte; o player usa o builder runtime para evitar magenta de material bake.
- `WorkAudio` estruturado sem clip.
- Poeira/debris via ParticleSystem leve.
- Duração QA de construção: 3s (`HomologDurationSeconds`) para capturas estáveis.
