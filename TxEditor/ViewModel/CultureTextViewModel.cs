﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using TxLib;
using System.Windows.Media;
using Unclassified.UI;

namespace TxEditor.ViewModel
{
	class CultureTextViewModel : ViewModelBase, IViewCommandSource
	{
		private ViewCommandManager viewCommandManager = new ViewCommandManager();
		public ViewCommandManager ViewCommandManager { get { return viewCommandManager; } }

		public TextKeyViewModel TextKeyVM { get; private set; }

		private string cultureName;
		public string CultureName
		{
			get { return cultureName; }
		}

		private string cultureNativeName;
		public string CultureNativeName
		{
			get { return cultureNativeName; }
		}

		private string text;
		public string Text
		{
			get { return text; }
			set
			{
				if (value != text)
				{
					text = value;
					OnPropertyChanged("Text");
					TextKeyVM.Validate();
					TextKeyVM.MainWindowVM.FileModified = true;
				}
			}
		}

		public string CursorChar
		{
			get { return null; }
			set
			{
				TextKeyVM.MainWindowVM.CursorChar = value;
			}
		}

		private bool lastOfLanguage;
		public bool LastOfLanguage
		{
			get { return lastOfLanguage; }
			set
			{
				if (value != lastOfLanguage)
				{
					lastOfLanguage = value;
					OnPropertyChanged("LastOfLanguage");
					if (lastOfLanguage)
					{
						SeparatorBrush = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0));
					}
					else
					{
						SeparatorBrush = new SolidColorBrush(Color.FromArgb(0x18, 0, 0, 0));
					}
				}
			}
		}

		private Brush separatorBrush;
		public Brush SeparatorBrush
		{
			get { return separatorBrush; }
			set
			{
				if (value != separatorBrush)
				{
					separatorBrush = value;
					OnPropertyChanged("SeparatorBrush");
				}
			}
		}

		private Brush backgroundBrush;
		public Brush BackgroundBrush
		{
			get { return backgroundBrush; }
			set
			{
				if (value != backgroundBrush)
				{
					backgroundBrush = value;
					OnPropertyChanged("BackgroundBrush");
				}
			}
		}

		private ObservableCollection<QuantifiedTextViewModel> quantifiedTextVMs;
		public ObservableCollection<QuantifiedTextViewModel> QuantifiedTextVMs
		{
			get
			{
				if (quantifiedTextVMs == null)
				{
					quantifiedTextVMs = new ObservableCollection<QuantifiedTextViewModel>();
				}
				return quantifiedTextVMs;
			}
		}

		public CultureTextViewModel(string cultureName, TextKeyViewModel textKeyVM)
		{
			this.cultureName = cultureName;

			TextKeyVM = textKeyVM;
			LastOfLanguage = true;   // Change value once to set the brush value

			BackgroundBrush = cultureName == TextKeyVM.MainWindowVM.PrimaryCulture ?
				new SolidColorBrush(Color.FromArgb(20, 0, 192, 0)) :
				new SolidColorBrush(Color.FromArgb(20, 0, 192, 0));

			cultureNativeName = Tx.U(CultureInfo.GetCultureInfo(cultureName).NativeName);
		}

		#region Commands

		private DelegateCommand addCount0Command;
		public DelegateCommand AddCount0Command
		{
			get
			{
				if (addCount0Command == null)
				{
					addCount0Command = new DelegateCommand(OnAddCount0);
				}
				return addCount0Command;
			}
		}

		private DelegateCommand addCount1Command;
		public DelegateCommand AddCount1Command
		{
			get
			{
				if (addCount1Command == null)
				{
					addCount1Command = new DelegateCommand(OnAddCount1);
				}
				return addCount1Command;
			}
		}

		private DelegateCommand addCommand;
		public DelegateCommand AddCommand
		{
			get
			{
				if (addCommand == null)
				{
					addCommand = new DelegateCommand(OnAdd);
				}
				return addCommand;
			}
		}

		private DelegateCommand refreshCommand;
		public DelegateCommand RefreshCommand
		{
			get
			{
				if (refreshCommand == null)
				{
					refreshCommand = new DelegateCommand(OnRefresh);
				}
				return refreshCommand;
			}
		}

		#endregion Commands

		#region Command handlers

		private void OnAddCount0()
		{
			QuantifiedTextViewModel newVM = new QuantifiedTextViewModel(this);
			newVM.Count = 0;
			QuantifiedTextVMs.Add(newVM);
			TextKeyVM.MainWindowVM.FileModified = true;
			newVM.ViewCommandManager.InvokeLoaded("FocusText");
		}

		private void OnAddCount1()
		{
			QuantifiedTextViewModel newVM = new QuantifiedTextViewModel(this);
			newVM.Count = 1;
			QuantifiedTextVMs.Add(newVM);
			TextKeyVM.MainWindowVM.FileModified = true;
			newVM.ViewCommandManager.InvokeLoaded("FocusText");
		}

		private void OnAdd()
		{
			QuantifiedTextViewModel newVM = new QuantifiedTextViewModel(this);
			newVM.Count = -1;
			QuantifiedTextVMs.Add(newVM);
			TextKeyVM.MainWindowVM.FileModified = true;
			newVM.ViewCommandManager.InvokeLoaded("FocusCount");
		}

		private void OnRefresh()
		{
			var arr = QuantifiedTextVMs.ToArray();
			QuantifiedTextVMs.Clear();
			foreach (var item in arr)
			{
				QuantifiedTextVMs.InsertSorted(item, (a, b) => QuantifiedTextViewModel.Compare(a, b));
			}
		}

		#endregion Command handlers

		/// <summary>
		/// Compares this CultureTextViewModel instance with another instance to determine the sort
		/// order in the editor view.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(CultureTextViewModel other)
		{
			string otherName = other.CultureName;
			if (CultureName.Length >= 2 && otherName.Length >= 2)
			{
				// Prefer primary culture (with or without region; if set)
				if (!string.IsNullOrEmpty(TextKeyVM.MainWindowVM.PrimaryCulture))
				{
					bool thisPrimary = CultureName.StartsWith(TextKeyVM.MainWindowVM.PrimaryCulture);
					bool otherPrimary = otherName.StartsWith(TextKeyVM.MainWindowVM.PrimaryCulture);
					if (thisPrimary && !otherPrimary) return -1;
					if (!thisPrimary && otherPrimary) return 1;
				}
				
				if (string.Compare(CultureName.Substring(0, 2), otherName.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					// Same language, prefer shorter names (without region)
					if (CultureName.Length != otherName.Length)
					{
						return CultureName.Length - otherName.Length;
						// If this.length < other.length, then the result is negative and this comes before other
					}
				}
			}
			return string.Compare(CultureName, otherName, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}