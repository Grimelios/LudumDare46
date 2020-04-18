using static Engine.Graphics.GL;

namespace Engine.Graphics
{
	public static class GLUtilities
	{
		public static unsafe void GenerateBuffers(out uint bufferId, out uint indexId)
		{
			var buffers = new uint[2];

			fixed (uint* address = &buffers[0])
			{
				glGenBuffers(2, address);
			}

			bufferId = buffers[0];
			indexId = buffers[1];
		}

		public static unsafe void DeleteBuffers(uint bufferId, uint indexId)
		{
			uint[] buffers =
			{
				bufferId,
				indexId
			};

			fixed (uint* address = &buffers[0])
			{
				glDeleteBuffers(2, address);
			}
		}
	}
}
