using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Script that when attached to an object will cause it to grow until it reaches the desired Scale
/// </summary>
public class GrowEffect : MonoBehaviour
{
    private enum Effect
    {
        BOUNCE_IN_OUT,
        ELASTIC_IN_OUT,
        LINEAR,
        CIRCULAR_IN_OUT
    }

    [Header("Configuration")]
    [Tooltip("The desired scale to grow to")]
    [SerializeField] float m_desiredScale = 1.0f;
    [Tooltip("How much time it will take to reach the desired scale")]
    [SerializeField] float m_timeToReachDesiredScale = 1.0f;
    [Tooltip("How much time it takes before the grow effect starts")]
    [SerializeField] float m_timeToDelayStart = 0.0f;
    [Tooltip("Effect on the grow animation")]
    [SerializeField] Effect m_interpolationEffect = Effect.ELASTIC_IN_OUT;

    //private member variables
    private float m_currentTime = 0.0f;

    private void OnEnable()
    {
        //set the local scale to zero so it can grow
        transform.localScale = Vector3.zero;
        m_currentTime = 0.0f;
    }

    void Update()
    {
        if (m_timeToDelayStart <= 0.0)
        {
            //increment current time by DT
            m_currentTime += Time.deltaTime;

            if (!(m_currentTime > m_timeToReachDesiredScale))
            {
                //Switch as to which effect will happen
                switch (m_interpolationEffect)
                {
                    case Effect.BOUNCE_IN_OUT:
                        gameObject.transform.localScale = (Vector3.one * Mathf.LerpUnclamped(0, m_desiredScale, Interpolation.BounceInOut(m_currentTime / m_timeToReachDesiredScale)));
                        break;
                    case Effect.ELASTIC_IN_OUT:
                        gameObject.transform.localScale = (Vector3.one * Mathf.LerpUnclamped(0, m_desiredScale, Interpolation.ElasticInOut(m_currentTime / m_timeToReachDesiredScale)));
                        break;
                    case Effect.LINEAR:
                        gameObject.transform.localScale = (Vector3.one * Mathf.LerpUnclamped(0, m_desiredScale, Interpolation.Linear(m_currentTime / m_timeToReachDesiredScale)));
                        break;
                    case Effect.CIRCULAR_IN_OUT:
                        gameObject.transform.localScale = (Vector3.one * Mathf.LerpUnclamped(0, m_desiredScale, Interpolation.CircularInOut(m_currentTime / m_timeToReachDesiredScale)));
                        break;
                }
            }
        }
        else
        {
            //reduce the delay start by DT
            m_timeToDelayStart -= Time.deltaTime;
        }
    }
}
