using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;

namespace HolidayShowEditor.Regions
{
    public class PanelHostRegionAdapter : RegionAdapterBase<Panel>
    {
        public PanelHostRegionAdapter(IRegionBehaviorFactory behaviorFactory)
            : base(behaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, Panel regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (FrameworkElement element in e.NewItems)
                    {
                        regionTarget.Children.Add(element);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (FrameworkElement currentElement in e.OldItems)
                        regionTarget.Children.Remove(currentElement);
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    } 

}
