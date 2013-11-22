using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectiveCommons.FiniteStateAutomation
{
    public abstract class StateMachine<TState>
    {
        private readonly object _stateLocker = new object();

        public TState CurrentState { get; private set; }
        public IStateBehaviour CurrentBehaviour { get; set; }
        private IDictionary<String, IStateBehaviour> Behaviours { get; set; }

        protected abstract void EnterStateImplement(TState State);

        protected void EnterState(TState State)
        {
            try { }
            catch (Exception)
            {
                
                throw;
            }
        }

        public StateMachine(IDictionary<string, IStateBehaviour> Behaviours)
        {
            this.Behaviours = Behaviours;
        }

        private void Switch(String NextBehaviourName, TState NextState) { Switch(Behaviours[NextBehaviourName], NextState); }
        private void Switch(IStateBehaviour NextBehaviour, TState NextState)
        {
            CurrentBehaviour.NavigationRequired -= OnNavigationRequired;

            CurrentState = NextState;
            CurrentBehaviour = NextBehaviour;

            CurrentBehaviour.NavigationRequired += OnNavigationRequired;
        }

        private void OnNavigationRequired(object Sender, NavigationEventArgs<TState> NavigationEventArgs)
        {
            Switch(NavigationEventArgs.NextStateName, NavigationEventArgs.NextState);
        }

        public interface IStateBehaviour {
            event EventHandler<NavigationEventArgs<TState>> NavigationRequired;
        }

        public class StateBehaviour<TConcreteState> : IStateBehaviour where TConcreteState : TState
        {
            public IList<ExceptionNavigator> ExceptionNavigators { get; set; }
            public IList<EventNavigator> EventNavigators { get; set; }
            public ContinueNavigator ExitNavigator { get; set; }

            public StateBehaviour() { }

            public StateBehaviour<TConcreteState> Catch<TException, TNext>(String NextStateName,
                                                                 Func<TConcreteState, TException, TNext> NextStateProducer)
                where TException : Exception
                where TNext : TState
            {
                ExceptionNavigators.Add(new ExceptionNavigator<TException, TNext>(NextStateName, NextStateProducer));
                return this;
            }

            public StateBehaviour<TConcreteState> ContinueWith<TNext>(String NextStateName, Func<TConcreteState, TNext> NextStateProducer)
                where TNext : TState
            {
                ExitNavigator = new ContinueNavigator<TNext>(NextStateName, NextStateProducer);
                return this;
            }

            public void ProcessState(TState State)
            {
                try
                {
                    //State.EntryPoint();
                }
                catch (Exception e)
                {
                    var catcher = ExceptionNavigators.FirstOrDefault(en => en.ExceptionType == e.GetType());
                    if (catcher == null) throw;
                    else
                    {
                        OnNavigationRequired(catcher.NextBehaviourName, catcher.GetNextState(State, e));
                    }
                }
            }

            public event EventHandler<NavigationEventArgs<TState>> NavigationRequired;
            protected virtual void OnNavigationRequired(String NextBehaviourName, TState NextState)
            {
                var handler = NavigationRequired;
                if (handler != null) handler(this, new NavigationEventArgs<TState>(NextBehaviourName, NextState));
            }



            public abstract class NavigatorBase
            {
                public String NextBehaviourName { get; protected set; }
            }

            public abstract class ExceptionNavigator : NavigatorBase
            {
                public abstract Type ExceptionType { get; }
                public abstract TState GetNextState(TState State, Exception Exception);
            }

            public class ExceptionNavigator<TException, TNextState> : ExceptionNavigator
                where TException : Exception
                where TNextState : TState
            {
                public override Type ExceptionType
                {
                    get { return typeof(TException); }
                }

                public Func<TState, TException, TNextState> NewStateFactory { get; private set; }

                public override TState GetNextState(TState State, Exception Exception)
                {
                    return NewStateFactory((TState)State, (TException)Exception);
                }

                public ExceptionNavigator(String NextBehaviourName, Func<TState, TException, TNextState> NewStateFactory)
                {
                    this.NextBehaviourName = NextBehaviourName;
                    this.NewStateFactory = NewStateFactory;
                }
            }

            public abstract class EventNavigator : NavigatorBase { }
            public class EventNavigator<TNextState> : EventNavigator
                where TNextState : TState
            {
                public Func<TState, TNextState> NewStateFactory { get; private set; }

                public EventNavigator(String NextBehaviourName, Func<TState, TNextState> NewStateFactory)
                {
                    this.NextBehaviourName = NextBehaviourName;
                    this.NewStateFactory = NewStateFactory;
                }
            }


            public abstract class ContinueNavigator : NavigatorBase { }
            public class ContinueNavigator<TNextState> : ContinueNavigator
                where TNextState : TState
            {
                public Func<TConcreteState, TNextState> NewStateFactory { get; private set; }

                public ContinueNavigator(String NextBehaviourName, Func<TConcreteState, TNextState> NewStateFactory)
                {
                    this.NextBehaviourName = NextBehaviourName;
                    this.NewStateFactory = NewStateFactory;
                }
            }
        }

        public class NavigationEventArgs<TState> : EventArgs
        {
            public String NextStateName { get; private set; }
            public TState NextState { get; private set; }

            public NavigationEventArgs(string NextStateName, TState NextState)
            {
                this.NextStateName = NextStateName;
                this.NextState = NextState;
            }
        }


    }
}