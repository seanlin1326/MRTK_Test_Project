using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace ThrowBallModel
{
    public class BallThrower : MonoBehaviour
    {
        public enum Status
        {
            Idle,Throwing, Thrown, Holding
        }

        [SerializeField] private GameObject ball;
        private Rigidbody ballRigid;

        private float startTime, endTime, swipeDistance, swipeTime;
        private Vector2 startPos;
        private Vector2 endPos;

        [SerializeField] private float holdingBallDistanceOffset = 8f;

        //the min distance to count it as a flick
        [SerializeField] private float MinSwipeDist = 0;

        [SerializeField] private float MaxBallSpeed = 40;
        [SerializeField] private float smooth = 0.7f;
        private bool canNotSwitchStatus = false;
        private float ballVelocity = 0;
        private float ballSpeed = 0;
        private Vector3 angle;

        private Status status = Status.Idle;
        private Coroutine currentCoroutine;

        // Start is called before the first frame update
        private void Start()
        {
            SetupBall();
        }

        private void SetupBall()
        {
            if (ball == null)
            {
                GameObject ball = GameObject.FindGameObjectWithTag("Player");
                this.ball = ball;
            }
            if (ball == null)
            {
                Debug.LogError("找不到可以丟出的球");
                return;
            }
            ballRigid = this.ball.GetComponent<Rigidbody>();
            ResetBall();
        }

        private void ResetBall()
        {
            angle = Vector3.zero;
            endPos = Vector3.zero;
            startPos = Vector3.zero;
            ballSpeed = 0;
            startTime = 0;
            endTime = 0;
            swipeDistance = 0;
            swipeTime = 0;
            //thrown = false;
            //holding = false;

            ballRigid.velocity = Vector3.zero;
            ballRigid.useGravity = false;
            ball.transform.position = transform.position;

            SwitchStatus(Status.Idle);
        }

        [EditorButton]
        private void PickupBall()
        {
            SwitchStatus(Status.Holding);
            currentCoroutine = StartCoroutine(PickupBallCoroutine());
        }

        private IEnumerator PickupBallCoroutine()
        {
            Vector3 mousePos;
            Vector3 newPosition;
           
            //canNotSwitchStatus = true;
            //mousePos = Input.mousePosition;
            //mousePos.z = Camera.main.nearClipPlane;
            //newPosition = Camera.main.ScreenToWorldPoint(mousePos) + new Vector3(0f, -0.5f, 2f);

            //Tweener tweener = ball.transform.DOLocalMove(newPosition, 1);
            //tweener.SetEase(Ease.OutQuad);
            //yield return tweener.WaitForCompletion();
            //canNotSwitchStatus = false;

            //int clickCount = 0;
            while (status == Status.Holding)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //clickCount += 1;
                    //if (clickCount > 1)
                    //{
                        SwitchStatus(Status.Throwing);
                        currentCoroutine = StartCoroutine(ThrowBallCoroutine());
                        yield break;
                    //}
                    
                }
                mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane + holdingBallDistanceOffset;
                newPosition = Camera.main.ScreenToWorldPoint(mousePos);
                ball.transform.localPosition = Vector3.Lerp(ball.transform.position, newPosition, 80 * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator ThrowBallCoroutine()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100f))
            {
                if (rayHit.transform == ball.transform)
                {
                    startTime = Time.time;
                    startPos = Input.mousePosition;
                }
                else
                {
                    Debug.LogError("射線沒有射到球");
                    ResetBall();
                    yield break;
                }
            }
            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    endTime = Time.time;
                    endPos = Input.mousePosition;
                    swipeDistance = (endPos - startPos).magnitude;
                    swipeTime = endTime - startTime;
                    //Debug.Log($"swipeDistance:{swipeDistance} swipeTime:{swipeTime}");
                    if(swipeTime < 2f &&  swipeDistance > 30f)
                    {
                        //throw ball
                        CalculateAngle();
                        CalculateSpeed();
                        ballRigid.AddForce(
                                           new Vector3(angle.x * ballSpeed, angle.y * (ballSpeed/2), -angle.z * ballSpeed *2) ) ;
                        ballRigid.useGravity = true;
                        SwitchStatus(Status.Thrown);
                      
                        Invoke(nameof(ResetBall),5f);
                      
                    }
                    else
                    {
                        Debug.Log("沒有丟出 重設");
                        ResetBall();
                    }
                    yield break;
                }
                yield return null;
            }
        }

        private void CalculateAngle()
        {
            angle = Camera.main.ScreenToWorldPoint(new Vector3(endPos.x, endPos.y +50f,(Camera.main.nearClipPlane+5)   ));
            //Debug.Log(angle);
         
        }
        private void CalculateSpeed()
        {
            //if (swipeTime > 0)
            //{
            //    ballVelocity = swipeDistance / (swipeDistance - swipeTime);
            //}
            //ballSpeed = ballVelocity * 40f;

            //if(ballSpeed >= MaxBallSpeed)
            //{
            //    ballSpeed = MaxBallSpeed;
            //}
            ballSpeed = MaxBallSpeed;
            swipeTime = 0;
        }
        private void SwitchStatus(Status switchToStatus)
        {
            if (canNotSwitchStatus)
            {
                Debug.LogError("當前無法切換狀態");
            }
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
            status = switchToStatus;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && status == Status.Idle)
            {
                ResetBall();
                PickupBall();
            }
        }
    }
}