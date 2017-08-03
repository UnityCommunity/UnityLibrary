using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Representation of four-dimensional vectors.
/// </summary>
namespace UnityLibrary
{
    [Serializable]
    public struct Vector4Serializable : ISerializable
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

        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public float z;

        /// <summary>
        /// W component of the vector.
        /// </summary>
        public float w;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4Serializable"/> struct.
        /// </summary>
        /// <param name="vector">Vector.</param>
        public Vector4Serializable(Vector2 vector) : this(vector.x, vector.y)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4Serializable"/> struct.
        /// </summary>
        /// <param name="vector">Vector.</param>
        public Vector4Serializable(Vector3 vector) : this(vector.x, vector.y, vector.z)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4Serializable"/> struct.
        /// </summary>
        /// <param name="vector">Vector.</param>
        public Vector4Serializable(Vector4 vector) : this(vector.x, vector.y, vector.z, vector.w)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4Serializable"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public Vector4Serializable(float x, float y) : this(x, y, 0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4Serializable"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public Vector4Serializable(float x, float y, float z) : this(x, y, z, 0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4Serializable"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// <param name="w">The width.</param>
        public Vector4Serializable(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj is Vector4Serializable || obj is Vector4)
            {
                Vector4Serializable vector = (Vector4Serializable)obj;
                return this.x == vector.x && this.y == vector.y && this.z == vector.z && this.w == vector.w;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", this.x, this.y, this.z, this.w);
        }

        #endregion

        #region Operators Overload

        public static implicit operator Vector4Serializable(Vector2 vector)
        {
            return new Vector4Serializable(vector);
        }

        public static implicit operator Vector2(Vector4Serializable vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static implicit operator Vector4Serializable(Vector3 vector)
        {
            return new Vector4Serializable(vector);
        }

        public static implicit operator Vector3(Vector4Serializable vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }

        public static implicit operator Vector4Serializable(Vector4 vector)
        {
            return new Vector4Serializable(vector);
        }

        public static implicit operator Vector4(Vector4Serializable vector)
        {
            return new Vector4(vector.x, vector.y, vector.z, vector.w);
        }

        public static implicit operator Vector4Serializable(Vector2Serializable vector)
        {
            return new Vector4Serializable((Vector4)vector);
        }

        public static implicit operator Vector2Serializable(Vector4Serializable vector)
        {
            return new Vector2Serializable((Vector4)vector);
        }

        public static implicit operator Vector4Serializable(Vector3Serializable vector)
        {
            return new Vector4Serializable((Vector4)vector);
        }

        public static implicit operator Vector3Serializable(Vector4Serializable vector)
        {
            return new Vector3Serializable((Vector4)vector);
        }

        #endregion

        #region ISerializable implementation

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("x", this.x, typeof(float));
            info.AddValue("y", this.y, typeof(float));
            info.AddValue("z", this.w, typeof(float));
            info.AddValue("w", this.w, typeof(float));
        }

        #endregion

    }
}
