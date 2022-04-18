using System;
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

    }
}
