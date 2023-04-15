using UnityEngine;
using System.Collections;

/* 
 * Usage: attach this script to a camera or any other object, call Shake() method to start shaking it
* To turn off, change influence to zero
* Attach the camera to an empty game object to keep its local position and rotation
*/
namespace UnityLibrary
{
    public class CameraShake : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float shakeInfluence = 0.5f;
        [Range(0f, 10f)]
        public float rotationInfluence = 0f;

        private Vector3 OriginalPos;
        private Quaternion OriginalRot;
        private bool isShakeRunning = false;

/// <summary>
/// Will shake the camera with a random value between minIntensity and maxIntensity for duration
/// </summary>
/// <param name="minIntensity"></param>
/// <param name="maxIntensity"></param>
/// <param name="duration"></param>
        public void Shake(float minIntensity, float maxIntensity, float duration)
        {
            if (isShakeRunning)
                return;

            OriginalPos = transform.position;
            OriginalRot = transform.rotation;

            float shake = Random.Range(minIntensity, maxIntensity) * shakeInfluence;
            duration *= shakeInfluence;

            StartCoroutine(ProcessShake(shake, duration));
        }

        IEnumerator ProcessShake(float shake, float duration)
        {
            isShakeRunning = true;
            float countdown = duration;
            float initialShake = shake;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;

                float lerpIntensity = countdown / duration;
                shake = Mathf.Lerp(0f, initialShake, lerpIntensity);

                transform.position = OriginalPos + Random.insideUnitSphere * shake;
                transform.rotation = Quaternion.Euler(OriginalRot.eulerAngles + Random.insideUnitSphere * shake * rotationInfluence);

                yield return null;
            }

            transform.position = OriginalPos;
            transform.rotation = OriginalRot;
            isShakeRunning = false;
        }
    }
}