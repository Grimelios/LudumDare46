using System.Diagnostics;
using Engine.Core;
using Engine.Interfaces;

namespace Engine.Components.Interpolation
{
	public class ColorInterpolator : Interpolator<Color>
	{
		private IColorable target;
		private IRenderColorable renderTarget;
		private IColorContainer containerTarget;

		public ColorInterpolator(IColorable target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, Color.White, Color.White, easeType, 0, isRepeatable)
		{
		}

		public ColorInterpolator(IRenderColorable target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, Color.White, Color.White, easeType, 0, isRepeatable)
		{
		}

		public ColorInterpolator(IColorContainer target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, Color.White, Color.White, easeType, 0, isRepeatable)
		{
		}

		public ColorInterpolator(IColorable target, Color start, Color end, EaseTypes easeType, float duration = 0,
			bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			this.target = target;
		}

		public ColorInterpolator(IRenderColorable target, Color start, Color end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			renderTarget = target;
		}

		public ColorInterpolator(IColorContainer target, Color start, Color end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			containerTarget = target;
		}

		protected override void Lerp(float t)
		{
			var color = Color.Lerp(Start, End, t);

			if (target != null)
			{
				target.Color = color;
			}
			else if (renderTarget != null)
			{
				renderTarget.Color.SetValue(color, true);
			}
			else
			{
				containerTarget.SetColor(color, true);
			}
		}
	}
}
