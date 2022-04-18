using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    class JobSerializer
    {
		Queue<IJob> _jobQueue = new Queue<IJob>();
		object _lock = new object();
		bool _flush = false;

		public void Push(IJob job)
		{
			bool flush = false;

			lock (_lock) {
				_jobQueue.Enqueue(job);
				if (_flush == false)
					flush = _flush = true;
			}

			if (flush)
				Flush();
		}

		void Flush()
		{
			while (true) {
				IJob job = Pop();
				if (job == null)
					return;

				job.Execute();
			}
		}

		IJob Pop()
		{
			lock (_lock) {
				if (_jobQueue.Count == 0) {
					_flush = false;
					return null;
				}
				return _jobQueue.Dequeue();
			}
		}
	}
}
