using System;
using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Controllers
{
    public class PlatformMover : MonoBehaviour
    {
        [SerializeField] private Vector3 moveTo = Vector3.zero;
        [SerializeField] private float moveTime = 1f;
        [SerializeField] private Ease ease = Ease.InOutQuad;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
            Move();
        }

        private void Move()
        {
            // PingPong Movement
            transform.DOMove(startPosition + moveTo, moveTime)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}