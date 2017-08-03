using UnityEngine;
using System.Collections;

// usage: attach this script into camera, call Shake() method to start
// source: http://answers.unity3d.com/answers/992509/view.html
namespace UnityLibrary
{
    public class CameraShake : MonoBehaviour
    {
        public bool shakePosition;
        public bool shakeRotation;

        public float shakeIntensityMin = 0.1f;
        public float shakeIntensityMax = 0.5f;
        public float shakeDecay = 0.02f;

        private Vector3 OriginalPos;
        private Quaternion OriginalRot;

        private bool isShakeRunning = false;

        // call this function to start shaking
        public void Shake()
        {
            OriginalPos = transform.position;
            OriginalRot = transform.rotation;
            StartCoroutine("ProcessShake");
        }

        IEnumerator ProcessShake()
        {
            if (!isShakeRunning)
            {
                isShakeRunning = true;
                float currentShakeIntensity = Random.Range(shakeIntensityMin, shakeIntensityMax);

                while (currentShakeIntensity > 0)
                {
                    if (shakePosition)
                    {
                        transform.position = OriginalPos + Random.insideUnitSphere * currentShakeIntensity;
                    }
                    if (shakeRotation)
                    {
                        transform.rotation = new Quaternion(OriginalRot.x + Random.Range(-currentShakeIntensity, currentShakeIntensity) * .2f,
                                                             OriginalRot.y + Random.Range(-currentShakeIntensity, currentShakeIntensity) * .2f,
                                                             OriginalRot.z + Random.Range(-currentShakeIntensity, currentShakeIntensity) * .2f,
                                                             OriginalRot.w + Random.Range(-currentShakeIntensity, currentShakeIntensity) * .2f);
                    }
                    currentShakeIntensity -= shakeDecay;
                    yield return null;
                }

                isShakeRunning = false;
            }
        }
    }
}