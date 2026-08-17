using UnityEngine;

namespace Abigobaldo.Game
{
    [CreateAssetMenu(fileName = "CustomerDialogueData", menuName = "Abigobaldo/Customers/Dialogue Data")]
    public sealed class CustomerDialogueData : ScriptableObject
    {
        [TextArea(2, 5)] public string ninoFirstIntro;
        [TextArea(2, 5)] public string ninoNameIntro;
        [TextArea(2, 5)] public string ninoAnyOrder;
        [TextArea(2, 5)] public string ninoCuscuzOrder;
        [TextArea(2, 5)] public string seuZeIntro;
        [TextArea(2, 5)] public string seuZeNinoAnswer;
        [TextArea(2, 5)] public string marciaIntro;
        [TextArea(2, 5)] public string ninoReturnAfterCuscuz;
        [TextArea(2, 5)] public string ninoReturnAfterAnyFood;
        [TextArea(2, 5)] public string finalMarcia;
        [TextArea(2, 5)] public string finalSeuZe;
    }
}
