using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class PlanetBase : Node3D
{
    protected MeshInstance3D[] _faces;

    // Editor controlls
    [ExportGroup("Controls")]
    [Export]
    bool HasToReinitialize { get; set; }

    // General properties of an ellipsoid
    [ExportGroup("General Properties")]
    [Export]
    public float Radius { get; set; } = 1;

    // Graphical properties
    [ExportGroup("Graphical Properties")]
    [Export]
    public int Resolution { get; set; } = 30;

    // Geological properties
    [ExportGroup("Geological Properties")]
    [Export]
    public MeshInstance3D[] Faces
    {
        get => _faces;
        set => _faces = value;
    }

    // TODO: Landform generation propeties i.e Noise parameters & Colors



    public override void _Ready()
    {
        base._Ready();
        Initialize();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (HasToReinitialize)
            Initialize();
    }

    public void Initialize()
    {
        GD.Print("Initialiing planet base");

        // Delete previous faces
        foreach(var child in this.GetChildren())
        {
            if(child is MeshInstance3D meshInstance)
                meshInstance.QueueFree();
        }

        // Create faces
        _faces = new MeshInstance3D[6];

        for(int i=0; i < 6; i++)
        {
            _faces[i] = new MeshInstance3D();
            _faces[i].Mesh = new ArrayMesh();

            this.AddChild(_faces[i]);
        }
        var surfaceArray = new Godot.Collections.Array();
        var vertices = new List<Vector3>();
        var indeces = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        // For each face on a unit cube, the plane that is perpendicular with will be 1 unit away from the face's center
        // The initial width and height of the faces will the 2 units, we will base our x and z on this
        // With this in mind, we will first create a vertex for a flat face, normalize  it, then curve it using the radius
        // For the other faces we'll just swarp the values and the signs for x, y and z
        for(int x=0; x<Resolution; x++)
        {
            for(int z=0; z<Resolution; z++)
            {
                var vertex = new Vector3(2.0f / (x + 1) - 1.0f, 1, 2.0f / (z + 1) - 1.0f) ;//* Radius;

                normals.Add(vertex.Normalized());  // Should be fine since we don't have landforms yet;
                uvs.Add(new Vector2(1.0f / (x + 1), 1.0f / (z + 1))); // This maps a texture to only one face, TODO: Think/work on it later
                vertices.Add(vertex);

                if(x > 0 && z > 0)
                {
                    // After some observation, I concluded that the general formula was index = x*(Resolution + 1) + z
                    // Sinse we ignore the first row and column, we can refer to the revious vertices to create the indeces
                    // For each triangle, I will start at the top left and count in a clockwise rotation

                    // For the first tringle, we'll have vertices for (x-1, z-1), (x-1, z) then (x, z)
                    indeces.Add(x*(Resolution) + z);
                    indeces.Add((x - 1)*(Resolution) + z);
                    indeces.Add((x - 1)*(Resolution) + (z - 1));

                    // For the second tringle, we'll have vertices for (x-1, z-1), (x, z) then (x, z - 1)
                    indeces.Add(x*(Resolution) + (z - 1));
                    indeces.Add(x*(Resolution) + z);
                    indeces.Add((x - 1)*(Resolution) + (z - 1));
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
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[0].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Bottom face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.Z, -vert.Y, vert.X)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(norm => new Vector3(norm.Z, -norm.Y, norm.X)).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[1].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Right face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.Z, vert.X, vert.Y)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(norm => new Vector3(norm.Z, norm.X, norm.Y)).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[2].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Left face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(-vert.Y, vert.X, vert.Z)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(norm => new Vector3(-norm.Y, norm.X, norm.Z)).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[3].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Back face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.Y, vert.Z, vert.X)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(norm => new Vector3(norm.Y, norm.Z, norm.X)).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[4].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        // Front face
        surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.Select(vert => new Vector3(vert.X, vert.Z, -vert.Y)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(norm => new Vector3(norm.X, norm.Z, -norm.Y)).ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        (_faces[5].Mesh as ArrayMesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        foreach(var face in _faces)
            face.Show();

        HasToReinitialize = false;
    }
}
