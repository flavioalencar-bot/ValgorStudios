# 03 — Data bindings (dados reais)

| UI | Fonte | Notas |
|----|-------|-------|
| Nome | `LocalPlayerProfile.DisplayName` | real |
| Nível | `CityController.GetCastleLevel()` | = nível Castelo |
| XP barra | 0 | sistema XP de jogador ausente |
| Poder | — | `PlayerPowerSystemAvailable=false` (omitido) |
| VIP | — | `VipSystemAvailable=false` (omitido) |
| Online | literal "Online" | sessão local |
| Alimento/Madeira/Pedra/Minério/Ouro/Diamantes | `Wallet.Get` | abreviado na UI; full no tooltip |
| Fila construção | `DescribeConstructionQueue()` | timers reais |
| Fila pesquisa | `DescribeResearchQueue()` | estado real Coleta+ |
| Treino/Cura | Em breve | módulos ausentes |
| Marchas | "0 · ocioso" | contagem cidade local; mapa não injetado |
| Labels edifício | nível + DisplayName | 17 canônicos |
| Chat | `LocalSystemChatProvider` | só sistema/eventos locais |
| Heróis/Dragão/Mundo | Navigator real | — |
| Itens/Correio/Aliança/Passe/Eventos/Loja/Benefícios | disabled | Em breve |
