using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The base class for every object that can be sliced by the cutting algorithm
/// </summary>
public class Sliceable : MonoBehaviour
{
    [HideInInspector] public int recursionNumber;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public Rigidbody rb;

    public MeshWrapper meshWrapper;
    
    // Start is called before the first frame update
    void Awake()
    {
        //Assign a mesh volume if the mesh is convex
        //Note: this is called in Awake for convex meshes because concave meshes use 
        //this info in Start -- a better solution would be to use properties
        if (meshWrapper.isConvex)
        {
            meshWrapper.volume = MeshVolume.CalculateConvexVolume(meshWrapper.mesh);
        }
        
    }

    private void Start()
    {
        if (!meshWrapper.isConvex)
        {
            InitializeConcave();
        }
    }

    public void InitializeConcave()
    {
        //Volume of convex volume = SUM volume of convex components
        meshWrapper.volume = 0f;

        foreach (Sliceable s in meshWrapper.convexDecomp)
        {
            meshWrapper.volume += s.meshWrapper.volume;
            s.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Clears all components on a gameobject
    /// </summary>
    public void ClearComponents()
    {
        foreach (var com in gameObject.GetComponents<Component>())
        {
            if(com != this && com!= transform)
            {
                Destroy(com);
            }
        }
        //Destroy this last to not interupt the method
        Destroy(this);
    }

    /// <summary>
    /// Sets up a new mesh for the sliceable object
    /// </summary>
    /// <param name="newMesh"> The fragment that will become a new sliceable</param>
    /// <param name="wholeObject"> The object that was sliced to create the fragment</param>
    public void SetupSliced(Mesh newMesh, Sliceable wholeObject)
    {
        meshFilter.mesh = newMesh;
        if(meshCollider) meshCollider.sharedMesh = newMesh;

        //Keep track of cutting depth to potetially limit # of cuts
        recursionNumber = 1 + wholeObject.recursionNumber;
        transform.localPosition = Vector3.zero;
        meshWrapper.Clone(wholeObject.meshWrapper);
        meshWrapper.mesh = newMesh;
    }

    public void SetupSlicedConvex(Mesh newMesh, Sliceable wholeObject)
    {
        SetupSliced(newMesh, wholeObject);


        Material[] temp = new Material[meshFilter.mesh.subMeshCount];
        temp[temp.Length - 1] = GameManager.instance.bloodMat;



        for (int i = 0; i < wholeObject.meshRenderer.materials.Length; i++)
        {
            temp[i] = wholeObject.meshRenderer.materials[i];
        }


        meshRenderer.materials = temp;
    }



    public void SetupSlicedConcave(MeshWrapper newMeshWrapper, Sliceable originial)
    {

        SetupSliced(newMeshWrapper.mesh, originial);
        //meshCollider.convex = false;
        meshWrapper = newMeshWrapper;
        meshRenderer.materials = meshWrapper.materials.ToArray();

    }





    public void SetupSliced(Mesh newMesh, Sliceable wholeObject, string name)
    {
        SetupSliced(newMesh, wholeObject);
        gameObject.name = name + recursionNumber;
    }

    public void SetupSlicedConcave(MeshWrapper newMeshWrapper, Sliceable originial, string name)
    {

        SetupSlicedConcave(newMeshWrapper, originial);
        gameObject.name = name + " " + recursionNumber;
    }


    public void SetupSlicedConvex(Mesh newMesh, Sliceable originial, string name)
    {

        SetupSlicedConvex(newMesh, originial);
        gameObject.name = name + " " + recursionNumber;
    }
}
