using Engine.Content;
using Engine.Core._2D;
using Engine.Graphics;
using GlmSharp;
using LD46.Entities.Core;

namespace LD46.Entities
{
	public class Card : SimpleEntity2D
	{
		private static Texture texture = ContentCache.GetTexture("Card.png");

		private int attack;
		private int defense;
		private int health;
		private int maxHealth;

		public Card(CardData data)
		{
			const int Offset = 16;

			attack = data.Attack;
			defense = data.Defense;
			health = data.Health;
			maxHealth = health;

			var font = ContentCache.GetFont("Debug");
			var w = texture.Width / 2 - 2;
			var h = texture.Height / 2 - 2;

			Attach(new Sprite(texture));
			Attach(new SpriteText(font, data.Name, Alignments.Center, true), new vec2(0, h - 28));
			Attach(new SpriteText(font, attack.ToString(), Alignments.Center, true), new vec2(-w + Offset,
				-h + Offset));
			Attach(new SpriteText(font, defense.ToString(), Alignments.Center, true), new vec2(-w + Offset * 2,
				-h + Offset * 2));
			Attach(new SpriteText(font, health.ToString(), Alignments.Center, true), new vec2(w - Offset,
				-h + Offset));
			Attach(new SpriteText(font, maxHealth.ToString(), Alignments.Center, true), new vec2(w - Offset * 2,
				-h + Offset * 2));

			/*
			InputProcessor.Add(data =>
			{
				if (!data.TryGetData(out MouseData mouse))
				{
					return;
				}

				var left = mouse.Query(GLFW.GLFW_MOUSE_BUTTON_LEFT, InputStates.Held);
				var right = mouse.Query(GLFW.GLFW_MOUSE_BUTTON_RIGHT, InputStates.Held);
				var r = Rotation;

				if (left ^ right)
				{
					r += DeltaTime.Value * (left ? -1 : 1);
				}

				SetTransform(Scene.Camera.ToWorld(mouse.Location), r, false);
			});
			*/
		}
	}
}
