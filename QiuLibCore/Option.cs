namespace UTMTdrid
{
    internal class Option<T>
    {
        private string[] strings;
        private string description;

        public Option(string[] strings, string description)
        {
            this.strings = strings;
            this.description = description;
        }
    }
}