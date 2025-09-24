using UnityEngine;

namespace Project.Scripts.Controllers.Obstacles
{
    public static class WaitFor
    {
        static readonly WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
        public static WaitForFixedUpdate FixedUpdate => fixedUpdate;
        
        static readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        public static WaitForEndOfFrame EndOfFrame => endOfFrame;
    }
}