# 09 — Player audit (Etapa A)

## Como auditar

1. Build QA com define `VALGOR_CITY_PROGRESSION_QA`.
2. Rodar:

```text
Valgor.exe -cityProgressionQA -capitalLayoutV2Preview -screen-width 1600 -screen-height 900
```

3. Conferir banner: *Capital Layout V2 Preview — sem migração de save*.
4. Revisar pasta `screenshots/`.

## Checklist visual

- [ ] Castelo foco, levemente ao norte
- [ ] Torre no NE (não no eixo norte)
- [ ] Academia a oeste
- [ ] Campo no setor SO / militar
- [ ] Mercado sul/centro
- [ ] Arena a leste
- [ ] Portão ao sul
- [ ] Overview inclui Torre sem cortar (1080×640 e 1920×1080)
- [ ] Save **não** mudou de versão após fechar o player

## Status

Aguardando aprovação visual explícita antes da Etapa B.
