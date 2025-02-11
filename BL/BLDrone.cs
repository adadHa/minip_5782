﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BO;
using System.Runtime.CompilerServices;

namespace BL
{
    internal sealed partial class BL : BlApi.IBL
    {
        //this functions initialize the drones
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void InitializeDrones()
        {
            lock (dalObject)
            {
                List<DO.Drone> dalDrones = (List<DO.Drone>)dalObject.GetDrones();
                foreach (DO.Drone dalDrone in dalDrones)
                {
                    DroneForList newDrone = new DroneForList
                    {
                        Id = dalDrone.Id,
                        Model = dalDrone.Model,
                        MaxWeight = (BO.WheightCategories)dalDrone.MaxWeight
                    };
                    if (dalObject.GetParcels(x => x.Delivered == null && x.DroneId == dalDrone.Id).Count() == 1)
                    {
                        DO.Parcel p = dalObject.GetParcels(x => x.Delivered == null && x.DroneId == dalDrone.Id).ToList()[0];
                        newDrone.DeliveredParcelNumber = p.Id;
                        newDrone.Status = DroneStatuses.Shipping;

                        Location senderLocation = new Location
                        {
                            Latitude = dalObject.GetCustomer(p.SenderId).Latitude,
                            Longitude = dalObject.GetCustomer(p.SenderId).Longitude
                        };
                        Location targetLocation = new Location
                        {
                            Latitude = dalObject.GetCustomer(p.TargetId).Latitude,
                            Longitude = dalObject.GetCustomer(p.TargetId).Longitude
                        };

                        if (p.PickedUp == null) // parcel was binded but not picked up
                        {
                            newDrone.Location = GetMostCloseStationLocation(senderLocation);
                            //calculate the battery
                            double consumptionRate = getConsumptionRate((WheightCategories)dalDrone.MaxWeight);
                            double minimalBattery = (Distance(newDrone.Location, senderLocation)
                                                    + Distance(senderLocation, targetLocation)) * consumptionRate;
                            newDrone.Battery = rand.NextDouble();
                            if (newDrone.Battery < minimalBattery) newDrone.Battery = minimalBattery;
                        }

                        else if (p.Delivered == null) // parcel was picked up but not deliverd
                        {
                            newDrone.Location = senderLocation;
                            //calculate the battery
                            double consumptionRate = getConsumptionRate((WheightCategories)dalDrone.MaxWeight);
                            double minimalBattery = Distance(senderLocation, targetLocation) * consumptionRate;
                            newDrone.Battery = rand.NextDouble() * 100;
                            if (newDrone.Battery < minimalBattery) newDrone.Battery = minimalBattery;
                        }
                    }
                    else
                    {
                        newDrone.Status = (DroneStatuses)rand.Next(0, 2); // rand between Available and Maintenance
                        newDrone.DeliveredParcelNumber = -1;
                    }

                    if (newDrone.Status == DroneStatuses.Maintenance)
                    {
                        List<DO.Station> stations = dalObject.GetStations().ToList();
                        DO.Station chosenStation = stations[0];
                        foreach (DO.Station station in stations)
                        {
                            if (station.FreeChargeSlots != 0)
                            {
                                chosenStation = station;
                                newDrone.Location = new Location { Latitude = station.Latitude, Longitude = station.Longitude };
                                newDrone.Battery = rand.NextDouble() * 20;
                                break;
                            }
                        }
                        if (chosenStation.FreeChargeSlots != 0)
                        {
                            dalObject.ChargeDrone(newDrone.Id, chosenStation.Id);
                        }
                    }

                    else if (newDrone.Status == DroneStatuses.Available)
                    {
                        // build the customersWithRecievedParcels list:
                        List<DO.Customer> customersWithRecievedParcels = new List<DO.Customer>();
                        foreach (DO.Customer customer in dalObject.GetCustomers())
                        {
                            if (dalObject.GetParcels(p => p.TargetId == customer.Id).Count() > 0)
                                customersWithRecievedParcels.Add(customer);
                        }
                        int randIndex = rand.Next(0, customersWithRecievedParcels.Count());
                        DO.Customer randCustomer = customersWithRecievedParcels[randIndex];
                        newDrone.Location = new Location { Latitude = randCustomer.Latitude, Longitude = randCustomer.Longitude };

                        // calculate the battery
                        double consumptionRate = getConsumptionRate((WheightCategories)dalDrone.MaxWeight);
                        double minimalBattery = Distance(newDrone.Location, GetMostCloseStationLocation(newDrone.Location))
                                                * consumptionRate; // the desired battery to overcame the distance between the most close station to the drone
                        newDrone.Battery = rand.NextDouble() * 100;
                        if (newDrone.Battery < minimalBattery) newDrone.Battery = minimalBattery;
                    }
                    newDrone.Battery = Math.Round(newDrone.Battery, 1);
                    BLDrones.Add(newDrone);
                }
            }
        }



        //this function adds a drone to the database
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDrone(int id, string model, string weight, int initialStationId)
        {
            try
            {
                lock (dalObject)
                {
                    double batteryStatus = Math.Round(rand.NextDouble() * rand.Next(20, 41), 1);
                    DO.Station initialStation = dalObject.GetStation(initialStationId);
                    if (initialStation.FreeChargeSlots == 0)
                        throw new DalApi.NoChargeSlotsException(initialStation);
                    dalObject.AddDrone(id, model, weight);
                    BLDrones.Add(new BO.DroneForList
                    {
                        Id = id,
                        Model = model,
                        MaxWeight = (BO.WheightCategories)Enum.Parse(typeof(BO.WheightCategories), weight),
                        Battery = batteryStatus,
                        DeliveredParcelNumber = -1,
                        Status = BO.DroneStatuses.Maintenance,
                        Location = new BO.Location()
                        {
                            Longitude = initialStation.Longitude,
                            Latitude = initialStation.Longitude
                        }

                    });

                    dalObject.ChargeDrone(id, initialStationId);
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
        //this function updates the drone
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateDrone(int id, string newModel)
        {
            try
            {
                lock (dalObject)
                {
                    BLDrones[GetBLDroneIndex(id)].Model = newModel;
                    dalObject.UpdateDrone(id, newModel); 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }
        //this fuction charge a drone who needs to be charged
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ChargeDrone(int id)
        {
            try
            {
                lock (dalObject)
                {
                    // First, we look for the closet station.
                    DroneForList drone = BLDrones[GetBLDroneIndex(id)];
                    if (drone == null)
                        throw new DalApi.IdIsNotExistException(id, "Drone");
                    if (drone.Status != DroneStatuses.Available)
                        throw new DroneCannotBeChargedException(drone);
                    List<DO.Station> stations =
                        dalObject.GetStations(x => x.FreeChargeSlots > 0).ToList(); // we choose from the available stations.
                    DO.Station mostCloseStation = stations[0];
                    double mostCloseDistance = 0;
                    double distance = 0;
                    foreach (DO.Station station in stations)
                    {
                        Location stationL = new Location { Latitude = station.Latitude, Longitude = station.Longitude };
                        distance = Distance(stationL, drone.Location);
                        if (mostCloseDistance > distance)
                        {
                            mostCloseDistance = distance;
                            mostCloseStation = station;
                        }
                    }

                    // now we check if the battery status of the drone allow it to get there.
                    double consumptionRate = getConsumptionRate(drone.MaxWeight);
                    // Heavy -- heavyDrElectConsumption
                    if (drone.Battery <= mostCloseDistance * consumptionRate)
                    {
                        throw new NotEnoughBatteryException(drone, mostCloseStation);
                    }

                    else
                    {
                        //finaly we change desired fields
                        dalObject.ChargeDrone(id, mostCloseStation.Id);
                        int droneIndex = BLDrones.FindIndex(x => x == drone);
                        drone.Location.Latitude = mostCloseStation.Latitude;
                        drone.Location.Longitude = mostCloseStation.Longitude;
                        drone.Battery -= Math.Round(mostCloseDistance * consumptionRate, 1);
                        drone.Status = DroneStatuses.Maintenance;
                        BLDrones[droneIndex] = drone;
                    } 
                }
            }
            catch (DalApi.NoChargeSlotsException e)
            {
                throw new NoChargeSlotsException(e.ToString());
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }

        //This function get an wheight category and returns its power consumption rate
        // if no parameter is given, the function will return the charging rate
        [MethodImpl(MethodImplOptions.Synchronized)]
        private double getConsumptionRate(WheightCategories? maxWeight = null)
        {
            switch (maxWeight)
            {
                case WheightCategories.Light:
                    return lightDrElectConsumption;
                case WheightCategories.Medium:
                    return mediumDrElectConsumption;
                case WheightCategories.Heavy:
                    return heavyDrElectConsumption;
                default:
                    return chargingRate;
            }
        }

        //this function release a drone from charge and updates his baterry status after the charge
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReleaseDroneFromCharging(int id, double chargingTime)
        {
            try
            {
                lock (dalObject)
                {
                    DroneForList d = BLDrones[GetBLDroneIndex(id)];
                    if (d.Status != DroneStatuses.Maintenance)
                    {
                        throw new DroneCannotBeReleasedException(d);
                    }
                    else
                    {
                        dalObject.StopCharging(id);
                        d.Status = DroneStatuses.Available;
                        d.Battery += chargingTime * dalObject.ViewElectConsumptionData()[0];
                        BLDrones[GetBLDroneIndex(id)] = d;
                    } 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }

        // This functions returns the distanse between two locations.
        private double Distance(Location l1, Location l2)
        {
            // calculate the distance using Pythagoras 
            double a = Math.Pow(l1.Latitude - l2.Latitude, 2); // a = (x1-x2)^2
            double b = Math.Pow(l1.Longitude - l2.Longitude, 2);// b = (y1 - y2)^2
            return Math.Sqrt(a + b); // dis = sqrt(a - b)
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void BindDrone(int id)
        {
            try
            {
                lock (dalObject)
                {
                    DroneForList drone = BLDrones[GetBLDroneIndex(id)];
                    if (drone.Status != DroneStatuses.Available)
                        throw new DroneIsAlreadyBindException(drone);
                    List<DO.Parcel> availableParcels =
                        dalObject.GetParcels(p => IsAbleToDoDelivery(drone, p)).ToList(); // get only available parcels
                    if (availableParcels.Count() == 0)
                        throw new NoParcelsToBindException(drone);
                    DO.Parcel bestParcelToBind = availableParcels[0];
                    foreach (DO.Parcel parcel in availableParcels)
                    {
                        if (IsMoreGoodToBind(drone, parcel, bestParcelToBind))
                        {
                            bestParcelToBind = parcel;
                        }
                    }
                    BLDrones[GetBLDroneIndex(id)].Status = DroneStatuses.Shipping;
                    BLDrones[GetBLDroneIndex(id)].DeliveredParcelNumber = bestParcelToBind.Id;
                    dalObject.BindParcel(bestParcelToBind.Id, id); 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }

        //This function determine whether a drone is able to do a delivery <=>
        // 1. its wheight capacity is large enough.
        // 2. and is able to drive the path:
        //   drone current location --> sender location --> target location --> most close station (to the target)
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsAbleToDoDelivery(DroneForList drone, DO.Parcel p)
        {
            lock (dalObject)
            {
                if ((int)p.Wheight > (int)drone.MaxWeight) return false;
                DO.Customer sender = dalObject.GetCustomer(p.SenderId);
                Location senderLoaction = new Location { Latitude = sender.Latitude, Longitude = sender.Longitude };
                DO.Customer target = dalObject.GetCustomer(p.TargetId);
                Location targetLoaction = new Location { Latitude = target.Latitude, Longitude = target.Longitude };
                double consumptionRate = getConsumptionRate(drone.MaxWeight);
                double totalDistance = Distance(drone.Location, senderLoaction) + Distance(senderLoaction, targetLoaction)
                                        + Distance(targetLoaction, GetMostCloseStationLocation(targetLoaction));
                double desiredBattery = totalDistance * consumptionRate;
                if (drone.Battery >= desiredBattery)
                    return true;
                else return false; 
            }
        }

        //This function determine who, of two customers, has a more suitable parcel to bind to the drone.
        // the function returns true if the parcel of customer1 is more suitable.
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool IsMoreGoodToBind(DroneForList drone, DO.Parcel parcel1, DO.Parcel parcel2)
        {
            DO.Customer p1Sender = dalObject.GetCustomer(parcel1.SenderId);
            Location p1SenderLocation = new Location { Latitude = p1Sender.Latitude, Longitude = p1Sender.Longitude };
            DO.Customer p2Sender = dalObject.GetCustomer(parcel2.SenderId);
            Location p2SenderLocation = new Location { Latitude = p2Sender.Latitude, Longitude = p2Sender.Longitude };
            if (parcel1.Priority > parcel2.Priority)
                return true;
            else if (parcel1.Priority == parcel2.Priority &&
                Distance(drone.Location, p1SenderLocation) < Distance(drone.Location, p2SenderLocation))
                return true;
            else return false;
        }

        //This function collects a parcel by its shipping drone
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CollectParcelByDrone(int id)
        {
            try
            {
                lock (dalObject)
                {
                    DroneForList drone = BLDrones[GetBLDroneIndex(id)];
                    if (drone.Status != DroneStatuses.Shipping)
                        throw new DroneCannotCollectParcelException(drone);
                    DO.Parcel parcel = dalObject.GetParcels(p => p.DroneId == id).ToList()[0];
                    if (parcel.PickedUp != null)
                        throw new DroneCannotCollectParcelException(drone, parcel);
                    DO.Customer sender = dalObject.GetCustomer(parcel.SenderId);
                    dalObject.CollectParcelByDrone(id, parcel.Id);

                    // update battery and location
                    Location senderLocation = new Location { Latitude = sender.Latitude, Longitude = sender.Longitude };
                    double consumption = Distance(drone.Location, senderLocation) * getConsumptionRate(drone.MaxWeight);
                    drone.Battery -= consumption;
                    drone.Location = senderLocation;
                    BLDrones[GetBLDroneIndex(id)] = drone; 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }

        // This function ,akes a drone supply its parcel to its target customer.
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SupplyParcel(int id)
        {
            try
            {
                lock (dalObject)
                {
                    DroneForList drone = BLDrones[GetBLDroneIndex(id)];
                    if (drone.Status != DroneStatuses.Shipping)
                        throw new DroneCannotSupplyParcelException(drone);
                    DO.Parcel parcel = dalObject.GetParcels(p => p.DroneId == id).ToList()[0];
                    if (parcel.Delivered != null)
                        throw new DroneCannotSupplyParcelException(drone, parcel);
                    // update the parcel on data layer:
                    dalObject.SupplyParcelToCustomer(parcel.Id);
                    // update the drone on BL layer:
                    drone.Status = DroneStatuses.Available;
                    DO.Customer target = dalObject.GetCustomer(parcel.TargetId);
                    Location targetLoaction = new Location { Latitude = target.Latitude, Longitude = target.Longitude };
                    double consumption = Distance(drone.Location, targetLoaction) * getConsumptionRate(drone.MaxWeight);
                    drone.Battery -= consumption;
                    drone.Location = targetLoaction;
                    BLDrones[GetBLDroneIndex(id)] = drone; 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }


        //this function view the drones details
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string ViewDrone(int id)
        {
            lock (dalObject)
            {
                return GetDrone(id).ToString(); 
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Drone GetDrone(int id)
        {
            try
            {
                lock (dalObject)
                {
                    if (BLDrones.FindIndex(x => x.Id == id) == -1) throw new IdIsNotExistException(id, "Drone");
                    DroneForList drone = BLDrones[GetBLDroneIndex(id)];
                    ParcelInTransfer transferedParcel = new ParcelInTransfer();
                    if (drone.Status == DroneStatuses.Shipping)
                    {
                        DO.Parcel p = dalObject.GetParcels(x => x.DroneId == id).ToList()[0];
                        transferedParcel.Id = p.Id;
                        transferedParcel.Wheight = (WheightCategories)p.Wheight;
                        transferedParcel.Priority = (Priorities)p.Priority;
                        transferedParcel.Sender = new CustomerInDelivery { Id = p.SenderId, Name = dalObject.GetCustomer(p.SenderId).Name };
                        transferedParcel.Receiver = new CustomerInDelivery { Id = p.TargetId, Name = dalObject.GetCustomer(p.TargetId).Name };
                        transferedParcel.PickUpLocation = new Location { Latitude = dalObject.GetCustomer(p.SenderId).Latitude, Longitude = dalObject.GetCustomer(p.SenderId).Longitude };
                        transferedParcel.TargetLocation = new Location { Latitude = dalObject.GetCustomer(p.TargetId).Latitude, Longitude = dalObject.GetCustomer(p.TargetId).Longitude };
                    }

                    Drone resultDrone = new Drone
                    {
                        Id = drone.Id,
                        Model = drone.Model,
                        MaxWeight = drone.MaxWeight,
                        Status = drone.Status,
                        Battery = drone.Battery,
                        Location = drone.Location,
                        TransferedParcel = transferedParcel
                    };
                    return resultDrone; 
                }
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }

        //This function returns an index to the desired drone id from the list on BL database
        [MethodImpl(MethodImplOptions.Synchronized)]
        private int GetBLDroneIndex(int id)
        {
            try
            {
                int i = 0;
                lock (dalObject)
                {
                    i = BLDrones.FindIndex(x => x.Id == id); 
                }
                if (i == -1)
                {
                    throw new DalApi.IdIsNotExistException(id, "Drone"); // ??? should we throw Dal exception about bl's drone exception?
                }
                return i;
            }
            catch (DalApi.IdIsNotExistException e)
            {
                throw new IdIsNotExistException(e);
            }
        }

        public string ViewDronesList()
        {
            string result = "";
            foreach (var item in GetDrones())
            {
                result += item.ToString() + "\n";
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DroneForList> GetDrones(Func<DroneForList, bool> filter = null)
        {
            lock (dalObject)
            {
                if (filter == null)
                    return BLDrones;
                List<DroneForList> a = BLDrones.Where(filter).ToList();
                return BLDrones.Where(filter).ToList(); 
            }
        }
    }


}

