# Abigobaldo — retomada rápida

## Abrir primeiro

Abra `Assets/Scenes/MainGame.unity` para trabalhar no jogo. O Build Settings já está configurado assim:

1. `menu`
2. `MainGame`

## Onde mexer

- Menu, botões e música: `Assets/Scenes/menu.unity`, `Assets/_Scripts/Menu/` e `Assets/Images/Menu/`.
- Cozinha, player, clientes e HUD: `Assets/Scenes/MainGame.unity`.
- Lógica de receitas: `Assets/_Scripts/Cooking/` e `Assets/_Scripts/Data/Recipes/`.
- Pedidos e diálogos: `Assets/_Scripts/Customers/`, `Assets/_Scripts/Managers/CustomerManager.cs` e `Assets/_Scripts/Data/Dialogs/CustomerDialogueData.asset`.
- Áudio do jogo: `Assets/Resources/Audio/Gameplay/`.
- Iluminação: `Assets/_Scripts/Managers/Lighting/Lightning.prefab` e `LightingManager.cs`.

## Checklist antes de gerar WebGL

- Entrar pelo menu e verificar a iluminação na MainGame.
- Conferir HUD, textos de diálogo e opções.
- Testar uma receita e uma entrega errada/perfeita.
- Testar os sons do liquidificador, frigideira, sino e Abigobaldo.
- Testar pausa com `Esc` e as opções de áudio.

## Organização

- `Assets/Images/Menu/` contém os assets ativos do menu.
- `Assets/Images/Legacy/` guarda exports antigos sem referência; não use como fonte principal.
- `My project (4)` é uma cópia de segurança completa do projeto. Não é a pasta que deve ser aberta para trabalhar.

Veja também `Assets/Docs/ProjectOrganizationReport.md` para o mapa completo.
