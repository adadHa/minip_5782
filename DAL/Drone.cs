﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    namespace DO
    {
        public struct Drone
        {
            public int Id { get; set; }
            public string Model { get; set; }
            public WheightCategories MaxWeight { get; set; }
            public override string ToString()
            {
                return $"Id: {Id}, Model: {Model}, Max Weight: {MaxWeight}";
            }
        }
    }
}