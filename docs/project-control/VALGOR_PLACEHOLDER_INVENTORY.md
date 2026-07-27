# VALGOR — Inventário de Placeholders (Beta 0.1)

**Atualizado:** 2026-07-27 · Patch arte mínima City (5 P0)  
**Escopo:** Main Menu, City, Heroes, Dragons, World Map

Classificação: **P0** bloqueia apresentação · **P1** muito provisório · **P2** aceitável na beta · **P3** detalhe futuro

---

## Main Menu

| Objeto | Função | Tipo atual | Arte necessária | Pri | Risco troca | Status |
|--------|--------|------------|-----------------|-----|-------------|--------|
| Fundo sólido UITK | Atmosfera | Cor plana `Background` | Pintura/castle skybox | P1 | Baixo | Melhorado (card/crest) |
| Logo “VALGOR” texto | Marca | Label UITK | Logotipo vetorial | P1 | Baixo | Texto + crest “V” |
| Botões menu | Navegação | UITK DeepBlue/ouro | Ícones de madeira/ouro | P2 | Baixo | Padronizado |
| Intro Vortex cards | Onboarding | Texto | Ilustrações | P2 | Baixo | OK beta |

## City — edifícios (silhuetas procedurais)

| Objeto | Função | Tipo atual | Arte necessária | Pri | Risco troca | Status |
|--------|--------|------------|-----------------|-----|-------------|--------|
| Castelo | HQ | Modular P0 (muralhas/torres/portão/praça) | Mesh/FBX final | P1 | Médio | **Arte mínima P0** |
| Fazenda | Comida | Casa + celeiro + campos + cercas | Barn final | P1 | Médio | **Arte mínima P0** |
| Armazém | Estoque | Galpão + caixas/barris + portão carga | Warehouse final | P1 | Médio | **Arte mínima P0** |
| Serraria | Madeira | Casa + toras | Sawmill | P1 | Médio | Distinto (kit) |
| Pedreira | Pedra | Blocos pedra | Quarry | P1 | Médio | Distinto (kit) |
| Mina | Ferro | Entrada escura | Mine mouth | P1 | Médio | Distinto (kit) |
| Academia | Conhecimento | Pedra + torre estudo + runas | Academy final | P1 | Médio | **Arte mínima P0** |
| Arena | Formação | Anel areia | Coliseu | P1 | Médio | Distinto (kit) |
| Hospital | Cura | Casa + cruz | Hospital | P1 | Médio | Distinto (kit) |
| Torre Dragões | Ninho | Torre alta + pouso + brasas | Dragon keep final | P1 | Médio | **Arte mínima P0** |
| Templo | Bônus | Pedra + colunas | Temple | P1 | Médio | Distinto (kit) |
| Mercado | Trocas | Barraca | Market stall | P1 | Médio | Distinto (kit) |
| Laboratório | Tech | Casa + orbe | Lab tower | P1 | Médio | Distinto (kit) |
| Institute | — | Casa | — | P3 | Baixo | Fora do foco UX |

## City — ambiente

| Objeto | Função | Tipo atual | Arte necessária | Pri | Risco troca | Status |
|--------|--------|------------|-----------------|-----|-------------|--------|
| Terreno/grama | Chão | Cube tint | Terrain paint | P2 | Baixo | OK |
| Praça/caminhos | Leitura | Cubes | Cobble textures | P2 | Baixo | Zonas + portões |
| Muralha | Limite + defesa | Edifício `wall` + anel visual por nível | Mesh final / segmentos | P1 | Médio | **Gameplay + visual por nível** |
| Árvores | Atmosfera | Cilindro+esfera | Tree prefabs | P2 | Baixo | OK |
| Indicadores coleta/upgrade | Feedback | Medalhão/chevron/badges | Ícones 2D finais | P2 | Baixo | **Ícones provisórios P0** |

## Heroes

| Objeto | Função | Tipo atual | Arte necessária | Pri | Risco troca | Status |
|--------|--------|------------|-----------------|-----|-------------|--------|
| Preview Vortex | Inspeção | FBX/dummy | Hero mesh final | P1 | Alto | Sanitizers anti-magenta |
| Cards roster | Lista | UITK | Cards ilustrados | P2 | Baixo | Polish visual |
| Heroínas | Roster | Placeholder | Arte consistente | P1 | Médio | Sem magenta |
| Facções | Labels | Texto | PT-BR | P2 | Baixo | PT-BR |

## Dragons

| Objeto | Função | Tipo atual | Arte necessária | Pri | Risco troca | Status |
|--------|--------|------------|-----------------|-----|-------------|--------|
| Ocupantes ninho | Presença | Sphere/Capsule | Silhueta dragão | P1 | Baixo | Ember/ash (não roxo) |
| Painel Torre | Status | UITK texto | Ícones fome/vínculo | P2 | Baixo | Via módulo existente |
| UI Dragons tab | HUD | Genérico | Painel dedicado | P2 | Médio | Reusa gateway |

## World Map

| Objeto | Função | Tipo atual | Arte necessária | Pri | Risco troca | Status |
|--------|--------|------------|-----------------|-----|-------------|--------|
| Terreno/oceano | Base | Cubes | Mapa painted | P1 | Baixo | Atmosfera melhorada |
| Nós | Seleção | Primitivos por kind | Ícones território | P1 | Médio | Silhuetas por tipo |
| Marcha | Exército | Capsule+cube | Unit icons | P2 | Médio | Placeholder OK |
| HUD filtros | UX | UITK | Ícones | P2 | Baixo | OK |

## Resumo

| Pri | Qtd aproximada | Ação Beta 0.1 |
|-----|----------------|---------------|
| P0 | Torre como marco + anti-magenta | Mitigado nesta sprint |
| P1 | Meshes finais edifícios/heróis/mapa | Silhuetas procedurais melhoradas; arte final depois |
| P2 | Texturas/ícones/HUD | Aceitável |
| P3 | Detalhes (Institute, props) | Adiado |

**Regra de troca:** preservar `definitionId`, `BuildingView`, colliders layer `Building`, filho `Visual`, e saves.
