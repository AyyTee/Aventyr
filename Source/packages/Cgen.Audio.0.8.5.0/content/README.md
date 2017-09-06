# Cgen.Audio #
Audio Submodule of CygnusJam Game Engine.

- **Author**: Alghi Fariansyah
- **Email**: com@cxo2.me
- **Latest Version**: 0.8.5

--------------------------------------------------------------------------------------------------

## Summary ##

Cgen.Audio is a simple yet powerful Cross-platform Audio Engine which is able to playback sound data.  
Written under C# language based on [NVorbis](https://github.com/SirusDoma/nvorbis) and [OpenTK](https://github.com/opentk/opentk) (OpenAL).

The engine supports 2 audio format: WAV and OGG.

## Version History ##

### v0.8.5
This release may require you to change few parts of your codes (Minor Changes)
- Added few Properties to `SoundBuffer`: `IsRelativeToListener`, `MinumumDistance`, `Attenuation`, `Position3D` and `Pan`
- Reworked `Volume` property: readjusted value, now the value range is between 0 (Mute) and 100 (Full)

### v0.8.3
- Added Supports for 32bit PCM, 32bit Float and 24bit PCM Wav Samples.

### v0.8.0 
This release <b>Break</b> the backward compatibility (Major Changes)
- Fixed various bugs when buffering the `SoundBuffer` object.
- Fixed instancing new `Sound` object bugs.
- `SoundSystem` now use streaming algorithm instead load all data into buffer to play the sounds.
- Added Automated Update Cycle of `SoundSystem`.
- Added <i>Deferred</i> audio streaming.
- Integrated `XRAM` and `EFX` Extension to the `SoundSystem`.

### v0.7.2
- Added `ISoundStreamReader` to implement custom audio decoder.
- Fixed minor bugs on `SoundSystem.Update(double)` cycle.

### v0.7.0
- Able to play WAV and OGG.

## Compiling Project ##

By default, the project target framework is targeted to .NET 2.0 (or equal to Mono 2.0) to ensure maximum backward compatibility against old hardware and / or old projects that still targeted to old framework.

Currently, This library doesn't depend on complicated external library, It's safe to compile the project right away with preferred Target Framework and Build Configurations.

However, the Build Configuration Platform (`x86`/`x64`) must match with the projects that referencing this library to make it work properly. <b>DO NOT</b> use `Any CPU` platform, because this library use native external dependency (e.g: the engine cannot determine which version of `openal32.dll` to use).

In Windows, `Unit Tests` Project require correct version of `openal32.dll` installed in your machine.

## Documentation ##

Currently, the engine doesn't have detailed API reference, it's currently under construction.  
Therefore, the basics usages is explain along with this document.

Before getting started with the engine, you need choose the proper usages of the engine, it depends application that you are developing.
There are 2 Usages:

### Usage Notes ###

- In a General Application (e.g: Winforms or WPF), it is recommended to use Automated Update Cycle by calling `SoundSystem.Instance.Start();` at initialization of program. This will create a separate thread that automatically call `SoundSystem.Instance.Update(double)` each 10ms. To stop it, call `SoundSystem.Instance.Stop();` at any point of your program.  

- In a Game Application, call `SoundSystem.Instance.Update(double)` on each frame in the game loop, where the parameter is the <i>frame time</i> / <i>delta time</i>.

Also, make sure the external dependencies is installed on target machine or shipped under same directory of your application. The external dependencies files is available under `Dependencies` folder. 

### 1. Initialization

It's easy to initialize the engine, you don't have to choice which audio device that you should pick or configuring specific hardware dependent configuration.  

The engine will setup everything automagically.
```
  // Initialize Sound System with default buffer
  SoundSystem.Instance.Initialize();
  
  // Or, you can specify the buffer size
  SoundSystem.Instance.Initialize(65536);
```

Basically, the `BufferSize` is the maximum data length per `Buffer`.  
It is recommended to use default `BufferSize`.

### 2. Handling Update Cycle

The `SoundSystem` need to updated frequently in order to stream the sound buffer properly.
There are 2 ways to Update the `SoundSystem.Instance`

- Invoke `SoundSystem.Instance.Start();` during initialization of your program, use this when you are developing non-game application.
- Invoke `SoundSystem.Instance.Update(double);` on each frame in the game loop / main loop if you are developing game application, this ensure the audio stream is keep synchronized with the game timing. The parameter is the elapsed time (in ms) between the previous frame (<i>frame time</i> / <i>delta time</i>)

### 3. Creating a Sound object

`Sound` has 3 Constructor available, have a look at example code:
```
  // Create a sound object
  Sound bell = new Sound("Path/To/Bell.wav");
  
  // You can also load it from array of byte
  byte[] audioData = System.IO.File.ReadAllBytes("Path/To/Bell,wav");
  bell = new Sound(audioData);
  
  // Or a System.IO.Stream
  System.IO.FileStream fstream = System.IO.File.OpenRead("Path/To/Bell.wav");
  bell = new Sound(fstream);
```

### 4. Common Sound Operations

You can `Play`, `Stop`, `Pause` or `Resume` a `Sound` instance at any point of your program <b>after the `SoundSystem` is initialized</b>.

```
  // Play the sound
  bell.Play();
  
  // Pause the sound
  bell.Pause();
  
  // Resume the sound
  bell.Resume();
  
  // Stop the sound
  bell.Stop();
  
  // You can also retrieve the status of sound
  Console.WriteLine(bell.State); // SoundState.Stopped 
  
  // Once the sound is no longer needed, dispose it
  bell.Dispose();
```

### 5. Configuring Sound

And of course you can configure sound elements such as `Volume`, `Pitch` and even `Position`  
It also contains some 3D Sounds Settings such as `Position3D`, `Attenuation` and `MinimumDistance`

```
  bell.Volume = 50f; // 50% Volume
  
  bell.Pitch = 2.0f; // 200% Pitch
  
  bell.Pan = -0.5f; // 50% Panning to Left, Note that this value will override Position3D settings.
  
  bell.IsRelativeToListener = false; // Position of Sound will stay remain at same position of listener in 3D plane
  
  bell.MinimumDistance = 3f; // Set the Minimum Distance: the maximum distance at which it is heard at its maximum volume.
  
  bell.Attenuation = 5f; // Set an attenuation factor: a multiplicative factor which makes the sound more or less loud according to its distance from the listener.
  
  bell.Position3D = new float[] { 1, 2, 3 }; // Set the 3D Position of the sound in 3D Plane, the array should consist with 3 component { X, Y, Z } . Note that this value will override Pan settings.
  
  bell.IsLooping = true; // The sound will loop.
  
  bell.Position = TimeSpan.FromSeconds(34); // Jump to 0:34
  
  Console.WriteLine(bell.Length.TotalSeconds); // Retrieve Sound Length, in Seconds
```

## Dependencies ##

This library uses several dependencies to perform specific operations.
The dependencies are separated into 2 types: Internal and External:

- External dependencies are included under `Dependencies` folder and must be installed or shipped along with the application and located under same folder with the main of application. These dependencies may installed by default in certain Operating System.

- Internal dependencies are compiled along with this library during compilation, the source code is located under `Source\Cgen\Dependencies`

List of dependencies:
- [OpenTK](https://github.com/opentk/opentk)
- [NVorbis](https://github.com/SirusDoma/nvorbis)

## License ##

This is an open-sourced library licensed under the [zlib/libpng license](http://github.com/SirusDoma/Cgen.Audio/blob/master/LICENSE.txt).
