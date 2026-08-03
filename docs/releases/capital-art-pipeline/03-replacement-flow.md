# 03 — Fluxo de substituição

```text
CreateBuildings
→ mesh proxy em Visual
→ CityArtReplacementController.ApplyAll
→ Resolve(buildingId, quality)
→ se HF/Med/Low disponível e validado:
     ocultar proxies
     Instantiate sob Visual/ArtHF
     strip colliders do prefab artístico
     RecacheRenderersAfterArtSwap
→ senão: nada (proxy permanece)
```

Garantias: mesmo BuildingId, nível, save, seleção, collider lógico no root.
