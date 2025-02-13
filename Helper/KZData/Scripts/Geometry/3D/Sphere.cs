using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using ProceduralToolkit;
using UnityEngine;

namespace KZLib.KZData
{
	[Serializable]
	public struct Sphere : IEquatable<Sphere>,IFormattable,IComparable,IComparable<Sphere>
	{
		public Vector3 center;
		public float radius;

		public float area => 4*Mathf.PI*radius*radius;
		public float volume => 4f/3f*Mathf.PI*radius*radius*radius;

		private static readonly Sphere unitSphere = new Sphere(Vector3.zero,1.0f);

		public static Sphere unit => unitSphere;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sphere(float radius) : this(Vector3.zero,radius) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sphere(Vector3 center,float radius)
		{
			this.center = center;
			this.radius = radius;
		}

		public void Set(Vector3 newCenter,float newRadius)
		{
			center = newCenter;
			radius = newRadius;
		}

		public Vector3 GetPoint(float horizontalAngle,float verticalAngle)
		{
			var horizontalRadians = horizontalAngle*Mathf.Deg2Rad;
            var verticalRadians = verticalAngle*Mathf.Deg2Rad;
            var cosVertical = Mathf.Cos(verticalRadians);

            return center + new Vector3(radius*Mathf.Sin(horizontalRadians)*cosVertical,radius*Mathf.Sin(verticalRadians),radius*Mathf.Cos(horizontalRadians)*cosVertical);
		}

		public List<Vector3> GetPoints(int count)
		{
			return Geometry.PointsOnCircle2(center, radius, count);
		}

		public bool Contains(Vector3 point)
		{
			return (point-center).magnitude < radius+Mathf.Epsilon;
		}

		public static Sphere Lerp(Sphere left,Sphere right, float time)
		{
			return new Sphere(Vector3.Lerp(left.center,right.center,time),Mathf.Lerp(left.radius,right.radius,time));
		}

		public static Sphere LerpUnclamped(Sphere left,Sphere right,float time)
		{
			return new Sphere(Vector3.LerpUnclamped(left.center,right.center,time),Mathf.LerpUnclamped(left.radius,right.radius,time));
		}

		public override string ToString()
		{
			return ToString(string.Empty,CultureInfo.InvariantCulture);
		}

		public string ToString(string format)
		{
			return ToString(format,CultureInfo.InvariantCulture);
		}

		public string ToString(string format,IFormatProvider? formatProvider)
		{
			if(string.IsNullOrEmpty(format))
			{
				format = "F2";
			}

			formatProvider ??= CultureInfo.InvariantCulture;

			return $"({center.x.ToString(format,formatProvider)},{center.y.ToString(format,formatProvider)}) - R : {radius.ToString(format,formatProvider)}";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(center,radius);
		}

		public override bool Equals(object other)
		{
			return other is Sphere sphere && Equals(sphere);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Sphere sphere)
		{
			return center == sphere.center && radius == sphere.radius;
		}

		public int CompareTo(object other)
		{
			if(other is Sphere sphere)
			{
				return CompareTo(sphere);
			}

			throw new ArgumentException($"{other} is not a Sphere");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Sphere sphere)
		{
			return radius.CompareTo(sphere.radius);
		}

		public static explicit operator Circle2D(Sphere sphere)
		{
			return new Circle2D((Vector2)sphere.center,sphere.radius);
		}

		public static explicit operator Circle3D(Sphere sphere)
		{
			return new Circle3D(sphere.center,Vector3.back,sphere.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Sphere operator +(Sphere left,Vector3 right)
		{
			return new Sphere(left.center+right,left.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Sphere operator +(Sphere left,float right)
		{
			return new Sphere(left.center,left.radius+right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Sphere operator -(Sphere left,Vector3 right)
		{
			return new Sphere(left.center-right,left.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Sphere operator -(Sphere left,float right)
		{
			return new Sphere(left.center,left.radius-right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Sphere operator *(Sphere left,float right)
		{
			return new Sphere(left.center,left.radius*right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Sphere operator /(Sphere left,float right)
		{
			return new Sphere(left.center,left.radius/right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Sphere left,Sphere right)
		{
			return left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Sphere left,Sphere right)
		{
			return !left.Equals(right);
		}
	}
}