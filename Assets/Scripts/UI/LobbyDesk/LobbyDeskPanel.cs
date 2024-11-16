using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyDeskPanel : MonoBehaviour
{
    [SerializeField] private GuestInfoCard guestInfoCardPrefab;
    [SerializeField] private GameObject guestInfoPanel;
    [SerializeField] private List<StayInfo> stayInfos;
    [SerializeField] private Transform guestInfoCardContainer;
    
    void Start()
    {
        stayInfos.ForEach(stayInfo =>
        {
            var guestInfoCard = Instantiate(guestInfoCardPrefab, guestInfoCardContainer);
            guestInfoCard.SetStayInfo(stayInfo);
            guestInfoCard.Button.onClick.AddListener(() =>
            {
                guestInfoPanel.SetActive(true);
                guestInfoPanel.GetComponent<GuestInfoPanel>().SetStayInfo(stayInfo);
            });
        });
    }

    private void OnEnable()
    {
        ClearGuestInfoCards();
    }

    private void ClearGuestInfoCards()
    {
        foreach (Transform child in guestInfoCardContainer)
        {
            Destroy(child.gameObject);
        }
    }
}