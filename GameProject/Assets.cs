// This is generated code.
using Game;
using Game.Rendering;
using Cgen.Audio;
using System.Collections.Generic;
using System.IO;

public class FontAssets 
{
	public readonly List<Font> Fonts;
 
	public readonly Font @arial = new Font(Path.Combine(new[] { @"Assets", @"Fonts", @"Arial", @"arial.fnt"})); 
	public readonly Font @inconsolata = new Font(Path.Combine(new[] { @"Assets", @"Fonts", @"Inconsolata", @"inconsolata.fnt"})); 
	
	public FontAssets()
	{
		Fonts = new List<Font>() {
			@arial,
			@inconsolata,
		};
	}
}

public class TextureAssets 
{
	public readonly List<TextureFile> Textures;
 
	public readonly TextureFile @default = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"default.png"})); 
	public readonly TextureFile @grid = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"grid.png"})); 
	public readonly TextureFile @lineBlur = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"lineBlur.png"})); 
	
	public TextureAssets()
	{
		Textures = new List<TextureFile>() {
			@default,
			@grid,
			@lineBlur,
		};
	}
}

public class SoundAssets 
{
	public readonly List<Sound> Sounds;
 
	public readonly Sound @test_sound = new Sound(Path.Combine(new[] { @"Assets", @"Sounds", @"test_sound.ogg"})); 
	
	public SoundAssets()
	{
		Sounds = new List<Sound>() {
			@test_sound,
		};
	}
}

