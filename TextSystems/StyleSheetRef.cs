using UnityEngine;

namespace Shoelace.Utilities
{
    [CreateAssetMenu(fileName = "New Style Sheet Ref", menuName = "Text Core/StyleSheetRef")]
    public class StyleSheetRef : ScriptableObject
    {
        public string baseHexColorValue;
        public string HighlightHexColorValue;
    }
}
