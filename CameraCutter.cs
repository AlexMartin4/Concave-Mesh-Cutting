using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EzySlice;

public class CameraCutter : Cutter
{

    static Camera camera;
    public bool applyForceOnCut;
    public float minimumMouseDrag;

    public delegate void sendPlane(CuttingPlane plane);
    public event sendPlane onMouseCut;

    public delegate void sendPoints(Vector2 a, Vector2 b);
    public event sendPoints onMouseUI;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        if (!camera) print("Camera not found!");

        StartCoroutine(SliceWithMouse());
    }

    public void Reset()
    {
        activeSliceables = new List<Sliceable>();
        this.enabled = true;
    }


    private void Update()
    {
       
    }

    // Update is called once per frame
    public static CuttingPlane getCutPlane(Vector2 a, Vector2 b)
    {
        return getCutPlane(a, b, Vector3.zero);
    }


    public static CuttingPlane getCutPlane(Vector2 a, Vector2 b, Vector3 orientation)
    {
        Vector3 worldA, worldB;
        worldA = camera.ScreenToWorldPoint(new Vector3(a.x, a.y, -camera.transform.position.z));
        worldB = camera.ScreenToWorldPoint(new Vector3(b.x, b.y, -camera.transform.position.z));

        Vector3 AtoB = worldB - worldA;

        Vector3 toMiddle = (worldA + 0.5f * AtoB) - camera.transform.position;

        Vector3 worldMiddle = new Vector3(0, 5f, 0);
        Vector3 planeOrient;

        //If we've spcified an orientation
        if (orientation != Vector3.zero)
        {
            planeOrient = orientation;
        }
        else
        {
            planeOrient = -worldMiddle + (worldA + 0.5f * AtoB);
        }

        //Debug.Log("World A: " + worldA.ToString() + " \n World B: " + worldB.ToString());
        Debug.DrawRay(worldA, AtoB, Color.green, float.PositiveInfinity);
        Debug.DrawRay(worldA, -AtoB, Color.green, float.PositiveInfinity);




        Vector3 perp = Vector3.Cross(AtoB, toMiddle).normalized;

        if(Vector3.Dot(planeOrient, perp) < 0)
        {
            perp = -perp;
        }



        Debug.DrawLine(worldA, worldA + perp, Color.red, float.PositiveInfinity);

        CuttingPlane result = new CuttingPlane();

        result.position = worldA;
        result.normal = perp;



        return result;
    }

    void CutBetweenScreenPoints(CuttingPlane p, Sliceable sliceable)
    {

        Sliceable upper, lower;
        if (sliceable.meshWrapper.isConvex)
        {
            SliceConvexObject(sliceable, p, applyForceOnCut);
        }
        else
        {
            SliceConcaveObject(sliceable, p, applyForceOnCut);
        }


    }


    public IEnumerator SliceWithMouse()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector2 start = Input.mousePosition;

                while (true)
                {

                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        Debug.Log("Cancelled cut");
                        break;
                    }

                    Vector2 end = Input.mousePosition;
                    if(onMouseUI!= null) onMouseUI(start, end);


                    if (Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        if ((end - start).magnitude > minimumMouseDrag)
                        {
                            CuttingPlane plane = getCutPlane(start, end, GameManager.instance.getCurrentGenCutNormal());
                            if(onMouseCut != null) onMouseCut(plane);
                           

                            if (activeSliceables != null)
                            {
                                List<Sliceable> temp = new List<Sliceable>(activeSliceables);

                                foreach (Sliceable s in temp)
                                {
                                    CutBetweenScreenPoints(plane, s);
                                }
                            }
                            else
                            {
                                Debug.Log("active sliceaceables is null");
                                break;
                            }
                        }
                        else Debug.Log("Mouse drag too short!");

                        break;
                    }

                    yield return null;
                }
            }

            yield return null;
        }
    }


    


    
}
