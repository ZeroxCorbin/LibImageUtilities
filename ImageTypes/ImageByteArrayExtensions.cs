namespace LibImageUtilities.ImageTypes;
public static class ImageByteArrayExtensions
{
    public static int GetImageWidth(this byte[] image) => ImageUtilities.GetImageWidth(image);
    public static int GetImageHeight(this byte[] image) => ImageUtilities.GetImageHeight(image);

    public static ImageUtilities.DPI GetImageDPI(this byte[] image) => ImageUtilities.GetImageDPI(image);
    public static byte[] SetImageDPI(this byte[] image, ImageUtilities.DPI dpi) => ImageUtilities.SetImageDPI(image, dpi);
    public static byte[] SetImageDPI(this byte[] image, int dpiX, int dpiY = 0) => ImageUtilities.SetImageDPI(image, dpiX, dpiY);
    public static byte[] SetImageDPI(this byte[] image, int dpi) => ImageUtilities.SetImageDPI(image, dpi);

    public static System.Drawing.Imaging.PixelFormat GetImagePixelFormat(this byte[] image) => ImageUtilities.GetImagePixelFormat(image);
    public static byte[] SetImagePixelFormat(this byte[] image, System.Drawing.Imaging.PixelFormat pixelFormat) => ImageUtilities.SetImagePixelFormat(image, pixelFormat);

    public static string GetImageUID(this byte[] image) => ImageUtilities.GetImageUID(image);
    public static string GetImageDataUID(this byte[] image) => ImageUtilities.GetImageDataUID(image);
}
