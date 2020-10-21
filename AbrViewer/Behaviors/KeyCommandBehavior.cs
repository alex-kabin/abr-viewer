using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace AbrViewer.Behaviors
{
    public class KeyCommandBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
                DependencyProperty.Register(
                    "Command",
                    typeof(ICommand),
                    typeof(KeyCommandBehavior),
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
                    typeof(KeyCommandBehavior),
                    new FrameworkPropertyMetadata(default)
                );

        public object CommandParameter {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }


        public static readonly DependencyProperty KeyProperty =
                DependencyProperty.Register(
                    "Key",
                    typeof(Key),
                    typeof(KeyCommandBehavior),
                    new PropertyMetadata(default(Key))
                );

        public Key Key {
            get => (Key)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        protected override void OnAttached() {
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
        }

        protected override void OnDetaching() {
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
        }

        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e) {
            if(e.Key != Key)
                return;

            if (Command != null && Command.CanExecute(CommandParameter)) {
                Command.Execute(CommandParameter);
            }
        }
    }
}
