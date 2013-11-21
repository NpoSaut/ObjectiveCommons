using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectiveCommons;

namespace ObjectiveCommonsTests
{
    [TestClass]
    public class RepeaterTests
    {
        private class CustomException : Exception { }
        private class CustomExceptionChild : CustomException { }
        private class CustomException2 : Exception { }

        [TestMethod]
        public void WithoutExceptions()
        {
            Repeater
                .Repeat(() => Thread.Sleep(10))
                .Catch<CustomException>()
                .Build().Run();
        }

        private class ThrowAndCatchResult
        {
            public int RunCounter = 0;
            public int CatchCounter = 0;
// ReSharper disable once ConvertToConstant.Local
            public int ThrowCount = 10;
        }
        private ThrowAndCatchResult ThrowAndCatch<TThrow, TCatch>(int throwCount = 10)
            where TThrow : Exception, new()
            where TCatch : Exception, new()
        {
            var res = new ThrowAndCatchResult() {ThrowCount = throwCount};
            Repeater
                .Repeat(() =>
                {
                    Thread.Sleep(3);
                    res.RunCounter++;
                    if (res.RunCounter <= res.ThrowCount) throw new TThrow();
                })
                .Catch<TCatch>(c => res.CatchCounter++)
                .Build().Run();
            return res;
        }

        [TestMethod]
        public void CatchingCustomException()
        {
            var res = ThrowAndCatch<CustomException, CustomException>();
            Assert.AreEqual(res.ThrowCount, res.CatchCounter, "Отловили сообщений меньше, чем ожидали");
            Assert.AreEqual(res.ThrowCount + 1, res.RunCounter, "Должны были сработать без ошибки на {0} раз, а сработали на {1}", res.ThrowCount + 1, res.RunCounter);
        }
        [TestMethod]
        public void CatchingCustomExceptionChild()
        {
            var res = ThrowAndCatch<CustomExceptionChild, CustomException>();
            Assert.AreEqual(res.ThrowCount, res.CatchCounter, "Отловили сообщений меньше, чем ожидали");
            Assert.AreEqual(res.ThrowCount + 1, res.RunCounter, "Должны были сработать без ошибки на {0} раз, а сработали на {1}", res.ThrowCount + 1, res.RunCounter);
        }
        [TestMethod]
        public void ThrowForeignException()
        {
            try
            {
                ThrowAndCatch<CustomException2, CustomException>();
                Assert.Fail("Должны были вылететь по исключению");
            }
            catch (CustomException2)
            {
                // Тест проиден
                return;
            }
            catch (Exception)
            {
                Assert.Fail("Пробросилось какое-то не такое исключение");
            }
        }
    }
}
