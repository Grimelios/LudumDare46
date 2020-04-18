﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Physics._3D.Utility.Memory;
using System.Diagnostics;
using Engine.Physics._3D.Utility;
using Engine.Physics._3D.Utility.Collections;
using Engine.Physics._3D.Trees;
using Engine.Physics._3D.CollisionDetection.CollisionTasks;

namespace Engine.Physics._3D.Collidables
{
    /// <summary>
    /// Compound shape containing a bunch of physicsShapes3D accessible through a tree acceleration structure. Useful for compounds with lots of children.
    /// </summary>
    public struct BigCompound : ICompoundShape
    {
        /// <summary>
        /// Acceleration structure for the compound children.
        /// </summary>
        public Tree Tree;
        /// <summary>
        /// Buffer of children within this compound.
        /// </summary>
        public Buffer<CompoundChild> Children;

        /// <summary>
        /// Creates a compound shape with an acceleration structure.
        /// </summary>
        /// <param name="children">Set of children in the compound.</param>
        /// <param name="physicsShapes3D">PhysicsShapes3D set in which child physicsShapes3D are allocated.</param>
        /// <param name="pool">Pool to use to allocate acceleration structures.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BigCompound(Buffer<CompoundChild> children, PhysicsShapes3D physicsShapes3D, BufferPool pool)
        {
            Debug.Assert(children.Length > 0, "Compounds must have a nonzero number of children.");
            Debug.Assert(Compound.ValidateChildIndices(ref children, physicsShapes3D), "Children must all be convex.");
            Children = children;
            Tree = new Tree(pool, children.Length);
            pool.Take(children.Length, out Buffer<BoundingBox> leafBounds);
            Compound.ComputeChildBounds(Children[0], Quaternion.Identity, physicsShapes3D, out leafBounds[0].Min, out leafBounds[0].Max);
            for (int i = 1; i < Children.Length; ++i)
            {
                ref var bounds = ref leafBounds[i];
                Compound.ComputeChildBounds(Children[i], Quaternion.Identity, physicsShapes3D, out bounds.Min, out bounds.Max);
            }
            Tree.SweepBuild(pool, leafBounds);
            pool.Return(ref leafBounds);
        }

        public void ComputeBounds(in Quaternion orientation, PhysicsShapes3D physicsShape3DBatches, out Vector3 min, out Vector3 max)
        {
            Compound.ComputeChildBounds(Children[0], orientation, physicsShape3DBatches, out min, out max);
            for (int i = 1; i < Children.Length; ++i)
            {
                ref var child = ref Children[i];
                Compound.ComputeChildBounds(Children[i], orientation, physicsShape3DBatches, out var childMin, out var childMax);
                BoundingBox.CreateMerged(min, max, childMin, childMax, out min, out max);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildBoundsToBatcher(ref BoundingBoxBatcher batcher, in RigidPose pose, in BodyVelocity velocity, int bodyIndex)
        {
            Compound.AddChildBoundsToBatcher(ref Children, ref batcher, pose, velocity, bodyIndex);
        }

        struct HitRotator<TRayHitHandler> : IShapeRayHitHandler where TRayHitHandler : struct, IShapeRayHitHandler
        {
            public TRayHitHandler HitHandler;
            public Matrix3x3 Orientation;
            public RayData OriginalRay;
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
                Matrix3x3.Transform(normal, Orientation, out var rotatedNormal);
                //The child index we report is the one we got from the LeafTester.
                HitHandler.OnRayHit(OriginalRay, ref maximumT, t, rotatedNormal, ChildIndex);
            }
        }


        unsafe struct LeafTester<TRayHitHandler> : IRayLeafTester where TRayHitHandler : struct, IShapeRayHitHandler
        {
            public CompoundChild* Children;
            public PhysicsShapes3D physicsShapes3D;
            public HitRotator<TRayHitHandler> HitRotator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LeafTester(in Buffer<CompoundChild> children, PhysicsShapes3D physicsShapes3D, in TRayHitHandler handler, in Matrix3x3 orientation, in RayData originalRay)
            {
                Children = children.Memory;
                this.physicsShapes3D = physicsShapes3D;
                HitRotator.HitHandler = handler;
                HitRotator.Orientation = orientation;
                HitRotator.OriginalRay = originalRay;
                HitRotator.ChildIndex = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void TestLeaf(int leafIndex, RayData* rayData, float* maximumT)
            {
                ref var child = ref Children[leafIndex];
                HitRotator.ChildIndex = leafIndex;
                physicsShapes3D[child.ShapeIndex.Type].RayTest(child.ShapeIndex.Index, child.LocalPose, *rayData, ref *maximumT, ref HitRotator);
            }
        }

        public void RayTest<TRayHitHandler>(in RigidPose pose, in RayData ray, ref float maximumT, PhysicsShapes3D physicsShapes3D, ref TRayHitHandler hitHandler) where TRayHitHandler : struct, IShapeRayHitHandler
        {
            Matrix3x3.CreateFromQuaternion(pose.Orientation, out var orientation);
            Matrix3x3.TransformTranspose(ray.Origin - pose.Position, orientation, out var localOrigin);
            Matrix3x3.TransformTranspose(ray.Direction, orientation, out var localDirection);
            var leafTester = new LeafTester<TRayHitHandler>(Children, physicsShapes3D, hitHandler, orientation, ray);
            Tree.RayCast(localOrigin, localDirection, ref maximumT, ref leafTester);
            //Copy the hitHandler to preserve any mutation.
            hitHandler = leafTester.HitRotator.HitHandler;
        }

        public unsafe void RayTest<TRayHitHandler>(in RigidPose pose, ref RaySource rays, PhysicsShapes3D physicsShapes3D, ref TRayHitHandler hitHandler) where TRayHitHandler : struct, IShapeRayHitHandler
        {
            //TODO: Note that we dispatch a bunch of scalar tests here. You could be more clever than this- batched tests are possible. 
            //May be worth creating a different traversal designed for low ray counts- might be able to get some benefit out of a semidynamic packet or something.
            Matrix3x3.CreateFromQuaternion(pose.Orientation, out var orientation);
            Matrix3x3.Transpose(orientation, out var inverseOrientation);
            var leafTester = new LeafTester<TRayHitHandler>(Children, physicsShapes3D, hitHandler, orientation, default);
            for (int i = 0; i < rays.RayCount; ++i)
            {
                rays.GetRay(i, out var ray, out var maximumT);
                leafTester.HitRotator.OriginalRay = *ray;
                Matrix3x3.Transform(ray->Origin - pose.Position, inverseOrientation, out var localOrigin);
                Matrix3x3.Transform(ray->Direction, inverseOrientation, out var localDirection);
                Tree.RayCast(localOrigin, localDirection, ref *maximumT, ref leafTester);
            }
            //Preserve any mutations.
            hitHandler = leafTester.HitRotator.HitHandler;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShapeBatch CreateShapeBatch(BufferPool pool, int initialCapacity, PhysicsShapes3D physicsShapes3D)
        {
            return new CompoundShapeBatch<BigCompound>(pool, initialCapacity, physicsShapes3D);
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

        unsafe struct Enumerator<TSubpairOverlaps> : IBreakableForEach<int> where TSubpairOverlaps : ICollisionTaskSubpairOverlaps
        {
            public BufferPool Pool;
            public void* Overlaps;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LoopBody(int i)
            {
                Unsafe.AsRef<TSubpairOverlaps>(Overlaps).Allocate(Pool) = i;
                return true;
            }
        }

        public unsafe void FindLocalOverlaps<TOverlaps, TSubpairOverlaps>(ref Buffer<OverlapQueryForPair> pairs, BufferPool pool, PhysicsShapes3D physicsShapes3D, ref TOverlaps overlaps)
            where TOverlaps : struct, ICollisionTaskOverlaps<TSubpairOverlaps>
            where TSubpairOverlaps : struct, ICollisionTaskSubpairOverlaps
        {
            //For now, we don't use anything tricky. Just traverse every child against the tree sequentially.
            //TODO: This sequentializes a whole lot of cache misses. You could probably get some benefit out of traversing all pairs 'simultaneously'- that is, 
            //using the fact that we have lots of independent queries to ensure the CPU always has something to do.
            Enumerator<TSubpairOverlaps> enumerator;
            enumerator.Pool = pool;
            for (int i = 0; i < pairs.Length; ++i)
            {
                ref var pair = ref pairs[i];
                enumerator.Overlaps = Unsafe.AsPointer(ref overlaps.GetOverlapsForPair(i));
                Unsafe.AsRef<BigCompound>(pair.Container).Tree.GetOverlaps(pair.Min, pair.Max, ref enumerator);
            }
        }

        unsafe struct SweepLeafTester<TOverlaps> : ISweepLeafTester where TOverlaps : ICollisionTaskSubpairOverlaps
        {
            public BufferPool Pool;
            public void* Overlaps;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void TestLeaf(int leafIndex, ref float maximumT)
            {
                Unsafe.AsRef<TOverlaps>(Overlaps).Allocate(Pool) = leafIndex;
            }
        }
        public unsafe void FindLocalOverlaps<TOverlaps>(in Vector3 min, in Vector3 max, in Vector3 sweep, float maximumT, BufferPool pool, PhysicsShapes3D physicsShapes3D, void* overlaps)
            where TOverlaps : ICollisionTaskSubpairOverlaps
        {
            SweepLeafTester<TOverlaps> enumerator;
            enumerator.Pool = pool;
            enumerator.Overlaps = overlaps;
            Tree.Sweep(min, max, sweep, maximumT, ref enumerator);
        }

        public void Dispose(BufferPool bufferPool)
        {
            bufferPool.Return(ref Children);
            Tree.Dispose(bufferPool);
        }

        /// <summary>
        /// Type id of compound physicsShapes3D.
        /// </summary>
        public const int Id = 7;
        public int TypeId { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Id; } }
    }


}