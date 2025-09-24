using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Controllers.Enemy
{
    public interface IDetectionStrategy
    {
        bool Execute(Transform player, Transform detector, Timer.CountdownTimer timer);
    }
}