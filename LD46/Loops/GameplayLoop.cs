using System;
using Engine;
using Engine.Core;
using Engine.Graphics._2D;
using Engine.Input;
using Engine.Input.Data;
using Engine.Utility;
using GlmSharp;
using LD46.Entities;
using LD46.Entities.Core;

namespace LD46.Loops
{
	public class GameplayLoop : LDLoop
	{
		private SimpleScene2D scene;
		private Hand hand;

		public override void Initialize()
		{
			var data = JsonUtilities.Deserialize<CardData[]>("Cards.json");

			scene = new SimpleScene2D();
			scene.Camera = Camera;
			scene.Canvas = Canvas;

			hand = new Hand(scene);
			hand.Add(new Card(data[0]));

			InputProcessor.Add(full =>
			{
				if (!full.TryGetData(out MouseData mouse))
				{
					return;
				}

				if (mouse.Query(GLFW.GLFW_MOUSE_BUTTON_LEFT, InputStates.PressedThisFrame))
				{
					var random = new Random();
					hand.Add(new Card(data[random.Next(data.Length)]));
				}
			});
		}

		public override void Update()
		{
			scene.Update();

			base.Update();
		}

		public override void Draw(SpriteBatch sb, float t)
		{
			scene.Draw(sb, t);
		}
	}
}
