# 08 — Player audit

**Build:** `Valgor-QA-Capital-Final`  
**Flags:** `-cityProgressionQA`  
**Capturas:** `screenshots/` (01–20)

## Observado

- Painel jogador: nome real (`Flavio`), nível = Castelo (`Nível 1`), status Online
- Chips de recursos: 6 tipos, valores QA abreviados (`1000M` = wallet homologação)
- Filas: construção/pesquisa reais; treino/cura “Em breve”
- Chat: abas Aliança/Mundo/Sistema; mensagens locais de sistema
- Menu inferior: Cidade + shells Itens/Correio/Aliança + Heróis/Dragão/Mundo/Missões
- Labels mundo: formato `{nível} {nome}` nos oficiais visíveis
- 1080×640 e 1920×1080: HUD legível, sem botões essenciais cortados
- Dragão: sem duplicação observada; `DragonNestView` preservado

## Não declarar

Aproximação à referência artística **não** é “igual” — screenshots mostram composição HUD + Capital atual (Art Pass + densidade), não cópia da referência.

## Performance (qualitativo)

Sessão QA estável após boot; sem crash. Medição formal de FPS no profiler fica como follow-up se Medium degradar.
