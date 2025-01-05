using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuestInfoPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text guestNameText;
    [SerializeField] private StatDisplay rateStatDisplay;
    [SerializeField] private StatDisplay nightStatDisplay;
    [SerializeField] private StatDisplay totalCostStatDisplay;
    [SerializeField] private Button checkInButton;
    public Button CheckInButton => checkInButton;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetGuest(GoapAgent guest)
    {
        var stayInfo = guest.StayInfo;
        guestNameText.text = stayInfo.GuestInfo.GuestName;
        rateStatDisplay.SetStatValue(stayInfo.Rate);
        nightStatDisplay.SetStatValue(stayInfo.Nights);
        totalCostStatDisplay.SetStatValue(stayInfo.TotalCost);
        gameObject.SetActive(true);
    }
}