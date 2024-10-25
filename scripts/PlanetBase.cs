using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public enum GeometryType
{
    Cube,
    Sphere
}

public enum FaceType
{
    Top,
    Bottom,
    Right,
    Left,
    Back,
    Front,
    All
}

[Tool]
public partial class PlanetBase : Node3D
{
    protected Array<MeshInstance3D> _faces;
    private float _radius = 1;
    private GeometryType _geometry = GeometryType.Sphere;
    private int _resolution = 30;
    private FaceType _faceType = FaceType.All;
    private Material _material;
    private Array<NoiseSettings> _noiseSettings;



    // Editor controlls


    [ExportGroup("Controls")]
    [Export]
    public GeometryType GeometryType { get => _geometry; set => SetProperty(ref _geometry, value); }
    [Export]
    public FaceType ShowFaces { get => _faceType; set => SetProperty(ref _faceType, value); }

    // General properties of an ellipsoid
    [ExportGroup("General Properties")]
    [Export]
    public float Radius { get => _radius; set => SetProperty(ref _radius, value); }

    // Graphical properties
    [ExportGroup("Graphical Properties")]
    [Export]
    public int Resolution { get => _resolution; set => SetProperty(ref _resolution, value); }

    // Geological properties
    [ExportGroup("Geometrical Properties")]
    [Export]
    public Array<MeshInstance3D> Faces{ get => _faces; set => SetProperty(ref _faces, value); }
    [Export]
    public Array<NoiseSettings> NoiseSettings { get => _noiseSettings; set => SetProperty(ref _noiseSettings, value); }

    // TODO: Landform generation propeties i.e Noise parameters & Colors

    [Export]
    public bool CanReinitialize {get; set;} = false;
    [Export]
    public int PendingValidations {get; set;} = 0;

    public override void _Ready()
    {
        base._Ready();
        Initialize();
        CanReinitialize = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void Initialize()
    {
        CanReinitialize = false;
        GD.Print("Initializing planet base");

        // Delete previous faces
        foreach(var child in this.GetChildren())
        {
            if(child is MeshInstance3D meshInstance)
                meshInstance.QueueFree();
        }

        // Create faces
        _faces = new Array<MeshInstance3D>(new MeshInstance3D[6]);

        for(int i=0; i < 6; i++)
        {
            _faces[i] = new MeshInstance3D();
            _faces[i].Mesh = new ArrayMesh();

            this.AddChild(_faces[i]);
        }
        var surfaceArray = new Godot.Collections.Array();
        var vertices = new List<Vector3>();
        var indeces = new List<int>();
        var uvs = new List<Vector2>();

        // For each face on a unit cube, the plane that is perpendicular with will be 1 unit away from the face's center
        // The initial width and height of the faces will the 2 units, we will base our x and z on this
        // With this in mind, we will first create a vertex for a flat face, normalize  it, then curve it using the radius
        // For the other faces we'll just swarp the values and the signs for x, y and z
        for(int x=0; x <= Resolution; x++)
        {
            for(int z=0; z <= Resolution; z++)
            {
                var vertex = new Vector3(2.0f / Resolution * x - 1.0f, 1, 2.0f / Resolution * z - 1.0f);

                uvs.Add(new Vector2(1.0f / Resolution * x, 1.0f / Resolution * z)); // This maps a texture to only one face, TODO: Think/work on it later

                if(_geometry == GeometryType.Sphere)
                    vertices.Add(vertex.Normalized() * _radius);
                else
                    vertices.Add(vertex * _radius);

                if(x > 0 && z > 0)
                {
                    // After some observation, I concluded that the general formula was index = x*(Resolution + 1) + z
                    // Sinse we ignore the first row and column, we can refer to the revious vertices to create the indeces
                    // For each triangle, I will start at the top left and count in a clockwise rotation

                    // For the first tringle, we'll have vertices for (x, z), (x-1, z) then (x-1, z-1)
                    indeces.Add(x*(Resolution + 1) + z);
                    indeces.Add((x - 1)*(Resolution + 1) + z);
                    indeces.Add((x - 1)*(Resolution + 1) + (z - 1));

                    // For the second tringle, we'll have vertices for (x, z - 1), (x, z) then (x-1, z-1)
                    indeces.Add(x*(Resolution + 1) + (z - 1));
                    indeces.Add(x*(Resolution + 1) + z);
                    indeces.Add((x - 1)*(Resolution + 1) + (z - 1));
                }
            }
        }

        surfaceArray.Resize((int)Mesh.ArrayType.Max);


        // Assign the mesh to the faces
        // I am using LINQ to swarp values and change signs accordingly for each face
        // With the UVs, I'm struggling to figure out how to map a texture to a sphere without stretching and compression
        // Top face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indeces.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = vertices.Select(vert => vert.Normalized()).ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[0].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Bottom face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.Z, -vert.Y, vert.X)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = surfaceArray[(int)Mesh.ArrayType.Vertex].AsVector3Array().Select(vert => vert.Normalized()).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[1].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Right face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.Z, vert.X, vert.Y)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = surfaceArray[(int)Mesh.ArrayType.Vertex].AsVector3Array().Select(vert => vert.Normalized()).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[2].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Left face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(-vert.Y, vert.X, vert.Z)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = surfaceArray[(int)Mesh.ArrayType.Vertex].AsVector3Array().Select(vert => vert.Normalized()).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[3].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Back face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.Y, vert.Z, vert.X)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = surfaceArray[(int)Mesh.ArrayType.Vertex].AsVector3Array().Select(vert => vert.Normalized()).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[4].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Front face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.X, vert.Z, -vert.Y)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = surfaceArray[(int)Mesh.ArrayType.Vertex].AsVector3Array().Select(vert => vert.Normalized()).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[5].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        if(_noiseSettings != null)
        for(int f=0; f<_faces.Count; f++)
        {
            var face = _faces[f];

            using(MeshDataTool dataTool = new MeshDataTool())
            {
                dataTool.CreateFromSurface(face.Mesh as ArrayMesh, (int)Mesh.ArrayType.Vertex);

                for(int s=0; s < _noiseSettings.Count; s++)
                {
                    for(int i = 0; i < dataTool.GetVertexCount(); i++)
                    {
                        var vert = dataTool.GetVertex(i);
                        //vert = vert.Normalized() * (vert.Length() + _noiseSettings[s].Evaluate(vert.Length() * 0.5f * (vert.X / Mathf.Abs(vert.X)), vert.Length() * 0.5f * (vert.X / Mathf.Abs(vert.X))));
                        vert = vert.Normalized() * (vert.Length() + _noiseSettings[s].Evaluate(vert));
                        dataTool.SetVertex(i, vert);
                    }
                }

                for(int i = 0; i<dataTool.GetFaceCount(); i++)
                {
                    var vert = dataTool.GetFaceVertex(i, 0);
                    var norm = dataTool.GetFaceNormal(i);
                    dataTool.SetVertexNormal(vert, norm);
                }

                face.Mesh = new ArrayMesh();

                dataTool.CommitToSurface(face.Mesh as ArrayMesh);

                face.Show();
            }
        }

        CanReinitialize = true;
    }

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);

        // GD.PrintRich($"[color=yellow]Validating property: [/color]{PendingValidations}");

        if (property["name"].AsStringName() == PropertyName.ShowFaces || property["name"].AsStringName() == PropertyName.NoiseSettings)
            Initialize();

        if(property["name"].AsStringName() == PropertyName.ShowFaces)
        {
            var face = Get(PropertyName.ShowFaces).As<FaceType>();
            GD.PrintRich($"[color=yellow]Displaying [color=white]{face}:{(int)face}[/color] face(s)[/color]");
            switch(face)
            {
                case FaceType.Front:
                case FaceType.Back:
                case FaceType.Right:
                case FaceType.Left:
                case FaceType.Top:
                case FaceType.Bottom:
                    for(int i = 0; i<6; i++)
                    {
                        if((int)face != i)
                            _faces[i].Hide();
                        else
                            _faces[i].Show();
                    } 
                    break;
                case FaceType.All:
                    for(int i = 0; i<6; i++)
                        _faces[i].Show();
                    break;
            }    
        }

        PendingValidations = Math.Max(0, --PendingValidations);
    }


    protected void SetProperty<T>(ref T field, T value)
    {
        field = value;
        NotifyPropertyListChanged();
    }

    private void _on_property_list_changed()
    {
        PendingValidations += 1;
    }
}
