using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileJoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Image bgImg;
    [SerializeField] private Image joystickImg;
    private Vector3 inputVector;
    [Tooltip("How far the joystick has to move before it starts taking input.")]
    [SerializeField] private float deadZone = 0.5f;

    private Vector2 originalPosition;

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
            inputVector = new Vector3(pos.x * 2 - 1 + 2 * bgImg.rectTransform.pivot.x, 0, pos.y * 2 - 1 +  2 * bgImg.rectTransform.pivot.y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Move Joystick image
            float sizeX = bgImg.rectTransform.sizeDelta.x / 2;
            float sizeY = bgImg.rectTransform.sizeDelta.y / 2;
            joystickImg.rectTransform.anchoredPosition = new Vector3(inputVector.x * sizeX, inputVector.z * sizeY);
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        Vector2 pos;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, ped.position, ped.pressEventCamera, out pos))
        {
            originalPosition = bgImg.rectTransform.anchoredPosition;

            pos.x /= bgImg.rectTransform.sizeDelta.x;
            pos.y /= bgImg.rectTransform.sizeDelta.y;
            bgImg.rectTransform.anchoredPosition +=
                new Vector2(
                    pos.x * 2 - 1 + 2 * bgImg.rectTransform.pivot.x,
                    pos.y * 2 - 1 + 2 * bgImg.rectTransform.pivot.y
                ) * bgImg.rectTransform.sizeDelta / 4;
        }
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        // Move joystick image to center when user stops touching
        inputVector = Vector3.zero;
        joystickImg.rectTransform.anchoredPosition = Vector3.zero;
        bgImg.rectTransform.anchoredPosition = originalPosition;
    }

    public bool IsInDeadZone() {
        return inputVector.x * inputVector.x + inputVector.z * inputVector.z < deadZone * deadZone;
    }

    public float Horizontal()
    {
        return IsInDeadZone() ? 0 : inputVector.x;
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

    public float Vertical()
    {
        return IsInDeadZone() ? 0 : inputVector.z;
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