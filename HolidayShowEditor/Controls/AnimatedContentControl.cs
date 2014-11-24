using System.Windows.Controls;
using Transitionals.Controls;

namespace HolidayShowEditor.Controls
{
    public class AnimatedContentControl : ContentControl
    {
        private const string EmptyPendingContent = "AnimatedContentControlEmptyState";
        private object _pendingContent = EmptyPendingContent;
        private TransitionElement _transitionBox;

        public AnimatedContentControl()
        {
            DefaultStyleKey = typeof (AnimatedContentControl);

            ApplyTemplate();
        }

        public override void OnApplyTemplate()
        {
            _transitionBox = (TransitionElement)GetTemplateChild("TransitionBox");

            base.OnApplyTemplate();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (_transitionBox == null) return;

            if (_transitionBox.IsTransitioning)
            {
                _pendingContent = newContent;
                return;
            }

            TransitionEventHandler[] @transitionEndedDelegateCopy = {null};

            TransitionEventHandler transitionEndedDelegate = delegate
            {
                _transitionBox.TransitionEnded -= @transitionEndedDelegateCopy[0];

                if (!(_pendingContent is string) || (string) _pendingContent != EmptyPendingContent)
                {
                    object pendingContent = _pendingContent;
                    _pendingContent = EmptyPendingContent;

                    OnContentChanged(newContent, pendingContent);
                }
            };

            @transitionEndedDelegateCopy[0] = transitionEndedDelegate;
            _transitionBox.TransitionEnded += transitionEndedDelegate;

            _transitionBox.Content = newContent;
        }
    }
}