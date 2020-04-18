using System;

namespace Engine.Core
{
	public abstract class RenderField<T> where T : struct
	{
		private TransformFlags setFlag;
		private TransformFlags interpolationFlag;
		private Flags<TransformFlags> flags;

		protected RenderField(Flags<TransformFlags> flags, TransformFlags setFlag, TransformFlags interpolationFlag,
			T initialValue)
		{
			this.flags = flags;
			this.setFlag = setFlag;
			this.interpolationFlag = interpolationFlag;

			Value = initialValue;
			OldValue = initialValue;
		}

		public T Value { get; private set; }
		public T OldValue { get; private set; }
		public T ResultValue { get; private set; }

		public void SetValue(T value, bool shouldInterpolate)
		{
			var v = flags.Value;

			// On the first frame of the field's existence, even if interpolation is enabled, the old value is snapped
			// to the current one (in order to prevent an erroneously huge delta from the origin).
			if ((v & setFlag) == 0)
			{
				OldValue = value;
				v |= setFlag;
			}

			// Interpolation is controlled by the final call to this function (rather than updating the old value each
			// call). This was done to account for situations where an interpolated field's value is set multiple
			// times on a single frame (which effectively negates interpolation if the value is the same).
			Value = value;

			if (shouldInterpolate)
			{
				v |= interpolationFlag;
			}
			else
			{
				v &= ~interpolationFlag;
			}

			flags.Value = v;
		}

		protected abstract T Interpolate(float t);

		public T Evaluate(float t)
		{
			// The interpolation flag is intentionally left untouched here. As a result, a field's interpolation flag
			// will only change when calling SetValue. This is done to account for the (unlikely, but possible)
			// situation when multiple render calls occur between update ticks (in which case interpolation should be
			// applied multiple times).
			if ((flags.Value & interpolationFlag) == 0)
			{
				OldValue = Value;
				ResultValue = Value;
			}
			else
			{
				ResultValue = Interpolate(t);
				OldValue = Value;
			}

			return ResultValue;
		}
	}
}
