using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace HollowKnightGymEnvironment
{
    internal class HollowKnightGymEnvironment : Mod
    {
        internal static HollowKnightGymEnvironment Instance { get; private set; }

        public HollowKnightGymEnvironment() : base("HollowKnightGymEnvironment") { }

        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public override void Initialize()
        {
            Log("Initializing");

            Instance = this;

            Log("Initialized");
        }
    }
}