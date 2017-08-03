using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Representation of RGBA colors.
/// </summary>
namespace UnityLibrary
{
    [Serializable]
    public struct ColorSerializable : ISerializable
    {

        #region Parameters

        /// <summary>
        /// Th Red.
        /// </summary>
        public float r;

        /// <summary>
        /// The Green.
        /// </summary>
        public float g;

        /// <summary>
        /// The Blue.
        /// </summary>
        public float b;

        /// <summary>
        /// The Alpha.
        /// </summary>
        public float a;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSerializable"/> struct.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public ColorSerializable(float r, float g, float b) : this(r, g, b, 1f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSerializable"/> struct.
        /// </summary>
        /// <param name="color">Color.</param>
        public ColorSerializable(Color color) : this(color.r, color.g, color.b, color.a)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSerializable"/> struct.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public ColorSerializable(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        #endregion

        #region Operators Overload

        public static implicit operator ColorSerializable(Color color)
        {
            return new ColorSerializable(color);
        }

        public static implicit operator Color(ColorSerializable color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }

        #endregion

        #region ISerializable implementation

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("r", this.r, typeof(float));
            info.AddValue("g", this.g, typeof(float));
            info.AddValue("b", this.b, typeof(float));
            info.AddValue("a", this.a, typeof(float));
        }

        #endregion

    }
}