using System.Collections.Concurrent;


namespace GPSLocator.Handlers
{
	public class UserRequestHandler
	{
		private readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

		public async Task<T> HandleUserRequestAsync<T>(string userId, Func<Task<T>> userTask)
		{
			var userSemaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

			await userSemaphore.WaitAsync();
			try
			{
				return await userTask();
			}
			finally
			{
				userSemaphore.Release();

				if (userSemaphore.CurrentCount == 1)
				{
					_userLocks.TryRemove(userId, out _);
				}
			}
		}

		public async Task HandleUserRequestAsync(string userId, Func<Task> userTask)
		{
			var userSemaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

			await userSemaphore.WaitAsync();
			try
			{
				await userTask();
			}
			finally
			{
				userSemaphore.Release();

				if (userSemaphore.CurrentCount == 1)
				{
					_userLocks.TryRemove(userId, out _);
				}
			}
		}
	}
}
