using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba5
{
    public class Fact
    {
        private int id;
        private bool isProved;

        public Fact(int id, bool isProved = false){ this.id = id; this.isProved = isProved; }
        public bool IsProved { get { return isProved; } set { isProved = value; } }
        public int Id { get { return id; }}
    }

    public class RuleNode
    {
        private Fact fact;
        private RuleNode parent;
        private List<List<RuleNode>> orPeremises;

        public RuleNode(Fact fact, RuleNode parent) { this.fact = fact;  this.parent = parent;
            orPeremises = new List<List<RuleNode>>(); }
        public Fact Fact { get { return fact; } }
        public RuleNode Parent { get { return parent; } }
        public List<List<RuleNode>> OrPremises { get { return orPeremises; } }
        public List<RuleNode> Premise (int i) { return orPeremises[i];}
        public int OrPremiseCount { get { return orPeremises.Count; } }

        public int PremiseCount(int ind)
        {
            if (orPeremises.Count < (ind+1)) throw new Exception("Выход за пределы диапазона");
            return orPeremises[ind].Count;
        }

        public bool IsPremiseProved(int i)
        {
            foreach (var node in OrPremises[i])
                if (!node.Fact.IsProved)
                    return false;
            return true;
        }

        public void AddOrPremise(HashSet<int> idsFact, HashSet<int> idsProvedFact)
        {
            List<RuleNode> list = new List<RuleNode>();
            foreach (int id in idsFact)
                if(id != this.fact.Id)
                    list.Add(new RuleNode(new Fact(id, idsProvedFact.Contains(id)), this));
            orPeremises.Add(list);
        }

        public bool IsOrPremiseProved
        {
            get
            {
                foreach (var orPeremise in orPeremises)
                { 
                    bool res = true;
                    foreach (RuleNode curPremise in orPeremise)
                    {
                        if (!curPremise.Fact.IsProved)
                            res = false;
                    }
                    if (res) return true;
                }
                return false;
            }
        }

        public HashSet<RuleNode> MinPremise()
        {
            RuleNode parent = this;
            int minWayInd = -1;
            int minWayCount = int.MaxValue;
            for (int i = 0; i < parent.OrPremiseCount; i++)
            {
                bool isPermiseProved = parent.IsPremiseProved(i);                
                int curPremiseCount = parent.PremiseCount(i);
                if (isPermiseProved && minWayCount > curPremiseCount)
                {
                    minWayCount = curPremiseCount;
                    minWayInd = i;
                }
            }   
            var res = minWayInd == -1? new HashSet<RuleNode>():
                new HashSet<RuleNode>(parent.Premise(minWayInd));
            return res;
        }//end of MinPremise()

        public List<RuleNode> OrPremiseTrueList
        {
            get
            {
                List<RuleNode> res = new List<RuleNode>();
                foreach (var orPeremise in orPeremises)
                {
                    foreach (RuleNode curPremise in orPeremise)
                    {
                        if (curPremise.Fact.IsProved && curPremise.OrPremiseCount == 0)
                            res.Add(curPremise);
                    }
                }
                return res;
            }
        }

    }

    public class BackwardOutput
    {
        private static HashSet<int> idsProvedFact = new HashSet<int>();

        private static List<List<HashSet<RuleNode>>> FindEvidenceBase(HashSet<RuleNode> ruleNodeProvedList)
        {
            List<List<HashSet<RuleNode>>> resBase = new List<List<HashSet<RuleNode>>>();
            LinkedList<RuleNode> provedList = new LinkedList<RuleNode>(ruleNodeProvedList);
            while(provedList.Count != 0)
            {
                RuleNode list = provedList.First();
                HashSet<RuleNode> minLists = list.Parent.MinPremise();
                List<RuleNode> allPremiseTrue = list.Parent.OrPremiseTrueList;
                foreach (var curList in allPremiseTrue)
                    provedList.Remove(curList);
                var way = new List<HashSet<RuleNode>>();
                var minListsReal = new HashSet<RuleNode>();
                foreach(var rN in minLists)
                {
                    if (rN.OrPremiseCount == 0)
                        minListsReal.Add(rN);
                }
                way.Add(minListsReal);
                var parent = list.Parent;
                while (parent!=null)
                {
                    var hS = new HashSet<RuleNode>();
                    hS.Add(parent);
                    way.Add(hS);
                    parent = parent.Parent;
                }
                resBase.Add(way);
            }
            return resBase;
        }

        private static bool UpTruePropagation(RuleNode premise, HashSet<RuleNode> ruleNodeProvedList)
        {
            premise.Fact.IsProved = true;
            ruleNodeProvedList.Add(premise);
            idsProvedFact.Add(premise.Fact.Id);
            RuleNode curProveNode = premise.Parent;
            while (curProveNode != null)
            {
                if (curProveNode.IsOrPremiseProved)
                {
                    curProveNode.Fact.IsProved = true;
                    idsProvedFact.Add(curProveNode.Fact.Id);
                }
                else
                    break;
                curProveNode = curProveNode.Parent;
            }
            return curProveNode == null;
        }

        public class QueueElem
        {
            public RuleNode node;
            public int orPeremiseInd;

            public QueueElem(RuleNode node, int orPeremiseInd)
            {
                this.node = node; this.orPeremiseInd = orPeremiseInd;
            }
        }

        public static HashSet<RuleNode> FindEvidenceBase(
            HashSet<int> needProve, List<Rule> rulesList, List<string> factsList)
        {
            HashSet<RuleNode> ruleNodeProvedList = new HashSet<RuleNode>();
            RuleNode root = new RuleNode(new Fact(-1), null);
            root.AddOrPremise(needProve, idsProvedFact);
            PriorityQueue<QueueElem> queue = new PriorityQueue<QueueElem>();
            queue.PriorityAdd(new QueueElem(root, 0), root.PremiseCount(0));
            bool isRootProved = false;
            #region algorithm
            while (queue.IsNotEmphty && !isRootProved)
            {
                QueueElem curQueueElem = queue.Dequeue();
                RuleNode curNode = curQueueElem.node;
                #region OrPremise for curNode
                List<RuleNode> orPremise = curNode.Premise(curQueueElem.orPeremiseInd);
                if (!curNode.Fact.IsProved) {
                    foreach (RuleNode ruleNode in orPremise)
                    {
                        if (!ruleNode.Fact.IsProved)
                        {
                            int indPremise = 0;
                            bool isList = true;
                            foreach (Rule rule in rulesList)
                            {
                                if (rule.idConclusion == ruleNode.Fact.Id)
                                {
                                    isList = false;
                                    ruleNode.AddOrPremise(rule.idsPremise, idsProvedFact);
                                    int premiseCount = ruleNode.PremiseCount(indPremise);
                                    if (premiseCount != 0)
                                    {
                                        if (!idsProvedFact.Contains(ruleNode.Fact.Id))
                                            queue.PriorityAdd(new QueueElem(ruleNode, indPremise), premiseCount);
                                    }
                                    else
                                        isRootProved = UpTruePropagation(ruleNode, ruleNodeProvedList);
                                    indPremise++;
                                }
                            }
                            if (isList)
                                isRootProved = UpTruePropagation(ruleNode, ruleNodeProvedList);
                        }
                    }
                }           
                #endregion
            }
            #endregion
            var evidenceBase = FindEvidenceBase(ruleNodeProvedList);
            var hashSetFactsList = new HashSet<RuleNode>(ruleNodeProvedList);
            return hashSetFactsList;
        }
    }
}
