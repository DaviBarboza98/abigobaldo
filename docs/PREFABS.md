# Prefabs

Guia de montagem dos prefabs atuais. Os campos de receita e identidade usam assets; nenhum prefab precisa escolher comida por enum.

## Regras

| Tipo | Componentes no root | Filhos recomendados |
|---|---|---|
| Objeto pegavel | `HoldableObject`, `ObjectIdentity`, `ObjectVisualPreset`, `Rigidbody`, collider | `Model`, `GripPoint` opcional |
| Container pegavel | script da station, `HoldableObject`, `Rigidbody`, collider | `Model`, anchors necessarios |
| Spawner | `ObjectSpawner`, collider | `Model`, `SpawnPoint` opcional |

## Dados

| Asset | Pasta | Funcao |
|---|---|---|
| `ObjectDefinition` | `Assets/_Scripts/Data/Objects` | Identidade leve usada para comparar ingredientes. |
| `RecipeData` | `Assets/_Scripts/Data/Recipes` | Ingredientes, estacao, transformacoes, tempos e aparencia de cada estado. |
| `RecipeBook` | `Assets/_Scripts/Data/RecipeBooks/RecipeBook.asset` | Lista unica das receitas e prefab global obrigatorio de `Charcoal`. |

Nos campos `In Progress Prefab`, `Result Prefab` e `Byproducts`, arraste o **GameObject do prefab**. O Inspector nao pede mais um componente `HoldableObject`; o sistema valida esse componente somente quando precisar instanciar o prefab.

## Estados Aquecidos

| Ordem | Estado | Configuracao no `RecipeData` |
|---:|---|---|
| 1 | `Raw` | Material e modelo opcionais. |
| 2 | `AlmostReady` | Tempo, material e modelo opcionais. |
| 3 | `Ready` | Usa `Processing Time`; material e modelo opcionais. |
| 4 | `Overdone` | Tempo, material e modelo opcionais. |
| 5 | `Burned` | Tempo, material e modelo opcionais. |
| 6 | `Carbonized` | Usa `Carbonized Time` e sempre substitui o objeto pelo `Charcoal Prefab` do `RecipeBook`. |

Receitas sem calor, como o blender, deixam `Uses Heat` desligado e terminam em `Processing Time`; elas nao queimam.

## Objetos

| Prefab | Componentes extras | `ObjectIdentity` | Observacao |
|---|---|---|---|
| `Egg` | `ObjectVisualPreset` | `Egg`, nao empratavel | Ingrediente. Configure pose de `FryingPan` se ele puder aparecer em container. |
| `Corn` | `ObjectVisualPreset` | `Corn`, nao empratavel | Ingrediente. Configure pose de `Blender` e `FryingPan`. |
| `CornFlakes` | `ObjectVisualPreset` | `CornFlakes`, nao empratavel | Resultado do blender. Configure pose de `Cuscuzeira` se um dia for visivel. |
| `FriedEgg` | `ObjectVisualPreset`, `PlateableObject`, `RotationTransform` | `FriedEgg`, empratavel | So ele pode virar omelete ao girar. |
| `Omelet` | `ObjectVisualPreset`, `PlateableObject` | `Omelet`, empratavel | Cozinha como receita propria. |
| `Cuscuz` | `ObjectVisualPreset`, `PlateableObject` | `Cuscuz`, empratavel | Conteudo da cuscuzeira fica oculto, mas precisa pose de `Plate`. |
| `RoastedCorn` | `ObjectVisualPreset`, `PlateableObject` | `RoastedCorn`, empratavel | Usa o proprio modelo do objeto em container/prato. |
| `Charcoal` | `ObjectVisualPreset`, `PlateableObject` | `Charcoal`, empratavel | Resultado carbonizado global. |
| `EggShellA/B` | nenhum | `EggShell`, nao empratavel | Subprodutos da receita do ovo. |
| `Plate` | `Plate` | `Plate`, nao empratavel | Precisa do filho `FoodRoot`. |
| `BlenderCup` | `BlenderCupContent` | `BlenderCup`, nao empratavel | Precisa do filho `ContentRoot`. |

Todo `ObjectIdentity` recebe:

| Campo | Preenchimento |
|---|---|
| `Definition` | Asset correspondente em `Data/Objects`. |

Comida que pode ir ao prato recebe `PlateableObject`. O campo antigo `Can Be Plated` fica escondido e so existe para prefabs velhos nao quebrarem.

## Containers

| Prefab | Componentes no root | Filhos | Campos |
|---|---|---|---|
| `FryingPan` | `FryingPanStation`, `HoldableObject`, `Rigidbody`, collider | `Model`, `ItemAnchor`, `SideEffectSpawnRoot`, `GripPoint` opcional | `Recipe Book`, `Item Anchor`, root das cascas; `Can Be Thrown` desligado. |
| `Cuscuzeira` | `CuscuzeiraStation`, `HoldableObject`, `Rigidbody`, collider | `Model`, `GripPoint` opcional | `Recipe Book`; `Show Contents` desligado; `Can Be Thrown` desligado. |
| `Blender` | `BlenderStation`, collider | `Model`, `CupAnchor/BlenderCup` encaixado | `Recipe Book`, referencia do copo opcional, `Spin Speed`. A base e fixa e nao possui `Rigidbody`. |
| `BlenderCup` | `BlenderCupContent`, `HoldableObject`, `ObjectIdentity`, `Rigidbody`, collider | `Model`, `Lid`, `ContentRoot`, `GripPoint` opcional | `Content Root`; `Station` pode ficar vazio quando o copo comeca filho do blender. |
| `Cooktop` | `CooktopSlot`, collider, `OutlineHighlightable` | `Model`, `ContainerAnchor` | `Container Anchor` recebe o ponto de encaixe; `Starting Container` e opcional. |

O `BlenderCup` deve ser uma instancia aninhada do prefab e filho de `Blender/CupAnchor`. Copo e motor possuem `OutlineHighlightable` independentes. Clique no copo para pega-lo; clique no motor segurando o copo para encaixa-lo. Sem o copo, o motor nao processa receitas.

`FryingPanStation` e `CuscuzeiraStation` herdam de `HeatedContainerStation`. Por isso, somente avancam o timer enquanto estiverem encaixadas em um `CooktopSlot`. Ao pegar o recipiente, ele libera a boca e pausa; ao clicar numa boca vazia segurando o recipiente, ele encaixa e retoma. Uma futura panela recebe a mesma capacidade herdando de `HeatedContainerStation`, sem alterar o cooktop.

## Plate

| GameObject | Componentes | Configuracao |
|---|---|---|
| `Plate` | componentes de objeto + `Plate` | `Food Root` aponta para o filho abaixo. |
| `FoodRoot` | somente `Transform` | Pivot onde o visual da comida aparece. |

O prato nao possui lista de comidas. Ele clona o proprio objeto recebido, remove fisica/scripts de gameplay da copia e aplica a pose `Plate` do `ObjectVisualPreset`.

## Visual Do Objeto

| Componente | Campo | Uso |
|---|---|---|
| `ObjectVisualPreset` | `Placements` | Fica no root do prefab real do objeto, nao em prefab separado. |
| Placement | `Target` | `Plate`, `FryingPan`, `Blender`, `Cuscuzeira` ou `Default`. |
| Placement | posicao, rotacao, escala | Ajuste local a partir do anchor do destino. |

O visual nasce assim: o container/prato instancia uma copia do prefab do objeto, tira `Rigidbody`, colliders e scripts de gameplay, coloca essa copia como filha do anchor e aplica a pose do `ObjectVisualPreset`.

Para um objeto entrar em um container, ele precisa ter uma entrada exata para o target daquele container. Exemplo: para entrar no liquidificador, precisa de `Blender`; para entrar na frigideira, precisa de `FryingPan`; para entrar na cuscuzeira, precisa de `Cuscuzeira`. A entrada `Default` serve apenas como fallback de pose quando o visual ja foi aceito por outro caminho.

| Ajuste | Onde fazer |
|---|---|
| Corrigir pivot torto/importacao do FBX | No filho `Model` dentro do prefab do objeto. |
| Definir como aparece no prato | `ObjectVisualPreset` no root, entrada `Plate`. |
| Definir como aparece na frigideira | `ObjectVisualPreset` no root, entrada `FryingPan`. |
| Definir como aparece no blender | `ObjectVisualPreset` no root, entrada `Blender`. |
| Tamanho padrao para qualquer container | Entrada `Default`. |

Sem uma entrada especifica, usa `Default`; sem `ObjectVisualPreset`, usa transform local zerado e escala `1`.

## Spawners

| Prefab | `ObjectSpawner.prefab` | Configuracao |
|---|---|---|
| `EggSpawner` | `Egg` | `Give Directly To Holder` ligado. |
| `CornSpawner` | `Corn` | `Give Directly To Holder` ligado. |
| `PlateSpawner` | `Plate` | `Give Directly To Holder` ligado. |

## Player

| GameObject | Componentes principais | Filhos |
|---|---|---|
| `Player` | `PlayerInput`, `PlayerMovement`, `PlayerCamera`, `PlayerCursor`, `PlayerInteractor`, `CharacterController` | `Model`, `CameraPivot` |
| `Model` | nenhum obrigatorio | `Body`, `Head` |
| `CameraPivot` | nenhum | `Camera` |
| `Camera` | `Camera`, `AudioListener` | `Holder` |
| `Holder` | `Holder` | nenhum |

`Head` deve ficar invisivel em primeira pessoa; `Body` continua visivel.

## Outros

| Prefab | Componentes |
|---|---|
| Porta | `OpenableDoor`, collider; pivot na dobradica. |
| Prop pegavel | estrutura de objeto pegavel e uma `ObjectDefinition` propria. |
| Prop fixo | renderers e collider apenas quando necessario. |

## Estruturas Vazias

| Prefab | Onde colocar o modelo | Componentes ja preparados |
|---|---|---|
| `Characters/Customer_Marcia` | `Model` | `CapsuleCollider`, `DialogueAnchor`. |
| `Characters/Customer_Nino` | `Model` | `CapsuleCollider`, `DialogueAnchor`. |
| `Characters/Customer_SeuZe` | `Model` | `CapsuleCollider`, `DialogueAnchor`. |
| `Environment/FoodTruck` | `ExteriorModel` e `InteriorModel` | `CustomerQueueRoot`, `PlayerSpawnPoint`. |
| `Environment/Door` | `Pivot/Model` | `OpenableDoor`, pivot, collider e eixo Z. |
| `Environment/TrashCan` | `Model` | Collider e `DepositPoint`; aguarda o sistema de descarte. |
| `Environment/ServiceCounter` | `Model` | Trigger, `PlateAnchor` e `CustomerAnchor`; aguarda pedidos/entrega. |
| `Props/Radio` | `Model` | Objeto pegavel, collider, `Radio.asset`; aguarda sistema de audio. |
| `Props/SaltShaker` | `Model` | Objeto pegavel, collider e `SaltShaker.asset`. |
| `Props/PepperShaker` | `Model` | Objeto pegavel, collider e `PepperShaker.asset`. |
| `UI/HUD` | UI sob o proprio root | `RectTransform`; aguarda feedback de interacao e pedidos. |
| `UI/DialoguePanel` | UI sob o proprio root | `RectTransform`; aguarda o sistema de dialogo. |
| `UI/OrderTicket` | UI sob o proprio root | `RectTransform`; aguarda o sistema de pedidos. |
| `UI/MainMenu` | UI sob o proprio root | `RectTransform`; aguarda a logica de menu. |
| `UI/ResultsScreen` | UI sob o proprio root | `RectTransform`; aguarda o sistema de resultados. |

## Checklist

| Feito | Verificacao |
|---|---|
| [ ] | Cada objeto aponta para seu `ObjectDefinition`. |
| [ ] | Cada comida empratavel tem `PlateableObject`. |
| [ ] | Cada objeto que aparece em container/prato tem `ObjectVisualPreset` no proprio prefab. |
| [ ] | Todas as stations apontam para `RecipeBook.asset`. |
| [ ] | `RecipeBook.Charcoal Prefab` aponta para `Charcoal`. |
| [ ] | `FriedEgg` possui `RotationTransform`; os outros objetos nao. |
| [ ] | `BlenderCup.ContentRoot` esta preenchido e o copo inicia filho do blender. |
| [ ] | `FryingPan.ItemAnchor` e o root das cascas estao preenchidos. |
| [ ] | `Cuscuzeira.Show Contents` esta desligado. |
| [ ] | Cada cooktop possui `ContainerAnchor`; `Starting Container` aponta para o recipiente que comeca naquela boca. |
| [ ] | `BlenderCup` continua uma instancia aninhada do prefab, com collider convexo e highlight proprio. |
