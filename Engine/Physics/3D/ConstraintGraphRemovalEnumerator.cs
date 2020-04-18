using Engine.Physics._3D.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Physics._3D
{
    /// <summary>
    /// Enumerates the bodies3D attached to an active constraint and removes the constraint's handle from all of the connected body constraint reference lists.
    /// </summary>
    struct ConstraintGraphRemovalEnumerator : IForEach<int>
    {
        internal Bodies3D bodies3D;
        internal int constraintHandle;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LoopBody(int bodyIndex)
        {
            //Note that this only looks in the active set. Directly removing inactive objects is unsupported- removals and adds activate all involved islands.
            bodies3D.RemoveConstraintReference(bodyIndex, constraintHandle);
        }
    }

}
