// This is generated code.
using Game.Rendering;
using Cgen.Audio;
using System.Collections.Generic;
using System.IO;

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

