using Engine.Physics._3D.Utility.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Physics._3D
{
    //TODO: It would be nice to have an index-aligned version of this. Quite a bit more complicated to implement since indices move around a lot and involve complex multithreaded bookkeeping.
    /// <summary>
    /// Convenience collection that stores extra properties about bodies3D, indexed by the body's handle.
    /// </summary>
    /// <typeparam name="T">Type of the data to store.</typeparam>
    /// <remarks>This is built for use cases relying on random access like the narrow phase. For maximum performance with sequential access, an index-aligned structure would be better.</remarks>
    public class BodyProperty<T> : IDisposable where T : unmanaged
    {
        Bodies3D bodies3D;
        BufferPool pool;
        Buffer<T> data;

        //The split constructor/initialize looks a bit odd, but this is very frequently used for narrow phase callbacks. The instance needs to exist before the simulation does.

        /// <summary>
        /// Constructs a new collection to store handle-aligned body properties. Assumes the Initialize function will be called later to provide the Bodies3D collection.
        /// </summary>
        /// <param name="pool">Pool from which to pull internal resources. If null, uses the later provided Bodies3D pool.</param>
        public BodyProperty(BufferPool pool = null)
        {
            this.pool = pool;
        }

        /// <summary>
        /// Constructs a new collection to store handle-aligned body properties.
        /// </summary>
        /// <param name="bodies3D">Bodies3D collection to track.</param>
        /// <param name="pool">Pool from which to pull internal resources. If null, uses the Bodies3D pool.</param>
        public BodyProperty(Bodies3D bodies3D, BufferPool pool = null)
        {
            this.bodies3D = bodies3D;
            this.pool = pool == null ? bodies3D.Pool : pool;
            this.pool.TakeAtLeast(bodies3D.HandleToLocation.Length, out data);
        }

        /// <summary>
        /// Initializes the body property collection if the Bodies3D-less constructor was used.
        /// </summary>
        /// <param name="bodies3D">Bodies3D collection to track.</param>
        public void Initialize(Bodies3D bodies3D)
        {
            if (this.bodies3D != null)
                throw new InvalidOperationException("Initialize should only be used on a collection which was constructed without defining the Bodies3D collection.");
            this.bodies3D = bodies3D;
            if (pool == null)
                pool = bodies3D.Pool;
            pool.TakeAtLeast(bodies3D.HandleToLocation.Length, out data);
        }

        /// <summary>
        /// Gets the mask associated with a body's handle.
        /// </summary>
        /// <param name="bodyHandle">Body handle to retrieve the collision mask of.</param>
        /// <returns>Collision mask associated with a body handle.</returns>
        public ref T this[int bodyHandle]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(bodies3D.BodyExists(bodyHandle));
                return ref data[bodyHandle];
            }
        }

        /// <summary>
        /// Ensures there is space for a given body handle and returns a reference to the used memory.
        /// </summary>
        /// <param name="bodyHandle">Body handle to allocate for.</param>
        /// <returns>Reference to the data for the given body.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Allocate(int bodyHandle)
        {
            Debug.Assert(bodies3D.BodyExists(bodyHandle), "The body handle should have been allocated in the bodies3D set before any attempt to create a property for it.");
            if (bodyHandle >= data.Length)
            {
                var targetCount = BufferPool.GetCapacityForCount<T>(bodies3D.HandlePool.HighestPossiblyClaimedId + 1);
                Debug.Assert(targetCount > data.Length, "Given what just happened, we must require a resize.");
                pool.ResizeToAtLeast(ref data, targetCount, Math.Min(bodies3D.HandlePool.HighestPossiblyClaimedId + 1, data.Length));
            }
            return ref data[bodyHandle];
        }

        /// <summary>
        /// Ensures that the internal structures has at least the given capacity.
        /// </summary>
        /// <param name="capacity">Capacity to ensure.</param>
        public void EnsureCapacity(int capacity)
        {
            var targetCount = BufferPool.GetCapacityForCount<T>(Math.Max(bodies3D.HandlePool.HighestPossiblyClaimedId + 1, capacity));
            if (targetCount > data.Length)
            {
                pool.ResizeToAtLeast(ref data, targetCount, Math.Min(data.Length, bodies3D.HandlePool.HighestPossiblyClaimedId + 1));
            }
        }

        /// <summary>
        /// Compacts the memory used by the collection to a safe minimum based on the Bodies3D collection.
        /// </summary>
        public void Compact()
        {
            var targetCount = BufferPool.GetCapacityForCount<T>(bodies3D.HandlePool.HighestPossiblyClaimedId + 1);
            if (targetCount < data.Length)
            {
                pool.ResizeToAtLeast(ref data, targetCount, bodies3D.HandlePool.HighestPossiblyClaimedId + 1);
            }
        }

        /// <summary>
        /// Returns all held resources.
        /// </summary>
        public void Dispose()
        {
            pool.Return(ref data);
        }
    }
}
