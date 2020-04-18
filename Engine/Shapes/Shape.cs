using Engine.Core;

namespace Engine.Shapes
{
	public abstract class Shape<TShape, TType, TPosition>
		where TShape : Shape<TShape, TType, TPosition>
		where TType : struct
		where TPosition : struct
	{
		protected TPosition position;

		protected Shape(TType type)
		{
			Type = type;
		}

		public TType Type { get; }

		public TPosition Position
		{
			get => position;
			set => position = value;
		}

		// This function is meant to contain the actual logic for each shape (using a relative position). The public
		// version below converts the point to relative coordinates if needed.
		protected abstract bool Contains(TPosition p);

		public abstract bool Contains(TPosition p, Coordinates coords);
		public abstract bool Overlaps(TShape other);
	}
}
