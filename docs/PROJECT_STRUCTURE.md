# Project Structure

O projeto agora usa o prototipo jogavel como jogo base. Nao existe mais separacao entre pastas temporarias, demo, old/new ou versoes paralelas.

## Pastas principais

```text
Assets/
  Audio/          Sons e musicas do jogo.
  Audio/SFX
  Audio/Musicas
  Images/         Imagens de UI, mouse, logo e sprites soltos.
  ImportedAssets/ Assets de terceiros. Evite editar diretamente.
  Materials/      Materiais e Texturas do jogo.
  Models/         FBX/modelos 3D do jogo.
  Prefabs/        Prefabs usaveis em cena/runtime.
  Scenes/         Cenas Unity.
  _Scripts/        Codigo do jogo.
  _Scripts/Data/           ScriptableObjects do jogo, principalmente recipes.
  Settings/       URP e configuracoes de render.
  Shaders/        Shaders proprios.
```

## Cena principal

- `Assets/Scenes/MainGame.unity` e a cena principal atual.
- A cena temporaria antiga foi removida.
- `Menu.unity` foi mantida para futura tela inicial.

## Codigo

Namespace base:

```csharp
Abigobaldo.Game
```

Pastas:

- `_Scripts/Player`: input, camera, movimento, interacao.
- `_Scripts/Objects`: objetos pegaveis, holder, spawners e prato.
- `_Scripts/Cooking`: recipes, containers, estados de comida e particulas.
- `_Scripts/Interaction`: portas, highlight e interfaces.
- `_Scripts/System`: configuracoes globais pequenas, como performance.
- `_Scripts/Data/Recipes`: assets de receita do jogo.

## Prefabs

- `Prefabs/Objects`: objetos pegaveis ou spawnaveis.
- `Prefabs/Spawners`: spawners que entregam objeto direto na mao.
- `Prefabs/Visuals`: visuais usados para comida empratada ou conteudo de container.
- `Prefabs/Player`: player principal.

## Regra de organizacao

- Nada novo deve ir para pastas temporarias, demo, old/new ou versoes paralelas.
- Visual prefab continua valendo: quando um container/prato precisa mostrar comida de forma bonita, use `Prefabs/Visuals`.
- Materiais ficam em `Assets/Materials`.
- Texturas ficam em `Assets/Materials/Textures`.
- Assets de plugin/terceiros ficam em `Assets/ImportedAssets`.
