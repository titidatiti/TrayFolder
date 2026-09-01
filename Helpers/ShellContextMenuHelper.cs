using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace TrayFolder.Helpers
{
    public static class NativeContextMenuHelper
    {
        #region COM Interfaces

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E4-0000-0000-C000-000000000046")]
        public interface IContextMenu
        {
            [PreserveSig]
            int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
            [PreserveSig]
            int InvokeCommand(ref CMINVOKECOMMANDINFO pici);
            [PreserveSig]
            int GetCommandString(UIntPtr idCmd, uint uType, IntPtr pReserved, StringBuilder pszName, uint cchMax);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214f4-0000-0000-c000-000000000046")]
        public interface IContextMenu2 : IContextMenu
        {
            [PreserveSig]
            new int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
            [PreserveSig]
            new int InvokeCommand(ref CMINVOKECOMMANDINFO pici);
            [PreserveSig]
            new int GetCommandString(UIntPtr idCmd, uint uType, IntPtr pReserved, StringBuilder pszName, uint cchMax);
            [PreserveSig]
            int HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BCFCE0A0-EC17-11d0-8D10-00A0C90F2719")]
        public interface IContextMenu3 : IContextMenu2
        {
            [PreserveSig]
            new int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
            [PreserveSig]
            new int InvokeCommand(ref CMINVOKECOMMANDINFO pici);
            [PreserveSig]
            new int GetCommandString(UIntPtr idCmd, uint uType, IntPtr pReserved, StringBuilder pszName, uint cchMax);
            [PreserveSig]
            new int HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);
            [PreserveSig]
            int HandleMenuMsg2(uint uMsg, IntPtr wParam, IntPtr lParam, out IntPtr plResult);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E6-0000-0000-C000-000000000046")]
        public interface IShellFolder
        {
            [PreserveSig]
            int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
            [PreserveSig]
            int EnumObjects(IntPtr hwnd, int grfFlags, out IntPtr ppenumIDList);
            [PreserveSig]
            int BindToObject(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IntPtr ppv);
            [PreserveSig]
            int BindToStorage(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IntPtr ppv);
            [PreserveSig]
            int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);
            [PreserveSig]
            int CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);
            [PreserveSig]
            int GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref uint rgfInOut);
            [PreserveSig]
            int GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, [In] ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);
            [PreserveSig]
            int GetDisplayNameOf(IntPtr pidl, uint uFlags, IntPtr pName);
            [PreserveSig]
            int SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
        }

        #endregion

        #region Structs and Constants

        [StructLayout(LayoutKind.Sequential)]
        public struct CMINVOKECOMMANDINFO
        {
            public int cbSize;
            public int fMask;
            public IntPtr hwnd;
            public IntPtr lpVerb;
            public IntPtr lpParameters;
            public IntPtr lpDirectory;
            public int nShow;
            public int dwHotKey;
            public IntPtr hIcon;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32.dll")]
        private static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr pv);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

        [DllImport("shell32.dll", ExactSpelling = true, PreserveSig = true)]
        private static extern int SHBindToParent(IntPtr pidl, [In] ref Guid riid, out IShellFolder ppv, out IntPtr ppidlLast);

        private const uint CMF_NORMAL = 0x00000000;

        private const uint TPM_RETURNCMD = 0x0100;
        private const uint TPM_RIGHTBUTTON = 0x0002;

        private const int WM_DRAWITEM = 0x002B;
        private const int WM_MEASUREITEM = 0x002C;
        private const int WM_INITMENUPOPUP = 0x0117;
        private const int WM_MENUCHAR = 0x0120;

        private static readonly Guid IID_IContextMenu = new Guid("000214E4-0000-0000-C000-000000000046");
        private static readonly Guid IID_IShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");

        #endregion

        public static void ShowContextMenu(string path, int x, int y, Window ownerWindow)
        {
            if (string.IsNullOrEmpty(path)) return;

            IntPtr pidlFull = IntPtr.Zero;
            IntPtr pidlChild = IntPtr.Zero;
            IShellFolder? parentFolder = null;
            IContextMenu? contextMenu = null;
            IntPtr hMenu = IntPtr.Zero;
            IntPtr iContextMenuPtr = IntPtr.Zero;
            HwndSource? hwndSource = null;
            HwndSourceHook? menuMessageHook = null;

            try
            {
                uint attributes = 0;
                if (Failed(SHParseDisplayName(path, IntPtr.Zero, out pidlFull, 0, out attributes))) return;
                Guid shellFolderId = IID_IShellFolder;
                if (Failed(SHBindToParent(pidlFull, ref shellFolderId, out parentFolder, out pidlChild))) return;

                IntPtr hwnd = new WindowInteropHelper(ownerWindow).EnsureHandle();
                IntPtr[] apidl = new IntPtr[] { pidlChild };
                Guid contextMenuId = IID_IContextMenu;
                if (Failed(parentFolder.GetUIObjectOf(IntPtr.Zero, 1, apidl, ref contextMenuId, IntPtr.Zero, out iContextMenuPtr))) return;

                contextMenu = (IContextMenu)Marshal.GetObjectForIUnknown(iContextMenuPtr);

                hMenu = CreatePopupMenu();
                if (hMenu == IntPtr.Zero) return;

                const uint commandIdFirst = 1;
                if (Failed(contextMenu.QueryContextMenu(hMenu, 0, commandIdFirst, 0x7FFF, CMF_NORMAL))) return;

                // Owner-drawn and dynamic shell extensions rely on these messages. Without
                // forwarding them, some handlers fail inside native code before .NET can catch it.
                IContextMenu3? contextMenu3 = contextMenu as IContextMenu3;
                IContextMenu2? contextMenu2 = contextMenu3 ?? contextMenu as IContextMenu2;
                if (contextMenu2 != null)
                {
                    menuMessageHook = CreateMenuMessageHook(contextMenu2, contextMenu3);
                    hwndSource = HwndSource.FromHwnd(hwnd);
                    hwndSource?.AddHook(menuMessageHook);
                }

                int command = TrackPopupMenuEx(hMenu, TPM_RETURNCMD | TPM_RIGHTBUTTON, x, y, hwnd, IntPtr.Zero);

                if (command > 0)
                {
                    CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO
                    {
                        cbSize = Marshal.SizeOf<CMINVOKECOMMANDINFO>(),
                        hwnd = hwnd,
                        lpVerb = (IntPtr)(command - commandIdFirst),
                        nShow = 1 // SW_SHOWNORMAL
                    };

                    contextMenu.InvokeCommand(ref invoke);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                if (menuMessageHook != null) hwndSource?.RemoveHook(menuMessageHook);
                if (hMenu != IntPtr.Zero) DestroyMenu(hMenu);
                if (contextMenu != null) Marshal.ReleaseComObject(contextMenu);
                if (iContextMenuPtr != IntPtr.Zero) Marshal.Release(iContextMenuPtr);
                if (parentFolder != null) Marshal.ReleaseComObject(parentFolder);
                if (pidlFull != IntPtr.Zero) CoTaskMemFree(pidlFull);
            }
        }

        private static HwndSourceHook CreateMenuMessageHook(IContextMenu2 contextMenu2, IContextMenu3? contextMenu3)
        {
            return (IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                if (message == WM_MENUCHAR && contextMenu3 != null)
                {
                    int result = contextMenu3.HandleMenuMsg2((uint)message, wParam, lParam, out IntPtr menuResult);
                    handled = !Failed(result);
                    return menuResult;
                }

                if (message != WM_DRAWITEM && message != WM_MEASUREITEM && message != WM_INITMENUPOPUP)
                {
                    return IntPtr.Zero;
                }

                if (contextMenu3 != null)
                {
                    int result = contextMenu3.HandleMenuMsg2((uint)message, wParam, lParam, out IntPtr menuResult);
                    handled = !Failed(result);
                    return menuResult;
                }

                handled = !Failed(contextMenu2.HandleMenuMsg((uint)message, wParam, lParam));
                return IntPtr.Zero;
            };
        }

        private static bool Failed(int hResult) => hResult < 0;
    }
}
