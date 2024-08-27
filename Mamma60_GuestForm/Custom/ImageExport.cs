using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mamma60_GuestForm.Models;

namespace Mamma60_GuestForm.Custom
{
    public class ImageExport
    {
        private ProfileViewModel _model;

        public ImageExport(ProfileViewModel model)
        {
            _model = model;
        }

        public void SaveImage()
        {
            string filename = _model.Id;
            byte[] imageByteArray = _model.ProfilePicture;
            string saveImagePath = "images/" + filename + ".png";

            if (File.Exists(saveImagePath))
            {
                File.Delete(saveImagePath);
            }

            File.WriteAllBytes(saveImagePath, imageByteArray);
        }
    }
}
