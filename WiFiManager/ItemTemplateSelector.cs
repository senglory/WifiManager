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

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (null == item)
                return DefaultnDataTemplate;
            if (item.GetType() == typeof(WifiNetwork))
            {
                return NoteDataTemplate;
            }


            return DefaultnDataTemplate;
        }
    }
}
