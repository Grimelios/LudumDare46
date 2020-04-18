using System;
using System.Diagnostics;
using System.Linq;
using Engine.Content;
using Engine.Core._3D;
using Engine.Graphics._3D;
using GlmSharp;

namespace Engine.Animation
{
	// TODO: Interpolate bone transforms while animating.
	public class Skeleton : MeshUser
	{
		private vec3[] defaultPose;

		public Skeleton(string filename) : this(ContentCache.GetMesh(filename), null)
		{
		}
		
		public Skeleton(Mesh mesh, vec3[] defaultPose) : base(mesh)
		{
			var indexes = mesh.BoneIndexes;
			var weights = mesh.BoneWeights;

			Debug.Assert(indexes.Length > 0, "Can't constrcut a skeleton with an empty bone index array.");
			Debug.Assert(indexes.Length == weights.Length, "Mesh bone arrays (indexes and weights) must be the same " +
				"length (when used to construct a skeleton).");

			Bones = new Bone[indexes.Max(p => Math.Max(p.x, p.y)) + 1];

			for (int i = 0; i < Bones.Length; i++)
			{
				Bones[i] = new Bone();
			}

			DefaultPose = defaultPose;
		}
		
		public Bone[] Bones { get; }

		// Note that setting a default pose snaps all bones to match (without interpolation).
		// TODO: Consider using a full Transform for the default pose (rather than only positions).
		public vec3[] DefaultPose
		{
			get => defaultPose;

			// TODO: Will a default pose ever be set past spawn?
			set
			{
				defaultPose = value;

				if (value == null)
				{
					PoseOrigin = vec3.Zero;

					return;
				}

				PoseOrigin = defaultPose.Aggregate(vec3.Zero, (current, p) => current + p) / value.Length;

				// It's assumed that if a default pose is set programatically, it's size will match the number of bones
				// on the skeleton.
				for (int i = 0; i < value.Length; i++)
				{
					Bones[i].Position.SetValue(value[i], false);
				}
			}
		}
		
		public vec3 PoseOrigin { get; private set; }

		public override void Recompute(float t)
		{
			foreach (var bone in Bones)
			{
				bone.Recompute(t);
			}

			base.Recompute(t);
		}
	}
}
