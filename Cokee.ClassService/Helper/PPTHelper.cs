using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

using Microsoft.Office.Interop.PowerPoint;

namespace Cokee.ClassService.Helper
{
    public static class MarshalForCore
    {
        internal const String OLEAUT32 = "oleaut32.dll";
        internal const String OLE32 = "ole32.dll";

        //[SecurityCritical]  // auto-generated_required
        public static Object? GetActiveObject(String progID)
        {
            Guid clsid;

            // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
            // CLSIDFromProgIDEx doesn't exist.
            try
            {
                CLSIDFromProgIDEx(progID, out clsid);
            }
            //            catch
            catch
            {
                try
                {
                    CLSIDFromProgID(progID, out clsid);
                }
                catch
                {
                    return null;
                }
            }
            object obj;
            try
            {
                GetActiveObject(ref clsid, IntPtr.Zero, out obj);
            }
            catch (COMException)
            {
                return null;
            }
            catch
            {
                return null;
            }
            return obj;
        }

        //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
        [DllImport(OLE32, PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        //[SecurityCritical]  // auto-generated
        private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

        //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
        [DllImport(OLE32, PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        //[SecurityCritical]  // auto-generated
        private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

        //[DllImport(Microsoft.Win32.Win32Native.OLEAUT32, PreserveSig = false)]
        [DllImport(OLEAUT32, PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        //[SecurityCritical]  // auto-generated
        private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out Object ppunk);
    }
}

