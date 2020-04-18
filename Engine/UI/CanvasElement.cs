using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Components;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Input;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Messaging;
using Engine.Props;
using GlmSharp;

namespace Engine.UI
{
	public abstract class CanvasElement : IPositionContainer2D, IReloadable, IDynamic
	{
		private Canvas canvas;
		private List<(Component2D Target, ivec2 Location)> childComponents;
		private List<(CanvasElement Element, ivec2 Location)> childElements;
		private ComponentCollection components;

		internal CanvasElementFlags flags;

		protected CanvasElement()
		{
			flags = CanvasElementFlags.None;
			IsDrawEnabled = true;
		}

		// When an element is added, all children are recursively traversed to determine if any of them are render
		// target users.
		internal IEnumerable<CanvasElement> ChildElements => childElements?.Select(e => e.Element);

		protected Canvas Canvas => canvas;
		protected ComponentCollection Components => components ??= new ComponentCollection();

		public vec2 Position { get; set; }
		public ivec2 Offset { get; set; }

		public bool IsDrawEnabled
		{
			get => (flags & CanvasElementFlags.IsDrawEnabled) > 0;
			set
			{
				if (value)
				{
					flags |= CanvasElementFlags.IsDrawEnabled;
				}
				else
				{
					flags &= ~CanvasElementFlags.IsDrawEnabled;
				}
			}
		}

		public Anchors Anchor { get; set; }

		protected internal virtual void Initialize(Canvas canvas)
		{
			Debug.Assert((flags & CanvasElementFlags.IsAdded) > 0, "Canvas elements must be initialized by adding " +
				"to the canvas (don't call Initialize directly).");
			Debug.Assert((flags & CanvasElementFlags.IsInitialized) == 0, "Canvas element was already initialized.");
			Debug.Assert(canvas != null, "Canvas elements must be initialized with a non-null canvas.");

			this.canvas = canvas;

			// By design, child elements can't be added if they were already initialized.
			childElements?.ForEach(e => e.Element.Initialize(canvas));

			// Having elements add themselves as render target users ensures that all nested children are added
			// recursively (due to the recursive call to initialize children above).
			if (this is IRenderTargetUser2D user)
			{
				canvas.RenderTargetUsers.Add(user);
			}

			flags |= CanvasElementFlags.IsInitialized;

			Properties.Access(this);
		}

		// This function is a bit of a hack to allow attachment of both components and sub-elements using a single
		// function signature (while still returning the concrete type directly).
		protected T Attach<T>(T item, ivec2? location = null) where T : IRenderable2D
		{
			Debug.Assert(item != null, "Can't attach a null item to canvas elements.");
			Debug.Assert(item is Component2D || item is CanvasElement, "Invalid attachment on canvas elements (must " +
				"be either a 2D component or another element).");

			switch (item)
			{
				case Component2D component:
					AttachComponent(component, location);
					break;

				case CanvasElement element:
					AttachElement(element, location);
					break;
			}

			return item;
		}

		private void AttachComponent<T>(T component, ivec2? location = null) where T : Component2D
		{
			var p = location ?? ivec2.Zero;

			if (childComponents == null)
			{
				childComponents = new List<(Component2D Target, ivec2 Location)>();
			}

			childComponents.Add((component, p));
			component.Position.SetValue(Position + p, false);
		}

		private void AttachElement<T>(T element, ivec2? location = null) where T : CanvasElement
		{
			Debug.Assert((element.flags & CanvasElementFlags.IsAdded) == 0, "Cannot add a child element that " +
				"has already been added to the canvas.");

			var p = location ?? ivec2.Zero;

			if (childElements == null)
			{
				childElements = new List<(CanvasElement Element, ivec2 Location)>();
			}

			childElements.Add((element, p));

			element.flags |= CanvasElementFlags.IsChild;
			element.SetPosition(Position + p, false);

			// This is equivalent to checking whether the current element is initialized (not the child).
			if (canvas != null)
			{
				element.Initialize(canvas);

				if (element is IRenderTargetUser2D user)
				{
					canvas.RenderTargetUsers.Add(user);
				}
			}
		}

		// This is useful for repositioning attachments based on property reload.
		protected void Relocate(int index, ivec2 location)
		{
			if (childComponents != null && index < childComponents.Count)
			{
				var target = childComponents[index].Target;
				childComponents[index] = (target, location);
				target.Position.SetValue(Position + location, false);

				return;
			}

			if (childElements != null)
			{
				// A single index can be used to relocate either components or elements.
				index -= childComponents?.Count ?? 0;

				var element = childElements[index].Element;
				childElements[index] = (element, location);
				element.SetPosition(Position + location, false);
			}
		}

		public virtual void SetPosition(vec2 position, bool shouldInterpolate)
		{
			Position = position;
			childComponents?.ForEach(c => c.Target.Position.SetValue(position + c.Location, shouldInterpolate));
			childElements?.ForEach(e => e.Element.SetPosition(position + e.Location, shouldInterpolate));
		}

		public virtual bool Reload(PropertyAccessor accessor, out string message)
		{
			message = null;

			return true;
		}

		public virtual void Dispose()
		{
			Debug.Assert((flags & CanvasElementFlags.IsAdded) == 0, "Since this element was added to the canvas, it " +
				"should be disposed by calling Canvas.Remove (rather than by calling Dispose directly).");
			Debug.Assert((flags & CanvasElementFlags.IsDisposed) == 0, "Canvas element was already disposed.");

			if (this is IControllable target)
			{
				InputProcessor.Remove(target);
			}

			if (this is IReceiver receiver)
			{
				MessageSystem.Unsubscribe(receiver);
			}

			flags |= CanvasElementFlags.IsDisposed;
		}

		public virtual void Update()
		{
			Debug.Assert((flags & CanvasElementFlags.IsInitialized) > 0, "Canvas element was never initialized.");

			if (!IsDrawEnabled)
			{
				return;
			}

			components?.Update();
			childElements?.ForEach(e => e.Element.Update());
		}

		public virtual void Draw(SpriteBatch sb, float t)
		{
			Debug.Assert((flags & CanvasElementFlags.IsInitialized) > 0, "Canvas element was never initialized.");

			if (!IsDrawEnabled)
			{
				return;
			}

			childComponents?.ForEach(c => c.Target.Draw(sb, t));
			childElements?.ForEach(e => e.Element.Draw(sb, t));
		}
	}
}
