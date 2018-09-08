using System;
using System.Collections.Generic;
using System.Linq;
using WiFiManager.Common.BusinessObjects;
using Xamarin.Forms;

namespace WiFiManager
{
    public class ItemTemplateSelector : Xamarin.Forms.DataTemplateSelector
    {
        public DataTemplate DefaultnDataTemplate { get; set; }
        public DataTemplate NoteDataTemplate { get; set; }

        public object FromLV { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (null == item)
                return DefaultnDataTemplate;
            if (item.GetType() == typeof(WifiNetworkDto))
            {
                return NoteDataTemplate;
            }


            return DefaultnDataTemplate;
        }
    }
}
