using Engine.Interfaces;
using Engine.Props;

namespace Engine.View
{
	public abstract class CameraView<TCamera, TView> : IDynamic, IReloadable
		where TCamera : Camera<TCamera, TView>
		where TView : CameraView<TCamera, TView>
	{
		protected CameraView()
		{
			Properties.Access(this);
		}

		public TCamera Camera { get; internal set; }

		internal virtual void OnActivate()
		{
		}

		internal virtual void OnDeactivate()
		{
		}

		public virtual bool Reload(PropertyAccessor accessor, out string message)
		{
			message = null;

			return true;
		}

		public virtual void Dispose()
		{
		}

		public virtual void Update()
		{
		}
	}
}
