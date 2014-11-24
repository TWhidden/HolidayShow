using HolidayShowEditor.ViewModels;
using HolidayShowEditor.Views;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor.BaseClasses
{
    public abstract class ViewAttachedViewModelBase<T> : ViewModelBase, IViewModel where T : IView
    {
        private T m_internalView;
        private string m_visualState;

        [Dependency]
        virtual public T InnerView
        {
            get
            {
                return m_internalView;
            }
            set
            {
                m_internalView = value;
                m_internalView.SetViewModel(this);
                View = value;
            }
        }

        [Dependency]
        public IUnityContainer GlobalContainer { get; set; }

        public string VisualState
        {
            get { return m_visualState; }
            set
            {
                m_visualState = value;
                base.OnPropertyChanged(() => VisualState);
            }
        }

        public virtual object View { get; protected set; }


       
    }
}
