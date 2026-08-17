# Relatorio de organizacao — Abigobaldo's Kitchen

## Auditoria

O projeto possui duas cenas de producao listadas no Build Settings: `Assets/Scenes/menu.unity` e `Assets/Scenes/MainGame.unity`.

O conteudo proprio ja esta majoritariamente separado de terceiros: os pacotes Low Poly Fire Particles, QMS Cartoon Skybox, UnityNonConvexMeshColliders e uma copia importada de TextMesh Pro ficam em `Assets/ImportedAssets`. Eles foram preservados sem movimentacao.

Dominios runtime encontrados: Player, Interaction, Objects/Plating, Cooking/Recipes, Customers/Dialogue, Managers/Game Flow, Lighting, Audio, Menu e Visuals.

## Estrutura adotada

Nao foi feita uma movimentacao em massa de assets nesta etapa. O projeto estava aberto no Editor e mover arquivos Unity fora do `AssetDatabase` durante esse estado traz risco desnecessario de referencias quebradas. A estrutura de alto nivel atual ja separa codigo, prefabs, arte, audio, cenas, configuracoes e terceiros; ela foi documentada no README.

Foi adicionada a ferramenta `BuildItchWebGL` em `_Scripts/Editor`, que pertence a ferramentas internas e nao participa do jogo em runtime.

## Pontos de atencao

- `Assets/menu.unity` existe fora de `Assets/Scenes` e nao esta no Build Settings. Nao foi apagada: seu conteudo e diferente da cena de menu usada no build e precisa de revisao manual antes de qualquer remocao.
- `Assets/Scenes/KbumAlterações.unity` nao esta no Build Settings. Trate-a como cena de desenvolvimento/backup ate decidir se deve virar uma cena formal de teste.
- `Assets/Images/-` e `Assets/Images/Menu` contem arquivos com nomes duplicados. Eles nao foram unidos ou removidos; referencias de UI precisam ser revisadas antes disso.
- `Assets/_Scripts/Data` contem instancias `.asset` e classes de dados dentro da mesma arvore. Funciona, mas em uma proxima migracao controlada recomenda-se mover as instancias para `Assets/Data/` via `AssetDatabase.MoveAsset`, sempre com Unity fechada ou pelo menu de Editor.
- Existem logs de build/compilacao no diretorio raiz e pastas derivadas (`Library`, `Temp`, `Logs`, `render_output`). Sao gerados/localmente e nao devem virar conteudo de jogo.

## Candidatos a revisao, nao a exclusao

| Caminho | Motivo | Acao segura |
| --- | --- | --- |
| `Assets/menu.unity` | Cena fora do fluxo de build | Abrir e comparar antes de remover/mover |
| `Assets/Scenes/KbumAlterações.unity` | Nome de alteracao/backup | Classificar como Dev ou substituir conscientemente |
| `Assets/Images/-` | Nome de pasta sem semantica | Auditar referencias e mover pelo AssetDatabase |
| `Assets/ImportedAssets/TextMesh Pro` | Pode duplicar o pacote TMP | Confirmar referencias e Package Manager antes de alterar |

## Convencao

Manter nomes funcionais legiveis para prefabs e scripts. Prefixos so sao recomendados para tipos ambigous fora do Inspector; nao foram aplicados em massa para evitar quebra de referencias e nomes de classes.

## Validacao pendente

O modulo **WebGL Build Support** nao estava instalado nesta maquina durante a auditoria. Sem ele, uma build WebGL nao pode ser executada. A ferramenta de build e as instrucoes ja estao prontas; apos instalar o modulo e fechar o Editor, rode a ferramenta pelo menu Unity e valide o ZIP no navegador/itch.io.
