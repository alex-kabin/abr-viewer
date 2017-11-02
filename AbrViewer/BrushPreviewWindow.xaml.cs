﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AbrViewer
{
	/// <summary>
	/// Interaction logic for BrushPreviewWindow.xaml
	/// </summary>
	public partial class BrushPreviewWindow : Window
	{
		public BrushPreviewWindow()
		{
			InitializeComponent();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Escape)
				Close();
		}
	}
}
