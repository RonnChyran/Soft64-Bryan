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
using Noesis.Javascript;

namespace Soft64WPF.Windows
{
    /// <summary>
    /// Interaction logic for JSWindow.xaml
    /// </summary>
    public partial class JSWindow : Window
    {
        public JSWindow()
        {
            InitializeComponent();

            JavascriptContext context = new JavascriptContext();
        }
    }
}
