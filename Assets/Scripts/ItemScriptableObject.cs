using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item", order = 1)]
public class ItemScriptableObject: ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
}