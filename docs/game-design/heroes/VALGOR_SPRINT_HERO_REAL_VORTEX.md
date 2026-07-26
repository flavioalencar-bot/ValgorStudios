# VALGOR — SPRINT HERO REAL: VORTEX

## Status (2026-07-26)

**Concluído para MVP jogável no preview:** modelo real aprovado, rig Humanoid, 16 animações mínimas, espada `Vortex_DragonSword`, Avatar Humanoid no Unity, Domínio do Rei (VFX ~10s), HeroesDemo sem dummy.

Pendências de arte (não bloqueiam preview): LOD1/2, texturas PBR canônicas separadas, refino manual de skinning/mocap.

## Objetivo

Substituir o `HumanoidDummy.prefab` pelo primeiro personagem 3D real do jogo: **Vortex, o Rei dos Dragões**.

Esta sprint deve deixar o projeto pronto para receber, validar, importar, rigar, animar e exibir o modelo final de Vortex no roster, preview 360° e cenas do jogo.

---

## Regra principal

O Cursor não deve tentar “inventar” um modelo 3D final apenas com primitivas.

Enquanto o arquivo 3D final não existir, ele deve:

1. preparar toda a pipeline de importação;
2. criar contratos, prefabs, sockets, materiais e animações;
3. validar automaticamente o arquivo quando ele for adicionado;
4. substituir o dummy imediatamente após a importação;
5. manter fallback técnico caso o asset esteja ausente.

---

## Identidade oficial de Vortex

- Único herói masculino.
- Personagem mais poderoso da franquia.
- Rei dos Dragões.
- Guerreiro adulto.
- Armadura medieval-fantástica negra e dourada.
- Presença imponente e nobre.
- Silhueta larga, atlética e reconhecível.
- Espada dracônica.
- Elemento: Chama Dracônica.
- Facção: Guarda da Ordem.
- Classe: Comandante Dracônico.
- Visual semi-realista: aproximadamente 60% realista e 40% estilizado.
- Não usar estética moderna, militar contemporânea, sci-fi ou zumbi.
- Não copiar personagens de outras franquias.

---

## Arquivos esperados do modelo final

```text
Assets/Valgor/Heroes/Characters/Vortex/
├── Models/
│   ├── Vortex_LOD0.fbx
│   ├── Vortex_LOD1.fbx
│   └── Vortex_LOD2.fbx
├── Textures/
│   ├── Vortex_Body_BaseColor.png
│   ├── Vortex_Body_Normal.png
│   ├── Vortex_Body_Mask.png
│   ├── Vortex_Armor_BaseColor.png
│   ├── Vortex_Armor_Normal.png
│   ├── Vortex_Armor_Mask.png
│   ├── Vortex_Weapon_BaseColor.png
│   └── Vortex_Weapon_Normal.png
├── Materials/
├── Animations/
├── Prefabs/
├── Portraits/
├── VFX/
├── Audio/
└── Data/
```

---

## Especificação técnica do modelo

### Escala e orientação

- Unity humanoide.
- Altura aproximada no jogo: 2,05 m.
- Unidade: metros.
- Eixo vertical: Y.
- Personagem olhando para +Z.
- Pivô no centro dos pés.
- Escala de importação: 1.
- T-pose ou A-pose compatível com Humanoid Avatar.

### Malha

Meta recomendada para mobile:

```text
LOD0: 55k–85k triângulos
LOD1: 25k–40k triângulos
LOD2: 8k–15k triângulos
```

- Evitar partes internas invisíveis.
- Armadura, corpo, cabelo e espada podem ser submeshes separados.
- Limitar quantidade de materiais.
- Preparar occlusion/culling.
- Não exceder orçamento sem registrar justificativa.

### Texturas

- Corpo e rosto: até 2048×2048.
- Armadura principal: até 2048×2048.
- Arma: até 1024×1024 ou 2048×2048.
- Máscaras compactadas para metallic, roughness/smoothness e AO.
- Compressão configurada para Android e iOS.
- Normal maps importados corretamente.
- Sem texturas 4K no MVP.

### Rig

- Unity Humanoid.
- Avatar válido.
- Root motion configurável.
- Ossos mínimos:
  - Hips
  - Spine
  - Chest
  - UpperChest
  - Neck
  - Head
  - braços completos
  - mãos
  - pernas completas
  - pés
- Ossos adicionais permitidos para:
  - capa;
  - cabelo;
  - ombreiras;
  - bainha;
  - acessórios.

---

## Sockets obrigatórios

```text
Socket_RightHand
Socket_LeftHand
Socket_BackWeapon
Socket_HipWeapon
Socket_HeadVFX
Socket_ChestVFX
Socket_FootLeftVFX
Socket_FootRightVFX
Socket_DragonLink
```

A espada deve poder ser anexada à mão, costas ou quadril.

---

## Animações mínimas

```text
Idle
Idle_Combat
Walk
Run
Turn_Left
Turn_Right
Attack_01
Attack_02
Heavy_Attack
Special_Power
Hit_Front
Hit_Back
Stun
Victory
Defeat
Death
```

### Poder especial

**Domínio do Rei**

- Duração ativa: 10 segundos.
- Recarga: 60 segundos.
- A animação deve:
  - erguer ou cravar a espada;
  - ativar runas douradas;
  - emitir energia dracônica;
  - criar aura de comando;
  - permitir integração futura com dragão.

---

## Materiais

Usar URP Lit ou Shader Graph compatível com URP.

Materiais mínimos:

```text
MAT_Vortex_Skin
MAT_Vortex_Hair
MAT_Vortex_ArmorBlack
MAT_Vortex_ArmorGold
MAT_Vortex_Cloth
MAT_Vortex_Eyes
MAT_Vortex_Sword
```

Características:

- metal negro com brilho controlado;
- ouro envelhecido;
- detalhes rúnicos emissivos;
- olhos com leve assinatura dracônica;
- sem excesso de bloom;
- iluminação neutra/fria.

---

## Prefab final

Criar:

```text
Assets/Valgor/Heroes/Characters/Vortex/Prefabs/Vortex_Hero.prefab
```

Estrutura esperada:

```text
Vortex_Hero
├── Model
├── Animator
├── HeroVisualController
├── HeroSocketRegistry
├── HeroMaterialController
├── HeroLODController
├── HeroVfxController
├── HeroAudioController
├── WeaponRoot
│   └── Vortex_DragonSword
└── PreviewAnchor
```

---

## Importador e validação

Criar:

```text
VortexAssetImportValidator
HeroModelImportProfile
HeroPrefabBuilder
HeroAvatarValidator
HeroTextureBudgetValidator
HeroMaterialValidator
HeroSocketValidator
HeroAnimationValidator
```

O menu do Unity deve incluir:

```text
Valgor
→ Heroes
→ Vortex
→ Validate Source Assets
→ Build Vortex Prefab
→ Open Vortex Preview
```

### Validações obrigatórias

- arquivo FBX existe;
- Avatar Humanoid é válido;
- escala correta;
- orientação correta;
- materiais atribuídos;
- texturas dentro do orçamento;
- sockets presentes;
- Animator Controller atribuído;
- LOD Group configurado;
- espada anexada;
- prefab Addressable;
- HeroDefinition aponta para o prefab final.

---

## Integração com catálogo

Atualizar somente o asset visual de:

```text
HERO_VORTEX_000
```

Não alterar:

- ID;
- nome;
- título;
- facção;
- classe;
- poder;
- duração;
- recarga;
- dados de combate.

Substituir:

```text
HumanoidDummy.prefab
```

por:

```text
Vortex_Hero.prefab
```

quando o prefab final estiver validado.

Se o asset estiver ausente, usar fallback técnico e registrar aviso legível.

---

## Preview 360°

No `HeroesDemo`:

- mostrar Vortex de corpo inteiro;
- centralizar automaticamente;
- rotação por drag;
- zoom por scroll/pinça;
- iluminação de estúdio;
- fundo escuro neutro;
- espada visível;
- animação Idle;
- botão para testar `Special_Power`;
- sem corte de pés ou cabeça.

---

## Performance mobile

- LOD automático.
- SkinnedMeshRenderer otimizado.
- Materiais compartilhados.
- Evitar scripts por frame sem necessidade.
- Culling correto.
- Addressables.
- Carregamento assíncrono.
- Descarregamento ao trocar de herói.
- Sem vazamento de RenderTexture ou materiais instanciados.

---

## Testes obrigatórios

### EditMode

- validação de escala;
- Avatar Humanoid válido;
- sockets obrigatórios;
- materiais obrigatórios;
- LOD Group;
- Addressable key;
- HeroDefinition aponta para o prefab correto.

### PlayMode

- Vortex aparece no preview;
- corpo inteiro visível;
- rotação funciona;
- zoom funciona;
- animação Idle toca;
- espada aparece;
- poder especial toca;
- troca para outro herói descarrega o modelo;
- retorno para Vortex recarrega corretamente;
- fallback funciona quando asset está ausente.

---

## Critérios de aceite

A sprint só pode ser considerada concluída quando:

- o modelo 3D final de Vortex estiver dentro do projeto;
- o prefab final tiver sido construído;
- Vortex aparecer no roster e preview 360°;
- corpo, rosto, armadura e espada estiverem visíveis;
- rig e animações funcionarem;
- materiais estiverem corretos;
- poder especial puder ser visualizado;
- dummy não for usado para Vortex;
- testes passarem;
- documentação e CHANGELOG forem atualizados;
- commit e push forem realizados.

---

## Mensagem pronta para o Cursor

```markdown
# VALGOR — SPRINT HERO REAL: VORTEX

Implemente integralmente a especificação contida em:

`docs/game-design/heroes/VALGOR_SPRINT_HERO_REAL_VORTEX.md`

Primeiro prepare toda a pipeline de importação e validação.

Não crie um falso modelo final com primitivas.

Quando o arquivo FBX ou GLB real de Vortex for adicionado, importe, valide, construa o prefab e substitua o dummy.

Não altere os dados de gameplay do personagem.

Pare somente se faltar o arquivo 3D fonte e informe exatamente:

- formato esperado;
- pasta;
- nome do arquivo;
- texturas faltantes;
- rig e animações faltantes;
- qual ferramenta externa precisa produzir cada item.

Antes de concluir:

- execute os testes;
- valide no `HeroesDemo`;
- atualize README e CHANGELOG;
- faça commit;
- faça push.
```
