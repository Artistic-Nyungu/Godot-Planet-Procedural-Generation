using Godot;

public static partial class NoiseFilter
{
    #nullable enable
    private static FastNoiseLite? _fastNoise = null;

    private static FastNoiseLite Init()
    {
        var fastNoise = new FastNoiseLite();
        fastNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        return fastNoise;
    }

    
    public static float Evaluate(this NoiseSettings NoiseSettings, Vector3 position)
    {
        if (_fastNoise == null)
            _fastNoise = Init();

        float h = 0f;
        for(int layer=0; layer<NoiseSettings.Layers; layer++)
        {
            float sampleX = position.X/NoiseSettings.Scale*(NoiseSettings.Frequency * Mathf.Pow(NoiseSettings.Lacunarity, layer+1));
            float sampleY = position.Y/NoiseSettings.Scale*(NoiseSettings.Frequency * Mathf.Pow(NoiseSettings.Lacunarity, layer+1));
            float sampleZ = position.Z/NoiseSettings.Scale*(NoiseSettings.Frequency * Mathf.Pow(NoiseSettings.Lacunarity, layer+1));

            h += _fastNoise.GetNoise3D(sampleX, sampleY, sampleZ)*(NoiseSettings.Amplitude * Mathf.Pow(NoiseSettings.Persistence, layer+1));
        }

        return h;
    }
}