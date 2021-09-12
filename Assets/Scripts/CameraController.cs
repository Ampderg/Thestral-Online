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
    private float resetResolutionTimer = -1;

    [SerializeField]
    private Vector3 offset = new Vector3(0, 1f, -25f);

    [SerializeField]
    [Range(1f, 5f)]
    private float zoom = 3;

    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Transform flipReference;

    [SerializeField]
    private float cameraSpeed = 20;
    [SerializeField]
    private float cameraSnapDistance = 0.1f;

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
            flipReference = targetTransform.Find("CharacterDisplay");
        }
    }

    void UpdateAspectRatio()
    {
        storedResolution = new Vector2(Screen.width, Screen.height);
        if (storedResolution.x % 2 != 0 || storedResolution.y % 2 != 0)
            resetResolutionTimer = 0.5f;
        else
            resetResolutionTimer = -1;

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
        if (zoom < 5)
        {
            ppc.assetsPPU = 20;
            ppc.refResolutionX = (int)(aspectRatio.x * 100 * zoom);
            ppc.refResolutionY = (int)(aspectRatio.y * 100 * zoom);
        }
        else
        {
            ppc.assetsPPU = 10;
            ppc.refResolutionX = (int)(aspectRatio.x * 100 * 4);
            ppc.refResolutionY = (int)(aspectRatio.y * 100 * 4);
        }
        if (Screen.width != storedResolution.x || Screen.height != storedResolution.y)
            UpdateAspectRatio();

        //if (resetResolutionTimer > 0)
        //{
        //    resetResolutionTimer -= Time.deltaTime;
        //    if (resetResolutionTimer <= 0)
        //    {
        //        if (storedResolution.x % 2 != 0) storedResolution.x--;
        //        if (storedResolution.y % 2 != 0) storedResolution.y--;
        //        Screen.SetResolution((int)storedResolution.x, (int)storedResolution.y, false);
        //        UpdateAspectRatio();
        //    }
        //}

        if (Input.GetButtonDown("Camera Zoom"))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                zoom = Input.GetAxisRaw("Camera Zoom") > 0 ? 1 : 5;
            else
                zoom -= Mathf.Sign(Input.GetAxisRaw("Camera Zoom"));
            if (zoom < 1) zoom = 1;
            if (zoom > 5) zoom = 5;
        }

        if (targetTransform != null)
        {
            Vector3 targetPos;
            if (flipReference)
            {
                Vector3 off = offset;
                off.x *= Mathf.Sign(flipReference.localScale.x);
                targetPos = targetTransform.position + off;
            }
            else
            {
                targetPos = targetTransform.position + offset;
            }
            if (Vector3.Distance(transform.position, targetPos) > cameraSnapDistance)
                transform.position = Vector3.MoveTowards(transform.position, targetPos, cameraSpeed * Time.deltaTime);
            else
                transform.position = targetPos;
        }
    }

}
