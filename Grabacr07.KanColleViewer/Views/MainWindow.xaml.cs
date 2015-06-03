﻿using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleViewer.Win32;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Settings2 = Grabacr07.KanColleViewer.Models.Settings;

namespace Grabacr07.KanColleViewer.Views
{
	/// <summary>
	/// KanColleViewer のメイン ウィンドウを表します。
	/// </summary>
	partial class MainWindow
	{
		public static MainWindow Current { get; private set; }
		public LeftTop rect { get; private set; }
		public class LeftTop
		{
			public int left { get; set; }
			public int top { get; set; }
			public int width { get; set; }
		}

		public MainWindow()
		{
			InitializeComponent();
			Current = this;
		}
		public void SetPos()
		{
			rect = new LeftTop();
			rect.left = Convert.ToInt32(this.Left);
			rect.top = Convert.ToInt32(this.Top);
			rect.width = Convert.ToInt32(this.ActualWidth);
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			// ToDo: 確認ダイアログを実装したかった…
			//e.Cancel = true;

			//var dialog = new ExitDialog { Owner = this, };
			//dialog.Show();

			SaveWindowSize();
			base.OnClosing(e);
		}
		protected override void OnStateChanged(EventArgs e)
		{
			SaveWindowSize();
			base.OnStateChanged(e);
		}
		public void RefreshNavigator()
		{
			App.ViewModelRoot.Navigator.ReNavigate();
		}
		private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
		}
		private void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			this.Cursor = Cursors.Arrow;
		}

		private void WebBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			this.Cursor = Cursors.AppStarting;
		}
		private void SaveWindowSize()
		{
			if (Settings2.Current.Orientation == OrientationType.Horizontal)
			{
				Settings2.Current.HorizontalSize = new System.Windows.Point(ActualWidth, ActualHeight);
			}
			else
			{
				Settings2.Current.VerticalSize = new System.Windows.Point(ActualWidth, ActualHeight);
			}
		}
	}
}
