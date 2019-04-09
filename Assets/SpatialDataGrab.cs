using UnityEngine.VR.WSA;
using UnityEngine;
using System.Collections;

public class SpatialDataGrab : MonoBehaviour {

    [Tooltip("Determines if the surface observer should be automatically started.")]
    public bool autoStartObserver = true;

    /// <summary>
    /// Used for gathering real-time Spatial Mapping data on the HoloLens.
    /// </summary>
    private SurfaceObserver surfaceObserver;

    /// <summary>
    /// Time when StartObserver() was called.
    /// </summary>
    [HideInInspector]
    public float StartTime { get; private set; }

    // Called when the GameObject is first created.
    protected void Awake()
    {
        surfaceObserver = gameObject.GetComponent<SurfaceObserver>();
        //StartCoroutine(UpdateLoop());
    }

    // Use this for initialization
    void Start () {
        surfaceObserver.SetVolumeAsAxisAlignedBox(Vector3.zero, new Vector3(3, 3, 3));
        surfaceObserver.Update(OnSurfaceChanged);
    }

    IEnumerator UpdateLoop()
    {
        var wait = new WaitForSeconds(2.5f);
        while (true)
        {
            surfaceObserver.Update(OnSurfaceChanged);
            yield return wait;
        }
    }

    System.Collections.Generic.Dictionary<SurfaceId, GameObject> spatialMeshObjects = new System.Collections.Generic.Dictionary<SurfaceId, GameObject>();

    private void OnSurfaceChanged(SurfaceId surfaceId, SurfaceChange changeType, Bounds bounds, System.DateTime updateTime)
    {
        switch (changeType)
        {
            case SurfaceChange.Added:
            case SurfaceChange.Updated:
                if (!spatialMeshObjects.ContainsKey(surfaceId))
                {
                    spatialMeshObjects[surfaceId] = new GameObject("spatial-mapping-" + surfaceId);
                    spatialMeshObjects[surfaceId].transform.parent = this.transform;
                    spatialMeshObjects[surfaceId].AddComponent<MeshRenderer>();
                }
                GameObject target = spatialMeshObjects[surfaceId];

                SurfaceData sd = new SurfaceData(
                    //the surface id returned from the system
                    surfaceId,
                    //the mesh filter that is populated with the spatial mapping data for this mesh
                    target.GetComponent<MeshFilter>() ?? target.AddComponent<MeshFilter>(),
                    //the world anchor used to position the spatial mapping mesh in the world
                    target.GetComponent<WorldAnchor>() ?? target.AddComponent<WorldAnchor>(),
                    //the mesh collider that is populated with collider data for this mesh, if true is passed to bakeMeshes below
                    target.GetComponent<MeshCollider>() ?? target.AddComponent<MeshCollider>(),
                    //triangles per cubic meter requested for this mesh
                    300,
                    //bakeMeshes - if true, the mesh collider is populated, if false, the mesh collider is empty.
                    true
                    );

                //SurfaceObserver.RequestMeshAsync(sd, OnDataReady);
                // make the request
                if (surfaceObserver.RequestMeshAsync(sd, NewSurfaceBaked))
                {
                    // New surface request is in the queue and the specified callback will be invoked at a later frame.
                }
                else
                {
                    // New surface request has failed.  No callback for this request will be issued.
                }
                break;
            case SurfaceChange.Removed:
                var obj = spatialMeshObjects[surfaceId];
                spatialMeshObjects.Remove(surfaceId);
                if (obj != null)
                {
                    GameObject.Destroy(obj);
                }
                break;
            default:
                break;
        }
    }

    // Request a new Surface's data given the SurfaceId and the SurfaceObserver.
    void RequestMeshData(SurfaceId id, SurfaceObserver observer)
    {
        // create a new GameObject to hold the new Surface with all the appropriate components
        GameObject newSurface = new GameObject("Surface-" + id.handle);

        // fill out the SurfaceData struct in order to call RequestMeshAsync
        SurfaceData sd;
        sd.id = id;
        sd.outputMesh = newSurface.AddComponent<MeshFilter>();
        sd.outputAnchor = newSurface.AddComponent<WorldAnchor>();
        sd.outputCollider = newSurface.AddComponent<MeshCollider>();
        sd.trianglesPerCubicMeter = 300.0f;
        sd.bakeCollider = true;

        // make the request
        if (observer.RequestMeshAsync(sd, NewSurfaceBaked))
        {
            // New surface request is in the queue and the specified callback will be invoked at a later frame.
        }
        else
        {
            // New surface request has failed.  No callback for this request will be issued.
        }
    }

    void NewSurfaceBaked(SurfaceData sd, bool outputWritten, float elapsedBakeTimeSeconds)
    {
        if (outputWritten)
        {

            // Request completed successfully
            //Debug.Log();
        }
        else
        {
            // Request has failed.
        }
    }
}

