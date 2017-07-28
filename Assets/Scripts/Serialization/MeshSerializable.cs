using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// A class that allows creating and modifying meshes from scripts.
/// </summary>
[Serializable]
public sealed class MeshSerializable : UnityEngine.Object, ISerializable
{
	
	#region Parameters

	public Vector3[] vertices;
	public int[] triangles;
	public Vector3[] normals;
	public Color[] colors;
	public Vector4[] tangents;
	public Color32[] colors32;
	public Vector2[] uv;
	public Vector2[] uv2;
	public Vector2[] uv3;
	public Vector2[] uv4;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="MeshSerializable"/> class.
	/// </summary>
	public MeshSerializable () : this ( new Mesh () )
	{
		
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MeshSerializable"/> class.
	/// </summary>
	/// <param name="mesh">Mesh.</param>
	public MeshSerializable ( Mesh mesh )
	{
		this.vertices = mesh.vertices;
		this.triangles = mesh.triangles;
		this.normals = mesh.normals;
		this.colors = mesh.colors;
		this.tangents = mesh.tangents;
		this.colors32 = mesh.colors32;
		this.uv = mesh.uv;
		this.uv2 = mesh.uv2;
		this.uv3 = mesh.uv3;
		this.uv4 = mesh.uv4;
	}

	#endregion

	#region Operators Overload

	public static implicit operator MeshSerializable ( Mesh mesh )
	{
		return new MeshSerializable ( mesh );
	}

	public static implicit operator Mesh ( MeshSerializable mesh )
	{
		Mesh result = new Mesh ();
		result.vertices = mesh.vertices;
		result.triangles = mesh.triangles;
		result.normals = mesh.normals;
		result.colors = mesh.colors;
		result.tangents = mesh.tangents;
		result.colors32 = mesh.colors32;
		result.uv = mesh.uv;
		result.uv2 = mesh.uv2;
		result.uv3 = mesh.uv3;
		result.uv4 = mesh.uv4;
		return result;
	}

	#endregion

	#region ISerializable implementation

	public void GetObjectData ( SerializationInfo info, StreamingContext context )
	{
		info.AddValue ( "vertices", this.vertices, typeof ( Vector3[] ) );
		info.AddValue ( "triangles", this.triangles, typeof ( int[] ) );
		info.AddValue ( "normals", this.normals, typeof ( Vector3[] ) );
		info.AddValue ( "colors", this.colors, typeof ( Color[] ) );
		info.AddValue ( "tangents", this.tangents, typeof ( Vector4[] ) );
		info.AddValue ( "colors32", this.colors32, typeof ( Color32[] ) );
		info.AddValue ( "uv", this.uv, typeof ( Vector3[] ) );
		info.AddValue ( "uv2", this.uv2, typeof ( Vector3[] ) );
		info.AddValue ( "uv3", this.uv3, typeof ( Vector3[] ) );
		info.AddValue ( "uv4", this.uv4, typeof ( Vector3[] ) );
	}

	#endregion

}
