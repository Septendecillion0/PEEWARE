using UnityEngine;

public class PeeMeterManager : Singleton<PeeMeterManager>
{
    public int maxPee { get; private set; } = 100;
    public float currentPee { get; private set; } = 0f;
    public float deltaPee = 0f; // Rate of pee increase per second

    // Event for UI updates
    public delegate void PeeValueChanged(float value);
    public event PeeValueChanged OnPeeValueChanged;

    private void Start()
    {
        currentPee = 0f;
        OnPeeValueChanged?.Invoke(currentPee);
    }

    private void Update()
    {
        currentPee += deltaPee * Time.deltaTime;
        ClampPee();
        OnPeeValueChanged?.Invoke(currentPee);
    }

    // Sudden increase due to scare
    public void Scare(float amount)
    {
        currentPee += amount;
        ClampPee();
        OnPeeValueChanged?.Invoke(currentPee);
    }

    // Decrease pee meter
    public void Pee(float amount)
    {
        currentPee -= amount;
        ClampPee();
        OnPeeValueChanged?.Invoke(currentPee);
    }

    private void ClampPee()
    {
        currentPee = Mathf.Clamp(currentPee, 0f, maxPee);
    }

    // For checking if full
    public bool IsFull()
    {
        return Mathf.Approximately(currentPee, maxPee);
    }
}