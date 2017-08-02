using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLibrary
{

	/// <summary>
	/// Transform extensions.
	/// Useful transform utilities and methods.
	/// </summary>
	public static class TransformExtensions
	{

		/// <summary>
		/// Rotates the transform so the forward vector points at target's current position.
		/// </summary>
		/// <param name="transform">Transform.</param>
		/// <param name="target">Target.</param>
		public static void LookAt2D ( this Transform transform, Transform target )
		{
			transform.LookAt2D ( ( Vector2 )target.position );
		}

		/// <summary>
		/// Rotates the transform so the forward vector points at worldPosition.
		/// </summary>
		/// <param name="transform">Transform.</param>
		/// <param name="worldPosition">World position.</param>
		public static void LookAt2D ( this Transform transform, Vector3 worldPosition )
		{
			transform.LookAt2D ( ( Vector2 )worldPosition );
		}

		/// <summary>
		/// Rotates the transform so the forward vector points at worldPosition.
		/// </summary>
		/// <param name="transform">Transform.</param>
		/// <param name="worldPosition">World position.</param>
		public static void LookAt2D ( this Transform transform, Vector2 worldPosition )
		{
			Vector2 distance = worldPosition - ( Vector2 )transform.position;
			transform.eulerAngles = new Vector3 ( 
				transform.eulerAngles.x, 
				transform.eulerAngles.y, 
				Mathf.Atan2 ( distance.y, distance.x ) * Mathf.Rad2Deg );
		}

	}

}