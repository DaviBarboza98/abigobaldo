# Cooking Architecture

## Decisoes

| Problema anterior | Solucao atual |
|---|---|
| O enum de comida precisava crescer a cada objeto. | Receitas referenciam assets `ObjectDefinition`. |
| Livro tinha um campo por station. | `RecipeBook` possui uma unica lista. |
| `RecipeData` tinha giro, hand mixing e spin. | Mecanicas especiais ficam nos componentes que realmente as usam. |
| Copo conhecia milho e flocao. | `BlenderCupContent` conhece somente encaixe e `ContentRoot`. |
| Prato listava tipos permitidos. | Cada objeto declara se pode ser empratado e qual visual usa. |
| Tirar e recolocar reiniciava o alimento. | `RecipeProgress` vive no objeto, nao na station. |
| Carbonizado podia depender de cada receita. | `RecipeBook` fornece um unico `Charcoal` obrigatorio para toda comida aquecida. |

## Dependencias

```text
ObjectDefinition <- ObjectIdentity <- HoldableObject prefab
        ^                 |
        |                 v
    RecipeData ------> RecipeProgress
        |                 ^
        v                 |
    RecipeBook ------> ContainerStation
                          |
             +------------+------------+
             |            |            |
        FryingPan       Blender    Cuscuzeira
```

## Limites Intencionais

| Limite | Motivo |
|---|---|
| Uma station tem uma classe concreta. | Comportamentos fisicos e controles sao diferentes; o tipo nao aparece como campo redundante no Inspector. |
| `RecipeStationType` ganha uma entrada para uma station realmente nova. | Adicionar receitas a stations existentes continua totalmente por dados. |
| Receita com varios ingredientes precisa de `In Progress Prefab`. | Depois de consumir varios objetos, deve existir um unico objeto que carregue o progresso. |
| Inventario interno e uma lista, mas a interacao retira o ultimo objeto. | Permite receitas futuras com quantidade sem complicar o controle atual. |

## Proximos Sistemas

| Prioridade | Sistema | Integracao prevista |
|---:|---|---|
| 1 | Clientes e pedidos | Consultar `ObjectDefinition` e `FoodState` do prato. |
| 2 | UI de feedback | Observar `RecipeProgress`, sem controlar a receita. |
| 3 | Audio | Reagir a eventos de station e mudanca de estado. |
| 4 | Receitas com varias porcoes | Expandir capacidade do prato sem mudar reconhecimento de receita. |
