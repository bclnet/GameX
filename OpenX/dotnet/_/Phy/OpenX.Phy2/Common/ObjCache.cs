using System.Collections.Generic;
using ACE.Server.Physics.Animation;

namespace OpenX.Physics.Common
{
    public static class ObjCache
    {
        public static Dictionary<uint, MotionTable> MotionTables;

        static ObjCache()
        {
            MotionTables = new Dictionary<uint, MotionTable>();
        }

        public static MotionTable GetMotionTable(uint id)
        {
            MotionTable mtable = null;
            MotionTables.TryGetValue(id, out mtable);
            return mtable;
        }
    }
}
