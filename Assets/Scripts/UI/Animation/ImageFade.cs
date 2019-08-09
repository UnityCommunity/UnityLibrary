using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Component that when attached to an Image will fade in or out based on isFadingIn
/// </summary>
[RequireComponent(typeof(Image))]
public class ImageFade : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The Time it takes for the object to fade")]
    [SerializeField] float m_timeToFade = 1.5f;
    [Tooltip("The Time it takes before the object begins its fading")]
    [SerializeField] float m_delayTimeToStart = 1.5f;
    [Tooltip("Boolean to determing if the image is fading in or out")]
    [SerializeField] bool m_isFadingIn = true;
    [Tooltip("Boolean to determing if the image is a button or out")]
    [SerializeField] bool m_doesHaveChildText = false;

    //Sibling Components
    private Image m_image;
    private Text m_text;

    //Member variables
    private Color m_startColor;
    private Color m_buttonTextStartColor;
    private Color m_desiredEndColor;
    private float m_timeElapsed = 0.0f;

    private void OnEnable()
    {
        m_timeElapsed = 0.0f;
        GetSiblingComponents();
        //set the starting color to the color of the object
        SetStartingColor();
        //Start the Fade Coroutine
        StartCoroutine(Fade());
    }

    //Function that sets the image color value determined on if you want to fade in or out
    private void SetStartingColor()
    {
        //start color is set to the images current color
        m_startColor = m_image.color;
        //create new varibles to be messed with later
        Color modifiedColor = m_startColor;
        Color modifiedTextColor = new Color();
        if (m_doesHaveChildText)
        {
            //if this had childed text component buttonTextStartcolor is set to the text's current color
            m_buttonTextStartColor = m_text.color;
            modifiedTextColor = m_buttonTextStartColor;
        }
        //sets the starting colors alpha values
        if (m_isFadingIn)
        {
            //if it is fading in set alpha values to 0
            modifiedColor.a = 0.0f;
            modifiedTextColor.a = 0.0f;
        }
        //Set the current colors to be the modified colors for fading
        m_image.color = modifiedColor;
        if (m_doesHaveChildText)
        {
            m_text.color = modifiedTextColor;
        }
    }

    /// <summary>
    /// Gets the Required Sibling components
    /// </summary>
    private void GetSiblingComponents()
    {
        m_image = GetComponent<Image>();
        if (m_doesHaveChildText)
        {
            m_text = GetComponentInChildren<Text>();
        }
    }

    /// <summary>
    /// Coroutine that fades the Image by lerping the alpha from either 1-0 or 0-1
    /// </summary>
    IEnumerator Fade()
    {
        //Wait for the delay 
        yield return new WaitForSeconds(m_delayTimeToStart);
        //create new color
        Color newColor = m_startColor;
        Color newTextColor = new Color();
        if (m_doesHaveChildText)
        {
            newTextColor = m_text.color;
        }
        while (m_timeElapsed < m_timeToFade)
        {
            float time = m_timeElapsed / m_timeToFade;
            if (m_isFadingIn)
            {
                //set new colors alpha to the lerp value of time
                newColor.a = Mathf.Lerp(0, m_startColor.a, time);
                if (m_doesHaveChildText)
                {
                    newTextColor.a = Mathf.Lerp(0, m_buttonTextStartColor.a, time);
                }
            }
            else
            {
                //set new colors alpha to the lerp value of time
                newColor.a = Mathf.Lerp(m_startColor.a, 0, time);
                if (m_doesHaveChildText)
                {
                    newTextColor.a = Mathf.Lerp(m_buttonTextStartColor.a, 0, time);
                }
            }
            //set image color to the new color
            if (m_doesHaveChildText)
            {
                m_text.color = newTextColor;
            }
            m_image.color = newColor;
            //add deltaTime to time elapsed
            m_timeElapsed += Time.deltaTime;
            yield return null;
        }
        //when the loop finishes set the colors to be what they are supposed to be
        if (m_isFadingIn)
        {
            m_image.color = m_startColor;
            if (m_doesHaveChildText)
            {
                m_text.color = m_buttonTextStartColor;
            }
        }
        else
        {
            //if not is fading in set the alpha values to 0
            Color desiredColor = m_startColor;
            desiredColor.a = 0.0f;
            m_image.color = desiredColor;
            if(m_doesHaveChildText)
            {
                desiredColor = m_buttonTextStartColor;
                desiredColor.a = 0.0f;
                m_text.color = desiredColor;
            }
        }
    }
}
