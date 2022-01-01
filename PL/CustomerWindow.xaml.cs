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
using BlApi;
namespace PL
{
    /// <summary>
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        private IBL BLObject { get; set; }
        private int Id;
        private int Phone;
        private double Longitude;
        private double Lattitude;


        // options/add mode
        public CustomerWindow(BO.Customer customer = null)
        {
            InitializeComponent();
            BLObject = BlFactory.GetBl();
        }

        private void ButtonAddCustomer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void IdValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((int.TryParse(IdValueTextBox.Text, out Id) && Id >= 0) || IdValueTextBox.Text == "")
            {
                IdValueTextBox.Background = Brushes.White;
            }
            else
            {
                IdValueTextBox.Background = Brushes.Red;
            }
        }

        private void NameValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Name = NameValueTextBox.Text;
        }

        private void PTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((int.TryParse(PhoneValueTextBox.Text, out Phone) && Phone >= 0) || PhoneValueTextBox.Text == "" || PhoneValueTextBox.Text.Length == 10)
            {
                PhoneValueTextBox.BorderBrush = Brushes.Green;
            }
            else
            {
                PhoneValueTextBox.BorderBrush = Brushes.Red;
            }
        }

        private void LongitudeValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((double.TryParse(LongitudeValueTextBox.Text, out Longitude) && Longitude >= 0) && (double.TryParse(LongitudeValueTextBox.Text, out Longitude) && Longitude <= 180) || LongitudeValueTextBox.Text == "")
            {
                LongitudeValueTextBox.Background = Brushes.White;
            }
            else
            {
                LongitudeValueTextBox.Background = Brushes.Red;
            }
        }

        private void LattitudeValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((double.TryParse(LattitudeValueTextBox.Text, out Lattitude) && Lattitude >= 0) && (double.TryParse(LattitudeValueTextBox.Text, out Longitude) && Lattitude <= 180) || LattitudeValueTextBox.Text == "")
            {
                LattitudeValueTextBox.Background = Brushes.White;
            }
            else
            {
                LattitudeValueTextBox.Background = Brushes.Red;
            }
        }
    }
}
