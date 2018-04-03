﻿//   HangarResourceUser.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System;
using System.Linq;
using System.Collections.Generic;
using AT_Utils;

namespace AT_Utils
{
    public abstract class AnimatedResourceConsumer : AnimatedConverterBase, IResourceConsumer
    {
        [KSPField] public string InputResources = string.Empty;
        protected List<ResourceLine> input;

        public List<PartResourceDefinition> GetConsumedResources()
        { return input.Select(i => i.Resource).ToList(); }

        public override string GetInfo()
        {
            setup_resources();
            var info = base.GetInfo();
            if(input != null && input.Count > 0)
            {
                info += "Inputs:\n";
                info += input.Aggregate("", (s, r) => s+"- "+r.Info+'\n');
            }
            return info;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            //check energy consumption; even generators consume energy
            if(EnergyConsumption <= 0) 
                EnergyConsumption = 0.01f;
        }

        protected virtual void setup_resources()
        {
            //parse input/output resources
            input  = ResourceLine.ParseResourcesToList(InputResources);
            if(input == null) 
            { 
                this.ConfigurationInvalid("unable to initialize INPUT resources"); 
                return; 
            }
            input.ForEach(r => r.InitializePump(part, RatesMultiplier));
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            setup_resources();
        }

        public override void SetRatesMultiplier(float mult)
        {
            base.SetRatesMultiplier(mult);
            setup_resources();
        }

        protected static string try_transfer(List<ResourceLine> resources, float rate = 1f, bool skip_failed = false)
        {
            string failed = "";
            foreach(var r in resources)
            {
                if(r.TransferResource(rate) && r.PartialTransfer) 
                { 
                    if(skip_failed) 
                        failed += (failed == ""? "" : ", ") + Utils.ParseCamelCase(r.Name);
                    else return Utils.ParseCamelCase(r.Name); 
                }
            }
            return failed;
        }
    }
}

