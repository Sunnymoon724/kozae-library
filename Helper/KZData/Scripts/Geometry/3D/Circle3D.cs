using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KZLib.KZData
{
	[Serializable]
	public struct Circle3D : IEquatable<Circle3D>,IFormattable,IComparable,IComparable<Circle3D>
	{
		public Vector3 center;
		public Vector3 normal;
		public float radius;

		public float perimeter => 2*Mathf.PI*radius;
		public float area => Mathf.PI*radius*radius;

		private static readonly Circle3D unitCircleXY = new Circle3D(Vector3.zero,Vector3.back,1.0f);
		private static readonly Circle3D unitCircleXZ = new Circle3D(Vector3.zero,Vector3.up,1.0f);
		private static readonly Circle3D unitCircleYZ = new Circle3D(Vector3.zero,Vector3.left,1.0f);

		public static Circle3D unitXY => unitCircleXY;
		public static Circle3D unitXZ => unitCircleXZ;
		public static Circle3D unitYZ => unitCircleYZ;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Circle3D(float radius) : this(Vector3.zero,Vector3.back,radius) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Circle3D(Vector3 center,float radius) : this(center,Vector3.back,radius) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Circle3D(Vector3 center,Vector3 normal,float radius)
		{
			this.center = center;
			this.normal = normal;
			this.radius = radius;
		}

		public void Set(Vector3 newCenter,Vector3 newNormal,float newRadius)
		{
			center = newCenter;
			normal = newNormal;
			radius = newRadius;
		}

		public Vector3 GetPoint(float angle)
		{
			var radians = angle * Mathf.Deg2Rad;
			var tangent = Vector3.Cross(normal,Vector3.up);

			if(tangent.magnitude < 0.001f)
			{
				tangent = Vector3.Cross(normal,Vector3.right);
			}

			tangent.Normalize();

			return center+(tangent*radius*Mathf.Cos(radians))+(normal*radius*Mathf.Sin(radians));
		}

		public IEnumerable<Vector3> GetPointGroup(int count)
		{
			var delta = 360.0f/count;
			var angle = 0.0f;

			for(var i=0;i<count;i++)
			{
				yield return GetPoint(angle);

				angle += delta;
			}
		}

		public bool Contains(Vector3 point)
		{
			var pivot = point-center;

			return Mathf.Abs(Vector3.Dot(pivot,normal)) <= Mathf.Epsilon && pivot.magnitude <= radius;
		}


		public static Circle3D Lerp(Circle3D left,Circle3D right, float time)
		{
			return new Circle3D(Vector3.Lerp(left.center,right.center,time),Vector3.Lerp(left.normal,right.normal,time),Mathf.Lerp(left.radius,right.radius,time));
		}

		public static Circle3D LerpUnclamped(Circle3D left,Circle3D right,float time)
		{
			return new Circle3D(Vector3.LerpUnclamped(left.center,right.center,time),Vector3.LerpUnclamped(left.normal,right.normal,time),Mathf.LerpUnclamped(left.radius,right.radius,time));
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

			return $"({center.x.ToString(format,formatProvider)},{center.y.ToString(format,formatProvider)}) - R : {radius.ToString(format,formatProvider)} / N : ({normal.x.ToString(format,formatProvider)},{normal.y.ToString(format,formatProvider)})";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(center,normal,radius);
		}

		public override bool Equals(object other)
		{
			return other is Circle3D circle && Equals(circle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Circle3D circle)
		{
			return center == circle.center && normal == circle.normal && radius == circle.radius;
		}

		public int CompareTo(object other)
		{
			if(other is Circle3D circle)
			{
				return CompareTo(circle);
			}

			throw new ArgumentException($"{other} is not a Circle3D");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Circle3D circle)
		{
			return radius.CompareTo(circle.radius);
		}

		public static explicit operator Sphere(Circle3D circle)
		{
			return new Sphere(circle.center,circle.radius);
		}

		public static explicit operator Circle2D(Circle3D circle)
		{
			return new Circle2D(circle.center,circle.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle3D operator +(Circle3D left,Vector3 right)
		{
			return new Circle3D(left.center+right,left.normal,left.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle3D operator +(Circle3D left,float right)
		{
			return new Circle3D(left.center,left.normal,left.radius+right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle3D operator -(Circle3D left,Vector3 right)
		{
			return new Circle3D(left.center-right,left.normal,left.radius);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle3D operator -(Circle3D left,float right)
		{
			return new Circle3D(left.center,left.normal,left.radius-right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle3D operator *(Circle3D left,float right)
		{
			return new Circle3D(left.center,left.normal,left.radius*right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Circle3D operator /(Circle3D left,float right)
		{
			return new Circle3D(left.center,left.normal,left.radius/right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Circle3D left,Circle3D right)
		{
			return left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Circle3D left,Circle3D right)
		{
			return !left.Equals(right);
		}
	}
}