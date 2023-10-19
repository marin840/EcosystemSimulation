using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AnimalBehaviour
{
    public class MateSearchState : AnimalAbstractState
    {
        private Rigidbody _rigidbody;
        private int maxNumberOfOffsprings = 10;
        
        public override void EnterState(AnimalStateManager animal)
        {
            _rigidbody = animal.GetComponent<Rigidbody>();
            animal.statusText.text = "Searching for a mate";
            Collider[] otherFriendsColliders = Physics.OverlapSphere(_rigidbody.position, animal.SenseRange, animal.friendsLayer);
            RaycastHit hit;
            
            foreach (var otherFriendCollider in otherFriendsColliders) 
            {
                    AnimalStateManager otherAnimal = otherFriendCollider.GetComponent<AnimalStateManager>();
                    Vector3 otherAnimalVec = otherAnimal.transform.position - _rigidbody.position;
                    if (Physics.Raycast(_rigidbody.position + Vector3.up * 2,
                        otherAnimalVec, out hit, otherAnimalVec.magnitude,
                        animal.waterMeshLayer))
                    {
                        continue;
                    }
                    if (!otherAnimal.isMale && otherAnimal.isAlive)
                    {
                        float speedDif = Math.Abs(animal.speedPref - otherAnimal.AnimalSpeed) / animal.speedPref;
                        float senseDif = Math.Abs(animal.sensePref - otherAnimal.SenseRange) / animal.sensePref;
                        float sizeDif = Math.Abs(animal.sizePref - otherAnimal.GetComponent<Transform>().localScale.x) / animal.sizePref;

                        float desirabilityCoef = 1 - speedDif - senseDif - sizeDif;
                        
                        if (Random.Range(0.0f, 1.0f) < desirabilityCoef * animal.reproductionUrge)
                        {
                            if (otherAnimal.CheckMatingResponse(animal))
                            {
                                // mate
                                Debug.Log("mating");
                                animal.statusText.text = otherAnimal.statusText.text = "mating";
                                animal.reproductionUrge = otherAnimal.reproductionUrge = 0;
                                animal.repUrgeIncSpeed /= 2;
                                otherAnimal.repUrgeIncSpeed /= 2;
                                animal.isMating = otherAnimal.isMating = true;

                                animal.movementDirection = (otherAnimal.transform.position - _rigidbody.position).normalized;
                                otherAnimal.movementDirection = animal.movementDirection * -1;

                                _rigidbody.AddForce(animal.movementDirection, ForceMode.Impulse);
                                MakeOffsprings(animal, otherAnimal);
                                
                                return;
                            }
                            else
                            {
                                Debug.Log("not impressed");
                            }
                        }

                    }
            }
        }

        private void MakeOffsprings(AnimalStateManager maleAnimal, AnimalStateManager femaleAnimal)
        {
            int numberOfOffsprings = Random.Range(1, maxNumberOfOffsprings + 1);
            if (maleAnimal.CompareTag("Rabbit"))
                numberOfOffsprings = Random.Range(1, maxNumberOfOffsprings - 2);
            float size, speed, senseRange;

            for (int i = 0; i < numberOfOffsprings; i++)
            {
                Transform femaleTransform = femaleAnimal.transform;
                bool isMale = Random.Range(0.0f, 1.0f) < 0.5;
                float randPoint = Random.Range(0.0f, 1.0f);
                size = randPoint * maleAnimal.transform.localScale.x +
                       (1 - randPoint) * femaleTransform.localScale.x;
                speed = randPoint * maleAnimal.AnimalSpeed + (1 - randPoint) * femaleAnimal.AnimalSpeed;
                senseRange = randPoint * maleAnimal.SenseRange + (1 - randPoint) * femaleAnimal.SenseRange;

                size *= Random.Range(0.9f, 1.1f);
                speed *= Random.Range(0.9f, 1.1f);
                senseRange *= Random.Range(0.9f, 1.1f);
                
                if(maleAnimal.CompareTag("Rabbit"))
                    RabbitManager.Instance.CreateNewRabbit(isMale, speed, senseRange, size, femaleTransform.position);
                else 
                    FoxManager.Instance.CreateNewFox(isMale, speed, senseRange, size, femaleTransform.position);
            }
        }

        public override void UpdateState(AnimalStateManager animal)
        {
        }
    }

}