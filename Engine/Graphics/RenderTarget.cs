using System;
using System.Diagnostics;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics
{
	public class RenderTarget : QuadSource
	{
		private uint framebufferId;
		private uint renderbufferId;
		private uint clearBits;

		public RenderTarget(ivec2 dimensions, RenderTargetFlags flags, TextureWraps wrap = TextureWraps.Repeat,
			TextureFilters filter = TextureFilters.Nearest) :
			this(dimensions.x, dimensions.y, flags, wrap, filter)
		{
		}

		public unsafe RenderTarget(int width, int height, RenderTargetFlags flags,
			TextureWraps wrap = TextureWraps.Repeat, TextureFilters filter = TextureFilters.Nearest) :
			base(width, height)
		{
			fixed (uint* address = &framebufferId)
			{
				glGenFramebuffers(1, address);
			}

			glBindFramebuffer(GL_FRAMEBUFFER, framebufferId);

			uint id;

			glGenTextures(1, &id);
			glBindTexture(GL_TEXTURE_2D, id);
			Id = id;

			var isColorEnabled = (flags & RenderTargetFlags.Color) > 0;
			var isDepthEnabled = (flags & RenderTargetFlags.Depth) > 0;
			var isStencilEnabled = (flags & RenderTargetFlags.Stencil) > 0;

			uint texFormat;
			uint texType;
			uint texAttachment;

			if (isColorEnabled)
			{
				texFormat = GL_RGBA;
				texType = GL_UNSIGNED_BYTE;
				texAttachment = GL_COLOR_ATTACHMENT0;
			}
			else
			{
				texFormat = GL_DEPTH_COMPONENT; 
				texType = GL_FLOAT;
				texAttachment = GL_DEPTH_ATTACHMENT;
			}

			glTexImage2D(GL_TEXTURE_2D, 0, (int)texFormat, (uint)width, (uint)height, 0, texFormat, texType, null);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)filter);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)filter);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int)wrap);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int)wrap);
			glFramebufferTexture2D(GL_FRAMEBUFFER, texAttachment, GL_TEXTURE_2D, id, 0);

			if (!isColorEnabled)
			{
				glDrawBuffer(GL_NONE);
				glReadBuffer(GL_NONE);
			}
			else if (isDepthEnabled)
			{
				var format = (uint)(isStencilEnabled ? GL_DEPTH24_STENCIL8 : GL_DEPTH_COMPONENT);
				var attachment = (uint)(isStencilEnabled ? GL_DEPTH_STENCIL_ATTACHMENT : GL_DEPTH_ATTACHMENT);

				fixed (uint* address = &renderbufferId)
				{
					glGenRenderbuffers(1, address);
				}

				glBindRenderbuffer(GL_RENDERBUFFER, renderbufferId);
				glRenderbufferStorage(GL_RENDERBUFFER, format, (uint)width, (uint)height);
				glFramebufferRenderbuffer(GL_FRAMEBUFFER, attachment, GL_RENDERBUFFER, renderbufferId);
				glBindRenderbuffer(GL_RENDERBUFFER, 0);
			}

			if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
			{
				Debug.Fail("Error creating render target.");
			}

			glBindFramebuffer(GL_FRAMEBUFFER, 0);

			if (isColorEnabled)
			{
				clearBits |= GL_COLOR_BUFFER_BIT;
			}

			if (isDepthEnabled)
			{
				clearBits |= GL_DEPTH_BUFFER_BIT;
			}

			if (isStencilEnabled)
			{
				clearBits |= GL_STENCIL_BUFFER_BIT;
			}
		}

		public override unsafe void Dispose()
		{
			fixed (uint* address = &framebufferId)
			{
				glDeleteFramebuffers(1, address);
			}

			fixed (uint* address = &renderbufferId)
			{
				glDeleteRenderbuffers(1, address);
			}

			base.Dispose();
		}

		public void Apply(bool shouldClear = true)
		{
			glBindFramebuffer(GL_FRAMEBUFFER, framebufferId);
			glViewport(0, 0, (uint)Width, (uint)Height);

			if (shouldClear)
			{
				glClear(clearBits);
			}
		}
	}
}
