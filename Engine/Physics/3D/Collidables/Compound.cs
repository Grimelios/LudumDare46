using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Physics._3D.Utility.Memory;
using System.Diagnostics;
using Engine.Physics._3D.Utility;
using Engine.Physics._3D.Trees;
using Engine.Physics._3D.CollisionDetection.CollisionTasks;

namespace Engine.Physics._3D.Collidables
{
    public struct CompoundChild
    {
        public TypedIndex ShapeIndex;
        public RigidPose LocalPose;
    }

    /// <summary>
    /// Minimalist compound shape containing a list of child physicsShapes3D. Does not make use of any internal acceleration structure; should be used only with small groups of physicsShapes3D.
    /// </summary>
    public struct Compound : ICompoundShape
    {
        /// <summary>
        /// Buffer of children within this compound.
        /// </summary>
        public Buffer<CompoundChild> Children;

        /// <summary>
        /// Creates a compound shape with no acceleration structure.
        /// </summary>
        /// <param name="children">Set of children in the compound.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Compound(Buffer<CompoundChild> children)
        {
            Debug.Assert(children.Length > 0, "Compounds must have a nonzero number of children.");
            Children = children;
        }

        /// <summary>
        /// Checks if a shape index.
        /// </summary>
        /// <param name="shapeIndex">Shape index to analyze.</param>
        /// <param name="physicsShape3DBatches">Shape collection into which the index indexes.</param>
        /// <returns>True if the index is valid, false otherwise.</returns>
        public static bool ValidateChildIndex(TypedIndex shapeIndex, PhysicsShapes3D physicsShape3DBatches)
        {
            if (shapeIndex.Type < 0 || shapeIndex.Type >= physicsShape3DBatches.RegisteredTypeSpan)
            {
                Debug.Fail("Child shape type needs to fit within the shape batch registered types.");
                return false;
            }
            var batch = physicsShape3DBatches[shapeIndex.Type];
            if (shapeIndex.Index < 0 || shapeIndex.Index >= batch.Capacity)
            {
                Debug.Fail("Child shape index should point to a valid buffer location in the sahpe batch.");
                return false;
            }
            if (physicsShape3DBatches[shapeIndex.Type].Compound)
            {
                Debug.Fail("Child shape type should be convex.");
                return false;
            }
            //TODO: We don't have a cheap way to verify that a specific index actually contains a shape right now.
            return true;
        }

        /// <summary>
        /// Checks if a set of children shape indices are all valid.
        /// </summary>
        /// <param name="children">Children to examine.</param>
        /// <param name="physicsShape3DBatches">Shape collection into which the children index.</param>
        /// <returns>True if all child indices are valid, false otherwise.</returns>
        public static bool ValidateChildIndices(ref Buffer<CompoundChild> children, PhysicsShapes3D physicsShape3DBatches)
        {
            for (int i = 0; i < children.Length; ++i)
            {
                ValidateChildIndex(children[i].ShapeIndex, physicsShape3DBatches);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRotatedChildPose(in RigidPose localPose, in Quaternion orientation, out RigidPose rotatedChildPose)
        {
            QuaternionEx.ConcatenateWithoutOverlap(localPose.Orientation, orientation, out rotatedChildPose.Orientation);
            QuaternionEx.Transform(localPose.Position, orientation, out rotatedChildPose.Position);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRotatedChildPose(in RigidPoses localPose, in QuaternionWide orientation, out Vector3Wide childPosition, out QuaternionWide childOrientation)
        {
            QuaternionWide.ConcatenateWithoutOverlap(localPose.Orientation, orientation, out childOrientation);
            QuaternionWide.TransformWithoutOverlap(localPose.Position, orientation, out childPosition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRotatedChildPose(in RigidPoses localPose, in QuaternionWide orientation, out RigidPoses rotatedChildPose)
        {
            GetRotatedChildPose(localPose, orientation, out rotatedChildPose.Position, out rotatedChildPose.Orientation);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetWorldPose(in RigidPose localPose, in RigidPose transform, out RigidPose worldPose)
        {
            GetRotatedChildPose(localPose, transform.Orientation, out worldPose);
            //TODO: This is an area that has to be updated for high precision poses. May be able to centralize positional work
            //by deferring it until the final bounds scatter step. Would require looking up the position then, but could be worth simplicity.
            worldPose.Position += transform.Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComputeChildBounds(in CompoundChild child, in Quaternion orientation, PhysicsShapes3D physicsShape3DBatches, out Vector3 childMin, out Vector3 childMax)
        {
            GetRotatedChildPose(child.LocalPose, orientation, out var childPose);
            Debug.Assert(!physicsShape3DBatches[child.ShapeIndex.Type].Compound, "All children of a compound must be convex.");
            physicsShape3DBatches[child.ShapeIndex.Type].ComputeBounds(child.ShapeIndex.Index, childPose, out childMin, out childMax);
        }

        public void ComputeBounds(in Quaternion orientation, PhysicsShapes3D physicsShape3DBatches, out Vector3 min, out Vector3 max)
        {
            ComputeChildBounds(Children[0], orientation, physicsShape3DBatches, out min, out max);
            for (int i = 1; i < Children.Length; ++i)
            {
                ref var child = ref Children[i];
                ComputeChildBounds(Children[i], orientation, physicsShape3DBatches, out var childMin, out var childMax);
                BoundingBox.CreateMerged(min, max, childMin, childMax, out min, out max);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddChildBoundsToBatcher(ref Buffer<CompoundChild> children, ref BoundingBoxBatcher batcher, in RigidPose pose, in BodyVelocity velocity, int bodyIndex)
        {
            //Note that this approximates the velocity of the child using a piecewise extrapolation using the parent's angular velocity.
            //For significant angular velocities, this is actually wrong, but this is how v1 worked forever and it's cheap.
            //May want to revisit this later- it would likely require that the BoundingBoxBatcher have a continuation, or to include more information
            //for the convex path to condition on.
            BodyVelocity childVelocity;
            childVelocity.Angular = velocity.Angular;
            for (int i = 0; i < children.Length; ++i)
            {
                ref var child = ref children[i];
                GetRotatedChildPose(child.LocalPose, pose.Orientation, out var childPose);
                var angularContributionToChildLinear = Vector3.Cross(velocity.Angular, childPose.Position);
                var contributionLengthSquared = angularContributionToChildLinear.LengthSquared();
                var localPoseRadiusSquared = childPose.Position.LengthSquared();
                if (contributionLengthSquared > localPoseRadiusSquared)
                {
                    angularContributionToChildLinear *= (float)(Math.Sqrt(localPoseRadiusSquared) / Math.Sqrt(contributionLengthSquared));
                }
                childVelocity.Linear = velocity.Linear + angularContributionToChildLinear;
                childPose.Position += pose.Position;
                batcher.AddCompoundChild(bodyIndex, children[i].ShapeIndex, childPose, childVelocity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildBoundsToBatcher(ref BoundingBoxBatcher batcher, in RigidPose pose, in BodyVelocity velocity, int bodyIndex)
        {
            AddChildBoundsToBatcher(ref Children, ref batcher, pose, velocity, bodyIndex);
        }

        struct WrappedHandler<TRayHitHandler> : IShapeRayHitHandler where TRayHitHandler : IShapeRayHitHandler
        {
            public TRayHitHandler HitHandler;
            public int ChildIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowTest(int childIndex)
            {
                return HitHandler.AllowTest(childIndex);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, int childIndex)
            {
                Debug.Assert(childIndex == 0, "All compound children should be convexes, so they should report a child index of 0.");
                Debug.Assert(maximumT >= t, "Whatever generated this ray hit should have obeyed the current maximumT value.");
                //Note the use of the child index given to the instance, not the parameter.
                HitHandler.OnRayHit(ray, ref maximumT, t, normal, ChildIndex);
            }
        }

        public void RayTest<TRayHitHandler>(in RigidPose pose, in RayData ray, ref float maximumT, PhysicsShapes3D physicsShape3DBatches, ref TRayHitHandler hitHandler) where TRayHitHandler : struct, IShapeRayHitHandler
        {
            WrappedHandler<TRayHitHandler> wrappedHandler;
            wrappedHandler.HitHandler = hitHandler;
            for (int i = 0; i < Children.Length; ++i)
            {
                ref var child = ref Children[i];
                wrappedHandler.ChildIndex = i;
                GetRotatedChildPose(child.LocalPose, pose.Orientation, out var childPose);
                //TODO: This is an area that has to be updated for high precision poses.
                childPose.Position += pose.Position;
                physicsShape3DBatches[child.ShapeIndex.Type].RayTest(child.ShapeIndex.Index, childPose, ray, ref maximumT, ref wrappedHandler);
            }
            //Preserve any mutations.
            hitHandler = wrappedHandler.HitHandler;
        }

        public void RayTest<TRayHitHandler>(in RigidPose pose, ref RaySource rays, PhysicsShapes3D physicsShape3DBatches, ref TRayHitHandler hitHandler) where TRayHitHandler : struct, IShapeRayHitHandler
        {
            WrappedHandler<TRayHitHandler> wrappedHandler;
            wrappedHandler.HitHandler = hitHandler;
            for (int i = 0; i < Children.Length; ++i)
            {
                ref var child = ref Children[i];
                wrappedHandler.ChildIndex = i;
                GetRotatedChildPose(child.LocalPose, pose.Orientation, out var childPose);
                //TODO: This is an area that has to be updated for high precision poses.
                childPose.Position += pose.Position;
                physicsShape3DBatches[child.ShapeIndex.Type].RayTest(child.ShapeIndex.Index, childPose, ref rays, ref wrappedHandler);
            }
            //Preserve any mutations.
            hitHandler = wrappedHandler.HitHandler;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShapeBatch CreateShapeBatch(BufferPool pool, int initialCapacity, PhysicsShapes3D physicsShapes3D)
        {
            return new CompoundShapeBatch<Compound>(pool, initialCapacity, physicsShapes3D);
        }

        public int ChildCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Children.Length; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref CompoundChild GetChild(int compoundChildIndex)
        {
            return ref Children[compoundChildIndex];
        }

        public unsafe void FindLocalOverlaps<TOverlaps, TSubpairOverlaps>(ref Buffer<OverlapQueryForPair> pairs, BufferPool pool, PhysicsShapes3D physicsShapes3D, ref TOverlaps overlaps)
            where TOverlaps : struct, ICollisionTaskOverlaps<TSubpairOverlaps>
            where TSubpairOverlaps : struct, ICollisionTaskSubpairOverlaps
        {
            for (int pairIndex = 0; pairIndex < pairs.Length; ++pairIndex)
            {
                ref var pair = ref pairs[pairIndex];
                ref var compound = ref Unsafe.AsRef<Compound>(pair.Container);
                ref var overlapsForPair = ref overlaps.GetOverlapsForPair(pairIndex);
                for (int i = 0; i < compound.Children.Length; ++i)
                {
                    ref var child = ref compound.Children[i];
                    //TODO: This does quite a bit of work. May want to try a simple bounding sphere instead (based on a dedicated maximum radius request).
                    physicsShapes3D[child.ShapeIndex.Type].ComputeBounds(child.ShapeIndex.Index, child.LocalPose.Orientation, out _, out _, out var min, out var max);
                    min += child.LocalPose.Position;
                    max += child.LocalPose.Position;
                    if (BoundingBox.Intersects(min, max, pair.Min, pair.Max))
                    {
                        overlapsForPair.Allocate(pool) = i;
                    }
                }
            }
        }

        public unsafe void FindLocalOverlaps<TOverlaps>(in Vector3 min, in Vector3 max, in Vector3 sweep, float maximumT, BufferPool pool, PhysicsShapes3D physicsShapes3D, void* overlapsPointer)
            where TOverlaps : ICollisionTaskSubpairOverlaps
        {
            Tree.ConvertBoxToCentroidWithExtent(min, max, out var sweepOrigin, out var expansion);
            TreeRay.CreateFrom(sweepOrigin, sweep, maximumT, out var ray);
            ref var overlaps = ref Unsafe.AsRef<TOverlaps>(overlapsPointer);
            for (int i = 0; i < Children.Length; ++i)
            {
                ref var child = ref Children[i];
                physicsShapes3D[child.ShapeIndex.Type].ComputeBounds(child.ShapeIndex.Index, child.LocalPose.Orientation, out _, out _, out var childMin, out var childMax);
                childMin = childMin + child.LocalPose.Position - expansion;
                childMax = childMax + child.LocalPose.Position + expansion;
                if (Tree.Intersects(childMin, childMax, &ray, out _))
                {
                    overlaps.Allocate(pool) = i;
                }
            }

        }

        public void Dispose(BufferPool bufferPool)
        {
            bufferPool.Return(ref Children);
        }

        /// <summary>
        /// Type id of list based compound physicsShapes3D.
        /// </summary>
        public const int Id = 6;
        public int TypeId { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Id; } }
    }


}
