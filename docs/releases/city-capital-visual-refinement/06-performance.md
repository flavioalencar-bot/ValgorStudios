# 06 — Performance

Build: `Valgor-QA-Capital-Visual`  
Medição: observação no player Windows + smoke 1080×640 (Profiler Unity não anexado nesta sessão).

| Cenário | Resolução | Tier | Qualidade | FPS méd* | FPS mín* | Observação |
|---------|-----------|------|-----------|----------|----------|------------|
| A | 1080×640 | I | Medium | ~50–58 | ~38 | estável |
| B | 1080×640 | VI | Medium | ~45–55 | ~32 | estável, props extras |
| C | 1080×640 | VI | High | ~40–50 | ~28 | utilizável; pássaros/dust extras |
| D | 1920×1080 | VI | High | n/d** | n/d** | captura overview; sem Profiler |

\*Estimativa observacional.  
\*\*Sem Unity Profiler anexado — batches/draw calls/tris/memória não instrumentados (P2).

Medium permanece configuração principal homologável. High sem travamento.
