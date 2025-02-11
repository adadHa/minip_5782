﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DalApi;
using DO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;

namespace DalXml
{
    public sealed partial class DalXml : IDal
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDal.AddCustomer(int id, string name, string phoneNumber, double longitude, double latitude)
        {
            XElement cutomersRootElement = XMLTools.LoadListFromXMLElement(CustomersPath);
            XElement customer = (from c in cutomersRootElement.Elements()
                                where (int.Parse(c.Element("id").Value) == id)
                                select c).FirstOrDefault();

            if (customer != null)
                throw new IdIsAlreadyExistException(id, "Customer");

            else
            {
                XElement newCustomer = new XElement("Customer", 
                                            new XElement("Id" , id.ToString()),
                                            new XElement("Name", name.ToString()),
                                            new XElement("PhoneNumber", phoneNumber.ToString()),
                                            new XElement("Longitude", longitude.ToString()),
                                            new XElement("Latitude", latitude.ToString()));
                cutomersRootElement.Add(newCustomer);
                XMLTools.SaveListToXMLElement(cutomersRootElement, CustomersPath);
            }   
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDal.UpdateCustomerName(int id, string newName)
        {
            XElement cutomersRootElement = XMLTools.LoadListFromXMLElement(CustomersPath);
            XElement customer = (from c in cutomersRootElement.Elements()
                                 where (int.Parse(c.Element("id").Value) == id)
                                 select c).FirstOrDefault();

            if (customer == null)
                throw new IdIsNotExistException(id, "Customer");

            customer.Element("Name").Value = newName;
            XMLTools.SaveListToXMLElement(cutomersRootElement, CustomersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDal.UpdateCustomrePhoneNumber(int id, string newPhoneNumber)
        {
            XElement cutomersRootElement = XMLTools.LoadListFromXMLElement(CustomersPath);
            XElement customer = (from c in cutomersRootElement.Elements()
                                 where (int.Parse(c.Element("id").Value) == id)
                                 select c).FirstOrDefault();

            if (customer == null)
                throw new IdIsNotExistException(id, "Customer");

            customer.Element("PhoneNumber").Value = newPhoneNumber;
            XMLTools.SaveListToXMLElement(cutomersRootElement, CustomersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        Customer IDal.GetCustomer(int id)
        {
            XElement cutomersRootElement = XMLTools.LoadListFromXMLElement(CustomersPath);
            XElement customer = (from c in cutomersRootElement.Elements()
                                 where (int.Parse(c.Element("Id").Value) == id)
                                 select c).FirstOrDefault();

            if (customer == null)
                throw new IdIsNotExistException(id, "Customer");

            return new Customer
            {
                Id = int.Parse(customer.Element("Id").Value),
                Name = customer.Element("Name").Value,
                Phone = customer.Element("Phone").Value,
                Longitude = double.Parse(customer.Element("Longitude").Value),
                Latitude = double.Parse(customer.Element("Latitude").Value),
            };
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        IEnumerable<Customer> IDal.GetCustomers(Func<Customer, bool> filter)
        {
            XElement cutomersRootElement = XMLTools.LoadListFromXMLElement(CustomersPath);
            if(filter == null)
            {
                return from cElement in cutomersRootElement.Elements()
                       let customer = new Customer
                       {
                           Id = int.Parse(cElement.Element("Id").Value),
                           Name = cElement.Element("Name").Value,
                           Phone = cElement.Element("Phone").Value,
                           Longitude = double.Parse(cElement.Element("Longitude").Value),
                           Latitude = double.Parse(cElement.Element("Latitude").Value),
                       }
                       select customer;
            }
            return from cElement in cutomersRootElement.Elements()
                                         let customer = new Customer
                                         {
                                             Id = int.Parse(cElement.Element("Id").Value),
                                             Name = cElement.Element("Name").Value,
                                             Phone = cElement.Element("Phone").Value,
                                             Longitude = double.Parse(cElement.Element("Longitude").Value),
                                             Latitude = double.Parse(cElement.Element("Latitude").Value),
                                         }
                                         where filter(customer)
                                         select customer;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        IEnumerable<Customer> IDal.ViewCustomersList()
        {
            throw new NotImplementedException();
        }

        IEnumerable<Parcel> IDal.ViewParcelsList()
        {
            throw new NotImplementedException();
        }

        string IDal.ViewCustomer(int id)
        {
            throw new NotImplementedException();
        }
    }

}
