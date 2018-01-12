using Foundation;
using System;
using UIKit;
using ASMultiSelectActionSheet;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMultiSelectActionSheetSamples
{
    public partial class HomeViewController : UIViewController, IASMultiSelectActionSheetDelegate
    {
        static AppDelegate _app
        {
            get { return (AppDelegate)UIApplication.SharedApplication.Delegate; }
        }

        public List<TestItem> DemoData { get; set; }

        public HomeViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            DemoData = new List<TestItem>();
            DemoData.Add(new TestItem("Option 1", String.Empty));
            DemoData.Add(new TestItem("Option 2", String.Empty));
            DemoData.Add(new TestItem("Option 3", String.Empty));
            DemoData.Add(new TestItem("Option 4", String.Empty));
            DemoData.Add(new TestItem("Option 5", String.Empty));

            BasicFunctionButton.TouchUpInside += (DisplayBasicFunctionality);
        }

        private void DisplayBasicFunctionality(object sender, EventArgs e)
        {
            var buttons = DemoData.Select(o => new Tuple<string, object>(o.Text, o)).ToList();
            if (buttons == null || !buttons.Any()) return;

            //The receiver of this action is an object; you can use whatever you need here or nothing at all
            Action<List<TestItem>> goAction = (items) => PerformGoAction(items);

            var optionsSection = new ASMultiSelectActionSheetSection("Multi Select", "Choose Your Options", buttons, ASMultiSelectActionSheetButtonStyle.MultiSelect, "GO!", goAction);
            if (optionsSection == null) return;

            var alertSheet = new ASMultiSelectActionSheet.ASMultiSelectActionSheet(new List<ASMultiSelectActionSheetSection> { optionsSection }, this);

            var vc = _app.PresentedViewController();

            alertSheet.ShowInView(vc.View, true);
        }

        private void PerformGoAction(List<TestItem> items)
        {
            StringBuilder sb = new StringBuilder("Here's what you chose:");
            foreach (var item in items)
                sb.Append(String.Format("\n{0}", item.Text));

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var alertController = UIAlertController.Create("Results", sb.ToString(), UIAlertControllerStyle.Alert);
                var okAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, alertAction => { });
                alertController.AddAction(okAction);
                _app.PresentedViewController().PresentViewController(alertController, true, null);
            });
        }

        //IASMultiSelectActionSheetDelegate
        public void ActionSheetPressedButtonAtIndex(ASMultiSelectActionSheet.ASMultiSelectActionSheet actionSheet, NSIndexPath index, ASMultiSelectActionSheetButton sender)
        {
            if (actionSheet != null)
            {
                if (sender.Style == ASMultiSelectActionSheetButtonStyle.Cancel)
                {
                    //TODO Perform any cancellation activities here
                    actionSheet.DismissAnimated(true);
                    return;
                }
                else if (sender.Style == ASMultiSelectActionSheetButtonStyle.PerformAction)
                {
                    //TODO Do whatever you need to when PerformAction button is tapped.
                    //      Button.data is an object so use it in whatever way you need.
                    List<TestItem> items = actionSheet.Sections?.ElementAt(0)?.Buttons?
                                                    .Where(b => b.Style == ASMultiSelectActionSheetButtonStyle.MultiSelect && b.Selected)
                                                    .Select(b => (TestItem)b.Data).ToList();
                    Action<List<TestItem>> command = (Action<List<TestItem>>)sender.Data;
                    command.Invoke(items);
                }

                actionSheet.DismissAnimated(true);
            }

            return;
        }

        public void ActionSheetDidDismiss(ASMultiSelectActionSheet.ASMultiSelectActionSheet actionSheet)
        {
            return;
        }

        public void ActionSheetDidPresent(ASMultiSelectActionSheet.ASMultiSelectActionSheet actionSheet)
        {
            return;
        }

        public void ActionSheetWillDismiss(ASMultiSelectActionSheet.ASMultiSelectActionSheet actionSheet)
        {
            return;
        }

        public void ActionSheetWillPresent(ASMultiSelectActionSheet.ASMultiSelectActionSheet actionSheet)
        {
            return;
        }
    }

    public class TestItem
    {
        public TestItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public string Text { get; set; }
        public object Value { get; set; }
    }
}