# Abigobaldo's Kitchen

Jogo 3D de cozinha e atendimento. O jogador prepara Cuscuz, Ovo frito, Omelete e Milho assado para clientes em situacao de rua. A demonstracao usa Nino, Marcia e Seu Ze, com dialogos ramificados, qualidade de comida, pontuacao e ciclo de dia/noite.

## Mapa rapido

```
Assets/
  _Scripts/          Runtime, dados de gameplay e ferramentas de Editor
  Prefabs/           Prefabs proprios de gameplay e visuais
  Scenes/            Cenas do fluxo do jogo
  Materials/         Materiais e texturas proprias organizadas
  Models/             Modelos fonte do jogo
  Images/             Sprites, fundos e arte de UI
  Audio/              Musicas e efeitos proprios
  Settings/           Configuracoes de renderizacao/URP
  ImportedAssets/    Pacotes de terceiros preservados na estrutura original
  Docs/              Documentacao de design e arquitetura
```

## Onde editar

| Quero alterar… | Local |
| --- | --- |
| Cenas que o jogo abre | `Assets/Scenes/menu.unity`, `Assets/Scenes/MainGame.unity` |
| Player | `Assets/Prefabs/Player/Player.prefab`, `Assets/_Scripts/Player/` |
| Item/ingrediente | `Assets/Prefabs/Objects/`, `Assets/_Scripts/Data/Objects/` |
| Receita | `Assets/_Scripts/Data/Recipes/` e `Assets/_Scripts/Data/RecipeBooks/RecipeBook.asset` |
| Recipiente/aparelho | `Assets/Prefabs/Containers/`, `Assets/_Scripts/Cooking/` |
| Cliente | `Assets/Prefabs/Characters/`, `Assets/_Scripts/Customers/` |
| Sequencia e pontuacao da demo | `Assets/_Scripts/Managers/CustomerManager.cs` |
| Roteiro da demo | `Assets/Docs/DLG_Demo_5Pedidos.md` |
| Luz e ciclo do dia | `Assets/_Scripts/Managers/Lighting/`, `DayNightManager.cs` |

## Sistemas principais

`Player` usa `PlayerInput`, `PlayerMovement`, `PlayerCamera` e `PlayerInteractor`.

`Objects` sao definidos por `ObjectDefinition`, representados por `ObjectIdentity` e manipulados por `HoldableObject`/`Holder`.

`Cooking` associa ingredientes a `RecipeData` pelo `RecipeBook`. `ContainerStation` e suas especializacoes (frigideira, cuscuzeira e liquidificador) processam a receita. `RecipeProgress` calcula Cru → Quase pronto → Pronto → Passado → Queimado → Carvao.

`Customers` sao instanciados por `NpcSpawner`; `CustomerManager` conduz os dialogos, pedidos, entrega e pontuacao.

## Fluxo de comida

```
ObjectDefinition → Prefab de objeto → RecipeData → ContainerStation
→ RecipeProgress → Plate → CustomerNpc → CustomerManager
```

## Convencoes

- Prefabs usam nomes claros por dominio: `Cuscuz`, `FryingPan`, `Nino`.
- Dados de receita e de objeto podem ter o mesmo nome porque pertencem a tipos diferentes; confira a pasta antes de editar.
- Scripts de runtime ficam em `_Scripts/<Dominio>`; scripts de Editor devem ficar em `_Scripts/Editor`.
- Nunca separe um asset Unity do seu arquivo `.meta` ao mover arquivos.
- Pacotes em `ImportedAssets` nao devem ser reorganizados internamente.

## Adicionando conteudo

### Ingrediente

1. Crie o `ObjectDefinition` em `_Scripts/Data/Objects`.
2. Crie o prefab em `Prefabs/Objects` com `HoldableObject`, `ObjectIdentity` e `ObjectVisualPreset`.
3. Adicione a definicao a uma receita no `RecipeBook`.

### Receita

1. Crie `RecipeData` em `_Scripts/Data/Recipes`.
2. Escolha o `RecipeStationType`, ingredientes, tempos e prefabs de processo/resultado.
3. Inclua a receita em `RecipeBook.asset`.

### Aparelho

1. Crie o prefab em `Prefabs/Containers` ou `Prefabs/Environment`.
2. Use uma especializacao de `ContainerStation` apropriada.
3. Aponte o `RecipeBook`, os anchors e, se quiser barra de cozimento, crie e atribua o `ProgressBar` Pivot em `CookingProgressBar`.

### NPC

1. Crie o prefab em `Prefabs/Characters` com `BoxCollider` e `CustomerNpc`.
2. Adicione-o ao `NpcSpawner` de `MainGame`.
3. Crie a aparicao e os ramos em `CustomerManager` e documente o roteiro em `Docs`.

## Build para itch.io

Na Unity: `Abigobaldo > Build > Build WebGL for itch.io`.

O processo gera `Área de Trabalho/Build Para ItchIO`, com a pasta WebGL, um ZIP pronto para upload e `INSTRUCOES_ITCHIO.txt`.

## Mais detalhes

Veja [ProjectOrganizationReport](Assets/Docs/ProjectOrganizationReport.md) para o inventario, decisoes e pontos de atencao.
