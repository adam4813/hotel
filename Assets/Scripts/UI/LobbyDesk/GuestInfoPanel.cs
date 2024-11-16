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

    private void Awake()
    {
        checkInButton.onClick.AddListener(() => gameObject.SetActive(false));
        gameObject.SetActive(false);
    }

    public void SetStayInfo(StayInfo stayInfo)
    {
        guestNameText.text = stayInfo.GuestInfo.GuestName;
        rateStatDisplay.SetStatValue(stayInfo.Rate);
        nightStatDisplay.SetStatValue(stayInfo.Nights);
        totalCostStatDisplay.SetStatValue(stayInfo.TotalCost);
        gameObject.SetActive(true);
    }
}