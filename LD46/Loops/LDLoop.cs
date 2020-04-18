using Engine.Loops;

namespace LD46.Loops
{
	public abstract class LDLoop : GameLoop2D<LoopTypes>
	{
		protected LDLoop() : base(LoopTypes.Gameplay)
		{
		}
	}
}
