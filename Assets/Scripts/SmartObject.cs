using UnityEngine;

public class SmartObject : MonoBehaviour
{
    public ActionData[] actions;
    public Transform interactionPoint;

    public Sim occupant;

    public bool IsFree => occupant == null;
}
