
using Android.App;
using Android.Widget;
using Android.OS;

namespace MaskedEditText
{
    [Activity(Label = "MaskedEditText", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var maskedText = FindViewById<MaskedEditText>(Resource.Id.phoneEdit);
            maskedText.Mask = "+@@:@@@@:@@@@@@";
        }
    }
}


