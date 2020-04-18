﻿using Engine.Physics._3D.Collidables;
using Engine.Physics._3D.CollisionDetection.CollisionTasks;
using Engine.Physics._3D.Utility;
using System.Numerics;

namespace Engine.Physics._3D.CollisionDetection.SweepTasks
{
    public struct SphereCylinderDistanceTester : IPairDistanceTester<SphereWide, CylinderWide>
    {
        public void Test(in SphereWide a, in CylinderWide b, in Vector3Wide offsetB, in QuaternionWide orientationA, in QuaternionWide orientationB, in Vector<int> inactiveLanes,
            out Vector<int> intersected, out Vector<float> distance, out Vector3Wide closestA, out Vector3Wide normal)
        {
            Matrix3x3Wide.CreateFromQuaternion(orientationB, out var orientationMatrixB);
            SphereCylinderTester.ComputeSphereToClosest(b, offsetB, orientationMatrixB, out _, out _, out _, out _, out _, out closestA);

            Vector3Wide.Length(closestA, out var contactDistanceFromSphereCenter);
            //Note negation; normal points from B to A by convention.
            Vector3Wide.Scale(closestA, new Vector<float>(-1) / contactDistanceFromSphereCenter, out normal);
            distance = contactDistanceFromSphereCenter - a.Radius;
            intersected = Vector.LessThan(distance, Vector<float>.Zero);
        }
    }
}
