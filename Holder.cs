using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectiveCommons
{
    public class Holder
    {
        public bool IsHolding { get; private set; }

        public Action ReleaseAction { get; set; }

        public Holder()
        {
            IsHolding = false;
        }
        public Holder(Action ReleaseAction)
        {
            IsHolding = false;
            this.ReleaseAction = ReleaseAction;
        }

        public HoldToken Hold()
        {
            return new HoldToken(this);
        }

        public class HoldToken : IDisposable
        {
            public Holder Parent { get; private set; }

            internal HoldToken(Holder OnHolder)
            {
                lock (OnHolder)
                {
                    if (OnHolder.IsHolding == false)
                    {
                        OnHolder.IsHolding = true;
                        Parent = OnHolder;
                    }
                }
            }

            public void Dispose()
            {
                if (Parent != null)
                lock (Parent)
                {
                    Parent.IsHolding = false;
                    if (Parent.ReleaseAction != null) Parent.ReleaseAction.Invoke();
                }
            }
        }
    }
}
