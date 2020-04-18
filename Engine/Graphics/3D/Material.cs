using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics._3D
{
	public class Material
	{
		public Material(float specular)
		{
			Specular = specular;
		}

		public float Specular { get; }
	}
}
