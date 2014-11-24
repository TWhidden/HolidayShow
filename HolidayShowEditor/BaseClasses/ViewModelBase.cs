using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HolidayShowEditor.BaseClasses
{
    
    public abstract class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        private static bool? m_isInDesignMode;

        /// <summary>
        /// Helper to raise the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            var changed = PropertyChanged;
            if (changed != null) changed(this, e);
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!m_isInDesignMode.HasValue)
                {
                    m_isInDesignMode = DesignerProperties.GetIsInDesignMode(new Button())
                                       || (null == Application.Current)
                                       || Application.Current.GetType() == typeof(Application);
                }

                return m_isInDesignMode.Value;
            }
        }

        protected void SetTimeout(int milliseconds, Action func)
        {
            var timer = new DispatcherTimerContainingAction
            {
                Interval = new TimeSpan(0, 0, 0, 0, milliseconds),
                Action = func
            };
            timer.Tick += OnTimeout;
            timer.Start();
        }

        private static void OnTimeout(object sender, EventArgs arg)
        {
            var t = sender as DispatcherTimerContainingAction;
            t.Stop();
            t.Action();
            t.Tick -= OnTimeout;
        }

        private class DispatcherTimerContainingAction : DispatcherTimer
        {
            public Action Action { get; set; }
        }

      
        
    }
}
