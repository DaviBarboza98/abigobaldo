# Abigobaldo's - Review do Core, Prefabs e Plano Ate Quinta

Data da analise: 10/08/2026  
Prazo considerado: quinta-feira, 13/08/2026  
Escopo analisado: `Assets/Scenes/MainGame.unity`, `Assets/_Scripts`, `Assets/_Prefabs`, `Assets/_Data`, `Assets/Materials`, `Assets/Settings/URP`, e `C:/Users/caaab/Downloads/Abigobaldos_GDD.docx`.

## Veredito Curto

O jogo nao esta monotono porque tem poucas receitas. Ele esta monotono porque cozinhar ainda nao tem jogo dentro.

Hoje o loop principal e quase sempre:

1. Pegar item.
2. Colocar em container.
3. Esperar.
4. Tirar item.
5. Colocar em outro lugar.

Isso e funcional, mas ainda nao e divertido. Overcooked tambem tem "pegar, colocar, esperar, entregar", mas ele fica bom porque combina isso com pressao de tempo, leitura de pedidos, gargalos de espaco, risco de erro, feedback audiovisual forte, fluxo de entrega e uma cozinha que obriga o jogador a tomar decisoes pequenas o tempo todo.

O Abigobaldo tem uma identidade melhor do que o core atual deixa aparecer. A ideia de um cozinheiro feliz alimentando pessoas em situacao de rua e muito mais forte do que "simulador de container". O caminho nao e refazer tudo por desespero. O caminho e escolher um core menor, mais expressivo, e polir ele ate dar vontade de repetir.

Minha recomendacao principal:

**Pare de adicionar sistemas por dois dias. Refaça o loop de pedido -> preparo -> entrega -> felicidade com 2 receitas boas, 1 cliente visivel e feedback forte.**

## Referencias De Jogos De Culinaria

Usei estes jogos como referencia externa:

- Overcooked - Team17 descreve o jogo como uma cozinha caotica onde chefs preparam, cozinham e servem pedidos antes que clientes desistam: https://www.team17.com/games/overcooked
- D - combina cozinhar/servir com layout, compra e posicionamento de equipamentos, progressao e restaurante procedural: https://store.steampowered.com/app/1599600/PlateUp/
- Cooking Mama - transforma cozinhar em mini-acoes curtas como cortar, assar, cozinhar e servir, com muitas receitas e feedback imediato: https://play.google.com/store/apps/details?id=jp.co.ofcr.cm00&hl=en_US
- Cook, Serve, Delicious 2 - enfatiza variedade, estacoes, dificuldade flexivel e crescimento de restaurante: https://www.cookservedelicious.com/main/

O aprendizado importante:

- Overcooked nao e divertido por ter receita complexa. Ele e divertido porque cada receita cria uma cadeia de microdecisoes sob pressao.
- PlateUp nao e divertido so pelo prato. Ele e divertido porque a cozinha vira um problema de layout e rotina.
- Cooking Mama nao e divertido por esperar timer. Ele e divertido porque cada etapa tem uma acao fisica simples.
- Cook, Serve, Delicious nao e divertido por realismo. Ele e divertido porque pedido, estacao e tempo competem pela atencao do jogador.

## Comparacao Direta Com Abigobaldo

### O Que Abigobaldo Ja Tem De Bom

- A fantasia central e boa: cozinhar de graca para ajudar pessoas. Isso da um tom proprio, diferente de restaurante capitalista generico.
- A camera em primeira pessoa combina com cozinha fisica e item na mao.
- O foodtruck como palco unico e uma boa limitacao. Ajuda escopo e identidade.
- Os itens pegaveis, portas, geladeira, armarios e utensilios criam potencial de "cozinha tactil".
- A culinaria nordestina e um diferencial real. Cuscuz, milho, fuba, sucos regionais e marmitas podem dar cara propria ao projeto.

### O Que Esta Matando A Graca

1. Nao existe cliente no loop atual.
   - Sem cliente, nao existe motivo emocional para cozinhar.
   - Sem pedido, o jogador nao tem objetivo.
   - Sem entrega, nao existe fechamento.
   - Sem reacao, nao existe recompensa.

2. Cozinhar nao tem acao.
   - Colocar ovo na frigideira nao pede decisao.
   - Esperar 10 segundos nao e gameplay se nada disputa sua atencao.
   - O liquidificador ficou tecnicamente trabalhoso, mas no fim ainda e "colocar milho, apertar botao".

3. As receitas nao diferem o suficiente.
   - Ovo frito e milho assado sao quase a mesma acao.
   - Cuscuz tambem vira "colocar item e esperar".
   - Fuba e o unico que muda um pouco porque usa liquidificador, mas ainda falta feedback forte.

4. Falta feedback sensorial.
   - A cena analisada nao tem `AudioSource`.
   - O YAML da cena nao contou `Canvas` ativo como componente direto, apesar de existir `UICanvas` na hierarquia.
   - Particulas existem em alguns prefabs, mas nao ainda como linguagem clara para o jogador.
   - Sem sons de pegar, colocar, fritar, vapor, pedido, acerto e erro, o jogo parece morto.

5. O tamanho dos objetos esta inconsistente.
   - Muitos prefabs usam escala 10.
   - Alguns usam 0.1 dentro do mesmo prefab.
   - Isso explica sua sensacao de "toda vez que coloco um item ele fica maior ou menor".
   - Esse e o maior problema de prefab hoje.

6. A arquitetura esta ficando cansativa.
   - Ja existem 43 scripts e cerca de 6166 linhas de C# para um MVP ainda sem cliente/entrega.
   - `FryingPan` e `Cuscuzeira` duplicam muita logica.
   - O historico de mudancas mostra que o conceito de Item/Objeto/Data/Container ficou oscilando.
   - O codigo funciona em partes, mas esta caro de manter.

7. Performance esta suspeita por configuracao e asset, nao so por codigo.
   - A cena tem 194 `GameObject` e 256 `PrefabInstance`.
   - Isso nao deveria dar 6-10 FPS sozinho.
   - Prefabs pegaveis usam muitos `MeshCollider`.
   - URP esta com HDR ligado, MSAA 2x, sombra da main light, additional lights habilitadas e shadow distance 50.
   - A cena tem lighting baked/realtime habilitado no YAML.
   - Varios modelos FBX parecem estar entrando com escala 10 e colliders de mesh.

## Auditoria De Prefabs

### Numeros

- Scripts: 43
- Prefabs: 23
- ScriptableObjects/assets analisados: 32
- Linhas de script aproximadas: 6166
- Prefabs de objeto: 14
- Prefabs de container: 3
- Receitas: 4 + database
- ObjectData: 19

### Prefabs Com Escala Suspeita

Quase todos os objetos principais aparecem com escala 10 em algum transform:

- `Assets/_Prefabs/Objects/Egg.prefab`
- `Assets/_Prefabs/Objects/Corn.prefab`
- `Assets/_Prefabs/Objects/Bottle.prefab`
- `Assets/_Prefabs/Objects/Bowl.prefab`
- `Assets/_Prefabs/Objects/FriedEgg.prefab`
- `Assets/_Prefabs/Objects/CornFlour.prefab`
- `Assets/_Prefabs/Objects/Cuscuz.prefab`
- `Assets/_Prefabs/Objects/Salt.prefab`
- `Assets/_Prefabs/Objects/Pepper.prefab`
- `Assets/_Prefabs/Objects/BlenderCup.prefab`
- `Assets/_Prefabs/Containers/FryingPan.prefab`
- `Assets/_Prefabs/Containers/Cuscuzeira.prefab`
- `Assets/_Prefabs/Containers/Blender.prefab`
- `Assets/_Prefabs/Player.prefab`

Isso e ruim porque:

- Collider fica mais dificil de prever.
- Visual em prato/frigideira/liquidificador fica inconsistente.
- Prefab aninhado com escala 10 e filho 0.1 vira bomba de manutencao.
- Qualquer script que instancie como filho pode herdar escala de forma inesperada.

### Regra Recomendada Para Todos Os Prefabs

Padrao novo:

- Root do prefab: escala `(1, 1, 1)`.
- Filho `Model`: escala ajustada se necessario, mas idealmente tambem `(1, 1, 1)` depois de corrigir import scale do FBX.
- Filho `Colliders`: colliders simples, sem mesh collider quando der.
- Filho `VisualRoot` ou `PlateRoot`: apenas pontos vazios.
- `HoldableObject` e `Rigidbody` ficam no root.
- `ObjectData.Prefab` aponta para o root pegavel.

Exemplo ideal:

```text
Corn.prefab
  Corn [HoldableObject, Rigidbody, Collider]
    Model [MeshRenderer]
    Particles [opcional]
```

Para containers pegaveis:

```text
FryingPan.prefab
  FryingPan [HoldableObject, Rigidbody, FryingPan, Collider]
    Model
    ObjectSurface
    SteamParticles
```

Para prato:

```text
Plate.prefab
  Plate [HoldableObject, Rigidbody, PlateContainer, Collider]
    Model
    ContentVisualRoot
```

### MeshColliders

Muitos objetos pegaveis usam `MeshCollider`. Isso e uma fonte provavel de:

- custo fisico alto;
- bugs de colisao;
- problemas com rigidbody dinamico;
- FPS ruim no Editor;
- dificuldade para segurar item sem atravessar parede.

Recomendacao ate quinta:

- Trocar objetos pequenos para `BoxCollider`, `SphereCollider` ou `CapsuleCollider`.
- Manter `MeshCollider` apenas em objetos estaticos grandes, sem Rigidbody dinamico.
- Para milho, ovo, garrafa, fuba, prato, sal e pimenta: colliders simples.

## Auditoria Da Cena Principal

### O Que A Cena Mostra

Objetos principais na cena:

- `Managers`
- `Main Camera`
- `Workspace`
- `Foodtruck`
- `Fridge`
- `Sink`
- `Cooktop`
- `Containers`
- `Props`
- `Utensilios`
- `UICanvas`
- `Directional Light`
- `GameInteraction`

Isso e uma boa base. A cozinha ja esta separada em areas.

### Problemas Visiveis Pela Estrutura

- Nao ha `AudioSource` contado na cena ou prefabs principais.
- Existem particulas em `FryingPan` e `Cuscuzeira`, mas nao ha sistema global de audio/feedback.
- Existe `UICanvas` como nome, mas a contagem YAML nao acusou `Canvas:` diretamente. Pode ser prefab/reference ou objeto incompleto. Vale verificar no Inspector.
- Nao ha objeto obvio de cliente/pedido/entrega no estado analisado.
- O foodtruck/cenario esta mais montado que o jogo em si. Isso e comum, mas perigoso no prazo.

## Auditoria Dos Scripts

### Bom

- `RecipeData` e `RecipeDatabase` existem. Isso e certo.
- `ObjectData` existe. Apesar de voce odiar configurar data, ele resolve identidade e prefab.
- `Holder` evita alocacoes grandes e usa buffers para overlap.
- `Highlightable` usa `MaterialPropertyBlock`, boa escolha para nao duplicar material a cada highlight.
- Separar `FryingPan`, `Cuscuzeira` e `BlenderCup` melhorou legibilidade em relacao a um container generico gigante.

### Ruim

- `FryingPan` e `Cuscuzeira` ainda sao quase irmas duplicadas.
- `RuntimeObjectState` virou necessario porque o sistema estava perdendo estado. Isso mostra que o dominio real nao e "lista de ObjectData", e sim "porcao/coisa cozinhavel com estado".
- O prato guarda informacao, mas ainda nao parece parte de um pedido real.
- O sistema de receita hoje transforma objetos, mas nao cria micro-acoes.
- Existem managers, UI scripts e sistemas planejados, mas o loop cliente-entrega ainda nao existe.

### Diagnostico De Arquitetura

O codigo esta preso entre duas ideias:

1. Simulacao fisica de objetos reais.
2. Sistema abstrato de receitas por dados.

As duas podem coexistir, mas hoje elas brigam. O item fisico some quando entra no container, volta como prefab, muda escala, muda material, perde estado, volta a cozinhar. Isso cria bugs constantes.

Sugestao de arquitetura para salvar o MVP:

- `HoldableObject`: objeto fisico pegavel.
- `FoodPortion`: estado runtime da comida: data, cook state, material, qualidade, contaminado, temperatura.
- `CookingStation`: recebe `FoodPortion`, processa tempo, devolve `FoodPortion`.
- Visual sempre e consequencia da porcao, nunca regra principal.
- Prefab sempre escala 1 no root.

Nao precisa refazer tudo agora, mas esse deveria ser o norte.

## Por Que Overcooked Funciona E O Seu Ainda Nao

Overcooked tem uma formula simples:

```text
pedido claro + caminho fisico + preparos curtos + timer + risco + entrega + pontuacao
```

O Abigobaldo hoje tem:

```text
item + container + timer + item
```

Falta:

- pedido;
- cliente;
- consequencia;
- erro legivel;
- som;
- UI;
- satisfacao;
- movimento no espaco;
- pequenas decisoes durante o cozimento.

O problema nao e "adicionar 20 receitas". Se voce adicionar 20 receitas no estado atual, tera 20 variacoes de monotonia.

## Como Deixar O Core Legal Ate Quinta

### Meta Realista

Uma fatia vertical curta:

```text
Cliente aparece -> pede algo -> jogador prepara -> emprata -> entrega -> cliente reage -> tela mostra felicidade/resultado.
```

Receitas suficientes:

- Ovo frito.
- Cuscuz.
- Milho assado ou fuba como extra se sobrar tempo.

### Mudanca De Design Mais Importante

Cada receita precisa ter pelo menos uma "acao gostosa", nao so um container.

Proposta:

#### Ovo frito

Loop atual:

```text
ovo -> frigideira -> espera -> prato
```

Loop melhor para MVP:

```text
ovo -> clicar na frigideira quebra ovo -> cascas aparecem na mao/chao -> ovo chia -> virar no tempo certo com E -> tirar -> prato
```

Mesmo que "virar" seja so apertar E uma vez quando uma barrinha chega no verde, ja existe jogo.

Feedback necessario:

- som de quebrar ovo;
- som de fritura;
- material mudando;
- particula leve;
- UI pequena de ponto;
- cliente feliz se ideal, menos feliz se queimado.

#### Cuscuz

Loop atual:

```text
fuba -> cuscuzeira -> espera -> cuscuz
```

Loop melhor para MVP:

```text
fuba -> cumbuca/prato de preparo -> adicionar agua simplificada por spawner/garrafa -> cuscuzeira -> vapor -> tirar no tempo
```

Se agua for dificil, nao simule liquido. Use "Bottle" como ingrediente abstrato `WaterBottle`. A garrafa some, toca som de despejar, e pronto.

Feedback:

- vapor desde o comeco;
- som de vapor;
- tampa tremendo simples;
- estado "pronto" com puff de vapor e som.

#### Fuba

Loop atual:

```text
milho -> liquidificador -> ligar -> fuba
```

Loop bom para MVP:

```text
milho -> copo -> ligar -> milho gira -> som de liquidificador -> fuba aparece
```

Este ja esta perto. O ganho agora e audiovisual.

## Plano De 3 Noites

### Noite 1 - Core De Pedido E Entrega

Objetivo: o jogo finalmente ter motivo.

Implementar:

- `CustomerRequest` simples com nome, pedido e paciencia/humor.
- Um cliente estatico na janela.
- UI simples de pedido: icone/nome do prato.
- `DeliveryZone`: interagir segurando prato valida conteudo.
- Resultado: correto, errado, cru, queimado, atrasado.
- Reacao textual do cliente no console ou UI.

Nao implementar:

- fila;
- varios clientes;
- dialogo complexo;
- save;
- dia/noite;
- NavMesh.

### Noite 2 - Feedback E Receita Divertida

Objetivo: cozinhar parecer uma acao, nao planilha.

Implementar:

- Sons placeholder:
  - pickup;
  - drop;
  - colocar item;
  - fritar;
  - vapor;
  - liquidificador;
  - entrega correta;
  - entrega ruim.
- UI de cozimento simples no mundo ou tela.
- Frigideira com "virar/confirmar ponto" opcional.
- Particulas da frigideira e cuscuzeira funcionando desde o inicio.
- Tela de resultado simples com felicidade.

Nao implementar:

- receitas novas;
- cliente andando;
- agua realista;
- objetos quebraveis.

### Noite 3 - Prefabs, Performance E Apresentacao

Objetivo: parar de parecer prototipo quebrado.

Implementar:

- Corrigir escala dos prefabs principais.
- Trocar MeshColliders de itens pequenos por colliders simples.
- Reduzir URP:
  - HDR off se nao for usado.
  - MSAA off ou 2x apenas se necessario.
  - Shadow distance menor, tipo 15-20.
  - Additional lights off se nao tiver luz extra util.
  - Depth texture off se nenhum shader/UI precisa.
- Um post visual minimo:
  - luz quente dentro do foodtruck;
  - materiais flat/coerentes;
  - skybox simples;
  - cor de highlight consistente.
- Build de teste fora do Editor.

Nao implementar:

- remodelar tudo;
- campanha;
- muitos pratos;
- sistema de qualidade completo.

## O Que Eu Cortaria Sem Pena

Ate quinta, eu cortaria:

- Liquidificador pegavel complexo.
- Agua fisica/visual.
- F5 camera externa.
- Portas de todos os moveis como gameplay necessaria.
- Sistema de cortar na tabua.
- Quebra de itens.
- Contaminacao de chao.
- Cliente andando.
- Varios dias.
- Muitas receitas.
- Trofeus.
- Save.

Nao porque essas ideias sao ruins. Porque elas nao salvam o core ate quinta.

## O Que Eu Manteria

- Foodtruck.
- Primeira pessoa.
- Pegar/soltar/arremessar.
- Geladeira abrivel como charme.
- Frigideira.
- Cuscuzeira.
- Liquidificador simples.
- Prato.
- 1 cliente.
- 2 pedidos.
- Felicidade final.

Isso preserva a alma: Abigobaldo cozinha para cuidar das pessoas.

## Refactor Recomendado Para Depois Do MVP

Depois da entrega de quinta, eu faria:

### 1. Padronizar Prefabs

Criar checklist unico:

- Root escala 1.
- `HoldableObject` no root.
- Rigidbody no root.
- Collider simples no root/filho `Colliders`.
- Mesh visual em `Model`.
- Root visual de comida em `ContentRoot`.
- Sem prefab aninhado com escala 10/0.1 misturado.

### 2. Criar FoodPortion

Substituir listas de `ObjectData` por algo assim:

```csharp
FoodPortion
{
    ObjectData Data;
    ObjectCookState CookState;
    Material RuntimeMaterial;
    float Quality;
    bool Contaminated;
    float Temperature;
}
```

Isso vira a unidade real do jogo.

### 3. Criar CookingStation Base

Nao necessariamente um `ItemContainer` generico enorme, mas uma base pequena:

- armazenamento de porcao;
- iniciar receita;
- atualizar tempo;
- trocar estado;
- gerar visual;
- entregar para holder/prato.

`FryingPan`, `Cuscuzeira` e `Blender` especializam so o que muda.

### 4. Separar Gameplay De Visual

Cada station deveria ter:

- script de regra;
- script visual;
- script audio.

Exemplo:

```text
FryingPan
  FryingPanStation.cs
  CookingVisuals.cs
  CookingAudio.cs
```

Assim voce para de quebrar regra quando mexe em fumaca/modelo.

## Performance: Hipoteses Mais Provaveis

Eu nao rodei Unity Profiler daqui, entao isso e analise estatica.

Mais provavel:

1. Editor + URP configurado pesado.
2. MeshColliders demais em objetos dinamicos.
3. Modelos FBX com escala/import ruim.
4. Luz/sombra/lighting realtime desnecessario.
5. Muitos materiais sem batching bom.
6. OneDrive atrapalhando import/cache durante Play Mode.

Menos provavel:

- Scripts de container sozinhos causarem 6-10 FPS. Eles nao parecem pesados o suficiente.
- Highlight sozinho matar FPS. Ele usa `MaterialPropertyBlock`, que e bom.
- Quantidade de objetos da cena, isoladamente. 194 GameObjects nao e absurdo.

Checklist de performance para testar:

- Rodar build standalone, nao so Play no Editor.
- Abrir Profiler e ver se o gargalo e CPU, GPU, Physics ou Rendering.
- Desligar sombras temporariamente.
- Desligar HDR, Depth Texture e Additional Lights.
- Trocar MeshColliders pequenos por colliders simples.
- Testar cena vazia com player + foodtruck + 5 objetos.
- Tirar projeto do OneDrive temporariamente se Play Mode estiver sofrendo com sync.

## Review Do GDD

O GDD ja sabia a resposta: "primeiro fechar o ciclo jogavel; depois ampliar conteudo, fisica, narrativa e polimento."

O projeto desviou porque muita energia foi para:

- containers pegaveis;
- liquidificador complexo;
- colisao de item segurado;
- portas;
- prefab setup;
- URP/highlight/particulas;
- estado queimado/carbonizado.

Essas coisas sao legais, mas vieram antes de cliente e entrega. Por isso parece que voce trabalhou muito e o jogo ainda nao parece jogo. Voce trabalhou muito mesmo. So que trabalhou em sistemas que sustentam o core, nao no core emocional/jogavel.

O GDD tambem diz que o foodtruck e o palco inteiro e que clientes devem parecer pessoas. Essa e a prioridade agora.

## Decisao De Design Recomendada

Para quinta, Abigobaldo deveria ser:

> Um jogo curto em primeira pessoa onde voce prepara comida nordestina simples para uma pessoa com fome, tentando entregar no ponto e ver ela ficar feliz.

Nao deveria ser:

> Um simulador fisico completo de cozinha com agua, portas, liquidificador modular, todos os utensilios pegaveis e varias receitas.

## MVP Replanejado

### Loop

```text
1. Cliente aparece na janela.
2. UI mostra pedido: "Quero cuscuz" ou "Quero ovo frito".
3. Jogador pega ingredientes.
4. Jogador prepara em 1 ou 2 estacoes.
5. Jogador emprata.
6. Jogador entrega.
7. Cliente reage.
8. Tela mostra felicidade, tempo e capricho.
```

### Receitas

#### Ovo frito

- Ingrediente: ovo.
- Estacao: frigideira.
- Feedback: casca, som, fritura, vapor leve.
- Estados: cru, pronto, passado, queimado.
- Entrega ideal: pronto.

#### Cuscuz

- Ingrediente: fuba ou flocao.
- Agua: abstraida por garrafa, sem liquido visual.
- Estacao: cuscuzeira.
- Feedback: vapor forte, som, tampa tremendo.
- Estados: cru, pronto, passado, queimado.

#### Opcional: Fuba

- Ingrediente: milho.
- Estacao: liquidificador.
- Feedback: som e item girando.
- Resultado: fuba.

## O Que Fazer Com Os Modelos

Voce disse que esta disposto a remodelar tudo. Eu nao recomendo remodelar tudo antes de quinta.

Remodele so:

1. Ovo frito, se o visual atual nao comunica prato.
2. Cuscuz, se o visual atual nao comunica comida pronta.
3. Cliente simples ou silhueta/personagem parado.
4. Prato/marmita se a entrega precisar ficar clara.

Nao remodele:

- foodtruck inteiro;
- todos os armarios;
- todos os utensilios;
- saleiro/pimenteiro;
- objetos decorativos.

Polish visual minimo vale mais:

- cores coerentes;
- luz quente;
- materiais sem brilho estranho;
- escala consistente;
- particulas certas;
- sons.

## Minha Ordem De Prioridade

Se voce me chamasse para implementar a proxima leva, eu faria nesta ordem:

1. `CustomerRequest`, `DeliveryZone`, UI simples de pedido e resultado.
2. AudioManager simples e sons placeholder.
3. Corrigir prefabs de escala 1 para ovo, milho, fuba, cuscuz, prato, frigideira, cuscuzeira, copo.
4. Trocar MeshColliders pequenos por colliders simples.
5. Ajustar URP para performance.
6. Melhorar feedback de frigideira/cuscuzeira.
7. So depois pensar em mais receitas.

## Conclusao

Voce nao precisa jogar tudo fora. Mas precisa parar de medir progresso por "quantos sistemas existem" e medir por "quantas vezes da vontade de servir mais um cliente".

Hoje, a base tecnica esta maior que o jogo. Ate quinta, o jogo precisa ficar maior que a base tecnica.

O caminho mais forte e:

- manter 2 receitas;
- fazer cliente e entrega;
- deixar cozinhar ter som, tempo, erro e reacao;
- limpar os prefabs mais usados;
- cortar todo realismo que nao aparece na felicidade final.

Esse e o Abigobaldo que vale terminar: nao o simulador perfeito, mas o cara feliz que faz uma comida simples e muda a noite de alguem.
