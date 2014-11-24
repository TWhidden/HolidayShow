using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using HolidayShowEditor.Controls;
using Microsoft.Practices.Prism.Regions;

namespace HolidayShowEditor.Regions
{
    public class AnimatedContentControlRegionAdapter : RegionAdapterBase<AnimatedContentControl>
    {
        public AnimatedContentControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, AnimatedContentControl regionTarget)
        {
            if (regionTarget.Content != null || (BindingOperations.GetBinding(regionTarget, ContentControl.ContentProperty) != null))
                throw new InvalidOperationException("Invalid operation in AnimatedContentControlRegionAdapter");

            region.ActiveViews.CollectionChanged += delegate
            {
                regionTarget.Content = region.ActiveViews.FirstOrDefault();
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AnimatedSingleActiveRegion();
        }
    }
}
