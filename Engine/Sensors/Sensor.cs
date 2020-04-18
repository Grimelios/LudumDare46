using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Shapes;

namespace Engine.Sensors
{
	public abstract class Sensor<TSensor, TShape, TShapeType, TSpace, TPosition> : IDisposable
		where TSensor: Sensor<TSensor, TShape, TShapeType, TSpace, TPosition>
		where TShape: Shape<TShape, TShapeType, TPosition>
		where TShapeType : struct
		where TSpace : Space<TSensor, TShape, TShapeType, TSpace, TPosition>
		where TPosition : struct
	{
		protected const string AssertMessage = "Can't modify a sensor mid-loop (this likely means that the sensor " +
			"was modified from a callback).";

		private bool isEnabled;
		private uint affects;
		private TPosition position;
		private TShape shape;

		protected Sensor(SensorTypes type, object owner, TShape shape, uint groups, uint affects, bool isCompound)
		{
			Type = type;
			Groups = groups;
			Affects = affects;

			// Note that sensor owner is allowed to be null (useful for manually creating debug sensors, but might be
			// applicable in finished games too).
			Owner = owner;
			Shape = shape;
			IsEnabled = true;
			IsCompound = isCompound;
			Contacts = new List<Sensor<TSensor, TShape, TShapeType, TSpace, TPosition>>();
		}

		internal bool IsTogglePending { get; private set; }
		internal bool IsCompound { get; }

		internal SensorTypes Type { get; }

		internal uint Groups { get; }

		// By design, sensor callbacks have an active nature (rather than passive). If one sensor affects another, the
		// first one's callbacks are triggered with data from the second (such that functions on the second sensor's
		// owner can be called as appropriate).
		public uint Affects
		{
			get => affects;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				affects = value;
			}
		}

		public object Owner { get; }

		public virtual TPosition Position
		{
			get => position;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				position = value;

				if (shape != null)
				{
					shape.Position = value;
				}
			}
		}

		public virtual TShape Shape
		{
			get => shape;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);
				Debug.Assert(value != null || !isEnabled, "Can't nullify the shape on an enabled sensor.");

				shape = value;

				if (shape != null)
				{
					shape.Position = position;
				}
			}
		}

		public TSpace Space { get; internal set; }

		public List<Sensor<TSensor, TShape, TShapeType, TSpace, TPosition>> Contacts { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnStay { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }

		public bool IsEnabled
		{
			get => isEnabled;
			set
			{
				// Disabling an active sensor is more complex than just swapping a flag (since existing contacts need
				// to be updated). As such, attempting to toggle a sensor mid-loop instead marks it to be changed later
				// (after the Space's main loop has finished).
				if (Space != null && Space.IsUpdateActive)
				{
					IsTogglePending = isEnabled != value;
				}
				else
				{
					if (isEnabled && !value)
					{
						ClearContacts();
					}

					isEnabled = value;
				}
			}
		}

		public bool TryGetContact<T>(out T target) where T : class
		{
			target = null;

			foreach (var contact in Contacts)
			{
				if (contact.Owner is T result)
				{
					target = result;

					// Only the first contact of the given type is returned.
					break;
				}
			}

			return target != null;
		}

		public virtual bool Overlaps(TSensor other)
		{
			return shape.Overlaps(other.shape);
		}

		public void Dispose()
		{
			ClearContacts();
		}

		internal void ClearContacts()
	    {
	        foreach (var other in Contacts)
	        {
				OnSeparate?.Invoke(other.Type, other.Owner);

		        other.OnSeparate?.Invoke(Type, Owner);
		        other.Contacts.Remove(this);
	        }

	        Contacts.Clear();
        }
	}
}
