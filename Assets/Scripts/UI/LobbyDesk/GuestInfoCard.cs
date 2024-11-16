using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GuestInfoCard : MonoBehaviour
{
    [SerializeField] private TMP_Text guestNameText;
    [SerializeField] private TMP_Text nightCountText;
    [SerializeField] private Image guestImage;
    public Button Button { get; private set; }

    private void Awake()
    {
        Button = GetComponent<Button>();
    }

    public void SetStayInfo(StayInfo stayInfo)
    {
        guestNameText.text = stayInfo.GuestInfo.GuestName;
        nightCountText.text = stayInfo.Nights;
        guestImage.sprite = stayInfo.GuestInfo.GuestImage;
    }
}
