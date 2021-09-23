using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DmvAppointmentScheduler
{
    class Program
    {
        public static Random random = new Random();
        public static List<Appointment> appointmentList = new List<Appointment>();
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();
            Calculation(customers, tellers);
            OutputTotalLengthToConsole();

        }
        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }

        static void Calculation(CustomerList customers, TellerList tellers)
        {
            // Your code goes here .....
            // Re-write this method to be more efficient instead of a assigning all customers to the same teller
           
            // foreach(Customer customer in customers.Customer)
            // {
            //     var appointment = new Appointment(customer, tellers.Teller[0]);
            //     appointmentList.Add(appointment);
            // }

            //My code
            //1. Add total duration property for each teller.
            
            //2. Add FindSuitableTeller method:
            //+ Find a teller that matches customer type and has the least total duration.
            //+ Find a teller that has the least total duration without matching customer type.
            //+ Return the teller who has a lesser new total duration.
            //+ Pick each customer one by one to find a suitable teller and assign an appointment.
            //+ Calculate the new total duration for the chosen teller.


            foreach (Customer customer in customers.Customer)
            {
                var suitableTeller = FindSuitableTeller(customer, tellers);
                var appointment = new Appointment(customer, suitableTeller);
                appointmentList.Add(appointment);
                var teller = tellers.Teller.Where((t) =>
                {
                    return t.id == suitableTeller.id;
                }).First();
                teller.totalDuration += appointment.duration;
            }
        }
        // My method
        private static Teller FindSuitableTeller(Customer customer, TellerList tellers)
        {
            var suitableTellersByType =
                from teller in tellers.Teller
                where teller.specialtyType == customer.type
                orderby teller.totalDuration
                select teller;

            var suitableTellerByType = suitableTellersByType.Count() > 0 ? suitableTellersByType.Take(1).ToArray()[0] : null;

            var suitableTellerByDuration =
                (from teller in tellers.Teller
                 orderby teller.totalDuration
                 select teller).Take(1).ToArray()[0];

            if (suitableTellerByType != null)
            {
                var totalDuration1 = suitableTellerByType.totalDuration
                        + Math.Ceiling(Convert.ToDouble(customer.duration) * Convert.ToDouble(suitableTellerByType.multiplier));

                var totalDuration2 = suitableTellerByDuration.totalDuration
                    + Math.Ceiling(Convert.ToDouble(customer.duration) * Convert.ToDouble(suitableTellerByDuration.multiplier));

                return totalDuration1 < totalDuration2 ? suitableTellerByType : suitableTellerByDuration;
            }
            else
            {
                return suitableTellerByDuration;
            }
        }



        static void OutputTotalLengthToConsole()
        {
            var tellerAppointments =
                from appointment in appointmentList
                group appointment by appointment.teller into tellerGroup
                select new
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };
            var max = tellerAppointments.OrderBy(i => i.totalDuration).LastOrDefault();
            Console.WriteLine("Teller " + max.teller.id + " will work for " + max.totalDuration + " minutes!");
        }

    }
}
