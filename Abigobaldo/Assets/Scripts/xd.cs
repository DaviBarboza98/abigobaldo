
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class xd : MonoBehaviour
{
    public Slider slider;
    public float maxTime = 4f;
    public UnityEvent onComplete;

    private float currentTime = 0f;
    public bool isInside = false;

    void Update()
    {
        if (isInside && Input.GetKey(KeyCode.E))
        {
            currentTime += Time.deltaTime;
            slider.value = currentTime / maxTime;

            if (currentTime >= maxTime)
            {
                Activate();
            }
        }
        else if(Input.GetKeyUp(KeyCode.E))
        {
            ResetSlider();
        }
    }

    void Activate()
    {
        
        onComplete.Invoke();
        ResetSlider();
    }

    void ResetSlider()
    {
        currentTime = 0f;
        slider.value = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            isInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInside = false;
            ResetSlider();
        }
    }
}
