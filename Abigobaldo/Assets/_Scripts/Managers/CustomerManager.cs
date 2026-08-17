using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abigobaldo.Game
{
    /// <summary>Console-only dialogue demo. Press 1–4 for the printed choices.</summary>
    [DefaultExecutionOrder(-150)]
    public sealed class CustomerManager : MonoBehaviour
    {
        private sealed class Order
        {
            public string customerId;
            public string foodId;
            public string foodLabel;
            public bool anyFood;

            public Order(string customer, string food, string label, bool acceptsAny = false)
            {
                customerId = customer; foodId = food; foodLabel = label; anyFood = acceptsAny;
            }
        }

        private sealed class Choice
        {
            public string text;
            public Action action;
            public Choice(string choiceText, Action choiceAction) { text = choiceText; action = choiceAction; }
        }

        public static CustomerManager Instance { get; private set; }

        [Header("Required")]
        [SerializeField] private NpcSpawner npcSpawner;
        [SerializeField] private DayNightManager dayNightManager;
        [Tooltip("Create a Camera in MainGame, put it where you want the cinematic shot, and assign it here.")]
        [SerializeField] private Camera dialogueCamera;
        [Header("Optional black bars already created in your UICanvas")]
        [SerializeField] private GameObject topBlackBar;
        [SerializeField] private GameObject bottomBlackBar;
        [SerializeField, Min(0f)] private float firstSpawnDelay = 5f;
        [SerializeField, Min(0f)] private float nextSpawnDelay = 1f;
        [Header("Cinematic transition")]
        [SerializeField, Min(0.1f)] private float cameraTransitionDuration = 2f;
        [SerializeField, Min(0.1f)] private float playerHideDuration = 1f;

        private readonly List<Order> orders = new List<Order>();
        private readonly Dictionary<string, int> respect = new Dictionary<string, int> { { "nino", 0 }, { "marcia", 0 }, { "seuze", 0 } };
        private readonly Dictionary<string, bool> history = new Dictionary<string, bool> { { "nino", false }, { "marcia", false }, { "seuze", false } };
        private CustomerNpc activeCustomer;
        private CustomerCinematicBars cinematicBars;
        private Camera playerCamera;
        private AudioListener playerAudioListener;
        private AudioListener dialogueAudioListener;
        private Vector3 dialogueShotPosition;
        private Quaternion dialogueShotRotation;
        private float dialogueShotFov;
        private MonoBehaviour[] disabledDuringDialogue;
        private Transform playerModel;
        private Vector3 playerModelScale;
        private Choice[] choices = Array.Empty<Choice>();
        private int visit;
        private int completedOrders;
        private int perfectOrders;
        private int poorQuality;
        private int severeDisrespect;
        private int dialoguePoints;
        private bool dialogueOpen;
        private bool endingDialogue;
        private bool waitingForFood;
        private string fourthCustomer;
        private string finalCustomer;

        private void Awake()
        {
            Instance = this;
            if (npcSpawner == null) npcSpawner = FindObjectOfType<NpcSpawner>();
            if (dayNightManager == null) dayNightManager = FindObjectOfType<DayNightManager>();
            if (dayNightManager == null)
            {
                GameObject managerObject = new GameObject("DayNightManager");
                dayNightManager = managerObject.AddComponent<DayNightManager>();
            }
            ConfigureCinematicBars();
            playerCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            if (playerCamera != null) playerAudioListener = playerCamera.GetComponent<AudioListener>();
            if (dialogueCamera != null)
            {
                dialogueShotPosition = dialogueCamera.transform.position;
                dialogueShotRotation = dialogueCamera.transform.rotation;
                dialogueShotFov = dialogueCamera.fieldOfView;
                dialogueAudioListener = dialogueCamera.GetComponent<AudioListener>();
                dialogueCamera.enabled = false;
                if (dialogueAudioListener != null) dialogueAudioListener.enabled = false;
            }
            if (topBlackBar != null) topBlackBar.SetActive(false);
            if (bottomBlackBar != null) bottomBlackBar.SetActive(false);
            orders.Add(new Order("nino", "", "qualquer comida", true));
            orders.Add(new Order("marcia", "Cuscuz", "cuscuz"));
            orders.Add(new Order("seuze", "FriedEgg", "ovo frito"));
            orders.Add(null);
            orders.Add(null);
        }

        private void Start()
        {
            Debug.Log("<b>[DIALOGUE]</b> Demo começa em " + firstSpawnDelay + " segundos. Leia as falas no Console e use as teclas <b>1–4</b>.");
            StartCoroutine(BeginDemo());
        }

        private IEnumerator BeginDemo()
        {
            yield return new WaitForSeconds(firstSpawnDelay);
            SpawnCurrentVisit();
        }

        private void Update()
        {
            if (!dialogueOpen || choices.Length == 0 || Keyboard.current == null) return;
            for (int index = 0; index < choices.Length; index++)
            {
                if (!Pressed(index + 1)) continue;
                Choice selected = choices[index];
                choices = Array.Empty<Choice>();
                selected.action?.Invoke();
                return;
            }
        }

        public void TalkTo(CustomerNpc customer)
        {
            if (customer == null || customer != activeCustomer || dialogueOpen || waitingForFood) return;
            StartCoroutine(BeginDialogueSequence());
        }

        private IEnumerator BeginDialogueSequence()
        {
            // This stays at zero for the whole sequence. The transition itself
            // uses unscaled time, so physics/cooking cannot progress underneath it.
            Time.timeScale = 0f;
            DisablePlayerControls();
            CachePlayerModel();

            float elapsed = 0f;
            Vector3 startPosition = dialogueCamera != null ? dialogueCamera.transform.position : Vector3.zero;
            Quaternion startRotation = dialogueCamera != null ? dialogueCamera.transform.rotation : Quaternion.identity;
            Vector3 targetPosition = dialogueCamera != null ? dialogueShotPosition : Vector3.zero;
            Quaternion targetRotation = dialogueCamera != null ? dialogueShotRotation : Quaternion.identity;
            float startFov = dialogueCamera != null ? dialogueCamera.fieldOfView : 60f;
            float targetFov = dialogueCamera != null ? dialogueShotFov : startFov;

            if (dialogueCamera != null)
            {
                if (playerCamera != null)
                {
                    dialogueCamera.CopyFrom(playerCamera);
                    dialogueCamera.transform.SetPositionAndRotation(playerCamera.transform.position, playerCamera.transform.rotation);
                    startPosition = playerCamera.transform.position;
                    startRotation = playerCamera.transform.rotation;
                    startFov = playerCamera.fieldOfView;
                    playerCamera.enabled = false;
                }
                dialogueCamera.enabled = true;
            }
            if (playerAudioListener != null) playerAudioListener.enabled = false;
            if (dialogueAudioListener != null) dialogueAudioListener.enabled = true;
            Cursor.visible = false;

            while (elapsed < cameraTransitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / cameraTransitionDuration);
                float smooth = Mathf.SmoothStep(0f, 1f, progress);
                cinematicBars?.SetProgress(smooth);
                if (dialogueCamera != null)
                {
                    dialogueCamera.transform.SetPositionAndRotation(
                        Vector3.Lerp(startPosition, targetPosition, smooth),
                        Quaternion.Slerp(startRotation, targetRotation, smooth));
                    dialogueCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, smooth);
                }
                SetPlayerModelVisibility(Mathf.Clamp01(elapsed / playerHideDuration));
                yield return null;
            }

            cinematicBars?.SetProgress(1f);
            SetPlayerModelVisibility(1f);
            dialogueOpen = true;

            switch (visit)
            {
                case 0: NinoFirst(); break;
                case 1: MarciaFirst(); break;
                case 2: SeuZeFirst(); break;
                case 3: FourthVisit(); break;
                case 4: FinalVisit(); break;
            }
        }

        public void ReceiveFood(CustomerNpc customer, CustomerServedFood food)
        {
            if (customer != activeCustomer || !waitingForFood || !food.IsValid) return;
            Order order = orders[visit];
            if (!order.anyFood && !Matches(food.FoodName, order.foodId))
            {
                Say(customer.RealName, "Não foi isso que eu pedi. Posso esperar o pedido certo.");
                Respect(customer.CustomerId, -1);
                return;
            }
            if (food.IsCharcoal)
            {
                Say(customer.RealName, "Isso é carvão. Estar com fome não quer dizer que eu vou comer isso.");
                Respect(customer.CustomerId, -3); poorQuality++;
                return;
            }
            if (food.State == FoodState.Raw)
            {
                Say(customer.RealName, "Isso ainda está cru. Consegue fazer de novo?");
                Respect(customer.CustomerId, -1);
                return;
            }

            food.Consume();
            customer.MarkDeliveryAccepted();
            waitingForFood = false;
            completedOrders++;
            ReactToFood(customer, food.State);
            StartCoroutine(FinishVisit());
        }

        private void SpawnCurrentVisit()
        {
            if (visit == 5) { FinishDay(); return; }
            SetVisitOrder();
            SetTimeOfDay();
            Order order = orders[visit];
            bool known = order.customerId != "nino" || history["nino"] || respect["nino"] >= 2;
            activeCustomer = npcSpawner == null ? null : npcSpawner.Spawn(order.customerId, known);
            if (activeCustomer == null)
            {
                Debug.LogError("[DIALOGUE] NpcSpawner não encontrou o prefab de " + order.customerId + ". Configure Marcia, Nino e SeuZe nele.");
                return;
            }
            Debug.Log("\n<b>[APARIÇÃO " + (visit + 1) + "/5 — " + VisitTime() + "]</b> " + activeCustomer.SpeakerName + " chegou. Olhe para ele e aperte <b>E</b>.");
        }

        private void SetVisitOrder()
        {
            if (visit == 3)
            {
                fourthCustomer = history["nino"] || respect["nino"] >= 3 ? "nino" : "marcia";
                orders[3] = fourthCustomer == "nino" ? new Order("nino", "Omelet", "omelete") : new Order("marcia", "", "qualquer comida", true);
            }
            if (visit == 4)
            {
                finalCustomer = severeDisrespect > 0 || respect["seuze"] <= respect["marcia"] ? "seuze" : "marcia";
                orders[4] = new Order(finalCustomer, "RoastedCorn", "milho assado");
            }
        }

        private void SetTimeOfDay()
        {
            if (dayNightManager == null) return;
            if (visit <= 1) dayNightManager.SetPeriod(DayNightManager.Period.Morning);
            else if (visit <= 3) dayNightManager.SetPeriod(DayNightManager.Period.Afternoon);
            else dayNightManager.SetPeriod(DayNightManager.Period.Night);
        }

        // ----- NINO / MANHÃ -----
        private void NinoFirst()
        {
            Show("???", "Moço... isso aí é comida?", C(
                ("É. Você está com fome?", () => { Respect("nino", 1); NinoNoMoney(); }),
                ("Quer alguma coisa?", NinoNoMoney),
                ("Você tem dinheiro?", NinoAsksForMoney),
                ("Tô ocupado.", NinoBusy)));
        }

        private void NinoAsksForMoney()
        {
            Respect("nino", -2);
            Show("???", "Não.", C(
                ("Não precisa pagar.", NinoOffer),
                ("Então complica.", NinoOffer),
                ("Então não dá.", () => Leave("Nino", "Tá... desculpa."))));
        }

        private void NinoBusy()
        {
            Show("???", "Tá. Desculpa.", C(
                ("Espera. Pode falar.", NinoNoMoney),
                ("Pode ir.", () => Leave("Nino", "Tá bom. Boa manhã."))));
        }

        private void NinoNoMoney()
        {
            Show("???", "Tô. Mas eu não tenho dinheiro.", C(
                ("Aqui não precisa pagar.", () => { Respect("nino", 2); NinoOffer(); }),
                ("Tudo bem. O que você quer comer?", NinoOffer),
                ("Eu também tenho contas.", () => { Respect("nino", -1); NinoOffer(); }),
                ("Então não posso fazer nada.", () => Leave("Nino", "É... eu entendo."))));
        }

        private void NinoOffer()
        {
            Show("???", "Pode ser qualquer coisa. Sério.", C(
                ("Antes, qual é seu nome?", () => { activeCustomer.RevealName(); NinoWhere(); }),
                ("Beleza. Espera aí.", () => Request("Nino")),
                ("Você fica sempre por aqui?", NinoWhere),
                ("Então não reclama do que vier.", () => { Respect("nino", -2); Request("Nino"); })));
        }

        private void NinoWhere()
        {
            activeCustomer.RevealName();
            Show("Nino", "Agora eu fico. Mais ali em cima.", C(
                ("Se quiser contar como veio parar aqui, eu escuto.", NinoHistory),
                ("Não precisa falar se não quiser.", NinoHistory),
                ("Entendi. Vou fazer sua comida.", () => Request("Nino")),
                ("Fez alguma besteira?", () => { Respect("nino", -2); Request("Nino"); })));
        }

        private void NinoHistory()
        {
            Show("Nino", "Eu trabalhava num depósito. Dividia um quarto com meu primo. Ele foi embora e depois cortaram gente no depósito. Não consegui segurar o aluguel sozinho.", C(
                ("E desde então você está se virando por aqui?", () => { Hear("nino", 2); Show("Nino", "É. Tem uma mulher mais ali em cima, Márcia. Ela também está com fome.", C(("Vou fazer sua comida.", () => Request("Nino")))); }),
                ("Sua família sabe?", () => { Hear("nino", 1); Request("Nino"); }),
                ("Você devia ter se planejado melhor.", () => { Hear("nino", -3); severeDisrespect++; Request("Nino"); }),
                ("Tá. E o que você quer comer?", () => { Hear("nino", 0); Request("Nino"); })));
        }

        // ----- MÁRCIA / MANHÃ -----
        private void MarciaFirst()
        {
            Show("???", "Bom dia. Isso aí é comida de verdade ou uma armadilha muito bem iluminada?", C(
                ("É comida. Está com fome?", () => { Respect("marcia", 1); MarciaHungry(); }),
                ("Você é a Márcia?", () => { activeCustomer.RevealName(); Respect("marcia", 1); MarciaHungry(); }),
                ("Depende. Você tem dinheiro?", () => { Respect("marcia", -2); MarciaHungry(); }),
                ("Não estou atendendo.", MarciaRejects)));
        }

        private void MarciaRejects()
        {
            Show("???", "Tudo bem. Eu consigo ouvir um não em quatro idiomas.", C(
                ("Espera. Está com fome?", MarciaHungry),
                ("É isso.", () => Leave("Márcia", "Espero que a manhã melhore para nós dois."))));
        }

        private void MarciaHungry()
        {
            activeCustomer.RevealName();
            Show("Márcia", "Estou. Bastante. Mas consigo conversar enquanto isso, se você tiver sorte.", C(
                ("O que você quer comer?", () => Request("Márcia")),
                ("Como você veio parar por aqui?", MarciaHistory),
                ("Você conhece o Nino?", () => Show("Márcia", "Conheço. Ele pede desculpa até para uma cadeira quando esbarra nela.", C(("E você, como veio parar aqui?", MarciaHistory), ("O que você quer comer?", () => Request("Márcia"))))),
                ("Todo mundo quer alguma coisa.", () => { Respect("marcia", -2); Request("Márcia"); })));
        }

        private void MarciaHistory()
        {
            Show("Márcia", "Eu tinha uma lojinha de costura. Quando fechou, fui adiando aluguel, conta e pedido de ajuda. Quando percebi, carregava tudo numa mochila e fingia que era temporário.", C(
                ("Você não precisa fingir nada aqui.", () => { Hear("marcia", 2); Request("Márcia"); }),
                ("Sinto muito.", () => { Hear("marcia", 1); Request("Márcia"); }),
                ("E sua família?", () => { Hear("marcia", 0); Request("Márcia"); }),
                ("Certo. Vai querer o quê?", () => { Hear("marcia", 0); Request("Márcia"); })));
        }

        // ----- SEUZE / TARDE -----
        private void SeuZeFirst()
        {
            Show("???", "Você é o cozinheiro?", C(
                ("Sou. Está com fome?", () => { Respect("seuze", 1); SeuZeEgg(); }),
                ("Quem quer saber?", () => { Respect("seuze", -1); SeuZeEgg(); }),
                ("Márcia falou de mim?", SeuZeEgg),
                ("Se veio pedir comida, fala logo.", () => { Respect("seuze", -2); SeuZeEgg(); })));
        }

        private void SeuZeEgg()
        {
            activeCustomer.RevealName();
            Show("Seu Zé", "Quero saber se tem ovo. O resto eu descubro depois.", C(
                ("Tem. Frito?", () => Request("Seu Zé")),
                ("Tem. Qual é seu nome?", () => Show("Seu Zé", "Seu Zé. Não precisa chamar de senhor; eu ainda não virei mobília.", C(("Prazer, Seu Zé.", () => Request("Seu Zé")), ("Como veio parar por aqui?", SeuZeHistory)))),
                ("Como o senhor veio parar por aqui?", SeuZeHistory),
                ("Só isso?", () => { Respect("seuze", -1); Request("Seu Zé"); })));
        }

        private void SeuZeHistory()
        {
            Show("Seu Zé", "Eu era pedreiro. Quando minhas costas pararam, a obra também parou de me chamar. Você perde diária, quarto, endereço... não acontece tudo em um dia.", C(
                ("O senhor não devia ter passado por isso sozinho.", () => { Hear("seuze", 2); Request("Seu Zé"); }),
                ("Obrigado por contar.", () => { Hear("seuze", 1); Request("Seu Zé"); }),
                ("Ainda dá para trabalhar?", () => { Hear("seuze", 0); Request("Seu Zé"); }),
                ("Então quer o ovo ou não?", () => { Hear("seuze", -1); Request("Seu Zé"); })));
        }

        // ----- RETORNOS / TARDE E NOITE -----
        private void FourthVisit()
        {
            if (fourthCustomer == "nino")
                Show("Nino", "Oi... eu voltei. Se ainda tiver alguma coisa.", C(("Pode chegar, Nino.", () => { Respect("nino", 1); Request("Nino"); }), ("Está com fome de novo?", () => Request("Nino")), ("Você voltou.", () => Request("Nino")), ("O que foi agora?", () => { Respect("nino", -2); Request("Nino"); })));
            else
                Show("Márcia", "Voltei. Uma pessoa comum teria vergonha. Ainda bem que eu não sou comum.", C(("Quer comer?", () => Request("Márcia")), ("Pode chegar.", () => { Respect("marcia", 1); Request("Márcia"); }), ("Como foi sua tarde?", () => Show("Márcia", "Longa. Mas encontrei o Nino, então não foi completamente perdida.", C(("O que você quer comer?", () => Request("Márcia"))))), ("Você de novo?", () => { Respect("marcia", -1); Request("Márcia"); })));
        }

        private void FinalVisit()
        {
            if (finalCustomer == "seuze")
                Show("Seu Zé", "Ainda tem comida?", C(("Tem. O que o senhor quer?", () => { Respect("seuze", 1); Request("Seu Zé"); }), ("Tenho milho.", () => Request("Seu Zé")), ("Agora resolveu voltar?", () => { Respect("seuze", -2); Request("Seu Zé"); }), ("Estou fechando.", () => Leave("Seu Zé", "Fome voltou antes."))));
            else
                Show("Márcia", "Boa noite, Abigobaldo. A cidade fica mais honesta de noite: todo mundo admite que está cansado.", C(("Ainda está com fome?", () => { Respect("marcia", 1); Request("Márcia"); }), ("Pode pedir.", () => Request("Márcia")), ("Você fala demais.", () => { Respect("marcia", -1); Request("Márcia"); }), ("Estou fechando.", () => Leave("Márcia", "Tudo bem. Boa noite, Abigobaldo."))));
        }

        private void Request(string speaker)
        {
            Order order = orders[visit];
            Show(speaker, "Pedido: " + (order.anyFood ? "qualquer comida" : order.foodLabel) + ".", C(("Pode deixar.", BeginFoodWaiting)));
        }

        private void BeginFoodWaiting()
        {
            EndDialogue();
            waitingForFood = true;
            activeCustomer.SetAcceptingDelivery(true);
            Order order = orders[visit];
            Debug.Log("<b>[PEDIDO " + (visit + 1) + "/5]</b> " + activeCustomer.RealName + " espera: <b>" + (order.anyFood ? "qualquer comida" : order.foodLabel) + "</b>. Entregue/jogue o prato nele.");
        }

        private void Leave(string speaker, string line)
        {
            Say(speaker, line);
            severeDisrespect++;
            EndDialogue();
            StartCoroutine(FinishVisit());
        }

        private IEnumerator FinishVisit()
        {
            yield return new WaitForSeconds(nextSpawnDelay);
            npcSpawner?.DespawnCurrent();
            activeCustomer = null;
            visit++;
            SpawnCurrentVisit();
        }

        private void ReactToFood(CustomerNpc customer, FoodState state)
        {
            if (state == FoodState.Ready) { Respect(customer.CustomerId, 2); perfectOrders++; Say(customer.RealName, "Tá bom de verdade. Obrigado."); }
            else if (state == FoodState.AlmostReady) { Respect(customer.CustomerId, 1); Say(customer.RealName, "Faltou pouco, mas dá pra comer. Obrigado."); }
            else if (state == FoodState.Overdone) Say(customer.RealName, "Passou um pouco. Mas eu aceito.");
            else { Respect(customer.CustomerId, -2); poorQuality++; Say(customer.RealName, "Queimou. Eu estava com fome, não pedindo carvão."); }
        }

        private void Hear(string customerId, int respectGain)
        {
            history[customerId] = true;
            Respect(customerId, respectGain);
            dialoguePoints += 6;
        }

        private void Respect(string customerId, int amount)
        {
            respect[customerId] += amount;
            dialoguePoints += amount > 0 ? amount : amount * 2;
        }

        private void Show(string speaker, string text, Choice[] nextChoices)
        {
            Say(speaker, text);
            choices = nextChoices;
            for (int index = 0; index < choices.Length; index++) Debug.Log("<b>" + (index + 1) + ".</b> " + choices[index].text);
        }

        private static void Say(string speaker, string text) => Debug.Log("\n<b>[" + speaker + "]</b>\n" + text);
        private static Choice[] C(params (string, Action)[] values)
        {
            Choice[] result = new Choice[values.Length];
            for (int index = 0; index < values.Length; index++) result[index] = new Choice(values[index].Item1, values[index].Item2);
            return result;
        }

        private void EndDialogue()
        {
            if (endingDialogue) return;
            StartCoroutine(EndDialogueSequence());
        }

        private IEnumerator EndDialogueSequence()
        {
            endingDialogue = true;
            choices = Array.Empty<Choice>();
            dialogueOpen = false;

            // Match the fade-out duration to the camera's configured transition.
            float elapsed = 0f;
            Vector3 startPosition = dialogueCamera != null ? dialogueCamera.transform.position : Vector3.zero;
            Quaternion startRotation = dialogueCamera != null ? dialogueCamera.transform.rotation : Quaternion.identity;
            float startFov = dialogueCamera != null ? dialogueCamera.fieldOfView : 60f;
            Vector3 targetPosition = playerCamera != null ? playerCamera.transform.position : startPosition;
            Quaternion targetRotation = playerCamera != null ? playerCamera.transform.rotation : startRotation;
            float targetFov = playerCamera != null ? playerCamera.fieldOfView : startFov;
            while (elapsed < cameraTransitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / cameraTransitionDuration));
                cinematicBars?.SetProgress(1f - smooth);
                if (dialogueCamera != null)
                {
                    dialogueCamera.transform.SetPositionAndRotation(
                        Vector3.Lerp(startPosition, targetPosition, smooth),
                        Quaternion.Slerp(startRotation, targetRotation, smooth));
                    dialogueCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, smooth);
                }
                SetPlayerModelVisibility(1f - Mathf.Clamp01(elapsed / playerHideDuration));
                yield return null;
            }

            Time.timeScale = 1f;
            cinematicBars?.Hide();
            if (topBlackBar != null) topBlackBar.SetActive(false);
            if (bottomBlackBar != null) bottomBlackBar.SetActive(false);
            if (dialogueCamera != null) dialogueCamera.enabled = false;
            if (playerCamera != null) playerCamera.enabled = true;
            if (dialogueAudioListener != null) dialogueAudioListener.enabled = false;
            if (playerAudioListener != null) playerAudioListener.enabled = true;
            RestorePlayerControls();
            SetPlayerModelVisibility(0f);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            endingDialogue = false;
        }

        private void ConfigureCinematicBars()
        {
            if (topBlackBar != null && bottomBlackBar != null) return;
            cinematicBars = FindObjectOfType<CustomerCinematicBars>();
            if (cinematicBars == null)
            {
                GameObject canvasRoot = GameObject.Find("UICanvas");
                // The old UICanvas is only an empty GameObject. A real UI Canvas
                // needs a RectTransform, so keep it untouched and create the
                // properly configured cinematic root beside it.
                if (canvasRoot == null || canvasRoot.GetComponent<RectTransform>() == null)
                    canvasRoot = new GameObject("CinematicCanvas", typeof(RectTransform));
                cinematicBars = canvasRoot.GetComponent<CustomerCinematicBars>();
                if (cinematicBars == null) cinematicBars = canvasRoot.AddComponent<CustomerCinematicBars>();
            }
            cinematicBars.EnsureCreated();
            topBlackBar = cinematicBars.TopBar;
            bottomBlackBar = cinematicBars.BottomBar;
        }

        private void CachePlayerModel()
        {
            if (playerModel != null) return;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            playerModel = FindChild(player.transform, "Model");
            if (playerModel != null) playerModelScale = playerModel.localScale;
        }

        // The model shrinks away over one second; physics, colliders and the
        // camera stay untouched, so this is safe while the game is paused.
        private void SetPlayerModelVisibility(float hiddenProgress)
        {
            if (playerModel == null) return;
            playerModel.localScale = Vector3.Lerp(playerModelScale, Vector3.zero, hiddenProgress);
        }

        private static Transform FindChild(Transform parent, string childName)
        {
            if (parent == null) return null;
            if (parent.name == childName) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindChild(child, childName);
                if (found != null) return found;
            }
            return null;
        }

        private void DisablePlayerControls()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            disabledDuringDialogue = new MonoBehaviour[]
            {
                player.GetComponent<PlayerInput>(), player.GetComponent<PlayerMovement>(),
                player.GetComponent<PlayerInteractor>(), player.GetComponent<PlayerCamera>(), player.GetComponent<PlayerCursor>()
            };
            foreach (MonoBehaviour behaviour in disabledDuringDialogue)
                if (behaviour != null) behaviour.enabled = false;
        }

        private void RestorePlayerControls()
        {
            if (disabledDuringDialogue == null) return;
            foreach (MonoBehaviour behaviour in disabledDuringDialogue)
                if (behaviour != null) behaviour.enabled = true;
            disabledDuringDialogue = null;
        }

        private static bool Pressed(int number) => number switch
        {
            1 => Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame,
            2 => Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame,
            3 => Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame,
            4 => Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame,
            _ => false
        };

        private static bool Matches(string served, string requested)
        {
            if (string.Equals(served, requested, StringComparison.OrdinalIgnoreCase)) return true;
            if (requested == "FriedEgg") return served.IndexOf("FriedEgg", StringComparison.OrdinalIgnoreCase) >= 0 || served.IndexOf("Ovo", StringComparison.OrdinalIgnoreCase) >= 0;
            if (requested == "Omelet") return served.IndexOf("Omelet", StringComparison.OrdinalIgnoreCase) >= 0 || served.IndexOf("Omelete", StringComparison.OrdinalIgnoreCase) >= 0;
            if (requested == "RoastedCorn") return served.IndexOf("RoastedCorn", StringComparison.OrdinalIgnoreCase) >= 0 || served.IndexOf("Corn", StringComparison.OrdinalIgnoreCase) >= 0 || served.IndexOf("Milho", StringComparison.OrdinalIgnoreCase) >= 0;
            return false;
        }

        private string VisitTime() => visit <= 1 ? "MANHÃ" : visit <= 3 ? "TARDE" : "NOITE";

        private void FinishDay()
        {
            int stories = (history["nino"] ? 1 : 0) + (history["marcia"] ? 1 : 0) + (history["seuze"] ? 1 : 0);
            bool best = completedOrders == 5 && perfectOrders == 5 && stories == 3 && respect["nino"] >= 6 && respect["marcia"] >= 5 && respect["seuze"] >= 5 && severeDisrespect == 0;
            bool worst = severeDisrespect >= 2 || completedOrders <= 2 || poorQuality >= 3 || LowRespectCount() >= 2;
            bool onlyFood = completedOrders == 5 && perfectOrders == 5 && stories == 0 && severeDisrespect == 0;
            string possibility = best ? "RESPEITO" : worst ? "PORTA FECHADA" : onlyFood ? "SÓ COMIDA" : "FOI UM DIA";
            int score = Mathf.Clamp(perfectOrders * 10 + (completedOrders - perfectOrders) * 5 + completedOrders * 4 + Mathf.Clamp(dialoguePoints, -20, 30), 0, 100);
            string grade = score >= 90 ? "A" : score >= 75 ? "B" : score >= 60 ? "C" : score >= 40 ? "D" : "F";
            string ending = best ? "Cinco refeições. Três histórias. Ninguém precisou fingir que a fome era pouca coisa." : worst ? "Ter comida nunca foi a mesma coisa que ajudar." : onlyFood ? "Você alimentou todo mundo, mas conheceu os pedidos — não as pessoas." : "O dia terminou melhor do que começou, mas não perfeitamente.";
            Debug.Log("\n<b>===== RESULTADO =====</b>\nPossibilidade: <b>" + possibility + "</b>\nNota: <b>" + grade + " — " + score + "/100</b>\nPedidos: " + completedOrders + "/5 | No ponto: " + perfectOrders + "/5 | Histórias: " + stories + "/3\nRespeito — Nino " + respect["nino"] + " | Márcia " + respect["marcia"] + " | SeuZe " + respect["seuze"] + "\n\n" + ending);
        }

        private int LowRespectCount()
        {
            int count = 0;
            foreach (int value in respect.Values) if (value <= -3) count++;
            return count;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (dialogueOpen) EndDialogue();
        }
    }
}
