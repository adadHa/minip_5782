﻿using System;
using System.Collections.Generic;
using System.Linq;
using BO;
using System.Runtime.CompilerServices;
namespace BL
{
    internal sealed partial class BL : BlApi.IBL
    {

        //this function adds a station to the database
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddStation(int id, string name, int freeChargingSlots, Location location)
        {

            try
            {
                lock (dalObject)
                {
                    dalObject.AddStation(id, name, freeChargingSlots, location.Longitude, location.Latitude); 
                }
            }
            catch (DalApi.IdIsAlreadyExistException e)
            {
                throw new IdIsAlreadyExistException(e.ToString());
            }
            catch (DalApi.NoChargeSlotsException e)
            {
                throw new NoChargeSlotsException(e.ToString());
            }

        }

        //This function updates a station with a new name and/or a new charging slots capacity.
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateStation(int id, string newName, int newNum)
        {
            try
            {
                lock (dalObject)
                {
                    if (newName != "") // check if there was an input for this value
                    {
                        dalObject.UpdateStationName(id, newName);
                    }
                    if (newNum != -1) // check if there was an input for this value
                    {
                        if (newNum >= 0) // it should be a positive number
                            dalObject.UpdateStationChargeSlotsCap(id, newNum);
                        else
                            throw new ArgumentOutOfRangeException("charging slots capacity");
                    } 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }
        //this function view the station details
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string ViewStation(int id)
        {
            lock (dalObject)
            {
                return GetStation(id).ToString(); 
            }
        }
        //This function returns a StationForList from the datasource (on BL) by an index.
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Station GetStation(int id)
        {
            try
            {
                lock (dalObject)
                {
                    List<DO.DroneCharge> l = dalObject.GetDroneCharges(x => x.StationId == id).ToList();
                    List<DroneInCharge> listOfDronesInCharge = new List<DroneInCharge>();
                    foreach (DO.DroneCharge droneCharge in l)
                    {
                        DroneForList BLDrone = BLDrones[GetBLDroneIndex(droneCharge.DroneId)];
                        listOfDronesInCharge.Add(new DroneInCharge
                        {
                            DroneId = BLDrone.Id,
                            BatteryStatus = BLDrone.Battery
                        });
                    }

                    DO.Station station = dalObject.GetStation(id);
                    Station resultStation = new Station
                    {
                        Id = station.Id,
                        Name = station.Name,
                        Location = new Location { Latitude = station.Latitude, Longitude = station.Longitude },
                        FreeChargeSlots = station.FreeChargeSlots,
                        ListOfDronesInCharge = listOfDronesInCharge
                    };
                    return resultStation; 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }
        //this function view the station details
        public string ViewStationsList()
        {
            string result = "";
            foreach (var item in GetStations())
            {
                result += item.ToString() + "\n";
            }
            return result;
        }
        public string ViewStationsWithFreeChargeSlots()
        {
            string result = "";
            foreach (var item in GetStations(x => x.FreeChargeSlots > 0))
            {
                result += item.ToString() + "\n";
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<StationForList> GetStations(Func<DO.Station, bool> filter = null)
        {
            lock (dalObject)
            {
                List<DO.Station> dalStations = dalObject.GetStations(filter).ToList();
                List<StationForList> resultList = new List<StationForList>();
                int occupiedChargingSlots = 0;
                foreach (DO.Station station in dalStations)
                {
                    occupiedChargingSlots = dalObject.GetDroneCharges(x => x.StationId == station.Id).Count();
                    resultList.Add(new StationForList
                    {
                        Id = station.Id,
                        Name = station.Name,
                        AvailableChargingSlots = station.FreeChargeSlots,
                        OccupiedChargingSlots = occupiedChargingSlots
                    });
                }
                return resultList; 
            }
        }

        //This function return the most close station's location to a given loaction
        //check on a filtered stations list if a filter was provided
        [MethodImpl(MethodImplOptions.Synchronized)]
        private Location GetMostCloseStationLocation(Location location, Func<DO.Station, bool> filter = null)
        {
            List<DO.Station> stations;
            lock (dalObject)
            {
                stations = dalObject.GetStations(filter).ToList(); // we choose from the available stations. 
            }
            if (stations.Count() == 0) return null;
            DO.Station mostCloseStation = stations[0];
            double mostCloseDistance = 0;
            double distance = 0;
            foreach (DO.Station station in stations)
            {
                Location stationL = new Location { Latitude = station.Latitude, Longitude = station.Longitude };
                distance = Distance(stationL, location);
                if (mostCloseDistance > distance)
                {
                    mostCloseDistance = distance;
                    mostCloseStation = station;
                }
            }
            return new Location { Latitude = mostCloseStation.Latitude, Longitude = mostCloseStation.Longitude };
        }

    }
}
/*public BL()
       {
           IDal.IDal dalObject = new DalObject.DalObject();
           

           BLDrones = (List<Drone>)dalObject.ViewDronesList();
           List<StationForList> stations = (List<StationForList>)dalObject.ViewStationsList();
           for (int i = 0; i < BLDrones.Count; i++)
           {
               Drone drone = BLDrones[i];
               if (drone.Status != DroneStatuses.Available)
               {
                   drone.Status = (DroneStatuses)rand.Next(0, 2);
                   if (drone.Status == DroneStatuses.Maintenance)
                   {
                       randomStation = stations[rand.Next(0, stations.Count)].;
                       drone.Battery  = rand.NextDouble() * 20;
                   }
                   if (drone.Status == DroneStatuses.Shipping)
                   {

                   }
               }
           }
         }          */