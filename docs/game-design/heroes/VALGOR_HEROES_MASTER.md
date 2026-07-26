# VALGOR — BÍBLIA OFICIAL DE HERÓIS E IMPLEMENTAÇÃO

**Status:** consolidado para implementação  
**Escopo:** Vortex + 10 heroínas, facções, poderes especiais, magia, skins, dados e integração Unity/backend  
**Fonte de verdade:** decisões e artes aprovadas durante a produção conceitual

---

## 1. Princípios obrigatórios

- Vortex é o único herói masculino, protagonista e personagem mais poderoso.
- A Consorte de Valgor é a heroína mais poderosa depois de Vortex.
- Todas as heroínas são adultas.
- Direção visual semi-realista, aproximadamente 60% realista e 40% estilizada.
- Iluminação neutra ou fria; evitar amarelo, sépia e excesso de laranja.
- Identidade medieval-fantástica original, sem armas modernas.
- Cada personagem possui silhueta, rosto, roupa, cor, arma e função próprias.
- Cada personagem possui um poder especial temporário com duração e recarga.
- Magia existe oficialmente no universo de Valgor e está ligada a runas, dragões, elementos, artefatos e linhagens antigas.
- O sistema deve ser orientado a dados para permitir balanceamento sem recompilar o cliente.

---

## 2. Sistema oficial de facções

### Rosa de Sangue
Identidade: agressão, dano explosivo, execução, assassinas e duelistas.

### Asas do Amanhecer
Identidade: velocidade, precisão, magia, controle e mobilidade.

### Guarda da Ordem
Identidade: defesa, proteção, liderança, suporte e resistência.

### Relação circular

```text
Rosa de Sangue > Guarda da Ordem
Guarda da Ordem > Asas do Amanhecer
Asas do Amanhecer > Rosa de Sangue
```

### Bônus de composição aprovados

| Formação | Bônus |
|---|---:|
| 3 da mesma facção | +5% ATQ total da tropa |
| 3 de uma facção + 2 de outra | +7% ATQ total da tropa |
| 4 da mesma facção | +10% ATQ total da tropa |
| 5 da mesma facção | +15% ATQ total da tropa |

### Regra de vantagem recomendada para o MVP

- Dano causado contra a facção neutralizada: **+15%**.
- Dano recebido da facção que neutraliza: comportamento natural do mesmo sistema.
- A vantagem deve vir do backend/configuração remota.
- Não acumular duas vantagens de facção sobre o mesmo ataque.

---

## 3. Poder especial temporário

### Máquina de estados

```text
READY → ACTIVE → COOLDOWN → READY
```

### Regras

- `READY`: habilidade disponível.
- `ACTIVE`: efeito permanece ativo pelo tempo definido.
- `COOLDOWN`: habilidade indisponível até a recarga terminar.
- A duração e a recarga são individuais.
- A interface exibe disponibilidade, tempo ativo e recarga.
- O servidor valida ativação em modos competitivos.
- O cliente pode prever a animação, mas não decide o resultado.
- Interrupções, silêncio e morte devem ser configuráveis por habilidade.

### Campos mínimos

```text
specialPowerId
heroId
name
activeDurationSec
cooldownSec
targetType
effects[]
interruptible
canActivateWhileControlled
vfxAddress
sfxAddress
animationState
```

---

## 4. Magia no universo

Escolas iniciais:

1. Elemental: fogo, gelo, vento e raio.
2. Luz: cura, escudo e purificação.
3. Sombra: maldição, drenagem e controle.
4. Dracônica: rara, vinculada a Vortex, dragões e linhagens especiais.
5. Rúnica: artefatos, armas, portais, cidades e proteção.
6. Abissal: dano em área, enfraquecimento e bloqueio de mobilidade.
7. Etérea: órbitas, portais, aprisionamento e desvio de projéteis.

### Novos atributos

```text
magicAttack
magicDefense
mana
manaRegen
controlResistance
cooldownReduction
elementalResistance
```

No MVP, mana pode ser omitida e os especiais podem funcionar apenas por recarga. A arquitetura deve permitir ativá-la futuramente.

---

## 5. Catálogo oficial

### Vortex — O Rei dos Dragões

- **Código:** `HERO_VORTEX_000`
- **Status:** APROVADO_CONCEITUAL
- **Raridade:** Mítica
- **Facção:** `GUARDA_DA_ORDEM`
- **Classe:** Comandante Dracônico
- **Função:** Liderança / Dano / Controle
- **Posição:** Linha de frente
- **Arma:** Espada dracônica e vínculo com dragão
- **Elemento:** Chama Dracônica
- **Poder especial:** **Domínio do Rei**
- **Duração:** 10s
- **Recarga:** 60s
- **Efeitos:** Aumenta ATQ e DEF dos aliados; Invoca presença dracônica; Reduz resistência inimiga; Concede imunidade breve a controle.
- **Observações visuais:** Personagem masculino central e mais poderoso da franquia.

### Elyra — A Caçadora Esmeralda

- **Código:** `HERO_ELYRA_001`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `ASAS_DO_AMANHECER`
- **Classe:** Arqueira
- **Função:** Dano de longo alcance
- **Posição:** Retaguarda
- **Arma:** Arco longo recurvo esmeralda
- **Elemento:** Natureza
- **Poder especial:** **Olho da Caçadora**
- **Duração:** 10s
- **Recarga:** 35s
- **Efeitos:** Aumenta alcance; Aumenta chance crítica; Marca inimigos; Dano adicional contra criaturas.
- **Observações visuais:** Heroína 01. Sem capa; aljava e flechas nas costas.

### A definir — A Consorte de Valgor

- **Código:** `HERO_CONSORTE_002`
- **Status:** APROVADA
- **Raridade:** Mítica
- **Facção:** `GUARDA_DA_ORDEM`
- **Classe:** Lanceira Real
- **Função:** Suporte / Liderança / Dano
- **Posição:** Linha intermediária
- **Arma:** Lança-cajado celestial branca e dourada
- **Elemento:** Luz Sagrada
- **Poder especial:** **Voto Eterno**
- **Duração:** 10s
- **Recarga:** 45s
- **Efeitos:** Aumenta ATQ e DEF dos aliados; Restaura vida continuamente; Concede imunidade a controle; Fortalece tropas próximas.
- **Observações visuais:** Heroína 02, noiva de Vortex e mais forte depois dele. Skin real aprovada com capa curta estruturada até a cintura, sem véu e sem cauda nupcial.

### A definir — A Arqueira da Sombra

- **Código:** `HERO_SOMBRA_003`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `ROSA_DE_SANGUE`
- **Classe:** Atiradora Élfica
- **Função:** Dano / Controle
- **Posição:** Retaguarda
- **Arma:** Besta élfica ornamentada
- **Elemento:** Sombra
- **Poder especial:** **Domínio Sombrio**
- **Duração:** 8s
- **Recarga:** 42s
- **Efeitos:** Aumenta precisão; Dispara projéteis sombrios adicionais; Marca alvos; Reduz defesa dos inimigos marcados.
- **Observações visuais:** Heroína 03. Body medieval de couro preto, tranças, detalhes roxos e dourados, sem capa.

### Lyrianne — A Sentinela de Prata

- **Código:** `HERO_LYRIANNE_004`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `GUARDA_DA_ORDEM`
- **Classe:** Sentinela
- **Função:** Precisão / Proteção
- **Posição:** Linha intermediária
- **Arma:** Arco lunar de haste longa
- **Elemento:** Luz Lunar
- **Poder especial:** **Julgamento Prateado**
- **Duração:** 9s
- **Recarga:** 44s
- **Efeitos:** Amplia alcance; Cria escudo nos aliados; Dispara flechas lunares em linha; Purifica efeitos negativos.
- **Observações visuais:** Heroína 04. Pele morena, cabelo platinado, roupa branca justa de inspiração atlética-medieval.

### Akemi — A Lâmina Celeste

- **Código:** `HERO_AKEMI_005`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `ASAS_DO_AMANHECER`
- **Classe:** Duelista
- **Função:** Assassina de curto alcance
- **Posição:** Linha de frente móvel
- **Arma:** Lâminas gêmeas celestes
- **Elemento:** Gelo Celeste
- **Poder especial:** **Dança das Lâminas**
- **Duração:** 8s
- **Recarga:** 40s
- **Efeitos:** Aumenta velocidade; Ataques atingem múltiplos alvos; Permite investidas consecutivas; Eleva esquiva.
- **Observações visuais:** Heroína 05. Linhagem nipo-brasileira, macacão azul profundo, duas lâminas curvas luminosas.

### Serena Rubra — A Caçadora Carmesim

- **Código:** `HERO_SERENA_006`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `ROSA_DE_SANGUE`
- **Classe:** Atiradora
- **Função:** Longa distância / Dano crítico
- **Posição:** Retaguarda
- **Arma:** Lançador rúnico carmesim de longo alcance
- **Elemento:** Fogo
- **Poder especial:** **Coração Carmesim**
- **Duração:** 9s
- **Recarga:** 42s
- **Efeitos:** Aumenta dano crítico; Dispara projéteis acelerados; Ignora parte da defesa; Prioriza alvos enfraquecidos.
- **Observações visuais:** Heroína 06. Ruiva ondulada, pele clara com sardas discretas, top medieval, micro shorts e tiras rosa-escuro.

### A definir — A Maga do Abismo

- **Código:** `HERO_ABISMO_007`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `ASAS_DO_AMANHECER`
- **Classe:** Maga
- **Função:** Dano em área / Controle / Enfraquecimento
- **Posição:** Retaguarda
- **Arma:** Cajado do Abismo com orbe violeta
- **Elemento:** Abismo
- **Poder especial:** **Domínio do Vazio**
- **Duração:** 7s
- **Recarga:** 50s
- **Efeitos:** Cria zona sombria; Reduz velocidade inimiga; Causa dano contínuo; Bloqueia habilidades de mobilidade.
- **Observações visuais:** Heroína 07. Loira, capa preta curta com capuz até a cintura, roupa preta e prata.

### Zahara — A Guardiã dos Círculos

- **Código:** `HERO_ZAHARA_008`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `GUARDA_DA_ORDEM`
- **Classe:** Mística
- **Função:** Controle arcano de médio alcance
- **Posição:** Linha intermediária
- **Arma:** Anéis rúnicos gêmeos
- **Elemento:** Éter Safira
- **Poder especial:** **Órbita Real**
- **Duração:** 10s
- **Recarga:** 46s
- **Efeitos:** Cria órbitas defensivas; Aprisiona inimigos; Desvia projéteis; Rompe formações.
- **Observações visuais:** Heroína 08. Pele negra, cabelo branco cacheado, turquesa e ouro.

### Nyxara — A Guardiã das Sombras

- **Código:** `HERO_NYXARA_009`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `ROSA_DE_SANGUE`
- **Classe:** Executora
- **Função:** Assassina de controle
- **Posição:** Flanco
- **Arma:** Correntes-lâmina gêmeas
- **Elemento:** Sombra Rubra
- **Poder especial:** **Juízo Noturno**
- **Duração:** 8s
- **Recarga:** 43s
- **Efeitos:** Prende alvos; Causa sangramento; Salta para a retaguarda; Executa inimigos com pouca vida.
- **Observações visuais:** Heroína 09. Preto, prata e rubi; cabelo trançado; visual frio e implacável.

### Vespera — A Dama do Leque

- **Código:** `HERO_VESPERA_010`
- **Status:** APROVADA
- **Raridade:** Lendária
- **Facção:** `ASAS_DO_AMANHECER`
- **Classe:** Estrategista
- **Função:** Atiradora de médio e longo alcance
- **Posição:** Retaguarda
- **Arma:** Leque de lâminas e dardos ocultos
- **Elemento:** Crepúsculo Violeta
- **Poder especial:** **Suspiro Final**
- **Duração:** 11s
- **Recarga:** 47s
- **Efeitos:** Dispara chuva de dardos; Reduz precisão inimiga; Aumenta velocidade de ataque aliada; Finaliza alvos marcados.
- **Observações visuais:** Heroína 10. Roxo profundo e ouro; leque armado; perfil refinado e calculista.

---

## 6. Distribuição por facção

### Rosa de Sangue
- A Arqueira da Sombra
- Serena Rubra
- Nyxara

### Asas do Amanhecer
- Elyra
- Akemi
- A Maga do Abismo
- Vespera

### Guarda da Ordem
- Vortex
- A Consorte de Valgor
- Lyrianne
- Zahara

A distribuição inicial não precisa ser numericamente idêntica porque Vortex não integra todas as formações comuns. O backend deve permitir redistribuição sem alterar código.

---

## 7. Skins

### Skin real da Consorte de Valgor

Direção aprovada:

- armadura branca e dourada;
- cajado original;
- penteado original;
- capa curta estruturada terminando na cintura;
- sem véu;
- sem cauda longa;
- aparência de guerreira real, não de vestido de casamento;
- cobertura completa;
- identidade luminosa e régia.

### Regra de monetização justa

Recomendação: skins devem ser prioritariamente cosméticas. Quando houver bônus, aplicar em modos de progressão e limitar ou normalizar em PvP competitivo.

Campos:

```text
skinId
heroId
name
rarity
modelAddress
materialSetAddress
portraitAddress
vfxOverrides
sfxOverrides
animationOverrides
statModifiers
competitiveNormalization
```

---

## 8. Modelo de dados

### HeroDefinition

```csharp
public sealed class HeroDefinition
{
    public string Id;
    public string DisplayName;
    public string Title;
    public HeroRarity Rarity;
    public HeroFaction Faction;
    public HeroClass Class;
    public CombatRole Role;
    public CombatPosition Position;
    public string ElementId;
    public string WeaponId;
    public BaseStats BaseStats;
    public string SpecialPowerId;
    public string DefaultSkinId;
    public string PrefabAddress;
    public string PortraitAddress;
}
```

### SpecialPowerDefinition

```csharp
public sealed class SpecialPowerDefinition
{
    public string Id;
    public string HeroId;
    public string DisplayName;
    public float ActiveDurationSec;
    public float CooldownSec;
    public TargetType TargetType;
    public bool Interruptible;
    public bool CanActivateWhileControlled;
    public List<EffectDefinition> Effects;
    public string AnimationState;
    public string VfxAddress;
    public string SfxAddress;
}
```

### HeroRuntimeState

```csharp
public sealed class HeroRuntimeState
{
    public string HeroId;
    public SpecialPowerState SpecialState;
    public double ActiveUntilServerTime;
    public double CooldownUntilServerTime;
}
```

---

## 9. Backend

### Entidades

- `hero_definitions`
- `hero_special_powers`
- `hero_special_effects`
- `hero_skins`
- `hero_factions`
- `faction_advantages`
- `faction_team_bonuses`
- `player_heroes`
- `player_hero_progression`
- `player_hero_skins`

### Endpoints mínimos

```text
GET  /api/heroes/catalog
GET  /api/heroes/{heroId}
GET  /api/heroes/factions
GET  /api/heroes/team-bonuses
GET  /api/players/me/heroes
POST /api/battle/{battleId}/heroes/{heroId}/special/activate
POST /api/teams/validate
```

### Validações de ativação

- herói pertence ao jogador;
- herói está vivo;
- poder está em `READY`;
- batalha e alvo são válidos;
- jogador possui autoridade sobre a unidade;
- relógio do servidor determina duração e cooldown;
- impedir chamadas duplicadas com idempotency key.

---

## 10. Unity

### Estrutura

```text
Assets/Valgor/Heroes/
├── Core/
├── Data/
├── Factions/
├── SpecialPowers/
├── UI/
├── Preview360/
├── VFX/
├── Skins/
├── Vortex/
├── Elyra/
├── Consorte/
├── ShadowArcher/
├── Lyrianne/
├── Akemi/
├── SerenaRubra/
├── AbyssMage/
├── Zahara/
├── Nyxara/
└── Vespera/
```

### Componentes

```text
HeroCatalogService
HeroDefinitionSO
HeroRuntimeController
HeroFactionResolver
FactionBonusCalculator
FactionAdvantageResolver
SpecialPowerController
SpecialPowerStateMachine
SpecialPowerButtonView
SpecialPowerCooldownView
HeroTeamBuilder
HeroRosterView
HeroCardView
HeroPreviewController
HeroSkinController
```

### Interface de poder

O botão deve apresentar:

- ícone exclusivo;
- estado disponível;
- contagem regressiva;
- preenchimento radial;
- tempo ativo;
- bloqueio visual;
- feedback de toque;
- VFX de disponibilidade;
- aviso de erro validado pelo servidor.

### Tela de heróis

Filtros:

- Todos;
- Rosa de Sangue;
- Asas do Amanhecer;
- Guarda da Ordem.

Cards:

- retrato;
- raridade;
- nível;
- estrelas;
- facção;
- classe;
- indicador de melhoria;
- fragmentos para desbloqueio;
- skin ativa.

---

## 11. Fases de implementação

### Sprint H01 — Dados e catálogo
- enums;
- ScriptableObjects;
- tabelas e migrations;
- seed dos personagens;
- endpoints de catálogo;
- testes.

### Sprint H02 — Facções e formação
- filtros;
- vantagem circular;
- bônus por composição;
- validador de equipe;
- UI de explicação;
- telemetria.

### Sprint H03 — Poderes especiais
- máquina de estados;
- sincronização;
- botão e cooldown;
- efeitos;
- VFX/SFX;
- testes de abuso e reconexão.

### Sprint H04 — Magia
- atributos mágicos;
- tipos de efeito;
- resistências;
- escolas;
- magias em área e controle;
- depuração visual.

### Sprint H05 — Skins e preview
- tela 360°;
- troca de skin;
- Addressables;
- LOD;
- materiais;
- animações;
- skin real da Consorte.

### Sprint H06 — Balanceamento e QA
- simulação de equipes;
- limites de bônus;
- testes PvE/PvP;
- performance mobile;
- telemetria;
- ajustes remotos.

---

## 12. Critérios de aceite

- Catálogo contém Vortex e as 10 heroínas.
- Cada personagem possui facção.
- Cada personagem possui poder especial único.
- Estados `READY`, `ACTIVE` e `COOLDOWN` funcionam.
- Duração e recarga vêm de dados.
- Facções respeitam a relação circular.
- Bônus de equipe são calculados corretamente.
- Magia suporta dano, controle, escudo, cura e enfraquecimento.
- UI mostra facção e recarga.
- Reconexão restaura o estado correto.
- Backend impede ativações inválidas.
- Dados podem ser balanceados sem recompilar.
- Android e iOS mantêm a meta de desempenho.
- Testes automatizados cobrem cálculos e transições de estado.

---

## 13. Prompt mestre para o Cursor

```markdown
# VALGOR — IMPLEMENTAÇÃO DO SISTEMA DE HERÓIS

Leia integralmente a Bíblia Oficial de Heróis e trate-a como fonte de verdade.

Implemente em sprints incrementais:

1. Catálogo orientado a dados para Vortex e as 10 heroínas.
2. Facções Rosa de Sangue, Asas do Amanhecer e Guarda da Ordem.
3. Relação circular de vantagem.
4. Bônus de composição 3, 3+2, 4 e 5.
5. Poder especial único por personagem, com estados READY, ACTIVE e COOLDOWN.
6. Validação de ativação no backend.
7. UI de roster, filtros, cards e explicação de facção.
8. Sistema de magia extensível.
9. Suporte a skins e preview 360°.
10. Seeds, migrations, testes, documentação e telemetria.

Requisitos:
- Unity 6 LTS, URP, Input System, UI Toolkit e Addressables.
- Backend .NET 9, PostgreSQL e Redis.
- Nenhuma regra de combate crítica somente no cliente.
- Configuração e balanceamento orientados a dados.
- Código modular, testável e documentado.
- Não alterar nomes, facções, armas, papéis ou poderes sem registrar decisão.
- Para nomes marcados “A definir”, manter código interno e exibir título temporariamente.
- Usar dummy visual somente onde o modelo 3D final ainda não estiver disponível.
- Atualizar README e CHANGELOG.
- Criar testes unitários, integração e PlayMode.
- Fazer commit e push ao concluir cada sprint.

Não avance para a sprint seguinte com compilação quebrada ou testes falhando.
```

---

## 14. Pendências que exigem decisão futura

- Nome civil da Consorte de Valgor.
- Nome civil da Arqueira da Sombra.
- Nome civil da Maga do Abismo.
- Valores finais de atributos.
- Multiplicador final de vantagem de facção.
- Política definitiva de bônus estatísticos em skins.
- Formação padrão: três, quatro ou cinco heróis por marcha.
- Uso de mana no MVP ou apenas cooldown.
