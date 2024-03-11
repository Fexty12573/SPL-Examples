using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core.Rendering;

namespace AssetBrowser;
internal static class IconRepository
{
    public static TextureHandle UpArrow { get; private set; }
    public static TextureHandle Folder { get; private set; }
    public static TextureHandle File { get; private set; }

    public static void Initialize(string assetDir)
    {
        UpArrow = Load("arrow-up-white.png");
        Folder = Load("folder.png");
        File = Load("file.png");

        return;

        TextureHandle Load(string file)
        {
            return Renderer.LoadTexture(Path.Combine(assetDir, file), out _, out _);
        }
    }
}
