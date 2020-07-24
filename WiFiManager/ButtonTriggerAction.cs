using System;
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


		public static readonly BindableProperty BgColorProperty = BindableProperty.Create("BgColor", typeof(Color), typeof(Color)
            ,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var control = bindable as Button;
                    var changingFrom = oldValue as string;
                    var changingTo = newValue as string;
                });
		public Color BackgroundColorValue
		{
			get { return BackgroundColor; }
			set { BackgroundColor = value; }
		}
	}
}
