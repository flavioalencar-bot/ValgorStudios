"""7) Generate consolidated pending-art / pipeline report."""

from __future__ import annotations

from pathlib import Path

import bpy

from validate_scene import validate_scene
from validate_scale_pivot import validate_scale_pivot
from validate_humanoid_rig import validate_humanoid_rig
from validate_sockets import validate_sockets
from validate_materials_textures import validate_materials_textures
from export_fbx import _hero_mesh_candidates, _count_tris
from vortex_common import (
    REPORTS_DIR,
    REQUIRED_TEXTURES,
    HUMANOID_BONES_MIN,
    LOD0_TRIS,
    LOD1_TRIS,
    LOD2_TRIS,
    RIG_OBJECT_NAME,
    REQUIRED_COLLECTIONS,
)
from vortex_report import Report


def generate_pending_report(report: Report | None = None) -> Report:
    report = report or Report("Vortex — Pending Production Report")

    # Run structural validators into separate mini reports for summary
    scene_r = validate_scene(Report("scene"))
    scale_r = validate_scale_pivot(Report("scale"))
    rig_r = validate_humanoid_rig(Report("rig"))
    sock_r = validate_sockets(Report("sockets"))
    mat_r = validate_materials_textures(Report("mats"))

    for label, r in (
        ("Scene scaffold", scene_r),
        ("Scale / pivot", scale_r),
        ("Rig", rig_r),
        ("Sockets", sock_r),
        ("Materials / textures", mat_r),
    ):
        report.add(
            f"{label} hard failures",
            r.failed == 0,
            f"PASS={r.passed} FAIL={r.failed} WARN={r.warnings}",
            level="error" if r.failed else "info",
        )

    hero = _hero_mesh_candidates()
    report.add(
        "Final artistic mesh (not primitives)",
        len(hero) > 0,
        "MISSING — produce sculpted/modeled Vortex body+armor+cape+weapon" if not hero else f"{len(hero)} mesh(es)",
        level="warn" if not hero else "info",
    )

    # LOD structure emptiness
    for lod, budget in (("VORTEX_LOD0", LOD0_TRIS), ("VORTEX_LOD1", LOD1_TRIS), ("VORTEX_LOD2", LOD2_TRIS)):
        col = bpy.data.collections.get(lod)
        meshes = [o for o in (col.objects if col else []) if o.type == "MESH"]
        tris = sum(_count_tris(o) for o in meshes) if meshes else 0
        report.add(
            f"{lod} has real mesh in budget {budget[0]}–{budget[1]} tris",
            bool(meshes) and budget[0] <= tris <= budget[1],
            f"meshes={len(meshes)} tris≈{tris}",
            level="warn",
        )

    # Rig completeness
    arm = bpy.data.objects.get(RIG_OBJECT_NAME)
    bones = {b.name for b in arm.data.bones} if arm and arm.type == "ARMATURE" else set()
    missing_bones = [b for b in HUMANOID_BONES_MIN if b not in bones]
    report.add(
        "Humanoid rig complete",
        len(missing_bones) == 0,
        f"missing {len(missing_bones)} bones" if missing_bones else "complete",
        level="warn",
    )

    # Textures on disk
    tex_dir = Path(r"C:\Valgor_Studio\production\Vortex\textures")
    missing_tex = [t for t in REQUIRED_TEXTURES if not (tex_dir / t).is_file()]
    report.add(
        "All brief textures on disk",
        len(missing_tex) == 0,
        f"missing {len(missing_tex)}/{len(REQUIRED_TEXTURES)}" if missing_tex else "all present",
        level="warn",
    )

    # Animations
    actions = [a.name for a in bpy.data.actions]
    report.add(
        "Animation actions present",
        len(actions) > 0,
        f"actions={actions}" if actions else "none — need Idle…Death + Special_Power",
        level="warn",
    )

    # Artistic backlog (explicit)
    report.notes.extend(
        [
            "PRODUZIR ARTISTICAMENTE (fora de primitivas técnicas):",
            "1) Corpo/rosto masculino adulto de Vortex (semi-realista, ~2,05 m) conforme prancha aprovada.",
            "2) Cabelo longo escuro + barba definida.",
            "3) Armadura medieval-fantástica preta/dourada com motivos dracônicos e ombreiras.",
            "4) Capa preta com bordados dourados.",
            "5) Espada dracônica com gema vermelha / brasa alaranjada.",
            "6) Texturas PBR listadas no brief (Body/Armor/Cape/Weapon/Hair/Eyes).",
            "7) Rig Humanoid completo (ossos mínimos Unity) + skinning.",
            "8) LODs 0/1/2 dentro do orçamento de triângulos.",
            "9) 16 animações (Idle…Death) incluindo Special_Power (Domínio do Rei).",
            "10) Retrato/portrait e VFX/SFX ficam para etapas posteriores no Unity.",
            "NÃO usar o cubo Vortex_HeightReference_2_05m nem o GroundReference como arte final.",
        ]
    )

    # Collection presence quick list
    present = {c.name for c in bpy.data.collections}
    missing_cols = [c for c in REQUIRED_COLLECTIONS if c not in present]
    if missing_cols:
        report.notes.append("Coleções ausentes: " + ", ".join(missing_cols))

    return report


if __name__ == "__main__":
    r = generate_pending_report()
    out = r.write(REPORTS_DIR / "07_pending_report.txt")
    print(r.to_text())
    print(f"Wrote {out}")
