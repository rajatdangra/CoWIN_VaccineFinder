using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Models
{
    public class NotifierFactory
    {
        public NotifierFactory(INotifier notifier)
        {
            _notifier = notifier;
        }
        private INotifier _notifier;

        public void Notify(string message)
        {
            _notifier.Notify(message);
        }
    }
}
