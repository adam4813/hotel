using UnityEngine;

[CreateAssetMenu(fileName = "StayInfo", menuName = "Stay Info")]
public class StayInfo :ScriptableObject
{
    public GuestInfo GuestInfo;
    public string Rate;
    public string Nights;
    public string TotalCost;
}