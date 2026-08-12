# Prefab Setup - Versao Simples

Esta e a versao para testar logo: container simulator, sem zoom station.

## Controles

- `Mouse Esquerdo`: pegar objeto, pegar item no spawner e segurar/interagir com portas.
- `E`: acao culinaria: depositar item em container, retirar item de container e empratar.
- `G` clicado: soltar.
- `G` segurado: arremessar para onde a camera olha.
- `R` segurado + mouse: rotacionar item na mao. Isso tambem mistura o ovo quase pronto para virar omelete.
- Scroll: aproxima/afasta item na mao.

## Regra Geral

- Todo container aceita 1 item por vez.
- Se o container esta vazio e o item combina com uma receita, ele entra.
- Se o container tem item e voce esta de mao vazia, voce tira o item.
- Se o container tem item pronto e voce segura um prato vazio, ele emprata.
- Containers podem ser pegos como objetos. O conteudo fica filho do container e continua dentro dele.
- Enquanto o container esta na mao, o cozimento pausa.
- O timer fica no item com `CookableItem`, nao no container.
- Tirar e recolocar o item continua de onde parou.
- Itens so podem ser empratados quando chegam em `Ready` ou pior. `Raw` e `AlmostReady` nao entram no prato.
- Para virar omelete, o ovo frito precisa estar em `AlmostReady` e acumular rotacao suficiente na mao usando `R` + mouse.

## Pastas

```text
Assets
  Audio/SFX
  Audio/Musicas
  Images
  ImportedAssets
  Materials
  Materials/Textures
  Models
  Prefabs/Objects
  Prefabs/Player
  Prefabs/Spawners
  Prefabs/Visuals
  Scenes
  _Scripts
  _Scripts/Data/RecipeBooks
  _Scripts/Data/Recipes
  Settings
  Shaders
```

## Objeto Pegavel

No root do prefab:

- `HoldableObject`
- `ObjectIdentity`
- `Rigidbody`
- Collider

Opcional:

- `GripPoint`: filho vazio para ajustar como o objeto fica na mao.
- `CookableItem`: nao precisa colocar manualmente em comida de receita; o container adiciona quando necessario.

## Recipes

As receitas ficam ligadas pelo asset unico:

- `Assets/_Scripts/Data/RecipeBooks/DemoRecipeBook.asset`

Todo station deve receber esse `DemoRecipeBook`. O station nao precisa escolher tipo no Inspector; cada script ja sabe o proprio tipo.

Receitas cadastradas nele:

- `FryingPan_FriedEggRecipe`: `Egg` vira `FriedEgg` cru, solta cascas, cozinha e pode queimar.
- `FryingPan_OmeletRecipe`: `Omelet` cozinha e pode queimar.
- `FryingPan_RoastedCornRecipe`: `Corn` vira `RoastedCorn` cru, cozinha e pode queimar.
- `Blender_CornFlakesRecipe`: `Corn` gira no eixo Z por 5s e vira `CornFlakes`. Nao queima.
- `Cuscuzeira_CuscuzRecipe`: `CornFlakes` vira `Cuscuz` cru, cozinha e pode queimar.

No Blender, o item nao fica na base. Ele fica dentro de `BlenderCup.ContentRoot`, entao ao pegar o copo o conteudo vai junto e a receita pausa ate o copo encaixar de novo.

## Visual Prefab Unico

Use um prefab visual por comida quando possivel. Esse prefab pode servir para prato, frigideira, liquidificador e outros containers.

No root do prefab visual:

- `ObjectVisualPreset`

Em `Placements`, crie uma entrada por lugar onde esse visual aparece:

- `Default`: fallback se nao existir configuracao especifica.
- `Plate`: pose quando esta no prato.
- `FryingPan`: pose quando esta na frigideira.
- `Blender`: pose quando esta no liquidificador.
- `Cuscuzeira`: pose caso algum dia a cuscuzeira mostre conteudo visual.

Cada entrada controla:

- `Prefab Override`: opcional. Use se nesse lugar precisa trocar o modelo.
- `Local Position`: posicao local dentro do root/anchor.
- `Local Euler Angles`: rotacao local.
- `Local Scale`: escala local. Se deixar `(0, 0, 0)`, o jogo usa `(1, 1, 1)`.

Exemplo: `FriedEggVisual` pode ter `Plate` pequeno e centralizado no prato, `FryingPan` maior e mais baixo na frigideira, e `Blender` vazio/sem uso.

Estados:

- `Raw`: 0s
- `AlmostReady`: 5s
- `Ready`: 10s
- `Overdone`: 15s
- `Burned`: 20s
- `Carbonized`: 25s, vira `Charcoal` se a receita mandar.

## Frigideira

Objeto da cena com `FryingPanStation`.

Campos:

- `Recipe Book`: `DemoRecipeBook`
- `Item Anchor`: ponto em cima da frigideira onde o item aparece.
- `Side Effect Spawn Root`: ponto onde cascas de ovo aparecem.
- `Show Contained Object`: ligado.
- `Create Fallback Particles`: ligado.

Fluxo ovo:

1. Pega `Egg`.
2. `E` na frigideira.
3. O ovo some, nasce `FriedEgg` cru em cima da frigideira.
4. Cascas nascem fora.
5. Depois de 10s fica pronto.
6. Pode tirar com mao vazia ou empratar com prato.

Fluxo omelete:

1. Faca o ovo entrar na frigideira.
2. Tire quando estiver `AlmostReady`.
3. Segure `R` e mexa o mouse bastante.
4. Ele vira `Omelet` cru.
5. Coloque de volta na frigideira ate ficar pronto.

## Blender

O blender da cena usa `BlenderStation`. O copo e prefab separado: `BlenderCup`.

`BlenderCup` precisa ter:

- `HoldableObject`
- `ObjectIdentity` com `Kind = BlenderCup`
- `BlenderCupContent`
- `Rigidbody`
- Collider
- Filho `ContentRoot`

O `ContentRoot` fica dentro do copo. O `BlenderStation` usa esse transform automaticamente quando precisa guardar o item/conteudo.

Regras:

- O copo comeca encaixado na base porque esta como filho dela na cena.
- Se pegar o copo e apertar `E` na base com ele na mao, ele volta para a posicao original.
- Se tiver comida dentro, o copo ainda pode ser pego. A comida continua filha do `ContentRoot` e a receita pausa ate encaixar o copo de novo.
- Para testar: coloque `Corn`, espere 5s, ele vira `CornFlakes`.

## Cuscuzeira

Objeto da cena com `CuscuzeiraStation`.

Campos:

- `Recipe Book`: `DemoRecipeBook`
- `Side Effect Spawn Root`: pode ficar vazio.
- `Show Contained Object`: desligado.
- `Create Fallback Particles`: ligado.

Ela nao mostra item dentro. Ela so guarda estado, cozinha, solta fumaca e deixa empratar/tirar quando fizer sentido.

## Prato

Prefab `Plate`:

- `HoldableObject`
- `ObjectIdentity` com `Kind = Plate`
- `Plate`
- `Rigidbody`
- Collider
- Filho `Root` ou `FoodRoot` para posicionar comida

`Plate.Plated Food Visuals` deve mapear:

- `FriedEgg` -> `PlatedFriedEggVisual`
- `Omelet` -> `PlatedOmeletVisual`
- `Cuscuz` -> `PlatedCuscuzVisual`
- `Charcoal` -> `PlatedCharcoalVisual`

## Particulas

Jeito rapido:

- Nao configure nada. Frigideira/cuscuzeira criam uma fumaca simples em runtime.

Jeito bonito:

1. Crie um filho chamado `Particles` no container.
2. Adicione `ParticleSystem`.
3. Adicione `StationParticles` no mesmo objeto.
4. Configure forma, quantidade, tamanho e lifetime no ParticleSystem da Unity.
5. O codigo liga/desliga e troca a cor conforme o estado da comida.

## Console

Os containers logam:

- item recebido;
- estado novo com tempo;
- item pronto/processado;
- item empratado;
- falhas simples, tipo item errado ou comida ainda crua.
