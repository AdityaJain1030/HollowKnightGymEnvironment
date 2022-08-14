using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using InControl;
using Modding;
using UnityEngine;
// using 

namespace HKGymEnv {
	
	public class EnvGameManager {
		public CurrentTimestep timestep;
		private static float TimeScaleDuringFrameAdvance = 0f;
		private InputDeviceShim inputDeviceShim = new();
		private HitboxReaderManager hitboxManager = new();
		// private 
		// private int _width = Screen.width;
		// private int _height = Screen.height;
		public EnvGameManager () {
			// _hitboxReader = new HitboxReader();
			hitboxManager.Load();
			InputManager.AttachDevice(inputDeviceShim);
			ModHooks.AfterPlayerDeadHook += AfterPlayerDeadHook;
			ModHooks.AfterTakeDamageHook += TakeDamageHook;
		}

		public void Close()
		{
			hitboxManager.Unload();
			InputManager.DetachDevice(inputDeviceShim);
			ModHooks.AfterPlayerDeadHook -= AfterPlayerDeadHook;
			ModHooks.AfterTakeDamageHook -= TakeDamageHook;
		}
		// public Step Step(Actions action) {
		// 	return new Step();
		// }

		public CurrentTimestep GetObservation() {
			timestep.observation = new GameObservation(Screen.width, Screen.height);
			var hitboxes = hitboxManager.GetHitboxes();
			Camera camera = Camera.main;
			foreach (var pair in hitboxes)
			{
				foreach (Collider2D collider2D in pair.Value)
				{
					RenderHitboxToObservation(camera, collider2D, pair.Key, timestep.observation);
				}
			}

			return timestep;
			// return observation;
			// return new int[10, 10];
		}
		#region RendererUtils
		private void RenderHitboxToObservation(Camera camera, Collider2D collider2D, HitboxType type, GameObservation observation)
		{
			if (collider2D == null || !collider2D.isActiveAndEnabled)
			{
				return;
			}
			switch (collider2D)
			{
				case BoxCollider2D boxCollider2D:
					Vector2 halfSize = boxCollider2D.size / 2f;
					Vector2 topLeft = new(-halfSize.x, halfSize.y);
					Vector2 topRight = halfSize;
					Vector2 bottomRight = new(halfSize.x, -halfSize.y);
					Vector2 bottomLeft = -halfSize;

					List<Vector2> boxPoints = new List<Vector2>
					{
						topLeft, topRight, bottomRight, bottomLeft
					};
					PaintPolygonToGameState(boxPoints, camera, collider2D, type, observation);

					break;
				case PolygonCollider2D polygonCollider2D:
					for (int i = 0; i < polygonCollider2D.pathCount; i++)
					{
						List<Vector2> polygonPoints = new(polygonCollider2D.GetPath(i));
						PaintPolygonToGameState(polygonPoints, camera, collider2D, type, observation);
					}
					break;
				case EdgeCollider2D edgeCollider2D:
					PaintPolygonToGameState(new(edgeCollider2D.points), camera, collider2D, type, observation);
					break;
				case CircleCollider2D circleCollider2D:
					Vector2 center = LocalToScreenPoint(camera, circleCollider2D, Vector2.zero);
					Vector2 right = LocalToScreenPoint(camera, collider2D, Vector2.right * circleCollider2D.radius);
					int radius = (int)Math.Round(Vector2.Distance(center, right));
					observation.AddCircleToState(
						center,
						radius,
						(int)type + 1
					);
					break;

			}
		}
		private Vector2 LocalToScreenPoint(Camera camera, Collider2D collider2D, Vector2 point)
		{
			Vector2 result = camera.WorldToScreenPoint((Vector2)collider2D.transform.TransformPoint(point + collider2D.offset));
			return new Vector2((int)Math.Round(result.x), (int)Math.Round(Screen.height - result.y));
		}

		private void PaintPolygonToGameState(List<Vector2> points, Camera camera, Collider2D collider2D, HitboxType hitboxType, GameObservation observation)
		{
			List<Vector2> projectedPoints = points.Select(point => LocalToScreenPoint(camera, collider2D, point)).ToList();
			observation.AddPolygonToObservation(projectedPoints, (int)hitboxType + 1);
		}
		#endregion

		#region FrameAdvUtils
		private void ToggleFrameAdvance()
        {
            if (Time.timeScale != 0)
            {
                if (GameManager.instance.GetComponent<TimeScale>() == null)
                    GameManager.instance.gameObject.AddComponent<TimeScale>();
                Time.timeScale = 0f;
                TimeScaleDuringFrameAdvance = DebugMod.CurrentTimeScale;
                DebugMod.CurrentTimeScale = 0;
                DebugMod.TimeScaleActive = true;
            }
            else
            {
                DebugMod.CurrentTimeScale = TimeScaleDuringFrameAdvance;
                Time.timeScale = DebugMod.CurrentTimeScale;
            }
        }
		private static IEnumerator AdvanceMyFrame(int framesSkippedPerAction)
        {
            Time.timeScale = 1f;
			for (int i = 0; i < framesSkippedPerAction; i++)
			{
				yield return new WaitForFixedUpdate();
			}
            
            Time.timeScale = 0;
        }
		#endregion
		
		public void DoAction(Actions action, int framesSkippedPerAction) {
			switch (action)
			{
				case Actions.MoveLeft:
					inputDeviceShim.DoWalk(true);
					break;
				case Actions.MoveRight:
					inputDeviceShim.DoWalk(false);
					break;
				case Actions.AttackLeft:
					inputDeviceShim.DoAttack(0);
					break;
				case Actions.AttackRight:
					inputDeviceShim.DoAttack(1);
					break;
				case Actions.AttackUp:
					inputDeviceShim.DoAttack(2);
					break;
				case Actions.AttackDown:
					inputDeviceShim.DoAttack(3);
					break;
				case Actions.Jump:
					inputDeviceShim.DoJump();
					break;
				case Actions.DashLeft:
					inputDeviceShim.DoDash(true);
					break;
				case Actions.DashRight:
					inputDeviceShim.DoDash(false);
					break;
				case Actions.CastLeft:
					inputDeviceShim.DoCast(0);
					break;
				case Actions.CastRight:
					inputDeviceShim.DoCast(1);
					break;
				case Actions.CastUp:
					inputDeviceShim.DoCast(2);
					break;
				case Actions.CastDown:
					inputDeviceShim.DoCast(3);
					break;
				default:
					break;
			}
			ToggleFrameAdvance();
			GameManager.instance.StartCoroutine(AdvanceMyFrame(framesSkippedPerAction));	
			inputDeviceShim.ResetState();
		}

		public void DoAction(int action, int framesSkippedPerAction) {
			DoAction((Actions)action, framesSkippedPerAction);
		}

		

		public int TakeDamageHook(int hazardType, int damageAmount) {
			timestep.reward -= damageAmount; 
			return damageAmount;
		}

		public void AfterPlayerDeadHook() {
			timestep.reward -= 100;
			timestep.done = true;
		}

	}
}