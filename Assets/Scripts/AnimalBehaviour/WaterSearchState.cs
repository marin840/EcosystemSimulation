using UnityEngine;
using System.Collections;

namespace AnimalBehaviour
{
    public class WaterSearchState : AnimalAbstractState
    {
        private Rigidbody _rigidbody;
        private Animator _animator;

        private bool goingToWater;
        private Vector3 nearestWaterPoint;

        private float waterLevel = 0.35f;
        public override void EnterState(AnimalStateManager animal)
        {
            _rigidbody = animal.GetComponent<Rigidbody>();
            _animator = animal.GetComponent<Animator>();
            
            goingToWater = false;

            animal.statusText.text = "Searching for water";
        }

        
        public override void UpdateState(AnimalStateManager animal)
        {
            if (!goingToWater)
            {
                nearestWaterPoint = CalculateWaterDirection(animal);
                if (nearestWaterPoint.x == 0 && nearestWaterPoint.z == 0)
                {
                    if (!animal.randomMovement)
                    {
                        animal.randomMovement = true;
                        animal.StartCoroutine(animal.RandomMovement());
                    }
                    return;
                }
                goingToWater = true;
            }
            
            animal.movementDirection = (nearestWaterPoint - _rigidbody.position).normalized;
            if ((_rigidbody.position - nearestWaterPoint).magnitude < 0.3f)
            {
                _animator.Play("Drink");
                animal.thirstIncSpeed = animal.reduceThirstSpeed ;
                animal.movementDirection = Vector3.zero;
            }
        }
        
        
        public Vector3 CalculateWaterDirection(AnimalStateManager animal)
        {
            Vector3 nearestVec = Vector3.zero;
            RaycastHit hit;
            float nearestPoint = float.MaxValue;

            for (int angle = 0; angle < 360; angle += 5)
            {
                if (Physics.Raycast(_rigidbody.position + Vector3.up * 2,
                    Quaternion.AngleAxis(angle, Vector3.up) * Vector3.left, out hit, animal.SenseRange, animal.waterMeshLayer))
                {
                    if (hit.distance < nearestPoint)
                    {
                        nearestPoint = hit.distance;
                        nearestVec = hit.point;
                    }
                }
            }

            nearestVec.y = waterLevel;
            return nearestVec;
        }
    }
}

