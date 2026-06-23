using System.Collections.Generic;
using FPSGame.AI;
using FPSGame.Core;
using FPSGame.Save;
using UnityEngine;

namespace FPSGame.UI
{
    /// <summary>
    /// Wins the campaign level when all hostile units are eliminated and spawn waves are finished.
    /// Unlocks the next level via SaveProfileService when launched from the campaign menu.
    /// </summary>
    public class CampaignLevelWinController : MonoBehaviour
    {
        [SerializeField] private GameplayVictoryUI victoryUi;
        [SerializeField] private bool requireCampaignContext = true;
        [SerializeField] private float winCheckDelaySeconds = 0.15f;

        private readonly HashSet<CharacterBase> aliveEnemies = new HashSet<CharacterBase>();
        private EnemySpawnPoint[] spawnPoints;
        private bool hasWon;
        private bool hadHostileEncounter;
        private float pendingWinCheckAt = -1f;

        private void Start()
        {
            spawnPoints = FindObjectsOfType<EnemySpawnPoint>();

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                    spawnPoints[i].EnemySpawned += RegisterEnemy;
            }

            RegisterExistingEnemies();
        }

        private void OnDestroy()
        {
            if (spawnPoints == null)
                return;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                    spawnPoints[i].EnemySpawned -= RegisterEnemy;
            }

            foreach (CharacterBase enemy in aliveEnemies)
            {
                if (enemy != null)
                    enemy.OnDied -= HandleEnemyDied;
            }

            aliveEnemies.Clear();
        }

        private void Update()
        {
            if (hasWon || pendingWinCheckAt < 0f)
                return;

            if (Time.unscaledTime < pendingWinCheckAt)
                return;

            pendingWinCheckAt = -1f;
            TryCompleteLevel();
        }

        public void ForceWinForDebug()
        {
            if (hasWon)
                return;

            CompleteLevel(unlockProgression: CampaignSessionState.HasActiveCampaignContext
                && !CampaignSessionState.IsSecretStage);
        }

        private void RegisterExistingEnemies()
        {
            CharacterBase[] characters = FindObjectsOfType<CharacterBase>();
            for (int i = 0; i < characters.Length; i++)
                RegisterEnemy(characters[i]);
        }

        private void RegisterEnemy(CharacterBase character)
        {
            if (character == null || character.Faction != Faction.Enemy || !character.IsAlive)
                return;

            if (!aliveEnemies.Add(character))
                return;

            hadHostileEncounter = true;
            character.OnDied += HandleEnemyDied;
            ScheduleWinCheck();
        }

        private void HandleEnemyDied()
        {
            aliveEnemies.RemoveWhere(enemy => enemy == null || !enemy.IsAlive);
            ScheduleWinCheck();
        }

        private void ScheduleWinCheck()
        {
            pendingWinCheckAt = Time.unscaledTime + winCheckDelaySeconds;
        }

        private void TryCompleteLevel()
        {
            if (hasWon)
                return;

            aliveEnemies.RemoveWhere(enemy => enemy == null || !enemy.IsAlive);
            if (aliveEnemies.Count > 0)
                return;

            if (!AreSpawnWavesFinished())
                return;

            if (requireCampaignContext && !CampaignSessionState.HasActiveCampaignContext)
                return;

            bool unlock = CampaignSessionState.HasActiveCampaignContext
                && !CampaignSessionState.IsSecretStage;

            CompleteLevel(unlock);
        }

        private bool AreSpawnWavesFinished()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return hadHostileEncounter && aliveEnemies.Count == 0;

            bool anySpawnStarted = false;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                EnemySpawnPoint point = spawnPoints[i];
                if (point == null)
                    continue;

                if (point.IsSpawning)
                    return false;

                if (point.HasRun)
                    anySpawnStarted = true;
            }

            return anySpawnStarted;
        }

        private void CompleteLevel(bool unlockProgression)
        {
            if (hasWon)
                return;

            hasWon = true;

            string message = BuildVictoryMessage(unlockProgression);
            if (unlockProgression)
            {
                SaveProfileService.UnlockNextLevelAfterWin(
                    CampaignSessionState.ActiveCampaign,
                    CampaignSessionState.ActiveLevel);
            }

            GameplayPauseService.PushPause();
            victoryUi?.Show(message);
            Debug.Log($"[CampaignLevelWin] Level complete. unlock={unlockProgression}");
        }

        private static string BuildVictoryMessage(bool unlockProgression)
        {
            if (!unlockProgression)
                return "Mission Complete!";

            int campaign = CampaignSessionState.ActiveCampaign;
            int level = CampaignSessionState.ActiveLevel;

            if (level < CampaignKeys.LevelsPerCampaign)
                return $"Level {level} complete!\nLevel {level + 1} unlocked.";

            if (campaign < CampaignKeys.CampaignCount)
                return $"Campaign {campaign} complete!\nCampaign {campaign + 1} Level 1 unlocked.";

            return "Campaign complete!";
        }
    }
}
