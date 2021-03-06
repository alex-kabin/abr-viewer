﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace AbrViewer.Behaviors
{
    public class DoubleClickCommandBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
                DependencyProperty.Register(
                    "Command",
                    typeof(ICommand),
                    typeof(DoubleClickCommandBehavior),
                    new FrameworkPropertyMetadata(default(ICommand))
                );

        public ICommand Command {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
                DependencyProperty.Register(
                    "CommandParameter",
                    typeof(object),
                    typeof(DoubleClickCommandBehavior),
                    new FrameworkPropertyMetadata(default)
                );

        public object CommandParameter {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttached() {
            AssociatedObject.MouseLeftButtonDown +=  AssociatedObject_MouseLeftButtonDown;
        }

        protected override void OnDetaching() {
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.ClickCount != 2)
                return;

            if (Command != null && Command.CanExecute(CommandParameter)) {
                Command.Execute(CommandParameter);
            }
        }
    }
}