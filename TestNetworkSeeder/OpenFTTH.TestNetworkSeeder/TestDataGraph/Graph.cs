using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGraph
{
    public class Graph
    {
        public Dictionary<Guid, Edge> Edges = new Dictionary<Guid, Edge>();

        public Dictionary<Guid, Node> Nodes = new Dictionary<Guid, Node>();

        private UndirectedGraph<Guid, Edge<Guid>> _g;

        private Dictionary<object, Guid> _edgeToLinkId = new Dictionary<object, Guid>();


        public List<Guid> ShortestPath(Guid fromNodeId, Guid toNodeId)
        {
            List<Guid> result = new List<Guid>();
            //Func<Edge<string>, double> lineDistances = e => 1; // constant cost

            Func<Edge<Guid>, double> lineDistances = e => Edges[_edgeToLinkId[e]].Length;

            TryFunc<Guid, IEnumerable<Edge<Guid>>> tryGetPath = GetGraphForTracing().ShortestPathsDijkstra(lineDistances, fromNodeId);

            IEnumerable<Edge<Guid>> path;
            tryGetPath(toNodeId, out path);

            if (path != null)
            {
                foreach (var edge in path)
                {
                    result.Add(_edgeToLinkId[edge]);
                }
            }

            return result;
        }

        public List<Guid> ShortestPathOnGraphSubset(Guid fromNodeId, Guid toNodeId, List<Guid> nodes)
        {
            List<Guid> result = new List<Guid>();

            // For fast node existence check
            HashSet<Guid> nodeCheck = new HashSet<Guid>();
            foreach (var nodeId in nodes)
                nodeCheck.Add(nodeId);

            Dictionary<object, Guid> tempEdgeToLinkId = new Dictionary<object, Guid>();

            // Create temp graph with 
            var tempGraph = new UndirectedGraph<Guid, Edge<Guid>>();

            // Add vertices and edges
            foreach (var nodeId in nodes)
            {
                var node = Nodes[nodeId];

                tempGraph.AddVertex(node.Id);
            }

            foreach (var nodeId in nodes)
            {
                var node = Nodes[nodeId];

                foreach (var link in node.Edges)
                {
                    if (nodeCheck.Contains(link.StartNode.Id) && nodeCheck.Contains(link.EndNode.Id))
                    {

                        var edge = new Edge<Guid>(link.StartNode.Id, link.EndNode.Id);

                        tempGraph.AddEdge(edge);
                        tempEdgeToLinkId.Add(edge, link.Id);
                    }
                }
            }

            //Func<Edge<string>, double> lineDistances = e => 1; // constant cost

            Func<Edge<Guid>, double> lineDistances = e => Edges[tempEdgeToLinkId[e]].Length;

            TryFunc<Guid, IEnumerable<Edge<Guid>>> tryGetPath = tempGraph.ShortestPathsDijkstra(lineDistances, fromNodeId);

            IEnumerable<Edge<Guid>> path;
            tryGetPath(toNodeId, out path);

            if (path != null)
            {
                foreach (var edge in path)
                {
                    result.Add(tempEdgeToLinkId[edge]);
                }
            }

            return result;
        }

        public List<Guid> FindLinkPathEnds(List<Guid> links)
        {
            List<Guid> result = new List<Guid>();

            foreach (var linkId in links)
            {
                var graphLink = Edges[linkId];

                // Check if we find no links (in the links list) related to the start node. If that's the case, it's an end
                bool linkStartFound = true;

                foreach (var startLink in graphLink.StartNode.Edges)
                {
                    if (startLink.Id != linkId && links.Exists(id => id == startLink.Id))
                        linkStartFound = false;
                }

                if (linkStartFound)
                {
                    result.Add(linkId);
                }


                // Check if we find no links (in the links list) related to the end node. If that's the case, it's an end
                bool linkEndFound = true;

                foreach (var endLink in graphLink.EndNode.Edges)
                {
                    if (endLink.Id != linkId && links.Exists(id => id == endLink.Id))
                        linkEndFound = false;
                }

                if (linkEndFound)
                {
                    result.Add(linkId);
                }
            }

            return result;
        }

        public List<Guid> SortLinkPath(List<Guid> links)
        {
            if (links.Count == 1)
                return new List<Guid>() { links[0] };

            var ends = FindLinkPathEnds(links);

            if (ends.Count < 1)
                throw new NotSupportedException("No ends found in path. Make sure the links represent a path (not a trail or walk with repeating edges or vertices). Links: " + IdStringList(links));

            if (ends.Count == 1)
                throw new NotSupportedException("Only one end found in path. Make sure the links represent a path (not a trail or walk with repeating edges or vertices). Links: " + IdStringList(links));

            if (ends.Count > 2)
                throw new NotSupportedException(ends.Count + " found in path. Make sure the links represent a connected path. Ends: " + IdStringList(ends));

            List<Guid> linksSorted = new List<Guid>();
            List<Guid> linksRemaning = new List<Guid>();
            linksRemaning.AddRange(links);

            Guid? currentId = ends[0];
            linksRemaning.Remove(currentId.Value);
            
            while (currentId != null)
            {
                linksSorted.Add(currentId.Value);

                var currentLink = Edges[currentId.Value];
                currentId = null;

                foreach (var neighborLink in currentLink.NeighborEdges())
                {
                    if (linksRemaning.Contains(neighborLink))
                    {
                        currentId = neighborLink;
                        linksRemaning.Remove(neighborLink);
                    }
                }
            }

            if (linksSorted.Count != links.Count)
                throw new NotSupportedException("Only " + linksSorted.Count + " out of " + links.Count + " could be sorted. Make sure the links represent a connected path. Links:" + IdStringList(links));

            return linksSorted;

        }

        /// <summary>
        /// Returns a list of node-link-node-link-node etc. from a list of link (ids)
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public List<Guid> GetNodeLinkPathFromLinkPath(List<Guid> links)
        {
            List<Guid> result = new List<Guid>();

            // Do a sort, also to check if link represent a valid path
            var sortedLinks = SortLinkPath(links);

            bool firstLink = true;

            Node prevNode = null;

            foreach (var linkId in sortedLinks)
            {
                var currentLink = Edges[linkId];

                if (firstLink)
                {
                    // if more than one link, we need start with the right node
                    if (sortedLinks.Count > 1)
                    {
                        if (!currentLink.StartNode.Edges.Contains(Edges[sortedLinks[1]]))
                        {
                            result.Add(currentLink.StartNode.Id);
                            prevNode = currentLink.StartNode;
                        }
                        else
                        {
                            result.Add(currentLink.EndNode.Id);
                            prevNode = currentLink.EndNode;
                        }
                    }
                    else
                    {
                        result.Add(currentLink.StartNode.Id);
                        prevNode = currentLink.StartNode;
                    }
                }

                // add the link
                result.Add(linkId);

                // add the node
                var nextNode = GetLinkOtherEnd(Edges[linkId], prevNode);
                result.Add(nextNode.Id);

                prevNode = nextNode;
                firstLink = false;
            }

            return result;
        }

        private Node GetLinkOtherEnd(Edge link, Node end)
        {
            if (link.StartNode == end)
                return link.EndNode;
            else
                return link.StartNode;
        }


        private string IdStringList(List<Guid> ids)
        {
            string idStr = "";
            foreach (var id in ids)
            {
                if (idStr.Length > 1)
                    idStr += ",";

                idStr += id;
            }

            return idStr;
        }


        private UndirectedGraph<Guid, Edge<Guid>> GetGraphForTracing()
        {
            if (_g != null)
                return _g;

            _g = new UndirectedGraph<Guid, Edge<Guid>>();

            foreach (var node in Nodes)
            {
                _g.AddVertex(node.Key);
            }

            foreach (var link in Edges)
            {
                var edge = new Edge<Guid>(link.Value.StartNode.Id, link.Value.EndNode.Id);
                _g.AddEdge(edge);
                _edgeToLinkId.Add(edge, link.Key);
            }

            return _g;
        }
    }



   

}
