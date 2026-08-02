# 08 — Auditoria no player

## Build

`builds/windows/Valgor-QA-Capital-Visual/Valgor.exe`  
Resolução base: **1080×640** (smoke `-checkpointSmoke -captureEvidence`)

## Checklist

| # | Item | Resultado |
|---|------|-----------|
| 1 | Enquadramento inicial | OK |
| 2 | 17 edifícios | OK (suíte + layout) |
| 3–5 | Seleção / painéis | OK (smoke UX) |
| 6–7 | Zoom / pan | OK (controles preservados) |
| 8 | Eixo Portão→Castelo→Torre | OK |
| 9 | Rios / pontes | OK (builder + overview) |
| 10 | Praça | OK |
| 11 | Distritos | OK (props + shots) |
| 12–15 | Residência / Fundição / Campo / Salão | OK (lógica + smoke) |
| 16 | Dragão presente | OK (Torre) |
| 17 | Dragão ausente (marcha) | Parcial* |
| 18–20 | Tiers I/III/VI | OK |
| 21–22 | Medium / High | Medium OK; High gates OK |
| 23–25 | Clique / sobreposição / HUD | OK (decor Ignore Raycast) |

\*Estado DEPLOYED não forçado no smoke desta sessão — regra lógica já coberta por testes DragonFoundation (P2 evidência visual).

Screenshots `01`–`20` em `screenshots/`.
