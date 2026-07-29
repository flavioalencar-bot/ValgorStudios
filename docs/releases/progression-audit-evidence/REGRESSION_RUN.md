# Regressão — 2026-07-29

## Runs

| # | Suite | Exe | Resultado |
|---|-------|-----|-----------|
| 1 | Building Construction Visual + UX + Instituto | `Valgor-QA-Building-Construction-Visual` | **PASS 17/0** (andaimes, timer, save Nv.30, institute lock→unlock→persist) |
| 2 | City Progression Smooth (tiers 1→6 até Nv.30) | `Valgor-QA-City-Progression-Smooth` | **PASS** (todos cruzamentos de tier + câmera Δ=0 + reload Nv.30) |
| 3 | Checkpoint smoke jornada | `Valgor-Beta-0.2.4` | **PASS exit 0** (~6 min); evidências núcleo em `checkpoint-smoke/` (full set em `builds/windows/Valgor-Beta-0.2.4/evidence`) |

## Asserts Instituto (suite 1)

```
[OK] institute-starts-locked
[OK] institute-unlocks-after-academy
[OK] institute-persists-unlocked
```

## Fix incluído

`CityController.RefreshSoftLocks()` — Instituto `Locked` → `Available` quando Academia ≥ Nv.1.  
Chamado em `SyncBetaProgress`, fim de construção e force-upgrade QA.
