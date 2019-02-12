using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UiPath.IntelligentOCR.FC;

namespace Omnius
{
    public static class XmlExstensions
    {
        public static Tnode FindSingleNode<Tnode>(this XNode node, Func<Tnode, bool> criteria) where Tnode : XNode
        {
            return node.FindNodes(criteria).Single();
        }

        public static String ID(this XElement node)
        {
            return node.Attribute("id")?.Value;
        }

        public static Tnode FindNode<Tnode>(this XNode node, Func<Tnode, bool> criteria) where Tnode : XNode
        {
            return node.FindNodes(criteria).FirstOrDefault();
        }

        public static List<Tnode> FindImmediateChildren<Tnode>(this XContainer node, Func<Tnode, bool> criteria) where Tnode : XNode
        {
            return node.Nodes()
                       .Where(x => x is Tnode)
                       .Select(x => (Tnode)x)
                       .Where(criteria)
                       .ToList();
        }

        public static List<Tnode> FindNodes<Tnode>(this XNode node, Func<Tnode, bool> criteria) where Tnode : XNode
        {
            if (node is Tnode && criteria(node as Tnode))
            {
                return new Tnode[] { node as Tnode }.ToList();
            }

            if (node is XContainer)
            {
                return ((XContainer)node).Nodes().SelectMany(x => x.FindNodes<Tnode>(criteria)).ToList();
            }

            return new List<Tnode>();

        }
    }
}
