using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadanie3TovarV1.ModelsDB
{
    public partial class Product
    {
        public string ProductPhotoFullPath
        {
            get
            {
                if(this.ProductPhoto == null)
                {
                    return "Image\\picture.png";
                }
                else
                {
                    string namePhoto = Directory.GetCurrentDirectory() + "\\image\\" + ProductPhoto;
                    return namePhoto;
                }
                //if (string.IsNullOrEmpty(ProductPhoto))
                //    return "Image\\picture.png";

                //// Проверяем существует ли файл
                //string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Image", ProductPhoto);
                //if (File.Exists(fullPath))
                //    return $"Image\\{ProductPhoto}";
                //else
                //    return "Image\\picture.png";
            }
        }
    }
}
