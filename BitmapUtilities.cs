﻿using System;
using System.Linq;
using System.Text;

namespace LibImageUtilities;
public static class DrawingBitmap
{
    public static System.Drawing.Bitmap GetBitmap(byte[] image)
    {
        using System.IO.MemoryStream ms = new(image);
        return new System.Drawing.Bitmap(ms);
    }
    public static System.Drawing.Bitmap LoadBitmap(string path) => new(path);
    public static void SaveBitmap(System.Drawing.Bitmap bitmap, string path) => bitmap.Save(path);

    public static System.Drawing.Bitmap CreateBitmap(int width, int height) => new(width, height);
    public static System.Drawing.Bitmap CreateBitmap(int width, int height, System.Drawing.Imaging.PixelFormat format) => new(width, height, format);
    public static System.Drawing.Bitmap CreateBitmap(int width, int height, System.Drawing.Imaging.PixelFormat format, System.Drawing.Color color)
    {
        System.Drawing.Bitmap bitmap = new(width, height, format);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            g.Clear(color);
        return bitmap;
    }
    public static System.Drawing.Bitmap CreateBitmap(int width, int height, System.Drawing.Color color) => CreateBitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb, color);
    public static System.Drawing.Bitmap CreateBitmap(int width, int height, byte[] bytes)
    {
        System.Drawing.Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
        bitmap.UnlockBits(data);
        return bitmap;
    }
    public static System.Drawing.Bitmap CreateBitmap(byte[] image)
    {
        using System.IO.MemoryStream ms = new(image);
        return new System.Drawing.Bitmap(ms);
    }
    public static System.Drawing.Bitmap CreateBitmap(byte[] image, int dpiX, int dpiY)
    {
        SetDPI(image, dpiX, dpiY);
        using System.IO.MemoryStream ms = new(image);

        System.Drawing.Bitmap bitmap = new(ms);

        return bitmap;
    }

    public static System.Drawing.Bitmap CreateRandomBitmap(int width, int height)
    {
        Random random = new();
        System.Drawing.Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int alpha = 255; // Full opacity
                int red = random.Next(256); // 0 to 255
                int green = random.Next(256); // 0 to 255
                int blue = random.Next(256); // 0 to 255
                System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(alpha, red, green, blue);
                bitmap.SetPixel(x, y, randomColor);
            }
        return bitmap;
    }
    public static System.Drawing.Bitmap CreateRandomBitmapFast(int width, int height)
    {
        Random random = new();
        System.Drawing.Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Drawing.Rectangle rect = new(0, 0, width, height);
        System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

        int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
        int byteCount = bitmapData.Stride * bitmapData.Height;
        byte[] pixels = new byte[byteCount];

        for (int y = 0; y < height; y++)
        {
            int yPos = y * bitmapData.Stride;
            for (int x = 0; x < width; x++)
            {
                int xPos = x * bytesPerPixel;
                pixels[yPos + xPos + 3] = 255; // Alpha channel (fully opaque)
                pixels[yPos + xPos + 2] = (byte)random.Next(256); // Red
                pixels[yPos + xPos + 1] = (byte)random.Next(256); // Green
                pixels[yPos + xPos] = (byte)random.Next(256); // Blue
            }
        }

        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
        bitmap.UnlockBits(bitmapData);

        return bitmap;
    }

    public static byte[] GetBytes(this System.Drawing.Bitmap bitmap)
    {
        using System.IO.MemoryStream ms = new();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        return ms.ToArray();
    }

    public static System.Drawing.Bitmap ResizeBitmap(System.Drawing.Bitmap bitmap, int width, int height)
    {
        System.Drawing.Bitmap resized = new(width, height);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(resized))
            g.DrawImage(bitmap, 0, 0, width, height);
        return resized;
    }
    public static System.Drawing.Bitmap CropBitmap(System.Drawing.Bitmap bitmap, int x, int y, int width, int height)
    {
        System.Drawing.Bitmap cropped = new(width, height);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(cropped))
            g.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, width, height), new System.Drawing.Rectangle(x, y, width, height), System.Drawing.GraphicsUnit.Pixel);
        return cropped;
    }
    public static System.Drawing.Bitmap RotateBitmap(System.Drawing.Bitmap bitmap, float angle)
    {
        System.Drawing.Bitmap rotated = new(bitmap.Width, bitmap.Height);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(rotated))
        {
            g.TranslateTransform(bitmap.Width / 2, bitmap.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-bitmap.Width / 2, -bitmap.Height / 2);
            g.DrawImage(bitmap, 0, 0);
        }
        return rotated;
    }
    public static System.Drawing.Bitmap FlipBitmap(System.Drawing.Bitmap bitmap, bool horizontal, bool vertical)
    {
        System.Drawing.Bitmap flipped = new(bitmap.Width, bitmap.Height);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(flipped))
        {
            if (horizontal)
            {
                g.ScaleTransform(-1, 1);
                g.TranslateTransform(-bitmap.Width, 0);
            }
            if (vertical)
            {
                g.ScaleTransform(1, -1);
                g.TranslateTransform(0, -bitmap.Height);
            }
            g.DrawImage(bitmap, 0, 0);
        }
        return flipped;
    }

    #region DPI
    private const double InchesPerMeter = 39.3701;
    public static void SetDPI(byte[] image, int dpiX, int dpiY)
    {
        if (IsPng(image))
            SetPngDPI(image, dpiX, dpiY);
        else
            SetBitmapDPI(image, dpiX, dpiY);
    }
    public static bool IsPng(byte[] bytes)
    {
        // PNG files start with an 8-byte signature: 89 50 4E 47 0D 0A 1A 0A
        byte[] pngSignature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        return bytes.Take(pngSignature.Length).SequenceEqual(pngSignature);
    }
    public static void SetBitmapDPI(byte[] image, int dpiX, int dpiY)
    {
        byte[] valueX = BitConverter.GetBytes(ConvertDPI(dpiX));
        byte[] valueY = BitConverter.GetBytes(ConvertDPI(dpiY));

        int i = 38;
        foreach (byte b in valueX)
            image[i++] = b;

        i = 42;
        foreach (byte b in valueY)
            image[i++] = b;
    }
    public static void SetPngDPI(byte[] image, int dpiX, int dpiY)
    {
        // Find the pHYs chunk and set the DPI values
        int pos = 8; // Skip the PNG signature
        while (pos < image.Length)
        {
            int length = BitConverter.ToInt32(image.Skip(pos).Take(4).Reverse().ToArray(), 0);
            string type = Encoding.ASCII.GetString(image, pos + 4, 4);
            if (type == "pHYs")
            {
                byte[] dpiXBytes = BitConverter.GetBytes(ConvertDPI(dpiX)).Reverse().ToArray();
                byte[] dpiYBytes = BitConverter.GetBytes(ConvertDPI(dpiY)).Reverse().ToArray();
                Array.Copy(dpiXBytes, 0, image, pos + 8, 4);
                Array.Copy(dpiYBytes, 0, image, pos + 12, 4);
                break;
            }
            pos += length + 12; // Move to the next chunk
        }
    }
    private static int ConvertDPI(int dpi) => (int)Math.Round(dpi * InchesPerMeter);
    #endregion
}
