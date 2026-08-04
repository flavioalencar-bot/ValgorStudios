# 08 — Zero Tripo usage

Tripo API calls nesta correção: **0**
Tripo tokens consumidos nesta correção: **0**

Proteções:
- `CityCapitalWorldNoTripo.NeverCallTripoApi`
- Flag `-capitalWorldNoTripoCorrection`
- `TRIPOS_PAUSED.flag`
- ImportCli local-only quando pausado/NoTripo
- Build script mata processos tripo antes do build
