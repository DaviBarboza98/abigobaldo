# Handoff do projeto Abigobaldo

Leia isto primeiro e depois inspecione o projeto Unity. O projeto esta em desenvolvimento ativo, portanto o codigo, os prefabs e a cena atual sempre vencem este texto se algo tiver mudado.

## O jogo

Abigobaldo e um jogo de culinaria em primeira pessoa, cartoon/low poly, dentro de um food truck brasileiro.

Abigobaldo e um cozinheiro alegre que prepara comida regional para pessoas em situacao de vulnerabilidade. O foco nao deve ser tratar essas pessoas como "pontuacao", mas mostrar acolhimento, conversa, comida e felicidade. O projeto se relaciona com a ODS 2, Fome Zero, e com saude/bem-estar.

O loop desejado para a primeira versao completa e:

1. abrir o food truck;
2. receber um cliente;
3. conversar ou anotar seu pedido;
4. pegar ingredientes;
5. cozinhar;
6. empratar;
7. entregar;
8. ver a reacao;
9. terminar a noite em uma tela de resultados focada na felicidade das pessoas.

O primeiro slice planejado tem tres clientes: dois fazem pedidos e um demonstra o sistema de dialogo. Os pratos principais sao cuscuz, ovo frito e omelete. Milho assado tambem existe atualmente como receita extra.

## Direcao do projeto

- Codigo, variaveis, nomes tecnicos e assets devem ficar em ingles.
- `Cuscuz` e `Cuscuzeira` sao nomes corretos e devem permanecer assim.
- "Item" foi substituido por "Object": use `HoldableObject` e `Holder`.
- O usuario prefere uma solucao simples e funcional a uma arquitetura enorme.
- Nao crie campos redundantes no Inspector. Um `FryingPanStation` ja e uma frigideira; ele nao precisa de `ContainerKind = FryingPan`.
- O usuario cria/ajusta modelos e prefabs visuais. O agente deve cuidar da logica, componentes, referencias e dados sempre que puder.
- Antes de editar, analise a cena, os prefabs e os scripts reais.
- Nao altere `global.json`: ele usa o SDK escolar `6.0.428`.
- Nao reverta mudancas locais do usuario.

## Ideias que foram abandonadas

Nao reintroduza sem conversar:

- zoom estilo Cooking Mama;
- `RecipeContainer` ou `ItemContainer` antigos;
- `OutputSpawnPoint` para comida pronta;
- fantasma do objeto mostrando seu lugar original;
- `HomeSlot` generico;
- script antigo de fogao;
- recipes com campos hardcoded como "Hand Mixing";
- BlenderCup com campos especificos de milho/flocao;
- agua e simulacao fisica de varios graos no liquidificador;
- particulas controladas por um sistema de codigo. O usuario vai configurar VFX depois.

## Controles definidos

| Controle | Acao |
|---|---|
| WASD | Andar |
| Shift | Correr |
| Clique esquerdo | Pegar objetos, usar spawners, mexer portas, operar controles e encaixar containers |
| E | Interagir com conteudo: inserir, retirar ou transferir |
| G curto | Soltar |
| G segurado | Arremessar para onde a camera olha |
| R + mouse | Rotacionar o objeto na mao |
| Scroll | Aproximar ou afastar o objeto |
| V | Liberar/travar cursor |
| P | Pausar |

## Logica principal que deve ser preservada

### Objetos

- Objetos pegaveis usam `HoldableObject`, Rigidbody e collider.
- Enquanto segurados, continuam com colisao contra o mundo.
- Dentro de um container ficam parentados ao `Content Anchor`, kinematic e sem colisao.
- Containers pegaveis preservam o objeto real dentro deles.
- Frigideira e cuscuzeira nao devem ser arremessaveis.
- O arremesso usa o forward da camera, nunca o eixo local do Holder.

### Containers

- `E` com objeto na mao mirando no container insere o objeto.
- `E` com container na mao mirando em objeto insere o objeto no container segurado.
- `E` entre dois containers transfere o ultimo conteudo.
- `E` com mao vazia retira o ultimo conteudo.
- Inserir algo em um container pegavel nao troca automaticamente o que esta na mao pelo container.
- `Plate` e um container de capacidade 1.
- Containers nunca podem ser empratados.

### Visual dentro de containers

- Cada prefab real possui um unico `ObjectVisualPreset`.
- Nele sao configurados position, rotation e scale locais para Plate, FryingPan, Blender e Cuscuzeira.
- A referencia e o `Content Anchor` do container.
- Um objeto so entra se possuir o target daquele container em seu `ObjectVisualPreset`.
- O sistema guarda o objeto real invisivel e mostra um clone visual sem Rigidbody/collider/gameplay.
- Cuscuzeira nao mostra conteudo. Ela so guarda a informacao.
- Nao use a pasta de visual prefabs como sistema principal; ela e em grande parte legado.

### Receitas

- `RecipeData` define ingredientes, station, tempos, aparencias, prefabs de transformacao e byproducts.
- Todas as stations apontam para o mesmo asset `RecipeBook.asset`.
- `ObjectDefinition` serve apenas como identidade estavel para receitas e futuros pedidos. Mantenha-o pequeno.
- O progresso fica no proprio alimento por meio de `RecipeProgress`.
- Tirar e recolocar comida nao pode reiniciar seu estado.
- Receitas comuns novas em stations existentes devem ser adicionadas por dados, sem alterar C#.
- Mecanicas especiais pertencem a componentes especificos, nao a campos universais de RecipeData.

Estados de comida aquecida:

| Estado | Tempo atual |
|---|---:|
| Raw | 0 s |
| AlmostReady | 5 s |
| Ready | 10 s |
| Overdone | 15 s |
| Burned | 20 s |
| Carbonized | 25 s |

Todo alimento carbonizado obrigatoriamente vira o prefab global `Charcoal`, substituindo objeto, identidade e visual anteriores.

### Receitas atuais

- Egg + FryingPan -> FriedEgg cru; duas cascas surgem imediatamente.
- Corn + Blender por 5 s -> CornFlakes; o Blender desliga sozinho.
- CornFlakes + Cuscuzeira -> Cuscuz.
- FriedEgg em AlmostReady + cinco segundos mexendo com `R` -> Omelet cru.
- Omelet + FryingPan -> Omelet cozido.
- Corn + FryingPan -> RoastedCorn.

Omelete e baseado em cinco segundos de movimento ativo, nao em velocidade nem graus acumulados.

### Stations

- `FryingPanStation` e `CuscuzeiraStation` herdam de `HeatedContainerStation`.
- Elas so cozinham encaixadas em `CooktopSlot`.
- Ao pega-las, saem do cooktop e pausam.
- Ao encaixar novamente, retomam.
- `CooktopSlot` deve aceitar futuras panelas que tambem herdem de `HeatedContainerStation`.

### Liquidificador

- `Blender` e a base fixa.
- `BlenderCup` e outro prefab, pegavel e removivel.
- O cup comeca como prefab aninhado dentro de `Blender/CupAnchor`.
- Motor e copo precisam de collider/highlight independentes.
- Clique no copo pega o copo.
- `E` no copo insere/retira ingrediente.
- Clique no motor liga/desliga.
- Segurando o copo, clique no motor para encaixar.
- O conteudo viaja com o copo.
- O processamento pausa sem o copo encaixado.
- O visual deve girar no eixo Z.

### Highlights e portas

- O highlight atual e um contorno URP por `OutlineHighlightable` e `Outline.shader`.
- Cada objeto pode definir sua propria cor; nao precisa de manager global.
- Copo e motor do Blender devem destacar separadamente.
- Portas usam `OpenableDoor`, clique esquerdo segurado, pivot configuravel, eixo Z e limite de 90 graus.

## Arquitetura atual

Scripts centrais:

- `PlayerInput`: entrada direta pelo Input System.
- `PlayerInteractor`: raycast, highlight e regras de interacao.
- `Holder`: follow fisico, zoom, drop, throw e rotacao.
- `HoldableObject`: estados fisicos do objeto.
- `ContainerStation`: conteudo, receitas, visuais, byproducts e resultados.
- `HeatedContainerStation`: exige cooktop.
- `BlenderStation`, `FryingPanStation` e `CuscuzeiraStation`: comportamentos concretos.
- `BlenderCupContent`: copo removivel que delega conteudo para o Blender.
- `Plate`: container de uma porcao.
- `ObjectSpawner`: entrega objetos e consegue preencher um container segurado.
- `RecipeData`, `RecipeBook` e `RecipeProgress`: sistema de receitas.
- `ObjectVisualPreset`: placement visual por container.
- `PerformanceManager` e `RuntimeVisibilityCuller`: configuracao de performance.

O namespace principal e `Abigobaldo.Game`.

## Estado atual importante

O ultimo commit observado foi `7813d615`, mas existem mudancas locais posteriores. Preserve-as.

Mudancas locais que apareceram enquanto este handoff era escrito:

- o `CornVisual` incorreto foi removido de `Fried_Egg.asset`;
- a pose de Plate do `FriedEgg.prefab` foi ajustada;
- throw force do Player voltou de 100 para 8;
- fog da MainGame foi ligado;
- a instancia aninhada do BlenderCup foi ativada;
- `hiddenFirstPersonParts` do Player ficou vazio. Se a cabeca ainda deve ficar oculta em primeira pessoa, coloque `Head` de volta.

Pendencias tecnicas mais importantes:

1. `Cuscuz.asset` atualmente nao substitui CornFlakes por identidade Cuscuz.
2. `RoastedCorn.asset` atualmente continua usando Corn como resultado.
3. Empratamento direto aceita qualquer objeto com target Plate; decidir/enforcar se apenas comida Ready pode entrar.
4. O `Blender.prefab` observado estava serializado para girar no eixo Y; conferir e mudar para Z.
5. `RecipeProgress` nao limpa um model override antigo quando o estado seguinte deixa modelPrefab vazio.
6. Uma transferencia invalida entre containers pode limpar `activeRecipe` da station.
7. O core precisa de teste completo de retirar/reinserir, materiais e carbonizacao.

## Sistemas ainda faltando

- clientes funcionando;
- dialogos;
- pedidos;
- entrega;
- HUD;
- ticket;
- tela de resultados;
- fim de turno;
- audio de gameplay;
- radio funcional;
- VFX;
- lixeira/limpeza;
- objetos quebraveis.

Os prefabs de clientes e UI existentes sao principalmente estruturas vazias.

## Menu e build

O menu visual existe, mas os botoes apenas animam. Nao ha codigo carregando `MainGame`.

Ha duas cenas:

- `Assets/Scenes/menu.unity`, a mais nova;
- `Assets/menu.unity`, duplicata.

`EditorBuildSettings.asset` esta inconsistente:

- referencia MainGame com GUID antigo;
- referencia `Tutorials.unity`, que nao existe.

Antes de gerar build, refaca a lista com apenas `Assets/Scenes/menu.unity` e `Assets/Scenes/MainGame.unity`.

## URP, iluminacao e performance

- Unity `2022.3.62f3`.
- URP `14.0.12`.
- SRP Batcher ligado, MSAA 2x.
- `PerformanceManager` esta na MainGame.
- O Directional Light esta Mixed, mas MainGame ainda nao possui bake.
- Existe um sistema `LightingManager` inspirado no Roblox, com Sky, Atmosphere, Bloom, Color Correction, Depth of Field e luzes locais.
- O prefab se chama `Lightning.prefab` e esta incorretamente dentro da pasta de scripts.
- `LightingManager` e `GameplayManager` ainda nao estao instalados na MainGame.
- Nao torne tudo Baked antes de criar lightmaps.
- Otimize com o Unity Profiler; nao dependa apenas de desligar tudo fora da camera.

## Proxima ordem recomendada

1. Ler `git status` e preservar todas as mudancas locais.
2. Abrir MainGame e eliminar erros de Console.
3. Corrigir Cuscuz, RoastedCorn, eixo Z do Blender e ocultacao da Head.
4. Testar todas as receitas, materiais, pausa, retomada, prato e Charcoal.
5. Corrigir menu e Build Settings.
6. Implementar o loop minimo dos tres clientes.
7. Adicionar HUD, dialogo, ticket, entrega e resultados.
8. Adicionar audio/VFX.
9. Fazer bake e otimizar com Profiler.

## Como trabalhar com o usuario

- Fale em portugues e seja direto.
- Mostre o que foi feito, nao apenas um plano abstrato.
- Nao obrigue o usuario a preencher dezenas de campos repetidos.
- Explique campos do Inspector de forma simples quando necessario.
- Automatize referencias e dados quando for seguro.
- O usuario cuida do acabamento visual dos prefabs, mas espera que o agente conecte o sistema.
- Nao apague assets "inuteis" sem verificar referencias e sem um commit de seguranca.
- Nao mude a versao escolar do projeto.
- Sempre informe o que nao foi testado em Play Mode.
