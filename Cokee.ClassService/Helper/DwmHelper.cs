using System;
using System.Runtime.InteropServices;

namespace Cokee.ClassService.Helper
{
    public static class Dwmapi
    {
        public const string LibraryName = "Dwmapi.dll";

        [DllImport(LibraryName, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DwmIsCompositionEnabled();
    }
}
