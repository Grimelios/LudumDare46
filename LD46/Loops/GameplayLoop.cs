using System;
using Engine;
using Engine.Graphics._2D;
using Engine.Input;
using Engine.Input.Data;
using Engine.Utility;
using LD46.Entities;
using LD46.Entities.Core;

namespace LD46.Loops
{
	public class GameplayLoop : LDLoop
	{
		private SimpleScene2D scene;
		private Hand hand;
		private Board board;

		public override void Initialize()
		{
			var data = JsonUtilities.Deserialize<CardData[]>("Cards.json");
			var random = new Random();

			scene = new SimpleScene2D();
			scene.Camera = Camera;
			scene.Canvas = Canvas;

			hand = new Hand(scene);
			board = new Board();

			for (int i = 0; i < 5; i++)
			{
				hand.Add(new Card(data[random.Next(data.Length)]));
			}

			InputProcessor.Add(full =>
			{
				if (!full.TryGetData(out MouseData mouse))
				{
					return;
				}

				if (mouse.Query(GLFW.GLFW_MOUSE_BUTTON_LEFT, InputStates.PressedThisFrame))
				{
					board.PlayerLane.Add(hand.Play(0));
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
