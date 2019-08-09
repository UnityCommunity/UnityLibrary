using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpFromOffset : MonoBehaviour
{
    /// <summary>
    /// Effect Enum only to be used by this class, for chossing which interpolation effect to use
    /// </summary>
    private enum Effect
    {
        BOUNCE_OUT,
        ELASTIC_OUT,
        LINEAR,
        CIRCULAR_OUT,
        CIRCULAR_IN,
        CIRCULAR_IN_OUT,
        INVERSE_CIRCULAR_IN_OUT
    }

    [Header("Configuration")]
    [Tooltip("The The Effect you would like to happen")]
    [SerializeField] Effect m_effect = default;
    [Tooltip("The direction you want the object to lerp from")]
    [SerializeField] Vector3 m_offsetDirection = default;
    [Tooltip("The the distnace you want to multiply the direction by")]
    [SerializeField] float m_offsetDistance = 1.0f;
    [Tooltip("How much time it will take for the object to reach its original position")]
    [SerializeField] float m_timeToReachPosition = 1.5f;
    [Tooltip("How much time it will take to start moving")]
    [SerializeField] float m_delayStart = 1.5f;


    //Memeber variables
    private Vector3 m_offset = Vector3.zero;
    private Vector3 m_desiredPos = Vector3.zero;
    private float m_currentTime = 0.0f;

    void Awake()
    {
        Setup();
    }
    /// <summary>
    /// Method that sets up all of the needed things and places the object at the calcuated offset
    /// </summary>
    void Setup()
    {
        //calculate offset position
        m_offset = m_offsetDirection.normalized * m_offsetDistance;
        //set the desired position to the current position
        m_desiredPos = transform.position;
        //move the object to the offset position to begin lerping
        transform.position = m_desiredPos + m_offset;
    }

    void Update()
    {
        if (m_delayStart <= 0.0f)
        {
            //add deltaTime to the current time
            m_currentTime += Time.deltaTime;
            if (m_currentTime < m_timeToReachPosition)
            {
                //based on which effect is selected this will lerp from the curretn position to the desired position
                switch (m_effect)
                {
                    //Lerps using the BouceOut Interpolation
                    case Effect.BOUNCE_OUT:
                        transform.position = Vector3.LerpUnclamped(m_desiredPos + m_offset, m_desiredPos, Interpolation.BounceOut(m_currentTime / m_timeToReachPosition));
                        break;
                    //Lerps using the ElasticOut Interpolation
                    case Effect.ELASTIC_OUT:
                        transform.position = Vector3.LerpUnclamped(m_desiredPos + m_offset, m_desiredPos, Interpolation.ElasticOut(m_currentTime / m_timeToReachPosition));
                        break;
                    //Lerps using the Linear Interpolation
                    case Effect.LINEAR:
                        transform.position = Vector3.LerpUnclamped(m_desiredPos + m_offset, m_desiredPos, Interpolation.Linear(m_currentTime / m_timeToReachPosition));
                        break;
                    //Lerps using the CircularOut Interpolation
                    case Effect.CIRCULAR_OUT:
                        transform.position = Vector3.LerpUnclamped(m_desiredPos + m_offset, m_desiredPos, Interpolation.CircularOut(m_currentTime / m_timeToReachPosition));
                        break;
                    //Lerps using the CircularInOut Interpolation
                    case Effect.CIRCULAR_IN_OUT:
                        transform.position = Vector3.LerpUnclamped(m_desiredPos + m_offset, m_desiredPos, Interpolation.CircularInOut(m_currentTime / m_timeToReachPosition));
                        break;
                    //Lerps using the CircularIn Interpolation
                    case Effect.CIRCULAR_IN:
                        transform.position = Vector3.LerpUnclamped(m_desiredPos + m_offset, m_desiredPos, Interpolation.SineIn(m_currentTime / m_timeToReachPosition));
                        break;
                }

                return;
            }
            //set the current position to the desired position
            transform.position = m_desiredPos;
        }
        else
        {
            //remove Deltatime from the m_delayStart
            m_delayStart -= Time.deltaTime;
        }
    }
}
