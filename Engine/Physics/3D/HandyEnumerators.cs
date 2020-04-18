using Engine.Physics._3D.Constraints;
using Engine.Physics._3D.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Physics._3D
{
    /// <summary>
    /// Collects body handles associated with an active constraint.
    /// </summary>
    public unsafe struct ActiveConstraintBodyHandleCollector : IForEach<int> 
    {
        public Bodies3D bodies3D;
        public int* Handles;
        public int Index;

        public ActiveConstraintBodyHandleCollector(Bodies3D bodies3D, int* handles)
        {
            this.bodies3D = bodies3D;
            Handles = handles;
            Index = 0;
        }

        public void LoopBody(int bodyIndex)
        {
            Handles[Index++] = bodies3D.ActiveSet.IndexToHandle[bodyIndex];
        }
    }

    public unsafe struct ReferenceCollector : IForEach<int>
    {
        public int* References;
        public int Index;

        public ReferenceCollector(int* references)
        {
            References = references;
            Index = 0;
        }

        public void LoopBody(int reference)
        {
            References[Index++] = reference;
        }
    }


    public unsafe struct FloatCollector : IForEach<float>
    {
        public float* Values;
        public int Index;

        public FloatCollector(float* values)
        {
            Values = values;
            Index = 0;
        }

        public void LoopBody(float value)
        {
            Values[Index++] = value;
        }
    }
}
