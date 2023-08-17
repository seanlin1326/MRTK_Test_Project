using System.Collections;
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
    /// <summary>
    /// This class manages the state for the game orb entity and orchestrates the manipulation of related references 
    /// in response to state and input changes.
    /// </summary>
    public class OrbManager : MonoBehaviour, IMixedRealityInputHandler
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
        private IMixedRealityController trackedController;
        [Header("Events")]
        public UnityEvent OnFire = new UnityEvent();
        public UnityEvent OnRetrieve = new UnityEvent();
        [Header("PowerUp")]
        [SerializeField]private float powerUpMax = 1.15f;
        [SerializeField] private float powerUpForceMultiplier=3f;
        private float powerUpTimer;
        private bool poweringUp = false;


        private OrbState state = OrbState.Idle;
        private IMixedRealityPointer trackedLinePointer;
        private bool wasTracked = false;
      
     
       
        public bool IsTracking => solverHandler.TransformTarget != null;
        private bool IsPoweringUp
        {
            get => poweringUp;
            set
            {
                if(poweringUp != value)
                {
                    poweringUp = value;
                    PowerUpUpdate();
                }
            }
        }
        private OrbState CurrebtState
        {
            get => state;
            set
            {
                if(state != value)
                {
                    state = value;
                    OrbStateUpdate();
                }
            }
        }
        private float PowerUpForce => Mathf.Clamp(powerUpTimer, 0.0f, powerUpMax) * powerUpForceMultiplier;
        private Vector3 TrackedPointerDirection => trackedLinePointer != null ? trackedLinePointer.Rotation * Vector3.forward : Vector3.zero;
        private void Awake()
        {
            //Default initialize values
            PowerUpUpdate();
            OrbStateUpdate();
        }
        private void OnEnable()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
        }
        private void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler>(this);
        }
        private void Update()
        {
            bool isTracked = this.IsTracking;
            if(wasTracked != IsTracking)
            {
                trackedController = isTracked ? GetTrackedController(solverHandler.CurrentTrackedHandedness):null;
                trackedLinePointer = isTracked ? GetLinePointer(trackedController) : null;
                wasTracked = isTracked;
            }
            if (isTracked)
            {
                // If we are now tracking a controller and we were idle, then switch to source tracking state
                if(CurrebtState == OrbState.Idle)
                {
                    CurrebtState = OrbState.SourceTracked;
                }
                else if(CurrebtState == OrbState.SourceTracked && IsPoweringUp)
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
      
            if(trackedLinePointer != null)
            {
                var forceVector = TrackedPointerDirection * PowerUpForce;
                CurrebtState = OrbState.PhysicsTracked;
                orbRigidbody.AddForce(forceVector, ForceMode.Impulse);
                OnFire?.Invoke();
            }
        }

        #region IMixedRealityInputHandler implementation
        public void OnInputUp(InputEventData eventData)
        {
            if (IsTrackingSource(eventData.SourceId)
                && eventData.MixedRealityInputAction.Description == nameof(DeviceInputType.Select))
            {
                if(CurrebtState == OrbState.SourceTracked)
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

       

        public void OnInputDown(InputEventData eventData)
        {
            if (IsTrackingSource(eventData.SourceId)
                  && eventData.MixedRealityInputAction.Description == nameof(DeviceInputType.Select))
            {
                if (CurrebtState == OrbState.SourceTracked)
                {
                    IsPoweringUp = true;
                    powerUpTimer = 0.0f;
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
            if (controller == null)
            {
                return null;
            }
            foreach (var pointer in controller?.InputSource.Pointers)
            {
                if(pointer is LinePointer linePointer)
                {
                    return linePointer;
                }
            }
            return null;
        }
        #endregion
        
    }
}