using System;
using FilesDistributor.EventArgs;

namespace FilesDistributor.Abstract
{
    public interface ILocationsWatcher<TModel>
    {
        event EventHandler<CreatedEventArgs<TModel>> Created;
    }
}
