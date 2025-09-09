using System;
using Project.Scripts.Input;
using UnityEngine;

namespace Project.Scripts.Controllers.AdvancedPlayer
{
    public class CameraController : MonoBehaviour
    {
        private float currentXAngle;
        private float currentYAngle;

        [Header("Vertical Settings")]
        [Range(0f, 90f)] public float upperVerticalLimit = 35f;
        [Range(0f, 90f)] public float lowerVerticalLimit = 35f;
        
        [Header("Camera Settings")]
        public float cameraSpeed = 50f;
        public bool smoothCameraRotation;
        [Range(1f, 50f)] public float cameraSmoothingFactor = 25f;

        [Header("Input S.O.")]
        private Transform tr;
        private Camera cam;
        [SerializeField] private InputReader input;

        public Vector3 GetUpDirection() => tr.up;
        public Vector3 GetFacingDirection() => tr.forward;

        private void Awake()
        {
            tr = transform;
            cam = GetComponentInChildren<Camera>();
            
            currentXAngle = tr.localRotation.eulerAngles.x;
            currentYAngle = tr.localRotation.eulerAngles.y;
        }

        private void Update()
        {
            RotateCamera(input.LookDirection.x, -input.LookDirection.y);
        }

        private void RotateCamera(float horizontalInput, float verticalInput)
        {
            if (smoothCameraRotation)
            {
                horizontalInput = Mathf.Lerp(0, horizontalInput, Time.smoothDeltaTime * cameraSmoothingFactor);
                verticalInput = Mathf.Lerp(0, verticalInput, Time.smoothDeltaTime * cameraSmoothingFactor);
            }

            currentXAngle += verticalInput * cameraSpeed * Time.smoothDeltaTime;
            currentYAngle += horizontalInput * cameraSpeed * Time.smoothDeltaTime;
            
            currentXAngle = Mathf.Clamp(currentXAngle, -upperVerticalLimit, lowerVerticalLimit);
            
            tr.localRotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
        }
    }
}