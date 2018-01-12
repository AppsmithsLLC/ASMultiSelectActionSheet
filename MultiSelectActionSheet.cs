using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace ASMultiSelectActionSheet
{
    public class ASMultiSelectActionSheet : UIView
    {
        //Private Properties
        float ShadowRadius => 4.0f;
        float ShadowOpacity => 0.2f;
        float HostsCornerRadius => 3.0f;
        float Spacing => 5.0f;
        float ArrowBaseWidth => 20.0f;
        float ArrowHeight => 9.0f;
        float FixedWidth => 365.0f;
        float FixedWidthContinuous => 345.0f;

        UIScrollView _scrollView { get; set; }
        CustomActionSheetTriangle _arrowView { get; set; }
        CustomActionSheetView _scrollViewHost { get; set; }
        CGRect _finalContentFrame { get; set; }
        UIColor _realBGColor { get; set; }
        UIPopoverArrowDirection _anchoredArrowDirection { get; set; }

        bool _anchoredAtPoint { get; set; }
        CGPoint _anchorPoint { get; set; }

        bool _isiPad => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

        IASMultiSelectActionSheetDelegate _delegate { get; set; }

        UIView _targetView { get; set; }
        bool _isVisible { get { return _targetView != null; } }

        //Public Properties
        public static bool DisableCustomEasing { get; set; }
        public List<ASMultiSelectActionSheetSection> Sections { get; set; }

        public ASMultiSelectActionSheet(List<ASMultiSelectActionSheetSection> sectionsToAdd, IASMultiSelectActionSheetDelegate caller)
        {
            if (sectionsToAdd == null || !sectionsToAdd.Any())
            {
                Console.WriteLine("********** Must supply at least one section.");
                return;
            }
            Sections = sectionsToAdd;

            _delegate = caller;

            UITapGestureRecognizer tap = new UITapGestureRecognizer((gesture) =>
            {
                Tapped(gesture);
            });
            AddGestureRecognizer(tap);

            _scrollViewHost = new CustomActionSheetView();
            _scrollViewHost.BackgroundColor = UIColor.Clear;

            _scrollView = new UIScrollView();
            _scrollView.BackgroundColor = UIColor.Clear;
            _scrollView.ShowsHorizontalScrollIndicator = false;
            _scrollView.ShowsVerticalScrollIndicator = false;

            _scrollViewHost.AddSubview(_scrollView);
            AddSubview(_scrollViewHost);

            BackgroundColor = UIColor.FromWhiteAlpha(0.0f, 0.3f);
            _realBGColor = BackgroundColor;

            int index = 0;

            foreach (ASMultiSelectActionSheetSection section in Sections) 
            {
                section.Index = index;

                _scrollView.AddSubview(section);
                section.ButtonPressedAction = indexPath => ButtonPressed(indexPath);
                        
                index++;
            }
        }

        /// <summary>
        /// Shows the action sheet.
        /// </summary>
        /// <param name="view">The hosting view in which the action sheet should be shown.</param>
        /// <param name="animated">Whether the action sheet should show with an animation.</param>
        public void ShowInView(UIView view, bool animated)
        {
            if (_isVisible)
            {
                Console.WriteLine("********** Action Sheet is already visible.");
                return;
            }

            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
    
            _targetView = view;

            LayoutSheetInitial(true);
    
            if (_delegate != null)
            {
                _delegate.ActionSheetWillPresent(this);
            }

            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), OrientationChanged);

            LayoutForVisible(!animated);

            _targetView.AddSubview(this);
    
            if (!animated)
            {
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                if (_delegate != null)
                {
                    _delegate.ActionSheetDidPresent(this);
                }
            }
            else
            {
                float duration = _isiPad ? 0.3f : AnimationDurationForSectionCount(Sections.Count);
                Animate(duration,
                       () =>
                       {
                           LayoutForVisible(true);
                       },
                       () =>
                       {
                           UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                            if (_delegate != null)
                            {
                                _delegate.ActionSheetDidPresent(this);
                            }
                       });
            }
 
        }

        /// <summary>
        /// Shows the action sheet from a specific point.
        /// This method is only available on iPad devices.
        /// </summary>
        /// <param name="point">The point to show the action sheet from.An arrow will point towards this point.</param>
        /// <param name="view">The hosting view in which the action sheet should be shown.</param>
        /// <param name="arrowDirection">The direction in which the arrow should show and the side of the section at which the arrow should be placed.</param>
        /// <param name="animated">Whether the action sheet should show with an animation.</param>
        public void ShowFromPointInView(CGPoint point, UIView view, UIPopoverArrowDirection arrowDirection, bool animated)
        {
            if (_isVisible)
            {
                Console.WriteLine("********** Action Sheet is already visible.");
                return;
            }

            if (!_isiPad)
            {
                ShowInView(view, animated);
            }

            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();

            _targetView = view;

            MoveToPoint(point, arrowDirection, false);

            if (_delegate != null)
            {
                _delegate.ActionSheetWillPresent(this);
            }

            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), OrientationChanged);

            LayoutForVisible(!animated);

            _targetView.AddSubview(this);

            if (!animated)
            {
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                if (_delegate != null)
                {
                    _delegate.ActionSheetDidPresent(this);
                }
            }
            else
            {
                float duration = 0.3f;
                Animate(duration,
                       () =>
                       {
                           LayoutForVisible(true);
                       },
                       () =>
                       {
                           UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                           if (_delegate != null)
                           {
                               _delegate.ActionSheetDidPresent(this);
                           }
                       });
            }
        }

        /// <summary>
        /// Moves the action sheet to a new point.
        /// This method can only be called if the action sheet is already visible on screen.
        /// This method is only available on iPad devices.
        /// </summary>
        /// <param name="point">The point to show the action sheet from. An arrow will point towards this point.</param>
        /// <param name="arrowDirection">The direction in which the arrow should show and the side of the section at which the arrow should be placed.</param>
        /// <param name="animated">Whether the action sheet should change point with an animation. If you are invoking this method from the animated interface orientation change methods of UIViewController (@c willAnimateRotationToInterfaceOrientation:) pass @c NO because the UIViewController will itself do the animation.</param>
        public void MoveToPoint(CGPoint point, UIPopoverArrowDirection arrowDirection, bool animated)
        {
            if (!_isiPad)
            {
                return;
            }

            if (!_isVisible)
            {
                Console.WriteLine("********** Action Sheet must be visible in order to move the anchor point");
                return;
            }

            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();

            DisableCustomEasing = true;
        
            if (animated) {
                Animate(0.3,
                       () =>
                       {
                           Changes(point, arrowDirection);
                       },
                       () =>
                       {
                           UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                       });
            }
            else 
            {
                Changes(point, arrowDirection);
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
            }
    
            DisableCustomEasing = false;
        }

        /// <summary>
        /// Dismisses the action sheet.
        /// </summary>
        /// <param name="animated">Whether the action sheet should be dismissed with an animation.</param>
        public void DismissAnimated(bool animated)
        {

            if (!_isVisible)
            {
                Console.WriteLine("********** Action Sheet must be visible in order to dismiss.");
                return;
            }

            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
    
            if (_delegate != null)
            {
                _delegate.ActionSheetWillDismiss(this);
            }
    
            if (animated) {
                float duration = _isiPad ? 0.3f : AnimationDurationForSectionCount(Sections.Count);
                Animate(duration,
                        () =>
                        {
                            LayoutForVisible(false);
                        },
                        () => {
                            DismissCompletion();
                        });
            }
            else {
                LayoutForVisible(false);
                DismissCompletion();
            }
        }

        private void Tapped(UIGestureRecognizer gesture)
        {
            if (HitTest(gesture.LocationInView(this), null) == this)
            {
                DismissAnimated(true);
            }
        }

        public override bool GestureRecognizerShouldBegin(UIGestureRecognizer gesture)
        {
            if (HitTest(gesture.LocationInView(this), null) == this)
            {
                return true;
            }
            return false;
        }

        private void ButtonPressed(NSIndexPath indexPath)
        {
            if (_delegate != null)
            {
                ASMultiSelectActionSheetButton button = Sections.ElementAtOrDefault(indexPath.Section)?.Buttons.ElementAtOrDefault(indexPath.Row);
                if (button != null || (button.Style == ASMultiSelectActionSheetButtonStyle.PerformAction && button.Data == null)) 
                    _delegate.ActionSheetPressedButtonAtIndex(this, indexPath, button);
            }
        }

        private void OrientationChanged(NSNotification notification)
        {
            if (_targetView != null && !Equals(Bounds, _targetView.Bounds))
            {
                DisableCustomEasing = true;
                Animate((_isiPad ? 0.4 : 0.3), 0.0,
                        UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseInOut, 
                           () => { 
                                if (_anchoredAtPoint)
                                {
                                    MoveToPoint(_anchorPoint, _anchoredArrowDirection, false);
                                }
                                else
                                {
                                    LayoutSheetInitial(false);
                                }    
                            }, 
                           () => { 
                                DisableCustomEasing = false;
                            });
           }
        }    

        private void LayoutSheetInitial(bool initial)
        {
            Frame = _targetView.Bounds;

            _scrollViewHost.Layer.CornerRadius = 0.0f;
            _scrollViewHost.Layer.ShadowOpacity = 0.0f;
            _scrollViewHost.BackgroundColor = UIColor.Clear;
            CGRect frame = Frame;

            if (_isiPad)
            {
                float fixedWidth = FixedWidth;
                frame.X = (frame.Width - fixedWidth) / 2.0f;
                frame.Width = fixedWidth;
            }

            base.Draw(AlignmentRectInsets.InsetRect(frame));

            LayoutSheetForFrame(frame, !_isiPad, initial, false);
        }

        private void LayoutSheetForFrame(CGRect frame, bool fitToRect, bool initial, bool continuous)
        {
            if (continuous)
                frame.Width = FixedWidthContinuous;
            
            float spacing = 2.0f * Spacing;
            float width = (float)frame.Width > FixedWidthContinuous ? FixedWidthContinuous : (float)frame.Width;

            if (!continuous) 
                width -= 2.0f * spacing;
            
            float height = (continuous ? 0.0f : spacing);
               
            foreach (ASMultiSelectActionSheetSection section in Sections)
            {
                if (initial)
                    section.SetupForContinuous(continuous);

                CGRect f = section.LayoutForWidth(width);

                f.Y = height;
        
                if (!continuous)
                    f.X = spacing;

                section.Frame = f;

                height += (float)f.Height + spacing;
            }

            if (continuous)
                height -= spacing;
    
            _scrollView.ContentSize = new CGSize(frame.Width, height);
    
            if (!fitToRect && !continuous)
                frame.Height = _targetView.Bounds.Height - frame.GetMinY();
    
            if (height > frame.Height) 
            {
                _scrollViewHost.Frame = new CGRect(new CGPoint((frame.Width - FixedWidth) / 2.0f - spacing, frame.GetMinY() + Spacing), new CGSize(frame.Width, frame.Height));
            }
            else 
            {
                float finalY = 0.0f;
                float finalX = 0.0f;
                
                if (fitToRect) {
                    finalY = ((float)frame.GetMaxY() / 2.0f) - height / 2.0f;
                    finalX = (float)(frame.GetMaxX() / 2.0f) - (width / 2.0f) - spacing;
                }
                else if (continuous) {
                    finalY = (float)frame.GetMinY();
                    finalX = (float)frame.GetMinX();
                }
                else {
                    finalY = (float)frame.GetMinY() + (float)(frame.Height - height) / 2.0f;
                    finalX = (float)frame.GetMinX();
                }
                
                _scrollViewHost.Frame = new CGRect(new CGPoint(finalX, finalY), _scrollView.ContentSize);
            }
    
            _finalContentFrame = _scrollViewHost.Frame;
    
            _scrollView.Frame = _scrollViewHost.Bounds;

            _scrollView.ScrollRectToVisible(new CGRect(new CGPoint(0.0f, _scrollView.ContentSize.Height - 1.0f), new CGSize(1.0f, 1.0f)), false);
        }

        private void LayoutForVisible(bool visible)
        {
            UIView viewTomodify = _scrollViewHost;
            if (visible)
            {
                BackgroundColor = _realBGColor;
                if (_isiPad)
                {
                    viewTomodify.Alpha = 1.0f;
                    if (_arrowView != null) _arrowView.Alpha = 1.0f;
                }
                else
                {
                    viewTomodify.Frame = _finalContentFrame;
                }
            }
            else
            {
                base.BackgroundColor = UIColor.Clear;
                if (_isiPad)
                {
                    viewTomodify.Alpha = 0.0f;
                    if (_arrowView != null) _arrowView.Alpha = 0.0f;
                }
                else
                {
                    viewTomodify.Frame = new CGRect(new CGPoint(viewTomodify.Frame.X, _targetView.Bounds.Height), _scrollView.ContentSize);
                }
            }
        }

        private void AnchorSheetAtPoint(CGPoint point, UIPopoverArrowDirection arrowDirection, CGRect frame)
        {         
            _anchoredAtPoint = true;
            _anchorPoint = point;
            _anchoredArrowDirection = arrowDirection;
            CGRect finalFrame = _scrollViewHost.Frame;

            float arrowHeight = ArrowHeight;
            float arrrowBaseWidth = ArrowBaseWidth;

            bool leftOrRight = (arrowDirection == UIPopoverArrowDirection.Left || arrowDirection == UIPopoverArrowDirection.Right);
    
            CGRect arrowFrame = new CGRect(CGPoint.Empty, new CGSize((leftOrRight ? arrowHeight : arrrowBaseWidth), (leftOrRight ? arrrowBaseWidth : arrowHeight)));
    
            if (arrowDirection == UIPopoverArrowDirection.Right) {
                arrowFrame.X = point.X - arrowHeight;
                arrowFrame.Y = point.Y - arrrowBaseWidth/2.0f;        
                finalFrame.X = point.X - finalFrame.Width - arrowHeight;
            }
            else if (arrowDirection == UIPopoverArrowDirection.Left) {
                arrowFrame.X = point.X;
                arrowFrame.Y = point.Y - arrrowBaseWidth/2.0f;
        
                finalFrame.X = point.X + arrowHeight;
            }
            else if (arrowDirection == UIPopoverArrowDirection.Up) {
                arrowFrame.X = point.X - arrrowBaseWidth/2.0f;
                arrowFrame.Y = point.Y;
        
                finalFrame.Y = point.Y + arrowHeight;
            }
            else if (arrowDirection == UIPopoverArrowDirection.Down) {
                arrowFrame.X = point.X - arrrowBaseWidth/2.0f;
                arrowFrame.Y = point.Y - arrowHeight;
        
                finalFrame.Y = point.Y - finalFrame.Height - arrowHeight;
            }
    
            if (leftOrRight) {
                finalFrame.Y = NMath.Min(NMath.Max(frame.GetMaxY() - finalFrame.Height, arrowFrame.GetMaxY() - finalFrame.Height + HostsCornerRadius), 
                                         NMath.Min(NMath.Max(frame.GetMinY(), point.Y - finalFrame.Height / 2.0f), arrowFrame.GetMinY() - HostsCornerRadius));
            }
            else {
                finalFrame.X = NMath.Min(NMath.Max(NMath.Min(frame.GetMinX(), arrowFrame.GetMinX() - HostsCornerRadius), point.X - finalFrame.Width / 2.0f), 
                                         NMath.Max(frame.GetMaxX() - finalFrame.Width, arrowFrame.GetMaxX() + HostsCornerRadius - finalFrame.Width));
            }
    
            if (_arrowView == null) {
                _arrowView = new CustomActionSheetTriangle();
                AddSubview(_arrowView);
            }

            _arrowView.SetFrameWithArrowDirection(arrowFrame, arrowDirection);
    
            if (!_targetView.Bounds.Contains(finalFrame) || !_targetView.Bounds.Contains(arrowFrame))
            {
                Console.WriteLine("WARNING: Action sheet does not fit within view bounds! Select a different arrow direction or provide a different anchor point.");
            }
    
            _scrollViewHost.Frame = finalFrame;
        }

        private void Changes(CGPoint point, UIPopoverArrowDirection arrowDirection)
        {
            Frame = _targetView.Bounds;
            CGRect finalFrame = CGRect.Empty;
            float arrowHeight = ArrowHeight;
            float spacing = Spacing;

            if (arrowDirection == UIPopoverArrowDirection.Right)
            {
                finalFrame.Width = point.X - arrowHeight;
                finalFrame.Height = _targetView.Bounds.Height;
            }
            else if (arrowDirection == UIPopoverArrowDirection.Left)
            {
                finalFrame.Width = _targetView.Bounds.Width - point.X - arrowHeight;
                finalFrame.Height = _targetView.Bounds.Height;
                finalFrame.X = point.X + arrowHeight;
            }
            else if (arrowDirection == UIPopoverArrowDirection.Up)
            {
                finalFrame.Width = _targetView.Bounds.Width;
                finalFrame.Height = _targetView.Bounds.Height - point.Y - arrowHeight;
                finalFrame.Y = point.Y + arrowHeight;
            }
            else if (arrowDirection == UIPopoverArrowDirection.Down)
            {
                finalFrame.Width = _targetView.Bounds.Width;
                finalFrame.Height = point.Y - arrowHeight;
            }
            else
            {
                throw new ArgumentException("********** Invalid arrow direction.");
            }

            finalFrame.X += spacing;
            finalFrame.Y += spacing;
            finalFrame.Height -= spacing * 2.0f;
            finalFrame.Width -= spacing * 2.0f;

            base.Draw(AlignmentRectInsets.InsetRect(finalFrame));

            _scrollViewHost.BackgroundColor = UIColor.White;

            _scrollViewHost.Layer.CornerRadius = HostsCornerRadius;

            _scrollViewHost.Layer.ShadowColor = UIColor.Black.CGColor;
            _scrollViewHost.Layer.ShadowOffset = CGSize.Empty;
            _scrollViewHost.Layer.ShadowRadius = ShadowRadius;
            _scrollViewHost.Layer.ShadowOpacity = ShadowOpacity;

            LayoutSheetForFrame(finalFrame, false, true, true);
            AnchorSheetAtPoint(point, arrowDirection, finalFrame);
        }

        private void DismissCompletion()
        {
            if (_arrowView != null)_arrowView.RemoveFromSuperview();
            _arrowView = null;        
            _targetView = null;
            RemoveFromSuperview();
        
            _anchoredAtPoint = false;
            _anchoredArrowDirection = 0;
            _anchorPoint = CGPoint.Empty;
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);

            UIApplication.SharedApplication.EndIgnoringInteractionEvents();
        
            if (_delegate != null)
            {
                _delegate.ActionSheetDidDismiss(this);    
            }
        }

        private float AnimationDurationForSectionCount(int count)
        {
            return (float)NMath.Max(0.22f, NMath.Min(count * 0.12f, 0.45f));
        }
    }

    class CustomActionSheetLayer : CAShapeLayer
    {
        public void AddAnimationForKey(CAAnimation animation, string key)
        {
            if (!ASMultiSelectActionSheet.DisableCustomEasing && (animation is CABasicAnimation))
            {
                CAMediaTimingFunction func = new CAMediaTimingFunction(0.215f, 0.61f, 0.355f, 1.0f);
                animation.TimingFunction = func;
            }

            base.AddAnimation(animation, key);
        }
    }

    class CustomActionSheetTriangle : UIView
    {
        float ShadowRadius => 4.0f;
        float ShadowOpacity => 0.2f;
        //NOTE Why do we need this?
        //public static Type LayerType { get { return typeof(CustomActionSheetLayer); } }

        UIBezierPath TrianglePath(CGRect rect, UIPopoverArrowDirection arrowDirection, bool closePath)
        {
            UIBezierPath path = new UIBezierPath();
            if (arrowDirection == UIPopoverArrowDirection.Down)
            {
                path.MoveTo(CGPoint.Empty);
                path.AddLineTo(new CGPoint(rect.Width / 2.0f, rect.Height));
                path.AddLineTo(new CGPoint(rect.Width, 0.0f));
            }
            else if (arrowDirection == UIPopoverArrowDirection.Left)
            {
                path.MoveTo(new CGPoint(rect.Width, 0.0f));
                path.AddLineTo(new CGPoint(0.0f, rect.Height / 2.0f));
                path.AddLineTo(new CGPoint(rect.Width, rect.Height));
            }
            else if (arrowDirection == UIPopoverArrowDirection.Right)
            {
                path.MoveTo(CGPoint.Empty);
                path.AddLineTo(new CGPoint(rect.Width, rect.Height / 2.0f));
                path.AddLineTo(new CGPoint(0.0f, rect.Height));
            }
            else if (arrowDirection == UIPopoverArrowDirection.Up)
            {
                path.MoveTo(new CGPoint(0.0f, rect.Height));
                path.AddLineTo(new CGPoint(rect.Width / 2.0f, 0.0f));
                path.AddLineTo(new CGPoint(rect.Width, rect.Height));
            }

            if (closePath)
            {
                path.ClosePath();
            }

            return path;
        }

        public void SetFrameWithArrowDirection(CGRect frame, UIPopoverArrowDirection direction)
        {
            Frame = frame;
            Layer.ShadowPath = TrianglePath(frame, direction, true).CGPath;
            Layer.ShadowPath = TrianglePath(frame, direction, false).CGPath;

            bool leftOrRight = (direction == UIPopoverArrowDirection.Left || direction == UIPopoverArrowDirection.Right);
            CGRect pathRect = new CGRect(CGPoint.Empty, 
                                         new CGSize(frame.Width + (leftOrRight ? ShadowRadius + 1.0f : 2.0f * (ShadowRadius + 1.0f)), 
                                                    frame.Height + (leftOrRight ? 2.0f * (ShadowRadius + 1.0f) : ShadowRadius + 1.0f)));

            if (direction == UIPopoverArrowDirection.Up)
            {
                pathRect.Y -= ShadowRadius + 1.0f;
            }
            else if (direction == UIPopoverArrowDirection.Left)
            {
                pathRect.X -= ShadowRadius + 1.0f;
            }

            UIBezierPath path = UIBezierPath.FromRect(pathRect);

            CAShapeLayer mask = new CAShapeLayer();
            mask.Path = path.CGPath;
            mask.FillColor = UIColor.Black.CGColor;

            Layer.Mask = mask;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowOffset = CGSize.Empty;
            Layer.ShadowRadius = ShadowRadius;
            Layer.ShadowOpacity = ShadowOpacity;
            Layer.ContentsScale = UIScreen.MainScreen.Scale;
            Layer.BackgroundColor = UIColor.White.CGColor;
        }
    }

    class CustomActionSheetView : UIView
    {
        public static Type LayerClass { get { return typeof(CustomActionSheetView); } }
    }
}
