using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace AbrViewer
{
	public class DoubleClickCommandBehavior : Behavior<FrameworkElement>
	{
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register("Command", typeof(ICommand), typeof(DoubleClickCommandBehavior), new PropertyMetadata(default(ICommand)));

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.Register("CommandParameter", typeof(object), typeof(DoubleClickCommandBehavior), new PropertyMetadata(default(object)));

		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		protected override void OnAttached()
		{
			AssociatedObject.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(AssociatedObject_MouseLeftButtonDown);
		}

		void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if(e.ClickCount != 2)
				return;

			if(Command != null)
				if(Command.CanExecute(CommandParameter))
					Command.Execute(CommandParameter);
		}

		protected override void OnDetaching()
		{
			AssociatedObject.MouseLeftButtonDown -=AssociatedObject_MouseLeftButtonDown;
		}
	}
}