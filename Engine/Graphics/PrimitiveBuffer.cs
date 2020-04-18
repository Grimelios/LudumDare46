using System;
using System.Linq;
using System.Runtime.InteropServices;
using static Engine.Graphics.GL;

namespace Engine.Graphics
{
	public class PrimitiveBuffer : IDisposable
	{
		private static readonly uint[] RestartModes =
		{
			GL_LINE_LOOP,
			GL_LINE_STRIP,
			GL_TRIANGLE_FAN,
			GL_TRIANGLE_STRIP
		};

		private uint usage;
		private uint bufferId;
		private uint indexId;
		private uint bufferSize;
		private uint indexCount;
		private uint allocatedBufferCapacity;
		private uint allocatedIndexCapacity;
		private int maxIndex;
		private byte[] buffer;
		private ushort[] indexBuffer;
		private bool isPrimitiveRestartEnabled;

		public PrimitiveBuffer(BufferFrequencies frequency, BufferUsages usage, out uint bufferId,
			out uint indexId, uint bufferCapacity = 0, uint indexCapacity = 0) :
			this(frequency, usage, bufferCapacity, indexCapacity)
		{
			bufferId = this.bufferId;
			indexId = this.indexId;
		}

		// Note that buffer capacity is given in bytes while index capacity is given in unsigned shorts.
		public unsafe PrimitiveBuffer(BufferFrequencies frequency, BufferUsages usage, uint bufferCapacity = 0,
			uint indexCapacity = 0)
		{
			this.usage = (uint)frequency + (uint)usage;

			GLUtilities.GenerateBuffers(out bufferId, out indexId);

			buffer = new byte[bufferCapacity];
			indexBuffer = new ushort[indexCapacity];
			maxIndex = -1;

			if (bufferCapacity > 0)
			{
				allocatedBufferCapacity = bufferCapacity;

				glBindBuffer(GL_ARRAY_BUFFER, bufferId);
				glBufferData(GL_ARRAY_BUFFER, bufferCapacity, (void*)0, this.usage);
			}

			if (indexCapacity > 0)
			{
				allocatedIndexCapacity = sizeof(ushort) * indexCapacity;

				glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
				glBufferData(GL_ELEMENT_ARRAY_BUFFER, allocatedIndexCapacity, (void*)0, this.usage);
			}
		}

		internal uint BufferId => bufferId;
		internal uint BufferSize => bufferSize;
		internal uint IndexId => indexId;
		internal uint IndexSize => sizeof(ushort) * indexCount;

		internal int BaseVertex => Math.Max(maxIndex, 0);

		internal uint Mode
		{
			set => isPrimitiveRestartEnabled = RestartModes.Contains(value);
		}

		public void Buffer<T>(T[] data, ushort[] indices, int start = 0, int length = -1) where T : struct
		{
			BufferData(data, start, length);
			BufferIndexes(indices, true);
		}

		// In some cases, data and indexes can be buffered separately (e.g. a liquid surface, where a single large
		// array of points is buffered, followed by many individual strips of indexes).
		public void BufferData<T>(T[] data, int start = 0, int length = -1)
		{
			var size = Marshal.SizeOf(typeof(T));
			var sizeInBytes = size * (length != -1 ? length : data.Length);
			var newSize = bufferSize + sizeInBytes;

			if (newSize > buffer.Length)
			{
				var newCapacity = buffer.Length == 0 ? 1 : buffer.Length * 2;

				while (newSize > newCapacity)
				{
					newCapacity *= 2;
				}

				var newBuffer = new byte[newCapacity];
				Array.Copy(buffer, newBuffer, bufferSize);
				buffer = newBuffer;
			}

			// See https://stackoverflow.com/a/4636735/7281613.
			System.Buffer.BlockCopy(data, start * size, buffer, (int)bufferSize, sizeInBytes);
			bufferSize += (uint)sizeInBytes;
		}

		// For some local array of indexes, the global max index (across all buffered vertices) should only be
		// recomputed if this particular index buffer is associated with a *new* set of vertices. If multiple sets of
		// indexes are buffered for some vertex array (e.g. rendering a liquid surface using multiple triangle strips),
		// only the first should recompute that maximum value.
		public void BufferIndexes(ushort[] indexes, bool shouldRecomputeMax)
		{
			var newCount = indexCount + indexes.Length;

			if (newCount > indexCount)
			{
				do
				{
					newCount *= 2;
				}
				while (newCount < indexCount);

				var newBuffer = new ushort[newCount];
				Array.Copy(indexBuffer, newBuffer, indexCount);
				indexBuffer = newBuffer;
			}

			if (shouldRecomputeMax)
			{
				var max = -1;

				for (int i = 0; i < indexes.Length; i++)
				{
					var index = indexes[i];

					indexBuffer[indexCount + i] = (ushort)(maxIndex + index + 1);
					max = Math.Max(max, index);
				}

				maxIndex += max + 1;
			}
			else
			{
				for (int i = 0; i < indexes.Length; i++)
				{
					indexBuffer[indexCount + i] = (ushort)(maxIndex + indexes[i] + 1);
				}
			}

			indexCount += (uint)indexes.Length;

			if (isPrimitiveRestartEnabled)
			{
				indexBuffer[indexCount] = Constants.PrimitiveRestartIndex;
				indexCount++;
			}
		}

		// This clears the buffer *without* issuing a draw call.
		public void Clear()
		{
			bufferSize = 0;
			indexCount = 0;
			maxIndex = -1;
		}

		public void Dispose()
		{
			GLUtilities.DeleteBuffers(bufferId, indexId);
		}

		public unsafe uint Flush()
		{
			if (isPrimitiveRestartEnabled)
			{
				glEnable(GL_PRIMITIVE_RESTART);
			}
			else
			{
				glDisable(GL_PRIMITIVE_RESTART);
			}

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (byte* address = &buffer[0])
			{
				if (bufferSize > allocatedBufferCapacity)
				{
					allocatedBufferCapacity = (uint)buffer.Length;
					glBufferData(GL_ARRAY_BUFFER, allocatedBufferCapacity, address, usage);
				}
				else
				{
					glBufferSubData(GL_ARRAY_BUFFER, 0, bufferSize, address);
				}
			}

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);

			fixed (ushort* address = &indexBuffer[0])
			{
				var capacity = sizeof(ushort) * indexBuffer.Length;

				if (capacity > allocatedIndexCapacity)
				{
					allocatedIndexCapacity = (uint)capacity;
					glBufferData(GL_ELEMENT_ARRAY_BUFFER, allocatedIndexCapacity, address, usage);
				}
				else
				{
					glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, 0, sizeof(ushort) * indexCount, address);
				}
			}

			var count = indexCount;

			Clear();

			return count;
		}
	}
}
