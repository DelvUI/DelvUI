using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiScene;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Tex;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Lumina.Data.Files.TexFile;

namespace DelvUI.Helpers
{
    public static class TextureLoader
    {
        public static TextureWrap? LoadTexture(string path, bool manualLoad)
        {
            if (!manualLoad)
            {
                TexFile? iconFile = Plugin.DataManager.GetFile<TexFile>(path);
                if (iconFile != null)
                {
                    return Plugin.UiBuilder.LoadImageRaw(iconFile.GetRgbaImageData(), iconFile.Header.Width, iconFile.Header.Height, 4);
                }
            }

            return ManuallyLoadTexture(path);
        }

        private static unsafe TextureWrap? ManuallyLoadTexture(string path)
        {
            try
            {
                var fileStream = new FileStream(path, FileMode.Open);
                var reader = new BinaryReader(fileStream);

                // read header
                int headerSize = Unsafe.SizeOf<TexHeader>();
                ReadOnlySpan<byte> headerData = reader.ReadBytes(headerSize);
                TexHeader Header = MemoryMarshal.Read<TexHeader>(headerData);

                // read image data
                byte[] rawImageData = reader.ReadBytes((int)fileStream.Length - headerSize);
                byte[] imageData = new byte[Header.Width * Header.Height * 4];

                if (!ProcessTexture(Header.Format, rawImageData, imageData, Header.Width, Header.Height))
                {
                    return null;
                }

                return Plugin.UiBuilder.LoadImageRaw(GetRgbaImageData(imageData), Header.Width, Header.Height, 4);
            }
            catch
            {
                PluginLog.Error("Error loading texture: " + path);
                return null;
            }
        }

        private static bool ProcessTexture(TextureFormat format, byte[] src, byte[] dst, int width, int height)
        {
            switch (format)
            {
                case TextureFormat.DXT1: Decompress(SquishOptions.DXT1, src, dst, width, height); return true;
                case TextureFormat.DXT3: Decompress(SquishOptions.DXT3, src, dst, width, height); return true;
                case TextureFormat.DXT5: Decompress(SquishOptions.DXT5, src, dst, width, height); return true;
                case TextureFormat.A8R8G8B8: Array.Copy(src, dst, dst.Length); return true;
            }

            return false;
        }

        private static void Decompress(SquishOptions squishOptions, byte[] src, byte[] dst, int width, int height)
        {
            var decompressed = Squish.DecompressImage(src, width, height, squishOptions);
            Array.Copy(decompressed, dst, dst.Length);
        }

        private static byte[] GetRgbaImageData(byte[] imageData)
        {
            var dst = new byte[imageData.Length];

            for (var i = 0; i < dst.Length; i += 4)
            {
                dst[i] = imageData[i + 2];
                dst[i + 1] = imageData[i + 1];
                dst[i + 2] = imageData[i];
                dst[i + 3] = imageData[i + 3];
            }

            return dst;
        }
    }
}
