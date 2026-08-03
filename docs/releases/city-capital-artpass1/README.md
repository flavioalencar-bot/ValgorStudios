# Capital — Art Pass 1.0 (Arquitetura e Materiais)

**Commit alvo:** `feat(city): art pass architecture and materials`  
**Build:** `builds/windows/Valgor-QA-Capital-ArtPass1/Valgor.exe`  
**Base preservada:** `67b1f26` (+ polish overview)

## Escopo (somente visual)

| Marco | Mudança |
|-------|---------|
| Castelo | Vestimenta externa (`CityCastleArtDressing`) — fundação, escadaria, contrafortes, jardins, tochas, brasões. Prefab lógico intacto. |
| Torre dos Dragões | Platô, pilares, braseiros, correntes, cristais, pad ampliado. `NestOccupants` / `DragonNestView` intactos. |
| Muralha | Blocos, ameias, passarelas, torres com bandeiras. Entidade lógica `wall` única. |
| Portão Principal | Arco, portas reforçadas, ferragens, ponte, iluminação. |
| Praça Central | Piso nobre, fonte, brasão, bancos, canteiros, estátuas, tochas (Ignore Raycast). |
| Estrada Imperial | Pavimento, desgaste, bordas de grama, drenagem. |

## Materiais

Biblioteca compartilhada (`CityArtMaterialLibrary` + `CityArtMaterialCatalog`):

1. pedra clara · 2. pedra escura · 3. madeira · 4. telhado · 5. terra · 6. grama · 7. água · 8. metal

GPU Instancing + `sharedMaterial`. Static batching em Terrain/Roads/Decor/Vegetation.

## Não alterado

IDs, posições lógicas, save, regras de edifícios, demais kits de edifícios, `DragonNestView`.

## Screenshots

Ver `screenshots/`.
