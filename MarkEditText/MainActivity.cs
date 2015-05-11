
using Android.App;
using Android.Widget;
using Android.OS;

namespace MaskedEditText
{
    [Activity(Label = "MaskedEditText", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
        }
    }
}


