# Organização do projeto — 21/08/2026

## Cenas de produção

O Build Settings contém somente as duas cenas que devem ser usadas no jogo:

1. `Assets/Scenes/menu.unity` — menu inicial, opções de música/SFX e transição.
2. `Assets/Scenes/MainGame.unity` — gameplay, HUD, diálogo, clientes, cozinha e managers.

Não há outra cena de produção fora dessa lista. Mantenha o menu como cena 0 e a MainGame como cena 1.

## Mapa rápido da MainGame

| Grupo da Hierarchy | Responsabilidade |
| --- | --- |
| `Managers` | EventSystem, GameInteraction, Lighting, CustomerManager e DayNightManager. |
| `Setup` | Entrada da cena, HUD, CinematicCanvas e câmeras de diálogo. |
| `Workspace` | Cozinha/food truck, Player, estações, spawners, pia, lixeira e balcão. |
| `NpcSpawner` | Ordem fixa de clientes: Nino, SeuZé, Marcia, Nino, SeuZé. |
| `UICanvas` | Canvas de HUD criado/atualizado pelo `GameplayHud`. |

## Estrutura de assets

- `_Scripts/` — código separado por sistema: Audio, Cooking, Customers, Interaction, Managers, Menu, Objects, Player, UI e Visuals.
- `Prefabs/` — prefabs de gameplay separados por Characters, Containers, Environment, Objects, Player, Props, Spawners, UI e Visuals.
- `Materials/` — materiais e texturas próprias.
- `Audio/` — músicas, efeitos gerais e mixer. Áudios de gameplay carregados em runtime ficam em `Resources/Audio/Gameplay`.
- `Images/Menu/` — assets ativos do menu, organizados em Backgrounds, Branding, Buttons, Icons e Panels.
- `ImportedAssets/` — conteúdo de terceiros; não reorganizar sem conferir licença/referências.
- `Scenes/` — somente cenas de produção.

## Limpeza feita nesta revisão

- Os exports antigos e sem referência do menu foram movidos de `Assets/Images/-` para `Assets/Images/Legacy/Unused_Menu_Exports`.
- Fundos duplicados sem referência foram movidos para `Assets/Images/Legacy/Unused_Backgrounds`.
- Nenhum asset foi apagado; os `.meta` foram preservados, portanto GUIDs e referências permanecem seguros.

## Atenção antes de editar

- `My project (4)` na raiz é uma segunda cópia inteira de um projeto Unity. Ela não é usada pela cena atual; trate como backup até decidir se quer arquivar ou excluir.
- `Assets/Images/Legacy/` é material guardado para referência, não deve entrar na UI nova.
- Há um documento de handoff antigo na raiz (`AI_HANDOFF.md`). O código, as cenas e o Build Settings atuais prevalecem sobre ele.
- Abra `MainGame` ao trabalhar na cozinha; abra `menu` ao mexer em telas, música ou botões.

## Retomada sugerida

1. Abrir `Assets/Scenes/MainGame.unity` e conferir o Console.
2. Testar a entrada pelo menu e comparar a iluminação com abrir a MainGame diretamente.
3. Ajustar posições/tamanho dos textos em `HUD Canvas` e `CinematicCanvas` no Inspector.
4. Testar receitas, clientes, áudio e pausa (`Esc`) em uma partida completa.
5. Só depois gerar um novo WebGL ZIP para o itch.io.
