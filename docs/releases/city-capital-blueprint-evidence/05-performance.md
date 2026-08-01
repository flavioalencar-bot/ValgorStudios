# 05 — Performance (player)

Resolução base da auditoria: **1080×640** (windowed).

Build: `builds/windows/Valgor-QA-Capital-Blueprint/Valgor.exe`  
Método: CheckpointSmoke `-checkpointSmoke -captureEvidence` + observação no player.

| Cenário | Resolução | Tier visual | Qualidade | FPS méd* | FPS mín* | Observação |
|---------|-----------|-------------|-----------|----------|----------|------------|
| A | 1080×640 | I | Medium (player default) | ~55–60 | ~40 | estável, sem travamento |
| B | 1080×640 | VI (force visual) | Medium | ~50–58 | ~35 | estável |
| C | 1080×640 | VI | High | n/d | n/d | não forçado nesta sessão — P2 |

\*Estimativa observacional no player Windows (sem Unity Profiler anexado). Sem crash; JobTempAlloc warnings pontuais no smoke (P2).

Batches / draw calls / tris / memória: não instrumentados com Profiler nesta sessão (P2). Medium aceitável para homologação.
