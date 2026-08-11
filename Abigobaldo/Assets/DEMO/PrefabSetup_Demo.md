# Prefab Setup DEMO - Container Simulator

A demo voltou para uma logica simples:

- `E` interage.
- Todo container guarda no maximo 1 item.
- Se o container estiver vazio e voce estiver segurando um ingrediente valido, ele recebe o item.
- Se o container tiver item dentro e voce clicar nele com a mao vazia, voce tira o item.
- Se o container tiver item dentro e voce clicar nele segurando um prato vazio, ele emprata e remove o item do container.
- O tempo/estado fica no item com `DemoCookableItem`, nao no container.
- Frigideira e cuscuzeira so esquentam.
- Liquidificador processa/gira.

Frigideira, cuscuzeira e blender NAO precisam ser prefabs. Pode usar direto os objetos da cena. Eles so precisariam virar prefab se voce quisesse duplicar a mesma estacao varias vezes ou instanciar por codigo. Para a demo, configure direto na cena.

## Pastas

```text
Assets/DEMO
  Data/Recipes
  Prefabs/Objects
  Prefabs/Player
  Prefabs/Spawners
  Prefabs/Visuals
  Scripts
  Shaders
```

## Item Pegavel

Todo item pegavel usa:

```text
Object_AlgumaCoisa
  Model
  GripPoint opcional
```

No root:

- `DemoHoldableObject`
- `DemoObjectIdentity`
- `Rigidbody`
- Collider
- `DemoCookableItem` apenas se esse item guarda tempo/estado de cozinha

`DemoObjectIdentity.Kind` define o tipo: `Egg`, `FriedEgg`, `Corn`, `CornFlakes`, `Cuscuz`, `Omelet`, `RoastedCorn`, `Charcoal`, etc.

## Estados

Estados de comida:

- `Raw`
- `AlmostReady`
- `Ready`
- `Overdone`
- `Burned`
- `Carbonized`

Tempo padrao sugerido:

- `AlmostReady Time`: 5
- `Ready Time`: 10
- `Overdone Time`: 15
- `Burned Time`: 20
- `Carbonized Time`: 25

Se `Carbonized Turns Into Charcoal` estiver ligado, o container troca o item por `Object_Charcoal`.

## Receita

As receitas base ja foram criadas em:

```text
Assets/DEMO/Data/Recipes
```

Arquivos:

- `Recipe_FryingPan_FriedEgg`
- `Recipe_FryingPan_Omelet`
- `Recipe_FryingPan_RoastedCorn`
- `Recipe_Blender_CornFlakes`
- `Recipe_Cuscuzeira_Cuscuz`

Se precisar criar outra receita: `Create > Abigobaldo Demo > Recipe`.

Campos importantes:

- `Container Kind`: onde a receita funciona.
- `Input Kind`: ingrediente inicial.
- `Resume Input Kinds`: tipos que podem voltar para o container sem resetar.
- `Output On Insert Prefab`: prefab que substitui o ingrediente assim que entra.
- `Output When Ready Prefab`: prefab que substitui quando chega em `Ready`.
- `Charcoal Prefab`: `Object_Charcoal`.
- `Contained Visual Prefab`: opcional, visual separado dentro do container.
- `State Visuals`: material/modelo opcional por estado.
- `Spawned On Insert Prefabs`: coisas que nascem ao colocar ingrediente, tipo cascas de ovo.

Se nao tiver `Contained Visual Prefab`, o proprio item fica no `ItemAnchor`.

## Frigideira

Objeto da cena:

```text
Station_FryingPan
  ItemAnchor
  SideEffectSpawnRoot
  Model
```

No root:

- Collider
- `DemoContainerStation`

Config:

- `Container Kind`: `FryingPan`
- `Item Anchor`: ponto em cima da frigideira
- `Side Effect Spawn Root`: ponto onde as cascas aparecem
- `Recipes`: receitas da frigideira

Receitas:

### Ovo Frito

- `Container Kind`: `FryingPan`
- `Input Kind`: `Egg`
- `Resume Input Kinds`: `FriedEgg`
- `Output On Insert Prefab`: `Object_FriedEgg`
- `Charcoal Prefab`: `Object_Charcoal`
- `Contained Visual Prefab`: opcional, visual do ovo em cima da frigideira
- `Spawned On Insert Prefabs`: `Object_EggShellA`, `Object_EggShellB`
- `State Visuals`: coloque materiais por estado para mudar cor

### Milho Assado

- `Container Kind`: `FryingPan`
- `Input Kind`: `Corn`
- `Resume Input Kinds`: `RoastedCorn` ou `Corn`, dependendo do prefab que voce usar
- `Output On Insert Prefab`: vazio se quiser usar o proprio milho
- `Charcoal Prefab`: `Object_Charcoal`
- `State Visuals`: materiais por estado

## Omelete

No `Object_FriedEgg`, adicione `DemoCookableItem` e configure:

- `Hand Mix Output Prefab`: `Object_Omelet`
- `Hand Mix Required State`: `AlmostReady`
- `Hand Mix Required Intensity`: algo entre 60 e 100

Fluxo:

1. Ovo entra na frigideira e vira `Object_FriedEgg`.
2. Antes/depois de chegar em `AlmostReady`, tire o `Object_FriedEgg`.
3. Segure `R` e mexa o mouse bastante.
4. Ele vira `Object_Omelet` cru.
5. Coloque o omelete na frigideira para cozinhar.

Receita do omelete:

- `Container Kind`: `FryingPan`
- `Input Kind`: `Omelet`
- `Resume Input Kinds`: `Omelet`
- `Output On Insert Prefab`: vazio
- `Charcoal Prefab`: `Object_Charcoal`
- `State Visuals`: materiais por estado

## Liquidificador

Objeto da cena:

```text
Station_Blender
  ItemAnchor
  Model
```

No root:

- Collider
- `DemoContainerStation`

Config:

- `Container Kind`: `Blender`
- `Item Anchor`: ponto dentro do copo
- `Recipes`: receita do milho para flocao

Receita milho -> flocao:

- `Container Kind`: `Blender`
- `Input Kind`: `Corn`
- `Output When Ready Prefab`: prefab do flocao, por exemplo `Object_CornFlakes`
- `Contained Visual Prefab`: opcional, milho pequeno dentro do copo
- `Spins In Container`: ligado
- `Spin Speed`: 720
- `Ready Time`: 5
- `Carbonized Turns Into Charcoal`: desligado
- `Can Burn`: desligado

## Cuscuzeira

Objeto da cena:

```text
Station_Cuscuzeira
  ItemAnchor
  Model
```

No root:

- Collider
- `DemoContainerStation`

Config:

- `Container Kind`: `Cuscuzeira`
- `Item Anchor`: pode ser um ponto dentro/centro dela, mesmo que o modelo nao mostre nada
- `Recipes`: receita do flocao para cuscuz

Receita flocao -> cuscuz:

- `Container Kind`: `Cuscuzeira`
- `Input Kind`: `CornFlakes`
- `Resume Input Kinds`: `Cuscuz`
- `Output On Insert Prefab`: `Object_Cuscuz`
- `Charcoal Prefab`: `Object_Charcoal`
- `Contained Visual Prefab`: vazio
- `State Visuals`: materiais por estado

A cuscuzeira nao precisa mudar visualmente. Depois a gente coloca particula de vapor/fumaca.

## Prato

```text
Object_Plate
  Model
  FoodRoot
```

No root:

- `DemoHoldableObject`
- `DemoObjectIdentity` com `Kind = Plate`
- `DemoPlate`
- `Rigidbody`
- Collider

`DemoPlate.Plated Food Visuals`:

- `FriedEgg` -> `Visual_FriedEgg_Plated`
- `Omelet` -> `Visual_Omelet_Plated`
- `Cuscuz` -> `Visual_Cuscuz_Plated`
- `RoastedCorn` -> visual do milho assado, se criar
- `Charcoal` -> `Visual_Charcoal_Plated`

Visual de prato nao deve ter script, rigidbody ou collider. E so mesh/renderers.
