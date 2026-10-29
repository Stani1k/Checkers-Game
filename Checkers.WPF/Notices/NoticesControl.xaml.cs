using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Checkers.WPF.Notices;

public partial class NoticesControl : UserControl
{
    public NoticesControl()
    {
        InitializeComponent();
    }

    private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        if (e.Uri is not { Scheme: "http" or "https" }) return;
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Serilog.Log.Warning(ex, "Could not open {Url}", e.Uri);
        }
    }
}
