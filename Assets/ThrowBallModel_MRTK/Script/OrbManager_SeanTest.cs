﻿using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine.Events;
using UnityEngine;
using System;
namespace ThrowBallModel_MRTK
{
    public class OrbManager_SeanTest : MonoBehaviour,IMixedRealityInputHandler
    {
        private enum OrbState
        {
            Idle,
            SourceTracked,
            PhysicsTracked,
        };
        [Header("References")]
        [SerializeField]
        private Rigidbody orbRigidbody;
        [SerializeField]
        private MeshRenderer mesh;
        [SerializeField]
        private SolverHandler solverHandler;
        [Header("Events")]
        public UnityEvent OnFire = new UnityEvent();
        public UnityEvent OnRetrieve = new UnityEvent();
        [Header("PowerUp")]
        [SerializeField] private float powerUpMax = 1.15f;
        [SerializeField] private float powerUpForceMultiplier = 3f;

        private IMixedRealityController trackedController;
        private IMixedRealityPointer trackedLinePointer;

        private float powerUpTimer;
        private bool poweringUp = false;
        private bool wasTracked = false;



        [SerializeField]private OrbState state = OrbState.Idle;
        private OrbState CurrebtState
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    OrbStateUpdate();
                }
            }
        }
        private bool IsPoweringUp
        {
            get => poweringUp;
            set
            {
                if (poweringUp != value)
                {
                    poweringUp = value;
                    PowerUpUpdate();
                }
            }
        }
        private float PowerUpForce => Mathf.Clamp(powerUpTimer, 0.0f, powerUpMax) * powerUpForceMultiplier;
        private Vector3 TrackedPointerDirection => trackedLinePointer != null ? trackedLinePointer.Rotation * Vector3.forward : Vector3.zero;
        public bool IsTracking => solverHandler.TransformTarget != null;

        private void OnEnable()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
        }
        private void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler>(this);
        }

        [EditorButton]
        private void Test()
        {
            
            
           
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Test();
            }
            //記錄這針球的solver有沒有作用
            bool isTracked = IsTracking;
            var newController = GetTrackedController(solverHandler.CurrentTrackedHandedness);
            //if (trackedController != newController)
            //{
            //   if(newController == null)
            //    {
            //        Debug.Log("null");

            //    }
            //    else
            //    {
            //        Debug.Log(newController.ControllerHandedness);
            //    }
            //}
            trackedController = isTracked ? GetTrackedController(solverHandler.CurrentTrackedHandedness) : null;
              
      
          
            
            if (isTracked)
            {
                // If we are now tracking a controller and we were idle, then switch to source tracking state
                if (CurrebtState == OrbState.Idle)
                {
                    CurrebtState = OrbState.SourceTracked;
                }
                else if (CurrebtState == OrbState.SourceTracked && IsPoweringUp)
                {
                    powerUpTimer += Time.deltaTime;
                }
            }
            else
            {
                CurrebtState = OrbState.Idle;
            }
        }
        private void PowerUpUpdate()
        {
            powerUpTimer = 0.0f;
        }
        private void OrbStateUpdate()
        {
            IsPoweringUp = false;
            solverHandler.UpdateSolvers = CurrebtState != OrbState.PhysicsTracked;
            mesh.enabled = CurrebtState != OrbState.Idle;
            orbRigidbody.isKinematic = CurrebtState != OrbState.PhysicsTracked;
        }
        private void Fire()
        {
            trackedLinePointer =  GetLinePointer(trackedController);
            if (trackedLinePointer != null)
            {
                var forceVector = TrackedPointerDirection * PowerUpForce;
                CurrebtState = OrbState.PhysicsTracked;
                orbRigidbody.AddForce(forceVector, ForceMode.Impulse);
                OnFire?.Invoke();
            }
        }
        #region IMixedRealityInputHandler implementation
        public void OnInputDown(InputEventData eventData)
        {

          
            if (IsTrackingSource(eventData.SourceId) && 
                   eventData.MixedRealityInputAction.Description == nameof(DeviceInputType.Select))
            {
                if (CurrebtState == OrbState.SourceTracked)
                {
                    IsPoweringUp = true;
                    powerUpTimer = 0.0f;
                }

            }
        }

        public void OnInputUp(InputEventData eventData)
        {
          
            if (IsTrackingSource(eventData.SourceId) &&
                eventData.MixedRealityInputAction.Description == nameof(DeviceInputType.Select))
            {
                if (CurrebtState == OrbState.SourceTracked)
                {
                    Fire();
                }
                else
                {
                    CurrebtState = OrbState.SourceTracked;
                    OnRetrieve?.Invoke();
                }
            }
        }
        #endregion
        #region -- Helpers --

        private bool IsTrackingSource(uint sourceID)
        {
            return trackedController?.InputSource.SourceId == sourceID;
        }
        private IMixedRealityController GetTrackedController(Handedness handedness)
        {

            foreach (IMixedRealityController c in CoreServices.InputSystem?.DetectedControllers)
            {
                if (c.ControllerHandedness.IsMatch(handedness))
                {
                    return c;
                }
            }
            return null;
        }
        private IMixedRealityPointer GetLinePointer(IMixedRealityController controller)
        {
           if(controller == null)
            {
                Debug.LogError("找不到GetLinePointer");
                return null;
            }
            else
            {
                //Debug.Log("有找到GetLinePointer");
            }
          
            foreach (var pointer in controller?.InputSource.Pointers)
            {
                
                if (pointer is LinePointer linePointer)
                {
                    return linePointer;
                }
            }
            return null;
        }
        #endregion
    }
}