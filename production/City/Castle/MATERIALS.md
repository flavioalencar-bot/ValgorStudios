# Convenção de materiais — Castle Tier 1

Alvo URP (Lit ou material runtime-safe do projeto).

| Slot / nome sugerido | Uso | Notas |
|----------------------|-----|--------|
| `M_Castle_Stone` | muralhas, keep, torres | albedo pedra clara; tiling estável |
| `M_Castle_Roof` | telhados cônicos / pirâmide | azul real da referência |
| `M_Castle_Wood` | porta, mastros | madeira escura |
| `M_Castle_MetalGold` | finiais, águia, filetes | metal |
| `M_Castle_Banner` | panos das bandeiras | azul bandeira |
| `M_Castle_Crest` | brasão (porta + bandeiras) | ouro / detalhe |
| `M_Castle_WindowEmissive` | janelas | emissivo quente opcional |

## Nomes de nós (recomendado no GLB/FBX)

- `Gate_Main` — porta
- `Crest_Gate` — brasão na porta
- `Banner_*` + `Crest_Banner_*` — bandeiras com brasão
- `Tower_*` / `Keep` / `Roof_*`

Se o asset já trouxer brasões bakeados, os nós `Crest_*` são opcionais.
