# City Progression QA — evidências

## Causa do bug (duplo clique)
O modo QA só lia `-cityProgressionQA`. Abrir o exe sem argumentos deixava `IsActive=false`.

## Ativação definitiva
A build `Valgor-QA-City-Progression` compila com a define `VALGOR_CITY_PROGRESSION_QA`.
Duplo clique já entra em homologação (navega à City automaticamente).
CLI `-cityProgressionQA` continua válida no Editor / builds sem a define.

Build normal (betas) **não** inclui a define → sem QA.

## Save
`city-progression-qa` → `valgor.city.production.v1.city-progression-qa`

## Scripts
- `scripts/run-city-progression-qa.ps1`
- `scripts/validate-qa-city-progression-double-click.ps1`
- `scripts/build-qa-city-progression.ps1`

## Capturas
- `00-banner-resources-qa-panel.png` — banner + recursos QA + painel
- `01`…`08` — Nv.1→30 / tiers
- `09-qa-panel.png`
- `10-reload-nv30.png`
- `11-reset-nv1.png`
- `auto-test-report.txt`
- `double-click-boot.log`
