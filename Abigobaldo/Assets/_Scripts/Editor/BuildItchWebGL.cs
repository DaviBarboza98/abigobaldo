using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Abigobaldo.EditorTools
{
    /// <summary>Creates the WebGL folder and ready-to-upload zip used by itch.io.</summary>
    public static class BuildItchWebGL
    {
        private const string OutputFolderName = "Build Para ItchIO";
        private const string WebGlFolderName = "WebGL";
        private const string ZipFileName = "AbigobaldosKitchen_WebGL_ItchIO.zip";

        [MenuItem("Abigobaldo/Build/Build WebGL for itch.io")]
        public static void Build()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string root = Path.Combine(desktop, OutputFolderName);
            string webgl = Path.Combine(root, WebGlFolderName);
            string zip = Path.Combine(root, ZipFileName);

            if (Directory.Exists(webgl)) Directory.Delete(webgl, true);
            if (File.Exists(zip)) File.Delete(zip);
            Directory.CreateDirectory(root);

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;

            string[] scenes = { "Assets/Scenes/menu.unity", "Assets/Scenes/MainGame.unity" };
            BuildReport report = BuildPipeline.BuildPlayer(scenes, webgl, BuildTarget.WebGL, BuildOptions.None);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("WebGL build failed. Check the Console for the exact Unity error.");
                return;
            }

            File.WriteAllText(Path.Combine(root, "INSTRUCOES_ITCHIO.txt"), Instructions());
            ZipFile.CreateFromDirectory(webgl, zip, System.IO.Compression.CompressionLevel.Optimal, false);
            Debug.Log($"Build ready: {root}");
        }

        private static string Instructions()
        {
            return "ABIGOBALDO'S KITCHEN — ENVIO PARA ITCH.IO\n\n" +
                   "1. Abra https://itch.io/dashboard e crie ou edite a pagina do jogo.\n" +
                   "2. Em Uploads, envie o arquivo AbigobaldosKitchen_WebGL_ItchIO.zip desta pasta.\n" +
                   "3. Marque a opcao: This file will be played in the browser.\n" +
                   "4. Escolha HTML/JavaScript ou WebGL quando o itch.io perguntar a plataforma.\n" +
                   "5. Em Embed options, deixe o tamanho em 960 x 600 ou escolha Fullscreen.\n" +
                   "6. Salve a pagina e use View page para testar no navegador.\n\n" +
                   "IMPORTANTE: envie o ZIP, nao a pasta WebGL e nao os arquivos separados.\n" +
                   "O ZIP ja possui index.html na raiz, que e o formato certo para o itch.io.\n";
        }
    }
}
