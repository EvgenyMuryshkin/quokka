using System;
using System.Reactive.Subjects;
using Quokka.Public.Content;

namespace Quokka.Public.Logging
{
    public class ObservableLogStream : BaseLogStream, IObservable<LogRecord>, IDisposable
    {
        ReplaySubject<LogRecord> _subject = new ReplaySubject<LogRecord>();

        public ObservableLogStream(IContentDomainProvider contentDomainProvider) : base(contentDomainProvider)
        {
        }

        protected override void OnLog(LogRecord record)
        {
            _subject.OnNext(record);
        }

        public IDisposable Subscribe(IObserver<LogRecord> observer)
        {
            return _subject.Subscribe(observer);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _subject.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
