using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Svg;

public static class MediaLoader
{
    private static readonly string[] SupportedExtensions = { ".svg", ".png", ".jpg", ".jpeg", ".gif" };

    public static bool IsSupportedFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        foreach (string supportedExtension in SupportedExtensions)
        {
            if (extension == supportedExtension)
            {
                return true;
            }
        }
        return false;
    }

    public static Texture2D LoadTexture(GraphicsDevice device, string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension == ".svg")
        {
            return LoadSvgTexture(device, filePath);
        }

        return LoadRasterTexture(device, filePath);
    }

    public static string[] GetSupportedExtensions()
    {
        return (string[])SupportedExtensions.Clone();
    }

    private static Texture2D LoadRasterTexture(GraphicsDevice device, string filePath)
    {
        using (var image = System.Drawing.Image.FromFile(filePath))
        using (var bitmap = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        using (var gfx = System.Drawing.Graphics.FromImage(bitmap))
        {
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gfx.Clear(System.Drawing.Color.Transparent);
            gfx.DrawImage(image, 0, 0, image.Width, image.Height);
            return CreateTextureFromBitmap(device, bitmap);
        }
    }

    private static Texture2D LoadSvgTexture(GraphicsDevice device, string filePath)
    {
        SvgDocument svgDocument = SvgDocument.Open(filePath);
        int width;
        int height;

        if (svgDocument.ViewBox != SvgViewBox.Empty)
        {
            width = (int)Math.Ceiling(svgDocument.ViewBox.Width);
            height = (int)Math.Ceiling(svgDocument.ViewBox.Height);
        }
        else
        {
            width = (int)Math.Ceiling(svgDocument.Width.Value);
            height = (int)Math.Ceiling(svgDocument.Height.Value);
        }

        // Fallback for SVGs that omit explicit dimensions.
        if (width <= 0 || height <= 0)
        {
            width = 1000;
            height = 1000;
        }

        int maxDimension = Math.Max(width, height);
        const int targetMaxDimension = 1600;
        float renderScale = Math.Min(4f, Math.Max(1f, targetMaxDimension / (float)maxDimension));

        int renderWidth = Math.Max(1, (int)Math.Round(width * renderScale));
        int renderHeight = Math.Max(1, (int)Math.Round(height * renderScale));

        using (var bitmap = new System.Drawing.Bitmap(renderWidth, renderHeight))
        using (var gfx = System.Drawing.Graphics.FromImage(bitmap))
        {
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gfx.Clear(System.Drawing.Color.Transparent);
            gfx.ScaleTransform(renderScale, renderScale);
            svgDocument.Draw(gfx);
            return CreateTextureFromBitmap(device, bitmap);
        }
    }

    private static Texture2D CreateTextureFromBitmap(GraphicsDevice device, System.Drawing.Bitmap bitmap)
    {
        Texture2D texture = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);
        var buffer = new byte[bitmap.Width * bitmap.Height * 4];
        var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, buffer.Length);
        bitmap.UnlockBits(bitmapData);

        // Swap red/blue channels because Bitmap and MonoGame color orders differ.
        for (int i = 0; i < buffer.Length; i += 4)
        {
            byte temp = buffer[i];
            buffer[i] = buffer[i + 2];
            buffer[i + 2] = temp;
        }

        texture.SetData(buffer);
        return texture;
    }
}
