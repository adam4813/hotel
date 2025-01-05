using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GuestInfoCard : MonoBehaviour
{
    [SerializeField] private TMP_Text guestNameText;
    [SerializeField] private TMP_Text nightCountText;
    [SerializeField] private Image guestImage;
    public GoapAgent Guest { get; private set; }
    public Button Button { get; private set; }

    private void Awake()
    {
        Button = GetComponent<Button>();
    }

    public void SetGuest(GoapAgent guest)
    {
        var stayInfo = guest.StayInfo;
        guestNameText.text = stayInfo.GuestInfo.GuestName;
        nightCountText.text = stayInfo.Nights;
        guestImage.sprite = stayInfo.GuestInfo.GuestImage;
        Guest = guest;
    }
}
