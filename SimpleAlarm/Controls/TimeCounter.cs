using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SimpleAlarm.Controls
{
	class TimeCounter : Control
	{
		public static DependencyProperty TimeProperty
			= DependencyProperty.Register("Time", typeof(DateTime), typeof(TimeCounter),
				new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.AffectsRender, OnTimeChanged));
		public static DependencyProperty ShowSecondsProperty
			= DependencyProperty.Register("ShowSeconds", typeof(bool), typeof(TimeCounter),
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static DependencyProperty InverseAnimationProperty
			= DependencyProperty.Register("InverseAnimation", typeof(bool), typeof(TimeCounter),
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

		private FormattedText[] text;
		private FormattedText[] animatedText;
		private double[] animatedOffset;
		private Task[] animationTask;
		private FormattedText spliter;
		private Size textSize;
		private DrawingGroup group = new DrawingGroup();

		public TimeCounter()
		{
			ClipToBounds = true;
		}

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

		public bool InverseAnimation
		{
			get => (bool)GetValue(InverseAnimationProperty);
			set => SetValue(InverseAnimationProperty, value);
		}

		public string Label
		{
			get => ShowSeconds ? Time.ToString("hh:mm:ss") : Time.ToString("hh:mm");
		}

		private FormattedText CreateFormattedText(string text)
		{
			Typeface type = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
			return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, type, FontSize, Foreground);
		}

		public void Animate(int pos, char to)
		{
			if (animatedText == null)
				return;

			if (animationTask[pos] != null)
			{
				Task.Run(() =>
				{
					animationTask[pos].Wait();
					Dispatcher.Invoke(() => Animate(pos, to));
				});
				return;
			}

			string dateText = Label;
			animatedText[pos] = CreateFormattedText(to.ToString());
			animatedOffset[pos] = 0;

			animationTask[pos] = Task.Run(() =>
			{
				Stopwatch watch = new Stopwatch();
				double max = textSize.Height;
				const int frames = 50;

				for (int f = 0; f <= frames; f++)
				{
					double k = (f - frames) / (double)frames;
					double y = max * (1 - k * k);
					Dispatcher.Invoke(() => 
					{
						animatedOffset[pos] = InverseAnimation ? -y : y;
						UpdateDrawing();
					});
					Thread.Sleep(300 / frames);
				}

				Dispatcher.Invoke(() =>
				{
					animationTask[pos] = null;
					text[pos] = CreateFormattedText(to.ToString());
					animatedText[pos] = null;
					UpdateDrawing();
				});
			});
		}

		private void UpdateDrawing()
		{
			using (DrawingContext dc = group.Open())
			{
				double offset = 0;
				for (int i = 0, j = 0; i < text.Length + (ShowSeconds ? 2 : 1); i++)
				{
					if (i == 2 || i == 5)
					{
						dc.DrawText(spliter, new Point(offset, 0));
						offset += spliter.Width;
					}
					else
					{
						if (animatedText[j] != null)
						{
							double initialY = animatedOffset[j] > 0 ? (-textSize.Height) : textSize.Height;
							dc.DrawText(text[j], new Point(offset, animatedOffset[j]));
							dc.DrawText(animatedText[j], new Point(offset, initialY + animatedOffset[j]));
						}
						else
						{
							dc.DrawText(text[j], new Point(offset, 0));
						}
						offset += textSize.Width;
						j++;
					}
				}
			}
		}

		private void InitializeTimeText()
		{
			// Calculate size & Create spliter
			FormattedText model = CreateFormattedText("0");
			textSize = new Size(model.Width, model.Height);
			spliter = CreateFormattedText(":");

			// Create label characters
			int textLength = ShowSeconds ? 6 : 4;
			text = new FormattedText[textLength];
			animatedText = new FormattedText[textLength];
			animatedOffset = new double[textLength];
			animationTask = new Task[textLength];
			for (int i = 0, j = 0; i < Label.Length; i++)
			{
				if (i == 2 || i == 5) continue;
				text[j++] = CreateFormattedText(Label[i].ToString());
			}
		}

		private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeCounter counter = d as TimeCounter;
			string timeNew = ((DateTime) e.NewValue).ToString("hhmmss");
			string timeOld = ((DateTime) e.OldValue).ToString("hhmmss");
			
			int count = counter.ShowSeconds ? 6 : 4;
			for (int i = 0; i < count; i++)
			{
				if (timeOld[i] != timeNew[i])
					counter.Animate(i, timeNew[i]);
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			return new Size(textSize.Width * text.Length + spliter.Width * (ShowSeconds ? 2 : 1), textSize.Height);
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			UpdateDrawing();
			dc.DrawDrawing(group);
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			InitializeTimeText();
		}
	}
}
