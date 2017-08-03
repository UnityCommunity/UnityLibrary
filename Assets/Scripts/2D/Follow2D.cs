using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Follows a GameObject in a Smooth way and with various settings
/// Author: Manuel Otheo (@Lootheo) with guidance from Hasan Bayat (EmpireWorld)
/// 
/// https://www.reddit.com/r/Unity3D/comments/6iskah/movetowards_vs_lerp_vs_slerp_vs_smoothdamp/
/// How to use: Attach it to a GameObject and then assign the target to follow and the variables like offset and speed
/// If it's not moving check the speed
/// 
/// TODO: Make more efficient usage of the vector3 to vector2;
/// </summary>
/// 
namespace UnityLibrary
{
    public class Follow2D : MonoBehaviour
    {

        public enum FollowType
        {
            MoveTowards,
            Lerp,
            Slerp,
            SmoothDamp,
            Acceleration
        }

        #region Fields

        public Transform target;
        public FollowType followType = FollowType.MoveTowards;
        public Vector2 speed;
        public Vector2 time;
        public Vector2 acceleration;
        public Vector2 offset;
        public bool bounds;
        public Vector2 lowerBounds;
        public Vector2 higherBounds;
        #endregion

        #region Variables

        protected Vector2 velocity;
        protected Vector2 step;
        private Vector2 localSpeed;

        #endregion

        #region MonoBehaviour Messages

        protected virtual void Update()
        {

            // Exit if the target object not specified
            if (target == null)
            {
                return;
            }

            switch (followType)
            {
                case FollowType.MoveTowards:
                    MoveTowards();
                    break;
                case FollowType.Lerp:
                    Lerp();
                    break;
                case FollowType.Slerp:
                    Slerp();
                    break;
                case FollowType.SmoothDamp:
                    SmoothDamp();
                    break;
                case FollowType.Acceleration:
                    Acceleration();
                    break;
            }

            if (bounds)
            {
                CheckForBounds();
            }
        }

        #endregion

        #region Methods

        protected virtual void MoveTowards()
        {
            step = speed * Time.deltaTime;
            transform.position = new Vector2(Vector2.MoveTowards(transform.position, (Vector2)target.position + offset, step.x).x, Vector2.MoveTowards(transform.position, (Vector2)target.position + offset, step.x).y);
        }

        protected virtual void Lerp()
        {
            float posX = Mathf.Lerp(transform.position.x, target.position.x + offset.x, time.x * Time.fixedDeltaTime);
            float posY = Mathf.Lerp(transform.position.y, target.position.y + offset.y, time.y * Time.fixedDeltaTime);
            transform.position = new Vector3(posX, posY, transform.position.z);
        }

        protected virtual void Slerp()
        {
            float posX = Vector3.Slerp(transform.position, (Vector3)((Vector2)target.position + offset), time.x * Time.fixedDeltaTime).x;
            float posY = Vector3.Slerp(transform.position, (Vector3)((Vector2)target.position + offset), time.y * Time.fixedDeltaTime).y;
            transform.position = new Vector3(posX, posY, transform.position.z);
        }

        protected virtual void SmoothDamp()
        {
            Vector2 position;

            position.x = Mathf.SmoothDamp(transform.position.x, target.position.x + offset.x, ref velocity.x, time.x);
            position.y = Mathf.SmoothDamp(transform.position.y, target.position.y + offset.y, ref velocity.y, time.y);

            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
        protected virtual void Acceleration()
        {
            if (Vector2.Distance(transform.position, (Vector2)target.position + offset) == 0)
                localSpeed = Vector2.zero;
            else
            {
                localSpeed = localSpeed + acceleration * Time.deltaTime;
                step = localSpeed * Time.deltaTime;
                transform.position = new Vector2(Vector2.MoveTowards(transform.position, (Vector2)target.position + offset, step.x).x, Vector2.MoveTowards(transform.position, (Vector2)target.position + offset, step.x).y);
            }
        }

        protected virtual void CheckForBounds()
        {
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, lowerBounds.x, higherBounds.x), Mathf.Clamp(transform.position.y, lowerBounds.y, higherBounds.y), transform.position.z);
        }

        #endregion

    }
}
