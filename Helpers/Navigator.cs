using System.Windows.Controls;

namespace proyecto_tdp_2.Helpers
{
    public static class Navigator
    {
        public static event Action<UserControl>? OnNavigate;
        public static void NavigateTo(UserControl view)
        {
            OnNavigate?.Invoke(view);
        }
    }
}
