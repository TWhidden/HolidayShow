using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using HolidayShowEditor.Controllers;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor
{
    public partial class ShellWindow : ModernWindow
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;


        public ShellWindow(IEventAggregator eventAggregator, IUnityContainer container)
        {
            _eventAggregator = eventAggregator;
            _container = container;
            InitializeComponent();

            
        }

    }
}
