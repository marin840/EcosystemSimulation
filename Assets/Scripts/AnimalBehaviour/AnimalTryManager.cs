using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace AnimalBehaviour
{
    public abstract class AnimalTryManager : MonoBehaviour
    {
        /*private Rigidbody _rigidbody;
        private Transform _transform;
        private Animator _animator;
        
        [Header("Animation curves")]
        [SerializeField] private AnimationCurve _hungerSpeedCurve;
        [SerializeField] private AnimationCurve _hungerSenseCurve, _hungerSizeCurve;
        public Vector3 movementDirection { get; set; }

        [Header("Layer masks")] 
        public LayerMask plantMask;
        public LayerMask borderLayer, decorationLayer, waterMeshLayer, rabbitLayer;
    
        [field: SerializeField] public float SenseRange { get; set; }
        [field: SerializeField] public float AnimalSpeed { get; set; }

        [Header("UI status")] 
        public Image hungerImage;
        public Image thirstImage, reproductionImage;
        public TextMeshProUGUI statusText;
        
        [Header("Rabbit parameters")]
        public float hunger;
        public float thirst, reproductionUrge, hungerIncSpeed, thirstIncSpeed, repUrgeIncSpeed, reduceThirstSpeed;

        [Header("Rabbit flags")] 
        public bool randomMovement;
        public bool isMale, isMating, isAlive = true, isUnderDeveloped;
        private bool isStopped, hasJumped, readyForMateSearch = true, readyToMate = true;

        [Header("Rabbit preferences")] public float speedPref;
        public float sensePref, sizePref;
        public int reproductionCooldownTime = 30, developmentTime = 15;
        
        [field: SerializeField] public AnimalAbstractState currentState { get; set; }

        public AnimalAbstractState FoodSearchState = new AnimalFoodSearchState();
        public AnimalAbstractState WaterSearchState = new AnimalFoodSearchState();
        public AnimalAbstractState MateSearchState = new AnimalMateSearchState();
        //public RabbitAbstractState FleeState = new RabbitFleeState();

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;
            _animator = GetComponent<Animator>();
        }
        
        // Start is called before the first frame update
        void Start()
        {
            hunger = Random.Range(0, 50) / 100f;
            thirst = Random.Range(0, 50) / 100f;
            reproductionUrge = Random.Range(0, 50) / 100f;
            if (isUnderDeveloped)
            {
                reproductionUrge = 0;
                StartCoroutine(DevelopRabbit());
            }

            //hungerIncSpeed = _hungerSpeedCurve.Evaluate(AnimalSpeed) + _hungerSenseCurve.Evaluate(SenseRange) +
            //                 _hungerSizeCurve.Evaluate(_transform.localScale.x);
            hungerIncSpeed = ((float)Math.Pow(AnimalSpeed * 10, 2) + 4 * SenseRange + 20 * _transform.localScale.x)/2000;
            thirstIncSpeed = hungerIncSpeed * 2;
            reduceThirstSpeed = thirstIncSpeed * -10;
            repUrgeIncSpeed = Random.Range(hungerIncSpeed, thirstIncSpeed);

            speedPref = AnimalSpeed + (Random.Range(-25, 25) / 100f) * AnimalSpeed;
            sensePref = SenseRange + (Random.Range(-25, 25) / 100f) * SenseRange;
            sizePref = isMale ? Random.Range(0.3f, 0.4f) : Random.Range(0.33f, 0.43f);
            
            
            currentState = WaterSearchState;
            currentState.EnterState(this);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!isAlive) return;
            if(isStopped)
                movementDirection = Vector3.zero;

            RaycastHit hit;
            if (Physics.SphereCast(_rigidbody.position - movementDirection * 0.1f, 0.05f, movementDirection, out hit, 0.4f, decorationLayer))
            {
                if (!hasJumped)
                {
                    hasJumped = true;
                    StartCoroutine(Jump());
                }
            }

            Vector3 newVelocity = movementDirection * AnimalSpeed;
            _rigidbody.velocity = new Vector3(newVelocity.x , _rigidbody.velocity.y, newVelocity.z);
            
            //currentState.UpdateState(this);
        }

        private IEnumerator Jump()
        {
            Vector3 movementDir2d = new Vector3(movementDirection.x, 0, movementDirection.z);
            Vector3 vec = ((Quaternion.AngleAxis(Random.Range(45, 315), Vector3.up) * movementDir2d).normalized * 2 + Vector3.up) * 3;
            _rigidbody.AddForce(vec, ForceMode.Impulse);
            yield return new WaitForSeconds(2);
            hasJumped = false;
        }
        
        public IEnumerator RandomMovement()
        {
            Vector3 waterPoint = ((WaterSearchState) WaterSearchState).CalculateWaterDirection(this);
            if ((_rigidbody.position - waterPoint).magnitude < 0.3f)
            {
                movementDirection = (Quaternion.AngleAxis(Random.Range(-45,45), Vector3.up) * (_rigidbody.position - waterPoint)).normalized;
            }
            else
            {
                Vector3 borderPoint = CalculateBorderDirection();
                if ((_rigidbody.position - borderPoint).magnitude < 0.3f)
                {
                    movementDirection = (Quaternion.AngleAxis(Random.Range(-45,45), Vector3.up) * (_rigidbody.position - borderPoint)).normalized;
                } else
                    movementDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0,Random.Range(-1.0f, 1.0f)).normalized;
            }

            movementDirection = new Vector3(movementDirection.x, 0, movementDirection.z).normalized;
            yield return new WaitForSeconds(Random.Range(2, 5));
            randomMovement = false;
        }

        public Vector3 CalculateBorderDirection()
        {
            Vector3 nearestVec = Vector3.zero;
            RaycastHit hit;
            float nearestPoint = float.MaxValue;

            for (int angle = 0; angle < 360; angle += 20)
            {
                if (Physics.Raycast(_rigidbody.position,
                    Quaternion.AngleAxis(angle, Vector3.up) * Vector3.left, out hit, SenseRange, borderLayer))
                {
                    if (hit.distance < nearestPoint)
                    {
                        nearestPoint = hit.distance;
                        nearestVec = hit.point;
                    }
                }
            }

            return nearestVec;
        }

        public void EatFood()
        {
            StartCoroutine(StopMoving());
            hunger = Mathf.Clamp(hunger - Random.Range(5, 11)/10.0f, 0, 1f);
        }
        
        private IEnumerator StopMoving()
        {
            isStopped = true;
            movementDirection = Vector3.zero;
            yield return new WaitForSeconds(2f);
            isStopped = false;
        }

        public bool CheckMatingResponse(AnimalStateManager animal)
        {
            float speedDif = Math.Abs(speedPref - rabbit.AnimalSpeed) / speedPref;
            float senseDif = Math.Abs(sensePref - rabbit.SenseRange) / sensePref;
            float sizeDif = Math.Abs(sizePref - rabbit._transform.localScale.x) / sizePref;

            float desirabilityCoef = 1 - speedDif - senseDif - sizeDif;
            if (Random.Range(0.0f, 1.0f) < desirabilityCoef * rabbit.reproductionUrge)
            {
                return true;
            }

            return false;
        }
        
        private void Update()
        {
            if (!isAlive) return;
            
            hunger = Mathf.Clamp(hunger + hungerIncSpeed * Time.deltaTime, 0, 1f);
            thirst = Mathf.Clamp(thirst + thirstIncSpeed * Time.deltaTime, 0, 1f);
            if(!isUnderDeveloped)
                reproductionUrge = Mathf.Clamp(reproductionUrge + repUrgeIncSpeed * Time.deltaTime, 0, 1f);

            hungerImage.fillAmount = Mathf.Clamp(hunger, 0, 1f);
            thirstImage.fillAmount = Mathf.Clamp(thirst, 0, 1f);
            reproductionImage.fillAmount = Mathf.Clamp(reproductionUrge, 0, 1f);

            if (hunger >= 1 || thirst >= 1)
            {
                string s = hunger >= 1 ? "hunger" : "thirst";
                _rigidbody.velocity = Vector3.zero;
                RabbitManager.Instance.RabbitDies(gameObject, s);
                _animator.Play("Rab_Death");
                isAlive = false;
                StartCoroutine(BodyDisappear());
            }

            if (isMating)
            {
                StartCoroutine(MatingCooldown());
                StartCoroutine(ReproductionCooldown());
            }
            // if (isMating) return;

            AnimalAbstractState priorityState = CalculatePriorityState();
            if (currentState == WaterSearchState && priorityState != WaterSearchState)
            {
                if(thirst < 0.1f)
                    SwitchState(priorityState);
            } else if (priorityState == WaterSearchState)
            {
                if (thirst - hunger > 0.2f || thirst > 0.7f)
                    SwitchState(WaterSearchState);
            } else if (currentState != priorityState)
            {
                SwitchState(priorityState);
            }
            else
            {
                SwitchState(FoodSearchState);
            }
        
            Debug.DrawRay(_transform.position, movementDirection, Color.red);
        }

        public void SwitchState(AnimalAbstractState state)
        {
            currentState = state;
            thirstIncSpeed = hungerIncSpeed * 2;
            //currentState.EnterState(this);
        }

        private AnimalAbstractState CalculatePriorityState()
        {
            // if predator nearby return flee state

            if (hunger > thirst && hunger > 0.7)
                return FoodSearchState;

            if (thirst > hunger && thirst > 0.7)
                return WaterSearchState;

            if (isMale && readyForMateSearch && readyToMate)
            {
                readyForMateSearch = false;
                StartCoroutine(MatingCooldown());
                return MateSearchState;
            }

            return hunger > thirst ? FoodSearchState : WaterSearchState;
        }

        private IEnumerator MatingCooldown()
        {
            yield return new WaitForSeconds(5);
            readyForMateSearch = true;
            isMating = false;
        }

        private IEnumerator ReproductionCooldown()
        {
            readyToMate = false;
            yield return new WaitForSeconds(reproductionCooldownTime);
            readyToMate = true;
        }
        
        private IEnumerator DevelopRabbit()
        {
            yield return new WaitForSeconds(developmentTime + Random.Range(-5, 6));
            isUnderDeveloped = false;
            _transform.localScale *= 2;
            AnimalSpeed *= 2;
            SenseRange *= 2;
            
            hungerIncSpeed *= 2;
            thirstIncSpeed = hungerIncSpeed * 2;
            reduceThirstSpeed = thirstIncSpeed * -10;

            speedPref *= 2;
            sensePref *= 2;
            sizePref *= 2;
            
            repUrgeIncSpeed = Random.Range(hungerIncSpeed, thirstIncSpeed);
        }

        private IEnumerator BodyDisappear()
        {
            yield return new WaitForSeconds(10);
            if (gameObject)
            {
                Destroy(gameObject);
            }
        }*/
    }
}
