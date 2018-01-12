using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace ASMultiSelectActionSheet
{
    public class ASMultiSelectActionSheetSection : UIView
    {
        float ShadowRadius => 4.0f;
        float ShadowOpacity => 0.2f;
        float HostsCornerRadius => 10.0f;
        float Spacing => 1.0f;
        float FixedWidth => 320.0f;
        bool iOS7 => UIDevice.CurrentDevice.CheckSystemVersion(7, 0);
        List<UIView> horizontalLines { get; set; }
        UIView controlLine { get; set; }

        public UILabel TitleLabel { get; set; }
        public UILabel MessageLabel { get; set; }
        public List<ASMultiSelectActionSheetButton> Buttons { get; set; }
        public UIView ContentView { get; set; }
        public int Index { get; set; }

        public Action<NSIndexPath> ButtonPressedAction { get; set; }
        public ASMultiSelectActionSheetButton ActionButton { get; set; }

        /// <summary>
        /// Creates an action sheet section.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="message">Subtitle.</param>
        /// <param name="buttonDefs">Button titles and an optional object.</param>
        /// <param name="style">Button style; this controls appearance and behavior in some cases.</param>
        /// <param name="actionButtonTitle">If defined, the section will also include a stacked action button.</param>
        /// <param name="actionObject">Optional: A custom action or command to bind to the action button.</param>
        public ASMultiSelectActionSheetSection(string title, string message, List<Tuple<string, object>> buttonDefs, ASMultiSelectActionSheetButtonStyle style, string actionButtonTitle, object actionObject = null)
        {
            if (!String.IsNullOrEmpty(title))
            {
                UILabel titleLabel = new UILabel();
                titleLabel.BackgroundColor = UIColor.Clear;
                titleLabel.TextAlignment = UITextAlignment.Center;
                titleLabel.Font = UIFont.BoldSystemFontOfSize(25.0f);
                titleLabel.TextColor = UIColor.Black;
                titleLabel.Lines = 1;
                titleLabel.Text = title;
                TitleLabel = titleLabel;
                AddSubview(TitleLabel);
            }

            if (!String.IsNullOrEmpty(message))
            {
                UILabel messageLabel = new UILabel();
                messageLabel.BackgroundColor = UIColor.Clear;
                messageLabel.TextAlignment = UITextAlignment.Center;
                messageLabel.Font = UIFont.SystemFontOfSize(15.0f);
                messageLabel.TextColor = UIColor.Black;
                messageLabel.Lines = 0;
                messageLabel.Text = message;
                MessageLabel = messageLabel;
                AddSubview(MessageLabel);
            }

            horizontalLines = new List<UIView>();
            UIView titleLine = new UIView();
            titleLine.BackgroundColor = UIColor.FromRGB(0.63f, 0.63f, 0.63f);
            horizontalLines.Add(titleLine);
            AddSubview(titleLine);

            List<ASMultiSelectActionSheetButton> buttons = new List<ASMultiSelectActionSheetButton>();
            int index = 0;

            if (buttonDefs.Any())
            {
                foreach (var d in buttonDefs)
                {
                    ASMultiSelectActionSheetButton b = MakeButtonWithTitle(d.Item1, style, index++);
                    b.Data = d.Item2;
                    buttons.Add(b);

                    AddSubview(b);
                }
            }

            ASMultiSelectActionSheetButton cancel = MakeButtonWithTitle("Cancel", ASMultiSelectActionSheetButtonStyle.Cancel, index++);
            buttons.Add(cancel);
            AddSubview(cancel);

            controlLine = new UIView();
            controlLine.BackgroundColor = UIColor.FromRGB(0.63f, 0.63f, 0.63f);
            AddSubview(controlLine);

            ASMultiSelectActionSheetButton action = MakeButtonWithTitle(actionButtonTitle, ASMultiSelectActionSheetButtonStyle.PerformAction, index++);
            action.Data = actionObject;
            action.Enabled = false;
            buttons.Add(action);
            AddSubview(action);
            ActionButton = action;

            Buttons = buttons;
        }

        public void SetButtonStyleAtIndex(ASMultiSelectActionSheetButtonStyle style, int index)
        {
            if (index < Buttons.Count)
            {
                ASMultiSelectActionSheetButton button = Buttons.ElementAt(index);
                button.SetButtonStyleForButton(style);
            }
            else
            {
                Console.WriteLine("ERROR: Index out of bounds.");
                return;
            }
        }

        public void SetupForContinuous(bool continuous)
        {
            if (continuous)
            {
                BackgroundColor = UIColor.Clear;
                Layer.CornerRadius = 0.0f;
                Layer.ShadowOpacity = 0.0f;
            }
            else
            {
                BackgroundColor = UIColor.White;
                Layer.CornerRadius = HostsCornerRadius;
                Layer.ShadowColor = UIColor.Black.CGColor;
                Layer.ShadowOffset = CGSize.Empty;
                Layer.ShadowRadius = ShadowRadius;
                Layer.ShadowOpacity = ShadowOpacity;
            }
        }

        private ASMultiSelectActionSheetButton MakeButtonWithTitle(string title, ASMultiSelectActionSheetButtonStyle style, int row)
        {
            ASMultiSelectActionSheetButton b = new ASMultiSelectActionSheetButton();
            b.Layer.MasksToBounds = true;
            b.Layer.BorderWidth = 0.0f;
            b.SetTitle(title, UIControlState.Normal);
            b.TouchUpInside += ButtonPressed;
            b.Style = style;
            b.Row = row;
            b.SetButtonStyleForButton(style);

            return b;
        }

        public void ButtonPressed(object sender, EventArgs e)
        {
            if (((ASMultiSelectActionSheetButton)sender).Style == ASMultiSelectActionSheetButtonStyle.MultiSelect)
            {
                ((ASMultiSelectActionSheetButton)sender).Selected = !((ASMultiSelectActionSheetButton)sender).Selected;
                if (((ASMultiSelectActionSheetButton)sender).Selected || Buttons.Any(b => b.Selected))
                    ActionButton.Enabled = true;
                else
                    ActionButton.Enabled = false;
                
                return;
            }

            if (ButtonPressedAction != null)
            {
                var indexPath = NSIndexPath.FromRowSection(((ASMultiSelectActionSheetButton)sender).Row, Index);
                ButtonPressedAction.Invoke(indexPath);
            }
        }

        public CGRect LayoutForWidth(float width)
        {
            float buttonHeight = 50.0f;
            float spacing = Spacing;
            float height = 0.0f;

            if (TitleLabel != null)
            {
                height += spacing;
                TitleLabel.SizeToFit();
                height += (float)TitleLabel.Frame.Height * 1.5f;
                TitleLabel.Frame = new CGRect(new CGPoint(spacing, spacing), new CGSize(width - spacing * 2.0f, TitleLabel.Frame.Height * 1.5f));
            }

            if (MessageLabel != null)
            {
                height += spacing;

                CGSize maxLabelSize = new CGSize(width - spacing * 2.0f, width);
                float messageLabelHeight = 0.0f;

                if (iOS7)
                {
                    var attributes = MessageLabel.Font;
                    messageLabelHeight = (float)MessageLabel.AttributedText.GetBoundingRect(maxLabelSize, NSStringDrawingOptions.UsesLineFragmentOrigin, null).Height; 
                }
                else
                {
                    messageLabelHeight = (float)UIStringDrawing.StringSize(MessageLabel.Text, MessageLabel.Font, maxLabelSize).Height;
                }

                MessageLabel.Frame = new CGRect(new CGPoint(spacing, height), new CGSize(width - spacing * 2.0f, messageLabelHeight));
                height += messageLabelHeight;            
            }

            int lineIndex = 0;

            //Add the title dividing line
            UIView line = horizontalLines.ElementAt(lineIndex);
            line.Frame = new CGRect(new CGPoint(0.0f, height + 1), new CGSize(width, 1.0f));
            height++;
            lineIndex++;

            ASMultiSelectActionSheetButton cancel = null;
            ASMultiSelectActionSheetButton action = null;

            foreach (ASMultiSelectActionSheetButton button in Buttons) 
            {
                height += spacing;
                if (button.Style == ASMultiSelectActionSheetButtonStyle.Cancel)
                {
                    float x = 1.0f;
                    float y = height;
                    if (action != null)
                    {
                        y = (float)action.Frame.Y;
                    }
                    button.Frame = new CGRect(new CGPoint(x, y), new CGSize(((width - spacing * 2.0f) / 2) - 2, buttonHeight));
                    cancel = button;
                    height += buttonHeight;
                }
                else if (button.Style == ASMultiSelectActionSheetButtonStyle.PerformAction)
                {
                    float x = width / 2 + 1;
                    float y = height;
                    if (cancel != null)
                    {
                        y = (float)cancel.Frame.Y;
                        height -= buttonHeight -= spacing;

                        controlLine.Frame = new CGRect(new CGPoint(width / 2, height - 2), new CGSize(1.0f, buttonHeight + spacing + 2));
                    }
                    button.Frame = new CGRect(new CGPoint(x, y), new CGSize(((width - spacing * 2.0f)/2) - 1, buttonHeight));
                    action = button;
                    height += buttonHeight;
                }
                else
                {
                    button.Frame = new CGRect(new CGPoint(spacing, height), new CGSize(width - spacing * 2.0f, buttonHeight));  
                    height += buttonHeight;

                    //UIView buttonLine = horizontalLines.ElementAt(lineIndex);
                    line.Frame = new CGRect(new CGPoint(0.0f, height + 1), new CGSize(width, 1.0f));
                    height++;
                    lineIndex++;
                }

            }

            if (ContentView != null) 
            {
                height += spacing;
                ContentView.Frame = new CGRect(new CGPoint(spacing, height), new CGSize(width - spacing * 2.0f, ContentView.Frame.Height));
                height += (float)ContentView.Frame.Height;
            }

            height += spacing;
            Frame = new CGRect(CGPoint.Empty, new CGSize(width, height));
            return Frame;
        }
    }
}

