namespace FilesDistributor.EventArgs
{
    public class CreatedEventArgs<TModel> : System.EventArgs
    {
        public TModel CreatedItem { get; set; }
    }
}
