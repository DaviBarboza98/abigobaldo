# Prefab Setup DEMO - Versao Simples

Esta e a versao para testar logo: container simulator, sem zoom station.

## Controles

- `Mouse Esquerdo`: pegar objeto.
- `E`: interagir com spawner/container/porta.
- `G` clicado: soltar.
- `G` segurado: arremessar para onde a camera olha.
- `R` segurado + mouse: rotacionar item na mao. Isso tambem mistura o ovo quase pronto para virar omelete.
- Scroll: aproxima/afasta item na mao.

## Regra Geral

- Todo container aceita 1 item por vez.
- Se o container esta vazio e o item combina com uma receita, ele entra.
- Se o container tem item e voce esta de mao vazia, voce tira o item.
- Se o container tem item pronto e voce segura um prato vazio, ele emprata.
- O timer fica no item com `DemoCookableItem`, nao no container.
- Tirar e recolocar o item continua de onde parou.
- Itens so podem ser empratados quando chegam em `Ready` ou pior. `Raw` e `AlmostReady` nao entram no prato.

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

## Objeto Pegavel

No root do prefab:

- `DemoHoldableObject`
- `DemoObjectIdentity`
- `Rigidbody`
- Collider

Opcional:

- `GripPoint`: filho vazio para ajustar como o objeto fica na mao.
- `DemoCookableItem`: nao precisa colocar manualmente em comida de receita; o container adiciona quando necessario.

## Recipes

Ja existem:

- `Recipe_FryingPan_FriedEgg`: `Egg` vira `FriedEgg` cru, solta cascas, cozinha e pode queimar.
- `Recipe_FryingPan_Omelet`: `Omelet` cozinha e pode queimar.
- `Recipe_FryingPan_RoastedCorn`: `Corn` vira `RoastedCorn` cru, cozinha e pode queimar.
- `Recipe_Blender_CornFlakes`: `Corn` gira no eixo Z por 5s e vira `CornFlakes`. Nao queima.
- `Recipe_Cuscuzeira_Cuscuz`: `CornFlakes` vira `Cuscuz` cru, cozinha e pode queimar.

Estados:

- `Raw`: 0s
- `AlmostReady`: 5s
- `Ready`: 10s
- `Overdone`: 15s
- `Burned`: 20s
- `Carbonized`: 25s, vira `Object_Charcoal` se a receita mandar.

## Frigideira

Objeto da cena com `DemoFryingPanStation`.

Campos:

- `Container Kind`: `FryingPan`
- `Item Anchor`: ponto em cima da frigideira onde o item aparece.
- `Side Effect Spawn Root`: ponto onde cascas de ovo aparecem.
- `Show Contained Object`: ligado.
- `Create Fallback Particles`: ligado.
- `Recipes`: ovo frito, omelete e milho assado.

Fluxo ovo:

1. Pega `Object_Egg`.
2. `E` na frigideira.
3. O ovo some, nasce `Object_FriedEgg` cru em cima da frigideira.
4. Cascas nascem fora.
5. Depois de 10s fica pronto.
6. Pode tirar com mao vazia ou empratar com prato.

Fluxo omelete:

1. Faca o ovo entrar na frigideira.
2. Tire quando estiver `AlmostReady`.
3. Segure `R` e mexa o mouse bastante.
4. Ele vira `Object_Omelet` cru.
5. Coloque de volta na frigideira ate ficar pronto.

## Blender

O blender da cena usa `DemoBlenderStation`. O copo e prefab separado: `BlenderCup`.

`BlenderCup` precisa ter:

- `DemoHoldableObject`
- `DemoObjectIdentity` com `Kind = BlenderCup`
- `DemoBlenderCupContent`
- `Rigidbody`
- Collider
- Filho `ContentRoot`

O `ContentRoot` fica dentro do copo e e o `Item Anchor` do `DemoBlenderStation`.

Regras:

- O copo comeca encaixado na base porque esta como filho dela na cena.
- Se pegar o copo e apertar `E` na base com ele na mao, ele volta para a posicao original.
- Se tiver comida dentro, o copo fica travado e nao pode ser pego.
- Para testar: coloque `Corn`, espere 5s, ele vira `CornFlakes`.

## Cuscuzeira

Objeto da cena com `DemoCuscuzeiraStation`.

Campos:

- `Container Kind`: `Cuscuzeira`
- `Item Anchor`: pode ficar vazio.
- `Side Effect Spawn Root`: pode ficar vazio.
- `Show Contained Object`: desligado.
- `Create Fallback Particles`: ligado.
- `Recipes`: cuscuz.

Ela nao mostra item dentro. Ela so guarda estado, cozinha, solta fumaca e deixa empratar/tirar quando fizer sentido.

## Prato

Prefab `Object_Plate`:

- `DemoHoldableObject`
- `DemoObjectIdentity` com `Kind = Plate`
- `DemoPlate`
- `Rigidbody`
- Collider
- Filho `Root` ou `FoodRoot` para posicionar comida

`DemoPlate.Plated Food Visuals` deve mapear:

- `FriedEgg` -> `Visual_FriedEgg_Plated`
- `Omelet` -> `Visual_Omelet_Plated`
- `Cuscuz` -> `Visual_Cuscuz_Plated`
- `Charcoal` -> `Visual_Charcoal_Plated`

## Particulas

Jeito rapido:

- Nao configure nada. Frigideira/cuscuzeira criam uma fumaca simples em runtime.

Jeito bonito:

1. Crie um filho chamado `Particles` no container.
2. Adicione `ParticleSystem`.
3. Adicione `DemoStationParticles` no mesmo objeto.
4. Configure forma, quantidade, tamanho e lifetime no ParticleSystem da Unity.
5. O codigo liga/desliga e troca a cor conforme o estado da comida.

## Console

Os containers logam:

- item recebido;
- estado novo com tempo;
- item pronto/processado;
- item empratado;
- falhas simples, tipo item errado ou comida ainda crua.
