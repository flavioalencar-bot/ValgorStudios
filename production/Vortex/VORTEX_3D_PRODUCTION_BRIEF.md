# VALGOR — PRODUÇÃO 3D REAL DO VORTEX

## Objetivo

Produzir o modelo 3D final de **Vortex, o Rei dos Dragões**, pronto para Unity 6, Android e iOS, seguindo integralmente a prancha técnica aprovada.

## Fonte visual oficial

Usar como referência obrigatória a prancha técnica aprovada de Vortex:

- frente;
- lateral esquerda;
- lateral direita;
- costas;
- rosto e expressões;
- armadura;
- espada dracônica;
- paleta de cores;
- materiais;
- escala aproximada de 2,05 m.

Nenhuma parte visual deve ser redesenhada sem aprovação.

---

## Entregáveis obrigatórios

```text
Vortex_LOD0.fbx
Vortex_LOD1.fbx
Vortex_LOD2.fbx
Vortex_Avatar.fbx
Vortex_Animations.fbx
```

Texturas:

```text
Vortex_Body_BaseColor.png
Vortex_Body_Normal.png
Vortex_Body_Mask.png
Vortex_Armor_BaseColor.png
Vortex_Armor_Normal.png
Vortex_Armor_Mask.png
Vortex_Cape_BaseColor.png
Vortex_Cape_Normal.png
Vortex_Cape_Mask.png
Vortex_Weapon_BaseColor.png
Vortex_Weapon_Normal.png
Vortex_Weapon_Mask.png
Vortex_Hair_BaseColor.png
Vortex_Hair_Normal.png
Vortex_Eyes_Emission.png
```

---

## Direção visual

- Fantasia medieval sombria.
- Semi-realista: 60% realista e 40% estilizado.
- Armadura preta e dourada.
- Motivos dracônicos.
- Capa preta com bordados dourados.
- Espada com brilho de brasa alaranjado.
- Gema dracônica vermelha.
- Cabelo longo escuro.
- Barba adulta bem definida.
- Aparência nobre, poderosa e intimidante.
- Sem elementos modernos, sci-fi ou zumbis.

---

## Escala e orientação

- Altura: 2,05 m.
- Unidade: metros.
- Eixo vertical: Y.
- Frente: +Z.
- Pivô: centro dos pés.
- Escala Unity: 1.
- A-pose ou T-pose.
- Aplicar transforms antes da exportação.

---

## Malha

### LOD0

- 55k a 85k triângulos.
- Silhueta completa.
- Rosto de alta qualidade.
- Armadura detalhada.
- Capa com geometria otimizada.
- Espada separada.
- Cabelo com cards ou malha otimizada.

### LOD1

- 25k a 40k triângulos.
- Preservar rosto, ombreiras e silhueta.
- Reduzir detalhes secundários.

### LOD2

- 8k a 15k triângulos.
- Preservar leitura geral.
- Simplificar capa, cabelo e ornamentos.

---

## Materiais

```text
MAT_Vortex_Skin
MAT_Vortex_Hair
MAT_Vortex_ArmorBlack
MAT_Vortex_ArmorGold
MAT_Vortex_Cloth
MAT_Vortex_Eyes
MAT_Vortex_Sword
```

PBR metálico/roughness.

- Texturas até 2048 no MVP.
- Normal maps corretos.
- AO e roughness compactados.
- Emission somente em runas, olhos e espada.
- Sem excesso de bloom.

---

## Rig

Unity Humanoid.

Ossos mínimos:

```text
Hips
Spine
Chest
UpperChest
Neck
Head
LeftShoulder
LeftUpperArm
LeftLowerArm
LeftHand
RightShoulder
RightUpperArm
RightLowerArm
RightHand
LeftUpperLeg
LeftLowerLeg
LeftFoot
LeftToes
RightUpperLeg
RightLowerLeg
RightFoot
RightToes
```

Ossos adicionais:

```text
Cape_01..N
Hair_01..N
ShoulderArmor_L
ShoulderArmor_R
SwordSheath
Accessories
```

---

## Sockets

Criar empties/bones:

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

---

## Animações

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

### Special_Power — Domínio do Rei

- Erguer ou cravar a espada.
- Ativar runas douradas.
- Emitir energia dracônica.
- Aura de comando.
- Preparar socket para futura presença do dragão.
- Duração visual compatível com poder ativo de 10 segundos.

---

## Exportação FBX

- Selected Objects.
- Apply Transform.
- Forward: -Z Forward.
- Up: Y Up.
- Add Leaf Bones: desativado.
- Bake Animation: ativado apenas no arquivo de animações.
- NLA Strips: conforme necessidade.
- Simplify: 0.
- Armature e meshes selecionados.
- Texturas não embutidas; usar arquivos externos.

---

## Estrutura no projeto

```text
client/Assets/Valgor/Heroes/Characters/Vortex/
├── Models/
├── Textures/
├── Materials/
├── Animations/
├── Prefabs/
├── Portraits/
├── VFX/
├── Audio/
└── Data/
```

Arquivo principal:

```text
client/Assets/Valgor/Heroes/Characters/Vortex/Models/Vortex_LOD0.fbx
```

---

## Checklist de aprovação

- [ ] Rosto corresponde à prancha.
- [ ] Cabelo e barba correspondem à prancha.
- [ ] Armadura preta e dourada correta.
- [ ] Ombreiras dracônicas corretas.
- [ ] Capa e bordados corretos.
- [ ] Espada dracônica correta.
- [ ] Altura 2,05 m.
- [ ] Pivô nos pés.
- [ ] Humanoid Avatar válido.
- [ ] Sockets completos.
- [ ] LOD0/1/2.
- [ ] Materiais PBR.
- [ ] Animações exportadas.
- [ ] Preview 360° funcionando.
- [ ] Sem dummy para HERO_VORTEX_000.
