using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScreenController : MonoBehaviour
{
    [SerializeField] Button nextLevel;
    [SerializeField] int levelNum = 0;
    private float speed = 2.0f;
    private Color tempImage;
    private Color tempText;
    private Text childText;
    // Start is called before the first frame update
    void Start()
    {
        nextLevel.gameObject.SetActive(true);
        childText = nextLevel.GetComponentInChildren<Text>();
        tempImage = nextLevel.image.color;
        tempText = childText.color;
    }

    private void Update()
    {
        tempImage.a = tempText.a =  (float)((Mathf.Sin(Time.time * speed) + 1.0) / 2.0);
        nextLevel.image.color = tempImage;
        childText.color = tempText;
       
    }

    public void NextLevel()
    {
        GameManager.instance.FadeToBlackAndLoad(.1f, levelNum);
    }


}
