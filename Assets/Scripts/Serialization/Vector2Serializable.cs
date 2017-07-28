using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Representation of 2D vectors and points.
/// </summary>
[Serializable]
public struct Vector2Serializable : ISerializable
{
	
	#region Parametres

	/// <summary>
	/// X component of the vector.
	/// </summary>
	public float x;

	/// <summary>
	/// Y component of the vector.
	/// </summary>
	public float y;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Vector2Serializable"/> struct.
	/// </summary>
	/// <param name="vector">Vector.</param>
	public Vector2Serializable ( Vector2 vector ) : this ( vector.x, vector.y )
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Vector2Serializable"/> struct.
	/// </summary>
	/// <param name="vector">Vector.</param>
	public Vector2Serializable ( Vector3 vector ) : this ( vector.x, vector.y )
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Vector2Serializable"/> struct.
	/// </summary>
	/// <param name="vector">Vector.</param>
	public Vector2Serializable ( Vector4 vector ) : this ( vector.x, vector.y )
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Vector2Serializable"/> struct.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Vector2Serializable ( float x, float y )
	{
		this.x = x;
		this.y = y;
	}

	#endregion

	#region Methods

	public override bool Equals ( object obj )
	{
		if ( obj is Vector2Serializable || obj is Vector2 )
		{
			Vector2Serializable vector = ( Vector2Serializable )obj;
			return this.x == vector.x && this.y == vector.y;
		}
		return false;
	}

	public override int GetHashCode ()
	{
		return this.x.GetHashCode () ^ this.y.GetHashCode () << 2;
	}

	public override string ToString ()
	{
		return string.Format ( "({0}, {1})", this.x, this.y );
	}

	#endregion

	#region Operators Overload

	public static implicit operator Vector2Serializable ( Vector2 vector )
	{
		return new Vector2Serializable ( vector );
	}

	public static implicit operator Vector2 ( Vector2Serializable vector )
	{
		return new Vector2 ( vector.x, vector.y );
	}

	public static implicit operator Vector2Serializable ( Vector3 vector )
	{
		return new Vector2Serializable ( vector );
	}

	public static implicit operator Vector3 ( Vector2Serializable vector )
	{
		return new Vector3 ( vector.x, vector.y );
	}

	public static implicit operator Vector2Serializable ( Vector4 vector )
	{
		return new Vector2Serializable ( vector );
	}

	public static implicit operator Vector4 ( Vector2Serializable vector )
	{
		return new Vector4 ( vector.x, vector.y );
	}

	public static implicit operator Vector2Serializable ( Vector3Serializable vector )
	{
		return new Vector2Serializable ( ( Vector2 )vector );
	}

	public static implicit operator Vector3Serializable ( Vector2Serializable vector )
	{
		return new Vector3Serializable ( ( Vector2 )vector );
	}

	public static implicit operator Vector2Serializable ( Vector4Serializable vector )
	{
		return new Vector2Serializable ( ( Vector2 )vector );
	}

	public static implicit operator Vector4Serializable ( Vector2Serializable vector )
	{
		return new Vector4Serializable ( ( Vector2 )vector );
	}

	#endregion

	#region ISerializable implementation

	public void GetObjectData ( SerializationInfo info, StreamingContext context )
	{
		info.AddValue ( "x", this.x, typeof ( float ) );
		info.AddValue ( "y", this.y, typeof ( float ) );
	}

	#endregion

}
