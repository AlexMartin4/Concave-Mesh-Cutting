using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshVolume 
{
    //Code curtesy of https://answers.unity.com/questions/52664/how-would-one-calculate-a-3d-mesh-volume-in-unity.html
    //User: Statement; Egons


    public static float CalculateConvexVolume(Mesh mesh)
    {
        Vector3 center = getRandomInsidePoint(mesh);

        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]] - center;
            Vector3 p2 = vertices[triangles[i + 1]] - center;
            Vector3 p3 = vertices[triangles[i + 2]] - center;
            volume += SignedTriangleVolume(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }

    public static float SignedTriangleVolume(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    //Works only for convex meshes
    public static Vector3 getRandomInsidePoint(Mesh mesh)
    {
        int A, B;
        A = Random.Range(0, mesh.vertexCount);

        do
        {
            B = Random.Range(0, mesh.vertexCount);
        }
        while (A == B);

        Vector3 posA = mesh.vertices[A];
        Vector3 posB = mesh.vertices[B];

        Vector3 AtoB = posB - posA;

        Vector3 result = posA + 0.5f * AtoB;
       // Debug.DrawLine(Vector3.zero, result, Color.green, float.PositiveInfinity);
       // Debug.DrawLine(posA, result, Color.red, float.PositiveInfinity);
       

        return result;

    }

    //Returns true if the mesh is above the plane, false if under (edge case when one point 
    public static bool MeshAbovePlane(Mesh mesh, Transform transform, Vector3 position, Vector3 normal)
    {

        int A = Random.Range(0, mesh.vertexCount);
        Vector4 baseMeshA = new Vector4(mesh.vertices[A].x, mesh.vertices[A].y, mesh.vertices[A].z, 1);
        Vector3 worldA = (Vector3)(transform.localToWorldMatrix*baseMeshA);

        Vector3 PosToA = -position +  worldA; /*Times some tranform*/;
        //Vector3 PosToA = -position +  (mesh.vertices[A]);

        //Debug.DrawLine(position, position + PosToA, Color.blue, float.PositiveInfinity);

        float result = Vector3.Dot(PosToA, normal);

        /*if(result == 0)
        {
            Debug.Log("Edge case reccursion")
            return meshAbovePlane(mesh, transform, position, normal);
        }*/

        if (result > 0)
        {
            return true;
        }

        return false;
    }
}
