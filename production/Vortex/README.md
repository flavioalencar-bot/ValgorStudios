# VALGOR — Vortex 3D Production Package

Pacote de produção Blender do Vortex (modelo Tripo aprovado + rig Humanoid + animações + espada).

## Arquivos principais

| Arquivo | Função |
|---------|--------|
| `VORTEX_3D_PRODUCTION_BRIEF.md` | Spec artística/técnica |
| `Vortex_Production.blend` | Cena de produção |
| `import_vortex_base_model.py` | Importa `source/Vortex_Base.glb\|fbx` |
| `rig_animate_weapon_vortex.py` | Rig Humanoid, 16 clips, espada, export FBX |
| `mark_visual_approved.py` | Libera gate visual / export Unity |
| `prepare_unity_import.py` | Staging + cópia para `client/.../Models` |
| `run_all_validations.py` | Validação automática completa |

## Pipeline atual (rig + animações)

```bat
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b "C:\Valgor_Studio\production\Vortex\Vortex_Production.blend" --python "C:\Valgor_Studio\production\Vortex\rig_animate_weapon_vortex.py"
```

Exporta:

- `export/Vortex_LOD0.fbx` (malha + armature + clips)
- `export/Vortex_DragonSword.fbx`

Depois copie para Unity ou use `COPY_INTO_UNITY=1` no `prepare_unity_import.py`, e rode:

`Valgor.Heroes.EditorTools.VortexUnityIntegration.IntegrateFromCommandLine`

## Pasta `source/`

Coloque aqui:

- `Vortex_Base.glb` **ou**
- `Vortex_Base.fbx`

Quando o arquivo aparecer, rode o import (ou o watcher):

```bat
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b "C:\Valgor_Studio\production\Vortex\Vortex_Production.blend" --python "C:\Valgor_Studio\production\Vortex\import_vortex_base_model.py"
```

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Valgor_Studio\production\Vortex\watch_source_and_import.ps1"
```

O import:

1. importa no `Vortex_Production.blend`
2. valida escala/orientação
3. ajusta para 2,05 m
4. aplica transforms
5. organiza coleções
6. valida materiais e rig
7. gera `reports/09_import_base_model.txt` e `reports/10_base_import_corrections.txt`
8. **não** substitui arte com `VALGOR_APPROVED_ART`
9. **não** exporta para Unity (`VALGOR_VISUAL_APPROVED=False`)

## Scripts auxiliares

1. `validate_scene.py`
2. `validate_scale_pivot.py`
3. `validate_humanoid_rig.py`
4. `validate_sockets.py`
5. `validate_materials_textures.py`
6. `export_fbx.py` — bloqueado até aprovação visual
7. `generate_pending_report.py`
8. `prepare_unity_import.py` — bloqueado até aprovação visual
9. `mark_visual_approved.py` — só após review humano
10. `ensure_placeholder_materials.py`

## Correção de proporção

```bat
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b "C:\Valgor_Studio\production\Vortex\Vortex_Production.blend" --python "C:\Valgor_Studio\production\Vortex\fix_vortex_proportions.py"
```

- Alvo: ~2,05 m e ~8,25 cabeças (pernas mais longas, cabeça relativamente menor)
- Atualiza guia técnico + sockets + previews em `previews/proportion_fix/`
- Se houver malha/rig importados: ajusta escala, ossos de perna/tronco/cabeça e aplica transforms
- Não libera export Unity

## Aprovação visual → Unity

Somente depois da review:

```bat
blender -b Vortex_Production.blend --python mark_visual_approved.py
blender -b Vortex_Production.blend --python export_fbx.py
```
