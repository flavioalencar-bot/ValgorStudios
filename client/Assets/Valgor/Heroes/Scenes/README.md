# HeroesDemo

Cena de validação visual do módulo de heróis.

## Caminho

`Assets/Valgor/Heroes/Scenes/HeroesDemo.unity`

## Como abrir

1. Abra o projeto `client/` no Unity Hub com **6000.0.58f2**.
2. No menu: **Valgor → Heroes → Rebuild Catalog And Demo Scene**
   - regenera o catálogo a partir do seed espelhado;
   - cria/atualiza `HeroesDemo.unity`;
   - abre a cena.
3. Pressione **Play**.

Atalhos:

| Menu | Ação |
|------|------|
| Valgor → Heroes → Rebuild Catalog From Seed | Só catálogo |
| Valgor → Heroes → Open Heroes Demo Scene | Abre/cria a cena |
| Valgor → Heroes → Validate Demo In Play Mode | Play + captura |

## O que a demo mostra

- Roster com 11 personagens
- Filtros: Todos / Rosa de Sangue / Asas do Amanhecer / Guarda da Ordem
- Cards: nome, título, raridade, facção, poder
- Painel de detalhe
- Preview 360° com cubo dummy colorido por facção
- Troca de personagem ao clicar no card
- Indicador READY / ACTIVE / COOLDOWN
- Botão para simular ativação do poder especial (cliente)

## Placeholders

- Prefabs 3D Addressables (`heroes/{id}/prefab`)
- Retratos (`heroes/{id}/portrait`)
- VFX/SFX de especial
- Cubo técnico no preview (não é arte final)
- Skin real da Consorte (só chave Addressable)

## Capturas

- UI (roster/filtros/detalhe/READY): `HeroesDemo_Preview.png`
- Play Mode preview 3D (RT do dummy humanoide): `HeroesDemo_Preview3D.png`
- Play Mode camera: `HeroesDemo_PlayMode.png`
- Relatório batch: `HeroesDemo_Validation.txt`

## Preview 3D

- Prefab: `Assets/Valgor/Heroes/Prefabs/HumanoidDummy.prefab`
- Layer: `HeroPreview`
- Câmera dedicada + luz + RenderTexture no painel `hero-preview-image`
- RT com `Contain` (corpo inteiro, sem crop de pés)
- Drag no painel para girar, scroll para zoom
- Layout: detalhe/poder no scroll superior; preview flexível abaixo (sem sobrepor o botão)
- Cores: Rosa de Sangue vermelho escuro · Asas azul · Guarda dourado
- Responsivo: coluna única abaixo de 900px / mobile
