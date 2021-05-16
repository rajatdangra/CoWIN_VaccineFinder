using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Co_WIN_Status
{
    public class VaccineFinder
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public VaccineFinder()
        {
        }

        public void CheckVaccineAvailabilityStatus(UserDetails userDetails)
        {
            try
            {
                string stInfo = "Status API Call Started.\n";
                logger.Info(stInfo);
                Console.WriteLine(stInfo);

                bool vaccineSlotFound = false;
                while (!vaccineSlotFound)
                {
                    StringBuilder slots = new StringBuilder();
                    APIResponse response = APIs.CheckCalendarByPin(userDetails);
                    foreach (var center in response.centers)
                    {
                        foreach (var session in center.sessions)
                        {
                            if (session.available_capacity > 0 && session.min_age_limit <= userDetails.AgeCriteria)
                            {
                                vaccineSlotFound = true;
                                var details = string.Format("Date: {0}, Name: {1}, Centre ID: {2}, Min Age: {3}, Available Capacity: {4}, Address: {5}", session.date, center.name, center.center_id, session.min_age_limit, session.available_capacity, center.address);
                                slots.Append(details + "\n");
                            }
                        }
                    }
                    if (vaccineSlotFound)
                    {
                        var slotDetails = "Vaccine Slots are available:\n" + slots.ToString();
                        stInfo = "Slots Found: Status API Call End.";
                        Co_WIN_Status.Email.SendEmail(slotDetails);
                        Console.WriteLine(stInfo);
                        logger.Info(stInfo);
                        break;
                    }
                    else
                    {
                        stInfo = "No Slots Found. Last status checked: " + DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss tt");
                        logger.Info(stInfo);
                        Console.WriteLine(stInfo);
                        Thread.Sleep(AppConfig.PollingTime);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in CheckVaccineAvailabilityStatus:\n" + ex);
            }
        }
    }
}
