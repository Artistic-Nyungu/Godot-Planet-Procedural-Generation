using Godot;

[GlobalClass]
[Tool]
public partial class NoiseSettings:Resource
{
    [Export(PropertyHint.Range, "0,1,0.05")]
    public float Frequency { get; set; }
    [Export]
    public float Amplitude { get; set; }
    [Export]
    public float Lacunarity { get; set; }
    [Export]
    public float Persistence { get; set; }
    [Export]
    public float Scale { get; set; }
    [Export]
    public int Layers { get; set; }
    [Export]
    public Vector2 Offset { get; set; } = Vector2.Zero;
}