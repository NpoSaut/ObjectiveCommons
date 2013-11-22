using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectiveCommons.FiniteStateAutomation;

namespace ObjectiveCommonsTests
{
    [TestClass]
    public class StateMachineTests
    {
        private class TestStateContext
        {
        }

        private abstract class TestState : IState
        {
            public int cycle { get; set; }
            public abstract void EntryPoint();
        }
        private class TestStateWorker : TestState
        {
            public override void EntryPoint() { cycle++; }
        }
        private class TestStatePrint : TestState
        {
            public override void EntryPoint() { Console.WriteLine("couner is {0}", cycle); }
        }
        private class TestStateDone : TestState
        {

            public override void EntryPoint() { Console.WriteLine("counting done"); }
        }


        [TestMethod]
        public void TestMethod1()
        {
            var sm = new StateMachine<TestState, TestStateContext>(
                new List<IStateBehaviour>()
                {
                    new StateBehaviour<TestState>()
                        .ContinueWith("Print", Worker => new TestStatePrint()),
                    new StateBehaviour<TestState>()
                        .ContinueWith("Counting")
                },
                new TestStateContext());
        }
    }
}
