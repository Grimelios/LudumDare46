using System.Diagnostics;
using Engine.Graphics._3D.Rendering;
using Engine.Physics._3D;
using Engine.Sensors._3D;
using Engine.Shapes._3D;
using Engine.UI;
using GlmSharp;

namespace Engine.Entities._3D
{ 
	public abstract class Scene3D<TGroup> : Scene<Entity3D<TGroup>, EntityHandle3D<TGroup>, TGroup, Scene3D<TGroup>,
		World3D, Sensor3D, Shape3D, ShapeTypes3D, Space3D, vec3> where TGroup : struct
	{
		protected Scene3D(MasterRenderer3D renderer, Canvas canvas, World3D world, Space3D space) :
			base(canvas, world, space)
		{
			Debug.Assert(renderer != null, "3D scenes must be created with a non-null renderer.");

			Renderer = renderer;
		}

		public MasterRenderer3D Renderer { get; }
	}
}
