using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSoft.HIS
{
    public class OrderResponse : IDisposable
    {
        public bool Succeed { get; set; }
        public string Description { get; set; }
        public object ResultValue { get; set; }

        public OrderResponse(bool succeed, string desc, object resultValue)
        {
            Succeed = succeed;
            Description = desc;
            ResultValue = resultValue;
        }

        private bool disposed;

        ~OrderResponse()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;
            if (disposing)
            {
                Succeed = false;
                Description = string.Empty;
                ResultValue = null;
            }

            this.disposed = true;
        }
    }
}
