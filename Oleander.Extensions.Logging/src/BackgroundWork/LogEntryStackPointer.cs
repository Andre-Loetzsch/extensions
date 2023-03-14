namespace Oleander.Extensions.Logging.BackgroundWork
{
    internal class LogEntryStackPointer
    {
        public int AddPointer { get; private set; }
        public int GetPointer { get; private set; } = 1;

        public void Change()
        {
            this.AddPointer = this.AddPointer == 0 ? 1 : 0;
            this.GetPointer = this.AddPointer == 0 ? 1 : 0;
        }
    }
}