// This is generated code.
using Game.Rendering;
using Cgen.Audio;
using System.Collections.Generic;

public class FontAssets 
{
	public readonly List<Font> Fonts;
 
	public readonly Font @arial = new Font(@"Assets\Fonts\arial.fnt"); 
	public readonly Font @inconsolata = new Font(@"Assets\Fonts\inconsolata.fnt"); 
	
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
 
	public readonly TextureFile @default = new TextureFile(@"Assets\Fonts\default.png"); 
	public readonly TextureFile @grid = new TextureFile(@"Assets\Fonts\grid.png"); 
	public readonly TextureFile @lineBlur = new TextureFile(@"Assets\Fonts\lineBlur.png"); 
	
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
 
	public readonly Sound @test_sound = new Sound(@"Assets\Fonts\test_sound.ogg"); 
	
	public SoundAssets()
	{
		Sounds = new List<Sound>() {
			@test_sound,
		};
	}
}

