using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WiFiManager.Common.BusinessObjects;

namespace WiFiManager.Common
{
    public class WifiNetworksObservableCollection : ObservableCollection<WifiNetworkDto>
    {
        public WifiNetworksObservableCollection()
            : base()
        {

        }

        public WifiNetworksObservableCollection(IEnumerable<WifiNetworkDto> lst)
            :base(lst)
        {

        }

        /// <summary>
        /// Avoids adding duplicated BssID
        /// </summary>
        /// <param name="dto"></param>
        public void TryAdd(WifiNetworkDto dto)
        {
            foreach(var item in this.Items)
            {
                if (item.BssID == dto.BssID)
                    return;
            }
            Add(dto);
        }
    }
}
