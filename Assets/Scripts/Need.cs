using UnityEngine;

public class Need
{
    public float Value;
    public float Decay;
    public UrgencyCurve UrgencyCurve;

    public Need(float value, float decay, UrgencyCurve urgencyCurve)
    {
        Value = value;
        Decay = decay;
        UrgencyCurve = urgencyCurve;
        
    }
}
