using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Models
{
    public class NotifierFactory
    {
        private INotifier _notifier;
        public NotifierFactory(INotifier notifier)
        {
            _notifier = notifier;
        }

        public void Notify(string message)
        {
            _notifier.Notify(message);
        }
    }
}
