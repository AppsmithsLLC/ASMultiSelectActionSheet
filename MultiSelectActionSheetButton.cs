using System.Drawing;
using CoreGraphics;
using UIKit;

namespace ASMultiSelectActionSheet
{
    public class ASMultiSelectActionSheetButton : UIButton
    {
        public int Row { get; set; }
        public object Data { get; set; }
        public ASMultiSelectActionSheetButtonStyle Style { get; set; }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (Style != ASMultiSelectActionSheetButtonStyle.MultiSelect)
                return;

            UIImageView imageView = ImageView;
            UILabel label = TitleLabel;

            CGRect imageFrame = imageView.Frame;
            CGRect labelFrame = label.Frame;

            float imageY = (float)((Frame.Height / 2) - (imageFrame.Height / 2));
            imageView.Frame = new RectangleF((float)Frame.Width - (float)imageView.Frame.Width - 15.0f, imageY, (float)imageView.Frame.Width, (float)imageView.Frame.Height);

            float labelY = (float)((Frame.Height / 2) - (labelFrame.Height / 2));
            label.Frame = new RectangleF(15.0f, labelY, (float)label.Frame.Width, (float)label.Frame.Height);
        }

        public void SetButtonStyleForButton(ASMultiSelectActionSheetButtonStyle style)
        {
            UIColor backgroundColor = UIColor.White;
            UIColor borderColor = UIColor.White;
            UIColor titleColor = UIColor.Black;
            UIFont font = null;

            switch (style)
            {
                case ASMultiSelectActionSheetButtonStyle.Default:
                    font = UIFont.SystemFontOfSize(15.0f);
                    break;
                case ASMultiSelectActionSheetButtonStyle.MultiSelect:
                    font = UIFont.SystemFontOfSize(15.0f);
                    break;
                case ASMultiSelectActionSheetButtonStyle.Cancel:
                    font = UIFont.BoldSystemFontOfSize(25.0f);
                    titleColor = UIColor.FromRGB(0.21f, 0.54f, 0.75f);
                    break;
                case ASMultiSelectActionSheetButtonStyle.PerformAction:
                    font = UIFont.SystemFontOfSize(25.0f);
                    titleColor = UIColor.FromRGB(0.21f, 0.54f, 0.75f);
                    SetTitleColor(UIColor.DarkGray, UIControlState.Disabled);
                    break;
                default:
                    break;
            }

            SetTitleColor(titleColor, UIControlState.Normal);
            TitleLabel.Font = font;
            SetBackgroundImage(PixelImageWithColor(backgroundColor), UIControlState.Normal);
            SetBackgroundImage(PixelImageWithColor(borderColor), UIControlState.Highlighted);
            Layer.BorderColor = borderColor.CGColor;
            Layer.CornerRadius = 5.0f;

            if (style == ASMultiSelectActionSheetButtonStyle.MultiSelect)
            {
                SetImage(UIImage.FromFile("check-circle-Small.png"), UIControlState.Selected);
            }
        }

        private UIImage PixelImageWithColor(UIColor color)
        {
            UIGraphics.BeginImageContextWithOptions(new CGSize(1.0f, 1.0f), true, 0);

            color.SetFill();
            UIBezierPath.FromRect(new CGRect(CGPoint.Empty, new CGSize(1.0f, 1.0f))).Fill();
            UIImage img = UIGraphics.GetImageFromCurrentImageContext();

            UIGraphics.EndImageContext();

            return img.CreateResizableImage(UIEdgeInsets.Zero);
        }
    }

    public enum ASMultiSelectActionSheetButtonStyle
    {
        Default,
        Cancel,
        MultiSelect,
        PerformAction
    }
}
