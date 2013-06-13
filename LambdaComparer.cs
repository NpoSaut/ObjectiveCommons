using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectiveCommons
{
    /// <summary>
    /// Реализует интерфейс IComparer на основе указанного лямбда-выражения
    /// </summary>
    /// <typeparam name="T">Тип объекта для сравнения</typeparam>
    public class LambdaComparer<T> : IComparer<T>
    {
        /// <summary>
        /// Лямбда-выражение, используемое для сравнения
        /// </summary>
        public Func<T, T, int> Predicate { get; private set; }

        /// <summary>
        /// Создаёт компаратор на основе лямбда-выражения
        /// </summary>
        /// <param name="Predicate">Выражение для сравнения элементов</param>
        public LambdaComparer(Func<T, T, int> Predicate)
        {
            this.Predicate = Predicate;
        }

        public int Compare(T a, T b)
        {
            return Predicate(a, b);
        }
    }

    /// <summary>
    /// Реализует интерфейс IEqualityComparer на основе указанного лямбда-выражения
    /// </summary>
    /// <typeparam name="T">Тип объекта для сравнения</typeparam>
    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// Предикат для сравнения
        /// </summary>
        public Func<T, T, Boolean> EqualityPredicate { get; private set; }

        public Func<T, int> HashCodeExtractor { get; set; }

        public LambdaEqualityComparer(Func<T, T, Boolean> EqualityPredicate)
        {
            this.EqualityPredicate = EqualityPredicate;
        }
        public LambdaEqualityComparer(Func<T, int> HashCodeExtractor)
        {
            this.HashCodeExtractor = HashCodeExtractor;
        }

        public bool Equals(T a, T b)
        {
            if (EqualityPredicate != null)
                return EqualityPredicate(a, b);
            else
                return HashCodeExtractor(a) == HashCodeExtractor(b);
        }

        public int GetHashCode(T obj)
        {
            if (HashCodeExtractor != null) return HashCodeExtractor(obj);
            else return obj.GetHashCode();
        }
    }
}
