using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vexe.Runtime.Types;

/// <summary>
/// what kind of information is associated with the button
/// </summary>
public enum MenuButtonType
{
    text
}

/// <summary>
/// a versatile menu button that sends a message back to its parent when clicked or hovered over
/// </summary>
public class MenuButtonScript : BaseBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public Text           buttonText; //text of this button
    public MenuButtonType buttonType; //enum that represents how this menu button is being used
    public Image          buttonImage;//image of this button

    public AudioClip[] clickSounds; //one of these is played at random when the button is clicked

    /// <summary>
    /// sets up the button by setting the text directly (note: only use on text buttons, as the other types set the text automatically)
    /// </summary>
    public void setButtonText(string text)
    {
        buttonText.text = text;
        buttonType = MenuButtonType.text;
    }

    /// <summary>
    /// sets the color for this button
    /// </summary>
    public void setColor(Color c)
    {
        if (buttonImage.color != c)
            buttonImage.color = c;
    }

    /// <summary>
    /// reports back to the parent object in a slightly different way for each button type
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        //play the random sound with the audio source attached to the main camera since we dont want UI sounds to overlap and this button may cease to exist before the sound is done
        int soundToPlay = Random.Range(0, clickSounds.Length);
        Camera.main.GetComponent<AudioSource>().clip = clickSounds[soundToPlay];
        Camera.main.GetComponent<AudioSource>().volume = MessageHandlerScript.instance.SFXVolumeSetting;
        Camera.main.GetComponent<AudioSource>().Play();

        switch (buttonType)
        {
            case MenuButtonType.text:
                SendMessageUpwards("TextButtonSelected", buttonText.text, SendMessageOptions.DontRequireReceiver);
                break;
            default:
                Debug.LogError("MenuButtonScript cant handle this button type!");
                break;
        }
    }

    /// <summary>
    /// reports back to the parent object in a slightly different way for each button type
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (buttonType)
        {
            case MenuButtonType.text:
                SendMessageUpwards("TextButtonHovered", buttonText.text, SendMessageOptions.DontRequireReceiver);
                break;
            default:
                Debug.LogError("MenuButtonScript cant handle this button type!");
                break;
        }
    }
}