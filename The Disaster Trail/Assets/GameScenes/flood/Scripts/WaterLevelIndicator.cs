using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterLevelIndicator : MonoBehaviour
{
    [SerializeField] private FloodGameManager gameManager;
    [SerializeField] private Image waterLevelIndicator;
    [SerializeField] private float waterLevelIndicatorStartPosition = -128;
    [SerializeField] private float waterLevelIndicatorEndPosition = 0;
    [Header("Blinking")]
    [SerializeField] private bool enableBlinking = true;
    [SerializeField] private float blinkStartLevel = 0.75f;
    [SerializeField] private float blinkPeriod = 1.0f;
    [SerializeField] private Color blinkColor = Color.red;
    

    private float nextBlinkTime;
    private bool isRed = false;

    private Image bg;
    private Color oldBgColor;
    private Color oldFgColor;
    // Start is called before the first frame update
    void Start()
    {
        bg = GetComponent<Image>();
        oldBgColor = bg.color;
        oldFgColor = waterLevelIndicator.color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float waterLevel = gameManager.GetWaterLevel();

        waterLevelIndicator.rectTransform.anchoredPosition = new Vector2(
            waterLevelIndicator.rectTransform.anchoredPosition.x,
            Mathf.Lerp(
                waterLevelIndicatorStartPosition,
                waterLevelIndicatorEndPosition,
                waterLevel));


        while(enableBlinking && waterLevel > blinkStartLevel && Time.time > nextBlinkTime)
        {
            nextBlinkTime += blinkPeriod * 0.5f;
            isRed = !isRed;
            bg.color = isRed ? blinkColor : oldBgColor;
            waterLevelIndicator.color = isRed ? blinkColor : oldFgColor;
        }
    }
}
