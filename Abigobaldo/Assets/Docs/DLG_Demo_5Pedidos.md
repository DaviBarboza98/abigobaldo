# DLG — Demo: Um Dia de Comida

Este é o roteiro-fonte da demo. Ele não é código: pode mudar textos, opções e
efeitos aqui antes de o sistema de diálogo ser implementado.

## Como ler/escrever

- `[NÓ: ID]` é uma fala/trecho.
- `FALA:` vai para o Console.
- `OPÇÕES:` aceita de `1` a `4`. O jogador aperta a tecla correspondente.
- `SEM OPÇÕES` significa: mostrar a fala e avançar para `PRÓXIMO`.
- `EFEITO:` altera o estado salvo do dia.
- `PEDIDO:` abre a fase de cozinha. O NPC fica até receber comida válida,
  recusar a ajuda ou o pedido ser encerrado.
- Textos entre `[COLCHETES]` são lugares livres para editar/depois expandir.

## Estado global

```text
RESPEITO_NINO = 0
RESPEITO_MARCIA = 0
RESPEITO_SEUZE = 0

HISTORIA_NINO = false
HISTORIA_MARCIA = false
HISTORIA_SEUZE = false
HISTORIAS_OUVIDAS = quantidade de HISTORIA_* que forem true

PEDIDOS_CONCLUIDOS = 0
PEDIDOS_PERDIDOS = 0
PRATOS_NO_PONTO = 0
QUALIDADE_PESSIMA = 0        # Queimado ou Carvão
DESRESPEITO_GRAVE = 0

NINO_PRIMEIRO_PEDIDO = ""
CLIENTE_QUARTO_PEDIDO = ""
CLIENTE_ULTIMO_PEDIDO = ""
```

## Regras de entrega — iguais para os três

Comidas disponíveis: `Cuscuz`, `Ovo Frito`, `Omelete`, `Milho Assado`.

| Entrega | Efeito | Reação genérica |
|---|---:|---|
| Pedido certo, No ponto | `RESPEITO +2`, `PRATOS_NO_PONTO +1` | “Tá bom de verdade. Obrigado.” |
| Pedido certo, Quase no ponto | `RESPEITO +1` | “Faltou pouco, mas dá pra comer. Obrigado.” |
| Pedido certo, Passado | `RESPEITO +0` | “Passou um pouco. Mas eu aceito.” |
| Pedido certo, Cru | `RESPEITO -1` | “Isso ainda está cru. Consegue fazer de novo?” |
| Pedido certo, Queimado | `RESPEITO -2`, `QUALIDADE_PESSIMA +1` | “Queimou. Eu estava com fome, não pedindo carvão.” |
| Carvão | `RESPEITO -3`, `QUALIDADE_PESSIMA +1` | “Não. Isso eu não vou comer.” |
| Comida errada em pedido específico | `RESPEITO -1` | “Não foi isso que eu pedi. Posso esperar o pedido certo.” |

`Qualquer comida` aceita qualquer uma das quatro, mas a qualidade ainda conta.
Quando a entrega for aceita: `PEDIDOS_CONCLUIDOS +1` e o NPC sai.

## Pontuação — máximo 100

```text
COMIDA (0 a 50):
  cada pedido aceito: No ponto +10 | Quase +8 | Passado +5 | Cru +2 | Queimado/Carvão +0
  comida errada não fecha pedido e não soma.

AJUDA (0 a 20):
  cada pedido concluído +4.

VÍNCULO (0 a 30):
  história de cada personagem +6 (máximo 18)
  resposta respeitosa relevante +1 ou +2 (máximo 12)
  respostas agressivas reduzem pontos.

NOTA: A 90–100 | B 75–89 | C 60–74 | D 40–59 | F 0–39
```

## Escolha da possibilidade final

```text
POSSIBILIDADE 1 — RESPEITO
  5 pedidos concluídos; 5 no ponto; 3 histórias ouvidas;
  respeito: Nino >= 6, Márcia >= 5, SeuZe >= 5; desrespeito grave = 0.

POSSIBILIDADE 4 — PORTA FECHADA
  DESRESPEITO_GRAVE >= 2, ou PEDIDOS_CONCLUIDOS <= 2,
  ou QUALIDADE_PESSIMA >= 3, ou dois clientes com respeito <= -3.

POSSIBILIDADE 3 — SÓ COMIDA
  5 pedidos concluídos; 5 no ponto; nenhuma história ouvida;
  sem desrespeito grave.

POSSIBILIDADE 2 — FOI UM DIA
  Todo resultado que não seja 1, 3 ou 4.
```

---

# APARIÇÃO 1 — 08:00 — NINO (obrigatória)

## [NÓ: NINO_01]

**FALA — Nino:** “Moço... isso aí é comida?”

**OPÇÕES**

1. “É. Você está com fome?”
   - `EFEITO: RESPEITO_NINO +1`
   - `PRÓXIMO: NINO_02`
2. “Quer alguma coisa?”
   - `PRÓXIMO: NINO_02`
3. “Você tem dinheiro?”
   - `EFEITO: RESPEITO_NINO -2`
   - `PRÓXIMO: NINO_SEM_DINHEIRO`
4. “Tô ocupado.”
   - `EFEITO: RESPEITO_NINO -2`
   - `PRÓXIMO: NINO_RECUA`

## [NÓ: NINO_RECUA]

**FALA — Nino:** “Tá. Desculpa.”

**OPÇÕES**

1. “Espera. Pode falar.” → `NINO_02`
2. “Você está com fome?” → `NINO_02`
3. “Foi mal. Eu estava distraído.” `EFEITO: RESPEITO_NINO +1` → `NINO_02`
4. “Pode ir.” `EFEITO: RESPEITO_NINO -2; PEDIDOS_PERDIDOS +1` → `NINO_SAI`

## [NÓ: NINO_SEM_DINHEIRO]

**FALA — Nino:** “Não.”

**OPÇÕES**

1. “Eu não perguntei se você tinha dinheiro.” `EFEITO: RESPEITO_NINO +2` → `NINO_03`
2. “Não precisa pagar.” `EFEITO: RESPEITO_NINO +1` → `NINO_03`
3. “Então complica.” `EFEITO: RESPEITO_NINO -1` → `NINO_03`
4. “Então não dá.” `EFEITO: RESPEITO_NINO -3; PEDIDOS_PERDIDOS +1` → `NINO_SAI`

## [NÓ: NINO_02]

**FALA — Nino:** “Tô. Mas eu não tenho dinheiro.”

**OPÇÕES**

1. “Aqui não precisa pagar.” `EFEITO: RESPEITO_NINO +2` → `NINO_03`
2. “Tudo bem. O que você quer comer?” → `NINO_03`
3. “Eu também tenho conta para pagar.” `EFEITO: RESPEITO_NINO -1` → `NINO_03`
4. “Então não posso fazer nada.” `EFEITO: RESPEITO_NINO -3; PEDIDOS_PERDIDOS +1` → `NINO_SAI`

## [NÓ: NINO_03]

**FALA — Nino:** “Pode ser qualquer coisa. Sério.”

**OPÇÕES**

1. “Antes, qual é seu nome?” → `NINO_NOME`
2. “Beleza. Espera aí.” → `PEDIDO_NINO_1`
3. “Você fica sempre por aqui?” → `NINO_ONDE_FICA`
4. “Então não reclama do que vier.” `EFEITO: RESPEITO_NINO -2` → `PEDIDO_NINO_1`

## [NÓ: NINO_NOME]

**FALA — Nino:** “Nino.”

**SEM OPÇÕES**

**FALA — Abigobaldo:** “Prazer, Nino.”

`PRÓXIMO: NINO_ONDE_FICA`

## [NÓ: NINO_ONDE_FICA]

**FALA — Nino:** “Agora eu fico. Mais ali em cima.”

**OPÇÕES**

1. “Se quiser contar como veio parar aqui, eu escuto.” → `HISTORIA_NINO`
2. “Não precisa falar se não quiser.” → `HISTORIA_NINO`
3. “Entendi. Vou fazer sua comida.” → `PEDIDO_NINO_1`
4. “Fez alguma besteira?” `EFEITO: RESPEITO_NINO -2` → `PEDIDO_NINO_1`

## [NÓ: HISTORIA_NINO]

**FALA — Nino:** “Eu trabalhava num depósito. Dividia um quarto com meu primo.”

**SEM OPÇÕES**

**FALA — Nino:** “Ele foi embora da cidade. Depois cortaram gente no depósito. Eu tentei segurar o aluguel sozinho... não consegui.”

**OPÇÕES**

1. “E desde então você está se virando por aqui?” `EFEITO: HISTORIA_NINO = true; RESPEITO_NINO +2` → `NINO_INDICA_MARCIA`
2. “Sua família sabe?” `EFEITO: HISTORIA_NINO = true; RESPEITO_NINO +1` → `NINO_INDICA_MARCIA`
3. “Você devia ter se planejado melhor.” `EFEITO: HISTORIA_NINO = true; RESPEITO_NINO -3; DESRESPEITO_GRAVE +1` → `PEDIDO_NINO_1`
4. “Tá. E o que você quer comer?” `EFEITO: HISTORIA_NINO = true` → `PEDIDO_NINO_1`

## [NÓ: NINO_INDICA_MARCIA]

**FALA — Nino:** “Tem uma mulher ali mais pra cima. Márcia. Ela também está com fome.”

**SEM OPÇÕES**

`EFEITO: RESPEITO_NINO +1`

`PRÓXIMO: PEDIDO_NINO_1`

## [PEDIDO: PEDIDO_NINO_1]

**FALA — Nino:** “Qualquer coisa está bom.”

```text
PEDIDO: QUALQUER COMIDA
ACEITA: Cuscuz | Ovo Frito | Omelete | Milho Assado
SALVAR: NINO_PRIMEIRO_PEDIDO = comida_entregue
```

Após entrega aceita: `PRÓXIMO: NINO_1_AGRADECE`.

## [NÓ: NINO_1_AGRADECE]

**FALA — Nino:** “Obrigado mesmo.”

**SEM OPÇÕES**

`NPC SAI. PRÓXIMA APARIÇÃO: MARCIA_01`

## [NÓ: NINO_SAI]

**FALA — Nino:** “Tá bom. Boa manhã.”

**SEM OPÇÕES**

`NPC SAI. PRÓXIMA APARIÇÃO: MARCIA_01`

---

# APARIÇÃO 2 — 10:30 — MÁRCIA

## [NÓ: MARCIA_01]

**FALA — Márcia:** “Bom dia. Isso aí é comida de verdade ou uma armadilha muito bem iluminada?”

**OPÇÕES**

1. “É comida. Está com fome?” `EFEITO: RESPEITO_MARCIA +1` → `MARCIA_02`
2. “Você é a Márcia?” `EFEITO: RESPEITO_MARCIA +1` → `MARCIA_NINO_FALOU`
3. “Depende. Você tem dinheiro?” `EFEITO: RESPEITO_MARCIA -2` → `MARCIA_02`
4. “Não estou atendendo.” `EFEITO: RESPEITO_MARCIA -2` → `MARCIA_RECUA`

## [NÓ: MARCIA_NINO_FALOU]

**FALA — Márcia:** “Então o Nino falou de mim. Ele fala pouco. Isso é quase uma carta de recomendação.”

**SEM OPÇÕES**

`PRÓXIMO: MARCIA_02`

## [NÓ: MARCIA_RECUA]

**FALA — Márcia:** “Tudo bem. Eu consigo ouvir um 'não' em quatro idiomas e com três tipos de porta fechando.”

**OPÇÕES**

1. “Espera. Está com fome?” → `MARCIA_02`
2. “Foi mal, pode falar.” `EFEITO: RESPEITO_MARCIA +1` → `MARCIA_02`
3. “Tenho cuscuz se quiser.” → `PEDIDO_MARCIA_1`
4. “É isso.” `EFEITO: RESPEITO_MARCIA -2; PEDIDOS_PERDIDOS +1` → `MARCIA_SAI`

## [NÓ: MARCIA_02]

**FALA — Márcia:** “Estou. Bastante. Mas consigo conversar enquanto isso, se você tiver sorte.”

**OPÇÕES**

1. “O que você quer comer?” → `PEDIDO_MARCIA_1`
2. “Como você veio parar por aqui?” → `HISTORIA_MARCIA`
3. “Você conhece o Nino?” → `MARCIA_NINO`
4. “Todo mundo quer alguma coisa.” `EFEITO: RESPEITO_MARCIA -2` → `PEDIDO_MARCIA_1`

## [NÓ: MARCIA_NINO]

**FALA — Márcia:** “Conheço. Ele acha que pedir ajuda é atrapalhar. Eu acho que ele pede desculpa até para uma cadeira quando esbarra nela.”

**OPÇÕES**

1. “Ele parece preocupado.” `EFEITO: RESPEITO_MARCIA +1` → `MARCIA_02`
2. “Ele falou que você estava com fome.” → `MARCIA_02`
3. “Ele não é problema meu.” `EFEITO: RESPEITO_MARCIA -2` → `MARCIA_02`
4. “E você?” → `HISTORIA_MARCIA`

## [NÓ: HISTORIA_MARCIA]

**FALA — Márcia:** “Eu tinha uma lojinha de costura. Pequena, mas era minha. Quando fechou, eu fui adiando tudo: aluguel, conta, pedido de ajuda.”

**SEM OPÇÕES**

**FALA — Márcia:** “Quando percebi, estava carregando minhas coisas numa mochila e fingindo que era temporário.”

**OPÇÕES**

1. “Você não precisa fingir nada aqui.” `EFEITO: HISTORIA_MARCIA = true; RESPEITO_MARCIA +2` → `PEDIDO_MARCIA_1`
2. “Sinto muito.” `EFEITO: HISTORIA_MARCIA = true; RESPEITO_MARCIA +1` → `PEDIDO_MARCIA_1`
3. “E sua família?” `EFEITO: HISTORIA_MARCIA = true` → `PEDIDO_MARCIA_1`
4. “Certo. Vai querer o quê?” `EFEITO: HISTORIA_MARCIA = true` → `PEDIDO_MARCIA_1`

## [PEDIDO: PEDIDO_MARCIA_1]

**FALA — Márcia:** “Cuscuz. Se tiver cuscuz, eu quero cuscuz. Comida simples não precisa fingir que é pouca coisa.”

```text
PEDIDO: CUSCUZ
```

Após entrega aceita: `PRÓXIMO: MARCIA_1_AGRADECE`.

## [NÓ: MARCIA_1_AGRADECE]

**FALA — Márcia:** “Obrigada. E, só para deixar claro: eu teria feito um discurso maior, mas estou ocupada comendo.”

**SEM OPÇÕES**

`NPC SAI. PRÓXIMA APARIÇÃO: SEUZE_01`

## [NÓ: MARCIA_SAI]

**FALA — Márcia:** “Tudo bem. Espero que a manhã melhore para nós dois.”

**SEM OPÇÕES**

`NPC SAI. PRÓXIMA APARIÇÃO: SEUZE_01`

---

# APARIÇÃO 3 — 14:00 — SEUZÉ

## [NÓ: SEUZE_01]

**FALA — SeuZe:** “Você é o cozinheiro?”

**OPÇÕES**

1. “Sou. Está com fome?” `EFEITO: RESPEITO_SEUZE +1` → `SEUZE_02`
2. “Quem quer saber?” `EFEITO: RESPEITO_SEUZE -1` → `SEUZE_02`
3. “Márcia falou de mim?” → `SEUZE_02`
4. “Se veio pedir comida, fala logo.” `EFEITO: RESPEITO_SEUZE -2` → `SEUZE_02`

## [NÓ: SEUZE_02]

**FALA — SeuZe:** “Quero saber se tem ovo. O resto eu descubro depois.”

**OPÇÕES**

1. “Tem. Frito?” → `PEDIDO_SEUZE_1`
2. “Tem. Qual é seu nome?” → `SEUZE_NOME`
3. “Como o senhor veio parar por aqui?” → `HISTORIA_SEUZE`
4. “Só isso?” `EFEITO: RESPEITO_SEUZE -1` → `PEDIDO_SEUZE_1`

## [NÓ: SEUZE_NOME]

**FALA — SeuZe:** “SeuZe. Não precisa chamar de senhor; eu ainda não virei mobília.”

**OPÇÕES**

1. “Prazer, SeuZe.” `EFEITO: RESPEITO_SEUZE +1` → `PEDIDO_SEUZE_1`
2. “Por que esse nome?” → `HISTORIA_SEUZE`
3. “Certo. Ovo frito?” → `PEDIDO_SEUZE_1`
4. “Tá bom.” → `PEDIDO_SEUZE_1`

## [NÓ: HISTORIA_SEUZE]

**FALA — SeuZe:** “Eu era pedreiro. Trabalhei em obra desde moleque. Quando minhas costas pararam, a obra também parou de me chamar.”

**SEM OPÇÕES**

**FALA — SeuZe:** “Aí você vai perdendo diária, quarto, endereço. Não acontece tudo em um dia. Esse é o truque.”

**OPÇÕES**

1. “O senhor não devia ter passado por isso sozinho.” `EFEITO: HISTORIA_SEUZE = true; RESPEITO_SEUZE +2` → `PEDIDO_SEUZE_1`
2. “Obrigado por contar.” `EFEITO: HISTORIA_SEUZE = true; RESPEITO_SEUZE +1` → `PEDIDO_SEUZE_1`
3. “Ainda dá para trabalhar?” `EFEITO: HISTORIA_SEUZE = true` → `PEDIDO_SEUZE_1`
4. “Então quer o ovo ou não?” `EFEITO: HISTORIA_SEUZE = true; RESPEITO_SEUZE -1` → `PEDIDO_SEUZE_1`

## [PEDIDO: PEDIDO_SEUZE_1]

**FALA — SeuZe:** “Ovo frito. A gema pode ficar mole. O mundo já está duro o bastante.”

```text
PEDIDO: OVO FRITO
```

Após entrega aceita: `PRÓXIMO: SEUZE_1_AGRADECE`.

## [NÓ: SEUZE_1_AGRADECE]

**FALA — SeuZe:** “Tá bom. Não se acostume com elogio; ele pesa.”

**SEM OPÇÕES**

`PRÓXIMA APARIÇÃO: escolher NINO_02_VISITA se HISTORIAS_OUVIDAS >= 2 ou RESPEITO_NINO >= 3; senão MARCIA_02_VISITA.`

---

# APARIÇÃO 4 — 17:30 — retorno variável

## [NÓ: NINO_02_VISITA]

**FALA — Nino:** “Oi... eu voltei. Se ainda tiver alguma coisa.”

**OPÇÕES**

1. “Pode chegar, Nino.” `EFEITO: RESPEITO_NINO +1` → `PEDIDO_NINO_2`
2. “Está com fome de novo?” → `PEDIDO_NINO_2`
3. “Você voltou.” → `PEDIDO_NINO_2`
4. “O que foi agora?” `EFEITO: RESPEITO_NINO -2` → `PEDIDO_NINO_2`

## [PEDIDO: PEDIDO_NINO_2]

**FALA — Nino:** “Omelete. Se tiver.”

```text
PEDIDO: OMELETE
```

Após entrega aceita: `PRÓXIMO: QUARTO_FIM`.

## [NÓ: MARCIA_02_VISITA]

**FALA — Márcia:** “Voltei. Eu sei, eu sei: uma pessoa comum teria vergonha. Ainda bem que eu não sou comum.”

**OPÇÕES**

1. “Quer comer?” → `PEDIDO_MARCIA_2`
2. “Pode chegar.” `EFEITO: RESPEITO_MARCIA +1` → `PEDIDO_MARCIA_2`
3. “Como foi sua tarde?” → `MARCIA_TARDE`
4. “Você de novo?” `EFEITO: RESPEITO_MARCIA -1` → `PEDIDO_MARCIA_2`

## [NÓ: MARCIA_TARDE]

**FALA — Márcia:** “Longa. Mas eu encontrei o Nino, então não foi uma tarde completamente perdida.”

**SEM OPÇÕES**

`PRÓXIMO: PEDIDO_MARCIA_2`

## [PEDIDO: PEDIDO_MARCIA_2]

**FALA — Márcia:** “Dessa vez você escolhe. Me surpreende, mas comestivelmente.”

```text
PEDIDO: QUALQUER COMIDA
ACEITA: Cuscuz | Ovo Frito | Omelete | Milho Assado
```

Após entrega aceita: `PRÓXIMO: QUARTO_FIM`.

## [NÓ: QUARTO_FIM]

**FALA — Console:** “[O sol começa a baixar. O foodtruck acende suas luzes.]”

**SEM OPÇÕES**

`PRÓXIMA APARIÇÃO: ÚLTIMA — 20:00.`

---

# APARIÇÃO 5 — 20:00 — noite

Escolha quem retorna:

```text
Se DESRESPEITO_GRAVE >= 1 ou RESPEITO_SEUZE <= RESPEITO_MARCIA:
  CLIENTE_ULTIMO_PEDIDO = "SeuZe"
Senão:
  CLIENTE_ULTIMO_PEDIDO = "Marcia"
```

## [NÓ: SEUZE_FINAL]

**FALA — SeuZe:** “Ainda tem comida?”

**OPÇÕES**

1. “Tem. O que o senhor quer?” `EFEITO: RESPEITO_SEUZE +1` → `PEDIDO_SEUZE_FINAL`
2. “Tenho milho.” → `PEDIDO_SEUZE_FINAL`
3. “Agora resolveu voltar?” `EFEITO: RESPEITO_SEUZE -2` → `PEDIDO_SEUZE_FINAL`
4. “Estou fechando.” `EFEITO: PEDIDOS_PERDIDOS +1; RESPEITO_SEUZE -2` → `FINAL_AVALIAR`

## [PEDIDO: PEDIDO_SEUZE_FINAL]

**FALA — SeuZe:** “Milho assado.”

```text
PEDIDO: MILHO ASSADO
```

Após entrega aceita: `PRÓXIMO: FINAL_AVALIAR`.

## [NÓ: MARCIA_FINAL]

**FALA — Márcia:** “Boa noite, Abigobaldo. A cidade fica mais honesta de noite: todo mundo admite que está cansado.”

**OPÇÕES**

1. “Ainda está com fome?” `EFEITO: RESPEITO_MARCIA +1` → `PEDIDO_MARCIA_FINAL`
2. “Pode pedir.” → `PEDIDO_MARCIA_FINAL`
3. “Você fala demais.” `EFEITO: RESPEITO_MARCIA -1` → `PEDIDO_MARCIA_FINAL`
4. “Estou fechando.” `EFEITO: PEDIDOS_PERDIDOS +1; RESPEITO_MARCIA -2` → `FINAL_AVALIAR`

## [PEDIDO: PEDIDO_MARCIA_FINAL]

**FALA — Márcia:** “Milho assado. Para terminar o dia com algo simples e quente.”

```text
PEDIDO: MILHO ASSADO
```

Após entrega aceita: `PRÓXIMO: FINAL_AVALIAR`.

---

# FINAIS

## [FINAL: POSSIBILIDADE_1 — RESPEITO]

**FALA — Console:** “A noite chega, mas os três ficam alguns minutos perto do foodtruck.”

**FALA — Nino:** “Amanhã você vai estar aqui?”

**FALA — Márcia:** “Ele vai. Agora tem reputação a sustentar.”

**FALA — SeuZe:** “Não enche. Só agradece.”

**FALA — Console:** “Cinco refeições. Três histórias. Ninguém precisou fingir que a fome era pouca coisa.”

## [FINAL: POSSIBILIDADE_2 — FOI UM DIA]

**FALA — Console:** “O dia termina melhor do que começou, mas não perfeitamente.”

**FALA — Márcia:** “Obrigada pela comida.”

**FALA — SeuZe:** “Amanhã a gente vê.”

**FALA — Console:** “Você ajudou. Só não conseguiu alcançar todo mundo do mesmo jeito.”

## [FINAL: POSSIBILIDADE_3 — SÓ COMIDA]

**FALA — Console:** “Cinco pratos bons. Cinco pessoas menos famintas — contando Abigobaldo.”

**FALA — SeuZe:** “Você cozinha bem.”

**FALA — Console:** “Abigobaldo sabe que Nino é tímido, Márcia fala muito e SeuZe reclama. Mas não sabe o que cada um perdeu antes de chegar à rua.”

**FALA — Abigobaldo:** “Pelo menos ninguém ficou com fome.”

## [FINAL: POSSIBILIDADE_4 — PORTA FECHADA]

**FALA — Console:** “A cozinha ainda tem comida. Alguns ingredientes nem foram usados.”

**FALA — Console:** “Do outro lado da rua, Nino, Márcia e SeuZe ficam juntos. Nenhum olha para o foodtruck.”

**FALA — Abigobaldo:** “Eu tinha comida.”

**FALA — Console:** “Ter comida nunca foi a mesma coisa que ajudar.”

## Final especial 4 — NINGUÉM COMEU

Condição: `PEDIDOS_CONCLUIDOS = 0`.

**FALA — Nino:** “Você ainda está ocupado?”

Opção final `4. “Estou fechando.”`

**FALA — Console:** “A cozinha termina o dia quase igual começou. Cheia. E inútil.”
