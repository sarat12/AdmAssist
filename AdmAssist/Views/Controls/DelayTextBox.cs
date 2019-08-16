using System;
using System.Windows.Controls;
using System.Timers;
using System.Windows;

namespace AdmAssist.Views.Controls
{
    public class DelayTextBox : TextBox
    {
        #region private globals

        private readonly Timer _delayTimer; // used for the delay
        private bool _timerElapsed; // if true OnTextChanged is fired.
        private bool _keysPressed; // makes event fire immediately if it wasn't a keypress
        private readonly int _delayTime = 250;//for now best empiric value

        public static readonly DependencyProperty DelayTimeProperty =
            DependencyProperty.Register("DelayTime", typeof(int), typeof(DelayTextBox));

        #endregion

        #region ctor

        public DelayTextBox()
        {
            // Initialize Timer
            _delayTimer = new Timer(_delayTime);
            _delayTimer.Elapsed += DelayTimer_Elapsed;

            _previousTextChangedEventArgs = null;

            AddHandler(PreviewKeyDownEvent, new System.Windows.Input.KeyEventHandler(DelayTextBox_PreviewKeyDown));

            PreviousTextValue = String.Empty;
        }

        void DelayTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!_delayTimer.Enabled)
                _delayTimer.Enabled = true;
            else
            {
                _delayTimer.Enabled = false;
                _delayTimer.Enabled = true;
            }

            _keysPressed = true;
        }

        #endregion

        #region event handlers

        void DelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _delayTimer.Enabled = false;// stop timer.

            _timerElapsed = true;// set timer elapsed to true, so the OnTextChange knows to fire

            Dispatcher.Invoke(new DelayOverHandler(DelayOver), null);// use invoke to get back on the UI thread.
        }

        #endregion

        #region overrides

        private TextChangedEventArgs _previousTextChangedEventArgs;
        public string PreviousTextValue { get; private set; }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            // if the timer elapsed or text was changed by something besides a keystroke
            // fire base.OnTextChanged
            if (_timerElapsed || !_keysPressed)
            {
                _timerElapsed = false;
                _keysPressed = false;
                base.OnTextChanged(e);

                System.Windows.Data.BindingExpression be = GetBindingExpression(TextProperty);
                if (be != null && be.Status == System.Windows.Data.BindingStatus.Active) be.UpdateSource();

                PreviousTextValue = Text;
            }

            _previousTextChangedEventArgs = e;
        }

        #endregion

        #region delegates

        public delegate void DelayOverHandler();

        #endregion

        #region private helpers

        private void DelayOver()
        {
            if (_previousTextChangedEventArgs != null)
            {
                OnTextChanged(_previousTextChangedEventArgs);
                GetBindingExpression(TextProperty)?.UpdateSource();
            }
        }

        #endregion
    }
}
