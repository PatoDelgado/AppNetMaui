namespace HolaMundo
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            NadaBtn.IsEnabled = true;

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void OnAntiCounterClicked(object? sender, EventArgs e)
        {
            if (count > 0)
            {
                count--;
            }

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            if (count == 0)
            {
                NadaBtn.IsEnabled = false;
            }

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}

