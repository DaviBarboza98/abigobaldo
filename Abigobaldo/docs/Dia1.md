# DIA 1 - PRODUCAO VISUAL, MODELOS E IDENTIDADE

## Objetivo Do Dia

Transformar o projeto em algo que ja pareca "Abigobaldo's" antes mesmo de estar totalmente jogavel.

O Dia 1 nao e para programar sistema complexo. E para criar os modelos, materiais, decoracoes e elementos visuais que a demo precisa para nao parecer vazia. No fim do dia, ao abrir a cena, o foodtruck deve parecer um lugar com dono, historia e personalidade.

Frase de fechamento do dia:

> "Agora isso parece o foodtruck do Abigobaldo."

## Prioridade Real

Se tudo der errado e faltar tempo, priorize nesta ordem:

1. Foodtruck com personalidade.
2. Comidas legiveis.
3. Clientes simples, mas diferentes.
4. Utensilios essenciais.
5. Decoracoes extras.

O jogador vai perdoar um cliente simples. Ele nao vai perdoar nao entender o que e ovo, cuscuz, prato, pedido ou onde entregar.

---

## Lista Mestre De Modelos 3D Do Dia 1

Esta e a lista de producao visual do Dia 1. Ela existe para voce nao ficar decidindo asset por asset no meio do cansaco.

Legenda:

- **Obrigatorio**: precisa existir na demo.
- **Muito recomendado**: nao quebra gameplay se faltar, mas da muita personalidade.
- **Opcional**: so faz se sobrar tempo.

### 1. Foodtruck E Estrutura

#### Obrigatorio

- `Foodtruck_Body`: corpo externo principal do foodtruck.
- `Foodtruck_Roof`: teto/cobertura, se estiver separado.
- `Foodtruck_ServiceWindow`: janela de atendimento.
- `Foodtruck_FrontWindow`: vidro/parabrisa frontal.
- `Foodtruck_Door`: porta lateral/cabine, se aparecer.
- `Foodtruck_Wheels`: rodas.
- `Foodtruck_Bumper`: para-choque.
- `Foodtruck_InteriorWalls`: paredes internas.
- `Foodtruck_Floor`: piso interno.
- `Foodtruck_Counter`: bancada de preparo.
- `Foodtruck_ServiceCounter`: balcao da janela.

#### Muito Recomendado

- `Foodtruck_Sign`: placa com nome `Abigobaldo's`.
- `Foodtruck_Awning`: toldo ou borda superior da janela.
- `Foodtruck_NamePlate`: plaquinha frontal/lateral.
- `Foodtruck_InteriorTrim`: acabamentos internos simples.

#### Opcional

- `Foodtruck_Stickers`: adesivos/frases.
- `Foodtruck_DirtMarks`: marcas de uso/sujeira leve.
- `Foodtruck_LicensePlate`: placa do veiculo.

### 2. Cozinha Fixa

#### Obrigatorio

- `Kitchen_Cooktop`: cooktop/fogao onde fica a frigideira/cuscuzeira.
- `Kitchen_Sink`: pia.
- `Kitchen_Faucet`: torneira.
- `Kitchen_Fridge`: geladeira.
- `Kitchen_Shelf`: prateleira.
- `Kitchen_Cabinet`: armario/balcao.

#### Muito Recomendado

- `Kitchen_CabinetDoors`: portas dos armarios, se forem visiveis.
- `Kitchen_FridgeDoor`: porta da geladeira, se abre.
- `Kitchen_SmallShelf`: prateleira pequena para temperos/decoracao.

#### Opcional

- `Kitchen_Drawer`: gaveta.
- `Kitchen_Hooks`: ganchos para utensilios.
- `Kitchen_WallRail`: trilho/barra na parede.

### 3. Estacoes E Utensilios De Gameplay

#### Obrigatorio

- `Station_FryingPan`: frigideira.
- `Station_CuscuzeiraBody`: corpo da cuscuzeira.
- `Station_CuscuzeiraLid`: tampa da cuscuzeira.
- `Item_Plate`: prato.
- `Item_Egg`: ovo inteiro.
- `Food_BrokenEgg`: gema/clara na frigideira.
- `Food_Eggshell_A`: casca de ovo parte 1.
- `Food_Eggshell_B`: casca de ovo parte 2.
- `Food_FriedEgg`: ovo frito.
- `Food_Omelet`: omelete.
- `Food_CrushedCorn`: milho triturado/flocao.
- `Food_Cuscuz`: cuscuz pronto.
- `Item_Corn`: milho inteiro, se o fluxo de milho triturado entrar na demo.

#### Muito Recomendado

- `Item_CrushedCornBowl`: cumbuca/pote com milho triturado.
- `Item_CuscuzPlateVisual`: versao bonita do cuscuz no prato, se diferente do cuscuz solto.
- `Station_FryingPanFoodRootMarker`: pequeno marcador invisivel/empty no Unity, nao precisa mesh.
- `Station_CuscuzeiraSteamHoles`: furinhos/saida visual de vapor.

#### Opcional

- `Item_Spatula`: espatula.
- `Item_Spoon`: colher.
- `Item_PotHolder`: pano/pegador de panela.
- `Food_BurntOverlay`: mesh/material simples para comida queimada, se quiser visual extra.

### 4. Liquidificador

Mesmo que o liquidificador seja cortavel da demo, modele separado se der, porque ele ja aparece no foodtruck e e importante para o futuro.

#### Muito Recomendado

- `Blender_Base`: base/motor.
- `Blender_Cup`: copo transparente.
- `Blender_Lid`: tampa.
- `Blender_Button`: botao.

#### Opcional

- `Blender_Blades`: laminas internas.
- `Blender_Cable`: fio/cabo.
- `Blender_CupMarks`: marcacoes no copo.
- `Blender_CrushedCornInside`: visual de milho triturado dentro.

### 5. Cliente E Personagens

#### Obrigatorio

- `Character_Abigobaldo`: modelo do Abigobaldo.
- `Customer_BaseBody`: corpo base de cliente.
- `Customer_SeuZe`: variacao do Seu Ze.
- `Customer_Nino`: variacao do Nino.
- `Customer_Marcia`: variacao da Marcia.

#### Muito Recomendado

- `Customer_Hat_A`: chapeu para variacao.
- `Customer_Hair_A`: cabelo simples.
- `Customer_Beard_A`: barba/bigode.
- `Customer_Bag_A`: sacola/bolsa.

#### Opcional

- `Customer_EmotionHappy`: boca/olho feliz.
- `Customer_EmotionSad`: boca/olho triste.
- `Customer_EmotionAnnoyed`: sobrancelha/reclamacao.

### 6. Pedido, UI Fisica E Papelaria

#### Obrigatorio

- `Prop_OrderPaper`: papel de pedido.
- `Prop_OrderBoard`: mural/quadro de pedidos.
- `Prop_OrderClip`: clip/pregador/fita segurando papel.

#### Muito Recomendado

- `Prop_Pencil`: lapis/caneta.
- `Prop_Notepad`: bloquinho de anotacao.
- `Prop_Stamp`: carimbo de pedido entregue.

#### Opcional

- `Prop_OldNotes`: papeis antigos decorativos.
- `Prop_MenuCard`: cardapio simples.
- `Prop_ReceiptStack`: pilha de papeis.

### 7. Decoracoes Do Foodtruck

#### Muito Recomendado

- `Prop_Radio`: radio completo. Separe visualmente corpo, antena, botoes e grade do alto-falante, mesmo que exporte como um unico modelo.
- `Prop_Calendar`: calendario de parede. Modele base, folha do mes, argola/prego e pelo menos um dia circulado ou riscado.
- `Prop_PhotoFrame`: porta-retrato/foto.
- `Prop_Cloth`: pano colorido.
- `Prop_StringLights`: luzinhas/fio de luz.
- `Prop_SmallPlant`: plantinha.
- `Prop_TrashBin`: lixeira.
- `Prop_SpiceJar_A`: pote de tempero 1.
- `Prop_SpiceJar_B`: pote de tempero 2.

#### Opcional

- `Prop_PanHanger`: panelas penduradas.
- `Prop_TrophyShelf`: pequena prateleira de trofeus futuros.
- `Prop_RecycledTrophy`: trofeu reciclado simples.
- `Prop_WallSticker`: adesivo/frase.
- `Prop_FoodCrate`: caixa de ingredientes.
- `Prop_Cup`: copo simples.
- `Prop_Bottle`: garrafa.
- `Prop_TowelHook`: gancho de pano.

### 8. Ambiente Externo Simples

#### Obrigatorio

- `Environment_Ground`: chao/rua simples.
- `Environment_CustomerSpot`: ponto visual onde cliente fica, pode ser apenas vazio no Unity.

#### Muito Recomendado

- `Environment_StreetLamp`: poste/luz externa.
- `Environment_Crate`: caixote.
- `Environment_Bench`: banco simples.

#### Opcional

- `Environment_HouseSilhouette`: silhueta de casa ao fundo.
- `Environment_WirePole`: poste com fios.
- `Environment_TrashBag`: saco de lixo decorativo.

### 9. Efeitos Visuais Modelados

#### Muito Recomendado

- `VFX_SteamPuffMesh`: mesh simples/soft para vapor, se usar mesh em vez de particula.
- `VFX_EggCrack`: pequeno decal/mesh de rachadura do ovo.

#### Opcional

- `VFX_BrokenPlateShard_A`
- `VFX_BrokenPlateShard_B`
- `VFX_BrokenPlateShard_C`

### 10. Se O Tempo Acabar

Faca so estes:

- Foodtruck exterior/interior retexturizado.
- Placa `Abigobaldo's`.
- Radio.
- Mural de pedidos.
- Lixeira.
- Abigobaldo.
- 1 corpo base + 5 variacoes simples de cliente.
- Ovo inteiro.
- Ovo quebrado.
- Ovo frito.
- Omelete.
- Milho triturado/flocao.
- Cuscuz.
- Frigideira.
- Cuscuzeira com tampa.
- Prato.
- Papel de pedido.

---

## 1. Foodtruck

### O Que Significa Fazer

O foodtruck precisa deixar de ser apenas um veiculo funcional e virar o espaco emocional da demo. Ele deve comunicar que Abigobaldo mora/trabalha ali ha um tempo, improvisa coisas, cuida dos objetos e gosta daquele lugar.

Isso envolve:

- retexturizar exterior;
- retexturizar interior;
- adicionar detalhes de vida;
- criar pontos de foco para gameplay;
- deixar a janela de atendimento clara;
- deixar o interior mais quente e acolhedor.

### Exterior

Fazer:

- Pintura principal amarela/cartoon mais agradavel.
- Placa com nome `Abigobaldo's`.
- Janela de atendimento bem evidente.
- Frente do foodtruck com cara simpatica.
- Rodas e para-choque com material coerente.
- Pequenos adesivos ou marcas de uso.

O que observar:

- De longe, a silhueta deve ser reconhecivel.
- A janela deve parecer o lugar onde clientes aparecem.
- A cor nao pode estourar demais na luz.

### Interior

Fazer:

- Bancada com materiais mais bonitos.
- Paredes internas mais quentes.
- Geladeira, pia e fogao com cores separadas.
- Area de pedidos visivel.
- Area de preparo limpa o suficiente para leitura.

O que observar:

- O jogador precisa entender onde cozinha.
- O interior nao pode parecer uma caixa amarela vazia.
- As estacoes importantes precisam ser encontradas sem explicacao longa.

### Personalidade

Adicionar pelo menos 5 elementos:

- Radio.
- Mural/clip de pedidos.
- Lixeira.
- Pano colorido.
- Luz interna quente.
- Calendario.
- Foto/quadrozinho.
- Panela pendurada.
- Potinho de tempero.
- Bilhetes antigos.
- Frase/adesivo.

Sugestoes de frases:

- `Hoje ninguem dorme de barriga vazia`
- `Comida quente, coracao quentinho`
- `Pague com um sorriso`
- `Cuscuz salva`

### Pronto Quando

- O foodtruck visto de fora tem identidade.
- O interior tem pelo menos radio, mural de pedidos e lixeira.
- A janela de atendimento esta clara.
- A cena parece mais quente e menos vazia.

---

## 2. Abigobaldo

### O Que Significa Fazer

Abigobaldo e a assinatura visual do jogo. Ele nao precisa ter animacao perfeita na demo, mas precisa ter silhueta e carisma.

Fazer:

- Ajustar modelo atual.
- Garantir chapeu, bigode, barriga e avental.
- Corrigir materiais.
- Garantir que a cabeca nao atrapalha a camera em primeira pessoa.
- Ter uma versao visivel pela janela/exterior, se a camera mostrar.

### Visual Esperado

Abigobaldo deve parecer:

- alegre;
- redondo;
- gentil;
- meio exagerado;
- cartoon;
- cozinheiro de foodtruck, nao chef de restaurante chique.

### Pronto Quando

- O modelo e reconhecivel em 2 segundos.
- O bigode e a barriga aparecem bem.
- A roupa tem pelo menos 2 cores principais.
- Nao ha parte do modelo cobrindo a camera do jogador.

---

## 3. Clientes

### O Que Significa Fazer

Criar 3 clientes para a demo. Eles podem compartilhar a mesma base, mas precisam parecer pessoas diferentes.

Nao tente fazer modelos super detalhados. Faca 1 corpo base e varie:

- cor da roupa;
- cabelo/chapeu;
- barba;
- acessorio;
- altura;
- postura;
- rosto simples.

### Lista De Clientes

#### Seu Ze

Visual:

- senhor simples;
- roupa discreta;
- postura cansada;
- talvez chapeu ou cabelo branco.

Funcao:

- primeiro dialogo;
- mostra vergonha de pedir comida.

#### Nino

Visual:

- mais jovem;
- roupa mais colorida;
- postura mais solta.

Funcao:

- primeiro pedido: ovo frito.

#### Marcia

Visual:

- adulta;
- roupa pratica;
- expressao cansada, mas gentil.

Funcao:

- pedido de cuscuz.

### Pronto Quando

- Existem 3 prefabs ou 3 variacoes prontas para aparecer na janela.
- Da para diferenciar cada cliente sem ler o nome.
- Todos tem escala coerente.
- Todos cabem no ponto da janela.

---

## 4. Comidas

### O Que Significa Fazer

Os modelos de comida precisam ser extremamente legiveis. A demo depende disso.

Se o jogador nao entende visualmente "isso e ovo", "isso e cuscuz", "isso e omelete", a receita perde graca.

### Modelos Obrigatorios

#### Ovo Inteiro

Uso:

- item pegavel;
- ingrediente inicial.

Precisa:

- silhueta oval;
- cor clara;
- tamanho bom na mao.

#### Ovo Quebrado / Gema E Clara

Uso:

- visual na frigideira.

Precisa:

- gema amarela forte;
- clara branca/amarelada;
- formato achatado.

#### Casca De Ovo

Uso:

- feedback visual;
- lixo opcional.

Precisa:

- dois pedacos simples;
- cor parecida com ovo;
- ser pequeno, mas visivel.

#### Ovo Frito

Uso:

- resultado do ovo.

Precisa:

- gema visivel;
- clara achatada;
- borda levemente irregular.

#### Omelete

Uso:

- receita extra/alternativa da demo.

Precisa:

- formato dobrado ou oval;
- amarelo forte;
- diferente do ovo frito.

#### Milho Triturado / Flocao

Uso:

- resultado do milho batido/triturado;
- ingrediente do cuscuz.

Precisa:

- varias pequenas meshes simples de milho quebrado;
- pedacinhos amarelos irregulares;
- leitura de "milho triturado", nao cereal industrial bonito;
- quantidade suficiente para formar um montinho/punhado.

Como modelar sem enlouquecer:

- faca 5 a 8 mini meshes low poly diferentes;
- cada pedacinho pode ser um cubinho/lasca arredondada irregular;
- varie escala e rotacao no prefab;
- use 2 ou 3 tons de amarelo;
- junte esses pedacos em um prefab `CrushedCorn`;
- se precisar parecer mais cheio, duplique os mesmos pedacos.

Nao fazer:

- nao modelar centenas de flocos;
- nao fazer textura realista;
- nao tentar simular graos individuais com fisica;
- nao fazer "corn flakes" de cereal matinal.

Ideia no jogo:

```text
Milho inteiro -> liquidificador -> varias pequenas meshes de milho quebrado -> isso e o flocao da demo
```

#### Cuscuz

Uso:

- resultado da cuscuzeira.

Precisa:

- formato simples e reconhecivel;
- amarelo claro;
- parecer comida pronta.

### Pronto Quando

- Todas as comidas tem modelo ou placeholder legivel.
- Todas usam materiais cartoon.
- Todas tem escala testada na mao, na frigideira/cuscuzeira e no prato.

---

## 5. Utensilios E Estacoes

### Frigideira

Fazer:

- modelo limpo;
- ponto visual para ovo/omelete;
- material separado para metal/cabo;
- encaixe no fogao.

Pronto quando:

- da para entender onde o ovo aparece;
- parece uma frigideira mesmo vista de cima.

### Cuscuzeira

Fazer:

- corpo;
- tampa, se der tempo;
- ponto visual para cuscuz;
- local de vapor.

Pronto quando:

- da para entender que e um recipiente de cozimento;
- vapor pode sair de um ponto claro.

### Prato

Fazer:

- prato com root/ponto central claro;
- escala correta;
- material simples.

Pronto quando:

- ovo, cuscuz e omelete aparecem bem em cima.

### Pote De Milho Triturado

Fazer:

- pote ou cumbuca simples;
- visual de milho triturado/flocao;
- item pegavel ou spawner visual.

Pronto quando:

- jogador entende que aquilo e ingrediente do cuscuz.

### Papel De Pedido

Fazer:

- papel simples;
- pode ser plano com textura ou UI;
- espaco para nome e prato.

Pronto quando:

- da para usar como UI/papel no Dia 2.

---

## 6. Detalhamento Dos Modelos E Pivots

### Regra Geral De Modelagem

Todo modelo da demo deve ser pensado em tres camadas:

```text
Objeto
  Model
  Colliders
  Roots / Anchors
```

O `Model` e o visual.  
Os `Colliders` sao a fisica/interacao.  
Os `Roots / Anchors` sao pontos vazios para comida, particula, camera, tampa, mao ou entrega.

### Sobre Pivot Point

Pivot importa quando o objeto:

- abre;
- gira;
- e segurado na mao;
- tem tampa;
- e colocado em uma estacao;
- recebe comida em cima;
- vai quebrar/arremessar;
- precisa alinhar com outro objeto.

Pivot importa menos quando o objeto e so decoracao estatica.

Regra geral:

- Objeto pegavel: pivot no centro de massa aproximado ou no ponto onde ele deve ficar equilibrado na mao.
- Tampa/porta: pivot na dobradica/eixo real de rotacao.
- Prato/container: pivot no centro do objeto, com um `ContentRoot` separado em cima.
- Comida no prato: pivot no ponto que deve encostar no `ContentRoot`.
- Cliente: pivot no centro dos pes, no chao.
- Decoracao de parede: pivot no centro ou no ponto de fixacao.

Evitar:

- pivot muito longe do mesh;
- root com escala 10;
- rotacao aplicada no root sem necessidade;
- mesh visual e collider com tamanhos diferentes.

### Foodtruck

#### Separar Ou Nao Separar?

Para a demo, o foodtruck pode ter partes separadas apenas onde existe interacao ou troca visual.

Separar:

- corpo principal;
- porta da cabine, se abre;
- janela de atendimento, se abre/fecha;
- rodas, se quiser girar ou ajustar;
- interior/bancada;
- objetos decorativos importantes;
- mural de pedidos;
- radio;
- lixeira;
- luz interna.

Pode ser um mesh so:

- paredes externas;
- teto;
- para-choque, se nao interage;
- detalhes fixos pequenos.

#### Pivots Recomendados

- Corpo do foodtruck: centro da base do veiculo.
- Porta: eixo na dobradica.
- Janela de atendimento: eixo na dobradica/trilho, se for abrir.
- Rodas: centro exato da roda.
- Mural de pedidos: centro do mural ou ponto de fixacao na parede.
- Radio: centro da base.
- Lixeira: centro da base, no chao.

#### Roots Uteis

Criar GameObjects vazios:

```text
Foodtruck
  CustomerPoint
  DeliveryPoint
  OrderBoardRoot
  RadioPoint
  InteriorLightPoint
  PlayerSpawnPoint
```

### Liquidificador

Mesmo que o liquidificador nao seja receita principal da demo, ele aparece no foodtruck e deve estar modelado de forma certa para nao virar dor depois.

#### Pecas Separadas

Separar:

- `Blender_Base`: motor/base fixa.
- `Blender_Cup`: copo removivel.
- `Blender_Lid`: tampa removivel ou animavel.
- `Blender_Button`: botao, se for clicavel.
- `Blender_ContentRoot`: ponto vazio dentro do copo.

Pode ser junto:

- detalhes pequenos do motor;
- cabo;
- pes da base;
- marcacoes do copo, se nao forem interativas.

#### Por Que Separar?

- A base fica parada na bancada.
- O copo pode ser pegavel no jogo completo.
- A tampa precisa abrir/fechar no modo estacao.
- O conteudo precisa aparecer dentro do copo.
- O botao pode ligar/desligar sem selecionar o copo.

#### Pivots Recomendados

- Base: centro da base, na parte que encosta na bancada.
- Copo: centro da base do copo, onde encaixa no motor.
- Tampa: centro da tampa se ela for removida para cima; dobradica/eixo se ela abrir girando.
- Botao: centro do botao.
- `ContentRoot`: centro interno do copo, um pouco acima do fundo.

#### Hierarquia Recomendada

```text
Blender
  Blender_Base
    Model
    Button
    CupAnchor
  Blender_Cup
    Model
    LidAnchor
    ContentRoot
  Blender_Lid
    Model
```

Para a demo, pode simplificar:

```text
Blender_Decorativo
  Base
  Cup
  Lid
```

Mas modele separado se conseguir.

### Frigideira

#### Pecas Separadas

Separar:

- corpo da frigideira;
- cabo;
- `FoodRoot`;
- `SteamRoot`;
- opcional: area/mesh do ovo na frigideira.

Pode ser junto:

- corpo e cabo, se nao for animar;
- detalhes pequenos.

#### Pivots Recomendados

- Se a frigideira for pegavel/balancavel: pivot perto do ponto onde a mao segura o cabo, ou centro de massa entre corpo e cabo se a fisica importar.
- Se ela ficar fixa no fogao: pivot no centro da frigideira.
- `FoodRoot`: centro da area interna da frigideira.
- `SteamRoot`: um pouco acima da comida.

#### Observacao Para Omelete

Se omelete exige balancar a frigideira, e melhor ter:

```text
FryingPan
  Model
  HandleGripPoint
  FoodRoot
  SteamRoot
```

O `HandleGripPoint` ajuda a alinhar a mao/camera se no futuro tiver modo estacao.

### Cuscuzeira

#### Pecas Separadas

Separar:

- corpo/panela;
- tampa;
- `FoodRoot`;
- `SteamRoot`;
- opcional: alca da tampa, se quiser interagir.

Pode ser junto:

- alcas laterais;
- detalhes fixos.

#### Pivots Recomendados

- Corpo: centro da base.
- Tampa removivel: centro da tampa.
- Tampa com dobradica: pivot na dobradica.
- `FoodRoot`: centro interno, onde o cuscuz aparece.
- `SteamRoot`: ponto no topo/furos de vapor.

#### Observacao

Na demo, a tampa pode ser so visual. No jogo completo, ela deve ser separada para abrir/fechar em modo estacao.

### Prato

#### Pecas Separadas

Separar:

- modelo do prato;
- collider simples;
- `ContentRoot`.

#### Pivots Recomendados

- Prato: centro da base.
- `ContentRoot`: centro em cima do prato, onde a comida vai aparecer.

#### Regra Importante

Comida no prato deve usar o proprio pivot do prefab da comida. O prato nao deve corrigir escala da comida escondido.

### Papel De Pedido / Mural

#### Pecas Separadas

Separar:

- mural/base;
- papeis individuais;
- clips/fita, se der tempo;
- `PaperSpawnRoot` ou pontos de encaixe.

#### Pivots Recomendados

- Mural: centro ou ponto de fixacao.
- Papel: centro do papel.
- Ponto de encaixe: onde o papel deve aparecer no mural.

#### Observacao

Se faltar tempo, o papel pode ser UI. Mas ter pelo menos um mural fisico no foodtruck ajuda muito a personalidade.

### Radio

#### Pecas Separadas

Separar:

- corpo do radio;
- botoes, se for interativo;
- antena, se quiser charme;
- `AudioSourcePoint`.

#### Pivots Recomendados

- Radio: centro da base.
- Botao: centro do botao.
- Antena: base da antena se for animar.

### Lixeira

#### Pecas Separadas

Separar:

- corpo;
- tampa, se abrir;
- `TrashTrigger`;
- opcional: pedal.

#### Pivots Recomendados

- Corpo: centro da base.
- Tampa: dobradica/eixo de abertura.
- Trigger: dentro da boca da lixeira.

#### Observacao

Mesmo se a lixeira nao funcionar no Dia 2, modele ela. Ela comunica que a cozinha pode ficar baguncada e que existe cuidado com desperdicio.

### Clientes

#### Separar Ou Nao Separar?

Para a demo, clientes podem ser simples.

Separar:

- corpo;
- cabeca;
- cabelo/chapeu;
- acessorios;
- olhos/boca, se quiser trocar expressao.

Pode ser junto:

- roupa inteira;
- sapatos;
- detalhes pequenos.

#### Pivots Recomendados

- Cliente: centro dos pes no chao.
- Cabeca: base do pescoco, se for animar olhar.
- Acessorios: pivot no ponto de encaixe.

#### Variacoes Rapidas

Use o mesmo corpo base e varie:

- cor da roupa;
- chapeu;
- cabelo;
- barba;
- altura;
- escala horizontal;
- expressao.

### Comidas

#### Pivots Por Comida

- Ovo inteiro: centro do ovo.
- Ovo quebrado/gema: centro da gema ou centro do conjunto.
- Casca: centro do pedaco.
- Ovo frito: centro da gema ou centro da clara, mas com base no plano da frigideira/prato.
- Omelete: centro da base.
- Milho triturado/flocao: centro da base do monte/pote.
- Cuscuz: centro da base.

#### Regra De Escala

Todos os alimentos devem ser testados em 3 lugares:

- na mao;
- na estacao;
- no prato.

Se parecer bom em um lugar e ruim em outro, o problema provavelmente e pivot/escala do prefab.

---

## 7. Paleta De Cores E Referencias Visuais

### O Que Significa Fazer

Paleta nao e so "escolher cores bonitas". Ela precisa comunicar:

- comida regional;
- calor humano;
- noite acolhedora;
- cartoon;
- foodtruck simples;
- Abigobaldo feliz.

Use as paletas abaixo como direcao. Voce pode pesquisar as referencias e adaptar.

### Referencias Para Pesquisar

Pesquise termos como:

- `festa junina color palette`
- `cordel nordestino colors`
- `feira livre nordeste barraca comida`
- `food truck cartoon color palette`
- `Overcooked food truck kitchen`
- `Cooking Mama kitchen UI`
- `low poly cartoon kitchen`
- `Brazilian street food stand colors`
- `cuscuz nordestino fotografia`
- `cozinha nordestina simples`
- `forro radio vintage`
- `cartoon chef character apron`

### Paleta 1 - Foodtruck Cuscuz Quentinho

Boa para a demo atual, porque conversa com milho/cuscuz.

- Amarelo milho: `#E5B93F`
- Amarelo claro creme: `#F4D77A`
- Marrom bancada: `#8B5A3C`
- Azul avental/contraste: `#2E4F7A`
- Vermelho pequeno destaque: `#C94A3A`
- Branco quente: `#FFF2D2`
- Cinza metal: `#9BA3A6`

Uso:

- Foodtruck: amarelo milho + creme.
- Interior: creme + marrom.
- Detalhes: vermelho em botoes/adesivos.
- Abigobaldo: azul no avental para contrastar.

### Paleta 2 - Sao Joao / Feira

Mais colorida e festiva.

- Amarelo bandeirinha: `#F2C94C`
- Azul ceu: `#2D9CDB`
- Vermelho bandeirinha: `#EB5757`
- Verde folha: `#27AE60`
- Laranja milho assado: `#F2994A`
- Marrom madeira: `#6B4423`
- Off-white papel: `#F7E7C1`

Uso:

- Decoracoes, bandeirinhas, panos, papeis e UI.
- Cuidado para nao virar carnaval de cor. Use como acento, nao tudo ao mesmo tempo.

### Paleta 3 - Cordel E Rua A Noite

Mais estilizada, com contraste forte.

- Preto cordel: `#1F1A17`
- Papel cordel: `#F2E2B8`
- Vermelho queimado: `#A6372D`
- Amarelo ocre: `#D99A2B`
- Azul noite: `#263B59`
- Verde apagado: `#5B7C47`

Uso:

- UI de pedido.
- Tela de menu.
- Cartazes do foodtruck.
- Resultado final.

### Paleta 4 - Interior Acolhedor

Boa para luz e materiais internos.

- Luz quente: `#FFD27A`
- Parede amarela suave: `#DFAF45`
- Madeira quente: `#7A4E2D`
- Metal frio leve: `#8FA0A6`
- Azul sombra: `#31435C`
- Vermelho tapete/pano: `#B83A32`

Uso:

- Interior do foodtruck.
- Luzes.
- Bancadas.
- Pequenos objetos.

### Paleta Do Abigobaldo

Abigobaldo e galego, entao pele/cabelo podem ser mais claros, mas a roupa precisa destacar ele dentro do foodtruck amarelo.

#### Opcao A - Avental Azul Classico

- Pele clara quente: `#F2C6A0`
- Cabelo/bigode loiro: `#E8C45C`
- Camisa branca quente: `#FFF0D0`
- Avental azul escuro: `#244A75`
- Calca marrom: `#6A4328`
- Sapato preto suave: `#252525`

Por que funciona:

- Azul contrasta com o foodtruck amarelo.
- Branco passa ideia de cozinheiro.
- Marrom combina com cozinha simples.

#### Opcao B - Avental Verde Feira

- Pele clara quente: `#F2C6A0`
- Cabelo/bigode loiro: `#E8C45C`
- Camisa creme: `#FFE8B5`
- Avental verde escuro: `#2F6B4F`
- Detalhe vermelho: `#C94A3A`
- Calca azul escuro: `#263B59`

Por que funciona:

- Verde lembra feira/ingrediente.
- Vermelho pequeno da carisma.
- Menos comum que azul.

#### Opcao C - Avental Vermelho Com Azul

- Pele clara quente: `#F2C6A0`
- Cabelo/bigode loiro: `#E8C45C`
- Camisa branca: `#FFF2D2`
- Avental vermelho queimado: `#B83A32`
- Detalhe azul: `#2E4F7A`
- Calca marrom: `#6B4423`

Por que funciona:

- Vermelho chama muito a atencao.
- Bom para personagem principal.
- Cuidado para nao brigar com UI/alertas.

#### Minha Recomendacao

Use **Opcao A - Avental Azul Classico**.

Motivo:

- O foodtruck ja e amarelo.
- Azul e contraste complementar.
- Abigobaldo fica legivel tanto dentro quanto fora.
- Combina com roupa de cozinheiro cartoon.

### Paleta Dos Clientes

Clientes precisam ser diferentes, mas nao neon demais.

Sugestao:

- Seu Ze: marrom, bege, verde apagado.
- Nino: vermelho/laranja pequeno, camiseta azul ou verde.
- Marcia: roupa terracota, pano amarelo ou creme.

Regra:

- Cliente nao pode ter a mesma cor dominante do fundo onde aparece.
- Pelo menos um detalhe diferente por cliente.

### Paleta De UI / Pedido

Pedido em papel:

- Papel: `#F7E7C1`
- Texto: `#2A1D14`
- Carimbo/acerto: `#2F8F4E`
- Erro: `#B83A32`
- Destaque: `#D99A2B`

Menu:

- Fundo azul noite ou foodtruck desfocado.
- Titulo amarelo/creme.
- Botoes marrom/azul.

---

## 8. Texturas E Materiais

### O Que Significa Fazer

Nao e fazer textura realista. E criar uma paleta coerente.

Regras:

- cartoon;
- cores fortes, mas controladas;
- comida mais saturada que utensilio;
- interior quente;
- exterior simpatico;
- metal sem brilho exagerado.

### Materiais Minimos

- Amarelo foodtruck.
- Parede interior quente.
- Bancada madeira/marrom.
- Metal cinza.
- Ovo branco.
- Gema amarelo forte.
- Cuscuz amarelo claro.
- Omelete amarelo/dourado.
- Papel bege.
- Roupa de cada cliente.

### Pronto Quando

- Nada esta rosa/missing material.
- O foodtruck nao parece plastico brilhante demais.
- Comida chama atencao.
- Cliente nao se mistura com fundo.

---

## 9. Organizacao Dos Assets

### O Que Significa Fazer

Mesmo com pressa, nao jogar tudo solto.

Sugestao:

```text
Assets/_Demo
  Models
  Materials
  Prefabs
    Characters
    Food
    Stations
    Props
  Audio
  UI
```

Se nao quiser mover tudo agora, pelo menos nomeie corretamente.

### Nomes

Use nomes claros:

- `Customer_SeuZe`
- `Customer_Nino`
- `Customer_Marcia`
- `Food_Egg`
- `Food_FriedEgg`
- `Food_Omelet`
- `Food_CrushedCorn`
- `Station_FryingPan`
- `Prop_Radio`
- `Prop_OrderBoard`

### Pronto Quando

- Voce consegue achar qualquer asset da demo em menos de 10 segundos.
- Nenhum modelo novo fica com nome tipo `Cube.023`.

---

## 10. Criterio De Fechamento Do Dia 1

No fim do Dia 1, voce deve abrir a cena e conferir:

- O foodtruck tem nome, cor e decoracao.
- O interior parece habitado.
- Existem 3 clientes ou variacoes.
- Existem todos os modelos de comida da demo.
- Existem os utensilios principais.
- As comidas aparecem com escala coerente.
- O projeto nao ficou mais baguncado.

Nao precisa:

- sistema de cliente funcionando;
- receita funcionando;
- menu pronto;
- som completo;
- tela final.

---

## Checklist Final Do Dia 1

### Foodtruck

- [ ] Exterior retexturizado.
- [ ] Interior retexturizado.
- [ ] Placa `Abigobaldo's` criada.
- [ ] Janela de atendimento clara.
- [ ] Radio modelado/posicionado.
- [ ] Mural ou clip de pedidos criado.
- [ ] Lixeira criada ou placeholder colocado.
- [ ] Luz interna posicionada ou ponto definido.
- [ ] Pelo menos 3 decoracoes extras colocadas.

### Personagens

- [ ] Abigobaldo revisado.
- [ ] Cliente base criado.
- [ ] Seu Ze criado.
- [ ] Nino criado.
- [ ] Marcia criada.
- [ ] Clientes testados na posicao da janela.

### Comidas

- [ ] Ovo inteiro.
- [ ] Ovo quebrado/gema/clara.
- [ ] Casca de ovo.
- [ ] Ovo frito.
- [ ] Omelete.
- [ ] Milho triturado/flocao.
- [ ] Cuscuz.
- [ ] Escalas testadas no prato/mao/estacao.

### Utensilios

- [ ] Frigideira.
- [ ] Cuscuzeira.
- [ ] Prato.
- [ ] Pote/cumbuca para milho triturado.
- [ ] Papel de pedido.

### Materiais E Organizacao

- [ ] Materiais cartoon aplicados.
- [ ] Nenhum material rosa/missing.
- [ ] Assets novos nomeados corretamente.
- [ ] Assets organizados ou pelo menos agrupados.
- [ ] Cena salva.
