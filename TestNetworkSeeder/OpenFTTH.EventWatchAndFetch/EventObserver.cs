using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.EventWatchAndFetch
{
    public class EventObserver<BaseEventType> : IObserver<BaseEventType>
    {
        private IDisposable unsubscriber;

        private List<BaseEventType> _events = new List<BaseEventType>();

        private Predicate<BaseEventType> _startCriteria;

        private Predicate<BaseEventType> _stopCriteria;

        private bool _startCriteriaMeet = false;

        private bool _stopCriteriaMeet = false;


        public virtual void Subscribe(IObservable<BaseEventType> provider, Predicate<BaseEventType> startCriteria, Predicate<BaseEventType> stopCriteria)
        {
            unsubscriber = provider.Subscribe(this);
            _startCriteria = startCriteria;
            _stopCriteria = stopCriteria;
        }

        public bool StartCriteriaMeet => _startCriteriaMeet;
        public bool StopCriteriaMeet => _stopCriteriaMeet;

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            // Do nothing.
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(BaseEventType value)
        {
            if (_startCriteriaMeet && !_stopCriteriaMeet)
                _events.Add(value);

            if (!_startCriteriaMeet)
            {
                if (_startCriteria.Invoke(value))
                {
                    _startCriteriaMeet = true;
                    _events.Add(value); // We want the event that triggers start two
                }
            }
            else if (!_stopCriteriaMeet)
            {
                if (_stopCriteria.Invoke(value))
                    _stopCriteriaMeet = true;
            }
        }
    }
}
