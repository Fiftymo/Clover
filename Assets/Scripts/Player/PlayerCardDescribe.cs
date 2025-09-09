using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardDescribe : MonoBehaviour
{
    [Header("ÃèÊö¿ò×é¼þ")]
    public Image describeImage;
    public TextMeshProUGUI describeText;

    public void SetDescribe(CardData card)
    {
        describeImage.sprite = card.GetSprite();
        describeText.text = card.description;
    }
}
