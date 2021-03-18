#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetPerformanceDemo
{
    [TestClass]
    public class BigOTests
    {
        private Random _random = new Random();
        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_N_squared(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x)).ToArray();
            var unique = new List<TestData>();
            
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < array1.Length; i++)
            {
                var isUnique = true;
                for (var j = 0; j < array1.Length; j++)
                {
                    if (array1[i].Id == array1[j].Id && i != j)
                    {
                        isUnique = false;
                    }
                }

                if (isUnique)
                {
                    unique.Add(array1[i]);
                }
            }
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_N_squared)} took {stopwatch.ElapsedMilliseconds} ms");

            unique.Count.Should().Be(array1Size);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
             
        [TestMethod]
        [DataRow(10,10)]
        [DataRow(200,200)]
        [DataRow(3000,3000)]
        [DataRow(40000,40000)]
        public void Runtime_of_N_times_S(int array1Size, int array2Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x)).ToArray();
            var array2 = Enumerable.Range(0, array2Size).Select(x => new TestData(x)).ToArray();
            var merged = new List<(TestData left, TestData right)>();
            
            var stopwatch = Stopwatch.StartNew();

            foreach (var td1 in array1)
            {
                foreach (var td2 in array2)
                {
                    if (td1.Id == td2.Id)
                    {
                        merged.Add((td1, td2));
                    }
                }
            }
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_N_times_S)} took {stopwatch.ElapsedMilliseconds} ms");

            merged.Count.Should().Be(array1Size);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_Linq_Distinct(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x)).ToArray();
            var stopwatch = Stopwatch.StartNew();

            var unique = array1.Distinct(new TestDataComparer()).ToArray();
            
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_Linq_Distinct)} took {stopwatch.ElapsedMilliseconds} ms");

            unique.Length.Should().Be(array1Size);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
        
        [TestMethod]
        [DataRow(10,10)]
        [DataRow(200,200)]
        [DataRow(3000,3000)]
        [DataRow(40000,40000)]
        public void Runtime_of_Linq_Join(int array1Size, int array2Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x)).ToArray();
            var array2 = Enumerable.Range(0, array2Size).Select(x => new TestData(x)).ToArray();
            var stopwatch = Stopwatch.StartNew();

            var merged = array1.Join(array2,
                a1 => a1.Id,
                a2 => a2.Id,
                (left, right) => (left, right))
                .ToArray();
            
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_Linq_Join)} took {stopwatch.ElapsedMilliseconds} ms");

            merged.Length.Should().Be(array1Size);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_N(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x)).ToArray();
            var set = new HashSet<TestData>();
            var unique = new List<TestData>();
            var comparer = new TestDataComparer();
            
            var stopwatch = Stopwatch.StartNew();

            foreach (var t in array1)
            {
                if (!set.Contains(t, comparer))
                {
                    set.Add(t);
                    unique.Add(t);
                }
            }
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_N)} took {stopwatch.ElapsedMilliseconds} ms");

            unique.Count.Should().Be(array1Size);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_N_better(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x)).ToArray();
            var unique = new HashSet<TestData>(new TestDataComparer());
            
            var stopwatch = Stopwatch.StartNew();

            foreach (var t in array1)
            {
                unique.Add(t);
            }
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_N)} took {stopwatch.ElapsedMilliseconds} ms");

            unique.Count.Should().Be(array1Size);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_slow_group_order_by(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x % 100, $"Test Data {x}") {Value = _random.Next()}).ToArray();
            var group = new Dictionary<int, List<TestData>>();
            
            var stopwatch = Stopwatch.StartNew();

            foreach (var t in array1)
            {
                if (!group.ContainsKey(t.Id))
                {
                    group.Add(t.Id, new List<TestData>());
                    foreach (var ti in array1)
                    {
                        if (t.Id == ti.Id)
                        {
                            var add = true;
                            for (var i = 0; i < group[t.Id].Count; i++)
                            {
                                if (ti.Value < group[t.Id][i].Value)
                                {
                                    add = false;
                                    group[t.Id].Insert(i, ti);
                                }
                            }

                            if (add)
                            {
                                group[t.Id].Add(ti);
                            }
                        }
                    }
                }
            }
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_slow_group_order_by)} took {stopwatch.ElapsedMilliseconds} ms");

            group.Count.Should().Be(Math.Min(100, array1Size));
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_linq_group_order_by(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(x % 100, $"Test Data {x}") {Value = x}).ToArray();
            
            var stopwatch = Stopwatch.StartNew();

            var group = array1.GroupBy(x => x.Id)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(y => y.Value).ToArray()
                );
                
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_linq_group_order_by)} took {stopwatch.ElapsedMilliseconds} ms");

            group.Count.Should().Be(Math.Min(100, array1Size));
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }

        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_slow_sort_array(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(_random.Next(), $"Test Data {x}")).ToArray();
            var ordered = new TestData [array1Size];
            
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < array1Size; i++)
            {
                ordered[i] = array1[i];
                for (var j = i - 1; j >= 0; j--)
                {
                    if (array1[i].Id < ordered[j].Id)
                    {
                        ordered[j + 1] = ordered[j];
                        ordered[j] = array1[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }
                
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_slow_sort_array)} took {stopwatch.ElapsedMilliseconds} ms");

            for (var i = 1; i < ordered.Length; i++)
            {
                ordered[i-1].Id.Should().BeLessOrEqualTo(ordered[i].Id);
            }
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }

        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_link_sort_array(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(_random.Next(), $"Test Data {x}")).ToArray();
            
            var stopwatch = Stopwatch.StartNew();

            var ordered = array1.OrderBy(x => x.Id).ToArray();
                
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_slow_sort_array)} took {stopwatch.ElapsedMilliseconds} ms");

            for (var i = 1; i < ordered.Length; i++)
            {
                ordered[i-1].Id.Should().BeLessOrEqualTo(ordered[i].Id);
            }
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }

        
        [TestMethod]
        [DataRow(10)]
        [DataRow(200)]
        [DataRow(3000)]
        [DataRow(40000)]
        public void Runtime_of_binary_sort_array(int array1Size)
        {
            var array1 = Enumerable.Range(0, array1Size).Select(x => new TestData(_random.Next(), $"Test Data {x}")).ToArray();
            var ordered = new List<TestData>(array1Size);
            var comparer = new TestDataIdComparer();
            
            var stopwatch = Stopwatch.StartNew();

            foreach (var testData in array1)
            {
                var position = ordered.BinarySearch(testData, comparer);
                if (position < 0)
                {
                    ordered.Insert(-(position + 1), testData);
                }
            }
                
            stopwatch.Stop();
            Debug.WriteLine($"{nameof(Runtime_of_slow_sort_array)} took {stopwatch.ElapsedMilliseconds} ms");

            for (var i = 1; i < ordered.Count; i++)
            {
                ordered[i-1].Id.Should().BeLessOrEqualTo(ordered[i].Id);
            }
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        }
        
        private class TestData 
        {
            public int Id { get; set; }
            public string Name { get; set; }
            
            public int Value { get; set; }

            public TestData(int id = 0, string name = "") {
                Id = id;
                Name = string.IsNullOrEmpty(name) ? $"Test Data #{id}" : name;
            }
        }
        
        private class TestDataComparer : IEqualityComparer<TestData>
        {
            public bool Equals(TestData? x, TestData? y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode(TestData obj)
            {
                return obj.Id.GetHashCode();
            }
        }
        
        private class TestDataIdComparer : IComparer<TestData>
        {
            public int Compare(TestData? x, TestData? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Id.CompareTo(y.Id);
            }
        }
        
        private class TestDataValueComparer : IComparer<TestData>
        {
            public int Compare(TestData? x, TestData? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Value.CompareTo(y.Value);
            }
        }
        
        private class TestDataNameComparer : IComparer<TestData>
        {
            public int Compare(TestData? x, TestData? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }
        }
    }
}
