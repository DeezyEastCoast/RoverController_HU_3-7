//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18063
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OakhillLandroverController
{
    
    internal partial class Resources
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resources.manager == null))
                {
                    Resources.manager = new System.Resources.ResourceManager("OakhillLandroverController.Resources", typeof(Resources).Assembly);
                }
                return Resources.manager;
            }
        }
        internal static Microsoft.SPOT.Bitmap GetBitmap(Resources.BitmapResources id)
        {
            return ((Microsoft.SPOT.Bitmap)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        internal static string GetString(Resources.StringResources id)
        {
            return ((string)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum StringResources : short
        {
            GroundEfx_Window = -29485,
            wndSetup = -11028,
            String1 = 1228,
            wndCapture = 5848,
            EcoCam_Window = 6454,
            wndMain = 19335,
        }
        [System.SerializableAttribute()]
        internal enum BitmapResources : short
        {
            Keyboard_320x128_Up_Lowercase = -27169,
            Keyboard_320x128_Up_Symbols = -5791,
            Keyboard_320x128_Down_Uppercase = -2978,
            Keyboard_320x128_Down_Numbers = -1630,
            Keyboard_320x128_Up_Uppercase = 2629,
            Stinkmeaner_Thumb = 3809,
            Keyboard_320x128_Up_Numbers = 16467,
            Keyboard_320x128_Down_Lowercase = 20444,
            Keyboard_320x128_Down_Symbols = 30074,
        }
    }
}
