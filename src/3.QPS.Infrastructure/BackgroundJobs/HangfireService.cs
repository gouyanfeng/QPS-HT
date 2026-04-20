using Hangfire;

namespace QPS.Infrastructure.BackgroundJobs;

public class HangfireService
{
    public void SchedulePowerOffJob(Guid orderId, int delayMinutes)
    {
        BackgroundJob.Schedule<PowerOffJob>(job => job.Execute(orderId), TimeSpan.FromMinutes(delayMinutes));
    }
}