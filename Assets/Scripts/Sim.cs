using System.Collections.Generic;
using UnityEngine;

public class Sim : MonoBehaviour
{
    public float minAcceptableScore = 5f; //The threshold for an interaction to be relevant
    public float distanceFalloff = 0.1f;

    public Needs[] needTypes;

    public Dictionary<Needs, float> needs = new Dictionary<Needs, float>();

    void Awake()
    {
        foreach (Needs needType in needTypes)
        {
            needs[needType] = needType.startValue;
        }
        ;
    }

    public void Decay(float minutes)
    {
        foreach (Needs need in needTypes)
        {
            this.needs[need] = Mathf.Clamp(this.needs[need] - need.decayPerMinute * minutes, 0.0f, 100.0f);
        }
    }

    public void Apply(ActionData action, float minutes)
    {
        foreach (var ad in action.Ads)
        {
            needs[ad.need] = Mathf.Clamp(needs[ad.need] + ad.gain / action.durationMinutes * minutes, 0f, 100f);
        }
    }

    public float ScoreAction(ActionData action)
    {
        float score = 0;

        foreach (var ad in action.Ads)
        {
            float value = needs[ad.need];
            float gain = ad.gain > 0 ? Mathf.Min(ad.gain, 100f - value) : ad.gain;

            score += ad.need.Urgency(value) * gain;
        }

        return score;
    }

    public Choice? Choose()
    {
        Choice? best = null;
        float bestScore = minAcceptableScore;

        var smartObjects = FindObjectsByType<SmartObject>(FindObjectsSortMode.None);

        foreach (SmartObject sObj in smartObjects)
        {
            if (sObj.gameObject == gameObject || !sObj.IsFree) continue;


            float dist = Vector3.Distance(transform.position, sObj.interactionPoint.position);

            foreach (var action in sObj.actions)
            {
                float score = ScoreAction(action) / (1f + dist * distanceFalloff);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = new Choice { smartObj = sObj, action = action, score = score };
                }
            }
        }
        return best;
    }
}

public struct Choice
{
    public SmartObject smartObj;
    public ActionData action;
    public float score;
}