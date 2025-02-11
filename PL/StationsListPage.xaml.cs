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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BO;
using BlApi;
namespace PL
{
    /// <summary>
    /// Interaction logic for StationsListPage.xaml
    /// </summary>
    public partial class StationsListPage : Page
    {
        IBL blObject = BlFactory.GetBl();
        public StationsListPage()
        {
            InitializeComponent();
            StationsListView.DataContext = blObject.GetStations();
        }

        private void AddStationButton_Click(object sender, RoutedEventArgs e)
        {
            StationWindow stationWindow = new StationWindow();
            stationWindow.Closed += StationWindow_Closed;
            stationWindow.Show();
        }
        private void StationWindow_Closed(object sender, EventArgs e)
        {
            StationsListView.DataContext = blObject.GetStations();
        }
        private void StationsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StationForList station = (StationForList)((ListView)sender).SelectedItem;
            if (station != null)
            {
               new StationWindow(station).Show();
            }
        }

        private void orderByFreeSlots_Checked(object sender, RoutedEventArgs e)
        {
            var currenList = (IEnumerable<StationForList>)StationsListView.DataContext;
            var resultList = from station in currenList
                             orderby station.AvailableChargingSlots descending
                             select station;
            StationsListView.DataContext = resultList;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StationsListView.DataContext = blObject.GetStations();
            orderByFreeSlots.IsChecked = false;
        }
    }
}
// todo: aoutomatic update list when a new customer is being added