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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Unclassified.UI
{
	public partial class IconButton : Button, ICollapsableToolbarItem
	{
		public static DependencyProperty IconSourceProperty = DependencyProperty.Register(
			"IconSource",
			typeof(ImageSource),
			typeof(IconButton));

		public static DependencyProperty ContentVisibilityProperty = DependencyProperty.Register(
			"ContentVisibility",
			typeof(Visibility),
			typeof(IconButton),
			new PropertyMetadata(Visibility.Collapsed));

		public static DependencyProperty OrientationProperty = DependencyProperty.Register(
			"Orientation",
			typeof(Orientation),
			typeof(IconButton),
			new PropertyMetadata(Orientation.Horizontal));

		public ImageSource IconSource
		{
			get { return (ImageSource) GetValue(IconSourceProperty); }
			set { SetValue(IconSourceProperty, value); }
		}

		public Visibility ContentVisibility
		{
			get { return (Visibility) GetValue(ContentVisibilityProperty); }
			set { SetValue(ContentVisibilityProperty, value); }
		}

		public Orientation Orientation
		{
			get { return (Orientation) GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public int CollapsePriority { get; set; }

		public IconButton()
		{
			InitializeComponent();
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);
			ContentVisibility = (newContent is string ? !string.IsNullOrEmpty((string) newContent) : newContent != null) ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}