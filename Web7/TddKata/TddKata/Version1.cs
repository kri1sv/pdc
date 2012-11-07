using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TddKata
{
    public class StringCalculatorTests
    {
        [Test]
        public void should_return_0_for_empty()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            var result = calculator.Add("");
            //assert 
            Assert.AreEqual(0, result);
        }

        [Test]
        public void should_return_2_for_2()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            var result = calculator.Add("2");
            //assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public void should_return_5_for_2_and_3()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            var result = calculator.Add("2,3");
            //assert
            Assert.AreEqual(5, result);
        }

        [Test]
        public void should_return_sum_for_any_numbers()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            var result = calculator.Add("7,4,5,3,3");
            //assert
            Assert.AreEqual(7+4+5+3+3, result);
        }

        [Test]
        public void should_support_new_line_as_delimiter()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            var result = calculator.Add("1\n2,3");
            //assert
            Assert.AreEqual(6, result);
        }

        [Test]
        public void should_support_different_delimiters()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            var result = calculator.Add("//:\n1:2");
            //assert
            Assert.AreEqual(3, result);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "negatives not allowed -1")]
        public void should_throw_exception_for_one_negative()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            calculator.Add("-1,3");
        }

        [Test]
        [ExpectedException(ExpectedMessage = "negatives not allowed -4 -1 -3 -5")]
        public void should_throw_exception_for_any_negatives()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            calculator.Add("-4, 1,-1,-3,-5");
        }

        [Test]
        public void should_sum_except_bigger_than_1000()
        {
            //arrange
            var calculator = new StringCalculator();
            //act
            var result = calculator.Add("2,1001");
            //assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public void should_support_delimiters_of_any_length()
        {
            //arrange
            var calulator = new StringCalculator();
            //act
            var result = calulator.Add("//[***]\n1***2***3");
            //assert
            Assert.AreEqual(6, result);
        }

        [Test]
        public void should_support_different_delimiters_of_any_length()
        {
            //arrange
            var calulator = new StringCalculator();
            //act
            var result = calulator.Add("//[***][%]\n1***2%3");
            //assert
            Assert.AreEqual(6, result);
        }
    }

    internal class StringCalculator
    {
        public int Add(string numbers)
        {
            var delimiters = new[] {",", "\n"};
            if (numbers.StartsWith("//"))
            {
                List<string> delims = new List<string>();
                int currentIndex = 2;
                while (numbers.Substring(currentIndex, 1) != "\n")
                {
                    string delimiter = GetDelimiter(numbers, ref currentIndex);
                    delims.Add(delimiter);
                    currentIndex++;
                }
                delimiters = delims.ToArray();
                numbers = numbers.Substring(currentIndex);
            }
            var parsedNumbers = numbers.Split(delimiters, StringSplitOptions.None).Select(s => Parse(s));
            var negatives = parsedNumbers.Where(n => n < 0);
            if (negatives.Count() > 0)
            {
                string message = negatives.Aggregate("negatives not allowed", (current, n) => current + " " + n);
                throw new Exception(message);
            }
            return parsedNumbers.Where(n => n <= 1000).Sum();
        }

        private string GetDelimiter(string numbers, ref int currentIndex)
        {
            string result;
            if (numbers.Substring(currentIndex, 1) == "[")
            {
                currentIndex++;
                int index2 = numbers.IndexOf("]", currentIndex);
                result = numbers.Substring(currentIndex, index2 - currentIndex);
                currentIndex = index2;
            }
            else
            {
                result = numbers.Substring(currentIndex, 1);
            }
            return result;
        }

        private static int Parse(string numbers)
        {
            int result;
            Int32.TryParse(numbers, out result);
            return result;
        }
    }
}
