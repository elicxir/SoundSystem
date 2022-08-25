using UnityEngine;
using UnityEditor;
using System.Collections;

public class Import_SoundSystem : AssetPostprocessor
{

    private void OnPreprocessAudio()
    {
        AudioImporter AI = assetImporter as AudioImporter;

        if (AI.assetPath.Contains("SoundSystem/Import/Audio/BGM/"))
        {
            var format = new AudioImporterSampleSettings();
            format.compressionFormat = AudioCompressionFormat.Vorbis;
            format.loadType = AudioClipLoadType.Streaming;
            format.quality = 100;

            AI.defaultSampleSettings = format;
        }
        if (AI.assetPath.Contains("SoundSystem/Import/Audio/SE/"))
        {
            var format = new AudioImporterSampleSettings();
            format.compressionFormat = AudioCompressionFormat.ADPCM;
            format.loadType = AudioClipLoadType.CompressedInMemory;
            format.quality = 100;

            AI.defaultSampleSettings = format;
        }

    }
}
