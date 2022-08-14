using Gym.Envs;
using Gym.Observations;
using NumSharp;
// using EnvGameManager;

namespace HKGymEnv
{
	public enum Actions
        {
            MoveLeft,
            MoveRight,
            AttackLeft,
            AttackRight,
            AttackUp,
            AttackDown,
            Jump,
            CancelJump,
            DashLeft,
            DashRight,
            CastLeft,
            CastRight,
            CastUp,
            CastDown,
            None
            // TODO: Implement these eventually
            // Heal,
            // CancelHeal,
            // ChargeNailArt,
            // NailArtLeft,
            // NailArtRight,
            // NailArtUp,
            // NailArtDashLeft,
            // NailArtDashRight
        }
    public class HallOfGodsEnv : Env
    {

		// public struct Config {
		// 	public int width;
		// 	public int height;
		// 	public int framesSkippedPerAction = 5;
		// }

		// private Config _envConfig;
        private EnvGameManager manager = new();

        public HallOfGodsEnv()
        {

        }

        ///<summary>
        /// DO NOT USE
        ///</summary>
        public override byte[] Render(string mode = "human")
        {
            return new byte[] { };
        }

        public override Step Step(int action)
        {
			manager.DoAction(action, 5);
			// EnvGameManager.skipFrames(framesSkippedPerAction);
			CurrentTimestep state = manager.GetObservation();
			// GameState formattedState = manager.ResizeAndNormalize(state, _envConfig.width, _envConfig.height);
			return new Step(state.observation.state, state.reward, state.done, null);
            // throw new NotImplementedException();(
        }

        public override void Close()
        {
            manager.Close();
        }

        public override NDArray Reset()
        {
            return np.zeros((10, 10));
            // manager.Reset();
        }

        public override void Seed(int seed) {

        }
    }
}
