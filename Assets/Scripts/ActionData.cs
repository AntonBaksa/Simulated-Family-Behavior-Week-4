using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Need", menuName = "Sims/Action")]
public class ActionData : ScriptableObject
{
    public string displayName;
    public float durationMinutes = 20f;
    public Advertisement[] Ads;
}

[Serializable]
public class Advertisement
{
    public Needs need;
    public float gain;
}