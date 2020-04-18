
namespace Engine.Interfaces
{
	public interface IComponent : IDynamic
	{
		bool IsComplete { get; }
		bool IsEnabled { get; }

		void Reset();
	}
}
