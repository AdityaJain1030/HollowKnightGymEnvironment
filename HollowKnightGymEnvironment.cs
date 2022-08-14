using System.Reflection;
using Modding;

using UObject = UnityEngine.Object;

namespace HollowKnightGymEnvironment
{
    internal class HollowKnightGymEnvironment : Mod
    {
        internal static HollowKnightGymEnvironment Instance { get; private set;}

        public HollowKnightGymEnvironment() :
            base("HollowKnightGymEnvironment")
        {
            Instance = this;
        }

        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public override void Initialize()
        {
            
        }
    }
}
