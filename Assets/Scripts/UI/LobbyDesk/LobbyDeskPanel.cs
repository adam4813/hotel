using System.Collections.Generic;
using UnityEngine;

public class LobbyDeskPanel : MonoBehaviour
{
    public delegate void GuestCheckedIn(GoapAgent guest);

    public static event GuestCheckedIn OnGuestCheckedIn;

    [SerializeField] private GuestInfoCard guestInfoCardPrefab;
    [SerializeField] private GuestInfoPanel guestInfoPanel;
    [SerializeField] private List<GoapAgent> guests;
    [SerializeField] private Transform guestInfoCardContainer;

    private GuestInfoCard selectedGuestInfoCard;

    private void Awake()
    {
        gameObject.SetActive(false);
        guestInfoPanel.CheckInButton.onClick.AddListener(OnClickCheckIn);
    }

    private void OnEnable()
    {
        foreach (var guest in guests)
        {
            InstantiateGuestInfoCard(guest);
        }
    }

    private void OnDisable()
    {
        DestroySelectedGuestInfoCard();
        ClearGuestInfoCards();
        guests.Clear();
    }

    private void InstantiateGuestInfoCard(GoapAgent guest)
    {
        var guestInfoCard = Instantiate(guestInfoCardPrefab, guestInfoCardContainer);
        guestInfoCard.SetGuest(guest);
        guestInfoCard.Button.onClick.AddListener(() =>
        {
            selectedGuestInfoCard = guestInfoCard;
            guestInfoPanel.SetGuest(guest);
            guestInfoPanel.gameObject.SetActive(true);
        });
    }

    private void OnClickCheckIn()
    {
        if (selectedGuestInfoCard == null) return;

        OnGuestCheckedIn?.Invoke(selectedGuestInfoCard.Guest);
        RemoveGuest(selectedGuestInfoCard.Guest);
        DestroySelectedGuestInfoCard();
    }

    private void ClearGuestInfoCards()
    {
        foreach (Transform child in guestInfoCardContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddGuest(GoapAgent guest)
    {
        guests.Add(guest);
    }

    private void RemoveGuest(GoapAgent guest)
    {
        guests.Remove(guest);
        if (selectedGuestInfoCard.Guest == guest)
        {
            DestroySelectedGuestInfoCard();
        }

        if (guests.Count == 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void DestroySelectedGuestInfoCard()
    {
        if (selectedGuestInfoCard == null) return;
        Destroy(selectedGuestInfoCard.gameObject);
        selectedGuestInfoCard = null;
        guestInfoPanel.gameObject.SetActive(false);
    }
}