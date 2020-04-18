using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces._2D;
using Engine.Physics;
using Engine.Shapes._2D;
using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Utility
{
	public static class Utilities
	{
		public static int EnumCount<T>()
		{
			return Enum.GetValues(typeof(T)).Length;
		}

		public static T EnumParse<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value);
		}

		public static byte Lerp(byte start, byte end, float t)
		{
			return (byte)(((float)end - start) * t + start);
		}

		public static int Lerp(int start, int end, float t)
		{
			return (int)(((float)end - start) * t) + start;
		}

		public static float Lerp(float start, float end, float t)
		{
			return (end - start) * t + start;
		}

		public static float Length(vec2 v)
		{
			return (float)Math.Sqrt(LengthSquared(v));
		}

		public static float Length(vec3 v)
		{
			return (float)Math.Sqrt(LengthSquared(v));
		}

		public static float LengthSquared(vec2 v)
		{
			return v.x * v.x + v.y * v.y;
		}

		public static float LengthSquared(vec3 v)
		{
			return v.x * v.x + v.y * v.y + v.z * v.z;
		}

		public static float Distance(vec2 p1, vec2 p2)
		{
			return Length(p2 - p1);
		}

		public static float Distance(vec3 p1, vec3 p2)
		{
			return Length(p2 - p1);
		}

		public static float DistanceSquared(vec2 p1, vec2 p2)
		{
			return LengthSquared(p2 - p1);
		}

		public static float DistanceSquared(vec3 p1, vec3 p2)
		{
			return LengthSquared(p2 - p1);
		}

		public static float DistanceToLine(vec2 p, vec2 e0, vec2 e1, out float t)
		{
			return Line2D.Distance(p, e0, e1, out t);
		}

		public static float DistanceSquaredToLine(vec2 p, vec2 e0, vec2 e1, out float t)
		{
			return Line2D.DistanceSquared(p, e0, e1, out t);
		}

		public static float DistanceToLine(vec3 p, vec3 e0, vec3 e1)
		{
			return Line3D.Distance(p, e0, e1);
		}

		public static float DistanceToLine(vec3 p, vec3 e0, vec3 e1, out float t)
		{
			return Line3D.Distance(p, e0, e1, out t);
		}

		public static float DistanceSquaredToLine(vec3 p, vec3 e0, vec3 e1)
		{
			return Line3D.DistanceSquared(p, e0, e1);
		}

		public static float DistanceSquaredToLine(vec3 p, vec3 e0, vec3 e1, out float t)
		{
			return Line3D.DistanceSquared(p, e0, e1, out t);
		}

		public static vec3 ClosestPointOnLine(vec3 p, vec3 e0, vec3 e1)
		{
			return Line3D.ClosestPoint(p, e0, e1);
		}

		public static float DistanceSquaredToTriangle(vec3 p, vec3 p0, vec3 p1, vec3 p2, out vec3 result,
			out vec3 n, bool shouldNormalize = true)
		{
			return DistanceSquaredToTriangle(p, new [] { p0, p1, p2 }, out result, out n, shouldNormalize);
		}

		public static float DistanceSquaredToTriangle(vec3 p, vec3[] triangle, out vec3 result, out vec3 n,
			bool shouldNormalize = true)
		{
			// See https://www.geometrictools.com/GTEngine/Include/Mathematics/GteDistPointTriangleExact.h.
			var diff = p - triangle[0];
			var e0 = triangle[1] - triangle[0];
			var e1 = triangle[2] - triangle[0];
			var a00 = Dot(e0, e0);
			var a01 = Dot(e0, e1);
			var a11 = Dot(e1, e1);
			var b0 = -Dot(diff, e0);
			var b1 = -Dot(diff, e1);
			var det = a00 * a11 - a01 * a01;
			var t0 = a01 * b1 - a11 * b0;
			var t1 = a01 * b0 - a00 * b1;

			if (t0 + t1 <= det)
			{
				if (t0 < 0)
				{
					if (t1 < 0)
					{
						if (b0 < 0)
						{
							t1 = 0;

							if (-b0 >= a00)
							{
								t0 = 1;
							}
							else
							{
								t0 = -b0 / a00;
							}
						}
						else
						{
							t0 = 0;

							if (b1 >= 0)
							{
								t1 = 0;
							}
							else if (-b1 >= a11)
							{
								t1 = 1;
							}
							else
							{
								t1 = -b1 / a11;
							}
						}
					}
					else
					{
						t0 = 0;

						if (b1 >= 0)
						{
							t1 = 0;
						}
						else if (-b1 >= a11)
						{
							t1 = 1;
						}
						else
						{
							t1 = -b1 / a11;
						}
					}
				}
				else if (t1 < 0)
				{
					t1 = 0;

					if (b0 >= 0)
					{
						t0 = 0;
					}
					else if (-b0 >= a00)
					{
						t0 = 1;
					}
					else
					{
						t0 = -b0 / a00;
					}
				}
				else
				{
					var invDet = 1 / det;

					t0 *= invDet;
					t1 *= invDet;
				}
			}
			else
			{
				float tmp0;
				float tmp1;
				float numer;
				float denom;

				if (t0 < 0)
				{
					tmp0 = a01 + b0;
					tmp1 = a11 + b1;

					if (tmp1 > tmp0)
					{
						numer = tmp1 - tmp0;
						denom = a00 - 2 * a01 + a11;

						if (numer >= denom)
						{
							t0 = 1;
							t1 = 0;
						}
						else
						{
							t0 = numer / denom;
							t1 = 1 - t0;
						}
					}
					else
					{
						t0 = 0;

						if (tmp1 <= 0)
						{
							t1 = 1;
						}
						else if (b1 >= 0)
						{
							t1 = 0;
						}
						else
						{
							t1 = -b1 / a11;
						}
					}
				}
				else if (t1 < 0)
				{
					tmp0 = a01 + b1;
					tmp1 = a00 + b0;

					if (tmp1 > tmp0)
					{
						numer = tmp1 - tmp0;
						denom = a00 - 2 * a01 + a11;

						if (numer >= denom)
						{
							t1 = 1;
							t0 = 0;
						}
						else
						{
							t1 = numer / denom;
							t0 = 1 - t1;
						}
					}
					else
					{
						t1 = 0;

						if (tmp1 <= 0)
						{
							t0 = 0;
						}
						else if (b0 >= 0)
						{
							t0 = 0;
						}
						else
						{
							t0 = -b0 / a00;
						}
					}
				}
				else
				{
					numer = a11 + b1 - a01 - b0;

					if (numer <= 0)
					{
						t0 = 0;
						t1 = 1;
					}
					else
					{
						denom = a00 - 2 * a01 + a11;

						if (numer >= denom)
						{
							t0 = 1;
							t1 = 0;
						}
						else
						{
							t0 = numer / denom;
							t1 = 1 - t0;
						}
					}
				}
			}

			result = triangle[0] + t0 * e0 + t1 * e1;
			diff = p - result;
			n = shouldNormalize ? Normalize(diff) : diff;

			return Dot(diff, diff);
		}

		public static void ClosestEdge(vec3 p, vec3[] triangle, out vec3 e0, out vec3 e1)
		{
			var indexes = ClosestEdgeIndexes(p, triangle);
			e0 = triangle[indexes.x];
			e1 = triangle[indexes.y];
		}

		public static ivec2 ClosestEdgeIndexes(vec3 p, vec3[] triangle)
		{
			var d = float.MaxValue;
			var indexes = ivec2.Zero;

			for (int i = 0; i < 3; i++)
			{
				var p1 = triangle[i];
				var p2 = triangle[(i + 1) % 3];
				var squared = DistanceSquaredToLine(p, p1, p2);

				if (squared < d)
				{
					d = squared;
					indexes.x = i;
					indexes.y = (i + 1) % 3;
				}
			}

			return indexes;
		}

		public static vec3 ClosestVertex(vec3 p, vec3[] triangle)
		{
			var d = float.MaxValue;
			var result = vec3.Zero;

			foreach (var point in triangle)
			{
				var squared = DistanceSquared(point, p);

				if (squared < d)
				{
					result = point;
					d = squared;
				}
			}

			return result;
		}

		public static vec3 Normal(vec3[] triangle, WindingTypes winding, bool shouldNormalize = true)
		{
			return Normal(triangle[0], triangle[1], triangle[2], winding, shouldNormalize);
		}

		public static vec3 Normal(vec3 p0, vec3 p1, vec3 p2, WindingTypes winding, bool shouldNormalize = true)
		{
			var v = Cross(p1 - p0, p2 - p0) * (winding == WindingTypes.Clockwise ? 1 : -1);

			return shouldNormalize ? Normalize(v) : v;
		}

		public static vec3 Barycentric(vec3 p, vec3[] triangle)
		{
			return Barycentric(p, triangle[0], triangle[1], triangle[2]);
		}

		public static vec3 Barycentric(vec3 p, vec3 p0, vec3 p1, vec3 p2)
		{
			// See https://gamedev.stackexchange.com/a/23745/91516.
			var v0 = p1 - p0;
			var v1 = p2 - p0;
			var v2 = p - p0;
			var d00 = Dot(v0, v0);
			var d01 = Dot(v0, v1);
			var d11 = Dot(v1, v1);
			var d20 = Dot(v2, v0);
			var d21 = Dot(v2, v1);
			var denom = d00 * d11 - d01 * d01;
			var v = (d11 * d20 - d01 * d21) / denom;
			var w = (d00 * d21 - d01 * d20) / denom;
			var u = 1.0f - v - w;

			return new vec3(u, v, w);
		}

		public static float Angle(vec2 v)
		{
			return (float)Math.Atan2(v.y, v.x);
		}

		public static float Angle(vec2 p1, vec2 p2)
		{
			return Angle(p2 - p1);
		}

		// Note that this computes the angle between two 3D vectors, *not* between two 3D points (which doesn't
		// actually make sense without a reference vector).
		public static float Angle(vec3 v1, vec3 v2)
		{
			Debug.Assert(v1.Length > Constants.DefaultEpsilon && v2.Length > Constants.DefaultEpsilon, "3D angle " +
				"computation can't use vec3.Zero for either vector.");

			// Without this check, NaN can be returned.
			if (v1 == v2)
			{
				return 0;
			}

			// See https://www.analyzemath.com/stepbystep_mathworksheets/vectors/vector3D_angle.html.
			return (float)Math.Acos(Dot(v1, v2) / (v1.Length * v2.Length));
		}

		// This returns the shortest delta between the given angles (between -Pi and Pi).
		public static float Delta(float a1, float a2)
		{
			var delta = Math.Abs(a1 - a2);

			if (delta > Constants.Pi)
			{
				delta = -(Constants.TwoPi - delta);
			}

			return delta;
		}

		public static int Clamp(int v, int limit)
		{
			return Clamp(v, -limit, limit);
		}

		public static int Clamp(int v, int min, int max)
		{
			return v < min ? min : (v > max ? max : v);
		}

		public static float Clamp(float v, float limit)
		{
			return Clamp(v, -limit, limit);
		}

		public static float Clamp(float v, float min, float max)
		{
			return v < min ? min : (v > max ? max : v);
		}

		// This restricts the given angle to have an absolute value less than 2π.
		public static float RestrictAngle(float angle)
		{
			if (Math.Abs(angle) > Constants.TwoPi)
			{
				angle -= Constants.TwoPi * Math.Sign(angle);
			}

			return angle;
		}

		public static float Dot(vec2 v)
		{
			return v.x * v.x + v.y * v.y;
		}

		public static float Dot(vec2 v1, vec2 v2)
		{
			return vec2.Dot(v1, v2);
		}

		public static float Dot(vec3 v)
		{
			return v.x * v.x + v.y * v.y + v.z * v.z;
		}

		public static float Dot(vec3 v1, vec3 v2)
		{
			return vec3.Dot(v1, v2);
		}
		
		public static float ToDegrees(float radians)
		{
			return radians * 180 / Constants.Pi;
		}

		public static float ToRadians(float degrees)
		{
			return degrees * Constants.Pi / 180;
		}

		public static byte[] GetBytes<T>(T[] data) where T : struct
		{
			var size = Marshal.SizeOf(typeof(T)) * data.Length;
			var bytes = new byte[size];

			// See https://stackoverflow.com/a/4636735/7281613.
			Buffer.BlockCopy(data, 0, bytes, 0, size);

			return bytes;
		}

		public static vec2 Direction(float angle)
		{
			var x = (float)Math.Cos(angle);
			var y = (float)Math.Sin(angle);

			return new vec2(x, y);
		}

		public static ivec2 ComputeOrigin(int width, int height, Alignments alignment)
		{
			bool left = (alignment & Alignments.Left) > 0;
			bool right = (alignment & Alignments.Right) > 0;
			bool top = (alignment & Alignments.Top) > 0;
			bool bottom = (alignment & Alignments.Bottom) > 0;

			var x = left ? 0 : (right ? width : width / 2);
			var y = top ? 0 : (bottom ? height : height / 2);

			return new ivec2(x, y);
		}

		public static vec2 ParseVec2(string value)
		{
			// The expected format is "X|Y".
			var tokens = value.Split('|');

			Debug.Assert(tokens.Length == 2, "Wrong vec2 format (expected \"X|Y\").");

			var x = float.Parse(tokens[0]);
			var y = float.Parse(tokens[1]);

			return new vec2(x, y);
		}

		public static vec3 ParseVec3(string value)
		{
			// The expected format is "X|Y|Z".
			var tokens = value.Split('|');

			Debug.Assert(tokens.Length == 3, "Wrong vec3 format (expected \"X|Y|Z\").");

			var x = float.Parse(tokens[0]);
			var y = float.Parse(tokens[1]);
			var z = float.Parse(tokens[2]);

			return new vec3(x, y, z);
		}

		public static quat ParseQuat(string value)
		{
			// The expected format is "X|Y|Z|W".
			var tokens = value.Split('|');

			Debug.Assert(tokens.Length == 4, "Wrong quaternion format (expected \"X|Y|Z|W\").");

			var x = float.Parse(tokens[0]);
			var y = float.Parse(tokens[1]);
			var z = float.Parse(tokens[2]);
			var w = float.Parse(tokens[3]);

			return new quat(x, y, z, w);
		}

		public static Bounds2D ParseBounds(string value)
		{
			// The expected format is "X|Y|Width|Height".
			var tokens = value.Split('|');

			Debug.Assert(tokens.Length == 4, "Incorrect bounds format (expected \"X|Y|Width|Height\").");

			var x = int.Parse(tokens[0]);
			var y = int.Parse(tokens[1]);
			var width = int.Parse(tokens[2]);
			var height = int.Parse(tokens[3]);

			return new Bounds2D(x, y, width, height);
		}

		public static vec2 Project(vec2 v, vec2 onto)
		{
			// See https://math.oregonstate.edu/home/programs/undergrad/CalculusQuestStudyGuides/vcalc/dotprod/dotprod.html.
			return vec2.Dot(onto, v) / vec2.Dot(onto, onto) * onto;
		}

		public static vec3 Project(vec3 v, vec3 onto)
		{
			// See https://math.oregonstate.edu/home/programs/undergrad/CalculusQuestStudyGuides/vcalc/dotprod/dotprod.html.
			return vec3.Dot(onto, v) / vec3.Dot(onto, onto) * onto;
		}

		public static vec3 ProjectOntoPlane(vec3 v, vec3 n)
		{
			// See https://www.maplesoft.com/support/help/Maple/view.aspx?path=MathApps%2FProjectionOfVectorOntoPlane.
			return v - Project(v, n);
		}

		public static vec3 ProjectOntoPlane(vec3 p, vec3 planePoint, vec3 n)
		{
			return ProjectOntoPlane(p - planePoint, n);
		}

		public static vec2 Normalize(float x, float y)
		{
			return Normalize(new vec2(x, y));
		}

		public static vec2 Normalize(vec2 v)
		{
			if (v == vec2.Zero)
			{
				return vec2.Zero;
			}

			return v / Length(v);
		}

		public static vec3 Normalize(float x, float y, float z)
		{
			return Normalize(new vec3(x, y, z));
		}

		public static vec3 Normalize(vec3 v)
		{
			if (v == vec3.Zero)
			{
				return vec3.Zero;
			}

			return v / Length(v);
		}

		public static vec3 Average(List<vec3> list, bool shouldNormalize = false)
		{
			if (list.Count == 0)
			{
				return vec3.Zero;
			}

			var result = list.Aggregate(vec3.Zero, (current, v) => current + v) / list.Count;

			if (shouldNormalize)
			{
				result = Normalize(result);
			}

			return result;
		}

		public static vec3 ProjectedAverage(List<vec3> list, bool shouldNormalize = false)
		{
			if (list.Count == 0)
			{
				return vec3.Zero;
			}

			var result = vec3.Zero;

			for (int i = 0; i < list.Count; i++)
			{
				var v = list[i];
				result += v;

				for (int j = i; j < list.Count; j++)
				{
					list[j] -= Project(v, list[j]);
				}
			}

			if (shouldNormalize)
			{
				result = Normalize(result);
			}

			return result;
		}

		public static vec2 Rotate(vec2 v, float angle)
		{
			return angle == 0 ? v : RotationMatrix2D(angle) * v;
		}

		public static vec3 Rotate(vec3 v, float angle, vec3 axis)
		{
			Debug.Assert(axis != vec3.Zero, "Can't rotate around a vec3.Zero axis.");

			return angle == 0 ? v : quat.FromAxisAngle(angle, axis) * v;
		}

		public static mat2 RotationMatrix2D(float rotation)
		{
			var sin = (float)Math.Sin(rotation);
			var cos = (float)Math.Cos(rotation);

			return new mat2(cos, sin, -sin, cos);
		}

		public static vec3 Reflect(vec3 v, vec3 normal)
		{
			// See https://stackoverflow.com/a/35010966/7281613 (originally https://math.stackexchange.com/a/13263, but
			// after some frustrating digging, I'm pretty sure it's wrong).
			var d = Dot(v, normal);
			var result = 2 * d * normal - v;

			if (d < 0)
			{
				result *= -1;
			}

			return result;
		}

		public static vec3 Cross(vec3 v1, vec3 v2)
		{
			return vec3.Cross(v1, v2);
		}

		public static quat Orientation(vec3 v1, vec3 v2)
		{
			var dot = Dot(v1, v2);

			// See https://stackoverflow.com/a/1171995.
			if (Math.Abs(dot) > 0.99999f)
			{
				return quat.Identity;
			}

			var a = Cross(v1, v2);
			var w = (float)Math.Sqrt(LengthSquared(v1) * LengthSquared(v2)) + dot;
			
			return new quat(a.x, a.y, a.z, w).Normalized;
		}

		// This is useful for transforming 3D vectors onto the flat XZ plane.
		public static quat FlatProjection(vec3 normal)
		{
			if (normal == vec3.UnitY)
			{
				return quat.Identity;
			}

			var angle = Angle(normal, vec3.UnitY);
			var axis = Normalize(Cross(vec3.UnitY, normal));

			return quat.FromAxisAngle(angle, axis);
		}

		public static quat LookAt(vec3 eye, vec3 target)
		{
			// See https://stackoverflow.com/a/52551983/7281613.
			var q = new quat();
			var f = Normalize(eye - target);
			var r = Normalize(Cross(vec3.UnitY, f));
			var u = Cross(f, r);
			var trace = r.x + u.y + f.z;

			if (trace > 0)
			{
				var s = 0.5f / (float)Math.Sqrt(trace + 1);

				q.w = 0.25f / s;
				q.x = (u.z - f.y) * s;
				q.y = (f.x - r.z) * s;
				q.z = (r.y - u.x) * s;
			}
			else
			{
				if (r.x > u.y && r.x > f.z)
				{
					var s = 2 * (float)Math.Sqrt(1 + r.x - u.y - f.z);

					q.w = (u.z - f.y) / s;
					q.x = 0.25f * s;
					q.y = (u.x + r.y) / s;
					q.z = (f.x + r.z) / s;
				}
				else if (u.y > f.z)
				{
					var s = 2 * (float)Math.Sqrt(1 + u.y - r.x - f.z);

					q.w = (f.x - r.z) / s;
					q.x = (u.x + r.y) / s;
					q.y = 0.25f * s;
					q.z = (f.y + u.z) / s;
				}
				else
				{
					var s = 2 * (float)Math.Sqrt(1 + f.z - r.x - u.y);

					q.w = (r.y - u.x) / s;
					q.x = (f.x + r.z) / s;
					q.y = (f.y + u.z) / s;
					q.z = 0.25f * s;
				}
			}

			return q;
		}

		public static bool IsWithinTriangle(vec3 p, vec3[] triangle)
		{
			return IsWithinTriangle(p, triangle[0], triangle[1], triangle[2]);
		}

		public static bool IsWithinTriangle(vec3 p, vec3 p0, vec3 p1, vec3 p2)
		{
			var b = Barycentric(p, p0, p1, p2);

			return b.x >= 0 && b.x <= 1 && b.y >= 0 && b.y <= 1 && b.z >= 0 && b.z <= 1;
		}

		public static bool Intersects(vec3 p1, vec3 p2, vec3[] triangle, out float t)
		{
			return Intersects(p1, p2, triangle[0], triangle[1], triangle[2], out t);
		}

		public static bool Intersects(vec3 p1, vec3 p2, vec3 v0, vec3 v1, vec3 v2, out float t)
		{
			const float Epsilon = 0.0000001f;

			t = -1;

			// See https://en.m.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm.
			var e1 = v1 - v0;
			var e2 = v2 - v0;
			var ray = p2 - p1;
			var h = Cross(ray, e2);
			var a = Dot(e1, h);

			// This means the ray is parallel to the triangle.
			if (a > -Epsilon && a < Epsilon)
			{
				return false;
			}

			var f = 1 / a;
			var s = p1 - v0;
			var u = f * Dot(s, h);

			if (u < 0 || u > 1)
			{
				return false;
			}

			var q = Cross(s, e1);
			var v = f * Dot(ray, q);

			if (v < 0 || u + v > 1)
			{
				return false;
			}

			// At this point, we can determine where on the line the intersection point lies.
			var tPrime = f * Dot(e2, q);
			
			if (tPrime >= 0 && tPrime <= 1)
			{
				t = tPrime;

				return true;
			}

			// This means that there's a line intersection, but not a ray intersection.
			return false;
		}

		public static string[] WrapLines(string value, SpriteFont font, int width)
		{
			Debug.Assert(width >= font.Glyphs.Max(g => g.Width + g.Offset.x), "Line wrap width must be at least as " +
				"wide as the widest rendered glyph for the given font.");

			var lines = new List<string>();
			var words = value.Split(' ');
			var builder = new StringBuilder();
			var currentWidth = 0;
			var spaceWidth = font.Measure(" ").x;

			foreach (var word in words)
			{
				var wordWidth = font.Measure(word).x;

				if (currentWidth + wordWidth > width)
				{
					lines.Add(builder.ToString());
					builder.Clear();
					builder.Append(word + " ");
					currentWidth = wordWidth + spaceWidth;
				}
				else
				{
					currentWidth += wordWidth + spaceWidth;
					builder.Append(word + " ");
				}
			}

			if (builder.Length > 0)
			{
				lines.Add(builder.ToString());
			}

			return lines.ToArray();
		}
	}
}
