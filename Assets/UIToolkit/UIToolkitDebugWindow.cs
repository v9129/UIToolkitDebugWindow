using System.Linq;
using Cysharp.Threading.Tasks;
using Kino;
using UTKDebugWindow;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class UIToolkitDebugWindow : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] StyleSheet styleSheet;

    void Start()
    {
        SetupDebugWindow();

        // キーボード不備対応: https://forum.unity.com/threads/how-to-prevent-textfield-taking-focus-upon-keyboard-input.1122568/#post-7601608
        uiDocument.rootVisualElement.RegisterCallback<NavigationMoveEvent>(e => e.StopImmediatePropagation(), TrickleDown.TrickleDown);
    }

    void SetupDebugWindow()
    {
        var root = new VisualElement();
        root.styleSheets.Add(styleSheet);
        uiDocument.rootVisualElement.Add(root);
        root.AddToClassList("dwd");
        var window = root.AddElement(new DebugWindow(), window =>
        {
            window.Value = true;
            window.Text = "デバッグウィンドウ";
        });

        var tabContents = Enumerable.Range(0, 3).Select(_ => new VisualElement()).ToArray();

        void ApplyActiveTab(int index)
        {
            for (var i = 0; i < tabContents.Length; i++)
            {
                tabContents[i].style.display = i == index ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        window.AddElement(new VisualElement(), buttons =>
        {
            buttons.AddToClassList("tabs");
            buttons.AddElement(new Button() { text = "シーンデバッグ" }, button => button.clicked += () => ApplyActiveTab(0));
            buttons.AddElement(new Button() { text = "サンプルUI" }, button => button.clicked += () => ApplyActiveTab(1));
            buttons.AddElement(new Button() { text = "新規ウィンドウ" }, button => button.clicked += () => ApplyActiveTab(2));
            ApplyActiveTab(0);
        });
        foreach (var tabContent in tabContents)
        {
            window.AddElement(tabContent);
        }

        SetupDebugWindowTab1(tabContents[0]);
        SetupDebugWindowTab2(tabContents[1]);
        SetupDebugWindowTab3(tabContents[2]);
    }

    void SetupDebugWindowTab1(VisualElement content)
    {
        var sceneCamera = FindObjectOfType<Camera>();
        var directionalLight = FindObjectOfType<Light>();
        var glitch = FindObjectOfType<AnalogGlitch>();
        content.AddElement(new Label() { text = "※ウィンドウ移動は上部を右クリックでドラッグ" });
        content.AddElement(new Foldout() { value = true }, foldout =>
        {
            foldout.text = "KinoGlitch";
            var sliderA = foldout.AddElement(new Slider(0, 1), slider =>
            {
                slider.label = "Jitter";
                slider.value = glitch.scanLineJitter;
                slider.showInputField = true;
                slider.RegisterValueChangedCallback(e => glitch.scanLineJitter = e.newValue);
            });
            var sliderB = foldout.AddElement(new Slider(0, 1), slider =>
            {
                slider.label = "Jump";
                slider.value = glitch.verticalJump;
                slider.showInputField = true;
                slider.RegisterValueChangedCallback(e => glitch.verticalJump = e.newValue);
            });
            var sliderC = foldout.AddElement(new Slider(0, 1), slider =>
            {
                slider.label = "Shake";
                slider.value = glitch.horizontalShake;
                slider.showInputField = true;
                slider.RegisterValueChangedCallback(e => glitch.horizontalShake = e.newValue);
            });
            var sliderD = foldout.AddElement(new Slider(0, 1), slider =>
            {
                slider.label = "ColorDrift";
                slider.value = glitch.colorDrift;
                slider.showInputField = true;
                slider.RegisterValueChangedCallback(e => glitch.colorDrift = e.newValue);
            });
            foldout.AddElement(new VisualElement(), buttons =>
            {
                buttons.AddToClassList("row");
                buttons.AddToClassList("expand");
                buttons.AddElement(new Button(), button =>
                {
                    button.text = "Random";
                    button.clicked += () =>
                    {
                        sliderA.value = Random.value;
                        sliderB.value = Random.value;
                        sliderC.value = Random.value;
                        sliderD.value = Random.value;
                    };
                });
                buttons.AddElement(new Button(), button =>
                {
                    button.text = "Reset";
                    button.clicked += () => { sliderA.value = sliderB.value = sliderC.value = sliderD.value = 0; };
                });
            });
        });
        content.AddElement(new Foldout(), foldout =>
        {
            foldout.text = "Env";
            foldout.AddElement(new Slider(0, 180), slider =>
            {
                slider.label = "Light";
                slider.showInputField = true;
                slider.RegisterValueChangedCallback(e => directionalLight.transform.localRotation = Quaternion.Euler(e.newValue, -30, 0));
            });
            foldout.AddElement(new Slider(10f, 100f), slider =>
            {
                slider.label = "FOV";
                slider.showInputField = true;
                slider.RegisterValueChangedCallback(e => sceneCamera.fieldOfView = e.newValue);
            });
        });
        content.AddElement(new Foldout() { value = true }, foldout =>
        {
            foldout.text = "Position (Camera)";
            foldout.AddElement(new VisualElement(), vector3 =>
            {
                vector3.AddToClassList("row");
                vector3.AddToClassList("expand");
                var x = vector3.AddElement(new TextField() { label = "x", isReadOnly = true, focusable = false });
                var y = vector3.AddElement(new TextField() { label = "y", isReadOnly = true, focusable = false });
                var z = vector3.AddElement(new TextField() { label = "z", isReadOnly = true, focusable = false });
                sceneCamera.ObserveEveryValueChanged(c => c.transform.position).Subscribe(position =>
                {
                    x.value = position.x.ToString("0.00");
                    y.value = position.y.ToString("0.00");
                    z.value = position.z.ToString("0.00");
                }).AddTo(this);
            });
        });
    }

    void SetupDebugWindowTab2(VisualElement content)
    {
        var scrollView = content.AddElement(new ScrollView(), scrollView =>
        {
            scrollView.style.flexGrow = 1;
            scrollView.style.maxHeight = 400;
        });
        scrollView.AddElement(new Foldout() { value = true }, foldout =>
        {
            foldout.text = "Samples";
            foldout.value = true;
            foldout.AddElement(new Toggle("Toggle") { label = "Toggle", value = true });
            foldout.AddElement(new TextField() { label = "TextField", value = "サンプルテキスト" });
            foldout.AddElement(new TextField() { label = "TextField", multiline = true }, textField =>
            {
                textField.style.whiteSpace = WhiteSpace.Normal;
                FetchDummyTextAsync().ToObservable().Subscribe(text => textField.value = text).AddTo(this);
            });
            foldout.AddElement(new Slider(0f, 100f) { label = "Slider" }, slider => slider.showInputField = true);
            foldout.AddElement(new MinMaxSlider(20, 60, 0, 100) { label = "MinMaxSlider" });
            foldout.AddElement(new ProgressBar() { title = "ProgressBar" }, progressBar =>
            {
                progressBar.lowValue = 0;
                progressBar.highValue = 1;
                float Frac(float value) => value - Mathf.FloorToInt(value);
                this.UpdateAsObservable().Select(_ => Frac(progressBar.value + 0.0001f)).Subscribe(v => progressBar.value = v).AddTo(this);
            });
            foldout.AddElement(new VisualElement(), container =>
            {
                container.AddToClassList("row");
                container.AddElement(new RadioButton() { label = "RadioButton" }, b => b.value = true);
                container.AddElement(new RadioButton() { label = "RadioButton" });
                container.AddElement(new RadioButton() { label = "RadioButton" });
            });
        });
        scrollView.AddElement(new Foldout() { value = true }, foldout =>
        {
            foldout.text = "Buttons";
            foldout.value = true;
            foldout.AddElement(new VisualElement(), container =>
            {
                container.style.flexWrap = Wrap.Wrap;
                container.AddToClassList("row");
                for (var i = 1; i <= 20; i++)
                {
                    container.AddElement(new Button() { text = $"{i:d2}" }, button => button.style.flexBasis = Random.Range(30, 100));
                }
            });
        });
    }

    void SetupDebugWindowTab3(VisualElement content)
    {
        content.AddElement(new Button() { text = "新規ウィンドウ" }, button =>
        {
            var count = 0;
            button.clicked += () =>
            {
                FetchDummyTextAsync().ToObservable().Subscribe(dummyText =>
                {
                    var root = uiDocument.rootVisualElement.Q(null, "dwd");
                    var window = root.AddElement(new DebugWindow(), window =>
                    {
                        window.Value = true;
                        window.transform.position = new Vector3(50f * count, 50f * count);
                        window.Text = $"ダミーテキスト {++count}";
                        var textField = window.AddElement(new TextField() { isReadOnly = false, multiline = true });
                        textField.style.whiteSpace = WhiteSpace.Normal;
                        textField.value = dummyText;
                    });
                }).AddTo(this);
            };
        });
    }

    /// <summary>
    /// ダミーテキスト取得
    /// </summary>
    async UniTask<string> FetchDummyTextAsync()
    {
        // thx: https://zenn.dev/sabigara/articles/88757a61fdba8e
        var url = "https://lorem-jpsum.vercel.app/api?sentence_count=5&format=plain&source=ginga-tetsudo";
        var op = await UnityWebRequest.Get(url).SendWebRequest();
        var json = op.downloadHandler.text;
        return JsonUtility.FromJson<DummyTextResponse>(json).content;
    }

    [System.Serializable]
    public class DummyTextResponse
    {
        public string content;
    }
}
