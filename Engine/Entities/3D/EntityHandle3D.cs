using Engine.Physics._3D;
using Engine.Sensors._3D;
using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Entities._3D
{
	public class EntityHandle3D<TGroup> : EntityHandle<Entity3D<TGroup>, EntityHandle3D<TGroup>, TGroup,
		Scene3D<TGroup>, World3D, Sensor3D, Shape3D, ShapeTypes3D, Space3D, vec3> where TGroup : struct
	{
	}
}
