using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Sim), typeof(NavMeshAgent))]
public class SimController : MonoBehaviour
{
    enum State 
    { 
        Idle, 
        Moving, 
        Performing
    }

    public float emergency = 15f;
    public float idleMinutes = 10f;

    public float arriveRadius = 1.5f;
    public float commitMinutes = 10f;

    public TMP_Text needsText;

    Sim sim;
    NavMeshAgent agent;

    State state;
    Choice currentChoice;
    float minutesInState;

    const float minutesPerSecond = 10f;

    private void Awake()
    {
        sim = GetComponent<Sim>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Pick();
    }

    // Update is called once per frame
    void Update()
    {
        float min = Time.deltaTime * minutesPerSecond;

        sim.Decay(min);
        minutesInState += min;

        switch (state)
        {
            case State.Idle:
                if (minutesInState >= idleMinutes) Pick();
                break;

            case State.Moving:
                if (Arrived()) Enter(State.Performing);
                break;

            case State.Performing:
                sim.Apply(currentChoice.action, min);
                bool done = minutesInState >= currentChoice.action.durationMinutes;
                bool interrupt = minutesInState >= commitMinutes && Interrupt();

                if (done || interrupt) Pick();

                break;
        }
        UpdateNeedsUI();
    }

    void Enter(State s)
    {
        state = s;
        minutesInState = 0;
    }

    bool Interrupt()
    {
        foreach (var need in sim.needs)
        {
            if (need.Value < emergency &&
                (currentChoice.action == null ||
                Array.TrueForAll(currentChoice.action.Ads, a => a.need != need.Key)))
                return true;
        }

        return false;
    }

    bool Arrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance <= agent.stoppingDistance) return true;

        return false;
    }

    private void Release()
    {
        if (currentChoice.smartObj != null && currentChoice.smartObj.occupant == sim)
            currentChoice.smartObj.occupant = null;

        currentChoice = default;
    }

    void Pick()
    {
        var next = sim.Choose();

        if (next.HasValue
            && next.Value.smartObj == currentChoice.smartObj
            && next.Value.action == currentChoice.action)
        {
            if (state == State.Performing
                && minutesInState >= currentChoice.action.durationMinutes)
                minutesInState = 0f;
            return;
        }

        Release();

        if (next == null)
        {
            Enter(State.Idle);
            return;
        }

        currentChoice = next.Value;
        currentChoice.smartObj.occupant = sim;

        agent.SetDestination(currentChoice.smartObj.interactionPoint.position);

        Enter(State.Moving);
    }

    public void UpdateNeedsUI()
    {
        if(needsText == null)
        {
            Debug.Log("no text :(");
            return;
        }

        string action = currentChoice.action == null ? (" ") : currentChoice.action.displayName;

        needsText.text = $"{this.name}: Action: {action} Needs: " + string.Join(" / ", sim.needTypes.Select(n => $"{n.name}: {sim.needs[n]:F0}"));
    }
}