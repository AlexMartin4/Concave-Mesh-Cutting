using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///The mesh wrapper handles all operations directly involving the mesh
///It also stores the volume, convexity, materials and other info
/// </summary>
[System.Serializable]
public class MeshWrapper 
{

    public Mesh mesh;
    //public List<ContactPoint> contactPoints;
    public float volume;
    public List<Sliceable> neighbors;

    [Header("Convexity")]
    public bool isConvex = false;


    public List<Sliceable> convexDecomp;
    [HideInInspector]
    public List<Material> materials;
    [HideInInspector]
    public List<Vector3> cutPositions;


    //1 if upper, -1 is lower, 0 is unassigned
    [HideInInspector]
    public int isUpper = 0;

    //Used for new convex meshes
    public MeshWrapper(Mesh newMesh, List<Sliceable> newNeighbors)
    {
        mesh = newMesh;
        neighbors = newNeighbors;
        volume = MeshVolume.CalculateConvexVolume(newMesh);
        isConvex = true;
    }

    //Used for new concave meshes
    public MeshWrapper(Mesh newMesh, List<Sliceable> newNeighbors, float newVolume)
    {
        mesh = newMesh;
        neighbors = newNeighbors;
        volume = newVolume;
        isConvex = false;

    }

    public MeshWrapper(Mesh newMesh, List<Sliceable> newNeighbors, float newVolume, List<Sliceable> decomp)
    {
        mesh = newMesh;
        neighbors = newNeighbors;
        volume = newVolume;
        isConvex = false;
        convexDecomp = decomp;
    }


    public MeshWrapper(Mesh newMesh, List<Sliceable> newNeighbors, float newVolume, List<Sliceable> decomp, List<Material> mats)
    {
        mesh = newMesh;
        neighbors = newNeighbors;
        volume = newVolume;
        isConvex = false;
        convexDecomp = decomp;
        materials = mats;
    }

    /// <summary>
    /// Copies members from the model to this object
    /// </summary>
    /// <param name="model"></param>
    public void Clone(MeshWrapper model)
    {
        mesh = model.mesh;
        neighbors = model.neighbors;
        isConvex = model.isConvex;
        volume = model.volume;
    }


    //Used to merge a neighborhood of meshWrappers
    //The list of sliceable that is passed in is assumed to be a full neighborhood
    public static MeshWrapper Merge(List<Sliceable> sliceables)
    {

        Mesh resultMesh = new Mesh();
        float newVolume = 0;
        List<CombineInstance> combine = new List<CombineInstance>();
        List<Material> mats = new List<Material>();

        foreach(Sliceable s in sliceables)
        {
            for(int ii = 0; ii < s.meshWrapper.mesh.subMeshCount; ii++)
            {
                CombineInstance temp = new CombineInstance();
                temp.mesh = s.meshWrapper.mesh;
                temp.subMeshIndex = ii;
                temp.transform = Matrix4x4.identity;
                newVolume += s.meshWrapper.volume;
                combine.Add(temp);
                mats.Add(s.meshRenderer.materials[ii]);
            }
            
        }

        resultMesh.CombineMeshes(combine.ToArray(), false);

        //The new mesh shouldn't have any neighbors (given the assumption made above)
        List<Sliceable> newNeighbors = new List<Sliceable>();
        
        return new MeshWrapper(resultMesh, newNeighbors, newVolume, sliceables, mats);

    }

   
}


