using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace DirtyThirtyShowdown
{
#if UNITY_EDITOR
    /// <summary>
    /// Editor-only balance simulation tool. Simulates thousands of matches between
    /// all character matchups using AI player strategies, then outputs a balance report.
    ///
    /// Usage: Open via menu Tools > Dirty Thirty Showdown > Balance Simulator
    /// </summary>
    public class BalanceSimulator : UnityEditor.EditorWindow
    {
        // Simulation settings
        private int matchesPerMatchup = 1000;
        private float simulatedDeltaTime = 1f / 60f; // 60 FPS simulation
        private bool includeDetails = true;
        private Vector2 scrollPosition;
        private string lastReport = "";

        // Bar physics settings (mirror ArmWrestleController defaults)
        private float barSensitivity = 0.1f;
        private float barDamping = 0.95f;
        private float momentumDecay = 2f;
        private float roundTimeLimit = 45f;
        private float mashPowerPerPress = 1f;
        private float maxMashRate = 20f;

        // Ability settings (mirror AbilitySystem defaults)
        private float powerSurgeMultiplier = 2f;
        private float trashTalkSlowdown = 0.75f;
        private float danceEfficiency = 0.5f;

        [UnityEditor.MenuItem("Tools/Dirty Thirty Showdown/Balance Simulator")]
        public static void ShowWindow()
        {
            var window = GetWindow<BalanceSimulator>("Balance Simulator");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(scrollPosition);

            UnityEditor.EditorGUILayout.LabelField("Dirty Thirty Showdown — Balance Simulator", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUILayout.Space();

            // Simulation settings
            UnityEditor.EditorGUILayout.LabelField("Simulation Settings", UnityEditor.EditorStyles.boldLabel);
            matchesPerMatchup = UnityEditor.EditorGUILayout.IntSlider("Matches per Matchup", matchesPerMatchup, 100, 10000);
            includeDetails = UnityEditor.EditorGUILayout.Toggle("Include Detailed Stats", includeDetails);

            UnityEditor.EditorGUILayout.Space();

            // Bar physics
            UnityEditor.EditorGUILayout.LabelField("Bar Physics", UnityEditor.EditorStyles.boldLabel);
            barSensitivity = UnityEditor.EditorGUILayout.FloatField("Bar Sensitivity", barSensitivity);
            barDamping = UnityEditor.EditorGUILayout.Slider("Bar Damping", barDamping, 0.8f, 1f);
            momentumDecay = UnityEditor.EditorGUILayout.FloatField("Momentum Decay", momentumDecay);
            roundTimeLimit = UnityEditor.EditorGUILayout.FloatField("Round Time Limit (s)", roundTimeLimit);
            mashPowerPerPress = UnityEditor.EditorGUILayout.FloatField("Mash Power Per Press", mashPowerPerPress);
            maxMashRate = UnityEditor.EditorGUILayout.FloatField("Max Mash Rate (Hz)", maxMashRate);

            UnityEditor.EditorGUILayout.Space();

            // Ability tuning
            UnityEditor.EditorGUILayout.LabelField("Ability Tuning", UnityEditor.EditorStyles.boldLabel);
            powerSurgeMultiplier = UnityEditor.EditorGUILayout.FloatField("Power Surge Multiplier", powerSurgeMultiplier);
            trashTalkSlowdown = UnityEditor.EditorGUILayout.Slider("Trash Talk Slowdown", trashTalkSlowdown, 0.1f, 1f);
            danceEfficiency = UnityEditor.EditorGUILayout.Slider("Dance Efficiency Penalty", danceEfficiency, 0.1f, 1f);

            UnityEditor.EditorGUILayout.Space();

            if (GUILayout.Button("Run Full Simulation", GUILayout.Height(40)))
            {
                lastReport = RunFullSimulation();
            }

            if (GUILayout.Button("Run Quick Test (100 matches)", GUILayout.Height(25)))
            {
                int saved = matchesPerMatchup;
                matchesPerMatchup = 100;
                lastReport = RunFullSimulation();
                matchesPerMatchup = saved;
            }

            UnityEditor.EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(lastReport))
            {
                UnityEditor.EditorGUILayout.LabelField("Results", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.TextArea(lastReport, GUILayout.ExpandHeight(true));
            }

            UnityEditor.EditorGUILayout.EndScrollView();
        }

        #region Simulation Core

        private string RunFullSimulation()
        {
            var characters = GetAllCharacters();
            var results = new Dictionary<string, MatchupResult>();
            var characterWins = new Dictionary<string, int>();
            var characterLosses = new Dictionary<string, int>();

            foreach (var c in characters)
            {
                characterWins[c.Name] = 0;
                characterLosses[c.Name] = 0;
            }

            // Run all matchups
            for (int i = 0; i < characters.Length; i++)
            {
                for (int j = i; j < characters.Length; j++)
                {
                    string key = $"{characters[i].Name} vs {characters[j].Name}";

                    UnityEditor.EditorUtility.DisplayProgressBar("Balance Simulation",
                        $"Simulating {key}...",
                        (float)(i * characters.Length + j) / (characters.Length * characters.Length));

                    var result = SimulateMatchup(characters[i], characters[j]);
                    results[key] = result;

                    characterWins[characters[i].Name] += result.Player1Wins;
                    characterLosses[characters[i].Name] += result.Player2Wins;
                    characterWins[characters[j].Name] += result.Player2Wins;
                    characterLosses[characters[j].Name] += result.Player1Wins;
                }
            }

            UnityEditor.EditorUtility.ClearProgressBar();

            return FormatReport(characters, results, characterWins, characterLosses);
        }

        private MatchupResult SimulateMatchup(SimCharacter p1Char, SimCharacter p2Char)
        {
            var result = new MatchupResult();
            var strategies = new AIStrategy[] {
                AIStrategy.RandomMasher,
                AIStrategy.OptimalMasher,
                AIStrategy.AbilitySpammer,
                AIStrategy.Strategic
            };

            int matchesPerStrategy = matchesPerMatchup / (strategies.Length * strategies.Length);
            if (matchesPerStrategy < 1) matchesPerStrategy = 1;

            // Test all strategy combinations
            foreach (var p1Strat in strategies)
            {
                foreach (var p2Strat in strategies)
                {
                    for (int m = 0; m < matchesPerStrategy; m++)
                    {
                        var matchResult = SimulateMatch(p1Char, p2Char, p1Strat, p2Strat);
                        result.TotalMatches++;

                        if (matchResult.Winner == 1)
                            result.Player1Wins++;
                        else
                            result.Player2Wins++;

                        result.TotalRounds += matchResult.RoundsPlayed;
                        result.TotalDuration += matchResult.TotalDuration;
                        result.P1Ability1Uses += matchResult.P1Ability1Uses;
                        result.P1Ability2Uses += matchResult.P1Ability2Uses;
                        result.P2Ability1Uses += matchResult.P2Ability1Uses;
                        result.P2Ability2Uses += matchResult.P2Ability2Uses;
                        result.TimeoutRounds += matchResult.TimeoutRounds;
                    }
                }
            }

            return result;
        }

        private SingleMatchResult SimulateMatch(SimCharacter p1Char, SimCharacter p2Char,
            AIStrategy p1Strategy, AIStrategy p2Strategy)
        {
            var matchResult = new SingleMatchResult();
            int p1Score = 0;
            int p2Score = 0;
            int roundsToWin = 2;

            while (p1Score < roundsToWin && p2Score < roundsToWin)
            {
                var roundResult = SimulateRound(p1Char, p2Char, p1Strategy, p2Strategy);
                matchResult.RoundsPlayed++;
                matchResult.TotalDuration += roundResult.Duration;
                matchResult.P1Ability1Uses += roundResult.P1Ability1Uses;
                matchResult.P1Ability2Uses += roundResult.P1Ability2Uses;
                matchResult.P2Ability1Uses += roundResult.P2Ability1Uses;
                matchResult.P2Ability2Uses += roundResult.P2Ability2Uses;

                if (roundResult.Timeout) matchResult.TimeoutRounds++;

                if (roundResult.Winner == 1)
                    p1Score++;
                else
                    p2Score++;
            }

            matchResult.Winner = p1Score >= roundsToWin ? 1 : 2;
            return matchResult;
        }

        private RoundResult SimulateRound(SimCharacter p1Char, SimCharacter p2Char,
            AIStrategy p1Strategy, AIStrategy p2Strategy)
        {
            var result = new RoundResult();

            // Bar state
            float barPosition = 0f;
            float barVelocity = 0f;
            float p1MashPower = 0f;
            float p2MashPower = 0f;

            // Modifiers
            float p1Multiplier = 1f;
            float p2Multiplier = 1f;
            bool p1InputDisabled = false;
            bool p2InputDisabled = false;
            bool barLocked = false;
            int barLockedForPlayer = 0;
            bool controlsReversed = false;
            bool p1HasShield = false;
            bool p2HasShield = false;

            // Modifier timers
            float p1MultiplierTimer = 0f;
            float p2MultiplierTimer = 0f;
            float p1InputDisableTimer = 0f;
            float p2InputDisableTimer = 0f;
            float barLockTimer = 0f;
            float controlsReversedTimer = 0f;

            // Cooldowns
            float p1Ability1CD = 0f;
            float p1Ability2CD = 0f;
            float p2Ability1CD = 0f;
            float p2Ability2CD = 0f;

            // AI state
            float p1MashTimer = 0f;
            float p2MashTimer = 0f;

            float timeElapsed = 0f;
            float dt = simulatedDeltaTime;

            while (timeElapsed < roundTimeLimit)
            {
                // --- AI Decisions ---

                // Player 1 mashing
                float p1MashInterval = GetMashInterval(p1Strategy);
                p1MashTimer += dt;
                if (p1MashTimer >= p1MashInterval && !p1InputDisabled)
                {
                    float power = mashPowerPerPress;
                    if (controlsReversed) power = -power;
                    p1MashPower += power * p1Multiplier;
                    p1MashTimer = 0f;
                }

                // Player 2 mashing
                float p2MashInterval = GetMashInterval(p2Strategy);
                p2MashTimer += dt;
                if (p2MashTimer >= p2MashInterval && !p2InputDisabled)
                {
                    float power = mashPowerPerPress;
                    if (controlsReversed) power = -power;
                    p2MashPower += power * p2Multiplier;
                    p2MashTimer = 0f;
                }

                // Player 1 abilities
                p1Ability1CD -= dt;
                p1Ability2CD -= dt;
                if (p1Ability1CD <= 0f && ShouldUseAbility(p1Strategy, p1Char.Ability1Type, barPosition, timeElapsed, 1))
                {
                    result.P1Ability1Uses++;
                    bool blocked = TryBlockWithShield(p1Char.Ability1Type, ref p2HasShield);
                    if (!blocked)
                    {
                        ApplyAbility(p1Char.Ability1Type, 1, p1Char.Ability1Duration,
                            ref p1Multiplier, ref p2Multiplier,
                            ref p1MultiplierTimer, ref p2MultiplierTimer,
                            ref p1InputDisabled, ref p2InputDisabled,
                            ref p1InputDisableTimer, ref p2InputDisableTimer,
                            ref barLocked, ref barLockedForPlayer, ref barLockTimer,
                            ref controlsReversed, ref controlsReversedTimer,
                            ref p1HasShield, ref p2HasShield);
                    }
                    p1Ability1CD = p1Char.Ability1Cooldown;
                }
                if (p1Ability2CD <= 0f && ShouldUseAbility(p1Strategy, p1Char.Ability2Type, barPosition, timeElapsed, 1))
                {
                    result.P1Ability2Uses++;
                    bool blocked = TryBlockWithShield(p1Char.Ability2Type, ref p2HasShield);
                    if (!blocked)
                    {
                        ApplyAbility(p1Char.Ability2Type, 1, p1Char.Ability2Duration,
                            ref p1Multiplier, ref p2Multiplier,
                            ref p1MultiplierTimer, ref p2MultiplierTimer,
                            ref p1InputDisabled, ref p2InputDisabled,
                            ref p1InputDisableTimer, ref p2InputDisableTimer,
                            ref barLocked, ref barLockedForPlayer, ref barLockTimer,
                            ref controlsReversed, ref controlsReversedTimer,
                            ref p1HasShield, ref p2HasShield);
                    }
                    p1Ability2CD = p1Char.Ability2Cooldown;
                }

                // Player 2 abilities
                p2Ability1CD -= dt;
                p2Ability2CD -= dt;
                if (p2Ability1CD <= 0f && ShouldUseAbility(p2Strategy, p2Char.Ability1Type, barPosition, timeElapsed, 2))
                {
                    result.P2Ability1Uses++;
                    bool blocked = TryBlockWithShield(p2Char.Ability1Type, ref p1HasShield);
                    if (!blocked)
                    {
                        ApplyAbility(p2Char.Ability1Type, 2, p2Char.Ability1Duration,
                            ref p1Multiplier, ref p2Multiplier,
                            ref p1MultiplierTimer, ref p2MultiplierTimer,
                            ref p1InputDisabled, ref p2InputDisabled,
                            ref p1InputDisableTimer, ref p2InputDisableTimer,
                            ref barLocked, ref barLockedForPlayer, ref barLockTimer,
                            ref controlsReversed, ref controlsReversedTimer,
                            ref p1HasShield, ref p2HasShield);
                    }
                    p2Ability1CD = p2Char.Ability1Cooldown;
                }
                if (p2Ability2CD <= 0f && ShouldUseAbility(p2Strategy, p2Char.Ability2Type, barPosition, timeElapsed, 2))
                {
                    result.P2Ability2Uses++;
                    bool blocked = TryBlockWithShield(p2Char.Ability2Type, ref p1HasShield);
                    if (!blocked)
                    {
                        ApplyAbility(p2Char.Ability2Type, 2, p2Char.Ability2Duration,
                            ref p1Multiplier, ref p2Multiplier,
                            ref p1MultiplierTimer, ref p2MultiplierTimer,
                            ref p1InputDisabled, ref p2InputDisabled,
                            ref p1InputDisableTimer, ref p2InputDisableTimer,
                            ref barLocked, ref barLockedForPlayer, ref barLockTimer,
                            ref controlsReversed, ref controlsReversedTimer,
                            ref p1HasShield, ref p2HasShield);
                    }
                    p2Ability2CD = p2Char.Ability2Cooldown;
                }

                // --- Update modifier timers ---
                if (p1MultiplierTimer > 0f) { p1MultiplierTimer -= dt; if (p1MultiplierTimer <= 0f) p1Multiplier = 1f; }
                if (p2MultiplierTimer > 0f) { p2MultiplierTimer -= dt; if (p2MultiplierTimer <= 0f) p2Multiplier = 1f; }
                if (p1InputDisableTimer > 0f) { p1InputDisableTimer -= dt; if (p1InputDisableTimer <= 0f) p1InputDisabled = false; }
                if (p2InputDisableTimer > 0f) { p2InputDisableTimer -= dt; if (p2InputDisableTimer <= 0f) p2InputDisabled = false; }
                if (barLockTimer > 0f) { barLockTimer -= dt; if (barLockTimer <= 0f) { barLocked = false; barLockedForPlayer = 0; } }
                if (controlsReversedTimer > 0f) { controlsReversedTimer -= dt; if (controlsReversedTimer <= 0f) controlsReversed = false; }

                // --- Bar Physics (mirrors ArmWrestleController) ---
                float netForce = (p2MashPower - p1MashPower) * barSensitivity;
                barVelocity += netForce * dt;
                barVelocity *= Mathf.Pow(barDamping, dt * 60f);

                float newPosition = barPosition + barVelocity * dt;

                // Bar lock
                if (barLocked)
                {
                    if (barLockedForPlayer == 1)
                        newPosition = Mathf.Min(newPosition, barPosition);
                    else if (barLockedForPlayer == 2)
                        newPosition = Mathf.Max(newPosition, barPosition);
                }

                barPosition = Mathf.Clamp(newPosition, -1f, 1f);

                // Mash power decay
                p1MashPower = Mathf.Lerp(p1MashPower, 0f, momentumDecay * dt);
                p2MashPower = Mathf.Lerp(p2MashPower, 0f, momentumDecay * dt);

                // --- Win check ---
                if (barPosition <= -1f)
                {
                    result.Winner = 1;
                    result.Duration = timeElapsed;
                    return result;
                }
                if (barPosition >= 1f)
                {
                    result.Winner = 2;
                    result.Duration = timeElapsed;
                    return result;
                }

                timeElapsed += dt;
            }

            // Timeout — whoever bar is closer to wins
            result.Timeout = true;
            result.Duration = roundTimeLimit;
            result.Winner = barPosition < 0 ? 1 : 2;
            if (Mathf.Approximately(barPosition, 0f))
                result.Winner = Random.Range(0, 2) == 0 ? 1 : 2;

            return result;
        }

        #endregion

        #region AI Strategies

        private enum AIStrategy
        {
            RandomMasher,   // Random timing, inconsistent
            OptimalMasher,  // Max speed input
            AbilitySpammer, // Use abilities on cooldown
            Strategic       // Save abilities for key moments
        }

        private float GetMashInterval(AIStrategy strategy)
        {
            float minInterval = 1f / maxMashRate;
            return strategy switch
            {
                AIStrategy.RandomMasher => Random.Range(minInterval, minInterval * 3f),
                AIStrategy.OptimalMasher => minInterval,
                AIStrategy.AbilitySpammer => minInterval * 1.5f, // Slightly slower (distracted by abilities)
                AIStrategy.Strategic => minInterval * 1.2f, // Slightly sub-optimal
                _ => minInterval * 2f
            };
        }

        private bool ShouldUseAbility(AIStrategy strategy, AbilityType type, float barPosition, float timeElapsed, int playerNumber)
        {
            // Positive barPosition = closer to P2 winning
            float advantage = playerNumber == 1 ? -barPosition : barPosition; // positive = I'm winning

            switch (strategy)
            {
                case AIStrategy.RandomMasher:
                    return Random.value < 0.3f; // 30% chance when available

                case AIStrategy.OptimalMasher:
                    return Random.value < 0.5f; // Uses abilities sometimes

                case AIStrategy.AbilitySpammer:
                    return true; // Always use on cooldown

                case AIStrategy.Strategic:
                    // Shield: use proactively
                    if (type == AbilityType.Shield)
                        return true;
                    // Offensive abilities: use when losing or neutral
                    if (advantage < 0.2f)
                        return true;
                    // Defensive abilities (CakeToss): use when losing
                    if (type == AbilityType.CakeToss && advantage < -0.1f)
                        return true;
                    // FakeOut: use when opponent is winning (reverse their momentum)
                    if (type == AbilityType.FakeOut && advantage < -0.2f)
                        return true;
                    // Save abilities when winning comfortably
                    return Random.value < 0.15f;

                default:
                    return Random.value < 0.5f;
            }
        }

        #endregion

        #region Ability Application

        private bool IsOffensiveAbility(AbilityType type)
        {
            return type == AbilityType.Flash || type == AbilityType.TrashTalk ||
                   type == AbilityType.WinkFlirt || type == AbilityType.Dance ||
                   type == AbilityType.FakeOut;
        }

        private bool TryBlockWithShield(AbilityType type, ref bool targetHasShield)
        {
            if (type == AbilityType.Shield) return false;
            if (!IsOffensiveAbility(type)) return false;
            if (!targetHasShield) return false;

            targetHasShield = false;
            return true;
        }

        private void ApplyAbility(AbilityType type, int playerNumber, float duration,
            ref float p1Mult, ref float p2Mult,
            ref float p1MultTimer, ref float p2MultTimer,
            ref bool p1Disabled, ref bool p2Disabled,
            ref float p1DisableTimer, ref float p2DisableTimer,
            ref bool barLock, ref int lockPlayer, ref float lockTimer,
            ref bool reversed, ref float reversedTimer,
            ref bool p1Shield, ref bool p2Shield)
        {
            int target = playerNumber == 1 ? 2 : 1;

            switch (type)
            {
                case AbilityType.PowerSurge:
                    if (playerNumber == 1) { p1Mult = powerSurgeMultiplier; p1MultTimer = duration; }
                    else { p2Mult = powerSurgeMultiplier; p2MultTimer = duration; }
                    break;

                case AbilityType.Flash:
                    // Flash is a brief distraction — simulate as slight input disable
                    if (target == 1) { p1Disabled = true; p1DisableTimer = duration; }
                    else { p2Disabled = true; p2DisableTimer = duration; }
                    break;

                case AbilityType.TrashTalk:
                    if (target == 1) { p1Mult = trashTalkSlowdown; p1MultTimer = duration; }
                    else { p2Mult = trashTalkSlowdown; p2MultTimer = duration; }
                    break;

                case AbilityType.Shield:
                    if (playerNumber == 1) p1Shield = true;
                    else p2Shield = true;
                    break;

                case AbilityType.WinkFlirt:
                    if (target == 1) { p1Disabled = true; p1DisableTimer = duration; }
                    else { p2Disabled = true; p2DisableTimer = duration; }
                    break;

                case AbilityType.Dance:
                    if (target == 1) { p1Mult = danceEfficiency; p1MultTimer = duration; }
                    else { p2Mult = danceEfficiency; p2MultTimer = duration; }
                    break;

                case AbilityType.CakeToss:
                    barLock = true;
                    lockPlayer = playerNumber;
                    lockTimer = duration;
                    break;

                case AbilityType.FakeOut:
                    reversed = true;
                    reversedTimer = duration;
                    break;
            }
        }

        #endregion

        #region Character Data

        private struct SimCharacter
        {
            public string Name;
            public AbilityType Ability1Type;
            public float Ability1Cooldown;
            public float Ability1Duration;
            public AbilityType Ability2Type;
            public float Ability2Cooldown;
            public float Ability2Duration;
        }

        private SimCharacter[] GetAllCharacters()
        {
            return new SimCharacter[]
            {
                new SimCharacter {
                    Name = "Eli",
                    Ability1Type = AbilityType.PowerSurge, Ability1Cooldown = 15f, Ability1Duration = 3f,
                    Ability2Type = AbilityType.Flash, Ability2Cooldown = 10f, Ability2Duration = 0.5f
                },
                new SimCharacter {
                    Name = "Lene",
                    Ability1Type = AbilityType.TrashTalk, Ability1Cooldown = 16f, Ability1Duration = 2f,
                    Ability2Type = AbilityType.Shield, Ability2Cooldown = 22f, Ability2Duration = 0f
                },
                new SimCharacter {
                    Name = "Nati",
                    Ability1Type = AbilityType.WinkFlirt, Ability1Cooldown = 12f, Ability1Duration = 1.5f,
                    Ability2Type = AbilityType.Dance, Ability2Cooldown = 18f, Ability2Duration = 4f
                },
                new SimCharacter {
                    Name = "Sabi",
                    Ability1Type = AbilityType.CakeToss, Ability1Cooldown = 20f, Ability1Duration = 2f,
                    Ability2Type = AbilityType.FakeOut, Ability2Cooldown = 24f, Ability2Duration = 2f
                }
            };
        }

        #endregion

        #region Results

        private class MatchupResult
        {
            public int TotalMatches;
            public int Player1Wins;
            public int Player2Wins;
            public int TotalRounds;
            public float TotalDuration;
            public int P1Ability1Uses;
            public int P1Ability2Uses;
            public int P2Ability1Uses;
            public int P2Ability2Uses;
            public int TimeoutRounds;

            public float P1WinRate => TotalMatches > 0 ? (float)Player1Wins / TotalMatches * 100f : 0f;
            public float AvgDuration => TotalMatches > 0 ? TotalDuration / TotalMatches : 0f;
            public float AvgRounds => TotalMatches > 0 ? (float)TotalRounds / TotalMatches : 0f;
            public float TimeoutRate => TotalRounds > 0 ? (float)TimeoutRounds / TotalRounds * 100f : 0f;
        }

        private struct SingleMatchResult
        {
            public int Winner;
            public int RoundsPlayed;
            public float TotalDuration;
            public int P1Ability1Uses;
            public int P1Ability2Uses;
            public int P2Ability1Uses;
            public int P2Ability2Uses;
            public int TimeoutRounds;
        }

        private struct RoundResult
        {
            public int Winner;
            public float Duration;
            public bool Timeout;
            public int P1Ability1Uses;
            public int P1Ability2Uses;
            public int P2Ability1Uses;
            public int P2Ability2Uses;
        }

        #endregion

        #region Report Formatting

        private string FormatReport(SimCharacter[] characters, Dictionary<string, MatchupResult> results,
            Dictionary<string, int> wins, Dictionary<string, int> losses)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("  DIRTY THIRTY SHOWDOWN — BALANCE REPORT");
            sb.AppendLine($"  {matchesPerMatchup} matches per matchup");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine();

            // Overall character rankings
            sb.AppendLine("── OVERALL CHARACTER WIN RATES ──");
            sb.AppendLine();
            foreach (var c in characters)
            {
                int total = wins[c.Name] + losses[c.Name];
                float winRate = total > 0 ? (float)wins[c.Name] / total * 100f : 0f;
                string bar = new string('█', Mathf.RoundToInt(winRate / 5f));
                string rating = winRate > 55f ? "STRONG" : winRate < 45f ? "WEAK" : "BALANCED";
                sb.AppendLine($"  {c.Name,-6} {winRate,5:F1}% [{bar,-20}] {rating}");
            }
            sb.AppendLine();

            // Matchup matrix
            sb.AppendLine("── MATCHUP MATRIX (P1 win %) ──");
            sb.AppendLine();
            sb.Append("        ");
            foreach (var c in characters) sb.Append($"{c.Name,-8}");
            sb.AppendLine();

            foreach (var p1 in characters)
            {
                sb.Append($"  {p1.Name,-6}");
                foreach (var p2 in characters)
                {
                    string key = GetMatchupKey(p1.Name, p2.Name);
                    if (results.TryGetValue(key, out var r))
                    {
                        float winRate = key.StartsWith(p1.Name) ? r.P1WinRate : 100f - r.P1WinRate;
                        sb.Append($"{winRate,6:F1}% ");
                    }
                    else
                    {
                        sb.Append("   —    ");
                    }
                }
                sb.AppendLine();
            }
            sb.AppendLine();

            // Detailed matchup stats
            if (includeDetails)
            {
                sb.AppendLine("── DETAILED MATCHUP STATS ──");
                sb.AppendLine();
                foreach (var kvp in results)
                {
                    var r = kvp.Value;
                    sb.AppendLine($"  {kvp.Key}");
                    sb.AppendLine($"    Matches: {r.TotalMatches}  |  P1 wins: {r.Player1Wins} ({r.P1WinRate:F1}%)  |  P2 wins: {r.Player2Wins} ({100f - r.P1WinRate:F1}%)");
                    sb.AppendLine($"    Avg rounds/match: {r.AvgRounds:F1}  |  Avg duration: {r.AvgDuration:F1}s  |  Timeout rate: {r.TimeoutRate:F1}%");
                    if (r.TotalMatches > 0)
                    {
                        sb.AppendLine($"    P1 abilities: {(float)r.P1Ability1Uses / r.TotalMatches:F1}/{(float)r.P1Ability2Uses / r.TotalMatches:F1} per match");
                        sb.AppendLine($"    P2 abilities: {(float)r.P2Ability1Uses / r.TotalMatches:F1}/{(float)r.P2Ability2Uses / r.TotalMatches:F1} per match");
                    }
                    sb.AppendLine();
                }
            }

            // Balance warnings
            sb.AppendLine("── BALANCE WARNINGS ──");
            sb.AppendLine();
            bool anyWarning = false;
            foreach (var kvp in results)
            {
                if (kvp.Value.P1WinRate > 60f || kvp.Value.P1WinRate < 40f)
                {
                    anyWarning = true;
                    sb.AppendLine($"  ⚠ {kvp.Key}: {kvp.Value.P1WinRate:F1}% — significant imbalance!");
                }
                if (kvp.Value.TimeoutRate > 50f)
                {
                    anyWarning = true;
                    sb.AppendLine($"  ⚠ {kvp.Key}: {kvp.Value.TimeoutRate:F1}% timeout rate — rounds too long?");
                }
            }
            if (!anyWarning)
                sb.AppendLine("  All matchups within acceptable range.");

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════");

            string report = sb.ToString();
            Debug.Log(report);
            return report;
        }

        private string GetMatchupKey(string name1, string name2)
        {
            // Always use alphabetical order to find existing key
            if (string.Compare(name1, name2) <= 0)
                return $"{name1} vs {name2}";
            return $"{name2} vs {name1}";
        }

        #endregion
    }
#endif
}
