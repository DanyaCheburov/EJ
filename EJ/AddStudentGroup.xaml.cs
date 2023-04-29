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

namespace EJ
{
    /// <summary>
    /// Логика взаимодействия для AddStudentGroup.xaml
    /// </summary>
    public partial class AddStudentGroup : Window
    {
        public AddStudentGroup()
        {
            InitializeComponent();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.ToList();
            ComboUsers.ItemsSource = BDEntities.GetContext().Users.ToList();
        }

        private void Add_student_Click(object sender, RoutedEventArgs e)
        {

        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}