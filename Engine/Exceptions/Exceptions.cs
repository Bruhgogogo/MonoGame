using System;

namespace MonoEngine.Exceptions
{
    public class DoubleDestroyException : Exception
    {
        public DoubleDestroyException() : base("Attempt to destroy a destroyed object at twice!")
        {

        }
    }
}