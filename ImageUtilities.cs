using DotImaging;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace LibImageUtilities;

public class ImageUtilities
{
    public const double InchesPerMeter = 39.3701;

    public class DPI
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// Get the DPI of a PNG or BMP image.
    /// </summary>
    /// <param name="image"></param>
    /// <returns>DPI</returns>
    /// <exception cref="ArgumentException"></exception>
    public static DPI GetImageDPI(byte[] image)
    {
        if (ImageUtilities_PNG.IsPng(image))
        {
            return ImageUtilities_PNG.GetPngDPI(image);
        }
        else
        {
            return ImageUtilities_BMP.IsBmp(image) ? ImageUtilities_BMP.GetBmpDPI(image) : throw new ArgumentException("Unsupported image format.");
        }
    }
    /// <summary>
    /// Set the header DPI of a PNG or BMP image.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dpiX"></param>
    /// <param name="dpiY"></param>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] SetImageDPI(byte[] image, int dpiX, int dpiY = 0)
    {
        if (dpiX <= 0)
        {
            throw new ArgumentException("DPI value must be greater than zero.");
        }

        if (dpiY <= 0)
        {
            dpiY = dpiX; // Use dpiX if dpiY is not provided or invalid
        }

        if (ImageUtilities_PNG.IsPng(image))
        {
            return ImageUtilities_PNG.SetPngDPI(image, dpiX, dpiY);
        }
        else
        {
            return ImageUtilities_BMP.IsBmp(image) ? ImageUtilities_BMP.SetBitmapDPI(image, dpiX, dpiY) : throw new ArgumentException("Unsupported image format.");
        }
    }

    /// <summary>
    /// Get the UID of the entire byte array.
    /// </summary>
    /// <param name="image"></param>
    /// <returns>SHA256 Hash string with hyphen removed</returns>
    public static string GetImageUID(byte[] image)
    {
        try
        {
            using SHA256 md5 = SHA256.Create();
            return BitConverter.ToString(md5.ComputeHash(image)).Replace("-", String.Empty);

        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
    /// <summary>
    /// Get the UID of the image data array only. No header information.
    /// </summary>
    /// <param name="image"></param>
    /// <returns>SHA256 Hash string with hyphen removed</returns>
    public static string GetImageDataUID(byte[] image)
    {
        try
        {
            byte[] imageData;

            if (ImageUtilities_PNG.IsPng(image))
            {
                imageData = ImageUtilities_PNG.ExtractIDATData(image);
            }
            else
            {
                imageData = ImageUtilities_BMP.IsBmp(image) ? ImageUtilities_BMP.ExtractBitmapData(image) : throw new ArgumentException("Unsupported image format.");
            }

            using SHA256 sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(imageData)).Replace("-", string.Empty);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static int DotsPerInch(int dpm) => (int)Math.Round(dpm / InchesPerMeter);
    public static int DotsPerMeter(int dpi) => (int)Math.Round(dpi * InchesPerMeter);

    //Seek #FileIndex, 7   ' position of balance information
    //Get #FileIndex, , BalanceInfo
    //If BalanceInfo = Asc("C") Or BalanceInfo = Asc("3") Then
    //  ColorFlag = Chr(BalanceInfo)
    //End If
    //public static void SetBitmapColorFlag(byte[] image, char balanceInfo)
    //{
    //    byte[] value = BitConverter.GetBytes(balanceInfo);

    //    int i = 7;
    //    foreach (byte b in value)
    //        image[i++] = b;
    //}

    //public static byte[] RotatePNG(byte[] img, double angle = 0)
    //{

    //    System.Windows.Media.Imaging.PngBitmapEncoder encoder = new();
    //    using MemoryStream ms = new(img);
    //    using MemoryStream stream = new();

    //    // Create a BitmapImage from the byte array
    //    BitmapImage bitmapImage = new();
    //    bitmapImage.BeginInit();
    //    bitmapImage.StreamSource = ms;
    //    bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
    //    bitmapImage.EndInit();
    //    bitmapImage.Freeze();

    //    // Apply rotation if angle is not zero
    //    if (angle != 0)
    //    {
    //        TransformedBitmap transformedBitmap = new(
    //            bitmapImage, new System.Windows.Media.RotateTransform(angle));
    //        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(transformedBitmap));
    //    }
    //    else
    //    {
    //        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));
    //    }

    //    encoder.Save(stream);
    //    stream.Close();

    //    return stream.ToArray();

    //}

    //public static byte[] AddOverlayPNG(byte[] img, byte[] overlay)
    //{

    //    System.Windows.Media.DrawingGroup dg = new();

    //    BitmapImage renderBitmap1 = CreateBitmap(img);
    //    BitmapImage renderBitmap2 = CreateBitmap(overlay);

    //    System.Windows.Media.ImageDrawing id1 = new(renderBitmap1, new System.Windows.Rect(0, 0, renderBitmap1.Width, renderBitmap1.Height));
    //    System.Windows.Media.ImageDrawing id2 = new(renderBitmap2, new System.Windows.Rect(0, 0, renderBitmap2.Width, renderBitmap2.Height));

    //    dg.Children.Add(id1);
    //    dg.Children.Add(id2);

    //    RenderTargetBitmap combinedImg = new(
    //        (int)renderBitmap1.Width,
    //        (int)renderBitmap1.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);

    //    System.Windows.Media.DrawingVisual dv = new();
    //    using (System.Windows.Media.DrawingContext dc = dv.RenderOpen())
    //    {
    //        dc.DrawDrawing(dg);
    //    }

    //    combinedImg.Render(dv);

    //    System.Windows.Media.Imaging.PngBitmapEncoder encoder = new();
    //    using MemoryStream stream = new();
    //    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(combinedImg));

    //    encoder.Save(stream);
    //    stream.Close();

    //    return stream.ToArray();

    //}

    ////public static byte[] ConvertToBmp(byte[] img)
    ////{
    ////    System.Windows.Media.Imaging.BmpBitmapEncoder encoder = new();
    ////    using var ms = new System.IO.MemoryStream(img);
    ////    using MemoryStream stream = new();
    ////    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(ms));
    ////    encoder.Save(stream);
    ////    stream.Close();

    ////    return stream.ToArray();
    ////}
    ////public static byte[] ConvertToBmp(byte[] img, int dpi)
    ////{
    ////    System.Windows.Media.Imaging.BmpBitmapEncoder encoder = new();
    ////    using var ms = new System.IO.MemoryStream(img);
    ////    using MemoryStream stream = new();
    ////    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(ms));
    ////    encoder.Save(stream);
    ////    stream.Close();

    ////    byte[] ret = stream.ToArray();

    ////    if (dpi > 0)
    ////        SetBitmapDPI(ret, dpi);

    ////    return ret;
    ////}
    //public static System.Windows.Media.Imaging.BitmapImage CreateBitmap(byte[] data, int decodePixelWidth = 0)
    //{
    //    if (data == null || data.Length < 2)
    //        return null;

    //    try
    //    {
    //        System.Windows.Media.Imaging.BitmapImage img = new();

    //        using (MemoryStream memStream = new(data))
    //        {
    //            img.BeginInit();
    //            img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
    //            img.StreamSource = memStream;
    //            img.DecodePixelWidth = decodePixelWidth;
    //            img.EndInit();
    //            img.Freeze();

    //        }
    //        return img;
    //    }
    //    catch { }

    //    return null;
    //}

    //public static byte[] ImageToBytes(System.Drawing.Image image)
    //{
    //    System.Drawing.ImageConverter converter = new();
    //    return (byte[])converter.ConvertTo(image, typeof(byte[]));
    //}

    //private static byte[] imageToBytes(System.Windows.Media.ImageSource imageSource)
    //{
    //    System.Windows.Media.Imaging.PngBitmapEncoder encoder = new();
    //    byte[] bytes = null;

    //    if (imageSource is System.Windows.Media.Imaging.BitmapSource bitmapSource)
    //    {
    //        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapSource));

    //        using MemoryStream stream = new();
    //        encoder.Save(stream);
    //        bytes = stream.ToArray();
    //    }

    //    return bytes;
    //}
    //public static byte[] ImageToBytes(this System.Windows.Media.DrawingImage source)
    //{
    //    System.Windows.Media.DrawingVisual drawingVisual = new();
    //    System.Windows.Media.DrawingContext drawingContext = drawingVisual.RenderOpen();
    //    drawingContext.DrawImage(source, new System.Windows.Rect(new System.Windows.Point(0, 0), new System.Windows.Size(source.Width, source.Height)));
    //    drawingContext.Close();

    //    RenderTargetBitmap bmp = new((int)source.Width, (int)source.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
    //    bmp.Render(drawingVisual);
    //    return imageToBytes(bmp);
    //}

    //public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this System.Windows.Media.DrawingImage source)
    //{
    //    System.Windows.Media.DrawingVisual drawingVisual = new();
    //    System.Windows.Media.DrawingContext drawingContext = drawingVisual.RenderOpen();
    //    drawingContext.DrawImage(source, new System.Windows.Rect(new System.Windows.Point(0, 0), new System.Windows.Size(source.Width, source.Height)));
    //    drawingContext.Close();

    //    RenderTargetBitmap bmp = new((int)source.Width, (int)source.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
    //    bmp.Render(drawingVisual);
    //    return bmp;
    //}

    public static void RedrawFiducial(string path, bool is300)
    {
        // load your photo
        using FileStream fs = new(path, FileMode.Open);
        Bitmap photo = (Bitmap)Bitmap.FromStream(fs);
        fs.Close();

        Bitmap newmap = new(photo.Width, photo.Height);
        newmap.SetResolution(photo.HorizontalResolution, photo.VerticalResolution);
        //if (photo.Height != 2400)
        //    File.AppendAllText($"{UserDataDirectory}\\Small Images List", Path.GetFileName(path));

        //if (is300)
        //{//600 DPI
        //    if ((photo.Height > 2400 && photo.Height != 4800) || photo.Height < 2000)
        //        return;
        //}
        //else
        //{//300 DPI
        //    if ((photo.Height > 1200) || photo.Height < 1000)
        //        return;
        //}

        using Graphics graphics = Graphics.FromImage(newmap);
        graphics.DrawImage(photo, 0, 0, photo.Width, photo.Height);

        if (is300)
        {//300 DPI
            graphics.FillRectangle(Brushes.White, 0, 976, 150, photo.Height - 976);
            graphics.FillRectangle(Brushes.Black, 15, 975, 45, 45);
        }
        else
        {
            graphics.FillRectangle(Brushes.White, 0, 1900, 195, photo.Height - 1900);
            graphics.FillRectangle(Brushes.Black, 30, 1950, 90, 90);
        }

        newmap.Save(path, ImageFormat.Png);

    }
}
