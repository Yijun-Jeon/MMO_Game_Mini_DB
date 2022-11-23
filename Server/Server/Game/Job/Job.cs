using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public interface IJob
    {
        // 실행
        void Execute();
    }

    // 인자가 없는 Job
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

    // 인자 하나를 받는 Job
    public class Job<T1> : IJob
    {
        Action<T1> _action;
        T1 _t1;

        public Job(Action<T1> action, T1 t1)
        {
            _action = action;
            _t1 = t1;
        }
        public void Execute()
        {
            _action.Invoke(_t1);
        }
    }

    // 인자 두 개를 받는 Job
    public class Job<T1,T2> : IJob
    {
        Action<T1, T2> _action;
        T1 _t1;
        T2 _t2;

        public Job(Action<T1, T2> action, T1 t1, T2 t2)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
        }
        public void Execute()
        {
            _action.Invoke(_t1,_t2);
        }
    }

    // 인자 세 개를 받는 Job
    public class Job<T1, T2, T3> : IJob
    {
        Action<T1, T2, T3> _action;
        T1 _t1;
        T2 _t2;
        T3 _t3;

        public Job(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
        }
        public void Execute()
        {
            _action.Invoke(_t1, _t2, _t3);
        }
    }
}

