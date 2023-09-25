using Dalamud.Interface.Internal;
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
        public static IDalamudTextureWrap? LoadTexture(string path, bool manualLoad)
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

        private static unsafe IDalamudTextureWrap? ManuallyLoadTexture(string path)
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
                Plugin.Logger.Error("Error loading texture: " + path);
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
                case TextureFormat.B5G5R5A1: ProcessA1R5G5B5(src, dst, width, height); return true;
                case TextureFormat.B4G4R4A4: ProcessA4R4G4B4(src, dst, width, height); return true;
                case TextureFormat.L8: ProcessR3G3B2(src, dst, width, height); return true;
                case TextureFormat.B8G8R8A8: Array.Copy(src, dst, dst.Length); return true;
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

        private static void ProcessA1R5G5B5(Span<byte> src, byte[] dst, int width, int height)
        {
            for (var i = 0; (i + 2) <= 2 * width * height; i += 2)
            {
                var v = BitConverter.ToUInt16(src.Slice(i, sizeof(UInt16)).ToArray(), 0);

                var a = (uint)(v & 0x8000);
                var r = (uint)(v & 0x7C00);
                var g = (uint)(v & 0x03E0);
                var b = (uint)(v & 0x001F);

                var rgb = ((r << 9) | (g << 6) | (b << 3));
                var argbValue = (a * 0x1FE00 | rgb | ((rgb >> 5) & 0x070707));

                for (var j = 0; j < 4; ++j)
                {
                    dst[i * 2 + j] = (byte)(argbValue >> (8 * j));
                }
            }
        }

        private static void ProcessA4R4G4B4(Span<byte> src, byte[] dst, int width, int height)
        {
            for (var i = 0; (i + 2) <= 2 * width * height; i += 2)
            {
                var v = BitConverter.ToUInt16(src.Slice(i, sizeof(UInt16)).ToArray(), 0);

                for (var j = 0; j < 4; ++j)
                {
                    dst[i * 2 + j] = (byte)(((v >> (4 * j)) & 0x0F) << 4);
                }
            }
        }

        private static void ProcessR3G3B2(Span<byte> src, byte[] dst, int width, int height)
        {
            for (var i = 0; i < width * height; ++i)
            {
                var r = (uint)(src[i] & 0xE0);
                var g = (uint)(src[i] & 0x1C);
                var b = (uint)(src[i] & 0x03);

                dst[i * 4 + 0] = (byte)(b | (b << 2) | (b << 4) | (b << 6));
                dst[i * 4 + 1] = (byte)(g | (g << 3) | (g << 6));
                dst[i * 4 + 2] = (byte)(r | (r << 3) | (r << 6));
                dst[i * 4 + 3] = 0xFF;
            }
        }
    }
}
