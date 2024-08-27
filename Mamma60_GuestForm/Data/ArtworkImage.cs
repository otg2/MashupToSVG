using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Mamma60_GuestForm.Data
{
    public class ArtworkImage
    {

        public string ImageId { get; set; }
        public int Left { get; set; } 
        public int Top { get; set; }
        public string PreprocessedPath { get; set; }
        public string OriginalImagePath { get; set; }
        public string SvgPolyline { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public ArtworkImage(string imageId, Point position, string preprocessedPath, string originalImagePath, string svgPolyline, int width, int height)
        {

            ImageId = imageId;
            Left = position.X;
            Top = position.Y;
            PreprocessedPath = preprocessedPath;
            OriginalImagePath = originalImagePath;
            SvgPolyline = svgPolyline;
            Width = width;
            Height = height;

        }
    }
}
