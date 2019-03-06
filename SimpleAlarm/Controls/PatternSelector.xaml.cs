using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace SimpleAlarm.Controls
{
	/// <summary>
	/// PatternSelector.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class PatternSelector : ItemsControl
	{
		public static DependencyProperty ButtonCountProperty
			= DependencyProperty.Register("Buttons", typeof(IEnumerable<AlarmPattern>), typeof(PatternSelector),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, ButtonCountChanged));

		public static DependencyProperty SelectedIndexProperty
			= DependencyProperty.Register("SelectedIndex", typeof(int), typeof(PatternSelector),
				new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, SelectedIndexChanged));

		public ObservableCollection<ButtonEntry> ButtonsCollection = new ObservableCollection<ButtonEntry>();
		public event EventHandler SelectedChanged;

		public PatternSelector()
		{
			InitializeComponent();
		}

		public IEnumerable<AlarmPattern> Buttons
		{
			get => (IEnumerable<AlarmPattern>)GetValue(ButtonCountProperty);
			set => SetValue(ButtonCountProperty, value);
		}
		
		public int SelectedIndex
		{
			get => (int)GetValue(SelectedIndexProperty);
			set => SetValue(SelectedIndexProperty, value);
		}

		public static void ButtonCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PatternSelector control = d as PatternSelector;
			IEnumerable<AlarmPattern> list = e.NewValue as IEnumerable<AlarmPattern>;
			control.ButtonsCollection.Clear();

			int i = 1;
			foreach(AlarmPattern pattern in (IEnumerable<AlarmPattern>) e.NewValue)
				control.ButtonsCollection.Add(new ButtonEntry() { Index = i++, Parent = control });
			control.ItemsSource = control.ButtonsCollection;
		}

		public static void SelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PatternSelector control = d as PatternSelector;
			if ((int)e.OldValue > 0)
				control.ButtonsCollection[(int)e.OldValue - 1].OnSelectedChanged();
			if ((int)e.NewValue > 0)
				control.ButtonsCollection[(int)e.NewValue - 1].OnSelectedChanged();
			control.SelectedChanged?.Invoke(control, null);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ButtonEntry ent = ((Button)sender).Tag as ButtonEntry;
			SelectedIndex = ent.Index;
		}
	}

	public class ButtonEntry : INotifyPropertyChanged
	{
		public int Index { get; set; }
		public PatternSelector Parent { get; set; }
		public bool Selected
		{
			get => Parent.SelectedIndex == Index;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnSelectedChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
		}
	}
}
