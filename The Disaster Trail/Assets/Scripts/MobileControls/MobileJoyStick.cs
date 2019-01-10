using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileJoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    static Image bgImg;
    static Image joystickImg;
    static Vector3 inputVector;

    void Start()
    {
        // Get background image
        bgImg = GetComponent<Image>();
        // Get the joy stick image from the background's child
        joystickImg = transform.GetChild(0).GetComponent<Image>();
    }

    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, ped.position, ped.pressEventCamera, out pos))
        {
            pos.x = (pos.x / bgImg.rectTransform.sizeDelta.x);
            pos.y = (pos.y / bgImg.rectTransform.sizeDelta.y);

            // Makes center of joystick image pos.x and pos.y = 0
            inputVector = new Vector3(pos.x * 2 + 1, 0, pos.y * 2 - 1);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Move Joystick image
            float sizeX = bgImg.rectTransform.sizeDelta.x / 2;
            float sizeY = bgImg.rectTransform.sizeDelta.y / 2;
            joystickImg.rectTransform.anchoredPosition = new Vector3(inputVector.x * sizeX, inputVector.z * sizeY);
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        // Move joystick image to center when user stops touching
        inputVector = Vector3.zero;
        joystickImg.rectTransform.anchoredPosition = Vector3.zero;
    }

    static public float Horizontal()
    {
        return inputVector.x;
        /*
        if (inputVector.x != 0)
        {
            return inputVector.x;
        }
        else
        {
            return Input.GetAxis("Horizontal");
        }
        */
    }

    static public float Vertical()
    {
        return inputVector.z;
        /*
        if (inputVector.z != 0)
        {
            return inputVector.z;
        }
        else
        {
            return Input.GetAxis("Vertical");
        }
        */
    }
}