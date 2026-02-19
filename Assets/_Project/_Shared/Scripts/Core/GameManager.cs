using System.Collections;
using UnityEngine;
using Brawler.Fighter;
using Brawler.Arena;
using Brawler.Input;

namespace Brawler.Core
{
    /// <summary>
    /// Manages the match flow: rounds, KOs, respawns, and win conditions.
    ///
    /// SCAFFOLD - Students wire up the match logic.
    /// See Lesson 02: Wiring GameManager for step-by-step guide.
    ///
    /// Match flow:
    ///   1. Match starts with Countdown
    ///   2. Players fight until one is KO'd (blast zone)
    ///   3. Round ends, winner gets a point
    ///   4. First to roundsToWin wins the match
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Match settings (rounds, timing, etc.)")]
        [SerializeField] private MatchConfig matchConfig;

        [Header("References - Wire these in Inspector")]
        [Tooltip("Spawn points for each player (index 0 = P1, index 1 = P2)")]
        [SerializeField] private SpawnPoint[] spawnPoints;

        [Tooltip("Fighter prefabs or references (assign after spawning)")]
        [SerializeField] private FighterBase[] fighters;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;

        // Match state
        public GameState CurrentState { get; private set; } = GameState.Waiting;
        public int CurrentRound { get; private set; } = 0;
        public int[] RoundWins { get; private set; } = new int[2]; // [P1 wins, P2 wins]

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameManager] Multiple instances! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // TODO STEP 1: Subscribe to GameEvents.OnFighterKO
            // GameEvents.OnFighterKO += OnFighterKO;

            // Validate required references before auto-starting
            if (!ValidateReferences()) return;

            StartMatch();
        }

        /// <summary>
        /// Check that all required Inspector references are assigned.
        /// Logs specific, actionable errors for each missing reference.
        /// </summary>
        private bool ValidateReferences()
        {
            bool valid = true;

            if (matchConfig == null)
            {
                Debug.LogError("[GameManager] MatchConfig not assigned! " +
                    "Create one: Right-click in Project > Create > Brawler > Match Config, " +
                    "then drag it into the GameManager's Match Config field.", this);
                valid = false;
            }

            if (fighters == null || fighters.Length < 2)
            {
                Debug.LogError("[GameManager] Fighters array needs 2 entries! " +
                    "Set Fighters array size to 2, then drag your P1 fighter into Element 0 " +
                    "and P2 fighter into Element 1.", this);
                valid = false;
            }
            else
            {
                if (fighters[0] == null)
                {
                    Debug.LogError("[GameManager] Fighters Element 0 (Player 1) is empty! " +
                        "Drag your P1 fighter from the Hierarchy into the Fighters array Element 0.", this);
                    valid = false;
                }
                if (fighters.Length < 2 || fighters[1] == null)
                {
                    Debug.LogError("[GameManager] Fighters Element 1 (Player 2) is empty! " +
                        "Drag your P2 fighter from the Hierarchy into the Fighters array Element 1.", this);
                    valid = false;
                }
            }

            if (spawnPoints == null || spawnPoints.Length < 2 || spawnPoints[0] == null || spawnPoints[1] == null)
            {
                Debug.LogWarning("[GameManager] Spawn Points array is incomplete. " +
                    "Set size to 2 and assign SpawnPoint_P1 (Element 0) and SpawnPoint_P2 (Element 1). " +
                    "Fighters will still work but won't respawn at correct positions.", this);
            }

            return valid;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // TODO: Unsubscribe from events
            // GameEvents.OnFighterKO -= OnFighterKO;
        }

        /// <summary>
        /// Start a new match.
        /// </summary>
        public void StartMatch()
        {
            if (matchConfig == null)
            {
                Debug.LogError("[GameManager] MatchConfig not assigned! StartMatch() aborted — " +
                    "fighters will not be initialized. Create a MatchConfig asset and assign it.", this);
                return;
            }

            // Initialize fighters with their input handlers
            for (int i = 0; i < fighters.Length; i++)
            {
                if (fighters[i] != null)
                {
                    var inputHandler = fighters[i].GetComponent<PlayerInputHandler>();
                    if (inputHandler != null)
                    {
                        inputHandler.Initialize(i);
                    }
                    else
                    {
                        Debug.LogError($"[GameManager] Fighter '{fighters[i].gameObject.name}' (Element {i}) " +
                            "has no PlayerInputHandler component! Add one and assign BrawlerInputActions.", this);
                    }
                    fighters[i].Initialize(i, inputHandler);
                    Log($"Fighter '{fighters[i].FighterName}' ({fighters[i].gameObject.name}) initialized as Player {i + 1}");
                }
            }

            CurrentRound = 0;
            RoundWins[0] = 0;
            RoundWins[1] = 0;

            Log("Match starting!");

            StartRound();
        }

        /// <summary>
        /// Start a new round.
        /// </summary>
        private void StartRound()
        {
            CurrentRound++;
            Log($"Round {CurrentRound} starting!");

            // TODO STEP 2: Position fighters at spawn points
            // for (int i = 0; i < fighters.Length; i++)
            // {
            //     if (fighters[i] != null && spawnPoints[i] != null)
            //     {
            //         fighters[i].Respawn(spawnPoints[i].Position);
            //     }
            // }

            // TODO STEP 3: Start countdown
            // SetState(GameState.Countdown);
            // StartCoroutine(CountdownCoroutine());
        }

        /*
         * TODO STEP 3: Implement countdown coroutine
         *
        private IEnumerator CountdownCoroutine()
        {
            // Fire round start event (for UI)
            GameEvents.OnRoundStart?.Invoke(CurrentRound);

            // Wait for countdown
            yield return new WaitForSeconds(matchConfig.roundStartDelay);

            // Start fighting!
            SetState(GameState.Fighting);
        }
        */

        /// <summary>
        /// Called when a fighter is KO'd.
        /// TODO STEP 1: Subscribe to this in Start()
        /// </summary>
        private void OnFighterKO(FighterKOEventArgs args)
        {
            if (CurrentState != GameState.Fighting) return;

            Log($"Player {args.PlayerIndex} KO'd via {args.ZoneType}!");

            // The player who got KO'd loses the round
            // So the OTHER player wins
            int winnerIndex = args.PlayerIndex == 0 ? 1 : 0;

            // TODO STEP 4: End the round
            // StartCoroutine(EndRoundCoroutine(winnerIndex));
        }

        /*
         * TODO STEP 4: Implement end round coroutine
         *
        private IEnumerator EndRoundCoroutine(int roundWinner)
        {
            SetState(GameState.RoundEnd);

            // Award round win
            RoundWins[roundWinner]++;
            GameEvents.OnRoundEnd?.Invoke(roundWinner);
            GameEvents.OnRoundScoreChanged?.Invoke(roundWinner, RoundWins[roundWinner]);

            Log($"Player {roundWinner} wins round {CurrentRound}! Score: P1={RoundWins[0]} P2={RoundWins[1]}");

            // Wait before next round
            yield return new WaitForSeconds(matchConfig.roundEndDelay);

            // TODO STEP 5: Check win condition
            // CheckMatchEnd(roundWinner);
        }
        */

        /*
         * TODO STEP 5: Implement win condition check
         *
        private void CheckMatchEnd(int lastRoundWinner)
        {
            // Check if someone has won enough rounds
            if (RoundWins[lastRoundWinner] >= matchConfig.roundsToWin)
            {
                // Match over!
                EndMatch(lastRoundWinner);
            }
            else
            {
                // Next round
                StartRound();
            }
        }
        */

        /// <summary>
        /// End the match with a winner.
        /// </summary>
        public void EndMatch(int winnerIndex)
        {
            SetState(GameState.MatchEnd);
            Log($"GAME! Player {winnerIndex} wins the match!");

            GameEvents.OnMatchEnd?.Invoke(winnerIndex);
        }

        /// <summary>
        /// Pause/unpause the match.
        /// </summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Fighting)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (CurrentState == GameState.Paused)
            {
                SetState(GameState.Fighting);
                Time.timeScale = 1f;
            }
        }

        private void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            GameEvents.OnGameStateChanged?.Invoke(newState);

            if (logStateChanges)
            {
                Log($"State changed to: {newState}");
            }
        }

        private void Log(string message)
        {
            if (logStateChanges)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }

        /// <summary>
        /// Assign fighter references.
        /// Call this after spawning fighters.
        /// </summary>
        public void SetFighters(FighterBase player1, FighterBase player2)
        {
            fighters = new FighterBase[] { player1, player2 };
        }

        /// <summary>
        /// Get the spawn point for a player.
        /// </summary>
        public SpawnPoint GetSpawnPoint(int playerIndex)
        {
            if (spawnPoints == null || playerIndex >= spawnPoints.Length)
                return null;
            return spawnPoints[playerIndex];
        }
    }
}
