using Serilog.Core;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;


namespace ArticlesWebApp.Api.Abstractions;


public abstract class BaseLogSink<T> : ILogEventSink where T : BaseEventEntity, new()
{
    protected readonly DbContext _dbContext;
    protected readonly IHttpContextAccessor HttpContextAccessor;
    //protected readonly IFormatProvider FormatProvider;

    protected BaseLogSink(
            DbContext dbContext,
            IHttpContextAccessor httpContextAccessor
            //IFormatProvider formatProvider = null
            )
    {
        _dbContext = dbContext;
        HttpContextAccessor = httpContextAccessor;
        //FormatProvider = formatProvider;
    }

    public async void Emit(LogEvent logEvent)
    {
        // here should be all the main logic
        var log = CreateLog(logEvent);
        await SaveLog(log);
    }

    protected abstract T CreateLog(LogEvent logEvent);

    protected virtual async Task SaveLog(T logEntity)
    {
        await _dbContext.Set<T>().AddAsync(logEntity);
        await _dbContext.SaveChangesAsync();
    }
}
