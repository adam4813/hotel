using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomStatusBadge : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color cleanColor;
    [SerializeField] private Color dirtyColor;

    public void SetStatus(RoomStatus status)
    {
        statusText.text = status == RoomStatus.Clean ? "Clean" : "Dirty";
        backgroundImage.color = status == RoomStatus.Clean ? cleanColor : dirtyColor;
    }
}