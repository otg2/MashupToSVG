using ClipperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mamma60_GuestForm.Custom;

namespace Mamma60_GuestForm.Models
{
    public class Shape
    {
        public int Index { get; set; }
        public string FileName { get; set; }
        public string SourcePath { get; set; }
        public string ProcessedPath { get; set; }
        public string ExpandedPath { get; set; }
        public List<Point> BoundingBoxPoints { get; set; }
        public Shape BoundaryShapeRef { get; set; }

        public Image ImageRef { get; set; }

        public Point Location { get; set; }
        public Point BestLocation { get; set; }

        public Color DrawColor { get; set; }

        public float Rotation { get; set; }
        public float BestRotation { get; set; }
        public double ErrorContribution { get; set; }

        // Real shape
        public Shape(string filePath, string sourceFolder, string targetFolder, int index, Point startingLocation)
        {
            var fileName = AutoCrop.FilepathToFilename(filePath, sourceFolder);
            var fileAsBitmap = AutoCrop.FilepathToBitmap(filePath, true);

            this.SourcePath = fileName;
            this.FileName = fileName;

            this.ImageRef = AutoCrop.TrimWhiteSpace(this, fileAsBitmap);
            this.BoundingBoxPoints = AutoCrop.GenerateBoundingBox(this, 10);

            var removedOutFilename = this.FileName.Replace(".out", "");

            var processedPath = targetFolder + "//" + removedOutFilename + ".png";
            this.ProcessedPath = processedPath;
            this.ImageRef.Save(processedPath, ImageFormat.Png);

            this.Location = startingLocation;
            this.Rotation = 0;
            this.BestLocation = this.Location;

            this.Index = index;
        }

        public Shape(string filePath, string targetFolder)
        {
            var fileName = filePath.Replace(targetFolder, "").Replace(@"\\","");
            // TODO: Will images in artwork be small enough that they need no scaling? LOOK AT DIS

            // TODO: Need to run images_clipped without scaling to get the real popup image. That is, images_clipped without scale
            var fileAsBitmap = AutoCrop.FilepathToBitmap(filePath, false);

            this.SourcePath = fileName;
            this.FileName = fileName;

            this.ImageRef = AutoCrop.TrimWhiteSpace(this, fileAsBitmap);
            this.BoundingBoxPoints = AutoCrop.GenerateBoundingBox(this, 10);

            var removedOutFilename = this.FileName.Replace(".out", "");

            var processedPath = targetFolder + "//" + removedOutFilename;
            this.ProcessedPath = processedPath;

            var leftTop = AutoCrop.GetTopLeftCorner(this, fileAsBitmap);
            this.BestLocation = new Point(leftTop.Item1, leftTop.Item2);
            this.Location = new Point(fileAsBitmap.Width, fileAsBitmap.Height);
        }

        // Replica - TODO: Create this as a separate class?
        public Shape(Point location, float rotation, List<Point> boundingBoxPoints, Image imageRef)
        {
            this.Location = location;
            this.Rotation = rotation;
            this.BoundingBoxPoints = boundingBoxPoints;
            this.ImageRef = imageRef;
        }

        // TODO: Remove the get from functions and simply name CurrentDrawingCoordinates and BestDrawingCoordinates
        public List<IntPoint> GetDrawingCoordinates()
        {
            return GetBoundingCoordinates(Location, Rotation);
        }

        public List<IntPoint> GetBestDrawingCoordinates()
        {
            return GetBoundingCoordinates(BestLocation, BestRotation);
        }

        private List<IntPoint> GetBoundingCoordinates(Point referencePointLocation, double referenceRotation)
        {
            var drawCoordinates = new List<IntPoint>();

            Point center = new Point(referencePointLocation.X + ImageRef.Width / 2, referencePointLocation.Y + ImageRef.Height / 2);
            // Translate and rotate
            foreach (var point in BoundingBoxPoints)
            {
                Point translatedPoint = new Point(point.X + referencePointLocation.X, point.Y + referencePointLocation.Y);
                drawCoordinates.Add(new IntPoint(translatedPoint.X, translatedPoint.Y));
            }
            return drawCoordinates;
        }


        

    }
}
