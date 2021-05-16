﻿using NLog;
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
                string stInfo = "Status API Call Started for PinCode: " + userDetails.PinCode;
                logger.Info(stInfo);
                Console.WriteLine(stInfo);

                bool vaccineSlotFound = false;
                while (!vaccineSlotFound)
                {
                    StringBuilder slots = new StringBuilder();
                    APIResponse response = APIs.CheckCalendarByPin(userDetails);
                    int counter = 0;
                    foreach (var center in response.centers)
                    {
                        foreach (var session in center.sessions)
                        {
                            if (session.available_capacity > 0 && session.min_age_limit <= userDetails.AgeCriteria)
                            {
                                vaccineSlotFound = true;
                                counter++;
                                var details = string.Format(counter + ") Date: {0}, Name: {1}, Centre ID: {2}, Min Age: {3}, Available Capacity: {4}, Address: {5}", session.date, center.name, center.center_id, session.min_age_limit, session.available_capacity, center.address);
                                slots.Append(details + "\n");
                            }
                        }
                    }
                    if (vaccineSlotFound)
                    {
                        var slotDetails = "Hi" + (!string.IsNullOrWhiteSpace(userDetails.FullName) ? " " + userDetails.FullName : "") + ",\n\nVaccine Slots are available for PinCode: " + userDetails.PinCode + "\n\n" + slots.ToString() + "\nRegards,\nYour Vaccine Finder :)";
                        stInfo = "Slots Found: Status API Call End.";
                        Console.WriteLine(stInfo);
                        logger.Info(stInfo);
                        Co_WIN_Status.Email.SendEmail(slotDetails);
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
                int waitTime = 10;
                Console.WriteLine("Program will be Automatically closed in " + waitTime + " Seconds");
                Thread.Sleep(waitTime * 1000);
            }
            catch (Exception ex)
            {
                logger.Error("Error in CheckVaccineAvailabilityStatus:\n" + ex);
            }
        }
    }
}
