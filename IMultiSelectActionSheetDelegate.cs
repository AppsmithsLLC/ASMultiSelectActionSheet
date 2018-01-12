using Foundation;

namespace ASMultiSelectActionSheet
{
    public interface IASMultiSelectActionSheetDelegate
    {
        /// <summary>
        /// Called before the action sheet will present.
        /// Included to mimic typical UIView life cycle.
        /// </summary>
        /// <param name="actionSheet">Action sheet.</param>
        void ActionSheetWillPresent(ASMultiSelectActionSheet actionSheet);

        /// <summary>
        /// Called before the action sheet will present.
        /// Included to mimic typical UIView life cycle.
        /// </summary>
        /// <param name="actionSheet">Action sheet.</param>
        void ActionSheetDidPresent(ASMultiSelectActionSheet actionSheet);

        /// <summary>
        /// Called before the action sheet will dismiss.
        /// Included to mimic typical UIView life cycle.
        /// </summary>
        /// <param name="actionSheet">Action sheet.</param>
        void ActionSheetWillDismiss(ASMultiSelectActionSheet actionSheet);

        /// <summary>
        /// Called after the action sheet did dismiss.
        /// Included to mimic typical UIView life cycle.
        /// </summary>
        /// <param name="actionSheet">Action sheet.</param>
        void ActionSheetDidDismiss(ASMultiSelectActionSheet actionSheet);

        /// <summary>
        /// Called when a button in any section of the action sheet is pressed. 
        /// Unlike UIActionSheet, JGActionSheet does not automatically dismiss when a button is pressed. 
        /// You need to manually call dismissAnimated: from the delegate to dismiss the action sheet.
        /// </summary>
        /// <param name="actionSheet">Action sheet.</param>
        /// <param name="index">The index path of the pressed button. (Section, Row)</param>
        void ActionSheetPressedButtonAtIndex(ASMultiSelectActionSheet actionSheet, NSIndexPath index, ASMultiSelectActionSheetButton sender);

    }
}
