using System;

namespace Engine.Graphics._3D
{
	internal class MeshHandle
	{
		public MeshHandle(int count, int offset, int baseVertex)
		{
			Count = count;
			Offset = offset;
			BaseVertex = baseVertex;
		}

		public int Count { get; }
		public int Offset { get; }
		public int BaseVertex { get; }
	}
}
