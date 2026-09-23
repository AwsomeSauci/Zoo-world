using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Feeding;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Unity.UI
{
    public sealed class TastyPresenter : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private RectTransform labelRoot;
        [SerializeField] private TastyLabel prefab;
        [SerializeField, Min(0.1f)] private float lifetime = 1.15f;
        private readonly List<Entry> active = new List<Entry>();
        private readonly Stack<TastyLabel> pool = new Stack<TastyLabel>();
        private readonly Dictionary<AnimalId, int> stacks = new Dictionary<AnimalId, int>();
        private IConsumptionEvents events;
        private IAnimalLookup animals;
        private IDisposable subscription;

        private struct Entry
        {
            public TastyLabel Label;
            public AnimalId Owner;
            public float Remaining;
        }

        [Inject]
        public void Construct(IConsumptionEvents events, IAnimalLookup animals)
        {
            this.events = events;
            this.animals = animals;
            Subscribe();
        }

        private void OnEnable() => Subscribe();
        private void Subscribe()
        {
            if (events == null || subscription != null || !isActiveAndEnabled) return;
            subscription = events.Consumed.ObserveOn(UnityFrameProvider.Update)
                .Subscribe(Show, error => Debug.LogException(error, this), _ => { });
        }

        private void Show(Consumption consumption)
        {
            if (!animals.TryGetAlive(consumption.Eater, out _)) return;
            var label = pool.Count > 0 ? pool.Pop() : Instantiate(prefab, labelRoot);
            label.gameObject.SetActive(true);
            active.Add(new Entry { Label = label, Owner = consumption.Eater, Remaining = lifetime });
        }

        private void LateUpdate()
        {
            stacks.Clear();
            var kept = 0;
            for (var i = 0; i < active.Count; i++)
            {
                var entry = active[i];
                entry.Remaining -= Time.deltaTime;
                if (entry.Remaining <= 0 || !animals.TryGetAlive(entry.Owner, out var animal))
                {
                    Return(entry.Label);
                    continue;
                }
                var position = animal.Body.Position;
                var screen = worldCamera.WorldToScreenPoint(new Vector3(position.X, 0, position.Y));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(labelRoot, screen, null, out var local);
                stacks.TryGetValue(entry.Owner, out var stack);
                stacks[entry.Owner] = stack + 1;
                entry.Label.Render(local + Vector2.down * (32 + stack * 24), entry.Remaining);
                active[kept++] = entry;
            }
            active.RemoveRange(kept, active.Count - kept);
        }

        private void Return(TastyLabel label)
        {
            label.gameObject.SetActive(false);
            pool.Push(label);
        }

        private void OnDisable()
        {
            subscription?.Dispose();
            subscription = null;
            foreach (var entry in active) Return(entry.Label);
            active.Clear();
        }
    }
}
