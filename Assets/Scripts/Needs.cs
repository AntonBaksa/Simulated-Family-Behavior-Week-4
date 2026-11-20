using UnityEngine;

[CreateAssetMenu(fileName = "Need", menuName = "Sims/Need")]
public class Needs : ScriptableObject
{
    public float startValue = 10f;
    public float decayPerMinute = 0.15f;
    public UrgencyCurve urgencyCurve = UrgencyCurve.Linear;

    public float Urgency(float value)
    {
        float lack = 1f - Mathf.Clamp01(value / 100f);

        switch (urgencyCurve)
        {
            case UrgencyCurve.Linear:
                return lack;
            case UrgencyCurve.Quadratic:
                return lack * lack;
            default:
                return lack;
        }
    }
}

public enum UrgencyCurve 
{ 
    Linear, 
    Quadratic
}