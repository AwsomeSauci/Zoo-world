using System;
using R3;
using TMPro;
using UnityEngine;
using Zenject;
using ZooWorld.Application.Statistics;

namespace ZooWorld.Unity.UI
{
    public sealed class DeathCountersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text preyCount;
        [SerializeField] private TMP_Text predatorCount;
        private IDeathStatistics statistics;
        private IDisposable subscription;

        [Inject]
        public void Construct(IDeathStatistics statistics)
        {
            this.statistics = statistics;
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void Subscribe()
        {
            if (statistics == null || subscription != null || !isActiveAndEnabled) return;
            subscription = statistics.Counts.Subscribe(Render, error => Debug.LogException(error, this), _ => { });
        }

        private void Render(DeathCounts counts)
        {
            preyCount.text = counts.Prey.ToString();
            predatorCount.text = counts.Predators.ToString();
        }

        private void OnDisable()
        {
            subscription?.Dispose();
            subscription = null;
        }
    }
}
