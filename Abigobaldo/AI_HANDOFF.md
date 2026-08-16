# Handoff do projeto Abigobaldo

Atualizado em 2026-08-16. Leia este arquivo e depois confira o projeto Unity. Cena, prefabs e codigo vencem este texto caso algo tenha mudado.

## Leia primeiro

- Projeto Unity: `C:\Users\caaab\OneDrive\Documentos\Projects\abigobaldo\Abigobaldo`.
- Unity `2022.3.62f3`, URP `14.0.12`, namespace principal `Abigobaldo.Game`.
- HEAD atual: `d20affbe` (`main`). Existem mudancas locais posteriores; nao descarte nem reverta.
- Nao altere `global.json`: ele usa o SDK escolar `6.0.428`.
- Fale com o usuario em portugues. Codigo, variaveis, nomes tecnicos e assets devem ficar em ingles.
- `Cuscuz` e `Cuscuzeira` sao as grafias corretas e devem permanecer assim.
- O usuario prefere uma solucao simples, funcional e orientada por dados a arquiteturas grandes ou campos repetidos no Inspector.
- O usuario monta e ajusta os prefabs visuais; o agente deve conectar scripts, referencias e dados sempre que for seguro.
- Nao atualize este Handoff durante o trabalho normal. Atualize somente quando o usuario pedir.

## O jogo

Abigobaldo e um jogo de culinaria cartoon/low poly em primeira pessoa dentro de um food truck brasileiro. Abigobaldo cozinha comida regional para pessoas em situacao de vulnerabilidade. O tratamento deve ser humano, com conversa, acolhimento e felicidade, alinhado a ODS 2, Fome Zero, e saude/bem-estar.

Loop desejado:

1. receber um cliente;
2. conversar ou descobrir seu pedido;
3. pegar ingredientes;
4. cozinhar;
5. empratar;
6. entregar;
7. ver a reacao;
8. encerrar a noite com resultados focados na felicidade das pessoas.

O primeiro slice tem tres clientes: dois fazem pedidos e um demonstra dialogo. O foco de receitas e `Cuscuz`, `FriedEgg` e `Omelet`. `RoastedCorn` ainda existe no sistema como receita extra.

O core de pegar, cozinhar, queimar e empratar existe. NPCs, dialogos, pedidos, entrega e UI ainda sao a proxima grande etapa. O jogo precisa rodar em WebGL no itch.io.

## Nao reintroduzir

Estas ideias foram abandonadas ou adiadas:

- zoom estilo Cooking Mama;
- `RecipeContainer`/`ItemContainer` antigos;
- `OutputSpawnPoint` para comida pronta;
- fantasma de reposicionamento e `HomeSlot` generico;
- `ContainerKind`/`StationKind` redundante;
- receitas com campos universais hardcoded, como `HandMixing`;
- BlenderCup com campos especificos para milho/flocao;
- simulacao fisica de varios graos no liquidificador;
- particulas controladas por um sistema geral de codigo.

## Controles atuais

| Controle | Acao |
|---|---|
| WASD | Andar |
| Shift | Correr |
| Clique esquerdo | Pegar objeto, usar acao fisica/especifica e segurar porta |
| E | Interagir, inserir, retirar, transferir ou encaixar |
| G curto | Soltar |
| G segurado | Arremessar para onde a camera olha |
| R + mouse | Rotacionar objeto na mao |
| Scroll | Aproximar ou afastar objeto segurado |
| V | Liberar/travar cursor |
| P | Pausar |

Regra mental: clique esquerdo e acao fisica (`IPickupInteractable`/`IHoldInteractable`); `E` e interacao (`IInteractable`) e conteudo.

- Spawner com clique esquerdo entrega o objeto para a mao.
- Spawner com `E` serve apenas para combinacoes com o objeto/container ja segurado.
- Cooktop usa `E` para encaixar frigideira/cuscuzeira segurada.
- Base do Blender usa clique para ligar/desligar e `E` para encaixar o copo segurado.
- Copo do Blender usa `E` para inserir/retirar conteudo.
- O objeto e pego na rotacao mundial exata em que se encontra naquele momento.

## Core que deve ser preservado

### Objetos e containers

- Objetos pegaveis usam `HoldableObject`, `Rigidbody` e collider. `Item` foi substituido por `Object`; o antigo `ItemHolder` agora e `Holder`.
- O arremesso usa o `forward` da camera, nunca o eixo local do Holder.
- Enquanto segurados, objetos continuam colidindo com o mundo.
- Dentro de um container, o objeto real fica parentado ao `Content Anchor`, kinematic e invisivel. Um clone puramente visual e exibido.
- Containers pegaveis preservam o objeto real dentro deles.
- Frigideira e cuscuzeira nao devem ser arremessaveis.
- `Plate` e um container de capacidade 1. Containers nunca podem ser empratados.
- `E` entre containers transfere o ultimo conteudo. Inserir em container pegavel nao troca automaticamente a mao pelo container.

### Visuais

- Cada prefab real usa um `ObjectVisualPreset`.
- O preset lista placements locais para `Plate`, `FryingPan`, `Blender` e `Cuscuzeira`, relativos ao `Content Anchor`.
- Um objeto so entra se possuir o target daquele container no preset.
- Cuscuzeira guarda a informacao, mas nao mostra o conteudo.
- A pasta de visual prefabs e em grande parte legado; o prefab real e a fonte do visual atual.

### Receitas

- `RecipeData` define ingredientes, station, tempos, aparencias, prefabs de transformacao e byproducts.
- Todas as stations usam `Assets/_Scripts/Data/RecipeBooks/RecipeBook.asset`.
- `ObjectDefinition` e apenas identidade estavel para receitas/pedidos.
- `RecipeProgress` fica no alimento; retirar e recolocar nao pode reiniciar o tempo.
- Receitas comuns devem ser adicionadas por dados. Mecanicas especiais ficam em componentes especificos.
- Estados aquecidos: `Raw` 0 s, `AlmostReady` 5 s, `Ready` 10 s, `Overdone` 15 s, `Burned` 20 s e `Carbonized` 25 s.
- Todo alimento `Carbonized` obrigatoriamente vira o prefab global `Charcoal`, substituindo objeto, identidade e visual.
- Egg + FryingPan cria FriedEgg cru e duas cascas imediatamente.
- Corn + Blender por 5 s cria CornFlakes e desliga o motor.
- CornFlakes + Cuscuzeira cria Cuscuz.
- FriedEgg em `AlmostReady` + cinco segundos de movimento ativo com `R` cria Omelet cru.
- Omelet cozinha na frigideira. RoastedCorn tambem existe atualmente.

### Stations

- `FryingPanStation` e `CuscuzeiraStation` herdam de `HeatedContainerStation`.
- So cozinham encaixadas em `CooktopSlot`; pegar pausa e reencaixar retoma.
- O Cooktop deve continuar aceitando futuras subclasses de `HeatedContainerStation`.
- Blender e base fixa; BlenderCup e prefab separado, pegavel, removivel e com conteudo persistente.
- Motor e copo possuem collider/highlight independentes.
- Somente o copo e alvo direto de conteudo. A base nunca deve retirar ingrediente por engano.

### Portas e highlight

- Highlight e contorno URP via `OutlineHighlightable` e `Outline.shader`, com cor opcional por objeto.
- Portas usam `OpenableDoor`, pivot configuravel, eixo configuravel e limite normalmente de 90 graus.
- O corpo do player empurra a porta por colisao (`IBodyPushable`).
- O movimento do mouse agora considera de qual lado da porta o player esta, para o gesto continuar natural nos dois lados.

## Mudancas locais sem commit

Preserve os arquivos mostrados por `git status`. As mudancas atuais incluem:

- porta com direcao de arraste dependente do lado do player;
- pickup mantendo a rotacao mundial atual, sem salvar uma pose antiga;
- separacao mais clara entre clique esquerdo e `E`;
- `IsDirectInteractionTarget` para impedir que a base do Blender se comporte como copo;
- Cooktop movido para interacao com `E`;
- spawner: clique pega; `E` so combina com a mao/container.

Arquivos modificados: `BlenderStation.cs`, `ContainerStation.cs`, `CooktopSlot.cs`, `OpenableDoor.cs`, `BlenderCupContent.cs`, `HoldableObject.cs`, `Holder.cs`, `IObjectContainer.cs`, `ObjectSpawner.cs`, `Plate.cs` e `PlayerInteractor.cs`.

Estas alteracoes ainda precisam de compilacao e teste completo em Play Mode. Tambem existem `.codex/`, `blender_tools/` e `render_output/` nao rastreados; nao apague automaticamente.

## Arte nova no Blender

O usuario remodelou o foodtruck, toda a cozinha e o Abigobaldo. Tambem criou campainha, placa do balcao, espelho, livros, lapis, duas pilhas de pratos separados, bandejas com ovos, garrafas, pote de sorvete, bilhete de geladeira e modelos de agua para copo, torneira e pia.

Esses modelos ainda nao substituem os modelos/prefabs da Unity. O arquivo pronto para exportar e:

`C:\Users\caaab\Downloads\Abigobaldo_Blender_Work\abigobaldoGame_FBX_READY.blend`

- Original intacto: `C:\Users\caaab\Downloads\abigobaldoGame.blend`.
- Backup antes das correcoes: `abigobaldoGame_FBX_READY_PRE_SOLIDIFY_BACKUP.blend`.
- Auditoria: `C:\Users\caaab\Downloads\Abigobaldo_Blender_Work\SOLIDIFY_AUDIT.md`.
- Colecoes: `00_REFERENCE`, `10_ENVIRONMENT`, `20_STATIONS`, `30_OBJECTS`, `40_CHARACTERS`, `50_EFFECTS`.
- 123 objetos foram organizados; malhas repetidas seguras foram compartilhadas, reduzindo 122 meshes para 73 sem alterar transforms/pivots.
- Solidify minimo ja foi aplicado e convertido em geometria apenas na estante, armario do cooktop, armario da pia, geladeira, corpo/anel da cuscuzeira e placa do balcao.
- O corpo complexo do foodtruck recebeu faces internas invertidas porque Solidify deformava a malha.
- Nao aplique Solidify global novamente.
- Exporte para Unity com `Forward: -Z Forward` e `Up: Y Up`, preferencialmente por Collection/objetos selecionados.
- O exportador Blender 5.2 pode avisar sobre materiais de instancias compartilhadas de rodas, garrafas e ovos. O FBX foi reimportado e os materiais ficaram corretos.

Ao integrar a arte nova, preserve a logica dos prefabs atuais e troque principalmente os filhos visuais, colliders e anchors. Nao refaca o core apenas por causa dos modelos.

## Estado da Unity

- Cenas principais: `Assets/Scenes/menu.unity` e `Assets/Scenes/MainGame.unity`.
- Build Settings ja esta correto com menu seguido de MainGame.
- Ainda existem `Assets/menu.unity` e uma cena `Kbum...` de alteracoes fora do build; nao delete sem conferir referencias.
- O menu visual existe, mas nao foi encontrado codigo para os botoes carregarem MainGame.
- `PerformanceManager` esta na MainGame e possui perfil WebGL: render scale 0.9, MSAA 1x, sombras menores e culling por frustum/distancia.
- `Lightning`/`LightingManager` esta presente na MainGame sob Managers. A iluminacao ainda precisa de ajuste visual e bake real.
- `GameplayManager` existe, mas nao esta instalado na MainGame.
- Crosshair esta preparada em `PlayerCursor`, segue o mouse destravado e fica centralizada travada. `crosshairSprite` ainda esta vazio; o usuario vai fornecer um PNG.
- `Player.prefab` esta com `hiddenFirstPersonParts` vazio e `modelRoot` vazio. O novo Abigobaldo ainda precisa ser integrado e a cabeca deve ser ocultada em primeira pessoa.
- O Blender prefab ainda esta serializado com `spinAxis: Y`; o desejado e Z.

## Problemas conhecidos

1. `Cuscuz.asset` esta sem `resultPrefab`, entao pode manter identidade CornFlakes.
2. `RoastedCorn.asset` usa Corn como resultado; deve apontar para RoastedCorn.
3. `RecipeProgress` nao limpa um model override antigo quando o estado seguinte nao possui `modelPrefab`.
4. Transferencia invalida entre containers pode limpar `activeRecipe` da origem antes de restaurar o objeto.
5. Empratamento direto ainda pode aceitar comida nao pronta; de station para Plate existe verificacao de Ready.
6. Eixo do Blender precisa ser Z no prefab.
7. Head e crosshair precisam de referencias no Player novo.
8. O core precisa de teste completo de retirar/reinserir, materiais, pausa, retomada, Plate e Charcoal.

## Sistemas faltando

- NPCs/clientes funcionais;
- dialogos, pedidos e convencimento;
- entrega e reacao;
- HUD, ticket e tela de resultados;
- fim de turno;
- agua da pia enchendo o BlenderCup;
- pilhas reais substituindo o PlateSpawner;
- radio, campainha e audio de gameplay;
- VFX, lixeira/limpeza e objetos quebraveis.

## Proxima ordem recomendada

1. Ler `git status` e preservar as mudancas locais.
2. Abrir MainGame, compilar e eliminar erros de Console.
3. Testar e corrigir a nova distribuicao clique/E, portas nos dois lados e pickup na rotacao atual.
4. Corrigir Cuscuz, RoastedCorn, eixo Z, transferencia invalida e model override.
5. Exportar/importar a nova arte e substituir somente os visuais dos prefabs existentes.
6. Configurar Head e PNG da crosshair.
7. Testar todas as receitas ate Plate/Charcoal.
8. Implementar o loop minimo dos tres clientes, dialogos, pedidos, entrega e resultados.
9. Fazer audio/VFX, bake e medir WebGL com Unity Profiler.

Sempre informe o que nao foi testado em Play Mode. Nao apague assets supostamente inuteis sem conferir referencias e criar um ponto de seguranca.
