using MAG.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MAG.General
{
    [System.Serializable] public class Vector3Event : UnityEvent<Vector3> { }

    public class InputManager : MonoBehaviour
    {
        public Camera cameraReference;
        public Transform boardOrigin;

        // Mouse Controls
        private bool areaClicked = false;
        private Vector3 mouseDownPosition = Vector3.zero;
        private Vector3 mouseDownWorldPosition = Vector3.zero;
        private Vector3 mouseHoldPosition = Vector3.zero;
        private Vector3 mouseHoldWorldPosition = Vector3.zero;
        private Vector3 mouseUpPosition = Vector3.zero;
        private Vector3 mouseUpWorldPosition = Vector3.zero;

        // Touch Contros
        private int fingerID = 0;

        private bool inputActive = true;

        // --- Properties ---
        public bool InputActive => inputActive;

        // --- Events ---
        private Vector3Event onMouseDown;
        public Vector3Event OnMouseDown => onMouseDown;

        private void Awake()
        {
            onMouseDown = new Vector3Event();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if(inputActive)
                ProcessInput(dt);
        }

        private void ProcessInput(float dt)
        {
            if(Input.touchCount > 0)
            {
                bool foundTouch = false;

                // --- Touch Input ---
                for(int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.touches[i];
                    bool currentTouchActive = areaClicked && fingerID == touch.fingerId;

                    // Check the Touch Phase and Execute
                    if(touch.phase == TouchPhase.Began && !areaClicked)
                        foundTouch = OnTouchDown(touch.position);
                    else if(touch.phase == TouchPhase.Moved && currentTouchActive)
                        OnTouchHold(touch.position);
                    else if(touch.phase == TouchPhase.Ended && currentTouchActive)
                        OnTouchUp(touch.position);
                    else if(touch.phase == TouchPhase.Canceled && currentTouchActive)
                        OnTouchUp(mouseHoldPosition);

                    if(foundTouch)
                    {
                        fingerID = touch.fingerId;
                        break;
                    }
                }
            }
            else
            {
                // --- Mouse Input ---
                if(Input.GetMouseButtonDown(0))
                    OnTouchDown(Input.mousePosition);
                else if(Input.GetMouseButton(0) && areaClicked)
                    OnTouchHold(Input.mousePosition);
                else if(Input.GetMouseButtonUp(0) && areaClicked)
                    OnTouchUp(Input.mousePosition);
            }
        }

        private bool OnTouchDown(Vector3 pointerPosition)
        {
            UnityEngine.EventSystems.EventSystem eventsystem = UnityEngine.EventSystems.EventSystem.current;

            if(eventsystem.IsPointerOverGameObject())
                return false;

            // --- On Mouse Down ---
            // Check if Movement MouseArea is Hit
            mouseDownPosition = pointerPosition;

            // Do PlaneCast to Check WorldPosition of Mouse
            Plane plane = new Plane(transform.forward, boardOrigin.position);
            Ray ray = cameraReference.ScreenPointToRay(pointerPosition);

            if(plane.Raycast(ray, out float enter))
            {
                mouseDownWorldPosition = ray.GetPoint(enter);
                //Debug.DrawLine(boardOrigin.position, mouseDownWorldPosition, Color.white, 10f);

                onMouseDown.Invoke(mouseDownWorldPosition);
                areaClicked = true;
                return true;
            }

            return false;
        }

        private void OnTouchHold(Vector3 pointerPosition)
        {
            mouseHoldPosition = pointerPosition;

            // Do PlaneCast to Check WorldPosition of Mouse
            Plane plane = new Plane(transform.forward, boardOrigin.position);
            Ray ray = cameraReference.ScreenPointToRay(pointerPosition);

            if(plane.Raycast(ray, out float enter))
            {
                mouseHoldWorldPosition = ray.GetPoint(enter);
            }
        }

        private void OnTouchUp(Vector3 pointerPosition)
        {
            // --- OnMouse Release ---
            // If the input was started and released, please execute the movement
            mouseUpPosition = pointerPosition;

            // Do PlaneCast to Check WorldPosition of Mouse
            Plane plane = new Plane(transform.forward, boardOrigin.position);
            Ray ray = cameraReference.ScreenPointToRay(pointerPosition);

            if(plane.Raycast(ray, out float enter))
            {
                mouseUpWorldPosition = ray.GetPoint(enter);
                Vector3 headingWorld = mouseUpWorldPosition - mouseDownWorldPosition;
                Debug.DrawLine(mouseDownWorldPosition, mouseUpWorldPosition, Color.yellow, 2f);
            }

            areaClicked = false;
        }
    }
}
