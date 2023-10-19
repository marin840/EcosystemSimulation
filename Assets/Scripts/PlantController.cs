using System;
using System.Collections;
using System.Collections.Generic;
using AnimalBehaviour;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    private Animator _animator;
    private Collider _collider;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        AnimalStateManager rabbitManager = other.GetComponent<AnimalStateManager>();
        if (rabbitManager != null && other.CompareTag("Rabbit"))
        {
            if (rabbitManager.currentState == rabbitManager.FoodSearchState)
            {
                Animator rabbitAnimator = other.GetComponent<Animator>();
                rabbitAnimator.Play("Rab_Eat");
                
                _animator.SetTrigger("Eat");
                _collider.enabled = false;
                rabbitManager.EatFood();
            }
        }
    }

    void DestroyPlant()
    {
        PlantsManager.Instance.EatPlant(gameObject);
        Destroy(gameObject);
    }

    public Vector3 GetPlantPosition()
    {
        return transform.position;
    }
}
