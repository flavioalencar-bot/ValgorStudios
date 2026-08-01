# 05 — Performance

Meta: `1080×640`.

Medidas aplicadas no blockout:

- Primitivas Unity leves (sem mesh pesada nova)
- Colliders removidos de decoração/rios/vegetação
- Partículas limitadas (fumaça opcional por qualidade)
- Qualidade ambiental: `CityAmbientQuality` Low/Medium/High
- Fog exponencial; far clip 160

Registro runtime (preencher no player QA):

| Métrica | Tier I | Tier VI |
|---------|--------|---------|
| FPS médio | TBD | TBD |
| FPS mínimo | TBD | TBD |
| Draw calls | TBD | TBD |
| Batches | TBD | TBD |
| Triângulos | TBD | TBD |

Screenshots do player devem ser anexadas em `screenshots/` após build QA.
