using System;
using System.Windows.Forms;
using CodeProject.Dialog;

namespace WindowsConverter.Extensions
{
    static class FormExtensions
    {
        public static void ShowInfo(this IWin32Window window, string text, string caption = "Information")
        {
            MsgBox.Show(window, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        public static void ShowWarning(this IWin32Window window, string text, string caption = "Warning")
        {
            MsgBox.Show(window, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
        }

        public static void ShowError(this IWin32Window window, string text, string caption = "Error")
        {
            MsgBox.Show(window, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        public static bool ShowYesNoQuestion(this IWin32Window window, string text, string caption = "Question")
        {
            return MsgBox.Show(window, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public static void TryInvoke<TEventArgs>(this Form form, EventHandler<TEventArgs> eventHandler, object sender, TEventArgs eventArgs)
            where TEventArgs : EventArgs
        {
            if (form.InvokeRequired)
            {
                form.Invoke(eventHandler, new[] { sender, eventArgs });
            }
            else
            {
                eventHandler(sender, eventArgs);
            }
        }
    }
}
