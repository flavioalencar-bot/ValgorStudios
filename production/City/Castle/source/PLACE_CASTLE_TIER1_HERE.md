# Coloque aqui o Castelo Tier 1 (asset real)

Arquivos aceitos (um deles desbloqueia a integração):

1. `Castle_Tier1.glb`  ← preferido
2. `Castle_Tier1.fbx`

## Referência visual oficial

`docs/references/city/castle_tier1_reference.png`

## Regras

- Não substituir arte já marcada como aprovada.
- Escala alvo: footprint City ~6–8 u (ver validador).
- Pivô: centro da base no chão (pés / base da plataforma em Y=0).
- Frente do portão principal alinhada a **+Z** local (mesmo eixo do fallback procedural).
- Brasão deve estar na **porta** e nas **bandeiras** no modelo (ou slots nomeados).

## Validação

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File production/City/Castle/validate_castle_tier1_source.ps1
```

(Alternativa, se Python estiver no PATH: `python production/City/Castle/validate_castle_tier1_source.py`)

Enquanto o arquivo não existir, o status permanece:

`Castle Tier 1: BLOQUEADO POR ASSET REAL`
