# DIA 2 - CORE JOGAVEL DA DEMO

## Objetivo Do Dia

Fazer a demo existir do inicio ao fim.

No Dia 2, a prioridade nao e deixar bonito. A prioridade e ter o fluxo completo:

```text
Menu -> jogo -> cliente -> dialogo/pedido -> cozinhar -> empratar -> entregar -> proximo cliente -> resultado final
```

No fim do dia, a demo deve ser jogavel inteira, mesmo que ainda esteja com UI feia, sons faltando e animacoes simples.

Frase de fechamento do dia:

> "Ja da para jogar a demo inteira."

---

## 1. Menu Inicial

### O Que Significa Fazer

Criar uma tela inicial simples para abrir a demo com intencao, em vez de dar play direto na cena.

Nao precisa ser menu complexo. Precisa ter:

- titulo;
- botao de jogar;
- botao de sair;
- opcional: creditos.

### Funcionalidade Minima

Menu com:

- `Jogar Demo`: carrega a cena principal.
- `Sair`: fecha o jogo ou para Play Mode no Editor.

### Visual Minimo

- Fundo com cor quente ou imagem do foodtruck.
- Titulo `Abigobaldo's`.
- Botoes grandes.

### Pronto Quando

- Ao apertar Play, o menu aparece.
- Clicar em `Jogar Demo` entra no jogo.
- Clicar em `Sair` funciona ou pelo menos nao quebra.

---

## 2. Sequencia De Clientes

### O Que Significa Fazer

A demo tera 3 clientes em ordem fixa. Nada de fila procedural ainda.

Ordem:

1. Seu Ze conversa.
2. Nino pede ovo frito.
3. Marcia pede cuscuz.

O sistema precisa:

- mostrar um cliente na janela;
- tocar/mostrar dialogo;
- esperar o jogador avancar;
- criar pedido quando necessario;
- esperar entrega quando houver pedido;
- trocar para o proximo cliente;
- terminar a demo apos o terceiro.

### Implementacao Simples

Criar um `DemoCustomerSequence` ou equivalente.

Cada entrada da sequencia pode ter:

- prefab/modelo do cliente;
- nome;
- falas;
- pedido opcional;
- texto de acerto;
- texto de erro;
- se conversa apenas ou se pede comida.

### Pronto Quando

- Os 3 clientes aparecem na ordem certa.
- Um cliente some antes do proximo aparecer.
- Clientes de conversa nao pedem prato.
- Clientes de pedido aguardam entrega.
- Depois do terceiro cliente, aparece resultado final.

---

## 3. Sistema De Dialogo

### O Que Significa Fazer

Permitir que o jogador converse com clientes apertando uma tecla para avancar falas.

Na demo, dialogo nao precisa ter escolha. Ele pode ser linear.

### Funcionalidade Minima

- Mostrar nome do cliente.
- Mostrar fala atual.
- Apertar `E` avanca fala.
- Ao terminar falas, o sistema:
  - encerra conversa, se for cliente de dialogo;
  - cria pedido, se for cliente com pedido.

### Falas Obrigatorias

#### Seu Ze

- Pergunta se ali e onde estao dando comida.
- Pergunta o nome de Abigobaldo.
- Fica com vergonha de pedir.
- Diz que talvez volte depois.

#### Nino

- Ja chama Abigobaldo pelo nome.
- Diz que ouviu falar dele.
- Pede ovo frito.

#### Marcia

- Ja chama Abigobaldo pelo nome.
- Pede cuscuz quentinho.

### Pronto Quando

- O texto nao passa da tela.
- O jogador entende quem esta falando.
- O dialogo avanca sem travar.
- Ao terminar conversa, a sequencia continua.

---

## 4. Sistema De Pedido

### O Que Significa Fazer

Quando um cliente pede comida, o jogo precisa registrar qual prato ele quer e mostrar isso ao jogador.

Pedido nao e so texto: e o objetivo atual.

### Pedido Precisa Guardar

- nome do cliente;
- receita pedida;
- item/estado esperado;
- fala de sucesso;
- fala de erro.

### Pedidos Da Demo

#### Nino

- Pedido: ovo frito.
- Aceita: `FriedEgg` no ponto ou aceitavel.

#### Marcia

- Pedido: cuscuz.
- Aceita: `Cuscuz` no ponto ou aceitavel.

### Visual Do Pedido

Versao minima:

- UI no canto da tela com nome e prato.

Versao melhor:

- papelzinho com nome e prato, preso no mural.

### Pronto Quando

- Quando cliente pede, o jogador consegue ver o pedido atual.
- Pedido some ou muda depois da entrega.
- Entrega correta e incorreta sao reconhecidas.

---

## 5. Entrega Na Janela

### O Que Significa Fazer

Criar uma area/interacao onde o jogador entrega o prato ao cliente.

O jogador deve:

1. Preparar comida.
2. Colocar comida no prato.
3. Segurar prato.
4. Interagir na janela.
5. Receber feedback.

### Funcionalidade Minima

- `DeliveryZone` na janela.
- Se jogador segura prato, validar conteudo.
- Se nao segura prato, mostrar "Preciso entregar um prato".
- Se prato esta vazio, mostrar "O prato esta vazio".
- Se prato esta errado, cliente reage.
- Se prato esta certo, cliente agradece.

### Pronto Quando

- Nino aceita ovo frito.
- Marcia aceita cuscuz.
- Entrega errada nao trava a demo.
- Depois da entrega, vai para proximo cliente.

---

## 6. Receitas Da Demo

## 6.1 Ovo Frito

### O Que Significa Fazer

Permitir que ovo vire ovo frito na frigideira.

### Versao Minima

1. Jogador pega ovo.
2. Interage com frigideira.
3. Ovo some da mao.
4. Visual de ovo aparece na frigideira.
5. Timer inicia.
6. Quando pronto, material/estado muda.
7. Jogador tira ovo frito.
8. Coloca no prato.

### Versao Melhor

- Entrar em modo zoom da frigideira.
- Jogador quebra ovo.
- Casca aparece.
- Som de ovo quebrando.

### Pronto Quando

- Ovo nao volta para cru depois de pronto.
- Ovo pode ser retirado.
- Ovo pode ser empratado.
- Pedido do Nino aceita o prato.

## 6.2 Cuscuz

### O Que Significa Fazer

Permitir que fuba/flocao vire cuscuz na cuscuzeira.

### Versao Minima

1. Jogador pega fuba/flocao.
2. Interage com cuscuzeira.
3. Fuba some da mao.
4. Visual aparece na cuscuzeira.
5. Timer inicia.
6. Vapor aparece.
7. Quando pronto, vira cuscuz.
8. Jogador tira e emprata.

### Versao Melhor

- Abrir/fechar tampa.
- Vapor aumenta quando esta cozinhando.

### Pronto Quando

- Cuscuz fica pronto.
- Cuscuz pode ser retirado.
- Cuscuz pode ser empratado.
- Pedido da Marcia aceita o prato.

## 6.3 Omelete

### O Que Significa Fazer

Usar a frigideira para criar uma variacao do ovo. A omelete deve parecer mais interativa que ovo frito.

### Versao Minima

1. Jogador coloca ovo na frigideira.
2. Durante cozimento, jogador aperta uma acao de "balancar/mexer".
3. Receita muda para omelete.
4. Timer termina.
5. Jogador retira omelete.

### Versao Melhor

- Jogador pega a frigideira ou usa modo zoom.
- Precisa balancar no meio do cozimento.
- Se nao balancar, vira ovo frito.

### Pronto Quando

- E possivel fazer omelete de forma intencional.
- O jogador entende que precisa executar uma acao extra.
- Omelete pode ser feita como receita extra, mesmo sem cliente obrigatorio.

---

## 7. Empratamento

### O Que Significa Fazer

Comida pronta precisa ir para o prato antes da entrega.

Na demo, o prato pode comportar apenas 1 comida. Isso simplifica muito.

### Funcionalidade Minima

- Segurando prato, interagir com comida pronta move comida para prato.
- Segurando comida pronta, interagir com prato coloca comida no prato.
- Visual aparece no root do prato.
- Prato registra qual comida tem.

### Pronto Quando

- Ovo frito aparece no prato.
- Cuscuz aparece no prato.
- Omelete aparece no prato.
- O tamanho nao muda de forma estranha.
- Entrega le o conteudo correto.

---

## 8. Tela De Resultado

### O Que Significa Fazer

Encerrar a demo com uma tela que mostre que a noite teve impacto.

Nao e so "Game Over". E a recompensa emocional.

### Dados Minimos

- clientes atendidos;
- conversas feitas;
- pratos entregues;
- pratos corretos;
- felicidade final;
- frase final.

### Exemplo

```text
NOITE ENCERRADA

1 pessoa conversou com Abigobaldo
2 pratos foram entregues
Felicidade da noite: 82%

"Amanha talvez tenha mais gente na janela."
```

### Pronto Quando

- Aparece depois do terceiro cliente.
- Mostra pelo menos 3 informacoes.
- Tem botao para voltar ao menu ou reiniciar.

---

## 9. Ordem De Implementacao Do Dia 2

### Primeiro Bloco - Estrutura

1. Menu inicial.
2. Cena principal carregando.
3. Sequencia de clientes.
4. Dialogo linear.

Sem isso, nao existe demo.

### Segundo Bloco - Pedido E Entrega

1. Pedido atual.
2. UI/papel simples.
3. DeliveryZone.
4. Validacao de prato.
5. Troca de cliente apos entrega.

Sem isso, cozinhar nao tem objetivo.

### Terceiro Bloco - Receitas

1. Ovo frito.
2. Cuscuz.
3. Omelete.

Sem isso, pedidos nao podem ser completados.

### Quarto Bloco - Resultado

1. Contar entregas.
2. Contar acertos.
3. Mostrar tela final.

Sem isso, a demo nao fecha.

---

## 10. Cortes De Emergencia Do Dia 2

Se estiver atrasado, corte nesta ordem:

1. Modo zoom fixo.
2. Papel fisico no mural, use UI simples.
3. Casca de ovo.
4. Tampa da cuscuzeira.
5. Animacao de cliente.
6. Receita omelete com movimento fisico; use botao simples.

Nao cortar:

- clientes;
- dialogo;
- pedido;
- entrega;
- resultado;
- pelo menos ovo e cuscuz funcionando.

---

## Criterio De Fechamento Do Dia 2

No fim do Dia 2, voce deve conseguir jogar assim:

1. Abrir menu.
2. Clicar em jogar.
3. Conversar com Seu Ze.
4. Fazer ovo para Nino.
5. Fazer cuscuz para Marcia.
6. Entregar os pratos.
7. Ver tela final.

Se isso acontecer, Dia 2 venceu.

---

## Checklist Final Do Dia 2

### Menu

- [ ] Menu inicial aparece.
- [ ] Botao `Jogar Demo` funciona.
- [ ] Botao `Sair` ou `Voltar` existe.

### Clientes

- [ ] Seu Ze aparece.
- [ ] Nino aparece.
- [ ] Marcia aparece.
- [ ] Clientes aparecem na ordem correta.
- [ ] Cliente anterior some antes do proximo.

### Dialogo

- [ ] Caixa de dialogo aparece.
- [ ] Nome do cliente aparece.
- [ ] Falas avancam com E.
- [ ] Clientes de conversa terminam sem pedido.
- [ ] Clientes de pedido criam pedido.

### Pedido

- [ ] Pedido de ovo frito aparece.
- [ ] Pedido de cuscuz aparece.
- [ ] Omelete existe como receita extra/alternativa.
- [ ] Pedido atual e visivel.
- [ ] Pedido some/muda depois da entrega.

### Receitas

- [ ] Ovo frito pode ser feito.
- [ ] Cuscuz pode ser feito.
- [ ] Omelete pode ser feita.
- [ ] Comida muda para estado pronto.
- [ ] Comida pode ser retirada da estacao.
- [ ] Comida nao reseta para cru ao ser retirada.

### Prato E Entrega

- [ ] Prato aceita ovo frito.
- [ ] Prato aceita cuscuz.
- [ ] Prato aceita omelete.
- [ ] DeliveryZone reconhece prato.
- [ ] Entrega correta funciona.
- [ ] Entrega errada mostra feedback.
- [ ] Sequencia continua apos entrega.

### Resultado

- [ ] Tela final aparece depois do terceiro cliente.
- [ ] Resultado mostra conversas.
- [ ] Resultado mostra pratos entregues.
- [ ] Resultado mostra felicidade/acertos.
- [ ] Existe botao para reiniciar ou voltar ao menu.
