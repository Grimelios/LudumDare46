using Engine.Content;
using Engine.Graphics._3D;

namespace Engine.Core._3D
{
	public class Model : MeshUser
	{
		public Model(string filename) : this(ContentCache.GetMesh(filename))
		{
		}

		public Model(Mesh mesh) : base(mesh)
		{
		}
	}
}
