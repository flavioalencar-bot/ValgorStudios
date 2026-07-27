# VALGOR — Visual gap Beta 0.2

**Data:** 2026-07-27 · Build `Valgor-Beta-0.2` · Commit `f732589`  
Auditoria visual apenas — **sem implementação**.

## Objetivo visual declarado vs executável

| Expectativa (macro / docs) | Estado no exe | Gap |
|----------------------------|---------------|-----|
| City “reconstrução visual completa” | Kits silhueta / low-poly | Alto |
| HUD / UI premium | Tema escuro + ouro legível, sem ícones | Médio |
| Heróis com presença | Iniciais + blocos 3D | Alto |
| Vortex real (não dummy) | Entrada de dados OK; modelo provisório | Médio (dados OK / arte NOK) |
| Dragões integrados | UI/Torre OK; mesh Torre simples | Médio |
| World Map legível | Intermitente; preto na corrida 19:23 | Crítico (funcional+visual) |
| Brand Splash/Loading | Não evidenciado | Médio |
| Sem watermark de desenvolvimento | “Development Build” permanente | Baixo–médio |

## Por superfície

### Menu
- Card funcional, tipografia OK.
- Fundo preto vazio; brand fraca.
- Layout quebra em resolução baixa (clipping).
- Nav de gameplay vazando no menu.

### City
- Castelo e alguns edifícios com mesh modular mínima (P0 art packs).
- Muitos volumes ainda genéricos; muralha = blocos.
- Iluminação dia/noite existe (evidência `art-09`) mas atmosfera limitada.
- Ícones flutuantes (coleta/status) legíveis, estética provisória.

### HUD
- Barra de recursos densa mas legível (PT-BR).
- Construtores visíveis (`Construção 0/1 · livre`).
- Sem iconografia; só texto.
- Painéis Detalhes/Atualizar consistentes (ouro/azul).

### Heróis
- Grid e filtros OK.
- Gap: portraits, modelo Vortex, VFX, animação.
- Nomes curtos vs títulos inconsistentes na UI.

### Dragões
- Integração de painel OK.
- Torre e criaturas sem arte final.

### World Map
- Quando saudável (18:47): terreno flat + shapes + painéis densos.
- Gap: identidade de mapa, ícones de nó, ausência de códigos técnicos na UI.
- Quando quebrado (19:23): gap total (preto).

## Matriz de aceite visual (Beta 0.2)

| Critério | Aceite? |
|----------|---------|
| Legibilidade das ações (Detalhes/Atualizar/Coletar/Ir) | Sim |
| Identidade visual “produto acabado” | Não |
| Mapa utilizável visualmente (corrida atual) | Não |
| Heróis apresentáveis a stakeholder externo | Não (protótipo) |
| City reconhecível como base | Sim (protótipo) |

## Conclusão visual

A Beta 0.2 é um **protótipo jogável com UI operacional**, não um vertical slice visual fechado. O maior regresso visual observado nesta auditoria é o **World Map preto** acoplado à falha de persistência de energia; o maior gap estrutural continua sendo **arte de personagens/mapa/muralha**.
