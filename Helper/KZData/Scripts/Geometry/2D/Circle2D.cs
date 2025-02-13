using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using ProceduralToolkit;
using UnityEngine;

namespace KZLib.KZData
{
	[Serializable]
	public struct Circle2D : IEquatable<Circle2D>,IFormattable,IComparable,IComparable<Circle2D>
	{
		public Vector2 center;
		public float radius;

		public float perimeter => 2*Mathf.PI*radius;
		public float area => Mathf.PI*radius*radius;

		private static readonly Circle2D unitCircle = new Circle2D(Vector2.zero,1.0f);

		public static Circle2D unit => unitCircle;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Circle2D(float radius) : this(Vector2.zero,radius) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Circle2D(Vector2 center,float radius)
		{
			this.center = center;
			this.radius = radius;
		}

		public void Set(Vector2 newCenter,float newRadius)
		{
			center = newCenter;
			radius = newRadius;
		}

		public Vector2 GetPoint(float angle)
		{
			var radians = angle*Mathf.Deg2Rad;

			return center+new Vector2(radius*Mathf.Sin(radians),radius*Mathf.Cos(radians));
		}

		public IEnumerable<Vector2> GetPointGroup(int count)
		{
			var delta = 360.0f/count;
			var angle = 0.0f;

			for(var i=0;i<count;i++)
			{
				yield return GetPoint(angle);

				angle += delta;
			}
		}

		public bool Contains(Vector2 point)
		{
			return (point-center).magnitude < radius+Mathf.Epsilon;
		}

		public static Circle2D Lerp(Circle2D left,Circle2D right,float time)
		{
			return new Circle2D(Vector2.Lerp(left.center,right.center,time),Mathf.Lerp(left.radius,right.radius,time));
		}

		public static Circle2D LerpUnclamped(Circle2D left,Circle2D right,float time)
		{
			return new Circle2D(Vector2.LerpUnclamped(left.center,right.center,time),Mathf.LerpUnclamped(left.radius,right.radius,time));
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
			return other is Circle2D circle && Equals(circle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Circle2D circle)
		{
			return center == circle.center && radius == circle.radius;
		}

		public int CompareTo(object other)
		{
			if(other is Circle2D circle)
			{
				return CompareTo(circle);
			}

			throw new ArgumentException($"{other} is not a Circle2D");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Circle2D circle)
		{
			return radius.CompareTo(circle.radius);
		}

		public static explicit operator Sphere(Circle2D circle)
		{
			return new Sphere((Vector3)circle.center,circle.radius);
		}

		public static explicit operator Circle3D(Circle2D circle)
		{
			return new Circle3D((Vector3)circle.center,Vector3.back,circle.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle2D operator +(Circle2D left,Vector2 right)
		{
			return new Circle2D(left.center+right,left.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle2D operator +(Circle2D left,float right)
		{
			return new Circle2D(left.center,left.radius+right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle2D operator -(Circle2D left,Vector2 right)
		{
			return new Circle2D(left.center-right,left.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle2D operator -(Circle2D left,float right)
		{
			return new Circle2D(left.center,left.radius-right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle2D operator *(Circle2D left,float right)
		{
			return new Circle2D(left.center,left.radius*right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle2D operator /(Circle2D left,float right)
		{
			return new Circle2D(left.center,left.radius/right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Circle2D left,Circle2D right)
		{
			return left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Circle2D left,Circle2D right)
		{
			return !left.Equals(right);
		}
	}
}