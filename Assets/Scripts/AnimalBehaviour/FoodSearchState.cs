using UnityEngine;
using System.Collections;
using UnityEditor;

namespace AnimalBehaviour
{
    public class FoodSearchState : AnimalAbstractState
    {
        private Rigidbody _rigidbody;

        private bool goingToFood;
        private Vector3 nearestFoodPoint;

        public override void EnterState(AnimalStateManager animal)
        {
            _rigidbody = animal.GetComponent<Rigidbody>();
            goingToFood = false;

            animal.statusText.text = "Searching for food";
        }

        public override void UpdateState(AnimalStateManager animal)
        {
            Collider[] foodColliders = Physics.OverlapSphere(_rigidbody.position, animal.SenseRange, animal.foodLayer);
            GameObject food = null;

            double minDist = double.MaxValue;
            Vector3 closestFoodPos = Vector3.zero;
            foreach (var hitCollider in foodColliders)
            {
                Vector3 foodPos;
                if (animal.CompareTag("Rabbit"))
                    foodPos = hitCollider.GetComponent<PlantController>().GetPlantPosition();
                else
                    foodPos = hitCollider.transform.position;
                
                Vector3 foodVec = foodPos - _rigidbody.position;
                if (foodVec.magnitude < minDist)
                {
                    if (CheckIfReachable(animal, foodVec))
                    {
                        minDist = foodVec.magnitude;
                        closestFoodPos = foodPos;
                        food = hitCollider.gameObject;
                    }
                }
            }

            if (minDist < animal.SenseRange)
            {
                goingToFood = true;
                Vector3 newDirection = CalculateMovingDirection(animal, closestFoodPos, minDist);
                animal.movementDirection = newDirection.normalized;

                if (animal.CompareTag("Fox"))
                {
                    if (minDist < 0.1f)
                    {
                        // eat the rabbit
                        animal.EatFood();
                        RabbitManager.Instance.RabbitDies(food, "fox");
                        animal.RabbitKilled(food);
                    }
                }
            }
            else
            {
                goingToFood = false;
                if (!animal.randomMovement)
                {
                    animal.randomMovement = true;
                    animal.StartCoroutine(animal.RandomMovement());
                }
            }
        }


        private Vector3 CalculateMovingDirection(AnimalStateManager animal, Vector3 closestFoodPos, double distance)
        {
            int k = -1;
            RaycastHit hit;
            Vector3 baseVector = closestFoodPos - _rigidbody.position;
            
            if (!Physics.Raycast(_rigidbody.position + Vector3.up * 2,
                baseVector, out hit, (float)distance, animal.waterMeshLayer))
            {
                return baseVector;
            }
            
            for (int angle = 15; angle <= 45;)
            {
                if (!Physics.Raycast(_rigidbody.position + Vector3.up * 2,
                    Quaternion.AngleAxis(angle * k, Vector3.up) * baseVector, out hit, (float)distance, animal.waterMeshLayer))
                {
                    return Quaternion.AngleAxis(angle, Vector3.up) * baseVector;
                }

                if (k == 1)
                    angle += 15;

                k *= -1;
            }
            
            return new Vector3(Random.Range(-1.0f, 1.0f), 0,Random.Range(-1.0f, 1.0f)).normalized;
        }

        private bool CheckIfReachable(AnimalStateManager animal, Vector3 foodVec)
        {
            RaycastHit hit;
            for (int angle = -45; angle <= 45; angle += 15)
            {
                if (!Physics.Raycast(_rigidbody.position - 0.1f * foodVec + Vector3.up * 2,
                    Quaternion.AngleAxis(angle, Vector3.up) * foodVec, out hit, foodVec.magnitude, animal.waterMeshLayer))
                {
                    return true;
                }
            }

            return false;
        }
    }    
}

