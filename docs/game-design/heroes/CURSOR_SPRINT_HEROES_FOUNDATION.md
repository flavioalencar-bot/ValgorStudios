# CURSOR — SPRINT HEROES FOUNDATION

Use `VALGOR_HEROES_MASTER.md` and `heroes.seed.json` as the source of truth.

## Deliverables

- Import and validate the seed.
- Create backend migrations and seeds.
- Create Unity ScriptableObject definitions.
- Implement faction enums and circular advantage resolver.
- Implement faction composition bonus calculator.
- Implement special power READY/ACTIVE/COOLDOWN state machine.
- Implement authoritative activation endpoint.
- Implement roster filters and hero cards.
- Implement cooldown button and radial timer.
- Implement magic effect abstraction.
- Add unit, integration, EditMode and PlayMode tests.
- Add Addressables placeholders for hero prefabs, portraits, VFX and skins.
- Document replacement of dummy assets.
- Update README and CHANGELOG.
- Commit and push.

## Mandatory tests

1. Rosa beats Guarda.
2. Guarda beats Asas.
3. Asas beats Rosa.
4. Bonuses 3, 3+2, 4 and 5 return exact approved multipliers.
5. Special cannot activate during cooldown.
6. Reconnection restores active/cooldown timestamps.
7. Duplicate activation is idempotent.
8. Invalid owner/hero/battle requests are rejected.
9. Catalog returns all 11 characters.
10. Names marked “A definir” remain valid through internal IDs.
