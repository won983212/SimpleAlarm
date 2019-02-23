using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleAlarm.Controls
{
	class TimeCounter : Control
	{
		public static DependencyProperty TimeProperty
			= DependencyProperty.Register("Time", typeof(DateTime), typeof(TimeCounter), 
				new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, TimeChanged));
		public static DependencyProperty ShowSecondsProperty
			= DependencyProperty.Register("ShowSeconds", typeof(bool), typeof(TimeCounter));

		private FormattedText text = null;

		public DateTime Time
		{
			get => (DateTime)GetValue(TimeProperty);
			set => SetValue(TimeProperty, value);
		}

		public bool ShowSeconds
		{
			get => (bool)GetValue(ShowSecondsProperty);
			set => SetValue(ShowSecondsProperty, value);
		}

		public static void TimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeCounter obj = d as TimeCounter;
			obj.CreateFormattedText();
		}

		private void CreateFormattedText()
		{
			Typeface type = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
			text = new FormattedText(ShowSeconds ? Time.ToString("hh:mm:ss") : Time.ToString("hh:mm"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, type, FontSize, Foreground);
		}

		protected override Size MeasureOverride(Size constraint)
		{
			return new Size(text.MinWidth, text.Height);
		}

		protected override void OnRender(DrawingContext dc)
		{
			dc.DrawText(text, new Point());
		}
	}
}
