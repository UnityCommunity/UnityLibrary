using System.Threading;
using System.Text;

// Info: OptimizedStringOperation
// Concatenating many strings allocates a lot of temporary memory in the managed heap. It is recommend to use System.Text.StringBuffer instead.
// Sometimes this is hard to fix, especially if you already have written a lot of code. This solution should help you to work around that problem easily.
// source: https://bitbucket.org/Unity-Technologies/enterprise-support

namespace StringOperationUtil
{
    /// <summary>
    /// Using this,you can optimize string concat operation easily.
    /// To use this , you should put this on the top of code.
    /// ------
    /// using StrOpe = StringOperationUtil.OptimizedStringOperation;
    /// ------
    /// 
    /// - before code
    /// string str = "aaa" + 20 + "bbbb";
    /// 
    /// - after code
    /// string str = StrOpe.i + "aaa" + 20 + "bbbb";
    /// 
    /// "StrOpe.i" is for MainThread , do not call from other theads.
    /// If "StrOpe.i" is called from Mainthread , reuse same object.
    /// 
    /// You can also use "StrOpe.small" / "StrOpe.medium" / "StrOpe.large" instead of "StrOpe.i". 
    /// These are creating instance.
    /// </summary>
    public class OptimizedStringOperation
    {
        private static OptimizedStringOperation instance = null;
        #if !UNITY_WEBGL
        private static Thread singletonThread = null;
        #endif
        private StringBuilder sb = null;

        static OptimizedStringOperation()
        {
            instance = new OptimizedStringOperation(1024);
        }
        private OptimizedStringOperation(int capacity)
        {
            sb = new StringBuilder(capacity);
        }

        public static OptimizedStringOperation Create(int capacity)
        {
            return new OptimizedStringOperation(capacity);
        }

        public static OptimizedStringOperation small
        {
            get
            {
                return Create(64);
            }
        }

        public static OptimizedStringOperation medium
        {
            get
            {
                return Create(256);
            }
        }
        public static OptimizedStringOperation large
        {
            get
            {
                return Create(1024);
            }
        }

        public static OptimizedStringOperation i
        {
            get
            {
                #if !UNITY_WEBGL
                // Bind instance to thread.
                if (singletonThread == null )
                {
                    singletonThread = Thread.CurrentThread;
                }
                // check thread...
                if (singletonThread != Thread.CurrentThread)
                {
                    #if DEBUG || UNITY_EDITOR
                    UnityEngine.Debug.LogError("Execute from another thread.");
                    #endif
                    return small;
                }
                #endif
                instance.sb.Length = 0;
                return instance;
            }
        }

        public int Capacity
        {
            set { this.sb.Capacity = value; }
            get { return sb.Capacity; }
        }

        public int Length
        {
            set { this.sb.Length = value; }
            get { return this.sb.Length; }
        }
        public OptimizedStringOperation Remove(int startIndex, int length)
        {
            sb.Remove(startIndex, length);
            return this;
        }
        public OptimizedStringOperation Replace(string oldValue, string newValue)
        {
            sb.Replace(oldValue, newValue);
            return this;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public void Clear()
        {
            // StringBuilder.Clear() doesn't support .Net 3.5...
            // "Capasity = 0" doesn't work....
            sb = new StringBuilder(0);
        }

        public OptimizedStringOperation ToLower()
        {
            int length = sb.Length;
            for (int i = 0; i < length; ++i)
            {
                if (char.IsUpper(sb[i]))
                {
                    sb.Replace(sb[i], char.ToLower(sb[i]), i, 1);
                }
            }
            return this;
        }
        public OptimizedStringOperation ToUpper()
        {
            int length = sb.Length;
            for (int i = 0; i < length; ++i)
            {
                if (char.IsLower(sb[i]))
                {
                    sb.Replace(sb[i], char.ToUpper(sb[i]), i, 1);
                }
            }
            return this;
        }

        public OptimizedStringOperation Trim()
        {
            return TrimEnd().TrimStart();
        }

        public OptimizedStringOperation TrimStart()
        {
            int length = sb.Length;
            for (int i = 0; i < length; ++i)
            {
                if (!char.IsWhiteSpace(sb[i]))
                {
                    if (i > 0)
                    {
                        sb.Remove(0, i);
                    }
                    break;
                }
            }
            return this;
        }
        public OptimizedStringOperation TrimEnd()
        {
            int length = sb.Length;
            for (int i = length - 1; i >= 0; --i)
            {
                if (!char.IsWhiteSpace(sb[i]))
                {
                    if (i < length - 1)
                    {
                        sb.Remove(i, length - i);
                    }
                    break;
                }
            }
            return this;
        }


        public static implicit operator string(OptimizedStringOperation t)
        {
            return t.ToString();
        }

        #region ADD_OPERATOR
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, bool v)
        {
            t.sb.Append(v);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, int v)
        {
            t.sb.Append(v);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, short v)
        {
            t.sb.Append(v);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, byte v)
        {
            t.sb.Append(v);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, float v)
        {
            t.sb.Append(v);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, char c)
        {
            t.sb.Append(c);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, char[] c)
        {
            t.sb.Append(c);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, string str)
        {
            t.sb.Append(str);
            return t;
        }
        public static OptimizedStringOperation operator +(OptimizedStringOperation t, StringBuilder sb)
        {
            t.sb.Append(sb);
            return t;
        }
        #endregion ADD_OPERATOR
    }
}
