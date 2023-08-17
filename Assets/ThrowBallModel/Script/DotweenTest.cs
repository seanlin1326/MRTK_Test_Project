using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace ThrowBallModel
{
    public class DotweenTest : MonoBehaviour
    {
        [SerializeField] private GameObject ball;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private DG.Tweening.Ease getBallEase;
        private Vector3 ballStartPoint;
        Coroutine currentRunCoroutine;
        private void Start()
        {
            ballStartPoint =(ball!= null)? ball.transform.position: new Vector3(10,10,10);
           
        }
        [EditorButton]
        private void ResetBall()
        {
            ball.transform.position = ballStartPoint;
        }
        [EditorButton]
        private void MoveBall()
        {
            if (currentRunCoroutine != null)
            {
                StopCoroutine(currentRunCoroutine);
                currentRunCoroutine = null;
                Debug.Log(currentRunCoroutine == null);
            }
            currentRunCoroutine =  StartCoroutine(MoveBallCoroutine());
        }
        IEnumerator MoveBallCoroutine()
        { 
            yield return null;
            Tweener tweener = ball.transform.DOMove(targetPoint.transform.position, 1);
            tweener.SetEase(getBallEase);
            yield return tweener.WaitForCompletion();
        }
    }
}