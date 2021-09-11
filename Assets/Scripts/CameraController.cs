using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private PixelPerfectCamera ppc;

    [SerializeField]
    private Vector2 aspectRatio = new Vector2(16, 9);
    private Vector2 storedResolution;

    [SerializeField]
    [Range(1f, 4f)]
    private float zoom = 3;

    [SerializeField]
    private Transform targetTransform;

    // Start is called before the first frame update
    void Start()
    {
        UpdateAspectRatio();
        ServerLink.OnEntityCreated += OnEntityCreated;
    }

    private void OnDestroy()
    {
        ServerLink.OnEntityCreated -= OnEntityCreated;
    }

    private void OnEntityCreated(object sender, ServerLink.EntityCreatedEventArgs e)
    {
        if(e.entityInstance.HasAuthority())
        {
            targetTransform = e.entityInstance.transform;
        }
    }

    void UpdateAspectRatio()
    {
        storedResolution = new Vector2(Screen.width, Screen.height);
        aspectRatio = storedResolution.normalized;
        if (aspectRatio.y < aspectRatio.x)
            aspectRatio /= aspectRatio.y;
        else
            aspectRatio /= aspectRatio.x;
    }

    // Update is called once per frame
    void Update()
    {
        //handle camera zoom
        ppc.refResolutionX = (int)(aspectRatio.x * 100 * zoom);
        ppc.refResolutionY = (int)(aspectRatio.y * 100 * zoom);
        if (Screen.width != storedResolution.x || Screen.height != storedResolution.y)
            UpdateAspectRatio();

        if(targetTransform != null)
            transform.position = targetTransform.position + Vector3.back * 25;
    }

}
