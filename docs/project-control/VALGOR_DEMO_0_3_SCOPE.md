# VALGOR — Escopo Demo 0.3 (congelamento e mudança de estratégia)

**Documento:** `docs/project-control/VALGOR_DEMO_0_3_SCOPE.md`  
**Status:** CONGELADO — aguardando assets reais  
**Base funcional:** Beta 0.2.1 (`builds/windows/Valgor-Beta-0.2.1/Valgor.exe`, commit de fechamento P0/P1)  
**Atualizado:** 2026-07-27

---

## 1. Mudança de estratégia

A macro sprint de arte procedural / cubos / silhuetas **para aqui**.

| Antes | Agora |
|-------|--------|
| Vertical slice com arte mínima procedural | Vertical slice com **assets reais** (FBX/GLB) |
| Cubos/silhuetas podiam “fechar” visual | Cubos/silhuetas **não** contam como concluído |
| Macro sprints automáticas | Só implementação sob **ordem expressa** |

### Regras vigentes

1. **Cursor:** lógica, integração, prefabs, UI, save, testes e build.  
2. **Modelos visuais:** produzidos **fora** do Unity; entregues em FBX/GLB.  
3. Componente **não** pode ser marcado visualmente concluído com cubos, silhuetas ou procedural simples.  
4. Sem modelo real → registrar **`BLOQUEADO POR ASSET`**.  
5. **Não** substituir assets aprovados.  
6. **Não** iniciar novas funcionalidades sem ordem expressa.  
7. **Não** gerar mais macro sprints automáticas.  
8. Até ordem + assets: **não** alterar a City; **não** commit de arte procedural nova; **não** gerar nova build visual.

---

## 2. Fluxo da Demo 0.3

Fluxo único autorizado (demo jogável com assets reais nos pontos-chave):

```text
Menu
→ Novo Jogo
→ City
→ Castelo
→ Muralha
→ Heróis / Vortex
→ Torre dos Dragões
→ Dragão
→ World Map
→ marcha
→ retorno à City
```

Fora desse fluxo: **congelado** (sem evolução de produto nesta linha).

---

## 3. Conteúdo congelado

Não evoluir nesta Demo 0.3 (exceto ordem expressa futura):

- Demais edifícios da City além de Castelo + Muralha (Fazenda, Armazém, Academia, Arena, Hospital, Templo, Mercado, Laboratório, Serraria, Pedreira, Mina, etc.) — **lógica existente permanece**; **sem** rework visual
- Missões / campanha expandida
- Novos heróis além do pipeline Vortex já existente
- PvP, alianças, SvS, monetização, loja
- Novos sistemas de mapa / criaturas / territórios
- Macro patches automáticos de “recuperação visual” com procedural
- Rebuilds Windows só por arte procedural

**Builds congeladas (não sobrescrever sem pasta nova e ordem):**

- `Valgor-Beta-0.1`
- `Valgor-Beta-0.2`
- `Valgor-Beta-0.2.1`

---

## 4. Primeira entrega futura — DEMO CITY 0.3 — CASTELO E MURALHA

**Só inicia** quando os assets abaixo estiverem na pasta de entrada (ver §5).

| Arquivo obrigatório | Papel |
|---------------------|--------|
| `Castle_Tier1.fbx` **ou** `.glb` | Castelo nível visual Tier 1 |
| `Wall_Gate_Tier1.fbx` **ou** `.glb` | Portão principal |
| `Wall_Tower_Tier1.fbx` **ou** `.glb` | Torre de muralha |
| `Wall_Segment_Tier1.fbx` **ou** `.glb` | Segmento de muralha |

Até lá: implementação City **parada**; status dos itens = **`BLOQUEADO POR ASSET`**.

---

## 5. Pasta de entrada e manifesto (planejado — não criado nesta etapa)

Espelhar o padrão Vortex (`production/Vortex/source/` + staging + manifesto).

### Pastas previstas (criar somente na ordem de início da Demo City 0.3)

```text
production/City/
  Castle/                 ← Castelo Tier 1 (preparado; BLOQUEADO POR ASSET REAL)
    source/               ← DROP Castle_Tier1.glb|.fbx
    unity_staging/
    reports/
  source/                 ← demais assets City (muralha etc.) quando ordenado
```

### Destino Unity previsto (após ordem + assets)

```text
client/Assets/Valgor/City/Art/Castle/
client/Assets/Valgor/City/Art/Wall/
```

### Manifesto previsto (`unity_import_manifest.json`)

Campos mínimos:

- `demo`: `"0.3"`
- `slice`: `"castle-wall-tier1"`
- `blocked`: `true` enquanto faltar qualquer arquivo da §4
- `source_files`: lista dos quatro assets com path e extensão
- `unity_targets`: paths em `Assets/Valgor/City/Art/...`
- `do_not_replace_approved`: `true`
- `next_unity_steps`: import → prefab → wire no `CityEnvironmentBuilder` / views de `castle` e `wall`

**Nesta etapa:** pastas/manifesto **não** foram criados no disco — apenas especificados aqui, conforme ordem de criar **somente** este documento.

---

## 6. Auditoria dos pontos de importação (estado atual)

| Ponto | Situação | Notas |
|-------|----------|--------|
| Pipeline herói Vortex | Existente | `production/Vortex/` → `client/Assets/Valgor/Heroes/Characters/Vortex/` · manifesto `unity_staging/unity_import_manifest.json` · **não substituir** arte aprovada |
| City — Castelo visual | Procedural | `CityEnvironmentBuilder` + silhuetas/primitivos — **não** conta como concluído visual |
| City — Muralha visual | Procedural | `ApplyWallLevel` / `BuildWallRing` / `BuildMainGate` com `CreatePrimitive` — **não** conta como concluído visual |
| Prefab Addressables City | Ausente para castelo/muralha reais | Sem chave `city/castle/...` equivalente ao `heroes/HERO_VORTEX_000/prefab` |
| Drop folder City Tier1 | **Ausente** | Aguardando criação na ordem Demo City 0.3 |

Conclusão da auditoria: integração de **lógica** City/Castelo/Muralha já existe no executável 0.2.1; o **gancho visual real** ainda não tem pasta de entrada nem assets — tudo **`BLOQUEADO POR ASSET`**.

---

## 7. Responsabilidades do Cursor

| Faz | Não faz |
|-----|---------|
| Lógica, save, UI, navegação, testes | Modelar Castelo/Muralha/Dragão em DCC |
| Importar FBX/GLB entregues, prefabs, materiais URP | Marcar visual “OK” com cubos/procedural |
| Wire de IDs (`castle`, `wall`) sem quebrar colliders/seleção | Substituir assets aprovados |
| Build Windows **somente** sob ordem expressa | Macro sprints automáticas |
| Registrar `BLOQUEADO POR ASSET` quando faltar modelo | Alterar City / nova arte procedural / nova build **agora** |

---

## 8. Itens `BLOQUEADO POR ASSET`

| ID | Item | Motivo |
|----|------|--------|
| D03-BA-001 | Castelo visual Demo 0.3 | Sem `Castle_Tier1` em `production/City/Castle/source/` — ver `CASTLE_TIER1_STATUS.md` |
| D03-BA-002 | Portão da muralha | Sem `Wall_Gate_Tier1` |
| D03-BA-003 | Torre da muralha | Sem `Wall_Tower_Tier1` |
| D03-BA-004 | Segmento da muralha | Sem `Wall_Segment_Tier1` |
| D03-BA-005 | Prefabs City Art Castelo/Muralha | Dependem de D03-BA-001…004 |
| D03-BA-006 | Substituição do procedural de castelo/muralha no executável | Ordem + assets + integração |

*(Vortex já possui pipeline de asset real; polish visual adicional do preview permanece sujeito a assets aprovados — não reabrir com dummy.)*

---

## 9. Critérios de aceite (Demo 0.3 — quando desbloqueada)

### Aceite funcional (já coberto pela base 0.2.1; regressão sob ordem)

- Menu → Novo Jogo → City → seleção Castelo e Muralha → Heróis/Vortex → Torre/Dragão → World Map → marcha → retorno City
- Save/Continuar intactos
- Sem P0 de energia/mapa/menu reabertos

### Aceite visual (Castelo + Muralha Tier 1) — **só após assets**

- Modelos reais (não cubos/silhueta/procedural) no Castelo e na Muralha (portão, torre, segmento)
- Seleção/colliders/`definitionId` `castle` e `wall` preservados
- Sem magenta / missing material crítico
- Assets aprovados **não** sobrescritos
- Evidência PNG no executável **somente** após ordem de build

### O que **não** aceita como “visual concluído”

- `GameObject.CreatePrimitive` / silhuetas / arte mínima procedural atuais da City

---

## 10. Status agora

| Item | Estado |
|------|--------|
| Macro sprint atual | **PARADA** |
| City / arte procedural | **Não alterar** |
| Nova build | **Não gerar** |
| Demo City 0.3 — Castelo e Muralha | **`BLOQUEADO POR ASSET`** |
| Próxima ação humana | Entregar os 4 arquivos Tier1 na pasta de entrada (quando criada sob ordem) |
| Próxima ação Cursor | Aguardar ordem expressa + presença dos assets |

**Parecer:** congelamento registrado. Nenhuma implementação City, pasta física, commit de arte ou build nesta etapa — apenas este documento.
