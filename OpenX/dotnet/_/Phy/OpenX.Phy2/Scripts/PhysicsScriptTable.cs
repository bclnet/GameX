using ACE.Entity.Enum;
using System.Collections.Generic;

namespace OpenX.Physics
{
    public class PhysicsScriptTable
    {
        public Dictionary<long, PhysicsScriptTableData> ScriptTable;

        public void Release()
        {

        }

        public uint GetScript(PlayScript? type, float? mod)
        {
            return 0;
        }
    }
}
