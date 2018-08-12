using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common.BusinessObjects
{
    public abstract class BaseObj
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual bool Fit(string filter)
        {
            return Name.Contains(filter);
        }
    }
}
