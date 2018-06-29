using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba5
{
    public struct Rule
    {
        public HashSet<int> idsPremise { get; set; }
        public int idConclusion { get; set; }
    }

    public class ProductModel
    {
        public List<string> factsList = new List<string>();
        public HashSet<int> mainFactsId = new HashSet<int>();
        public List<Rule> rulesList = new List<Rule>();

        public ProductModel(string path)
        {
            string[] allRulesStrArr = File.ReadAllLines(path);
            foreach (var rStr in allRulesStrArr)
            {
                string ruleStr = rStr;
                ruleStr = ruleStr.Replace("->", "~");
                //разделение на посылку и заключение
                string[] parts = ruleStr.Split('~');
                if (parts.Length == 2)
                {
                    Rule curRule = new Rule();
                    curRule.idsPremise = new HashSet<int>();
                    //работа с посылкой
                    string[] factsPremise = parts[0].Split('&');
                    foreach (var fPr in factsPremise)
                    {
                        string curFact = fPr.Trim();
                        if (factsList.IndexOf(curFact) == -1)
                            factsList.Add(curFact);
                        curRule.idsPremise.Add(factsList.IndexOf(curFact));
                    }
                    string factConclusion = parts[1];
                    if(factConclusion.IndexOf("[") != -1 && factConclusion.IndexOf("]") != -1)
                    {
                        string listMainFactsStr = factConclusion.Replace("[", "");
                        listMainFactsStr = listMainFactsStr.Replace("]", "");
                        string[] listMainFacts = listMainFactsStr.Split(',');
                        foreach(var mainF in listMainFacts)
                        {
                            Rule curRuleMF = new Rule();
                            curRuleMF.idsPremise = new HashSet<int>(curRule.idsPremise);
                            string mainFact = mainF.Trim();
                            if (factsList.IndexOf(mainFact) == -1)
                            {
                                factsList.Add(mainFact);
                                mainFactsId.Add(factsList.IndexOf(mainFact));
                            }
                            curRuleMF.idConclusion = factsList.IndexOf(mainFact);
                            rulesList.Add(curRuleMF);
                        }
                    }
                    else
                    {
                        string curFact = factConclusion.Trim();
                        if (factsList.IndexOf(curFact) == -1)
                            factsList.Add(curFact);
                        curRule.idConclusion = factsList.IndexOf(curFact);
                        rulesList.Add(curRule);
                    }
                }
                else if(rStr!="")
                {
                    Console.WriteLine("Ошибка при чтении файла, плохая запись правила");
                    return;
                }
            }
        }//конец конструктора ProductModel

        #region PrintFunctions
        public void PrintRule(Rule r)
        {
            string strPrint = "";
            foreach (var rP in r.idsPremise)
                strPrint += factsList[rP] + " & ";
            strPrint = strPrint.Remove(strPrint.LastIndexOf("&"), 1);
            strPrint += "->  " + factsList[r.idConclusion];
            Console.WriteLine(strPrint);
        }//конец PrintRule

        public void PrintAllRules()
        {
            foreach (var rule in rulesList)
            {
                PrintRule(rule);
                Console.WriteLine("-------------------------------------------------");
            }
        }//конец PrintAllRules

        public void PrintFact(int id)
        {
            Console.WriteLine(id + ") " + factsList[id]);
        }//конец PrintFact

        public void PrintAllFacts()
        {
            for (int i=0; i < factsList.Count; i++)
            {
                PrintFact(i);
                Console.WriteLine("-------------------------------------------------");
            }
        }//конец PrintAllFacts

        public void PrintByFilterMainFacts(HashSet<int> allFactsIds)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Main Facts: ");
            Console.ResetColor();
            foreach (var factId in allFactsIds)
                if (mainFactsId.Contains(factId))
                    PrintFact(factId);
            Console.WriteLine();
        }//конец PrintByFilterMainFacts

        public void PrintFacts(HashSet<int> allFactsIds)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Facts: ");
            Console.ResetColor();
            foreach (var factId in allFactsIds)
                    PrintFact(factId);
            Console.WriteLine();
        }//конец PrintFacts
        #endregion

        public int FactIdByText(string factText)
        {
            return factsList.IndexOf(factText);
        }//конец FactIdByText

        public List<int> FactsIdByFactsText(string[] factsTextArr)
        {
            List<int> res = new List<int>(factsTextArr.Length);
            foreach (var factText in factsTextArr)
                res.Add(FactIdByText(factText));
            return res;
        }//конец FactsIdByFactsText

        public HashSet<int> DirectOutput(List<int> trueFactsIds, bool isPrintRools)
        {
            #region PrintHeader
            if (isPrintRools)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DirectOutput: ");
                Console.ResetColor();
            }
            #endregion
            HashSet<int> provenFactsIds = new HashSet<int>(trueFactsIds);
            HashSet<int> provenRulesIds = new HashSet<int>();
            int oldCount = 0;
            do
            {
                oldCount = provenFactsIds.Count;
                for (int i = 0; i < rulesList.Count; i++)
                {
                    if (provenRulesIds.Contains(i)) continue;
                    Rule rool = rulesList[i];
                    var intersectdSet = new HashSet<int>(rool.idsPremise);
                    intersectdSet.IntersectWith(provenFactsIds);
                    if (intersectdSet.Count == rool.idsPremise.Count)
                    {
                        #region PrintRool
                        if (isPrintRools)
                        {
                            string strPrint = "";
                            foreach (var elId in provenFactsIds)
                                strPrint += factsList[elId] + " ";
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("proven: " + strPrint);
                            Console.ResetColor();
                            PrintRule(rool);
                            Console.WriteLine("---------------------------------");
                        }
                        #endregion
                        provenRulesIds.Add(i);
                        provenFactsIds.Add(rool.idConclusion);
                    }
                }
            } while (provenFactsIds.Count != oldCount);
            return provenFactsIds;
        }//конец  DirectOutput

        /*public HashSet<int> ReverseOutput(List<int> needFactsIds, bool isPrintRools)
        {
            #region PrintHeader
            if (isPrintRools)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ReverseOutput: ");
                Console.ResetColor();
            }
            #endregion
            HashSet<int> provenFactsIds = new HashSet<int>(needFactsIds);
            HashSet<int> provenRulesIds = new HashSet<int>();
            List<int> rightSideFactsIds = new List<int>();
            int oldCount = 0;
            do
            {
                provenFactsIds.ExceptWith(rightSideFactsIds);
                oldCount = provenFactsIds.Count;
                for (int i = 0; i < rulesList.Count; i++)
                {
                    if (provenRulesIds.Contains(i)) continue;
                    Rule rool = rulesList[i];
                    if (provenFactsIds.Contains(rool.idConclusion))
                    {
                        rightSideFactsIds.Add(rool.idConclusion);
                        #region PrintRool
                        if (isPrintRools)
                        {
                            string strPrint = "";
                            foreach (var elId in provenFactsIds)
                                strPrint += factsList[elId] + " ";
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("proven: " + strPrint);
                            Console.ResetColor();
                            PrintRule(rool);
                            Console.WriteLine("---------------------------------");
                        }
                        #endregion
                        provenRulesIds.Add(i);
                        foreach(var id in rool.idsPremise)
                            provenFactsIds.Add(id);
                    }
                }
            } while (provenFactsIds.Count != oldCount);
            return provenFactsIds;
        }*///конец ReverseOutput

        public HashSet<int> ReverseOutput(List<int> needFactsIds, bool isPrintRools)
        {
            #region PrintHeader
            if (isPrintRools)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ReverseOutput: ");
                Console.ResetColor();
            }
            #endregion
            HashSet<int> needSetFactsIds = new HashSet<int>(needFactsIds);
            var provenNodes = BackwardOutput.FindEvidenceBase(needSetFactsIds,
                this.rulesList, this.factsList);
            var provenFactsIds = new HashSet<int>();
            foreach (var node in provenNodes)
                provenFactsIds.Add(node.Fact.Id);
            return provenFactsIds;
        }//конец ReverseOutput
    }
}
