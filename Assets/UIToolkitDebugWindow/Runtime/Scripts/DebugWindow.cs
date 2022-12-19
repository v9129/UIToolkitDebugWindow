using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UTKDebugWindow
{
    [Serializable]
    public class DebugWindow : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<DebugWindow, UxmlTraits>
        {
        }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Text = new() { name = "text" };
            UxmlBoolAttributeDescription m_Value = new() { name = "value", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                if (ve is DebugWindow self)
                {
                    self.foldout.text = m_Text.GetValueFromBag(bag, cc);
                    self.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
                }
            }
        }

        public override VisualElement contentContainer => foldout.contentContainer;

        [SerializeField] bool value;

        public bool Value
        {
            get => value;
            set
            {
                if (this.value == value)
                    return;

                using var evt = ChangeEvent<bool>.GetPooled(this.value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        public string Text { get => foldout.text; set => foldout.text = value; }

        public VisualElement WindowContentContainer => foldout.contentContainer;

        public void SetValueWithoutNotify(bool newValue)
        {
            value = newValue;
            foldout.value = value;
        }

        public static readonly string ussClassName = "dwd-window";
        public static readonly string foldoutUssClassName = ussClassName + "__foldout";

        readonly Foldout foldout;

        public DebugWindow()
        {
            value = false;
            AddToClassList(ussClassName);
            RegisterCallback<PointerDownEvent>(_ => BringToFront(), TrickleDown.TrickleDown);
            foldout = new Foldout
            {
                value = value
            };
            foldout.RegisterValueChangedCallback((evt) =>
            {
                if (evt.currentTarget == evt.target)
                {
                    Value = foldout.value;
                    evt.StopPropagation();
                }
            });
            foldout.AddToClassList(foldoutUssClassName);
            hierarchy.Add(foldout);
            var dragArea = new VisualElement() { name = "drag-area" };
            dragArea.AddToClassList("unity-foldout__drag-area");
            foldout.Q<Toggle>().Add(dragArea);
            var manipulator = new DragManipulator(this);
            dragArea.AddManipulator(manipulator);
        }
    }
}
