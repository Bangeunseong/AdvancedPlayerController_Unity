namespace Project.Scripts.FSM.State
{
    public interface IState
    {
        void OnEnter();
        void OnExit() { }
        void Update() { }
        void FixedUpdate() { }
    }
}