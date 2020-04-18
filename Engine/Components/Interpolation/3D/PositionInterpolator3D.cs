using System.Diagnostics;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Components.Interpolation._3D
{
	public class PositionInterpolator3D : Interpolator<vec3>
	{
		private IPositionable3D target;
		private IRenderPositionable3D renderTarget;
		private IPositionContainer3D containerTarget;

		public PositionInterpolator3D(IPositionable3D target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, vec3.Zero, vec3.Zero, easeType, 0, isRepeatable)
		{
		}

		public PositionInterpolator3D(IRenderPositionable3D target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, vec3.Zero, vec3.Zero, easeType, 0, isRepeatable)
		{
		}

		public PositionInterpolator3D(IPositionContainer3D target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, vec3.Zero, vec3.Zero, easeType, 0, isRepeatable)
		{
		}

		public PositionInterpolator3D(IPositionable3D target, vec3 start, vec3 end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			this.target = target;
		}

		public PositionInterpolator3D(IRenderPositionable3D target, vec3 start, vec3 end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			renderTarget = target;
		}

		public PositionInterpolator3D(IPositionContainer3D target, vec3 start, vec3 end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			containerTarget = target;
		}

		protected override void Lerp(float t)
		{
			var p = vec3.Lerp(Start, End, t);

			if (target != null)
			{
				target.Position = p;
			}
			else if (renderTarget != null)
			{
				renderTarget.Position.SetValue(p, true);
			}
			else
			{
				containerTarget.SetPosition(p, true);
			}
		}
	}
}
