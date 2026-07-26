# City Tests

Testes de lógica da Player City Foundation ficam em:

`tools/Valgor.GameLogic.Tests/CityFoundationTests.cs`

Cobertura:

- limites de câmera (`CityBounds`)
- seleção / desseleção
- saldo não negativo
- evento de recurso
- SceneIds + transições City ⇄ WorldMap
- sessão ativa

Execução:

```bash
dotnet test tools/Valgor.GameLogic.Tests
```
