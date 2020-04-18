using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Messaging;
using Engine.Utility;
using GlmSharp;

namespace Engine.UI
{
	public class Canvas : IDynamic, IRenderTargetUser2D, IReceiver
	{
		// Using a list (rather than a set) makes removing elements easier after they're disposed.
		private List<CanvasElement> elements;
		private HashSet<IRenderTargetUser2D> renderTargetUsers;

		public Canvas()
		{
			elements = new List<CanvasElement>();
			renderTargetUsers = new HashSet<IRenderTargetUser2D>();
			IsDrawEnabled = true;

			MessageSystem.Subscribe(this, (int)CoreMessageTypes.ResizeWindow, data =>
			{
				elements.ForEach(e =>
				{
					if (e.Anchor != Anchors.None)
					{
						e.SetPosition(ComputePlacement(e), false);
					}
				});
			});
		}

		// When child elements are attached to other elements, those children need to be added as render targets if
		// applicable.
		internal HashSet<IRenderTargetUser2D> RenderTargetUsers => renderTargetUsers;

		public List<MessageHandle> MessageHandles { get; set; }

		public bool IsDrawEnabled { get; set; }

		public void Load(string filename, bool shouldClear = false)
		{
			if (shouldClear)
			{
				Clear();
			}

			foreach (var element in JsonUtilities.Deserialize<CanvasElement[]>(filename, true))
			{
				Add(element);
			}
		}

		public T Add<T>(T element) where T : CanvasElement
		{
			Debug.Assert((element.flags & CanvasElementFlags.IsChild) == 0, "Can't add a child element to the " +
				"canvas directly (the parent should manage its own children instead).");

			var anchor = element.Anchor;

			if (anchor != Anchors.None)
			{
				element.SetPosition(ComputePlacement(element), false);
			}

			// This flag safeguards against the possible mistake of initializing an element from within rather than by
			// adding it to the canvas.
			element.flags |= CanvasElementFlags.IsAdded;
			element.flags &= ~CanvasElementFlags.IsRemoved;
			element.Initialize(this);

			elements.Add(element);

			return element;
		}

		public void Remove(CanvasElement element)
		{
			var flags = element.flags;

			Debug.Assert(element != null, "Can't remove a null canvas element.");
			Debug.Assert((flags & CanvasElementFlags.IsChild) == 0, "Can't remove a child element from the canvas " +
				"directly (since children are managed by the parent).");
			Debug.Assert((flags & CanvasElementFlags.IsRemoved) == 0, "Canvas element was already removed.");
			Debug.Assert((flags & CanvasElementFlags.IsAdded) > 0, "Element was never added to the canvas.");

			element.flags &= ~CanvasElementFlags.IsAdded;
			element.flags |= CanvasElementFlags.IsRemoved;
			element.Dispose();

			elements.Remove(element);

			if (element is IRenderTargetUser2D user)
			{
				renderTargetUsers.Remove(user);
			}
		}

		public void Clear()
		{
			elements.ForEach(e => e.Dispose());
			elements.Clear();
		}

		public void Dispose()
		{
			elements.ForEach(e => e.Dispose());

			MessageSystem.Unsubscribe(this);
		}

		public T GetElement<T>() where T : CanvasElement
		{
			return elements.OfType<T>().First();
		}

		// This function is only called if an anchor was set (the anchor can be None for free-moving elements that
		// aren't locked to a particular anchor location).
		private vec2 ComputePlacement(CanvasElement element)
		{
			var anchor = element.Anchor;

			bool left = (anchor & Anchors.Left) > 0;
			bool right = (anchor & Anchors.Right) > 0;
			bool top = (anchor & Anchors.Top) > 0;
			bool bottom = (anchor & Anchors.Bottom) > 0;

			var dimensions = Resolution.WindowDimensions;
			var offset = element.Offset;
			var width = dimensions.x;
			var height = dimensions.y;
			var x = left ? offset.x : (right ? width - offset.x : width / 2 + offset.x);
			var y = top ? offset.y : (bottom ? height - offset.y : height / 2 + offset.y);

			return new vec2(x, y);
		}

		public void Update()
		{
			if (!IsDrawEnabled)
			{
				return;
			}

			for (int i = elements.Count - 1; i >= 0; i--)
			{
				var element = elements[i];

				if (element.IsDrawEnabled)
				{
					element.Update();
				}
			}
		}

		public void DrawTargets(SpriteBatch sb, float t)
		{
			if (!IsDrawEnabled)
			{
				return;
			}

			foreach (var user in renderTargetUsers)
			{
				if (user.IsDrawEnabled)
				{
					user.DrawTargets(sb, t);
				}
			}
		}

		public void Draw(SpriteBatch sb, float t)
		{
			if (!IsDrawEnabled)
			{
				return;
			}

			foreach (var element in elements)
			{
				if (element.IsDrawEnabled)
				{
					element.Draw(sb, t);
				}
			}
		}
	}
}
