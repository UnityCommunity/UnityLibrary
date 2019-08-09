using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterGrowEffect : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The desired Scale to reach")]
    [SerializeField] float m_desiredScale = 1.0f;
    [Tooltip("How much time it will take to reach the desired scale")]
    [SerializeField] float m_timeToReachDesiredScale = 1.0f;
    [Tooltip("How fast the helicopter effect will spin")]
    [SerializeField] float m_rotationSpeed = 2.0f;
    [Tooltip("the angle of rotation per frame")]
    [SerializeField] float m_rotationAngle = 10.0f;

    //private member variables
    private float m_currentTime = 0.0f;
    private Quaternion startingRotation = Quaternion.identity;

    private void Awake()
    {
        //set the base transform to 0 so it can grow
        transform.localScale = Vector3.zero;
        //set the staring rotation variable to the current rotation
        startingRotation = transform.rotation;
    }

    void Update()
    {
        //increment current time by delta time
        m_currentTime += Time.deltaTime;
        if (m_currentTime <= m_timeToReachDesiredScale)
        {
            //rotate the object by the rotation angle and rotation speed
            gameObject.transform.Rotate(Vector3.forward, (m_rotationAngle * (m_rotationSpeed * Time.deltaTime)));
            //lerp the scale to the desired scale
            gameObject.transform.localScale = (Vector3.one * Mathf.LerpUnclamped(0, m_desiredScale, Interpolation.ElasticInOut(m_currentTime/m_timeToReachDesiredScale)));
            //leave the function
            return;
        }
        else
        {
            //when the lerping is done set the rotation to the starting rotation
            gameObject.transform.rotation = startingRotation;
            //set the scale to the desired scale
            gameObject.transform.localScale = Vector3.one * m_desiredScale;
        }
    }
}
