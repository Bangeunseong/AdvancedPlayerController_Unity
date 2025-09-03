using System;
using System.Collections;
using KBCore.Refs;
using Project.Scripts.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace Project.Scripts.Controllers
{
    public class CameraController : ValidatedMonoBehaviour
    {
        [Header("References")]
        [SerializeField, Anywhere] private InputReader input;
        [SerializeField, Anywhere] private CinemachineOrbitalFollow freeLookVCam;

        [Header("Settings")] 
        [SerializeField, Range(0.5f, 3f)] private float speedMultiplier = 1f;
        
        private bool isRMBPressed;
        private bool isDeviceMouse;
        private bool cameraMovementLock;

        private void OnEnable()
        {
            input.Look += OnLook;
            input.EnableMouseControlCamera += OnEnableMouseControlCamera;
            input.DisableMouseControlCamera += OnDisableMouseControlCamera;
        }

        private void OnDisable()
        {
            input.Look -= OnLook;
            input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
            input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
        }

        private void OnLook(Vector2 cameraMovement, bool isDeviceMouse)
        {
            if (cameraMovementLock) return;
            if (isDeviceMouse && !isRMBPressed) return;
            
            float deviceMultiplier = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;

            freeLookVCam.HorizontalAxis.Value += cameraMovement.x * speedMultiplier * deviceMultiplier;
            freeLookVCam.VerticalAxis.Value += cameraMovement.y * speedMultiplier * deviceMultiplier;
        }
        
        private void OnEnableMouseControlCamera()
        {
            isRMBPressed = true;
                
            // Lock the cursor and hide it
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(DisableMouseForFrame());
        }

        private void OnDisableMouseControlCamera()
        {
            isRMBPressed = false;

            // Unlock the cursor and make it visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Reset the camera axis to prevent jumping when re-enabling mouse control
            freeLookVCam.HorizontalAxis.Value = 0f;
            freeLookVCam.VerticalAxis.Value = 0f;
        }

        private IEnumerator DisableMouseForFrame()
        {
            cameraMovementLock = true;
            yield return new WaitForEndOfFrame();
            cameraMovementLock = false;
        }

    }
}