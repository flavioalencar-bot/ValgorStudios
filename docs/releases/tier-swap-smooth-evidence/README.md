# Context menu / tier-swap smooth — evidências

## Causa do tremor
1. `CastleTierTransition` fazia scale punch (0.82→1.0) e no fim forçava `localScale = Vector3.one`, apagando a escala de apresentação.
2. `OpenContextFor` aplicava `FocusOrthoSize` por tier no `FocusOn`, mudando o zoom ao (re)selecionar o Castelo.

## Correção
- Transição: fade ~0,4s no filho visual + brilho discreto; root lógico intacto.
- Câmera: `LockPose` / `SuppressFocus` durante a troca; sem override de ortho na seleção.
- Colliders do asset visual removidos; Rigidbody vira kinematic.

## Build
`builds/windows/Valgor-QA-City-Progression-Smooth/Valgor.exe`
