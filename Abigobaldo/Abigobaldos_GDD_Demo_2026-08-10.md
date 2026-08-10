# ABIGOBALDO'S - GDD DA DEMO

Versao: Demo "cheirinho do jogo"  
Data: 10/08/2026  
Prazo de producao: 3 dias  
Motor: Unity URP  
Genero da demo: cozinha fisica em primeira pessoa, narrativa curta, foodtruck cartoon  

---

## 1. Intencao Da Demo

Esta demo nao precisa mostrar o jogo completo. Ela precisa provar que o jogo **pode ser especial**.

Ela deve dar um cheirinho de:

- cozinhar com as maos;
- conversar com pessoas;
- preparar comida simples;
- entregar um prato quente;
- ver alguem ficar feliz;
- sentir que o foodtruck de Abigobaldo tem alma.

A demo nao e sobre quantidade. E sobre sabor.

### Frase Da Demo

> Em uma noite simples, Abigobaldo abre seu foodtruck e recebe cinco pessoas: algumas pedem comida, outras so precisam conversar. Entre ovo, cuscuz, panela chiando e papeizinhos de pedido, o jogador descobre que cozinhar aqui e uma forma de cuidado.

---

## 2. Escopo Da Demo

### Vai Existir

- Menu inicial.
- Foodtruck jogavel.
- Interior com mais personalidade.
- Abigobaldo visivel/modelado.
- 5 clientes.
- 3 clientes pedem comida.
- 2 clientes conversam.
- Sistema simples de dialogo.
- Sistema simples de pedido.
- Papelzinho/anotacao de pedido.
- Entrega na janela.
- Tela de resultados.
- 3 receitas:
  - cuscuz;
  - ovo frito;
  - omelete.
- Estacoes:
  - frigideira;
  - cuscuzeira;
  - prato/montagem;
  - lixeira, se der tempo.
- Sons basicos.
- Musica/radio.
- Iluminacao e ambientacao cartoon.

### Nao Vai Existir

- Tabua de cortar.
- Agua fisica.
- Liquidificador obrigatorio.
- Milho assado.
- Cuscuz com ovo na demo, a menos que sobre tempo.
- Save.
- Campanha de 7 noites.
- Cliente andando com NavMesh.
- Varios mapas.
- Sistema completo de sujeira.
- Sistema completo de saude/nutricao.
- Trofeus persistentes.
- Lojas/upgrades.
- Muitas receitas.
- Fisica complexa de liquidos.

---

## 3. Objetivo Emocional

Ao terminar a demo, quem jogar deve entender:

1. Abigobaldo e um cozinheiro feliz.
2. O foodtruck e um lugar acolhedor.
3. Os clientes tem vergonha, fome, historia e humanidade.
4. Cozinhar e uma pequena acao de cuidado.
5. O jogo completo poderia virar algo muito maior.

Se a pessoa terminar pensando "eu queria ver mais uma noite", a demo cumpriu seu papel.

---

## 4. Gameplay Da Demo

### Loop Principal

```text
Menu -> entrar no foodtruck -> cliente aparece -> conversa ou pedido -> cozinhar -> empratar -> entregar -> reacao -> proximo cliente -> resultado final
```

### Fluxo Completo

1. Jogador abre o jogo no menu.
2. Clica em "Comecar Demo".
3. Aparece dentro do foodtruck.
4. Radio toca.
5. Primeiro cliente aparece na janela.
6. Cliente conversa e pergunta o nome do cozinheiro.
7. Segundo cliente tambem conversa e reforca quem e Abigobaldo.
8. A partir dai, os proximos clientes ja chegam chamando ele pelo nome.
9. Tres clientes pedem pratos.
10. Jogador cozinha no foodtruck.
11. Jogador entrega os pratos na janela.
12. Clientes reagem.
13. Tela final mostra resultado da noite.

---

## 5. Clientes Da Demo

### Estrutura

Total: 5 clientes.

- 2 clientes de dialogo.
- 3 clientes com pedido.

Os dois primeiros existem para apresentar Abigobaldo e criar o efeito social:

> "Ah, entao voce e o Abigobaldo? Me falaram de voce."

### Cliente 1 - Seu Zé

Tipo: dialogo  
Funcao: apresenta o tema da vergonha/fome  

Resumo:

Seu Zé chega na janela, mas nao pede comida de primeira. Ele esta com vergonha. Ele pergunta o nome de Abigobaldo e tenta disfarcar a fome.

Dialogo exemplo:

- "Boa noite... aqui e onde tao dando comida?"
- "Desculpa perguntar, moço. Qual e seu nome?"
- "Abigobaldo? Nome forte. Combina com cheiro de comida boa."
- "Hoje eu nao vou pedir nao... so queria saber se era verdade."

Resultado:

Ele vai embora sem pedir, mas fala de Abigobaldo para outras pessoas.

### Cliente 2 - Dona Lúcia

Tipo: dialogo  
Funcao: reforca cuidado e ODS Saude/Bem-estar  

Resumo:

Dona Lúcia chega cansada. Tambem tem vergonha. Pergunta se precisa pagar. Abigobaldo tranquiliza.

Dialogo exemplo:

- "Disseram que um tal de Abigobaldo tava ajudando o povo."
- "Mas eu nao tenho dinheiro hoje, meu filho."
- "E de graça mesmo?"
- "Entao amanha eu volto com coragem de pedir."

Resultado:

Ela espalha o nome de Abigobaldo.

### Cliente 3 - Nino

Tipo: pedido  
Pedido: ovo frito  
Funcao: primeiro pedido simples  

Dialogo exemplo:

- "Opa, Abigobaldo! Seu Zé falou que tu salva a noite."
- "Me ve um ovo frito? Se sair meio torto eu finjo que foi charme."

Reacao:

- Pronto: "Rapaz, isso aqui levantou meu espirito."
- Queimado: "Crocrante... ate demais."

### Cliente 4 - Marta

Tipo: pedido  
Pedido: cuscuz  
Funcao: receita com cuscuzeira  

Dialogo exemplo:

- "Abigobaldo, Dona Lúcia disse que tu cozinha com carinho."
- "Se tiver um cuscuz quentinho, eu aceitava."

Reacao:

- Pronto: "Quentinho assim da ate vontade de ficar mais um pouco."
- Cru/ruim: "Agradeco do mesmo jeito, mas esse aqui ficou tristinho."

### Cliente 5 - Reclamão

Tipo: pedido  
Pedido: omelete  
Funcao: receita mais divertida da demo  

Dialogo exemplo:

- "Entao voce e o Abigobaldo famoso?"
- "Quero ver se esse bigode sabe fazer omelete."
- "E sem queimar, viu?"

Reacao:

- Pronto: "Hmph. Ta bom. Ta muito bom. Mas eu nao vou repetir."
- Queimado: "Eu pedi omelete, nao lembranca de incendio."

---

## 6. Dialogo E Pedido

### Sistema De Dialogo

Precisa ser simples:

- cliente aparece;
- caixa de dialogo mostra fala;
- jogador aperta E para avancar;
- algumas falas podem ativar pedido;
- outras encerram conversa.

Nao precisa escolha de resposta na demo.

### Sistema De Pedido

Quando cliente pede comida:

1. Aparece UI/papel com nome do cliente.
2. Papel mostra receita pedida.
3. Papel fica em algum lugar do foodtruck ou na tela.
4. Ao entregar, o pedido e validado.

### Pedido Em Papel

Visual:

- papel amarelado;
- texto grande;
- desenho/icone simples do prato;
- nome do cliente;
- um clipe ou fita prendendo na parede.

Exemplo:

```text
NINO
Pedido: Ovo Frito
Obs: "pode ser torto"
```

---

## 7. Receitas Da Demo

### 7.1 Ovo Frito

Objetivo:

Ser a receita tutorial.

Fluxo ideal:

```text
pegar ovo -> interagir com frigideira -> modo frigideira -> quebrar ovo -> fritar -> tirar no ponto -> colocar no prato -> entregar
```

Versao minima aceitavel:

```text
pegar ovo -> colocar na frigideira -> ovo visual aparece quebrado -> espera ponto -> tirar -> prato
```

Feedback obrigatorio:

- som de ovo quebrando;
- som de fritura;
- visual de ovo na frigideira;
- mudanca de cor/material no ponto;
- vapor/fumaca leve.

### 7.2 Cuscuz

Objetivo:

Mostrar identidade nordestina.

Fluxo ideal:

```text
pegar flocao/fuba -> abrir cuscuzeira -> despejar -> fechar -> esperar vapor -> tirar cuscuz -> prato -> entregar
```

Versao minima aceitavel:

```text
pegar fuba/flocao -> colocar na cuscuzeira -> esperar -> tirar cuscuz -> prato
```

Feedback obrigatorio:

- vapor forte;
- som de vapor;
- tampa/estado visual;
- cuscuz pronto bem legivel.

### 7.3 Omelete

Objetivo:

Ser a receita mais "legalzinha" da demo.

Fluxo ideal:

```text
ovo -> frigideira -> quebrar -> fritar -> balancar frigideira no meio -> vira omelete -> tirar -> prato
```

Versao minima aceitavel:

```text
ovo -> frigideira -> durante cozimento apertar/segurar acao de balancar -> resultado omelete
```

Feedback obrigatorio:

- som de ovo;
- som de frigideira mexendo;
- visual do ovo virando omelete;
- cliente reconhece se foi omelete ou ovo frito.

---

## 8. Estacoes Necessarias

### Frigideira

Usada para:

- ovo frito;
- omelete.

Precisa ter:

- modelo da frigideira;
- root para visual do ovo/omelete;
- particula de vapor/fritura;
- som de fritura;
- estado de cozimento;
- interacao para retirar;
- interacao extra para omelete.

### Cuscuzeira

Usada para:

- cuscuz.

Precisa ter:

- modelo da cuscuzeira;
- tampa, se der tempo;
- root para visual;
- particula de vapor;
- som de vapor;
- estado de cozimento.

### Prato

Usado para:

- montagem e entrega.

Precisa ter:

- modelo;
- root para comida;
- aceitar 1 item na demo;
- ser entregavel na janela.

### Janela De Entrega

Usada para:

- cliente aparecer;
- dialogar;
- entregar prato.

Precisa ter:

- trigger/area de entrega;
- ponto do cliente;
- ponto de camera/olhar;
- UI de dialogo.

### Lixeira

Opcional, mas recomendada se houver tempo.

Usada para:

- jogar casca de ovo;
- descartar comida ruim;
- dar personalidade ao foodtruck.

---

## 9. Foodtruck Da Demo

O foodtruck e quase o protagonista visual da demo.

Pelas imagens atuais, ele ja tem:

- silhueta externa boa;
- cor amarela forte;
- janela grande;
- interior funcional;
- Abigobaldo visivel;
- bancada, pia, geladeira e liquidificador.

O que falta:

- personalidade;
- decoracao;
- luz;
- objetos de vida;
- contraste visual;
- identidade de Abigobaldo.

### Decoracoes Necessarias

Prioridade alta:

- radio;
- mural/clip de pedidos;
- lixeira;
- luzinhas ou lampada quente;
- pano/toalha colorida;
- pequenos papeis/anotacoes;
- placa com nome "Abigobaldo's";
- adesivo/frase no foodtruck.

Prioridade media:

- plantinha;
- calendario;
- foto velha;
- colher/panela pendurada;
- caixa de ingredientes;
- potes;
- guardanapos.

Prioridade baixa:

- trofeus;
- quadros;
- adesivos extras;
- objetos escondidos.

### Frases/Adesivos Possiveis

- "Comida quente, coracao quentinho"
- "Abigobaldo's"
- "Hoje ninguem dorme de barriga vazia"
- "Cuscuz salva"
- "Pague com um sorriso"

---

## 10. Modelos 3D Necessarios

### Personagens

Obrigatorio:

- Abigobaldo polido.
- Cliente 1: Seu Zé.
- Cliente 2: Dona Lúcia.
- Cliente 3: Nino.
- Cliente 4: Marta.
- Cliente 5: Reclamão.

Versao realista para 3 dias:

- 1 corpo base de cliente.
- 5 variacoes por:
  - cor de roupa;
  - chapeu/cabelo;
  - barba;
  - acessorio;
  - altura/escala leve;
  - postura.

### Comidas

Obrigatorio:

- ovo inteiro;
- ovo quebrado na frigideira;
- casca de ovo;
- ovo frito pronto;
- omelete;
- fuba/flocao;
- cuscuz pronto.

Opcional:

- ovo queimado como material;
- cuscuz queimado como material;
- prato sujo/erro.

### Utensilios

Obrigatorio:

- frigideira;
- cuscuzeira;
- prato;
- pote de fuba/flocao;
- papel de pedido.

Opcional:

- espatula;
- colher;
- tampa da cuscuzeira separada;
- lixeira.

### Foodtruck/Decoracao

Obrigatorio:

- exterior retexturizado;
- interior retexturizado;
- radio;
- mural de pedidos;
- luz interna;
- placa/nome;
- lixeira.

Opcional:

- planta;
- fotos;
- calendario;
- adesivos;
- potes;
- pano.

---

## 11. Textura E Estilo

### Direcao

Cartoon low poly.

Regras:

- cores mais vivas;
- materiais foscos;
- comida bem legivel;
- poucos detalhes pequenos;
- formas exageradas;
- interior mais quente que exterior.

### Foodtruck

Exterior:

- amarelo principal;
- detalhes claros;
- placa preta/amarela;
- rodas simples;
- janela grande.

Interior:

- amarelo quente nas paredes;
- bancada marrom;
- metal cinza/azulado;
- objetos coloridos.

### Personagens

Devem parecer amigaveis, nao realistas.

Importante:

- olhos simples;
- silhueta clara;
- roupas diferentes;
- expressao facil de entender.

---

## 12. Iluminacao Da Demo

Objetivo:

Fazer o foodtruck parecer quente e acolhedor.

### Setup Minimo

- Luz direcional externa suave.
- Luz quente dentro do foodtruck.
- Luz na janela destacando cliente.
- Sombras leves ou desativadas se pesar.
- Skybox simples.

### Clima

Fim de tarde/noite inicial.

Exterior pode ser simples, mas o foodtruck deve parecer um ponto de calor no mundo.

---

## 13. Audio Necessario

### Musica/Radio

Obrigatorio:

- radio tocando musica ou audio escolhido por voce.
- som deve vir de dentro do foodtruck.

O radio pode alternar clipes se voce quiser:

- musica ambiente;
- chiado;
- vinheta;
- audio engracado;
- silencio curto.

### Sons De Interacao

Obrigatorio:

- pegar item;
- soltar item;
- clique de interacao;
- prato colocado;
- pedido recebido;
- entrega correta;
- entrega errada.

### Sons De Cozinha

Obrigatorio:

- ovo quebrando;
- fritura;
- vapor da cuscuzeira;
- comida pronta;
- queimando/fumaca.

### Vozes/Clientes

Minimo:

- som curto de fala tipo murmúrio;
- som positivo;
- som decepcionado.

Nao precisa dublagem completa.

---

## 14. UI Necessaria

### Menu Inicial

Botoes:

- Jogar Demo
- Creditos
- Sair

Visual:

- foodtruck no fundo ou imagem simples;
- titulo "Abigobaldo's";
- musica/radio tocando.

### HUD

Minimo:

- mira simples;
- texto de interacao;
- item na mao;
- pedido atual ou papel visivel.

### Dialogo

Caixa simples:

- nome do cliente;
- fala;
- tecla para avancar.

### Pedido

Papel ou UI:

- nome do cliente;
- receita;
- icone/desenho.

### Cozimento

Pode ser:

- barra pequena;
- texto "quase pronto";
- feedback visual/sonoro.

### Resultado

Mostrar:

- pessoas atendidas;
- conversas realizadas;
- pedidos corretos;
- pratos ruins/queimados;
- felicidade final;
- frase final.

Exemplo:

```text
NOITE ENCERRADA

3 pratos entregues
2 pessoas conversaram com Abigobaldo
Felicidade da noite: 78%

"Amanhã talvez tenha mais gente na janela."
```

---

## 15. Highlight / Selecionar Objetos

Voce disse que highlight estava irritando.

Alternativas para a demo:

### Opcao 1 - Sem contorno, apenas UI

Quando olhar para algo interativo:

- mira muda de ponto para maozinha;
- texto aparece: "Pegar Ovo", "Interagir", "Entregar".

Vantagem:

- simples;
- nao mexe em material;
- nao causa visual estranho.

### Opcao 2 - Outline So Em Objetos Grandes

Usar outline apenas para:

- estacoes;
- clientes;
- entrega.

Itens pequenos usam UI.

### Opcao 3 - Brilho/Sombra No Root

Em vez de highlight no material, colocar um pequeno circulo/sombra no chao/bancada abaixo do objeto selecionado.

Minha recomendacao para 3 dias:

**Use UI de texto + mira mudando. Corte highlight por enquanto.**

---

## 16. Sistemas Da Demo

### Obrigatorios

- Player movement/camera.
- Pegar/soltar/arremessar.
- Interacao por raycast.
- Dialogo linear.
- Cliente sequencial.
- Pedido simples.
- Receita simples.
- Cozimento por tempo.
- Empratamento.
- Entrega.
- Resultado final.
- Audio basico.
- Menu.

### Opcionais

- quebrar prato;
- quebrar ovo na parede;
- lixeira funcional;
- radio com troca de musica;
- papel fisico de pedido no mural;
- pequenas animacoes de cliente.

---

## 17. Plano De Desenvolvimento Em 3 Dias

## DIA 1 - PRODUCAO VISUAL E MODELOS

Objetivo:

Ter todos os assets visuais necessarios para a demo nao parecer vazia.

### Modelar / Ajustar

Foodtruck:

- retexturizar exterior;
- retexturizar interior;
- adicionar placa/nome;
- adicionar mural de pedidos;
- adicionar radio;
- adicionar lixeira;
- adicionar luz interna;
- adicionar 3 a 5 decoracoes pequenas.

Personagens:

- polir Abigobaldo;
- criar corpo base de cliente;
- criar 5 variacoes simples.

Comidas:

- ovo;
- ovo quebrado/gema;
- casca;
- ovo frito;
- omelete;
- fuba/flocao;
- cuscuz.

Utensilios:

- frigideira;
- cuscuzeira;
- prato;
- pote de fuba;
- papel pedido.

### Texturas

- paleta cartoon;
- materiais foscos;
- comida mais saturada;
- interior mais quente;
- exterior amarelo limpo e simpatico.

### Criterio De Fechamento Do Dia 1

No fim do dia 1, deve ser possivel abrir a cena e sentir:

> "Agora isso parece o foodtruck de alguem."

Nao precisa estar tudo programado.

---

## DIA 2 - CORE JOGAVEL

Objetivo:

Implementar a demo do inicio ao fim.

### Sistemas

- Menu inicial.
- Sequencia de 5 clientes.
- Dialogo linear.
- Pedido simples.
- UI de dialogo.
- UI/papel de pedido.
- Entrega na janela.
- Resultado final.

### Receitas

Implementar:

- ovo frito;
- cuscuz;
- omelete.

Versao minima:

- colocar ingrediente na estacao;
- iniciar timer;
- mudar visual/material;
- retirar;
- empratar;
- entregar.

Versao melhor:

- estacao com zoom fixo para frigideira/cuscuzeira.

### Cliente

Fluxo:

1. Seu Zé conversa.
2. Dona Lúcia conversa.
3. Nino pede ovo.
4. Marta pede cuscuz.
5. Reclamão pede omelete.

### Criterio De Fechamento Do Dia 2

No fim do dia 2, deve ser possivel jogar a demo inteira, mesmo feia ou sem som completo.

---

## DIA 3 - POLIMENTO, SOM E BUGFIX

Objetivo:

Fazer a demo parecer apresentavel.

### Audio

- radio funcionando;
- som de pegar;
- som de soltar;
- som de fritura;
- som de vapor;
- som de entrega;
- som de dialogo;
- som de erro/acerto.

### Visual

- luz quente interna;
- skybox/ambiente simples;
- particulas de fritura/vapor;
- UI legivel;
- menu bonito;
- tela final bonita;
- foodtruck com decoracoes finais.

### Performance

- trocar MeshCollider pequeno por collider simples;
- reduzir shadows/URP se FPS ruim;
- testar build fora do Editor;
- limitar objetos spawnados;
- checar escala dos prefabs principais.

### Bugfix

Testar:

- cliente 1 avanca dialogo;
- cliente 2 avanca dialogo;
- pedidos aparecem;
- entrega correta funciona;
- entrega errada mostra feedback;
- receitas nao resetam estado;
- prato recebe item no tamanho certo;
- tela final aparece;
- menu reinicia demo.

### Criterio De Fechamento Do Dia 3

Uma pessoa deve conseguir jogar sem voce explicar tudo.

---

## 18. Checklist Final Da Demo

### Gameplay

- [ ] Menu abre.
- [ ] Jogar Demo entra na cena.
- [ ] Player anda.
- [ ] Player pega objetos.
- [ ] Player interage.
- [ ] 5 clientes aparecem em sequencia.
- [ ] 2 clientes conversam.
- [ ] 3 clientes pedem comida.
- [ ] Dialogos avancam.
- [ ] Pedido aparece.
- [ ] Ovo frito pode ser feito.
- [ ] Cuscuz pode ser feito.
- [ ] Omelete pode ser feita.
- [ ] Prato recebe comida.
- [ ] Entrega valida pedido.
- [ ] Resultado final aparece.

### Visual

- [ ] Foodtruck exterior retexturizado.
- [ ] Foodtruck interior com personalidade.
- [ ] Radio visivel.
- [ ] Mural de pedidos visivel.
- [ ] Lixeira visivel.
- [ ] Luz interna quente.
- [ ] Clientes diferentes.
- [ ] Comidas legiveis.
- [ ] UI legivel.

### Audio

- [ ] Radio toca.
- [ ] Sons de item.
- [ ] Sons de cozinha.
- [ ] Sons de cliente/dialogo.
- [ ] Sons de entrega.

### Performance

- [ ] Build testada.
- [ ] FPS aceitavel.
- [ ] Sem erro vermelho no console.
- [ ] Colliders simples nos objetos principais.

---

## 19. Cortes De Emergencia

Se faltar tempo, cortar nesta ordem:

1. Prato quebravel.
2. Ovo quebravel na parede.
3. Lixeira funcional.
4. Mural fisico de pedido; usar UI simples.
5. Zoom fixo da cuscuzeira.
6. Zoom fixo da frigideira.
7. Omelete como minigame; virar apenas receita por tempo.
8. Cliente animado; deixar cliente parado.

Nao cortar:

- clientes;
- dialogo;
- pedido;
- entrega;
- resultado final;
- foodtruck com personalidade minima;
- sons basicos.

Essas coisas sao a demo.

---

## 20. Resultado Esperado

A demo ideal dura de 5 a 8 minutos.

Ela deve ter:

- uma abertura simples;
- um foodtruck simpatico;
- cinco encontros;
- tres pratos;
- uma tela final;
- sensacao de carinho.

Ela nao precisa ser grande. Precisa ser clara.

Mensagem final da demo:

> "Essa foi so uma noite. Amanhã talvez tenha mais gente precisando de um prato quente."

