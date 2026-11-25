# ITU-CS-464-LAB-08-BSSE23013

**CS464 Game Development, Lab 08 (Audio, Loading, Events)**
Muhammad Sarmad Ahmad, BSSE23013

This lab is built on top of my Lab 07 project, ROLL RUSH. The ten menu screens and their tweens from Lab 07 still work as before. What is new is a real (small) level, an async loading screen in front of it, sound everywhere, a proper Audio Mixer with working volume settings, and a C# event system that ties the gameplay to the HUD and the audio.

- **Demo video:** https://drive.google.com/file/d/1BswE24Fq6tec0HX-fgtV3u-AfJRqAOSw/view?usp=sharing
- **Audio Mixer screenshot:** `Screenshots/audio-mixer.png`
- **Unity version:** 6000.3.24f1 (URP, Input System, TextMeshPro)

## How to run

1. Clone the repo and open the folder in Unity Hub with Unity **6000.3.24f1**. The first import takes a while.
2. Open `Assets/Scenes/MenuSystem.unity` and press Play (Game view set to a portrait ratio).
3. Flow: Title, Main Menu, Play, pick a level, Start. The loading screen appears, then the level.


Keys in the level: WASD or arrow keys to move, Esc to pause. On a touch screen you drag.

## Loading flow



1. START shows the Loading screen.
2. `allowSceneActivation` is set to `false`, so Unity loads the scene data but does not switch it on.
3. The bar follows `op.progress / 0.9` (Unity stops at 0.9 while activation is blocked). The bar moves smoothly towards the real value and the screen stays for at least a short minimum time so it does not flash.
4. Only when the bar is full does the code set `allowSceneActivation = true`, wait for `isDone`, show the HUD and call `GameManager.Begin()`. The player cannot move before that.
5. Replay and Next Level unload the old Gameplay scene first and run the same routine again. Quit to Menu unloads it.

## Audio Mixer

One mixer, `Assets/Lab08/Audio/GameMixer.mixer`:

| Group | Parent | Exposed parameter | What goes through it |
|---|---|---|---|
| Master | none | `MasterVolume` | everything |
| Music | Master | `MusicVolume` | the two music sources |
| SFX | Master | `SfxVolume` | the four gameplay SFX sources |
| UI | Master | `UiVolume` | the two UI sound sources |

Every AudioSource in the project has its output set to one of these groups. There is no AudioSource on the buttons; all UI sounds go through one reusable `UIAudio` object (`UIAudio.Play(UISound.Click)`).

**Sounds.** UI: hover, press, back, open, close, pause, resume, success, error. Gameplay: coin, heal, hurt, boost, win, lose. Music: menu, game, results. The SFX player uses `PlayOneShot` on a round-robin set of sources with a random pitch (about plus or minus two semitones) and a small volume change, so repeated coins do not sound identical.

## Volume mapping

The sliders in Settings go from 0 to 1, but the mixer works in decibels, so `VolumeSettings.ToDecibels` does:

```csharp
Mathf.Log10(Mathf.Clamp(slider, 0.0001f, 1f)) * 20f
```

1.0 is 0 dB, 0.5 is about -6 dB, and the minimum 0.0001 is exactly -80 dB (silent). The clamp is there because `Log10(0)` is minus infinity.

- **Persistence:** each slider is saved in PlayerPrefs (`Lab08.MasterVolume` and so on) and applied in `Start`, so the saved values come back on the next launch.
- **Mute:** the Mute all toggle is stored separately (`Lab08.Muted`). It pushes Master to -80 dB but never touches the saved slider values, so unmuting brings back exactly what the player had.
- **Pause:** pausing sets `Time.timeScale = 0` and `AudioListener.pause = true`, which stops gameplay sounds. The music and UI sources have `ignoreListenerPause` turned on, so the music keeps playing (ducked down to about a third) and the pause menu buttons still make sound. Resume and returning to the menu restore everything.

## Music transitions

`MusicPlayer` has two AudioSources and crossfades between them with an equal-power curve (cos / sin) over 1.5 seconds, on unscaled time so it also works while paused. `MusicController` listens to the game events and decides the track:

| Event | Music |
|---|---|
| `MenuShown` | menu loop |
| `GameStarted` | game loop |
| `GameWon` / `GameLost` | results loop |

The music loops are made seamless (the tail of each loop wraps into its start).

## Events: publishers and subscribers

| Event | Publisher | Subscribers |
|---|---|---|
| `Health.Damaged` (instance) | `Health` | `HurtSound`, `HitSparks`, `ShakeOnHit` |
| `GameEvents.PlayerDamaged` / `PlayerHealed` | `Health` | `HealthBarListener`, `HitFlash` |
| `GameEvents.CoinCollected` | `GameManager` | `CoinCounter` |
| `Pickup.Collected` (static) | `Pickup` | `PickupFeedback` (sound + sparkle) |
| `GameEvents.BoostStarted` | `Pickup` | `BoostTrail` |
| `GameEvents.GameStarted` / `MenuShown` | `GameManager`, `UIManager` | `MusicController` |
| `GameEvents.GameWon` / `GameLost` | `GameManager` | `ResultSounds`, `MusicController`, `UIManager` |

`Health.Damaged` alone has three independent listeners, and none of them knows about the others. A static event can only be invoked from the class that declares it, so `GameEvents` has small `Raise...` methods the publishers call.

Declaration and invocation (`Health.cs`, `GameEvents.cs`):

```csharp
public event Action<int, int> Damaged;

Damaged?.Invoke(Current, maxHealth);
GameEvents.RaisePlayerDamaged(Current, maxHealth);
```

Subscribe and unsubscribe (`HitSparks.cs`):

```csharp
void OnEnable()  { health.Damaged += OnDamaged; }
void OnDisable() { health.Damaged -= OnDamaged; }

void OnDamaged(int current, int max) { sparks.Emit(24); }
```

Every listener subscribes in `OnEnable` and unsubscribes in `OnDisable`. This matters most for the static events, because they outlive the Gameplay scene and a destroyed listener left on the list would throw errors after a replay.

## Project structure

```
Assets/
  Lab07/          menu system from Lab 07 (UI scripts, builder, sprites), with sound hooks added
  Lab08/
    Audio/        GameMixer.mixer + UI / SFX / Music clips
    Scripts/      events, audio players, volume settings, loading, gameplay
    Editor/       Lab08 > Build Everything, audio import settings
  Scenes/         MenuSystem.unity, Gameplay.unity
Tools/
  make_audio.py   generates every sound in Assets/Lab08/Audio
  make_sprites.py generates the Lab 07 sprites
```

## Asset credits

- **All audio** (9 UI sounds, 6 SFX, 3 music loops) was made by me using AI with a small Python/NumPy script, `Tools/make_audio.py`. No third-party audio is used, so there is nothing to attribute.
- **Fonts:** Lilita One and Poppins (SIL Open Font License), see `Assets/Lab07/Licenses`.
- **Sprites:** drawn by script (`Tools/make_sprites.py`).
- Lecture 08, 09 and 10 slides were the reference for the colour/UI work, the audio setup and the sound design.
