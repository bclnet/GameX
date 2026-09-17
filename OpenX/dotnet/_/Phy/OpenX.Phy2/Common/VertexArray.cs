using System.Collections.Generic;
using ACE.Entity.Enum;
using ACE.Server.Physics.Entity;

namespace OpenX.Physics.Common
{
    public class VertexArray
    {
        public VertexType Type;
        public List<Vertex> Vertices;

        public VertexArray()
        {
            Vertices = new List<Vertex>();
        }

        public VertexArray(VertexType type, int numVerts)
        {
            Vertices = new List<Vertex>(numVerts);
            Type = type;
        }
    }
}
