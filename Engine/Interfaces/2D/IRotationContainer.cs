using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interfaces._2D
{
	public interface IRotationContainer
	{
		float Rotation { get; }

		void SetRotation(float rotation, bool shouldInterpolate);
	}
}
