using System.Collections.Generic;
using System.Diagnostics;
using Engine.Input.Data;
using Engine.Utility;
using static Engine.GLFW;

namespace Engine.Input
{
	// TODO: If menu items can move, might need special handling if that item moves over a stationary cursor.
	public class ClickableSet<T> where T : class, IClickable
	{
		private T hoveredItem;
		private T clickedItem;
		private IEnumerable<T> items;

		private int buttonUsed;

		// Clickable sets are meant to use the same collection as the parent object (such that changes to that
		// collection are immediately processed by the set).
		public ClickableSet(IEnumerable<T> items)
		{
			this.items = items;
		}

		// This is useful for accessibility.
		public bool UseRightClickSelection { get; set; }

		public T HoveredItem => hoveredItem;
		public T ClickedItem => clickedItem;

		// Mouse submissions (i.e. clicking an item) takes priority over submission via other input methods (like
		// pressing enter on a keyboard).
		public ClickFlags ProcessMouse(MouseData data)
		{
			var location = data.Location;
			var flags = ClickFlags.None;

			// Hovering only counts if the mouse also moved this frame (useful for prioritizing conflicting selection
			// changes from different devices, e.g. between keyboard and mouse).
			if (Utilities.LengthSquared(data.Delta) > 0)
			{
				if (hoveredItem != null && !hoveredItem.Contains(location))
				{
					hoveredItem.OnUnhover();
					hoveredItem = null;
				}

				// No new items can be hovered while click is still held. This means that if the player clicks an item,
				// then drags to a new one (without releasing the mouse), that new item won't react until the mouse is
				// released. The mouse movement requirement above still applies, so the new item *actually* won't react
				// until the mouse is released *and* moved.
				if (clickedItem == null)
				{
					foreach (T item in items)
					{
						if (item.IsEnabled && item != hoveredItem && item.Contains(location))
						{
							// It's possible for the mouse to move between items in a single frame.
							hoveredItem?.OnUnhover();
							hoveredItem = item;
							hoveredItem.OnHover(location);

							flags |= ClickFlags.WasHovered;

							break;
						}
					}
				}
			}

			if (clickedItem != null && data.Query(buttonUsed, InputStates.ReleasedThisFrame))
			{
				// This function is still called even if the mouse is no longer hovering over the clicked item.
				clickedItem.OnRelease();
				clickedItem = null;

				return flags;
			}

			if (hoveredItem == null)
			{
				return flags;
			}

			// It's also possible for the mouse to move to a new item and click on the same frame (should be rare, but
			// would be considered a valid click for something like a TAS).
			var button = UseRightClickSelection ? GLFW_MOUSE_BUTTON_RIGHT : GLFW_MOUSE_BUTTON_LEFT;

			if (data.Query(button, InputStates.PressedThisFrame))
			{
				clickedItem = hoveredItem;
				clickedItem.OnClick(location);
				buttonUsed = button;

				flags |= ClickFlags.WasClicked;
			}

			return flags;
		}

		public void OnDisable(T item)
		{
			if (item == clickedItem)
			{
				item.OnRelease();
				clickedItem = null;
			}

			if (item == hoveredItem)
			{
				item.OnUnhover();
				hoveredItem = null;
			}
		}
	}
}
