using System.Linq;
using Microsoft.Practices.Prism.Regions;

namespace HolidayShowEditor.Regions
{
    public class AnimatedSingleActiveRegion : SingleActiveRegion
    {
        public override void Activate(object view)
        {
            object currentActiveView = ActiveViews.FirstOrDefault();

            base.Activate(view);

            if (currentActiveView != null && currentActiveView != view && this.Views.Contains(currentActiveView))
            {
                base.Deactivate(currentActiveView);
            }
        }
    }
}
