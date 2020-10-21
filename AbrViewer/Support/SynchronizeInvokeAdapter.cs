using System;
using System.ComponentModel;
using System.Threading;

namespace AbrViewer.Support
{
    public class SynchronizeInvokeAdapter : ISynchronizeInvoke
    {
        private readonly SynchronizationContext _ctx;

        public SynchronizeInvokeAdapter(SynchronizationContext ctx) {
            _ctx = ctx;
        }

        public SynchronizeInvokeAdapter() : this(SynchronizationContext.Current) {
        }

        public IAsyncResult BeginInvoke(Delegate method, object[] args) {
            if (_ctx != null) {
                _ctx.Post(_ => method.DynamicInvoke(args), null);
            } else {
                method.DynamicInvoke(args);
            }
            return null;
        }

        public object EndInvoke(IAsyncResult result) {
            return null;
        }

        public object Invoke(Delegate method, object[] args) {
            if(_ctx != null) { 
                _ctx.Send(_ => method.DynamicInvoke(args), null);
            } else {
                method.DynamicInvoke(args);
            }
            return null;
        }

        public bool InvokeRequired => true;
    }
}