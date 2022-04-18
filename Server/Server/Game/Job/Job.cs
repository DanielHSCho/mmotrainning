﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public interface IJob
    {
        void Execute();
    }

    public class Job : IJob
    {
        Action _action;

        public Job(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action.Invoke();
        }
    }
}
