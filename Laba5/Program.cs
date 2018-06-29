using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Laba5
{
    class Program
    {
        static void TestFileRun()
        {
            ProductModel prodModel = new ProductModel("test.txt");
            prodModel.PrintAllRules();
            prodModel.PrintAllFacts();
            string[] directStrArr = new string[] {
                "A",
                "B"
            };
            List<int> trueFactsId = prodModel.FactsIdByFactsText(directStrArr);
            var provedFacts = prodModel.DirectOutput(trueFactsId, true);
            prodModel.PrintFacts(provedFacts);
            string[] needStrArr = new string[] {
                "K",
                "L",
                "P"
            };
            List<int> needFactsId = prodModel.FactsIdByFactsText(needStrArr);
            var needFacts = prodModel.ReverseOutput(needFactsId, true);
            prodModel.PrintFacts(needFacts);
        }

        static void RulesFileRun()
        {
           ProductModel prodModel = new ProductModel("rules.txt");
           /*prodModel.PrintAllRules();
           prodModel.PrintAllFacts();*/
            string[] directStrArr = new string[] {
                "Вещества",
                "Соединения углерода с водородом",
                "В составе есть кислород",
                "Есть альдегидная группа CHO",
                "Есть гидроксильная группа OH"
            };
            List<int> trueFactsId = prodModel.FactsIdByFactsText(directStrArr);
            var provedFacts = prodModel.DirectOutput(trueFactsId, true);
            prodModel.PrintFacts(provedFacts);
            prodModel.PrintByFilterMainFacts(provedFacts);
            string[] needStrArr = new string[] {
                "CH4O2N(глицин)",
                "K2SO4",
                "C(углерод)"
            };
            List<int> needFactsId = prodModel.FactsIdByFactsText(needStrArr);
            var needFacts = prodModel.ReverseOutput(needFactsId, true);
            prodModel.PrintFacts(needFacts);
        }

        static void Main(string[] args)
        {
            //TestFileRun();
            RulesFileRun();
        }
    }
}
