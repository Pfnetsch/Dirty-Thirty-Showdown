using UnityEngine;

namespace PartyArmWrestling
{
    /// <summary>
    /// Simple test controller for quickly testing the core mechanics
    /// without the full UI setup. Attach to a GameObject and assign references.
    /// 
    /// Controls:
    /// P1: Space (mash), Q (ability 1), E (ability 2)
    /// P2: Enter (mash), O (ability 1), P (ability 2)
    /// 
    /// Press R to reset the bar
    /// Press T to start a test match
    /// </summary>
    public class QuickTestController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArmWrestleController armWrestleController;

        [Header("Test Settings")]
        [SerializeField] private float mashPower = 1f;

        [Header("Debug Display")]
        [SerializeField] private bool showDebugUI = true;

        private float p1MashCount = 0;
        private float p2MashCount = 0;
        private bool testMatchActive = false;

        private void Update()
        {
            if (armWrestleController == null) return;

            // Player 1 input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                armWrestleController.AddMashPower(1, mashPower);
                p1MashCount++;
            }

            // Player 2 input
            if (Input.GetKeyDown(KeyCode.Return))
            {
                armWrestleController.AddMashPower(2, mashPower);
                p2MashCount++;
            }

            // Test ability effects
            if (Input.GetKeyDown(KeyCode.Q))
            {
                armWrestleController.SetMashMultiplier(1, 2f, 3f);
                Debug.Log("P1 Power Surge activated!");
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                armWrestleController.SetInputDisabled(2, true, 1.5f);
                Debug.Log("P2 input disabled for 1.5s!");
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                armWrestleController.SetMashMultiplier(2, 2f, 3f);
                Debug.Log("P2 Power Surge activated!");
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                armWrestleController.SetControlsReversed(true, 2f);
                Debug.Log("Controls reversed for 2s!");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                armWrestleController.ResetBar();
                p1MashCount = 0;
                p2MashCount = 0;
                Debug.Log("Bar reset!");
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                testMatchActive = !testMatchActive;
                Debug.Log($"Test match: {(testMatchActive ? "STARTED" : "STOPPED")}");
            }
        }

        private void OnGUI()
        {
            if (!showDebugUI || armWrestleController == null) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.normal.textColor = Color.white;

            float barPos = armWrestleController.BarPosition;
            
            GUI.Label(new Rect(20, 20, 600, 40), $"Bar Position: {barPos:F3}", style);
            GUI.Label(new Rect(20, 60, 600, 40), $"P1 Mashes: {p1MashCount}  |  P2 Mashes: {p2MashCount}", style);
            
            // Visual bar
            int p1Bars = Mathf.RoundToInt((1 - barPos) * 10);
            int p2Bars = Mathf.RoundToInt((1 + barPos) * 10);
            string visualBar = $"P1 [{new string('=', p1Bars)}|{new string('=', p2Bars)}] P2";
            GUI.Label(new Rect(20, 100, 600, 40), visualBar, style);

            // Controls help
            GUIStyle smallStyle = new GUIStyle(GUI.skin.label);
            smallStyle.fontSize = 16;
            smallStyle.normal.textColor = Color.yellow;
            
            GUI.Label(new Rect(20, 160, 600, 30), "P1: SPACE=mash, Q=PowerSurge, E=DisableP2", smallStyle);
            GUI.Label(new Rect(20, 185, 600, 30), "P2: ENTER=mash, O=PowerSurge, P=ReverseControls", smallStyle);
            GUI.Label(new Rect(20, 210, 600, 30), "R=Reset, T=Toggle Test Match", smallStyle);
        }
    }
}
