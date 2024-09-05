using DotImaging;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibImageUtilities.ImageTypes.Png
{
    public static class ByteArrayExtensions
    {
        public static bool IsPng(this byte[] img) => Utilities.IsPng(img);

        /// <summary>
        /// Get PNG image from PNG or BMP image.
        /// Copies DPI, PixelFormat, and metadata if converted from BMP.
        /// </summary>
        /// <param name="img"></param>
        /// <returns>Converted BMP or original image</returns>
        public static byte[] GetPng(this byte[] img) => Utilities.GetPng(img);
        /// <summary>
        /// Get PNG image from PNG or BMP image. Sets the DPI in the PNG image, if needed.
        /// Copies PixelFormat and metadata if converted from BMP.
        /// </summary>
        /// <param name="img"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns>Converted BMP or updated orginal</returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] GetPng(this byte[] img, int dpiX, int dpiY = 0) => Utilities.GetPng(img, dpiX, dpiY);
        public static byte[] GetPng(this byte[] img, ImageUtilities.DPI dpi) => Utilities.GetPng(img, dpi);
        public static byte[] GetPng(this byte[] img, PixelFormat pixelFormat) => Utilities.GetPng(img, pixelFormat);

        public static byte[] ConvertBmpToPng(this byte[] bmp, PixelFormat? pixelFormat = null) => Utilities.ConvertBmpToPng(bmp, pixelFormat);


    }
}
