using Engine.Physics._3D.Constraints;
using Engine.Physics._3D.CollisionDetection;
using Engine.Physics._3D.CollisionDetection.CollisionTasks;
using Engine.Physics._3D.Constraints.Contact;
using Engine.Physics._3D.Collidables;
using Engine.Physics._3D.CollisionDetection.SweepTasks;

namespace Engine.Physics._3D
{
    /// <summary>
    /// Helper class to register the default types within a simulation instance.
    /// </summary>
    public static class DefaultTypes
    {
        /// <summary>
        /// Registers the set of constraints that are packaged in the engine.
        /// </summary>
        public static void RegisterDefaults(Solver solver, NarrowPhase narrowPhase)
        {
            solver.Register<BallSocket>();
            solver.Register<AngularHinge>();
            solver.Register<AngularSwivelHinge>();
            solver.Register<SwingLimit>();
            solver.Register<TwistServo>();
            solver.Register<TwistLimit>();
            solver.Register<TwistMotor>();
            solver.Register<AngularServo>();
            solver.Register<AngularMotor>();
            solver.Register<Weld>();
            solver.Register<VolumeConstraint>();
            solver.Register<DistanceServo>();
            solver.Register<DistanceLimit>();
            solver.Register<CenterDistanceConstraint>();
            solver.Register<AreaConstraint>();
            solver.Register<PointOnLineServo>();
            solver.Register<LinearAxisServo>();
            solver.Register<LinearAxisMotor>();
            solver.Register<LinearAxisLimit>();
            solver.Register<AngularAxisMotor>();
            solver.Register<OneBodyAngularServo>();
            solver.Register<OneBodyAngularMotor>();
            solver.Register<OneBodyLinearServo>();
            solver.Register<OneBodyLinearMotor>();
            solver.Register<SwivelHinge>();
            solver.Register<Hinge>();
            solver.Register<BallSocketMotor>();
            solver.Register<BallSocketServo>();

            solver.Register<Contact1OneBody>();
            solver.Register<Contact2OneBody>();
            solver.Register<Contact3OneBody>();
            solver.Register<Contact4OneBody>();
            solver.Register<Contact1>();
            solver.Register<Contact2>();
            solver.Register<Contact3>();
            solver.Register<Contact4>();
            solver.Register<Contact2NonconvexOneBody>();
            solver.Register<Contact3NonconvexOneBody>();
            solver.Register<Contact4NonconvexOneBody>();
            //We may later use more contacts in the nonconvex manifold; for now we clamped it to 4.
            solver.Register<Contact2Nonconvex>();
            solver.Register<Contact3Nonconvex>();
            solver.Register<Contact4Nonconvex>();

            narrowPhase.RegisterContactConstraintAccessor(new NonconvexTwoBodyAccessor<Contact4Nonconvex, Contact4NonconvexPrestepData, Contact4NonconvexAccumulatedImpulses, ContactImpulses4, ConstraintCache4>());
            narrowPhase.RegisterContactConstraintAccessor(new NonconvexTwoBodyAccessor<Contact3Nonconvex, Contact3NonconvexPrestepData, Contact3NonconvexAccumulatedImpulses, ContactImpulses3, ConstraintCache3>());
            narrowPhase.RegisterContactConstraintAccessor(new NonconvexTwoBodyAccessor<Contact2Nonconvex, Contact2NonconvexPrestepData, Contact2NonconvexAccumulatedImpulses, ContactImpulses2, ConstraintCache2>());

            narrowPhase.RegisterContactConstraintAccessor(new NonconvexOneBodyAccessor<Contact4NonconvexOneBody, Contact4NonconvexOneBodyPrestepData, Contact4NonconvexAccumulatedImpulses, ContactImpulses4, ConstraintCache4>());
            narrowPhase.RegisterContactConstraintAccessor(new NonconvexOneBodyAccessor<Contact3NonconvexOneBody, Contact3NonconvexOneBodyPrestepData, Contact3NonconvexAccumulatedImpulses, ContactImpulses3, ConstraintCache3>());
            narrowPhase.RegisterContactConstraintAccessor(new NonconvexOneBodyAccessor<Contact2NonconvexOneBody, Contact2NonconvexOneBodyPrestepData, Contact2NonconvexAccumulatedImpulses, ContactImpulses2, ConstraintCache2>());

            narrowPhase.RegisterContactConstraintAccessor(new ConvexTwoBodyAccessor<Contact4, Contact4PrestepData, Contact4AccumulatedImpulses, ContactImpulses4, ConstraintCache4>());
            narrowPhase.RegisterContactConstraintAccessor(new ConvexTwoBodyAccessor<Contact3, Contact3PrestepData, Contact3AccumulatedImpulses, ContactImpulses3, ConstraintCache3>());
            narrowPhase.RegisterContactConstraintAccessor(new ConvexTwoBodyAccessor<Contact2, Contact2PrestepData, Contact2AccumulatedImpulses, ContactImpulses2, ConstraintCache2>());
            narrowPhase.RegisterContactConstraintAccessor(new ConvexTwoBodyAccessor<Contact1, Contact1PrestepData, Contact1AccumulatedImpulses, ContactImpulses1, ConstraintCache1>());
            narrowPhase.RegisterContactConstraintAccessor(new ConvexOneBodyAccessor<Contact4OneBody, Contact4OneBodyPrestepData, Contact4AccumulatedImpulses, ContactImpulses4, ConstraintCache4>());
            narrowPhase.RegisterContactConstraintAccessor(new ConvexOneBodyAccessor<Contact3OneBody, Contact3OneBodyPrestepData, Contact3AccumulatedImpulses, ContactImpulses3, ConstraintCache3>());
            narrowPhase.RegisterContactConstraintAccessor(new ConvexOneBodyAccessor<Contact2OneBody, Contact2OneBodyPrestepData, Contact2AccumulatedImpulses, ContactImpulses2, ConstraintCache2>());
            narrowPhase.RegisterContactConstraintAccessor(new ConvexOneBodyAccessor<Contact1OneBody, Contact1OneBodyPrestepData, Contact1AccumulatedImpulses, ContactImpulses1, ConstraintCache1>());

        }

        /// <summary>
        /// Creates a task registry containing the default collision pair types.
        /// </summary>
        public static CollisionTaskRegistry CreateDefaultCollisionTaskRegistry()
        {
            var defaultTaskRegistry = new CollisionTaskRegistry();
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsSphere, SphereWide, PhysicsSphere, SphereWide, SpherePair, SpherePairWide, Convex1ContactManifoldWide, SpherePairTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsSphere, SphereWide, PhysicsCapsule3D, CapsuleWide, SphereIncludingPair, SphereIncludingPairWide<PhysicsCapsule3D, CapsuleWide>, Convex1ContactManifoldWide, SphereCapsuleTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsSphere, SphereWide, PhysicsBox, BoxWide, SphereIncludingPair, SphereIncludingPairWide<PhysicsBox, BoxWide>, Convex1ContactManifoldWide, SphereBoxTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsSphere, SphereWide, Triangle, TriangleWide, SphereIncludingPair, SphereIncludingPairWide<Triangle, TriangleWide>, Convex1ContactManifoldWide, SphereTriangleTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsSphere, SphereWide, PhysicsCylinder, CylinderWide, SphereIncludingPair, SphereIncludingPairWide<PhysicsCylinder, CylinderWide>, Convex1ContactManifoldWide, SphereCylinderTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsSphere, SphereWide, ConvexHull, ConvexHullWide, SphereIncludingPair, SphereIncludingPairWide<ConvexHull, ConvexHullWide>, Convex1ContactManifoldWide, SphereConvexHullTester>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsSphere, Compound, ConvexCompoundOverlapFinder<PhysicsSphere, SphereWide, Compound>, ConvexCompoundContinuations<Compound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsSphere, BigCompound, ConvexCompoundOverlapFinder<PhysicsSphere, SphereWide, BigCompound>, ConvexCompoundContinuations<BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsSphere, Mesh, ConvexCompoundOverlapFinder<PhysicsSphere, SphereWide, Mesh>, ConvexMeshContinuations<Mesh>, MeshReduction>());

            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsCapsule3D, CapsuleWide, PhysicsCapsule3D, CapsuleWide, FliplessPair, FliplessPairWide<PhysicsCapsule3D, CapsuleWide>, Convex2ContactManifoldWide, CapsulePairTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsCapsule3D, CapsuleWide, PhysicsBox, BoxWide, CollisionPair, ConvexPairWide<PhysicsCapsule3D, CapsuleWide, PhysicsBox, BoxWide>, Convex2ContactManifoldWide, CapsuleBoxTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsCapsule3D, CapsuleWide, Triangle, TriangleWide, CollisionPair, ConvexPairWide<PhysicsCapsule3D, CapsuleWide, Triangle, TriangleWide>, Convex2ContactManifoldWide, CapsuleTriangleTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsCapsule3D, CapsuleWide, PhysicsCylinder, CylinderWide, CollisionPair, ConvexPairWide<PhysicsCapsule3D, CapsuleWide, PhysicsCylinder, CylinderWide>, Convex2ContactManifoldWide, CapsuleCylinderTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsCapsule3D, CapsuleWide, ConvexHull, ConvexHullWide, CollisionPair, ConvexPairWide<PhysicsCapsule3D, CapsuleWide, ConvexHull, ConvexHullWide>, Convex2ContactManifoldWide, CapsuleConvexHullTester>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsCapsule3D, Compound, ConvexCompoundOverlapFinder<PhysicsCapsule3D, CapsuleWide, Compound>, ConvexCompoundContinuations<Compound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsCapsule3D, BigCompound, ConvexCompoundOverlapFinder<PhysicsCapsule3D, CapsuleWide, BigCompound>, ConvexCompoundContinuations<BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsCapsule3D, Mesh, ConvexCompoundOverlapFinder<PhysicsCapsule3D, CapsuleWide, Mesh>, ConvexMeshContinuations<Mesh>, MeshReduction>());

            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsBox, BoxWide, PhysicsBox, BoxWide, FliplessPair, FliplessPairWide<PhysicsBox, BoxWide>, Convex4ContactManifoldWide, BoxPairTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsBox, BoxWide, Triangle, TriangleWide, CollisionPair, ConvexPairWide<PhysicsBox, BoxWide, Triangle, TriangleWide>, Convex4ContactManifoldWide, BoxTriangleTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsBox, BoxWide, PhysicsCylinder, CylinderWide, CollisionPair, ConvexPairWide<PhysicsBox, BoxWide, PhysicsCylinder, CylinderWide>, Convex4ContactManifoldWide, BoxCylinderTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsBox, BoxWide, ConvexHull, ConvexHullWide, CollisionPair, ConvexPairWide<PhysicsBox, BoxWide, ConvexHull, ConvexHullWide>, Convex4ContactManifoldWide, BoxConvexHullTester>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsBox, Compound, ConvexCompoundOverlapFinder<PhysicsBox, BoxWide, Compound>, ConvexCompoundContinuations<Compound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsBox, BigCompound, ConvexCompoundOverlapFinder<PhysicsBox, BoxWide, BigCompound>, ConvexCompoundContinuations<BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsBox, Mesh, ConvexCompoundOverlapFinder<PhysicsBox, BoxWide, Mesh>, ConvexMeshContinuations<Mesh>, MeshReduction>());

            defaultTaskRegistry.Register(new ConvexCollisionTask<Triangle, TriangleWide, Triangle, TriangleWide, FliplessPair, FliplessPairWide<Triangle, TriangleWide>, Convex4ContactManifoldWide, TrianglePairTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<Triangle, TriangleWide, PhysicsCylinder, CylinderWide, CollisionPair, ConvexPairWide<Triangle, TriangleWide, PhysicsCylinder, CylinderWide>, Convex4ContactManifoldWide, TriangleCylinderTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<Triangle, TriangleWide, ConvexHull, ConvexHullWide, CollisionPair, ConvexPairWide<Triangle, TriangleWide, ConvexHull, ConvexHullWide>, Convex4ContactManifoldWide, TriangleConvexHullTester>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<Triangle, Compound, ConvexCompoundOverlapFinder<Triangle, TriangleWide, Compound>, ConvexCompoundContinuations<Compound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<Triangle, BigCompound, ConvexCompoundOverlapFinder<Triangle, TriangleWide, BigCompound>, ConvexCompoundContinuations<BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<Triangle, Mesh, ConvexCompoundOverlapFinder<Triangle, TriangleWide, Mesh>, ConvexMeshContinuations<Mesh>, MeshReduction>());

            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsCylinder, CylinderWide, PhysicsCylinder, CylinderWide, FliplessPair, FliplessPairWide<PhysicsCylinder, CylinderWide>, Convex4ContactManifoldWide, CylinderPairTester>());
            defaultTaskRegistry.Register(new ConvexCollisionTask<PhysicsCylinder, CylinderWide, ConvexHull, ConvexHullWide, CollisionPair, ConvexPairWide<PhysicsCylinder, CylinderWide, ConvexHull, ConvexHullWide>, Convex4ContactManifoldWide, CylinderConvexHullTester>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsCylinder, Compound, ConvexCompoundOverlapFinder<PhysicsCylinder, CylinderWide, Compound>, ConvexCompoundContinuations<Compound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsCylinder, BigCompound, ConvexCompoundOverlapFinder<PhysicsCylinder, CylinderWide, BigCompound>, ConvexCompoundContinuations<BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<PhysicsCylinder, Mesh, ConvexCompoundOverlapFinder<PhysicsCylinder, CylinderWide, Mesh>, ConvexMeshContinuations<Mesh>, MeshReduction>());

            defaultTaskRegistry.Register(new ConvexCollisionTask<ConvexHull, ConvexHullWide, ConvexHull, ConvexHullWide, FliplessPair, FliplessPairWide<ConvexHull, ConvexHullWide>, Convex4ContactManifoldWide, ConvexHullPairTester>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<ConvexHull, Compound, ConvexCompoundOverlapFinder<ConvexHull, ConvexHullWide, Compound>, ConvexCompoundContinuations<Compound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<ConvexHull, BigCompound, ConvexCompoundOverlapFinder<ConvexHull, ConvexHullWide, BigCompound>, ConvexCompoundContinuations<BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new ConvexCompoundCollisionTask<ConvexHull, Mesh, ConvexCompoundOverlapFinder<ConvexHull, ConvexHullWide, Mesh>, ConvexMeshContinuations<Mesh>, MeshReduction>());

            defaultTaskRegistry.Register(new CompoundPairCollisionTask<Compound, Compound, CompoundPairOverlapFinder<Compound, Compound>, CompoundPairContinuations<Compound, Compound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new CompoundPairCollisionTask<Compound, BigCompound, CompoundPairOverlapFinder<Compound, BigCompound>, CompoundPairContinuations<Compound, BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new CompoundPairCollisionTask<Compound, Mesh, CompoundPairOverlapFinder<Compound, Mesh>, CompoundMeshContinuations<Compound, Mesh>, CompoundMeshReduction>());

            defaultTaskRegistry.Register(new CompoundPairCollisionTask<BigCompound, BigCompound, CompoundPairOverlapFinder<BigCompound, BigCompound>, CompoundPairContinuations<BigCompound, BigCompound>, NonconvexReduction>());
            defaultTaskRegistry.Register(new CompoundPairCollisionTask<BigCompound, Mesh, CompoundPairOverlapFinder<BigCompound, Mesh>, CompoundMeshContinuations<BigCompound, Mesh>, CompoundMeshReduction>());

            defaultTaskRegistry.Register(new CompoundPairCollisionTask<Mesh, Mesh, MeshPairOverlapFinder<Mesh, Mesh>, MeshPairContinuations<Mesh, Mesh>, CompoundMeshReduction>());

            return defaultTaskRegistry;
        }

        /// <summary>
        /// Creates a task registry containing the default sweep task types.
        /// </summary>
        public static SweepTaskRegistry CreateDefaultSweepTaskRegistry()
        {
            var defaultTaskRegistry = new SweepTaskRegistry();
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsSphere, SphereWide, PhysicsSphere, SphereWide, SpherePairDistanceTester>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsSphere, SphereWide, PhysicsCapsule3D, CapsuleWide, SphereCapsuleDistanceTester>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsSphere, SphereWide, PhysicsCylinder, CylinderWide, SphereCylinderDistanceTester>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsSphere, SphereWide, PhysicsBox, BoxWide, SphereBoxDistanceTester>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsSphere, SphereWide, Triangle, TriangleWide, SphereTriangleDistanceTester>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsSphere, SphereWide, ConvexHull, ConvexHullWide, GJKDistanceTester<PhysicsSphere, SphereWide, SphereSupportFinder, ConvexHull, ConvexHullWide, ConvexHullSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsSphere, SphereWide, Compound, ConvexCompoundSweepOverlapFinder<PhysicsSphere, Compound>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsSphere, SphereWide, BigCompound, ConvexCompoundSweepOverlapFinder<PhysicsSphere, BigCompound>>());
            defaultTaskRegistry.Register(new ConvexHomogeneousCompoundSweepTask<PhysicsSphere, SphereWide, Mesh, Triangle, TriangleWide, ConvexCompoundSweepOverlapFinder<PhysicsSphere, Mesh>>());

            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCapsule3D, CapsuleWide, PhysicsCapsule3D, CapsuleWide, CapsulePairDistanceTester>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCapsule3D, CapsuleWide, PhysicsCylinder, CylinderWide, GJKDistanceTester<PhysicsCapsule3D, CapsuleWide, CapsuleSupportFinder, PhysicsCylinder, CylinderWide, CylinderSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCapsule3D, CapsuleWide, PhysicsBox, BoxWide, CapsuleBoxDistanceTester>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCapsule3D, CapsuleWide, Triangle, TriangleWide, GJKDistanceTester<PhysicsCapsule3D, CapsuleWide, CapsuleSupportFinder, Triangle, TriangleWide, TriangleSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCapsule3D, CapsuleWide, ConvexHull, ConvexHullWide, GJKDistanceTester<PhysicsCapsule3D, CapsuleWide, CapsuleSupportFinder, ConvexHull, ConvexHullWide, ConvexHullSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsCapsule3D, CapsuleWide, Compound, ConvexCompoundSweepOverlapFinder<PhysicsCapsule3D, Compound>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsCapsule3D, CapsuleWide, BigCompound, ConvexCompoundSweepOverlapFinder<PhysicsCapsule3D, BigCompound>>());
            defaultTaskRegistry.Register(new ConvexHomogeneousCompoundSweepTask<PhysicsCapsule3D, CapsuleWide, Mesh, Triangle, TriangleWide, ConvexCompoundSweepOverlapFinder<PhysicsCapsule3D, Mesh>>());

            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCylinder, CylinderWide, PhysicsCylinder, CylinderWide, GJKDistanceTester<PhysicsCylinder, CylinderWide, CylinderSupportFinder, PhysicsCylinder, CylinderWide, CylinderSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCylinder, CylinderWide, PhysicsBox, BoxWide, GJKDistanceTester<PhysicsCylinder, CylinderWide, CylinderSupportFinder, PhysicsBox, BoxWide, BoxSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCylinder, CylinderWide, Triangle, TriangleWide, GJKDistanceTester<PhysicsCylinder, CylinderWide, CylinderSupportFinder, Triangle, TriangleWide, TriangleSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsCylinder, CylinderWide, ConvexHull, ConvexHullWide, GJKDistanceTester<PhysicsCylinder, CylinderWide, CylinderSupportFinder, ConvexHull, ConvexHullWide, ConvexHullSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsCylinder, CylinderWide, Compound, ConvexCompoundSweepOverlapFinder<PhysicsCylinder, Compound>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsCylinder, CylinderWide, BigCompound, ConvexCompoundSweepOverlapFinder<PhysicsCylinder, BigCompound>>());
            defaultTaskRegistry.Register(new ConvexHomogeneousCompoundSweepTask<PhysicsCylinder, CylinderWide, Mesh, Triangle, TriangleWide, ConvexCompoundSweepOverlapFinder<PhysicsCylinder, Mesh>>());

            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsBox, BoxWide, PhysicsBox, BoxWide, GJKDistanceTester<PhysicsBox, BoxWide, BoxSupportFinder, PhysicsBox, BoxWide, BoxSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsBox, BoxWide, Triangle, TriangleWide, GJKDistanceTester<PhysicsBox, BoxWide, BoxSupportFinder, Triangle, TriangleWide, TriangleSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<PhysicsBox, BoxWide, ConvexHull, ConvexHullWide, GJKDistanceTester<PhysicsBox, BoxWide, BoxSupportFinder, ConvexHull, ConvexHullWide, ConvexHullSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsBox, BoxWide, Compound, ConvexCompoundSweepOverlapFinder<PhysicsBox, Compound>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<PhysicsBox, BoxWide, BigCompound, ConvexCompoundSweepOverlapFinder<PhysicsBox, BigCompound>>());
            defaultTaskRegistry.Register(new ConvexHomogeneousCompoundSweepTask<PhysicsBox, BoxWide, Mesh, Triangle, TriangleWide, ConvexCompoundSweepOverlapFinder<PhysicsBox, Mesh>>());

            defaultTaskRegistry.Register(new ConvexPairSweepTask<Triangle, TriangleWide, Triangle, TriangleWide, GJKDistanceTester<Triangle, TriangleWide, TriangleSupportFinder, Triangle, TriangleWide, TriangleSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexPairSweepTask<Triangle, TriangleWide, ConvexHull, ConvexHullWide, GJKDistanceTester<Triangle, TriangleWide, TriangleSupportFinder, ConvexHull, ConvexHullWide, ConvexHullSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<Triangle, TriangleWide, Compound, ConvexCompoundSweepOverlapFinder<Triangle, Compound>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<Triangle, TriangleWide, BigCompound, ConvexCompoundSweepOverlapFinder<Triangle, BigCompound>>());
            defaultTaskRegistry.Register(new ConvexHomogeneousCompoundSweepTask<Triangle, TriangleWide, Mesh, Triangle, TriangleWide, ConvexCompoundSweepOverlapFinder<Triangle, Mesh>>());

            defaultTaskRegistry.Register(new ConvexPairSweepTask<ConvexHull, ConvexHullWide, ConvexHull, ConvexHullWide, GJKDistanceTester<ConvexHull, ConvexHullWide, ConvexHullSupportFinder, ConvexHull, ConvexHullWide, ConvexHullSupportFinder>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<ConvexHull, ConvexHullWide, Compound, ConvexCompoundSweepOverlapFinder<ConvexHull, Compound>>());
            defaultTaskRegistry.Register(new ConvexCompoundSweepTask<ConvexHull, ConvexHullWide, BigCompound, ConvexCompoundSweepOverlapFinder<ConvexHull, BigCompound>>());
            defaultTaskRegistry.Register(new ConvexHomogeneousCompoundSweepTask<ConvexHull, ConvexHullWide, Mesh, Triangle, TriangleWide, ConvexCompoundSweepOverlapFinder<ConvexHull, Mesh>>());

            defaultTaskRegistry.Register(new CompoundPairSweepTask<Compound, Compound, CompoundPairSweepOverlapFinder<Compound, Compound>>());
            defaultTaskRegistry.Register(new CompoundPairSweepTask<Compound, BigCompound, CompoundPairSweepOverlapFinder<Compound, BigCompound>>());
            defaultTaskRegistry.Register(new CompoundHomogeneousCompoundSweepTask<Compound, Mesh, Triangle, TriangleWide, CompoundPairSweepOverlapFinder<Compound, Mesh>>());

            defaultTaskRegistry.Register(new CompoundPairSweepTask<BigCompound, BigCompound, CompoundPairSweepOverlapFinder<BigCompound, BigCompound>>());
            defaultTaskRegistry.Register(new CompoundHomogeneousCompoundSweepTask<BigCompound, Mesh, Triangle, TriangleWide, CompoundPairSweepOverlapFinder<BigCompound, Mesh>>());

            //TODO: No mesh-mesh at the moment.
            return defaultTaskRegistry;
        }
    }
}
