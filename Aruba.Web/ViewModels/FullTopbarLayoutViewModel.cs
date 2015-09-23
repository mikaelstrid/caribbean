namespace Caribbean.Aruba.Web.ViewModels
{
    public class FullTopbarLayoutViewModel
    {
        public MenuItem ActiveMenuItem { get; set; }
    }

    public enum MenuItem
    {
        Unknown,
        Start,
        Trycksaksoversikt
    }
}