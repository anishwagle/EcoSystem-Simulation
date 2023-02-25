using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    public List<Triangle> Triangles  = new List<Triangle>();
    

    public int xSize = 200;
    public int zSize = 200;
    public int offsetx = 0;
    public int offsetz = 0;
    public int resolution = 1;
    public MeshGenerator()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();

        CreateShape();
        UpdateMesh();

        GetComponent<MeshFilter>().mesh = mesh;

    }

    float Activation(float i)
    {
        return i > 0 ? 1 : 0;
    }

    List<int> AddTriangleForPlane(char orientation){
     
        int planeCount = Triangles.Count / 6;
        var triangleSide = new List<int>();
        if(orientation == 'f'){
            triangleSide.Add(4 * planeCount + 0);
            triangleSide.Add(4 * planeCount + 1);
            triangleSide.Add(4 * planeCount + 2);
            triangleSide.Add(4 * planeCount + 2);
            triangleSide.Add(4 * planeCount + 1);
            triangleSide.Add(4 * planeCount + 3);
        }
        else{
            triangleSide.Add(4 * planeCount + 2);
            triangleSide.Add(4 * planeCount + 1);
            triangleSide.Add(4 * planeCount + 0);
            triangleSide.Add(4 * planeCount + 3);
            triangleSide.Add(4 * planeCount + 1);
            triangleSide.Add(4 * planeCount + 2);
        }
        return triangleSide;
    }
    void Createplane(int x, int y, int z, char axis, char orientation)
    {
        var triangle = new Triangle();
        triangle.vertices = new List<Vector3>();
        triangle.triangles = new List<int>();
        
        
        if (axis == 'h')
        {
            triangle.vertices.Add(new Vector3(x, y, z));
            triangle.vertices.Add(new Vector3(x, y, z + 1));
            triangle.vertices.Add(new Vector3(x + 1, y, z));
            triangle.vertices.Add(new Vector3(x + 1, y, z + 1));
            
            
        }
        else if (axis == 'v')
        {
            triangle.vertices.Add(new Vector3(x , y, z));
            triangle.vertices.Add(new Vector3(x , y + 1, z));
            triangle.vertices.Add(new Vector3(x , y, z + 1));
            triangle.vertices.Add(new Vector3(x , y + 1, z + 1));
            
        }
        else if (axis == 's')
        {
            triangle.vertices.Add(new Vector3(x, y, z));
            triangle.vertices.Add(new Vector3(x, y + 1, z));
            triangle.vertices.Add(new Vector3(x+1, y, z));
            triangle.vertices.Add(new Vector3(x+1, y + 1, z ));
            

        }
        triangle.triangles.AddRange(AddTriangleForPlane(orientation));
        Triangles.Add(triangle);

    }

    void CreateBox(int x, int y, int z)
    {
        
        Createplane(x, y + 1, z,'h','f');
        Createplane(x, y, z, 'h', 'b');
        Createplane(x+1, y, z, 'v', 'f');
        Createplane(x, y, z, 'v', 'b');
        Createplane(x, y, z, 's', 'f');
        Createplane(x, y, z+1, 's', 'b');
        
    }
    void CreateShape()
    {
        for(int y = 0, x = 0; x < 1; x += 1){
            for(var z=0; z < 1; z += 1){

                CreateBox(x,y, z);   
                y++;
            }

        }
        
    }

    void UpdateMesh()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        foreach(var triangle in Triangles){
            vertices.AddRange(triangle.vertices);
            triangles.AddRange(triangle.triangles);
        }

        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
    }

}
public class Triangle{
    public List<Vector3> vertices { get; set; }
    public List<int> triangles { get; set; }
}
