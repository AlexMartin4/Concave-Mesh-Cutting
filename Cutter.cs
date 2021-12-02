using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class Cutter : MonoBehaviour
{
  

    [Header("Cutter settings")]
    public GameObject genericSliceablePrefab;

    //Keeps track of all sliceable currently in the scene
    public List<Sliceable> activeSliceables;

    public float separationForce;
    public delegate void generatedNewPieces(List<Sliceable> upper, List<Sliceable> lower);
    public event generatedNewPieces onSlice;

    
    public struct CuttingPlane
    {
        public Vector3 position;
        public Vector3 normal;
    }

    /// <summary>
    /// This keeps track of an individual convex component cut
    /// </summary>
    public struct Split
    {
        public Sliceable original;
        public Sliceable upper;
        public Sliceable lower;

        public Split(Sliceable org, Sliceable up, Sliceable lo)
        {
            original = org;
            upper = up;
            lower = lo;
        }
    }


    /// <summary>
    /// Main Method of the Cutter script
    /// Cuts an abitrary object represented by a Sliceable object
    /// </summary>
    /// <param name="sliceable">Object to cut</param>
    /// <param name="plane">Cutting plane</param>
    /// <param name="addForces">If true, add force to the generated pieces</param>
    /// <param name="displacement">Moves the cutting plane </param>
    /// <returns></returns>
    public bool SliceConcaveObject(Sliceable sliceable, CuttingPlane plane, bool addForces, Vector3 displacement)
    {
        plane.position += displacement;

        //Check for empty
        if (sliceable.meshWrapper.convexDecomp.Count <= 0)
        {
            Debug.Log("Cannot slice concave object with no convex subparts");
            return false;

        }

        //We're spliting the generated meshes in an upper and lower part
        List<Sliceable> upper = new List<Sliceable>(); List<Sliceable> lower = new List<Sliceable>();
        List<Split> splits = new List<Split>();


        //Sliceable has a list of its convex components
        foreach (Sliceable s in sliceable.meshWrapper.convexDecomp)
        {
            Sliceable tempUpper = null; Sliceable tempLower = null;

            //SliceMesh returns true if the mesh coincides with the cutting plane
            //If the mesh was split we'll deal with it later i.e. when all the other pieces are in the right groups
            if (SliceMesh(s, plane.position, plane.normal, out tempUpper, out tempLower))
            {
                Split newSplit = new Split(s, tempUpper, tempLower);
                splits.Add(newSplit);
            }
            //All the pieces that are staying whole are assigned to the correct group
            else
            {
                if (MeshVolume.MeshAbovePlane(s.meshWrapper.mesh, s.transform, plane.position, plane.normal))
                {
                    upper.Add(s);
                    s.meshWrapper.isUpper = 1;
                }
                else
                {
                    lower.Add(s);
                    s.meshWrapper.isUpper = -1;
                }
            }

        }

        //If no pieces got cut we can stop now
        if (splits.Count == 0)
        {
            return false;
        }

        //Rearrange the neighbors of all the newly made meshes i.e. those that got split
        foreach (Split spl in splits)
        {
            #region Add the generated pieces to the upper and lower groups
            spl.upper.meshWrapper.isUpper = 1;
            spl.lower.meshWrapper.isUpper = -1;
            upper.Add(spl.upper);
            lower.Add(spl.lower);
            spl.upper.meshWrapper.neighbors = new List<Sliceable>();
            spl.lower.meshWrapper.neighbors = new List<Sliceable>();
            #endregion

            //New neighbors are neighbors of the original piece in on the same side of the cutting plane
            foreach (Sliceable n in spl.original.meshWrapper.neighbors)
            {
                if (n.meshWrapper.isUpper > 0)
                {
                    spl.upper.meshWrapper.neighbors.Add(n);
                    n.meshWrapper.neighbors.Add(spl.upper);
                }
                else if (n.meshWrapper.isUpper < 0)
                {
                    spl.lower.meshWrapper.neighbors.Add(n);
                    n.meshWrapper.neighbors.Add(spl.lower);
                }
                else
                {
                    n.meshWrapper.neighbors.Add(spl.upper);
                    n.meshWrapper.neighbors.Add(spl.lower);

                }


                n.meshWrapper.neighbors.Remove(spl.original);
            }

            Destroy(spl.original.gameObject);
        }

        //Instantiate both halves of the new mesh group
        if (onSlice != null)
        {
            onSlice(InstantiateHalf(sliceable, upper, plane.normal, addForces),
                    InstantiateHalf(sliceable, lower, -plane.normal, addForces));
        }
        else
        {
            InstantiateHalf(sliceable, upper, plane.normal, addForces);
            InstantiateHalf(sliceable, lower, -plane.normal, addForces);
        }

        //Clear the originial mesh from the active list
        activeSliceables.Remove(sliceable);
        sliceable.ClearComponents();

        return true;




    }

    /// <summary>
    /// Cuts a convex object
    /// </summary>
    /// <param name="original">Object that was cut</param>
    /// <param name="center">Center of the cutting plane</param>
    /// <param name="normal">Normal of the cutting plane</param>
    /// <param name="upperMesh">Outputs a new Sliceable object above the cut </param>
    /// <param name="lowerMesh">Outputs a new Sliceable object above the cut </param>
    /// <returns></returns>
    protected bool SliceMesh(Sliceable original, Vector3 center, Vector3 normal, out Sliceable upperMesh, out Sliceable lowerMesh)
    {
        SlicedHull s = original.gameObject.Slice(center, normal);
        if (s != null)
        {
            Sliceable upper = Instantiate(genericSliceablePrefab, original.transform).GetComponent<Sliceable>();
            upper.SetupSlicedConvex(s.upperHull, original, original.gameObject.name + " Upper ");


            upperMesh = upper;

            Sliceable lower = Instantiate(genericSliceablePrefab, original.transform).GetComponent<Sliceable>();
            lower.SetupSlicedConvex(s.lowerHull, original, original.gameObject.name + " Lower ");


            lowerMesh = lower;

            activeSliceables.Remove(original);
            original.ClearComponents();
            return true;
        }
        else
        {
            upperMesh = null;
            lowerMesh = null;
            return false;
        }
    }

    //After finding the slicing plane trhough camera projection we can cut the convex object
    public bool SliceConvexObject(Sliceable s, CuttingPlane plane, bool AddForces)
    {
        Sliceable upper, lower;

        if (SliceMesh(s, plane.position, plane.normal, out upper, out lower))
        {

            activeSliceables.Add(upper);
            activeSliceables.Add(lower);
            Debug.Log("Sliced convex!");
            if (onSlice != null)
            {
                List<Sliceable> newUppers = new List<Sliceable>();
                newUppers.Add(upper);

                if (AddForces && upper.rb) upper.rb.AddForce(plane.normal.normalized * separationForce);


                List<Sliceable> newLowers = new List<Sliceable>();
                newLowers.Add(lower);

                if (AddForces && lower.rb) lower.rb.AddForce(plane.normal.normalized * separationForce);

                onSlice(newUppers, newLowers);
            }
            

            return true;
        }

        onSlice(new List<Sliceable>(), new List<Sliceable>());

        return false;

    }

    public bool SliceConcaveObject(Sliceable sliceable, CuttingPlane plane, bool addForces)
    {
        return SliceConcaveObject(sliceable, plane, addForces, Vector3.zero);
    }

    /// <summary>
    ///Merges all neighborhoods of components on one side of the cutting plane
    ///Different neighborhoods will result in distinct meshes
    /// </summary>
    /// <param name="sliceable">The original object that was cut</param>
    /// <param name="upper">List of components to be merged</param>
    /// <param name="normal">Normal to the cutting plane (indicates force direction) </param>
    /// <param name="addForces">If true, add forces to the generated pieces</param>
    /// <returns>The list of generated sliceables</returns>

    private List<Sliceable> InstantiateHalf(Sliceable sliceable, List<Sliceable> upper, Vector3 normal, bool addForces)
    {
        List<Sliceable> result = new List<Sliceable>();

        foreach (List<Sliceable> group in SliceableMethods.getNeighborhoods(upper))
        {

            MeshWrapper combinedMesh = MeshWrapper.Merge(group);
            Sliceable newSliceable = Instantiate(genericSliceablePrefab, sliceable.transform).GetComponent<Sliceable>();
            newSliceable.SetupSlicedConcave(combinedMesh, sliceable, sliceable.name);


            //Move all members of the group to be children of the new Sliceable
            //Note: This makes the Hiearchy of the generated cut pieces into a Tree
            foreach (Sliceable s in group)
            {
                s.transform.SetParent(newSliceable.transform);
            }

            activeSliceables.Add(newSliceable);

            newSliceable.InitializeConcave();

            if (addForces && newSliceable.rb) newSliceable.rb.AddForce(normal.normalized * separationForce);

            result.Add(newSliceable);
        }

        return result;
    }


}
