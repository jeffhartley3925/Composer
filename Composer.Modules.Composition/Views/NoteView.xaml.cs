using System.Windows;
using Composer.Modules.Composition.ViewModels;
using System.ComponentModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.Views
{
	using System;
	using System.Windows.Controls;

	using Composer.Infrastructure;
	using Composer.Repository.DataService;

	public partial class NoteView : INoteView
	{
		private static IEventAggregator _ea;

		private NoteViewModel noteViewModel = null;

		public Grid _dispositionContainer = null;
		public Grid DispositionContainer
		{
			get
			{
				if (_dispositionContainer == null)
				{
					_dispositionContainer = (Grid)LayoutRoot.FindName("Disposition");
				}
				return _dispositionContainer;
			}
		}

		public Guid NtId;

		public string NoteId
		{
			get
			{
				return (string)GetValue(NoteIdProperty);
			}
			set
			{
				NtId = Guid.Parse(value);
				SetValue(NoteIdProperty, value);
				OnPropertyChanged("NoteId");
			}
		}

		public NoteView()
		{
			InitializeComponent();
			_ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
			this.SubscribeEvents();
		}

		public static readonly DependencyProperty NoteIdProperty =
			DependencyProperty.Register("NoteId", typeof(string), typeof(NoteView), new PropertyMetadata("", null));

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler ph = PropertyChanged;

			if (ph != null)
				ph(this, new PropertyChangedEventArgs(name));
		}

		NoteViewModel _viewModel;

		public void SubscribeEvents()
		{
			_ea.GetEvent<ShowDispositionButtons>().Subscribe(OnShowDispositionButtons);
			_ea.GetEvent<HideDispositionButtons>().Subscribe(OnHideDispositionButtons);
		}

		public void OnShowDispositionButtons(Tuple<Guid, string> payload)
		{
			try
			{
				var nTiD = payload.Item1;
				if (nTiD == Guid.Empty) return;
				var nT = Utils.GetNote(nTiD);
				if (nT.Id != NtId) return;
				var isPendingAction = IsPendingAction(nT);
				if (isPendingAction)
				{
					var view = new NoteDispositionView(nT.Id) { DataContext = new NoteDispositionViewModel(nTiD.ToString()) };
					if (DispositionContainer.Children.Count == 0)
					{
						DispositionContainer.Children.Add(view);
					}
				}
				else
				{
					OnHideDispositionButtons(string.Empty);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private static bool IsPendingAction(Note nT)
		{
			var result = false;
			if (CollaborationManager.IsPendingDelete(Collaborations.GetStatus(nT)))
			{
				if (Collaborations.CurrentCollaborator != null)
				{
					if (nT.Audit.CollaboratorIndex == -1 || nT.Audit.CollaboratorIndex == Collaborations.CurrentCollaborator.Index)
					{
						result = true;
					}
				}
			}
			else
			{
				if (nT.Status == "2,8")
				{
					
				}
				if ((CollaborationManager.IsPendingAdd(Collaborations.GetStatus(nT))))
				{
					result = true;
				}
			}
			return result;
		}

		public void OnHideDispositionButtons(object payload)
		{
			DispositionContainer.Children.Clear();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (!DesignerProperties.IsInDesignTool)
			{
				_viewModel = new NoteViewModel(NoteId);
				NtId = _viewModel.Note.Id;
				_ViewModels.notes.Add(_viewModel);
				DataContext = _viewModel;
			}
		}
	}
}