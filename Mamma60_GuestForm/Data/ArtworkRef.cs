using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mamma60_GuestForm.Data
{
    public class ArtworkRef
    {
        public List<ArtworkImage> Artworks;
        public int Width { get; set; }
        public int Height { get; set; }
        public ArtworkRef() {
            Artworks = new List<ArtworkImage>();
        }
    }
}
