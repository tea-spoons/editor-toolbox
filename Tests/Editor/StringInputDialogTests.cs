namespace TeaSpoons.EditorToolbox.Editor.Tests
{
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class StringInputDialogTests
    {
        private StringInputDialog dialog;

        [TearDown]
        public void TearDown()
        {
            if (dialog != null)
            {
                dialog.CloseWindow();
            }

            dialog = null;
        }

        [Test]
        public void CreateAppliesTheGivenTitleAndInitialValue()
        {
            dialog = StringInputDialog.Create("Create", "Name?", "start", "Create", "Cancel", _ => { }, null);

            Assert.IsNotNull(dialog);
            Assert.AreEqual("Create", dialog.titleContent.text);
            Assert.AreEqual("start", dialog.Value);
        }

        [Test]
        public void ShowOpensTheWindow()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore("Opening an editor window needs a graphics device (not available with -nographics).");
            }

            dialog = StringInputDialog.Show("Create", "Name?", "x", "Create", "Cancel", _ => { });

            Assert.IsNotNull(dialog);
            Assert.AreEqual("Create", dialog.titleContent.text);
        }

        [Test]
        public void ConfirmPassesTheEnteredTextToTheCallback()
        {
            string received = null;
            dialog = StringInputDialog.Create("Create", "Name?", string.Empty, "Create", "Cancel", text => received = text, null);

            dialog.Value = "MyModule";
            dialog.Confirm();

            Assert.AreEqual("MyModule", received);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void ConfirmDoesNothingWhileTheTextIsBlank(string blank)
        {
            var confirmed = false;
            dialog = StringInputDialog.Create("Create", "Name?", blank, "Create", "Cancel", _ => confirmed = true, null);

            Assert.IsFalse(dialog.CanConfirm);
            dialog.Confirm();

            Assert.IsFalse(confirmed);
        }

        [Test]
        public void CancelCallsTheCancelCallbackAndNotTheConfirmCallback()
        {
            var confirmed = false;
            var cancelled = false;
            dialog = StringInputDialog.Create("Create", "Name?", "x", "Create", "Cancel", _ => confirmed = true, () => cancelled = true);

            dialog.Cancel();

            Assert.IsTrue(cancelled);
            Assert.IsFalse(confirmed);
        }

        [Test]
        public void ClosingTheWindowCountsAsCancelling()
        {
            var cancelled = false;
            dialog = StringInputDialog.Create("Create", "Name?", "x", "Create", "Cancel", _ => { }, () => cancelled = true);

            dialog.CloseWindow();

            Assert.IsTrue(cancelled);
        }

        [Test]
        public void OnlyTheFirstResultIsReported()
        {
            var confirmCount = 0;
            var cancelCount = 0;
            dialog = StringInputDialog.Create("Create", "Name?", "x", "Create", "Cancel", _ => confirmCount++, () => cancelCount++);

            dialog.Confirm();
            dialog.Confirm();
            dialog.Cancel();

            Assert.AreEqual(1, confirmCount);
            Assert.AreEqual(0, cancelCount);
        }
    }
}
