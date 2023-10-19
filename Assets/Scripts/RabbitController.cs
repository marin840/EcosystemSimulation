using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RabbitController : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody _rigidbody;
    private Transform _transform;
    
    private Vector3 movementDirection;

    public LayerMask plantMask, borderLayer, decorationLayer, waterMeshLayer;
    
    [field: SerializeField] private float SenseRange { get; set; } = 3.0f;
    [field: SerializeField] private float RabbitSpeed { get; set; } = 0.3f;

    [SerializeField] private bool goingToFood, isThirsty, goingToWater;
    private bool randomMovement, isEating, isDrinking;

    public Image hungerImage, thirstImage, reproductionImage;
    private float hunger, thirst, reproductionUrge;
    private float hungerIncSpeed, thirstIncSpeed, repUrgeIncSpeed, reduceThirstSpeed;
    
    Vector3 nearestWaterPoint = Vector3.zero;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _transform = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        //animator = GetComponent<Animator>();
        //animator.Play("Rab_Jump_InPlace");
        //animator.SetInteger("IdleIndex", Random.Range(0, 4));

        hunger = Random.Range(0, 40) / 100f;
        thirst = Random.Range(0, 40) / 100f;
        reproductionUrge = Random.Range(0, 20) / 100f;

        hungerIncSpeed = (float)Math.Pow(RabbitSpeed * 10, 2) + 4 * SenseRange;
        thirstIncSpeed = hungerIncSpeed * 2;
        reduceThirstSpeed = thirstIncSpeed * -10;

        StartCoroutine(RandomMovement());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //_rigidbody.position += movementDirection * 0.5f * Time.fixedDeltaTime;
        Vector3 newVelocity = movementDirection * RabbitSpeed;
        _rigidbody.velocity = new Vector3(newVelocity.x , _rigidbody.velocity.y, newVelocity.z);

        SenseEnvironment(_rigidbody.position, SenseRange);
    }

    private IEnumerator RandomMovement()
    {
        randomMovement = true;
        movementDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0,Random.Range(-1.0f, 1.0f)).normalized;
        yield return new WaitForSeconds(Random.Range(3, 6));
        randomMovement = false;
    }

    void SenseEnvironment(Vector3 center, float radius)
    {
        if (isEating || isDrinking) return;

        if (isThirsty)
        {
            if (!goingToWater)
            {
                nearestWaterPoint = CalculateWaterDirection();
                if (nearestWaterPoint == Vector3.zero)
                {
                    randomMovement = true;
                    StartCoroutine(RandomMovement());
                    return;
                }
                nearestWaterPoint.y = 0.35f;
            }
            //Debug.Log(nearestWaterPoint);
            
            goingToWater = true;
            movementDirection = (nearestWaterPoint - _transform.position).normalized;
            if ((_transform.position - nearestWaterPoint).magnitude < 0.2f)
            {
                thirstIncSpeed = reduceThirstSpeed ;
                StartCoroutine(DrinkWater());
            }

            return;
        }
        
        Collider[] foodColliders = Physics.OverlapSphere(center, radius, plantMask);
        
        double minDist = Double.MaxValue;
        Vector3 closestFoodVec = Vector3.zero;
        foreach (var hitCollider in foodColliders)
        {
            Vector3 foodVec = hitCollider.GetComponent<PlantController>().GetPlantPosition();
            double dist = (foodVec - _rigidbody.position).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closestFoodVec = foodVec;
            }
        }

        if (minDist < Double.MaxValue)
        {
            goingToFood = true;
            movementDirection = (closestFoodVec - _transform.position).normalized;
        }
        else
        {
            goingToFood = false;
            if (!randomMovement)
            {
                randomMovement = true;
                StartCoroutine(RandomMovement());
            }
        }
    }

    private Vector3 CalculateWaterDirection()
    {
        Vector3 nearestVec = Vector3.zero;
        RaycastHit hit;
        float nearestPoint = float.MaxValue;

        for (int angle = 0; angle < 360; angle += 45)
        {
            if (Physics.Raycast(_rigidbody.position + Vector3.up * 2,
                Quaternion.AngleAxis(angle, Vector3.up) * Vector3.left, out hit, SenseRange, waterMeshLayer))
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
        isEating = true;
        hunger = Mathf.Clamp(hunger - 0.5f, 0, 1f);
        StartCoroutine(StopMoving());
    }

    private IEnumerator DrinkWater()
    {
        isDrinking = true;
        movementDirection = Vector3.zero;
        yield return new WaitForSeconds(2f);
        isDrinking = false;
        goingToWater = false;
        if (thirst < 0.2)
            isThirsty = false;
        thirstIncSpeed = hungerIncSpeed * 2;
    }
    private IEnumerator StopMoving()
    {
        movementDirection = Vector3.zero;
        yield return new WaitForSeconds(1.5f);
        isEating = false;
    }

    private void Update()
    {
        hunger += hungerIncSpeed / 100000;
        thirst += thirstIncSpeed / 100000;

        if (thirst > 0.5)
        {
            isThirsty = true;
        }
        
        hungerImage.fillAmount = Mathf.Clamp(hunger, 0, 1f);
        thirstImage.fillAmount = Mathf.Clamp(thirst, 0, 1f);
        reproductionImage.fillAmount = Mathf.Clamp(reproductionUrge, 0, 1f);
        
        Debug.DrawRay(_transform.position, movementDirection, Color.red);
    }
    
}
