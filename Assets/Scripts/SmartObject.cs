using UnityEngine;

public class SmartObject
{
    public string Name;
    public Advertisement[] Ads;
    public float Duration;

    public SmartObject(string name, float duration, params Advertisement[] ads)
    {
        Name = name;
        Ads = ads;
        Duration = duration;
    }
}
