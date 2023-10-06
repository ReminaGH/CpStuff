using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent {

    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedArgs : EventArgs {
        public BaseCounter selectedCounter;
    }

    //Gives access to movespeed directly via Unity
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform KitchenObjectHoldPoint;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private void Awake() {
        if (Instance != null) {
            Debug.Log("There is more than 1 player instance");
        }
        Instance = this;
    }

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e) {
        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
        if (selectedCounter != null) {
            selectedCounter.Interact(this);
        }
    }

    private void Update() {
        HandleMovement();
        HandleInteractions();
    }

    //returns the value of walking to another object, referenced in player_controller
    public bool IsWalking() {
        return isWalking;
    }


    //Bad code example, change later as it is used by the event OnInteractAction instead
    private void HandleInteractions() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        //Check last interact distance
        if (moveDir != Vector3.zero) {
            lastInteractDir = moveDir;
        }

        //Direction check of the object infront of, using raycas
        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)) {

                //clearCounter.Interact(); // Commented out to not work
                if (baseCounter != selectedCounter) {
                    SetSelectedCounter(baseCounter);
                }

                } else {
                SetSelectedCounter(null);

                }
        } else {
            SetSelectedCounter(null);

        }
        
    }

    //Function for movement
    private void HandleMovement() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRaidus = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRaidus, moveDir, moveDistance);

        //Physicis check
        if (!canMove) {
            //Cannot move towards moveDir

            //Attempt move x only
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRaidus, moveDirX, moveDistance);

            if (canMove) {
                //Can only move X
                moveDir = moveDirX;
            } else {
            //Cannot move on only the x

            //Attempt to move on the z
            Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
               canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRaidus, moveDirZ, moveDistance);
                if (canMove) {
                    //Can only move z
                    moveDir = moveDirZ;
                } else {
                    //Cannot move in any direction
                }
            }
        }

        if (canMove) {
            transform.position += moveDir * moveDistance;
        }

        //Tests to see if the player is walking or not
        isWalking = moveDir != Vector3.zero;

        //Direction facing of the player with interplation
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter) {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedArgs {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform() {
        return KitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public bool HasKitchenObject() {
        return kitchenObject != null;
    }
}




//_______________________SAVED CODE____________________________//

//Selected counter code, old

/*Vector2 inputVector = gameInput.GetMovementVectorNormalized();
Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

//Check last interact distance
if (moveDir != Vector3.zero) {
    lastInteractDir = moveDir;
}

//Direction check of the object infront of, using raycas
float interactDistance = 2f;
if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
    if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)) {
        //Has clearCounter
        clearCounter.Interact();
    }
}*/