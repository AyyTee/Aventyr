// This is generated code.
using Game.Rendering;
using Cgen.Audio;
using System.Collections.Generic;
using System.IO;

public class FontAssets 
{
    public readonly List<Font> Fonts;
 
    public Font @Arial { get; } = new Font(Path.Combine(new[] { @"Assets", @"Fonts", @"Arial", @"Arial.fnt"})); 
    public Font @Inconsolata { get; } = new Font(Path.Combine(new[] { @"Assets", @"Fonts", @"Inconsolata", @"Inconsolata.fnt"})); 

    public FontAssets()
    {
        Fonts = new List<Font>() {
            @Arial,
            @Inconsolata,
        };
    }
}

public class TextureAssets 
{
    public readonly List<TextureFile> Textures;
 
    public TextureFile @BayerMatrix { get; } = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"BayerMatrix.png"})); 
    public TextureFile @Default { get; } = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"Default.png"})); 
    public TextureFile @Grid { get; } = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"Grid.png"})); 
    public TextureFile @LineBlur { get; } = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"LineBlur.png"})); 

    public TextureAssets()
    {
        Textures = new List<TextureFile>() {
            @BayerMatrix,
            @Default,
            @Grid,
            @LineBlur,
        };
    }
}

public class SoundAssets 
{
    public readonly List<Sound> Sounds;
 
    public Sound @TestSound { get; } = new Sound(Path.Combine(new[] { @"Assets", @"Sounds", @"TestSound.ogg"})); 

    public SoundAssets()
    {
        Sounds = new List<Sound>() {
            @TestSound,
        };
    }
}

