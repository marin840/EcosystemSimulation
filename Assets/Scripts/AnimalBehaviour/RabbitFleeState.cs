using UnityEngine;

namespace AnimalBehaviour
{
    public class RabbitFleeState : AnimalAbstractState
    {
        private Rigidbody _rigidbody;
        
        public override void EnterState(AnimalStateManager animal)
        {
            _rigidbody = animal.GetComponent<Rigidbody>();
            animal.statusText.text = "Running away";
        }

        public override void UpdateState(AnimalStateManager rabbit)
        {
            Collider[] foxColliders = Physics.OverlapSphere(_rigidbody.position, rabbit.SenseRange * 0.4f, rabbit.foxLayer);

            double minDist = double.MaxValue;
            Vector3 runVec = Vector3.zero;
            foreach (var hitCollider in foxColliders)
            {
                Vector3 foxPos = hitCollider.transform.position;
                runVec = _rigidbody.position - foxPos;
                
                if (runVec.magnitude < minDist)
                {
                    minDist = runVec.magnitude;
                }
            }
            
            rabbit.movementDirection = runVec.normalized;
        }
    }

}
