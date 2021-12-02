using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SliceableMethods 
{
    
    //TODO: Make more efficient with LINQ
    /// <summary>
    /// Finds all distinct neighborhoods (adjacent convex components)
    /// in a given list of Sliceable objects.
    /// </summary>
    /// <param name="sliceables"></param>
    /// <returns></returns>
    public static List<List<Sliceable>> getNeighborhoods(List<Sliceable> sliceables)
    {
        List<List<Sliceable>> result = new List<List<Sliceable>>();

        while (sliceables.Count > 0)
        {
            List<Sliceable> newNeighborhood = new List<Sliceable>();

            //Find all the nodes connected to the first node in the input list
            newNeighborhood.Add(sliceables[0]);
            for (int i = 0; i < newNeighborhood.Count; i++)
            {
                foreach (Sliceable s in newNeighborhood[i].meshWrapper.neighbors)
                {
                    if (sliceables.Contains(s) && !newNeighborhood.Contains(s))
                    {
                        newNeighborhood.Add(s);
                    }
                }
            }

            //Remove all nodes dicovered from the input list to avoid double counts
            foreach (Sliceable s in newNeighborhood)
            {
                sliceables.Remove(s);
            }

            //Add the neighborhood to the output
            result.Add(newNeighborhood);
        }


        return result;
    }
}
