using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomManagementPanel : MonoBehaviour
{
    [SerializeField] private RoomStatusBadge roomStatusBadge;
    [SerializeField] private Button roomCleaningButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text roomNumberText;
    private HotelRoom hotelRoom;

    private void Awake()
    {
        gameObject.SetActive(false);
        roomCleaningButton.onClick.AddListener(OnRoomCleaningButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnRoomCleaningButtonClicked()
    {
        roomStatusBadge.SetStatus(RoomStatus.Clean);
        hotelRoom.SetRoomStatus(RoomStatus.Clean);
    }

    private void OnEnable()
    {
        roomNumberText.text = hotelRoom.RoomNumber;
        roomStatusBadge.SetStatus(hotelRoom.RoomStatus);
        closeButton.gameObject.SetActive(hotelRoom.RoomStatus == RoomStatus.Dirty);
    }

    private void OnDisable()
    {
        hotelRoom = null;
    }

    public void SetHotelRoom(HotelRoom room)
    {
        hotelRoom = room;
        gameObject.SetActive(true);
    }
}