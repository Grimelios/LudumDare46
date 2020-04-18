﻿using Engine.Physics._3D.Utility.Collections;
using Engine.Physics._3D.Utility.Memory;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Engine.Physics._3D.CollisionDetection
{


    /// <summary>
    /// The cached pair data created by a single worker during the last execution of narrow phase pair processing.
    /// </summary>
    public struct WorkerPairCache
    {
        public struct PreallocationSizes
        {
            public int ElementCount;
            public int ElementSizeInBytes;
        }
        internal BufferPool pool; //note that this reference makes the entire worker pair cache nonblittable. That's why the pair cache uses managed arrays to store the worker caches.
        int minimumPerTypeCapacity;
        int workerIndex;
        //Note that the per-type batches are untyped.
        //The caller will have the necessary type knowledge to interpret the buffer.
        internal Buffer<UntypedList> constraintCaches;
        internal Buffer<UntypedList> collisionCaches;

        public struct PendingAdd
        {
            public CollidablePair Pair;
            public CollidablePairPointers Pointers;
        }

        /// <summary>
        /// The set of pair-pointer associations created by this worker that should be added to the pair mapping.
        /// </summary>
        public QuickList<PendingAdd> PendingAdds;
        /// <summary>
        /// The set of pairs to remove from the pair cache generated by the worker.
        /// </summary>
        public QuickList<CollidablePair> PendingRemoves;

        public WorkerPairCache(int workerIndex, BufferPool pool,
            ref QuickList<PreallocationSizes> minimumSizesPerConstraintType,
            ref QuickList<PreallocationSizes> minimumSizesPerCollisionType,
            int pendingCapacity, int minimumPerTypeCapacity = 128)
        {
            this.workerIndex = workerIndex;
            this.pool = pool;
            this.minimumPerTypeCapacity = minimumPerTypeCapacity;
            const float previousCountMultiplier = 1.25f;
            pool.TakeAtLeast(PairCache.CollisionConstraintTypeCount, out constraintCaches);
            pool.TakeAtLeast(PairCache.CollisionTypeCount, out collisionCaches);
            for (int i = 0; i < minimumSizesPerConstraintType.Count; ++i)
            {
                ref var sizes = ref minimumSizesPerConstraintType[i];
                if (sizes.ElementCount > 0)
                    constraintCaches[i] = new UntypedList(sizes.ElementSizeInBytes, Math.Max(minimumPerTypeCapacity, (int)(previousCountMultiplier * sizes.ElementCount)), pool);
                else
                    constraintCaches[i] = new UntypedList();
            }
            //Clear out the remainder of slots to avoid invalid data.
            constraintCaches.Clear(minimumSizesPerConstraintType.Count, constraintCaches.Length - minimumSizesPerConstraintType.Count);
            for (int i = 0; i < minimumSizesPerCollisionType.Count; ++i)
            {
                ref var sizes = ref minimumSizesPerCollisionType[i];
                if (sizes.ElementCount > 0)
                    collisionCaches[i] = new UntypedList(sizes.ElementSizeInBytes, Math.Max(minimumPerTypeCapacity, (int)(previousCountMultiplier * sizes.ElementCount)), pool);
                else
                    collisionCaches[i] = new UntypedList();
            }
            //Clear out the remainder of slots to avoid invalid data.
            collisionCaches.Clear(minimumSizesPerCollisionType.Count, collisionCaches.Length - minimumSizesPerCollisionType.Count);

            PendingAdds = new QuickList<PendingAdd>(pendingCapacity, pool);
            PendingRemoves = new QuickList<CollidablePair>(pendingCapacity, pool);
        }


        public void GetMaximumCacheTypeCounts(out int collision, out int constraint)
        {
            collision = 0;
            constraint = 0;
            for (int i = collisionCaches.Length - 1; i >= 0; --i)
            {
                if (collisionCaches[i].Count > 0)
                {
                    collision = i + 1;
                    break;
                }
            }
            for (int i = constraintCaches.Length - 1; i >= 0; --i)
            {
                if (constraintCaches[i].Count > 0)
                {
                    constraint = i + 1;
                    break;
                }
            }
        }

        public void AccumulateMinimumSizes(
            ref QuickList<PreallocationSizes> minimumSizesPerConstraintType,
            ref QuickList<PreallocationSizes> minimumSizesPerCollisionType)
        {
            //Note that the count is expanded only as a constraint or cache of a given type is encountered.
            for (int i = 0; i < constraintCaches.Length; ++i)
            {
                ref var constraintCache = ref constraintCaches[i];
                if (constraintCache.Count > 0)
                {
                    if (i >= minimumSizesPerConstraintType.Count)
                    {
                        minimumSizesPerConstraintType.Count = i + 1;
                    }
                    ref var sizes = ref minimumSizesPerConstraintType[i];
                    sizes.ElementCount = Math.Max(sizes.ElementCount, constraintCache.Count);
                    //Technically this element size assignment may occur multiple times, but it's also simple and a one time process.
                    Debug.Assert(sizes.ElementSizeInBytes == 0 || sizes.ElementSizeInBytes == constraintCache.ElementSizeInBytes, "Either this size hasn't been initialized, or it should match.");
                    sizes.ElementSizeInBytes = constraintCache.ElementSizeInBytes;
                }
            }
            for (int i = collisionCaches.Length - 1; i >= 0; --i)
            {
                ref var collisionCache = ref collisionCaches[i];
                if (collisionCache.Count > 0)
                {
                    if (i >= minimumSizesPerCollisionType.Count)
                    {
                        minimumSizesPerCollisionType.Count = i + 1;
                    }
                    ref var sizes = ref minimumSizesPerCollisionType[i];
                    sizes.ElementCount = Math.Max(sizes.ElementCount, collisionCache.Count);
                    //Technically this element size assignment may occur multiple times, but it's also simple and a one time process.
                    Debug.Assert(sizes.ElementSizeInBytes == 0 || sizes.ElementSizeInBytes == collisionCache.ElementSizeInBytes, "Either this size hasn't been initialized, or it should match.");
                    sizes.ElementSizeInBytes = collisionCache.ElementSizeInBytes;
                }
            }
        }

        //Note that we have no-collision-data overloads. The vast majority of types don't actually have any collision data cached.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WorkerCacheAdd<TCollision, TConstraint>(ref TCollision collisionCache, ref TConstraint constraintCache, out CollidablePairPointers pointers)
            where TCollision : IPairCacheEntry where TConstraint : IPairCacheEntry
        {
            pointers.ConstraintCache = new PairCacheIndex(workerIndex, constraintCache.CacheTypeId, constraintCaches[constraintCache.CacheTypeId].Add(ref constraintCache, minimumPerTypeCapacity, pool));

            if (typeof(TCollision) == typeof(EmptyCollisionCache))
                pointers.CollisionDetectionCache = new PairCacheIndex();
            else
                pointers.CollisionDetectionCache = new PairCacheIndex(workerIndex, collisionCache.CacheTypeId, collisionCaches[collisionCache.CacheTypeId].Add(ref collisionCache, minimumPerTypeCapacity, pool));

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PairCacheIndex Add<TCollision, TConstraint>(ref CollidablePair pair, ref TCollision collisionCache, ref TConstraint constraintCache)
            where TCollision : IPairCacheEntry where TConstraint : IPairCacheEntry
        {
            PendingAdd pendingAdd;
            WorkerCacheAdd(ref collisionCache, ref constraintCache, out pendingAdd.Pointers);
            pendingAdd.Pair = pair;
            PendingAdds.Add(pendingAdd, pool);
            return pendingAdd.Pointers.ConstraintCache;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update<TCollision, TConstraint>(ref CollidablePairPointers pointers, ref TCollision collisionCache, ref TConstraint constraintCache)
            where TCollision : IPairCacheEntry where TConstraint : IPairCacheEntry
        {
            WorkerCacheAdd(ref collisionCache, ref constraintCache, out pointers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void* GetConstraintCachePointer(PairCacheIndex constraintCacheIndex)
        {
            return constraintCaches[constraintCacheIndex.Type].Buffer.Memory + constraintCacheIndex.Index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void* GetCollisionCachePointer(PairCacheIndex collisionCacheIndex)
        {
            return collisionCaches[collisionCacheIndex.Type].Buffer.Memory + collisionCacheIndex.Index;
        }

        public void Dispose()
        {
            for (int i = 0; i < constraintCaches.Length; ++i)
            {
                if (constraintCaches[i].Buffer.Allocated)
                    pool.Return(ref constraintCaches[i].Buffer);
            }
            pool.Return(ref constraintCaches);
            for (int i = 0; i < collisionCaches.Length; ++i)
            {
                if (collisionCaches[i].Buffer.Allocated)
                    pool.Return(ref collisionCaches[i].Buffer);
            }
            pool.Return(ref collisionCaches);
            this = new WorkerPairCache();
            //note that the pending collections are not disposed here; they are disposed upon flushing immediately after the narrow phase completes.
        }
    }
}
