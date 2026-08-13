# Cooking Setup

## Fluxo

| Etapa | Responsavel |
|---|---|
| Reconhecer o objeto | `ObjectIdentity` compara assets `ObjectDefinition`. |
| Encontrar receita | `RecipeBook` procura um `RecipeData` da station cujos ingredientes correspondam. |
| Guardar tempo e estado | `RecipeProgress` no objeto processado. |
| Aquecer/processar | `ContainerStation`; pausa quando o container esta na mao. |
| Queimar e mudar visual | Estados e aparencias configurados diretamente no `RecipeData`. |
| Transformar ao girar | `RotationTransform`, somente no prefab que possui essa capacidade. |
| Posicionar visual | `ObjectVisualPreset`. |
| Empratar | `Plate` consulta `CanBePlated` e o visual do proprio objeto. |

## Criar Uma Receita

1. Crie ou reutilize um `ObjectDefinition` para cada ingrediente.
2. Crie `RecipeData` em `Assets/_Scripts/Data/Recipes`.
3. Escolha `Required Station` e preencha `Ingredients` com asset e quantidade.
4. Use `In Progress Prefab` apenas quando os ingredientes devem virar outro objeto assim que entram.
5. Use `Result Prefab` apenas quando o objeto deve ser substituido ao ficar pronto.
6. Defina `Processing Time`.
7. Para uma receita aquecida, ligue `Uses Heat` e configure tempos e aparencias de `Raw` ate `Burned`.
8. `Carbonized Time` sempre produz o `Charcoal Prefab` global do `RecipeBook`.
9. Preencha `Byproducts`, se houver, e adicione a receita ao `RecipeBook.asset`.

Adicionar outra receita de frigideira, blender ou cuscuzeira nao exige alterar C#.

## Receitas Atuais

| Receita | Entrada | Durante processo | Resultado | Usa calor |
|---|---|---|---|---|
| `FryingPan_FriedEggRecipe` | `Egg` | `FriedEgg` cru | mesmo objeto | Sim |
| `FryingPan_OmeletRecipe` | `Omelet` | mesmo objeto | mesmo objeto | Sim |
| `FryingPan_RoastedCornRecipe` | `Corn` | `RoastedCorn` cru | mesmo objeto | Sim |
| `Blender_CornFlakesRecipe` | `Corn` | mesmo objeto | `CornFlakes` | Nao |
| `Cuscuzeira_CuscuzRecipe` | `CornFlakes` | `Cuscuz` cru | mesmo objeto | Sim |

## Tempos Atuais

| Estado | Tempo total em receita de fogo |
|---|---:|
| `Raw` | 0 a 5 s |
| `AlmostReady` | 5 a 10 s |
| `Ready` | 10 a 15 s |
| `Overdone` | 15 a 20 s |
| `Burned` | 20 a 25 s |
| `Carbonized` | 25 s; substitui por `Charcoal` |

O blender usa apenas `Processing Time = 5`; ao terminar, para automaticamente e nao queima.

## Controles

| Controle | Acao |
|---|---|
| `E` | Depositar, retirar ou empratar em containers; encaixar o copo na base. |
| Clique esquerdo | Pegar objetos, acionar o blender e manipular portas. |
| `G` | Soltar; segurado, arremessar para a frente da camera. |
| `R` + mouse | Rotacionar objeto; em `FriedEgg` quase pronto acumula 720 graus para virar omelete cru. |
| Scroll | Aproximar ou afastar o objeto na mao. |
