using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Quaternions are used to represent rotations.
/// </summary>
[Serializable]
public struct QuaternionSerializable : ISerializable
{
	
	#region Parameters

	/// <summary>
	/// The x component.
	/// </summary>
	public float x;

	/// <summary>
	/// The y component.
	/// </summary>
	public float y;

	/// <summary>
	/// The z component.
	/// </summary>
	public float z;

	/// <summary>
	/// The w component.
	/// </summary>
	public float w;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="QuaternionSerializable"/> struct.
	/// </summary>
	/// <param name="quaternion">Quaternion.</param>
	public QuaternionSerializable ( Quaternion quaternion ) : this ( quaternion.x, quaternion.y, quaternion.z, quaternion.w )
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QuaternionSerializable"/> struct.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="w">The width.</param>
	public QuaternionSerializable ( float x, float y, float z, float w )
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	#endregion

	#region Operators Overload

	public static implicit operator QuaternionSerializable ( Quaternion quaternion )
	{
		return new QuaternionSerializable ( quaternion );
	}

	public static implicit operator Quaternion ( QuaternionSerializable quaternion )
	{
		return new Quaternion ( quaternion.x, quaternion.y, quaternion.z, quaternion.w );
	}

	#endregion

	#region ISerializable implementation

	public void GetObjectData ( SerializationInfo info, StreamingContext context )
	{
		info.AddValue ( "x", this.x, typeof ( float ) );
		info.AddValue ( "y", this.y, typeof ( float ) );
		info.AddValue ( "z", this.z, typeof ( float ) );
		info.AddValue ( "w", this.w, typeof ( float ) );
	}

	#endregion

}
