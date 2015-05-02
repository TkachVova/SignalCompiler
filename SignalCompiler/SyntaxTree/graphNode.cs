using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SignalCompiler.SyntaxTree
{
    public class GraphNode
    {
        [XmlAttribute]
        public nodesTypes Id;
        [XmlAttribute]
        public string Label;

        public GraphNode(nodesTypes id, string label)
        {
            this.Id = id;
            this.Label = label;
        }
        public GraphNode()
        {
        }
    }
}
