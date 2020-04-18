using Engine;
using Engine.Content;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Input;
using Engine.Input.Data;
using Engine.Shapes._2D;
using GlmSharp;
using LD46.Entities.Core;

namespace LD46.Entities
{
	public class Card : SimpleEntity2D
	{
		private const int Width = 150;
		private const int Height = 220;

		private int attack;
		private int defense;
		private int health;
		private int maxHealth;

		public Card(CardData data)
		{
			const int HalfWidth = Width / 2;
			const int HalfHeight = Height / 2;
			const int Offset = 15;

			attack = data.Attack;
			defense = data.Defense;
			health = data.Health;
			maxHealth = health;

			var font = ContentCache.GetFont("Debug");

			Attach(new SpriteText(font, data.Name, Alignments.Center), new vec2(0, HalfHeight - Offset));
			Attach(new SpriteText(font, attack.ToString(), Alignments.Center), new vec2(-HalfWidth + Offset,
				-HalfHeight + Offset));
			Attach(new SpriteText(font, defense.ToString(), Alignments.Center), new vec2(-HalfWidth + Offset * 2,
				-HalfHeight + Offset * 2));
			Attach(new SpriteText(font, health.ToString(), Alignments.Center), new vec2(HalfWidth - Offset,
				-HalfHeight + Offset));
			Attach(new SpriteText(font, maxHealth.ToString(), Alignments.Center), new vec2(HalfWidth - Offset * 2,
				-HalfHeight + Offset * 2));

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
		}

		public override void Draw(SpriteBatch sb, float t)
		{
			var rect = new Rectangle(Position.x, Position.y, Width, Height);
			rect.Rotation = Rotation;

			sb.Draw(rect, Color.White);

			base.Draw(sb, t);
		}
	}
}
