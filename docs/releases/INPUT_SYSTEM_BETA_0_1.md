# VALGOR — Input System (Beta 0.1)

## Decisão

`PlayerSettings.activeInputHandler = Input System Package` (valor `1`).

Teclado e mouse usam o **Unity Input System** (`Keyboard.current`, actions em `_Valgor/Input`).

## Warning legado `XInput1_3.dll`

Em builds antigas (handler **Both**), o runtime Unity emitia:

```text
XInput1_3.dll not found. Trying XInput9_1_0.dll instead...
```

Isso vinha do probe de gamepad legado, **não** do código Valgor. Não era bloqueante.

## Beta 0.1

- Sem distribuição manual de `XInput1_3.dll`
- Input System only → teclado/mouse OK
- Gamepad opcional futuro via Input System (Windows.Gaming.Input), sem DLL antiga
