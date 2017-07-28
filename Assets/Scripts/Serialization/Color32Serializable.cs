using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Representation of RGBA colors in 32 bit format.
/// </summary>
[Serializable]
public struct Color32Serializable : ISerializable
{
	
	#region Parameters

	/// <summary>
	/// Th Red.
	/// </summary>
	public byte r;

	/// <summary>
	/// The Green.
	/// </summary>
	public byte g;

	/// <summary>
	/// The Blue.
	/// </summary>
	public byte b;

	/// <summary>
	/// The Alpha.
	/// </summary>
	public byte a;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorSerializable"/> struct.
	/// </summary>
	/// <param name="color">Color.</param>
	public Color32Serializable ( Color32 color ) : this ( color.r, color.g, color.b, color.a )
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorSerializable"/> struct.
	/// </summary>
	/// <param name="r">The red component.</param>
	/// <param name="g">The green component.</param>
	/// <param name="b">The blue component.</param>
	/// <param name="a">The alpha component.</param>
	public Color32Serializable ( byte r, byte g, byte b, byte a )
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	#endregion

	#region Operators Overload

	public static implicit operator Color32Serializable ( Color32 color )
	{
		return new Color32Serializable ( color );
	}

	public static implicit operator Color32 ( Color32Serializable color )
	{
		return new Color32 ( color.r, color.g, color.b, color.a );
	}

	#endregion

	#region ISerializable implementation

	public void GetObjectData ( SerializationInfo info, StreamingContext context )
	{
		info.AddValue ( "r", this.r, typeof ( byte ) );
		info.AddValue ( "g", this.g, typeof ( byte ) );
		info.AddValue ( "b", this.b, typeof ( byte ) );
		info.AddValue ( "a", this.a, typeof ( byte ) );
	}

	#endregion

}
