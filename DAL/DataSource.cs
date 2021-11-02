﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalObject
{
    struct DataSource
    {
        static internal List<IDAL.DO.Drone> Drones = new List<IDAL.DO.Drone>();
        static internal List<IDAL.DO.Station> Stations = new List<IDAL.DO.Station>();
        static internal List<IDAL.DO.Customer> Customers = new List<IDAL.DO.Customer>();
        static internal List<IDAL.DO.Parcel> Parcels = new List<IDAL.DO.Parcel>();
        static Random rand = new Random();
        internal class Config
        {
            public static int availableDrElectConsumption;
            public static int lightDrElectConsumption;
            public static int mediumDrElectConsumption;
            public static int heavyDrElectConsumption;
            public int chargingRate;

        }

        //This function initailizes the data structures.
        static public void Initialize()
        {
            // initialize customers
            string[] names = { "Yosi","Dani","Avi", "Rafi", "Yoel"
                        , "David", "Israel", "Levi", "Ben", "Moti"};
            string[] phoneNumbers = { "05467651241", "0524931828", "0526067135",
                    "0527839442", "0524711136", "0588824245", "0586934625",
                    "0542444196", "0549035643", "0542463885" };
            int[] ids = new int[10];
            for (int i = 0; i < 10; i++)
                ids[i] = rand.Next(10000000, 99999999);
            for (int i = 0; i < 10; i++)
            {
                Customers.Add(new IDAL.DO.Customer()
                {
                    Id = ids[i],
                    Name = names[i],
                    Phone = phoneNumbers[i],
                    Longitude = rand.NextDouble() * 180,
                    Latitude = rand.NextDouble() * 180
                });
            }

            // generate general random values that will guide the initialization.
            int shippedParcels = rand.Next(0, 5);  // 0-5 shipped parcels

            // initialize shipped parcels
            for (int i = 0; i < shippedParcels; i++)
            {
                IDAL.DO.WheightCategories wheight = (IDAL.DO.WheightCategories)rand.Next(0, 3);
                IDAL.DO.Priorities priority = (IDAL.DO.Priorities)rand.Next(0, 3);
                DateTime defaultDate = new DateTime();
                Parcels.Add(new IDAL.DO.Parcel()
                {
                    Id = i,
                    SenderId = Customers[rand.Next(0, 9)].Id,
                    TargetId = Customers[rand.Next(0, 9)].Id,
                    Wheight = wheight,
                    Priority = priority,
                    DroneId = i,
                    Requested = DateTime.Now,
                    Scheduled = defaultDate,
                    PickedUp = defaultDate,
                    Delivered = defaultDate
                });
            }
            //initialize waiting parcels
            for (int i = shippedParcels; i < 10; i++)
            {
                IDAL.DO.WheightCategories wheight = (IDAL.DO.WheightCategories)rand.Next(0, 3);
                IDAL.DO.Priorities priority = (IDAL.DO.Priorities)rand.Next(0, 3);
                DateTime defaultDate = new DateTime();
                Parcels.Add(new IDAL.DO.Parcel()
                {
                    Id = shippedParcels + i,
                    SenderId = Customers[rand.Next(0, 9)].Id,
                    TargetId = Customers[rand.Next(0, 9)].Id,
                    Wheight = wheight,
                    Priority = priority,
                    DroneId = 0,
                    Requested = DateTime.Now,
                    Scheduled = defaultDate,
                    PickedUp = defaultDate,
                    Delivered = defaultDate
                });
            }
            //initialize stations
            for (int i = 0; i < 2; i++)
            {
                Stations.Add(new IDAL.DO.Station()
                {
                    Id = i,
                    Name = "station" + i.ToString(),
                    Longitude = rand.NextDouble() * 180,
                    Latitude = rand.NextDouble() * 180,
                    ChargeSlots = rand.Next(0, 5)
                });
            }

            //initialize Drones
            for (int i = 0; i < shippedParcels; i++) // drones which send the "shipped parcels"
            {
                IDAL.DO.WheightCategories maxWeight = (IDAL.DO.WheightCategories)rand.Next(0, 3);
                double battery = rand.NextDouble() * 100;
                Drones.Add(new IDAL.DO.Drone()
                {
                    Id = i,
                    Model = "",
                    MaxWeight = maxWeight,
                    Status = IDAL.DO.DroneStatuses.Shipping,
                    Battery = battery
                });
            }
            for (int i = 5 - shippedParcels; i > 0; i--) // the other drones which are'nt on shipping mode.
            {
                IDAL.DO.WheightCategories maxWeight = (IDAL.DO.WheightCategories)rand.Next(0, 3);
                IDAL.DO.DroneStatuses status = (IDAL.DO.DroneStatuses)rand.Next(0, 2); // "Available" or "Maintenance"
                double battery = rand.NextDouble() * 100;
                Drones.Add(new IDAL.DO.Drone()
                {
                    Id = shippedParcels + i,
                    Model = "",
                    MaxWeight = maxWeight,
                    Status = status,
                    Battery = battery
                });
            }
        }
    }
}





