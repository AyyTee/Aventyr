// This is generated code.
using Game;
using Game.Rendering;
using Cgen.Audio;
using System.Collections.Generic;
using System.IO;

public class FontAssets 
{
	public readonly List<Font> Fonts;
 
	public readonly Font @Arial = new Font(Path.Combine(new[] { @"Assets", @"Fonts", @"Arial", @"Arial.fnt"})); 
	public readonly Font @Inconsolata = new Font(Path.Combine(new[] { @"Assets", @"Fonts", @"Inconsolata", @"Inconsolata.fnt"})); 
	
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
 
	public readonly TextureFile @Default = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"Default.png"})); 
	public readonly TextureFile @Grid = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"Grid.png"})); 
	public readonly TextureFile @LineBlur = new TextureFile(Path.Combine(new[] { @"Assets", @"Textures", @"LineBlur.png"})); 
	
	public TextureAssets()
	{
		Textures = new List<TextureFile>() {
			@Default,
			@Grid,
			@LineBlur,
		};
	}
}

public class SoundAssets 
{
	public readonly List<Sound> Sounds;
 
	public readonly Sound @TestSound = new Sound(Path.Combine(new[] { @"Assets", @"Sounds", @"TestSound.ogg"})); 
	
	public SoundAssets()
	{
		Sounds = new List<Sound>() {
			@TestSound,
		};
	}
}

