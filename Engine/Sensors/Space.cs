using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Interfaces;
using Engine.Shapes;
using Engine.Structures;

namespace Engine.Sensors
{
	public abstract class Space<TSensor, TShape, TShapeType, TSpace, TPosition> : IDynamic
		where TSensor : Sensor<TSensor, TShape, TShapeType, TSpace, TPosition>
		//where TMulti : MultiSensor<TSensor, TShape, TShapeType, TSpace, TPosition>
		where TShape : Shape<TShape, TShapeType, TPosition>
		where TShapeType : struct
		where TSpace : Space<TSensor, TShape, TShapeType, TSpace, TPosition>
		where TPosition : struct
	{
		private SafeList<TSensor> sensors;

		protected Space()
		{
			sensors = new SafeList<TSensor>();
		}

		// If the update function is currently active, sensors can't be disabled or removed directly. Instead, they're
		// marked to be toggled or destroyed following the loop.
		internal bool IsUpdateActive { get; private set; }

		// This is useful for visualizers.
		internal List<TSensor> Sensors => sensors.MainList;

		public void Add(TSensor sensor)
		{
			sensors.Add(sensor);
			sensor.Space = (TSpace)this;
		}

		public void Remove(TSensor sensor)
		{
			sensor.Dispose();
			sensors.Remove(sensor);
		}

		public void Update()
		{
			IsUpdateActive = true;

			var mainList = sensors.MainList;

			// Loop through sensors to update contacts (and call callbacks as appropriate).
			for (int i = 0; i < mainList.Count; i++)
			{
				var sensor1 = mainList[i];

				if (!sensor1.IsEnabled)
				{
					continue;
				}

				Debug.Assert(sensor1.Shape != null, "Enabled sensor never had its shape set.");

				var owner1 = sensor1.Owner;
				var type1 = sensor1.Type;
				var groups1 = sensor1.Groups;
				var affects1 = sensor1.Affects;
				var contacts1 = sensor1.Contacts;
				var isZone1 = sensor1.Type == SensorTypes.Zone;

				for (int j = i + 1; j < mainList.Count; j++)
				{
					var sensor2 = mainList[j];

					if (!sensor2.IsEnabled)
					{
						continue;
					}

					Debug.Assert(sensor2.Shape != null, "Enabled sensor never had its shape set.");

					// Although contacts are symmetric, callbacks can be configured to trigger in only one direction.
					var oneAffectsTwo = (affects1 & sensor2.Groups) > 0;
					var twoAffectsOne = (sensor2.Affects & groups1) > 0;

					// By design, zones cannot interact with each other (only entity-entity or entity-zone collisions
					// are allowed).
					if (!sensor2.IsEnabled || (isZone1 && sensor2.Type == SensorTypes.Zone) || !(oneAffectsTwo ||
						twoAffectsOne))
					{
						continue;
					}

					var owner2 = sensor2.Owner;
					var type2 = sensor2.Type;
					var contacts2 = sensor2.Contacts;
					var overlaps = sensor1.Overlaps(sensor2);

					// Contacts are symmetric.
					if (contacts1.Contains(sensor2))
					{
						if (overlaps)
						{
							// By design, OnStay isn't called on the frame the sensors touch.
							if (oneAffectsTwo) { sensor1.OnStay?.Invoke(type2, owner2); }
							if (twoAffectsOne) { sensor2.OnStay?.Invoke(type1, owner1); }
						}
						else
						{
							contacts1.Remove(sensor2);
							contacts2.Remove(sensor1);

							if (oneAffectsTwo) { sensor1.OnSeparate?.Invoke(type2, owner2); }
							if (twoAffectsOne) { sensor2.OnSeparate?.Invoke(type1, owner1); }
						}

						continue;
					}

					if (overlaps)
					{
						contacts1.Add(sensor2);
						contacts2.Add(sensor1);

						if (oneAffectsTwo) { sensor1.OnSense?.Invoke(type2, owner2); }
						if (twoAffectsOne) { sensor2.OnSense?.Invoke(type1, owner1); }
					}
				}
			}

			IsUpdateActive = false;
			ProcessChanges();
		}

		private void ProcessChanges()
		{
			sensors.ProcessChanges();

			foreach (var sensor in sensors.MainList.Where(sensor => sensor.IsTogglePending))
			{
				sensor.IsEnabled = !sensor.IsEnabled;

				if (!sensor.IsEnabled)
				{
					sensor.ClearContacts();
				}
			}
		}
	}
}
