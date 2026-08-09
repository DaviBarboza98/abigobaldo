# Abigobaldo - setup de prefabs URP

Este projeto agora usa URP. Materiais comuns devem usar `Universal Render Pipeline/Lit`.
Materiais de particula devem usar `Universal Render Pipeline/Particles/Unlit` ou outro shader de particula URP.

## Nomes novos

- **Objeto**: tudo que pode ser pego na mao e tem fisica. Ex: milho, ovo, prato, saleiro, pimenteiro, tabua, copo do liquidificador, frigideira, cuscuzeira.
- **Container**: objeto ou movel que recebe objetos e pode processar receita. Ex: frigideira, cuscuzeira, liquidificador, prato.
- **Spawner de Objeto**: ponto que cria um objeto prefab e entrega para a mao do player.
- **Dock Slot**: ponto de encaixe para objetos especiais. Ex: copo do liquidificador na base.
- **Porta**: parte rotacionavel de armario/geladeira/bancada.

`Item` ainda existe so como compatibilidade. Prefabs novos devem usar `Objeto`.
`ItemData` ainda existe para nao quebrar receitas/assets antigos, mas no menu aparece como `Objeto Data`.

## Manager global

Crie um GameObject vazio em `Managers`:

1. Nome: `GameInteractionManager`
2. Componente: `GameInteractionManager`
3. Configure:
   - `Highlight Color`: cor do objeto selecionado.
   - `Emission Color`: cor do brilho no URP.
   - `Emission Intensity`: intensidade do brilho.

O `Highlightable` nao tem mais cor propria para voce configurar em dois lugares. Ele so aplica o efeito usando as cores do `GameInteractionManager`.

Se quiser cor individual em algum objeto especifico, ligue `Use Local Colors` no `Highlightable` desse prefab. Se ficar desligado, ele usa o manager global.

## Botao de normalizacao

Na Unity, use o menu:

`Abigobaldo/Gameplay/Normalizar cena e prefabs`

Ele faz o trabalho chato automaticamente:

- adiciona `Highlightable` onde faltar;
- coloca layers corretas;
- transforma `MeshCollider` de objeto fisico em `Convex`;
- adiciona collider basico quando faltar;
- aplica presets bons de particula;
- salva prefabs nas pastas `Assets/_Prefabs/Objects`, `Assets/_Prefabs/Containers` e `Assets/_Prefabs/Spawners`.

Use esse menu depois de organizar objetos na cena e antes de testar o Play.

## Layers do projeto

Use **Layers**, nao Tags, para gameplay de interacao/fisica. Layers entram direto em raycast e colisao, entao sao melhores para este projeto.

Layers criadas:

- `Player`: prefab do player.
- `Interactable`: objetos interativos genericos.
- `Objeto`: tudo que e pegavel/fisico.
- `Container`: frigideira, cuscuzeira, liquidificador, prato e futuros containers.
- `Door`: portas de armario, geladeira e freezer.
- `Spawner`: spawners de objeto/agua.
- `HomeSlot`: encaixes/slots interativos, como o ponto do copo do liquidificador.

Os scripts principais tentam aplicar a layer automaticamente quando o GameObject ainda esta em `Default`. Se voce colocar uma layer manualmente no prefab, o codigo respeita e nao troca.

## Player

No prefab do player:

- Root na layer `Player`.
- `Model` pode ter filhos `Body` e `Head`.
- `Head` e ocultado pelo `PlayerCamera` em primeira pessoa quando o filho se chama exatamente `Head`.

## Todo prefab de Objeto

Use esta base para milho, ovo, agua, fuba, cuscuz, saleiro, pimenteiro, tabua, prato, copo do liquidificador, frigideira e cuscuzeira:

1. Root do prefab com:
   - `Transform`
   - `Rigidbody`
   - `Collider` ou colliders nos filhos
   - `Objeto`
   - `Highlightable`
   - Layer pode ficar `Default`; o `Objeto` troca para `Objeto` automaticamente ao iniciar.
2. No `Rigidbody`:
   - `Use Gravity`: ligado quando solto; o script ajusta ao pegar.
   - `Collision Detection`: `Continuous Speculative` ou `Continuous Dynamic` para objetos pequenos.
3. No `Objeto`:
   - `Objeto Data`: opcional. Use apenas em objetos que participam de receitas antigas baseadas em `ItemData`.
   - `Can Be Held`: ligado.
   - `Can Be Thrown`: desligue em containers que nao podem ser jogados, como frigideira/cuscuzeira/copo do liquidificador se desejar.
   - `Create Home Slot From Initial Pose`: deixe desligado no fluxo atual.
   - `Home Slot Padding`: legado do encaixe antigo.

## Spawner de Objeto

Para milho, ovo, agua etc:

1. Crie um GameObject vazio no ponto de spawn.
2. Adicione `BoxCollider` com `Is Trigger` ligado.
3. Adicione `ObjetoSpawner`.
4. Em `Objeto Prefab`, arraste o prefab do objeto.
5. Adicione `Highlightable`.
6. Layer pode ficar `Default`; o `ObjetoSpawner` troca para `Spawner`.

O spawner sempre entrega o objeto direto no `ItemHolder` quando a mao esta vazia.

Para o spawner de agua, crie um prefab `Objeto_Agua` com `Objeto` e use esse prefab no `ObjetoSpawner`. Data so e necessaria se a agua entrar em receita.

## Portas

Use em portas de armario, geladeira, freezer e bancadas:

1. O objeto da porta deve ter o pivot/origin no ponto da dobradica.
2. Adicione collider na porta. Pode ser `BoxCollider` ou `MeshCollider`.
3. Adicione `OpenableDoor`.
4. Adicione `Highlightable`.
5. Configure:
   - `Pivot`: vazio se o script estiver no proprio objeto com pivot correto.
   - `Rotation Axis`: normalmente `Z` nas portas atuais.
   - `Max Open Angle`: normalmente `90`.
   - `Follow Speed`: quanto mais alto, mais rapido segue a camera.
   - `Invert Direction`: ligue se a porta estiver abrindo para o lado errado.
6. Layer pode ficar `Default`; o `OpenableDoor` troca para `Door`.

Uso no jogo: olhe para a porta, segure `E`, mova a camera. Ao soltar `E`, ela para onde ficou.

## Particulas

Agora o codigo nao cria particulas automaticamente. Voce configura no prefab/cena.

Estrutura recomendada:

1. Crie um filho vazio chamado `Particles`.
2. Dentro dele, crie um ou mais GameObjects com `ParticleSystem`.
3. Configure tudo no Inspector:
   - `Shape`
   - `Start Lifetime`
   - `Start Speed`
   - `Start Size`
   - `Emission Rate`
   - Material URP de particula
4. No objeto que precisa controlar particula, adicione `ParticleEmitterController`.
5. Arraste o `ParticleSystem` para o campo `Target`.
6. Escolha um `Preset`:
   - `SoftSteam`: vapor leve para frigideira.
   - `HeavySteam`: vapor forte para cuscuzeira.
   - `DarkBurnSmoke`: fumaca escura.
   - `BlueCooktopFlame`: chama azul do cooktop.
7. Clique no menu de contexto do componente e use `Apply Preset`, ou use `Abigobaldo/Gameplay/Normalizar cena e prefabs` para escolher automaticamente pelo nome na hierarquia.
8. Scripts como `FryingPan`, `Cuscuzeira` e `Blender` chamam:
   - `Play()`
   - `Stop()`
   - `SetColor(Color)`
   - `SetRate(float)`

## Scripts de receita

Nao existe mais classe base de receita/container. A logica fica distribuida nos scripts especificos:
- `FryingPan`: ingredientes, fritura, vapor leve, visual em cima da frigideira e estados de queima.
- `Cuscuzeira`: ingredientes, cozimento, vapor forte e estados de queima.
- `Blender`: ingredientes dentro do copo, ligar/desligar, animacao de mistura e resultado final.

As receitas continuam modulares via `RecipeData` e `RecipeDatabase`.

`Max Items`:
Quantidade maxima de ingredientes guardados antes de bloquear entrada.

`Recipe Database`:
Banco global de receitas. Use `Assets/_Data/Receitas/RecipeDatabase_MVP.asset`.

`Local Recipes`:
Lista opcional de receitas so daquele container. Pode deixar vazio se usar o database.

`Output Spawn Point`:
Ponto onde o resultado aparece se o player retirar o item pronto com a mao vazia. Tambem e usado para subprodutos, tipo cascas de ovo. Se vazio, usa a posicao do container + um offset para cima.

`Can Be Picked Up`:
Existe na `FryingPan` e na `Cuscuzeira`. Liga se o container inteiro pode ser pego na mao. No liquidificador novo, quem e pegavel e o `CopoDoLiquidificador`, nao a base/motor.

`Container Item Data`:
ObjetoData que representa o proprio container quando ele e pegavel. Ex: `Frigideira.asset`, `Cuscuzeira.asset`.

`Frying Motion Radius` / `Frying Motion Speed`:
Campos da `FryingPan`. Controlam o movimento visual pequeno enquanto cozinha.

`Blender Morph Start Time`:
Tempo antes dos ingredientes comecarem a encolher no liquidificador.

`Blender Morph Duration`:
Duracao do encolhimento ate virar o resultado.

`Blender Shrink Scale`:
Escala minima antes de trocar para o resultado final.

`Steam Particles`:
Arraste aqui o `ParticleEmitterController` configurado por voce. O codigo so liga/desliga, muda cor e muda rate.

`Steam Color`:
Cor normal da particula.

`Burned Steam Color`:
Cor quando passou do ponto/queimou/carbonizou.

`Steam Rate`:
Quantidade emitida enquanto cozinha.

`Spin Speed` / `Shake Radius` / `Morph Start Time` / `Morph Duration`:
Campos do `Blender`. Controlam os ingredientes girando, mexendo e encolhendo ate virar o resultado.

`Show Debug Logs`:
Liga mensagens no Console com tempo e estado da receita.

## Prefabs especificos

### Frigideira

Root:
- `Objeto`
- `FryingPan`
- `Highlightable`
- `Rigidbody`
- Collider

`Objeto`:
- `Can Be Held`: ligado
- `Can Be Thrown`: desligado
- `Create Home Slot From Initial Pose`: desligado

`FryingPan`:
- `Can Be Picked Up`: ligado
- `Container Item Data`: `Frigideira`
- `Item Surface`: filho vazio em cima da panela
- `Steam Particles`: `ParticleEmitterController` do filho `Particles`

### Cuscuzeira

Parecida com frigideira, mas:
- Use o script `Cuscuzeira`, nao `FryingPan`.
- Vapor mais forte no `ParticleSystem`.

### Liquidificador base/motor

Root da base:
- Collider
- `Blender`
- `Highlightable`

Crie tambem um filho vazio/collider no ponto onde o copo encaixa. O transform desse proprio objeto ja e o encaixe:
- Nome sugerido: `CopoSlot`
- `BoxCollider` com `Is Trigger` ligado
- `BlenderCupSlot`
- `Accepted Data`: data do copo do liquidificador
- `Linked Blender`: pode deixar vazio se o slot for filho da base; ele acha o `Blender` automaticamente.

`Blender`:
- `Cup Content Root`: vazio dentro do copo encaixado
- `Required Cup Slot`: o `BlenderCupSlot` do copo. Com isso a base so liga/processa com o copo encaixado.

### Copo do liquidificador

Prefab separado:
- `Objeto`
- `Rigidbody`
- Collider
- `Highlightable`
- Material transparente URP/Lit com `Surface Type: Transparent`
- `Role`: `CopoLiquidificador`
- `Create Home Slot From Initial Pose`: desligado. O encaixe fica no `BlenderCupSlot` da base.

### Prato

Root:
- `Objeto`
- `PlateContainer`
- `Highlightable`
- `Rigidbody`
- Collider

`PlateContainer`:
- `Max Items`: `1` por enquanto.
- `Content Visual Root`: vazio em cima do prato.

### Tabua de cortar

Por enquanto:
- `Objeto`
- `Rigidbody`
- Collider
- `Highlightable`

Nao coloque script de receita/container ainda.

### Saleiro e pimenteiro

- `Objeto`
- `Rigidbody`
- Collider
- `Highlightable`
- `Can Be Held`: ligado
- `Can Be Thrown`: opcional

### Cooktop / bocas do fogao

Por enquanto o cooktop fica como objeto de cena/visual, sem script proprio.
Para fogo azul, crie um filho `Particles` com `ParticleSystem` e `ParticleEmitterController`, usando o preset `BlueCooktopFlame`.

### Armarios, geladeira, freezer

Cada porta separada:
- Collider
- `OpenableDoor`
- `Highlightable`

Use `Rotation Axis: Z` nas portas atuais. Se abrir para o lado errado, marque `Invert Direction`.
