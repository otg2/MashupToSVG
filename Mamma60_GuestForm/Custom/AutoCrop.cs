using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipperLib;
using Pathh = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Mamma60_GuestForm.Custom;
using Mamma60_GuestForm.Models;
using Mamma60_GuestForm.Data;
using System.Text.Json;


namespace Mamma60_GuestForm.Custom
{
    public static class AutoCrop
    {
        

        public static List<Shape> GenerateShapes(string sourceFolder, string targetFolder)
        {
            int assignIndex = 1;
            int assignedWidth = 0;
            int assignedHeight = 0;

            List<Shape> shapes = new List<Shape>();

            var files = GetImagePaths(sourceFolder);
            foreach (var file in files)
            {
                if (file.Contains("boundary")) continue; // Skip adding boundary in shapes, that comes later
                var shape = new Shape(file, sourceFolder, targetFolder, assignIndex, new Point(assignedWidth, assignedHeight));
                shapes.Add(shape);

                assignIndex++;
                // TODO: Handle how initial coordinates are given
                assignedWidth += Convert.ToInt32(shape.ImageRef.Width / shapes.Count);
                assignedHeight += Convert.ToInt32(shape.ImageRef.Height / shapes.Count);
            }

            return shapes;
        }



        public static string FilepathToFilename(string path, string sourceFolder) => path.Replace(sourceFolder, "").Replace("\\", "").Replace(".png", "");

        private static List<string> GetImagePaths(string sourceFolder)
        {
            var filepaths = new List<string>();
            var ext = new List<string> { "png" };

            var dir = Directory.GetCurrentDirectory();
            var files = Directory
                .EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach (var file in files)
                filepaths.Add(file);
            return filepaths;
        }

        public static Bitmap FilepathToBitmap(string filePath, bool shouldScale)
        {
            Image image;
            Bitmap bitmap;
            using (Stream bmpStream = File.Open(filePath, FileMode.Open))
            {
                image = Image.FromStream(bmpStream);
                bitmap = new Bitmap(image);
            }
            // Scaling
            if(shouldScale)
                if (false)
                    //return ResizeImage(image, bitmap.Width / Config.ScaleFactor, bitmap.Height / Config.ScaleFactor);
                    return ResizeImage(image, bitmap.Width / 10, bitmap.Height / 10);
                //bitmap = new Bitmap(bitmap, new Size(bitmap.Width / Config.ScaleFactor, bitmap.Height / Config.ScaleFactor));
                else
            {
                    var targetFactor = (double)bitmap.Width / 350;
                    var width = Convert.ToInt32(bitmap.Width / targetFactor);
                    var height = Convert.ToInt32(bitmap.Height / targetFactor);
                    return ResizeImage(image, width, height);
                    //bitmap = new Bitmap(bitmap, new Size(width, height));
                }
            
            return bitmap;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // TODO: Fix bounding bound of shape 2 (curved forms)
        // TODO: When that is done, fix ordering of point. Can be done via clipper and no path intersection
        public static Image TrimWhiteSpace(Shape shape, Bitmap bmp)
        {
            if (Image.GetPixelFormatSize(bmp.PixelFormat) != 32)
                throw new InvalidOperationException("Autocrop currently only supports 32 bits per pixel images.");

            // Initialize variables
            var cropColor = Color.Transparent;

            var bottom = 0;
            var left = bmp.Width; // Set the left crop point to the width so that the logic below will set the left value to the first non crop color pixel it comes across.
            var right = 0;
            var top = bmp.Height; // Set the top crop point to the height so that the logic below will set the top value to the first non crop color pixel it comes across.

            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            // Trim image and store bounding box
            unsafe
            {
                var dataPtr = (byte*)bmpData.Scan0;

                for (var y = 0; y < bmp.Height; y++)
                {
                    for (var x = 0; x < bmp.Width; x++)
                    {
                        var rgbPtr = dataPtr + (x * 4);

                        var b = rgbPtr[0];
                        var g = rgbPtr[1];
                        var r = rgbPtr[2];
                        var a = rgbPtr[3];

                        // If any of the pixel RGBA values don't match and the crop color is not transparent, or if the crop color is transparent and the pixel A value is not transparent
                        if (
                            //(cropColor.A > 0 && (b != cropColor.B || g != cropColor.G || r != cropColor.R || a != cropColor.A)) 
                            //|| 
                            (cropColor.A == 0 && a > 150)
                            )
                        {
                            if (x < left)
                                left = x;

                            if (x >= right)
                                right = x + 1;

                            if (y < top)
                                top = y;

                            if (y >= bottom)
                                bottom = y + 1;
                        }

                        /*
                         if( y % heightInterval ==0)
                        {
                            boundingLeft = bitmap.widht;
                            boundingRight = 0;
                        }
                         */
                    }

                    dataPtr += bmpData.Stride;
                }
            }

            bmp.UnlockBits(bmpData);

            if (left < right && top < bottom)
                return bmp.Clone(new Rectangle(left, top, right - left, bottom - top), bmp.PixelFormat);

            return null; // Entire image should be cropped, so just return null
        }

        public static (int,int) GetTopLeftCorner(Shape shape, Bitmap bmp)
        {
            if (Image.GetPixelFormatSize(bmp.PixelFormat) != 32)
                throw new InvalidOperationException("Autocrop currently only supports 32 bits per pixel images.");

            // Initialize variables
            var cropColor = Color.Transparent;

            var bottom = 0;
            var left = bmp.Width; // Set the left crop point to the width so that the logic below will set the left value to the first non crop color pixel it comes across.
            var right = 0;
            var top = bmp.Height; // Set the top crop point to the height so that the logic below will set the top value to the first non crop color pixel it comes across.

            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            // Trim image and store bounding box
            unsafe
            {
                var dataPtr = (byte*)bmpData.Scan0;

                for (var y = 0; y < bmp.Height; y++)
                {
                    for (var x = 0; x < bmp.Width; x++)
                    {
                        var rgbPtr = dataPtr + (x * 4);

                        var b = rgbPtr[0];
                        var g = rgbPtr[1];
                        var r = rgbPtr[2];
                        var a = rgbPtr[3];

                        // If any of the pixel RGBA values don't match and the crop color is not transparent, or if the crop color is transparent and the pixel A value is not transparent
                        if (
                            (cropColor.A == 0 && a > 150)
                            )
                        {
                            if (x < left)
                                left = x;

                            if (x >= right)
                                right = x + 1;

                            if (y < top)
                                top = y;

                            if (y >= bottom)
                                bottom = y + 1;
                        }
                    }

                    dataPtr += bmpData.Stride;
                }
            }

            bmp.UnlockBits(bmpData);

            return (left, top);
        }

        // TODO: Combine this with upper function to skip scanning image twice
        public static List<Point> GenerateBoundingBox(Shape shape, int n)
        {
            var bitmap = new Bitmap(shape.ImageRef);

            // density - split image into NxN frames
            float densityPercentage = 1.0f / n;
            var heightInterval = Convert.ToInt32((bitmap.Height * densityPercentage) / 2);

            var boundingBoxPoints = new List<Point>();

            if (Image.GetPixelFormatSize(bitmap.PixelFormat) != 32)
                throw new InvalidOperationException("Autocrop currently only supports 32 bits per pixel images.");

            // Initialize variables
            var cropColor = Color.Transparent;

            var left = bitmap.Width;
            var right = 0;

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // Trim image and store bounding box
            unsafe
            {
                var dataPtr = (byte*)bmpData.Scan0;

                for (var y = 0; y < bitmap.Height; y += heightInterval)
                {
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var rgbPtr = dataPtr + (x * 4);

                        var b = rgbPtr[0];
                        var g = rgbPtr[1];
                        var r = rgbPtr[2];
                        var a = rgbPtr[3];

                        // If any of the pixel RGBA values don't match and the crop color is not transparent, or if the crop color is transparent and the pixel A value is not transparent
                        if ((cropColor.A > 0 && (b != cropColor.B || g != cropColor.G || r != cropColor.R || a != cropColor.A)) || (cropColor.A == 0 && a != 0))
                        {
                            if (x < left)
                                left = x;

                            if (x >= right)
                                right = x + 1;
                        }
                    }

                    var pointLeft = new Point(left, y);
                    var pointRight = new Point(right, y);
                    boundingBoxPoints.Add(new Point(left, y));
                    boundingBoxPoints.Add(new Point(right, y));

                    left = bitmap.Width;
                    right = 0;

                    dataPtr += bmpData.Stride * heightInterval;
                }
            }
            bitmap.UnlockBits(bmpData);

            var indexedNums = boundingBoxPoints.Select((num, idx) => new { num, idx });
            var evens = indexedNums.Where(x => x.idx % 2 == 0).OrderByDescending(x => x.idx);
            var odds = indexedNums.Where(x => x.idx % 2 == 1);
            var endSequence = odds.Concat(evens).Select(x => x.num).ToList();

            return endSequence;
        }

        // TODO: Add things below to another Storing class of some sort
        /*------------------------------------------------------------------------------------------*/


        public static void GenerateExpandedImages(string sourceFolder, UserProfileContext context)
        {
            ClearDirectory("FinalData/Final_Images_Clipped");
            ClearDirectory("FinalData/Final_UserInfo");
            ClearDirectory("FinalData/Final_Images_Nobg");
            

            var artworkRef = new List<ArtworkImage>();//new ArtworkRef();

            var shapes = new List<Shape>();

            var files = GetImagePaths(sourceFolder);
            foreach (var file in files)
            {
                var shape = new Shape(file, sourceFolder);

                var layerSaveSplitByComma = shape.SourcePath.Split(",");
                var imageId = layerSaveSplitByComma[3].Substring(0, 36);

                var polyline = "";
                var boundingPoints = shape.BoundingBoxPoints;
                for (var i = 0; i < boundingPoints.Count; i++)
                {
                    polyline += 
                            (shape.BestLocation.X + boundingPoints[i].X).ToString() 
                        +   "," 
                        +   (shape.BestLocation.Y + boundingPoints[i].Y).ToString() 
                        +   " ";
                }
                artworkRef.Add(new ArtworkImage(
                        imageId,
                        shape.BestLocation,
                        "FinalData/Final_Images_Clipped/" + imageId + ".png",
                        "FinalData/Final_Images_Nobg/" + imageId + ".png", 
                        polyline,
                        shape.Location.X,
                        shape.Location.Y
                    ));

                shape.FileName = imageId;
                shapes.Add(shape);
            }
            foreach (var shape in shapes)
            {
                // Safe image
                SaveImage("FinalData/Final_Images_Clipped", shape);
                // Generate JSON file
                var userInfo = context.ProfileViewModel.Where(x => x.Id == shape.FileName).FirstOrDefault();
                var userInfoRef = new UserDisplayInfo(userInfo.Name, userInfo.Memory, userInfo.HexColor);
                File.WriteAllText(@"FinalData\Final_UserInfo\" + String.Format("{0}.json",shape.FileName), JsonSerializer.Serialize(userInfoRef),Encoding.UTF8);
            }


            // IMAGES CLIPPED NO BG
            var shapesNobg = new List<Shape>();
            var files_noBG = GetImagePaths("images_nobg");
            foreach (var file in files_noBG)
            {
                var shape = new Shape(file, "images_nobg");

                var imageId = shape.SourcePath;

                shape.FileName = imageId.Replace(".png","");
                shapesNobg.Add(shape);
            }
            foreach (var shape in shapesNobg)
            {
                SaveImage("FinalData/Final_Images_Nobg", shape);
            }


            var serializedArtworkReference = JsonSerializer.Serialize(artworkRef);
            File.WriteAllText(@"FinalData\artwork.json", serializedArtworkReference);
        }

        private static void ClearDirectory(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private static void SaveImage(string targetFolder, Shape shape)
        {
            var expandedPath = targetFolder + "/" + shape.FileName + ".png";
            shape.ExpandedPath = expandedPath;
            shape.ImageRef.Save(expandedPath, ImageFormat.Png);
        }
    }
}
