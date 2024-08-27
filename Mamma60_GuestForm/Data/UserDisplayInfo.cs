using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mamma60_GuestForm.Data
{
    public class UserDisplayInfo
    {
        public string Name { get; set; }

        public string Memory { get; set; }

        public string HexColor { get; set; }
        public UserDisplayInfo(string name, string memory, string hex)
        {
            Name = name;
            Memory = memory;
            HexColor = hex;
        }
    }
}
