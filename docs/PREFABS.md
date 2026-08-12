# Prefabs

Guia direto para montar prefabs no Unity. Leia a linha do prefab, crie os GameObjects filhos, adicione os componentes e preencha os campos.

## Pastas

| Pasta | Uso |
|---|---|
| `Assets/Prefabs/Characters` | Abigobaldo e clientes. |
| `Assets/Prefabs/Containers` | Frigideira, cuscuzeira, blender/base e futuros containers. |
| `Assets/Prefabs/Environment` | Foodtruck, cozinha, portas, bancadas, cooktops. |
| `Assets/Prefabs/Objects` | Objetos pegaveis: comidas, prato, copo do blender, cascas. |
| `Assets/Prefabs/Player` | Player. |
| `Assets/Prefabs/Props` | Decoracoes e objetos genericos. |
| `Assets/Prefabs/Spawners` | Spawners de objetos. |
| `Assets/Prefabs/UI` | Interface. |
| `Assets/Prefabs/Visuals` | Visuals de comida para prato/container. |

## Regras Rapidas

| Tipo | Regra |
|---|---|
| Objeto pegavel | Root com `HoldableObject`, `ObjectIdentity`, `Rigidbody` e collider. |
| Container pegavel | Root com script da station, `HoldableObject`, `ObjectIdentity`, `Rigidbody` e collider. |
| Visual prefab | Sem `Rigidbody`, sem collider, sem `HoldableObject`, sem `ObjectIdentity`. |
| Modelo | Fica no filho `Model` ou `Mesh`. |
| Ajuste de mao | Filho opcional `GripPoint`. |
| Receitas | Stations recebem `DemoRecipeBook.asset`. |
| Tipo do station | Nao configurar. O script ja define: `FryingPanStation`, `CuscuzeiraStation`, `BlenderStation`. |

## Assets de Data

| Asset | Caminho | Usado por |
|---|---|---|
| `DemoRecipeBook` | `Assets/_Scripts/Data/RecipeBooks/DemoRecipeBook.asset` | Todos os stations. |
| Recipes | `Assets/_Scripts/Data/Recipes` | Referenciadas dentro do `DemoRecipeBook`. |

## Player

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `Player` | root | `PlayerInput`, `PlayerMovement`, `PlayerCamera`, `PlayerCursor`, `PlayerInteractor` | Referenciar camera e holder se nao pegar automatico. |
| `Model` | `Player` | nenhum obrigatorio | Organizacao. |
| `Body` | `Model` | renderers | Corpo visivel. |
| `Head` | `Model` | renderers | Deixar oculto em primeira pessoa. |
| `CameraPivot` | `Player` | nenhum | Pivot da camera. |
| `Camera` | `CameraPivot` | `Camera`, `AudioListener` | Camera principal. |
| `Holder` | `Camera` ou `CameraPivot` | `Holder` | Ponto onde objeto fica na mao. |

## Objeto Pegavel Padrao

Use para: `Egg`, `Corn`, `CornFlakes`, `Cuscuz`, `FriedEgg`, `Omelet`, `Charcoal`, `EggShellA`, `EggShellB`, `RoastedCorn`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `ObjectName` | root | `HoldableObject`, `ObjectIdentity`, `Rigidbody`, collider | `ObjectIdentity.kind`; `HoldableObject.canBeThrown`. |
| `Model` | `ObjectName` | renderers/mesh | Visual do objeto. |
| `GripPoint` | `ObjectName` | nenhum | Opcional. Arrastar no `HoldableObject.gripPoint`. |

## Objetos Atuais

| Prefab | Pasta | Kind | Collider recomendado | Observacao |
|---|---|---|---|---|
| `Egg` | `Objects` | `Egg` | `SphereCollider` ou `MeshCollider convex` | Filho visual deve ser `Model/Mesh`, nao `Ovo`. |
| `Corn` | `Objects` | `Corn` | `CapsuleCollider` | OK. |
| `CornFlakes` | `Objects` | `CornFlakes` | `BoxCollider` pequeno | Melhor visual como varios floquinhos sem fisica. |
| `Cuscuz` | `Objects` | `Cuscuz` | `BoxCollider` ou `MeshCollider convex` | Pode ser empratado quando pronto. |
| `FriedEgg` | `Objects` | `FriedEgg` | `BoxCollider` baixo ou `MeshCollider convex` | Recebe `CookableItem` em runtime. |
| `Omelet` | `Objects` | `Omelet` | `BoxCollider` baixo ou `MeshCollider convex` | Recebe `CookableItem` em runtime. |
| `Charcoal` | `Objects` | `Charcoal` | `BoxCollider` | Pode ser empratado. |
| `EggShellA` | `Objects` | `Generic` ou `Decorative` | `MeshCollider convex` | Spawn lateral da receita do ovo. |
| `EggShellB` | `Objects` | `Generic` ou `Decorative` | `MeshCollider convex` | Spawn lateral da receita do ovo. |
| `Plate` | `Objects` | `Plate` | `MeshCollider convex` ou `BoxCollider` | Tem script `Plate`. |
| `BlenderCup` | `Objects` | `BlenderCup` | `MeshCollider convex` ou colliders simples | Copo pegavel separado da base. |

## Plate

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `Plate` | root | `HoldableObject`, `ObjectIdentity`, `Rigidbody`, `Plate`, collider | `ObjectIdentity.kind = Plate`; preencher `foodRoot` e `foodVisuals`. |
| `Model` | `Plate` | renderers/mesh | Visual do prato. |
| `FoodRoot` | `Plate` | nenhum | Onde comida empratada aparece. |
| `GripPoint` | `Plate` | nenhum | Opcional. |

## Visual Prefab

Use para: `FriedEggVisual`, `OmeletVisual`, `CuscuzVisual`, `CharcoalVisual`, `CornVisual`, `CornFlakesVisual`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `FoodVisual` | root | `ObjectVisualPreset` | Configurar poses por target. |
| `Mesh` | `FoodVisual` | renderers/mesh | Sem collider. |

| Target no `ObjectVisualPreset` | Uso |
|---|---|
| `Default` | Fallback. |
| `Plate` | Posição no prato. |
| `FryingPan` | Posição na frigideira. |
| `Blender` | Posição dentro do copo do blender. |
| `Cuscuzeira` | Só usar se algum dia mostrar conteúdo. |

## FryingPan

Criar em `Assets/Prefabs/Containers/FryingPan.prefab`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `FryingPan` | root | `FryingPanStation`, `HoldableObject`, `ObjectIdentity`, `Rigidbody`, collider | `recipeBook = DemoRecipeBook`; `itemAnchor`; `sideEffectSpawnRoot`; `canBeThrown = false`. |
| `Model` | `FryingPan` | renderers/mesh | Visual da frigideira. |
| `ItemAnchor` | `FryingPan` | nenhum | Onde o item/visual fica em cima da frigideira. |
| `SideEffectSpawnRoot` | `FryingPan` | nenhum | Onde cascas de ovo spawnam. |
| `Particles` | `FryingPan` | `ParticleSystem`, `StationParticles` | Fumaça baixa. |
| `GripPoint` | `FryingPan` | nenhum | Opcional. |

## Cuscuzeira

Criar em `Assets/Prefabs/Containers/Cuscuzeira.prefab`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `Cuscuzeira` | root | `CuscuzeiraStation`, `HoldableObject`, `ObjectIdentity`, `Rigidbody`, collider | `recipeBook = DemoRecipeBook`; `canBeThrown = false`; `showContainedObject = false`. |
| `Model` | `Cuscuzeira` | renderers/mesh | Visual da cuscuzeira. |
| `Particles` | `Cuscuzeira` | `ParticleSystem`, `StationParticles` | Vapor. |
| `GripPoint` | `Cuscuzeira` | nenhum | Opcional. |

## Blender Base

Criar em `Assets/Prefabs/Containers/Blender.prefab`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `Blender` | root | `BlenderStation`, collider | `recipeBook = DemoRecipeBook`; `cup = BlenderCupContent` se o copo for filho inicial. |
| `Model` | `Blender` | renderers/mesh | Base fixa do liquidificador. |
| `CupAnchor` | `Blender` | nenhum | Ponto onde o `BlenderCup` fica encaixado. |
| `Button` | `Blender` | collider opcional | Futuro botão separado. |

## BlenderCup

Prefab em `Assets/Prefabs/Objects/BlenderCup.prefab`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `BlenderCup` | root | `HoldableObject`, `ObjectIdentity`, `Rigidbody`, `BlenderCupContent`, collider | `ObjectIdentity.kind = BlenderCup`; `contentRoot = ContentRoot`; `canBeThrown = false`. |
| `Model` | `BlenderCup` | renderers/mesh | Copo transparente. |
| `Lid` | `BlenderCup` | renderers/mesh | Tampa. |
| `ContentRoot` | `BlenderCup` | nenhum | Onde o conteúdo fica. Vai junto se pegar o copo. |
| `GripPoint` | `BlenderCup` | nenhum | Opcional. |

## Spawner Padrao

Use para: `EggSpawner`, `CornSpawner`, `PlateSpawner`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `ThingSpawner` | root | `ObjectSpawner`, collider | `prefab`; `giveDirectlyToHolder = true`; `replaceHeldObject = true`. |
| `Model` | `ThingSpawner` | renderers/mesh | Visual do item exposto. |
| `SpawnPoint` | `ThingSpawner` | nenhum | Opcional. |

## Door

Criar em `Assets/Prefabs/Environment`.

| GameObject | Filho de | Componentes | Campos |
|---|---|---|---|
| `Door` | root | `OpenableDoor`, collider | Pivot no eixo da dobradiça; `rotationAxis`; `maxOpenAngle = 90`. |
| `Model` | `Door` | renderers/mesh | Visual da porta. |

## Props

| Tipo | GameObjects | Componentes no root | Exemplo |
|---|---|---|---|
| Prop pegavel | `PropName > Model, GripPoint` | `HoldableObject`, `ObjectIdentity`, `Rigidbody`, collider | Radio, saleiro, pimenteiro. |
| Prop fixo | `PropName > Model` | collider só se bloquear ou interagir | Poster, lâmpada, calendário. |

## Checklist Curta

| Feito | Tarefa |
|---|---|
| [ ] | Criar `Containers/FryingPan.prefab`. |
| [ ] | Criar `Containers/Cuscuzeira.prefab`. |
| [ ] | Criar `Containers/Blender.prefab`. |
| [ ] | Revisar `Objects/BlenderCup.prefab`. |
| [ ] | Renomear filhos antigos tipo `Ovo`, `Item_Charcoal`, `Food_EggShellA` para `Mesh`. |
| [ ] | Remover colliders dos prefabs em `Visuals`. |
| [ ] | Renomear `Plated*Visual` para `*Visual`. |
| [ ] | Adicionar `ObjectVisualPreset` em todos os visuals. |
| [ ] | Garantir `DemoRecipeBook.asset` em todos os stations. |
