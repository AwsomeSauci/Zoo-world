using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Contacts;
using ZooWorld.Application.Feeding;
using ZooWorld.Application.Movement;
using ZooWorld.Application.Simulation;
using ZooWorld.Application.Spawning;
using ZooWorld.Application.Statistics;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;
using ZooWorld.Unity;
using ZooWorld.Unity.Physics;
using ZooWorld.Unity.UI;
using ZooWorld.Unity.World;
using Physics = UnityEngine.Physics;

namespace ZooWorld.Tests.PlayMode
{
    public sealed class ZooSceneTests
    {
        private DiContainer container;
        private SimulationMode previousPhysicsMode;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousPhysicsMode = Physics.simulationMode;
            yield return SceneManager.LoadSceneAsync("Zoo", LoadSceneMode.Single);
            container = Object.FindAnyObjectByType<SceneContext>().Container;
            Object.FindAnyObjectByType<SimulationDriver>().enabled = false;
            Physics.simulationMode = SimulationMode.Script;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Physics.simulationMode = previousPhysicsMode;
            var empty = SceneManager.CreateScene("Empty test scene");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync("Zoo");
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator SavedSceneSpawnsAndSimulatesBothSpecies()
        {
            var pipeline = container.Resolve<SimulationPipeline>();
            var population = container.Resolve<AnimalPopulation>();
            for (var i = 0; i < 3500; i++)
            {
                pipeline.Tick(0.02f);
                Physics.Simulate(0.02f);
                if (i % 500 == 0) yield return null;
            }
            Assert.That(population.Count, Is.GreaterThan(5));
            var roles = container.Resolve<FoodChainRegistry>();
            var prey = 0;
            var predators = 0;
            foreach (var animal in population.All)
            {
                Assert.That(roles.TryGet(animal.Id, out var role), Is.True);
                if (role == ZooWorld.Domain.Feeding.FoodRole.Prey) prey++; else predators++;
            }
            Assert.That(prey, Is.GreaterThan(0));
            Assert.That(predators, Is.GreaterThan(0));
            var counts = container.Resolve<DeathStatistics>().Counts.CurrentValue;
            Assert.That(counts.Prey + counts.Predators, Is.GreaterThan(0));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AutomaticFixedUpdateSpawnsMovesAndConsumes()
        {
            var snake = Create("snake", new PlanarVector(-0.3f, 0));
            var frog = Create("frog", new PlanarVector(0.3f, 0));
            var origin = snake.Body.Position;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            Object.FindAnyObjectByType<SimulationDriver>().enabled = true;
            var population = container.Resolve<AnimalPopulation>();
            var spawned = false;
            var deadline = Time.realtimeSinceStartup + 3;
            while (Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForFixedUpdate();
                foreach (var animal in population.All)
                    if (animal.Id.Value > frog.Id.Value) spawned = true;
            }
            Assert.That(frog.IsAlive, Is.False);
            Assert.That(snake.IsAlive, Is.True);
            Assert.That((snake.Body.Position - origin).Length, Is.GreaterThan(0.1f));
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Prey, Is.GreaterThanOrEqualTo(1));
            Assert.That(spawned, Is.True, "The real Unity clock must drive the spawn schedule.");
        }

        [UnityTest]
        public IEnumerator FailedStepStopsTheSceneAndReleasesEveryPhysicsBody()
        {
            var snake = Create("snake", new PlanarVector(-2, 0));
            var frog = Create("frog", new PlanarVector(2, 0));
            var snakeBody = (AnimalBody)snake.Body;
            var frogBody = (AnimalBody)frog.Body;
            var driver = Object.FindAnyObjectByType<SimulationDriver>();
            var population = container.Resolve<AnimalPopulation>();
            driver.Construct(new SimulationPipeline(new FailingStep()), population);
            LogAssert.Expect(LogType.Exception, new Regex("Expected simulation failure"));
            Physics.simulationMode = SimulationMode.FixedUpdate;
            driver.enabled = true;
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(driver.enabled, Is.False);
            Assert.That(population.Count, Is.Zero);
            Assert.That(snakeBody.gameObject.activeInHierarchy, Is.False);
            Assert.That(frogBody.gameObject.activeInHierarchy, Is.False);
            Assert.That(container.Resolve<FoodChainRegistry>().TryGet(snake.Id, out _), Is.False);
            Assert.That(Physics.simulationMode, Is.EqualTo(SimulationMode.FixedUpdate));
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PlanarAdapterSupportsBoxAndCapsuleThroughTheSamePool()
        {
            foreach (var shapeType in new[] { typeof(BoxCollider), typeof(CapsuleCollider) })
            {
                var root = new GameObject("Adapter test pool");
                root.SetActive(false);
                var prefab = Object.Instantiate(container.Resolve<IAnimalPrefabCatalog>().Get("frog"), root.transform);
                Object.DestroyImmediate(prefab.GetComponent<SphereCollider>());
                var shape = (Collider)prefab.gameObject.AddComponent(shapeType);
                shape.sharedMaterial = container.Resolve<IAnimalPrefabCatalog>().Get("frog").GetComponent<Collider>().sharedMaterial;
                if (shape is BoxCollider box) box.size = new Vector3(1.4f, 0.6f, 0.8f);
                if (shape is CapsuleCollider capsule) { capsule.radius = 0.4f; capsule.height = 1.4f; capsule.direction = 2; }
                var catalog = new SinglePrefabCatalog(prefab);
                using (var pool = new AnimalBodyPool(catalog, container.Resolve<IContactSink>(),
                    GameObject.Find("Animals").transform, root.transform, container.Resolve<AnimalContactFilter>()))
                {
                    var factory = new AnimalFactory(container.Resolve<AnimalCatalog>(), pool);
                    var animal = factory.Create("frog", new PlanarVector(-10.5f, 2));
                    var body = (PlanarAnimalBody)animal.Body;
                    animal.Body.Activate();
                    Physics.SyncTransforms();
                    body.PhysicsBody.linearVelocity = Vector3.right * 3;
                    for (var i = 0; i < 25; i++) Physics.Simulate(0.02f);
                    Assert.That(body.PhysicsBody.linearVelocity.x, Is.LessThan(-0.5f), shapeType.Name);
                    animal.Dispose();
                    using (var reused = factory.Create("frog", new PlanarVector(3, -2)))
                    {
                        reused.Body.Activate();
                        Physics.SyncTransforms();
                        Assert.That(reused.Body, Is.SameAs(body));
                        Assert.That(reused.Id, Is.Not.EqualTo(animal.Id));
                        Assert.That(body.PhysicsBody.position, Is.EqualTo(new Vector3(3, 0, -2)));
                    }
                }
                Object.Destroy(root);
                yield return null;
            }
        }

        private sealed class SinglePrefabCatalog : IAnimalPrefabCatalog
        {
            private readonly AnimalBody prefab;
            public SinglePrefabCatalog(AnimalBody prefab) => this.prefab = prefab;
            public AnimalBody Get(string speciesId) => prefab;
        }

        private sealed class FailingStep : ISimulationStep
        {
            public void Tick(float deltaTime) => throw new System.InvalidOperationException("Expected simulation failure.");
        }

        [UnityTest]
        public IEnumerator BrokenCounterDoesNotStopConsumptionOrDestroySurvivors()
        {
            var snake = Create("snake", new PlanarVector(-2, 0));
            var frog = Create("frog", new PlanarVector(2, 0));
            var hud = Object.FindAnyObjectByType<DeathCountersView>();
            typeof(DeathCountersView).GetField("preyCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(hud, null);
            container.Resolve<ContactBuffer>().Report(snake.Id, frog.Id);
            Physics.simulationMode = SimulationMode.FixedUpdate;
            var driver = Object.FindAnyObjectByType<SimulationDriver>();
            driver.enabled = true;
            LogAssert.Expect(LogType.Exception, new Regex("NullReferenceException"));
            yield return new WaitForFixedUpdate();
            yield return null;
            hud.enabled = false;
            Assert.That(driver.enabled, Is.True);
            Assert.That(snake.IsAlive, Is.True);
            Assert.That(frog.IsAlive, Is.False);
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Prey, Is.EqualTo(1));
            Assert.That(container.Resolve<AnimalPopulation>().Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ConsecutiveMealsShowSeparateLabelsAndReuseThemAfterExpiry()
        {
            var snake = Create("snake", new PlanarVector(-3, 0));
            var first = Create("frog", new PlanarVector(0, 0));
            var second = Create("frog", new PlanarVector(3, 0));
            var contacts = container.Resolve<ContactBuffer>();
            contacts.Report(snake.Id, first.Id);
            contacts.Report(snake.Id, second.Id);
            ResolveContacts();
            Assert.That(Object.FindObjectsByType<TastyLabel>().Length, Is.Zero,
                "Contact processing must not create UI objects.");
            yield return null;
            var labels = Object.FindObjectsByType<TastyLabel>();
            Assert.That(labels.Length, Is.EqualTo(2));
            // ObserveOn delivers in Update; positions are applied in the following LateUpdate.
            yield return null;
            var a = labels[0].GetComponent<RectTransform>().anchoredPosition;
            var b = labels[1].GetComponent<RectTransform>().anchoredPosition;
            Assert.That(Mathf.Abs(a.y - b.y), Is.EqualTo(24).Within(0.01f));
            yield return new WaitForSeconds(1.3f);
            Assert.That(Object.FindObjectsByType<TastyLabel>().Length, Is.Zero);
            var third = Create("frog", new PlanarVector(3, 0));
            contacts.Report(snake.Id, third.Id);
            ResolveContacts();
            yield return null;
            var reused = Object.FindObjectsByType<TastyLabel>();
            Assert.That(reused.Length, Is.EqualTo(1));
            Assert.That(labels, Does.Contain(reused[0]));
            var presenter = Object.FindAnyObjectByType<TastyPresenter>();
            presenter.enabled = false;
            Assert.That(Object.FindObjectsByType<TastyLabel>().Length, Is.Zero);
            presenter.enabled = true;
            yield return null;
            Assert.That(Object.FindObjectsByType<TastyLabel>().Length, Is.Zero);
        }

        [UnityTest]
        public IEnumerator CameraResizeUpdatesTheBoundsBeforeAnimalMovement()
        {
            var camera = Camera.main;
            camera.orthographicSize = 5;
            var snake = Create("snake", new PlanarVector(0, 7));
            container.Resolve<SimulationPipeline>().Tick(0.02f);
            Physics.Simulate(0.02f);
            var bounds = container.Resolve<IWorldBoundsProvider>().Bounds;
            Assert.That(bounds.Max.Y, Is.EqualTo(5).Within(0.01f));
            Assert.That(((PlanarAnimalBody)snake.Body).PhysicsBody.linearVelocity.z, Is.Negative);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisabledViewsDropPendingLabelsAndReplayCurrentCountsOnEnable()
        {
            var snake = Create("snake", new PlanarVector(-3, 0));
            var frog = Create("frog", new PlanarVector(3, 0));
            var presenter = Object.FindAnyObjectByType<TastyPresenter>();
            var counters = Object.FindAnyObjectByType<DeathCountersView>();
            counters.enabled = false;
            container.Resolve<ContactBuffer>().Report(snake.Id, frog.Id);
            ResolveContacts();
            presenter.enabled = false;
            presenter.enabled = true;
            counters.enabled = true;
            yield return null;
            Assert.That(Object.FindObjectsByType<TastyLabel>().Length, Is.Zero);
            Assert.That(counters.transform.Find("Prey Count").GetComponent<TMP_Text>().text, Is.EqualTo("1"));
            var next = Create("frog", new PlanarVector(3, 0));
            container.Resolve<ContactBuffer>().Report(snake.Id, next.Id);
            ResolveContacts();
            yield return null;
            Assert.That(Object.FindObjectsByType<TastyLabel>().Length, Is.EqualTo(1));
            Assert.That(counters.transform.Find("Prey Count").GetComponent<TMP_Text>().text, Is.EqualTo("2"));
        }

        [UnityTest]
        public IEnumerator PhysicsContactConsumesOnceAndUpdatesBothUiAndLabel()
        {
            var snake = Create("snake", new PlanarVector(-0.7f, 0));
            var frog = Create("frog", new PlanarVector(0.7f, 0));
            ((PlanarAnimalBody)snake.Body).PhysicsBody.linearVelocity = Vector3.right * 2;
            ((PlanarAnimalBody)frog.Body).PhysicsBody.linearVelocity = Vector3.left * 2;
            for (var i = 0; i < 15; i++)
            {
                Physics.Simulate(0.02f);
                // Inspect the solver result BEFORE death processing could hide or correct a bounce.
                Assert.That(((PlanarAnimalBody)snake.Body).PhysicsBody.linearVelocity.x, Is.EqualTo(2).Within(0.001f));
                ResolveContacts();
            }
            Assert.That(frog.IsAlive, Is.False);
            Assert.That(snake.IsAlive, Is.True);
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Prey, Is.EqualTo(1));
            yield return null;
            Assert.That(Object.FindObjectsByType<TastyLabel>().Length, Is.EqualTo(1));
            var hud = Object.FindAnyObjectByType<DeathCountersView>();
            Assert.That(hud.transform.Find("Prey Count").GetComponent<TMP_Text>().text, Is.EqualTo("1"));
        }

        [UnityTest]
        public IEnumerator PredatorEatingAnotherPredatorKeepsItsVelocity()
        {
            var first = Create("snake", new PlanarVector(-0.7f, 0));
            var second = Create("snake", new PlanarVector(0.7f, 0));
            ((PlanarAnimalBody)first.Body).PhysicsBody.linearVelocity = Vector3.right * 2;
            ((PlanarAnimalBody)second.Body).PhysicsBody.linearVelocity = Vector3.left * 2;
            for (var i = 0; i < 15; i++)
            {
                Physics.Simulate(0.02f);
                Assert.That(((PlanarAnimalBody)first.Body).PhysicsBody.linearVelocity.x, Is.EqualTo(2).Within(0.001f));
                ResolveContacts();
            }
            Assert.That(first.IsAlive, Is.True);
            Assert.That(second.IsAlive, Is.False);
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Predators, Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ContinuousCollisionConsumptionDoesNotBounceOrMissThePrey()
        {
            var snake = Create("snake", new PlanarVector(-2, 0));
            var frog = Create("frog", new PlanarVector(2, 0));
            ((PlanarAnimalBody)snake.Body).PhysicsBody.linearVelocity = Vector3.right * 100;
            ((PlanarAnimalBody)frog.Body).PhysicsBody.linearVelocity = Vector3.left * 100;
            Physics.Simulate(0.02f);
            Assert.That(((PlanarAnimalBody)snake.Body).PhysicsBody.linearVelocity.x, Is.EqualTo(100).Within(0.01f));
            ResolveContacts();
            Assert.That(frog.IsAlive, Is.False);
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Prey, Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator BothAnimalTypesBounceOffSavedLevelWalls()
        {
            var wall = GameObject.Find("Walls/West").GetComponent<BoxCollider>();
            Assert.That(wall.isTrigger, Is.False);
            foreach (var species in new[] { "frog", "snake" })
            {
                var animal = Create(species, new PlanarVector(wall.bounds.min.x - 2, wall.bounds.center.z));
                var body = (PlanarAnimalBody)animal.Body;
                var start = body.PhysicsBody.position;
                body.PhysicsBody.linearVelocity = Vector3.right * 4;
                for (var i = 0; i < 30; i++) ContactStep();
                Assert.That(body.PhysicsBody.linearVelocity.x, Is.LessThan(-0.5f),
                    $"{species}: start={start}, end={body.PhysicsBody.position}, wall={wall.bounds}, " +
                    $"wallEnabled={wall.enabled}, layersIgnored={Physics.GetIgnoreLayerCollision(body.gameObject.layer, wall.gameObject.layer)}");
                Assert.That(body.Position.X, Is.LessThan(wall.bounds.min.x - body.SpawnRadius), species);
                Assert.That(animal.IsAlive, Is.True);
                container.Resolve<AnimalDeathService>().TryKill(animal.Id);
                container.Resolve<AnimalDeathService>().Tick(0.02f);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpawnPointsAvoidWallsAndOccupiedLevelReturnsNoPosition()
        {
            var source = container.Resolve<ISpawnPointSource>();
            var obstacles = LayerMask.GetMask("Obstacle");
            Assert.That(obstacles, Is.Not.Zero);
            Assert.That(GameObject.Find("Walls").GetComponentsInChildren<BoxCollider>().Length, Is.GreaterThanOrEqualTo(5));
            for (var i = 0; i < 150; i++)
            {
                Assert.That(source.TryFind("frog", out var point), Is.True);
                Assert.That(Physics.CheckSphere(new Vector3(point.X, 0, point.Y), 0.6f, obstacles), Is.False);
            }

            var blocker = new GameObject("Occupied level", typeof(BoxCollider));
            blocker.layer = LayerMask.NameToLayer("Obstacle");
            blocker.GetComponent<BoxCollider>().size = new Vector3(1000, 10, 1000);
            Physics.SyncTransforms();
            Assert.That(source.TryFind("frog", out _), Is.False);
            Object.Destroy(blocker);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TwoPreyBounceWithoutDying()
        {
            var first = Create("frog", new PlanarVector(-0.7f, 0));
            var second = Create("frog", new PlanarVector(0.7f, 0));
            var a = (PlanarAnimalBody)first.Body;
            var b = (PlanarAnimalBody)second.Body;
            a.PhysicsBody.linearVelocity = Vector3.right * 2;
            b.PhysicsBody.linearVelocity = Vector3.left * 2;
            for (var i = 0; i < 20; i++) ContactStep();
            Assert.That(a.PhysicsBody.linearVelocity.x, Is.LessThan(-0.5f));
            Assert.That(b.PhysicsBody.linearVelocity.x, Is.GreaterThan(0.5f));
            Assert.That(first.IsAlive && second.IsAlive, Is.True);
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Prey, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FreshAndReusedBodiesActivateAtTheRequestedPosition()
        {
            var first = Create("frog", new PlanarVector(-6, -1.5f));
            var body = (PlanarAnimalBody)first.Body;
            Assert.That(body.PhysicsBody.position, Is.EqualTo(new Vector3(-6, 0, -1.5f)));
            container.Resolve<AnimalDeathService>().TryKill(first.Id);
            container.Resolve<AnimalDeathService>().Tick(0.02f);
            var reused = Create("frog", new PlanarVector(8, -5));
            Assert.That(reused.Body, Is.SameAs(body));
            Assert.That(body.PhysicsBody.position, Is.EqualTo(new Vector3(8, 0, -5)));
            Physics.Simulate(0.02f);
            Assert.That(body.PhysicsBody.position, Is.EqualTo(new Vector3(8, 0, -5)));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PooledBodyGetsNewIdentityAndIgnoresOldContacts()
        {
            var snake = Create("snake", new PlanarVector(-5, 0));
            var old = Create("frog", PlanarVector.Zero);
            var body = (PlanarAnimalBody)old.Body;
            var deaths = container.Resolve<AnimalDeathService>();
            Assert.That(deaths.TryKill(old.Id), Is.True);
            deaths.Tick(0.02f);
            var replacement = Create("frog", new PlanarVector(5, 0));
            Assert.That(replacement.Body, Is.SameAs(body));
            Assert.That(replacement.Id, Is.Not.EqualTo(old.Id));
            Assert.That(body.PhysicsBody.linearVelocity.sqrMagnitude, Is.Zero);
            container.Resolve<ContactBuffer>().Report(snake.Id, old.Id);
            container.Resolve<ContactResolutionService>().Tick(0.02f);
            Assert.That(replacement.IsAlive, Is.True);
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Prey, Is.Zero);
            body.PhysicsBody.position = new Vector3(-3.6f, 0, 0);
            body.PhysicsBody.linearVelocity = Vector3.left * 3;
            for (var i = 0; i < 25; i++) ContactStep();
            Assert.That(replacement.IsAlive, Is.False);
            Assert.That(snake.IsAlive, Is.True);
            Assert.That(container.Resolve<DeathStatistics>().Counts.CurrentValue.Prey, Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SnakeReturnsFromOutsideCameraBounds()
        {
            var bounds = container.Resolve<IWorldBoundsProvider>().Bounds;
            var snake = Create("snake", new PlanarVector(bounds.Max.X + 1, 0));
            var returned = false;
            for (var i = 0; i < 250; i++)
            {
                snake.Tick(0.02f);
                Physics.Simulate(0.02f);
                if (bounds.Contains(snake.Body.Position, 0.5f)) { returned = true; break; }
            }
            Assert.That(returned, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SnakeReturnsPastEastCornerAfterAspectShrink()
            => ReturnPastObstacleAfterCameraResize("snake", new PlanarVector(8, 0), true);

        [UnityTest]
        public IEnumerator FrogReturnsPastEastCornerAfterAspectShrink()
            => ReturnPastObstacleAfterCameraResize("frog", new PlanarVector(8, 0), true);

        [UnityTest]
        public IEnumerator SnakeReturnsPastSouthWallAfterCameraZoom()
            => ReturnPastObstacleAfterCameraResize("snake", new PlanarVector(0, -8), false);

        [UnityTest]
        public IEnumerator FrogReturnsPastSouthWallAfterCameraZoom()
            => ReturnPastObstacleAfterCameraResize("frog", new PlanarVector(0, -8), false);

        private IEnumerator ReturnPastObstacleAfterCameraResize(string species, PlanarVector start, bool shrinkAspect)
        {
            var animal = Create(species, start);
            if (shrinkAspect) Camera.main.aspect = 0.3f;
            else Camera.main.orthographicSize = 5;
            var boundsProvider = container.Resolve<CameraWorldBounds>();
            boundsProvider.Tick(0.02f);
            var bounds = boundsProvider.Bounds;
            Assert.That(bounds.Contains(start), Is.False, "The resized camera must leave the animal outside the frame.");

            var returned = false;
            for (var i = 0; i < 3000; i++)
            {
                animal.Tick(0.02f);
                Physics.Simulate(0.02f);
                if (bounds.Contains(animal.Body.Position, 0.5f))
                {
                    returned = true;
                    break;
                }
                if (i % 500 == 0) yield return null;
            }
            var end = animal.Body.Position;
            Assert.That(returned, Is.True,
                $"{species} did not return inside the camera's safe bounds after 60 simulated seconds. " +
                $"End=({end.X}, {end.Y}); bounds=({bounds.Min.X}, {bounds.Min.Y})..({bounds.Max.X}, {bounds.Max.Y}).");
        }

        [UnityTest]
        public IEnumerator PhysicalMotorPreservesJumpDistanceInFreeSpace()
        {
            var frog = Create("frog", PlanarVector.Zero);
            var jump = new JumpLocomotion(2.2f, 0.35f, 1.4f, 0.7f);
            for (var i = 0; i < 40; i++)
            {
                frog.Body.ApplyMotion(jump.Step(new PlanarVector(1, 0), 0.02f), 0.02f);
                Physics.Simulate(0.02f);
            }
            Assert.That(frog.Body.Position.X, Is.EqualTo(2.2f).Within(0.015f));
            yield return null;
        }

        private AnimalInstance Create(string species, PlanarVector position)
        {
            var animal = container.Resolve<AnimalFactory>().Create(species, position);
            container.Resolve<AnimalPopulation>().Add(animal);
            animal.Body.Activate();
            Physics.SyncTransforms();
            return animal;
        }

        private void ContactStep()
        {
            Physics.Simulate(0.02f);
            ResolveContacts();
        }

        private void ResolveContacts()
        {
            container.Resolve<ContactResolutionService>().Tick(0.02f);
            container.Resolve<AnimalDeathService>().Tick(0.02f);
        }
    }
}
