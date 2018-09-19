using Xamarin.Forms;


namespace WiFiManager
{
    public class ButtonTriggerAction : TriggerAction<VisualElement>
    {
        public Color BackgroundColor { get; set; }

        protected override void Invoke(VisualElement visual)
        {
            var button = visual as Button;
            if (button == null) return;
            if (BackgroundColor != null) button.BackgroundColor = BackgroundColor;
        }
    }
}
