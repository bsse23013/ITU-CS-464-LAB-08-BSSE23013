using UnityEditor;
using UnityEngine;

// Import settings for every clip in Assets/Lab08/Audio (same idea as Lab07SpriteImporter), so a fresh
// clone imports them the same way (Lecture 09: short effects Decompress On Load + mono, music Streaming).
public class Lab08AudioImporter : AssetPostprocessor
{
    const string Folder = "Assets/Lab08/Audio/";

    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith(Folder)) return;

        AudioImporter importer = (AudioImporter)assetImporter;
        bool music = assetPath.Contains("/Music/");

        AudioImporterSampleSettings s = importer.defaultSampleSettings;
        if (music)
        {
            s.loadType = AudioClipLoadType.Streaming;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = 0.6f;
        }
        else
        {
            s.loadType = AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.PCM;
        }
        importer.defaultSampleSettings = s;
        importer.forceToMono = !music;
    }
}
