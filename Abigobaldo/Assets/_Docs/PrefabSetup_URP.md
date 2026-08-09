# Abigobaldo - setup de prefabs URP

Este projeto agora usa URP. Materiais comuns devem usar `Universal Render Pipeline/Lit`.
Materiais de particula devem usar `Universal Render Pipeline/Particles/Unlit` ou outro shader de particula URP.

## Nomes novos

- **Objeto**: tudo que pode ser pego na mao e tem fisica. Ex: milho, ovo, prato, saleiro, pimenteiro, tabua, copo do liquidificador, frigideira, cuscuzeira.
- **Container**: objeto ou movel que recebe objetos e pode processar receita. Ex: frigideira, cuscuzeira, liquidificador, prato.
- **Spawner de Objeto**: ponto que cria um objeto prefab e entrega para a mao do player.
- **Home Slot**: clone invisivel do objeto na posicao original. Quando voce segura o objeto certo e olha para esse ponto, o clone aparece transparente; ao apertar `E`, o objeto encaixa ali e o clone some.
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
   - `Placement Ghost Color`: cor/transparencia do clone que aparece no ponto original.

O `Highlightable` nao tem mais cor propria para voce configurar em dois lugares. Ele so aplica o efeito usando as cores do `GameInteractionManager`.

## Layers do projeto

Use **Layers**, nao Tags, para gameplay de interacao/fisica. Layers entram direto em raycast e colisao, entao sao melhores para este projeto.

Layers criadas:

- `Player`: prefab do player.
- `Interactable`: objetos interativos genericos.
- `Objeto`: tudo que e pegavel/fisico.
- `Container`: frigideira, cuscuzeira, liquidificador, prato e futuros containers.
- `Door`: portas de armario, geladeira e freezer.
- `Spawner`: spawners de objeto/agua.
- `HomeSlot`: pontos fantasmas de reposicionamento.

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
   - `Objeto Data`: arraste o asset de `Assets/_Data/Objetos`.
   - `Can Be Held`: ligado.
   - `Can Be Thrown`: desligue em containers que nao podem ser jogados, como frigideira/cuscuzeira/copo do liquidificador se desejar.
   - `Create Home Slot From Initial Pose`: ligue em objetos que devem voltar para o ponto original.
   - `Home Slot Padding`: folga extra em cima do tamanho visual real do objeto. O slot pega a posicao/rotacao inicial e cria um clone transparente invisivel ali.

## Spawner de Objeto

Para milho, ovo, agua etc:

1. Crie um GameObject vazio no ponto de spawn.
2. Adicione `BoxCollider` com `Is Trigger` ligado.
3. Adicione `ObjetoSpawner`.
4. Em `Objeto Prefab`, arraste o prefab do objeto.
5. `Spawn Point` e opcional. Se vazio, usa o proprio transform.
6. `Pick Up On Spawn`: ligado para nascer direto na mao.
7. Adicione `Highlightable`.
8. Layer pode ficar `Default`; o `ObjetoSpawner` troca para `Spawner`.

Para o spawner de agua, crie um prefab `Objeto_Agua` com `Objeto` e `ObjetoData Agua`, e use esse prefab no `ObjetoSpawner`.

## Portas

Use em portas de armario, geladeira, freezer e bancadas:

1. O objeto da porta deve ter o pivot/origin no ponto da dobradica.
2. Adicione collider na porta. Pode ser `BoxCollider` ou `MeshCollider`.
3. Adicione `OpenableDoor`.
4. Adicione `Highlightable`.
5. Configure:
   - `Pivot`: vazio se o script estiver no proprio objeto com pivot correto.
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
6. Scripts como `ItemContainer` chamam:
   - `Play()`
   - `Stop()`
   - `SetColor(Color)`
   - `SetRate(float)`

## ItemContainer explicado

`Container Type`:
Tipo do container usado para achar receitas. Ex: `Frigideira`, `Cuscuzeira`, `Liquidificador`.

`Max Items`:
Quantidade maxima de ingredientes guardados antes de bloquear entrada.

`Recipe Database`:
Banco global de receitas. Use `Assets/_Data/Receitas/RecipeDatabase_MVP.asset`.

`Local Recipes`:
Lista opcional de receitas so daquele container. Pode deixar vazio se usar o database.

`Output Spawn Point`:
Ponto onde o resultado aparece se o player retirar o item pronto com a mao vazia. Tambem e usado para subprodutos, tipo cascas de ovo. Se vazio, usa a posicao do container + um offset para cima.

`Can Be Picked Up`:
Liga se o container inteiro pode ser pego na mao. Use em `Frigideira` e `Cuscuzeira`. Para o liquidificador novo, normalmente quem e pegavel e o `CopoDoLiquidificador`, nao a base/motor.

`Container Item Data`:
ObjetoData que representa o proprio container quando ele e pegavel. Ex: `Frigideira.asset`, `Cuscuzeira.asset`.

`Create Stove Slot On Awake`:
Cria automaticamente o ponto de fogao na posicao inicial. Use em frigideira/cuscuzeira se quiser encaixe automatico no local original.

`Stove Slot Size`:
Tamanho da area clicavel para recolocar o container no fogao.

`Use Content Visuals`:
Liga quando o container deve mostrar visualmente o conteudo. Use:
- Frigideira: sim, para ovo/milho aparecer em cima.
- Liquidificador: sim, para ingredientes aparecerem dentro do copo.
- Cuscuzeira: normalmente nao, porque o vapor ja indica cozimento.

`Content Visual Root`:
Transform onde os modelos visuais dos ingredientes/resultados serao instanciados. So precisa se `Use Content Visuals` estiver ligado.

`Content Visual Local Offset`:
Posicao local do visual em relacao ao root.

`Content Visual Scale`:
Escala do visual gerado dentro/em cima do container.

`Frying Motion Radius` / `Frying Motion Speed`:
Movimento visual pequeno enquanto cozinha. Na frigideira parece fritura; no liquidificador parece mexer.

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

`Requires Manual Activation`:
Use no liquidificador. Mao vazia + `E` liga/desliga.

`Show Debug Logs`:
Liga mensagens no Console com tempo e estado da receita.

## Prefabs especificos

### Frigideira

Root:
- `Objeto`
- `ItemContainer`
- `Highlightable`
- `Rigidbody`
- Collider

`Objeto`:
- `Can Be Held`: ligado
- `Can Be Thrown`: desligado
- `Create Home Slot From Initial Pose`: ligado

`ItemContainer`:
- `Container Type`: `Frigideira`
- `Can Be Picked Up`: ligado
- `Container Item Data`: `Frigideira`
- `Create Stove Slot On Awake`: ligado
- `Use Content Visuals`: ligado
- `Content Visual Root`: filho vazio em cima da panela
- `Steam Particles`: `ParticleEmitterController` do filho `Particles`

### Cuscuzeira

Parecida com frigideira, mas:
- `Container Type`: `Cuscuzeira`
- `Use Content Visuals`: desligado, a nao ser que voce queira ver cuscuz dentro.
- Vapor mais forte no `ParticleSystem`.

### Liquidificador base/motor

Root da base:
- Collider
- `ItemContainer`
- `Highlightable`

`ItemContainer`:
- `Container Type`: `Liquidificador`
- `Can Be Picked Up`: desligado
- `Requires Manual Activation`: ligado
- `Use Content Visuals`: ligado
- `Content Visual Root`: vazio dentro do copo encaixado
- `Steam Particles`: vazio/null

### Copo do liquidificador

Prefab separado:
- `Objeto`
- `Rigidbody`
- Collider
- `Highlightable`
- Material transparente URP/Lit com `Surface Type: Transparent`
- `Create Home Slot From Initial Pose`: ligado, se quiser encaixar de volta.

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

Nao coloque `ItemContainer` ainda.

### Saleiro e pimenteiro

- `Objeto`
- `Rigidbody`
- Collider
- `Highlightable`
- `Can Be Held`: ligado
- `Can Be Thrown`: opcional

### Cooktop / bocas do fogao

Como agora cada cooktop esta separado:
- Em cada boca, coloque `StoveSlot` se ela aceitar containers no fogo.
- Adicione `BoxCollider` trigger no ponto clicavel.
- Configure `Accepted Type` para a panela esperada, se estiver usando slots separados.
- Para fogo azul, crie um filho `Particles` com `ParticleSystem` e `ParticleEmitterController`.
- Arraste esse `ParticleEmitterController` para `Flame Particles` no `StoveSlot`.
- Configure cor, shape, range, quantidade e lifetime direto no `ParticleSystem`.

### Armarios, geladeira, freezer

Cada porta separada:
- Collider
- `OpenableDoor`
- `Highlightable`

Se abrir para o lado errado, marque `Invert Direction`.
