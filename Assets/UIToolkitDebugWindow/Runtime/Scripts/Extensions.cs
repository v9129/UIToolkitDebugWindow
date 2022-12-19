using System;
using UnityEngine.UIElements;

namespace UTKDebugWindow
{
    public static class Extensions
    {
        public static T AddElement<T>(this VisualElement parent, T visualElement, Action<T> initializer = null) where T : VisualElement
        {
            parent.Add(visualElement);
            initializer?.Invoke(visualElement);
            return visualElement;
        }
    }
}
