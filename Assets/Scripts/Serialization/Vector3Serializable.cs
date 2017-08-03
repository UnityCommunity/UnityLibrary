using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Representation of 3D vectors and points.
/// </summary>
namespace UnityLibrary
{
    [Serializable]
    public struct Vector3Serializable : ISerializable
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Serializable"/> struct.
        /// </summary>
        /// <param name="vector">Vector.</param>
        public Vector3Serializable(Vector2 vector) : this(vector.x, vector.y)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Serializable"/> struct.
        /// </summary>
        /// <param name="vector">Vector.</param>
        public Vector3Serializable(Vector3 vector) : this(vector.x, vector.y, vector.z)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Serializable"/> struct.
        /// </summary>
        /// <param name="vector">Vector.</param>
        public Vector3Serializable(Vector4 vector) : this(vector.x, vector.y, vector.z)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Serializable"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public Vector3Serializable(float x, float y) : this(x, y, 0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Serializable"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public Vector3Serializable(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj is Vector3Serializable || obj is Vector3)
            {
                Vector3Serializable vector = (Vector3Serializable)obj;
                return this.x == vector.x && this.y == vector.y && this.z == vector.z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", this.x, this.y, this.z);
        }

        #endregion

        #region Operators Overload

        public static implicit operator Vector3Serializable(Vector2 vector)
        {
            return new Vector3Serializable(vector);
        }

        public static implicit operator Vector2(Vector3Serializable vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static implicit operator Vector3Serializable(Vector3 vector)
        {
            return new Vector3Serializable(vector);
        }

        public static implicit operator Vector3(Vector3Serializable vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }

        public static implicit operator Vector3Serializable(Vector4 vector)
        {
            return new Vector3Serializable(vector);
        }

        public static implicit operator Vector4(Vector3Serializable vector)
        {
            return new Vector4(vector.x, vector.y, vector.z);
        }

        public static implicit operator Vector3Serializable(Vector2Serializable vector)
        {
            return new Vector3Serializable((Vector3)vector);
        }

        public static implicit operator Vector2Serializable(Vector3Serializable vector)
        {
            return new Vector2Serializable((Vector3)vector);
        }

        public static implicit operator Vector3Serializable(Vector4Serializable vector)
        {
            return new Vector3Serializable((Vector3)vector);
        }

        public static implicit operator Vector4Serializable(Vector3Serializable vector)
        {
            return new Vector4Serializable((Vector3)vector);
        }

        #endregion

        #region ISerializable implementation

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("x", this.x, typeof(float));
            info.AddValue("y", this.y, typeof(float));
            info.AddValue("z", this.z, typeof(float));
        }

        #endregion

    }
}
