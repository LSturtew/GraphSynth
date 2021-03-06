﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using GraphSynth.Representation;

namespace GraphSynth
{
    public partial class WPFFiler : BasicFiler
    {
        //public override ruleSet OpenRuleSet(string filename)
        //{
        //    ruleSet newRuleSet = null;
        //    StreamReader ruleReader = null;
        //    try
        //    {
        //        ruleReader = new StreamReader(filename);
        //        var ruleDeserializer = new XmlSerializer(typeof(ruleSet));
        //        newRuleSet = (ruleSet)ruleDeserializer.Deserialize(ruleReader);
        //        newRuleSet.rulesDir = Path.GetDirectoryName(filename) + "/";
        //        newRuleSet.filer = this;
        //        var numRules = newRuleSet.ruleFileNames.Count;
        //        int numLoaded;
        //        if (progWindow.backgroundWorker.CancellationPending) return null;
        //        progress = -5;
        //        newRuleSet.rules = LoadRulesFromFileNames(newRuleSet.rulesDir, newRuleSet.ruleFileNames,
        //                                                  out numLoaded);

        //        SearchIO.output(Path.GetFileName(filename) + " successfully loaded");
        //        if (numRules == numLoaded) SearchIO.output("     and all rules loaded successfully.");
        //        else
        //            SearchIO.output("     but "
        //                            + (numRules - numLoaded) + " rules did not load.");

        //        newRuleSet.initializeFileWatcher(newRuleSet.rulesDir);
        //        if ((string.IsNullOrWhiteSpace(newRuleSet.name)) || (newRuleSet.name == "Untitled"))
        //            newRuleSet.name = Path.GetFileNameWithoutExtension(filename);
        //    }
        //    catch (Exception ioe)
        //    {
        //        SearchIO.output("***XML Serialization Error***", 0);
        //        SearchIO.output(ioe.ToString());
        //    }
        //    finally
        //    {
        //        if (ruleReader != null) ruleReader.Close();
        //    }

        //    return newRuleSet;
        //}

        protected override List<grammarRule> LoadRulesFromFileNames(string ruleDir, List<string> ruleFileNames,
                                                                    out int numLoaded)
        {
            var progStart = 5;
            var progStep = (double)(100 - progStart) / ruleFileNames.Count;
            var step = 0;
            var rules = new List<grammarRule>();
            numLoaded = 0;
            while (numLoaded < ruleFileNames.Count)
            {
                var rulePath = ruleDir + ruleFileNames[numLoaded];
                if (File.Exists(rulePath))
                {
                    SearchIO.output("Loading " + ruleFileNames[numLoaded]);
                    object ruleObj = OpenRuleAndCanvas(rulePath);
                    if (ruleObj is grammarRule)
                        rules.Add((grammarRule)ruleObj);
                    else if (ruleObj is object[])
                        foreach (object o in (object[])ruleObj)
                            if (o is grammarRule)
                                rules.Add((grammarRule)o);
                    numLoaded++;
                }
                else
                {
                    SearchIO.output("Rule Not Found: " + ruleFileNames[numLoaded]);
                    ruleFileNames.RemoveAt(numLoaded);
                }
                if (progWindow.backgroundWorker.CancellationPending) return null;
                progress = -(progStart + (int)(progStep * step++));
            }
            return rules;
        }

        public override void ReloadSpecificRule(ruleSet rs, int i)
        {
            lock (fileTransfer)
            {
                dispatch.Invoke((ThreadStart)delegate
                                                  {
                                                      var rulePath = rs.rulesDir + rs.ruleFileNames[i];
                                                      SearchIO.output("Loading " + rs.ruleFileNames[i]);
                                                      object ruleObj = Open(rulePath);
                                                      if (ruleObj is grammarRule)
                                                          rs.rules[i] = (grammarRule)ruleObj;
                                                      else if (ruleObj is object[] &&
                                                               ((object[])ruleObj)[0] is grammarRule)
                                                          rs.rules[i] = ((grammarRule)((object[])ruleObj)[0]);
                                                  });
            }
        }
    }
}