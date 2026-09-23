using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Composition.Movement;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Tests.EditMode
{
    public sealed class MovementTests
    {
        private static readonly WorldBounds Bounds = new WorldBounds(new PlanarVector(-5, -5), new PlanarVector(5, 5));

        [TestCase(0.02f)]
        [TestCase(0.03f)]
        [TestCase(0.07f)]
        public void JumpIntegratesConfiguredDistanceAcrossPartialSteps(float dt)
        {
            var jump = new JumpLocomotion(2.5f, 0.35f, 1.4f, 0.7f);
            var displacement = PlanarVector.Zero;
            for (var elapsed = 0f; elapsed < 0.8f; elapsed += dt)
                displacement += jump.Step(new PlanarVector(1, 0), dt).Velocity * dt;
            Assert.That(displacement.X, Is.EqualTo(2.5f).Within(0.0001f));
            Assert.That(displacement.Y, Is.Zero);
        }

        [Test]
        public void JumpDirectionIsFixedUntilNextJump()
        {
            var jump = new JumpLocomotion(2, 0.4f, 1, 1);
            jump.Step(new PlanarVector(1, 0), 0.1f);
            var inFlight = jump.Step(new PlanarVector(0, 1), 0.1f);
            Assert.That(inFlight.Velocity.X, Is.GreaterThan(0));
            Assert.That(inFlight.Velocity.Y, Is.Zero);
            jump.Step(new PlanarVector(0, 1), 0.8f);
            var next = jump.Step(new PlanarVector(0, 1), 0.1f);
            Assert.That(next.Velocity.Y, Is.GreaterThan(0));
            Assert.That(next.Velocity.X, Is.Zero);
        }

        [Test]
        public void SharedFactoryCreatesIndependentJumpClocks()
        {
            var asset = ScriptableObject.CreateInstance<JumpLocomotionAsset>();
            try
            {
                var factory = asset.CreateFactory();
                var a = factory();
                var b = factory();
                var direction = new PlanarVector(1, 0);
                a.Step(direction, 0.5f);
                Assert.That(a.Step(direction, 0.1f).Velocity.Length, Is.Zero);
                Assert.That(b.Step(direction, 0.1f).Velocity.Length, Is.GreaterThan(0));
            }
            finally { Object.DestroyImmediate(asset); }
        }

        [Test]
        public void CompiledMovementKeepsItsSettingsWhenTheAssetChanges()
        {
            var asset = ScriptableObject.CreateInstance<LinearLocomotionAsset>();
            try
            {
                var factory = asset.CreateFactory();
                var serialized = new SerializedObject(asset);
                serialized.FindProperty("speed").floatValue = 9;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var direction = new PlanarVector(1, 0);
                Assert.That(factory().Step(direction, 1).Velocity.X, Is.EqualTo(2.2f));
                Assert.That(asset.CreateFactory()().Step(direction, 1).Velocity.X, Is.EqualTo(9));
            }
            finally { Object.DestroyImmediate(asset); }
        }

        [Test]
        public void ReturnDirectionOverridesWanderUntilSafelyInside()
        {
            var steering = new ReturnToBoundsBehaviour(new ConstantSteering(new PlanarVector(1, 0)), 1);
            Assert.That(steering.Evaluate(new PlanarVector(6, 0), in Bounds, 0.02f).X, Is.LessThan(0));
            Assert.That(steering.Evaluate(new PlanarVector(4.9f, 0), in Bounds, 0.02f).X, Is.LessThan(0));
            Assert.That(steering.Evaluate(new PlanarVector(3.5f, 0), in Bounds, 0.02f).X, Is.GreaterThan(0));
        }

        [Test]
        public void NewBoundsAreUsedImmediatelyAfterResize()
        {
            var steering = new ReturnToBoundsBehaviour(new ConstantSteering(new PlanarVector(1, 0)), 1);
            var small = new WorldBounds(new PlanarVector(-2, -2), new PlanarVector(2, 2));
            Assert.That(steering.Evaluate(new PlanarVector(4, 0), in Bounds, 0.02f).X, Is.Positive);
            Assert.That(steering.Evaluate(new PlanarVector(4, 0), in small, 0.02f).X, Is.Negative);
        }

        [Test]
        public void BouncingWithoutInwardProgressEventuallyChoosesADetour()
        {
            var steering = new ReturnToBoundsBehaviour(new ConstantSteering(new PlanarVector(1, 0)), 1);
            var direction = PlanarVector.Zero;
            for (var i = 0; i < 175; i++)
                direction = steering.Evaluate(new PlanarVector(6 + i % 20 * 0.01f, 0), in Bounds, 0.02f);
            Assert.That(direction.X, Is.Negative, "The detour should retain an inward component.");
            Assert.That(direction.Y, Is.LessThan(-0.5f), "Bouncing must not keep resetting the progress timer.");
            Assert.That(direction.Length, Is.EqualTo(1).Within(0.0001f));
        }

        [Test]
        public void JumpPausesWithRegularProgressKeepTheDirectReturnDirection()
        {
            var steering = new ReturnToBoundsBehaviour(new ConstantSteering(new PlanarVector(1, 0)), 1);
            for (var jump = 0; jump < 5; jump++)
            {
                var position = new PlanarVector(20 - jump * 2, 0);
                for (var i = 0; i < 70; i++)
                {
                    var direction = steering.Evaluate(position, in Bounds, 0.02f);
                    Assert.That(direction.X, Is.EqualTo(-1));
                    Assert.That(direction.Y, Is.Zero);
                }
            }
        }

        [Test]
        public void CameraChangeCancelsAnObsoleteDetour()
        {
            var steering = new ReturnToBoundsBehaviour(new ConstantSteering(new PlanarVector(1, 0)), 1);
            var position = new PlanarVector(6, 0);
            var direction = PlanarVector.Zero;
            for (var i = 0; i < 175; i++) direction = steering.Evaluate(position, in Bounds, 0.02f);
            Assert.That(direction.Y, Is.Negative);

            var movedBounds = new WorldBounds(new PlanarVector(11, -5), new PlanarVector(21, 5));
            direction = steering.Evaluate(position, in movedBounds, 0.02f);
            Assert.That(direction.X, Is.EqualTo(1));
            Assert.That(direction.Y, Is.Zero);
        }

        [Test]
        public void FinishingReturnClearsRecoveryForTheNextExit()
        {
            var steering = new ReturnToBoundsBehaviour(new ConstantSteering(new PlanarVector(0, 1)), 1);
            for (var i = 0; i < 175; i++) steering.Evaluate(new PlanarVector(6, 0), in Bounds, 0.02f);
            var inside = steering.Evaluate(PlanarVector.Zero, in Bounds, 0.02f);
            Assert.That(inside.X, Is.Zero);
            Assert.That(inside.Y, Is.EqualTo(1));

            var outside = steering.Evaluate(new PlanarVector(6, 0), in Bounds, 0.02f);
            Assert.That(outside.X, Is.EqualTo(-1));
            Assert.That(outside.Y, Is.Zero);
        }

        [Test]
        public void LargeStepCanCrossSeveralJumpCycles()
        {
            var jump = new JumpLocomotion(2, 0.3f, 1, 1);
            var result = jump.Step(new PlanarVector(1, 0), 3);
            Assert.That(result.Velocity.X * 3, Is.EqualTo(6).Within(0.0001f));
        }

        [Test]
        public void InvalidJumpConfigurationIsRejected()
        {
            Assert.Throws<System.ArgumentException>(() => new JumpLocomotion(2, 1, 0.5f, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new LinearLocomotion(float.NaN));
        }

        private sealed class ConstantSteering : ISteeringBehaviour
        {
            private readonly PlanarVector direction;
            public ConstantSteering(PlanarVector direction) => this.direction = direction;
            public PlanarVector Evaluate(PlanarVector position, in WorldBounds bounds, float deltaTime) => direction;
        }
    }
}
