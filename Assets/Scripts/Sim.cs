using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Sim : MonoBehaviour
{
    public int inAct;
    const float Emergency = 15f;
    const float MinAcceptableScore = 5f; //The threshold for an interaction to be relevant
    const float IdleMinutes = 10f;
    private const int SimulationTime = 24 * 60 * 60;

    //Hard-coded needs list
    private Dictionary<string, Need> Needs = new Dictionary<string, Need>
    {
        { "Hunger", new Need(60f, 0.15f, UrgencyCurve.Quadratic) },
        { "Energy", new Need(60f, 0.15f, UrgencyCurve.Quadratic) },
        { "Bladder", new Need(60f, 0.15f, UrgencyCurve.Quadratic) },
        { "Fun", new Need(60f, 0.15f, UrgencyCurve.Linear) },
        { "Social", new Need(60f, 0.15f, UrgencyCurve.Linear) }
    };

    //Hard-coded world settings
    //Every object only has one interaction with a duration and advertised changes in needs
    static readonly SmartObject[] Objects =
    {
        new SmartObject("Fridge",   20,  new Advertisement("Hunger", 40f)),
        new SmartObject("Bed",      240, new Advertisement("Energy", 50f)),
        new SmartObject("Toilet",   5,   new Advertisement("Bladder", 60f)),
        new SmartObject("TV",       45,  new Advertisement("Fun", 30f)),
        new SmartObject("Computer", 60,  new Advertisement("Fun", 40f), new Advertisement("Energy", -10f)),
        new SmartObject("Phone",    20,  new Advertisement("Social", 25f), new Advertisement("Fun", 10f))
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        Sim sim = new Sim();
        SmartObject current = Choose(sim);

        Debug.Log("***NEW ACT: {0}" + current != null ? current.Name : "Idle");


        for (int t = 0; t < SimulationTime; t++)
        {
            if (Interrupt(sim, current) || Done(current, sim.inAct))
            {
                SmartObject next = Choose(sim);

                if (next != current)
                {
                    current = next;
                    sim.inAct = 0;

                    Debug.Log("***NEW ACT: {0}" + current != null ? current.Name : "Idle");

                    yield return new WaitForSeconds(2.0f);
                }
            }

            Debug.Log($"Hunger: {0} / Energy: {1} / Bladder {2} / Fun: {3} / Social: {4}" + 
                                                                sim.Needs["Hunger"].Value + 
                                                                sim.Needs["Energy"].Value + 
                                                                sim.Needs["Bladder"].Value + 
                                                                sim.Needs["Fun"].Value + 
                                                                sim.Needs["Social"].Value);

            yield return new WaitForSeconds(0.2f);

            sim.Tick(current);
            sim.inAct++;
        }
    }

    void Tick(SmartObject smartObj)
    {
        foreach (var n in Needs.Values) n.Value -= n.Decay;

        if (smartObj != null)
        {
            foreach (var ad in smartObj.Ads)
                Needs[ad.Need].Value += ad.Gain / smartObj.Duration;
        }

        foreach (var n in Needs.Values) n.Value = Mathf.Clamp(n.Value, 0f, 100f);
    }

    bool Done(SmartObject obj, int minutesInAct)
    {
        return minutesInAct >= (obj?.Duration ?? IdleMinutes);
    }

    bool Interrupt(Sim sim, SmartObject current)
    {
        foreach (var need in sim.Needs)
        {
            if ((need.Value.Value < Emergency) &&
                (current == null || current.Ads.All(a => a.Need != need.Key)))
                return true;
        }

        return false;
    }

    SmartObject Choose(Sim sim)
    {
        SmartObject best = null;
        float bestScore = MinAcceptableScore;

        foreach (var obj in Objects)
        {
            float score = Score(sim, obj);
            if (score > bestScore) { bestScore = score; best = obj; }
        }
        return best;
    }

    private float Score(Sim sim, SmartObject obj)
    {
        float score = 0f;

        foreach (var ad in obj.Ads)
        {
            var need = sim.Needs[ad.Need];
            float gain = ad.Gain > 0f ? Mathf.Min(ad.Gain, 100f - need.Value) : ad.Gain;

            score += Urgency(need.Value, need.UrgencyCurve) * gain;
        }

        return score;
    }

    float Urgency(float need, UrgencyCurve curve)
    {
        float lack = Mathf.Clamp(1 - need / 100f, 0f, 1f);

        switch (curve)
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
