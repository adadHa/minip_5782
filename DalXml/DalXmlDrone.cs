﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DalApi;
using DO;
using System.Runtime.CompilerServices;

namespace DalXml
{
    public sealed partial class DalXml : IDal
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDal.AddDrone(int id, string model, string weight)
        {
            List<Drone> list = XMLTools.LoadListFromXMLSerializer<Drone>(DronesPath);
            if (list.FindIndex(x => x.Id == id) != -1)
                throw new IdIsAlreadyExistException(id, "Drone");
            list.Add(
                new Drone
                {
                    Id = id,
                    Model = model,
                    MaxWeight = (DO.WheightCategories)Enum.Parse(typeof(DO.WheightCategories), weight)
                });
            XMLTools.SaveListToXMLSerializer<Drone>(list, DronesPath);
        }

        #region Charge and update functions
        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDal.ChargeDrone(int droneId, int stationId)
        {
            List<Drone> dronesList = XMLTools.LoadListFromXMLSerializer<Drone>(DronesPath);
            List<Station> stationsList = XMLTools.LoadListFromXMLSerializer<Station>(StationsPath);
            List<DroneCharge> dronesChargesList = XMLTools.LoadListFromXMLSerializer<DroneCharge>(DronesChargesPath);

            if (dronesList.FindIndex(x => x.Id == droneId) == -1)
                throw new IdIsNotExistException(droneId, "Drone");
            if (stationsList.FindIndex(x => x.Id == stationId) == -1)
                throw new IdIsNotExistException(stationId, "Station");

            Station station = stationsList.Find(x => x.Id == stationId);
            if (station.FreeChargeSlots == 0)
                throw new NoChargeSlotsException(station);
            dronesChargesList.Add(new DroneCharge { DroneId = droneId, StationId = stationId });
            stationsList.Remove(station);
            station.FreeChargeSlots--;
            stationsList.Add(station);

            XMLTools.SaveListToXMLSerializer<DroneCharge>(dronesChargesList, DronesChargesPath);
            XMLTools.SaveListToXMLSerializer<Station>(stationsList, StationsPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDal.StopCharging(int droneId)
        {
            List<Drone> dronesList = XMLTools.LoadListFromXMLSerializer<Drone>(DronesPath);
            List<Station> stationsList = XMLTools.LoadListFromXMLSerializer<Station>(StationsPath);
            List<DroneCharge> dronesChargesList = XMLTools.LoadListFromXMLSerializer<DroneCharge>(DronesChargesPath);

            if (dronesList.FindIndex(x => x.Id == droneId) != -1)
                throw new IdIsNotExistException(droneId, "Drone");
            Drone drone = dronesList.Find(x => x.Id == droneId);
            DroneCharge droneCharge = dronesChargesList.Find(x => x.DroneId == droneId);
            Station station = stationsList.Find(x => x.Id == droneCharge.StationId);

            stationsList.Remove(station);
            station.FreeChargeSlots--;
            stationsList.Add(station);
            dronesChargesList.Remove(droneCharge);

            XMLTools.SaveListToXMLSerializer<DroneCharge>(dronesChargesList, DronesChargesPath);
            XMLTools.SaveListToXMLSerializer<Station>(stationsList, StationsPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDal.UpdateDrone(int droneId, string newModel)
        {
            List<Drone> dronesList = XMLTools.LoadListFromXMLSerializer<Drone>(DronesPath);
            if (dronesList.FindIndex(x => x.Id == droneId) != -1)
                throw new IdIsNotExistException(droneId, "Drone");
            Drone drone = dronesList.Find(x => x.Id == droneId);
            dronesList.Remove(drone);
            drone.Model = newModel;
            dronesList.Add(drone);

            XMLTools.SaveListToXMLSerializer<Drone>(dronesList, DronesPath);
        }
        #endregion
        #region get functions
        [MethodImpl(MethodImplOptions.Synchronized)]
        Drone IDal.GetDrone(int id)
        {
            List<Drone> list = XMLTools.LoadListFromXMLSerializer<Drone>(DronesPath);
            Drone? drone = list.Find(x => x.Id == id);
            if (drone != null)
                return (Drone)drone;
            else
                throw new IdIsNotExistException(id, "Drone");
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        IEnumerable<Drone> IDal.GetDrones(Func<Drone, bool> filter = null)
        {
            List<Drone> list = XMLTools.LoadListFromXMLSerializer<Drone>(DronesPath);
            if (filter == null) return list;
            List<Drone> result = (List <Drone>)list.Where(filter);
            return result;
        }

        #endregion
        [MethodImpl(MethodImplOptions.Synchronized)]
        IEnumerable<Drone> IDal.ViewDronesList()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        string IDal.ViewDrone(int id)
        {
            throw new NotImplementedException();
        }

    }

}
