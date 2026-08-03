# 05 — Validação de vias (preview)

## Eixos desejados (V2)

| Via | Rota |
|---|---|
| Imperial | Portão → Praça → Castelo |
| Torre | Castelo → Torre dos Dragões (NE) |
| Mercado | Praça → Mercado (sul/centro) |
| Militar | Praça → Arena / Campo (SO) |
| Sagrado | Praça → Templo (leste) |
| Produção | conexões periféricas (farm, mill, quarry, mine) |

## Etapa A

- Posições dos edifícios mudam no preview; vias geradas a partir do layout ativo herdam a nova disposição.
- Não há promoção de malha de vias “oficial” nova sem aprovação do preview.
- Invariante: evitar ruas atravessando footprints (validar visualmente nas capturas).

## Etapa B

Regenerar malha definitiva, testes de path e evidência de não-overlap com edifícios.
