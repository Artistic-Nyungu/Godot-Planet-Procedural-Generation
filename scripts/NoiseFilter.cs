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

    public static float Evaluate(this NoiseSettings NoiseSettings, float x, float z)
    {
        if (_fastNoise == null)
            _fastNoise = Init();

        float y = 0f;
        for(int layer=0; layer<NoiseSettings.Layers; layer++)
        {
            float sampleX = x/NoiseSettings.Scale*(NoiseSettings.Frequency * Mathf.Pow(NoiseSettings.Lacunarity, layer+1));
            float sampleZ = z/NoiseSettings.Scale*(NoiseSettings.Frequency * Mathf.Pow(NoiseSettings.Lacunarity, layer+1));

            y += _fastNoise.GetNoise2D(sampleX, sampleZ)*(NoiseSettings.Amplitude * Mathf.Pow(NoiseSettings.Persistence, layer+1));
        }

        return y;
    }
}