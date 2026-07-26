# Coloque aqui o modelo base de Vortex

Arquivos aceitos (prioridade):

1. `Vortex_Base.glb`
2. `Vortex_Base.fbx`
3. `Vortex_Base.gltf` (alternativo)

## Importação automática

```bat
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b "C:\Valgor_Studio\production\Vortex\Vortex_Production.blend" --python "C:\Valgor_Studio\production\Vortex\import_vortex_base_model.py"
```

Ou watcher:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Valgor_Studio\production\Vortex\watch_source_and_import.ps1"
```

## Regras

- Não substitui arte marcada como aprovada (`VALGOR_APPROVED_ART`).
- Ajusta altura para ~2,05 m, pivô nos pés, organiza coleções.
- **Não** exporta para Unity até aprovação visual.
