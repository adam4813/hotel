﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private MouseDragPrefab mouseDragPrefab;
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private List<ItemScriptableObject> items;
    [SerializeField] private List<HotelRoom> hotelRooms;

    public List<GoapAgent> Agents { get; } = new();
    public List<ItemScriptableObject> Items => items;
    public List<HotelRoom> Rooms => hotelRooms;

    public MouseDragPrefab MouseDragPrefab => mouseDragPrefab;
    public GameObject ActivePrefab => roomPrefab;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            MouseDragPrefab.SetPrefab(roomPrefab);
        }
    }

    public void AddAgent(GoapAgent agent)
    {
        Agents.Add(agent);
    }
}