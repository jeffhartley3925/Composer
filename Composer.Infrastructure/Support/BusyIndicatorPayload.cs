namespace Composer.Infrastructure
{
    public class BusyIndicatorPayload
    {
        public string HeaderText { get; set; }

        public string BusyText { get; set; }

        public BusyIndicatorPayload(string headerText, string busyText)
        {
            this.HeaderText = headerText;
            this.BusyText = busyText;
        }
    }
}