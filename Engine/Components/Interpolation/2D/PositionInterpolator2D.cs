using System.Diagnostics;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.Components.Interpolation._2D
{
	public class PositionInterpolator2D : Interpolator<vec2>
	{
		private IPositionable2D target;
		private IRenderPositionable2D renderTarget;
		private IPositionContainer2D containerTarget;

		public PositionInterpolator2D(IPositionable2D target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, vec2.Zero, vec2.Zero, easeType, 0, isRepeatable)
		{
		}

		public PositionInterpolator2D(IRenderPositionable2D target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, vec2.Zero, vec2.Zero, easeType, 0, isRepeatable)
		{
		}

		public PositionInterpolator2D(IPositionContainer2D target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, vec2.Zero, vec2.Zero, easeType, 0, isRepeatable)
		{
		}

		public PositionInterpolator2D(IPositionable2D target, vec2 start, vec2 end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			this.target = target;
		}

		public PositionInterpolator2D(IRenderPositionable2D target, vec2 start, vec2 end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			renderTarget = target;
		}

		public PositionInterpolator2D(IPositionContainer2D target, vec2 start, vec2 end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			containerTarget = target;
		}

		protected override void Lerp(float t)
		{
			var p = vec2.Lerp(Start, End, t);

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
