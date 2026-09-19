using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TeaSpoons.EditorToolbox.Editor.Tests")]

namespace TeaSpoons.EditorToolbox.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A small utility window that asks for one line of text.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Enter confirms, Escape cancels. Closing the window counts as cancelling.</description></item>
    /// <item><description>The confirm button is disabled while the text is empty or only whitespace.</description></item>
    /// <item><description>The window does not block: the result is delivered through the callbacks passed to <see cref="Show"/>.</description></item>
    /// </list>
    /// </remarks>
    public class StringInputDialog : EditorWindow
    {
        private const float windowWidth = 360f;
        private const float windowHeight = 110f;
        private const string textControlName = "TeaSpoons.StringInputDialog.Text";

        private string message;
        private string okLabel;
        private string cancelLabel;
        private Action<string> onConfirm;
        private Action onCancel;
        private bool focusTextField = true;
        private bool finished;
        private bool shown;

        internal string Value { get; set; }

        /// <summary>
        /// Opens the dialog, centered on the editor's main window.
        /// </summary>
        /// <param name="title">The window title.</param>
        /// <param name="message">A short text shown above the input field.</param>
        /// <param name="initialValue">The text the input field starts with.</param>
        /// <param name="okLabel">The label of the confirm button.</param>
        /// <param name="cancelLabel">The label of the cancel button.</param>
        /// <param name="onConfirm">Called with the entered text when the user confirms.</param>
        /// <param name="onCancel">Called when the user cancels or closes the window.</param>
        /// <returns>The opened dialog.</returns>
        public static StringInputDialog Show(
            string title,
            string message,
            string initialValue,
            string okLabel,
            string cancelLabel,
            Action<string> onConfirm,
            Action onCancel = null)
        {
            var dialog = Create(title, message, initialValue, okLabel, cancelLabel, onConfirm, onCancel);
            dialog.ShowUtility();
            dialog.shown = true;
            return dialog;
        }

        // Builds the dialog without showing it, so the logic can be used and tested without a graphics device.
        internal static StringInputDialog Create(
            string title,
            string message,
            string initialValue,
            string okLabel,
            string cancelLabel,
            Action<string> onConfirm,
            Action onCancel)
        {
            var dialog = CreateInstance<StringInputDialog>();
            dialog.titleContent = new GUIContent(title);
            dialog.message = message;
            dialog.Value = initialValue ?? string.Empty;
            dialog.okLabel = okLabel;
            dialog.cancelLabel = cancelLabel;
            dialog.onConfirm = onConfirm;
            dialog.onCancel = onCancel;

            dialog.minSize = new Vector2(windowWidth, windowHeight);
            dialog.maxSize = dialog.minSize;

            var main = EditorGUIUtility.GetMainWindowPosition();
            dialog.position = new Rect(
                main.center.x - windowWidth * 0.5f,
                main.center.y - windowHeight * 0.5f,
                windowWidth,
                windowHeight);

            return dialog;
        }

        internal bool CanConfirm => !string.IsNullOrWhiteSpace(Value);

        // EditorWindow.Close throws when the window was never shown (for example when it was only created).
        internal void CloseWindow()
        {
            if (shown)
            {
                Close();
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        internal void Confirm()
        {
            if (finished || !CanConfirm)
            {
                return;
            }

            finished = true;
            var text = Value;
            var callback = onConfirm;
            CloseWindow();
            callback?.Invoke(text);
        }

        internal void Cancel()
        {
            if (finished)
            {
                return;
            }

            finished = true;
            var callback = onCancel;
            CloseWindow();
            callback?.Invoke();
        }

        private void OnGUI()
        {
            if (HandleKeys())
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);

            GUI.SetNextControlName(textControlName);
            Value = EditorGUILayout.TextField(Value);
            if (focusTextField)
            {
                EditorGUI.FocusTextInControl(textControlName);
                focusTextField = false;
            }

            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(cancelLabel, GUILayout.Width(90f)))
                {
                    Cancel();
                    GUIUtility.ExitGUI();
                }

                using (new EditorGUI.DisabledScope(!CanConfirm))
                {
                    if (GUILayout.Button(okLabel, GUILayout.Width(90f)))
                    {
                        Confirm();
                        GUIUtility.ExitGUI();
                    }
                }
            }

            GUILayout.Space(6f);
        }

        // Returns true when the window was closed by a key press.
        private bool HandleKeys()
        {
            var current = Event.current;
            if (current == null || current.type != EventType.KeyDown)
            {
                return false;
            }

            switch (current.keyCode)
            {
                case KeyCode.Escape:
                    current.Use();
                    Cancel();
                    GUIUtility.ExitGUI();
                    return true;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    current.Use();
                    Confirm();
                    if (finished)
                    {
                        GUIUtility.ExitGUI();
                    }

                    return finished;
                default:
                    return false;
            }
        }

        private void OnDestroy()
        {
            // The window was closed with the title bar button.
            if (!finished)
            {
                finished = true;
                onCancel?.Invoke();
            }
        }
    }
}
