using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace MetroSoft.HIS
{
    public class XMessageBox
    {
        public static void Show(string text)
        {
            MessageBox.Show (text);
        }

        //폼이 TOPMOST 일경우 메세제 박스 하단에 가려 표시 안됨 처리 메세지 박스 20201013 정민석
        public static void ShowTop(IWin32Window owner, string text, string caption)
        {
            Form frm = Application.OpenForms.OfType<Form>().Where((t) => t.TopMost).FirstOrDefault();
            MessageBox.Show((frm == null) ? owner : frm, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        public static void Show(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text);
        }
        public static void Show(string text, string caption)
        {
            MessageBox.Show(text, caption);
        }
        public static void Show(IWin32Window owner, string text, string caption)
        {
            MessageBox.Show(owner, text, caption);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(text, caption, buttons);
        }
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(owner, text, caption, buttons);
        }
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return MessageBox.Show(text, caption, buttons, icon, defaultButton);
        }
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
        }        
        public static void Warning(string text)
        {
            MessageBox.Show(text, "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(string text)
        {
            MessageBox.Show(text, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
        public static void Information(string text)
        {
            MessageBox.Show(text, "정보", MessageBoxButtons.OK, MessageBoxIcon.Information );
        }
        public static void Warning(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text, "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void Warning(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void Warning(IWin32Window owner, string text, string caption)
        {
            MessageBox.Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static DialogResult Warning(string text, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(text, caption, buttons, MessageBoxIcon.Warning);
        }
        public static DialogResult Warning(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(owner, text, caption, buttons, MessageBoxIcon.Warning);
        }
        public static DialogResult Warning(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }
        public static DialogResult Warning(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }
        public static DialogResult Warning(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return MessageBox.Show(text, caption, buttons, icon, defaultButton);
        }
        public static DialogResult Warning(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
        }
    }

    /// <summary>
    /// 버튼 캡션 바꾸는 메세지 박스 예/아니오 형태로는 의미전달이 어려울때..
    /// </summary>
    public class MessageBoxEx
    {
        delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int hook, HookProc callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetDlgItem(IntPtr hDlg, DialogResult nIDDlgItem);

        [DllImport("user32.dll")]
        static extern bool SetDlgItemText(IntPtr hDlg, DialogResult nIDDlgItem, string lpString);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        System.Threading.Timer _timeoutTimer; 

        static IntPtr g_hHook;

        static string yes;
        static string cancel;
        static string no;

        /// <summary>
        /// 메시지 박스를 띠웁니다.
        /// </summary>
        /// <param name="text">텍스트 입니다.</param>
        /// <param name="caption">캡션 입니다.</param>
        /// <param name="yes">예 문자열 입니다.</param>
        /// <param name="no">아니오 문자열 입니다.</param>
        /// <param name="cancel">취소 문자열 입니다.</param>
        /// <returns></returns>
        public static DialogResult Show(string text, string caption, string yes, string no, string cancel)
        {
            MessageBoxEx.yes = yes;
            MessageBoxEx.cancel = cancel;
            MessageBoxEx.no = no;
            g_hHook = SetWindowsHookEx(5, new HookProc(HookWndProc), IntPtr.Zero, GetCurrentThreadId());
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }

        public static DialogResult Show(string text, string caption, string yes, string no )
        {
            MessageBoxEx.yes = yes;
            MessageBoxEx.no = no;
            g_hHook = SetWindowsHookEx(5, new HookProc(HookWndProc), IntPtr.Zero, GetCurrentThreadId());
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNo , MessageBoxIcon.Question);
        }

           static int HookWndProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            IntPtr hChildWnd;

            if (nCode == 5)
            {
                hChildWnd = wParam;

                if (GetDlgItem(hChildWnd, DialogResult.Yes) != null)
                    SetDlgItemText(hChildWnd, DialogResult.Yes, MessageBoxEx.yes);

                if (GetDlgItem(hChildWnd, DialogResult.No) != null)
                    SetDlgItemText(hChildWnd, DialogResult.No, MessageBoxEx.no);

                if (GetDlgItem(hChildWnd, DialogResult.Cancel) != null)
                    SetDlgItemText(hChildWnd, DialogResult.Cancel, MessageBoxEx.cancel);

                UnhookWindowsHookEx(g_hHook);
            }
            else
                CallNextHookEx(g_hHook, nCode, wParam, lParam);

            return 0;
        }
    }

    /// <summary>
    /// 자동 닫히는 메세지 박스 ex.완료되었습니다.
    /// </summary>
    public class MessageBoxExAuto
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        System.Threading.Timer _timeoutTimer; //쓰레드 타이머 
        string _caption;
        const int WM_CLOSE = 0x0010; //close 명령

        MessageBoxExAuto(string text, string caption, int timeout)
        { _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, System.Threading.Timeout.Infinite);
            MessageBox.Show(text, caption);
        }
        //생성자 함수
        public static void Show(string text, string caption, int timeout =1)
        { new MessageBoxExAuto(text, caption, timeout*1000);
        }

        //시간이 다되면 close 메세지를 보냄 
        void OnTimerElapsed(object state)
        { IntPtr mbWnd = FindWindow(null, _caption);
            if (mbWnd != IntPtr.Zero) SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
    }



}
