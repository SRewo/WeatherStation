using System;
using System.Collections.Generic;
using System.Linq;

namespace WeatherStation.Library
{
    public abstract class FunctionalBuilder<TObject, TSelf>
        where TSelf : FunctionalBuilder<TObject, TSelf>
        where TObject : new()
    {
        private readonly List<Func<TObject, TObject>> _actions = new List<Func<TObject, TObject>>();

        public TSelf Do(Action<TObject> action)
        {
            return AddAction(action);
        }

        private TSelf AddAction(Action<TObject> action)
        {
            _actions.Add(x =>
            {
                action(x);
                return x;
            });
            return (TSelf) this;
        }

        public TObject Build()
        {
            return _actions.Aggregate(new TObject(), (x, f) => f(x));
        }
    }
}