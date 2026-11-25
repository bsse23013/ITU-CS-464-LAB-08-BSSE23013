"""
Makes every sound in Lab 08 from scratch with numpy, so there are no licence problems.
Run:  python make_audio.py <output folder>
Needs numpy and (for the music) ffmpeg on the PATH to write .ogg files.
"""
import os
import subprocess
import sys
import wave

import numpy as np

SR = 44100
out = sys.argv[1] if len(sys.argv) > 1 else "Audio"
for sub in ("UI", "SFX", "Music"):
    os.makedirs(os.path.join(out, sub), exist_ok=True)

rng = np.random.default_rng(464)


def t_of(sec):
    return np.arange(int(SR * sec)) / SR


def env(n, attack=0.005, release=0.1):
    e = np.ones(n)
    a = max(1, int(attack * SR))
    r = max(1, int(release * SR))
    e[:a] = np.linspace(0, 1, a)
    e[-r:] = np.minimum(e[-r:], np.linspace(1, 0, r))
    return e


def decay(n, rate):
    return np.exp(-np.arange(n) / SR * rate)


def tone(freq, sec, shape="sine", vol=1.0, rate=8.0, attack=0.004):
    t = t_of(sec)
    if shape == "sine":
        w = np.sin(2 * np.pi * freq * t)
    elif shape == "tri":
        w = 2 * np.abs(2 * ((freq * t) % 1) - 1) - 1
    elif shape == "square":
        w = np.sign(np.sin(2 * np.pi * freq * t)) * 0.6
    else:  # soft saw
        w = 2 * ((freq * t) % 1) - 1
        w = w * 0.6
    return w * decay(len(t), rate) * env(len(t), attack, 0.02) * vol


def sweep(f0, f1, sec, vol=1.0, rate=5.0):
    t = t_of(sec)
    f = np.linspace(f0, f1, len(t))
    ph = 2 * np.pi * np.cumsum(f) / SR
    return np.sin(ph) * decay(len(t), rate) * env(len(t), 0.003, 0.02) * vol


def noise(sec, vol=1.0, rate=20.0):
    n = int(SR * sec)
    return rng.uniform(-1, 1, n) * decay(n, rate) * env(n, 0.001, 0.01) * vol


def lowpass(x, k):
    # simple moving average, k samples
    if k <= 1:
        return x
    return np.convolve(x, np.ones(k) / k, mode="same")


def place(buf, clip, at):
    i = int(at * SR)
    end = min(len(buf), i + len(clip))
    if i < len(buf):
        buf[i:end] += clip[: end - i]


def save_wav(path, x):
    x = np.asarray(x, dtype=np.float64)
    peak = np.max(np.abs(x)) or 1
    x = x / peak * 0.85
    data = (x * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())


def mix(length, parts):
    b = np.zeros(int(SR * length))
    for clip, at in parts:
        place(b, clip, at)
    return b


# ---------------------------------------------------------------- UI sounds
save_wav(f"{out}/UI/ui_hover.wav", tone(1800, 0.06, "sine", rate=45) * 0.6)
save_wav(f"{out}/UI/ui_click.wav", mix(0.12, [(tone(900, 0.1, "tri", rate=40), 0), (noise(0.02, 0.15, 150), 0)]))
save_wav(f"{out}/UI/ui_back.wav", mix(0.16, [(tone(620, 0.07, "tri", rate=30), 0), (tone(440, 0.1, "tri", rate=30), 0.05)]))
save_wav(f"{out}/UI/ui_open.wav", sweep(380, 900, 0.18, rate=14) * 0.9)
save_wav(f"{out}/UI/ui_close.wav", sweep(900, 360, 0.18, rate=14) * 0.9)
save_wav(f"{out}/UI/ui_pause.wav", mix(0.3, [(tone(520, 0.14, "sine", rate=14), 0), (tone(390, 0.18, "sine", rate=14), 0.09)]))
save_wav(f"{out}/UI/ui_resume.wav", mix(0.3, [(tone(390, 0.14, "sine", rate=14), 0), (tone(520, 0.18, "sine", rate=14), 0.09)]))
save_wav(f"{out}/UI/ui_success.wav", mix(0.6, [(tone(f, 0.3, "tri", rate=7), i * 0.08) for i, f in enumerate([523.25, 659.25, 783.99, 1046.5])]))
save_wav(f"{out}/UI/ui_error.wav", mix(0.4, [(tone(200, 0.18, "square", rate=9), 0), (tone(150, 0.22, "square", rate=9), 0.12)]))

# ---------------------------------------------------------------- gameplay sounds
# coin: two quick notes
save_wav(f"{out}/SFX/sfx_coin.wav", mix(0.5, [(tone(988, 0.12, "sine", rate=12), 0), (tone(1319, 0.4, "sine", rate=7), 0.07)]))
# heart pickup: soft rising triad
save_wav(f"{out}/SFX/sfx_heal.wav", mix(0.7, [(tone(f, 0.35, "sine", rate=5), i * 0.1) for i, f in enumerate([440, 554.4, 659.25])]))
# hit: low thump + noise
hurt = mix(0.4, [(sweep(220, 60, 0.35, rate=9), 0), (lowpass(noise(0.25, 1.0, 14), 8), 0)])
save_wav(f"{out}/SFX/sfx_hurt.wav", hurt)
# boost: rising whoosh
w = lowpass(noise(0.6, 1.0, 3), 6) * np.linspace(0.2, 1, int(SR * 0.6))
save_wav(f"{out}/SFX/sfx_boost.wav", w * 0.7 + sweep(300, 1400, 0.6, 0.5, rate=3))
# win: bright arpeggio
notes = [523.25, 659.25, 783.99, 1046.5, 1318.5]
save_wav(f"{out}/SFX/sfx_win.wav", mix(1.4, [(tone(f, 0.7, "tri", rate=3.5), i * 0.12) for i, f in enumerate(notes)]))
# lose / death: falling minor notes
save_wav(f"{out}/SFX/sfx_lose.wav", mix(1.4, [(tone(f, 0.6, "sine", rate=3.5) + tone(f / 2, 0.6, "tri", 0.4, rate=3.5), i * 0.22) for i, f in enumerate([440, 392, 349.2, 261.6])]))


# ---------------------------------------------------------------- music (seamless loops)
def midi(n):
    return 440.0 * 2 ** ((n - 69) / 12)


def pad_note(freq, sec, vol=1.0):
    t = t_of(sec)
    w = sum(np.sin(2 * np.pi * freq * (1 + d) * t) for d in (-0.003, 0, 0.003)) / 3
    w += 0.3 * np.sin(2 * np.pi * freq * 2 * t)
    return w * env(len(t), 0.4, 0.6) * vol


def kick(vol=1.0):
    return sweep(150, 45, 0.25, vol, rate=14)


def hat(vol=0.3):
    return noise(0.05, vol, 70)


def snare(vol=0.5):
    return lowpass(noise(0.18, vol, 22), 2) + np.pad(tone(190, 0.1, "sine", vol * 0.4, rate=30), (0, int(SR * 0.18) - int(SR * 0.1)))


def render_loop(name, bpm, bars, builder, tail=2.0):
    beat = 60.0 / bpm
    length = beat * 4 * bars
    buf = np.zeros(int(SR * (length + tail)))
    builder(buf, beat, bars)
    n = int(SR * length)
    loop = buf[:n].copy()
    over = buf[n:]
    loop[: len(over)] += over  # let the tail ring into the start so the loop has no seam
    wavp = f"{out}/Music/{name}.wav"
    save_wav(wavp, loop)
    try:
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", wavp, "-c:a", "libvorbis", "-q:a", "4", f"{out}/Music/{name}.ogg"], check=True)
        os.remove(wavp)
    except Exception as e:  # keep the wav if ffmpeg is missing
        print("ffmpeg failed, keeping wav:", e)


CHORDS = [(57, [0, 3, 7]), (53, [0, 4, 7]), (48, [0, 4, 7]), (55, [0, 4, 7])]  # Am F C G


def menu_music(buf, beat, bars):
    for bar in range(bars):
        root, ch = CHORDS[bar % 4]
        at = bar * 4 * beat
        for iv in ch:
            place(buf, pad_note(midi(root + iv), 4 * beat + 1.0, 0.35), at)
        place(buf, pad_note(midi(root - 12), 4 * beat + 1.0, 0.4), at)
        # slow plucked arpeggio, one note every beat
        for i in range(8):
            n = root + 12 + ch[i % 3]
            place(buf, tone(midi(n), beat * 1.5, "sine", 0.22, rate=4), at + i * beat / 2)


def game_music(buf, beat, bars):
    for bar in range(bars):
        root, ch = CHORDS[bar % 4]
        at = bar * 4 * beat
        for b in range(4):
            place(buf, kick(0.9), at + b * beat)
            place(buf, hat(0.25), at + b * beat + beat / 2)
            if b in (1, 3):
                place(buf, snare(0.5), at + b * beat)
        for i in range(8):  # driving bass, eighth notes
            place(buf, tone(midi(root - 12), beat / 2, "saw", 0.35, rate=7), at + i * beat / 2)
        pattern = [0, 1, 2, 1, 2, 1, 0, 2]
        for i in range(8):  # arpeggio lead
            n = root + 12 + ch[pattern[i] % 3] + (12 if i % 4 == 3 else 0)
            place(buf, tone(midi(n), beat / 2, "square", 0.16, rate=9), at + i * beat / 2)
        place(buf, pad_note(midi(root), 4 * beat + 0.5, 0.2), at)


def results_music(buf, beat, bars):
    notes = [(60, 0), (64, 1), (67, 2), (72, 3)]
    for bar in range(bars):
        root, ch = CHORDS[(bar + 2) % 4]
        at = bar * 4 * beat
        for iv in ch:
            place(buf, pad_note(midi(root + 12 + iv), 4 * beat + 1.0, 0.3), at)
        for n, b in notes:
            place(buf, tone(midi(root + 24 + ch[b % 3]), beat * 2, "sine", 0.2, rate=3), at + b * beat)


render_loop("music_menu", 90, 8, menu_music)      # 8 bars at 90 bpm = 21.3 s
render_loop("music_game", 128, 8, game_music)     # 8 bars at 128 bpm = 15 s
render_loop("music_results", 80, 4, results_music)  # 4 bars at 80 bpm = 12 s
print("done")
