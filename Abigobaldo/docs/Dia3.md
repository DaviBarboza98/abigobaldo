# DIA 3 - POLIMENTO, SOM, PERFORMANCE E BUGFIX

## Objetivo Do Dia

Fazer a demo parecer apresentavel para outra pessoa jogar.

No Dia 3, nao e hora de criar sistema novo grande. E hora de pegar o que existe e deixar claro, gostoso e estavel.

Frase de fechamento do dia:

> "Uma pessoa consegue jogar sem eu explicar tudo."

---

## 1. Regra Principal Do Dia 3

Nao adicionar feature grande.

O Dia 3 existe para:

- colocar som;
- melhorar luz;
- deixar UI legivel;
- corrigir bugs;
- testar build;
- melhorar performance;
- ajustar escala/prefabs;
- garantir que a demo fecha.

Se surgir ideia nova, anote para depois. Nao implemente se ameacar a demo.

---

## 2. Audio

### O Que Significa Fazer

Audio e o que vai fazer o jogo parar de parecer morto.

Mesmo com modelos simples, som certo da vida:

- cliente falando;
- ovo fritando;
- vapor;
- radio;
- entrega correta.

### Radio

Fazer:

- Criar objeto `Radio` no foodtruck.
- Adicionar `AudioSource`.
- Tocar uma musica/audio em loop.
- Volume baixo o suficiente para nao atrapalhar dialogo.
- Opcional: trocar faixa ou tocar chiado.

Pronto quando:

- Ao iniciar a demo, o radio toca.
- O som parece vir do foodtruck.
- O volume nao irrita.

### Sons De Interacao

Adicionar sons para:

- pegar item;
- soltar item;
- arremessar;
- interagir;
- colocar item em estacao;
- colocar comida no prato.

O que significa:

Cada acao importante deve dar resposta imediata. Se o jogador clica e nada soa, parece que o jogo nao entendeu.

Pronto quando:

- Pegar e soltar item tem som.
- Colocar item em frigideira/cuscuzeira tem som.
- Empratar tem som.

### Sons De Cozinha

Adicionar sons para:

- ovo quebrando;
- fritura;
- vapor;
- comida pronta;
- queimando/erro.

Pronto quando:

- Frigideira chiando enquanto cozinha.
- Cuscuzeira com vapor audivel.
- Comida pronta tem um sinal sonoro simples.

### Sons De Cliente

Adicionar:

- som curto de fala ao avancar dialogo;
- som positivo na entrega correta;
- som negativo ou decepcionado na entrega errada.

Nao precisa dublagem completa.

Pronto quando:

- Cliente nao parece mudo.
- Entrega correta da satisfacao.
- Entrega errada e clara.

---

## 3. Visual E Iluminacao

### O Que Significa Fazer

Fazer o foodtruck parecer quente, legivel e cartoon.

O objetivo nao e realismo. E leitura e clima.

### Luz Interna

Fazer:

- Uma luz quente dentro do foodtruck.
- Destacar bancada e estacoes.
- Evitar sombra dura demais.

Pronto quando:

- Interior parece acolhedor.
- Comidas sao visiveis.
- Objetos importantes nao ficam escuros.

### Luz Da Janela

Fazer:

- Cliente na janela deve ser bem visivel.
- Pode usar uma luz suave apontada para area do cliente.

Pronto quando:

- O jogador bate o olho e entende que tem alguem na janela.

### Ambiente Externo

Fazer:

- Skybox simples.
- Chao/rua simples.
- Cor externa mais fria que interior.

Pronto quando:

- O foodtruck parece um ponto quente no ambiente.
- O exterior nao rouba atencao.

### Particulas

Adicionar/ajustar:

- vapor da cuscuzeira;
- fumaca/fritura da frigideira;
- fumaca mais escura se queimar, se ja existir.

Pronto quando:

- Vapor aparece assim que a cuscuzeira cozinha.
- Frigideira da sinal visual enquanto cozinha.
- Particulas nao ficam rosa.
- Particulas nao exageram ao ponto de cobrir comida.

---

## 4. UI

### O Que Significa Fazer

UI deve explicar o jogo sem voce do lado.

O jogador precisa saber:

- com quem esta falando;
- qual pedido esta ativo;
- o que esta selecionado;
- quando comida esta pronta;
- se entregou certo ou errado;
- quando a demo acabou.

### Menu

Polir:

- titulo;
- botoes;
- fundo;
- musica/radio opcional.

Pronto quando:

- Menu parece intencional, nao placeholder cru.

### Dialogo

Polir:

- caixa legivel;
- nome do cliente;
- texto com tamanho bom;
- indicacao de apertar E para continuar.

Pronto quando:

- Texto nao corta.
- Nome do cliente aparece claro.
- Jogador entende como avancar.

### Pedido

Polir:

- pedido atual visivel;
- nome do cliente;
- nome ou icone do prato;
- papel/estilo visual, se der tempo.

Pronto quando:

- Jogador nunca fica perdido sobre o que precisa cozinhar.

### Cozimento

Polir:

- barra simples ou texto;
- feedback de "pronto";
- feedback de "queimando".

Pronto quando:

- Jogador entende quando tirar a comida.

### Resultado

Polir:

- tela final com dados;
- frase emocional;
- botao para menu/reiniciar.

Pronto quando:

- A demo termina com sensacao de fechamento.

---

## 5. Feedback Sem Highlight Irritante

### O Que Significa Fazer

Trocar ou reduzir highlight se ele estiver irritando.

Recomendacao para demo:

- Mira muda quando olha para algo interativo.
- Texto aparece: `Pegar Ovo`, `Interagir`, `Entregar`.
- Evitar contorno/material em todos os objetos.

### Pronto Quando

- Jogador sabe o que pode usar.
- Objetos nao ficam visualmente feios.
- Nao precisa configurar highlight em cada prefab pequeno.

---

## 6. Performance

### O Que Significa Fazer

Garantir que a demo rode aceitavel.

Nao tente otimizar abstratamente. Teste e corte o que pesa.

### Passos

1. Testar no Editor.
2. Fazer build.
3. Testar build.
4. Se FPS ruim, reduzir visual/fisica.

### Ajustes Rapidos

#### Colliders

Trocar em objetos pequenos:

- MeshCollider -> BoxCollider/SphereCollider/CapsuleCollider.

Prioridade:

- ovo;
- prato;
- fuba;
- cuscuz;
- frigideira;
- cuscuzeira;
- clientes, se tiverem colisao.

#### Escala

Checar:

- root dos prefabs principais em escala 1;
- modelos ajustados no import ou filho `Model`;
- prato e estacoes nao alterando escala da comida.

#### URP

Se FPS ruim:

- reduzir shadow distance;
- desligar HDR se nao usado;
- desligar additional lights se nao necessarias;
- reduzir MSAA;
- desligar Depth Texture se nao precisar.

#### Objetos Spawnados

Garantir:

- cacos/lixo somem com tempo, se existirem;
- nao spawna item infinito sem controle;
- cascas de ovo nao acumulam demais.

### Pronto Quando

- Build roda em FPS aceitavel.
- Sem travadas fortes.
- Sem erro vermelho no Console.

---

## 7. Bugfix Obrigatorio

### Teste Completo Da Demo

Jogar do comeco ao fim pelo menos 3 vezes.

Testar:

- caminho perfeito;
- entrega errada;
- deixar comida passar do ponto;
- apertar botoes fora de ordem;
- tentar entregar prato vazio;
- tentar pegar coisa enquanto segura outra;
- reiniciar demo.

### Bugs Que Nao Podem Ficar

- cliente travar dialogo;
- pedido nao aparecer;
- entrega nao validar;
- receita nao terminar;
- prato nao receber comida;
- tela final nao aparecer;
- console com erro vermelho;
- item ficando gigante/minusculo;
- comida resetando estado;
- FPS injogavel.

### Bugs Que Podem Ficar Se Necessario

- animacao simples demais;
- cliente parado;
- objeto decorativo atravessando parede;
- UI pouco bonita;
- som repetitivo;
- pequenas imperfeicoes de textura.

---

## 8. Build

### O Que Significa Fazer

Nao entregar so Play Mode. A demo precisa ser testada como build.

### Passos

1. Abrir Build Settings.
2. Garantir cena de menu e cena principal na lista.
3. Buildar para Windows.
4. Abrir o `.exe`.
5. Jogar do inicio ao fim.
6. Anotar bugs.
7. Corrigir o que bloquear.
8. Buildar de novo.

### Pronto Quando

- Build abre.
- Menu funciona.
- Demo termina.
- Nao tem erro fatal.

---

## 9. Ordem De Trabalho Do Dia 3

### Primeiro Bloco - Bugfix Do Core

Corrigir tudo que impede completar a demo.

Prioridade:

1. Dialogo.
2. Pedido.
3. Receita.
4. Entrega.
5. Resultado.

### Segundo Bloco - UI

Deixar a demo entendivel.

Prioridade:

1. Dialogo legivel.
2. Pedido visivel.
3. Feedback de entrega.
4. Tela final.

### Terceiro Bloco - Audio

Adicionar vida.

Prioridade:

1. Radio.
2. Interacao.
3. Fritura/vapor.
4. Entrega.
5. Cliente.

### Quarto Bloco - Visual/Performance

Deixar apresentavel e rodando.

Prioridade:

1. Luz interna.
2. Particulas.
3. Colliders.
4. URP.
5. Build.

---

## 10. Cortes De Emergencia Do Dia 3

Se faltar tempo, corte nesta ordem:

1. Prato quebravel.
2. Ovo quebravel na parede.
3. Lixeira funcional.
4. Radio com multiplas faixas.
5. Animacoes de cliente.
6. Papel fisico no mural.
7. Particulas avancadas.
8. Menu bonito, deixando menu simples.

Nao cortar:

- audio basico;
- dialogo legivel;
- pedido visivel;
- entrega funcionando;
- tela final;
- build testada.

---

## Criterio De Fechamento Do Dia 3

No fim do Dia 3:

- Uma pessoa consegue abrir a demo.
- Entende como jogar.
- Conversa com clientes.
- Prepara as receitas.
- Entrega os pratos.
- Ve resultado final.
- Nao precisa de voce explicando tudo.

---

## Checklist Final Do Dia 3

### Audio

- [ ] Radio toca.
- [ ] Som de pegar item.
- [ ] Som de soltar item.
- [ ] Som de interagir.
- [ ] Som de colocar item em estacao.
- [ ] Som de fritura.
- [ ] Som de vapor.
- [ ] Som de comida pronta.
- [ ] Som de entrega correta.
- [ ] Som de entrega errada.
- [ ] Som curto de dialogo/cliente.

### Visual

- [ ] Luz interna quente.
- [ ] Cliente visivel na janela.
- [ ] Exterior simples e legivel.
- [ ] Particula da frigideira funcionando.
- [ ] Particula da cuscuzeira funcionando.
- [ ] Nenhuma particula rosa.
- [ ] Comidas visiveis nas estacoes.
- [ ] Comidas visiveis no prato.

### UI

- [ ] Menu esta apresentavel.
- [ ] Dialogo legivel.
- [ ] Nome do cliente aparece.
- [ ] Pedido atual aparece.
- [ ] Feedback de entrega aparece.
- [ ] Cozimento tem feedback.
- [ ] Tela final aparece.
- [ ] Tela final tem botao de reiniciar/menu.

### Performance

- [ ] Build foi feita.
- [ ] Build foi testada.
- [ ] FPS aceitavel.
- [ ] Sem erro vermelho no Console.
- [ ] MeshColliders pequenos removidos ou reduzidos.
- [ ] Escalas principais conferidas.
- [ ] URP ajustado se necessario.

### Bugfix

- [ ] Demo jogada do inicio ao fim 3 vezes.
- [ ] Entrega correta testada.
- [ ] Entrega errada testada.
- [ ] Prato vazio testado.
- [ ] Comida passada/queimada testada.
- [ ] Reiniciar/voltar ao menu testado.
- [ ] Nenhum sistema principal trava.

### Entrega Final

- [ ] Cena salva.
- [ ] Prefabs salvos.
- [ ] Build final gerada.
- [ ] Nome da build definido.
- [ ] Pasta da build organizada.
- [ ] Ultima execucao testada.
