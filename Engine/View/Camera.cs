using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Interfaces;
using Engine.Messaging;

namespace Engine.View
{
	// TODO: Consider making cameras reloadable.
	public abstract class Camera<TCamera, TView> : IReceiver, IDynamic
		where TCamera : Camera<TCamera, TView>
		where TView : CameraView<TCamera, TView>
	{
		private List<TView> views;
		private TView activeView;

		protected Camera()
		{
			views = new List<TView>();
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public TView ActiveView => activeView;

		public T GetView<T>() where T : TView
		{
			return views.OfType<T>().First();
		}

		public T Attach<T>(T view, bool shouldActivate = true) where T : TView
		{
			Debug.Assert(!views.OfType<T>().Any(), "Can't attach multiple camera views of the same type.");
			Debug.Assert(!views.Contains(view), "Duplicate camera view attached.");

			views.Add(view);
			view.Camera = (TCamera)this;

			if (shouldActivate)
			{
				Activate(view);
			}

			return view;
		}

		public T Activate<T>() where T : TView
		{
			var view = GetView<T>();

			// This means that a view with the given type is already active (meaning that nothing else has to be done).
			if (view == activeView)
			{
				return view;
			}

			Activate(view);

			return view;
		}

		public T Activate<T>(T view) where T : TView
		{
			// It's valid to activate the same view again (although nothing will happen).
			if (view == activeView)
			{
				return view;
			}

			// As a useful shorthand, if the given controller isn't attached, it's attached here instead (before being
			// activated).
			if (view != null && !views.Contains(view))
			{
				views.Add(view);
				view.Camera = (TCamera)this;
			}

			// Passing null effectively disables all views.
			activeView?.OnDeactivate();
			activeView = view;
			activeView?.OnActivate();

			return view;
		}

		// This version is useful when temporarily swapping to a new view (such as enabling the freecam via terminal,
		// which has to restore the old view once disabled).
		public T Activate<T>(T controller, out TView oldView) where T : TView
		{
			oldView = activeView;

			return Activate(controller);
		}

		public void Remove(TView view, bool shouldDispose = false)
		{
			Debug.Assert(views.Contains(view), "Given view isn't attached to the camera.");
			Debug.Assert(view != activeView, "Can't remove an active camera view.");

			views.Remove(view);

			if (shouldDispose)
			{
				view.Dispose();
			}
		}

		public void Dispose()
		{
			views.ForEach(c => c.Dispose());

			MessageSystem.Unsubscribe(this);
		}

		public void Update()
		{
			activeView?.Update();
		}
	}
}
