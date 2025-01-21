using UnityEngine;

public enum RoomStatus
{
    Clean,
    Dirty
}

public class HotelRoom : MonoBehaviour
{
    [SerializeField] private string roomNumber;
    [SerializeField] private RoomManagementPanel roomManagementPanel;
    [SerializeField] private Bed bed;

    public string RoomNumber => roomNumber;
    public RoomStatus RoomStatus { get; private set; } = RoomStatus.Dirty;

    public void OnMouseDown()
    {
        roomManagementPanel.SetHotelRoom(this);
    }

    public void SetRoomStatus(RoomStatus status)
    {
        RoomStatus = status;
    }
}