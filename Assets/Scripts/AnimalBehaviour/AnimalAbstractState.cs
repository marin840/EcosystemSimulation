using UnityEngine;

namespace AnimalBehaviour
{
    public abstract class AnimalAbstractState
    {
        public abstract void EnterState(AnimalStateManager animal);

        public abstract void UpdateState(AnimalStateManager animal);
    }
}

