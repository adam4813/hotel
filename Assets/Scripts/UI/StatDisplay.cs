using TMPro;
using UnityEngine;

public class StatDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text statValueText;
    public void SetStatValue(string value)
    {
        statValueText.text = value;
    }
}
