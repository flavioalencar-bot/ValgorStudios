# 07 — Known issues

## P0

Nenhum.

## P1

Nenhum bloqueante na auditoria 1080×640 (seleção Residência/Fundição/Campo/Salão/Muralha confirmadas via smoke + painéis).

## P2

- Enquadramento da câmera no smoke às vezes corta o Portão/Salão; painéis e labels confirmam IDs.
- Métricas Profiler (batches/tris/memória) e High quality não instrumentadas.
- Warnings `JobTempAlloc` durante smoke longo.
- Loop excessivo de log “Painel Abrir aberto” em alguns edifícios no smoke.
- Visuais de vários edifícios ainda em blockout (exceto Castelo detalhado).
- NRE em `BuildingView.SetConstructionProgress` durante Collect no smoke antigo — **corrigido** com guard `if (!this)`.

## Notas

- `lumbermill` = Serraria (alias `sawmill`).
- Casas decorativas ≠ Residência oficial.
