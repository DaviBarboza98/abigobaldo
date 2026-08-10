# ABIGOBALDO'S

## Game Design Document Completo

Versao: 1.0 - Replanejamento do jogo completo  
Data: 10/08/2026  
Genero: Cozinha fisica em primeira pessoa, narrativa leve, simulacao arcade  
Plataforma alvo inicial: PC  
Motor: Unity URP  
Tom: caloroso, brasileiro, nordestino, comedia fisica e cuidado humano  

---

## 1. Visao Geral

**Abigobaldo's** e um jogo de cozinha em primeira pessoa dentro de um foodtruck. O jogador controla Abigobaldo, um cozinheiro grande, feliz, bigodudo e generoso que viaja por bairros simples preparando comida de graca para pessoas em situacao de rua, moradores vulneraveis e figuras curiosas da comunidade.

O jogo mistura a tensao leve de jogos como **Overcooked**, o foco em micro-acoes culinarias de **Cooking Mama**, e a fisicalidade boba de um jogo em primeira pessoa onde objetos podem ser pegos, quebrados, jogados, derrubados e usados de forma manual.

O centro do jogo nao e vender comida. O centro e **cuidar de pessoas atraves de comida**.

O jogador nao trabalha por dinheiro. Trabalha por felicidade, gratidao, vinculo, memoria e comunidade.

### Fantasia Do Jogador

> "Eu sou Abigobaldo, estou dentro do meu foodtruck apertado, com meu radio tocando baixinho, a rua la fora cheia de gente esperando um prato quente, e eu vou cozinhar do meu jeito: meio atrapalhado, mas com carinho."

---

## 2. Identidade Do Jogo

### Frase De Venda

Um Cooking Mama fisico em primeira pessoa dentro de um foodtruck nordestino, onde voce prepara comida simples para pessoas que precisam, e cada prato muda um pouco a noite de alguem.

### Pilares

#### 1. Cozinhar Deve Ser Tatil

Cozinhar nao e apenas colocar item em container. Cada estacao importante vira um pequeno momento interativo com camera fixa/zoom:

- quebrar ovo batendo na borda;
- despejar flocao;
- tampar liquidificador;
- balancar frigideira;
- virar milho;
- mexer panela;
- cortar ingredientes;
- jogar lixo fora;
- empratar com cuidado.

#### 2. Clientes Sao Pessoas, Nao Tickets

Os clientes chegam para comer, mas tambem para conversar. Alguns so aparecem para puxar assunto, contar historia, pedir conselho, agradecer, reclamar, rir ou observar Abigobaldo cozinhando.

Eles tem:

- nome;
- jeito de falar;
- humor;
- pedido favorito;
- historia;
- reacoes ao prato;
- chance de retornar em outros dias.

#### 3. Culinaria Nordestina Como Alma

O jogo deve parecer brasileiro e nordestino em comida, objetos, cores, falas e ambiente. Cuscuz, milho, fuba, ovo, sucos regionais, marmitas, panela, cuscuzeira e foodtruck simples nao sao skin: sao identidade.

#### 4. A Cozinha E Um Palco Vivo

O foodtruck deve ter personalidade:

- luzes penduradas;
- radio;
- recados;
- papeizinhos de pedido;
- trofeus reciclados;
- panela velha;
- lixeira;
- pano estampado;
- plantas;
- fotos;
- marcas de uso;
- objetos que contam a vida de Abigobaldo.

#### 5. Cuidado Vale Mais Que Velocidade

Velocidade importa, mas nao domina. O jogo avalia:

- comida no ponto;
- comida quente;
- pedido correto;
- desperdicio;
- humor do cliente;
- cuidado especial com quem esta debilitado;
- limpeza basica;
- apresentacao.

---

## 3. ODS E Mensagem

O jogo conversa principalmente com:

### ODS 2 - Fome Zero E Agricultura Sustentavel

Abigobaldo combate a fome de forma comunitaria, preparando alimentos simples, acessiveis e nutritivos. O jogo nao trata comida como mercadoria principal, mas como cuidado.

Como aparece no jogo:

- alimentar pessoas vulneraveis;
- evitar desperdicio;
- valorizar receitas simples e regionais;
- mostrar que comida quente e dignidade importam;
- clientes debilitados melhoram quando recebem comida adequada.

### ODS 3 - Saude E Bem-Estar

O jogo representa alimentacao como parte de bem-estar, energia, acolhimento e recuperacao emocional. Nao promete cura milagrosa. O foco e cuidado, nutricao e dignidade.

Como aparece no jogo:

- clientes chegam cansados, tristes ou debilitados;
- refeicoes bem feitas melhoram humor e energia;
- pratos queimados, crus ou contaminados reduzem satisfacao;
- hidratacao e sucos podem entrar na versao completa;
- fim da noite mostra felicidade e bem-estar da comunidade.

### Cuidado Com O Tema

O jogo deve ser respeitoso. Pessoas em situacao de rua nao devem ser piada. A comedia vem de Abigobaldo, da cozinha, dos objetos e do caos culinario, nao da vulnerabilidade dos clientes.

---

## 4. Estrutura Do Jogo Completo

### Formato

Campanha curta de 7 noites.

Cada noite acontece em um local diferente. Abigobaldo permanece dentro do foodtruck; o lado de fora muda.

### Ritmo De Uma Noite

1. Abigobaldo chega ao local.
2. A luz e o clima da rua apresentam o bairro.
3. O radio liga.
4. O primeiro cliente aparece.
5. Clientes pedem comida, conversam ou apenas passam.
6. O jogador cozinha, emprata e entrega.
7. Pequenos eventos acontecem.
8. A noite termina.
9. Tela de resultado mostra felicidade, capricho, desperdicio e historias.
10. O foodtruck segue para o proximo local.

### Locais Da Campanha

#### Noite 1 - Rua De Terra

Tutorial emocional. Poucos clientes, pedidos simples, clima quente e acolhedor.

#### Noite 2 - Praca Esquecida

Mais movimento. Clientes conversam mais. Primeiros pedidos combinados.

#### Noite 3 - Debaixo Do Viaduto

Ambiente mais pesado. Clientes debilitados aparecem. Foco em cuidado.

#### Noite 4 - Feira Depois Do Expediente

Mais cor, mais barulho, mais ingredientes. Receitas com milho e sucos ganham destaque.

#### Noite 5 - Bairro Alagado

Mais dificuldade visual/ambiental. Luz fria, chuva, pedidos quentes valorizados.

#### Noite 6 - Centro A Noite

Mais caos. Grupos aparecem, clientes impacientes, pedidos multiplos.

#### Noite 7 - Ceia Comunitaria

Final. Clientes recorrentes retornam. O objetivo nao e so atender rapido, e encerrar historias.

---

## 5. Core Gameplay Loop

### Loop Principal

```text
Cliente aparece -> conversa/pedido -> papel de pedido -> pegar ingredientes -> usar estacoes com zoom -> montar prato/marmita -> entregar -> cliente reage -> proximo pedido/evento
```

### Loop De Curto Prazo

```text
Olhar -> pegar -> posicionar -> executar micro-acao -> ouvir/ver feedback -> decidir tirar/continuar -> montar
```

### Loop De Medio Prazo

```text
Atender varios clientes -> manter cozinha organizada -> evitar desperdicio -> lembrar pedidos -> gerenciar tempo de cozimento
```

### Loop De Longo Prazo

```text
Concluir noites -> rever clientes -> desbloquear receitas -> receber presentes -> decorar foodtruck -> melhorar felicidade da comunidade
```

---

## 6. Modos De Interacao

### 6.1 Modo Livre

O jogador anda dentro do foodtruck em primeira pessoa.

Acoes:

- pegar objetos;
- soltar;
- arremessar;
- abrir portas;
- olhar papeis;
- entregar prato;
- jogar lixo fora;
- acessar estacoes.

### 6.2 Modo Estacao / Zoom Fixo

Ao interagir com uma estacao importante, a camera entra em um enquadramento fixo, como uma bancada de trabalho.

Esse modo transforma a receita em mini-acao fisica.

Exemplos:

- Frigideira: quebrar ovo, virar, balancar.
- Liquidificador: abrir tampa, colocar ingrediente, fechar, ligar.
- Cuscuzeira: abrir, despejar flocao, fechar, esperar vapor.
- Tabua: cortar ingredientes.
- Pia: lavar utensilio.

### Regras Do Modo Estacao

- O jogador nao anda.
- A camera fica fixa ou com movimento limitado.
- O mouse manipula objeto/acao.
- A tecla de interacao confirma ou pega elementos.
- O jogador pode sair da estacao.
- A comida continua sujeita a tempo/estado.

### Por Que Isso Resolve O "Simulador De Container"

Porque o container deixa de ser caixa abstrata. Ele vira cena pequena com gesto:

- bater;
- abrir;
- despejar;
- tampar;
- balancar;
- cortar;
- mexer.

---

## 7. Controles Propostos

### Modo Livre

| Entrada | Acao |
|---|---|
| WASD | Movimento |
| Mouse | Olhar |
| Shift | Correr |
| Clique esquerdo | Pegar objeto / selecionar objeto pegavel |
| E | Interagir com estacao, porta, cliente, entrega |
| G clique | Soltar item |
| G segurar | Arremessar item |
| R segurar | Rotacionar item segurado |
| Scroll | Aproximar/afastar item segurado |
| V | Liberar/travar cursor |
| P | Pausar |

### Modo Estacao

| Entrada | Acao |
|---|---|
| Mouse | Mover peca/ferramenta |
| Clique esquerdo | Segurar/arrastar objeto da estacao |
| E | Acao principal da estacao |
| G | Soltar objeto da estacao |
| Esc ou tecla propria | Sair da estacao |

---

## 8. Sistemas Principais

### 8.1 Sistema De Clientes

Clientes aparecem na janela do foodtruck.

Tipos:

- Cliente com pedido.
- Cliente que so conversa.
- Cliente recorrente.
- Cliente debilitado.
- Cliente irritante/comico.
- Grupo de clientes.
- Cliente narrativo importante.

Dados de cliente:

- nome;
- retrato/modelo;
- voz/sons;
- falas;
- pratos favoritos;
- tolerancia de espera;
- humor inicial;
- historico;
- condicoes especiais.

### 8.2 Sistema De Conversa

Nem todo cliente precisa pedir comida imediatamente. Alguns chegam para:

- agradecer;
- contar algo do bairro;
- perguntar se sobrou comida;
- comentar do cheiro;
- fazer piada;
- entregar presente;
- avisar de um evento.

Conversas curtas devem aparecer em balao ou UI simples.

Exemplo:

> "Seu Abigobaldo, hoje eu so vim sentir o cheiro. Ja ajuda."

Esse tipo de fala da alma ao jogo.

### 8.3 Sistema De Pedido Em Papel

Quando um cliente faz pedido, um papel aparece.

Fluxo:

1. Cliente fala.
2. Papelzinho de pedido e criado.
3. Papel vai para mural/clip na parede.
4. Jogador pode olhar o pedido.
5. Ao entregar, papel recebe carimbo ou some.

Visual:

- papel amarelado;
- letra desenhada;
- icone do prato;
- nome do cliente;
- observacao: "bem quentinho", "sem queimar", "caprichado".

### 8.4 Sistema De Receitas

Receitas sao compostas por etapas, nao so ingredientes.

Exemplo:

```text
Ovo frito:
  1. Quebrar ovo na frigideira.
  2. Esperar clara firmar.
  3. Tirar no ponto.
```

```text
Cuscuz:
  1. Fazer flocao/fuba no liquidificador ou pegar pronto.
  2. Colocar na cuscuzeira.
  3. Tampar.
  4. Esperar vapor.
  5. Servir.
```

Receita deve registrar:

- nome;
- icone;
- ingredientes;
- estacoes;
- etapas;
- tempo ideal;
- estados de cozimento;
- resultado;
- qualidade.

### 8.5 Sistema De Qualidade

Qualidade nao precisa ser numero visivel o tempo todo.

Fatores:

- cozimento;
- temperatura;
- limpeza;
- desperdicio;
- apresentacao;
- pedido correto;
- tempo de espera;
- cliente especifico.

Estados de cozimento:

- cru;
- quase no ponto;
- no ponto;
- passado;
- queimado;
- carbonizado.

### 8.6 Sistema De Entrega

O jogador entrega segurando prato ou marmita.

A entrega valida:

- prato correto;
- estado correto;
- quantidade;
- acompanhamento;
- temperatura;
- tempo.

Cliente reage com:

- fala;
- animacao simples;
- mudanca de humor;
- aumento/reducao da felicidade final.

### 8.7 Sistema De Resultado Da Noite

No fim da noite:

- pessoas alimentadas;
- felicidade media;
- pratos perfeitos;
- pratos queimados;
- desperdicio;
- conversas importantes;
- presentes recebidos;
- clientes que prometem voltar.

Tela deve parecer emocional, nao planilha.

Exemplo:

```text
Noite 1 - Rua De Terra

5 pessoas alimentadas
4 pratos quentinhos
1 milho queimado, mas aceito com carinho
Seu Zé sorriu pela primeira vez na semana

Felicidade da noite: 82%
```

---

## 9. Receitas

### Receitas Da Primeira Noite

#### Ovo Frito

Ingredientes:

- ovo.

Estacao:

- frigideira.

Interacao:

- abrir modo frigideira;
- bater ovo na borda;
- gema e clara caem;
- cascas aparecem como lixo;
- ovo chia;
- tirar no ponto.

Possivel variacao:

- se balancar a frigideira no meio do cozimento, pode virar omelete.

#### Milho Assado

Ingredientes:

- milho.

Estacao:

- frigideira/grelha.

Interacao:

- colocar milho;
- virar para assar igual;
- tirar antes de queimar.

#### Fuba / Flocao

Ingredientes:

- milho.

Estacao:

- liquidificador.

Interacao:

- abrir tampa;
- colocar milho;
- fechar tampa;
- ligar;
- milho gira, quebra e vira floco.

Observacao:

- Nao precisa simular graos reais no MVP. Pode ser transicao visual: milho inteiro -> milho rachado -> particulas/flocos -> fuba.

#### Cuscuz

Ingredientes:

- fuba/flocao.

Estacao:

- cuscuzeira.

Interacao:

- abrir cuscuzeira;
- despejar flocao;
- fechar;
- vapor sobe;
- tirar no ponto.

Agua:

- Para MVP, agua pode ser abstraida. O flocao ja pode ser considerado hidratado, ou uma garrafa de agua pode ser ingrediente simbolico.

#### Cuscuz Com Ovo

Ingredientes:

- cuscuz pronto;
- ovo frito pronto.

Estacao:

- prato/marmita.

Interacao:

- colocar cuscuz;
- colocar ovo por cima.

#### Omelete

Ingredientes:

- ovo.

Estacao:

- frigideira.

Interacao:

- quebrar ovo;
- durante cozimento, pegar/balancar frigideira;
- se balancar no tempo certo, vira omelete.

### Receitas Futuras

- Cuscuz com manteiga.
- Cuscuz com queijo coalho.
- Tapioca.
- Baião de dois.
- Macaxeira.
- Caldo de feijao.
- Canja.
- Milho cozido.
- Pipoca.
- Bolo de milho.
- Mungunza.
- Suco de acerola.
- Suco de caju.
- Suco de umbu.

---

## 10. Estacoes De Cozinha

### Frigideira

Funcoes:

- ovo frito;
- omelete;
- milho assado;
- outras frituras.

Modo estacao:

- camera fixa para fogao/frigideira;
- objeto interativo sobre a frigideira;
- feedback de oleo, chiado e vapor;
- risco de queimar.

### Cuscuzeira

Funcoes:

- cuscuz;
- receitas cozidas no vapor.

Modo estacao:

- abrir tampa;
- despejar ingrediente;
- fechar;
- observar vapor;
- tirar no ponto.

Futuro:

- outras panelas usando mesma logica de recipiente removivel.

### Liquidificador

Funcoes:

- fuba/flocao;
- sucos;
- misturas.

Modo estacao:

- abrir tampa;
- inserir ingrediente;
- fechar;
- ligar;
- ver transicao.

Regra:

- Se tampa aberta, nao liga.
- Se ligar sem conteudo, faz som vazio.
- Se abrir enquanto ligado, erro comico.

### Tabua De Cortar

Futuro:

- cortar ingredientes;
- picar verduras;
- preparar acompanhamentos.

Modo estacao:

- camera fixa na tabua;
- faca segue mouse;
- cortes simples por cliques/arrasto.

### Pia

Futuro:

- lavar utensilios;
- limpar sujeira;
- encher garrafa.

### Lixeira

Funcoes:

- jogar casca de ovo;
- jogar comida queimada;
- jogar cacos;
- reduzir bagunca.

Deve ser fisica e satisfatoria: acertar lixo no cesto deve ser legal.

---

## 11. Fisica E Bagunca

### Objetos Quebraveis

#### Prato

Se arremessado forte:

- quebra;
- cria cacos;
- cacos somem depois de alguns segundos;
- penaliza desperdicio/bagunca.

#### Ovo

Se bater forte em parede/chao:

- quebra;
- cria gema/clara no local;
- cria cascas;
- some com o tempo;
- pode ser jogado no lixo.

### Lixo

Tipos:

- casca de ovo;
- comida queimada;
- prato quebrado;
- cacos;
- embalagem.

Lixo nao deve virar simulacao pesada. Pode sumir ao entrar no lixeiro.

### Contaminacao

Futuro:

- comida que cai no chao perde qualidade;
- se entregue, cliente reage mal;
- pode ser descartada.

---

## 12. Personagens

### Abigobaldo

Descricao:

- cozinheiro grande;
- bigode marcante;
- barriga;
- sorriso facil;
- avental gasto;
- jeito caloroso;
- meio estabanado.

Personalidade:

- generoso;
- teimoso;
- alegre;
- fala sozinho enquanto cozinha;
- trata todos pelo nome;
- acredita que comida quente muda o dia.

Frases:

- "Bora botar esse cuscuz pra levantar moral."
- "Queimou um tiquinho, mas amor nao queimou nao."
- "Hoje ninguem dorme de barriga vazia."

### Clientes Da Primeira Noite

#### Seu Zé

Pedido favorito:

- cuscuz simples.

Perfil:

- senhor cansado, gentil, fala devagar.

Funcao:

- tutorial emocional.

#### Dona Lúcia

Pedido favorito:

- cuscuz com ovo.

Perfil:

- debilitada, mas carinhosa.

Funcao:

- apresentar ODS Saude e cuidado.

#### Nino

Pedido favorito:

- milho assado.

Perfil:

- jovem brincalhao, faz piada, conversa mais que pede.

Funcao:

- leveza/comedia.

#### Marta

Pedido favorito:

- omelete.

Perfil:

- mae cansada, direta, agradece muito.

Funcao:

- pedido mais complexo.

#### O Reclamão

Pedido favorito:

- ovo frito "sem queimar pelo amor de Deus".

Perfil:

- irritante comico, mas nao cruel.

Funcao:

- humor, erro e conquista futura.

### Clientes Que So Conversam

Alguns clientes nao pedem comida naquele momento.

Exemplos:

- crianca que pergunta se o foodtruck voa;
- senhor que entrega um bilhete;
- pessoa que avisa que tem mais gente vindo;
- cliente antigo que agradece por ontem;
- morador que comenta da chuva.

Isso deixa o mundo vivo sem criar mais receita.

---

## 13. Historia

### Premissa

Abigobaldo herdou um foodtruck velho e decidiu usa-lo para distribuir comida em bairros onde muita gente passa necessidade. Ele nao tem muito dinheiro, mas tem fogao, panela, milho, ovo, radio e vontade.

Durante sete noites, ele visita lugares diferentes e encontra pessoas que revelam pequenos fragmentos de uma comunidade.

### Estrutura Narrativa

Cada noite tem:

- local;
- tema emocional;
- cliente principal;
- receita destaque;
- evento pequeno;
- resultado.

### Arco Geral

No comeco, Abigobaldo e so "o cara da comida". No final, ele vira parte da comunidade. O foodtruck deixa de ser so cozinha e vira ponto de encontro.

### Final

Na setima noite, varios clientes retornam. Alguns trazem presentes, bilhetes ou ingredientes. A tela final mostra nao so pontuacao, mas o impacto:

- quem comeu;
- quem voltou;
- quem sorriu;
- quem melhorou;
- que lembrancas ficaram no foodtruck.

---

## 14. Direcao De Arte

### Estilo Visual

Low poly carismatico, colorido e levemente exagerado.

Nao precisa realismo. Precisa leitura.

Caracteristicas:

- formas simples;
- cores fortes;
- materiais foscos;
- comida com silhueta clara;
- objetos arredondados o suficiente para parecerem amigaveis;
- sujeira e desgaste controlados.

### Paleta

Foodtruck:

- vermelho queimado;
- amarelo quente;
- azul gasto;
- branco sujo;
- metal simples.

Interior:

- luz quente;
- sombras suaves;
- madeira/metal;
- panos coloridos;
- detalhes verdes/plantas.

Noite:

- azul escuro do lado de fora;
- laranja/amarelo vindo do foodtruck;
- contraste entre rua fria e comida quente.

### Comida

Comida deve parecer grande, legivel e apetitosa.

Ovo:

- gema amarela forte;
- clara bem visivel;
- bordas mudam conforme cozinha.

Cuscuz:

- amarelo claro;
- textura simples de flocos;
- vapor.

Milho:

- amarelo saturado;
- marcas escuras ao assar.

Fuba:

- montinho granular simples;
- pode usar particulas/flocos.

---

## 15. Modelos 3D Necessarios

### Prioridade Alta

#### Personagens

- Abigobaldo corpo/cabeca/bracos.
- 5 clientes da primeira noite.
- Cliente generico extra.

#### Foodtruck

- exterior;
- janela de atendimento;
- interior;
- fogao/cooktop;
- bancada;
- geladeira;
- pia;
- lixeira;
- mural de pedidos;
- radio;
- luzes decorativas.

#### Itens De Comida

- ovo inteiro;
- ovo quebrado;
- gema/clara na frigideira;
- casca de ovo;
- ovo frito;
- milho;
- milho assado;
- milho quebrado/flocos;
- fuba/flocao;
- cuscuz;
- omelete.

#### Utensilios

- prato;
- marmita futura;
- frigideira;
- cuscuzeira;
- tampa da cuscuzeira;
- liquidificador base;
- copo;
- tampa do liquidificador;
- pote/cumbuca;
- faca futura;
- tabua.

### Prioridade Media

- garrafa de agua;
- garrafa de suco;
- pano;
- colher;
- espatula;
- papel de pedido;
- caneta/lapis;
- caixas/ingredientes.

### Prioridade Baixa

- plantas;
- quadros;
- fotos;
- trofeus;
- adesivos;
- objetos decorativos da prateleira.

---

## 16. Ambientacao E Environment

### Foodtruck

O foodtruck e o personagem silencioso do jogo.

Ele deve parecer:

- pequeno;
- usado;
- amado;
- cheio de improvisos;
- funcional, mas com alma.

Elementos:

- mural de pedidos;
- recados antigos;
- radio;
- luzinha pendurada;
- calendario;
- foto de familia;
- trofeus reciclados;
- panelas penduradas;
- lixeira visivel;
- caixa de ingredientes;
- marcas de gordura;
- janela para clientes.

### Rua

O lado de fora deve contar onde Abigobaldo esta.

Elementos:

- casas simples;
- postes;
- fios;
- calcada quebrada;
- banco;
- caixotes;
- barracas;
- chuva em algumas noites;
- pessoas passando;
- luz distante.

O jogador nao precisa andar fora. O exterior pode ser um palco observado pela janela.

---

## 17. Iluminacao

### Objetivo

Contrastar frio da rua com calor do foodtruck.

### Interior

- luz quente principal;
- sombras suaves;
- highlights nos objetos importantes;
- brilho leve em comida pronta;
- fogao e vapor chamando atencao.

### Exterior

- tons frios;
- postes azulados;
- silhuetas de clientes;
- chuva/neblina em noites especificas;
- luz do foodtruck vazando para fora.

### Progressao De Noite

Durante a noite:

- comeca no fim de tarde;
- escurece;
- luzes do foodtruck ficam mais importantes;
- clientes parecem mais destacados pela janela.

---

## 18. Audio

Audio e obrigatorio para o jogo parecer vivo.

### Sons De Cozinha

- pegar item;
- soltar item;
- arremessar;
- prato quebrando;
- ovo quebrando;
- fritura;
- vapor;
- liquidificador ligando;
- liquidificador funcionando;
- tampa abrindo/fechando;
- pedido recebido;
- entrega correta;
- entrega ruim.

### Ambiente

- radio com forro/instrumental leve;
- rua ao fundo;
- vento;
- chuva;
- conversa distante;
- panela chiando.

### Vozes

Nao precisa dublagem completa.

Pode usar:

- murmúrios;
- risadas;
- "hmm";
- "opa";
- "obrigado";
- sons vocais curtos.

---

## 19. UI

### UI Minima

- mira/interacao;
- nome do objeto selecionado;
- pedido atual;
- papel de pedido;
- indicador de cozimento;
- feedback de entrega;
- tela final da noite.

### UI De Pedido

Deve parecer papel fisico, nao menu moderno.

Campos:

- nome do cliente;
- desenho/icone do prato;
- observacao curta;
- tempo/humor opcional.

### UI De Cozimento

Preferir diegetica:

- vapor;
- som;
- cor;
- tremedeira;
- barra pequena apenas quando necessario.

### Tela Final

Deve ser emocional:

- felicidade;
- pratos entregues;
- desperdicio;
- melhores momentos;
- falas dos clientes;
- presentes.

---

## 20. Progressao

### Curto Prazo

Aprender receitas e atender pedidos.

### Medio Prazo

Desbloquear:

- novas receitas;
- novos clientes;
- novas estacoes;
- decoracoes;
- musicas do radio.

### Longo Prazo

Completar sete noites e formar uma comunidade em volta do foodtruck.

### Presentes

Clientes podem dar:

- desenho;
- bilhete;
- mini trofeu;
- objeto reciclado;
- ingrediente especial;
- foto.

Esses presentes ficam no foodtruck.

---

## 21. Arquitetura De Sistemas

### Managers

- `GameManager`: estado geral.
- `NightManager`: controla inicio/fim da noite.
- `CustomerManager`: cria clientes.
- `OrderManager`: pedidos ativos.
- `DialogueManager`: falas.
- `RecipeManager`: consulta receitas.
- `ScoreManager`: resultado/felicidade.
- `AudioManager`: sons.
- `UIManager`: telas.

### Gameplay

- `HoldableObject`: objeto fisico pegavel.
- `Holder`: mao do jogador.
- `Station`: base para estacoes com zoom.
- `StationCamera`: camera fixa da estacao.
- `FoodPortion`: comida com estado runtime.
- `PlateContainer`: montagem.
- `DeliveryZone`: entrega.
- `BreakableObject`: objetos quebraveis.
- `TrashBin`: remove lixo.

### Dados

- `ObjectData`
- `RecipeData`
- `CustomerData`
- `OrderData`
- `DialogueData`
- `NightData`

---

## 22. Organizacao De Prefabs

### Regra Geral

Todo prefab deve ter:

```text
PrefabRoot scale 1
  Model
  Colliders
  VisualRoots
  Particles
  Audio
```

### Objetos Pegaveis

```text
Egg
  HoldableObject
  Rigidbody
  Collider simples
  Model
```

### Estacoes

```text
FryingPanStation
  Station script
  StationCameraPoint
  InteractionRoot
  Model
  Particles
  Audio
```

### Cliente

```text
Customer
  CustomerController
  DialogueAnchor
  Model
  EmotionVisual
```

### Pedido

```text
OrderPaper
  OrderPaperView
  Mesh/Canvas
  Highlightable
```

---

## 23. Performance

### Metas

- 60 FPS em build simples.
- 30 FPS minimo aceitavel em PC fraco.
- Editor pode oscilar, mas build nao.

### Regras

- evitar MeshCollider em objeto dinamico pequeno;
- root dos prefabs em escala 1;
- luzes com sombra limitadas;
- audio/particulas com pooling se necessario;
- destruir lixo depois de tempo;
- nao usar `FindObjectOfType` em Update;
- UI atualiza quando muda;
- build testada fora do Editor.

### URP

Config recomendada para MVP:

- HDR off se nao estiver usando bloom/color grading forte;
- MSAA off ou 2x;
- shadow distance 15-20;
- additional lights off se nao forem necessarias;
- depth texture off se nao houver recurso que precise;
- baked lighting para interior, quando possivel.

---

## 24. MVP Replanejado

### Objetivo Do MVP

Uma noite jogavel com 5 clientes, 5 pedidos possiveis, foodtruck vivo, cozinha tatil e tela final de felicidade.

### Conteudo

Clientes:

- Seu Zé;
- Dona Lúcia;
- Nino;
- Marta;
- O Reclamão.

Receitas:

- ovo frito;
- milho assado;
- fuba/flocao;
- cuscuz;
- cuscuz com ovo;
- omelete.

Estacoes:

- frigideira;
- liquidificador;
- cuscuzeira;
- prato/marmita;
- lixeira.

Sistemas:

- pedido em papel;
- cliente na janela;
- entrega;
- resultado da noite;
- som basico;
- particulas basicas;
- breakable para ovo/prato se sobrar tempo.

### Criterio De Sucesso

O jogo deve fazer o jogador pensar:

> "Quero atender mais um cliente."

Se isso acontecer, o MVP venceu.

---

## 25. Roadmap

### Fase 1 - Uma Noite Boa

- core de cliente/pedido/entrega;
- 3 estacoes principais;
- 5 clientes;
- tela final.

### Fase 2 - Polimento Da Cozinha

- audio;
- particulas;
- iluminacao;
- escala/colliders;
- decoracao do foodtruck.

### Fase 3 - Mais Receitas E Estacoes

- tabua;
- pia;
- panela;
- sucos;
- marmitas.

### Fase 4 - Campanha

- 7 noites;
- locais;
- clientes recorrentes;
- dialogos;
- presentes.

### Fase 5 - Simulacao Extra

- quebraveis;
- sujeira;
- contaminacao;
- temperatura;
- desperdicio.

---

## 26. Declaracao Final De Design

Abigobaldo's nao deve ser sobre fazer comida perfeita em uma cozinha perfeita. Deve ser sobre tentar fazer uma comida boa em um lugar apertado, com pouco recurso, para pessoas que precisam muito de um gesto simples.

O jogo deve ser engracado quando a cozinha da errado, satisfatorio quando a comida fica no ponto, e carinhoso quando o cliente recebe o prato.

O jogador deve sair pensando menos em "quantos pontos fiz" e mais em:

> "Hoje eu alimentei alguem."

