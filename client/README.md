# Valgor — Cliente Unity

Abra o Unity Hub, escolha **Add / Open** e selecione a pasta `client`. O projeto foi preparado para **Unity 6 LTS 6000.0.58f2**; o Hub solicitará essa versão se ela ainda não estiver instalada. Na primeira abertura, o Unity resolve os pacotes e recria os arquivos derivados.

## Estrutura

- `Assets/_Valgor/Scenes`: fluxo inicial `Bootstrap`, `Loading` e casca de `MainMenu`.
- `Assets/_Valgor/Scripts/Runtime`: composição de serviços, carregamento de cenas, áudio, pooling, Addressables, input e localização.
- `Assets/_Valgor/UI`: documentos e estilos UI Toolkit.
- `Assets/_Valgor/Input`: mapa de ações Input System.
- `Assets/_Valgor/Settings`: ativos da URP voltados a dispositivos móveis.
- `ProjectSettings`: configuração de produto, build, qualidade e renderização.

O projeto não contém gameplay, conteúdo de exemplo ou cenas demonstrativas. `Bootstrap` inicializa os serviços persistentes, carrega `Loading` e segue para `MainMenu`.

Addressables está instalado e encapsulado em `AddressablesService`. Crie os grupos e marque os ativos endereçáveis no editor quando os primeiros conteúdos de produção existirem.
# Client

Projeto Unity 6 LTS do Valgor.

Abra esta pasta no Unity Hub com a versão **Unity 6 LTS**.
