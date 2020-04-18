using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Core
{
	// See https://stackoverflow.com/a/43379682.
	public class Primitive<T> where T : struct
	{
		public T Value { get; set; }

		public Primitive() : this(default)
		{
		}

		public Primitive(T value)
		{
			Value = value;
		}

		public static implicit operator T(Primitive<T> p)
		{
			return p.Value;
		}

		public static implicit operator Primitive<T>(T value)
		{
			return new Primitive<T>(value);
		}
	}
}
