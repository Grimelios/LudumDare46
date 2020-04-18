﻿using Engine.Physics._3D.Collidables;
using Engine.Physics._3D.CollisionDetection.CollisionTasks;
using Engine.Physics._3D.Utility;
using Engine.Physics._3D.Utility.Collections;
using Engine.Physics._3D.Utility.Memory;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.Physics._3D.CollisionDetection.SweepTasks
{
    //At the moment, this is basically an unused abstraction. But, if you wanted, this allows you to use a special cased overlap finder in certain cases.
    public interface IConvexCompoundSweepOverlapFinder<TShapeA, TCompoundB> where TShapeA : struct, IConvexShape where TCompoundB : struct, IBoundsQueryableCompound
    {
        unsafe void FindOverlaps(ref TShapeA shapeA, in Quaternion orientationA, in BodyVelocity velocityA,
              ref TCompoundB compoundB, in Vector3 offsetB, in Quaternion orientationB, in BodyVelocity velocityB, float maximumT,
              PhysicsShapes3D shapes, BufferPool pool, out ChildOverlapsCollection overlaps);
    }

    public struct ConvexCompoundSweepOverlapFinder<TShapeA, TCompoundB> : IConvexCompoundSweepOverlapFinder<TShapeA, TCompoundB>
        where TShapeA : struct, IConvexShape where TCompoundB : struct, IBoundsQueryableCompound
    {
        public unsafe void FindOverlaps(ref TShapeA shapeA, in Quaternion orientationA, in BodyVelocity velocityA,
            ref TCompoundB compoundB, in Vector3 offsetB, in Quaternion orientationB, in BodyVelocity velocityB, float maximumT,
            PhysicsShapes3D shapes, BufferPool pool, out ChildOverlapsCollection overlaps)
        {
            BoundingBoxHelpers.GetLocalBoundingBoxForSweep(ref shapeA, default, orientationA, velocityA, offsetB, orientationB, velocityB, maximumT, out var sweep, out var min, out var max);
            
            overlaps = default;
            compoundB.FindLocalOverlaps<ChildOverlapsCollection>(min, max, sweep, maximumT, pool, shapes, Unsafe.AsPointer(ref overlaps));
        }
    }
}
