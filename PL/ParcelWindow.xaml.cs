﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BO;
using BlApi;

namespace PL
{
    /// <summary>
    /// Interaction logic for ParcelWindow.xaml
    /// </summary>
    public partial class ParcelWindow : Window
    {
        IBL BLObject = BlFactory.GetBl();
        Parcel Parcel;
        public ParcelWindow(int? parcelId = null)
        {
            InitializeComponent();
            if(parcelId == null) //add mode
            {
                
            }
            else // options mode
            {
                Parcel = BLObject.GetParcel((int)parcelId);
                ParcelGrid.DataContext = Parcel;
                WheightValueComboBox.ItemsSource = Enum.GetValues(typeof(WheightCategories));
                PriorityValueComboBox.ItemsSource = Enum.GetValues(typeof(Priorities));
            }
        }

        private void WheightValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PriorityValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
